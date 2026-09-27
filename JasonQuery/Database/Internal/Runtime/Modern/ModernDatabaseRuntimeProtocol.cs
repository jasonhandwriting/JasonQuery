using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JasonQuery.Database.Internal.Runtime.Modern
{
    internal enum ModernDatabaseRuntimeOperation : byte
    {
        Ping = 1,
        Open = 2,
        ExecuteQuery = 3,
        ExecuteNonQuery = 4,
        ExecuteBatchNonQuery = 5,
        Close = 6,
        Shutdown = 7,
        Rekey = 8,
        CreateFreshDatabase = 9,
        CanOpenWithoutKey = 10
    }

    internal enum ModernDatabaseRuntimeFrameKind : byte
    {
        Request = 1,
        Response = 2
    }

    internal enum ModernDatabaseRuntimeResponseStatus : byte
    {
        None = 0,
        Success = 1,
        Failure = 2
    }

    internal enum ModernDatabaseRuntimeValueKind : byte
    {
        Null = 0,
        Int64 = 1,
        Double = 2,
        String = 3,
        ByteArray = 4,
        Decimal = 5,
        Boolean = 6,
        DateTime = 7,
        Guid = 8
    }

    internal sealed class ModernDatabaseRuntimeFrame
    {
        public ModernDatabaseRuntimeFrame(long requestId, ModernDatabaseRuntimeOperation operation,
                                          ModernDatabaseRuntimeFrameKind kind,
                                          ModernDatabaseRuntimeResponseStatus responseStatus, byte[] payload)
        {
            RequestId = requestId;
            Operation = operation;
            Kind = kind;
            ResponseStatus = responseStatus;
            Payload = payload ?? Array.Empty<byte>();
        }

        public long RequestId { get; }

        public ModernDatabaseRuntimeOperation Operation { get; }

        public ModernDatabaseRuntimeFrameKind Kind { get; }

        public ModernDatabaseRuntimeResponseStatus ResponseStatus { get; }

        public byte[] Payload { get; }
    }

    internal sealed class ModernDatabaseRuntimeParameter
    {
        public ModernDatabaseRuntimeParameter(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A runtime parameter name is required.", nameof(name));
            }

            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }

    internal sealed class ModernDatabaseRuntimeQueryResult
    {
        public ModernDatabaseRuntimeQueryResult(string[] columnNames, List<object[]> rows)
        {
            ColumnNames = columnNames ?? throw new ArgumentNullException(nameof(columnNames));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }

        public string[] ColumnNames { get; }

        public List<object[]> Rows { get; }
    }

    internal sealed class ModernDatabaseRuntimeIdentity
    {
        public ModernDatabaseRuntimeIdentity(string runtimeName, int protocolVersion, string sqliteVersion,
                                             string sqlCipherVersionPrefix, string nativeSha256)
        {
            RuntimeName = runtimeName;
            ProtocolVersion = protocolVersion;
            SqliteVersion = sqliteVersion;
            SqlCipherVersionPrefix = sqlCipherVersionPrefix;
            NativeSha256 = nativeSha256;
        }

        public string RuntimeName { get; }

        public int ProtocolVersion { get; }

        public string SqliteVersion { get; }

        public string SqlCipherVersionPrefix { get; }

        public string NativeSha256 { get; }
    }

    internal static class ModernDatabaseRuntimeProtocol
    {
        internal const int ProtocolVersion = 3;
        internal const string RuntimeName = "JasonQuery.ModernDbRuntime";
        internal const string QualifiedSqliteVersion = "3.53.3";
        internal const string QualifiedSqlCipherVersionPrefix = "4.17.0";
        internal const string QualifiedNativeSha256 = "25852CE7A4067CC79E73309D26C1AD9B5706E876BBDF48BE9A25379180FB9A07";
        internal const int MaxFrameBytes = 128 * 1024 * 1024;
        internal const int MaxStringBytes = 16 * 1024 * 1024;
        internal const int MaxFreshTemplateBytes = 16 * 1024 * 1024;
        internal const int MaxCollectionCount = 1000000;

        private const int Magic = 0x4A514452;
        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        internal static void WriteRequest(Stream output, long requestId, ModernDatabaseRuntimeOperation operation,
                                          Action<BinaryWriter> payloadWriter)
        {
            WriteFrame
            (
                output,
                requestId,
                operation,
                ModernDatabaseRuntimeFrameKind.Request,
                ModernDatabaseRuntimeResponseStatus.None,
                payloadWriter
            );
        }

        internal static void WriteSuccessResponse(Stream output, long requestId, ModernDatabaseRuntimeOperation operation,
                                                  Action<BinaryWriter> payloadWriter)
        {
            WriteFrame
            (
                output,
                requestId,
                operation,
                ModernDatabaseRuntimeFrameKind.Response,
                ModernDatabaseRuntimeResponseStatus.Success,
                payloadWriter
            );
        }

        internal static void WriteFailureResponse(Stream output, long requestId, ModernDatabaseRuntimeOperation operation,
                                                  string errorType, string errorMessage)
        {
            WriteFrame
            (
                output,
                requestId,
                operation,
                ModernDatabaseRuntimeFrameKind.Response,
                ModernDatabaseRuntimeResponseStatus.Failure,
                writer =>
                {
                    WriteString(writer, errorType ?? string.Empty);
                    WriteString(writer, errorMessage ?? string.Empty);
                }
            );
        }

        internal static bool TryReadFrame(Stream input, out ModernDatabaseRuntimeFrame frame)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var magicBytes = new byte[sizeof(int)];
            var magicRead = ReadExactAllowCleanEof(input, magicBytes, 0, magicBytes.Length);

            if (!magicRead)
            {
                frame = null;
                return false;
            }

            var magic = BitConverter.ToInt32(magicBytes, 0);

            if (magic != Magic)
            {
                throw new InvalidDataException("The Modern DB runtime frame magic is invalid.");
            }

            using (var headerBuffer = new MemoryStream(ReadExactBytes(input, 16), false))
            using (var headerReader = new BinaryReader(headerBuffer, Utf8, false))
            {
                var version = headerReader.ReadInt32();

                if (version != ProtocolVersion)
                {
                    throw new InvalidDataException("The Modern DB runtime protocol version is not supported.");
                }

                var kind = (ModernDatabaseRuntimeFrameKind)headerReader.ReadByte();
                var operation = (ModernDatabaseRuntimeOperation)headerReader.ReadByte();
                var responseStatus = (ModernDatabaseRuntimeResponseStatus)headerReader.ReadByte();
                var reserved = headerReader.ReadByte();
                var requestId = headerReader.ReadInt64();

                if (reserved != 0)
                {
                    throw new InvalidDataException("The Modern DB runtime frame reserved byte is invalid.");
                }

                EnsureKnownFrameKind(kind);
                EnsureKnownOperation(operation);
                EnsureValidStatus(kind, responseStatus);

                var lengthBytes = ReadExactBytes(input, sizeof(int));
                var payloadLength = BitConverter.ToInt32(lengthBytes, 0);

                if (payloadLength < 0 || payloadLength > MaxFrameBytes)
                {
                    throw new InvalidDataException("The Modern DB runtime frame payload length is invalid.");
                }

                frame = new ModernDatabaseRuntimeFrame
                (
                    requestId,
                    operation,
                    kind,
                    responseStatus,
                    ReadExactBytes(input, payloadLength)
                );

                return true;
            }
        }

        internal static BinaryReader OpenPayloadReader(ModernDatabaseRuntimeFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            return new BinaryReader(new MemoryStream(frame.Payload, false), Utf8, false);
        }

        internal static void EnsurePayloadConsumed(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                throw new InvalidDataException("The Modern DB runtime payload contains unexpected trailing bytes.");
            }
        }

        internal static void WriteString(BinaryWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value == null)
            {
                writer.Write(-1);
                return;
            }

            var bytes = Utf8.GetBytes(value);

            try
            {
                if (bytes.Length > MaxStringBytes)
                {
                    throw new InvalidDataException("The Modern DB runtime string exceeds the protocol limit.");
                }

                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        internal static string ReadString(BinaryReader reader)
        {
            var bytes = ReadByteArrayCore(reader, MaxStringBytes, true);

            if (bytes == null)
            {
                return null;
            }

            try
            {
                return Utf8.GetString(bytes);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        internal static void WriteByteArray(BinaryWriter writer, byte[] value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value == null)
            {
                writer.Write(-1);
                return;
            }

            if (value.Length > MaxFrameBytes)
            {
                throw new InvalidDataException("The Modern DB runtime byte array exceeds the protocol limit.");
            }

            writer.Write(value.Length);
            writer.Write(value);
        }

        internal static byte[] ReadByteArray(BinaryReader reader)
        {
            return ReadByteArrayCore(reader, MaxFrameBytes, true);
        }

        internal static void WriteParameters(BinaryWriter writer, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters)
        {
            if (parameters == null)
            {
                writer.Write(0);
                return;
            }

            if (parameters.Count > MaxCollectionCount)
            {
                throw new InvalidDataException("The Modern DB runtime parameter count exceeds the protocol limit.");
            }

            writer.Write(parameters.Count);

            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index] ?? throw new InvalidDataException("A Modern DB runtime parameter is null.");

                WriteString(writer, parameter.Name);
                WriteValue(writer, parameter.Value);
            }
        }

        internal static List<ModernDatabaseRuntimeParameter> ReadParameters(BinaryReader reader)
        {
            var count = ReadCount(reader, "parameter");
            var result = new List<ModernDatabaseRuntimeParameter>(count);

            for (var index = 0; index < count; index++)
            {
                result.Add
                (
                    new ModernDatabaseRuntimeParameter
                    (
                        ReadString(reader),
                        ReadValue(reader)
                    )
                );
            }

            return result;
        }

        internal static void WriteStringList(BinaryWriter writer, IReadOnlyList<string> values)
        {
            if (values == null)
            {
                writer.Write(0);
                return;
            }

            if (values.Count > MaxCollectionCount)
            {
                throw new InvalidDataException("The Modern DB runtime string-list count exceeds the protocol limit.");
            }

            writer.Write(values.Count);

            for (var index = 0; index < values.Count; index++)
            {
                WriteString(writer, values[index]);
            }
        }

        internal static List<string> ReadStringList(BinaryReader reader)
        {
            var count = ReadCount(reader, "string-list");
            var result = new List<string>(count);

            for (var index = 0; index < count; index++)
            {
                result.Add(ReadString(reader));
            }

            return result;
        }

        internal static void WriteQueryResult(BinaryWriter writer, ModernDatabaseRuntimeQueryResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.ColumnNames.Length > MaxCollectionCount || result.Rows.Count > MaxCollectionCount)
            {
                throw new InvalidDataException("The Modern DB runtime query result exceeds the protocol collection limit.");
            }

            writer.Write(result.ColumnNames.Length);

            for (var columnIndex = 0; columnIndex < result.ColumnNames.Length; columnIndex++)
            {
                WriteString(writer, result.ColumnNames[columnIndex]);
            }

            writer.Write(result.Rows.Count);

            for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
            {
                var row = result.Rows[rowIndex];

                if (row == null || row.Length != result.ColumnNames.Length)
                {
                    throw new InvalidDataException("The Modern DB runtime query row shape is invalid.");
                }

                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    WriteValue(writer, row[columnIndex]);
                }
            }
        }

        internal static ModernDatabaseRuntimeQueryResult ReadQueryResult(BinaryReader reader)
        {
            var columnCount = ReadCount(reader, "query-column");
            var columnNames = new string[columnCount];

            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                columnNames[columnIndex] = ReadString(reader) ?? string.Empty;
            }

            var rowCount = ReadCount(reader, "query-row");
            var rows = new List<object[]>(rowCount);

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var row = new object[columnCount];

                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    row[columnIndex] = ReadValue(reader);
                }

                rows.Add(row);
            }

            return new ModernDatabaseRuntimeQueryResult(columnNames, rows);
        }

        internal static void WriteIdentity(BinaryWriter writer, ModernDatabaseRuntimeIdentity identity)
        {
            WriteString(writer, identity.RuntimeName);
            writer.Write(identity.ProtocolVersion);
            WriteString(writer, identity.SqliteVersion);
            WriteString(writer, identity.SqlCipherVersionPrefix);
            WriteString(writer, identity.NativeSha256);
        }

        internal static ModernDatabaseRuntimeIdentity ReadIdentity(BinaryReader reader)
        {
            return new ModernDatabaseRuntimeIdentity
            (
                ReadString(reader),
                reader.ReadInt32(),
                ReadString(reader),
                ReadString(reader),
                ReadString(reader)
            );
        }

        internal static void WriteValue(BinaryWriter writer, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Null);
                return;
            }

            if (value is bool booleanValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Boolean);
                writer.Write(booleanValue);
                return;
            }

            if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Int64);
                writer.Write(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                return;
            }

            if (value is ulong unsignedLongValue)
            {
                if (unsignedLongValue <= long.MaxValue)
                {
                    writer.Write((byte)ModernDatabaseRuntimeValueKind.Int64);
                    writer.Write((long)unsignedLongValue);
                    return;
                }

                writer.Write((byte)ModernDatabaseRuntimeValueKind.Decimal);
                WriteString(writer, unsignedLongValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is float || value is double)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Double);
                writer.Write(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                return;
            }

            if (value is decimal decimalValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Decimal);
                WriteString(writer, decimalValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is DateTime dateTimeValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.DateTime);
                writer.Write(dateTimeValue.ToBinary());
                return;
            }

            if (value is Guid guidValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.Guid);
                WriteByteArray(writer, guidValue.ToByteArray());
                return;
            }

            if (value is byte[] byteArrayValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.ByteArray);
                WriteByteArray(writer, byteArrayValue);
                return;
            }

            if (value is string stringValue)
            {
                writer.Write((byte)ModernDatabaseRuntimeValueKind.String);
                WriteString(writer, stringValue);
                return;
            }

            throw new NotSupportedException
            (
                "The Modern DB runtime protocol does not support parameter type '" +
                value.GetType().FullName + "'."
            );
        }

        internal static object ReadValue(BinaryReader reader)
        {
            var kind = (ModernDatabaseRuntimeValueKind)reader.ReadByte();

            switch (kind)
            {
                case ModernDatabaseRuntimeValueKind.Null:
                    {
                        return DBNull.Value;
                    }
                case ModernDatabaseRuntimeValueKind.Int64:
                    {
                        return reader.ReadInt64();
                    }
                case ModernDatabaseRuntimeValueKind.Double:
                    {
                        return reader.ReadDouble();
                    }
                case ModernDatabaseRuntimeValueKind.String:
                    {
                        return ReadString(reader);
                    }
                case ModernDatabaseRuntimeValueKind.ByteArray:
                    {
                        return ReadByteArray(reader);
                    }
                case ModernDatabaseRuntimeValueKind.Decimal:
                    {
                        return decimal.Parse(ReadString(reader), NumberStyles.Number, CultureInfo.InvariantCulture);
                    }
                case ModernDatabaseRuntimeValueKind.Boolean:
                    {
                        return reader.ReadBoolean();
                    }
                case ModernDatabaseRuntimeValueKind.DateTime:
                    {
                        return DateTime.FromBinary(reader.ReadInt64());
                    }
                case ModernDatabaseRuntimeValueKind.Guid:
                    {
                        var bytes = ReadByteArray(reader);

                        try
                        {
                            if (bytes == null || bytes.Length != 16)
                            {
                                throw new InvalidDataException("The Modern DB runtime GUID payload is invalid.");
                            }

                            return new Guid(bytes);
                        }
                        finally
                        {
                            if (bytes != null)
                            {
                                Array.Clear(bytes, 0, bytes.Length);
                            }
                        }
                    }
                default:
                    {
                        throw new InvalidDataException("The Modern DB runtime value kind is not supported.");
                    }
            }
        }

        private static void WriteFrame(Stream output, long requestId, ModernDatabaseRuntimeOperation operation,
                                       ModernDatabaseRuntimeFrameKind kind, ModernDatabaseRuntimeResponseStatus responseStatus,
                                       Action<BinaryWriter> payloadWriter)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            EnsureKnownOperation(operation);
            EnsureKnownFrameKind(kind);
            EnsureValidStatus(kind, responseStatus);

            byte[] payload;

            using (var payloadStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(payloadStream, Utf8, true))
                {
                    payloadWriter?.Invoke(writer);
                    writer.Flush();
                }

                if (payloadStream.Length > MaxFrameBytes)
                {
                    throw new InvalidDataException("The Modern DB runtime frame exceeds the protocol limit.");
                }

                payload = payloadStream.ToArray();
            }

            try
            {
                using (var writer = new BinaryWriter(output, Utf8, true))
                {
                    writer.Write(Magic);
                    writer.Write(ProtocolVersion);
                    writer.Write((byte)kind);
                    writer.Write((byte)operation);
                    writer.Write((byte)responseStatus);
                    writer.Write((byte)0);
                    writer.Write(requestId);
                    writer.Write(payload.Length);
                    writer.Write(payload);
                    writer.Flush();
                }
            }
            finally
            {
                Array.Clear(payload, 0, payload.Length);
            }
        }

        private static int ReadCount(BinaryReader reader, string description)
        {
            var count = reader.ReadInt32();

            if (count < 0 || count > MaxCollectionCount)
            {
                throw new InvalidDataException("The Modern DB runtime " + description + " count is invalid.");
            }

            return count;
        }

        private static byte[] ReadByteArrayCore(BinaryReader reader, int maximumLength, bool allowNull)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var length = reader.ReadInt32();

            if (allowNull && length == -1)
            {
                return null;
            }

            if (length < 0 || length > maximumLength)
            {
                throw new InvalidDataException("The Modern DB runtime byte-array length is invalid.");
            }

            var bytes = reader.ReadBytes(length);

            if (bytes.Length != length)
            {
                throw new EndOfStreamException("The Modern DB runtime payload ended unexpectedly.");
            }

            return bytes;
        }

        private static byte[] ReadExactBytes(Stream input, int count)
        {
            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            var buffer = new byte[count];

            if (!ReadExactAllowCleanEof(input, buffer, 0, count))
            {
                throw new EndOfStreamException("The Modern DB runtime frame ended unexpectedly.");
            }

            return buffer;
        }

        private static bool ReadExactAllowCleanEof(Stream input, byte[] buffer, int offset, int count)
        {
            var total = 0;

            while (total < count)
            {
                var read = input.Read(buffer, offset + total, count - total);

                if (read == 0)
                {
                    if (total == 0)
                    {
                        return false;
                    }

                    throw new EndOfStreamException("The Modern DB runtime frame ended unexpectedly.");
                }

                total += read;
            }

            return true;
        }

        private static void EnsureKnownFrameKind(ModernDatabaseRuntimeFrameKind kind)
        {
            if (kind != ModernDatabaseRuntimeFrameKind.Request && kind != ModernDatabaseRuntimeFrameKind.Response)
            {
                throw new InvalidDataException("The Modern DB runtime frame kind is invalid.");
            }
        }

        private static void EnsureKnownOperation(ModernDatabaseRuntimeOperation operation)
        {
            if (operation < ModernDatabaseRuntimeOperation.Ping || operation > ModernDatabaseRuntimeOperation.CanOpenWithoutKey)
            {
                throw new InvalidDataException("The Modern DB runtime operation is invalid.");
            }
        }

        private static void EnsureValidStatus(ModernDatabaseRuntimeFrameKind kind, ModernDatabaseRuntimeResponseStatus responseStatus)
        {
            if (kind == ModernDatabaseRuntimeFrameKind.Request && responseStatus != ModernDatabaseRuntimeResponseStatus.None)
            {
                throw new InvalidDataException("A Modern DB runtime request contains an invalid response status.");
            }

            if (kind == ModernDatabaseRuntimeFrameKind.Response && responseStatus != ModernDatabaseRuntimeResponseStatus.Success
                && responseStatus != ModernDatabaseRuntimeResponseStatus.Failure)
            {
                throw new InvalidDataException("A Modern DB runtime response contains an invalid status.");
            }
        }
    }
}
