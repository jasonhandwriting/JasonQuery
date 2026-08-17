namespace JasonQuery.Core.Database.Metadata.PostgreSql.Table
{
    internal static class PostgreSqlTableSchemaTypeBuilder
    {
        public static string Build(int tableQty)
        {
            return SchemaTypeTextBuilder.Build(SchemaObjectNames.Tables, tableQty);
        }
    }
}