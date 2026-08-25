using JasonQuery.Core.Data.DataRows;
using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.SqlServer.View
{
    internal sealed class SqlServerViewSchemaRowBuilder
    {
        public DataRow Build(SqlServerViewSchemaBuildContext context, DataRow viewRowData, DataRow columnRowData, string schemaType)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (viewRowData == null)
            {
                throw new ArgumentNullException(nameof(viewRowData));
            }

            if (columnRowData == null)
            {
                throw new ArgumentNullException(nameof(columnRowData));
            }

            context.Validate();

            var metadataContext = context.metadataContext;

            var schemaDbo = viewRowData.GetSafeString("Schema_Dbo");
            var viewName = viewRowData.GetSafeString("Name");
            var schemaName = $"{schemaDbo}.{viewName}";
            var objectId = viewRowData.GetSafeString("Object_ID");
            var createDateText = viewRowData.GetSafeDateTimeText("Create_Date", metadataContext.dateTimeFormat);
            var modifyDateText = viewRowData.GetSafeDateTimeText("Modify_Date", metadataContext.dateTimeFormat);

            var columnName = columnRowData.GetSafeString("ColumnName");
            var columnTypeText = columnRowData.GetSafeString("ColumnType");

            var row = metadataContext.dtSchema.NewRow();

            row["SchemaObject"] = metadataContext.connectionName;
            row["SchemaNode"] = metadataContext.databaseName;
            row["SchemaDbo"] = schemaDbo;
            row["SchemaType"] = schemaType;

            if (row.Table.Columns.Contains("Schema_Browser"))
            {
                row["Schema_Browser"] = schemaName;
            }
            else
            {
                row["SchemaName"] = schemaName;
            }

            if (row.Table.Columns.Contains("ColumnInfo"))
            {
                row["ColumnInfo"] = $"{columnName}, {columnTypeText}";
            }
            else
            {
                row["SchemaName"] = $"{columnName}, {columnTypeText}";
            }

            row["ObjectID"] = objectId;
            row["CreateDate"] = createDateText;
            row["ModifyDate"] = modifyDateText;

            return row;
        }
    }
}
