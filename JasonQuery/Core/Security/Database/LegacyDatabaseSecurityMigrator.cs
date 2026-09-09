using System;
using System.IO;

namespace JasonQuery.Core.Security.Database
{
    public sealed class LegacyDatabaseSecurityMigrator
    {
        private const string BackupFileSuffix = ".database-encryption-v2-legacy-backup";

        private readonly IDatabaseSecurityMetadataStore _metadataStore;
        private readonly IDatabaseKeyProtector _databaseKeyProtector;
        private readonly IDatabaseSecurityMigrationDatabase _migrationDatabase;

        public LegacyDatabaseSecurityMigrator(IDatabaseSecurityMetadataStore metadataStore, IDatabaseKeyProtector databaseKeyProtector,
                                              IDatabaseSecurityMigrationDatabase migrationDatabase)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
            _migrationDatabase = migrationDatabase ?? throw new ArgumentNullException(nameof(migrationDatabase));
        }

        public static string GetBackupFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.GetFullPath(databaseFilePath) + BackupFileSuffix;
        }

        public bool HasInterruptedMigrationBackup(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return File.Exists(GetBackupFilePath(databaseFilePath));
        }

        public bool CanRecoverInterruptedMigrationWithDefaultPassword(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (_metadataStore.Exists)
            {
                return false;
            }

            var backupFilePath = GetBackupFilePath(databaseFilePath);

            return File.Exists(backupFilePath) && _migrationDatabase.CanOpen(backupFilePath, LegacyDatabaseSecurity.LegacyDefaultDatabasePassword);
        }

        public bool CanOpenWithDefaultPassword(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            return File.Exists(databaseFilePath) && _migrationDatabase.CanOpen(databaseFilePath, LegacyDatabaseSecurity.LegacyDefaultDatabasePassword);
        }

        public bool IsCustomPasswordValid(string databaseFilePath, string customPassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (_metadataStore.Exists || string.IsNullOrEmpty(customPassword))
            {
                return false;
            }

            var legacyDatabasePassword = LegacyDatabaseSecurity.CreateCustomDatabasePassword(customPassword);
            var backupFilePath = GetBackupFilePath(databaseFilePath);
            var databasePathToValidate = File.Exists(backupFilePath) ? backupFilePath : databaseFilePath;

            return File.Exists(databasePathToValidate) && _migrationDatabase.CanOpen(databasePathToValidate, legacyDatabasePassword);
        }

        public bool RecoverInterruptedMigrationIfNeeded(string databaseFilePath)
        {
            return RecoverInterruptedMigrationIfNeeded
            (
                databaseFilePath,
                LegacyDatabaseSecurity.LegacyDefaultDatabasePassword
            );
        }

        public DatabaseSecurityMigrationResult MigrateToWindowsCurrentUser(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (_metadataStore.Exists)
            {
                throw new InvalidOperationException("Database Encryption V2 metadata already exists. Legacy migration cannot run.");
            }

            RecoverInterruptedMigrationIfNeeded
            (
                databaseFilePath,
                LegacyDatabaseSecurity.LegacyDefaultDatabasePassword
            );

            if (!File.Exists(databaseFilePath))
            {
                throw new FileNotFoundException("The JasonQuery database file was not found.", databaseFilePath);
            }

            if (!_migrationDatabase.CanOpen(databaseFilePath, LegacyDatabaseSecurity.LegacyDefaultDatabasePassword))
            {
                throw new DatabaseSecurityStartupException
                (
                    DatabaseSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword,
                    "The database security information is missing, and JasonQuery.db cannot be opened with the historical default database password."
                );
            }

            byte[] databaseKey = null;
            byte[] protectedDatabaseKey = null;

            try
            {
                databaseKey = DatabaseKeyGenerator.Generate();

                var databasePassword = DatabaseKeyGenerator.ToDatabasePassword(databaseKey);

                protectedDatabaseKey = _databaseKeyProtector.Protect(databaseKey);

                var metadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(protectedDatabaseKey));

                return MigrateCore
                (
                    databaseFilePath,
                    LegacyDatabaseSecurity.LegacyDefaultDatabasePassword,
                    databasePassword,
                    metadata
                );
            }
            finally
            {
                Clear(databaseKey);
                Clear(protectedDatabaseKey);
            }
        }

        public DatabaseSecurityMigrationResult MigrateToCustomPassword(string databaseFilePath, string customPassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (string.IsNullOrEmpty(customPassword))
            {
                throw new ArgumentException("A legacy custom password is required.", nameof(customPassword));
            }

            if (_metadataStore.Exists)
            {
                throw new InvalidOperationException("Database Encryption V2 metadata already exists. Legacy migration cannot run.");
            }

            var legacyDatabasePassword = LegacyDatabaseSecurity.CreateCustomDatabasePassword(customPassword);

            RecoverInterruptedMigrationIfNeeded(databaseFilePath, legacyDatabasePassword);

            if (!File.Exists(databaseFilePath))
            {
                throw new FileNotFoundException("The JasonQuery database file was not found.", databaseFilePath);
            }

            if (!_migrationDatabase.CanOpen(databaseFilePath, legacyDatabasePassword))
            {
                throw new DatabaseSecurityStartupException
                (
                    DatabaseSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword,
                    "The legacy custom password could not unlock JasonQuery.db."
                );
            }

            byte[] salt = null;

            try
            {
                salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();

                var iterations = DatabaseSecurityConstants.DefaultPbkdf2Iterations;

                var databasePassword = CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword
                (
                    customPassword,
                    salt,
                    iterations
                );

                var metadata = DatabaseSecurityMetadata.CreateCustomPassword(salt, iterations);

                return MigrateCore
                (
                    databaseFilePath,
                    legacyDatabasePassword,
                    databasePassword,
                    metadata
                );
            }
            finally
            {
                Clear(salt);
            }
        }

        public void CleanupCompletedMigrationBackup(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            var backupFilePath = GetBackupFilePath(databaseFilePath);

            if (!File.Exists(backupFilePath))
            {
                return;
            }

            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException
                (
                    "A Database Encryption V2 migration backup exists without V2 metadata. " +
                    "The backup cannot be deleted until interrupted migration recovery has completed."
                );
            }

            File.Delete(backupFilePath);
        }

        private bool RecoverInterruptedMigrationIfNeeded(string databaseFilePath, string legacyDatabasePassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (_metadataStore.Exists)
            {
                return false;
            }

            var backupFilePath = GetBackupFilePath(databaseFilePath);

            if (!File.Exists(backupFilePath))
            {
                return false;
            }

            RestoreLegacyDatabase(databaseFilePath, backupFilePath, legacyDatabasePassword);
            return true;
        }

        private DatabaseSecurityMigrationResult MigrateCore(string databaseFilePath, string sourceDatabasePassword,
                                                            string targetDatabasePassword, DatabaseSecurityMetadata metadata)
        {
            var backupFilePath = GetBackupFilePath(databaseFilePath);

            if (File.Exists(backupFilePath))
            {
                throw new InvalidDataException("A previous database migration backup still exists and could not be safely resolved.");
            }

            string candidateDatabaseFilePath = null;
            var metadataCommitted = false;

            try
            {
                candidateDatabaseFilePath = CreateTemporaryDatabaseFilePath(databaseFilePath, "candidate");

                _migrationDatabase.CreateVerifiedCopy
                (
                    databaseFilePath,
                    candidateDatabaseFilePath,
                    sourceDatabasePassword
                );

                _migrationDatabase.ChangePassword
                (
                    candidateDatabaseFilePath,
                    sourceDatabasePassword,
                    targetDatabasePassword
                );

                ValidateV2Candidate
                (
                    candidateDatabaseFilePath,
                    targetDatabasePassword,
                    sourceDatabasePassword
                );

                File.Replace(candidateDatabaseFilePath, databaseFilePath, backupFilePath, true);
                candidateDatabaseFilePath = null;

                ValidateV2Candidate
                (
                    databaseFilePath,
                    targetDatabasePassword,
                    sourceDatabasePassword
                );

                _metadataStore.Save(metadata);
                ValidateCommittedMetadata(metadata);
                metadataCommitted = true;

                CleanupCompletedMigrationBackup(databaseFilePath);

                return new DatabaseSecurityMigrationResult(metadata, targetDatabasePassword);
            }
            catch (Exception migrationException)
            {
                if (!metadataCommitted && File.Exists(backupFilePath))
                {
                    try
                    {
                        RestoreLegacyDatabase
                        (
                            databaseFilePath,
                            backupFilePath,
                            sourceDatabasePassword
                        );

                        if (_metadataStore.Exists)
                        {
                            _metadataStore.Delete();
                        }
                    }
                    catch (Exception restoreException)
                    {
                        throw new InvalidDataException
                        (
                            "Database Encryption V2 migration failed, and the previous JasonQuery.db could not be restored automatically. " +
                            "Do not continue using this database until the backup has been recovered.",
                            new AggregateException(migrationException, restoreException)
                        );
                    }

                    throw new InvalidDataException
                    (
                        "Database Encryption V2 migration failed. The previous legacy JasonQuery.db was restored and V2 metadata was removed.",
                        migrationException
                    );
                }

                throw;
            }
            finally
            {
                DeleteFileIfExists(candidateDatabaseFilePath);
            }
        }

        private void RestoreLegacyDatabase(string databaseFilePath, string backupFilePath, string legacyDatabasePassword)
        {
            if (!_migrationDatabase.CanOpen(backupFilePath, legacyDatabasePassword))
            {
                throw new InvalidDataException
                (
                    "The Database Encryption V2 migration backup cannot be opened with the expected legacy database password. " +
                    "The current database was not overwritten."
                );
            }

            var restoreFilePath = CreateTemporaryDatabaseFilePath(databaseFilePath, "restore");

            try
            {
                _migrationDatabase.CreateVerifiedCopy
                (
                    backupFilePath,
                    restoreFilePath,
                    legacyDatabasePassword
                );

                if (File.Exists(databaseFilePath))
                {
                    File.Replace(restoreFilePath, databaseFilePath, null, true);
                    restoreFilePath = null;
                }
                else
                {
                    File.Move(restoreFilePath, databaseFilePath);
                    restoreFilePath = null;
                }

                if (!_migrationDatabase.CanOpen(databaseFilePath, legacyDatabasePassword))
                {
                    throw new InvalidDataException("The restored legacy JasonQuery.db could not be validated.");
                }

                File.Delete(backupFilePath);
            }
            finally
            {
                DeleteFileIfExists(restoreFilePath);
            }
        }

        private void ValidateV2Candidate(string databaseFilePath, string targetDatabasePassword, string sourceDatabasePassword)
        {
            if (!_migrationDatabase.CanOpen(databaseFilePath, targetDatabasePassword))
            {
                throw new InvalidDataException("The migrated JasonQuery.db could not be opened with the generated V2 database key.");
            }

            if (_migrationDatabase.CanOpen(databaseFilePath, sourceDatabasePassword))
            {
                throw new InvalidDataException("The migrated JasonQuery.db still accepts the historical legacy database password.");
            }
        }

        private void ValidateCommittedMetadata(DatabaseSecurityMetadata expectedMetadata)
        {
            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database Encryption V2 metadata was not finalized.");
            }

            expectedMetadata.Validate();

            var savedMetadata = _metadataStore.Load();

            savedMetadata.Validate();

            if (savedMetadata.Mode != expectedMetadata.Mode ||
                savedMetadata.MetadataVersion != expectedMetadata.MetadataVersion ||
                savedMetadata.EncryptionVersion != expectedMetadata.EncryptionVersion ||
                !string.Equals(savedMetadata.Protection, expectedMetadata.Protection, StringComparison.Ordinal) ||
                !string.Equals(savedMetadata.ProtectedDatabaseKey, expectedMetadata.ProtectedDatabaseKey, StringComparison.Ordinal) ||
                !string.Equals(savedMetadata.Kdf, expectedMetadata.Kdf, StringComparison.Ordinal) ||
                savedMetadata.Iterations != expectedMetadata.Iterations ||
                !string.Equals(savedMetadata.Salt, expectedMetadata.Salt, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Database Encryption V2 metadata validation failed after migration.");
            }
        }

        private static string CreateTemporaryDatabaseFilePath(string databaseFilePath, string purpose)
        {
            return Path.GetFullPath(databaseFilePath) + $".database-encryption-v2-{purpose}.{Guid.NewGuid():N}.tmp";
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void Clear(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }

        private static void ValidateDatabaseFilePath(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }
        }
    }
}
