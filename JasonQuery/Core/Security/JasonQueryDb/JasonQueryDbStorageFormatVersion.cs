namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Persisted physical storage format used by JasonQuery.db.
    ///
    /// This version is independent from Database Security metadataVersion
    /// and encryptionVersion.
    /// </summary>
    public enum JasonQueryDbStorageFormatVersion
    {
        /// <summary>
        /// Historical System.Data.SQLite 1.x storage using the legacy
        /// CryptoAPI-compatible encrypted SQLite codec characterized by
        /// JasonQuery.
        /// </summary>
        LegacySystemDataSQLiteCryptoApi = 1,

        /// <summary>
        /// Modern encrypted SQLite storage using the SQLCipher 4
        /// on-disk compatibility family.
        ///
        /// The persisted value identifies the storage-format family.
        /// SQLCipher, SQLite, OpenSSL, and managed-provider patch versions
        /// are implementation details and are not encoded in this value.
        /// </summary>
        SqlCipherCompatibility4 = 2
    }
}
