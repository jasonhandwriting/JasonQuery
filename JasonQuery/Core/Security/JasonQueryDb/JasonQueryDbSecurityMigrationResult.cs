using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityMigrationResult
    {
        public JasonQueryDbSecurityMigrationResult(JasonQueryDbSecurityMetadata metadata, string databasePassword)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                throw new ArgumentException("A migrated database password is required.", nameof(databasePassword));
            }

            DatabasePassword = databasePassword;
        }

        public JasonQueryDbSecurityMetadata Metadata { get; }

        public string DatabasePassword { get; }
    }
}
