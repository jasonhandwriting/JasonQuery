using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Database-level logical metadata preserved across physical storage migration.
    ///
    /// Physical/runtime PRAGMAs such as page_size, journal_mode, locking_mode,
    /// synchronous, secure_delete, schema_version, and freelist_count are
    /// intentionally absent from this contract.
    /// </summary>
    public sealed class JasonQueryDbStorageMigrationDatabaseMetadata
    {
        public JasonQueryDbStorageMigrationDatabaseMetadata(int userVersion, int applicationId, string sourceEncodingName)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSourceEncoding(sourceEncodingName);

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
    public sealed class JasonQueryDbStorageMigrationTableDefinition
    {
        public JasonQueryDbStorageMigrationTableDefinition(int tableId, string name, string createSql, int columnCount,
                                                           int rowIdAliasColumnCid, bool hasAutoincrement)
        {
            if (tableId < 0 || tableId >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxTables)
            {
                throw new ArgumentOutOfRangeException(nameof(tableId));
            }

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRequiredSql(createSql, nameof(createSql));
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureColumnCount(columnCount);

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

    public sealed class JasonQueryDbStorageMigrationColumnDefinition
    {
        public JasonQueryDbStorageMigrationColumnDefinition(int cid, string name, string declaredType, bool notNull,
                                                            int primaryKeyOrdinal, int hidden, string defaultSql)
        {
            if (cid < 0 || cid >= JasonQueryDbStorageMigrationLogicalStreamContract.MaxColumnsPerTable)
            {
                throw new ArgumentOutOfRangeException(nameof(cid));
            }

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));

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

            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureOptionalSql(defaultSql, nameof(defaultSql));

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
    public sealed class JasonQueryDbStorageMigrationSequenceEntry
    {
        public JasonQueryDbStorageMigrationSequenceEntry(string tableName, long sequenceValue)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(tableName, nameof(tableName));

            TableName = tableName;
            SequenceValue = sequenceValue;
        }

        public string TableName { get; }

        public long SequenceValue { get; }
    }

    public sealed class JasonQueryDbStorageMigrationSecondarySchemaObject
    {
        public JasonQueryDbStorageMigrationSecondarySchemaObject(JasonQueryDbStorageMigrationSchemaObjectKind kind,
                                                                 string name, string tableName, string originalSql)
        {
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureSupportedSecondarySchemaObjectKind(kind);
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(name, nameof(name));
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureIdentifier(tableName, nameof(tableName));
            JasonQueryDbStorageMigrationLogicalStreamContract.EnsureRequiredSql(originalSql, nameof(originalSql));

            Kind = kind;
            Name = name;
            TableName = tableName;
            OriginalSql = originalSql;
        }

        public JasonQueryDbStorageMigrationSchemaObjectKind Kind { get; }

        public string Name { get; }

        public string TableName { get; }

        public string OriginalSql { get; }
    }
}
