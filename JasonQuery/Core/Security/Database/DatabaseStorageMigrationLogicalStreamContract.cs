using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Frozen limits and fail-closed validation rules for the logical
    /// Storage V1 -> V2 migration stream.
    ///
    /// This contract describes the transport only. It does not open a
    /// database, create Storage V2, replace files, or update metadata.
    ///
    /// Deterministic order is ordinal table name, column cid, rowid-alias
    /// key value, ordinal sequence table name, then secondary objects in
    /// Index -> View -> Trigger order and ordinal name within each kind.
    /// </summary>
    public static class DatabaseStorageMigrationLogicalStreamContract
    {
        public const string Magic = DatabaseStorageMigrationWireProtocol.Magic;

        public const int CurrentVersion = DatabaseStorageMigrationWireProtocol.CurrentVersion;

        public const string RequiredSourceEncodingName = "UTF-8";

        public const int MaxSchemaObjects = 10000;

        public const int MaxTables = 4096;

        public const int MaxColumnsPerTable = 4096;

        public const int MaxFieldsPerRow = 4096;

        public const int MaxIdentifierUtf8Bytes = 32768;

        public const int MaxSqlUtf8Bytes = 4 * 1024 * 1024;

        public const long MaxTextUtf8Bytes = 1000000000L;

        public const long MaxBlobBytes = 1000000000L;

        public const int MaxValueChunkBytes = 64 * 1024;

        public const bool HasHardRowCountLimit = false;

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static void EnsureSupportedFrameKind(DatabaseStorageMigrationLogicalFrameKind frameKind)
        {
            if (!Enum.IsDefined(typeof(DatabaseStorageMigrationLogicalFrameKind), frameKind))
            {
                throw new NotSupportedException
                (
                    $"Storage-migration logical frame kind '{(byte)frameKind}' is not supported."
                );
            }
        }

        public static void EnsureSupportedSchemaObjectKind(DatabaseStorageMigrationSchemaObjectKind objectKind)
        {
            if (!Enum.IsDefined(typeof(DatabaseStorageMigrationSchemaObjectKind), objectKind))
            {
                throw new NotSupportedException
                (
                    $"Storage-migration schema object kind '{(byte)objectKind}' is not supported."
                );
            }
        }

        public static void EnsureSupportedSecondarySchemaObjectKind(DatabaseStorageMigrationSchemaObjectKind objectKind)
        {
            EnsureSupportedSchemaObjectKind(objectKind);

            if (objectKind == DatabaseStorageMigrationSchemaObjectKind.Table)
            {
                throw new NotSupportedException
                (
                    "Tables must be transferred through the table-schema frames, not as secondary schema objects."
                );
            }
        }

        public static void EnsureSupportedValueKind(DatabaseStorageMigrationValueKind valueKind)
        {
            if (!Enum.IsDefined(typeof(DatabaseStorageMigrationValueKind), valueKind))
            {
                throw new NotSupportedException
                (
                    $"Storage-migration value kind '{(byte)valueKind}' is not supported."
                );
            }
        }

        public static void EnsureSourceEncoding(string encodingName)
        {
            if (!string.Equals(encodingName, RequiredSourceEncodingName, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException
                (
                    $"Historical Storage V1 encoding '{encodingName ?? "<null>"}' is not supported."
                );
            }
        }

        public static void EnsureSchemaObjectCount(int count)
        {
            EnsureCountInRange(count, MaxSchemaObjects, "schema object count");
        }

        public static void EnsureTableCount(int count)
        {
            EnsureCountInRange(count, MaxTables, "table count");
        }

        public static void EnsureColumnCount(int count)
        {
            if (count <= 0 || count > MaxColumnsPerTable)
            {
                throw new InvalidDataException("The storage-migration column count is invalid.");
            }
        }

        public static void EnsureRowFieldCount(int fieldCount, int expectedColumnCount)
        {
            EnsureColumnCount(expectedColumnCount);

            if (fieldCount < 0 || fieldCount > MaxFieldsPerRow || fieldCount != expectedColumnCount)
            {
                throw new InvalidDataException
                (
                    "The storage-migration row field count does not match the table column count."
                );
            }
        }

        public static void EnsureRowCount(long rowCount)
        {
            if (rowCount < 0)
            {
                throw new InvalidDataException("The storage-migration row count is invalid.");
            }
        }

        public static void EnsureIdentifier(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("A non-empty identifier is required.", parameterName);
            }

            EnsureUtf8ByteCount(value, MaxIdentifierUtf8Bytes, parameterName, false);
        }

        public static void EnsureRequiredSql(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A non-empty SQL value is required.", parameterName);
            }

            EnsureUtf8ByteCount(value, MaxSqlUtf8Bytes, parameterName, false);
        }

        public static void EnsureOptionalSql(string value, string parameterName)
        {
            if (value == null)
            {
                return;
            }

            EnsureUtf8ByteCount(value, MaxSqlUtf8Bytes, parameterName, true);
        }

        public static void EnsureTextUtf8ByteLength(long length)
        {
            EnsureLargeValueLength(length, MaxTextUtf8Bytes, "text UTF-8");
        }

        public static void EnsureBlobByteLength(long length)
        {
            EnsureLargeValueLength(length, MaxBlobBytes, "BLOB");
        }

        public static void EnsureChunkByteLength(int length)
        {
            if (length <= 0 || length > MaxValueChunkBytes)
            {
                throw new InvalidDataException("The storage-migration value chunk length is invalid.");
            }
        }

        public static void EnsureSupportedHistoricalTableFeatures(bool withoutRowId, bool virtualTable, bool hasGeneratedColumns,
                                                                  int rowIdAliasColumnCid, int columnCount)
        {
            EnsureColumnCount(columnCount);

            if (withoutRowId)
            {
                throw new NotSupportedException("WITHOUT ROWID tables are not qualified by logical stream protocol V1.");
            }

            if (virtualTable)
            {
                throw new NotSupportedException("Virtual tables are not qualified by logical stream protocol V1.");
            }

            if (hasGeneratedColumns)
            {
                throw new NotSupportedException("Generated columns are not qualified by logical stream protocol V1.");
            }

            if (rowIdAliasColumnCid < 0 || rowIdAliasColumnCid >= columnCount)
            {
                throw new NotSupportedException
                (
                    "A qualified INTEGER PRIMARY KEY rowid-alias column is required by logical stream protocol V1."
                );
            }
        }

        public static int GetSecondarySchemaReplayOrder(DatabaseStorageMigrationSchemaObjectKind objectKind)
        {
            EnsureSupportedSecondarySchemaObjectKind(objectKind);

            if (objectKind == DatabaseStorageMigrationSchemaObjectKind.Index)
            {
                return 1;
            }

            if (objectKind == DatabaseStorageMigrationSchemaObjectKind.View)
            {
                return 2;
            }

            return 3;
        }

        public static int CompareIdentifiers(string left, string right)
        {
            return StringComparer.Ordinal.Compare(left, right);
        }

        public static Decoder CreateStrictUtf8Decoder()
        {
            return StrictUtf8.GetDecoder();
        }

        internal static UTF8Encoding GetStrictUtf8Encoding()
        {
            return StrictUtf8;
        }

        private static void EnsureCountInRange(int count, int maximum, string description)
        {
            if (count < 0 || count > maximum)
            {
                throw new InvalidDataException($"The storage-migration {description} is invalid.");
            }
        }

        private static void EnsureLargeValueLength(long length, long maximum, string description)
        {
            if (length < 0 || length > maximum)
            {
                throw new InvalidDataException($"The storage-migration {description} length is invalid.");
            }
        }

        private static void EnsureUtf8ByteCount(string value, int maximum, string parameterName, bool allowEmpty)
        {
            if (!allowEmpty && value.Length == 0)
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }

            int byteCount;

            try
            {
                byteCount = StrictUtf8.GetByteCount(value);
            }
            catch (EncoderFallbackException ex)
            {
                throw new ArgumentException("The value is not valid strict UTF-8 input.", parameterName, ex);
            }

            if (byteCount > maximum)
            {
                throw new ArgumentOutOfRangeException
                (
                    parameterName,
                    "The UTF-8 value exceeds the storage-migration logical stream limit."
                );
            }
        }
    }
}
