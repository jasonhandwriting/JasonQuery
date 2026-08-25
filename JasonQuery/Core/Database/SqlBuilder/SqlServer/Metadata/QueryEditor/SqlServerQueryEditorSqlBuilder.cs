using JasonQuery.Core.Data;
using JasonQuery.Core.Database.QueryEditor;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.QueryEditor
{
    internal static class SqlServerQueryEditorSqlBuilder
    {
        public static string BuildSchemaInfo()
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get SchemaName Information");

            sbSql.AppendLine("SELECT DISTINCT s.Name AS SchemaName");
            sbSql.AppendLine("  FROM sys.Schemas s");
            sbSql.AppendLine("       JOIN sys.Objects o ON s.Schema_ID = o.Schema_ID");
            sbSql.Append(" ORDER BY s.Name");

            return sbSql.ToString();
        }

        public static string BuildTableInfo(string databaseName, string schema)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information for 'Switch Database'");

            sbSql.AppendLine("SELECT o.Name AS SchemaName");
            sbSql.AppendLine($"  FROM {databaseName}.sys.Objects o");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Schemas s ON o.Schema_ID = s.Schema_ID");
            sbSql.AppendLine(" WHERE o.Type = 'U'");
            sbSql.AppendLine($"   AND s.Name = '{schema}'");
            sbSql.Append(" ORDER BY o.Name;");

            return sbSql.ToString();
        }

        public static string BuildViewInfo(string databaseName, string schema)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information for 'Switch Database'");

            sbSql.AppendLine("SELECT o.Name AS SchemaName");
            sbSql.AppendLine($"  FROM {databaseName}.sys.All_Objects o");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Schemas s ON o.Schema_ID = s.schema_ID");
            sbSql.AppendLine(" WHERE o.Type = 'V'");
            sbSql.AppendLine("   AND o.Is_Ms_Shipped = 0");
            sbSql.AppendLine($"   AND s.Name = '{schema}'");
            sbSql.Append(" ORDER BY o.Name");

            return sbSql.ToString();
        }

        public static string BuildDatabaseInfoForAutoComplete(bool excludeNativeDatabase)
        {
            var sbSql = new StringBuilder();
            var excludeClause = excludeNativeDatabase ? "\r\n WHERE Name NOT IN ('master', 'model', 'msdb', 'tempdb')" : string.Empty;

            SqlTraceHelper.AppendHeader(sbSql, "---Get Schema Information for AutoComplete (Switch Database)");

            sbSql.AppendLine("SELECT Name");
            sbSql.AppendLine($"  FROM master.sys.Databases{excludeClause}");
            sbSql.Append(" ORDER BY Name;");

            return sbSql.ToString();
        }

        public static string BuildTableAndViewInfoForAutoComplete(string schemaName, bool excludeNativeDatabase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table and View Information for 'AutoComplete' and 'Switch Database'");

            sbSql.AppendLine($"SELECT '{schemaName}' AS DB, SCHEMA_NAME(Schema_ID) AS SchemaNode, o.Name AS SchemaName, '{QueryEditorSchemaTypeNames.Table}' AS SchemaType");
            sbSql.AppendLine($"  FROM {schemaName}.sys.Objects o");
            sbSql.AppendLine(" WHERE Type = 'U'");
            sbSql.AppendLine("   AND Is_Ms_Shipped <> 1");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine($"SELECT '{schemaName}' AS DB, SCHEMA_NAME(Schema_ID) AS SchemaNode, o.Name AS SchemaName, '{QueryEditorSchemaTypeNames.View}' AS SchemaType");
            sbSql.AppendLine($"  FROM {schemaName}.sys.All_Objects o");
            sbSql.AppendLine(" WHERE Type = 'V'");
            sbSql.Append("   AND Is_Ms_Shipped <> 1");

            var sql = sbSql.ToString();

            if (!excludeNativeDatabase)
            {
                sql = sql.Replace("\r\n   AND Is_Ms_Shipped <> 1", string.Empty);
            }

            return sql;
        }
    }
}
