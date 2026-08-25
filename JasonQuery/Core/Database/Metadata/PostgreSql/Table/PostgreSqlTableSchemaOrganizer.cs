using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Table
{
    internal static class PostgreSqlTableSchemaOrganizer
    {
        public static HashSet<string> Organize(PostgreSqlTableSchemaBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.SourceTableSchemaInfo == null)
            {
                throw new ArgumentNullException(nameof(context.SourceTableSchemaInfo));
            }

            if (context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            var tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var processed = 0;

            context.TargetSchemaTable.BeginLoadData();

            try
            {
                foreach (DataRow dr in context.SourceTableSchemaInfo.AsEnumerable())
                {
                    var schemaName = dr.GetSafeString("DbName");
                    var tableName = dr.GetSafeString("TableName");
                    var columnName = dr.GetSafeString("ColumnName");
                    var columnType = dr.GetSafeString("ColumnType");
                    var dataTypeDb = dr.GetSafeString("DataType");
                    var dataLen = dr.GetSafeString("DataLength", "0");
                    var scale = dr.GetSafeString("Scale");
                    var rowCount = dr.GetSafeLong("RowCount");
                    var tableCountInSchema = dr.GetSafeInt("TableCountInSchema");

                    var tableSchemaType = PostgreSqlTableSchemaTypeBuilder.Build(tableCountInSchema);

                    var dataType = !string.IsNullOrWhiteSpace(columnType) ? columnType : PostgreSqlFallbackColumnTypeFormatter.Format(dataTypeDb, dataLen, scale);

                    var row = PostgreSqlTableSchemaRowBuilder.Build(context.TargetSchemaTable, context.DbConnectionName, schemaName,
                                                                    tableSchemaType, tableName, rowCount, columnName, dataType);

                    context.TargetSchemaTable.Rows.Add(row);

                    if (!string.IsNullOrWhiteSpace(tableName))
                    {
                        tableNames.Add(tableName);
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

            return tableNames;
        }
    }
}
