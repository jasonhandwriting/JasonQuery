using JasonQuery.Core.Database.CreateScript.MySql.Formatting;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Data;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal static class MySqlCreateScriptDispatcher
    {
        public static string Build(MySqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            var script = string.Empty;

            if (request == null || executeQuery == null)
            {
                return script;
            }

            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                case SchemaObjectNames.Views:
                case SchemaObjectNames.Functions:
                case SchemaObjectNames.Triggers:
                case SchemaObjectNames.Procedures:
                    {
                        script = MySqlShowCreateScriptAssembler.Build(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Indexes:
                    {
                        script = MySqlIndexCreateScriptAssembler.Build(request, executeQuery);
                        break;
                    }
                case SchemaObjectNames.Packages:
                    {
                        script = MySqlCreateScriptExecutionHelper.BuildUnsupportedPackageText();
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
                script = MySqlCreateScriptFormatterDispatcher.Format
                (
                    request.SchemaType,
                    request.SchemaNode,
                    request.SchemaName,
                    script
                );
            }

            return script;
        }
    }
}