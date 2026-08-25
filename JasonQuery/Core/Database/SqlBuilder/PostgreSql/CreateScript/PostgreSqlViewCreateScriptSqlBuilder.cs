using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript
{
    internal static class PostgreSqlViewCreateScriptSqlBuilder
    {
        public static string BuildViewDefinitionSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Definition Script");

            sbSql.AppendLine($"SELECT 'CREATE OR REPLACE VIEW ' || quote_ident('{schemaNode}') || '.' || quote_ident('{schemaName}') || '\r\n AS\r\n' || pg_get_viewdef(c.oid, true) AS Definition");
            sbSql.AppendLine("  FROM pg_class c");
            sbSql.AppendLine("       JOIN pg_namespace n ON n.oid = c.relnamespace");
            sbSql.AppendLine($" WHERE n.nspname = '{schemaNode}'");
            sbSql.AppendLine($"   AND c.relname = '{schemaName}'");
            sbSql.Append("   AND c.relkind = 'v'");

            return sbSql.ToString();
        }

        public static string BuildViewOwnerSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Creation Script(Owner)");

            //20250427 取得 Owner 訊息
            sbSql.AppendLine("SELECT 'ALTER TABLE ' || quote_ident(n.nspname) || '.' || quote_ident(c.relname) || '\r\nOWNER TO ' || r.rolname || ';'");
            sbSql.AppendLine("  FROM pg_class c");
            sbSql.AppendLine("       JOIN pg_namespace n ON n.oid = c.relnamespace");
            sbSql.AppendLine("       JOIN pg_roles r ON r.oid = c.relowner");
            sbSql.AppendLine($" WHERE n.nspname = '{schemaNode}'");
            sbSql.AppendLine($"   AND c.relname = '{schemaName}'");
            sbSql.Append("   AND c.relkind = 'v'");

            return sbSql.ToString();
        }
    }
}
