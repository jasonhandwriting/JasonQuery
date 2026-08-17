using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Function
{
    internal static class PostgreSqlFunctionMetadataSqlBuilder
    {
        public static string Build_Post11()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Information (PostgreSQL version >= 11)");

            sbSql.AppendLine("SELECT n.nspname AS DbName,");
            sbSql.AppendLine("       p.proname AS FunctionName,");
            sbSql.AppendLine("       p.proname || '(' || COALESCE(pg_get_function_identity_arguments(p.oid), '') || ')' AS FunctionDisplayName");
            sbSql.AppendLine("  FROM pg_proc p");
            sbSql.AppendLine("       JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("       LEFT JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine(" WHERE p.prokind = 'f'");
            sbSql.AppendLine("   AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("   AND t.typname <> 'trigger'");
            sbSql.Append(" ORDER BY n.nspname, p.proname, pg_get_function_identity_arguments(p.oid)");

            return sbSql.ToString();
        }

        public static string Build_Pre11()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Information (PostgreSQL version < 11)");

            sbSql.AppendLine("SELECT n.nspname AS DbName,");
            sbSql.AppendLine("       p.proname AS FunctionName,");
            sbSql.AppendLine("       p.proname || '(' || COALESCE(pg_get_function_identity_arguments(p.oid), '') || ')' AS FunctionDisplayName");
            sbSql.AppendLine("  FROM pg_proc p");
            sbSql.AppendLine("       JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("       LEFT JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine(" WHERE NOT p.proisagg");
            sbSql.AppendLine("   AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("   AND t.typname <> 'trigger'");
            sbSql.Append(" ORDER BY n.nspname, p.proname, pg_get_function_identity_arguments(p.oid)");

            return sbSql.ToString();
        }
    }
}