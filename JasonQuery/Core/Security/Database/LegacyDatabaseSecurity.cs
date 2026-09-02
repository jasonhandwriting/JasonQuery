using System;

namespace JasonQuery.Core.Security.Database
{
    public static class LegacyDatabaseSecurity
    {
        //Historical compatibility credential used by JasonQuery before Database Encryption V2.
        //This value is retained only for legacy database detection/migration.
        //It must never be used to create, normally open, or re-encrypt a V2 JasonQuery.db.
        internal const string LegacyDefaultDatabasePassword = "ytec1688";

        private const string LegacyCustomDatabasePasswordPrefix = "jasonquery1231";
        private const string LegacyCustomDatabasePasswordSuffix = "encryptDB!nf0";

        public static bool IsDefaultDatabasePassword(string databasePassword)
        {
            return string.Equals(databasePassword, LegacyDefaultDatabasePassword, StringComparison.Ordinal);
        }

        public static string CreateCustomDatabasePassword(string customPassword)
        {
            if (string.IsNullOrEmpty(customPassword))
            {
                throw new ArgumentException("A legacy custom database password is required.", nameof(customPassword));
            }

            return $"{LegacyCustomDatabasePasswordPrefix}{customPassword}{LegacyCustomDatabasePasswordSuffix}";
        }

        public static bool IsCustomDatabasePasswordMatch(string databasePassword, string customPassword)
        {
            if (string.IsNullOrEmpty(databasePassword) || string.IsNullOrEmpty(customPassword))
            {
                return false;
            }

            return string.Equals(databasePassword, CreateCustomDatabasePassword(customPassword), StringComparison.Ordinal);
        }
    }
}
