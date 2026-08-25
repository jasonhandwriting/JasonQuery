using JasonQuery.Core.Data;
using JasonQuery.Core.Database.QueryEditor;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor
{
    internal static class OracleQueryEditorSqlBuilder
    {
        public static string BuildTableAndViewNameSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table and View Information");

            sbSql.AppendLine($"SELECT '' AS SchemaNode, '{QueryEditorSchemaTypeNames.Table}' AS SchemaType, Table_Name AS SchemaName, UPPER(Owner) AS Schema");
            sbSql.AppendLine("  FROM All_Tables");
            sbSql.AppendLine($" WHERE UPPER(Owner) = '{ownerUppercase}'");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine($"SELECT '' AS SchemaNode, '{QueryEditorSchemaTypeNames.View}' AS SchemaType, View_Name AS SchemaName, UPPER(Owner) AS Schema");
            sbSql.AppendLine("  FROM All_Views");
            sbSql.AppendLine($" WHERE UPPER(Owner) = '{ownerUppercase}'");
            sbSql.AppendLine(" ORDER BY SchemaType, SchemaName");

            return sbSql.ToString();
        }

        public static string BuildTableInfo(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information");

            sbSql.AppendLine("SELECT Object_Name AS SchemaName");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'TABLE'");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{schemaName}'");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }

        public static string BuildViewInfo(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT Object_Name AS SchemaName");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'VIEW'");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{schemaName}'");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }
    }
}
