namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Persisted physical storage format used by JasonQuery.db.
    ///
    /// This version is independent from Database Security metadataVersion
    /// and encryptionVersion.
    /// </summary>
    public enum DatabaseStorageFormatVersion
    {
        /// <summary>
        /// Historical System.Data.SQLite 1.x storage using the legacy
        /// CryptoAPI-compatible encrypted SQLite codec characterized by
        /// JasonQuery.
        /// </summary>
        LegacySystemDataSQLiteCryptoApi = 1
    }
}
