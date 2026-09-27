using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbRecoveryConstants
    {
        public const int RecoveryEnvelopeVersion = 1;
        public const int RecoveryKeySizeBytes = 32;
        public const int RecoveryKeyIdSizeBytes = 4;
        public const int RecoveryKeyIdHexLength = RecoveryKeyIdSizeBytes * 2;
        public const int RecoveryKeyChecksumSizeBytes = 4;
        public const int RecoveryKeyChecksumHexLength = RecoveryKeyChecksumSizeBytes * 2;
        public const int RecoveryKeyEncodedSecretLength = 43;
        public const int RecoveryKeyEncodedLength = 65;
        public const int HkdfSaltSizeBytes = 32;
        public const int AesIvSizeBytes = 16;
        public const int AuthenticationTagSizeBytes = 32;
        public const int WrappedDatabaseKeySizeBytes = 48;

        public const string RecoveryKeyPrefix = "JQ1";
        public const string HkdfSha256 = "HKDF-SHA256";
        public const string Aes256Cbc = "AES-256-CBC";
        public const string HmacSha256 = "HMAC-SHA256";

        internal const string EncryptionInfo = "JasonQuery/Recovery/v1/enc";
        internal const string AuthenticationInfo = "JasonQuery/Recovery/v1/mac";
    }
}
