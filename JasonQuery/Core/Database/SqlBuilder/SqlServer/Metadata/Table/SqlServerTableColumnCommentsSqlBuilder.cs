using JasonQuery.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal static class SqlServerTableColumnCommentsSqlBuilder
    {
        public static string Build(IReadOnlyList<(string Schema, string Table)> tableList, string databaseName)
        {
            if (tableList == null || tableList.Count == 0 || string.IsNullOrWhiteSpace(databaseName))
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comments (Multi Tables)");

            var orConditions = tableList.Select
            (
                t => $"(s.Name = '{(t.Schema ?? string.Empty).Replace("'", "''")}' AND o.Name = '{(t.Table ?? string.Empty).Replace("'", "''")}')"
            );

            sbSql.AppendLine("SELECT s.Name AS SchemaName,");
            sbSql.AppendLine("       o.Name AS TableName,");
            sbSql.AppendLine("       c.Name AS ColumnName,");
            sbSql.AppendLine("       prop.Value AS Comments");
            sbSql.AppendLine($"  FROM {databaseName}.sys.Extended_Properties prop");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Objects o");
            sbSql.AppendLine("               ON prop.Major_ID = o.Object_ID");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Schemas s");
            sbSql.AppendLine("               ON o.Schema_ID = s.Schema_ID");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Columns c");
            sbSql.AppendLine("               ON prop.Major_ID = c.Object_ID");
            sbSql.AppendLine("              AND prop.Minor_ID = c.Column_ID");
            sbSql.AppendLine(" WHERE prop.Name = 'MS_Description'");
            sbSql.AppendLine("   AND (");
            sbSql.AppendLine(string.Join(" OR ", orConditions));
            sbSql.Append("       )");

            return sbSql.ToString();
        }
    }
}