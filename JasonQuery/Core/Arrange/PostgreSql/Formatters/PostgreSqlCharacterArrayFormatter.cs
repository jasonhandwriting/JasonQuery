using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Text;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlCharacterArrayFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.CharacterArray;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.CharacterArray)
            {
                formatted = null;
                return false;
            }

            var sourceText = rawValue?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(sourceText))
            {
                formatted = "{}";
                return true;
            }

            if (sourceText.StartsWith("{", StringComparison.Ordinal))
            {
                sourceText = sourceText.Substring(1);
            }

            if (sourceText.EndsWith("}", StringComparison.Ordinal))
            {
                sourceText = sourceText.Substring(0, sourceText.Length - 1);
            }

            var parts = sourceText.Split(new[] { "," }, StringSplitOptions.None);
            var sbResult = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                
                if (part.IndexOf('\\') >= 0)
                {
                    if (!part.StartsWith("\"", StringComparison.Ordinal))
                    {
                        sbResult.Append('"');
                    }

                    sbResult.Append(part.Replace("\\\\", "\\"));

                    if (!part.EndsWith("\"", StringComparison.Ordinal))
                    {
                        sbResult.Append('"');
                    }
                }
                else
                {
                    sbResult.Append(part);
                }

                if (i < parts.Length - 1)
                {
                    sbResult.Append(",");
                }
            }

            formatted = "{" + sbResult + "}";
            return true;
        }
    }
}
