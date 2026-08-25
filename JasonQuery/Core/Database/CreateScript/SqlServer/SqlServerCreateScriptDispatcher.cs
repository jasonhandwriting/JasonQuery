using JasonQuery.Core.Database.CreateScript.SqlServer.Formatting;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal static class SqlServerCreateScriptDispatcher
    {
        public static string Build(SqlServerCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var script = string.Empty;

            if (request == null || executeQuery == null)
            {
                return script;
            }

            var sbSqlLog = new StringBuilder();

            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        script = SqlServerTableCreateScriptAssembler.Build(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        script = SqlServerModuleCreateScriptAssembler.Build(request, executeQuery, "V", "View");
                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        script = SqlServerModuleCreateScriptAssembler.Build(request, executeQuery, "FN", "Function");
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        script = SqlServerModuleCreateScriptAssembler.Build(request, executeQuery, "TR", "Trigger");
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        script = SqlServerModuleCreateScriptAssembler.Build(request, executeQuery, "P", "Procedure");
                        break;
                    }
                case SchemaObjectNames.Indexes:
                    {
                        script = SqlServerIndexCreateScriptAssembler.Build(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Packages:
                    {
                        script = SqlServerCreateScriptExecutionHelper.BuildUnsupportedPackageText();
                        break;
                    }
                default:
                    {
                        script = string.Empty;
                        break;
                    }
            }

            if (!string.IsNullOrWhiteSpace(script))
            {
                script = SqlServerCreateScriptFormatterDispatcher.Format
                (
                    request.SchemaType,
                    request.SchemaNode,
                    request.SchemaDbo,
                    request.SchemaName,
                    script
                );
            }

            return script;
        }
    }
}
