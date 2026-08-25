using JasonQuery.Core.Data;
using JasonQuery.Core.Database.QueryEditor;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.QueryEditor
{
    internal static class PostgreSqlQueryEditorSqlBuilder
    {
        public static string BuildTableAndViewNameSql()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get all Table Name and View Name for Query Editor");

            sbSql.AppendLine("SELECT n.nspname AS SchemaNode,");
            sbSql.AppendLine("       CASE c.relkind");
            sbSql.AppendLine($"            WHEN 'r' THEN '{QueryEditorSchemaTypeNames.Table}'");
            sbSql.AppendLine($"            WHEN 'v' THEN '{QueryEditorSchemaTypeNames.View}'");
            sbSql.AppendLine("       END AS SchemaType,");
            sbSql.AppendLine("       c.relname AS SchemaName");
            sbSql.AppendLine("  FROM pg_catalog.pg_class c");
            sbSql.AppendLine("       JOIN pg_catalog.pg_namespace n");
            sbSql.AppendLine("         ON n.oid = c.relnamespace");
            sbSql.AppendLine(" WHERE c.relkind IN ('r', 'v')");
            sbSql.AppendLine("   AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.Append(" ORDER BY SchemaNode, SchemaType, SchemaName");

            return sbSql.ToString();
        }

        public static string BuildSchemaInfo()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get SchemaName Information");

            sbSql.AppendLine("SELECT DISTINCT SchemaNode");
            sbSql.AppendLine("  FROM (SELECT Table_Schema AS SchemaNode");
            sbSql.AppendLine("          FROM Information_Schema.Tables");
            sbSql.AppendLine("         WHERE Table_Type = 'BASE TABLE'");
            sbSql.AppendLine("           AND Table_Schema NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine("SELECT SchemaName AS SchemaNode");
            sbSql.AppendLine("  FROM pg_catalog.pg_views");
            sbSql.AppendLine("  WHERE SchemaName NOT IN ('pg_catalog', 'information_schema')) ss");
            sbSql.Append(" ORDER BY SchemaNode");

            return sbSql.ToString();
        }

        public static string BuildTableInfo(string schemaNode)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information");

            sbSql.AppendLine("SELECT Table_Name AS SchemaName");
            sbSql.AppendLine("  FROM Information_Schema.Tables");
            sbSql.AppendLine(" WHERE Table_Type = 'BASE TABLE'");
            sbSql.AppendLine("   AND Table_Schema NOT IN ('pg_catalog', 'information_schema')");
            sbSql.Append(string.IsNullOrWhiteSpace(schemaNode) ? string.Empty : $"   AND Table_Schema = '{schemaNode}'\r\n");
            sbSql.Append(" ORDER BY Table_Name");

            return sbSql.ToString();
        }

        public static string BuildViewInfo(string schemaNode = "")
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT ViewName AS SchemaName");
            sbSql.AppendLine("  FROM pg_catalog.pg_views");
            sbSql.AppendLine(" WHERE SchemaName NOT IN ('pg_catalog', 'information_schema')");
            sbSql.Append(" ORDER BY ViewName");

            return sbSql.ToString();
        }
    }
}
