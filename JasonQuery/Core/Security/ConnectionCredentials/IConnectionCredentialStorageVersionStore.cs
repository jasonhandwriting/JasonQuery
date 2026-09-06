using System.Data;

namespace JasonQuery.Core.Security.ConnectionCredentials
{
    /// <summary>
    /// Persists the database-wide DBInfo.Password storage version.
    /// A missing persisted value represents the historical Legacy V1 format.
    /// </summary>
    internal interface IConnectionCredentialStorageVersionStore
    {
        /// <summary>
        /// Reads the persisted storage version.
        /// Returns null when the metadata table or version marker does not exist.
        /// </summary>
        int? ReadPersistedVersion
        (
            IDbConnection connection,
            IDbTransaction transaction
        );

        /// <summary>
        /// Persists the current V2 storage version inside the supplied transaction.
        /// </summary>
        void WriteCurrentVersion
        (
            IDbConnection connection,
            IDbTransaction transaction
        );
    }
}
