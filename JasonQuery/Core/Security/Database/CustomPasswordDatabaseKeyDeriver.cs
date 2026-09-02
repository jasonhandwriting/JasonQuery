using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.Database
{
    public static class CustomPasswordDatabaseKeyDeriver
    {
        public static byte[] CreateSalt()
        {
            var salt = new byte[DatabaseSecurityConstants.PasswordSaltSizeBytes];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            return salt;
        }

        public static byte[] DeriveDatabaseKey(string password, byte[] salt, int iterations)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("A custom database password is required.", nameof(password));
            }

            if (salt == null || salt.Length < DatabaseSecurityConstants.PasswordSaltSizeBytes)
            {
                throw new ArgumentException($"The salt must contain at least {DatabaseSecurityConstants.PasswordSaltSizeBytes} bytes.", nameof(salt));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return keyDerivation.GetBytes(DatabaseSecurityConstants.DatabaseKeySizeBytes);
            }
        }

        public static string DeriveDatabasePassword(string password, byte[] salt, int iterations)
        {
            var databaseKey = DeriveDatabaseKey(password, salt, iterations);

            try
            {
                return DatabaseKeyGenerator.ToDatabasePassword(databaseKey);
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }
    }
}
