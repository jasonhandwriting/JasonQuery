using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityTransitionManager
    {
        private const string CandidateFileSuffix = ".database-security-change-candidate";
        private const string BackupFileSuffix = ".database-security-change-backup";
        private const string RollbackScratchFileSuffix = ".database-security-change-rollback-scratch";
        private const string JournalFileName = "JasonQuery.security-transition.json";

        private readonly IDatabaseSecurityMetadataStore _metadataStore;
        private readonly IDatabaseKeyProtector _databaseKeyProtector;
        private readonly IDatabaseSecurityMigrationDatabase _database;
        private readonly DatabaseSecurityTransitionJournalStore _journalStore;

        public DatabaseSecurityTransitionManager(IDatabaseSecurityMetadataStore metadataStore, IDatabaseKeyProtector databaseKeyProtector,
                                                 IDatabaseSecurityMigrationDatabase database, DatabaseSecurityTransitionJournalStore journalStore)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _journalStore = journalStore ?? throw new ArgumentNullException(nameof(journalStore));
        }

        public static string GetCandidateFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.GetFullPath(databaseFilePath) + CandidateFileSuffix;
        }

        public static string GetBackupFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.GetFullPath(databaseFilePath) + BackupFileSuffix;
        }

        public static string GetRollbackScratchFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.GetFullPath(databaseFilePath) + RollbackScratchFileSuffix;
        }

        public static string GetJournalFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(databaseFilePath)), JournalFileName);
        }

        public DatabaseSecurityBootstrapResult ChangeToWindowsCurrentUser(string databaseFilePath, string currentDatabasePassword)
        {
            ValidateCurrentDatabase(databaseFilePath, currentDatabasePassword);

            byte[] targetDatabaseKey = null;
            byte[] protectedTargetDatabaseKey = null;

            try
            {
                targetDatabaseKey = DatabaseKeyGenerator.Generate();

                var targetDatabasePassword = DatabaseKeyGenerator.ToDatabasePassword(targetDatabaseKey);

                protectedTargetDatabaseKey = _databaseKeyProtector.Protect(targetDatabaseKey);

                var targetMetadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(protectedTargetDatabaseKey)
                );

                return ExecuteChange(databaseFilePath, currentDatabasePassword, targetDatabasePassword, targetMetadata);
            }
            finally
            {
                Clear(targetDatabaseKey);
                Clear(protectedTargetDatabaseKey);
            }
        }

        public DatabaseSecurityBootstrapResult ChangeToCustomPassword(string databaseFilePath, string currentDatabasePassword, string newCustomPassword)
        {
            ValidateCurrentDatabase(databaseFilePath, currentDatabasePassword);

            if (string.IsNullOrEmpty(newCustomPassword))
            {
                throw new ArgumentException("A new custom password is required.", nameof(newCustomPassword));
            }

            byte[] salt = null;

            try
            {
                salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();

                var iterations = DatabaseSecurityConstants.DefaultPbkdf2Iterations;

                var targetDatabasePassword = CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword
                (
                    newCustomPassword,
                    salt,
                    iterations
                );

                var targetMetadata = DatabaseSecurityMetadata.CreateCustomPassword(salt, iterations);

                return ExecuteChange(databaseFilePath, currentDatabasePassword, targetDatabasePassword, targetMetadata);
            }
            finally
            {
                Clear(salt);
            }
        }

        public DatabaseSecurityBootstrapResult RecoverInterruptedChangeIfNeeded(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            var candidateFilePath = GetCandidateFilePath(databaseFilePath);
            var backupFilePath = GetBackupFilePath(databaseFilePath);
            var rollbackScratchFilePath = GetRollbackScratchFilePath(databaseFilePath);

            _journalStore.DeleteTemporaryFileIfExists();

            if (!_journalStore.Exists)
            {
                DeleteFileIfExists(candidateFilePath);
                DeleteFileIfExists(rollbackScratchFilePath);

                if (File.Exists(backupFilePath))
                {
                    throw new InvalidDataException
                    (
                        "A database-security transition backup exists without its recovery journal. " +
                        "JasonQuery cannot determine the correct database key safely."
                    );
                }

                return null;
            }

            var journal = _journalStore.Load();
            string sourceDatabasePassword = null;
            string targetDatabasePassword = null;

            try
            {
                sourceDatabasePassword = UnprotectDatabasePassword(journal.ProtectedSourceDatabasePassword);
                targetDatabasePassword = UnprotectDatabasePassword(journal.ProtectedTargetDatabasePassword);

                var currentMetadata = _metadataStore.Exists ? _metadataStore.Load() : null;
                var productionOpensTarget = File.Exists(databaseFilePath) && _database.CanOpen(databaseFilePath, targetDatabasePassword);
                var productionOpensSource = File.Exists(databaseFilePath) && _database.CanOpen(databaseFilePath, sourceDatabasePassword);

                if (productionOpensTarget)
                {
                    if (MetadataEquals(currentMetadata, journal.SourceMetadata))
                    {
                        _metadataStore.Save(journal.TargetMetadata);
                        ValidateCommittedMetadata(journal.TargetMetadata);
                    }
                    else if (!MetadataEquals(currentMetadata, journal.TargetMetadata))
                    {
                        throw CreateAmbiguousRecoveryException();
                    }

                    CleanupCompletedTransition(candidateFilePath, backupFilePath, rollbackScratchFilePath);

                    return DatabaseSecurityBootstrapResult.V2Ready(journal.TargetMetadata, targetDatabasePassword);
                }

                if (productionOpensSource)
                {
                    if (MetadataEquals(currentMetadata, journal.TargetMetadata))
                    {
                        _metadataStore.Save(journal.SourceMetadata);
                        ValidateCommittedMetadata(journal.SourceMetadata);
                    }
                    else if (!MetadataEquals(currentMetadata, journal.SourceMetadata))
                    {
                        throw CreateAmbiguousRecoveryException();
                    }

                    CleanupCompletedTransition(candidateFilePath, backupFilePath, rollbackScratchFilePath);

                    return DatabaseSecurityBootstrapResult.V2Ready(journal.SourceMetadata, sourceDatabasePassword);
                }

                if (File.Exists(backupFilePath) && _database.CanOpen(backupFilePath, sourceDatabasePassword))
                {
                    RestoreSourceDatabase(databaseFilePath, backupFilePath, rollbackScratchFilePath);
                    _metadataStore.Save(journal.SourceMetadata);
                    ValidateCommittedMetadata(journal.SourceMetadata);

                    if (!_database.CanOpen(databaseFilePath, sourceDatabasePassword))
                    {
                        throw new InvalidDataException("The previous JasonQuery.db could not be validated after transition recovery.");
                    }

                    CleanupCompletedTransition(candidateFilePath, backupFilePath, rollbackScratchFilePath);

                    return DatabaseSecurityBootstrapResult.V2Ready(journal.SourceMetadata, sourceDatabasePassword);
                }

                throw CreateAmbiguousRecoveryException();
            }
            finally
            {
                sourceDatabasePassword = null;
                targetDatabasePassword = null;
            }
        }

        private DatabaseSecurityBootstrapResult ExecuteChange(string databaseFilePath, string currentDatabasePassword,
                                                              string targetDatabasePassword, DatabaseSecurityMetadata targetMetadata)
        {
            EnsureReadyForNewTransition(databaseFilePath);

            var sourceMetadata = _metadataStore.Load();

            sourceMetadata.Validate();
            targetMetadata.Validate();

            var candidateFilePath = GetCandidateFilePath(databaseFilePath);
            var backupFilePath = GetBackupFilePath(databaseFilePath);
            var rollbackScratchFilePath = GetRollbackScratchFilePath(databaseFilePath);

            try
            {
                _database.CreateVerifiedCopy(databaseFilePath, candidateFilePath, currentDatabasePassword);
                _database.ChangePassword(candidateFilePath, currentDatabasePassword, targetDatabasePassword);

                if (!_database.CanOpen(candidateFilePath, targetDatabasePassword))
                {
                    throw new InvalidDataException("The database-security transition candidate could not be opened with its new key.");
                }

                if (_database.CanOpen(candidateFilePath, currentDatabasePassword))
                {
                    throw new InvalidDataException("The database-security transition candidate still accepts its previous key.");
                }

                SaveRecoveryJournal(sourceMetadata, targetMetadata, currentDatabasePassword, targetDatabasePassword);

                File.Replace(candidateFilePath, databaseFilePath, backupFilePath, true);

                if (!_database.CanOpen(databaseFilePath, targetDatabasePassword))
                {
                    throw new InvalidDataException("The updated JasonQuery.db could not be opened with its new key.");
                }

                _metadataStore.Save(targetMetadata);
                ValidateCommittedMetadata(targetMetadata);

                CleanupCompletedTransition(candidateFilePath, backupFilePath, rollbackScratchFilePath);

                return DatabaseSecurityBootstrapResult.V2Ready(targetMetadata, targetDatabasePassword);
            }
            catch (Exception changeException)
            {
                if (_journalStore.Exists)
                {
                    try
                    {
                        var recoveredResult = RecoverInterruptedChangeIfNeeded(databaseFilePath);

                        if (recoveredResult != null && MetadataEquals(recoveredResult.Metadata, targetMetadata)
                            && string.Equals(recoveredResult.DatabasePassword, targetDatabasePassword, StringComparison.Ordinal))
                        {
                            return recoveredResult;
                        }
                    }
                    catch (Exception recoveryException)
                    {
                        throw new InvalidDataException
                        (
                            "Changing JasonQuery.db security failed, and automatic transition recovery also failed.",
                            new AggregateException(changeException, recoveryException)
                        );
                    }

                    throw new InvalidDataException
                    (
                        "Changing JasonQuery.db security failed. The previous database security settings were restored.",
                        changeException
                    );
                }

                DeleteFileIfExists(candidateFilePath);
                DeleteFileIfExists(rollbackScratchFilePath);

                if (File.Exists(backupFilePath))
                {
                    throw new InvalidDataException
                    (
                        "Changing JasonQuery.db security failed and left an unexpected backup without a recovery journal.",
                        changeException
                    );
                }

                throw new InvalidDataException("Changing JasonQuery.db security failed before the database was replaced.", changeException);
            }
        }

        private void EnsureReadyForNewTransition(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                throw new FileNotFoundException("JasonQuery.db was not found.", databaseFilePath);
            }

            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database security metadata is required before changing database security.");
            }

            if (_journalStore.Exists || File.Exists(GetBackupFilePath(databaseFilePath)))
            {
                throw new InvalidDataException("A previous database-security transition must be recovered before starting another change.");
            }

            DeleteFileIfExists(GetCandidateFilePath(databaseFilePath));
            DeleteFileIfExists(GetRollbackScratchFilePath(databaseFilePath));
            _journalStore.DeleteTemporaryFileIfExists();
        }

        private void ValidateCurrentDatabase(string databaseFilePath, string currentDatabasePassword)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (string.IsNullOrWhiteSpace(currentDatabasePassword))
            {
                throw new ArgumentException("The current database password is required.", nameof(currentDatabasePassword));
            }

            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database security metadata is missing.");
            }

            var metadata = _metadataStore.Load();

            metadata.Validate();

            if (!_database.CanOpen(databaseFilePath, currentDatabasePassword))
            {
                throw new InvalidDataException("JasonQuery.db cannot be opened with the current resolved database key.");
            }
        }

        private void SaveRecoveryJournal(DatabaseSecurityMetadata sourceMetadata, DatabaseSecurityMetadata targetMetadata,
                                         string sourceDatabasePassword, string targetDatabasePassword)
        {
            byte[] protectedSource = null;
            byte[] protectedTarget = null;

            try
            {
                protectedSource = ProtectDatabasePassword(sourceDatabasePassword);
                protectedTarget = ProtectDatabasePassword(targetDatabasePassword);

                _journalStore.Save
                (
                    new DatabaseSecurityTransitionJournal
                    {
                        TransitionVersion = DatabaseSecurityTransitionJournal.CurrentVersion,
                        SourceMetadata = sourceMetadata,
                        TargetMetadata = targetMetadata,
                        ProtectedSourceDatabasePassword = Convert.ToBase64String(protectedSource),
                        ProtectedTargetDatabasePassword = Convert.ToBase64String(protectedTarget)
                    }
                );
            }
            finally
            {
                Clear(protectedSource);
                Clear(protectedTarget);
            }
        }

        private byte[] ProtectDatabasePassword(string databasePassword)
        {
            var bytes = Encoding.UTF8.GetBytes(databasePassword);

            try
            {
                return _databaseKeyProtector.Protect(bytes);
            }
            finally
            {
                Clear(bytes);
            }
        }

        private string UnprotectDatabasePassword(string protectedValue)
        {
            byte[] protectedBytes = null;
            byte[] plainBytes = null;

            try
            {
                protectedBytes = Convert.FromBase64String(protectedValue);
                plainBytes = _databaseKeyProtector.Unprotect(protectedBytes);

                return Encoding.UTF8.GetString(plainBytes);
            }
            finally
            {
                Clear(protectedBytes);
                Clear(plainBytes);
            }
        }

        private void ValidateCommittedMetadata(DatabaseSecurityMetadata expectedMetadata)
        {
            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database security metadata disappeared during a security transition.");
            }

            var committedMetadata = _metadataStore.Load();

            if (!MetadataEquals(committedMetadata, expectedMetadata))
            {
                throw new InvalidDataException("Database security metadata validation failed after a security transition.");
            }
        }

        private void CleanupCompletedTransition(string candidateFilePath, string backupFilePath, string rollbackScratchFilePath)
        {
            DeleteFileIfExists(candidateFilePath);
            DeleteFileIfExists(rollbackScratchFilePath);
            DeleteFileIfExists(backupFilePath);
            _journalStore.Delete();
        }

        private static void RestoreSourceDatabase(string databaseFilePath, string backupFilePath, string rollbackScratchFilePath)
        {
            DeleteFileIfExists(rollbackScratchFilePath);

            if (File.Exists(databaseFilePath))
            {
                File.Replace(backupFilePath, databaseFilePath, rollbackScratchFilePath, true);
                DeleteFileIfExists(rollbackScratchFilePath);
            }
            else
            {
                File.Move(backupFilePath, databaseFilePath);
            }
        }

        private static bool MetadataEquals(DatabaseSecurityMetadata left, DatabaseSecurityMetadata right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return left.MetadataVersion == right.MetadataVersion
                   && left.EncryptionVersion == right.EncryptionVersion
                   && left.Mode == right.Mode
                   && string.Equals(left.Protection, right.Protection, StringComparison.Ordinal)
                   && string.Equals(left.ProtectedDatabaseKey, right.ProtectedDatabaseKey, StringComparison.Ordinal)
                   && string.Equals(left.Kdf, right.Kdf, StringComparison.Ordinal)
                   && left.Iterations == right.Iterations
                   && string.Equals(left.Salt, right.Salt, StringComparison.Ordinal);
        }

        private static InvalidDataException CreateAmbiguousRecoveryException()
        {
            return new InvalidDataException
            (
                "JasonQuery found an incomplete database-security transition, but the database and metadata states do not form a safe recoverable pair."
            );
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
