using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Trigger
{
    internal static class PostgreSqlTriggerMetadataSqlBuilder
    {
        public static string Build(bool isVersion11OrGreater)
        {
            var sbSql = new StringBuilder();

            if (isVersion11OrGreater)
            {
                SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Information (PostgreSQL version >= 11)");

                sbSql.AppendLine("SELECT n.nspname AS DbName, 'Triggers' AS Triggers, p.proname AS TriggerName");
                sbSql.AppendLine("  FROM pg_proc p");
                sbSql.AppendLine("       JOIN pg_type t ON p.prorettype = t.oid");
                sbSql.AppendLine("       LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
                sbSql.AppendLine("       LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
                sbSql.AppendLine(" WHERE p.prokind <> 'p'");
                sbSql.AppendLine("   AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
                sbSql.AppendLine("   AND t.typname = 'trigger'");
                sbSql.Append(" ORDER BY n.nspname, p.proname");
            }
            else
            {
                SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Information (PostgreSQL version < 11)");

                sbSql.AppendLine("SELECT n.nspname AS DbName, 'Triggers' AS Triggers, p.proname AS TriggerName");
                sbSql.AppendLine("  FROM pg_proc p");
                sbSql.AppendLine("       JOIN pg_type t ON p.prorettype = t.oid");
                sbSql.AppendLine("       LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
                sbSql.AppendLine("       LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
                sbSql.AppendLine(" WHERE NOT p.proisagg");
                sbSql.AppendLine("   AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
                sbSql.AppendLine("   AND t.typname = 'trigger'");
                sbSql.Append(" ORDER BY n.nspname, p.proname");
            }

            return sbSql.ToString();
        }
    }
}