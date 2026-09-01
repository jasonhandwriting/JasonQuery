using System;

namespace JasonQuery.Core.Security.Database
{
    public static class DatabaseSecurityConstants
    {
        public const int MetadataVersion = 1;
        public const int EncryptionVersion = 2;
        public const int DatabaseKeySizeBytes = 32;
        public const int PasswordSaltSizeBytes = 16;
        public const int DefaultPbkdf2Iterations = 600000;

        public const string MetadataFileName = "JasonQuery.security.json";
        public const string Pbkdf2HmacSha256 = "PBKDF2-HMAC-SHA256";

        internal static readonly byte[] DpapiOptionalEntropy = System.Text.Encoding.UTF8.GetBytes("JasonQuery.DatabaseSecurity.V2");
    }
}
