using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.ModernDbMigration
{
    internal sealed class ModernStorageV2LogicalDigest : IDisposable
    {
        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);
        private static readonly byte[] EmptyBytes = new byte[0];

        private readonly SHA256 _hash = SHA256.Create();
        private readonly byte[] _scalar = new byte[8];
        private bool _finished;

        internal void AddDatabaseMetadata(JasonQueryDbStorageMigrationDatabaseMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            AddAscii("DB");
            AddInt32Raw(metadata.UserVersion);
            AddInt32Raw(metadata.ApplicationId);
            AddUtf8String(metadata.SourceEncodingName);
        }

        internal void BeginTable(int tableId, string tableName, int fieldCount)
        {
            AddAscii("TABLE");
            AddInt32Raw(tableId);
            AddUtf8String(tableName);
            AddInt32Raw(fieldCount);
        }

        internal void BeginRow(int fieldCount)
        {
            AddAscii("ROW");
            AddInt32Raw(fieldCount);
        }

        internal void EndRow()
        {
            AddAscii("ENDROW");
        }

        internal void EndTable(long rowCount)
        {
            AddAscii("ENDTABLE");
            AddInt64Raw(rowCount);
        }

        internal void AddSequence(string tableName, long sequenceValue)
        {
            AddAscii("SEQ");
            AddUtf8String(tableName);
            AddInt64Raw(sequenceValue);
        }

        internal void AddNull()
        {
            AddByte((byte)JasonQueryDbStorageMigrationValueKind.Null);
        }

        internal void AddInt64(long value)
        {
            AddByte((byte)JasonQueryDbStorageMigrationValueKind.Int64);
            AddInt64Raw(value);
        }

        internal void AddDouble(double value)
        {
            AddByte((byte)JasonQueryDbStorageMigrationValueKind.Double);
            AddInt64Raw(BitConverter.DoubleToInt64Bits(value));
        }

        internal void BeginText(long length)
        {
            AddLargeValueHeader(JasonQueryDbStorageMigrationValueKind.TextUtf8, length);
        }

        internal void BeginBlob(long length)
        {
            AddLargeValueHeader(JasonQueryDbStorageMigrationValueKind.Blob, length);
        }

        internal void AddValueChunk(byte[] bytes, int offset, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (offset < 0 || count < 0 || offset > bytes.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            AddBytes(bytes, offset, count);
        }

        internal string Finish()
        {
            EnsureActive();
            _hash.TransformFinalBlock(EmptyBytes, 0, 0);
            _finished = true;
            return ToHex(_hash.Hash);
        }

        public void Dispose()
        {
            _hash.Dispose();
            Array.Clear(_scalar, 0, _scalar.Length);
            _finished = true;
        }

        private void AddLargeValueHeader(JasonQueryDbStorageMigrationValueKind kind, long length)
        {
            if (kind == JasonQueryDbStorageMigrationValueKind.TextUtf8)
            {
                JasonQueryDbStorageMigrationLogicalStreamContract.EnsureTextUtf8ByteLength(length);
            }

            if (kind == JasonQueryDbStorageMigrationValueKind.Blob)
            {
                JasonQueryDbStorageMigrationLogicalStreamContract.EnsureBlobByteLength(length);
            }

            AddByte((byte)kind);
            AddInt64Raw(length);
        }

        private void AddUtf8String(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var bytes = StrictUtf8.GetBytes(value);

            try
            {
                AddInt32Raw(bytes.Length);
                AddBytes(bytes, 0, bytes.Length);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private void AddAscii(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);

            try
            {
                AddInt32Raw(bytes.Length);
                AddBytes(bytes, 0, bytes.Length);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        private void AddByte(byte value)
        {
            _scalar[0] = value;
            AddBytes(_scalar, 0, 1);
        }

        private void AddInt32Raw(int value)
        {
            unchecked
            {
                _scalar[0] = (byte)value;
                _scalar[1] = (byte)(value >> 8);
                _scalar[2] = (byte)(value >> 16);
                _scalar[3] = (byte)(value >> 24);
            }

            AddBytes(_scalar, 0, 4);
        }

        private void AddInt64Raw(long value)
        {
            unchecked
            {
                _scalar[0] = (byte)value;
                _scalar[1] = (byte)(value >> 8);
                _scalar[2] = (byte)(value >> 16);
                _scalar[3] = (byte)(value >> 24);
                _scalar[4] = (byte)(value >> 32);
                _scalar[5] = (byte)(value >> 40);
                _scalar[6] = (byte)(value >> 48);
                _scalar[7] = (byte)(value >> 56);
            }

            AddBytes(_scalar, 0, 8);
        }

        private void AddBytes(byte[] bytes, int offset, int count)
        {
            EnsureActive();

            if (count == 0)
            {
                return;
            }

            _hash.TransformBlock(bytes, offset, count, bytes, offset);
        }

        private void EnsureActive()
        {
            if (_finished)
            {
                throw new InvalidOperationException("The logical digest has already been finalized.");
            }
        }

        private static string ToHex(byte[] value)
        {
            var builder = new StringBuilder(value.Length * 2);

            for (var index = 0; index < value.Length; index++)
            {
                builder.Append(value[index].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
