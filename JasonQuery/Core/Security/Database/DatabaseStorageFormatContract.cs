using System;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Defines the persisted physical storage-format contract for JasonQuery.db.
    ///
    /// A missing persisted storageFormatVersion intentionally means the
    /// historical Legacy V1 format. This is a versioned compatibility rule,
    /// not a provider-probing or exception-based fallback.
    ///
    /// Unknown versions fail closed.
    /// </summary>
    public static class DatabaseStorageFormatContract
    {
        public const int LegacyVersion = (int)DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi;

        // Until a modern encrypted SQLite format is selected and implemented,
        // the current production storage format remains the historical V1 format.
        public const int CurrentVersion = LegacyVersion;

        /// <summary>
        /// Resolves persisted storage-format state.
        ///
        /// Missing version information is a defined representation of the
        /// historical Legacy V1 format. Unknown versions are rejected.
        /// </summary>
        public static DatabaseStorageFormatVersion ResolveVersion(int? persistedVersion)
        {
            if (!persistedVersion.HasValue || persistedVersion.Value == LegacyVersion)
            {
                return DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi;
            }

            throw new NotSupportedException
            (
                $"Unsupported JasonQuery.db storage format version " +
                $"'{persistedVersion.Value}'."
            );
        }

        /// <summary>
        /// Returns true only when the resolved persisted format differs from
        /// the current canonical storage format.
        ///
        /// While CurrentVersion is still Legacy V1 this returns false for
        /// both a missing marker and an explicit V1 marker.
        /// </summary>
        public static bool RequiresMigration(int? persistedVersion)
        {
            return (int)ResolveVersion(persistedVersion) != CurrentVersion;
        }

        /// <summary>
        /// Guards code paths that require the current canonical storage format.
        /// Unknown versions fail closed through ResolveVersion.
        /// </summary>
        public static void EnsureCurrentReady(int? persistedVersion)
        {
            var resolvedVersion = ResolveVersion(persistedVersion);

            if ((int)resolvedVersion != CurrentVersion)
            {
                throw new InvalidOperationException
                (
                    "JasonQuery.db must be migrated to the current " +
                    "storage format before this operation is allowed."
                );
            }
        }
    }
}
