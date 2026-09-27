namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Production runtime family selected for the persisted JasonQuery.db
    /// physical storage format.
    ///
    /// This value identifies the runtime implementation family only. It is not
    /// persisted and must not be used as a replacement for storageFormatVersion.
    /// </summary>
    internal enum JasonQueryDbStorageRuntimeKind
    {
        LegacySystemDataSQLite = 1,
        ModernSqlCipher = 2
    }
}
