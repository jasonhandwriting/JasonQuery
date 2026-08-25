using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.View
{
    internal static class PostgreSqlViewSchemaMetadataSqlBuilder
    {
        public static string Build()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Schema Information");

            sbSql.AppendLine("WITH view_list AS");
            sbSql.AppendLine("(");
            sbSql.AppendLine("    SELECT n.nspname AS DbName,");
            sbSql.AppendLine("           c.oid AS ViewOid,");
            sbSql.AppendLine("           c.relname AS ViewName");
            sbSql.AppendLine("      FROM pg_catalog.pg_class c");
            sbSql.AppendLine("           JOIN pg_catalog.pg_namespace n");
            sbSql.AppendLine("             ON n.oid = c.relnamespace");
            sbSql.AppendLine("     WHERE c.relkind = 'v'");
            sbSql.AppendLine("       AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("),");
            sbSql.AppendLine("view_count AS");
            sbSql.AppendLine("(");
            sbSql.AppendLine("    SELECT DbName,");
            sbSql.AppendLine("           COUNT(*) AS ViewCountInSchema");
            sbSql.AppendLine("      FROM view_list");
            sbSql.AppendLine("     GROUP BY DbName");
            sbSql.AppendLine(")");
            sbSql.AppendLine("SELECT v.DbName,");
            sbSql.AppendLine("       v.ViewName,");
            sbSql.AppendLine("       a.attname AS ColumnName,");
            sbSql.AppendLine("       pg_catalog.format_type(a.atttypid, a.atttypmod) AS ColumnType,");
            sbSql.AppendLine("       vc.ViewCountInSchema");
            sbSql.AppendLine("  FROM view_list v");
            sbSql.AppendLine("       JOIN view_count vc");
            sbSql.AppendLine("         ON vc.DbName = v.DbName");
            sbSql.AppendLine("       JOIN pg_catalog.pg_attribute a");
            sbSql.AppendLine("         ON a.attrelid = v.ViewOid");
            sbSql.AppendLine(" WHERE a.attnum > 0");
            sbSql.AppendLine("   AND NOT a.attisdropped");
            sbSql.Append(" ORDER BY v.DbName, v.ViewName, a.attnum");

            return sbSql.ToString();
        }
    }
}
