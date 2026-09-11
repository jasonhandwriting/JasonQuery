using System;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Database-level logical metadata preserved across physical storage migration.
    ///
    /// Physical/runtime PRAGMAs such as page_size, journal_mode, locking_mode,
    /// synchronous, secure_delete, schema_version, and freelist_count are
    /// intentionally absent from this contract.
    /// </summary>
    public sealed class DatabaseStorageMigrationDatabaseMetadata
    {
        public DatabaseStorageMigrationDatabaseMetadata(int userVersion, int applicationId, string sourceEncodingName)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSourceEncoding(sourceEncodingName);

            UserVersion = userVersion;
            ApplicationId = applicationId;
            SourceEncodingName = sourceEncodingName;
        }

        public int UserVersion { get; }

        public int ApplicationId { get; }

        public string SourceEncodingName { get; }
    }

    /// <summary>
    /// Hybrid table contract: original CREATE TABLE SQL is replay authority;
    /// structured metadata is validation and row-stream authority.
    /// </summary>
    public sealed class DatabaseStorageMigrationTableDefinition
    {
        public DatabaseStorageMigrationTableDefinition(int tableId, string name, string createSql, int columnCount,
                                                       int rowIdAliasColumnCid, bool hasAutoincrement)
        {
            if (tableId < 0 || tableId >= DatabaseStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));
            DatabaseStorageMigrationLogicalStreamContract.EnsureRequiredSql(createSql, nameof(createSql));
            DatabaseStorageMigrationLogicalStreamContract.EnsureColumnCount(columnCount);

            if (rowIdAliasColumnCid < 0 || rowIdAliasColumnCid >= columnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIdAliasColumnCid));
            }

            TableId = tableId;
            Name = name;
            CreateSql = createSql;
            ColumnCount = columnCount;
            RowIdAliasColumnCid = rowIdAliasColumnCid;
            HasAutoincrement = hasAutoincrement;
        }

        public int TableId { get; }

        public string Name { get; }

        public string CreateSql { get; }

        public int ColumnCount { get; }

        public int RowIdAliasColumnCid { get; }

        public bool HasAutoincrement { get; }
    }

    public sealed class DatabaseStorageMigrationColumnDefinition
    {
        public DatabaseStorageMigrationColumnDefinition(int cid, string name, string declaredType, bool notNull,
                                                        int primaryKeyOrdinal, int hidden, string defaultSql)
        {
            if (cid < 0 || cid >= DatabaseStorageMigrationLogicalStreamContract.MaxColumnsPerTable)
            {
                throw new ArgumentOutOfRangeException(nameof(cid));
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));

            if (declaredType == null)
            {
                declaredType = string.Empty;
            }

            if (primaryKeyOrdinal < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(primaryKeyOrdinal));
            }

            if (hidden != 0)
            {
                throw new NotSupportedException
                (
                    "Hidden/generated columns are not qualified by logical stream protocol V1."
                );
            }

            DatabaseStorageMigrationLogicalStreamContract.EnsureOptionalSql(defaultSql, nameof(defaultSql));

            Cid = cid;
            Name = name;
            DeclaredType = declaredType;
            NotNull = notNull;
            PrimaryKeyOrdinal = primaryKeyOrdinal;
            Hidden = hidden;
            DefaultSql = defaultSql;
        }

        public int Cid { get; }

        public string Name { get; }

        public string DeclaredType { get; }

        public bool NotNull { get; }

        public int PrimaryKeyOrdinal { get; }

        public int Hidden { get; }

        public string DefaultSql { get; }
    }

    /// <summary>
    /// Logical sqlite_sequence state for an AUTOINCREMENT table.
    /// sqlite_sequence itself is never replayed as a normal user table.
    /// </summary>
    public sealed class DatabaseStorageMigrationSequenceEntry
    {
        public DatabaseStorageMigrationSequenceEntry(string tableName, long sequenceValue)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(tableName, nameof(tableName));

            TableName = tableName;
            SequenceValue = sequenceValue;
        }

        public string TableName { get; }

        public long SequenceValue { get; }
    }

    public sealed class DatabaseStorageMigrationSecondarySchemaObject
    {
        public DatabaseStorageMigrationSecondarySchemaObject(DatabaseStorageMigrationSchemaObjectKind kind,
                                                             string name, string tableName, string originalSql)
        {
            DatabaseStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind(kind);
            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));
            DatabaseStorageMigrationLogicalStreamContract.EnsureIdentifier(tableName, nameof(tableName));
            DatabaseStorageMigrationLogicalStreamContract.EnsureRequiredSql(originalSql, nameof(originalSql));

            Kind = kind;
            Name = name;
            TableName = tableName;
            OriginalSql = originalSql;
        }

        public DatabaseStorageMigrationSchemaObjectKind Kind { get; }

        public string Name { get; }

        public string TableName { get; }

        public string OriginalSql { get; }
    }
}
