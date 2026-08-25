using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Text;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlBooleanArrayFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.BooleanArray;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.BooleanArray)
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

                if (part.StartsWith("T", StringComparison.OrdinalIgnoreCase))
                {
                    sbResult.Append("t");
                }
                else
                {
                    sbResult.Append("f");
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
