using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectKeywordApplyHelper
    {
        public static void Apply(IEnumerable<string> names, bool enableAutoComplete,
                                 string autoCompleteSourceName, Action<string> assignKeywordText)
        {
            if (enableAutoComplete)
            {
                AutoCompleteKeywordAppender.Append(names, autoCompleteSourceName);
            }

            if (assignKeywordText != null)
            {
                var keywordText = ScintillaKeywordTextBuilder.Build(names);

                assignKeywordText(keywordText);
            }
        }
    }
}