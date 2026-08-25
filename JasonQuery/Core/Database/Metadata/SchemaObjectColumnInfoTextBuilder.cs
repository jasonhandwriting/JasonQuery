namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectColumnInfoTextBuilder
    {
        public static string Build(string columnName, string columnType)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(columnType))
            {
                return columnName;
            }

            return $"{columnName}, {columnType}";
        }
    }
}
