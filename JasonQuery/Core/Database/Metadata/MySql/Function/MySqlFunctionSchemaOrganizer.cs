using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Function
{
    internal static class MySqlFunctionSchemaOrganizer
    {
        public static HashSet<string> Organize(MySqlFunctionSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.SchemaTable));
            }

            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (TraceLogger.Time($"Organize Function Info for {context.DatabaseName}"))
            {
                foreach (DataRow dr in context.FunctionRows)
                {
                    var name = dr.GetSafeString("FunctionName");

                    if (context.AddSchemaRow)
                    {
                        var row = MySqlFunctionSchemaRowBuilder.BuildRow(context, name);

                        if (row != null)
                        {
                            context.SchemaTable.Rows.Add(row);
                        }
                    }

                    if (context.AutoCompleteFunction && distinctNames.Add(name))
                    {
                        MyGlobal.AddAutoCompleteData(name, AutoCompleteSourceNames.Functions);
                    }
                }
            }

            return distinctNames;
        }
    }
}
