using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript
{
    internal static class PostgreSqlTriggerCreateScriptSqlBuilder
    {
        public static string BuildTriggerDefinitionSql11(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Function Creation Script (PostgreSQL version >= 11)");

            sbSql.AppendLine("SELECT s.Definition");
            sbSql.AppendLine("  FROM (SELECT PG_GET_FUNCTIONDEF(p.oid) AS Definition");
            sbSql.AppendLine("          FROM pg_proc p");
            sbSql.AppendLine("               JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine("         WHERE p.prokind IN ('f', 'a')"); //Function 或 Aggregate 都算，Trigger 用 typname 區分
            sbSql.AppendLine("           AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("           AND t.typname = 'trigger'"); //只抓傳回型別是 Trigger 的 Function
            sbSql.AppendLine($"           AND n.nspname = '{schemaNode}'");
            sbSql.Append($"           AND p.proname = '{schemaName}') s");

            return sbSql.ToString();
        }

        public static string BuildTriggerDefinitionSql10(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Function Creation Script (PostgreSQL version < 11)");

            sbSql.AppendLine("SELECT s.Definition");
            sbSql.AppendLine("  FROM (SELECT PG_GET_FUNCTIONDEF(p.oid) AS Definition");
            sbSql.AppendLine("          FROM pg_proc p");
            sbSql.AppendLine("               JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine("         WHERE NOT p.proisagg");  //不是 Aggregate Function
            sbSql.AppendLine("           AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine("           AND t.typname = 'trigger'");  //只抓傳回 Trigger 的 Function
            sbSql.AppendLine($"           AND n.nspname = '{schemaNode}'");
            sbSql.Append($"           AND p.proname = '{schemaName}') s");

            return sbSql.ToString();
        }
    }
}