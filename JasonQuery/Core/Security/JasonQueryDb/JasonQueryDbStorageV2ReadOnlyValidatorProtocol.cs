using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Binary control protocol for read-only validation of an existing Storage V2 candidate.
    /// Path and database password are sent only through the binary standard-input channel.
    /// </summary>
    public static class JasonQueryDbStorageV2ReadOnlyValidatorProtocol
    {
        public const string Magic = "JQMV";
        public const int CurrentVersion = 1;
        public const int TargetStorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;
        public const int MaxDatabasePathUtf8Bytes = JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePathUtf8Bytes;
        public const int MaxDatabasePasswordUtf8Bytes = JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePasswordUtf8Bytes;
        public const int MaxResponseMessageUtf8Bytes = JasonQueryDbStorageMigrationWireProtocol.MaxResponseMessageUtf8Bytes;

        private const byte RequestFrameKind = 1;
        private const byte ResponseFrameKind = 2;
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(Magic);
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static void WriteRequest(Stream stream, string databaseFilePath, byte[] databasePasswordUtf8)
        {
            ValidateWritableStream(stream);

            var pathBytes = EncodeRequiredUtf8
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
                    writer.Write(TargetStorageFormatVersion);
                    writer.Write(pathBytes.Length);
                    writer.Write(pathBytes);
                    writer.Write(databasePasswordUtf8.Length);
                    writer.Write(databasePasswordUtf8);
                    writer.Flush();
                }
            }
            finally
            {
                Array.Clear(pathBytes, 0, pathBytes.Length);
            }
        }

        public static JasonQueryDbStorageV2ReadOnlyValidatorRequest ReadRequest(Stream stream)
        {
            ValidateReadableStream(stream);

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                ReadAndValidateHeader(reader, RequestFrameKind);

                var targetStorageFormatVersion = reader.ReadInt32();

                if (targetStorageFormatVersion != TargetStorageFormatVersion)
                {
                    throw new NotSupportedException
                    (
                        $"Storage V2 read-only validation target format '{targetStorageFormatVersion}' is not supported."
                    );
                }

                var pathBytes = ReadRequiredBytes
                (
                    reader,
                    MaxDatabasePathUtf8Bytes,
                    "database path"
                );

                byte[] passwordBytes = null;

                try
                {
                    string databaseFilePath;

                    try
                    {
                        databaseFilePath = StrictUtf8.GetString(pathBytes);
                    }
                    catch (DecoderFallbackException ex)
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V2 validation database path is not valid UTF-8.",
                            ex
                        );
                    }

                    if (string.IsNullOrWhiteSpace(databaseFilePath))
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V2 validation database path is empty."
                        );
                    }

                    passwordBytes = ReadRequiredBytes
                    (
                        reader,
                        MaxDatabasePasswordUtf8Bytes,
                        "database password"
                    );

                    ValidateSecretBytes(passwordBytes);

                    var request = new JasonQueryDbStorageV2ReadOnlyValidatorRequest
                    (
                        databaseFilePath,
                        passwordBytes
                    );

                    passwordBytes = null;
                    return request;
                }
                finally
                {
                    Array.Clear(pathBytes, 0, pathBytes.Length);

                    if (passwordBytes != null)
                    {
                        Array.Clear(passwordBytes, 0, passwordBytes.Length);
                    }
                }
            }
        }

        public static void WriteStorageV2ValidatedResponse(Stream stream)
        {
            WriteResponse
            (
                stream,
                JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus.StorageV2Validated,
                string.Empty
            );
        }

        public static JasonQueryDbStorageV2ReadOnlyValidatorResponse ReadResponse(Stream stream)
        {
            ValidateReadableStream(stream);

            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                ReadAndValidateHeader(reader, ResponseFrameKind);

                var statusValue = reader.ReadByte();

                if (!Enum.IsDefined(typeof(JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus), statusValue))
                {
                    throw new NotSupportedException
                    (
                        $"Storage V2 read-only validation response status '{statusValue}' is not supported."
                    );
                }

                var messageBytes = ReadOptionalBytes
                (
                    reader,
                    MaxResponseMessageUtf8Bytes,
                    "response message"
                );

                try
                {
                    string message;

                    try
                    {
                        message = StrictUtf8.GetString(messageBytes);
                    }
                    catch (DecoderFallbackException ex)
                    {
                        throw new InvalidDataException
                        (
                            "The Storage V2 read-only validation response message is not valid UTF-8.",
                            ex
                        );
                    }

                    return new JasonQueryDbStorageV2ReadOnlyValidatorResponse
                    (
                        (JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus)statusValue,
                        message
                    );
                }
                finally
                {
                    Array.Clear(messageBytes, 0, messageBytes.Length);
                }
            }
        }

        internal static bool MagicMatches(byte[] value)
        {
            if (value == null || value.Length != MagicBytes.Length)
            {
                return false;
            }

            for (var index = 0; index < MagicBytes.Length; index++)
            {
                if (value[index] != MagicBytes[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static void WriteResponse(Stream stream, JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus status, string message)
        {
            ValidateWritableStream(stream);

            if (!Enum.IsDefined(typeof(JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (message == null)
            {
                message = string.Empty;
            }

            byte[] messageBytes;

            try
            {
                messageBytes = StrictUtf8.GetBytes(message);
            }
            catch (EncoderFallbackException ex)
            {
                throw new ArgumentException
                (
                    "The response message is not valid strict UTF-8 input.",
                    nameof(message),
                    ex
                );
            }

            if (messageBytes.Length > MaxResponseMessageUtf8Bytes)
            {
                Array.Clear(messageBytes, 0, messageBytes.Length);
                throw new ArgumentOutOfRangeException(nameof(message));
            }

            try
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    WriteHeader(writer, ResponseFrameKind);
                    writer.Write((byte)status);
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

        private static void WriteHeader(BinaryWriter writer, byte frameKind)
        {
            writer.Write(MagicBytes);
            writer.Write(CurrentVersion);
            writer.Write(frameKind);
        }

        private static void ReadAndValidateHeader(BinaryReader reader, byte expectedFrameKind)
        {
            var magic = reader.ReadBytes(MagicBytes.Length);

            if (magic.Length != MagicBytes.Length)
            {
                throw new EndOfStreamException
                (
                    "The Storage V2 read-only validation control header is incomplete."
                );
            }

            if (!MagicMatches(magic))
            {
                throw new InvalidDataException
                (
                    "The Storage V2 read-only validation control magic is invalid."
                );
            }

            var version = reader.ReadInt32();

            if (version != CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage V2 read-only validation protocol version '{version}' is not supported."
                );
            }

            var frameKind = reader.ReadByte();

            if (frameKind != expectedFrameKind)
            {
                throw new InvalidDataException
                (
                    "The Storage V2 read-only validation control frame kind is invalid."
                );
            }
        }

        private static byte[] EncodeRequiredUtf8(string value, int maximumBytes, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }

            byte[] bytes;

            try
            {
                bytes = StrictUtf8.GetBytes(value);
            }
            catch (EncoderFallbackException ex)
            {
                throw new ArgumentException
                (
                    "The value is not valid strict UTF-8 input.",
                    parameterName,
                    ex
                );
            }

            if (bytes.Length <= 0 || bytes.Length > maximumBytes)
            {
                Array.Clear(bytes, 0, bytes.Length);
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return bytes;
        }

        private static byte[] ReadRequiredBytes(BinaryReader reader, int maximumBytes, string description)
        {
            var length = reader.ReadInt32();

            if (length <= 0 || length > maximumBytes)
            {
                throw new InvalidDataException($"The {description} length is invalid.");
            }

            var bytes = reader.ReadBytes(length);

            if (bytes.Length != length)
            {
                Array.Clear(bytes, 0, bytes.Length);
                throw new EndOfStreamException($"The {description} payload is incomplete.");
            }

            return bytes;
        }

        private static byte[] ReadOptionalBytes(BinaryReader reader, int maximumBytes, string description)
        {
            var length = reader.ReadInt32();

            if (length < 0 || length > maximumBytes)
            {
                throw new InvalidDataException($"The {description} length is invalid.");
            }

            var bytes = reader.ReadBytes(length);

            if (bytes.Length != length)
            {
                Array.Clear(bytes, 0, bytes.Length);
                throw new EndOfStreamException($"The {description} payload is incomplete.");
            }

            return bytes;
        }

        private static void ValidateSecretBytes(byte[] value)
        {
            if (value == null || value.Length <= 0 || value.Length > MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "A bounded database password is required.",
                    nameof(value)
                );
            }

            ValidateStrictUtf8(value, "database password");
            ValidateCanonicalDatabasePassword(value);
        }

        private static void ValidateCanonicalDatabasePassword(byte[] value)
        {
            if (value.Length != 44 || value[43] != (byte)'=')
            {
                throw new InvalidDataException
                (
                    "The database password does not use the canonical Base64 encoding of a 32-byte logical database key."
                );
            }

            for (var index = 0; index < 43; index++)
            {
                var base64Value = GetBase64Value(value[index]);

                if (base64Value < 0)
                {
                    throw new InvalidDataException
                    (
                        "The database password contains an invalid Base64 byte."
                    );
                }

                if (index == 42 && (base64Value & 0x03) != 0)
                {
                    throw new InvalidDataException
                    (
                        "The database password is not a canonical Base64 encoding."
                    );
                }
            }
        }

        private static int GetBase64Value(byte value)
        {
            if (value >= (byte)'A' && value <= (byte)'Z')
            {
                return value - (byte)'A';
            }

            if (value >= (byte)'a' && value <= (byte)'z')
            {
                return value - (byte)'a' + 26;
            }

            if (value >= (byte)'0' && value <= (byte)'9')
            {
                return value - (byte)'0' + 52;
            }

            if (value == (byte)'+')
            {
                return 62;
            }

            if (value == (byte)'/')
            {
                return 63;
            }

            return -1;
        }

        private static void ValidateStrictUtf8(byte[] value, string description)
        {
            try
            {
                StrictUtf8.GetCharCount(value);
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException
                (
                    $"The {description} is not valid UTF-8.",
                    ex
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
                    "The Storage V2 read-only validation control stream must be readable.",
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
                    "The Storage V2 read-only validation control stream must be writable.",
                    nameof(stream)
                );
            }
        }
    }

    public sealed class JasonQueryDbStorageV2ReadOnlyValidatorRequest : IDisposable
    {
        private byte[] _databasePasswordUtf8;

        internal JasonQueryDbStorageV2ReadOnlyValidatorRequest(string databaseFilePath, byte[] databasePasswordUtf8)
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
                        nameof(JasonQueryDbStorageV2ReadOnlyValidatorRequest)
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

            Array.Clear(_databasePasswordUtf8, 0, _databasePasswordUtf8.Length);
            _databasePasswordUtf8 = null;
        }
    }

    public enum JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus : byte
    {
        StorageV2Validated = 1
    }

    public sealed class JasonQueryDbStorageV2ReadOnlyValidatorResponse
    {
        internal JasonQueryDbStorageV2ReadOnlyValidatorResponse(JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus status, string message)
        {
            Status = status;
            Message = message ?? string.Empty;
        }

        public JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus Status { get; }

        public string Message { get; }
    }
}
