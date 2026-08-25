using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.View
{
    internal static class PostgreSqlViewSchemaOrganizer
    {
        public static HashSet<string> Organize(PostgreSqlViewSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SourceViewSchemaInfo == null)
            {
                throw new ArgumentNullException(nameof(context.SourceViewSchemaInfo));
            }

            if (context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            var viewNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var columns = context.TargetSchemaTable.Columns;
            var processed = 0;

            context.TargetSchemaTable.BeginLoadData();

            try
            {
                foreach (DataRow dr in context.SourceViewSchemaInfo.AsEnumerable())
                {
                    var schemaName = dr.GetSafeString("DbName");
                    var viewName = dr.GetSafeString("ViewName");
                    var columnName = dr.GetSafeString("ColumnName");
                    var columnType = dr.GetSafeString("ColumnType");
                    var viewCountInSchema = dr.GetSafeInt("ViewCountInSchema");

                    var viewSchemaType = PostgreSqlViewSchemaTypeBuilder.Build(viewCountInSchema);

                    var row = PostgreSqlViewSchemaRowBuilder.Build(context.TargetSchemaTable, columns, context.DbConnectionName,
                                                                   schemaName, viewSchemaType, viewName, columnName, columnType);

                    context.TargetSchemaTable.Rows.Add(row);

                    if (!string.IsNullOrWhiteSpace(viewName))
                    {
                        viewNames.Add(viewName);
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
                context.TargetSchemaTable.EndLoadData();
            }

            return viewNames;
        }
    }
}
