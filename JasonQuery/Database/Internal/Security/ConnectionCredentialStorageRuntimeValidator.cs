using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Database.Internal.Runtime;
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    /// <summary>
    /// Read-only provider-neutral validation used after Storage V2 is opened through the isolated Modern runtime.
    /// Migration of legacy DBInfo.Password values remains a Storage V1 operation and must complete before physical migration.
    /// </summary>
    internal static class ConnectionCredentialStorageRuntimeValidator
    {
        private const string VersionQuery = "SELECT [MetadataValue] FROM [JasonQuerySecurityMetadata] " + "WHERE [MetadataKey] = 'ConnectionCredentialStorageVersion'";

        public static void EnsureV2Ready(IJasonQueryDatabaseRuntime runtime, string connectionString, string databasePassword)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            var table = runtime.ExecuteQuery(connectionString, databasePassword, VersionQuery);

            if (table == null)
            {
                throw new InvalidDataException("The connection credential storage version query returned no table.");
            }

            if (table.Rows.Count != 1 || table.Columns.Count < 1)
            {
                throw new InvalidDataException("The connection credential storage version marker is missing or duplicated.");
            }

            var value = table.Rows[0][0];

            if (value == null || value == DBNull.Value)
            {
                throw new InvalidDataException("The connection credential storage version marker contains a NULL value.");
            }

            var persistedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            var persistedVersion = SqliteConnectionCredentialStorageVersionStore.ParsePersistedVersionValue(persistedValue);

            ConnectionCredentialStorageContract.EnsureV2Ready(persistedVersion);
        }
    }
}
