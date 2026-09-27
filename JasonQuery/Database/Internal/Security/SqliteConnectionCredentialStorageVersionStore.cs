using JasonQuery.Core.Security.ConnectionCredentials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace JasonQuery.Database.Internal.Security
{
    internal sealed class SqliteConnectionCredentialStorageVersionStore : IConnectionCredentialStorageVersionStore
    {
        internal const string MetadataTableName = "JasonQuerySecurityMetadata";
        internal const string VersionMetadataKey = "ConnectionCredentialStorageVersion";
        internal const string MetadataUniqueIndexName = "UX_JasonQuerySecurityMetadata_MetadataKey";

        private const string MetadataPidColumn = "PID";
        private const string MetadataKeyColumn = "MetadataKey";
        private const string MetadataValueColumn = "MetadataValue";
        private const string MetadataUpgradeTableName = "__JasonQuerySecurityMetadata_MigrationCompatible";

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
            return TableExists(connection, transaction, MetadataTableName);
        }

        private static bool TableExists(IDbConnection connection, IDbTransaction transaction, string tableName)
        {
            using (var command = CreateCommand(connection, transaction, "SELECT 1 " + "FROM sqlite_master " + "WHERE type = 'table' " + "AND name = @TableName " + "LIMIT 1"))
            {
                AddParameter
                (
                    command,
                    "@TableName",
                    tableName
                );

                return command.ExecuteScalar() != null;
            }
        }

        private static void EnsureMetadataTable(IDbConnection connection, IDbTransaction transaction)
        {
            if (!MetadataTableExists(connection, transaction))
            {
                CreateMigrationCompatibleMetadataTable
                (
                    connection,
                    transaction,
                    MetadataTableName
                );

                CreateMetadataUniqueIndex(connection, transaction);
                return;
            }

            var schemaKind = GetMetadataSchemaKind(connection, transaction);

            switch (schemaKind)
            {
                case MetadataSchemaKind.MigrationCompatible:
                    {
                        return;
                    }
                case MetadataSchemaKind.MigrationCompatibleWithoutUniqueIndex:
                    {
                        CreateMetadataUniqueIndex(connection, transaction);

                        if (GetMetadataSchemaKind(connection, transaction) != MetadataSchemaKind.MigrationCompatible)
                        {
                            throw new InvalidDataException
                            (
                                "The JasonQuery security metadata table could not be normalized to the migration-compatible schema."
                            );
                        }

                        return;
                    }
                case MetadataSchemaKind.LegacyTextPrimaryKey:
                case MetadataSchemaKind.InlineUniqueConstraint:
                    {
                        RebuildMetadataTable(connection, transaction);
                        return;
                    }
                default:
                    {
                        throw new InvalidDataException
                        (
                            "The JasonQuery security metadata table has an unsupported schema and cannot be normalized safely."
                        );
                    }
            }
        }

        private static MetadataSchemaKind GetMetadataSchemaKind(IDbConnection connection, IDbTransaction transaction)
        {
            var columns = ReadMetadataColumns(connection, transaction);
            var indexes = ReadMetadataIndexes(connection, transaction);

            if (IsMigrationCompatibleColumns(columns))
            {
                if (indexes.Count == 0)
                {
                    return MetadataSchemaKind.MigrationCompatibleWithoutUniqueIndex;
                }

                if (indexes.Count == 1 && IsExpectedExplicitUniqueIndex(indexes[0]))
                {
                    return MetadataSchemaKind.MigrationCompatible;
                }

                if (indexes.Count == 1 && IsInlineUniqueIndex(indexes[0]))
                {
                    return MetadataSchemaKind.InlineUniqueConstraint;
                }

                return MetadataSchemaKind.Unsupported;
            }

            if (IsLegacyTextPrimaryKeyColumns(columns) && indexes.Count == 1 && IsLegacyPrimaryKeyAutoIndex(indexes[0]))
            {
                return MetadataSchemaKind.LegacyTextPrimaryKey;
            }

            return MetadataSchemaKind.Unsupported;
        }

        private static List<MetadataColumnDefinition> ReadMetadataColumns(IDbConnection connection, IDbTransaction transaction)
        {
            var columns = new List<MetadataColumnDefinition>();

            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    "PRAGMA table_info(" + QuoteSqlStringLiteral(MetadataTableName) + ")"
                )
            )
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add
                    (
                        new MetadataColumnDefinition
                        (
                            Convert.ToInt32(reader["cid"], CultureInfo.InvariantCulture),
                            Convert.ToString(reader["name"], CultureInfo.InvariantCulture),
                            Convert.ToString(reader["type"], CultureInfo.InvariantCulture),
                            Convert.ToInt32(reader["notnull"], CultureInfo.InvariantCulture) != 0,
                            Convert.ToInt32(reader["pk"], CultureInfo.InvariantCulture)
                        )
                    );
                }
            }

            return columns;
        }

        private static List<MetadataIndexDefinition> ReadMetadataIndexes(IDbConnection connection, IDbTransaction transaction)
        {
            var indexes = new List<MetadataIndexDefinition>();

            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    "PRAGMA index_list(" + QuoteSqlStringLiteral(MetadataTableName) + ")"
                )
            )
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    indexes.Add
                    (
                        new MetadataIndexDefinition
                        (
                            Convert.ToString(reader["name"], CultureInfo.InvariantCulture),
                            Convert.ToInt32(reader["unique"], CultureInfo.InvariantCulture) != 0,
                            Convert.ToString(reader["origin"], CultureInfo.InvariantCulture),
                            Convert.ToInt32(reader["partial"], CultureInfo.InvariantCulture) != 0
                        )
                    );
                }
            }

            foreach (var index in indexes)
            {
                using
                (
                    var command = CreateCommand
                    (
                        connection,
                        transaction,
                        "PRAGMA index_info(" + QuoteSqlStringLiteral(index.Name) + ")"
                    )
                )
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        index.Columns.Add
                        (
                            Convert.ToString
                            (
                                reader["name"],
                                CultureInfo.InvariantCulture
                            )
                        );
                    }
                }
            }

            return indexes;
        }

        private static bool IsMigrationCompatibleColumns(IReadOnlyList<MetadataColumnDefinition> columns)
        {
            return columns.Count == 3 && IsColumn(columns[0], 0, MetadataPidColumn, "INTEGER", false, 1) && IsColumn(columns[1], 1, MetadataKeyColumn, "TEXT", true, 0) && IsColumn(columns[2], 2, MetadataValueColumn, "TEXT", true, 0);
        }

        private static bool IsLegacyTextPrimaryKeyColumns(IReadOnlyList<MetadataColumnDefinition> columns)
        {
            return columns.Count == 2 && IsColumn(columns[0], 0, MetadataKeyColumn, "TEXT", true, 1) && IsColumn(columns[1], 1, MetadataValueColumn, "TEXT", true, 0);
        }

        private static bool IsColumn(MetadataColumnDefinition column, int cid, string name, string declaredType, bool notNull, int primaryKeyOrdinal)
        {
            return column.Cid == cid && string.Equals(column.Name, name, StringComparison.Ordinal)
                   && string.Equals(column.DeclaredType, declaredType, StringComparison.OrdinalIgnoreCase)
                   && column.NotNull == notNull && column.PrimaryKeyOrdinal == primaryKeyOrdinal;
        }

        private static bool IsExpectedExplicitUniqueIndex(MetadataIndexDefinition index)
        {
            return string.Equals(index.Name, MetadataUniqueIndexName, StringComparison.Ordinal) && index.Unique
                   && string.Equals(index.Origin, "c", StringComparison.Ordinal) && !index.Partial && index.Columns.Count == 1
                   && string.Equals(index.Columns[0], MetadataKeyColumn, StringComparison.Ordinal);
        }

        private static bool IsInlineUniqueIndex(MetadataIndexDefinition index)
        {
            return index.Unique && string.Equals(index.Origin, "u", StringComparison.Ordinal) && !index.Partial
                   && index.Columns.Count == 1 && string.Equals(index.Columns[0], MetadataKeyColumn, StringComparison.Ordinal);
        }

        private static bool IsLegacyPrimaryKeyAutoIndex(MetadataIndexDefinition index)
        {
            return index.Unique && string.Equals(index.Origin, "pk", StringComparison.Ordinal) && !index.Partial
                   && index.Name.StartsWith("sqlite_autoindex_", StringComparison.Ordinal) && index.Columns.Count == 1
                   && string.Equals(index.Columns[0], MetadataKeyColumn, StringComparison.Ordinal);
        }

        private static void RebuildMetadataTable(IDbConnection connection, IDbTransaction transaction)
        {
            if (TableExists(connection, transaction, MetadataUpgradeTableName))
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata schema-upgrade table already exists."
                );
            }

            var sourceRowCount = ReadTableRowCount
            (
                connection,
                transaction,
                MetadataTableName
            );

            CreateMigrationCompatibleMetadataTable
            (
                connection,
                transaction,
                MetadataUpgradeTableName
            );

            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    "INSERT INTO " + QuoteIdentifier(MetadataUpgradeTableName) +
                    " (" + QuoteIdentifier(MetadataKeyColumn) + ", " + QuoteIdentifier(MetadataValueColumn) + ") " +
                    "SELECT " + QuoteIdentifier(MetadataKeyColumn) + ", " + QuoteIdentifier(MetadataValueColumn) +
                    " FROM " + QuoteIdentifier(MetadataTableName)
                )
            )
            {
                command.ExecuteNonQuery();
            }

            var copiedRowCount = ReadTableRowCount
            (
                connection,
                transaction,
                MetadataUpgradeTableName
            );

            if (copiedRowCount != sourceRowCount)
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata rows could not be copied exactly during schema normalization."
                );
            }

            ExecuteSchemaCommand
            (
                connection,
                transaction,
                "DROP TABLE " + QuoteIdentifier(MetadataTableName)
            );

            ExecuteSchemaCommand
            (
                connection,
                transaction,
                "ALTER TABLE " + QuoteIdentifier(MetadataUpgradeTableName) +
                " RENAME TO " + QuoteIdentifier(MetadataTableName)
            );

            CreateMetadataUniqueIndex(connection, transaction);

            if (GetMetadataSchemaKind(connection, transaction) != MetadataSchemaKind.MigrationCompatible)
            {
                throw new InvalidDataException
                (
                    "The JasonQuery security metadata schema normalization did not produce the required migration-compatible shape."
                );
            }
        }

        private static void CreateMigrationCompatibleMetadataTable(IDbConnection connection, IDbTransaction transaction, string tableName)
        {
            ExecuteSchemaCommand
            (
                connection,
                transaction,
                "CREATE TABLE " + QuoteIdentifier(tableName) +
                " (" +
                QuoteIdentifier(MetadataPidColumn) + " INTEGER PRIMARY KEY, " +
                QuoteIdentifier(MetadataKeyColumn) + " TEXT NOT NULL, " +
                QuoteIdentifier(MetadataValueColumn) + " TEXT NOT NULL" +
                ")"
            );
        }

        private static void CreateMetadataUniqueIndex(IDbConnection connection, IDbTransaction transaction)
        {
            ExecuteSchemaCommand
            (
                connection,
                transaction,
                "CREATE UNIQUE INDEX " + QuoteIdentifier(MetadataUniqueIndexName) +
                " ON " + QuoteIdentifier(MetadataTableName) +
                " (" + QuoteIdentifier(MetadataKeyColumn) + ")"
            );
        }

        private static long ReadTableRowCount(IDbConnection connection, IDbTransaction transaction, string tableName)
        {
            using
            (
                var command = CreateCommand
                (
                    connection,
                    transaction,
                    "SELECT COUNT(*) FROM " + QuoteIdentifier(tableName)
                )
            )
            {
                return Convert.ToInt64
                (
                    command.ExecuteScalar(),
                    CultureInfo.InvariantCulture
                );
            }
        }

        private static void ExecuteSchemaCommand(IDbConnection connection, IDbTransaction transaction, string commandText)
        {
            using (var command = CreateCommand(connection, transaction, commandText))
            {
                command.ExecuteNonQuery();
            }
        }

        private static string QuoteIdentifier(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            return "[" + identifier.Replace("]", "]]") + "]";
        }

        private static string QuoteSqlStringLiteral(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return "'" + value.Replace("'", "''") + "'";
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

        private enum MetadataSchemaKind
        {
            Unsupported = 0,
            LegacyTextPrimaryKey = 1,
            InlineUniqueConstraint = 2,
            MigrationCompatibleWithoutUniqueIndex = 3,
            MigrationCompatible = 4
        }

        private sealed class MetadataColumnDefinition
        {
            public MetadataColumnDefinition(int cid, string name, string declaredType, bool notNull, int primaryKeyOrdinal)
            {
                Cid = cid;
                Name = name;
                DeclaredType = declaredType;
                NotNull = notNull;
                PrimaryKeyOrdinal = primaryKeyOrdinal;
            }

            public int Cid { get; }

            public string Name { get; }

            public string DeclaredType { get; }

            public bool NotNull { get; }

            public int PrimaryKeyOrdinal { get; }
        }

        private sealed class MetadataIndexDefinition
        {
            public MetadataIndexDefinition(string name, bool unique, string origin, bool partial)
            {
                Name = name;
                Unique = unique;
                Origin = origin;
                Partial = partial;
                Columns = new List<string>();
            }

            public string Name { get; }

            public bool Unique { get; }

            public string Origin { get; }

            public bool Partial { get; }

            public List<string> Columns { get; }
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

            using (var transaction = connection.BeginTransaction())
            {
                if (transaction == null)
                {
                    throw new InvalidOperationException
                    (
                        "The connection credential storage normalization transaction could not be created."
                    );
                }

                _versionStore.WriteCurrentVersion
                (
                    connection,
                    transaction
                );

                transaction.Commit();
            }

            var persistedVersion = _versionStore.ReadPersistedVersion(connection, null);

            ConnectionCredentialStorageContract.EnsureV2Ready(persistedVersion);
            return migrated;
        }
    }
}
