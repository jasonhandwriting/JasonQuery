using JasonQuery.Core.Data;
using JasonQuery.Core.Database.QueryEditor;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.QueryEditor
{
    internal static class MySqlQueryEditorSqlBuilder
    {
        public static string BuildTableInfo(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information for 'Switch Database'");

            sbSql.AppendLine("SELECT Table_Name AS SchemaName");
            sbSql.AppendLine("  FROM Information_Schema.Tables");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaName}'");
            sbSql.AppendLine("   AND Table_Type = 'BASE TABLE'");
            sbSql.Append(" ORDER BY Table_Name");

            return sbSql.ToString();
        }

        public static string BuildViewInfo(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information for 'Switch Database'");

            sbSql.AppendLine("SELECT Table_Name AS SchemaName");
            sbSql.AppendLine("  FROM Information_Schema.Views cc");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaName}'");
            sbSql.Append(" ORDER BY Table_Name");

            return sbSql.ToString();
        }

        public static string BuildDatabaseInfoForAutoComplete()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Schema Information for 'AutoComplete' and 'Switch Database'");

            sbSql.AppendLine("SELECT Schema_Name AS Name");
            sbSql.AppendLine("  FROM Information_Schema.Schemata");
            sbSql.Append(" ORDER BY Schema_Name;");

            return sbSql.ToString();
        }

        public static string BuildTableAndViewInfoForAutoComplete(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table and View Information for 'AutoComplete' and 'Switch Database'");

            sbSql.AppendLine($"SELECT '{schemaName}' AS DB, Table_Name AS SchemaName, '{QueryEditorSchemaTypeNames.Table}' AS SchemaType");
            sbSql.AppendLine("  FROM Information_Schema.Tables");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaName}'");
            sbSql.AppendLine("   AND Table_Type = 'BASE TABLE'");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine($"SELECT '{schemaName}' AS DB, Table_Name AS SchemaName, '{QueryEditorSchemaTypeNames.View}' AS SchemaType");
            sbSql.AppendLine("  FROM Information_Schema.Views cc");
            sbSql.Append($" WHERE Table_Schema = '{schemaName}'");

            return sbSql.ToString();
        }
    }
}
