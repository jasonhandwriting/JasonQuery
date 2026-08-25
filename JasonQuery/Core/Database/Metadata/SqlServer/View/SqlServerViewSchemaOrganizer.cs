using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.View
{
    internal sealed class SqlServerViewSchemaOrganizer
    {
        private readonly SqlServerViewSchemaRowBuilder _rowBuilder;

        public SqlServerViewSchemaOrganizer() : this(new SqlServerViewSchemaRowBuilder())
        {
        }

        public SqlServerViewSchemaOrganizer(SqlServerViewSchemaRowBuilder rowBuilder)
        {
            _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
        }

        public HashSet<string> Organize(SqlServerViewSchemaBuildContext context, DataTable dtViewInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dtViewInfo == null || dtViewInfo.Rows.Count == 0)
            {
                return distinctNames;
            }

            var metadataContext = context.metadataContext;

            var viewRows = dtViewInfo.AsEnumerable()
                                     .Where
                                      (
                                          rowData =>
                                          string.Equals(rowData.GetSafeString("DbName"), metadataContext.databaseName, StringComparison.OrdinalIgnoreCase)
                                      )
                                     .ToList();

            var schemaType = metadataContext.BuildSchemaTypeText(SchemaObjectNames.Views, viewRows.Count);

            foreach (var viewRowData in viewRows)
            {
                var schemaDbo = viewRowData.GetSafeString("Schema_Dbo");
                var viewName = viewRowData.GetSafeString("Name");

                var key =
                (
                    DbName: metadataContext.databaseName,
                    SchemaDbo: schemaDbo,
                    ViewName: viewName
                );

                if (!context.columnInfoMap.TryGetValue(key, out var columnRows))
                {
                    columnRows = new List<DataRow>();
                }

                foreach (var columnRowData in columnRows)
                {
                    var schemaRow = _rowBuilder.Build(context, viewRowData, columnRowData, schemaType);

                    metadataContext.dtSchema.Rows.Add(schemaRow);
                }

                if (!string.IsNullOrWhiteSpace(viewName))
                {
                    distinctNames.Add(viewName);
                }
            }

            return distinctNames;
        }
    }
}
