using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript.Index;
using JasonQuery.Core.Logging;
using System;
using System.Data;

namespace JasonQuery.Core.Database.CreateScript.Oracle.Index
{
    internal static class OracleIndexCreateScriptAssembler
    {
        #region Entry
        public static string Build(string schemaNameText, string ownerName, Func<string, DataTable> executeQuery)
        {
            var indexName = ExtractIndexName(schemaNameText);

            return BuildByIndexName(indexName, ownerName, executeQuery);
        }

        public static string BuildByIndexName(string indexName, string ownerName, Func<string, DataTable> executeQuery)
        {
            if (string.IsNullOrWhiteSpace(indexName))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(ownerName))
            {
                throw new ArgumentNullException(nameof(ownerName));
            }

            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            DataTable dtScript = null;

            try
            {
                using (TraceLogger.Time("SQL: Get Index Create Script"))
                {
                    var sql = OracleIndexCreateScriptSqlBuilder.BuildGetCreateScriptSql(ownerName, indexName);

                    dtScript = executeQuery(sql);
                }

                var scriptText = dtScript?.Rows.Count > 0 ? dtScript.Rows[0].GetSafeString("Scripts") : string.Empty;

                return NormalizeScriptText(scriptText, indexName);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtScript);
            }
        }
        #endregion

        #region Parse
        private static string ExtractIndexName(string schemaNameText)
        {
            if (string.IsNullOrWhiteSpace(schemaNameText))
            {
                return string.Empty;
            }

            var separator = MyGlobal.Separator;

            if (string.IsNullOrWhiteSpace(separator))
            {
                return schemaNameText.Trim();
            }

            var index = schemaNameText.IndexOf(separator, StringComparison.Ordinal);

            if (index < 0)
            {
                return schemaNameText.Trim();
            }

            return schemaNameText.Substring(0, index).Trim();
        }
        #endregion

        #region Normalize
        private static string NormalizeScriptText(string scriptText, string indexName)
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return string.Format("-- Oracle index create script not found: {0}", indexName);
            }

            var script = scriptText.Replace("\r\n", "\n")
                                   .Replace("\r", "\n")
                                   .Trim();

            script = script.Replace("\n", "\r\n");

            if (!script.EndsWith(";", StringComparison.Ordinal))
            {
                script += ";";
            }

            return script;
        }
        #endregion
    }
}
