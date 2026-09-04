using System;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityMigrationResult
    {
        public DatabaseSecurityMigrationResult(DatabaseSecurityMetadata metadata, string databasePassword)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A migrated database password is required.", nameof(databasePassword));
            }

            DatabasePassword = databasePassword;
        }

        public DatabaseSecurityMetadata Metadata { get; }

        public string DatabasePassword { get; }
    }
}
