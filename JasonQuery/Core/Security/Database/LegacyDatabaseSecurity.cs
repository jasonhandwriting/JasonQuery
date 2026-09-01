namespace JasonQuery.Core.Security.Database
{
    internal static class LegacyDatabaseSecurity
    {
        //Historical compatibility credential used by JasonQuery before Database Encryption V2.
        //This value is retained only for legacy database detection/migration.
        //It must never be used to create, normally open, or re-encrypt a V2 JasonQuery.db.
        internal const string LegacyDefaultDatabasePassword = "ytec1688";
    }
}
