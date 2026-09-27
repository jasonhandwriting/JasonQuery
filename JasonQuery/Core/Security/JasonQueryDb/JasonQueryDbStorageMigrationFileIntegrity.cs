using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// SHA-256 helpers for durable storage-migration artifact identity.
    /// </summary>
    internal static class JasonQueryDbStorageMigrationFileIntegrity
    {
        public static string ComputeSha256(string filePath)
        {
            var normalizedPath = NormalizeExistingFile(filePath);

            using (var stream = new FileStream(normalizedPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(stream);

                try
                {
                    return ToUpperHex(hash);
                }
                finally
                {
                    Array.Clear(hash, 0, hash.Length);
                }
            }
        }

        public static bool MatchesSha256(string filePath, string expectedSha256)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            ValidateCanonicalSha256(expectedSha256, nameof(expectedSha256));

            return string.Equals
            (
                ComputeSha256(filePath),
                expectedSha256,
                StringComparison.Ordinal
            );
        }

        public static void EnsureMatchesSha256(string filePath, string expectedSha256, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("An artifact description is required.", nameof(description));
            }

            if (!MatchesSha256(filePath, expectedSha256))
            {
                throw new InvalidDataException
                (
                    description + " does not match the durable migration identity."
                );
            }
        }

        public static string ComputeSha256(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(bytes);

                try
                {
                    return ToUpperHex(hash);
                }
                finally
                {
                    Array.Clear(hash, 0, hash.Length);
                }
            }
        }

        private static string NormalizeExistingFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A file path is required.", nameof(filePath));
            }

            var normalizedPath = Path.GetFullPath(filePath);

            if (!File.Exists(normalizedPath))
            {
                throw new FileNotFoundException
                (
                    "The migration integrity file was not found.",
                    normalizedPath
                );
            }

            return normalizedPath;
        }

        private static void ValidateCanonicalSha256(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
            {
                throw new ArgumentException
                (
                    "A canonical uppercase hexadecimal SHA-256 value is required.",
                    parameterName
                );
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var isDigit = character >= '0' && character <= '9';
                var isUpperHex = character >= 'A' && character <= 'F';

                if (!isDigit && !isUpperHex)
                {
                    throw new ArgumentException
                    (
                        "A canonical uppercase hexadecimal SHA-256 value is required.",
                        parameterName
                    );
                }
            }
        }

        private static string ToUpperHex(byte[] bytes)
        {
            const string hex = "0123456789ABCDEF";
            var characters = new char[bytes.Length * 2];

            for (var index = 0; index < bytes.Length; index++)
            {
                var value = bytes[index];

                characters[index * 2] = hex[value >> 4];
                characters[index * 2 + 1] = hex[value & 0x0F];
            }

            return new string(characters);
        }
    }
}
