using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Trigger
{
    internal static class MySqlTriggerSchemaOrganizer
    {
        public static HashSet<string> Organize(MySqlTriggerSchemaBuildContext context)
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

            using (TraceLogger.Time($"Organize Trigger Info for {context.DatabaseName}"))
            {
                foreach (DataRow dr in context.TriggerRows)
                {
                    var name = dr.GetSafeString("TriggerName");

                    if (context.AddSchemaRow)
                    {
                        var row = MySqlTriggerSchemaRowBuilder.BuildRow(context, dr);

                        if (row != null)
                        {
                            context.SchemaTable.Rows.Add(row);
                        }
                    }

                    if (context.AutoCompleteTrigger && distinctNames.Add(name))
                    {
                        MyGlobal.AddAutoCompleteData(name, SchemaObjectNames.Triggers);
                    }
                }
            }

            return distinctNames;
        }
    }
}