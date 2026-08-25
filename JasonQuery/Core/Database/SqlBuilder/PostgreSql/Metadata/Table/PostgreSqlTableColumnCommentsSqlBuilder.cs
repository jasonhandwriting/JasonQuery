using JasonQuery.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table
{
    internal static class PostgreSqlTableColumnCommentsSqlBuilder
    {
        public static string Build(IReadOnlyList<(string Schema, string Table)> tableList)
        {
            if (tableList == null || tableList.Count == 0)
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comments (Multi Tables)");

            var tupleConditions = tableList.Select
            (
                t => $"('{(t.Schema ?? string.Empty).Replace("'", "''")}', '{(t.Table ?? string.Empty).Replace("'", "''")}')"
            );

            sbSql.AppendLine("SELECT cols.Table_Schema AS SchemaName,");
            sbSql.AppendLine("       cols.Table_Name   AS TableName,");
            sbSql.AppendLine("       cols.Column_Name  AS ColumnName,");
            sbSql.AppendLine("       pg_catalog.col_description(");
            sbSql.AppendLine("           (cols.Table_Schema || '.' || cols.Table_Name)::regclass::oid,");
            sbSql.AppendLine("           cols.Ordinal_Position::int");
            sbSql.AppendLine("       ) AS Comments");
            sbSql.AppendLine("  FROM Information_Schema.Columns cols");
            sbSql.AppendLine(" WHERE (cols.Table_Schema, cols.Table_Name) IN");
            sbSql.AppendLine("       (");
            sbSql.AppendLine(string.Join("," + Environment.NewLine, tupleConditions));
            sbSql.Append("       )");

            return sbSql.ToString();
        }

        public static string BuildColumnCommentsInfo(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comments Info");

            sbSql.AppendLine("SELECT cols.Table_Schema AS SchemaName, cols.Table_Name AS TableName, cols.Column_Name AS ColumnName,");
            sbSql.AppendLine("       (pg_catalog.COL_DESCRIPTION((Table_Schema || '.' || Table_Name)::regclass::oid, Ordinal_Position::int)) AS Comments");
            sbSql.AppendLine("  FROM Information_Schema.Columns cols");
            sbSql.AppendLine($" WHERE cols.Table_Schema = '{schemaNode}'");
            sbSql.Append($"   AND cols.Table_Name = '{schemaName}';");

            return sbSql.ToString();
        }
    }
}
