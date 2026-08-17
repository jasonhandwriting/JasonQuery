using System;

namespace JasonLibrary.Core.Update
{
    public static class UpdateDigestValidator
    {
        private const string Sha256Prefix = "sha256:";
        private const int Sha256HexLength = 64;

        public static bool TryGetSha256(string digest, out string sha256)
        {
            sha256 = string.Empty;

            if (string.IsNullOrWhiteSpace(digest) || !digest.StartsWith(Sha256Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var value = digest.Substring(Sha256Prefix.Length);

            if (value.Length != Sha256HexLength)
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                var isHex = c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';

                if (!isHex)
                {
                    return false;
                }
            }

            sha256 = value.ToLowerInvariant();
            return true;
        }
    }
}