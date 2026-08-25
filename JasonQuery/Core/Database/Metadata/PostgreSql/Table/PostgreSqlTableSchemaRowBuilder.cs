using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Table
{
    internal static class PostgreSqlTableSchemaRowBuilder
    {
        public static DataRow Build(DataTable dtSchema, string schemaObject, string schemaNode, string schemaType,
                                    string tableName, long rowCount, string columnName, string dataType)
        {
            var row = dtSchema.NewRow();

            row["SchemaObject"] = schemaObject;
            row["SchemaNode"] = schemaNode;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = SchemaObjectNameWithCountTextBuilder.Build(tableName, rowCount);
            row["ColumnInfo"] = SchemaObjectColumnInfoTextBuilder.Build(columnName, dataType);

            return row;
        }
    }
}
