using JasonQuery.Core.Config;
using JasonQuery.Core.Data.Formatters;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaTypeTextBuilder
    {
        public static string Build(string objectName, int objectCount)
        {
            var qty = objectCount > 0 ? $"({DataSizeFormatter.FormatInt(objectCount)})" : "(0)";

            return $"{objectName}{MyGlobal.Separator}{qty}";
        }
    }
}
