using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityBootstrapResult
    {
        private JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState state, JasonQueryDbSecurityMetadata metadata, string databasePassword)
        {
            State = state;
            Metadata = metadata;
            DatabasePassword = databasePassword;
        }

        public JasonQueryDbSecurityStartupState State { get; }

        public JasonQueryDbSecurityMetadata Metadata { get; }

        public string DatabasePassword { get; }

        public static JasonQueryDbSecurityBootstrapResult DatabaseMissing()
        {
            return new JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState.DatabaseMissing, null, null);
        }

        public static JasonQueryDbSecurityBootstrapResult Legacy()
        {
            return new JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState.Legacy, null, null);
        }

        public static JasonQueryDbSecurityBootstrapResult V2Ready(JasonQueryDbSecurityMetadata metadata, string databasePassword)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A V2 database password is required.", nameof(databasePassword));
            }

            return new JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState.V2Ready, metadata, databasePassword);
        }

        public static JasonQueryDbSecurityBootstrapResult V2RecoveryRequired(JasonQueryDbSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (metadata.Mode != JasonQueryDbSecurityMode.WindowsCurrentUser || metadata.Recovery == null)
            {
                throw new InvalidOperationException("Recovery-required startup needs WindowsCurrentUser metadata with a recovery envelope.");
            }

            return new JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState.V2RecoveryRequired, metadata, null);
        }

        public static JasonQueryDbSecurityBootstrapResult V2CustomPasswordRequired(JasonQueryDbSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            return new JasonQueryDbSecurityBootstrapResult(JasonQueryDbSecurityStartupState.V2CustomPasswordRequired, metadata, null);
        }
    }
}
