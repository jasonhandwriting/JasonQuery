using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal static class SqlServerTableSqlBuilder
    {
        public static string BuildGetTableConstraintsSql(string schemaNode, string schemaName, string schemaDbo)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Constraint Information");

            sbSql.AppendLine("SELECT col.Column_Name AS ColumnName, col.Ordinal_Position, con.Constraint_Name AS ConstraintName, con.Constraint_Type AS ConstraintType");
            sbSql.AppendLine($"  FROM {schemaNode}.Information_Schema.Key_Column_Usage AS col, {schemaNode}.Information_Schema.Table_Constraints AS con");
            sbSql.AppendLine($" WHERE col.Table_Schema = '{schemaDbo}'");
            sbSql.AppendLine($"   AND col.Table_Name = '{schemaName}'");
            sbSql.AppendLine("   AND col.Table_Schema = con.Table_Schema");
            sbSql.AppendLine("   AND col.Table_Name = con.Table_Name");
            sbSql.AppendLine("   AND col.Constraint_Name = con.Constraint_Name");
            sbSql.Append(" ORDER BY col.Ordinal_Position;"); //排序是為後面要取 Create Table 的 Constraint 資料

            return sbSql.ToString();
        }
    }
}