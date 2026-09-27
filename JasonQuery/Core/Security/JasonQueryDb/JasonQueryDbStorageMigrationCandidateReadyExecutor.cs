using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Advances a hash-verified CandidateReady migration to the durable ReplacePrepared boundary.
    ///
    /// This executor is intentionally the final pre-replacement stage. It revalidates the
    /// existing Storage V2 candidate through the isolated Modern helper, durably prepares the
    /// target security metadata temporary file, and then commits ReplacePrepared to the journal.
    /// It does not replace JasonQuery.db, commit canonical security metadata, clean backups, or
    /// change runtime storage routing.
    /// </summary>
    internal sealed class JasonQueryDbStorageMigrationCandidateReadyExecutor
    {
        private readonly string _databaseFilePath;
        private readonly string _metadataFilePath;
        private readonly JasonQueryDbStorageMigrationRecovery _recovery;
        private readonly JasonQueryDbStorageMigrationJournalStore _journalStore;
        private readonly Func<string, string, byte[], JasonQueryDbStorageV2ReadOnlyValidationResult> _validateCandidate;

        public JasonQueryDbStorageMigrationCandidateReadyExecutor(string databaseFilePath, string metadataFilePath)
            : this (databaseFilePath, metadataFilePath, ValidateCandidateReadOnly)
        {
        }

        internal JasonQueryDbStorageMigrationCandidateReadyExecutor(string databaseFilePath, string metadataFilePath,
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
            _validateCandidate = validateCandidate ?? throw new ArgumentNullException(nameof(validateCandidate));

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

        public JasonQueryDbStorageMigrationReplacePreparationResult PrepareReplace(string modernHelperFilePath, byte[] targetDatabasePasswordUtf8)
        {
            ValidateSecret(targetDatabasePasswordUtf8);

            var plan = _recovery.Evaluate();

            if (plan.Action != JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace || plan.Journal == null || plan.Paths == null)
            {
                throw new InvalidOperationException
                (
                    "Database storage migration is not at the CandidateReady recovery boundary."
                );
            }

            var journal = plan.Journal;
            var paths = plan.Paths;

            EnsureJournalStillCandidateReady(journal);
            EnsureFrozenCandidateReadyInputs(journal, paths);

            var validation = _validateCandidate
            (
                modernHelperFilePath,
                paths.CandidateDatabaseFilePath,
                targetDatabasePasswordUtf8
            );

            if (validation == null || !string.Equals(validation.CandidateDatabaseSha256, journal.CandidateDatabaseSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "Storage V2 candidate read-only validation did not return the durable candidate identity."
                );
            }

            EnsureFrozenCandidateReadyInputs(journal, paths);
            EnsureJournalStillCandidateReady(journal);

            PrepareTargetMetadataTemporary(journal, paths);

            EnsureFrozenCandidateReadyInputs(journal, paths);
            EnsureTargetMetadataTemporary(journal, paths);
            EnsureJournalStillCandidateReady(journal);

            journal.Stage = JasonQueryDbStorageMigrationStage.ReplacePrepared;
            _journalStore.Save(journal);

            var committed = _journalStore.Load();

            EnsureCommittedReplacePrepared(journal, committed, paths);

            return new JasonQueryDbStorageMigrationReplacePreparationResult
            (
                paths,
                committed
            );
        }

        private static JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidateReadOnly(string modernHelperFilePath, string candidateDatabaseFilePath,
                                                                                               byte[] databasePasswordUtf8)
        {
            var runner = new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner();

            return runner.Validate
            (
                modernHelperFilePath,
                candidateDatabaseFilePath,
                databasePasswordUtf8
            );
        }

        private void EnsureJournalStillCandidateReady(JasonQueryDbStorageMigrationJournal expected)
        {
            if (!_journalStore.Exists)
            {
                throw new InvalidDataException
                (
                    "The canonical migration journal disappeared before ReplacePrepared."
                );
            }

            var current = _journalStore.Load();

            if (current.Stage != JasonQueryDbStorageMigrationStage.CandidateReady || !JournalIdentityEquals(expected, current))
            {
                throw new InvalidDataException
                (
                    "The canonical migration journal changed while CandidateReady was being prepared."
                );
            }
        }

        private static void EnsureFrozenCandidateReadyInputs(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
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

            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.CandidateDatabaseFilePath,
                journal.CandidateDatabaseSha256,
                "Storage V2 candidate"
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

        private static void PrepareTargetMetadataTemporary(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            if (Directory.Exists(paths.MetadataTemporaryFilePath))
            {
                throw new InvalidDataException
                (
                    "Target metadata temporary path is not a regular file."
                );
            }

            if (File.Exists(paths.MetadataTemporaryFilePath))
            {
                File.Delete(paths.MetadataTemporaryFilePath);
            }

            JasonQueryDbStorageMigrationMetadataContract.WriteDurablyCreateNew
            (
                paths.MetadataTemporaryFilePath,
                journal.TargetMetadata
            );

            EnsureTargetMetadataTemporary(journal, paths);
        }

        private static void EnsureTargetMetadataTemporary(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            JasonQueryDbStorageMigrationFileIntegrity.EnsureMatchesSha256
            (
                paths.MetadataTemporaryFilePath,
                journal.TargetMetadataSha256,
                "Target metadata temporary file"
            );

            var temporaryStore = new JasonQueryDbSecurityMetadataStore
            (
                paths.MetadataTemporaryFilePath
            );

            var loadedTargetMetadata = temporaryStore.Load();

            if (!JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(journal.TargetMetadata, loadedTargetMetadata))
            {
                throw new InvalidDataException
                (
                    "Target metadata temporary file does not match the durable target metadata snapshot."
                );
            }

            var expectedTargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
            (
                journal.SourceMetadata
            );

            if (!JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(expectedTargetMetadata, loadedTargetMetadata))
            {
                throw new InvalidDataException
                (
                    "Target metadata temporary file does not preserve the source security identity."
                );
            }
        }

        private static void EnsureCommittedReplacePrepared(JasonQueryDbStorageMigrationJournal expected, JasonQueryDbStorageMigrationJournal committed,
                                                           JasonQueryDbStorageMigrationArtifactPaths paths)
        {
            if (committed.Stage != JasonQueryDbStorageMigrationStage.ReplacePrepared || !JournalIdentityEquals(expected, committed))
            {
                throw new InvalidDataException
                (
                    "The durable ReplacePrepared journal does not match the validated migration identity."
                );
            }

            EnsureFrozenCandidateReadyInputs(committed, paths);
            EnsureTargetMetadataTemporary(committed, paths);
        }

        private static bool JournalIdentityEquals(JasonQueryDbStorageMigrationJournal left, JasonQueryDbStorageMigrationJournal right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.JournalVersion == right.JournalVersion
                   && string.Equals(left.OperationId, right.OperationId, StringComparison.Ordinal)
                   && left.ProtocolVersion == right.ProtocolVersion
                   && left.CandidateWriterProtocolVersion == right.CandidateWriterProtocolVersion
                   && left.SourceStorageFormatVersion == right.SourceStorageFormatVersion
                   && left.TargetStorageFormatVersion == right.TargetStorageFormatVersion
                   && string.Equals(left.SourceDatabaseSha256, right.SourceDatabaseSha256, StringComparison.Ordinal)
                   && string.Equals(left.CandidateDatabaseSha256, right.CandidateDatabaseSha256, StringComparison.Ordinal)
                   && string.Equals(left.SourceMetadataSha256, right.SourceMetadataSha256, StringComparison.Ordinal)
                   && string.Equals(left.TargetMetadataSha256, right.TargetMetadataSha256, StringComparison.Ordinal)
                   && JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(left.SourceMetadata, right.SourceMetadata)
                   && JasonQueryDbStorageMigrationMetadataContract.MetadataEquals(left.TargetMetadata, right.TargetMetadata);
        }

        private static void EnsureAbsent(string filePath, string description)
        {
            if (File.Exists(filePath) || Directory.Exists(filePath))
            {
                throw new InvalidDataException
                (
                    description + " exists before ReplacePrepared."
                );
            }
        }

        private static void ValidateSecret(byte[] value)
        {
            if (value == null || value.Length <= 0 || value.Length > JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "A bounded target database password is required.",
                    nameof(value)
                );
            }
        }
    }

    internal sealed class JasonQueryDbStorageMigrationReplacePreparationResult
    {
        public JasonQueryDbStorageMigrationReplacePreparationResult(JasonQueryDbStorageMigrationArtifactPaths paths,
                                                                    JasonQueryDbStorageMigrationJournal journal)
        {
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            Journal = journal ?? throw new ArgumentNullException(nameof(journal));
        }

        public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }

        public JasonQueryDbStorageMigrationJournal Journal { get; }
    }
}
