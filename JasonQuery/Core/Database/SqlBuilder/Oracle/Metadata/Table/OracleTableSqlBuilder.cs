using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table
{
    internal static class OracleTableSqlBuilder
    {
        public static string BuildGetTableInfoSql(string ownerUppercase, bool sortByColumnName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

            sbSql.AppendLine("SELECT 'TABLE' AS SchemaType,");
            sbSql.AppendLine("       c.Table_Name AS TableName,");
            sbSql.AppendLine("       c.Column_Name AS ColumnName,");
            sbSql.AppendLine("       c.Column_ID AS ColumnID,");
            sbSql.AppendLine("       c.Data_Type AS DataType,");
            sbSql.AppendLine("       c.Data_Length AS DataLength,");
            sbSql.AppendLine("       c.Data_Precision || ',' || c.Data_Scale AS Scale");
            sbSql.AppendLine("  FROM All_Tab_Columns c");
            sbSql.AppendLine("       JOIN All_Tables t");
            sbSql.AppendLine("         ON t.Owner = c.Owner");
            sbSql.AppendLine("        AND t.Table_Name = c.Table_Name");
            sbSql.AppendLine($" WHERE UPPER(c.Owner) = '{ownerUppercase}'");

            if (sortByColumnName)
            {
                sbSql.Append(" ORDER BY c.Table_Name, c.Column_Name");
            }
            else
            {
                sbSql.Append(" ORDER BY c.Table_Name, c.Column_ID");
            }

            return sbSql.ToString();
        }

        public static string BuildGetTableQuantitySql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get the Total Number of Entries for all Tables");

            sbSql.AppendLine("SELECT Table_Name AS TableName, NVL(MAX(Num_Rows), 0) AS NumRows");
            sbSql.AppendLine("  FROM All_Tables");
            sbSql.AppendLine($" WHERE Owner = '{ownerUppercase}'");
            sbSql.Append(" GROUP BY Table_Name");

            return sbSql.ToString();
        }

        public static string BuildGetTableConstraintsSql(string ownerUppercase, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Constraints Info");

            sbSql.AppendLine("SELECT cols.Table_Name AS TableName, cols.Column_Name AS ColumnName,");
            sbSql.AppendLine("       cons.Constraint_Type AS ConstraintType, cons.Constraint_Name AS ConstraintName");
            sbSql.AppendLine("  FROM All_Constraints cons, All_Cons_Columns cols");
            sbSql.AppendLine(" WHERE cons.Constraint_Name = cols.Constraint_Name");
            sbSql.AppendLine($"   AND UPPER(cols.Owner) = '{ownerUppercase}'");
            sbSql.AppendLine($"   AND cols.TABLE_NAME = '{schemaName}'");
            sbSql.Append(" ORDER BY cons.Constraint_Type DESC");

            return sbSql.ToString();
        }
    }
}
