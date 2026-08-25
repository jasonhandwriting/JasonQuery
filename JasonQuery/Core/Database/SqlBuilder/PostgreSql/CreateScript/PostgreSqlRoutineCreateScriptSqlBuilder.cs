using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript
{
    internal static class PostgreSqlRoutineCreateScriptSqlBuilder
    {
        /// <summary>
        /// PostgreSQL 11 以下：取得 Function 定義
        /// </summary>
        public static string BuildFunctionDefinitionSql_Pre11(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Definition Script (PostgreSQL version < 11)");

            sbSql.AppendLine("SELECT s.Definition");
            sbSql.AppendLine("  FROM (SELECT PG_GET_FUNCTIONDEF(p.oid) AS Definition");
            sbSql.AppendLine("          FROM pg_proc p");
            sbSql.AppendLine("               JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine("         WHERE NOT p.proisagg");
            sbSql.AppendLine($"           AND p.proname = '{schemaName}'");
            sbSql.AppendLine("           AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine($"           AND n.nspname = '{schemaNode}'");
            sbSql.Append("           AND t.typname <> 'trigger') s");

            return sbSql.ToString();
        }

        /// <summary>
        /// PostgreSQL 11 以上：取得 Function 定義 (不指定參數)
        /// </summary>
        public static string BuildFunctionDefinitionSql_Post11(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Definition Script (PostgreSQL version >= 11)");

            sbSql.AppendLine("SELECT s.Definition");
            sbSql.AppendLine("  FROM (SELECT PG_GET_FUNCTIONDEF(p.oid) AS Definition");
            sbSql.AppendLine("          FROM pg_proc p");
            sbSql.AppendLine("               JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine("         WHERE p.prokind <> 'p'");
            sbSql.AppendLine($"           AND p.proname = '{schemaName}'");
            sbSql.AppendLine("           AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine($"           AND n.nspname = '{schemaNode}'");
            sbSql.Append("           AND t.typname <> 'trigger') s");

            return sbSql.ToString();
        }

        /// <summary>
        /// PostgreSQL 11 以上：取得指定參數簽章的 Function 定義
        /// </summary>
        public static string BuildSpecificFunctionDefinitionSql_Post11(string schemaNode, string schemaName, string functionArgs)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Definition Script (PostgreSQL version >= 11)");

            sbSql.AppendLine("SELECT s.Definition");
            sbSql.AppendLine("  FROM (SELECT PG_GET_FUNCTIONDEF(p.oid) AS Definition");
            sbSql.AppendLine("          FROM pg_proc p");
            sbSql.AppendLine("               JOIN pg_type t ON p.prorettype = t.oid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_description d ON p.oid = d.objoid");
            sbSql.AppendLine("               LEFT OUTER JOIN pg_namespace n ON n.oid = p.pronamespace");
            sbSql.AppendLine("         WHERE p.prokind <> 'p'");
            sbSql.AppendLine($"           AND p.proname = '{schemaName}'");
            sbSql.AppendLine($"           AND pg_get_function_identity_arguments(p.oid) = '{functionArgs}'");
            sbSql.AppendLine("           AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine($"           AND n.nspname = '{schemaNode}'");
            sbSql.Append("           AND t.typname <> 'trigger') s");

            return sbSql.ToString();
        }

        public static string BuildProcedureDefinitionSql_Post11(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Create Script");

            sbSql.AppendLine("SELECT pg_get_functiondef(p.oid) AS RoutineDefinition");
            sbSql.AppendLine("  FROM pg_proc p");
            sbSql.AppendLine("       JOIN pg_namespace n ON (n.oid = p.pronamespace)");
            sbSql.AppendLine(" WHERE p.prokind = 'p'");
            sbSql.AppendLine($"   AND n.nspname = '{EscapeSql(schemaNode)}'");
            sbSql.Append($"   AND p.proname = '{EscapeSql(schemaName)}'");

            return sbSql.ToString();
        }

        public static string BuildSpecificProcedureDefinitionSql_Post11(string schemaNode, string schemaName, string functionArgs)
        {
            var functionArgsNew = EscapeSql(functionArgs);
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, $"---Get Specific Procedure Create Script ({functionArgsNew})");

            sbSql.AppendLine("SELECT pg_get_functiondef(p.oid) AS RoutineDefinition");
            sbSql.AppendLine("  FROM pg_proc p");
            sbSql.AppendLine("       JOIN pg_namespace n ON (n.oid = p.pronamespace)");
            sbSql.AppendLine(" WHERE p.prokind = 'p'");
            sbSql.AppendLine($"   AND n.nspname = '{EscapeSql(schemaNode)}'");
            sbSql.AppendLine($"   AND p.proname = '{EscapeSql(schemaName)}'");
            sbSql.Append($"   AND pg_get_function_identity_arguments(p.oid) = '{functionArgsNew}'");

            return sbSql.ToString();
        }

        private static string EscapeSql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
