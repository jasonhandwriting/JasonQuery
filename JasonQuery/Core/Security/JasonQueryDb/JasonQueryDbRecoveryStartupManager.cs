using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryStartupManager
    {
        private readonly IJasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly IJasonQueryDbKeyProtector _databaseKeyProtector;
        private readonly Func<JasonQueryDbSecurityMetadata, IJasonQueryDbMigrationDatabase> _databaseFactory;

        public JasonQueryDbRecoveryStartupManager(IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector,
                                                  Func<JasonQueryDbSecurityMetadata, IJasonQueryDbMigrationDatabase> databaseFactory)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
        }

        public JasonQueryDbSecurityBootstrapResult Recover(string databaseFilePath, string recoveryKeyText)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            var sourceMetadata = LoadRecoveryMetadata();
            byte[] databaseKey = null;
            byte[] reboundProtectedDatabaseKey = null;
            byte[] reboundVerificationKey = null;

            try
            {
                using (var recoveryKey = JasonQueryDbRecoveryKeyCodec.Decode(recoveryKeyText))
                {
                    var envelope = sourceMetadata.Recovery.ToEnvelope();

                    databaseKey = JasonQueryDbRecoveryEnvelopeProtector.Unprotect(envelope, recoveryKey);
                }

                var databasePassword = JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey);
                var database = _databaseFactory(sourceMetadata);

                if (database == null)
                {
                    throw new InvalidOperationException("The database-security runtime factory returned no database implementation.");
                }

                if (!database.CanOpen(databaseFilePath, databasePassword))
                {
                    throw new InvalidDataException
                    (
                        "The Recovery Key was accepted, but the recovered logical database key could not open JasonQuery.db."
                    );
                }

                var currentMetadata = LoadRecoveryMetadata();

                if (!MetadataEquals(sourceMetadata, currentMetadata))
                {
                    throw new InvalidOperationException
                    (
                        "Database security metadata changed during Recovery Key processing. No Windows protection rebind was committed."
                    );
                }

                reboundProtectedDatabaseKey = _databaseKeyProtector.Protect(databaseKey);

                if (reboundProtectedDatabaseKey == null || reboundProtectedDatabaseKey.Length == 0)
                {
                    throw new CryptographicException("Windows could not protect the recovered logical database key.");
                }

                reboundVerificationKey = _databaseKeyProtector.Unprotect(reboundProtectedDatabaseKey);

                if (!FixedTimeEquals(databaseKey, reboundVerificationKey))
                {
                    throw new CryptographicException
                    (
                        "The current Windows user protection could not reproduce the recovered logical database key."
                    );
                }

                var targetMetadata = CloneMetadata(sourceMetadata);

                targetMetadata.MetadataVersion = JasonQueryDbSecurityConstants.MetadataVersion;
                targetMetadata.ProtectedDatabaseKey = Convert.ToBase64String(reboundProtectedDatabaseKey);
                targetMetadata.Validate();

                _metadataStore.Save(targetMetadata);

                var committedMetadata = _metadataStore.Load();

                if (!MetadataEquals(targetMetadata, committedMetadata))
                {
                    throw new InvalidDataException("Recovery metadata validation failed after the Windows protection rebind.");
                }

                return JasonQueryDbSecurityBootstrapResult.V2Ready
                (
                    CloneMetadata(committedMetadata),
                    databasePassword
                );
            }
            finally
            {
                Clear(databaseKey);
                Clear(reboundProtectedDatabaseKey);
                Clear(reboundVerificationKey);
            }
        }

        private JasonQueryDbSecurityMetadata LoadRecoveryMetadata()
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
                    "Startup Recovery Key processing is available only for WindowsCurrentUser database security."
                );
            }

            if (metadata.Recovery == null)
            {
                throw new InvalidOperationException("No Recovery Key is configured for this JasonQuery.db.");
            }

            return metadata;
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

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;

            for (var index = 0; index < left.Length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
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
