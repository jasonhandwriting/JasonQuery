using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Coordinates one new physical Storage V1 -> V2 migration while holding the migration
    /// execution lock for the entire durable state-machine lifetime.
    ///
    /// This coordinator is intentionally not wired into startup or normal database routing.
    /// Existing journals are recovery work and are not resumed through this new-migration entry point.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationCoordinator
    {
        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly JasonQueryDbStorageMigrationOrchestrator _orchestrator;
        private readonly JasonQueryDbStorageMigrationCandidateReadyExecutor _candidateReadyExecutor;
        private readonly JasonQueryDbStorageMigrationForwardRecoveryExecutor _forwardRecoveryExecutor;
        private readonly JasonQueryDbStorageMigrationRecovery _recovery;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;
        private readonly Func<string, string, string, string, byte[], byte[], JasonQueryDbStorageMigrationProcessResult> _createCandidate;

        public JasonQueryDbStorageMigrationCoordinator(string databaseFilePath, string metadataFilePath)
            : this (databaseFilePath, metadataFilePath, CreateCandidateWithProcessRunner, ValidateCandidateReadOnly)
        {
        }

        internal JasonQueryDbStorageMigrationCoordinator(string databaseFilePath, string metadataFilePath,
                                                         Func<string, string, string, string, byte[], byte[], JasonQueryDbStorageMigrationProcessResult> createCandidate,
                                                         Func<string, string, byte[], JasonQueryDbStorageV2ReadOnlyValidationResult> validateCandidate)
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
            _createCandidate = createCandidate ?? throw new ArgumentNullException(nameof(createCandidate));

            if (validateCandidate == null)
            {
                throw new ArgumentNullException(nameof(validateCandidate));
            }

            JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                _metadataFilePath,
                Guid.NewGuid().ToString("N")
            );

            _orchestrator = new JasonQueryDbStorageMigrationOrchestrator
            (
                _databaseFilePath,
                _metadataFilePath
            );

            _candidateReadyExecutor = new JasonQueryDbStorageMigrationCandidateReadyExecutor
            (
                _databaseFilePath,
                _metadataFilePath,
                validateCandidate
            );

            _forwardRecoveryExecutor = new JasonQueryDbStorageMigrationForwardRecoveryExecutor
            (
                _databaseFilePath,
                _metadataFilePath
            );

            _recovery = new JasonQueryDbStorageMigrationRecovery
            (
                _databaseFilePath,
                _metadataFilePath
            );

            _journalStore = new JasonQueryDbStorageMigrationJournalStore
            (
                _databaseFilePath
            );
        }

        public JasonQueryDbStorageMigrationCoordinatorResult Migrate(string legacyHelperFilePath, string modernHelperFilePath,
                                                                     byte[] sourceDatabasePasswordUtf8, byte[] targetDatabasePasswordUtf8)
        {
            var sourcePassword = CloneSecret
            (
                sourceDatabasePasswordUtf8,
                JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePasswordUtf8Bytes,
                nameof(sourceDatabasePasswordUtf8)
            );

            byte[] targetPassword = null;

            try
            {
                targetPassword = CloneSecret
                (
                    targetDatabasePasswordUtf8,
                    JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes,
                    nameof(targetDatabasePasswordUtf8)
                );

                JasonQueryDbStorageMigrationCoordinatorResult result;
                Exception migrationFailure = null;

                JasonQueryDbStorageMigrationExecutionLock migrationLock = null;

                try
                {
                    migrationLock = JasonQueryDbStorageMigrationExecutionLock.AcquireForNewMigration
                    (
                        _databaseFilePath
                    );

                    try
                    {
                        result = MigrateWhileExecutionLockHeld
                        (
                            legacyHelperFilePath,
                            modernHelperFilePath,
                            sourcePassword,
                            targetPassword
                        );
                    }
                    catch (Exception ex)
                    {
                        migrationFailure = ex;

                        var cleanupFailure = TryCleanupUntrustedPreparingCandidateArtifacts();

                        if (cleanupFailure != null)
                        {
                            migrationFailure = new AggregateException
                            (
                                "Database storage migration failed and PreparingCandidate cleanup also failed.",
                                ex,
                                cleanupFailure
                            );

                            throw migrationFailure;
                        }

                        throw;
                    }
                }
                finally
                {
                    if (migrationLock != null)
                    {
                        try
                        {
                            migrationLock.Dispose();
                        }
                        catch (Exception lockDisposeFailure)
                        {
                            if (migrationFailure != null)
                            {
                                throw new AggregateException
                                (
                                    "Database storage migration failed and the execution lock could not be released cleanly.",
                                    migrationFailure,
                                    lockDisposeFailure
                                );
                            }

                            throw;
                        }
                    }
                }

                var completed = _recovery.Evaluate();

                if (completed.Action != JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired || completed.Journal != null || completed.Paths != null)
                {
                    throw new InvalidDataException
                    (
                        "The completed database storage migration did not return to the no-journal state."
                    );
                }

                return result;
            }
            finally
            {
                Array.Clear(sourcePassword, 0, sourcePassword.Length);

                if (targetPassword != null)
                {
                    Array.Clear(targetPassword, 0, targetPassword.Length);
                }
            }
        }

        private JasonQueryDbStorageMigrationCoordinatorResult MigrateWhileExecutionLockHeld(string legacyHelperFilePath, string modernHelperFilePath,
                                                                                            byte[] sourceDatabasePasswordUtf8, byte[] targetDatabasePasswordUtf8)
        {
            var preparation = _orchestrator.PrepareWhileExecutionLockHeld();

            var processResult = _createCandidate
            (
                legacyHelperFilePath,
                modernHelperFilePath,
                preparation.Paths.DatabaseFilePath,
                preparation.Paths.CandidateDatabaseFilePath,
                sourceDatabasePasswordUtf8,
                targetDatabasePasswordUtf8
            );

            if (processResult == null)
            {
                throw new InvalidDataException
                (
                    "The database storage-migration process runner did not return a completion result."
                );
            }

            var candidateReady = _orchestrator.CommitCandidateReady(preparation);

            if (candidateReady.Stage != JasonQueryDbStorageMigrationStage.CandidateReady || string.IsNullOrWhiteSpace(candidateReady.CandidateDatabaseSha256))
            {
                throw new InvalidDataException
                (
                    "The durable CandidateReady boundary was not established."
                );
            }

            var candidateReadyPlan = _recovery.Evaluate();

            if (candidateReadyPlan.Action != JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace)
            {
                throw new InvalidDataException
                (
                    "CandidateReady did not produce the frozen pre-replacement recovery action."
                );
            }

            var replacePreparation = _candidateReadyExecutor.PrepareReplace
            (
                modernHelperFilePath,
                targetDatabasePasswordUtf8
            );

            if (replacePreparation.Journal.Stage != JasonQueryDbStorageMigrationStage.ReplacePrepared
                || !string.Equals(replacePreparation.Journal.OperationId, preparation.Journal.OperationId, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "The durable ReplacePrepared boundary does not match the migration operation."
                );
            }

            var forwardResult = _forwardRecoveryExecutor.ExecuteWhileExecutionLockHeld();

            if (!string.Equals(forwardResult.OperationId, preparation.Journal.OperationId, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "Forward recovery completed a different migration operation."
                );
            }

            EnsureCompletedWhileExecutionLockHeld
            (
                preparation.Paths,
                candidateReady
            );

            return new JasonQueryDbStorageMigrationCoordinatorResult
            (
                preparation.Journal.OperationId,
                processResult.LogicalBytesForwarded,
                candidateReady.CandidateDatabaseSha256,
                candidateReady.TargetMetadataSha256
            );
        }

        private void EnsureCompletedWhileExecutionLockHeld(JasonQueryDbStorageMigrationArtifactPaths paths, JasonQueryDbStorageMigrationJournal candidateReady)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                candidateReady.CandidateDatabaseSha256,
                "Completed production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                candidateReady.TargetMetadataSha256,
                "Completed security metadata"
            );

            var metadataStore = new JasonQueryDbSecurityMetadataStore
            (
                paths.MetadataFilePath
            );

            var committedMetadata = metadataStore.Load();

            if (!JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(candidateReady.TargetMetadata, committedMetadata))
            {
                throw new InvalidDataException
                (
                    "Completed security metadata does not match the durable target metadata identity."
                );
            }

            EnsureAbsent(paths.JournalFilePath, "Canonical migration journal");
            EnsureAbsent(paths.JournalTemporaryFilePath, "Migration journal temporary file");
            EnsureAbsent(paths.CandidateDatabaseFilePath, "Storage V2 candidate");
            EnsureAbsent(paths.DatabaseBackupFilePath, "Database backup");
            EnsureAbsent(paths.MetadataTemporaryFilePath, "Target metadata temporary file");
            EnsureAbsent(paths.MetadataBackupFilePath, "Metadata backup");

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.DatabaseFilePath,
                "Completed production database"
            );

            if (!File.Exists(paths.LockFilePath) || Directory.Exists(paths.LockFilePath))
            {
                throw new InvalidDataException
                (
                    "The migration execution lock disappeared before full migration completion."
                );
            }
        }

        private Exception TryCleanupUntrustedPreparingCandidateArtifacts()
        {
            try
            {
                if (!_journalStore.Exists)
                {
                    return null;
                }

                var journal = _journalStore.Load();

                if (journal.Stage != JasonQueryDbStorageMigrationStage.PreparingCandidate)
                {
                    return null;
                }

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    _databaseFilePath,
                    _metadataFilePath,
                    journal.OperationId
                );

                DeleteUntrustedArtifact(paths.CandidateDatabaseFilePath);

                foreach (var suffix in new[] { "-journal", "-wal", "-shm" })
                {
                    DeleteUntrustedArtifact(paths.CandidateDatabaseFilePath + suffix);
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private static void DeleteUntrustedArtifact(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    "An untrusted PreparingCandidate artifact is not a regular file."
                );
            }

            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);

            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new IOException
                (
                    "An untrusted PreparingCandidate artifact could not be deleted."
                );
            }
        }

        private static void EnsureAbsent(string filePath, string description)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " remains after full migration completion."
                );
            }
        }

        private static byte[] CloneSecret(byte[] value, int maximumLength, string parameterName)
        {
            if (value == null || value.Length <= 0 || value.Length > maximumLength)
            {
                throw new ArgumentException
                (
                    "A bounded database password is required.",
                    parameterName
                );
            }

            var clone = new byte[value.Length];

            Buffer.BlockCopy
            (
                value,
                0,
                clone,
                0,
                value.Length
            );

            return clone;
        }

        private static JasonQueryDbStorageMigrationProcessResult CreateCandidateWithProcessRunner(string legacyHelperFilePath, string modernHelperFilePath,
                                                                                                  string sourceDatabaseFilePath, string candidateDatabaseFilePath,
                                                                                                  byte[] sourceDatabasePasswordUtf8, byte[] targetDatabasePasswordUtf8)
        {
            return new JasonQueryDbStorageMigrationProcessRunner().CreateCandidate
            (
                legacyHelperFilePath,
                modernHelperFilePath,
                sourceDatabaseFilePath,
                candidateDatabaseFilePath,
                sourceDatabasePasswordUtf8,
                targetDatabasePasswordUtf8
            );
        }

        private static JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidateReadOnly(string modernHelperFilePath, string candidateDatabaseFilePath,
                                                                                               byte[] targetDatabasePasswordUtf8)
        {
            return new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner().Validate
            (
                modernHelperFilePath,
                candidateDatabaseFilePath,
                targetDatabasePasswordUtf8
            );
        }
    }

    internal sealed class JasonQueryDbStorageMigrationCoordinatorResult
    {
        public JasonQueryDbStorageMigrationCoordinatorResult(string operationId, long logicalBytesForwarded, string targetDatabaseSha256,
                                                             string targetMetadataSha256)
        {
            if (string.IsNullOrWhiteSpace(operationId))
            {
                throw new ArgumentException
                (
                    "A migration operation id is required.",
                    nameof(operationId)
                );
            }

            if (logicalBytesForwarded < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalBytesForwarded));
            }

            OperationId = operationId;
            LogicalBytesForwarded = logicalBytesForwarded;
            TargetDatabaseSha256 = targetDatabaseSha256 ?? throw new ArgumentNullException(nameof(targetDatabaseSha256));
            TargetMetadataSha256 = targetMetadataSha256 ?? throw new ArgumentNullException(nameof(targetMetadataSha256));
        }

        public string OperationId { get; }

        public long LogicalBytesForwarded { get; }

        public string TargetDatabaseSha256 { get; }

        public string TargetMetadataSha256 { get; }
    }
}
