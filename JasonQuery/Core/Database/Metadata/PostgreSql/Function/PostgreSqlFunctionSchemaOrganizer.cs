using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Function
{
    internal static class PostgreSqlFunctionSchemaOrganizer
    {
        public static HashSet<string> Organize(PostgreSqlFunctionSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SourceFunctionSchemaInfo == null)
            {
                throw new ArgumentNullException(nameof(context.SourceFunctionSchemaInfo));
            }

            if (context.AddSchemaRow && context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            var functionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processed = 0;

            if (context.AddSchemaRow)
            {
                context.TargetSchemaTable.BeginLoadData();
            }

            try
            {
                foreach (DataRow dr in context.SourceFunctionSchemaInfo.AsEnumerable())
                {
                    var schemaName = dr.GetSafeString("DbName");
                    var functionName = dr.GetSafeString("FunctionName");
                    var displayName = dr.GetSafeString("FunctionDisplayName");

                    if (context.AddSchemaRow)
                    {
                        var functionCountInSchema = SchemaObjectCountResolver.Resolve(context.FunctionCountMap, schemaName);
                        var functionSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Functions, functionCountInSchema);
                        var row = PostgreSqlFunctionSchemaRowBuilder.Build(context.TargetSchemaTable, context.DbConnectionName,
                                                                           schemaName, functionSchemaType, displayName);

                        context.TargetSchemaTable.Rows.Add(row);
                    }

                    if (!string.IsNullOrWhiteSpace(functionName))
                    {
                        functionNames.Add(functionName);
                    }

                    processed++;

                    if (context.DoEventsInterval > 0 && processed % context.DoEventsInterval == 0)
                    {
                        Application.DoEvents();
                    }
                }
            }
            finally
            {
                if (context.AddSchemaRow)
                {
                    context.TargetSchemaTable.EndLoadData();
                }
            }

            return functionNames;
        }
    }
}
