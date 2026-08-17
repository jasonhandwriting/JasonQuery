using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Index
{
    internal static class OracleIndexSqlBuilder
    {
        public static string BuildGetIndexInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Index Information");

            sbSql.AppendLine("SELECT i.Index_Name AS IndexName,");
            sbSql.AppendLine("       i.Table_Name AS TableName,");
            sbSql.AppendLine("       i.Uniqueness AS Uniqueness,");
            sbSql.AppendLine("       i.Index_Type AS IndexType,");
            sbSql.AppendLine("       c.Column_Name AS ColumnName,");
            sbSql.AppendLine("       c.Column_Position AS ColumnPosition,");
            sbSql.AppendLine("       c.Descend AS Descend,");
            sbSql.AppendLine("       e.Column_Expression AS ColumnExpression");
            sbSql.AppendLine("  FROM All_Indexes i");
            sbSql.AppendLine("       JOIN All_Ind_Columns c");
            sbSql.AppendLine("         ON c.Index_Owner = i.Owner");
            sbSql.AppendLine("        AND c.Index_Name = i.Index_Name");
            sbSql.AppendLine("        AND c.Table_Owner = i.Table_Owner");
            sbSql.AppendLine("        AND c.Table_Name = i.Table_Name");
            sbSql.AppendLine("       LEFT JOIN All_Ind_Expressions e");
            sbSql.AppendLine("         ON e.Index_Owner = c.Index_Owner");
            sbSql.AppendLine("        AND e.Index_Name = c.Index_Name");
            sbSql.AppendLine("        AND e.Table_Owner = c.Table_Owner");
            sbSql.AppendLine("        AND e.Table_Name = c.Table_Name");
            sbSql.AppendLine("        AND e.Column_Position = c.Column_Position");
            sbSql.AppendLine($" WHERE UPPER(i.Owner) = '{ownerUppercase}'");
            sbSql.AppendLine("   AND i.Index_Type <> 'LOB'");
            sbSql.Append(" ORDER BY i.Index_Name, c.Column_Position");

            return sbSql.ToString();
        }
    }
}