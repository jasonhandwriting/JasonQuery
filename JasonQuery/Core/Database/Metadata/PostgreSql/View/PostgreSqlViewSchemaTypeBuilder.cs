namespace JasonQuery.Core.Database.Metadata.PostgreSql.View
{
    internal static class PostgreSqlViewSchemaTypeBuilder
    {
        public static string Build(int viewCount)
        {
            return SchemaTypeTextBuilder.Build(SchemaObjectNames.Views, viewCount);
        }
    }
}
