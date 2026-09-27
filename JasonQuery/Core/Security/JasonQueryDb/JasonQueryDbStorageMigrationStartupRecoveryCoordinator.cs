using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    internal enum JasonQueryDbStorageMigrationStartupRecoveryOutcome
    {
        NoRecoveryRequired = 0,
        RestartedPreparation = 1,
        CompletedForwardRecovery = 2
    }

    internal sealed class JasonQueryDbStorageMigrationStartupRecoveryResult
    {
        public JasonQueryDbStorageMigrationStartupRecoveryResult(JasonQueryDbStorageMigrationStartupRecoveryOutcome outcome, string operationId)
        {
            Outcome = outcome;
            OperationId = operationId;
        }

        public JasonQueryDbStorageMigrationStartupRecoveryOutcome Outcome { get; }

        public string OperationId { get; }
    }

    /// <summary>
    /// Startup-only dispatcher for an already durable physical Storage V1 -> V2
    /// migration journal.
    ///
    /// It acquires the migration execution lock before deciding whether normal
    /// startup may proceed. PreparingCandidate is safely reset to the V1 source
    /// state. CandidateReady is revalidated with a caller-supplied target credential.
    /// ReplacePrepared and later stages complete only in the frozen forward direction.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationStartupRecoveryCoordinator
    {
        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly JasonQueryDbStorageMigrationRecovery _recovery;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;
        private readonly JasonQueryDbStorageMigrationCandidateReadyExecutor _candidateReadyExecutor;
        private readonly JasonQueryDbStorageMigrationForwardRecoveryExecutor _forwardRecoveryExecutor;

        public JasonQueryDbStorageMigrationStartupRecoveryCoordinator(string databaseFilePath, string metadataFilePath)
            : this (databaseFilePath, metadataFilePath, ValidateCandidateReadOnly)
        {
        }

        internal JasonQueryDbStorageMigrationStartupRecoveryCoordinator(string databaseFilePath, string metadataFilePath,
                                                                        Func<string, string, byte[], JasonQueryDbStorageV2ReadOnlyValidationResult> validateCandidate)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException
                (
                    "A database file path is required.",
                    nameof(databaseFilePath)
                );
            }

            if (string.IsNullOrWhiteSpace(metadataFilePath))
            {
                throw new ArgumentException
                (
                    "A metadata file path is required.",
                    nameof(metadataFilePath)
                );
            }

            _databaseFilePath = Path.GetFullPath(databaseFilePath);
            _metadataFilePath = Path.GetFullPath(metadataFilePath);

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

            _recovery = new JasonQueryDbStorageMigrationRecovery
            (
                _databaseFilePath,
                _metadataFilePath
            );

            _journalStore = new JasonQueryDbStorageMigrationJournalStore
            (
                _databaseFilePath
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
        }

        public JasonQueryDbStorageMigrationStartupRecoveryResult RecoverIfNeeded(string modernHelperFilePath,
                                                                                 Func<JasonQueryDbStorageMigrationJournal, JasonQueryDbStorageMigrationArtifactPaths, byte[]> targetPasswordResolver)
        {
            JasonQueryDbStorageMigrationStartupRecoveryResult result;

            using (AcquireStartupExecutionLock())
            {
                if (!_journalStore.Exists)
                {
                    JasonQueryDbStorageMigrationRecovery.EnsureNoOrphanArtifacts
                    (
                        _databaseFilePath,
                        _metadataFilePath,
                        true
                    );

                    result = new JasonQueryDbStorageMigrationStartupRecoveryResult
                    (
                        JasonQueryDbStorageMigrationStartupRecoveryOutcome.NoRecoveryRequired,
                        null
                    );
                }
                else
                {
                    result = RecoverDurableJournal
                    (
                        modernHelperFilePath,
                        targetPasswordResolver
                    );
                }
            }

            var completed = _recovery.Evaluate();

            if (completed.Action != JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired || completed.Journal != null || completed.Paths != null)
            {
                throw new InvalidDataException
                (
                    "Startup migration recovery did not return to the no-journal state."
                );
            }

            return result;
        }

        private JasonQueryDbStorageMigrationStartupRecoveryResult RecoverDurableJournal(string modernHelperFilePath,
                                                                                        Func<JasonQueryDbStorageMigrationJournal, JasonQueryDbStorageMigrationArtifactPaths, byte[]> targetPasswordResolver)
        {
            var plan = _recovery.Evaluate();

            switch (plan.Action)
            {
                case JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation:
                    {
                        var operationId = plan.Journal.OperationId;

                        RestartPreparation(plan);

                        return new JasonQueryDbStorageMigrationStartupRecoveryResult
                        (
                            JasonQueryDbStorageMigrationStartupRecoveryOutcome.RestartedPreparation,
                            operationId
                        );
                    }
                case JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace:
                    {
                        var operationId = plan.Journal.OperationId;

                        PrepareCandidateReady
                        (
                            plan,
                            modernHelperFilePath,
                            targetPasswordResolver
                        );

                        var forward = _forwardRecoveryExecutor.ExecuteWhileExecutionLockHeld();

                        EnsureOperationId(operationId, forward.OperationId);

                        return new JasonQueryDbStorageMigrationStartupRecoveryResult
                        (
                            JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery,
                            operationId
                        );
                    }
                case JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase:
                case JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced:
                case JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata:
                case JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted:
                case JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration:
                    {
                        var operationId = plan.Journal.OperationId;
                        var forward = _forwardRecoveryExecutor.ExecuteWhileExecutionLockHeld();

                        EnsureOperationId(operationId, forward.OperationId);

                        return new JasonQueryDbStorageMigrationStartupRecoveryResult
                        (
                            JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery,
                            operationId
                        );
                    }
                case JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired:
                    {
                        throw new InvalidDataException
                        (
                            "A canonical migration journal exists but startup recovery classified no work."
                        );
                    }
                default:
                    {
                        throw new NotSupportedException
                        (
                            $"Startup migration recovery action '{(int)plan.Action}' is not supported."
                        );
                    }
            }
        }

        private void PrepareCandidateReady(JasonQueryDbStorageMigrationRecoveryPlan plan, string modernHelperFilePath,
                                           Func<JasonQueryDbStorageMigrationJournal, JasonQueryDbStorageMigrationArtifactPaths, byte[]> targetPasswordResolver)
        {
            if (targetPasswordResolver == null)
            {
                throw new InvalidOperationException
                (
                    "CandidateReady startup recovery requires a target database credential resolver."
                );
            }

            byte[] targetPassword = null;

            try
            {
                targetPassword = targetPasswordResolver
                (
                    plan.Journal,
                    plan.Paths
                );

                ValidateTargetPassword(targetPassword);

                var prepared = _candidateReadyExecutor.PrepareReplace
                (
                    modernHelperFilePath,
                    targetPassword
                );

                if (prepared.Journal.Stage != JasonQueryDbStorageMigrationStage.ReplacePrepared
                    || !string.Equals(prepared.Journal.OperationId, plan.Journal.OperationId, StringComparison.Ordinal))
                {
                    throw new InvalidDataException
                    (
                        "CandidateReady startup recovery did not establish the expected ReplacePrepared boundary."
                    );
                }
            }
            finally
            {
                if (targetPassword != null)
                {
                    Array.Clear(targetPassword, 0, targetPassword.Length);
                }
            }
        }

        private void RestartPreparation(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            if (plan == null || plan.Action != JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation
                || plan.Journal == null || plan.Paths == null || plan.Journal.Stage != JasonQueryDbStorageMigrationStage.PreparingCandidate)
            {
                throw new InvalidDataException
                (
                    "PreparingCandidate startup recovery does not match the frozen restart boundary."
                );
            }

            var journal = plan.Journal;
            var paths = plan.Paths;

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                journal.SourceDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                journal.SourceMetadataSha256,
                "Security metadata"
            );

            EnsureAbsent
            (
                paths.DatabaseBackupFilePath,
                "Database backup"
            );

            EnsureAbsent
            (
                paths.MetadataTemporaryFilePath,
                "Target metadata temporary file"
            );

            EnsureAbsent
            (
                paths.MetadataBackupFilePath,
                "Metadata backup"
            );

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.DatabaseFilePath,
                "Production database"
            );

            DeleteUntrustedArtifact(paths.CandidateDatabaseFilePath);

            foreach (var suffix in new[] { "-journal", "-wal", "-shm" })
            {
                DeleteUntrustedArtifact
                (
                    paths.CandidateDatabaseFilePath + suffix
                );
            }

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                journal.SourceDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                journal.SourceMetadataSha256,
                "Security metadata"
            );

            _journalStore.Delete();
        }

        private JasonQueryDbStorageMigrationExecutionLock AcquireStartupExecutionLock()
        {
            try
            {
                return JasonQueryDbStorageMigrationExecutionLock.AcquireForNewMigration
                (
                    _databaseFilePath
                );
            }
            catch (IOException)
            {
                if (!_journalStore.Exists)
                {
                    throw;
                }

                return JasonQueryDbStorageMigrationExecutionLock.Acquire
                (
                    _databaseFilePath
                );
            }
        }

        private static void ValidateTargetPassword(byte[] targetPassword)
        {
            if (targetPassword == null || targetPassword.Length <= 0 || targetPassword.Length > JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "CandidateReady startup recovery requires a bounded target database password."
                );
            }
        }

        private static void EnsureOperationId(string expectedOperationId, string actualOperationId)
        {
            if (!string.Equals(expectedOperationId, actualOperationId, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "Startup forward recovery completed a different migration operation."
                );
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
                    description + " exists at the PreparingCandidate restart boundary."
                );
            }
        }

        private static JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidateReadOnly(string modernHelperFilePath,
                                                                                               string candidateDatabaseFilePath, byte[] targetDatabasePasswordUtf8)
        {
            return new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner().Validate
            (
                modernHelperFilePath,
                candidateDatabaseFilePath,
                targetDatabasePasswordUtf8
            );
        }
    }
}
