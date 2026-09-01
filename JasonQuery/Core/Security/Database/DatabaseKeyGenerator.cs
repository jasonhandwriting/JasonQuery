using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.Database
{
    public static class DatabaseKeyGenerator
    {
        public static byte[] Generate()
        {
            var databaseKey = new byte[DatabaseSecurityConstants.DatabaseKeySizeBytes];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(databaseKey);
            }

            return databaseKey;
        }

        public static string ToDatabasePassword(byte[] databaseKey)
        {
            if (databaseKey == null || databaseKey.Length != DatabaseSecurityConstants.DatabaseKeySizeBytes)
            {
                throw new ArgumentException($"The database key must contain {DatabaseSecurityConstants.DatabaseKeySizeBytes} bytes.", nameof(databaseKey));
            }

            return Convert.ToBase64String(databaseKey);
        }
    }
}
