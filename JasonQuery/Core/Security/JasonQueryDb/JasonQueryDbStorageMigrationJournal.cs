using Newtonsoft.Json;
using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Durable recovery record for physical Storage V1 -> V2 migration.
    ///
    /// This is intentionally separate from JasonQueryDbSecurityTransitionJournal.
    /// It tracks explicit cross-provider stages instead of probing providers/keys.
    /// Plaintext passwords, database keys, row payloads, and SQL/data dumps are
    /// never persisted in this journal.
    /// </summary>
    public sealed class JasonQueryDbStorageMigrationJournal
    {
        public const int CurrentVersion = 1;

        [JsonProperty("journalVersion")]
        public int JournalVersion { get; set; }

        [JsonProperty("operationId")]
        public string OperationId { get; set; }

        [JsonProperty("stage")]
        public JasonQueryDbStorageMigrationStage Stage { get; set; }

        [JsonProperty("protocolVersion")]
        public int ProtocolVersion { get; set; }

        [JsonProperty("candidateWriterProtocolVersion")]
        public int CandidateWriterProtocolVersion { get; set; }

        [JsonProperty("sourceStorageFormatVersion")]
        public int SourceStorageFormatVersion { get; set; }

        [JsonProperty("targetStorageFormatVersion")]
        public int TargetStorageFormatVersion { get; set; }

        [JsonProperty("sourceDatabaseSha256")]
        public string SourceDatabaseSha256 { get; set; }

        [JsonProperty("candidateDatabaseSha256", NullValueHandling = NullValueHandling.Ignore)]
        public string CandidateDatabaseSha256 { get; set; }

        [JsonProperty("sourceMetadataSha256")]
        public string SourceMetadataSha256 { get; set; }

        [JsonProperty("targetMetadataSha256")]
        public string TargetMetadataSha256 { get; set; }

        [JsonProperty("sourceMetadata")]
        public JasonQueryDbSecurityMetadata SourceMetadata { get; set; }

        [JsonProperty("targetMetadata")]
        public JasonQueryDbSecurityMetadata TargetMetadata { get; set; }

        public void Validate()
        {
            if (JournalVersion != CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Database storage-migration journal version '{JournalVersion}' is not supported."
                );
            }

            if (!Guid.TryParseExact(OperationId, "N", out var operationId) || operationId == Guid.Empty
                || !string.Equals(OperationId, operationId.ToString("N"), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Database storage-migration operationId is invalid.");
            }

            if (!Enum.IsDefined(typeof(JasonQueryDbStorageMigrationStage), Stage))
            {
                throw new NotSupportedException
                (
                    $"Database storage-migration stage '{(int)Stage}' is not supported."
                );
            }

            JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedVersion(ProtocolVersion);

            if (CandidateWriterProtocolVersion != JasonQueryDbStorageV2CandidateWriterProtocol.CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"Storage V2 candidate-writer protocol version '{CandidateWriterProtocolVersion}' is not supported."
                );
            }

            JasonQueryDbStorageMigrationProtocolContract.EnsureSupportedRoute
            (
                SourceStorageFormatVersion,
                TargetStorageFormatVersion
            );

            EnsureCanonicalSha256(SourceDatabaseSha256, "sourceDatabaseSha256");
            EnsureCanonicalSha256(SourceMetadataSha256, "sourceMetadataSha256");
            EnsureCanonicalSha256(TargetMetadataSha256, "targetMetadataSha256");

            if (Stage == JasonQueryDbStorageMigrationStage.PreparingCandidate)
            {
                if (CandidateDatabaseSha256 != null)
                {
                    throw new InvalidOperationException
                    (
                        "PreparingCandidate must not persist a candidate database hash before candidate validation is complete."
                    );
                }
            }
            else
            {
                EnsureCanonicalSha256(CandidateDatabaseSha256, "candidateDatabaseSha256");
            }

            if (SourceMetadata == null || TargetMetadata == null)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration metadata snapshots are incomplete."
                );
            }

            SourceMetadata.Validate();
            TargetMetadata.Validate();

            var resolvedSource = (int)JasonQueryDbStorageFormatContract.ResolveVersion(SourceMetadata.StorageFormatVersion);

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

            var resolvedTarget = (int)JasonQueryDbStorageFormatContract.ResolveVersion(TargetMetadata.StorageFormatVersion);

            if (resolvedTarget != TargetStorageFormatVersion)
            {
                throw new InvalidOperationException
                (
                    "Database storage-migration target metadata does not match the declared target format."
                );
            }

            EnsureSecurityIdentityPreserved();
        }

        private static void EnsureCanonicalSha256(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
            {
                throw new InvalidOperationException
                (
                    $"Database storage-migration {propertyName} must be a canonical SHA-256 value."
                );
            }

            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];
                var isDigit = character >= '0' && character <= '9';
                var isUpperHex = character >= 'A' && character <= 'F';

                if (!isDigit && !isUpperHex)
                {
                    throw new InvalidOperationException
                    (
                        $"Database storage-migration {propertyName} must use uppercase hexadecimal SHA-256 encoding."
                    );
                }
            }
        }

        private void EnsureSecurityIdentityPreserved()
        {
            if (SourceMetadata.MetadataVersion != TargetMetadata.MetadataVersion
                || SourceMetadata.EncryptionVersion != TargetMetadata.EncryptionVersion
                || SourceMetadata.Mode != TargetMetadata.Mode
                || !string.Equals(SourceMetadata.Protection, TargetMetadata.Protection, StringComparison.Ordinal)
                || !string.Equals(SourceMetadata.ProtectedDatabaseKey, TargetMetadata.ProtectedDatabaseKey, StringComparison.Ordinal)
                || !string.Equals(SourceMetadata.Kdf, TargetMetadata.Kdf, StringComparison.Ordinal)
                || SourceMetadata.Iterations != TargetMetadata.Iterations
                || !string.Equals(SourceMetadata.Salt, TargetMetadata.Salt, StringComparison.Ordinal)
                || !JasonQueryDbRecoveryMetadata.MetadataEquals(SourceMetadata.Recovery, TargetMetadata.Recovery))
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
