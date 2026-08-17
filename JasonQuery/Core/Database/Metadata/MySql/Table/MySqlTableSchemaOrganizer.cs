using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql.Table
{
    internal static class MySqlTableSchemaOrganizer
    {
        public static HashSet<string> Organize(MySqlTableSchemaBuildContext context)
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

            using (TraceLogger.Time($"Organize Table Info for {context.DatabaseName}"))
            {
                foreach (DataRow dr in context.TableRows)
                {
                    var name = dr.GetSafeString("TableName");
                    var key = (SchemaName: context.DatabaseName, TableName: name);
                    long rowCount = context.RowCountMap.TryGetValue(key, out var count) ? count : 0;

                    if (context.AddSchemaRow)
                    {
                        var rows = MySqlTableSchemaRowBuilder.BuildRows(context, dr, rowCount);

                        foreach (var row in rows)
                        {
                            context.SchemaTable.Rows.Add(row);
                        }
                    }

                    if (context.AutoCompleteTable && distinctNames.Add(name))
                    {
                        MyGlobal.AddAutoCompleteData(name, SchemaObjectNames.Tables);
                    }
                }
            }

            return distinctNames;
        }
    }
}