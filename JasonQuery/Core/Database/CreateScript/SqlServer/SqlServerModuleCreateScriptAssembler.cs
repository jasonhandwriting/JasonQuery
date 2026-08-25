using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Module;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal static class SqlServerModuleCreateScriptAssembler
    {
        public static string Build(SqlServerCreateScriptRequest request, Func<string, DataTable> executeQuery,
                                   string objectType, string headerText)
        {
            var sql = SqlServerModuleCreateScriptSqlBuilder.BuildModuleDefinitionSql(request, objectType);
            var dtInfo = SqlServerCreateScriptExecutionHelper.ExecuteAndLog(sql, executeQuery);

            if (dtInfo?.Rows.Count <= 0)
            {
                return string.Empty;
            }

            var dr = dtInfo.Rows[0];
            var createDate = dr.GetSafeDateTimeText("Create_Date", request.DateTimeFormat);
            var modifyDate = dr.GetSafeDateTimeText("Modify_Date", request.DateTimeFormat);
            var definition = dr.GetSafeString("Definition");
            var sb = new StringBuilder();

            sb.AppendLine($"-- {headerText}: {request.SchemaDbo}.{request.SchemaName}");
            sb.AppendLine($"-- Created: {createDate}");
            sb.AppendLine($"-- Modified: {modifyDate}");
            sb.AppendLine();
            sb.Append(definition);

            if (!definition.TrimEnd().EndsWith("GO", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine();
                sb.Append("GO");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
    }
}
