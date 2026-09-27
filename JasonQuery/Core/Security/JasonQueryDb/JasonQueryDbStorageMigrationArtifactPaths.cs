using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Deterministic same-directory artifact paths for one physical Storage V1 -> V2 migration.
    /// Paths are derived locally from the production database, metadata path, and canonical operationId.
    /// They are never accepted from persisted journal data.
    /// </summary>
    public sealed class JasonQueryDbStorageMigrationArtifactPaths
    {
        private const string JournalSuffix = ".migration.json";
        private const string LockSuffix = ".migration.lock";

        private JasonQueryDbStorageMigrationArtifactPaths(string databaseFilePath, string metadataFilePath, string operationId)
        {
            DatabaseFilePath = databaseFilePath;
            MetadataFilePath = metadataFilePath;
            OperationId = operationId;

            JournalFilePath = GetJournalFilePath(databaseFilePath);
            JournalTemporaryFilePath = GetJournalTemporaryFilePath(databaseFilePath, operationId);
            CandidateDatabaseFilePath = databaseFilePath + ".migration." + operationId + ".candidate";
            DatabaseBackupFilePath = databaseFilePath + ".migration." + operationId + ".backup";
            MetadataTemporaryFilePath = metadataFilePath + ".migration." + operationId + ".tmp";
            MetadataBackupFilePath = metadataFilePath + ".migration." + operationId + ".backup";
            LockFilePath = GetLockFilePath(databaseFilePath);
        }

        public string DatabaseFilePath { get; }

        public string MetadataFilePath { get; }

        public string OperationId { get; }

        public string JournalFilePath { get; }

        public string JournalTemporaryFilePath { get; }

        public string CandidateDatabaseFilePath { get; }

        public string DatabaseBackupFilePath { get; }

        public string MetadataTemporaryFilePath { get; }

        public string MetadataBackupFilePath { get; }

        public string LockFilePath { get; }

        public static JasonQueryDbStorageMigrationArtifactPaths Create(string databaseFilePath, string metadataFilePath, string operationId)
        {
            var normalizedDatabaseFilePath = NormalizeFilePath(databaseFilePath, nameof(databaseFilePath));
            var normalizedMetadataFilePath = NormalizeFilePath(metadataFilePath, nameof(metadataFilePath));
            var normalizedOperationId = NormalizeOperationId(operationId);

            if (string.Equals(normalizedDatabaseFilePath, normalizedMetadataFilePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The database file and security metadata file must be different files.");
            }

            var databaseDirectory = Path.GetDirectoryName(normalizedDatabaseFilePath);
            var metadataDirectory = Path.GetDirectoryName(normalizedMetadataFilePath);

            if (!string.Equals(databaseDirectory, metadataDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration artifacts require JasonQuery.db and " +
                    "JasonQuery.security.json to reside in the same directory and volume."
                );
            }

            return new JasonQueryDbStorageMigrationArtifactPaths
            (
                normalizedDatabaseFilePath,
                normalizedMetadataFilePath,
                normalizedOperationId
            );
        }

        public static string GetJournalFilePath(string databaseFilePath)
        {
            return NormalizeFilePath(databaseFilePath, nameof(databaseFilePath)) + JournalSuffix;
        }

        public static string GetJournalTemporaryFilePath(string databaseFilePath, string operationId)
        {
            var normalizedDatabaseFilePath = NormalizeFilePath(databaseFilePath, nameof(databaseFilePath));
            var normalizedOperationId = NormalizeOperationId(operationId);

            return normalizedDatabaseFilePath + ".migration." + normalizedOperationId + ".journal.tmp";
        }

        public static string GetLockFilePath(string databaseFilePath)
        {
            return NormalizeFilePath(databaseFilePath, nameof(databaseFilePath)) + LockSuffix;
        }

        private static string NormalizeFilePath(string filePath, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A file path is required.", parameterName);
            }

            return Path.GetFullPath(filePath);
        }

        private static string NormalizeOperationId(string operationId)
        {
            if (!Guid.TryParseExact(operationId, "N", out var parsed) || parsed == Guid.Empty || !string.Equals(operationId, parsed.ToString("N"), StringComparison.Ordinal))
            {
                throw new ArgumentException
                (
                    "A canonical non-empty operationId in Guid N format is required.",
                    nameof(operationId)
                );
            }

            return operationId;
        }
    }
}
