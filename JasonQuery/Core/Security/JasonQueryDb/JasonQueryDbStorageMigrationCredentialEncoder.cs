using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Encodes the existing 32-byte logical database key into the canonical
    /// UTF-8 Base64 password bytes consumed by the isolated Storage V2 helpers.
    /// No immutable database-password string is created.
    /// </summary>
    internal static class JasonQueryDbStorageMigrationCredentialEncoder
    {
        public static byte[] EncodeDatabaseKey(byte[] databaseKey)
        {
            if (databaseKey == null)
            {
                throw new ArgumentNullException(nameof(databaseKey));
            }

            if (databaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes)
            {
                throw new ArgumentException
                (
                    $"The database key must contain exactly {JasonQueryDbSecurityConstants.DatabaseKeySizeBytes} bytes.",
                    nameof(databaseKey)
                );
            }

            var base64CharacterCount = ((databaseKey.Length + 2) / 3) * 4;
            var base64Characters = new char[base64CharacterCount];

            try
            {
                var written = Convert.ToBase64CharArray
                (
                    databaseKey,
                    0,
                    databaseKey.Length,
                    base64Characters,
                    0
                );

                var passwordUtf8 = Encoding.UTF8.GetBytes
                (
                    base64Characters,
                    0,
                    written
                );

                if (passwordUtf8.Length <= 0 || passwordUtf8.Length > JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes)
                {
                    Array.Clear(passwordUtf8, 0, passwordUtf8.Length);

                    throw new InvalidDataException
                    (
                        "The encoded Storage V2 database password is outside the qualified protocol bounds."
                    );
                }

                return passwordUtf8;
            }
            finally
            {
                Array.Clear(base64Characters, 0, base64Characters.Length);
            }
        }
    }
}
