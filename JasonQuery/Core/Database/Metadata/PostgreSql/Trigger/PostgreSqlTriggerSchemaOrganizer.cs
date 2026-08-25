using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Trigger
{
    internal static class PostgreSqlTriggerSchemaOrganizer
    {
        public static HashSet<string> Organize(PostgreSqlTriggerSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SourceTriggerSchemaInfo == null)
            {
                throw new ArgumentNullException(nameof(context.SourceTriggerSchemaInfo));
            }

            if (context.AddSchemaRow && context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            var triggerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processed = 0;

            if (context.AddSchemaRow)
            {
                context.TargetSchemaTable.BeginLoadData();
            }

            try
            {
                foreach (DataRow dr in context.SourceTriggerSchemaInfo.AsEnumerable())
                {
                    var schemaName = dr.GetSafeString("DbName");
                    var triggerName = dr.GetSafeString("TriggerName");

                    if (context.AddSchemaRow)
                    {
                        var triggerCountInSchema = SchemaObjectCountResolver.Resolve(context.TriggerCountMap, schemaName);
                        var triggerSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Triggers, triggerCountInSchema);
                        var row = PostgreSqlTriggerSchemaRowBuilder.Build(context.TargetSchemaTable, context.DbConnectionName,
                                                                          schemaName, triggerSchemaType, triggerName);

                        context.TargetSchemaTable.Rows.Add(row);
                    }

                    if (!string.IsNullOrWhiteSpace(triggerName))
                    {
                        triggerNames.Add(triggerName);
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

            return triggerNames;
        }
    }
}
