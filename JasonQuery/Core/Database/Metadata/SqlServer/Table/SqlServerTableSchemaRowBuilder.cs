using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Formatters;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.Table
{
    internal sealed class SqlServerTableSchemaRowBuilder
    {
        private static readonly HashSet<string> _lengthDataTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "char",
            "nchar",
            "varchar",
            "nvarchar"
        };

        public DataRow Build(SqlServerTableSchemaBuildContext context, DataRow tableRowData, DataRow columnRowData, string schemaType)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (tableRowData == null)
            {
                throw new ArgumentNullException(nameof(tableRowData));
            }

            context.Validate();

            var metadataContext = context.metadataContext;

            var schemaDbo = tableRowData.GetSafeString("Schema_Dbo");
            var tableName = tableRowData.GetSafeString("Name");
            var objectId = tableRowData.GetSafeString("Object_ID");
            var createDateText = tableRowData.GetSafeDateTimeText("Create_Date", metadataContext.dateTimeFormat);
            var modifyDateText = tableRowData.GetSafeDateTimeText("Modify_Date", metadataContext.dateTimeFormat);

            var rowCountKey =
            (
                DbName: metadataContext.databaseName,
                SchemaDbo: schemaDbo,
                TableName: tableName
            );

            var rowCount = context.rowCountMap.TryGetValue(rowCountKey, out var count) ? count : 0L;
            var rowCountSuffix = $"{metadataContext.separator}({DataSizeFormatter.FormatInt(rowCount)})";
            var schemaName = $"{schemaDbo}.{tableName}{rowCountSuffix}";

            var row = metadataContext.dtSchema.NewRow();

            row["SchemaObject"] = metadataContext.connectionName;
            row["SchemaNode"] = metadataContext.databaseName;
            row["SchemaDbo"] = schemaDbo;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = schemaName;
            row["ObjectID"] = objectId;
            row["CreateDate"] = createDateText;
            row["ModifyDate"] = modifyDateText;

            if (columnRowData != null)
            {
                var columnName = columnRowData.GetSafeString("Column_Name");
                var dataType = columnRowData.GetSafeString("Data_Type");
                var maxLength = columnRowData.GetSafeInt("Character_Maximum_Length");
                var lengthText = maxLength == -1 ? "max" : maxLength.ToString();
                var columnTypeText = _lengthDataTypes.Contains(dataType) ? $"{dataType}({lengthText})" : dataType;

                if (row.Table.Columns.Contains("ColumnInfo"))
                {
                    row["ColumnInfo"] = $"{columnName}, {columnTypeText}";
                }
                else
                {
                    row["SchemaName"] = $"{columnName}, {columnTypeText}";
                }
            }

            return row;
        }
    }
}
