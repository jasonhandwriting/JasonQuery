using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Creates the durable PreparingCandidate state for a physical Storage V1 -> V2 migration.
    ///
    /// This R4A core intentionally stops before child-process execution, replacement, metadata
    /// commit, startup integration, or runtime cutover. Production activation requires the
    /// later migration lock and recovery executor.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationOrchestrator
    {
        private readonly string _databaseFilePath;
        private readonly JasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;

        public JasonQueryDbStorageMigrationOrchestrator(string databaseFilePath, string metadataFilePath)
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

            var normalizedMetadataPath = Path.GetFullPath(metadataFilePath);

            JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                normalizedMetadataPath,
                Guid.NewGuid().ToString("N")
            );

            _metadataStore = new JasonQueryDbSecurityMetadataStore(normalizedMetadataPath);
            _journalStore = new JasonQueryDbStorageMigrationJournalStore(_databaseFilePath);
        }

        public JasonQueryDbStorageMigrationPreparationResult Prepare()
        {
            return PrepareCore(false);
        }

        internal JasonQueryDbStorageMigrationPreparationResult PrepareWhileExecutionLockHeld()
        {
            return PrepareCore(true);
        }

        private JasonQueryDbStorageMigrationPreparationResult PrepareCore(bool executionLockHeld)
        {
            if (_journalStore.Exists)
            {
                throw new InvalidOperationException
                (
                    "A database storage migration is already active and must be recovered before a new migration can start."
                );
            }

            JasonQueryDbStorageMigrationRecovery.EnsureNoOrphanArtifacts
            (
                _databaseFilePath,
                _metadataStore.MetadataFilePath,
                executionLockHeld
            );

            if (!File.Exists(_databaseFilePath))
            {
                throw new FileNotFoundException
                (
                    "The source JasonQuery database was not found.",
                    _databaseFilePath
                );
            }

            if (!_metadataStore.Exists)
            {
                throw new FileNotFoundException
                (
                    "The source database security metadata was not found.",
                    _metadataStore.MetadataFilePath
                );
            }

            JasonQueryDbStorageMigrationRecovery.EnsureNoSqliteSidecars
            (
                _databaseFilePath,
                "Source database"
            );

            var sourceMetadataSha256BeforeLoad = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
            (
                _metadataStore.MetadataFilePath
            );

            var sourceMetadata = _metadataStore.Load();

            var sourceMetadataSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
            (
                _metadataStore.MetadataFilePath
            );

            if (!string.Equals(sourceMetadataSha256BeforeLoad, sourceMetadataSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "Database security metadata changed while PreparingCandidate was being established."
                );
            }

            var resolvedSourceVersion = (int)JasonQueryDbStorageFormatContract.ResolveVersion
            (
                sourceMetadata.StorageFormatVersion
            );

            if (resolvedSourceVersion != JasonQueryDbStorageFormatContract.LegacyVersion)
            {
                throw new InvalidOperationException
                (
                    "A new physical storage migration can start only from Legacy Storage V1."
                );
            }

            var sourceDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
            (
                _databaseFilePath
            );

            var targetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
            (
                sourceMetadata
            );

            var targetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256
            (
                targetMetadata
            );

            var operationId = Guid.NewGuid().ToString("N");

            var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                _metadataStore.MetadataFilePath,
                operationId
            );

            EnsureOperationArtifactsAbsent(paths, executionLockHeld);

            var journal = new JasonQueryDbStorageMigrationJournal
            {
                JournalVersion = JasonQueryDbStorageMigrationJournal.CurrentVersion,
                OperationId = operationId,
                Stage = JasonQueryDbStorageMigrationStage.PreparingCandidate,
                ProtocolVersion = JasonQueryDbStorageMigrationProtocolContract.CurrentVersion,
                CandidateWriterProtocolVersion = JasonQueryDbStorageV2CandidateWriterProtocol.CurrentVersion,
                SourceStorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion,
                TargetStorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion,
                SourceDatabaseSha256 = sourceDatabaseSha256,
                CandidateDatabaseSha256 = null,
                SourceMetadataSha256 = sourceMetadataSha256,
                TargetMetadataSha256 = targetMetadataSha256,
                SourceMetadata = sourceMetadata,
                TargetMetadata = targetMetadata
            };

            journal.Validate();
            _journalStore.Save(journal);

            var committed = _journalStore.Load();

            if (committed.Stage != JasonQueryDbStorageMigrationStage.PreparingCandidate
                || !string.Equals(committed.OperationId, operationId, StringComparison.Ordinal)
                || !string.Equals(committed.SourceDatabaseSha256, sourceDatabaseSha256, StringComparison.Ordinal)
                || !string.Equals(committed.SourceMetadataSha256, sourceMetadataSha256, StringComparison.Ordinal)
                || !string.Equals(committed.TargetMetadataSha256, targetMetadataSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "The durable PreparingCandidate journal does not match the prepared migration identity."
                );
            }

            return new JasonQueryDbStorageMigrationPreparationResult(paths, committed);
        }

        internal JasonQueryDbStorageMigrationJournal CommitCandidateReady(JasonQueryDbStorageMigrationPreparationResult preparation)
        {
            if (preparation == null)
            {
                throw new ArgumentNullException(nameof(preparation));
            }

            if (!_journalStore.Exists)
            {
                throw new InvalidDataException
                (
                    "The canonical migration journal disappeared before CandidateReady."
                );
            }

            var current = _journalStore.Load();

            EnsurePreparationIdentity(preparation, current);

            if (current.Stage != JasonQueryDbStorageMigrationStage.PreparingCandidate)
            {
                throw new InvalidOperationException
                (
                    "CandidateReady can be committed only from PreparingCandidate."
                );
            }

            var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
            (
                _databaseFilePath,
                _metadataStore.MetadataFilePath,
                current.OperationId
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                current.SourceDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                current.SourceMetadataSha256,
                "Security metadata"
            );

            if (!File.Exists(paths.CandidateDatabaseFilePath) || Directory.Exists(paths.CandidateDatabaseFilePath))
            {
                throw new FileNotFoundException
                (
                    "The Storage V2 candidate database was not produced.",
                    paths.CandidateDatabaseFilePath
                );
            }

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

            var candidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
            (
                paths.CandidateDatabaseFilePath
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.DatabaseFilePath,
                current.SourceDatabaseSha256,
                "Production database"
            );

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataFilePath,
                current.SourceMetadataSha256,
                "Security metadata"
            );

            current.CandidateDatabaseSha256 = candidateDatabaseSha256;
            current.Stage = JasonQueryDbStorageMigrationStage.CandidateReady;

            _journalStore.Save(current);

            var committed = _journalStore.Load();

            EnsurePreparationIdentity(preparation, committed);

            if (committed.Stage != JasonQueryDbStorageMigrationStage.CandidateReady || !string.Equals(committed.CandidateDatabaseSha256, candidateDatabaseSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "The durable CandidateReady journal does not match the produced Storage V2 candidate."
                );
            }

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.CandidateDatabaseFilePath,
                committed.CandidateDatabaseSha256,
                "Storage V2 candidate"
            );

            return committed;
        }

        private static void EnsurePreparationIdentity(JasonQueryDbStorageMigrationPreparationResult preparation, JasonQueryDbStorageMigrationJournal journal)
        {
            if (preparation.Journal == null || preparation.Paths == null || journal == null
                || !string.Equals(preparation.Journal.OperationId, journal.OperationId, StringComparison.Ordinal)
                || preparation.Journal.JournalVersion != journal.JournalVersion || preparation.Journal.ProtocolVersion != journal.ProtocolVersion
                || preparation.Journal.CandidateWriterProtocolVersion != journal.CandidateWriterProtocolVersion
                || preparation.Journal.SourceStorageFormatVersion != journal.SourceStorageFormatVersion
                || preparation.Journal.TargetStorageFormatVersion != journal.TargetStorageFormatVersion
                || !string.Equals(preparation.Journal.SourceDatabaseSha256, journal.SourceDatabaseSha256, StringComparison.Ordinal)
                || !string.Equals(preparation.Journal.SourceMetadataSha256, journal.SourceMetadataSha256, StringComparison.Ordinal)
                || !string.Equals(preparation.Journal.TargetMetadataSha256, journal.TargetMetadataSha256, StringComparison.Ordinal)
                || !JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(preparation.Journal.SourceMetadata, journal.SourceMetadata)
                || !JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(preparation.Journal.TargetMetadata, journal.TargetMetadata))
            {
                throw new InvalidDataException
                (
                    "The PreparingCandidate migration identity changed before CandidateReady."
                );
            }
        }

        private static void EnsureAbsent(string filePath, string description)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " exists before CandidateReady."
                );
            }
        }

        private static void EnsureOperationArtifactsAbsent(JasonQueryDbStorageMigrationArtifactPaths paths, bool executionLockHeld)
        {
            foreach (var path in new[] { paths.JournalTemporaryFilePath, paths.CandidateDatabaseFilePath, paths.DatabaseBackupFilePath, paths.MetadataTemporaryFilePath, paths.MetadataBackupFilePath})
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    throw new InvalidDataException
                    (
                        "A database storage-migration artifact already exists before PreparingCandidate."
                    );
                }
            }

            if (!executionLockHeld)
            {
                if (File.Exists(paths.LockFilePath) || Directory.Exists(paths.LockFilePath))
                {
                    throw new InvalidDataException
                    (
                        "A database storage-migration lock artifact already exists before PreparingCandidate."
                    );
                }

                return;
            }

            if (!File.Exists(paths.LockFilePath) || Directory.Exists(paths.LockFilePath))
            {
                throw new InvalidDataException
                (
                    "The owned database storage-migration execution lock is unavailable before PreparingCandidate."
                );
            }
        }
    }

    internal sealed class JasonQueryDbStorageMigrationPreparationResult
    {
        public JasonQueryDbStorageMigrationPreparationResult(JasonQueryDbStorageMigrationArtifactPaths paths, JasonQueryDbStorageMigrationJournal journal)
        {
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            Journal = journal ?? throw new ArgumentNullException(nameof(journal));
        }

        public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }

        public JasonQueryDbStorageMigrationJournal Journal { get; }
    }
}
