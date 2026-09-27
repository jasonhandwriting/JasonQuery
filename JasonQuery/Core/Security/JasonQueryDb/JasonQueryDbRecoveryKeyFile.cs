using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbRecoveryKeyFile
    {
        public const string FileExtension = ".jqrecovery";

        private const int MaximumFileBytes = 1024;

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public static void SaveAndVerify(string filePath, string recoveryKeyText, string expectedKeyId)
        {
            ValidateFilePath(filePath);

            if (string.IsNullOrWhiteSpace(recoveryKeyText))
            {
                throw new ArgumentException("A recovery key is required.", nameof(recoveryKeyText));
            }

            if (File.Exists(filePath))
            {
                throw new IOException
                (
                    "The selected Recovery Key file already exists. Choose a new file name so an existing Recovery Key is never overwritten."
                );
            }

            using (var material = JasonQueryDbRecoveryKeyCodec.Decode(recoveryKeyText))
            {
                if (!string.IsNullOrWhiteSpace(expectedKeyId) && !string.Equals(material.KeyId, expectedKeyId, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The Recovery Key identifier does not match the recovery enrollment draft.");
                }
            }

            var bytes = StrictUtf8.GetBytes(recoveryKeyText);

            try
            {
                if (bytes.Length <= 0 || bytes.Length > MaximumFileBytes)
                {
                    throw new InvalidDataException("The Recovery Key file content length is invalid.");
                }

                using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                var reloaded = ReadEncodedKey(filePath);

                if (!string.Equals(reloaded, recoveryKeyText, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The saved Recovery Key file could not be verified.");
                }

                using (var material = JasonQueryDbRecoveryKeyCodec.Decode(reloaded))
                {
                    if (!string.IsNullOrWhiteSpace(expectedKeyId) && !string.Equals(material.KeyId, expectedKeyId, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException("The saved Recovery Key identifier does not match.");
                    }
                }
            }
            catch
            {
                TryDelete(filePath);
                throw;
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        public static string ReadEncodedKey(string filePath)
        {
            ValidateFilePath(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The Recovery Key file was not found.", filePath);
            }

            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Length <= 0 || fileInfo.Length > MaximumFileBytes)
            {
                throw new InvalidDataException("The Recovery Key file size is invalid.");
            }

            byte[] bytes = null;

            try
            {
                bytes = File.ReadAllBytes(filePath);

                var text = StrictUtf8.GetString(bytes);

                using (JasonQueryDbRecoveryKeyCodec.Decode(text))
                {
                    return text;
                }
            }
            catch (DecoderFallbackException ex)
            {
                throw new InvalidDataException("The Recovery Key file is not valid UTF-8.", ex);
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes, 0, bytes.Length);
                }
            }
        }

        public static void TryDelete(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                //Best-effort cleanup only. Do not replace the original recovery operation error.
            }
        }

        private static void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A Recovery Key file path is required.", nameof(filePath));
            }

            if (!string.Equals(Path.GetExtension(filePath), FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Recovery Key files must use the '{FileExtension}' extension.");
            }
        }
    }
}
