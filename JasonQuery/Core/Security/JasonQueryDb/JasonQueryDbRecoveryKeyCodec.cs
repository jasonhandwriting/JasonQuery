using System;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbRecoveryKeyCodec
    {
        public static JasonQueryDbRecoveryKeyMaterial Generate()
        {
            var keyIdBytes = new byte[JasonQueryDbRecoveryConstants.RecoveryKeyIdSizeBytes];
            var secret = new byte[JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes];

            try
            {
                using (var randomNumberGenerator = RandomNumberGenerator.Create())
                {
                    randomNumberGenerator.GetBytes(keyIdBytes);
                    randomNumberGenerator.GetBytes(secret);
                }

                return new JasonQueryDbRecoveryKeyMaterial(ToUpperHex(keyIdBytes), secret);
            }
            finally
            {
                Array.Clear(keyIdBytes, 0, keyIdBytes.Length);
                Array.Clear(secret, 0, secret.Length);
            }
        }

        public static string Encode(JasonQueryDbRecoveryKeyMaterial keyMaterial)
        {
            if (keyMaterial == null)
            {
                throw new ArgumentNullException(nameof(keyMaterial));
            }

            var secret = keyMaterial.CopySecret();

            try
            {
                var encodedSecret = ToBase64Url(secret);
                var checksum = ComputeChecksum(keyMaterial.KeyId, secret);

                return JasonQueryDbRecoveryConstants.RecoveryKeyPrefix + "-" + keyMaterial.KeyId + "-" + encodedSecret + "-" + checksum;
            }
            finally
            {
                Array.Clear(secret, 0, secret.Length);
            }
        }

        public static JasonQueryDbRecoveryKeyMaterial Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("A recovery key is required.");
            }

            var normalized = value.Trim();

            if (normalized.Length != JasonQueryDbRecoveryConstants.RecoveryKeyEncodedLength)
            {
                throw new FormatException("The recovery key length is invalid.");
            }

            if (!normalized.StartsWith(JasonQueryDbRecoveryConstants.RecoveryKeyPrefix + "-", StringComparison.Ordinal))
            {
                throw new FormatException("The recovery key version prefix is invalid.");
            }

            if (normalized[12] != '-' || normalized[56] != '-')
            {
                throw new FormatException("The recovery key format is invalid.");
            }

            var keyId = normalized.Substring(4, JasonQueryDbRecoveryConstants.RecoveryKeyIdHexLength);
            var encodedSecret = normalized.Substring(13, JasonQueryDbRecoveryConstants.RecoveryKeyEncodedSecretLength);
            var checksum = normalized.Substring(57, JasonQueryDbRecoveryConstants.RecoveryKeyChecksumHexLength);

            ValidateKeyId(keyId);
            ValidateUpperHex(checksum, JasonQueryDbRecoveryConstants.RecoveryKeyChecksumHexLength, "The recovery key checksum is invalid.");

            byte[] secret = null;

            try
            {
                secret = FromBase64Url(encodedSecret);

                if (secret.Length != JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes)
                {
                    throw new FormatException("The recovery key secret length is invalid.");
                }

                var expectedChecksum = ComputeChecksum(keyId, secret);

                if (!FixedTimeEqualsAscii(expectedChecksum, checksum))
                {
                    throw new FormatException("The recovery key checksum does not match.");
                }

                return new JasonQueryDbRecoveryKeyMaterial(keyId, secret);
            }
            finally
            {
                if (secret != null)
                {
                    Array.Clear(secret, 0, secret.Length);
                }
            }
        }

        internal static void ValidateKeyId(string keyId)
        {
            ValidateUpperHex(keyId, JasonQueryDbRecoveryConstants.RecoveryKeyIdHexLength, "The recovery key identifier is invalid.");
        }

        internal static byte[] DecodeKeyId(string keyId)
        {
            ValidateKeyId(keyId);

            var bytes = new byte[JasonQueryDbRecoveryConstants.RecoveryKeyIdSizeBytes];

            for (var index = 0; index < bytes.Length; index++)
            {
                var high = HexValue(keyId[index * 2]);
                var low = HexValue(keyId[(index * 2) + 1]);

                bytes[index] = (byte)((high << 4) | low);
            }

            return bytes;
        }

        private static string ComputeChecksum(string keyId, byte[] secret)
        {
            var prefix = Encoding.ASCII.GetBytes(JasonQueryDbRecoveryConstants.RecoveryKeyPrefix + "|" + keyId + "|");
            var payload = new byte[prefix.Length + secret.Length];
            byte[] hash = null;

            try
            {
                Buffer.BlockCopy(prefix, 0, payload, 0, prefix.Length);
                Buffer.BlockCopy(secret, 0, payload, prefix.Length, secret.Length);

                using (var sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(payload);
                }

                return ToUpperHex(hash, JasonQueryDbRecoveryConstants.RecoveryKeyChecksumSizeBytes);
            }
            finally
            {
                Array.Clear(prefix, 0, prefix.Length);
                Array.Clear(payload, 0, payload.Length);

                if (hash != null)
                {
                    Array.Clear(hash, 0, hash.Length);
                }
            }
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] FromBase64Url(string value)
        {
            if (value.Length != JasonQueryDbRecoveryConstants.RecoveryKeyEncodedSecretLength)
            {
                throw new FormatException("The recovery key secret encoding is invalid.");
            }

            for (var index = 0; index < value.Length; index++)
            {
                var ch = value[index];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-' || ch == '_')
                {
                    continue;
                }

                throw new FormatException("The recovery key secret encoding is invalid.");
            }

            var base64 = value.Replace('-', '+').Replace('_', '/') + "=";

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException ex)
            {
                throw new FormatException("The recovery key secret encoding is invalid.", ex);
            }
        }

        private static void ValidateUpperHex(string value, int expectedLength, string message)
        {
            if (string.IsNullOrEmpty(value) || value.Length != expectedLength)
            {
                throw new FormatException(message);
            }

            for (var index = 0; index < value.Length; index++)
            {
                var ch = value[index];

                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F'))
                {
                    continue;
                }

                throw new FormatException(message);
            }
        }

        private static int HexValue(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }

            return value - 'A' + 10;
        }

        private static string ToUpperHex(byte[] bytes)
        {
            return ToUpperHex(bytes, bytes.Length);
        }

        private static string ToUpperHex(byte[] bytes, int count)
        {
            const string Hex = "0123456789ABCDEF";
            var chars = new char[count * 2];

            for (var index = 0; index < count; index++)
            {
                chars[index * 2] = Hex[bytes[index] >> 4];
                chars[(index * 2) + 1] = Hex[bytes[index] & 0x0F];
            }

            return new string(chars);
        }

        private static bool FixedTimeEqualsAscii(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;

            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }
}
