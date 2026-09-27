using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Bounded in-memory bridge for the plaintext JQSM logical stream.
    /// The bridge never stages the logical database stream on disk.
    /// </summary>
    public static class JasonQueryDbStorageMigrationStreamBridge
    {
        public const int BufferSize = 64 * 1024;

        public static long Forward(Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.CanRead)
            {
                throw new ArgumentException("The logical-stream source must be readable.", nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException("The logical-stream destination must be writable.", nameof(destination));
            }

            var buffer = new byte[BufferSize];
            long totalBytes = 0;

            try
            {
                while (true)
                {
                    var read = source.Read(buffer, 0, buffer.Length);

                    if (read == 0)
                    {
                        break;
                    }

                    destination.Write(buffer, 0, read);
                    totalBytes += read;
                }

                destination.Flush();
                return totalBytes;
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }
}
