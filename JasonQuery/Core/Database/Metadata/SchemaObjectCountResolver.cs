using System.Collections.Generic;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectCountResolver
    {
        public static int Resolve(Dictionary<string, int> countMap, string schemaName)
        {
            if (countMap == null || string.IsNullOrWhiteSpace(schemaName))
            {
                return 0;
            }

            return countMap.TryGetValue(schemaName, out var count) ? count : 0;
        }
    }
}