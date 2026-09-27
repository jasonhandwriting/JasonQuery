using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.ModernDbMigration
{
    internal enum ModernDbMigrationRequestKind
    {
        CandidateWriter = 1,
        ReadOnlyValidator = 2
    }

    internal static class ModernDbMigrationRequestDispatcher
    {
        private const int MagicLength = 4;

        internal static Stream CreateReplayStream(Stream input, out ModernDbMigrationRequestKind requestKind)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!input.CanRead)
            {
                throw new ArgumentException
                (
                    "The Modern migration helper input stream must be readable.",
                    nameof(input)
                );
            }

            var prefix = new byte[MagicLength];
            var offset = 0;

            while (offset < prefix.Length)
            {
                var read = input.Read(prefix, offset, prefix.Length - offset);

                if (read <= 0)
                {
                    Array.Clear(prefix, 0, prefix.Length);

                    throw new EndOfStreamException
                    (
                        "The Modern migration helper request magic is incomplete."
                    );
                }

                offset += read;
            }

            try
            {
                if (MatchesMagic(prefix, JasonQueryDbStorageV2CandidateWriterProtocol.Magic))
                {
                    requestKind = ModernDbMigrationRequestKind.CandidateWriter;
                    return new PrefixReplayStream(prefix, input);
                }

                if (JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MagicMatches(prefix))
                {
                    requestKind = ModernDbMigrationRequestKind.ReadOnlyValidator;
                    return new PrefixReplayStream(prefix, input);
                }

                throw new InvalidDataException
                (
                    "The Modern migration helper request magic is not supported."
                );
            }
            catch
            {
                Array.Clear(prefix, 0, prefix.Length);
                throw;
            }
        }

        private static bool MatchesMagic(byte[] value, string expectedMagic)
        {
            var expected = Encoding.ASCII.GetBytes(expectedMagic);

            try
            {
                if (value.Length != expected.Length)
                {
                    return false;
                }

                for (var index = 0; index < expected.Length; index++)
                {
                    if (value[index] != expected[index])
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                Array.Clear(expected, 0, expected.Length);
            }
        }

        private sealed class PrefixReplayStream : Stream
        {
            private byte[] _prefix;
            private readonly Stream _inner;
            private int _prefixOffset;

            public PrefixReplayStream(byte[] prefix, Stream inner)
            {
                _prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public override bool CanRead => _prefix != null && _inner.CanRead;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_prefix == null)
                {
                    throw new ObjectDisposedException(nameof(PrefixReplayStream));
                }

                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                if (offset < 0 || count < 0 || offset > buffer.Length - count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var copied = 0;

                if (_prefixOffset < _prefix.Length && count > 0)
                {
                    var available = _prefix.Length - _prefixOffset;
                    var copyCount = Math.Min(available, count);

                    Buffer.BlockCopy
                    (
                        _prefix,
                        _prefixOffset,
                        buffer,
                        offset,
                        copyCount
                    );

                    _prefixOffset += copyCount;
                    copied += copyCount;
                    offset += copyCount;
                    count -= copyCount;
                }

                if (count > 0)
                {
                    copied += _inner.Read(buffer, offset, count);
                }

                return copied;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (_prefix != null)
                {
                    Array.Clear(_prefix, 0, _prefix.Length);
                    _prefix = null;
                }

                base.Dispose(disposing);
            }
        }
    }
}
