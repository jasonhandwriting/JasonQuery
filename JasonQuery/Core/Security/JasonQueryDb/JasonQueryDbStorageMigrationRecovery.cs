using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Read-only recovery evaluator for the durable Storage V1 -> V2 migration state machine.
    /// It derives every artifact path locally and classifies only hash-verified disk states.
    /// It never probes SQLite providers and never mutates production state.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationRecovery
    {
        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;

        public JasonQueryDbStorageMigrationRecovery(string databaseFilePath, string metadataFilePath)
        {
            _databaseFilePath = NormalizeFilePath(databaseFilePath, nameof(databaseFilePath));
            _metadataFilePath = NormalizeFilePath(metadataFilePath, nameof(metadataFilePath));

            JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                _metadataFilePath,
                Guid.NewGuid().ToString("N")
            );

            _journalStore = new JasonQueryDbStorageMigrationJournalStore(_databaseFilePath);
        }

        public JasonQueryDbStorageMigrationRecoveryPlan Evaluate()
        {
            if (!_journalStore.Exists)
            {
                EnsureNoOrphanArtifacts(_databaseFilePath, _metadataFilePath);

                return new JasonQueryDbStorageMigrationRecoveryPlan
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    null,
                    null
                );
            }

            var journal = _journalStore.Load();

            var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                _metadataFilePath,
                journal.OperationId
            );

            switch (journal.Stage)
            {
                case JasonQueryDbStorageMigrationStage.PreparingCandidate:
                    {
                        return EvaluatePreparingCandidate(journal, paths);
                    }
                case JasonQueryDbStorageMigrationStage.CandidateReady:
                    {
                        return EvaluateCandidateReady(journal, paths);
                    }
                case JasonQueryDbStorageMigrationStage.ReplacePrepared:
                    {
                        return EvaluateReplacePrepared(journal, paths);
                    }
                case JasonQueryDbStorageMigrationStage.DatabaseReplaced:
                    {
                        return EvaluateDatabaseReplaced(journal, paths);
                    }
                case JasonQueryDbStorageMigrationStage.MetadataCommitted:
                    {
                        return EvaluateMetadataCommitted(journal, paths);
                    }
                default:
                    {
                        throw new NotSupportedException
                        (
                            $"Database storage-migration stage '{(int)journal.Stage}' is not supported."
                        );
                    }
            }
        }

        internal static void EnsureNoOrphanArtifacts(string databaseFilePath, string metadataFilePath)
        {
            EnsureNoOrphanArtifacts(databaseFilePath, metadataFilePath, false);
        }

        internal static void EnsureNoOrphanArtifacts(string databaseFilePath, string metadataFilePath, bool allowOwnedExecutionLock)
        {
            var probe = JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                databaseFilePath,
                metadataFilePath,
                Guid.NewGuid().ToString("N")
            );

            if (File.Exists(probe.JournalFilePath) || Directory.Exists(probe.JournalFilePath))
            {
                throw new InvalidOperationException
                (
                    "A database storage-migration journal exists and must be recovered before normal startup."
                );
            }

            if (Directory.Exists(probe.LockFilePath))
            {
                throw new InvalidDataException
                (
                    "A database storage-migration lock artifact is not a regular file."
                );
            }

            var lockExists = File.Exists(probe.LockFilePath);

            if (!allowOwnedExecutionLock && lockExists)
            {
                throw new InvalidDataException
                (
                    "A database storage-migration lock artifact exists without a canonical migration journal."
                );
            }

            if (allowOwnedExecutionLock && !lockExists)
            {
                throw new InvalidDataException
                (
                    "The owned database storage-migration execution lock is missing."
                );
            }

            var directory = Path.GetDirectoryName(probe.DatabaseFilePath);

            foreach (var filePath in Directory.GetFileSystemEntries(directory))
            {
                var fileName = Path.GetFileName(filePath);

                if (IsOperationArtifactName(fileName, Path.GetFileName(probe.DatabaseFilePath), true) || IsOperationArtifactName(fileName, Path.GetFileName(probe.MetadataFilePath), false))
                {
                    throw new InvalidDataException
                    (
                        "An orphan database storage-migration artifact exists without a canonical migration journal."
                    );
                }
            }
        }

        internal static void EnsureNoSqliteSidecars(string databaseFilePath, string description)
        {
            foreach (var suffix in new[] { "-journal", "-wal", "-shm" })
            {
                if (File.Exists(databaseFilePath + suffix))
                {
                    throw new InvalidDataException
                    (
                        description + " has an active SQLite sidecar and cannot enter migration recovery."
                    );
                }
            }
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan EvaluatePreparingCandidate(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            EnsureHash(paths.DatabaseFilePath, journal.SourceDatabaseSha256, "Production database");
            EnsureHash(paths.MetadataFilePath, journal.SourceMetadataSha256, "Security metadata");
            EnsureAbsent(paths.DatabaseBackupFilePath, "Database backup");
            EnsureAbsent(paths.MetadataBackupFilePath, "Metadata backup");

            EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Production database");

            return Plan
            (
                JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation,
                journal,
                paths
            );
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan EvaluateCandidateReady(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            EnsureHash(paths.DatabaseFilePath, journal.SourceDatabaseSha256, "Production database");
            EnsureHash(paths.MetadataFilePath, journal.SourceMetadataSha256, "Security metadata");
            EnsureHash(paths.CandidateDatabaseFilePath, journal.CandidateDatabaseSha256, "Storage V2 candidate");
            EnsureAbsent(paths.DatabaseBackupFilePath, "Database backup");
            EnsureAbsent(paths.MetadataBackupFilePath, "Metadata backup");

            EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Production database");
            EnsureNoSqliteSidecars(paths.CandidateDatabaseFilePath, "Storage V2 candidate");

            return Plan
            (
                JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace,
                journal,
                paths
            );
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan EvaluateReplacePrepared(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            EnsureHash(paths.MetadataFilePath, journal.SourceMetadataSha256, "Security metadata");
            EnsureHash(paths.MetadataTemporaryFilePath, journal.TargetMetadataSha256, "Target metadata temporary file");
            EnsureAbsent(paths.MetadataBackupFilePath, "Metadata backup");

            var productionIsSource = Matches(paths.DatabaseFilePath, journal.SourceDatabaseSha256);
            var productionIsCandidate = Matches(paths.DatabaseFilePath, journal.CandidateDatabaseSha256);
            var candidateExistsAndMatches = Matches(paths.CandidateDatabaseFilePath, journal.CandidateDatabaseSha256);
            var candidateMissing = !File.Exists(paths.CandidateDatabaseFilePath) && !Directory.Exists(paths.CandidateDatabaseFilePath);
            var backupMissing = !File.Exists(paths.DatabaseBackupFilePath) && !Directory.Exists(paths.DatabaseBackupFilePath);
            var backupIsSource = Matches(paths.DatabaseBackupFilePath, journal.SourceDatabaseSha256);
            var beforeReplace = productionIsSource && candidateExistsAndMatches && backupMissing;
            var replaceAlreadyCompleted = productionIsCandidate && candidateMissing && backupIsSource;

            if (beforeReplace == replaceAlreadyCompleted)
            {
                throw new InvalidDataException
                (
                    "ReplacePrepared does not match exactly one frozen database replacement state."
                );
            }

            if (beforeReplace)
            {
                EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Production database");
                EnsureNoSqliteSidecars(paths.CandidateDatabaseFilePath, "Storage V2 candidate");

                return Plan
                (
                    JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase,
                    journal,
                    paths
                );
            }

            EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Replaced production database");

            return Plan
            (
                JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced,
                journal,
                paths
            );
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan EvaluateDatabaseReplaced(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            EnsureHash(paths.DatabaseFilePath, journal.CandidateDatabaseSha256, "Replaced production database");
            EnsureHash(paths.DatabaseBackupFilePath, journal.SourceDatabaseSha256, "Database backup");
            EnsureAbsent(paths.CandidateDatabaseFilePath, "Storage V2 candidate");

            EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Replaced production database");

            var metadataIsSource = Matches(paths.MetadataFilePath, journal.SourceMetadataSha256);
            var metadataIsTarget = Matches(paths.MetadataFilePath, journal.TargetMetadataSha256);

            if (metadataIsSource == metadataIsTarget)
            {
                throw new InvalidDataException
                (
                    "DatabaseReplaced does not match exactly one frozen security-metadata state."
                );
            }

            if (metadataIsSource)
            {
                EnsureHash(paths.MetadataTemporaryFilePath, journal.TargetMetadataSha256, "Target metadata temporary file");
                EnsureAbsent(paths.MetadataBackupFilePath, "Metadata backup");

                return Plan
                (
                    JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata,
                    journal,
                    paths
                );
            }

            EnsureAbsent(paths.MetadataTemporaryFilePath, "Target metadata temporary file");
            EnsureHash(paths.MetadataBackupFilePath, journal.SourceMetadataSha256, "Metadata backup");

            return Plan
            (
                JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted,
                journal,
                paths
            );
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan EvaluateMetadataCommitted(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            EnsureHash(paths.DatabaseFilePath, journal.CandidateDatabaseSha256, "Production database");
            EnsureHash(paths.MetadataFilePath, journal.TargetMetadataSha256, "Security metadata");
            EnsureAbsent(paths.CandidateDatabaseFilePath, "Storage V2 candidate");
            EnsureAbsent(paths.MetadataTemporaryFilePath, "Target metadata temporary file");

            EnsureOptionalHash(paths.DatabaseBackupFilePath, journal.SourceDatabaseSha256, "Database backup");
            EnsureOptionalHash(paths.MetadataBackupFilePath, journal.SourceMetadataSha256, "Metadata backup");
            EnsureNoSqliteSidecars(paths.DatabaseFilePath, "Production database");

            return Plan
            (
                JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration,
                journal,
                paths
            );
        }

        private static JasonQueryDbStorageMigrationRecoveryPlan Plan(JasonQueryDbStorageMigrationRecoveryAction action, JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            return new JasonQueryDbStorageMigrationRecoveryPlan(action, journal, paths);
        }

        private static void EnsureHash(string filePath, string expectedSha256, string description)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                filePath,
                expectedSha256,
                description
            );
        }

        private static void EnsureOptionalHash(string filePath, string expectedSha256, string description)
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

            EnsureHash(filePath, expectedSha256, description);
        }

        private static bool Matches(string filePath, string expectedSha256)
        {
            return JasonQueryDbStorageMigrationFileIntegrity.MatchesSha256(filePath, expectedSha256);
        }

        private static void EnsureAbsent(string filePath, string description)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " exists in a migration stage where it is not allowed."
                );
            }
        }

        private static bool IsOperationArtifactName(string fileName, string ownerFileName, bool databaseArtifact)
        {
            var prefix = ownerFileName + ".migration.";

            if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (databaseArtifact)
            {
                return fileName.EndsWith(".candidate", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".backup", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".journal.tmp", StringComparison.OrdinalIgnoreCase);
            }

            return fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".backup", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeFilePath(string filePath, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A file path is required.", parameterName);
            }

            return Path.GetFullPath(filePath);
        }
    }

    internal enum JasonQueryDbStorageMigrationRecoveryAction
    {
        NoRecoveryRequired = 0,
        RestartPreparation = 1,
        ValidateCandidateAndPrepareReplace = 2,
        ReplaceDatabase = 3,
        RecognizeDatabaseReplaced = 4,
        CommitMetadata = 5,
        RecognizeMetadataCommitted = 6,
        CleanupCommittedMigration = 7
    }

    internal sealed class JasonQueryDbStorageMigrationRecoveryPlan
    {
        public JasonQueryDbStorageMigrationRecoveryPlan(JasonQueryDbStorageMigrationRecoveryAction action, JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            Action = action;
            Journal = journal;
            Paths = paths;
        }

        public JasonQueryDbStorageMigrationRecoveryAction Action { get; }

        public JasonQueryDbStorageMigrationJournal Journal { get; }

        public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }
    }
}
