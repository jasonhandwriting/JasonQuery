using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryEnvelope
    {
        private readonly byte[] _salt;
        private readonly byte[] _iv;
        private readonly byte[] _wrappedDatabaseKey;
        private readonly byte[] _authenticationTag;

        public JasonQueryDbRecoveryEnvelope(int version, string keyId, string kdf, string cipher, string mac,
                                            byte[] salt, byte[] iv, byte[] wrappedDatabaseKey, byte[] authenticationTag)
        {
            if (version != JasonQueryDbRecoveryConstants.RecoveryEnvelopeVersion)
            {
                throw new InvalidDataException($"Recovery envelope version {version} is not supported.");
            }

            JasonQueryDbRecoveryKeyCodec.ValidateKeyId(keyId);

            if (!string.Equals(kdf, JasonQueryDbRecoveryConstants.HkdfSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The recovery envelope KDF is not supported.");
            }

            if (!string.Equals(cipher, JasonQueryDbRecoveryConstants.Aes256Cbc, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The recovery envelope cipher is not supported.");
            }

            if (!string.Equals(mac, JasonQueryDbRecoveryConstants.HmacSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The recovery envelope MAC is not supported.");
            }

            ValidateLength(salt, JasonQueryDbRecoveryConstants.HkdfSaltSizeBytes, nameof(salt));
            ValidateLength(iv, JasonQueryDbRecoveryConstants.AesIvSizeBytes, nameof(iv));
            ValidateLength(wrappedDatabaseKey, JasonQueryDbRecoveryConstants.WrappedDatabaseKeySizeBytes, nameof(wrappedDatabaseKey));
            ValidateLength(authenticationTag, JasonQueryDbRecoveryConstants.AuthenticationTagSizeBytes, nameof(authenticationTag));

            Version = version;
            KeyId = keyId;
            Kdf = kdf;
            Cipher = cipher;
            Mac = mac;
            _salt = (byte[])salt.Clone();
            _iv = (byte[])iv.Clone();
            _wrappedDatabaseKey = (byte[])wrappedDatabaseKey.Clone();
            _authenticationTag = (byte[])authenticationTag.Clone();
        }

        public int Version { get; }

        public string KeyId { get; }

        public string Kdf { get; }

        public string Cipher { get; }

        public string Mac { get; }

        public byte[] CopySalt()
        {
            return (byte[])_salt.Clone();
        }

        public byte[] CopyIv()
        {
            return (byte[])_iv.Clone();
        }

        public byte[] CopyWrappedDatabaseKey()
        {
            return (byte[])_wrappedDatabaseKey.Clone();
        }

        public byte[] CopyAuthenticationTag()
        {
            return (byte[])_authenticationTag.Clone();
        }

        private static void ValidateLength(byte[] value, int expectedLength, string parameterName)
        {
            if (value == null || value.Length != expectedLength)
            {
                throw new InvalidDataException
                (
                    $"{parameterName} must contain exactly {expectedLength} bytes."
                );
            }
        }
    }
}
