using System;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityBootstrapResult
    {
        private DatabaseSecurityBootstrapResult(DatabaseSecurityStartupState state, DatabaseSecurityMetadata metadata, string databasePassword)
        {
            State = state;
            Metadata = metadata;
            DatabasePassword = databasePassword;
        }

        public DatabaseSecurityStartupState State { get; }

        public DatabaseSecurityMetadata Metadata { get; }

        public string DatabasePassword { get; }

        public static DatabaseSecurityBootstrapResult DatabaseMissing()
        {
            return new DatabaseSecurityBootstrapResult(DatabaseSecurityStartupState.DatabaseMissing, null, null);
        }

        public static DatabaseSecurityBootstrapResult Legacy()
        {
            return new DatabaseSecurityBootstrapResult(DatabaseSecurityStartupState.Legacy, null, null);
        }

        public static DatabaseSecurityBootstrapResult V2Ready(DatabaseSecurityMetadata metadata, string databasePassword)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A V2 database password is required.", nameof(databasePassword));
            }

            return new DatabaseSecurityBootstrapResult(DatabaseSecurityStartupState.V2Ready, metadata, databasePassword);
        }

        public static DatabaseSecurityBootstrapResult V2CustomPasswordRequired(DatabaseSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            return new DatabaseSecurityBootstrapResult(DatabaseSecurityStartupState.V2CustomPasswordRequired, metadata, null);
        }
    }
}
