using Newtonsoft.Json;
using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryMetadata
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("keyId")]
        public string KeyId { get; set; }

        [JsonProperty("kdf")]
        public string Kdf { get; set; }

        [JsonProperty("cipher")]
        public string Cipher { get; set; }

        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("salt")]
        public string Salt { get; set; }

        [JsonProperty("iv")]
        public string Iv { get; set; }

        [JsonProperty("wrappedDatabaseKey")]
        public string WrappedDatabaseKey { get; set; }

        [JsonProperty("authenticationTag")]
        public string AuthenticationTag { get; set; }

        public static JasonQueryDbRecoveryMetadata Create(JasonQueryDbRecoveryEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            var salt = envelope.CopySalt();
            var iv = envelope.CopyIv();
            var wrappedDatabaseKey = envelope.CopyWrappedDatabaseKey();
            var authenticationTag = envelope.CopyAuthenticationTag();

            try
            {
                return new JasonQueryDbRecoveryMetadata
                {
                    Version = envelope.Version,
                    KeyId = envelope.KeyId,
                    Kdf = envelope.Kdf,
                    Cipher = envelope.Cipher,
                    Mac = envelope.Mac,
                    Salt = Convert.ToBase64String(salt),
                    Iv = Convert.ToBase64String(iv),
                    WrappedDatabaseKey = Convert.ToBase64String(wrappedDatabaseKey),
                    AuthenticationTag = Convert.ToBase64String(authenticationTag)
                };
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);
                Array.Clear(iv, 0, iv.Length);
                Array.Clear(wrappedDatabaseKey, 0, wrappedDatabaseKey.Length);
                Array.Clear(authenticationTag, 0, authenticationTag.Length);
            }
        }

        public JasonQueryDbRecoveryEnvelope ToEnvelope()
        {
            byte[] salt = null;
            byte[] iv = null;
            byte[] wrappedDatabaseKey = null;
            byte[] authenticationTag = null;

            try
            {
                salt = DecodeBase64(Salt, "The recovery envelope salt is invalid.");
                iv = DecodeBase64(Iv, "The recovery envelope IV is invalid.");
                wrappedDatabaseKey = DecodeBase64(WrappedDatabaseKey, "The wrapped database key is invalid.");
                authenticationTag = DecodeBase64(AuthenticationTag, "The recovery authentication tag is invalid.");

                return new JasonQueryDbRecoveryEnvelope
                (
                    Version,
                    KeyId,
                    Kdf,
                    Cipher,
                    Mac,
                    salt,
                    iv,
                    wrappedDatabaseKey,
                    authenticationTag
                );
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException("The database recovery metadata is invalid.", ex);
            }
            finally
            {
                Clear(salt);
                Clear(iv);
                Clear(wrappedDatabaseKey);
                Clear(authenticationTag);
            }
        }

        public void Validate()
        {
            ToEnvelope();
        }

        public static JasonQueryDbRecoveryMetadata Clone(JasonQueryDbRecoveryMetadata source)
        {
            if (source == null)
            {
                return null;
            }

            return new JasonQueryDbRecoveryMetadata
            {
                Version = source.Version,
                KeyId = source.KeyId,
                Kdf = source.Kdf,
                Cipher = source.Cipher,
                Mac = source.Mac,
                Salt = source.Salt,
                Iv = source.Iv,
                WrappedDatabaseKey = source.WrappedDatabaseKey,
                AuthenticationTag = source.AuthenticationTag
            };
        }

        public static bool MetadataEquals(JasonQueryDbRecoveryMetadata left, JasonQueryDbRecoveryMetadata right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.Version == right.Version
                   && string.Equals(left.KeyId, right.KeyId, StringComparison.Ordinal)
                   && string.Equals(left.Kdf, right.Kdf, StringComparison.Ordinal)
                   && string.Equals(left.Cipher, right.Cipher, StringComparison.Ordinal)
                   && string.Equals(left.Mac, right.Mac, StringComparison.Ordinal)
                   && string.Equals(left.Salt, right.Salt, StringComparison.Ordinal)
                   && string.Equals(left.Iv, right.Iv, StringComparison.Ordinal)
                   && string.Equals(left.WrappedDatabaseKey, right.WrappedDatabaseKey, StringComparison.Ordinal)
                   && string.Equals(left.AuthenticationTag, right.AuthenticationTag, StringComparison.Ordinal);
        }

        private static byte[] DecodeBase64(string value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException(message);
            }

            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException(message, ex);
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
