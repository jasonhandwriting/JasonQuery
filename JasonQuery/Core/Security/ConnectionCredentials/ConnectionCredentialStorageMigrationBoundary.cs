using System;
using System.Data;

namespace JasonQuery.Core.Security.ConnectionCredentials
{
    /// <summary>
    /// Owns the transaction boundary for a one-time Legacy V1 to V2
    /// connection credential migration.
    ///
    /// Credential values must be migrated by using the connection and
    /// transaction supplied to migrationWork. The V2 version marker is written
    /// only after migrationWork completes successfully and immediately before
    /// the transaction is committed.
    /// </summary>
    internal sealed class ConnectionCredentialStorageMigrationBoundary
    {
        private readonly IConnectionCredentialStorageVersionStore _versionStore;

        public ConnectionCredentialStorageMigrationBoundary(IConnectionCredentialStorageVersionStore versionStore)
        {
            _versionStore = versionStore ?? throw new ArgumentNullException(nameof(versionStore));
        }

        public void Execute(IDbConnection connection, Action<IDbConnection, IDbTransaction> migrationWork)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException
                (
                    "The connection credential migration database connection must already be open."
                );
            }

            if (migrationWork == null)
            {
                throw new ArgumentNullException(nameof(migrationWork));
            }

            var transaction = connection.BeginTransaction();

            if (transaction == null)
            {
                throw new InvalidOperationException
                (
                    "The connection credential migration transaction could not be created."
                );
            }

            using (transaction)
            {
                try
                {
                    var persistedVersion = _versionStore.ReadPersistedVersion(connection, transaction);

                    if (!ConnectionCredentialStorageContract.RequiresMigration(persistedVersion))
                    {
                        throw new InvalidOperationException
                        (
                            "Connection credentials are already stored using the V2 format. " +
                            "Legacy credential migration is not allowed."
                        );
                    }

                    migrationWork
                    (
                        connection,
                        transaction
                    );

                    //The version marker is intentionally the final database write before commit. Never mark V2 before all credential values have been migrated successfully.
                    _versionStore.WriteCurrentVersion
                    (
                        connection,
                        transaction
                    );

                    transaction.Commit();
                }
                catch (Exception migrationException)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackException)
                    {
                        throw new InvalidOperationException
                        (
                            "Connection credential migration failed, and its database transaction " +
                            "could not be rolled back safely.",
                            new AggregateException
                            (
                                migrationException,
                                rollbackException
                            )
                        );
                    }

                    throw;
                }
            }
        }
    }
}
