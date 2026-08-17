using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Text;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlCharArrayFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.CharArray;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.CharArray)
            {
                formatted = null;
                return false;
            }

            var arrayText = rawValue?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(arrayText))
            {
                formatted = "{}";
                return true;
            }

            if (arrayText.StartsWith("{", StringComparison.Ordinal))
            {
                arrayText = arrayText.Substring(1);
            }

            if (arrayText.EndsWith("}", StringComparison.Ordinal))
            {
                arrayText = arrayText.Substring(0, arrayText.Length - 1);
            }

            var parts = arrayText.Split(new[] { "," }, StringSplitOptions.None);
            var sbResult = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (part == "\\")
                {
                    sbResult.Append("\"\\\\\"");
                }
                else if (part == "\"")
                {
                    sbResult.Append("\"\\\"\"");
                }
                else if (string.IsNullOrEmpty(part) || part == "\0")
                {
                    sbResult.Append("\"\"");
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