using JasonQuery.Core.Security.ConnectionCredentials;
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    internal sealed class SqliteConnectionCredentialStorageVersionStore : IConnectionCredentialStorageVersionStore
    {
        internal const string MetadataTableName = "JasonQuerySecurityMetadata";
        internal const string VersionMetadataKey = "ConnectionCredentialStorageVersion";

        private const string MetadataKeyColumn = "MetadataKey";
        private const string MetadataValueColumn = "MetadataValue";

        public int? ReadPersistedVersion(IDbConnection connection, IDbTransaction transaction)
        {
            ValidateConnection(connection);
            ValidateTransaction(connection, transaction);

            if (!MetadataTableExists(connection, transaction))
            {
                return null;
            }

            using (var command = CreateCommand(connection, transaction, $"SELECT [{MetadataValueColumn}] " + $"FROM [{MetadataTableName}] " + $"WHERE [{MetadataKeyColumn}] = @MetadataKey"))
            {
                AddParameter
                (
                    command,
                    "@MetadataKey",
                    VersionMetadataKey
                );

                var value = command.ExecuteScalar();

                if (value == null)
                {
                    return null;
                }

                if (value == DBNull.Value)
                {
                    throw new InvalidDataException
                    (
                        "The connection credential storage version marker contains a NULL value."
                    );
                }

                return ParsePersistedVersionValue
                (
                    Convert.ToString
                    (
                        value,
                        CultureInfo.InvariantCulture
                    )
                );
            }
        }

        public void WriteCurrentVersion(IDbConnection connection, IDbTransaction transaction)
        {
            ValidateConnection(connection);

            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            ValidateTransaction(connection, transaction);
            EnsureMetadataTable(connection, transaction);

            var currentVersionValue = ConnectionCredentialStorageContract.CurrentVersion.ToString(CultureInfo.InvariantCulture);
            int updatedRows;

            using (var command = CreateCommand(connection, transaction, $"UPDATE [{MetadataTableName}] " + $"SET [{MetadataValueColumn}] = @MetadataValue " + $"WHERE [{MetadataKeyColumn}] = @MetadataKey"))
            {
                AddParameter
                (
                    command,
                    "@MetadataValue",
                    currentVersionValue
                );

                AddParameter
                (
                    command,
                    "@MetadataKey",
                    VersionMetadataKey
                );

                updatedRows = command.ExecuteNonQuery();
            }

            if (updatedRows > 1)
            {
                throw new InvalidDataException
                (
                    "Multiple connection credential storage version markers were updated."
                );
            }

            if (updatedRows == 1)
            {
                return;
            }

            using (var command = CreateCommand(connection, transaction, $"INSERT INTO [{MetadataTableName}] " + $"([{MetadataKeyColumn}], [{MetadataValueColumn}]) " + "VALUES (@MetadataKey, @MetadataValue)"))
            {
                AddParameter
                (
                    command,
                    "@MetadataKey",
                    VersionMetadataKey
                );

                AddParameter
                (
                    command,
                    "@MetadataValue",
                    currentVersionValue
                );

                var insertedRows = command.ExecuteNonQuery();

                if (insertedRows != 1)
                {
                    throw new InvalidDataException
                    (
                        "The connection credential storage version marker could not be persisted."
                    );
                }
            }
        }

        internal static int ParsePersistedVersionValue(string persistedValue)
        {
            if (string.IsNullOrEmpty(persistedValue))
            {
                throw new InvalidDataException
                (
                    "The connection credential storage version marker is empty."
                );
            }

            if (!int.TryParse(persistedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version))
            {
                throw new InvalidDataException
                (
                    "The connection credential storage version marker is not a valid integer."
                );
            }

            var canonicalValue = version.ToString(CultureInfo.InvariantCulture);

            if (!string.Equals(persistedValue, canonicalValue, StringComparison.Ordinal))
            {
                throw new InvalidDataException
                (
                    "The connection credential storage version marker is not in canonical form."
                );
            }

            ConnectionCredentialStorageContract.ResolveVersion(version);
            return version;
        }

        private static bool MetadataTableExists(IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = CreateCommand(connection, transaction, "SELECT 1 " + "FROM sqlite_master " + "WHERE type = 'table' " + "AND name = @TableName " + "LIMIT 1"))
            {
                AddParameter
                (
                    command,
                    "@TableName",
                    MetadataTableName
                );

                return command.ExecuteScalar() != null;
            }
        }

        private static void EnsureMetadataTable(IDbConnection connection, IDbTransaction transaction)
        {
            using (var command = CreateCommand(connection, transaction, $"CREATE TABLE IF NOT EXISTS [{MetadataTableName}] " + "(" + $"[{MetadataKeyColumn}] TEXT NOT NULL PRIMARY KEY, " + $"[{MetadataValueColumn}] TEXT NOT NULL" + ")"))
            {
                command.ExecuteNonQuery();
            }
        }

        private static IDbCommand CreateCommand(IDbConnection connection, IDbTransaction transaction, string commandText)
        {
            var command = connection.CreateCommand();

            if (command == null)
            {
                throw new InvalidOperationException
                (
                    "The SQLite metadata command could not be created."
                );
            }

            command.CommandText = commandText;
            command.Transaction = transaction;

            return command;
        }

        private static void AddParameter(IDbCommand command, string parameterName, object value)
        {
            var parameter = command.CreateParameter();

            if (parameter == null)
            {
                throw new InvalidOperationException
                (
                    "The SQLite metadata parameter could not be created."
                );
            }

            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static void ValidateConnection(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException
                (
                    "The JasonQuery security metadata database connection must be open."
                );
            }
        }

        private static void ValidateTransaction(IDbConnection connection, IDbTransaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            if (!ReferenceEquals(transaction.Connection, connection))
            {
                throw new InvalidOperationException
                (
                    "The security metadata transaction does not belong to the supplied database connection."
                );
            }
        }
    }

    /// <summary>
    /// Startup gate for DBInfo.Password storage semantics.
    /// The supplied JasonQuery.db connection must already be unlocked, validated,
    /// and open. Runtime credential consumers are allowed to proceed only after
    /// this gate confirms that the database-wide storage marker is V2.
    /// </summary>
    internal sealed class SqliteConnectionCredentialStorageStartupGate
    {
        private readonly IConnectionCredentialStorageVersionStore _versionStore;
        private readonly SqliteLegacyConnectionCredentialMigrator _migrator;

        public SqliteConnectionCredentialStorageStartupGate() : this(new SqliteConnectionCredentialStorageVersionStore())
        {
        }

        internal SqliteConnectionCredentialStorageStartupGate(IConnectionCredentialStorageVersionStore versionStore)
        {
            _versionStore = versionStore ?? throw new ArgumentNullException(nameof(versionStore));
            _migrator = new SqliteLegacyConnectionCredentialMigrator(_versionStore);
        }

        public bool EnsureReady(IDbConnection connection)
        {
            var migrated = _migrator.MigrateIfRequired(connection);
            var persistedVersion = _versionStore.ReadPersistedVersion(connection, null);

            ConnectionCredentialStorageContract.EnsureV2Ready(persistedVersion);
            return migrated;
        }
    }
}
