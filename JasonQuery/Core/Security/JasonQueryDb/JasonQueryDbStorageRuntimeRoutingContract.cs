using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Resolves the normal JasonQuery.db runtime exclusively from the persisted
    /// storageFormatVersion contract.
    ///
    /// Missing storageFormatVersion is the historical Legacy V1 representation.
    /// Explicit V1 routes to the legacy System.Data.SQLite runtime. Explicit V2
    /// routes to the modern SQLCipher runtime. Unknown versions fail closed.
    ///
    /// This contract does not probe providers, inspect database headers, attempt
    /// fallback opens, create connections, or perform migration.
    /// </summary>
    internal static class JasonQueryDbStorageRuntimeRoutingContract
    {
        public static JasonQueryDbStorageRuntimeRoute Resolve(int? persistedStorageFormatVersion)
        {
            var storageFormatVersion = JasonQueryDbStorageFormatContract.ResolveVersion
            (
                persistedStorageFormatVersion
            );

            switch (storageFormatVersion)
            {
                case JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi:
                    {
                        return new JasonQueryDbStorageRuntimeRoute
                        (
                            storageFormatVersion,
                            JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite
                        );
                    }
                case JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4:
                    {
                        return new JasonQueryDbStorageRuntimeRoute
                        (
                            storageFormatVersion,
                            JasonQueryDbStorageRuntimeKind.ModernSqlCipher
                        );
                    }
                default:
                    {
                        throw new NotSupportedException
                        (
                            $"Storage format '{(int)storageFormatVersion}' " +
                            "does not have a qualified JasonQuery runtime route."
                        );
                    }
            }
        }

        public static JasonQueryDbStorageRuntimeRoute Resolve(IJasonQueryDbSecurityMetadataStore metadataStore)
        {
            if (metadataStore == null)
            {
                throw new ArgumentNullException(nameof(metadataStore));
            }

            if (!metadataStore.Exists)
            {
                return Resolve((int?)null);
            }

            var metadata = metadataStore.Load();

            if (metadata == null)
            {
                throw new InvalidDataException
                (
                    "Database security metadata could not be loaded for storage runtime routing."
                );
            }

            return Resolve
            (
                metadata.StorageFormatVersion
            );
        }
    }
}
