using JasonQuery.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table
{
    internal static class OracleTableColumnCommentsSqlBuilder
    {
        public static string Build(IReadOnlyList<(string Schema, string Table)> tableList, string dbUserUppercase)
        {
            if (tableList == null || tableList.Count == 0)
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comments (Multi Tables)");

            var inTables = tableList.Select
            (
                t => $"'{(t.Table ?? string.Empty).ToUpperInvariant().Replace("'", "''")}'"
            );

            sbSql.AppendLine($"SELECT '{dbUserUppercase}' AS SchemaName,");
            sbSql.AppendLine("       ss.Table_Name  AS TableName,");
            sbSql.AppendLine("       ss.Column_Name AS ColumnName,");
            sbSql.AppendLine("       cc.Comments");
            sbSql.AppendLine("  FROM User_Tab_Columns ss");
            sbSql.AppendLine("       LEFT JOIN User_Col_Comments cc");
            sbSql.AppendLine("              ON ss.Table_Name = cc.Table_Name");
            sbSql.AppendLine("             AND ss.Column_Name = cc.Column_Name");
            sbSql.AppendLine(" WHERE ss.Table_Name IN (");
            sbSql.AppendLine(string.Join(", ", inTables));
            sbSql.Append(")");

            return sbSql.ToString();
        }
    }
}
