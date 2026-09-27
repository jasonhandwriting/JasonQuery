using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// R4F2C production boundary for one persisted Storage V1 -> V2 cutover.
    /// Routing is derived only from storageFormatVersion. No provider probing or fallback is permitted.
    /// </summary>
    internal sealed class JasonQueryDbStorageRuntimeCutoverCoordinator
    {
        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly IJasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly Func<string, string, byte[], byte[], JasonQueryDbStorageMigrationCoordinatorResult> _migrate;

        public JasonQueryDbStorageRuntimeCutoverCoordinator(string databaseFilePath, string metadataFilePath, IJasonQueryDbSecurityMetadataStore metadataStore)
            : this(databaseFilePath, metadataFilePath, metadataStore, null)
        {
        }

        internal JasonQueryDbStorageRuntimeCutoverCoordinator(string databaseFilePath, string metadataFilePath, IJasonQueryDbSecurityMetadataStore metadataStore,
                                                              Func<string, string, byte[], byte[], JasonQueryDbStorageMigrationCoordinatorResult> migrate)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            if (string.IsNullOrWhiteSpace(metadataFilePath))
            {
                throw new ArgumentException("A metadata file path is required.", nameof(metadataFilePath));
            }

            _databaseFilePath = Path.GetFullPath(databaseFilePath);
            _metadataFilePath = Path.GetFullPath(metadataFilePath);
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _migrate = migrate ?? MigrateWithQualifiedCoordinator;
        }

        public JasonQueryDbStorageRuntimeRoute ResolvePersistedRoute()
        {
            return JasonQueryDbStorageRuntimeRoutingContract.Resolve(_metadataStore);
        }

        public JasonQueryDbStorageMigrationCoordinatorResult MigrateLegacyStorageToModern(string legacyHelperFilePath, string modernHelperFilePath,
                                                                                          string resolvedDatabasePassword)
        {
            var before = ResolvePersistedRoute();

            if (before.RuntimeKind != JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)
            {
                throw new InvalidOperationException("A new physical migration may start only from persisted Storage V1.");
            }

            ValidateHelper(legacyHelperFilePath, "Legacy Storage V1 migration helper");
            ValidateHelper(modernHelperFilePath, "Modern Storage V2 migration helper");

            if (string.IsNullOrWhiteSpace(resolvedDatabasePassword))
            {
                throw new ArgumentException("A resolved database password is required.", nameof(resolvedDatabasePassword));
            }

            var passwordUtf8 = new UTF8Encoding(false, true).GetBytes(resolvedDatabasePassword);

            try
            {
                var result = _migrate(legacyHelperFilePath, modernHelperFilePath, passwordUtf8, passwordUtf8);
                var after = ResolvePersistedRoute();

                if (after.RuntimeKind != JasonQueryDbStorageRuntimeKind.ModernSqlCipher || after.StorageFormatVersion != JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4)
                {
                    throw new InvalidDataException("Storage migration completed without committing the persisted Storage V2 route.");
                }

                return result;
            }
            finally
            {
                Array.Clear(passwordUtf8, 0, passwordUtf8.Length);
            }
        }

        private JasonQueryDbStorageMigrationCoordinatorResult MigrateWithQualifiedCoordinator(string legacyHelperFilePath, string modernHelperFilePath,
                                                                                              byte[] sourcePasswordUtf8, byte[] targetPasswordUtf8)
        {
            return new JasonQueryDbStorageMigrationCoordinator(_databaseFilePath, _metadataFilePath).Migrate
            (
                legacyHelperFilePath,
                modernHelperFilePath,
                sourcePasswordUtf8,
                targetPasswordUtf8
            );
        }

        private static void ValidateHelper(string helperFilePath, string description)
        {
            if (string.IsNullOrWhiteSpace(helperFilePath))
            {
                throw new ArgumentException(description + " path is required.", nameof(helperFilePath));
            }

            if (!File.Exists(helperFilePath))
            {
                throw new FileNotFoundException(description + " was not found.", helperFilePath);
            }
        }
    }
}
