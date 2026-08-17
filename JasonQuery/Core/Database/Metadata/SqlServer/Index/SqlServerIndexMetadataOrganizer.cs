using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Metadata.SqlServer.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Index
{
    internal sealed class SqlServerIndexMetadataOrganizer
    {
        private readonly SqlServerSimpleSchemaRowBuilder _rowBuilder;

        public SqlServerIndexMetadataOrganizer() : this(new SqlServerSimpleSchemaRowBuilder())
        {
        }

        public SqlServerIndexMetadataOrganizer(SqlServerSimpleSchemaRowBuilder rowBuilder)
        {
            _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
        }

        public HashSet<string> Organize(SqlServerMetadataContext context, DataTable dtIndexInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dtIndexInfo == null || dtIndexInfo.Rows.Count == 0)
            {
                return distinctNames;
            }

            var indexRows = dtIndexInfo.AsEnumerable()
                                       .Where
                                        (
                                            rowData =>
                                            string.Equals(rowData.GetSafeString("DbName"), context.databaseName, StringComparison.OrdinalIgnoreCase)
                                        )
                                       .ToList();

            var schemaType = context.BuildSchemaTypeText("Indexes", indexRows.Count);

            foreach (var rowData in indexRows)
            {
                var schemaDbo = rowData.GetSafeString("Schema_Dbo");
                var name = rowData.GetSafeString("Name");
                var tableName = rowData.GetSafeString("tblname");
                var fullSchemaName = $"{schemaDbo}.{name} on {tableName}";
                var objectId = rowData.GetSafeString("Object_ID");

                var schemaRow = _rowBuilder.Build(context, schemaDbo,schemaType, fullSchemaName, objectId);

                context.dtSchema.Rows.Add(schemaRow);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    distinctNames.Add(name);
                }
            }

            return distinctNames;
        }
    }
}