using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityMetadata
    {
        [JsonProperty("metadataVersion")]
        public int MetadataVersion { get; set; }

        [JsonProperty("encryptionVersion")]
        public int EncryptionVersion { get; set; }

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseSecurityMode Mode { get; set; }

        [JsonProperty("protection", NullValueHandling = NullValueHandling.Ignore)]
        public string Protection { get; set; }

        [JsonProperty("protectedDatabaseKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ProtectedDatabaseKey { get; set; }

        [JsonProperty("kdf", NullValueHandling = NullValueHandling.Ignore)]
        public string Kdf { get; set; }

        [JsonProperty("iterations", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Iterations { get; set; }

        [JsonProperty("salt", NullValueHandling = NullValueHandling.Ignore)]
        public string Salt { get; set; }

        public static DatabaseSecurityMetadata CreateWindowsCurrentUser(string protectedDatabaseKey)
        {
            if (string.IsNullOrWhiteSpace(protectedDatabaseKey))
            {
                throw new ArgumentException("A protected database key is required.", nameof(protectedDatabaseKey));
            }

            return new DatabaseSecurityMetadata
            {
                MetadataVersion = DatabaseSecurityConstants.MetadataVersion,
                EncryptionVersion = DatabaseSecurityConstants.EncryptionVersion,
                Mode = DatabaseSecurityMode.WindowsCurrentUser,
                Protection = "DPAPI-CurrentUser",
                ProtectedDatabaseKey = protectedDatabaseKey
            };
        }

        public static DatabaseSecurityMetadata CreateCustomPassword(byte[] salt, int iterations)
        {
            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException("A password salt is required.", nameof(salt));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            return new DatabaseSecurityMetadata
            {
                MetadataVersion = DatabaseSecurityConstants.MetadataVersion,
                EncryptionVersion = DatabaseSecurityConstants.EncryptionVersion,
                Mode = DatabaseSecurityMode.CustomPassword,
                Kdf = DatabaseSecurityConstants.Pbkdf2HmacSha256,
                Iterations = iterations,
                Salt = Convert.ToBase64String(salt)
            };
        }

        public void Validate()
        {
            if (MetadataVersion != DatabaseSecurityConstants.MetadataVersion)
            {
                throw new NotSupportedException($"Database security metadata version {MetadataVersion} is not supported.");
            }

            if (EncryptionVersion != DatabaseSecurityConstants.EncryptionVersion)
            {
                throw new NotSupportedException($"Database encryption version {EncryptionVersion} is not supported.");
            }

            switch (Mode)
            {
                case DatabaseSecurityMode.WindowsCurrentUser:
                    {
                        ValidateWindowsCurrentUser();
                        break;
                    }
                case DatabaseSecurityMode.CustomPassword:
                    {
                        ValidateCustomPassword();
                        break;
                    }
                default:
                    {
                        throw new InvalidDataException($"Database security mode '{Mode}' is not supported.");
                    }
            }
        }

        private void ValidateWindowsCurrentUser()
        {
            if (!string.Equals(Protection, "DPAPI-CurrentUser", StringComparison.Ordinal))
            {
                throw new InvalidDataException("The Windows database-key protection value is invalid.");
            }

            if (string.IsNullOrWhiteSpace(ProtectedDatabaseKey))
            {
                throw new InvalidDataException("The protected database key is missing.");
            }

            ValidateBase64(ProtectedDatabaseKey, "The protected database key is invalid.");
        }

        private void ValidateCustomPassword()
        {
            if (!string.Equals(Kdf, DatabaseSecurityConstants.Pbkdf2HmacSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The database password KDF is invalid.");
            }

            if (Iterations <= 0)
            {
                throw new InvalidDataException("The database password iteration count is invalid.");
            }

            if (string.IsNullOrWhiteSpace(Salt))
            {
                throw new InvalidDataException("The database password salt is missing.");
            }

            var salt = ValidateBase64(Salt, "The database password salt is invalid.");

            if (salt.Length < DatabaseSecurityConstants.PasswordSaltSizeBytes)
            {
                throw new InvalidDataException("The database password salt is too short.");
            }
        }

        private static byte[] ValidateBase64(string value, string message)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException(message, ex);
            }
        }
    }
}
