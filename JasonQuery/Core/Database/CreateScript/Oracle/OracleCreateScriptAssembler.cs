using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript;
using JasonQuery.Core.Logging;
using System;
using System.Data;

namespace JasonQuery.Core.Database.CreateScript.Oracle
{
    internal static class OracleCreateScriptAssembler
    {
        #region Entry
        public static string Build(DataRow drSchema, string ownerName, Func<string, DataTable> executeQuery)
        {
            if (drSchema == null)
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

            var schemaTypeText = drSchema.GetSafeString("SchemaType");
            var schemaNameText = drSchema.GetSafeString("SchemaName");

            var ddlObjectType = ResolveDdlObjectType(schemaTypeText, schemaNameText);
            var objectName = ExtractObjectName(schemaNameText);

            if (string.IsNullOrWhiteSpace(ddlObjectType) || string.IsNullOrWhiteSpace(objectName))
            {
                return string.Empty;
            }

            DataTable dtScript = null;

            try
            {
                using (TraceLogger.Time($"SQL: Get Oracle {ddlObjectType} Create Script"))
                {
                    var sql = OracleCreateScriptSqlBuilder.BuildGetCreateScriptSql(ownerName, ddlObjectType, objectName);

                    dtScript = executeQuery(sql);
                }

                var scriptText = dtScript?.Rows.Count > 0 ? dtScript.Rows[0].GetSafeString("Scripts") : string.Empty;

                return NormalizeScriptText(scriptText, ddlObjectType, objectName);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtScript);
            }
        }
        #endregion

        #region Resolve
        private static string ResolveDdlObjectType(string schemaTypeText, string schemaNameText)
        {
            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Tables))
            {
                return "TABLE";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Views))
            {
                return "VIEW";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Functions))
            {
                return "FUNCTION";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Procedures))
            {
                return "PROCEDURE";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Triggers))
            {
                return "TRIGGER";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Indexes))
            {
                return "INDEX";
            }

            if (SchemaObjectTypeHelper.StartsWith(schemaTypeText, SchemaObjectNames.Packages))
            {
                return ResolvePackageDdlObjectType(schemaNameText);
            }

            return string.Empty;
        }

        private static string ResolvePackageDdlObjectType(string schemaNameText)
        {
            if (string.IsNullOrWhiteSpace(schemaNameText))
            {
                return "PACKAGE";
            }

            return schemaNameText.IndexOf("(Body)", StringComparison.OrdinalIgnoreCase) >= 0 ? "PACKAGE_BODY" : "PACKAGE";
        }
        #endregion

        #region Parse
        private static string ExtractObjectName(string schemaNameText)
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
        private static string NormalizeScriptText(string scriptText, string ddlObjectType, string objectName)
        {
            if (string.IsNullOrWhiteSpace(scriptText))
            {
                return string.Format("-- Oracle {0} create script not found: {1}", ddlObjectType, objectName);
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
