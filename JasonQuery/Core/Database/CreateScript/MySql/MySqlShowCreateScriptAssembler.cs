using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal static class MySqlShowCreateScriptAssembler
    {
        public static string Build(MySqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var sql = MySqlCreateScriptSqlBuilder.BuildShowCreateSql(request);
            var dtTemp = MySqlCreateScriptExecutionHelper.ExecuteSql(sql, executeQuery);

            if (dtTemp == null || dtTemp.Rows.Count <= 0)
            {
                return string.Empty;
            }

            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        return dtTemp.Rows[0].GetSafeString("Create Table");
                    }
                case SchemaObjectNames.Views:
                    {
                        return BuildViewScript(dtTemp, request);
                    }
                case SchemaObjectNames.Functions:
                    {
                        return dtTemp.Rows[0].GetSafeString("Create Function");
                    }
                case SchemaObjectNames.Triggers:
                    {
                        return dtTemp.Rows[0].GetSafeString("Create Trigger");
                    }
                case SchemaObjectNames.Procedures:
                    {
                        return dtTemp.Rows[0].GetSafeString("Create Procedure");
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static string BuildViewScript(DataTable dtTemp, MySqlCreateScriptRequest request)
        {
            var sqlPane = dtTemp.Rows[0].GetSafeString("Create View");
            var replaceOld = $" VIEW `{request.SchemaNode}`.`{request.SchemaName}` AS ";
            var replaceNew = $"\r\nVIEW `{request.SchemaNode}`.`{request.SchemaName}`\r\nAS\r\n";

            sqlPane = sqlPane.Replace(" DEFINER=", "\r\n    DEFINER=")
                             .Replace(" SQL SECURITY ", "\r\nSQL SECURITY ")
                             .Replace(replaceOld, replaceNew);

            return sqlPane;
        }
    }
}