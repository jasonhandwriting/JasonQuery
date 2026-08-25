using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class ScintillaKeywordTextBuilder
    {
        public static string Build(IEnumerable<string> objectNames)
        {
            var names = (objectNames ?? Enumerable.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x))
                                                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                   .ToArray();

            if (names.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", names) + " ";
        }
    }
}
