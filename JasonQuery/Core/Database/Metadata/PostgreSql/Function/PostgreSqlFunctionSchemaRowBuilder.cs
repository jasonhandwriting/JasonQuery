using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Function
{
    internal static class PostgreSqlFunctionSchemaRowBuilder
    {
        public static DataRow Build(DataTable dtSchema, string schemaObject, string schemaNode, string schemaType, string functionDisplayName)
        {
            var row = dtSchema.NewRow();

            row["SchemaObject"] = schemaObject;
            row["SchemaNode"] = schemaNode;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = functionDisplayName;

            return row;
        }
    }
}