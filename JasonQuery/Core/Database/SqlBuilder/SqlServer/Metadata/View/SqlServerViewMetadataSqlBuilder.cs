using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.View
{
    internal sealed class SqlServerViewMetadataSqlBuilder
    {
        public string Build(string databaseName, string excludeIsMsShippedClause)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName, SCHEMA_NAME(Schema_ID) AS Schema_Dbo, o.*");
            sbSql.AppendLine($"  FROM {databaseName}.sys.All_Objects o");
            sbSql.AppendLine($" WHERE o.Type = 'V'{excludeIsMsShippedClause}");
            sbSql.Append(" ORDER BY Name;");

            return sbSql.ToString();
        }

        public static string BuildViewObjectId(string schemaNode, string schemaDbo, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Object ID");

            sbSql.AppendLine("SELECT o.*");
            sbSql.AppendLine($"  FROM {schemaNode}.sys.All_Objects o");
            sbSql.AppendLine($"       INNER JOIN {schemaNode}.sys.Schemas s ON o.Schema_ID = s.Schema_ID");
            sbSql.AppendLine(" WHERE o.Type = 'V'");
            sbSql.AppendLine($"   AND o.Name = '{schemaName}'");
            sbSql.Append($"   AND s.Name = '{schemaDbo}';");

            return sbSql.ToString();
        }

        public static string BuildViewColumnInfo(string schemaNode, string schemaDbo, string objectId)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Column Information");

            sbSql.AppendLine("SELECT '1' AS \" \", c.Name AS Column_Name,");
            sbSql.AppendLine("       c.Column_ID,");
            sbSql.AppendLine("       TYPE_NAME(User_Type_ID) AS TypeName,");
            sbSql.AppendLine("       TYPE_NAME(User_Type_ID) AS DataTypeName,");
            sbSql.AppendLine("       TYPE_NAME(User_Type_ID) AS DataType,");
            sbSql.AppendLine("       c.Max_Length AS ColumnSize,");
            sbSql.AppendLine("       c.Scale AS NumericScale,");
            sbSql.AppendLine("       c.Precision AS NumericPrecision");
            sbSql.AppendLine($"  FROM {schemaNode}.sys.Columns c");
            sbSql.AppendLine($"       JOIN {schemaNode}.sys.Views v ON v.Object_ID = c.Object_ID");
            sbSql.AppendLine($"       JOIN {schemaNode}.sys.Schemas s ON v.Schema_ID = s.Schema_ID");
            sbSql.AppendLine($" WHERE c.Object_ID = {objectId}");
            sbSql.AppendLine($"   AND s.Name = '{schemaDbo}'");
            sbSql.Append(" ORDER BY c.Column_ID");

            return sbSql.ToString();
        }
    }
}