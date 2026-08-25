using JasonQuery.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class AutoCompleteKeywordAppender
    {
        public static void Append(string keywordText, bool enabled, string objectSource)
        {
            if (!enabled || string.IsNullOrWhiteSpace(keywordText))
            {
                return;
            }

            var parts = keywordText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return;
            }

            MyGlobal.AddAutoCompleteDataRange(parts, objectSource);
        }

        public static void Append(IEnumerable<string> names, string sourceName)
        {
            if (names == null || string.IsNullOrWhiteSpace(sourceName))
            {
                return;
            }

            foreach (var name in names.Where(x => !string.IsNullOrWhiteSpace(x))
                                      .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                MyGlobal.AddAutoCompleteData(name, sourceName);
            }
        }
    }
}
