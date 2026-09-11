using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Binary wire protocol shared by the modern migration orchestrator and
    /// the isolated historical Storage V1 helper.
    ///
    /// This type is intentionally self-contained so the legacy helper can
    /// compile it as a linked source file without referencing JasonQuery.exe.
    /// </summary>
    public static class DatabaseStorageMigrationWireProtocol
    {
        public const string Magic = "JQSM";

        public const int CurrentVersion = 1;

        public const int SourceStorageFormatVersion = 1;

        public const int TargetStorageFormatVersion = 2;

        public const int MaxDatabasePathUtf8Bytes = 32768;

        public const int MaxDatabasePasswordUtf8Bytes = 16384;

        public const int MaxResponseMessageUtf8Bytes = 4096;

        private const byte RequestFrameKind = 1;

        private const byte ResponseFrameKind = 2;

        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(Magic);

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static void WriteRequest(Stream stream, string databaseFilePath, byte[] databasePasswordUtf8)
        {
            ValidateWritableStream(stream);

            var databasePathBytes = EncodeRequiredUtf8
            (
                databaseFilePath,
                MaxDatabasePathUtf8Bytes,
                nameof(databaseFilePath)
            );

            ValidateSecretBytes(databasePasswordUtf8);

            try
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    WriteHeader(writer, RequestFrameKind);

                    writer.Write(SourceStorageFormatVersion);
                    writer.Write(TargetStorageFormatVersion);

                    writer.Write(databasePathBytes.Length);
                    writer.Write(databasePathBytes);

                    writer.Write(databasePasswordUtf8.Length);
                    writer.Write(databasePasswordUtf8);

                    writer.Flush();
                }
            }
            finally
            {
                Array.Clear(databasePathBytes, 0, databasePathBytes.Length);
            }
        }

        public static DatabaseStorageMigrationWireRequest ReadRequest(Stream stream)
        {
            ValidateReadableStream(stream);

            byte[] databasePathBytes = null;
            byte[] databasePasswordBytes = null;

            try
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    ReadAndValidateHeader(reader, RequestFrameKind);

                    var sourceStorageFormatVersion = reader.ReadInt32();
                    var targetStorageFormatVersion = reader.ReadInt32();

                    EnsureSupportedRoute
                    (
                        sourceStorageFormatVersion,
                        targetStorageFormatVersion
                    );

                    databasePathBytes = ReadRequiredBytes
                    (
                        reader,
                        MaxDatabasePathUtf8Bytes,
                        "database path"
                    );

                    databasePasswordBytes = ReadRequiredBytes
                    (
                        reader,
                        MaxDatabasePasswordUtf8Bytes,
                        "database password"
                    );
                }

                var databaseFilePath = StrictUtf8.GetString(databasePathBytes);

                if (string.IsNullOrWhiteSpace(databaseFilePath))
                {
                    throw new InvalidDataException
                    (
                        "The storage-migration database path is empty."
                    );
                }

                var request = new DatabaseStorageMigrationWireRequest
                (
                    databaseFilePath,
                    databasePasswordBytes
                );

                databasePasswordBytes = null;
                return request;
            }
            finally
            {
                if (databasePathBytes != null)
                {
                    Array.Clear
                    (
                        databasePathBytes,
                        0,
                        databasePathBytes.Length
                    );
                }

                if (databasePasswordBytes != null)
                {
                    Array.Clear
                    (
                        databasePasswordBytes,
                        0,
                        databasePasswordBytes.Length
                    );
                }
            }
        }

        public static void WriteControlReadyResponse(Stream stream)
        {
            WriteResponse
            (
                stream,
                DatabaseStorageMigrationWireResponseStatus.ControlChannelReady,
                "Legacy Storage V1 control channel ready."
            );
        }

        public static void WriteStorageV1ValidatedResponse(Stream stream)
        {
            WriteResponse
            (
                stream,
                DatabaseStorageMigrationWireResponseStatus.StorageV1Validated,
                "Legacy Storage V1 source validated read-only."
            );
        }

        public static void WriteResponse(Stream stream, DatabaseStorageMigrationWireResponseStatus status, string message)
        {
            ValidateWritableStream(stream);

            var messageBytes = EncodeOptionalUtf8
            (
                message,
                MaxResponseMessageUtf8Bytes,
                nameof(message)
            );

            try
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    WriteHeader(writer, ResponseFrameKind);

                    writer.Write((int)status);
                    writer.Write(messageBytes.Length);
                    writer.Write(messageBytes);

                    writer.Flush();
                }
            }
            finally
            {
                Array.Clear(messageBytes, 0, messageBytes.Length);
            }
        }

        public static DatabaseStorageMigrationWireResponse ReadResponse(Stream stream)
        {
            ValidateReadableStream(stream);

            byte[] messageBytes = null;

            try
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    ReadAndValidateHeader(reader, ResponseFrameKind);

                    var rawStatus = reader.ReadInt32();

                    if (!Enum.IsDefined(typeof(DatabaseStorageMigrationWireResponseStatus), rawStatus))
                    {
                        throw new NotSupportedException
                        (
                            $"Storage-migration wire response status '{rawStatus}' is not supported."
                        );
                    }

                    messageBytes = ReadOptionalBytes
                    (
                        reader,
                        MaxResponseMessageUtf8Bytes,
                        "response message"
                    );

                    return new DatabaseStorageMigrationWireResponse
                    (
                        (DatabaseStorageMigrationWireResponseStatus)rawStatus,
                        StrictUtf8.GetString(messageBytes)
                    );
                }
            }
            finally
            {
                if (messageBytes != null)
                {
                    Array.Clear
                    (
                        messageBytes,
                        0,
                        messageBytes.Length
                    );
                }
            }
        }

        private static void WriteHeader(BinaryWriter writer, byte frameKind)
        {
            writer.Write(MagicBytes);
            writer.Write(CurrentVersion);
            writer.Write(frameKind);
        }

        private static void ReadAndValidateHeader(BinaryReader reader, byte expectedFrameKind)
        {
            var magic = ReadExactly(reader, MagicBytes.Length);

            try
            {
                for (var index = 0; index < MagicBytes.Length; index++)
                {
                    if (magic[index] != MagicBytes[index])
                    {
                        throw new InvalidDataException
                        (
                            "The storage-migration wire magic value is invalid."
                        );
                    }
                }
            }
            finally
            {
                Array.Clear(magic, 0, magic.Length);
            }

            var protocolVersion = reader.ReadInt32();

            if (protocolVersion != CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage-migration wire protocol version " +
                    $"'{protocolVersion}' is not supported."
                );
            }

            var frameKind = reader.ReadByte();

            if (frameKind != expectedFrameKind)
            {
                throw new InvalidDataException
                (
                    $"Unexpected storage-migration wire frame kind '{frameKind}'."
                );
            }
        }

        private static void EnsureSupportedRoute(int sourceStorageFormatVersion, int targetStorageFormatVersion)
        {
            if (sourceStorageFormatVersion != SourceStorageFormatVersion || targetStorageFormatVersion != TargetStorageFormatVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage-migration wire route " +
                    $"'{sourceStorageFormatVersion} -> {targetStorageFormatVersion}' is not supported."
                );
            }
        }

        private static byte[] ReadRequiredBytes(BinaryReader reader, int maximumLength, string fieldName)
        {
            var value = ReadOptionalBytes(reader, maximumLength, fieldName);

            if (value.Length == 0)
            {
                throw new InvalidDataException
                (
                    $"The storage-migration {fieldName} is empty."
                );
            }

            return value;
        }

        private static byte[] ReadOptionalBytes(BinaryReader reader, int maximumLength, string fieldName)
        {
            var length = reader.ReadInt32();

            if (length < 0 || length > maximumLength)
            {
                throw new InvalidDataException
                (
                    $"The storage-migration {fieldName} length is invalid."
                );
            }

            return ReadExactly(reader, length);
        }

        private static byte[] ReadExactly(BinaryReader reader, int length)
        {
            var value = reader.ReadBytes(length);

            if (value.Length != length)
            {
                Array.Clear(value, 0, value.Length);

                throw new EndOfStreamException
                (
                    "The storage-migration wire frame ended unexpectedly."
                );
            }

            return value;
        }

        private static byte[] EncodeRequiredUtf8(string value, int maximumLength, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException
                (
                    "A non-empty value is required.",
                    parameterName
                );
            }

            return EncodeUtf8(value, maximumLength, parameterName);
        }

        private static byte[] EncodeOptionalUtf8(string value, int maximumLength, string parameterName)
        {
            return EncodeUtf8(value ?? string.Empty, maximumLength, parameterName);
        }

        private static byte[] EncodeUtf8(string value, int maximumLength, string parameterName)
        {
            var encoded = StrictUtf8.GetBytes(value);

            if (encoded.Length > maximumLength)
            {
                Array.Clear(encoded, 0, encoded.Length);

                throw new ArgumentOutOfRangeException
                (
                    parameterName,
                    "The UTF-8 value exceeds the storage-migration wire limit."
                );
            }

            return encoded;
        }

        private static void ValidateSecretBytes(byte[] databasePasswordUtf8)
        {
            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0 || databasePasswordUtf8.Length > MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "A valid UTF-8 database password payload is required.",
                    nameof(databasePasswordUtf8)
                );
            }
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
                    "The storage-migration wire stream must be readable.",
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
                    "The storage-migration wire stream must be writable.",
                    nameof(stream)
                );
            }
        }
    }

    public sealed class DatabaseStorageMigrationWireRequest : IDisposable
    {
        private byte[] _databasePasswordUtf8;

        internal DatabaseStorageMigrationWireRequest(string databaseFilePath, byte[] databasePasswordUtf8)
        {
            DatabaseFilePath = databaseFilePath ?? throw new ArgumentNullException(nameof(databaseFilePath));
            _databasePasswordUtf8 = databasePasswordUtf8 ?? throw new ArgumentNullException(nameof(databasePasswordUtf8));
        }

        public string DatabaseFilePath { get; }

        public byte[] DatabasePasswordUtf8
        {
            get
            {
                if (_databasePasswordUtf8 == null)
                {
                    throw new ObjectDisposedException
                    (
                        nameof(DatabaseStorageMigrationWireRequest)
                    );
                }

                return _databasePasswordUtf8;
            }
        }

        public void Dispose()
        {
            if (_databasePasswordUtf8 == null)
            {
                return;
            }

            Array.Clear
            (
                _databasePasswordUtf8,
                0,
                _databasePasswordUtf8.Length
            );

            _databasePasswordUtf8 = null;
        }
    }

    public enum DatabaseStorageMigrationWireResponseStatus
    {
        ControlChannelReady = 1,

        StorageV1Validated = 2
    }

    public sealed class DatabaseStorageMigrationWireResponse
    {
        internal DatabaseStorageMigrationWireResponse(DatabaseStorageMigrationWireResponseStatus status, string message)
        {
            Status = status;
            Message = message ?? string.Empty;
        }

        public DatabaseStorageMigrationWireResponseStatus Status { get; }

        public string Message { get; }
    }
}
