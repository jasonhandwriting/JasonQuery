using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript.Index
{
    internal static class PostgreSqlIndexCreateScriptSqlBuilder
    {
        public static string BuildIndexDefinitionSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Index Create Script");

            sbSql.AppendLine("SELECT clsIndex.relname AS IndexName,");
            sbSql.AppendLine("       pg_get_indexdef(clsIndex.oid) AS IndexDefinition,");
            sbSql.AppendLine("       pg_get_userbyid(clsIndex.relowner) AS IndexOwner");
            sbSql.AppendLine("  FROM pg_class clsIndex");
            sbSql.AppendLine("       JOIN pg_namespace ns ON (ns.oid = clsIndex.relnamespace)");
            sbSql.AppendLine(" WHERE clsIndex.relkind = 'i'");
            sbSql.AppendLine($"   AND ns.nspname = '{EscapeSql(schemaNode)}'");
            sbSql.Append($"   AND clsIndex.relname = '{EscapeSql(schemaName)}'");

            return sbSql.ToString();
        }

        private static string EscapeSql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
