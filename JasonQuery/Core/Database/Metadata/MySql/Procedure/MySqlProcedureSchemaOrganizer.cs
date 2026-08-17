using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Procedure
{
    internal static class MySqlProcedureSchemaOrganizer
    {
        public static void Organize(MySqlProcedureSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.SchemaTable));
            }

            using (TraceLogger.Time($"Organize Procedure Info for {context.DatabaseName}"))
            {
                foreach (DataRow dr in context.ProcedureRows)
                {
                    var name = dr.GetSafeString("ProcedureName");

                    if (!context.AddSchemaRow)
                    {
                        continue;
                    }

                    var row = MySqlProcedureSchemaRowBuilder.BuildRow(context, name);

                    if (row != null)
                    {
                        context.SchemaTable.Rows.Add(row);
                    }
                }
            }
        }
    }
}