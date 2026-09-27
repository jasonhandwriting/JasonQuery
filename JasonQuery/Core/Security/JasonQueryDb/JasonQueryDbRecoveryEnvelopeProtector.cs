using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbRecoveryEnvelopeProtector
    {
        private static readonly byte[] PayloadMagic = { (byte)'J', (byte)'Q', (byte)'R', (byte)'E' };

        public static JasonQueryDbRecoveryEnvelope Protect(byte[] databaseKey, JasonQueryDbRecoveryKeyMaterial recoveryKey)
        {
            ValidateDatabaseKey(databaseKey);

            if (recoveryKey == null)
            {
                throw new ArgumentNullException(nameof(recoveryKey));
            }

            byte[] recoverySecret = null;
            byte[] salt = null;
            byte[] iv = null;
            byte[] encryptionKey = null;
            byte[] authenticationKey = null;
            byte[] ciphertext = null;
            byte[] payload = null;
            byte[] authenticationTag = null;

            try
            {
                recoverySecret = recoveryKey.CopySecret();
                salt = RandomBytes(JasonQueryDbRecoveryConstants.HkdfSaltSizeBytes);
                iv = RandomBytes(JasonQueryDbRecoveryConstants.AesIvSizeBytes);
                encryptionKey = JasonQueryDbRecoveryHkdf.DeriveSha256(recoverySecret, salt, JasonQueryDbRecoveryConstants.EncryptionInfo);
                authenticationKey = JasonQueryDbRecoveryHkdf.DeriveSha256(recoverySecret, salt, JasonQueryDbRecoveryConstants.AuthenticationInfo);
                ciphertext = Encrypt(databaseKey, encryptionKey, iv);

                payload = BuildAuthenticatedPayload
                (
                    JasonQueryDbRecoveryConstants.RecoveryEnvelopeVersion,
                    recoveryKey.KeyId,
                    salt,
                    iv,
                    ciphertext
                );

                using (var hmac = new HMACSHA256(authenticationKey))
                {
                    authenticationTag = hmac.ComputeHash(payload);
                }

                return new JasonQueryDbRecoveryEnvelope
                (
                    JasonQueryDbRecoveryConstants.RecoveryEnvelopeVersion,
                    recoveryKey.KeyId,
                    JasonQueryDbRecoveryConstants.HkdfSha256,
                    JasonQueryDbRecoveryConstants.Aes256Cbc,
                    JasonQueryDbRecoveryConstants.HmacSha256,
                    salt,
                    iv,
                    ciphertext,
                    authenticationTag
                );
            }
            finally
            {
                Clear(recoverySecret);
                Clear(salt);
                Clear(iv);
                Clear(encryptionKey);
                Clear(authenticationKey);
                Clear(ciphertext);
                Clear(payload);
                Clear(authenticationTag);
            }
        }

        public static byte[] Unprotect(JasonQueryDbRecoveryEnvelope envelope, JasonQueryDbRecoveryKeyMaterial recoveryKey)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            if (recoveryKey == null)
            {
                throw new ArgumentNullException(nameof(recoveryKey));
            }

            if (!string.Equals(envelope.KeyId, recoveryKey.KeyId, StringComparison.Ordinal))
            {
                throw new CryptographicException("The recovery key does not match the recovery envelope.");
            }

            byte[] recoverySecret = null;
            byte[] salt = null;
            byte[] iv = null;
            byte[] ciphertext = null;
            byte[] expectedTag = null;
            byte[] encryptionKey = null;
            byte[] authenticationKey = null;
            byte[] payload = null;
            byte[] actualTag = null;
            byte[] databaseKey = null;

            try
            {
                recoverySecret = recoveryKey.CopySecret();
                salt = envelope.CopySalt();
                iv = envelope.CopyIv();
                ciphertext = envelope.CopyWrappedDatabaseKey();
                expectedTag = envelope.CopyAuthenticationTag();

                encryptionKey = JasonQueryDbRecoveryHkdf.DeriveSha256(recoverySecret, salt, JasonQueryDbRecoveryConstants.EncryptionInfo);
                authenticationKey = JasonQueryDbRecoveryHkdf.DeriveSha256(recoverySecret, salt, JasonQueryDbRecoveryConstants.AuthenticationInfo);
                payload = BuildAuthenticatedPayload(envelope.Version, envelope.KeyId, salt, iv, ciphertext);

                using (var hmac = new HMACSHA256(authenticationKey))
                {
                    actualTag = hmac.ComputeHash(payload);
                }

                if (!FixedTimeEquals(actualTag, expectedTag))
                {
                    throw new CryptographicException("Recovery envelope authentication failed.");
                }

                databaseKey = Decrypt(ciphertext, encryptionKey, iv);

                if (databaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes)
                {
                    throw new CryptographicException("The recovered database key length is invalid.");
                }

                var result = databaseKey;

                databaseKey = null;
                return result;
            }
            finally
            {
                Clear(recoverySecret);
                Clear(salt);
                Clear(iv);
                Clear(ciphertext);
                Clear(expectedTag);
                Clear(encryptionKey);
                Clear(authenticationKey);
                Clear(payload);
                Clear(actualTag);
                Clear(databaseKey);
            }
        }

        private static byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plaintext, 0, plaintext.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        private static byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var input = new MemoryStream(ciphertext, false))
                using (var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var output = new MemoryStream())
                {
                    var buffer = new byte[64];

                    try
                    {
                        while (true)
                        {
                            var read = cryptoStream.Read(buffer, 0, buffer.Length);

                            if (read <= 0)
                            {
                                break;
                            }

                            output.Write(buffer, 0, read);
                        }

                        return output.ToArray();
                    }
                    finally
                    {
                        Array.Clear(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        private static byte[] BuildAuthenticatedPayload(int version, string keyId, byte[] salt, byte[] iv, byte[] ciphertext)
        {
            var keyIdBytes = JasonQueryDbRecoveryKeyCodec.DecodeKeyId(keyId);
            var payloadLength = PayloadMagic.Length + 1 + keyIdBytes.Length + salt.Length + iv.Length + 4 + ciphertext.Length;
            var payload = new byte[payloadLength];
            var offset = 0;

            try
            {
                Buffer.BlockCopy(PayloadMagic, 0, payload, offset, PayloadMagic.Length);
                offset += PayloadMagic.Length;

                payload[offset++] = checked((byte)version);

                Buffer.BlockCopy(keyIdBytes, 0, payload, offset, keyIdBytes.Length);
                offset += keyIdBytes.Length;

                Buffer.BlockCopy(salt, 0, payload, offset, salt.Length);
                offset += salt.Length;

                Buffer.BlockCopy(iv, 0, payload, offset, iv.Length);
                offset += iv.Length;

                WriteInt32BigEndian(payload, offset, ciphertext.Length);
                offset += 4;

                Buffer.BlockCopy(ciphertext, 0, payload, offset, ciphertext.Length);
                return payload;
            }
            finally
            {
                Array.Clear(keyIdBytes, 0, keyIdBytes.Length);
            }
        }

        private static void WriteInt32BigEndian(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)((value >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(value & 0xFF);
        }

        private static byte[] RandomBytes(int count)
        {
            var bytes = new byte[count];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(bytes);
            }

            return bytes;
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
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

        private static void ValidateDatabaseKey(byte[] databaseKey)
        {
            if (databaseKey == null || databaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes)
            {
                throw new ArgumentException
                (
                    $"The database key must contain {JasonQueryDbSecurityConstants.DatabaseKeySizeBytes} bytes.",
                    nameof(databaseKey)
                );
            }
        }

        private static void Clear(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }
    }
}
