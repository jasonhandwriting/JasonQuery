using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbCustomPasswordKeyDeriver
    {
        public static byte[] CreateSalt()
        {
            var salt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];

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

            if (salt == null || salt.Length < JasonQueryDbSecurityConstants.PasswordSaltSizeBytes)
            {
                throw new ArgumentException($"The salt must contain at least {JasonQueryDbSecurityConstants.PasswordSaltSizeBytes} bytes.", nameof(salt));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return keyDerivation.GetBytes(JasonQueryDbSecurityConstants.DatabaseKeySizeBytes);
            }
        }

        public static string DeriveDatabasePassword(string password, byte[] salt, int iterations)
        {
            var databaseKey = DeriveDatabaseKey(password, salt, iterations);

            try
            {
                return JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey);
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }
    }
}
