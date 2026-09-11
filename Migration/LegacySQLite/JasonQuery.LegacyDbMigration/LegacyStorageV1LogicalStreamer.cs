using JasonQuery.Core.Security.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.LegacyDbMigration
{
    /// <summary>
    /// Streams a qualified historical Storage V1 database as the frozen logical
    /// migration protocol. This type is read-only by design and must never create,
    /// migrate, rekey, checkpoint, or otherwise modify the source JasonQuery.db.
    /// </summary>
    internal sealed class LegacyStorageV1LogicalStreamer
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public void Stream(string databaseFilePath, byte[] databasePasswordUtf8, Stream output)
        {
            ValidateRequest(databaseFilePath, databasePasswordUtf8, output);

            var fullPath = Path.GetFullPath(databaseFilePath);
            var sourceSnapshot = SourceSnapshot.Capture(fullPath);

            LegacyStorageV1Inventory inventory;

            try
            {
                StrictUtf8.GetCharCount(databasePasswordUtf8);

                using (var connection = CreateReadOnlyConnection(fullPath, databasePasswordUtf8))
                {
                    ValidateSystemConfig(connection);
                    inventory = ReadInventory(connection);

                    DatabaseStorageMigrationWireProtocol.WriteStorageV1LogicalStreamReadyResponse(output);
                    WriteSchema(connection, inventory, output);

                    WriteRows
                    (
                        connection,
                        inventory,
                        output
                    );

                    WriteSequenceState(inventory, output);
                    WriteSecondarySchema(inventory, output);
                    output.Flush();
                }
            }
            finally
            {
                try
                {
                    SQLiteConnection.ClearAllPools();
                }
                finally
                {
                    sourceSnapshot.EnsureUnchanged();
                }
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndDatabase(output);
            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndStream(output);
            output.Flush();
        }

        private static LegacyStorageV1Inventory ReadInventory(SQLiteConnection connection)
        {
            var metadata = ReadDatabaseMetadata(connection);
            var schemaObjects = ReadSchemaObjects(connection);

            var tables = new List<LegacyStorageV1Table>();
            var secondaryObjects = new List<DatabaseStorageMigrationSecondarySchemaObject>();

            var sqliteSequenceExists = false;

            foreach (var schemaObject in schemaObjects)
            {
                if (schemaObject.IsInternal)
                {
                    if (schemaObject.Kind == DatabaseStorageMigrationSchemaObjectKind.Table &&
                        string.Equals(schemaObject.Name, "sqlite_sequence", StringComparison.Ordinal))
                    {
                        sqliteSequenceExists = true;
                        continue;
                    }

                    if (schemaObject.Kind == DatabaseStorageMigrationSchemaObjectKind.Index &&
                        schemaObject.Name.StartsWith("sqlite_autoindex_", StringComparison.Ordinal) &&
                        schemaObject.Sql == null)
                    {
                        continue;
                    }

                    throw new NotSupportedException
                    (
                        $"Internal SQLite schema object '{schemaObject.Name}' is not supported by logical stream protocol V1."
                    );
                }

                if (schemaObject.Kind == DatabaseStorageMigrationSchemaObjectKind.Table)
                {
                    tables.Add
                    (
                        new LegacyStorageV1Table
                        (
                            schemaObject.Name,
                            schemaObject.Sql
                        )
                    );

                    continue;
                }

                secondaryObjects.Add
                (
                    new DatabaseStorageMigrationSecondarySchemaObject
                    (
                        schemaObject.Kind,
                        schemaObject.Name,
                        schemaObject.TableName,
                        schemaObject.Sql
                    )
                );
            }

            tables.Sort
            (
                (left, right) => DatabaseStorageMigrationLogicalStreamContract.CompareIdentifiers
                (
                    left.Name,
                    right.Name
                )
            );

            secondaryObjects.Sort(CompareSecondarySchemaObjects);

            DatabaseStorageMigrationLogicalStreamContract.EnsureTableCount(tables.Count);

            var totalSchemaObjectCount = checked(tables.Count + secondaryObjects.Count);

            DatabaseStorageMigrationLogicalStreamContract.EnsureSchemaObjectCount
            (
                totalSchemaObjectCount
            );

            var tableNames = new HashSet<string>(StringComparer.Ordinal);
            var autoincrementTableNames = new HashSet<string>(StringComparer.Ordinal);

            for (var tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                var table = tables[tableIndex];

                if (!tableNames.Add(table.Name))
                {
                    throw new InvalidDataException
                    (
                        $"Duplicate table name '{table.Name}' was found in the Storage V1 schema."
                    );
                }

                ReadTableDefinition(connection, table, tableIndex);

                if (table.Definition.HasAutoincrement)
                {
                    autoincrementTableNames.Add(table.Name);
                }
            }

            if (autoincrementTableNames.Count > 0 && !sqliteSequenceExists)
            {
                throw new InvalidDataException
                (
                    "Storage V1 contains AUTOINCREMENT tables but sqlite_sequence is missing."
                );
            }

            var sequenceEntries = ReadSequenceEntries
            (
                connection,
                sqliteSequenceExists,
                autoincrementTableNames
            );

            return new LegacyStorageV1Inventory
            (
                metadata,
                tables,
                secondaryObjects,
                sequenceEntries,
                totalSchemaObjectCount
            );
        }

        private static DatabaseStorageMigrationDatabaseMetadata ReadDatabaseMetadata(SQLiteConnection connection)
        {
            var encodingName = ReadRequiredPragmaString(connection, "encoding");

            DatabaseStorageMigrationLogicalStreamContract.EnsureSourceEncoding(encodingName);

            return new DatabaseStorageMigrationDatabaseMetadata
            (
                ReadPragmaInt32(connection, "user_version"),
                ReadPragmaInt32(connection, "application_id"),
                DatabaseStorageMigrationLogicalStreamContract.RequiredSourceEncodingName
            );
        }

        private static List<LegacyStorageV1SchemaObject> ReadSchemaObjects(SQLiteConnection connection)
        {
            const string sql = "SELECT type, name, tbl_name, sql " + "FROM sqlite_master " + "WHERE type IN ('table', 'index', 'view', 'trigger')";

            var result = new List<LegacyStorageV1SchemaObject>();

            using (var command = new SQLiteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (result.Count >= DatabaseStorageMigrationLogicalStreamContract.MaxSchemaObjects)
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V1 schema object count exceeds the logical stream limit."
                        );
                    }

                    var typeName = GetRequiredString(reader, 0, "schema object type");
                    var name = GetRequiredString(reader, 1, "schema object name");
                    var tableName = GetRequiredString(reader, 2, "schema object table name");
                    var sqlText = reader.IsDBNull(3) ? null : reader.GetString(3);

                    var kind = ParseSchemaObjectKind(typeName);
                    var isInternal = name.StartsWith("sqlite_", StringComparison.Ordinal);

                    DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier
                    (
                        name,
                        "schemaObjectName"
                    );

                    DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier
                    (
                        tableName,
                        "schemaObjectTableName"
                    );

                    if (!isInternal || kind == DatabaseStorageMigrationSchemaObjectKind.Table)
                    {
                        if (sqlText == null)
                        {
                            if (!(kind == DatabaseStorageMigrationSchemaObjectKind.Index && name.StartsWith("sqlite_autoindex_", StringComparison.Ordinal)))
                            {
                                throw new InvalidDataException
                                (
                                    $"Storage V1 schema object '{name}' does not contain replay SQL."
                                );
                            }
                        }
                        else
                        {
                            DatabaseStorageMigrationLogicalStreamContract.EnsureRequiredSql
                            (
                                sqlText,
                                "schemaObjectSql"
                            );
                        }
                    }

                    result.Add
                    (
                        new LegacyStorageV1SchemaObject
                        (
                            kind,
                            name,
                            tableName,
                            sqlText,
                            isInternal
                        )
                    );
                }
            }

            return result;
        }

        private static void ReadTableDefinition(SQLiteConnection connection, LegacyStorageV1Table table, int tableId)
        {
            if (table.CreateSql == null)
            {
                throw new InvalidDataException
                (
                    $"Storage V1 table '{table.Name}' does not contain CREATE TABLE SQL."
                );
            }

            var tokens = GetSqlTokens(table.CreateSql);
            var withoutRowId = ContainsTokenSequence(tokens, "WITHOUT", "ROWID");
            var virtualTable = ContainsTokenSequence(tokens, "CREATE", "VIRTUAL", "TABLE");
            var hasAutoincrement = ContainsTokenSequence(tokens, "AUTOINCREMENT");

            var columns = ReadColumns(connection, table.Name);

            if (columns.Count == 0)
            {
                throw new InvalidDataException
                (
                    $"Storage V1 table '{table.Name}' contains no columns."
                );
            }

            var hasGeneratedColumns = false;
            var primaryKeyColumnCount = 0;
            var rowIdAliasColumnCid = -1;

            foreach (var column in columns)
            {
                if (column.Hidden != 0)
                {
                    hasGeneratedColumns = true;
                }

                if (column.PrimaryKeyOrdinal > 0)
                {
                    primaryKeyColumnCount++;

                    if (column.PrimaryKeyOrdinal == 1 && string.Equals(column.DeclaredType.Trim(), "INTEGER", StringComparison.OrdinalIgnoreCase))
                    {
                        rowIdAliasColumnCid = column.Cid;
                    }
                }
            }

            if (primaryKeyColumnCount != 1 || HasPrimaryKeyIndex(connection, table.Name))
            {
                rowIdAliasColumnCid = -1;
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedHistoricalTableFeatures
            (
                withoutRowId,
                virtualTable,
                hasGeneratedColumns,
                rowIdAliasColumnCid,
                columns.Count
            );

            table.Definition = new DatabaseStorageMigrationTableDefinition
            (
                tableId,
                table.Name,
                table.CreateSql,
                columns.Count,
                rowIdAliasColumnCid,
                hasAutoincrement
            );

            table.Columns = columns;
        }

        private static List<DatabaseStorageMigrationColumnDefinition> ReadColumns(SQLiteConnection connection, string tableName)
        {
            var sql = "PRAGMA table_xinfo(" + QuoteSqlStringLiteral(tableName) + ")";
            var columns = new List<DatabaseStorageMigrationColumnDefinition>();

            using (var command = new SQLiteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (columns.Count >= DatabaseStorageMigrationLogicalStreamContract.MaxColumnsPerTable)
                    {
                        throw new InvalidDataException
                        (
                            $"Storage V1 table '{tableName}' exceeds the column-count limit."
                        );
                    }

                    var cid = Convert.ToInt32(reader["cid"], CultureInfo.InvariantCulture);
                    var name = Convert.ToString(reader["name"], CultureInfo.InvariantCulture);
                    var declaredType = reader["type"] == DBNull.Value ? string.Empty : Convert.ToString(reader["type"], CultureInfo.InvariantCulture);
                    var notNull = Convert.ToInt32(reader["notnull"], CultureInfo.InvariantCulture) != 0;
                    var defaultSql = reader["dflt_value"] == DBNull.Value ? null : Convert.ToString(reader["dflt_value"], CultureInfo.InvariantCulture);
                    var primaryKeyOrdinal = Convert.ToInt32(reader["pk"], CultureInfo.InvariantCulture);
                    var hidden = Convert.ToInt32(reader["hidden"], CultureInfo.InvariantCulture);

                    columns.Add
                    (
                        new DatabaseStorageMigrationColumnDefinition
                        (
                            cid,
                            name,
                            declaredType,
                            notNull,
                            primaryKeyOrdinal,
                            hidden,
                            defaultSql
                        )
                    );
                }
            }

            columns.Sort((left, right) => left.Cid.CompareTo(right.Cid));

            for (var index = 0; index < columns.Count; index++)
            {
                if (columns[index].Cid != index)
                {
                    throw new NotSupportedException
                    (
                        $"Storage V1 table '{tableName}' contains a non-contiguous column id model."
                    );
                }
            }

            return columns;
        }

        private static bool HasPrimaryKeyIndex(SQLiteConnection connection, string tableName)
        {
            var sql = "PRAGMA index_list(" + QuoteSqlStringLiteral(tableName) + ")";

            using (var command = new SQLiteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var originOrdinal = reader.GetOrdinal("origin");
                    var origin = reader.IsDBNull(originOrdinal) ? string.Empty : reader.GetString(originOrdinal);

                    if (string.Equals(origin, "pk", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static List<DatabaseStorageMigrationSequenceEntry> ReadSequenceEntries(SQLiteConnection connection, bool sqliteSequenceExists, HashSet<string> autoincrementTableNames)
        {
            var result = new List<DatabaseStorageMigrationSequenceEntry>();

            if (!sqliteSequenceExists)
            {
                return result;
            }

            const string sql = "SELECT name, typeof(seq), seq " + "FROM sqlite_sequence";

            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            using (var command = new SQLiteCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (result.Count >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
                    {
                        throw new InvalidDataException
                        (
                            "The sqlite_sequence entry count exceeds the logical stream limit."
                        );
                    }

                    var tableName = GetRequiredString(reader, 0, "sqlite_sequence table name");
                    var sequenceType = GetRequiredString(reader, 1, "sqlite_sequence value type");

                    if (!string.Equals(sequenceType, "integer", StringComparison.Ordinal))
                    {
                        throw new InvalidDataException
                        (
                            $"sqlite_sequence value for table '{tableName}' is not an INTEGER."
                        );
                    }

                    if (!autoincrementTableNames.Contains(tableName))
                    {
                        throw new InvalidDataException
                        (
                            $"sqlite_sequence contains an unexpected table '{tableName}'."
                        );
                    }

                    if (!seenNames.Add(tableName))
                    {
                        throw new InvalidDataException
                        (
                            $"sqlite_sequence contains a duplicate table '{tableName}'."
                        );
                    }

                    result.Add
                    (
                        new DatabaseStorageMigrationSequenceEntry
                        (
                            tableName,
                            reader.GetInt64(2)
                        )
                    );
                }
            }

            result.Sort
            (
                (left, right) => DatabaseStorageMigrationLogicalStreamContract.CompareIdentifiers
                (
                    left.TableName,
                    right.TableName
                )
            );

            return result;
        }

        private static void WriteSchema(SQLiteConnection connection, LegacyStorageV1Inventory inventory, Stream output)
        {
            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginDatabase
            (
                output,
                inventory.Metadata,
                inventory.Tables.Count,
                inventory.SchemaObjectCount
            );

            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginSchema
            (
                output,
                inventory.Tables.Count
            );

            foreach (var table in inventory.Tables)
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginTable
                (
                    output,
                    table.Definition
                );

                foreach (var column in table.Columns)
                {
                    DatabaseStorageMigrationLogicalStreamProtocol.WriteColumn
                    (
                        output,
                        table.Definition.TableId,
                        column
                    );
                }

                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndTable
                (
                    output,
                    table.Definition.TableId
                );
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndSchema(output);
        }

        private static void WriteRows(SQLiteConnection connection, LegacyStorageV1Inventory inventory, Stream output)
        {
            foreach (var table in inventory.Tables)
            {
                WriteTableRows
                (
                    connection,
                    table,
                    output
                );
            }
        }

        private static void WriteTableRows(SQLiteConnection connection, LegacyStorageV1Table table, Stream output)
        {
            var selectSql = BuildRowSelectSql(table);

            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginRows
            (
                output,
                table.Definition.TableId
            );

            long rowCount = 0;

            using (var command = new SQLiteCommand(selectSql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (rowCount == long.MaxValue)
                    {
                        throw new InvalidDataException
                        (
                            $"Storage V1 table '{table.Name}' row count exceeds Int64."
                        );
                    }

                    DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginRow
                    (
                        output,
                        table.Columns.Count
                    );

                    for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    {
                        var storageTypeOrdinal = checked(columnIndex * 2);
                        var valueOrdinal = checked(storageTypeOrdinal + 1);

                        var storageType = GetRequiredString
                        (
                            reader,
                            storageTypeOrdinal,
                            "SQLite storage class"
                        );

                        WriteValue
                        (
                            reader,
                            valueOrdinal,
                            storageType,
                            output
                        );
                    }

                    DatabaseStorageMigrationLogicalStreamProtocol.WriteEndRow(output);
                    rowCount++;
                }
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndRows
            (
                output,
                table.Definition.TableId,
                rowCount
            );
        }

        private static void WriteValue(SQLiteDataReader reader, int ordinal, string storageType, Stream output)
        {
            if (string.Equals(storageType, "null", StringComparison.Ordinal))
            {
                if (!reader.IsDBNull(ordinal))
                {
                    throw new InvalidDataException("SQLite reported NULL but the value reader did not.");
                }

                DatabaseStorageMigrationLogicalStreamProtocol.WriteNullValue(output);
                return;
            }

            if (reader.IsDBNull(ordinal))
            {
                throw new InvalidDataException
                (
                    $"SQLite reported storage class '{storageType}' for a NULL value."
                );
            }

            if (string.Equals(storageType, "integer", StringComparison.Ordinal))
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteInt64Value
                (
                    output,
                    Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
                );

                return;
            }

            if (string.Equals(storageType, "real", StringComparison.Ordinal))
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteDoubleValue
                (
                    output,
                    Convert.ToDouble(reader.GetValue(ordinal), CultureInfo.InvariantCulture)
                );

                return;
            }

            if (string.Equals(storageType, "text", StringComparison.Ordinal))
            {
                WriteTextUtf8(reader, ordinal, output);
                return;
            }

            if (string.Equals(storageType, "blob", StringComparison.Ordinal))
            {
                WriteBlob(reader, ordinal, output);
                return;
            }

            throw new NotSupportedException
            (
                $"SQLite storage class '{storageType}' is not supported by logical stream protocol V1."
            );
        }

        private static void WriteTextUtf8(SQLiteDataReader reader, int ordinal, Stream output)
        {
            var totalLength = reader.GetBytes(ordinal, 0, null, 0, 0);

            DatabaseStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(totalLength);

            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginTextUtf8
            (
                output,
                totalLength
            );

            if (totalLength == 0)
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndTextUtf8(output);
                return;
            }

            var byteBuffer = new byte[DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes];
            var charBuffer = new char[DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes];
            var decoder = DatabaseStorageMigrationLogicalStreamContract.CreateStrictUtf8Decoder();

            try
            {
                long offset = 0;

                while (offset < totalLength)
                {
                    var requested = (int)Math.Min(byteBuffer.Length, totalLength - offset);

                    var bytesRead = reader.GetBytes
                    (
                        ordinal,
                        offset,
                        byteBuffer,
                        0,
                        requested
                    );

                    if (bytesRead <= 0)
                    {
                        throw new EndOfStreamException
                        (
                            "The Storage V1 TEXT value ended before its reported UTF-8 byte length."
                        );
                    }

                    var count = checked((int)bytesRead);
                    var flush = offset + bytesRead == totalLength;

                    ValidateStrictUtf8Chunk
                    (
                        decoder,
                        byteBuffer,
                        count,
                        charBuffer,
                        flush
                    );

                    DatabaseStorageMigrationLogicalStreamProtocol.WriteTextUtf8Chunk
                    (
                        output,
                        byteBuffer,
                        0,
                        count
                    );

                    offset += bytesRead;
                }

                if (offset != totalLength)
                {
                    throw new InvalidDataException
                    (
                        "The Storage V1 TEXT UTF-8 byte count does not match its reported length."
                    );
                }
            }
            finally
            {
                Array.Clear(byteBuffer, 0, byteBuffer.Length);
                Array.Clear(charBuffer, 0, charBuffer.Length);
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndTextUtf8(output);
        }

        private static void ValidateStrictUtf8Chunk(Decoder decoder, byte[] byteBuffer, int byteCount, char[] charBuffer, bool flush)
        {
            int bytesUsed;
            int charsUsed;
            bool completed;

            decoder.Convert
            (
                byteBuffer,
                0,
                byteCount,
                charBuffer,
                0,
                charBuffer.Length,
                flush,
                out bytesUsed,
                out charsUsed,
                out completed
            );

            if (bytesUsed != byteCount)
            {
                throw new InvalidDataException
                (
                    "The bounded UTF-8 decoder did not consume the complete Storage V1 TEXT chunk."
                );
            }

            if (flush && !completed)
            {
                throw new InvalidDataException
                (
                    "The Storage V1 TEXT value ended with an incomplete UTF-8 sequence."
                );
            }

            if (charsUsed > 0)
            {
                Array.Clear(charBuffer, 0, charsUsed);
            }
        }

        private static void WriteBlob(SQLiteDataReader reader, int ordinal, Stream output)
        {
            var totalLength = reader.GetBytes(ordinal, 0, null, 0, 0);

            DatabaseStorageMigrationLogicalStreamContract.EnsureBlobByteLength(totalLength);

            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginBlob
            (
                output,
                totalLength
            );

            if (totalLength == 0)
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteEndBlob(output);
                return;
            }

            var buffer = new byte[DatabaseStorageMigrationLogicalStreamContract.MaxValueChunkBytes];

            try
            {
                long offset = 0;

                while (offset < totalLength)
                {
                    var requested = (int)Math.Min(buffer.Length, totalLength - offset);

                    var bytesRead = reader.GetBytes
                    (
                        ordinal,
                        offset,
                        buffer,
                        0,
                        requested
                    );

                    if (bytesRead <= 0)
                    {
                        throw new EndOfStreamException
                        (
                            "The Storage V1 BLOB ended before its reported length."
                        );
                    }

                    DatabaseStorageMigrationLogicalStreamProtocol.WriteBlobChunk
                    (
                        output,
                        buffer,
                        0,
                        checked((int)bytesRead)
                    );

                    offset += bytesRead;
                }

                if (offset != totalLength)
                {
                    throw new InvalidDataException
                    (
                        "The Storage V1 BLOB byte count does not match its reported length."
                    );
                }
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndBlob(output);
        }

        private static void WriteSequenceState(LegacyStorageV1Inventory inventory, Stream output)
        {
            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginSequenceState
            (
                output,
                inventory.SequenceEntries.Count
            );

            foreach (var entry in inventory.SequenceEntries)
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteSequenceEntry
                (
                    output,
                    entry
                );
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndSequenceState(output);
        }

        private static void WriteSecondarySchema(LegacyStorageV1Inventory inventory, Stream output)
        {
            DatabaseStorageMigrationLogicalStreamProtocol.WriteBeginSecondarySchema
            (
                output,
                inventory.SecondaryObjects.Count
            );

            foreach (var schemaObject in inventory.SecondaryObjects)
            {
                DatabaseStorageMigrationLogicalStreamProtocol.WriteSchemaObject
                (
                    output,
                    schemaObject
                );
            }

            DatabaseStorageMigrationLogicalStreamProtocol.WriteEndSecondarySchema(output);
        }

        private static string BuildRowSelectSql(LegacyStorageV1Table table)
        {
            var builder = new StringBuilder();

            builder.Append("SELECT ");

            for (var index = 0; index < table.Columns.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                var quotedColumn = QuoteIdentifier(table.Columns[index].Name);

                builder.Append("typeof(+");
                builder.Append(quotedColumn);
                builder.Append("), CASE typeof(+");
                builder.Append(quotedColumn);
                builder.Append(") WHEN 'text' THEN CAST(+");
                builder.Append(quotedColumn);
                builder.Append(" AS BLOB) ELSE +");
                builder.Append(quotedColumn);
                builder.Append(" END");

                EnsureGeneratedSqlCharacterLength(builder);
            }

            builder.Append(" FROM ");
            builder.Append(QuoteIdentifier(table.Name));
            builder.Append(" ORDER BY ");
            builder.Append
            (
                QuoteIdentifier
                (
                    table.Columns[table.Definition.RowIdAliasColumnCid].Name
                )
            );
            builder.Append(" ASC");

            EnsureGeneratedSqlLength(builder);

            return builder.ToString();
        }

        private static void EnsureGeneratedSqlCharacterLength(StringBuilder builder)
        {
            if (builder.Length > DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes)
            {
                throw new InvalidDataException
                (
                    "The generated read-only Storage V1 query exceeds the bounded SQL length."
                );
            }
        }

        private static void EnsureGeneratedSqlLength(StringBuilder builder)
        {
            if (StrictUtf8.GetByteCount(builder.ToString()) > DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes)
            {
                throw new InvalidDataException
                (
                    "The generated read-only Storage V1 query exceeds the bounded SQL length."
                );
            }
        }

        private static int CompareSecondarySchemaObjects(DatabaseStorageMigrationSecondarySchemaObject left,
                                                         DatabaseStorageMigrationSecondarySchemaObject right)
        {
            var orderComparison = DatabaseStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder(left.Kind).CompareTo
            (
                DatabaseStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder(right.Kind)
            );

            if (orderComparison != 0)
            {
                return orderComparison;
            }

            var nameComparison = DatabaseStorageMigrationLogicalStreamContract.CompareIdentifiers
            (
                left.Name,
                right.Name
            );

            if (nameComparison != 0)
            {
                return nameComparison;
            }

            return DatabaseStorageMigrationLogicalStreamContract.CompareIdentifiers
            (
                left.TableName,
                right.TableName
            );
        }

        private static DatabaseStorageMigrationSchemaObjectKind ParseSchemaObjectKind(string typeName)
        {
            if (string.Equals(typeName, "table", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseStorageMigrationSchemaObjectKind.Table;
            }

            if (string.Equals(typeName, "index", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseStorageMigrationSchemaObjectKind.Index;
            }

            if (string.Equals(typeName, "view", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseStorageMigrationSchemaObjectKind.View;
            }

            if (string.Equals(typeName, "trigger", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseStorageMigrationSchemaObjectKind.Trigger;
            }

            throw new NotSupportedException
            (
                $"SQLite schema object type '{typeName}' is not supported."
            );
        }

        private static List<string> GetSqlTokens(string sql)
        {
            var tokens = new List<string>();
            var index = 0;

            while (index < sql.Length)
            {
                var current = sql[index];

                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                if (current == '\'' || current == '"' || current == '`')
                {
                    SkipQuoted(sql, ref index, current);
                    continue;
                }

                if (current == '[')
                {
                    SkipBracketQuoted(sql, ref index);
                    continue;
                }

                if (current == '-' && index + 1 < sql.Length && sql[index + 1] == '-')
                {
                    SkipLineComment(sql, ref index);
                    continue;
                }

                if (current == '/' && index + 1 < sql.Length && sql[index + 1] == '*')
                {
                    SkipBlockComment(sql, ref index);
                    continue;
                }

                if (char.IsLetter(current) || current == '_')
                {
                    var start = index;

                    index++;

                    while (index < sql.Length && (char.IsLetterOrDigit(sql[index]) || sql[index] == '_'))
                    {
                        index++;
                    }

                    tokens.Add(sql.Substring(start, index - start).ToUpperInvariant());
                    continue;
                }

                index++;
            }

            return tokens;
        }

        private static bool ContainsTokenSequence(List<string> tokens, params string[] sequence)
        {
            if (sequence == null || sequence.Length == 0)
            {
                return false;
            }

            for (var start = 0; start <= tokens.Count - sequence.Length; start++)
            {
                var matched = true;

                for (var offset = 0; offset < sequence.Length; offset++)
                {
                    if (!string.Equals(tokens[start + offset], sequence[offset], StringComparison.Ordinal))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SkipQuoted(string sql, ref int index, char delimiter)
        {
            index++;

            while (index < sql.Length)
            {
                if (sql[index] != delimiter)
                {
                    index++;
                    continue;
                }

                if (index + 1 < sql.Length && sql[index + 1] == delimiter)
                {
                    index += 2;
                    continue;
                }

                index++;
                return;
            }

            throw new InvalidDataException("Storage V1 schema SQL contains an unterminated quoted token.");
        }

        private static void SkipBracketQuoted(string sql, ref int index)
        {
            index++;

            while (index < sql.Length)
            {
                if (sql[index] == ']')
                {
                    index++;
                    return;
                }

                index++;
            }

            throw new InvalidDataException("Storage V1 schema SQL contains an unterminated bracket identifier.");
        }

        private static void SkipLineComment(string sql, ref int index)
        {
            index += 2;

            while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
            {
                index++;
            }
        }

        private static void SkipBlockComment(string sql, ref int index)
        {
            index += 2;

            while (index + 1 < sql.Length)
            {
                if (sql[index] == '*' && sql[index + 1] == '/')
                {
                    index += 2;
                    return;
                }

                index++;
            }

            throw new InvalidDataException("Storage V1 schema SQL contains an unterminated block comment.");
        }

        private static string ReadRequiredPragmaString(SQLiteConnection connection, string pragmaName)
        {
            using (var command = new SQLiteCommand("PRAGMA " + pragmaName, connection))
            {
                var value = command.ExecuteScalar();

                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidDataException
                    (
                        $"PRAGMA {pragmaName} returned no value."
                    );
                }

                var text = Convert.ToString(value, CultureInfo.InvariantCulture);

                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidDataException
                    (
                        $"PRAGMA {pragmaName} returned an empty value."
                    );
                }

                return text;
            }
        }

        private static int ReadPragmaInt32(SQLiteConnection connection, string pragmaName)
        {
            using (var command = new SQLiteCommand("PRAGMA " + pragmaName, connection))
            {
                var value = command.ExecuteScalar();

                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidDataException
                    (
                        $"PRAGMA {pragmaName} returned no value."
                    );
                }

                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
        }

        private static string GetRequiredString(SQLiteDataReader reader, int ordinal, string description)
        {
            if (reader.IsDBNull(ordinal))
            {
                throw new InvalidDataException
                (
                    $"The Storage V1 {description} is NULL."
                );
            }

            var value = Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidDataException
                (
                    $"The Storage V1 {description} is empty."
                );
            }

            return value;
        }

        private static void ValidateSystemConfig(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT 1 FROM SystemConfig WHERE 1 = 2", connection))
            {
                command.ExecuteScalar();
            }
        }

        private static SQLiteConnection CreateReadOnlyConnection(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = databaseFilePath,
                Version = 3,
                ReadOnly = true,
                FailIfMissing = true,
                Pooling = false
            };

            var connection = new SQLiteConnection
            {
                ConnectionString = connectionStringBuilder.ConnectionString
            };

            try
            {
                connection.SetPassword(databasePasswordUtf8);
                connection.Open();

                if (connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException
                    (
                        "The historical Storage V1 database did not open."
                    );
                }

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static string QuoteIdentifier(string identifier)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier
            (
                identifier,
                nameof(identifier)
            );

            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private static string QuoteSqlStringLiteral(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        private static void ValidateRequest(string databaseFilePath, byte[] databasePasswordUtf8, Stream output)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new InvalidDataException
                (
                    "The historical Storage V1 source path is invalid."
                );
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new InvalidDataException
                (
                    "The historical Storage V1 database credential is missing."
                );
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (!output.CanWrite)
            {
                throw new ArgumentException
                (
                    "The logical Storage V1 output stream must be writable.",
                    nameof(output)
                );
            }

            if (!File.Exists(Path.GetFullPath(databaseFilePath)))
            {
                throw new FileNotFoundException
                (
                    "The historical Storage V1 source database was not found."
                );
            }
        }

        private sealed class LegacyStorageV1Inventory
        {
            public LegacyStorageV1Inventory(DatabaseStorageMigrationDatabaseMetadata metadata, List<LegacyStorageV1Table> tables,
                                            List<DatabaseStorageMigrationSecondarySchemaObject> secondaryObjects,
                                            List<DatabaseStorageMigrationSequenceEntry> sequenceEntries, int schemaObjectCount)
            {
                Metadata = metadata;
                Tables = tables;
                SecondaryObjects = secondaryObjects;
                SequenceEntries = sequenceEntries;
                SchemaObjectCount = schemaObjectCount;
            }

            public DatabaseStorageMigrationDatabaseMetadata Metadata { get; }

            public List<LegacyStorageV1Table> Tables { get; }

            public List<DatabaseStorageMigrationSecondarySchemaObject> SecondaryObjects { get; }

            public List<DatabaseStorageMigrationSequenceEntry> SequenceEntries { get; }

            public int SchemaObjectCount { get; }
        }

        private sealed class LegacyStorageV1Table
        {
            public LegacyStorageV1Table(string name, string createSql)
            {
                Name = name;
                CreateSql = createSql;
            }

            public string Name { get; }

            public string CreateSql { get; }

            public DatabaseStorageMigrationTableDefinition Definition { get; set; }

            public List<DatabaseStorageMigrationColumnDefinition> Columns { get; set; }
        }

        private sealed class LegacyStorageV1SchemaObject
        {
            public LegacyStorageV1SchemaObject(DatabaseStorageMigrationSchemaObjectKind kind, string name,
                                               string tableName, string sql, bool isInternal)
            {
                Kind = kind;
                Name = name;
                TableName = tableName;
                Sql = sql;
                IsInternal = isInternal;
            }

            public DatabaseStorageMigrationSchemaObjectKind Kind { get; }

            public string Name { get; }

            public string TableName { get; }

            public string Sql { get; }

            public bool IsInternal { get; }
        }

        private sealed class SourceSnapshot
        {
            private readonly string _databasePath;
            private readonly FileSnapshot _database;
            private readonly FileSnapshot _wal;
            private readonly FileSnapshot _shm;
            private readonly FileSnapshot _journal;

            private SourceSnapshot(string databasePath, FileSnapshot database, FileSnapshot wal,
                                   FileSnapshot shm, FileSnapshot journal)
            {
                _databasePath = databasePath;
                _database = database;
                _wal = wal;
                _shm = shm;
                _journal = journal;
            }

            public static SourceSnapshot Capture(string databasePath)
            {
                return new SourceSnapshot
                (
                    databasePath,
                    FileSnapshot.Capture(databasePath, true),
                    FileSnapshot.Capture(databasePath + "-wal", true),
                    FileSnapshot.Capture(databasePath + "-shm", true),
                    FileSnapshot.Capture(databasePath + "-journal", true)
                );
            }

            public void EnsureUnchanged()
            {
                _database.EnsureSame
                (
                    FileSnapshot.Capture(_databasePath, true),
                    "historical Storage V1 source database"
                );

                _wal.EnsureSame
                (
                    FileSnapshot.Capture(_databasePath + "-wal", true),
                    "historical Storage V1 WAL sidecar"
                );

                _shm.EnsureSame
                (
                    FileSnapshot.Capture(_databasePath + "-shm", true),
                    "historical Storage V1 SHM sidecar"
                );

                _journal.EnsureSame
                (
                    FileSnapshot.Capture(_databasePath + "-journal", true),
                    "historical Storage V1 rollback-journal sidecar"
                );
            }
        }

        private sealed class FileSnapshot
        {
            private FileSnapshot(bool exists, long length, DateTime lastWriteTimeUtc, byte[] sha256)
            {
                Exists = exists;
                Length = length;
                LastWriteTimeUtc = lastWriteTimeUtc;
                Sha256 = sha256;
            }

            public bool Exists { get; }

            public long Length { get; }

            public DateTime LastWriteTimeUtc { get; }

            public byte[] Sha256 { get; }

            public static FileSnapshot Capture(string filePath, bool includeHash)
            {
                if (!File.Exists(filePath))
                {
                    return new FileSnapshot
                    (
                        false,
                        0,
                        DateTime.MinValue,
                        null
                    );
                }

                var fileInfo = new FileInfo(filePath);
                byte[] hash = null;

                if (includeHash)
                {
                    using (var sha256 = SHA256.Create())
                    using (var stream = new FileStream
                    (
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read
                    ))
                    {
                        hash = sha256.ComputeHash(stream);
                    }
                }

                return new FileSnapshot
                (
                    true,
                    fileInfo.Length,
                    fileInfo.LastWriteTimeUtc,
                    hash
                );
            }

            public void EnsureSame(FileSnapshot other, string description)
            {
                if (Exists != other.Exists)
                {
                    throw new InvalidDataException
                    (
                        $"The {description} existence changed during logical streaming."
                    );
                }

                if (!Exists)
                {
                    return;
                }

                if (Length != other.Length || LastWriteTimeUtc != other.LastWriteTimeUtc || !HashesEqual(Sha256, other.Sha256))
                {
                    throw new InvalidDataException
                    (
                        $"The {description} changed during logical streaming."
                    );
                }
            }

            private static bool HashesEqual(byte[] left, byte[] right)
            {
                if (left == null || right == null || left.Length != right.Length)
                {
                    return false;
                }

                var difference = 0;

                for (var index = 0; index < left.Length; index++)
                {
                    difference |= left[index] ^ right[index];
                }

                return difference == 0;
            }
        }
    }
}
