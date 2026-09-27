using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityMetadata
    {
        [JsonProperty("metadataVersion")]
        public int MetadataVersion { get; set; }

        [JsonProperty("encryptionVersion")]
        public int EncryptionVersion { get; set; }

        [JsonProperty("storageFormatVersion", NullValueHandling = NullValueHandling.Ignore)]
        public int? StorageFormatVersion { get; set; }

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public JasonQueryDbSecurityMode Mode { get; set; }

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

        [JsonProperty("recovery", NullValueHandling = NullValueHandling.Ignore)]
        public JasonQueryDbRecoveryMetadata Recovery { get; set; }

        public static JasonQueryDbSecurityMetadata CreateWindowsCurrentUser(string protectedDatabaseKey)
        {
            if (string.IsNullOrWhiteSpace(protectedDatabaseKey))
            {
                throw new ArgumentException("A protected database key is required.", nameof(protectedDatabaseKey));
            }

            return new JasonQueryDbSecurityMetadata
            {
                MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion,
                EncryptionVersion = JasonQueryDbSecurityConstants.EncryptionVersion,
                Mode = JasonQueryDbSecurityMode.WindowsCurrentUser,
                Protection = "DPAPI-CurrentUser",
                ProtectedDatabaseKey = protectedDatabaseKey
            };
        }

        public static JasonQueryDbSecurityMetadata CreateCustomPassword(byte[] salt, int iterations)
        {
            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException("A password salt is required.", nameof(salt));
            }

            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            return new JasonQueryDbSecurityMetadata
            {
                MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion,
                EncryptionVersion = JasonQueryDbSecurityConstants.EncryptionVersion,
                Mode = JasonQueryDbSecurityMode.CustomPassword,
                Kdf = JasonQueryDbSecurityConstants.Pbkdf2HmacSha256,
                Iterations = iterations,
                Salt = Convert.ToBase64String(salt)
            };
        }

        public void Validate()
        {
            if (MetadataVersion != JasonQueryDbSecurityConstants.LegacyMetadataVersion && MetadataVersion != JasonQueryDbSecurityConstants.MetadataVersion)
            {
                throw new NotSupportedException($"Database security metadata version {MetadataVersion} is not supported.");
            }

            ValidateRecoveryMetadataVersion();

            if (EncryptionVersion != JasonQueryDbSecurityConstants.EncryptionVersion)
            {
                throw new NotSupportedException($"Database encryption version {EncryptionVersion} is not supported.");
            }

            JasonQueryDbStorageFormatContract.ResolveVersion(StorageFormatVersion);

            switch (Mode)
            {
                case JasonQueryDbSecurityMode.WindowsCurrentUser:
                    {
                        ValidateWindowsCurrentUser();
                        ValidateWindowsRecovery();
                        break;
                    }
                case JasonQueryDbSecurityMode.CustomPassword:
                    {
                        ValidateCustomPassword();
                        ValidateCustomPasswordRecovery();
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
            if (!string.Equals(Kdf, JasonQueryDbSecurityConstants.Pbkdf2HmacSha256, StringComparison.Ordinal))
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

            if (salt.Length < JasonQueryDbSecurityConstants.PasswordSaltSizeBytes)
            {
                throw new InvalidDataException("The database password salt is too short.");
            }
        }

        private void ValidateRecoveryMetadataVersion()
        {
            if (MetadataVersion == JasonQueryDbSecurityConstants.LegacyMetadataVersion && Recovery != null)
            {
                throw new InvalidDataException("Database security metadata version 1 cannot contain recovery metadata.");
            }
        }

        private void ValidateWindowsRecovery()
        {
            if (Recovery != null)
            {
                Recovery.Validate();
            }
        }

        private void ValidateCustomPasswordRecovery()
        {
            if (Recovery != null)
            {
                throw new InvalidDataException("CustomPassword metadata cannot contain a database recovery envelope.");
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
