using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table
{
    internal static class PostgreSqlTableSqlBuilder
    {
        public static string BuildGetTableConstraintsSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Constraints Info");

            //取得 Constraint 資訊
            sbSql.AppendLine("SELECT n.nspname AS SchemaName,");
            sbSql.AppendLine("       t.relname AS TableName,");
            sbSql.AppendLine("       a.attname AS ColumnName,");
            sbSql.AppendLine("       con.contype::text || ', ' || con.conname AS ConstraintInfo");
            sbSql.AppendLine("  FROM pg_constraint con");
            sbSql.AppendLine("       JOIN pg_class t ON con.conrelid = t.oid");
            sbSql.AppendLine("       JOIN pg_namespace n ON t.relnamespace = n.oid");
            sbSql.AppendLine("       JOIN UNNEST(con.conkey) AS cols(colnum) ON true");
            sbSql.AppendLine("       JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = cols.colnum");
            sbSql.AppendLine($" WHERE n.nspname = '{schemaNode}'");
            sbSql.AppendLine($"   AND t.relname = '{schemaName}'");
            sbSql.AppendLine("   AND con.contype IN ('p', 'f', 'u', 'c')");
            sbSql.Append(" ORDER BY con.conname, cols.colnum;");

            return sbSql.ToString();
        }
    }
}