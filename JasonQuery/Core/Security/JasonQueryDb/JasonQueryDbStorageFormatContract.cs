using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Defines the persisted physical storage-format contract for JasonQuery.db.
    ///
    /// A missing persisted storageFormatVersion intentionally means the
    /// historical Legacy V1 format. This is a versioned compatibility rule,
    /// not a provider-probing or exception-based fallback.
    ///
    /// Storage V2 is the SQLCipher 4 compatibility family qualified by
    /// JasonQuery. SQLCipher/SQLite/OpenSSL/provider patch versions are
    /// implementation details and are not persisted as the format identity.
    ///
    /// Unknown versions fail closed.
    /// </summary>
    public static class JasonQueryDbStorageFormatContract
    {
        public const int LegacyVersion = (int)JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi;

        public const int ModernVersion = (int)JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4;

        //Step 388 defines the V2 target contract only.
        //Normal JasonQuery runtime remains on Legacy V1 until the dedicated
        //migration/runtime-cutover steps are implemented and activated.
        public const int CurrentVersion = LegacyVersion;

        //Storage V2 on-disk compatibility profile.
        //These values describe SQLCipher's database-file compatibility parameters. They are intentionally separate from the user-password KDF stored in JasonQueryDbSecurityMetadata.
        public const int ModernSqlCipherCompatibility = 4;
        public const int ModernCipherPageSize = 4096;
        public const int ModernKdfIterations = 256000;
        public const string ModernKdfAlgorithm = "PBKDF2_HMAC_SHA512";
        public const string ModernHmacAlgorithm = "HMAC_SHA512";
        public const int ModernUseHmac = 1;
        public const int ModernPlaintextHeaderSize = 0;

        /// <summary>
        /// Resolves persisted storage-format state.
        ///
        /// Missing version information is a defined representation of the
        /// historical Legacy V1 format. Explicit V1 and V2 markers are
        /// recognized. Unknown versions are rejected.
        /// </summary>
        public static JasonQueryDbStorageFormatVersion ResolveVersion(int? persistedVersion)
        {
            if (!persistedVersion.HasValue || persistedVersion.Value == LegacyVersion)
            {
                return JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi;
            }

            if (persistedVersion.Value == ModernVersion)
            {
                return JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4;
            }

            throw new NotSupportedException
            (
                $"Unsupported JasonQuery.db storage format version " +
                $"'{persistedVersion.Value}'."
            );
        }

        /// <summary>
        /// Returns true only when the resolved persisted format differs from
        /// the storage format supported by the current production runtime.
        ///
        /// During Step 388, CurrentVersion intentionally remains Legacy V1.
        /// Therefore an explicit V2 marker is recognized but still requires
        /// the future migration/runtime-cutover path before normal use.
        /// </summary>
        public static bool RequiresMigration(int? persistedVersion)
        {
            return (int)ResolveVersion(persistedVersion) != CurrentVersion;
        }

        /// <summary>
        /// Guards code paths that require the storage format supported by the
        /// current production runtime. Unknown versions fail closed through
        /// ResolveVersion.
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
