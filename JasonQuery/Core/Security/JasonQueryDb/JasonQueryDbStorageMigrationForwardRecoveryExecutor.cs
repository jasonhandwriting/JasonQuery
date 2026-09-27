using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Executes only the forward-completion portion of a durable Storage V1 -> V2 migration.
    ///
    /// Supported entry states:
    /// ReplacePrepared, DatabaseReplaced, and MetadataCommitted.
    ///
    /// The executor never rolls back after database replacement. Every action is selected by the
    /// read-only recovery evaluator, and every destructive cleanup step occurs only after the
    /// MetadataCommitted boundary. The canonical migration journal is deleted last.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationForwardRecoveryExecutor
    {
        private const int MaximumRecoveryTransitions = 8;

        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly JasonQueryDbStorageMigrationRecovery _recovery;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;

        public JasonQueryDbStorageMigrationForwardRecoveryExecutor(string databaseFilePath, string metadataFilePath)
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
        }

        public JasonQueryDbStorageMigrationForwardRecoveryResult Execute()
        {
            if (!_journalStore.Exists)
            {
                var noJournalPlan = _recovery.Evaluate();

                if (noJournalPlan.Action != JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired)
                {
                    throw new InvalidDataException
                    (
                        "A migration journal appeared while resolving the no-recovery state."
                    );
                }

                return new JasonQueryDbStorageMigrationForwardRecoveryResult(null);
            }

            JasonQueryDbStorageMigrationForwardRecoveryResult result;

            using (JasonQueryDbStorageMigrationExecutionLock.Acquire(_databaseFilePath))
            {
                result = ExecuteWhileExecutionLockHeld();
            }

            var completed = _recovery.Evaluate();

            if (completed.Action != JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired || completed.Journal != null || completed.Paths != null)
            {
                throw new InvalidDataException
                (
                    "Database storage-migration forward recovery did not return to the no-journal state."
                );
            }

            return result;
        }

        internal JasonQueryDbStorageMigrationForwardRecoveryResult ExecuteWhileExecutionLockHeld()
        {
            var lockFilePath = JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath
            (
                _databaseFilePath
            );

            if (!File.Exists(lockFilePath) || Directory.Exists(lockFilePath))
            {
                throw new InvalidDataException
                (
                    "The database storage-migration execution lock is not present."
                );
            }

            return ExecuteLocked();
        }

        private JasonQueryDbStorageMigrationForwardRecoveryResult ExecuteLocked()
        {
            string operationId = null;

            for (var transition = 0; transition < MaximumRecoveryTransitions; transition++)
            {
                var plan = _recovery.Evaluate();

                if (plan.Journal != null)
                {
                    if (operationId == null)
                    {
                        operationId = plan.Journal.OperationId;
                    }
                    else if (!string.Equals(operationId, plan.Journal.OperationId, StringComparison.Ordinal))
                    {
                        throw new InvalidDataException
                        (
                            "Database storage-migration operation identity changed during forward recovery."
                        );
                    }
                }

                switch (plan.Action)
                {
                    case JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase:
                        {
                            ReplaceDatabase(plan);
                            break;
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced:
                        {
                            RecognizeDatabaseReplaced(plan);
                            break;
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata:
                        {
                            CommitMetadata(plan);
                            break;
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted:
                        {
                            RecognizeMetadataCommitted(plan);
                            break;
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration:
                        {
                            CleanupCommittedMigration(plan);
                            return new JasonQueryDbStorageMigrationForwardRecoveryResult(operationId);
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired:
                        {
                            throw new InvalidDataException
                            (
                                "The canonical migration journal disappeared while the execution lock was held."
                            );
                        }
                    case JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation:
                    case JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace:
                        {
                            throw new InvalidOperationException
                            (
                                "Forward recovery requires ReplacePrepared or a later durable migration stage."
                            );
                        }
                    default:
                        {
                            throw new NotSupportedException
                            (
                                $"Database storage-migration recovery action '{(int)plan.Action}' is not supported."
                            );
                        }
                }
            }

            throw new InvalidDataException
            (
                "Database storage-migration forward recovery exceeded the maximum transition count."
            );
        }

        private void ReplaceDatabase(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            EnsurePlan
            (
                plan,
                JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase,
                JasonQueryDbStorageMigrationStage.ReplacePrepared
            );

            var journal = plan.Journal;
            var paths = plan.Paths;

            EnsureReplacePreparedSourceState(journal, paths);

            File.Replace
            (
                paths.CandidateDatabaseFilePath,
                paths.DatabaseFilePath,
                paths.DatabaseBackupFilePath
            );

            var afterReplace = _recovery.Evaluate();

            EnsureSameOperation(journal, afterReplace.Journal);

            if (afterReplace.Action != JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced)
            {
                throw new InvalidDataException
                (
                    "Database replacement did not produce the frozen ReplacePrepared post-replace state."
                );
            }

            AdvanceStage
            (
                afterReplace.Journal,
                JasonQueryDbStorageMigrationStage.DatabaseReplaced
            );

            var committed = _recovery.Evaluate();

            EnsureSameOperation(journal, committed.Journal);

            if (committed.Action != JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata)
            {
                throw new InvalidDataException
                (
                    "DatabaseReplaced did not produce the expected metadata-commit recovery state."
                );
            }
        }

        private void RecognizeDatabaseReplaced(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            EnsurePlan
            (
                plan,
                JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced,
                JasonQueryDbStorageMigrationStage.ReplacePrepared
            );

            var journal = plan.Journal;

            AdvanceStage
            (
                journal,
                JasonQueryDbStorageMigrationStage.DatabaseReplaced
            );

            var committed = _recovery.Evaluate();

            EnsureSameOperation(journal, committed.Journal);

            if (committed.Action != JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata)
            {
                throw new InvalidDataException
                (
                    "Recognized database replacement did not produce the expected metadata-commit state."
                );
            }
        }

        private void CommitMetadata(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            EnsurePlan
            (
                plan,
                JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata,
                JasonQueryDbStorageMigrationStage.DatabaseReplaced
            );

            var journal = plan.Journal;
            var paths = plan.Paths;

            EnsureMetadataCommitSourceState(journal, paths);

            File.Replace
            (
                paths.MetadataTemporaryFilePath,
                paths.MetadataFilePath,
                paths.MetadataBackupFilePath
            );

            EnsureCommittedMetadataIdentity(journal, paths);

            var afterReplace = _recovery.Evaluate();

            EnsureSameOperation(journal, afterReplace.Journal);

            if (afterReplace.Action != JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted)
            {
                throw new InvalidDataException
                (
                    "Security-metadata replacement did not produce the frozen post-commit state."
                );
            }

            AdvanceStage
            (
                afterReplace.Journal,
                JasonQueryDbStorageMigrationStage.MetadataCommitted
            );

            var committed = _recovery.Evaluate();

            EnsureSameOperation(journal, committed.Journal);

            if (committed.Action != JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration)
            {
                throw new InvalidDataException
                (
                    "MetadataCommitted did not produce the expected cleanup state."
                );
            }
        }

        private void RecognizeMetadataCommitted(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            EnsurePlan
            (
                plan,
                JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted,
                JasonQueryDbStorageMigrationStage.DatabaseReplaced
            );

            var journal = plan.Journal;
            var paths = plan.Paths;

            EnsureCommittedMetadataIdentity(journal, paths);

            AdvanceStage
            (
                journal,
                JasonQueryDbStorageMigrationStage.MetadataCommitted
            );

            var committed = _recovery.Evaluate();

            EnsureSameOperation(journal, committed.Journal);

            if (committed.Action != JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration)
            {
                throw new InvalidDataException
                (
                    "Recognized metadata commit did not produce the expected cleanup state."
                );
            }
        }

        private void CleanupCommittedMigration(JasonQueryDbStorageMigrationRecoveryPlan plan)
        {
            EnsurePlan
            (
                plan,
                JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration,
                JasonQueryDbStorageMigrationStage.MetadataCommitted
            );

            var journal = plan.Journal;
            var paths = plan.Paths;

            EnsureCommittedMetadataIdentity(journal, paths);

            EnsureAbsent
            (
                paths.CandidateDatabaseFilePath,
                "Storage V2 candidate"
            );

            EnsureAbsent
            (
                paths.MetadataTemporaryFilePath,
                "Target metadata temporary file"
            );

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.DatabaseFilePath,
                "Production database"
            );

            EnsureVerifiedOptionalFile
            (
                paths.DatabaseBackupFilePath,
                journal.SourceDatabaseSha256,
                "Database backup"
            );

            EnsureVerifiedOptionalFile
            (
                paths.MetadataBackupFilePath,
                journal.SourceMetadataSha256,
                "Metadata backup"
            );

            DeleteOptionalFile(paths.DatabaseBackupFilePath);
            DeleteOptionalFile(paths.MetadataBackupFilePath);

            _journalStore.Delete();
        }

        private static void EnsureReplacePreparedSourceState(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                journal.SourceDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.CandidateDatabaseFilePath,
                journal.CandidateDatabaseSha256,
                "Storage V2 candidate"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                journal.SourceMetadataSha256,
                "Security metadata"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataTemporaryFilePath,
                journal.TargetMetadataSha256,
                "Target metadata temporary file"
            );

            EnsureAbsent
            (
                paths.DatabaseBackupFilePath,
                "Database backup"
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

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.CandidateDatabaseFilePath,
                "Storage V2 candidate"
            );
        }

        private static void EnsureMetadataCommitSourceState(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                journal.CandidateDatabaseSha256,
                "Replaced production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseBackupFilePath,
                journal.SourceDatabaseSha256,
                "Database backup"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                journal.SourceMetadataSha256,
                "Security metadata"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataTemporaryFilePath,
                journal.TargetMetadataSha256,
                "Target metadata temporary file"
            );

            EnsureAbsent
            (
                paths.CandidateDatabaseFilePath,
                "Storage V2 candidate"
            );

            EnsureAbsent
            (
                paths.MetadataBackupFilePath,
                "Metadata backup"
            );

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.DatabaseFilePath,
                "Replaced production database"
            );
        }

        private static void EnsureCommittedMetadataIdentity(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                journal.CandidateDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                journal.TargetMetadataSha256,
                "Security metadata"
            );

            var metadataStore = new JasonQueryDbSecurityMetadataStore
            (
                paths.MetadataFilePath
            );

            var committedMetadata = metadataStore.Load();

            if (!JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(journal.TargetMetadata, committedMetadata))
            {
                throw new InvalidDataException
                (
                    "Committed security metadata does not match the durable target metadata snapshot."
                );
            }

            var expectedTargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
            (
                journal.SourceMetadata
            );

            if (!JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(expectedTargetMetadata, committedMetadata))
            {
                throw new InvalidDataException
                (
                    "Committed security metadata does not preserve the source security identity."
                );
            }

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                paths.DatabaseFilePath,
                "Production database"
            );
        }

        private void AdvanceStage(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationStage targetStage)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            journal.Stage = targetStage;
            _journalStore.Save(journal);

            var committed = _journalStore.Load();

            if (committed.Stage != targetStage)
            {
                throw new InvalidDataException
                (
                    "Database storage-migration journal did not persist the expected forward-recovery stage."
                );
            }

            EnsureSameOperation(journal, committed);
        }

        private static void EnsureVerifiedOptionalFile(string filePath, string expectedSha256, string description)
        {
            if (Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " is not a regular file."
                );
            }

            if (!File.Exists(filePath))
            {
                return;
            }

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                filePath,
                expectedSha256,
                description
            );
        }

        private static void DeleteOptionalFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);

            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new IOException
                (
                    "A committed database storage-migration backup could not be deleted."
                );
            }
        }

        private static void EnsurePlan(JasonQueryDbStorageMigrationRecoveryPlan plan, JasonQueryDbStorageMigrationRecoveryAction expectedAction, JasonQueryDbStorageMigrationStage expectedStage)
        {
            if (plan == null || plan.Journal == null || plan.Paths == null || plan.Action != expectedAction || plan.Journal.Stage != expectedStage)
            {
                throw new InvalidDataException
                (
                    "Database storage-migration recovery plan does not match the expected forward-recovery boundary."
                );
            }
        }

        private static void EnsureSameOperation(JasonQueryDbStorageMigrationJournal expected, JasonQueryDbStorageMigrationJournal actual)
        {
            if (expected == null || actual == null)
            {
                throw new InvalidDataException
                (
                    "Database storage-migration operation identity is unavailable."
                );
            }

            if (!string.Equals(expected.OperationId, actual.OperationId, StringComparison.Ordinal)
                || expected.JournalVersion != actual.JournalVersion || expected.ProtocolVersion != actual.ProtocolVersion
                || expected.CandidateWriterProtocolVersion != actual.CandidateWriterProtocolVersion
                || expected.SourceStorageFormatVersion != actual.SourceStorageFormatVersion
                || expected.TargetStorageFormatVersion != actual.TargetStorageFormatVersion
                || !string.Equals(expected.SourceDatabaseSha256, actual.SourceDatabaseSha256, StringComparison.Ordinal)
                || !string.Equals(expected.CandidateDatabaseSha256, actual.CandidateDatabaseSha256, StringComparison.Ordinal)
                || !string.Equals(expected.SourceMetadataSha256, actual.SourceMetadataSha256, StringComparison.Ordinal)
                || !string.Equals(expected.TargetMetadataSha256, actual.TargetMetadataSha256, StringComparison.Ordinal)
                || !JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(expected.SourceMetadata, actual.SourceMetadata)
                || !JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(expected.TargetMetadata, actual.TargetMetadata))
            {
                throw new InvalidDataException
                (
                    "Database storage-migration operation identity changed during forward recovery."
                );
            }
        }

        private static void EnsureAbsent(string filePath, string description)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " exists in a migration state where it is not allowed."
                );
            }
        }
    }

    internal sealed class JasonQueryDbStorageMigrationForwardRecoveryResult
    {
        public JasonQueryDbStorageMigrationForwardRecoveryResult(string operationId)
        {
            OperationId = operationId;
        }

        public string OperationId { get; }
    }
}
