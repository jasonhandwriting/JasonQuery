using Newtonsoft.Json;
using System;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Durable recovery record for physical Storage V1 -> V2 migration.
    ///
    /// This is intentionally separate from DatabaseSecurityTransitionJournal.
    /// It tracks explicit cross-provider stages instead of probing providers/keys.
    /// Plaintext passwords, database keys, row payloads, and SQL/data dumps are
    /// never persisted in this journal.
    /// </summary>
    public sealed class DatabaseStorageMigrationJournal
    {
        public const int CurrentVersion = 1;

        [JsonProperty("journalVersion")]
        public int JournalVersion { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("stage")]
        public DatabaseStorageMigrationStage Stage { get; set; }

        [JsonProperty("protocolVersion")]
        public int ProtocolVersion { get; set; }

        [JsonProperty("sourceStorageFormatVersion")]
        public int SourceStorageFormatVersion { get; set; }

        [JsonProperty("targetStorageFormatVersion")]
        public int TargetStorageFormatVersion { get; set; }

        [JsonProperty("sourceMetadata")]
        public DatabaseSecurityMetadata SourceMetadata { get; set; }

        [JsonProperty("targetMetadata")]
        public DatabaseSecurityMetadata TargetMetadata { get; set; }

        public void Validate()
        {
            if (JournalVersion != CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Database storage-migration journal version '{JournalVersion}' is not supported."
                );
            }

            if (!Guid.TryParseExact(OperationId, "N", out var operationId) || operationId == Guid.Empty)
            {
                throw new InvalidOperationException("Database storage-migration operationId is invalid.");
            }

            if (!Enum.IsDefined(typeof(DatabaseStorageMigrationStage), Stage))
            {
                throw new NotSupportedException
                (
                    $"Database storage-migration stage '{(int)Stage}' is not supported."
                );
            }

            DatabaseStorageMigrationProtocolContract.EnsureSupportedVersion(ProtocolVersion);

            DatabaseStorageMigrationProtocolContract.EnsureSupportedRoute
            (
                SourceStorageFormatVersion,
                TargetStorageFormatVersion
            );

            if (SourceMetadata == null || TargetMetadata == null)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration metadata snapshots are incomplete."
                );
            }

            SourceMetadata.Validate();
            TargetMetadata.Validate();

            var resolvedSource = (int)DatabaseStorageFormatContract.ResolveVersion(SourceMetadata.StorageFormatVersion);

            if (resolvedSource != SourceStorageFormatVersion)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration source metadata does not match the declared source format."
                );
            }

            if (!TargetMetadata.StorageFormatVersion.HasValue || TargetMetadata.StorageFormatVersion.Value != TargetStorageFormatVersion)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration target metadata must explicitly identify the target format."
                );
            }

            var resolvedTarget = (int)DatabaseStorageFormatContract.ResolveVersion(TargetMetadata.StorageFormatVersion);

            if (resolvedTarget != TargetStorageFormatVersion)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration target metadata does not match the declared target format."
                );
            }

            EnsureSecurityIdentityPreserved();
        }

        private void EnsureSecurityIdentityPreserved()
        {
            if (SourceMetadata.MetadataVersion != TargetMetadata.MetadataVersion ||
                SourceMetadata.EncryptionVersion != TargetMetadata.EncryptionVersion ||
                SourceMetadata.Mode != TargetMetadata.Mode ||
                !string.Equals(SourceMetadata.Protection, TargetMetadata.Protection, StringComparison.Ordinal) ||
                !string.Equals(SourceMetadata.ProtectedDatabaseKey, TargetMetadata.ProtectedDatabaseKey, StringComparison.Ordinal) ||
                !string.Equals(SourceMetadata.Kdf, TargetMetadata.Kdf, StringComparison.Ordinal) ||
                SourceMetadata.Iterations != TargetMetadata.Iterations ||
                !string.Equals(SourceMetadata.Salt, TargetMetadata.Salt, StringComparison.Ordinal))
            {
                throw new InvalidOperationException
                (
                    "Physical database storage migration must preserve the existing " +
                    "database security identity and change only the storage format."
                );
            }
        }
    }
}
