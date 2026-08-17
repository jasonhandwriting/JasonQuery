using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Metadata.SqlServer.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Function
{
    internal sealed class SqlServerFunctionMetadataOrganizer
    {
        private readonly SqlServerSimpleSchemaRowBuilder _rowBuilder;

        public SqlServerFunctionMetadataOrganizer() : this(new SqlServerSimpleSchemaRowBuilder())
        {
        }

        public SqlServerFunctionMetadataOrganizer(SqlServerSimpleSchemaRowBuilder rowBuilder)
        {
            _rowBuilder = rowBuilder ?? throw new ArgumentNullException(nameof(rowBuilder));
        }

        public HashSet<string> Organize(SqlServerMetadataContext context, DataTable dtFunctionInfo)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var distinctNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dtFunctionInfo == null || dtFunctionInfo.Rows.Count == 0)
            {
                return distinctNames;
            }

            var functionRows = dtFunctionInfo.AsEnumerable()
                                             .Where
                                              (
                                                  rowData =>
                                                  string.Equals(rowData.GetSafeString("DbName"), context.databaseName, StringComparison.OrdinalIgnoreCase)
                                              )
                                             .ToList();

            var schemaType = context.BuildSchemaTypeText(SchemaObjectNames.Functions, functionRows.Count);

            foreach (var rowData in functionRows)
            {
                var schemaDbo = rowData.GetSafeString("Schema_Dbo");
                var name = rowData.GetSafeString("Name");
                var fullSchemaName = $"{schemaDbo}.{name}";
                var objectId = rowData.GetSafeString("Object_ID");

                var schemaRow = _rowBuilder.Build(context, schemaDbo, schemaType, fullSchemaName, objectId);

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