using JasonQuery.Core.Data;
using JasonQuery.Core.Database.CreateScript.SqlServer;
using System;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Module
{
    internal static class SqlServerModuleCreateScriptSqlBuilder
    {
        public static string BuildModuleDefinitionSql(SqlServerCreateScriptRequest request, string objectType)
        {
            var sb = new StringBuilder();

            SqlTraceHelper.AppendHeader(sb, $"---Get Module({objectType}) Definition");

            sb.AppendLine("SELECT m.Definition, o.*");
            sb.AppendLine($"  FROM {request.SchemaNode}.sys.All_Objects o");
            sb.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.All_Sql_Modules m ON o.Object_ID = m.Object_ID");

            if (string.Equals(objectType, "P", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(" WHERE o.Type IN ('P', 'RF', 'PC', 'X')");
            }
            else if (string.Equals(objectType, "FN", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine(" WHERE o.Type IN ('FN', 'IF', 'TF')");
            }
            else
            {
                sb.AppendLine($" WHERE o.Type = '{objectType}'");
            }

            sb.AppendLine($"   AND SCHEMA_NAME(o.Schema_ID) = '{request.SchemaDbo}'");

            if (request.ExcludeNativeDatabase)
            {
                sb.AppendLine("   AND o.Is_Ms_Shipped <> 1");
            }

            sb.Append($"   AND o.Name = '{request.SchemaNameWithoutSchemaDbo}';");

            return sb.ToString();
        }
    }
}