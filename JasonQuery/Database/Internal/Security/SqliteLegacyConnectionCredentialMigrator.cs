using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Core.Security.Legacy;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    /// <summary>
    /// Converts historical DBInfo.Password values to the V2 logical-value
    /// storage contract.
    ///
    /// This class implements the one-time migration engine only. Runtime
    /// activation is intentionally deferred until the credential save/load
    /// call sites are switched to V2 semantics.
    /// </summary>
    internal sealed class SqliteLegacyConnectionCredentialMigrator
    {
        internal const string DbInfoTableName = "DBInfo";

        internal const string PidColumnName = "PID";

        internal const string DomainUserColumnName = "DomainUser";

        internal const string PasswordColumnName = "Password";

        private readonly IConnectionCredentialStorageVersionStore _versionStore;

        private readonly ConnectionCredentialStorageMigrationBoundary _migrationBoundary;

        public SqliteLegacyConnectionCredentialMigrator() : this(new SqliteConnectionCredentialStorageVersionStore())
        {
        }

        internal SqliteLegacyConnectionCredentialMigrator(IConnectionCredentialStorageVersionStore versionStore)
        {
            _versionStore = versionStore ?? throw new ArgumentNullException(nameof(versionStore));

            _migrationBoundary = new ConnectionCredentialStorageMigrationBoundary(_versionStore);
        }

        /// <summary>
        /// Migrates every legacy DBInfo credential in the database.
        ///
        /// Returns true when a Legacy V1 database was migrated, including a
        /// database with zero saved credentials. Returns false when the database
        /// is already V2.
        /// </summary>
        public bool MigrateIfRequired(IDbConnection connection)
        {
            ValidateConnection(connection);

            var persistedVersion = _versionStore.ReadPersistedVersion(connection, null);
            var storageVersion = ConnectionCredentialStorageContract.ResolveVersion(persistedVersion);

            if (storageVersion == ConnectionCredentialStorageVersion.DatabaseProtectedLogicalValue)
            {
                return false;
            }

            //The boundary re-reads the version inside the transaction before any credential is changed. All row updates and the final V2 marker therefore share one transaction.
            _migrationBoundary.Execute
            (
                connection,
                MigrateLegacyRows
            );

            return true;
        }

        private static void MigrateLegacyRows(IDbConnection connection, IDbTransaction transaction)
        {
            var legacyRows = ReadLegacyRows(connection, transaction);

            foreach (var row in legacyRows)
            {
                if (string.IsNullOrEmpty(row.ProtectedPassword))
                {
                    var canonicalV2StoredValue = ConnectionCredentialStorageContract.ToV2StoredValue(row.ProtectedPassword);

                    //V2 uses one canonical representation for "no saved password": string.Empty. A historical NULL therefore requires an UPDATE, while an existing empty string does not.
                    if (!string.Equals(row.ProtectedPassword, canonicalV2StoredValue, StringComparison.Ordinal))
                    {
                        UpdatePassword
                        (
                            connection,
                            transaction,
                            row.Pid,
                            canonicalV2StoredValue
                        );
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.DomainUser))
                {
                    throw new InvalidDataException
                    (
                        $"DBInfo PID '{FormatPid(row.Pid)}' contains a saved " +
                        "legacy credential but has no DomainUser value."
                    );
                }

                var logicalPassword = UnprotectAndValidateLegacyCredential(row);
                var v2StoredValue = ConnectionCredentialStorageContract.ToV2StoredValue(logicalPassword);

                UpdatePassword
                (
                    connection,
                    transaction,
                    row.Pid,
                    v2StoredValue
                );
            }
        }

        private static string UnprotectAndValidateLegacyCredential(LegacyCredentialRow row)
        {
            string logicalPassword;

            try
            {
                logicalPassword = LegacyConnectionCredentialSecurity.Unprotect(row.ProtectedPassword, row.DomainUser);

                var canonicalProtectedPassword = LegacyConnectionCredentialSecurity.Protect(logicalPassword, row.DomainUser);

                if (!string.Equals(row.ProtectedPassword, canonicalProtectedPassword, StringComparison.Ordinal))
                {
                    throw new InvalidDataException
                    (
                        $"The legacy connection credential for DBInfo PID " +
                        $"'{FormatPid(row.Pid)}' is invalid or cannot be " +
                        "recovered using its stored DomainUser."
                    );
                }

                return logicalPassword;
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new InvalidDataException
                (
                    $"The legacy connection credential for DBInfo PID " +
                    $"'{FormatPid(row.Pid)}' could not be migrated.",
                    exception
                );
            }
        }

        private static List<LegacyCredentialRow> ReadLegacyRows(IDbConnection connection, IDbTransaction transaction)
        {
            var rows = new List<LegacyCredentialRow>();

            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    $"SELECT [{PidColumnName}], " +
                    $"[{DomainUserColumnName}], " +
                    $"[{PasswordColumnName}] " +
                    $"FROM [{DbInfoTableName}] " +
                    $"ORDER BY [{PidColumnName}]"
                )
            )
            using (var reader = command.ExecuteReader())
            {
                var pidOrdinal = reader.GetOrdinal(PidColumnName);
                var domainUserOrdinal = reader.GetOrdinal(DomainUserColumnName);
                var passwordOrdinal = reader.GetOrdinal(PasswordColumnName);

                while (reader.Read())
                {
                    var pid = reader.GetValue(pidOrdinal);

                    if (pid == null || pid == DBNull.Value)
                    {
                        throw new InvalidDataException
                        (
                            "A DBInfo row contains a NULL PID and cannot be migrated safely."
                        );
                    }

                    var domainUser = reader.IsDBNull(domainUserOrdinal) ? null : Convert.ToString(reader.GetValue(domainUserOrdinal), CultureInfo.InvariantCulture);
                    var protectedPassword = reader.IsDBNull(passwordOrdinal) ? null : Convert.ToString(reader.GetValue(passwordOrdinal), CultureInfo.InvariantCulture);

                    rows.Add
                    (
                        new LegacyCredentialRow
                        (
                            pid,
                            domainUser,
                            protectedPassword
                        )
                    );
                }
            }

            return rows;
        }

        private static void UpdatePassword(IDbConnection connection, IDbTransaction transaction, object pid, string password)
        {
            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    $"UPDATE [{DbInfoTableName}] " +
                    $"SET [{PasswordColumnName}] = @Password " +
                    $"WHERE [{PidColumnName}] = @Pid"
                )
            )
            {
                AddParameter
                (
                    command,
                    "@Password",
                    password
                );

                AddParameter
                (
                    command,
                    "@Pid",
                    pid
                );

                var affectedRows = command.ExecuteNonQuery();

                if (affectedRows != 1)
                {
                    throw new InvalidDataException
                    (
                        $"Credential migration for DBInfo PID " +
                        $"'{FormatPid(pid)}' affected {affectedRows} rows; " +
                        "exactly one row was required."
                    );
                }
            }
        }

        private static IDbCommand CreateCommand(IDbConnection connection, IDbTransaction transaction, string commandText)
        {
            var command = connection.CreateCommand();

            if (command == null)
            {
                throw new InvalidOperationException
                (
                    "The legacy credential migration command could not be created."
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
                    "The legacy credential migration parameter could not be created."
                );
            }

            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;

            command.Parameters.Add(parameter);
        }

        private static string FormatPid(object pid)
        {
            return Convert.ToString
            (
                pid,
                CultureInfo.InvariantCulture
            );
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
                    "The legacy connection credential migration database " +
                    "connection must already be open."
                );
            }
        }

        private sealed class LegacyCredentialRow
        {
            public LegacyCredentialRow(object pid, string domainUser, string protectedPassword)
            {
                Pid = pid;
                DomainUser = domainUser;
                ProtectedPassword = protectedPassword;
            }

            public object Pid { get; }

            public string DomainUser { get; }

            public string ProtectedPassword { get; }
        }
    }
}
