using JasonLibrary.Core;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript;
using JasonQuery.Core.Logging;
using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.Oracle
{
    internal static class OracleCreateScriptDispatcher
    {
        public static string Build(string schemaType, string schemaName, string packageSpecBody, string owner,
                                   string defaultOwner, Func<string, DataTable> executeQuery)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            var request = OracleCreateScriptRequest.Create
            (
                schemaType,
                schemaName,
                packageSpecBody,
                owner,
                defaultOwner
            );

            var scriptHeader = BuildScriptHeader(request, executeQuery);
            var scriptBody = BuildScriptBody(request, executeQuery);

            return string.Concat(scriptHeader, scriptBody);
        }

        private static string BuildScriptHeader(OracleCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            DataTable dtTemp = null;

            try
            {
                using (TraceLogger.Time("SQL: Get Object Creation Information"))
                {
                    var sql = OracleCreateScriptSqlBuilder.BuildGetObjectHeaderSql(request);

                    dtTemp = executeQuery(sql);
                }

                if (dtTemp?.Rows.Count <= 0)
                {
                    return string.Empty;
                }

                var dr = dtTemp.Rows[0];
                var status = dr.GetSafeString("Status");
                var createdRaw = dr.GetSafeString("Created");
                var createdText = createdRaw;

                if (DateTime.TryParse(createdRaw, out var dtCreated))
                {
                    createdText = dtCreated.ToString($"{MyLibrary.DateFormat} HH:mm:ss");
                }

                var sb = new StringBuilder();

                sb.AppendLine($"--{request.HeaderObjectText}: \"{request.Owner}\".\"{request.SchemaName}\"");
                sb.AppendLine($"--Status: {status}");
                sb.AppendLine($"--Created: {createdText}");
                sb.AppendLine();

                return sb.ToString();
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtTemp);
            }
        }

        private static string BuildScriptBody(OracleCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            DataTable dtTemp = null;

            try
            {
                using (TraceLogger.Time("SQL: Get Object Creation Script"))
                {
                    var sql = OracleCreateScriptSqlBuilder.BuildGetObjectCreateScriptSql(request);

                    dtTemp = executeQuery(sql);
                }

                if (dtTemp?.Rows.Count <= 0)
                {
                    return string.Empty;
                }

                var script = dtTemp.Rows[0].GetSafeString("ScriptText");

                script = NormalizeScriptText(script);

                if (string.Equals(request.DdlObjectType, "TABLE", StringComparison.OrdinalIgnoreCase))
                {
                    script = OracleCreateTableScriptBeautifier.Beautify
                    (
                        script,
                        request.SchemaName,
                        request.Owner,
                        executeQuery
                    );
                }
                else
                {
                    script = script.TrimStart('\r', '\n', ' ')
                                     .TrimEnd(' ');
                }

                return script;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtTemp);
            }
        }

        private static string NormalizeScriptText(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            script = Regex.Replace(script, @"(?<!\r)\n", "\r\n");

            return script.TrimStart('\r', '\n');
        }
    }
}