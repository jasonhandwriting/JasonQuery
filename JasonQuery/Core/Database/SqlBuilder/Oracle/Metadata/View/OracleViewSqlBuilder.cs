using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.View
{
    internal static class OracleViewSqlBuilder
    {
        public static string BuildGetViewInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT o.Object_Name AS ViewName,");
            sbSql.AppendLine("       v.Text_Length AS TextLength,");
            sbSql.AppendLine("       o.Status,");
            sbSql.AppendLine("       o.Created");
            sbSql.AppendLine("  FROM All_Objects o");
            sbSql.AppendLine("       JOIN All_Views v");
            sbSql.AppendLine("         ON v.Owner = o.Owner");
            sbSql.AppendLine("        AND v.View_Name = o.Object_Name");
            sbSql.AppendLine(" WHERE o.Object_Type = 'VIEW'");
            sbSql.AppendLine($"   AND UPPER(o.Owner) = '{ownerUppercase}'");
            sbSql.Append(" ORDER BY o.Object_Name");

            return sbSql.ToString();
        }

        public static string BuildGetViewColumnInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information (Including Column Name and Type)");

            sbSql.AppendLine("SELECT c.Table_Name AS ViewName,");
            sbSql.AppendLine("       c.Column_Name AS ColumnName,");
            sbSql.AppendLine("       c.Column_ID AS ColumnID,");
            sbSql.AppendLine("       c.Data_Type ||");
            sbSql.AppendLine("       CASE");
            sbSql.AppendLine("            WHEN c.Data_Type IN ('CHAR', 'VARCHAR2', 'NCHAR', 'NVARCHAR2')");
            sbSql.AppendLine("                 THEN '(' || c.Char_Length || ')'");
            sbSql.AppendLine("            WHEN c.Data_Type = 'NUMBER' AND c.Data_Precision IS NOT NULL");
            sbSql.AppendLine("                 THEN '(' || c.Data_Precision || ',' || c.Data_Scale || ')'");
            sbSql.AppendLine("            ELSE ''");
            sbSql.AppendLine("       END AS ColumnType");
            sbSql.AppendLine("  FROM All_Tab_Columns c");
            sbSql.AppendLine("       JOIN All_Views v");
            sbSql.AppendLine("         ON v.Owner = c.Owner");
            sbSql.AppendLine("        AND v.View_Name = c.Table_Name");
            sbSql.AppendLine($" WHERE UPPER(c.Owner) = '{ownerUppercase}'");
            sbSql.Append(" ORDER BY c.Table_Name, c.Column_ID");

            return sbSql.ToString();
        }
    }
}