using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using System;
using System.Text;

namespace JasonQuery.Core.Database.Metadata
{
    internal enum MetadataCommentObjectKind
    {
        Table,
        Column
    }

    internal sealed class MetadataCommentQueryRequest
    {
        public DataSourceType DataSourceType { get; set; }
        public MetadataCommentObjectKind ObjectKind { get; set; }
        public string SchemaDatabase { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
    }

    /// <summary>
    /// Pure, side-effect-free query templates for D1-B Golden tests.
    /// IMPORTANT: Forms are NOT wired to this component until D1-B2.
    /// Identifiers use the same existing-name policy as the DDL builders;
    /// metadata values are escaped as SQL string literals, not treated as identifiers.
    /// </summary>
    internal static class MetadataCommentQueryBuilder
    {
        public static string Build(MetadataCommentQueryRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            if (request.ObjectKind != MetadataCommentObjectKind.Table && request.ObjectKind != MetadataCommentObjectKind.Column)
            {
                return string.Empty;
            }

            if (request.DataSourceType != DataSourceType.Oracle && request.DataSourceType != DataSourceType.PostgreSql && request.DataSourceType != DataSourceType.SqlServer && request.DataSourceType != DataSourceType.MySql)
            {
                return string.Empty;
            }

            var table = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(request.DataSourceType, request.TableName);
            var schema = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(request.DataSourceType, request.SchemaName);
            var column = request.ObjectKind == MetadataCommentObjectKind.Column ? DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(request.DataSourceType, request.ColumnName) : string.Empty;

            if (string.IsNullOrEmpty(table) || (request.ObjectKind == MetadataCommentObjectKind.Column && string.IsNullOrEmpty(column)))
            {
                return string.Empty;
            }

            //PostgreSQL (when standard_conforming_strings is off) and MySQL have backslash-sensitive string literal modes. Do not guess the server's SQL mode here: reject such names until a parameterized provider query is qualified in D1-B2.
            if ((request.DataSourceType == DataSourceType.PostgreSql || request.DataSourceType == DataSourceType.MySql) && (HasBackslash(table) || HasBackslash(schema) || HasBackslash(column) || HasBackslash(request.SchemaDatabase)))
            {
                return string.Empty;
            }

            if (request.DataSourceType == DataSourceType.MySql)
            {
                //The table DDL falls back to SchemaName; column DDL requires SchemaDatabase.
                //A conflicting pair must fail closed rather than read a different database.
                var database = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(DataSourceType.MySql, request.SchemaDatabase);

                if (!string.IsNullOrEmpty(database) && !string.IsNullOrEmpty(schema) && !string.Equals(database, schema, StringComparison.Ordinal))
                {
                    return string.Empty;
                }

                if (request.ObjectKind == MetadataCommentObjectKind.Column && string.IsNullOrEmpty(database))
                {
                    return string.Empty;
                }

                schema = string.IsNullOrEmpty(database) ? schema : database;
            }
            else if (string.IsNullOrEmpty(schema))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(schema))
            {
                return string.Empty;
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        //Oracle folds unquoted schema identifiers to uppercase. Explicitly quoted schema identifiers remain case-sensitive (D1-B1 G04).
                        var isQuotedSchema = !string.IsNullOrEmpty(request.SchemaName) && request.SchemaName.Length >= 2 && request.SchemaName[0] == '"' && request.SchemaName[request.SchemaName.Length - 1] == '"';

                        var oracleSchema = isQuotedSchema ? schema : schema.ToUpperInvariant();

                        return BuildOracle(request.ObjectKind, oracleSchema, table, column);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return BuildPostgreSql(request.ObjectKind, schema, table, column);
                    }
                case DataSourceType.SqlServer:
                    {
                        var databaseValue = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(DataSourceType.SqlServer, request.SchemaDatabase);

                        if (string.IsNullOrEmpty(databaseValue))
                        {
                            return string.Empty;
                        }

                        var quotedDatabase = DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.SqlServer, request.SchemaDatabase);

                        return string.IsNullOrEmpty(quotedDatabase) ? string.Empty : BuildSqlServer(request.ObjectKind, quotedDatabase, schema, table, column);
                    }
                case DataSourceType.MySql:
                    {
                        return BuildMySql(request.ObjectKind, schema, table, column);
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildOracle(MetadataCommentObjectKind kind, string schema, string table, string column)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SELECT x.COMMENTS AS MetadataComment,");
            sb.AppendLine("       CASE WHEN x.COMMENTS IS NULL THEN 0 ELSE 1 END AS HasExistingComment");

            if (kind == MetadataCommentObjectKind.Table)
            {
                sb.AppendLine("  FROM ALL_TAB_COMMENTS x");
                sb.AppendLine(" WHERE x.TABLE_TYPE = 'TABLE'");
                sb.AppendLine($"   AND x.OWNER = '{Literal(schema)}'");
                sb.Append($"   AND x.TABLE_NAME = '{Literal(table)}'");
            }
            else
            {
                sb.AppendLine("  FROM ALL_COL_COMMENTS x");
                sb.AppendLine(" INNER JOIN ALL_TABLES t ON t.OWNER = x.OWNER AND t.TABLE_NAME = x.TABLE_NAME");
                sb.AppendLine($" WHERE x.OWNER = '{Literal(schema)}'");
                sb.AppendLine($"   AND x.TABLE_NAME = '{Literal(table)}'");
                sb.Append($"   AND x.COLUMN_NAME = '{Literal(column)}'");
            }

            return sb.ToString();
        }

        private static string BuildPostgreSql(MetadataCommentObjectKind kind, string schema, string table, string column)
        {
            var commentExpr = kind == MetadataCommentObjectKind.Table ? "pg_catalog.col_description(c.oid, 0)" : "pg_catalog.col_description(c.oid, a.attnum)";
            var sb = new StringBuilder();

            sb.AppendLine($"SELECT {commentExpr} AS MetadataComment,");
            sb.AppendLine($"       CASE WHEN {commentExpr} IS NULL THEN 0 ELSE 1 END AS HasExistingComment");
            sb.AppendLine("  FROM pg_catalog.pg_class c");
            sb.AppendLine(" INNER JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine(" INNER JOIN pg_catalog.pg_attribute a ON a.attrelid = c.oid");
            }

            //Preserve currently supported ordinary-table scope. 'p' partitioned tables require a separate, explicit capability decision (D1-B3).
            sb.AppendLine(" WHERE c.relkind = 'r'");
            sb.AppendLine($"   AND n.nspname = '{Literal(schema)}'");
            sb.Append($"   AND c.relname = '{Literal(table)}'");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine();
                sb.AppendLine("   AND a.attnum > 0 AND NOT a.attisdropped");
                sb.Append($"   AND a.attname = '{Literal(column)}'");
            }

            return sb.ToString();
        }

        private static string BuildSqlServer(MetadataCommentObjectKind kind, string db, string schema, string table, string column)
        {
            var sb = new StringBuilder();

            sb.AppendLine("SELECT CAST(ep.value AS nvarchar(max)) AS MetadataComment,");
            sb.AppendLine("       CASE WHEN ep.major_id IS NULL THEN 0 ELSE 1 END AS HasExistingComment");
            sb.AppendLine($"  FROM {db}.sys.tables t");
            sb.AppendLine($" INNER JOIN {db}.sys.schemas s ON s.schema_id = t.schema_id");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine($" INNER JOIN {db}.sys.columns c ON c.object_id = t.object_id");
            }

            sb.AppendLine($"  LEFT JOIN {db}.sys.extended_properties ep ON ep.class = 1");
            sb.AppendLine("   AND ep.major_id = t.object_id");
            sb.AppendLine(kind == MetadataCommentObjectKind.Table ? "   AND ep.minor_id = 0" : "   AND ep.minor_id = c.column_id");
            sb.AppendLine("   AND ep.name = N'MS_Description'");
            sb.AppendLine($" WHERE s.name = N'{Literal(schema)}'");
            sb.Append($"   AND t.name = N'{Literal(table)}'");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine();
                sb.Append($"   AND c.name = N'{Literal(column)}'");
            }

            return sb.ToString();
        }

        private static string BuildMySql(MetadataCommentObjectKind kind, string database, string table, string column)
        {
            var commentExpr = kind == MetadataCommentObjectKind.Table ? "t.TABLE_COMMENT" : "c.COLUMN_COMMENT";
            var sb = new StringBuilder();

            sb.AppendLine($"SELECT {commentExpr} AS MetadataComment,");
            sb.AppendLine($"       CASE WHEN {commentExpr} IS NULL OR {commentExpr} = '' THEN 0 ELSE 1 END AS HasExistingComment");
            sb.AppendLine("  FROM INFORMATION_SCHEMA.TABLES t");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine(" INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON c.TABLE_SCHEMA = t.TABLE_SCHEMA");
                sb.AppendLine("   AND c.TABLE_NAME = t.TABLE_NAME");
            }

            sb.AppendLine(" WHERE t.TABLE_TYPE = 'BASE TABLE'");
            sb.AppendLine($"   AND t.TABLE_SCHEMA = '{Literal(database)}'");
            sb.Append($"   AND t.TABLE_NAME = '{Literal(table)}'");

            if (kind == MetadataCommentObjectKind.Column)
            {
                sb.AppendLine();
                sb.Append($"   AND c.COLUMN_NAME = '{Literal(column)}'");
            }

            return sb.ToString();
        }

        private static bool HasBackslash(string value)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf('\\') >= 0;
        }

        private static string Literal(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
