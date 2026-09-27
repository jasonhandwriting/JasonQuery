using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityBootstrapper
    {
        private readonly IJasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly IJasonQueryDbKeyProtector _databaseKeyProtector;

        public JasonQueryDbSecurityBootstrapper(IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            _metadataStore = metadataStore ?? throw new ArgumentNullException(nameof(metadataStore));
            _databaseKeyProtector = databaseKeyProtector ?? throw new ArgumentNullException(nameof(databaseKeyProtector));
        }

        public static JasonQueryDbSecurityBootstrapper CreateDefault(string applicationDirectory)
        {
            if (string.IsNullOrWhiteSpace(applicationDirectory))
            {
                throw new ArgumentException("An application directory is required.", nameof(applicationDirectory));
            }

            var metadataFilePath = Path.Combine(applicationDirectory, JasonQueryDbSecurityConstants.MetadataFileName);

            return new JasonQueryDbSecurityBootstrapper(new JasonQueryDbSecurityMetadataStore(metadataFilePath), new JasonQueryDbDpapiKeyProtector());
        }

        public JasonQueryDbSecurityBootstrapResult Resolve(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            if (!File.Exists(databaseFilePath))
            {
                return JasonQueryDbSecurityBootstrapResult.DatabaseMissing();
            }

            if (!_metadataStore.Exists)
            {
                return JasonQueryDbSecurityBootstrapResult.Legacy();
            }

            var metadata = _metadataStore.Load();

            switch (metadata.Mode)
            {
                case JasonQueryDbSecurityMode.WindowsCurrentUser:
                    {
                        return ResolveWindowsCurrentUser(metadata);
                    }
                case JasonQueryDbSecurityMode.CustomPassword:
                    {
                        return JasonQueryDbSecurityBootstrapResult.V2CustomPasswordRequired(metadata);
                    }
                default:
                    {
                        throw new InvalidDataException($"Database security mode '{metadata.Mode}' is not supported.");
                    }
            }
        }

        public JasonQueryDbSecurityBootstrapResult ResolveCustomPassword(JasonQueryDbSecurityMetadata metadata, string customPassword)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            metadata.Validate();

            if (metadata.Mode != JasonQueryDbSecurityMode.CustomPassword)
            {
                throw new InvalidOperationException("The database security metadata does not use a custom password.");
            }

            var salt = Convert.FromBase64String(metadata.Salt);

            try
            {
                var databasePassword = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabasePassword(customPassword, salt, metadata.Iterations);

                return JasonQueryDbSecurityBootstrapResult.V2Ready(metadata, databasePassword);
            }
            finally
            {
                Array.Clear(salt, 0, salt.Length);
            }
        }

        private JasonQueryDbSecurityBootstrapResult ResolveWindowsCurrentUser(JasonQueryDbSecurityMetadata metadata)
        {
            byte[] databaseKey = null;

            try
            {
                var protectedDatabaseKey = Convert.FromBase64String(metadata.ProtectedDatabaseKey);

                databaseKey = _databaseKeyProtector.Unprotect(protectedDatabaseKey);

                var databasePassword = JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey);

                return JasonQueryDbSecurityBootstrapResult.V2Ready(metadata, databasePassword);
            }
            catch (CryptographicException ex)
            {
                if (metadata.Recovery != null)
                {
                    return JasonQueryDbSecurityBootstrapResult.V2RecoveryRequired(metadata);
                }

                throw new JasonQueryDbSecurityStartupException
                (
                    JasonQueryDbSecurityStartupErrorKind.WindowsCurrentUserKeyUnavailable,
                    "The Windows-protected JasonQuery database key could not be unlocked. " +
                    "The database may belong to another Windows user profile or computer, " +
                    "or Windows may have been reinstalled.",
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
