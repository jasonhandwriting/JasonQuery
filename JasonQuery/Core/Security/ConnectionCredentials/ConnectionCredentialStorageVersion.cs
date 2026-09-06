namespace JasonQuery.Core.Security.ConnectionCredentials
{
    /// <summary>
    /// Database-wide storage format used by DBInfo.Password.
    /// </summary>
    public enum ConnectionCredentialStorageVersion
    {
        /// <summary>
        /// Historical per-field protection based on the Windows DomainUser value.
        /// </summary>
        LegacyDomainUserProtected = 1,

        /// <summary>
        /// The logical password value is stored directly inside the
        /// Database Security V2-protected JasonQuery database.
        /// </summary>
        DatabaseProtectedLogicalValue = 2
    }
}
