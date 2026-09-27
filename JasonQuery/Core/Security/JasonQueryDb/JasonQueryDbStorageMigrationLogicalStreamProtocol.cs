using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Binary encoding for logical Storage V1 schema/row stream frames.
    ///
    /// This class is provider-neutral and performs no database I/O.
    /// Variable-length payloads are length-validated before allocation.
    /// Large TEXT/BLOB values use bounded chunk frames.
    /// </summary>
    public static class JasonQueryDbStorageMigrationLogicalStreamProtocol
    {
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes
        (
            JasonQueryDbStorageMigrationLogicalStreamContract.Magic
        );

        private static readonly UTF8Encoding StrictUtf8 = JasonQueryDbStorageMigrationLogicalStreamContract.GetStrictUtf8Encoding();

        public static void WriteBeginDatabase(Stream stream, JasonQueryDbStorageMigrationDatabaseMetadata metadata,
                                              int tableCount, int schemaObjectCount)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureTableCount(tableCount);
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSchemaObjectCount(schemaObjectCount);

            if (schemaObjectCount < tableCount)
            {
                throw new InvalidDataException
                (
                    "The storage-migration schema object count cannot be smaller than the table count."
                );
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase);
                writer.Write(metadata.UserVersion);
                writer.Write(metadata.ApplicationId);

                WriteRequiredUtf8
                (
                    writer,
                    metadata.SourceEncodingName,
                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                    nameof(metadata.SourceEncodingName)
                );

                writer.Write(tableCount);
                writer.Write(schemaObjectCount);
                writer.Flush();
            }
        }

        public static void WriteBeginSchema(Stream stream, int tableCount)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureTableCount(tableCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSchema);
                writer.Write(tableCount);
                writer.Flush();
            }
        }

        public static void WriteBeginTable(Stream stream, JasonQueryDbStorageMigrationTableDefinition table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginTable);
                writer.Write(table.TableId);
                WriteIdentifier(writer, table.Name);
                WriteRequiredSql(writer, table.CreateSql);
                writer.Write(table.ColumnCount);
                writer.Write(table.RowIdAliasColumnCid);
                writer.Write(table.HasAutoincrement);
                writer.Flush();
            }
        }

        public static void WriteColumn(Stream stream, int tableId, JasonQueryDbStorageMigrationColumnDefinition column)
        {
            if (tableId < 0 || tableId >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.Column);
                writer.Write(tableId);
                writer.Write(column.Cid);
                WriteIdentifier(writer, column.Name);

                WriteOptionalUtf8
                (
                    writer,
                    column.DeclaredType,
                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
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
            WriteTableIdFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndTable, tableId);
        }

        public static void WriteEndSchema(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSchema);
        }

        public static void WriteBeginRows(Stream stream, int tableId)
        {
            WriteTableIdFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.BeginRows, tableId);
        }

        public static void WriteBeginRow(Stream stream, int fieldCount)
        {
            if (fieldCount <= 0 || fieldCount > JasonQueryDbStorageMigrationLogicalStreamContract.MaxFieldsPerRow)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldCount));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginRow);
                writer.Write(fieldCount);
                writer.Flush();
            }
        }

        public static void WriteNullValue(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.NullValue);
        }

        public static void WriteInt64Value(Stream stream, long value)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.Int64Value);
                writer.Write(value);
                writer.Flush();
            }
        }

        public static void WriteDoubleValue(Stream stream, double value)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.DoubleValue);
                writer.Write(value);
                writer.Flush();
            }
        }

        public static void WriteBeginTextUtf8(Stream stream, long totalUtf8ByteLength)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(totalUtf8ByteLength);

            WriteLargeValueBeginFrame
            (
                stream,
                JasonQueryDbStorageMigrationLogicalFrameKind.BeginTextUtf8,
                totalUtf8ByteLength
            );
        }

        public static void WriteTextUtf8Chunk(Stream stream, byte[] buffer, int offset, int count)
        {
            WriteChunkFrame
            (
                stream,
                JasonQueryDbStorageMigrationLogicalFrameKind.TextUtf8Chunk,
                buffer,
                offset,
                count
            );
        }

        public static void WriteEndTextUtf8(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndTextUtf8);
        }

        public static void WriteBeginBlob(Stream stream, long totalByteLength)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureBlobByteLength(totalByteLength);

            WriteLargeValueBeginFrame
            (
                stream,
                JasonQueryDbStorageMigrationLogicalFrameKind.BeginBlob,
                totalByteLength
            );
        }

        public static void WriteBlobChunk(Stream stream, byte[] buffer, int offset, int count)
        {
            WriteChunkFrame
            (
                stream,
                JasonQueryDbStorageMigrationLogicalFrameKind.BlobChunk,
                buffer,
                offset,
                count
            );
        }

        public static void WriteEndBlob(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndBlob);
        }

        public static void WriteEndRow(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndRow);
        }

        public static void WriteEndRows(Stream stream, int tableId, long rowCount)
        {
            if (tableId < 0 || tableId >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRowCount(rowCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.EndRows);
                writer.Write(tableId);
                writer.Write(rowCount);
                writer.Flush();
            }
        }

        public static void WriteBeginSequenceState(Stream stream, int entryCount)
        {
            if (entryCount < 0 || entryCount > JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(entryCount));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSequenceState);
                writer.Write(entryCount);
                writer.Flush();
            }
        }

        public static void WriteSequenceEntry(Stream stream, JasonQueryDbStorageMigrationSequenceEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.SequenceEntry);
                WriteIdentifier(writer, entry.TableName);
                writer.Write(entry.SequenceValue);
                writer.Flush();
            }
        }

        public static void WriteEndSequenceState(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSequenceState);
        }

        public static void WriteBeginSecondarySchema(Stream stream, int objectCount)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSchemaObjectCount(objectCount);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.BeginSecondarySchema);
                writer.Write(objectCount);
                writer.Flush();
            }
        }

        public static void WriteSchemaObject(Stream stream, JasonQueryDbStorageMigrationSecondarySchemaObject schemaObject)
        {
            if (schemaObject == null)
            {
                throw new ArgumentNullException(nameof(schemaObject));
            }

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, JasonQueryDbStorageMigrationLogicalFrameKind.SchemaObject);
                writer.Write((byte)schemaObject.Kind);
                WriteIdentifier(writer, schemaObject.Name);
                WriteIdentifier(writer, schemaObject.TableName);
                WriteRequiredSql(writer, schemaObject.OriginalSql);
                writer.Flush();
            }
        }

        public static void WriteEndSecondarySchema(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndSecondarySchema);
        }

        public static void WriteEndDatabase(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndDatabase);
        }

        public static void WriteEndStream(Stream stream)
        {
            WriteEmptyFrame(stream, JasonQueryDbStorageMigrationLogicalFrameKind.EndStream);
        }

        public static JasonQueryDbStorageMigrationLogicalFrame ReadNextFrame(Stream stream)
        {
            ValidateReadableStream(stream);

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var frameKind = ReadAndValidateHeader(reader);

                switch (frameKind)
                {
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase:
                        {
                            return ReadBeginDatabaseFrame(reader);
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginSchema:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables,
                                    "table count"
                                )
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginTable:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadTableDefinition(reader)
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.Column:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadColumnFrame(reader)
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndTable:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginRows:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadTableId(reader)
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginRow:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadPositiveBoundedCount
                                (
                                    reader,
                                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxFieldsPerRow,
                                    "row field count"
                                )
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.Int64Value:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame(frameKind, reader.ReadInt64());
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.DoubleValue:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame(frameKind, reader.ReadDouble());
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginTextUtf8:
                        {
                            var length = reader.ReadInt64();
                            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(length);
                            return new JasonQueryDbStorageMigrationLogicalFrame(frameKind, length);
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.TextUtf8Chunk:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BlobChunk:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                new JasonQueryDbStorageMigrationLogicalChunk(ReadChunk(reader))
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginBlob:
                        {
                            var length = reader.ReadInt64();
                            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureBlobByteLength(length);
                            return new JasonQueryDbStorageMigrationLogicalFrame(frameKind, length);
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndRows:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadEndRowsFrame(reader)
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginSequenceState:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables,
                                    "sequence entry count"
                                )
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.SequenceEntry:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                new JasonQueryDbStorageMigrationSequenceEntry
                                (
                                    ReadIdentifier(reader, "sequence table name"),
                                    reader.ReadInt64()
                                )
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.BeginSecondarySchema:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadBoundedCount
                                (
                                    reader,
                                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxSchemaObjects,
                                    "secondary schema object count"
                                )
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.SchemaObject:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame
                            (
                                frameKind,
                                ReadSecondarySchemaObject(reader)
                            );
                        }
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndSchema:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.NullValue:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndTextUtf8:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndBlob:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndRow:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndSequenceState:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndSecondarySchema:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndDatabase:
                    case JasonQueryDbStorageMigrationLogicalFrameKind.EndStream:
                        {
                            return new JasonQueryDbStorageMigrationLogicalFrame(frameKind, null);
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

        private static JasonQueryDbStorageMigrationLogicalFrame ReadBeginDatabaseFrame(BinaryReader reader)
        {
            var metadata = new JasonQueryDbStorageMigrationDatabaseMetadata
            (
                reader.ReadInt32(),
                reader.ReadInt32(),
                ReadRequiredUtf8
                (
                    reader,
                    JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                    "source encoding"
                )
            );

            var tableCount = ReadBoundedCount
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables,
                "table count"
            );

            var schemaObjectCount = ReadBoundedCount
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxSchemaObjects,
                "schema object count"
            );

            if (schemaObjectCount < tableCount)
            {
                throw new InvalidDataException
                (
                    "The storage-migration schema object count cannot be smaller than the table count."
                );
            }

            return new JasonQueryDbStorageMigrationLogicalFrame
            (
                JasonQueryDbStorageMigrationLogicalFrameKind.BeginDatabase,
                new JasonQueryDbStorageMigrationBeginDatabase
                (
                    metadata,
                    tableCount,
                    schemaObjectCount
                )
            );
        }

        private static JasonQueryDbStorageMigrationTableDefinition ReadTableDefinition(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var name = ReadIdentifier(reader, "table name");
            var createSql = ReadRequiredSql(reader, "CREATE TABLE SQL");

            var columnCount = ReadPositiveBoundedCount
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxColumnsPerTable,
                "column count"
            );

            var rowIdAliasColumnCid = reader.ReadInt32();
            var hasAutoincrement = reader.ReadBoolean();

            return new JasonQueryDbStorageMigrationTableDefinition
            (
                tableId,
                name,
                createSql,
                columnCount,
                rowIdAliasColumnCid,
                hasAutoincrement
            );
        }

        private static JasonQueryDbStorageMigrationColumnFrame ReadColumnFrame(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var cid = reader.ReadInt32();
            var name = ReadIdentifier(reader, "column name");

            var declaredType = ReadOptionalUtf8
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                "declared type"
            );

            var notNull = reader.ReadBoolean();
            var primaryKeyOrdinal = reader.ReadInt32();
            var hidden = reader.ReadInt32();

            var defaultSql = ReadNullableUtf8
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                "column default SQL"
            );

            return new JasonQueryDbStorageMigrationColumnFrame
            (
                tableId,
                new JasonQueryDbStorageMigrationColumnDefinition
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

        private static JasonQueryDbStorageMigrationEndRows ReadEndRowsFrame(BinaryReader reader)
        {
            var tableId = ReadTableId(reader);
            var rowCount = reader.ReadInt64();

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRowCount(rowCount);
            return new JasonQueryDbStorageMigrationEndRows(tableId, rowCount);
        }

        private static JasonQueryDbStorageMigrationSecondarySchemaObject ReadSecondarySchemaObject(BinaryReader reader)
        {
            var rawKind = reader.ReadByte();
            var kind = (JasonQueryDbStorageMigrationSchemaObjectKind)rawKind;

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind(kind);

            return new JasonQueryDbStorageMigrationSecondarySchemaObject
            (
                kind,
                ReadIdentifier(reader, "schema object name"),
                ReadIdentifier(reader, "schema object table name"),
                ReadRequiredSql(reader, "secondary schema SQL")
            );
        }

        private static void WriteTableIdFrame(Stream stream, JasonQueryDbStorageMigrationLogicalFrameKind frameKind, int tableId)
        {
            if (tableId < 0 || tableId >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
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

        private static void WriteLargeValueBeginFrame(Stream stream, JasonQueryDbStorageMigrationLogicalFrameKind frameKind, long totalLength)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Write(totalLength);
                writer.Flush();
            }
        }

        private static void WriteChunkFrame(Stream stream, JasonQueryDbStorageMigrationLogicalFrameKind frameKind,
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

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureChunkByteLength(count);

            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Write(count);
                writer.Write(buffer, offset, count);
                writer.Flush();
            }
        }

        private static void WriteEmptyFrame(Stream stream, JasonQueryDbStorageMigrationLogicalFrameKind frameKind)
        {
            using (var writer = CreateWriter(stream))
            {
                WriteHeader(writer, frameKind);
                writer.Flush();
            }
        }

        private static void WriteHeader(BinaryWriter writer, JasonQueryDbStorageMigrationLogicalFrameKind frameKind)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind(frameKind);

            writer.Write(MagicBytes);
            writer.Write(JasonQueryDbStorageMigrationLogicalStreamContract.CurrentVersion);
            writer.Write((byte)frameKind);
        }

        private static JasonQueryDbStorageMigrationLogicalFrameKind ReadAndValidateHeader(BinaryReader reader)
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

            if (protocolVersion != JasonQueryDbStorageMigrationLogicalStreamContract.CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage-migration logical stream protocol version '{protocolVersion}' is not supported."
                );
            }

            var frameKind = (JasonQueryDbStorageMigrationLogicalFrameKind)reader.ReadByte();

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedFrameKind(frameKind);

            return frameKind;
        }

        private static BinaryWriter CreateWriter(Stream stream)
        {
            ValidateWritableStream(stream);
            return new BinaryWriter(stream, Encoding.UTF8, true);
        }

        private static void WriteIdentifier(BinaryWriter writer, string value)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(value, nameof(value));

            WriteRequiredUtf8
            (
                writer,
                value,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                nameof(value)
            );
        }

        private static void WriteRequiredSql(BinaryWriter writer, string value)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRequiredSql(value, nameof(value));

            WriteRequiredUtf8
            (
                writer,
                value,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                nameof(value)
            );
        }

        private static void WriteNullableSql(BinaryWriter writer, string value)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureOptionalSql(value, nameof(value));

            WriteNullableUtf8
            (
                writer,
                value,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
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
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxIdentifierUtf8Bytes,
                fieldName
            );

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(value, fieldName);

            return value;
        }

        private static string ReadRequiredSql(BinaryReader reader, string fieldName)
        {
            var value = ReadRequiredUtf8
            (
                reader,
                JasonQueryDbStorageMigrationLogicalStreamContract.MaxSqlUtf8Bytes,
                fieldName
            );

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRequiredSql(value, fieldName);

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

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureChunkByteLength(length);

            return ReadExactly(reader, length);
        }

        private static int ReadTableId(BinaryReader reader)
        {
            var tableId = reader.ReadInt32();

            if (tableId < 0 || tableId >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
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

    public sealed class JasonQueryDbStorageMigrationLogicalFrame : IDisposable
    {
        internal JasonQueryDbStorageMigrationLogicalFrame(JasonQueryDbStorageMigrationLogicalFrameKind kind, object payload)
        {
            Kind = kind;
            Payload = payload;
        }

        public JasonQueryDbStorageMigrationLogicalFrameKind Kind { get; }

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

    public sealed class JasonQueryDbStorageMigrationBeginDatabase
    {
        internal JasonQueryDbStorageMigrationBeginDatabase(JasonQueryDbStorageMigrationDatabaseMetadata metadata,
                                                           int tableCount, int schemaObjectCount)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            TableCount = tableCount;
            SchemaObjectCount = schemaObjectCount;
        }

        public JasonQueryDbStorageMigrationDatabaseMetadata Metadata { get; }

        public int TableCount { get; }

        public int SchemaObjectCount { get; }
    }

    public sealed class JasonQueryDbStorageMigrationColumnFrame
    {
        internal JasonQueryDbStorageMigrationColumnFrame(int tableId, JasonQueryDbStorageMigrationColumnDefinition column)
        {
            TableId = tableId;
            Column = column ?? throw new ArgumentNullException(nameof(column));
        }

        public int TableId { get; }

        public JasonQueryDbStorageMigrationColumnDefinition Column { get; }
    }

    public sealed class JasonQueryDbStorageMigrationEndRows
    {
        internal JasonQueryDbStorageMigrationEndRows(int tableId, long rowCount)
        {
            TableId = tableId;
            RowCount = rowCount;
        }

        public int TableId { get; }

        public long RowCount { get; }
    }

    public sealed class JasonQueryDbStorageMigrationLogicalChunk : IDisposable
    {
        private byte[] _bytes;

        internal JasonQueryDbStorageMigrationLogicalChunk(byte[] bytes)
        {
            _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public byte[] Bytes
        {
            get
            {
                if (_bytes == null)
                {
                    throw new ObjectDisposedException(nameof(JasonQueryDbStorageMigrationLogicalChunk));
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
