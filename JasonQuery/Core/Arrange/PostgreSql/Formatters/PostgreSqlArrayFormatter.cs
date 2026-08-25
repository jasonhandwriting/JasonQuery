using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using JasonQuery.Core.QueryEngine.Formatters;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlArrayFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Array;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Array)
            {
                formatted = null;
                return false;
            }

            var arrayText = rawValue?.ToString() ?? string.Empty;
            var maxLength = LargeTextPreviewLengthPolicy.Normalize(context?.LargeTextPreviewLength ?? LargeTextPreviewLengthPolicy.DefaultLength);

            if (arrayText.Length <= maxLength)
            {
                formatted = arrayText;
                return true;
            }

            var previewText = FormatterTextHelper.TrimToMaxLength(arrayText, maxLength, out bool isTruncated);
            var truncatedSuffix = context?.TruncatedText ?? string.Empty;

            formatted = isTruncated ? string.Concat(previewText, truncatedSuffix) : previewText;
            return true;
        }
    }
}
