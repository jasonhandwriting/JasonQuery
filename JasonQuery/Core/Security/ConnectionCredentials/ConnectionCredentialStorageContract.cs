using System;

namespace JasonQuery.Core.Security.ConnectionCredentials
{
    /// <summary>
    /// Defines the database-wide storage contract for DBInfo.Password.
    ///
    /// A missing persisted version is intentionally treated as the historical
    /// legacy format. This prevents an existing database from being interpreted
    /// as V2 merely because no version marker is present.
    ///
    /// V2 applies no additional per-field credential encryption. The logical
    /// password value is stored inside the Database Security V2-protected
    /// JasonQuery database.
    /// </summary>
    public static class ConnectionCredentialStorageContract
    {
        public const int LegacyVersion = (int)ConnectionCredentialStorageVersion.LegacyDomainUserProtected;

        public const int CurrentVersion = (int)ConnectionCredentialStorageVersion.DatabaseProtectedLogicalValue;

        /// <summary>
        /// Resolves the persisted database-wide credential storage version.
        /// Missing version information always means the historical legacy format.
        /// Unknown versions are rejected instead of being guessed.
        /// </summary>
        public static ConnectionCredentialStorageVersion ResolveVersion(int? persistedVersion)
        {
            if (!persistedVersion.HasValue || persistedVersion.Value == LegacyVersion)
            {
                return ConnectionCredentialStorageVersion.LegacyDomainUserProtected;
            }

            if (persistedVersion.Value == CurrentVersion)
            {
                return ConnectionCredentialStorageVersion.DatabaseProtectedLogicalValue;
            }

            throw new NotSupportedException
            (
                $"Unsupported connection credential storage version '{persistedVersion.Value}'."
            );
        }

        /// <summary>
        /// Returns true only when the database still uses the historical
        /// connection credential format and therefore requires migration.
        /// Unknown versions fail closed through ResolveVersion.
        /// </summary>
        public static bool RequiresMigration(int? persistedVersion)
        {
            return ResolveVersion(persistedVersion) == ConnectionCredentialStorageVersion.LegacyDomainUserProtected;
        }

        /// <summary>
        /// Guards V2 runtime access. Legacy or missing version information must
        /// be migrated before DBInfo.Password can be interpreted as a V2 value.
        /// Unknown versions fail closed through ResolveVersion.
        /// </summary>
        public static void EnsureV2Ready(int? persistedVersion)
        {
            var version = ResolveVersion(persistedVersion);

            if (version != ConnectionCredentialStorageVersion.DatabaseProtectedLogicalValue)
            {
                throw new InvalidOperationException
                (
                    "Connection credentials must be migrated to V2 before current-format credential access is allowed."
                );
            }
        }

        /// <summary>
        /// Converts a logical password to the V2 DBInfo.Password value.
        /// V2 intentionally performs no per-field encryption or encoding.
        /// Null represents no saved credential and is canonicalized to an empty string.
        /// </summary>
        public static string ToV2StoredValue(string logicalPassword)
        {
            return logicalPassword ?? string.Empty;
        }

        /// <summary>
        /// Converts a V2 DBInfo.Password value back to the logical password.
        /// V2 intentionally performs no per-field decryption or decoding.
        /// Null is treated as no saved credential.
        /// </summary>
        public static string FromV2StoredValue(string storedPassword)
        {
            return storedPassword ?? string.Empty;
        }
    }
}
