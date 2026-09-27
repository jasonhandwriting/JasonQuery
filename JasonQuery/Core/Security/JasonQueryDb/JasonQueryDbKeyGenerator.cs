using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbKeyGenerator
    {
        public static byte[] Generate()
        {
            var databaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(databaseKey);
            }

            return databaseKey;
        }

        public static string ToDatabasePassword(byte[] databaseKey)
        {
            if (databaseKey == null || databaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes)
            {
                throw new ArgumentException($"The database key must contain {JasonQueryDbSecurityConstants.DatabaseKeySizeBytes} bytes.", nameof(databaseKey));
            }

            return Convert.ToBase64String(databaseKey);
        }
    }
}
