using JasonQuery.Core.Config;
using JasonQuery.Core.Data.Formatters;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectNameWithCountTextBuilder
    {
        public static string Build(string objectName, long objectCount)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return string.Empty;
            }

            return $"{objectName}{MyGlobal.Separator}({DataSizeFormatter.FormatInt(objectCount)})";
        }
    }
}