using JasonQuery.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table
{
    internal static class MySqlTableColumnCommentsSqlBuilder
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

            sbSql.AppendLine("SELECT Table_Schema AS SchemaName,");
            sbSql.AppendLine("       Table_Name AS TableName,");
            sbSql.AppendLine("       Column_Name AS ColumnName,");
            sbSql.AppendLine("       Column_Comment AS Comments");
            sbSql.AppendLine("  FROM Information_Schema.Columns");
            sbSql.AppendLine(" WHERE (Table_Schema, Table_Name) IN");
            sbSql.AppendLine("       (");
            sbSql.AppendLine(string.Join("," + Environment.NewLine, tupleConditions));
            sbSql.Append("       )");

            return sbSql.ToString();
        }
    }
}