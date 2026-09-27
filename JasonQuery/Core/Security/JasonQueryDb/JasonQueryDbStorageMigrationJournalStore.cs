using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Durable canonical store for JasonQueryDbStorageMigrationJournal.
    ///
    /// The canonical journal is the authority for the last durably completed migration stage.
    /// Operation-specific journal temporary files are uncommitted scratch state and are never
    /// treated as a completed stage.
    /// </summary>
    public sealed class JasonQueryDbStorageMigrationJournalStore
    {
        private readonly string _databaseFilePath;
        private readonly string _journalFilePath;

        public JasonQueryDbStorageMigrationJournalStore(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            _databaseFilePath = Path.GetFullPath(databaseFilePath);
            _journalFilePath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalFilePath(_databaseFilePath);
        }

        public string DatabaseFilePath => _databaseFilePath;

        public string JournalFilePath => _journalFilePath;

        public bool Exists => File.Exists(_journalFilePath);

        public JasonQueryDbStorageMigrationJournal Load()
        {
            if (!Exists)
            {
                throw new FileNotFoundException("The database storage-migration journal was not found.", _journalFilePath);
            }

            var json = File.ReadAllText(_journalFilePath, Encoding.UTF8);

            JasonQueryDbStorageMigrationJournal journal;

            try
            {
                journal = JsonConvert.DeserializeObject<JasonQueryDbStorageMigrationJournal>(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("The database storage-migration journal is invalid.", ex);
            }

            if (journal == null)
            {
                throw new InvalidDataException("The database storage-migration journal is empty or invalid.");
            }

            journal.Validate();
            return journal;
        }

        public void Save(JasonQueryDbStorageMigrationJournal journal)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            journal.Validate();
            JasonQueryDbStorageMigrationJournal current = null;

            if (Exists)
            {
                current = Load();
            }

            ValidateStageTransition(current, journal);

            var temporaryFilePath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath
            (
                _databaseFilePath,
                journal.OperationId
            );

            if (File.Exists(temporaryFilePath))
            {
                if (current == null)
                {
                    throw new InvalidDataException
                    (
                        "An uncommitted database storage-migration journal temporary file exists without a canonical journal."
                    );
                }

                File.Delete(temporaryFilePath);
            }

            var directory = Path.GetDirectoryName(_journalFilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(journal, Formatting.Indented);
            var bytes = new UTF8Encoding(false).GetBytes(json);

            try
            {
                using (var stream = new FileStream(temporaryFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(_journalFilePath))
                {
                    File.Replace(temporaryFilePath, _journalFilePath, null, true);
                }
                else
                {
                    File.Move(temporaryFilePath, _journalFilePath);
                }

                var committed = Load();

                EnsureEquivalent(journal, committed);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);

                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
        }

        public void Delete()
        {
            if (!Exists)
            {
                return;
            }

            var journal = Load();

            var temporaryFilePath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath
            (
                _databaseFilePath,
                journal.OperationId
            );

            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }

            //The canonical journal is intentionally deleted last.
            File.Delete(_journalFilePath);
        }

        private static void ValidateStageTransition(JasonQueryDbStorageMigrationJournal current, JasonQueryDbStorageMigrationJournal next)
        {
            if (current == null)
            {
                if (next.Stage != JasonQueryDbStorageMigrationStage.PreparingCandidate)
                {
                    throw new InvalidOperationException
                    (
                        "A new database storage-migration journal must begin at PreparingCandidate."
                    );
                }

                return;
            }

            EnsureSameOperation(current, next);

            var currentStage = (int)current.Stage;
            var nextStage = (int)next.Stage;

            if (nextStage < currentStage)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration journal stage cannot move backward."
                );
            }

            if (nextStage > currentStage + 1)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration journal stage cannot skip durable stages."
                );
            }

            if (current.CandidateDatabaseSha256 != null && !string.Equals(current.CandidateDatabaseSha256, next.CandidateDatabaseSha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration candidate identity cannot change after CandidateReady."
                );
            }
        }

        private static void EnsureSameOperation(JasonQueryDbStorageMigrationJournal current, JasonQueryDbStorageMigrationJournal next)
        {
            if (!string.Equals(current.OperationId, next.OperationId, StringComparison.Ordinal)
                || current.JournalVersion != next.JournalVersion
                || current.ProtocolVersion != next.ProtocolVersion
                || current.CandidateWriterProtocolVersion != next.CandidateWriterProtocolVersion
                || current.SourceStorageFormatVersion != next.SourceStorageFormatVersion
                || current.TargetStorageFormatVersion != next.TargetStorageFormatVersion
                || !string.Equals(current.SourceDatabaseSha256, next.SourceDatabaseSha256, StringComparison.Ordinal)
                || !string.Equals(current.SourceMetadataSha256, next.SourceMetadataSha256, StringComparison.Ordinal)
                || !string.Equals(current.TargetMetadataSha256, next.TargetMetadataSha256, StringComparison.Ordinal)
                || !MetadataEquals(current.SourceMetadata, next.SourceMetadata)
                || !MetadataEquals(current.TargetMetadata, next.TargetMetadata))
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration journal operation identity changed unexpectedly."
                );
            }

            if (current.Stage == next.Stage && !string.Equals(current.CandidateDatabaseSha256, next.CandidateDatabaseSha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration journal cannot change candidate identity within the same durable stage."
                );
            }
        }

        private static void EnsureEquivalent(JasonQueryDbStorageMigrationJournal expected, JasonQueryDbStorageMigrationJournal actual)
        {
            if (expected.Stage != actual.Stage || !string.Equals(expected.CandidateDatabaseSha256, actual.CandidateDatabaseSha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "Database storage-migration journal validation failed after durable save."
                );
            }

            EnsureSameOperation(expected, actual);
        }

        private static bool MetadataEquals(JasonQueryDbSecurityMetadata left, JasonQueryDbSecurityMetadata right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.MetadataVersion == right.MetadataVersion
                   && left.EncryptionVersion == right.EncryptionVersion
                   && left.StorageFormatVersion == right.StorageFormatVersion
                   && left.Mode == right.Mode
                   && string.Equals(left.Protection, right.Protection, StringComparison.Ordinal)
                   && string.Equals(left.ProtectedDatabaseKey, right.ProtectedDatabaseKey, StringComparison.Ordinal)
                   && string.Equals(left.Kdf, right.Kdf, StringComparison.Ordinal)
                   && left.Iterations == right.Iterations
                   && string.Equals(left.Salt, right.Salt, StringComparison.Ordinal)
                   && JasonQueryDbRecoveryMetadata.MetadataEquals(left.Recovery, right.Recovery);
        }
    }
}
