using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Updater.Core
{
    public static class Sha256Digest
    {
        private const string Prefix = "sha256:";
        private const int HexLength = 64;

        public static bool TryNormalize(string digest, out string sha256)
        {
            sha256 = string.Empty;

            if (string.IsNullOrWhiteSpace(digest) || !digest.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var value = digest.Substring(Prefix.Length);

            if (value.Length != HexLength)
            {
                return false;
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var isHex = character >= '0' && character <= '9' || character >= 'a' && character <= 'f' || character >= 'A' && character <= 'F';

                if (!isHex)
                {
                    return false;
                }
            }

            sha256 = value.ToLowerInvariant();
            return true;
        }

        public static string ComputeFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(stream);
                var builder = new StringBuilder(hash.Length * 2);

                foreach (var value in hash)
                {
                    builder.Append(value.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}