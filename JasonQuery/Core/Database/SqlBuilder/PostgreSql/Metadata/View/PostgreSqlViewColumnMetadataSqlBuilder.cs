using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.View
{
    internal static class PostgreSqlViewColumnMetadataSqlBuilder
    {
        public static string Build()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get all Schema+Tables+Columns+ColumnType for View");

            //20250502 一次性查出所有視圖的欄位訊息
            sbSql.AppendLine("SELECT n.nspname AS DbName, c.relname AS ViewName, a.attname AS ColumnName,");
            sbSql.AppendLine("       pg_catalog.FORMAT_TYPE(a.atttypid, a.atttypmod) AS ColumnType");
            sbSql.AppendLine("  FROM pg_catalog.pg_class c");
            sbSql.AppendLine("       JOIN pg_catalog.pg_namespace n ON c.relnamespace = n.oid");
            sbSql.AppendLine("       JOIN pg_catalog.pg_attribute a ON a.attrelid = c.oid");
            sbSql.AppendLine(" WHERE n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("   AND c.relkind = 'v'");
            sbSql.AppendLine("   AND a.attnum > 0"); //排除系統欄位
            sbSql.Append("   AND NOT a.attisdropped"); //排除已刪除欄位

            return sbSql.ToString();
        }
    }
}