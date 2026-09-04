using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityBootstrapper
    {
        private readonly IDatabaseSecurityMetadataStore _metadataStore;
        private readonly IDatabaseKeyProtector _databaseKeyProtector;

        public DatabaseSecurityBootstrapper(IDatabaseSecurityMetadataStore metadataStore, IDatabaseKeyProtector databaseKeyProtector)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
        }

        public static DatabaseSecurityBootstrapper CreateDefault(string applicationDirectory)
        {
            if (string.IsNullOrWhiteSpace(applicationDirectory))
            {
                throw new ArgumentException("An application directory is required.", nameof(applicationDirectory));
            }

            var metadataFilePath = Path.Combine(applicationDirectory, DatabaseSecurityConstants.MetadataFileName);

            return new DatabaseSecurityBootstrapper(new DatabaseSecurityMetadataStore(metadataFilePath), new DpapiDatabaseKeyProtector());
        }

        public DatabaseSecurityBootstrapResult Resolve(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            if (!File.Exists(databaseFilePath))
            {
                return DatabaseSecurityBootstrapResult.DatabaseMissing();
            }

            if (!_metadataStore.Exists)
            {
                return DatabaseSecurityBootstrapResult.Legacy();
            }

            var metadata = _metadataStore.Load();

            switch (metadata.Mode)
            {
                case DatabaseSecurityMode.WindowsCurrentUser:
                    {
                        return ResolveWindowsCurrentUser(metadata);
                    }
                case DatabaseSecurityMode.CustomPassword:
                    {
                        return DatabaseSecurityBootstrapResult.V2CustomPasswordRequired(metadata);
                    }
                default:
                    {
                        throw new InvalidDataException($"Database security mode '{metadata.Mode}' is not supported.");
                    }
            }
        }

        public DatabaseSecurityBootstrapResult ResolveCustomPassword(DatabaseSecurityMetadata metadata, string customPassword)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            metadata.Validate();

            if (metadata.Mode != DatabaseSecurityMode.CustomPassword)
            {
                throw new InvalidOperationException("The database security metadata does not use a custom password.");
            }

            var salt = Convert.FromBase64String(metadata.Salt);

            try
            {
                var databasePassword = CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword(customPassword, salt, metadata.Iterations);

                return DatabaseSecurityBootstrapResult.V2Ready(metadata, databasePassword);
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);
            }
        }

        private DatabaseSecurityBootstrapResult ResolveWindowsCurrentUser(DatabaseSecurityMetadata metadata)
        {
            byte[] databaseKey = null;

            try
            {
                var protectedDatabaseKey = Convert.FromBase64String(metadata.ProtectedDatabaseKey);

                databaseKey = _databaseKeyProtector.Unprotect(protectedDatabaseKey);

                var databasePassword = DatabaseKeyGenerator.ToDatabasePassword(databaseKey);

                return DatabaseSecurityBootstrapResult.V2Ready(metadata, databasePassword);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidDataException
                (
                    "The Windows-protected JasonQuery database key could not be unlocked. " +
                    "The database may belong to another Windows user or computer.",
                    ex
                );
            }
            finally
            {
                if (databaseKey != null)
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }
            }
        }
    }
}
