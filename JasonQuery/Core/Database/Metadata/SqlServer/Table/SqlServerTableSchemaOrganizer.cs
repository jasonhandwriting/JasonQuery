using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Table
{
    internal sealed class SqlServerTableSchemaOrganizer
    {
        private readonly SqlServerTableSchemaRowBuilder _rowBuilder;

        public SqlServerTableSchemaOrganizer() : this(new SqlServerTableSchemaRowBuilder())
        {
        }

        public SqlServerTableSchemaOrganizer(SqlServerTableSchemaRowBuilder rowBuilder)
        {
            _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
        }

        public HashSet<string> Organize(SqlServerTableSchemaBuildContext context, DataTable dtTableInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dtTableInfo == null || dtTableInfo.Rows.Count == 0)
            {
                return distinctNames;
            }

            var metadataContext = context.metadataContext;

            var tableRows = dtTableInfo.AsEnumerable()
                                       .Where
                                        (
                                            rowData =>
                                            string.Equals(rowData.GetSafeString("DbName"), metadataContext.databaseName, StringComparison.OrdinalIgnoreCase)
                                        )
                                       .ToList();

            var schemaType = metadataContext.BuildSchemaTypeText(SchemaObjectNames.Tables, tableRows.Count);

            foreach (var tableRowData in tableRows)
            {
                var schemaDbo = tableRowData.GetSafeString("Schema_Dbo");
                var tableName = tableRowData.GetSafeString("Name");

                var key =
                (
                    DbName: metadataContext.databaseName,
                    SchemaDbo: schemaDbo,
                    TableName: tableName
                );

                if (!context.columnInfoMap.TryGetValue(key, out var columnRows))
                {
                    columnRows = new List<DataRow>();
                }

                foreach (var columnRowData in columnRows)
                {
                    var schemaRow = _rowBuilder.Build(context, tableRowData, columnRowData, schemaType);

                    metadataContext.dtSchema.Rows.Add(schemaRow);
                }

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    distinctNames.Add(tableName);
                }
            }

            return distinctNames;
        }
    }
}
