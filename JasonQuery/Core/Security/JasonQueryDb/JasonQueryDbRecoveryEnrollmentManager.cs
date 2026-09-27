using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryEnrollmentManager
    {
        private readonly Guid _managerId = Guid.NewGuid();
        private readonly IJasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly IJasonQueryDbKeyProtector _databaseKeyProtector;
        private readonly Func<JasonQueryDbSecurityMetadata, IJasonQueryDbMigrationDatabase> _databaseFactory;

        public JasonQueryDbRecoveryEnrollmentManager(IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector,
                                                     Func<JasonQueryDbSecurityMetadata, IJasonQueryDbMigrationDatabase> databaseFactory)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
        }

        public bool IsRecoveryConfigured()
        {
            var metadata = LoadWindowsMetadata();

            return metadata.Recovery != null;
        }

        public JasonQueryDbRecoveryEnrollmentDraft PrepareEnrollment(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            var sourceMetadata = LoadWindowsMetadata();
            byte[] databaseKey = null;

            try
            {
                databaseKey = ResolveAndVerifyDatabaseKey(databaseFilePath, sourceMetadata);

                using (var recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate())
                {
                    var recoveryKeyText = JasonQueryDbRecoveryKeyCodec.Encode(recoveryKey);
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var targetMetadata = CloneMetadata(sourceMetadata);

                    targetMetadata.MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion;
                    targetMetadata.Recovery = JasonQueryDbRecoveryMetadata.Create(envelope);
                    targetMetadata.Validate();

                    return new JasonQueryDbRecoveryEnrollmentDraft
                    (
                        _managerId,
                        CloneMetadata(sourceMetadata),
                        targetMetadata,
                        recoveryKeyText
                    );
                }
            }
            finally
            {
                Clear(databaseKey);
            }
        }

        public JasonQueryDbSecurityMetadata CommitEnrollment(string databaseFilePath, JasonQueryDbRecoveryEnrollmentDraft draft)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            ValidateDraft(draft);

            var currentMetadata = LoadWindowsMetadata();

            if (!MetadataEquals(currentMetadata, draft.SourceMetadata))
            {
                throw new InvalidOperationException
                (
                    "Database security metadata changed while the Recovery Key was being saved. " +
                    "No recovery enrollment was committed."
                );
            }

            byte[] databaseKey = null;

            try
            {
                databaseKey = ResolveAndVerifyDatabaseKey(databaseFilePath, currentMetadata);
            }
            finally
            {
                Clear(databaseKey);
            }

            var targetMetadata = CloneMetadata(draft.TargetMetadata);

            targetMetadata.Validate();
            _metadataStore.Save(targetMetadata);

            var committed = _metadataStore.Load();

            if (!MetadataEquals(committed, targetMetadata))
            {
                throw new InvalidDataException("Recovery enrollment metadata validation failed after save.");
            }

            return CloneMetadata(committed);
        }

        public JasonQueryDbSecurityMetadata DisableRecovery(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            var currentMetadata = LoadWindowsMetadata();

            if (currentMetadata.Recovery == null)
            {
                throw new InvalidOperationException("No Recovery Key is currently configured.");
            }

            byte[] databaseKey = null;

            try
            {
                databaseKey = ResolveAndVerifyDatabaseKey(databaseFilePath, currentMetadata);
            }
            finally
            {
                Clear(databaseKey);
            }

            var targetMetadata = CloneMetadata(currentMetadata);

            targetMetadata.MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion;
            targetMetadata.Recovery = null;
            targetMetadata.Validate();

            _metadataStore.Save(targetMetadata);

            var committed = _metadataStore.Load();

            if (!MetadataEquals(committed, targetMetadata))
            {
                throw new InvalidDataException("Recovery disable metadata validation failed after save.");
            }

            return CloneMetadata(committed);
        }

        private JasonQueryDbSecurityMetadata LoadWindowsMetadata()
        {
            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database security metadata is missing.");
            }

            var metadata = _metadataStore.Load();

            metadata.Validate();

            if (metadata.Mode != JasonQueryDbSecurityMode.WindowsCurrentUser)
            {
                throw new InvalidOperationException
                (
                    "Recovery Key enrollment is available only when JasonQuery.db uses Windows Protected mode."
                );
            }

            return metadata;
        }

        private byte[] ResolveAndVerifyDatabaseKey(string databaseFilePath, JasonQueryDbSecurityMetadata metadata)
        {
            byte[] protectedDatabaseKey = null;
            byte[] databaseKey = null;

            try
            {
                protectedDatabaseKey = Convert.FromBase64String(metadata.ProtectedDatabaseKey);
                databaseKey = _databaseKeyProtector.Unprotect(protectedDatabaseKey);

                if (databaseKey == null || databaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes)
                {
                    throw new InvalidDataException
                    (
                        $"The Windows-protected logical database key must contain {JasonQueryDbSecurityConstants.DatabaseKeySizeBytes} bytes."
                    );
                }

                var databasePassword = JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey);
                var database = _databaseFactory(metadata);

                if (database == null)
                {
                    throw new InvalidOperationException("The database-security runtime factory returned no database implementation.");
                }

                if (!database.CanOpen(databaseFilePath, databasePassword))
                {
                    throw new InvalidDataException
                    (
                        "JasonQuery.db could not be validated with the current Windows-protected logical database key."
                    );
                }

                var result = databaseKey;
                databaseKey = null;
                return result;
            }
            finally
            {
                Clear(protectedDatabaseKey);
                Clear(databaseKey);
            }
        }

        private void ValidateDraft(JasonQueryDbRecoveryEnrollmentDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            if (draft.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(JasonQueryDbRecoveryEnrollmentDraft));
            }

            if (draft.ManagerId != _managerId)
            {
                throw new InvalidOperationException("The recovery enrollment draft belongs to a different manager instance.");
            }

            if (draft.TargetMetadata.Recovery == null)
            {
                throw new InvalidDataException("The recovery enrollment draft does not contain a recovery envelope.");
            }

            draft.SourceMetadata.Validate();
            draft.TargetMetadata.Validate();
        }

        private static JasonQueryDbSecurityMetadata CloneMetadata(JasonQueryDbSecurityMetadata source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new JasonQueryDbSecurityMetadata
            {
                MetadataVersion = source.MetadataVersion,
                EncryptionVersion = source.EncryptionVersion,
                StorageFormatVersion = source.StorageFormatVersion,
                Mode = source.Mode,
                Protection = source.Protection,
                ProtectedDatabaseKey = source.ProtectedDatabaseKey,
                Kdf = source.Kdf,
                Iterations = source.Iterations,
                Salt = source.Salt,
                Recovery = JasonQueryDbRecoveryMetadata.Clone(source.Recovery)
            };
        }

        private static bool MetadataEquals(JasonQueryDbSecurityMetadata left, JasonQueryDbSecurityMetadata right)
        {
            if (left == null || right == null)
            {
                return left == right;
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

        private static void ValidateDatabaseFilePath(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            if (!File.Exists(databaseFilePath))
            {
                throw new FileNotFoundException("JasonQuery.db was not found.", databaseFilePath);
            }
        }

        private static void Clear(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }
    }
}
