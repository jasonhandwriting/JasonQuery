using System;
using System.IO;

namespace JasonQuery.Core.Security.Database
{
    public sealed class FreshDatabaseSecurityInitializer
    {
        private const string CandidateFileSuffix = ".database-encryption-v2-fresh-candidate";

        private readonly IDatabaseSecurityMetadataStore _metadataStore;
        private readonly IDatabaseKeyProtector _databaseKeyProtector;
        private readonly IDatabaseSecurityFreshInstallDatabase _freshInstallDatabase;

        public FreshDatabaseSecurityInitializer(IDatabaseSecurityMetadataStore metadataStore, IDatabaseKeyProtector databaseKeyProtector,
                                                IDatabaseSecurityFreshInstallDatabase freshInstallDatabase)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
            _freshInstallDatabase = freshInstallDatabase ?? throw new ArgumentNullException(nameof(freshInstallDatabase));
        }

        public static string GetCandidateFilePath(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);
            return Path.GetFullPath(databaseFilePath) + CandidateFileSuffix;
        }

        public bool RecoverInterruptedInitializationIfNeeded(string databaseFilePath)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (File.Exists(databaseFilePath))
            {
                return false;
            }

            var candidateFilePath = GetCandidateFilePath(databaseFilePath);

            if (!File.Exists(candidateFilePath))
            {
                if (_metadataStore.Exists)
                {
                    throw new InvalidDataException
                    (
                        "Database security metadata exists, but JasonQuery.db is missing and no recoverable fresh-install candidate exists."
                    );
                }

                return false;
            }

            if (!_metadataStore.Exists)
            {
                File.Delete(candidateFilePath);
                return false;
            }

            var metadata = _metadataStore.Load();

            if (metadata.Mode != DatabaseSecurityMode.WindowsCurrentUser)
            {
                throw new InvalidDataException
                (
                    "A fresh-install database candidate exists, but its security metadata does not use Windows Current User protection."
                );
            }

            var databasePassword = ResolveWindowsCurrentUserDatabasePassword(metadata);

            ValidateEncryptedDatabase(candidateFilePath, databasePassword);
            File.Move(candidateFilePath, databaseFilePath);
            ValidateEncryptedDatabase(databaseFilePath, databasePassword);

            return true;
        }

        public DatabaseSecurityBootstrapResult InitializeWindowsCurrentUser(string databaseFilePath, Stream plaintextTemplateStream)
        {
            ValidateDatabaseFilePath(databaseFilePath);

            if (plaintextTemplateStream == null)
            {
                throw new ArgumentNullException(nameof(plaintextTemplateStream));
            }

            if (!plaintextTemplateStream.CanRead)
            {
                throw new ArgumentException("The JasonQuery database template stream must be readable.", nameof(plaintextTemplateStream));
            }

            if (File.Exists(databaseFilePath))
            {
                throw new InvalidOperationException("JasonQuery.db already exists. Fresh database initialization cannot run.");
            }

            if (_metadataStore.Exists)
            {
                throw new InvalidOperationException("Database security metadata already exists. Fresh database initialization cannot run.");
            }

            var candidateFilePath = GetCandidateFilePath(databaseFilePath);

            if (File.Exists(candidateFilePath))
            {
                throw new InvalidDataException
                (
                    "A previous fresh-install database candidate still exists. Interrupted initialization recovery must run first."
                );
            }

            byte[] databaseKey = null;
            byte[] protectedDatabaseKey = null;
            var databaseFinalizedByThisAttempt = false;

            try
            {
                databaseKey = DatabaseKeyGenerator.Generate();

                var databasePassword = DatabaseKeyGenerator.ToDatabasePassword(databaseKey);

                protectedDatabaseKey = _databaseKeyProtector.Protect(databaseKey);

                var metadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(protectedDatabaseKey)
                );

                _freshInstallDatabase.CreateEncryptedDatabase
                (
                    plaintextTemplateStream,
                    candidateFilePath,
                    databasePassword
                );

                ValidateEncryptedDatabase(candidateFilePath, databasePassword);

                _metadataStore.Save(metadata);
                ValidateCommittedMetadata(metadata);

                File.Move(candidateFilePath, databaseFilePath);
                databaseFinalizedByThisAttempt = true;

                ValidateEncryptedDatabase(databaseFilePath, databasePassword);

                return DatabaseSecurityBootstrapResult.V2Ready(metadata, databasePassword);
            }
            catch (Exception initializationException)
            {
                try
                {
                    if (databaseFinalizedByThisAttempt && File.Exists(databaseFilePath))
                    {
                        File.Delete(databaseFilePath);
                    }

                    DeleteFileIfExists(candidateFilePath);

                    if (_metadataStore.Exists)
                    {
                        _metadataStore.Delete();
                    }
                }
                catch (Exception cleanupException)
                {
                    throw new InvalidDataException
                    (
                        "Fresh JasonQuery.db initialization failed, and its incomplete files could not be cleaned up safely.",
                        new AggregateException(initializationException, cleanupException)
                    );
                }

                throw new InvalidDataException
                (
                    "Fresh JasonQuery.db initialization failed and was rolled back.",
                    initializationException
                );
            }
            finally
            {
                Clear(databaseKey);
                Clear(protectedDatabaseKey);
            }
        }

        private string ResolveWindowsCurrentUserDatabasePassword(DatabaseSecurityMetadata metadata)
        {
            byte[] protectedDatabaseKey = null;
            byte[] databaseKey = null;

            try
            {
                protectedDatabaseKey = Convert.FromBase64String(metadata.ProtectedDatabaseKey);
                databaseKey = _databaseKeyProtector.Unprotect(protectedDatabaseKey);

                return DatabaseKeyGenerator.ToDatabasePassword(databaseKey);
            }
            finally
            {
                Clear(protectedDatabaseKey);
                Clear(databaseKey);
            }
        }

        private void ValidateEncryptedDatabase(string databaseFilePath, string databasePassword)
        {
            if (!_freshInstallDatabase.CanOpen(databaseFilePath, databasePassword))
            {
                throw new InvalidDataException
                (
                    "The fresh JasonQuery.db could not be opened with its generated database key."
                );
            }

            if (_freshInstallDatabase.CanOpenWithoutPassword(databaseFilePath))
            {
                throw new InvalidDataException
                (
                    "The fresh JasonQuery.db is still accessible without its generated database key."
                );
            }
        }

        private void ValidateCommittedMetadata(DatabaseSecurityMetadata expectedMetadata)
        {
            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException("Database security metadata was not finalized for the fresh JasonQuery.db.");
            }

            var savedMetadata = _metadataStore.Load();

            if (savedMetadata.Mode != DatabaseSecurityMode.WindowsCurrentUser || savedMetadata.MetadataVersion != expectedMetadata.MetadataVersion
                || savedMetadata.EncryptionVersion != expectedMetadata.EncryptionVersion
                || !string.Equals(savedMetadata.Protection, expectedMetadata.Protection, StringComparison.Ordinal)
                || !string.Equals(savedMetadata.ProtectedDatabaseKey, expectedMetadata.ProtectedDatabaseKey, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Database security metadata validation failed after fresh database initialization.");
            }
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void Clear(byte[] value)
        {
            if (value != null)
            {
                Array.Clear(value, 0, value.Length);
            }
        }

        private static void ValidateDatabaseFilePath(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }
        }
    }
}
