using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Binary encoding for logical Storage V1 schema/row stream frames.
    ///
    /// This class is provider-neutral and performs no database I/O.
    /// Variable-length payloads are length-validated before allocation.
    /// Large TEXT/BLOB values use bounded chunk frames.
    /// </summary>
    public static class DatabaseStorageMigrationLogicalStreamProtocol
    {
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes
        (
            DatabaseStorageMigrationLogicalStreamContract.Magic
        );

        private static readonly UTF8Encoding StrictUtf8 = DatabaseStorageMigrationLogicalStreamContract.GetStrictUtf8Encoding();

        public static void WriteBeginDatabase(Stream stream, DatabaseStorageMigrationDatabaseMetadata metadata,
                                              int tableCount, int schemaObjectCount)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureTableCount(tableCount);
            DatabaseStorageMigrationLogicalStreamContract.EnsureSchemaObjectCount(schemaObjectCount);

            if (schemaObjectCount < tableCount)
            {
                throw new InvalidDataException
                (
                    "The storage-migration schema object count cannot be smaller than the table count."
                );
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginDatabase);
                writer.Write(metadata.UserVersion);
                writer.Write(metadata.ApplicationId);

                WriteRequiredUtf8
                (
                    writer,
                    metadata.SourceEncodingName,
                    DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                    nameof(metadata.SourceEncodingName)
                );

                writer.Write(tableCount);
                writer.Write(schemaObjectCount);
                writer.Flush();
            }
        }

        public static void WriteBeginSchema(Stream stream, int tableCount)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureTableCount(tableCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginSchema);
                writer.Write(tableCount);
                writer.Flush();
            }
        }

        public static void WriteBeginTable(Stream stream, DatabaseStorageMigrationTableDefinition table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginTable);
                writer.Write(table.TableId);
                WriteIdentifier(writer, table.Name);
                WriteRequiredSql(writer, table.CreateSql);
                writer.Write(table.ColumnCount);
                writer.Write(table.RowIdAliasColumnCid);
                writer.Write(table.HasAutoincrement);
                writer.Flush();
            }
        }

        public static void WriteColumn(Stream stream, int tableId, DatabaseStorageMigrationColumnDefinition column)
        {
            if (tableId < 0 || tableId >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.Column);
                writer.Write(tableId);
                writer.Write(column.Cid);
                WriteIdentifier(writer, column.Name);

                WriteOptionalUtf8
                (
                    writer,
                    column.DeclaredType,
                    DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                    nameof(column.DeclaredType)
                );

                writer.Write(column.NotNull);
                writer.Write(column.PrimaryKeyOrdinal);
                writer.Write(column.Hidden);
                WriteNullableSql(writer, column.DefaultSql);
                writer.Flush();
            }
        }

        public static void WriteEndTable(Stream stream, int tableId)
        {
            WriteTableIdFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndTable, tableId);
        }

        public static void WriteEndSchema(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndSchema);
        }

        public static void WriteBeginRows(Stream stream, int tableId)
        {
            WriteTableIdFrame(stream, DatabaseStorageMigrationLogicalFrameKind.BeginRows, tableId);
        }

        public static void WriteBeginRow(Stream stream, int fieldCount)
        {
            if (fieldCount <= 0 || fieldCount > DatabaseStorageMigrationLogicalStreamContract.MaxFieldsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldCount));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginRow);
                writer.Write(fieldCount);
                writer.Flush();
            }
        }

        public static void WriteNullValue(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.NullValue);
        }

        public static void WriteInt64Value(Stream stream, long value)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.Int64Value);
                writer.Write(value);
                writer.Flush();
            }
        }

        public static void WriteDoubleValue(Stream stream, double value)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.DoubleValue);
                writer.Write(value);
                writer.Flush();
            }
        }

        public static void WriteBeginTextUtf8(Stream stream, long totalUtf8ByteLength)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(totalUtf8ByteLength);

            WriteLargeValueBeginFrame
            (
                stream,
                DatabaseStorageMigrationLogicalFrameKind.BeginTextUtf8,
                totalUtf8ByteLength
            );
        }

        public static void WriteTextUtf8Chunk(Stream stream, byte[] buffer, int offset, int count)
        {
            WriteChunkFrame
            (
                stream,
                DatabaseStorageMigrationLogicalFrameKind.TextUtf8Chunk,
                buffer,
                offset,
                count
            );
        }

        public static void WriteEndTextUtf8(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndTextUtf8);
        }

        public static void WriteBeginBlob(Stream stream, long totalByteLength)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureBlobByteLength(totalByteLength);

            WriteLargeValueBeginFrame
            (
                stream,
                DatabaseStorageMigrationLogicalFrameKind.BeginBlob,
                totalByteLength
            );
        }

        public static void WriteBlobChunk(Stream stream, byte[] buffer, int offset, int count)
        {
            WriteChunkFrame
            (
                stream,
                DatabaseStorageMigrationLogicalFrameKind.BlobChunk,
                buffer,
                offset,
                count
            );
        }

        public static void WriteEndBlob(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndBlob);
        }

        public static void WriteEndRow(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndRow);
        }

        public static void WriteEndRows(Stream stream, int tableId, long rowCount)
        {
            if (tableId < 0 || tableId >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureRowCount(rowCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.EndRows);
                writer.Write(tableId);
                writer.Write(rowCount);
                writer.Flush();
            }
        }

        public static void WriteBeginSequenceState(Stream stream, int entryCount)
        {
            if (entryCount < 0 || entryCount > DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(entryCount));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginSequenceState);
                writer.Write(entryCount);
                writer.Flush();
            }
        }

        public static void WriteSequenceEntry(Stream stream, DatabaseStorageMigrationSequenceEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.SequenceEntry);
                WriteIdentifier(writer, entry.TableName);
                writer.Write(entry.SequenceValue);
                writer.Flush();
            }
        }

        public static void WriteEndSequenceState(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndSequenceState);
        }

        public static void WriteBeginSecondarySchema(Stream stream, int objectCount)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSchemaObjectCount(objectCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.BeginSecondarySchema);
                writer.Write(objectCount);
                writer.Flush();
            }
        }

        public static void WriteSchemaObject(Stream stream, DatabaseStorageMigrationSecondarySchemaObject schemaObject)
        {
            if (schemaObject == null)
            {
                throw new ArgumentNullException(nameof(schemaObject));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, DatabaseStorageMigrationLogicalFrameKind.SchemaObject);
                writer.Write((byte)schemaObject.Kind);
                WriteIdentifier(writer, schemaObject.Name);
                WriteIdentifier(writer, schemaObject.TableName);
                WriteRequiredSql(writer, schemaObject.OriginalSql);
                writer.Flush();
            }
        }

        public static void WriteEndSecondarySchema(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndSecondarySchema);
        }

        public static void WriteEndDatabase(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndDatabase);
        }

        public static void WriteEndStream(Stream stream)
        {
            WriteEmptyFrame(stream, DatabaseStorageMigrationLogicalFrameKind.EndStream);
        }

        public static DatabaseStorageMigrationLogicalFrame ReadNextFrame(Stream stream)
        {
            ValidateReadableStream(stream);

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var frameKind = ReadAndValidateHeader(reader);

                switch (frameKind)
                {
                    case DatabaseStorageMigrationLogicalFrameKind.BeginDatabase:
                        {
                            return ReadBeginDatabaseFrame(reader);
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginSchema:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    DatabaseStorageMigrationLogicalStreamContract.MaxTables,
                                    "table count"
                                )
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginTable:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadTableDefinition(reader)
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.Column:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadColumnFrame(reader)
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.EndTable:
                    case DatabaseStorageMigrationLogicalFrameKind.BeginRows:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadTableId(reader)
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginRow:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadPositiveBoundedCount
                                (
                                    reader,
                                    DatabaseStorageMigrationLogicalStreamContract.MaxFieldsPerRow,
                                    "row field count"
                                )
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.Int64Value:
                        {
                            return new DatabaseStorageMigrationLogicalFrame(frameKind, reader.ReadInt64());
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.DoubleValue:
                        {
                            return new DatabaseStorageMigrationLogicalFrame(frameKind, reader.ReadDouble());
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginTextUtf8:
                        {
                            var length = reader.ReadInt64();
                            DatabaseStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(length);
                            return new DatabaseStorageMigrationLogicalFrame(frameKind, length);
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.TextUtf8Chunk:
                    case DatabaseStorageMigrationLogicalFrameKind.BlobChunk:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                new DatabaseStorageMigrationLogicalChunk(ReadChunk(reader))
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginBlob:
                        {
                            var length = reader.ReadInt64();
                            DatabaseStorageMigrationLogicalStreamContract.EnsureBlobByteLength(length);
                            return new DatabaseStorageMigrationLogicalFrame(frameKind, length);
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.EndRows:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadEndRowsFrame(reader)
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginSequenceState:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    DatabaseStorageMigrationLogicalStreamContract.MaxTables,
                                    "sequence entry count"
                                )
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.SequenceEntry:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                new DatabaseStorageMigrationSequenceEntry
                                (
                                    ReadIdentifier(reader, "sequence table name"),
                                    reader.ReadInt64()
                                )
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.BeginSecondarySchema:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    DatabaseStorageMigrationLogicalStreamContract.MaxSchemaObjects,
                                    "secondary schema object count"
                                )
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.SchemaObject:
                        {
                            return new DatabaseStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadSecondarySchemaObject(reader)
                            );
                        }
                    case DatabaseStorageMigrationLogicalFrameKind.EndSchema:
                    case DatabaseStorageMigrationLogicalFrameKind.NullValue:
                    case DatabaseStorageMigrationLogicalFrameKind.EndTextUtf8:
                    case DatabaseStorageMigrationLogicalFrameKind.EndBlob:
                    case DatabaseStorageMigrationLogicalFrameKind.EndRow:
                    case DatabaseStorageMigrationLogicalFrameKind.EndSequenceState:
                    case DatabaseStorageMigrationLogicalFrameKind.EndSecondarySchema:
                    case DatabaseStorageMigrationLogicalFrameKind.EndDatabase:
                    case DatabaseStorageMigrationLogicalFrameKind.EndStream:
                        {
                            return new DatabaseStorageMigrationLogicalFrame(frameKind, null);
                        }
                    default:
                        {
                            throw new NotSupportedException
                            (
                                $"Storage-migration logical frame kind '{(byte)frameKind}' is not supported."
                            );
                        }
                }
            }
        }

        private static DatabaseStorageMigrationLogicalFrame ReadBeginDatabaseFrame(BinaryReader reader)
        {
            var metadata = new DatabaseStorageMigrationDatabaseMetadata
            (
                reader.ReadInt32(),
                reader.ReadInt32(),
                ReadRequiredUtf8
                (
                    reader,
                    DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                    "source encoding"
                )
            );

            var tableCount = ReadBoundedCount
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxTables,
                "table count"
            );

            var schemaObjectCount = ReadBoundedCount
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxSchemaObjects,
                "schema object count"
            );

            if (schemaObjectCount < tableCount)
            {
                throw new InvalidDataException
                (
                    "The storage-migration schema object count cannot be smaller than the table count."
                );
            }

            return new DatabaseStorageMigrationLogicalFrame
            (
                DatabaseStorageMigrationLogicalFrameKind.BeginDatabase,
                new DatabaseStorageMigrationBeginDatabase
                (
                    metadata,
                    tableCount,
                    schemaObjectCount
                )
            );
        }

        private static DatabaseStorageMigrationTableDefinition ReadTableDefinition(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var name = ReadIdentifier(reader, "table name");
            var createSql = ReadRequiredSql(reader, "CREATE TABLE SQL");

            var columnCount = ReadPositiveBoundedCount
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxColumnsPerTable,
                "column count"
            );

            var rowIdAliasColumnCid = reader.ReadInt32();
            var hasAutoincrement = reader.ReadBoolean();

            return new DatabaseStorageMigrationTableDefinition
            (
                tableId,
                name,
                createSql,
                columnCount,
                rowIdAliasColumnCid,
                hasAutoincrement
            );
        }

        private static DatabaseStorageMigrationColumnFrame ReadColumnFrame(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var cid = reader.ReadInt32();
            var name = ReadIdentifier(reader, "column name");

            var declaredType = ReadOptionalUtf8
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                "declared type"
            );

            var notNull = reader.ReadBoolean();
            var primaryKeyOrdinal = reader.ReadInt32();
            var hidden = reader.ReadInt32();

            var defaultSql = ReadNullableUtf8
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                "column default SQL"
            );

            return new DatabaseStorageMigrationColumnFrame
            (
                tableId,
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

        private static DatabaseStorageMigrationEndRows ReadEndRowsFrame(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var rowCount = reader.ReadInt64();

            DatabaseStorageMigrationLogicalStreamContract.EnsureRowCount(rowCount);
            return new DatabaseStorageMigrationEndRows(tableId, rowCount);
        }

        private static DatabaseStorageMigrationSecondarySchemaObject ReadSecondarySchemaObject(BinaryReader reader)
        {
            var rawKind = reader.ReadByte();
            var kind = (DatabaseStorageMigrationSchemaObjectKind)rawKind;

            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind(kind);

            return new DatabaseStorageMigrationSecondarySchemaObject
            (
                kind,
                ReadIdentifier(reader, "schema object name"),
                ReadIdentifier(reader, "schema object table name"),
                ReadRequiredSql(reader, "secondary schema SQL")
            );
        }

        private static void WriteTableIdFrame(Stream stream, DatabaseStorageMigrationLogicalFrameKind frameKind, int tableId)
        {
            if (tableId < 0 || tableId >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Write(tableId);
                writer.Flush();
            }
        }

        private static void WriteLargeValueBeginFrame(Stream stream, DatabaseStorageMigrationLogicalFrameKind frameKind, long totalLength)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Write(totalLength);
                writer.Flush();
            }
        }

        private static void WriteChunkFrame(Stream stream, DatabaseStorageMigrationLogicalFrameKind frameKind,
                                            byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0 || count < 0 || offset > buffer.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureChunkByteLength(count);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Write(count);
                writer.Write(buffer, offset, count);
                writer.Flush();
            }
        }

        private static void WriteEmptyFrame(Stream stream, DatabaseStorageMigrationLogicalFrameKind frameKind)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Flush();
            }
        }

        private static void WriteHeader(BinaryWriter writer, DatabaseStorageMigrationLogicalFrameKind frameKind)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind(frameKind);

            writer.Write(MagicBytes);
            writer.Write(DatabaseStorageMigrationLogicalStreamContract.CurrentVersion);
            writer.Write((byte)frameKind);
        }

        private static DatabaseStorageMigrationLogicalFrameKind ReadAndValidateHeader(BinaryReader reader)
        {
            var magic = ReadExactly(reader, MagicBytes.Length);

            try
            {
                for (var index = 0; index < MagicBytes.Length; index++)
                {
                    if (magic[index] != MagicBytes[index])
                    {
                        throw new InvalidDataException("The storage-migration logical stream magic value is invalid.");
                    }
                }
            }
            finally
            {
                Array.Clear(magic, 0, magic.Length);
            }

            var protocolVersion = reader.ReadInt32();

            if (protocolVersion != DatabaseStorageMigrationLogicalStreamContract.CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage-migration logical stream protocol version '{protocolVersion}' is not supported."
                );
            }

            var frameKind = (DatabaseStorageMigrationLogicalFrameKind)reader.ReadByte();

            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind(frameKind);

            return frameKind;
        }

        private static BinaryWriter CreateWriter(Stream stream)
        {
            ValidateWritableStream(stream);
            return new BinaryWriter(stream, Encoding.UTF8, true);
        }

        private static void WriteIdentifier(BinaryWriter writer, string value)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(value, nameof(value));

            WriteRequiredUtf8
            (
                writer,
                value,
                DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                nameof(value)
            );
        }

        private static void WriteRequiredSql(BinaryWriter writer, string value)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureRequiredSql(value, nameof(value));

            WriteRequiredUtf8
            (
                writer,
                value,
                DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                nameof(value)
            );
        }

        private static void WriteNullableSql(BinaryWriter writer, string value)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureOptionalSql(value, nameof(value));

            WriteNullableUtf8
            (
                writer,
                value,
                DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                nameof(value)
            );
        }

        private static void WriteRequiredUtf8(BinaryWriter writer, string value, int maximumLength, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }

            WriteOptionalUtf8(writer, value, maximumLength, parameterName);
        }

        private static void WriteOptionalUtf8(BinaryWriter writer, string value, int maximumLength, string parameterName)
        {
            var bytes = StrictUtf8.GetBytes(value ?? string.Empty);

            try
            {
                if (bytes.Length > maximumLength)
                {
                    throw new ArgumentOutOfRangeException(parameterName);
                }

                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private static void WriteNullableUtf8(BinaryWriter writer, string value, int maximumLength, string parameterName)
        {
            if (value == null)
            {
                writer.Write(-1);
                return;
            }

            WriteOptionalUtf8(writer, value, maximumLength, parameterName);
        }

        private static string ReadIdentifier(BinaryReader reader, string fieldName)
        {
            var value = ReadRequiredUtf8
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                fieldName
            );

            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(value, fieldName);

            return value;
        }

        private static string ReadRequiredSql(BinaryReader reader, string fieldName)
        {
            var value = ReadRequiredUtf8
            (
                reader,
                DatabaseStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                fieldName
            );

            DatabaseStorageMigrationLogicalStreamContract.EnsureRequiredSql(value, fieldName);

            return value;
        }

        private static string ReadRequiredUtf8(BinaryReader reader, int maximumLength, string fieldName)
        {
            var value = ReadOptionalUtf8(reader, maximumLength, fieldName);

            if (value.Length == 0)
            {
                throw new InvalidDataException($"The storage-migration {fieldName} is empty.");
            }

            return value;
        }

        private static string ReadOptionalUtf8(BinaryReader reader, int maximumLength, string fieldName)
        {
            var length = reader.ReadInt32();

            if (length < 0 || length > maximumLength)
            {
                throw new InvalidDataException($"The storage-migration {fieldName} length is invalid.");
            }

            var bytes = ReadExactly(reader, length);

            try
            {
                return StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException
                (
                    $"The storage-migration {fieldName} is not valid strict UTF-8.",
                    ex
                );
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private static string ReadNullableUtf8(BinaryReader reader, int maximumLength, string fieldName)
        {
            var length = reader.ReadInt32();

            if (length == -1)
            {
                return null;
            }

            if (length < 0 || length > maximumLength)
            {
                throw new InvalidDataException($"The storage-migration {fieldName} length is invalid.");
            }

            var bytes = ReadExactly(reader, length);

            try
            {
                return StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException
                (
                    $"The storage-migration {fieldName} is not valid strict UTF-8.",
                    ex
                );
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private static byte[] ReadChunk(BinaryReader reader)
        {
            var length = reader.ReadInt32();

            DatabaseStorageMigrationLogicalStreamContract.EnsureChunkByteLength(length);

            return ReadExactly(reader, length);
        }

        private static int ReadTableId(BinaryReader reader)
        {
            var tableId = reader.ReadInt32();

            if (tableId < 0 || tableId >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new InvalidDataException("The storage-migration table id is invalid.");
            }

            return tableId;
        }

        private static int ReadBoundedCount(BinaryReader reader, int maximum, string fieldName)
        {
            var value = reader.ReadInt32();

            if (value < 0 || value > maximum)
            {
                throw new InvalidDataException($"The storage-migration {fieldName} is invalid.");
            }

            return value;
        }

        private static int ReadPositiveBoundedCount(BinaryReader reader, int maximum, string fieldName)
        {
            var value = reader.ReadInt32();

            if (value <= 0 || value > maximum)
            {
                throw new InvalidDataException($"The storage-migration {fieldName} is invalid.");
            }

            return value;
        }

        private static byte[] ReadExactly(BinaryReader reader, int length)
        {
            var value = reader.ReadBytes(length);

            if (value.Length != length)
            {
                Array.Clear(value, 0, value.Length);
                throw new EndOfStreamException("The storage-migration logical stream frame ended unexpectedly.");
            }

            return value;
        }

        private static void ValidateReadableStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException
                (
                    "The storage-migration logical stream must be readable.",
                    nameof(stream)
                );
            }
        }

        private static void ValidateWritableStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException
                (
                    "The storage-migration logical stream must be writable.",
                    nameof(stream)
                );
            }
        }
    }

    public sealed class DatabaseStorageMigrationLogicalFrame : IDisposable
    {
        internal DatabaseStorageMigrationLogicalFrame(DatabaseStorageMigrationLogicalFrameKind kind, object payload)
        {
            Kind = kind;
            Payload = payload;
        }

        public DatabaseStorageMigrationLogicalFrameKind Kind { get; }

        public object Payload { get; }

        public T GetPayload<T>()
        {
            if (!(Payload is T typedPayload))
            {
                throw new InvalidOperationException
                (
                    $"Logical frame '{Kind}' does not contain payload type '{typeof(T).FullName}'."
                );
            }

            return typedPayload;
        }

        public void Dispose()
        {
            if (Payload is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public sealed class DatabaseStorageMigrationBeginDatabase
    {
        internal DatabaseStorageMigrationBeginDatabase(DatabaseStorageMigrationDatabaseMetadata metadata,
                                                       int tableCount, int schemaObjectCount)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            TableCount = tableCount;
            SchemaObjectCount = schemaObjectCount;
        }

        public DatabaseStorageMigrationDatabaseMetadata Metadata { get; }

        public int TableCount { get; }

        public int SchemaObjectCount { get; }
    }

    public sealed class DatabaseStorageMigrationColumnFrame
    {
        internal DatabaseStorageMigrationColumnFrame(int tableId, DatabaseStorageMigrationColumnDefinition column)
        {
            TableId = tableId;
            Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public int TableId { get; }

        public DatabaseStorageMigrationColumnDefinition Column { get; }
    }

    public sealed class DatabaseStorageMigrationEndRows
    {
        internal DatabaseStorageMigrationEndRows(int tableId, long rowCount)
        {
            TableId = tableId;
            RowCount = rowCount;
        }

        public int TableId { get; }

        public long RowCount { get; }
    }

    public sealed class DatabaseStorageMigrationLogicalChunk : IDisposable
    {
        private byte[] _bytes;

        internal DatabaseStorageMigrationLogicalChunk(byte[] bytes)
        {
            _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public byte[] Bytes
        {
            get
            {
                if (_bytes == null)
                {
                    throw new ObjectDisposedException(nameof(DatabaseStorageMigrationLogicalChunk));
                }

                return _bytes;
            }
        }

        public void Dispose()
        {
            if (_bytes == null)
            {
                return;
            }

            Array.Clear(_bytes, 0, _bytes.Length);
            _bytes = null;
        }
    }
}
