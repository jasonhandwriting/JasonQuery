using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.View
{
    internal static class MySqlViewSchemaOrganizer
    {
        public static HashSet<string> Organize(MySqlViewSchemaBuildContext context)
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

            using (TraceLogger.Time($"Organize View Info for {context.DatabaseName}"))
            {
                foreach (DataRow dr in context.ViewRows)
                {
                    var name = dr.GetSafeString("ViewName");

                    if (context.AddSchemaRow)
                    {
                        var rows = MySqlViewSchemaRowBuilder.BuildRows(context, name);

                        foreach (var row in rows)
                        {
                            context.SchemaTable.Rows.Add(row);
                        }
                    }

                    if (context.AutoCompleteView && distinctNames.Add(name))
                    {
                        MyGlobal.AddAutoCompleteData(name, SchemaObjectNames.Views);
                    }
                }
            }

            return distinctNames;
        }
    }
}
