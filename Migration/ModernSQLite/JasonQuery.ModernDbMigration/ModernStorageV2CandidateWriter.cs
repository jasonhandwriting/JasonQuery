using JasonQuery.Core.Security.JasonQueryDb;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JasonQuery.ModernDbMigration
{
    internal sealed class ModernStorageV2CandidateWriter
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);
        private static readonly byte[] EmptyBytes = new byte[0];

        public void CreateAndValidate(string candidateDatabasePath, byte[] databasePasswordUtf8, Stream logicalStream)
        {
            ValidateRequest(candidateDatabasePath, databasePasswordUtf8, logicalStream);
            ModernSqlCipherRuntime.InitializeAndValidate();

            var fullCandidatePath = Path.GetFullPath(candidateDatabasePath);

            EnsureCreateNewCandidatePath(fullCandidatePath);

            try
            {
                var state = CreateCandidate(fullCandidatePath, databasePasswordUtf8, logicalStream);

                ModernSqlCipherRuntime.DurablyFlushFile(fullCandidatePath);
                ValidateClosedCandidate(fullCandidatePath, databasePasswordUtf8, state);
            }
            catch
            {
                ModernSqlCipherRuntime.DeleteCandidateArtifactsBestEffort(fullCandidatePath);
                throw;
            }
        }

        private static CandidateState CreateCandidate(string databasePath, byte[] databasePasswordUtf8, Stream logicalStream)
        {
            sqlite3 db = null;
            var committed = false;

            try
            {
                db = ModernSqlCipherRuntime.OpenCreateNew(databasePath, databasePasswordUtf8);
                ModernSqlCipherRuntime.ConfigureWriter(db);
                ModernSqlCipherRuntime.ValidateFrozenProfile(db);
                ModernSqlCipherRuntime.ValidateWriterPragmas(db);
                ModernSqlCipherRuntime.Exec(db, "BEGIN IMMEDIATE;");

                try
                {
                    var state = ConsumeLogicalStream(db, logicalStream);

                    ModernSqlCipherRuntime.Exec
                    (
                        db,
                        "PRAGMA user_version = " + state.Metadata.UserVersion.ToString(CultureInfo.InvariantCulture) + ";"
                    );

                    ModernSqlCipherRuntime.Exec
                    (
                        db,
                        "PRAGMA application_id = " + state.Metadata.ApplicationId.ToString(CultureInfo.InvariantCulture) + ";"
                    );

                    ModernSqlCipherRuntime.Exec(db, "COMMIT;");
                    committed = true;
                    ModernSqlCipherRuntime.ValidateFrozenProfile(db);
                    ModernSqlCipherRuntime.ValidateWriterPragmas(db);
                    ModernSqlCipherRuntime.ValidateIntegrity(db);

                    return state;
                }
                catch
                {
                    if (!committed)
                    {
                        ModernSqlCipherRuntime.TryRollback(db);
                    }

                    throw;
                }
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(db);
            }
        }

        private static CandidateState ConsumeLogicalStream(sqlite3 db, Stream logicalStream)
        {
            using (var sourceDigest = new ModernStorageV2LogicalDigest())
            using (var beginDatabaseFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase))
            {
                var beginDatabase = beginDatabaseFrame.GetPayload<JasonQueryDbStorageMigrationBeginDatabase>();
                var metadata = beginDatabase.Metadata;

                sourceDigest.AddDatabaseMetadata(metadata);

                var tables = ReadAndReplaySchema(db, logicalStream, beginDatabase);
                var stagingTableName = CreateEncryptedStagingTable(db);

                try
                {
                    ReadAndReplayRows(db, logicalStream, tables, stagingTableName, sourceDigest);

                    var sequenceEntries = ReadAndReplaySequenceState(db, logicalStream, tables, sourceDigest);

                    ReadAndReplaySecondarySchema(db, logicalStream, beginDatabase, tables);

                    using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndDatabase))
                    {
                    }

                    using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndStream))
                    {
                    }

                    EnsureLogicalStreamEnded(logicalStream);
                    EnsureStagingTableEmpty(db, stagingTableName);

                    ModernSqlCipherRuntime.ExecSingleStatement
                    (
                        db,
                        "DROP TABLE " + ModernSqlCipherRuntime.QuoteIdentifier(stagingTableName) + ";",
                        "drop encrypted migration staging table"
                    );

                    return new CandidateState
                    (
                        metadata,
                        tables,
                        sequenceEntries,
                        sourceDigest.Finish()
                    );
                }
                catch
                {
                    throw;
                }
            }
        }

        private static List<TableState> ReadAndReplaySchema(sqlite3 db, Stream logicalStream, JasonQueryDbStorageMigrationBeginDatabase beginDatabase)
        {
            using (var beginSchemaFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSchema))
            {
                var tableCount = beginSchemaFrame.GetPayload<int>();

                if (tableCount != beginDatabase.TableCount)
                {
                    throw new InvalidDataException("The BeginSchema table count does not match BeginDatabase.");
                }
            }

            var tables = new List<TableState>(beginDatabase.TableCount);
            string previousTableName = null;

            for (var tableIndex = 0; tableIndex < beginDatabase.TableCount; tableIndex++)
            {
                using (var beginTableFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginTable))
                {
                    var table = beginTableFrame.GetPayload<JasonQueryDbStorageMigrationTableDefinition>();

                    if (table.TableId != tableIndex)
                    {
                        throw new InvalidDataException("Logical table IDs must be contiguous and match schema order.");
                    }

                    EnsureStrictlyIncreasingIdentifier(previousTableName, table.Name, "table");
                    previousTableName = table.Name;
                    EnsureCreateSqlPrefix(table.CreateSql, "TABLE");
                    ModernSqlCipherRuntime.ExecSingleStatement(db, table.CreateSql, "replay CREATE TABLE");
                    VerifySchemaObject(db, "table", table.Name, table.Name, table.CreateSql);

                    var targetColumns = ReadTargetColumns(db, table.Name);

                    if (targetColumns.Count != table.ColumnCount)
                    {
                        throw new InvalidDataException("The replayed table column count does not match the logical stream.");
                    }

                    for (var columnIndex = 0; columnIndex < table.ColumnCount; columnIndex++)
                    {
                        using (var columnFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.Column))
                        {
                            var payload = columnFrame.GetPayload<JasonQueryDbStorageMigrationColumnFrame>();

                            if (payload.TableId != table.TableId || payload.Column.Cid != columnIndex)
                            {
                                throw new InvalidDataException("Logical column IDs must be contiguous and belong to the active table.");
                            }

                            VerifyColumnDefinition(payload.Column, targetColumns[columnIndex]);
                        }
                    }

                    using (var endTableFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndTable))
                    {
                        if (endTableFrame.GetPayload<int>() != table.TableId)
                        {
                            throw new InvalidDataException("EndTable does not match the active table.");
                        }
                    }

                    var rowIdColumn = targetColumns[table.RowIdAliasColumnCid];

                    if (rowIdColumn.PrimaryKeyOrdinal != 1 || !string.Equals(rowIdColumn.DeclaredType, "INTEGER", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException("The logical rowid alias is not an INTEGER PRIMARY KEY in the replayed table.");
                    }

                    tables.Add
                    (
                        new TableState
                        (
                            table.TableId,
                            table.Name,
                            table.ColumnCount,
                            table.RowIdAliasColumnCid,
                            rowIdColumn.Name,
                            table.HasAutoincrement
                        )
                    );
                }
            }

            using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSchema))
            {
            }

            return tables;
        }

        private static void ReadAndReplayRows(sqlite3 db, Stream logicalStream, List<TableState> tables,
                                              string stagingTableName, ModernStorageV2LogicalDigest sourceDigest)
        {
            foreach (var table in tables)
            {
                using (var beginRowsFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginRows))
                {
                    if (beginRowsFrame.GetPayload<int>() != table.TableId)
                    {
                        throw new InvalidDataException("BeginRows does not match the expected table order.");
                    }
                }

                var columns = ReadTargetColumns(db, table.Name);

                if (columns.Count != table.ColumnCount)
                {
                    throw new InvalidDataException("The target table column count changed before row replay.");
                }

                sourceDigest.BeginTable(table.TableId, table.Name, table.ColumnCount);

                long rowCount = 0;
                long previousRowId = 0;
                var hasPreviousRowId = false;

                while (true)
                {
                    using (var frame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(logicalStream))
                    {
                        if (frame.Kind == JasonQueryDbStorageMigrationLogicalFrameKind.EndRows)
                        {
                            var endRows = frame.GetPayload<JasonQueryDbStorageMigrationEndRows>();

                            if (endRows.TableId != table.TableId || endRows.RowCount != rowCount)
                            {
                                throw new InvalidDataException("EndRows does not match the replayed table or row count.");
                            }

                            table.SourceRowCount = rowCount;
                            sourceDigest.EndTable(rowCount);
                            break;
                        }

                        if (frame.Kind != JasonQueryDbStorageMigrationLogicalFrameKind.BeginRow)
                        {
                            throw new InvalidDataException("A table row section contains an unexpected logical frame.");
                        }

                        var fieldCount = frame.GetPayload<int>();

                        JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRowFieldCount(fieldCount, table.ColumnCount);
                        sourceDigest.BeginRow(fieldCount);

                        var rowValues = new RowValue[fieldCount];

                        try
                        {
                            for (var fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                            {
                                rowValues[fieldIndex] = ReadRowValue
                                (
                                    db,
                                    logicalStream,
                                    stagingTableName,
                                    sourceDigest
                                );
                            }

                            using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndRow))
                            {
                            }

                            sourceDigest.EndRow();

                            var rowIdValue = rowValues[table.RowIdAliasColumnCid];

                            if (rowIdValue.Kind != JasonQueryDbStorageMigrationValueKind.Int64)
                            {
                                throw new InvalidDataException("The logical rowid-alias field must use the Int64 value kind.");
                            }

                            var rowId = rowIdValue.Int64Value;

                            if (hasPreviousRowId && rowId <= previousRowId)
                            {
                                throw new InvalidDataException("Logical rows are not strictly ordered by the rowid alias.");
                            }

                            InsertTargetRow(db, table, columns, rowValues, stagingTableName, rowId);
                            previousRowId = rowId;
                            hasPreviousRowId = true;
                            rowCount = checked(rowCount + 1);
                        }
                        finally
                        {
                            ClearRowValues(rowValues);
                        }
                    }
                }
            }
        }

        private static RowValue ReadRowValue(sqlite3 db, Stream logicalStream, string stagingTableName, ModernStorageV2LogicalDigest sourceDigest)
        {
            using (var frame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(logicalStream))
            {
                switch (frame.Kind)
                {
                    case JasonQueryDbStorageMigrationLogicalFrameKind.NullValue:
                        {
                            sourceDigest.AddNull();
                            return RowValue.Null();
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.Int64Value:
                        {
                            var value = frame.GetPayload<long>();
                            sourceDigest.AddInt64(value);
                            return RowValue.Int64(value);
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.DoubleValue:
                        {
                            var value = frame.GetPayload<double>();
                            sourceDigest.AddDouble(value);
                            return RowValue.Double(value);
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginTextUtf8:
                        {
                            return ReadLargeValue
                            (
                                db,
                                logicalStream,
                                stagingTableName,
                                JasonQueryDbStorageMigrationValueKind.TextUtf8,
                                frame.GetPayload<long>(),
                                sourceDigest
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginBlob:
                        {
                            return ReadLargeValue
                            (
                                db,
                                logicalStream,
                                stagingTableName,
                                JasonQueryDbStorageMigrationValueKind.Blob,
                                frame.GetPayload<long>(),
                                sourceDigest
                            );
                        }
                    default:
                        {
                            throw new InvalidDataException("A row contains an unexpected logical value frame.");
                        }
                }
            }
        }

        private static RowValue ReadLargeValue(sqlite3 db, Stream logicalStream, string stagingTableName, JasonQueryDbStorageMigrationValueKind kind,
                                               long totalLength, ModernStorageV2LogicalDigest sourceDigest)
        {
            if (totalLength > int.MaxValue)
            {
                throw new NotSupportedException("The logical value exceeds the qualified sqlite3_blob length range.");
            }

            if (kind == JasonQueryDbStorageMigrationValueKind.TextUtf8)
            {
                sourceDigest.BeginText(totalLength);
            }

            if (kind == JasonQueryDbStorageMigrationValueKind.Blob)
            {
                sourceDigest.BeginBlob(totalLength);
            }

            var stagingValueId = CreateStagingValue(db, stagingTableName, checked((int)totalLength));

            sqlite3_blob stagingBlob = null;
            Decoder decoder = kind == JasonQueryDbStorageMigrationValueKind.TextUtf8 ? StrictUtf8.GetDecoder() : null;

            char[] chars = decoder == null ? null : new char[JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes];
            long written = 0;

            try
            {
                if (totalLength > 0)
                {
                    ModernSqlCipherRuntime.CheckRc
                    (
                        db,
                        raw.sqlite3_blob_open(db, "main", stagingTableName, "Payload", stagingValueId, 1, out stagingBlob),
                        "open encrypted staging value"
                    );

                    if (raw.sqlite3_blob_bytes(stagingBlob) != checked((int)totalLength))
                    {
                        throw new InvalidDataException("The encrypted staging value length is incorrect.");
                    }
                }

                while (written < totalLength)
                {
                    using (var chunkFrame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(logicalStream))
                    {
                        var expectedChunkKind = kind == JasonQueryDbStorageMigrationValueKind.TextUtf8
                            ? JasonQueryDbStorageMigrationLogicalFrameKind.TextUtf8Chunk
                            : JasonQueryDbStorageMigrationLogicalFrameKind.BlobChunk;

                        if (chunkFrame.Kind != expectedChunkKind)
                        {
                            throw new InvalidDataException("A large logical value ended before its declared byte length.");
                        }

                        using (var chunk = chunkFrame.GetPayload<JasonQueryDbStorageMigrationLogicalChunk>())
                        {
                            var bytes = chunk.Bytes;

                            if (bytes.LongLength > totalLength - written)
                            {
                                throw new InvalidDataException("A logical value chunk exceeds its declared total byte length.");
                            }

                            if (decoder != null)
                            {
                                ValidateUtf8Chunk(decoder, bytes, chars, false);
                            }

                            sourceDigest.AddValueChunk(bytes, 0, bytes.Length);

                            if (bytes.Length > 0)
                            {
                                ModernSqlCipherRuntime.CheckRc
                                (
                                    db,
                                    raw.sqlite3_blob_write
                                    (
                                        stagingBlob,
                                        new ReadOnlySpan<byte>(bytes),
                                        checked((int)written)
                                    ),
                                    "write encrypted staging value"
                                );
                            }

                            written += bytes.Length;
                        }
                    }
                }

                var expectedEndKind = kind == JasonQueryDbStorageMigrationValueKind.TextUtf8
                    ? JasonQueryDbStorageMigrationLogicalFrameKind.EndTextUtf8
                    : JasonQueryDbStorageMigrationLogicalFrameKind.EndBlob;

                using (ReadExpectedFrame(logicalStream, expectedEndKind))
                {
                }

                if (decoder != null)
                {
                    ValidateUtf8Chunk(decoder, EmptyBytes, chars, true);
                }

                return RowValue.Staged(kind, stagingValueId, totalLength);
            }
            finally
            {
                ModernSqlCipherRuntime.CloseBlob(stagingBlob);

                if (chars != null)
                {
                    Array.Clear(chars, 0, chars.Length);
                }
            }
        }

        private static void InsertTargetRow(sqlite3 db, TableState table, List<TargetColumn> columns,
                                            RowValue[] values, string stagingTableName, long rowId)
        {
            sqlite3_stmt statement = null;

            try
            {
                var sql = BuildInsertSql(table.Name, columns, values);

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare target row insert");

                for (var index = 0; index < values.Length; index++)
                {
                    BindRowValue(db, statement, index + 1, values[index]);
                }

                ModernSqlCipherRuntime.CheckStepDone(db, statement, "insert target row");
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];

                if (!value.IsStaged)
                {
                    continue;
                }

                CopyStagedValueToTarget
                (
                    db,
                    stagingTableName,
                    value,
                    table.Name,
                    columns[index].Name,
                    rowId
                );

                if (value.Kind == JasonQueryDbStorageMigrationValueKind.TextUtf8)
                {
                    CastTargetColumnToText(db, table, columns[index].Name, rowId);
                }

                DeleteStagingValue(db, stagingTableName, value.StagingValueId);
            }
        }

        private static List<JasonQueryDbStorageMigrationSequenceEntry> ReadAndReplaySequenceState(sqlite3 db, Stream logicalStream, List<TableState> tables, ModernStorageV2LogicalDigest sourceDigest)
        {
            int entryCount;

            using (var beginFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSequenceState))
            {
                entryCount = beginFrame.GetPayload<int>();
            }

            var autoincrementTables = new HashSet<string>(StringComparer.Ordinal);

            foreach (var table in tables)
            {
                if (table.HasAutoincrement)
                {
                    autoincrementTables.Add(table.Name);
                }
            }

            if (entryCount > 0 && autoincrementTables.Count == 0)
            {
                throw new InvalidDataException("sqlite_sequence state was streamed without an AUTOINCREMENT table.");
            }

            if (autoincrementTables.Count > 0)
            {
                ModernSqlCipherRuntime.Exec(db, "DELETE FROM sqlite_sequence;");
            }

            var entries = new List<JasonQueryDbStorageMigrationSequenceEntry>(entryCount);
            string previousName = null;

            for (var index = 0; index < entryCount; index++)
            {
                using (var entryFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.SequenceEntry))
                {
                    var entry = entryFrame.GetPayload<JasonQueryDbStorageMigrationSequenceEntry>();

                    EnsureStrictlyIncreasingIdentifier(previousName, entry.TableName, "sqlite_sequence table");
                    previousName = entry.TableName;

                    if (!autoincrementTables.Contains(entry.TableName))
                    {
                        throw new InvalidDataException("sqlite_sequence references a table that is not qualified as AUTOINCREMENT.");
                    }

                    InsertSequenceEntry(db, entry);
                    sourceDigest.AddSequence(entry.TableName, entry.SequenceValue);
                    entries.Add(entry);
                }
            }

            using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSequenceState))
            {
            }

            if (autoincrementTables.Count > 0)
            {
                var actualCount = ModernSqlCipherRuntime.QueryInt64(db, "SELECT count(*) FROM sqlite_sequence;");

                if (actualCount != entryCount)
                {
                    throw new InvalidDataException("The replayed sqlite_sequence row count is not exact.");
                }
            }

            return entries;
        }

        private static void ReadAndReplaySecondarySchema(sqlite3 db, Stream logicalStream, JasonQueryDbStorageMigrationBeginDatabase beginDatabase, List<TableState> tables)
        {
            int objectCount;

            using (var beginFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSecondarySchema))
            {
                objectCount = beginFrame.GetPayload<int>();
            }

            if (checked(beginDatabase.TableCount + objectCount) != beginDatabase.SchemaObjectCount)
            {
                throw new InvalidDataException("The secondary schema object count does not match BeginDatabase.");
            }

            var tableNames = new HashSet<string>(StringComparer.Ordinal);
            var viewNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var table in tables)
            {
                tableNames.Add(table.Name);
            }

            JasonQueryDbStorageMigrationSchemaObjectKind? previousKind = null;
            string previousName = null;

            for (var index = 0; index < objectCount; index++)
            {
                using (var objectFrame = ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.SchemaObject))
                {
                    var schemaObject = objectFrame.GetPayload<JasonQueryDbStorageMigrationSecondarySchemaObject>();

                    if (schemaObject.Kind == JasonQueryDbStorageMigrationSchemaObjectKind.Index && !tableNames.Contains(schemaObject.TableName))
                    {
                        throw new InvalidDataException("A logical index references an unknown table.");
                    }

                    if (schemaObject.Kind == JasonQueryDbStorageMigrationSchemaObjectKind.View && !string.Equals(schemaObject.TableName, schemaObject.Name, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("A logical view must use its own name as sqlite_schema.tbl_name.");
                    }

                    if (schemaObject.Kind == JasonQueryDbStorageMigrationSchemaObjectKind.Trigger && !tableNames.Contains(schemaObject.TableName) && !viewNames.Contains(schemaObject.TableName))
                    {
                        throw new InvalidDataException("A logical trigger references an unknown table or view.");
                    }

                    EnsureSecondarySchemaOrder(previousKind, previousName, schemaObject.Kind, schemaObject.Name);
                    previousKind = schemaObject.Kind;
                    previousName = schemaObject.Name;
                    EnsureCreateSqlPrefix(schemaObject.OriginalSql, GetSchemaKindSqlToken(schemaObject.Kind));
                    ModernSqlCipherRuntime.ExecSingleStatement(db, schemaObject.OriginalSql, "replay secondary schema object");

                    VerifySchemaObject
                    (
                        db,
                        GetSchemaKindSqliteName(schemaObject.Kind),
                        schemaObject.Name,
                        schemaObject.TableName,
                        schemaObject.OriginalSql
                    );

                    if (schemaObject.Kind == JasonQueryDbStorageMigrationSchemaObjectKind.View && !viewNames.Add(schemaObject.Name))
                    {
                        throw new InvalidDataException("A duplicate logical view name was found.");
                    }
                }
            }

            using (ReadExpectedFrame(logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSecondarySchema))
            {
            }
        }

        private static void ValidateClosedCandidate(string databasePath, byte[] databasePasswordUtf8, CandidateState state)
        {
            if (ModernSqlCipherRuntime.StartsWithPlainSqliteHeader(databasePath))
            {
                throw new InvalidDataException("The Storage V2 candidate unexpectedly contains a plaintext SQLite header.");
            }

            ModernSqlCipherRuntime.EnsureNoSidecars(databasePath);

            if (!ModernSqlCipherRuntime.IsReadRejectedWithoutKey(databasePath))
            {
                throw new InvalidDataException("The Storage V2 candidate can be read without a database password.");
            }

            if (ModernSqlCipherRuntime.ContainsByteSequence(databasePath, databasePasswordUtf8))
            {
                throw new InvalidDataException("The Storage V2 candidate contains the database password byte sequence.");
            }

            var wrongPasswordUtf8 = CreateDistinctDatabasePassword(databasePasswordUtf8);

            try
            {
                if (!ModernSqlCipherRuntime.IsReadRejectedWithKey(databasePath, wrongPasswordUtf8))
                {
                    throw new InvalidDataException("The Storage V2 candidate can be read with a wrong database password.");
                }
            }
            finally
            {
                ModernSqlCipherRuntime.ClearBytes(wrongPasswordUtf8);
            }

            sqlite3 scanDb = null;
            sqlite3 valueDb = null;

            try
            {
                scanDb = ModernSqlCipherRuntime.OpenReadOnly(databasePath, databasePasswordUtf8);
                valueDb = ModernSqlCipherRuntime.OpenReadOnly(databasePath, databasePasswordUtf8);

                ModernSqlCipherRuntime.Exec(scanDb, "PRAGMA temp_store = MEMORY;");
                ModernSqlCipherRuntime.Exec(valueDb, "PRAGMA temp_store = MEMORY;");
                ModernSqlCipherRuntime.ValidateFrozenProfile(scanDb);
                ModernSqlCipherRuntime.ValidateIntegrity(scanDb);

                var encoding = ModernSqlCipherRuntime.QueryText(scanDb, "PRAGMA encoding;");

                JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSourceEncoding(encoding);

                var targetMetadata = new JasonQueryDbStorageMigrationDatabaseMetadata
                (
                    checked((int)ModernSqlCipherRuntime.QueryInt64(scanDb, "PRAGMA user_version;")),
                    checked((int)ModernSqlCipherRuntime.QueryInt64(scanDb, "PRAGMA application_id;")),
                    encoding
                );

                using (var targetDigest = new ModernStorageV2LogicalDigest())
                {
                    targetDigest.AddDatabaseMetadata(targetMetadata);

                    foreach (var table in state.Tables)
                    {
                        ScanTargetTable(scanDb, valueDb, table, targetDigest);
                    }

                    ValidateTargetSequenceState(scanDb, state.SequenceEntries, targetDigest);

                    var targetDigestValue = targetDigest.Finish();

                    if (!string.Equals(state.SourceLogicalDigest, targetDigestValue, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("The Storage V2 candidate logical digest does not match the source logical stream.");
                    }
                }
            }
            finally
            {
                ModernSqlCipherRuntime.CloseDatabase(valueDb);
                ModernSqlCipherRuntime.CloseDatabase(scanDb);
            }

            ModernSqlCipherRuntime.EnsureNoSidecars(databasePath);
        }

        private static void ScanTargetTable(sqlite3 scanDb, sqlite3 valueDb, TableState table, ModernStorageV2LogicalDigest digest)
        {
            var columns = ReadTargetColumns(valueDb, table.Name);

            if (columns.Count != table.ColumnCount)
            {
                throw new InvalidDataException("The candidate table column count changed before final validation.");
            }

            digest.BeginTable(table.TableId, table.Name, table.ColumnCount);

            sqlite3_stmt rowIdStatement = null;
            long rowCount = 0;
            long previousRowId = 0;
            var hasPreviousRowId = false;

            try
            {
                var sql = "SELECT " + ModernSqlCipherRuntime.QuoteIdentifier(table.RowIdColumnName)
                          + " FROM " + ModernSqlCipherRuntime.QuoteIdentifier(table.Name)
                          + " ORDER BY " + ModernSqlCipherRuntime.QuoteIdentifier(table.RowIdColumnName)
                          + ";";

                ModernSqlCipherRuntime.CheckRc
                (
                    scanDb,
                    raw.sqlite3_prepare_v2(scanDb, sql, out rowIdStatement),
                    "prepare candidate rowid scan"
                );

                while (true)
                {
                    var rc = raw.sqlite3_step(rowIdStatement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(scanDb, rc, "candidate rowid scan");
                    }

                    var rowId = raw.sqlite3_column_int64(rowIdStatement, 0);

                    if (hasPreviousRowId && rowId <= previousRowId)
                    {
                        throw new InvalidDataException("Candidate rows are not strictly ordered by the rowid alias.");
                    }

                    AddTargetRowToDigest(valueDb, table, columns, rowId, digest);
                    previousRowId = rowId;
                    hasPreviousRowId = true;
                    rowCount = checked(rowCount + 1);
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(rowIdStatement);
            }

            if (rowCount != table.SourceRowCount)
            {
                throw new InvalidDataException("The candidate table row count does not match the source logical stream.");
            }

            digest.EndTable(rowCount);
        }

        private static void AddTargetRowToDigest(sqlite3 db, TableState table, List<TargetColumn> columns,
                                                 long rowId, ModernStorageV2LogicalDigest digest)
        {
            var values = ReadTargetRowDescriptors(db, table, columns, rowId);

            digest.BeginRow(values.Length);

            for (var index = 0; index < values.Length; index++)
            {
                var value = values[index];

                switch (value.Kind)
                {
                    case JasonQueryDbStorageMigrationValueKind.Null:
                        {
                            digest.AddNull();
                            break;
                        }
                    case JasonQueryDbStorageMigrationValueKind.Int64:
                        {
                            digest.AddInt64(value.Int64Value);
                            break;
                        }
                    case JasonQueryDbStorageMigrationValueKind.Double:
                        {
                            digest.AddDouble(value.DoubleValue);
                            break;
                        }
                    case JasonQueryDbStorageMigrationValueKind.TextUtf8:
                        {
                            digest.BeginText(value.Length);
                            AddTargetLargeValue(db, table.Name, columns[index].Name, rowId, value.Length, true, digest);
                            break;
                        }
                    case JasonQueryDbStorageMigrationValueKind.Blob:
                        {
                            digest.BeginBlob(value.Length);
                            AddTargetLargeValue(db, table.Name, columns[index].Name, rowId, value.Length, false, digest);
                            break;
                        }
                    default:
                        {
                            throw new InvalidDataException("The candidate contains an unsupported SQLite storage class.");
                        }
                }
            }

            digest.EndRow();
        }

        private static TargetValue[] ReadTargetRowDescriptors(sqlite3 db, TableState table, List<TargetColumn> columns, long rowId)
        {
            sqlite3_stmt statement = null;

            try
            {
                var sql = BuildTargetRowDescriptorSql(table, columns);

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare candidate row descriptor scan");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int64(statement, 1, rowId), "bind candidate rowid");

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    ModernSqlCipherRuntime.CheckRc(db, rc, "candidate row descriptor scan");
                    throw new InvalidDataException("The candidate row could not be re-read by its rowid alias.");
                }

                var values = new TargetValue[columns.Count];

                for (var index = 0; index < columns.Count; index++)
                {
                    var baseColumn = index * 3;
                    var storageClass = raw.sqlite3_column_text(statement, baseColumn).utf8_to_string();

                    if (string.Equals(storageClass, "null", StringComparison.Ordinal))
                    {
                        values[index] = TargetValue.Null();
                        continue;
                    }

                    if (string.Equals(storageClass, "integer", StringComparison.Ordinal))
                    {
                        values[index] = TargetValue.Int64(raw.sqlite3_column_int64(statement, baseColumn + 1));
                        continue;
                    }

                    if (string.Equals(storageClass, "real", StringComparison.Ordinal))
                    {
                        values[index] = TargetValue.Double(raw.sqlite3_column_double(statement, baseColumn + 1));
                        continue;
                    }

                    if (string.Equals(storageClass, "text", StringComparison.Ordinal))
                    {
                        values[index] = TargetValue.Large(JasonQueryDbStorageMigrationValueKind.TextUtf8, raw.sqlite3_column_int64(statement, baseColumn + 2));
                        continue;
                    }

                    if (string.Equals(storageClass, "blob", StringComparison.Ordinal))
                    {
                        values[index] = TargetValue.Large(JasonQueryDbStorageMigrationValueKind.Blob, raw.sqlite3_column_int64(statement, baseColumn + 2));
                        continue;
                    }

                    throw new InvalidDataException("The candidate contains an unknown SQLite storage class.");
                }

                rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_DONE)
                {
                    throw new InvalidDataException("The candidate rowid alias is not unique.");
                }

                return values;
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void AddTargetLargeValue(sqlite3 db, string tableName, string columnName, long rowId, long length, bool isText, ModernStorageV2LogicalDigest digest)
        {
            if (length < 0 || length > int.MaxValue)
            {
                throw new InvalidDataException("The candidate large-value length is outside the qualified sqlite3_blob range.");
            }

            if (length == 0)
            {
                if (isText)
                {
                    ValidateUtf8Chunk(StrictUtf8.GetDecoder(), EmptyBytes, new char[1], true);
                }

                return;
            }

            sqlite3_blob blob = null;
            var buffer = new byte[JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes];
            Decoder decoder = isText ? StrictUtf8.GetDecoder() : null;
            char[] chars = decoder == null ? null : new char[JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes];

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    db,
                    raw.sqlite3_blob_open(db, "main", tableName, columnName, rowId, 0, out blob),
                    "open candidate large value"
                );

                if (raw.sqlite3_blob_bytes(blob) != checked((int)length))
                {
                    throw new InvalidDataException("The candidate large-value byte length is inconsistent.");
                }

                long offset = 0;

                while (offset < length)
                {
                    var count = Math.Min(buffer.Length, checked((int)(length - offset)));
                    var span = new Span<byte>(buffer, 0, count);

                    ModernSqlCipherRuntime.CheckRc
                    (
                        db,
                        raw.sqlite3_blob_read(blob, span, checked((int)offset)),
                        "read candidate large value"
                    );

                    if (decoder != null)
                    {
                        ValidateUtf8Chunk(decoder, buffer, count, chars, false);
                    }

                    digest.AddValueChunk(buffer, 0, count);
                    offset += count;
                }

                if (decoder != null)
                {
                    ValidateUtf8Chunk(decoder, EmptyBytes, chars, true);
                }
            }
            finally
            {
                ModernSqlCipherRuntime.CloseBlob(blob);
                Array.Clear(buffer, 0, buffer.Length);

                if (chars != null)
                {
                    Array.Clear(chars, 0, chars.Length);
                }
            }
        }

        private static void ValidateTargetSequenceState(sqlite3 db, List<JasonQueryDbStorageMigrationSequenceEntry> entries, ModernStorageV2LogicalDigest digest)
        {
            var sqliteSequenceExists = ModernSqlCipherRuntime.QueryInt64
            (
                db,
                "SELECT count(*) FROM sqlite_schema WHERE type='table' AND name='sqlite_sequence';"
            ) != 0;

            if (!sqliteSequenceExists)
            {
                if (entries.Count != 0)
                {
                    throw new InvalidDataException("The candidate is missing sqlite_sequence state.");
                }

                return;
            }

            var actualCount = ModernSqlCipherRuntime.QueryInt64(db, "SELECT count(*) FROM sqlite_sequence;");

            if (actualCount != entries.Count)
            {
                throw new InvalidDataException("The candidate sqlite_sequence row count does not match the source.");
            }

            foreach (var entry in entries)
            {
                var actual = QuerySequenceValue(db, entry.TableName);

                if (actual != entry.SequenceValue)
                {
                    throw new InvalidDataException("The candidate sqlite_sequence value does not match the source.");
                }

                digest.AddSequence(entry.TableName, actual);
            }
        }

        private static List<TargetColumn> ReadTargetColumns(sqlite3 db, string tableName)
        {
            sqlite3_stmt statement = null;
            var columns = new List<TargetColumn>();

            try
            {
                var sql = "PRAGMA table_xinfo(" + ModernSqlCipherRuntime.QuoteIdentifier(tableName) + ");";

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare table_xinfo");

                while (true)
                {
                    var rc = raw.sqlite3_step(statement);

                    if (rc == raw.SQLITE_DONE)
                    {
                        break;
                    }

                    if (rc != raw.SQLITE_ROW)
                    {
                        ModernSqlCipherRuntime.CheckRc(db, rc, "table_xinfo");
                    }

                    var cid = checked((int)raw.sqlite3_column_int64(statement, 0));
                    var name = raw.sqlite3_column_text(statement, 1).utf8_to_string();
                    var declaredType = raw.sqlite3_column_type(statement, 2) == raw.SQLITE_NULL ? string.Empty : raw.sqlite3_column_text(statement, 2).utf8_to_string();
                    var notNull = raw.sqlite3_column_int64(statement, 3) != 0;
                    var defaultSql = raw.sqlite3_column_type(statement, 4) == raw.SQLITE_NULL ? null : raw.sqlite3_column_text(statement, 4).utf8_to_string();
                    var primaryKeyOrdinal = checked((int)raw.sqlite3_column_int64(statement, 5));
                    var hidden = checked((int)raw.sqlite3_column_int64(statement, 6));

                    columns.Add(new TargetColumn(cid, name, declaredType, notNull, primaryKeyOrdinal, hidden, defaultSql));
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }

            columns.Sort((left, right) => left.Cid.CompareTo(right.Cid));

            for (var index = 0; index < columns.Count; index++)
            {
                if (columns[index].Cid != index)
                {
                    throw new InvalidDataException("The candidate table contains non-contiguous column IDs.");
                }
            }

            return columns;
        }

        private static void VerifyColumnDefinition(JasonQueryDbStorageMigrationColumnDefinition source, TargetColumn target)
        {
            if (source.Cid != target.Cid || !string.Equals(source.Name, target.Name, StringComparison.Ordinal)
                || !string.Equals(source.DeclaredType ?? string.Empty, target.DeclaredType ?? string.Empty, StringComparison.Ordinal)
                || source.NotNull != target.NotNull || source.PrimaryKeyOrdinal != target.PrimaryKeyOrdinal || source.Hidden != target.Hidden
                || !string.Equals(source.DefaultSql, target.DefaultSql, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The replayed table structure does not match the logical schema metadata.");
            }
        }

        private static void VerifySchemaObject(sqlite3 db, string type, string name, string tableName, string originalSql)
        {
            sqlite3_stmt statement = null;
            byte[] typeBytes = null;
            byte[] nameBytes = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    db,
                    raw.sqlite3_prepare_v2
                    (
                        db,
                        "SELECT tbl_name,sql FROM sqlite_schema WHERE type=? AND name=?;",
                        out statement
                    ),
                    "prepare schema object verification"
                );

                typeBytes = StrictUtf8.GetBytes(type);
                nameBytes = StrictUtf8.GetBytes(name);
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_text(statement, 1, new ReadOnlySpan<byte>(typeBytes)), "bind schema type");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_text(statement, 2, new ReadOnlySpan<byte>(nameBytes)), "bind schema name");

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    ModernSqlCipherRuntime.CheckRc(db, rc, "schema object verification");
                    throw new InvalidDataException("The replayed schema object was not found.");
                }

                var actualTableName = raw.sqlite3_column_text(statement, 0).utf8_to_string();

                var actualSql = raw.sqlite3_column_type(statement, 1) == raw.SQLITE_NULL ? null : raw.sqlite3_column_text(statement, 1).utf8_to_string();

                if (!string.Equals(actualTableName, tableName, StringComparison.Ordinal) || !string.Equals(actualSql, originalSql, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The replayed sqlite_schema object does not exactly match the logical stream.");
                }

                if (raw.sqlite3_step(statement) != raw.SQLITE_DONE)
                {
                    throw new InvalidDataException("The replayed sqlite_schema object name is not unique.");
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
                ModernSqlCipherRuntime.ClearBytes(typeBytes);
                ModernSqlCipherRuntime.ClearBytes(nameBytes);
            }
        }

        private static string CreateEncryptedStagingTable(sqlite3 db)
        {
            string tableName;

            while (true)
            {
                tableName = "__jq_v2_stage_" + Guid.NewGuid().ToString("N");

                var sql = "SELECT count(*) FROM sqlite_schema WHERE name=" + QuoteSqlString(tableName) + ";";

                if (ModernSqlCipherRuntime.QueryInt64(db, sql) == 0)
                {
                    break;
                }
            }

            ModernSqlCipherRuntime.ExecSingleStatement
            (
                db,
                "CREATE TABLE " + ModernSqlCipherRuntime.QuoteIdentifier(tableName) +
                "(ValueId INTEGER PRIMARY KEY, Payload BLOB NOT NULL);",
                "create encrypted migration staging table"
            );

            return tableName;
        }

        private static long CreateStagingValue(sqlite3 db, string stagingTableName, int length)
        {
            sqlite3_stmt statement = null;

            try
            {
                var sql = "INSERT INTO " + ModernSqlCipherRuntime.QuoteIdentifier(stagingTableName) + "(Payload) VALUES(zeroblob(?));";

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare encrypted staging insert");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int(statement, 1, length), "bind encrypted staging length");
                ModernSqlCipherRuntime.CheckStepDone(db, statement, "insert encrypted staging value");

                return raw.sqlite3_last_insert_rowid(db);
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void CopyStagedValueToTarget(sqlite3 db, string stagingTableName, RowValue value, string targetTableName, string targetColumnName, long targetRowId)
        {
            if (value.Length == 0)
            {
                return;
            }

            sqlite3_blob sourceBlob = null;
            sqlite3_blob targetBlob = null;
            var buffer = new byte[JasonQueryDbStorageMigrationLogicalStreamContract.MaxValueChunkBytes];

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    db,
                    raw.sqlite3_blob_open(db, "main", stagingTableName, "Payload", value.StagingValueId, 0, out sourceBlob),
                    "open encrypted staging value for read"
                );

                ModernSqlCipherRuntime.CheckRc
                (
                    db,
                    raw.sqlite3_blob_open(db, "main", targetTableName, targetColumnName, targetRowId, 1, out targetBlob),
                    "open candidate value for incremental write"
                );

                var expectedLength = checked((int)value.Length);

                if (raw.sqlite3_blob_bytes(sourceBlob) != expectedLength || raw.sqlite3_blob_bytes(targetBlob) != expectedLength)
                {
                    throw new InvalidDataException("The staged or target incremental value length is incorrect.");
                }

                var offset = 0;

                while (offset < expectedLength)
                {
                    var count = Math.Min(buffer.Length, expectedLength - offset);
                    var span = new Span<byte>(buffer, 0, count);

                    ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_blob_read(sourceBlob, span, offset), "read encrypted staging chunk");

                    ModernSqlCipherRuntime.CheckRc
                    (
                        db,
                        raw.sqlite3_blob_write(targetBlob, new ReadOnlySpan<byte>(buffer, 0, count), offset),
                        "write candidate value chunk"
                    );

                    offset += count;
                }
            }
            finally
            {
                ModernSqlCipherRuntime.CloseBlob(targetBlob);
                ModernSqlCipherRuntime.CloseBlob(sourceBlob);
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        private static void CastTargetColumnToText(sqlite3 db, TableState table, string columnName, long rowId)
        {
            sqlite3_stmt statement = null;

            try
            {
                var quotedColumn = ModernSqlCipherRuntime.QuoteIdentifier(columnName);

                var sql = "UPDATE " + ModernSqlCipherRuntime.QuoteIdentifier(table.Name)
                          + " SET " + quotedColumn + "=CAST(" + quotedColumn + " AS TEXT) WHERE "
                          + ModernSqlCipherRuntime.QuoteIdentifier(table.RowIdColumnName) + "=?;";

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare candidate TEXT cast");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int64(statement, 1, rowId), "bind candidate TEXT rowid");
                ModernSqlCipherRuntime.CheckStepDone(db, statement, "cast candidate value to TEXT");

                if (raw.sqlite3_changes(db) != 1)
                {
                    throw new InvalidDataException("The candidate TEXT cast did not update exactly one row.");
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void DeleteStagingValue(sqlite3 db, string stagingTableName, long stagingValueId)
        {
            sqlite3_stmt statement = null;

            try
            {
                var sql = "DELETE FROM " + ModernSqlCipherRuntime.QuoteIdentifier(stagingTableName) + " WHERE ValueId=?;";

                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, sql, out statement), "prepare encrypted staging delete");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int64(statement, 1, stagingValueId), "bind encrypted staging ID");
                ModernSqlCipherRuntime.CheckStepDone(db, statement, "delete encrypted staging value");

                if (raw.sqlite3_changes(db) != 1)
                {
                    throw new InvalidDataException("The encrypted staging value was not deleted exactly once.");
                }
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
            }
        }

        private static void EnsureStagingTableEmpty(sqlite3 db, string stagingTableName)
        {
            var sql = "SELECT count(*) FROM " + ModernSqlCipherRuntime.QuoteIdentifier(stagingTableName) + ";";

            if (ModernSqlCipherRuntime.QueryInt64(db, sql) != 0)
            {
                throw new InvalidDataException("The encrypted migration staging table is not empty at end of replay.");
            }
        }

        private static string BuildInsertSql(string tableName, List<TargetColumn> columns, RowValue[] values)
        {
            var builder = new StringBuilder();

            builder.Append("INSERT INTO ");
            builder.Append(ModernSqlCipherRuntime.QuoteIdentifier(tableName));
            builder.Append('(');

            for (var index = 0; index < columns.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                builder.Append(ModernSqlCipherRuntime.QuoteIdentifier(columns[index].Name));
            }

            builder.Append(") VALUES(");

            for (var index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                builder.Append(values[index].IsStaged ? "zeroblob(?)" : "?");
            }

            builder.Append(");");
            return builder.ToString();
        }

        private static void BindRowValue(sqlite3 db, sqlite3_stmt statement, int parameterIndex, RowValue value)
        {
            switch (value.Kind)
            {
                case JasonQueryDbStorageMigrationValueKind.Null:
                    {
                        ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_null(statement, parameterIndex), "bind NULL logical value");
                        return;
                    }
                case JasonQueryDbStorageMigrationValueKind.Int64:
                    {
                        ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int64(statement, parameterIndex, value.Int64Value), "bind Int64 logical value");
                        return;
                    }
                case JasonQueryDbStorageMigrationValueKind.Double:
                    {
                        ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_double(statement, parameterIndex, value.DoubleValue), "bind Double logical value");
                        return;
                    }
                case JasonQueryDbStorageMigrationValueKind.TextUtf8:
                case JasonQueryDbStorageMigrationValueKind.Blob:
                    {
                        ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int(statement, parameterIndex, checked((int)value.Length)), "bind staged logical value length");
                        return;
                    }
                default:
                    {
                        throw new InvalidDataException("An unsupported logical value kind reached target binding.");
                    }
            }
        }

        private static void InsertSequenceEntry(sqlite3 db, JasonQueryDbStorageMigrationSequenceEntry entry)
        {
            sqlite3_stmt statement = null;
            byte[] nameBytes = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc
                (
                    db,
                    raw.sqlite3_prepare_v2(db, "INSERT INTO sqlite_sequence(name,seq) VALUES(?,?);", out statement),
                    "prepare sqlite_sequence insert"
                );

                nameBytes = StrictUtf8.GetBytes(entry.TableName);
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_text(statement, 1, new ReadOnlySpan<byte>(nameBytes)), "bind sqlite_sequence table name");
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_int64(statement, 2, entry.SequenceValue), "bind sqlite_sequence value");
                ModernSqlCipherRuntime.CheckStepDone(db, statement, "insert sqlite_sequence state");
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
                ModernSqlCipherRuntime.ClearBytes(nameBytes);
            }
        }

        private static long QuerySequenceValue(sqlite3 db, string tableName)
        {
            sqlite3_stmt statement = null;
            byte[] nameBytes = null;

            try
            {
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_prepare_v2(db, "SELECT seq FROM sqlite_sequence WHERE name=?;", out statement), "prepare sqlite_sequence query");
                nameBytes = StrictUtf8.GetBytes(tableName);
                ModernSqlCipherRuntime.CheckRc(db, raw.sqlite3_bind_text(statement, 1, new ReadOnlySpan<byte>(nameBytes)), "bind sqlite_sequence query name");

                var rc = raw.sqlite3_step(statement);

                if (rc != raw.SQLITE_ROW)
                {
                    ModernSqlCipherRuntime.CheckRc(db, rc, "read sqlite_sequence state");
                    throw new InvalidDataException("The candidate sqlite_sequence entry is missing.");
                }

                var value = raw.sqlite3_column_int64(statement, 0);

                if (raw.sqlite3_step(statement) != raw.SQLITE_DONE)
                {
                    throw new InvalidDataException("The candidate contains duplicate sqlite_sequence entries.");
                }

                return value;
            }
            finally
            {
                ModernSqlCipherRuntime.FinalizeStatement(statement);
                ModernSqlCipherRuntime.ClearBytes(nameBytes);
            }
        }

        private static string BuildTargetRowDescriptorSql(TableState table, List<TargetColumn> columns)
        {
            var builder = new StringBuilder();

            builder.Append("SELECT ");

            for (var index = 0; index < columns.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                var quoted = ModernSqlCipherRuntime.QuoteIdentifier(columns[index].Name);

                builder.Append("typeof(+");
                builder.Append(quoted);
                builder.Append("),CASE typeof(+");
                builder.Append(quoted);
                builder.Append(") WHEN 'text' THEN NULL WHEN 'blob' THEN NULL ELSE +");
                builder.Append(quoted);
                builder.Append(" END,CASE WHEN typeof(+");
                builder.Append(quoted);
                builder.Append(") IN ('text','blob') THEN length(CAST(+");
                builder.Append(quoted);
                builder.Append(" AS BLOB)) ELSE NULL END");
            }

            builder.Append(" FROM ");
            builder.Append(ModernSqlCipherRuntime.QuoteIdentifier(table.Name));
            builder.Append(" WHERE ");
            builder.Append(ModernSqlCipherRuntime.QuoteIdentifier(table.RowIdColumnName));
            builder.Append("=?;");

            return builder.ToString();
        }

        private static void ValidateUtf8Chunk(Decoder decoder, byte[] bytes, char[] chars, bool flush)
        {
            ValidateUtf8Chunk(decoder, bytes, bytes.Length, chars, flush);
        }

        private static void ValidateUtf8Chunk(Decoder decoder, byte[] bytes, int count, char[] chars, bool flush)
        {
            int bytesUsed;
            int charsUsed;
            bool completed;

            decoder.Convert(bytes, 0, count, chars, 0, chars.Length, flush, out bytesUsed, out charsUsed, out completed);

            if (bytesUsed != count)
            {
                throw new InvalidDataException("Strict incremental UTF-8 validation did not consume the entire chunk.");
            }

            if (flush && !completed)
            {
                throw new InvalidDataException("Strict incremental UTF-8 validation did not complete at end-of-value.");
            }
        }

        private static JasonQueryDbStorageMigrationLogicalFrame ReadExpectedFrame(Stream logicalStream, JasonQueryDbStorageMigrationLogicalFrameKind expectedKind)
        {
            var frame = JasonQueryDbStorageMigrationLogicalStreamProtocol.ReadNextFrame(logicalStream);

            if (frame.Kind != expectedKind)
            {
                frame.Dispose();
                throw new InvalidDataException
                (
                    "Expected logical frame '" + expectedKind + "' but received '" + frame.Kind + "'."
                );
            }

            return frame;
        }

        private static void EnsureLogicalStreamEnded(Stream logicalStream)
        {
            var trailing = logicalStream.ReadByte();

            if (trailing != -1)
            {
                throw new InvalidDataException("Trailing bytes were found after the EndStream frame.");
            }
        }

        private static void EnsureCreateNewCandidatePath(string databasePath)
        {
            var directory = Path.GetDirectoryName(databasePath);

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException("The Storage V2 candidate directory does not exist.");
            }

            if (File.Exists(databasePath) || File.Exists(databasePath + "-journal") || File.Exists(databasePath + "-wal") || File.Exists(databasePath + "-shm"))
            {
                throw new IOException("The Storage V2 candidate path or one of its sidecars already exists.");
            }
        }

        private static byte[] CreateDistinctDatabasePassword(byte[] databasePasswordUtf8)
        {
            var wrong = new byte[databasePasswordUtf8.Length];

            Buffer.BlockCopy(databasePasswordUtf8, 0, wrong, 0, wrong.Length);
            wrong[0] = wrong[0] == (byte)'A' ? (byte)'B' : (byte)'A';

            return wrong;
        }

        private static void ValidateRequest(string databasePath, byte[] databasePasswordUtf8, Stream logicalStream)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A candidate database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length <= 0 || databasePasswordUtf8.Length > JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException("A bounded database password is required.", nameof(databasePasswordUtf8));
            }

            StrictUtf8.GetCharCount(databasePasswordUtf8);

            if (logicalStream == null || !logicalStream.CanRead)
            {
                throw new ArgumentException("A readable logical migration stream is required.", nameof(logicalStream));
            }
        }

        private static void EnsureStrictlyIncreasingIdentifier(string previous, string current, string description)
        {
            if (previous != null && JasonQueryDbStorageMigrationLogicalStreamContract.CompareIdentifiers(previous, current) >= 0)
            {
                throw new InvalidDataException("Logical " + description + " names are not in strict ordinal order.");
            }
        }

        private static void EnsureSecondarySchemaOrder(JasonQueryDbStorageMigrationSchemaObjectKind? previousKind, string previousName,
                                                       JasonQueryDbStorageMigrationSchemaObjectKind currentKind, string currentName)
        {
            if (!previousKind.HasValue)
            {
                return;
            }

            var previousRank = JasonQueryDbStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder(previousKind.Value);
            var currentRank = JasonQueryDbStorageMigrationLogicalStreamContract.GetSecondarySchemaReplayOrder(currentKind);

            if (currentRank < previousRank)
            {
                throw new InvalidDataException("Secondary schema objects are not in the frozen kind order.");
            }

            if (currentRank == previousRank && JasonQueryDbStorageMigrationLogicalStreamContract.CompareIdentifiers(previousName, currentName) >= 0)
            {
                throw new InvalidDataException("Secondary schema object names are not in strict ordinal order.");
            }
        }

        private static void EnsureCreateSqlPrefix(string sql, string requiredKindToken)
        {
            var trimmed = (sql ?? string.Empty).TrimStart();

            if (!trimmed.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase) || trimmed.IndexOf(requiredKindToken, StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidDataException("A streamed schema SQL value does not have the required CREATE kind.");
            }
        }

        private static string GetSchemaKindSqlToken(JasonQueryDbStorageMigrationSchemaObjectKind kind)
        {
            switch (kind)
            {
                case JasonQueryDbStorageMigrationSchemaObjectKind.Index:
                    {
                        return "INDEX";
                    }
                case JasonQueryDbStorageMigrationSchemaObjectKind.View:
                    {
                        return "VIEW";
                    }
                case JasonQueryDbStorageMigrationSchemaObjectKind.Trigger:
                    {
                        return "TRIGGER";
                    }
                default:
                    {
                        throw new NotSupportedException("The secondary schema object kind is not supported by the V2 writer.");
                    }
            }
        }

        private static string GetSchemaKindSqliteName(JasonQueryDbStorageMigrationSchemaObjectKind kind)
        {
            return GetSchemaKindSqlToken(kind).ToLowerInvariant();
        }

        private static string QuoteSqlString(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        private static void ClearRowValues(RowValue[] values)
        {
            if (values == null)
            {
                return;
            }

            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] != null)
                {
                    values[index].Clear();
                }
            }
        }

        private sealed class CandidateState
        {
            internal CandidateState(JasonQueryDbStorageMigrationDatabaseMetadata metadata, List<TableState> tables,
                                    List<JasonQueryDbStorageMigrationSequenceEntry> sequenceEntries, string sourceLogicalDigest)
            {
                Metadata = metadata;
                Tables = tables;
                SequenceEntries = sequenceEntries;
                SourceLogicalDigest = sourceLogicalDigest;
            }

            internal JasonQueryDbStorageMigrationDatabaseMetadata Metadata { get; }
            internal List<TableState> Tables { get; }
            internal List<JasonQueryDbStorageMigrationSequenceEntry> SequenceEntries { get; }
            internal string SourceLogicalDigest { get; }
        }

        private sealed class TableState
        {
            internal TableState(int tableId, string name, int columnCount, int rowIdAliasColumnCid, string rowIdColumnName, bool hasAutoincrement)
            {
                TableId = tableId;
                Name = name;
                ColumnCount = columnCount;
                RowIdAliasColumnCid = rowIdAliasColumnCid;
                RowIdColumnName = rowIdColumnName;
                HasAutoincrement = hasAutoincrement;
            }

            internal int TableId { get; }
            internal string Name { get; }
            internal int ColumnCount { get; }
            internal int RowIdAliasColumnCid { get; }
            internal string RowIdColumnName { get; }
            internal bool HasAutoincrement { get; }
            internal long SourceRowCount { get; set; }
        }

        private sealed class TargetColumn
        {
            internal TargetColumn(int cid, string name, string declaredType, bool notNull, int primaryKeyOrdinal, int hidden, string defaultSql)
            {
                Cid = cid;
                Name = name;
                DeclaredType = declaredType;
                NotNull = notNull;
                PrimaryKeyOrdinal = primaryKeyOrdinal;
                Hidden = hidden;
                DefaultSql = defaultSql;
            }

            internal int Cid { get; }
            internal string Name { get; }
            internal string DeclaredType { get; }
            internal bool NotNull { get; }
            internal int PrimaryKeyOrdinal { get; }
            internal int Hidden { get; }
            internal string DefaultSql { get; }
        }

        private sealed class RowValue
        {
            private RowValue(JasonQueryDbStorageMigrationValueKind kind)
            {
                Kind = kind;
            }

            internal JasonQueryDbStorageMigrationValueKind Kind { get; }
            internal long Int64Value { get; private set; }
            internal double DoubleValue { get; private set; }
            internal long StagingValueId { get; private set; }
            internal long Length { get; private set; }
            internal bool IsStaged => Kind == JasonQueryDbStorageMigrationValueKind.TextUtf8 || Kind == JasonQueryDbStorageMigrationValueKind.Blob;

            internal static RowValue Null()
            {
                return new RowValue(JasonQueryDbStorageMigrationValueKind.Null);
            }

            internal static RowValue Int64(long value)
            {
                return new RowValue(JasonQueryDbStorageMigrationValueKind.Int64) { Int64Value = value };
            }

            internal static RowValue Double(double value)
            {
                return new RowValue(JasonQueryDbStorageMigrationValueKind.Double) { DoubleValue = value };
            }

            internal static RowValue Staged(JasonQueryDbStorageMigrationValueKind kind, long stagingValueId, long length)
            {
                if (kind != JasonQueryDbStorageMigrationValueKind.TextUtf8 && kind != JasonQueryDbStorageMigrationValueKind.Blob)
                {
                    throw new ArgumentOutOfRangeException(nameof(kind));
                }

                return new RowValue(kind) { StagingValueId = stagingValueId, Length = length };
            }

            internal void Clear()
            {
                Int64Value = 0;
                DoubleValue = 0;
                StagingValueId = 0;
                Length = 0;
            }
        }

        private sealed class TargetValue
        {
            private TargetValue(JasonQueryDbStorageMigrationValueKind kind)
            {
                Kind = kind;
            }

            internal JasonQueryDbStorageMigrationValueKind Kind { get; }
            internal long Int64Value { get; private set; }
            internal double DoubleValue { get; private set; }
            internal long Length { get; private set; }

            internal static TargetValue Null()
            {
                return new TargetValue(JasonQueryDbStorageMigrationValueKind.Null);
            }

            internal static TargetValue Int64(long value)
            {
                return new TargetValue(JasonQueryDbStorageMigrationValueKind.Int64) { Int64Value = value };
            }

            internal static TargetValue Double(double value)
            {
                return new TargetValue(JasonQueryDbStorageMigrationValueKind.Double) { DoubleValue = value };
            }

            internal static TargetValue Large(JasonQueryDbStorageMigrationValueKind kind, long length)
            {
                if (length < 0)
                {
                    throw new InvalidDataException("The candidate large-value length is invalid.");
                }

                return new TargetValue(kind) { Length = length };
            }
        }
    }
}
