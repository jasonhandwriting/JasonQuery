using System.Data;

namespace JasonQuery.Core.Database.Metadata.PostgreSql.Trigger
{
    internal static class PostgreSqlTriggerSchemaRowBuilder
    {
        public static DataRow Build(DataTable dtSchema, string schemaObject, string schemaNode, string schemaType, string triggerName)
        {
            var row = dtSchema.NewRow();

            row["SchemaObject"] = schemaObject;
            row["SchemaNode"] = schemaNode;
            row["SchemaType"] = schemaType;
            row["SchemaName"] = triggerName;

            return row;
        }
    }
}