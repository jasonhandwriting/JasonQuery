using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.View
{
    internal static class PostgreSqlViewMetadataSqlBuilder
    {
        public static string Build()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT v.SchemaName AS DbName, 'Views' AS Views, v.ViewName");
            sbSql.AppendLine("  FROM pg_catalog.pg_views v");
            sbSql.AppendLine(" WHERE v.SchemaName NOT IN ('pg_catalog', 'information_schema')");
            sbSql.Append(" ORDER BY v.SchemaName, v.ViewName");

            return sbSql.ToString();
        }
    }
}