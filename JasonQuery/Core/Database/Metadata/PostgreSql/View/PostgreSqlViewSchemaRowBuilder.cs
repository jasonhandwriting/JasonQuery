using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.View
{
    internal static class PostgreSqlViewSchemaRowBuilder
    {
        public static DataRow Build(DataTable dtSchema, DataColumnCollection columns, string schemaObject, string schemaNode,
                                    string schemaType, string viewName, string columnName, string columnType)
        {
            var row = dtSchema.NewRow();

            row["SchemaObject"] = schemaObject;
            row["SchemaNode"] = schemaNode;
            row["SchemaType"] = schemaType;

            if (columns.Contains("Schema_Browser"))
            {
                row["Schema_Browser"] = viewName;
            }
            else
            {
                row["SchemaName"] = viewName;
            }

            var columnInfo = SchemaObjectColumnInfoTextBuilder.Build(columnName, columnType);

            if (columns.Contains("ColumnInfo"))
            {
                row["ColumnInfo"] = columnInfo;
            }
            else
            {
                row["SchemaName"] = columnInfo;
            }

            return row;
        }
    }
}
