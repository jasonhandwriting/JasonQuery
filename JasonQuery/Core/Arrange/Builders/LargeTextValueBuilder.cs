using JasonLibrary.Core.Schema;
using JasonQuery.Core.Arrange.Formatting;
using JasonQuery.Core.QueryEngine.Formatters;
using JasonQuery.Core.QueryEngine.Types;
using System;

namespace JasonQuery.Core.Arrange.Builders
{
    internal static class LargeTextValueBuilder
    {
        internal static LargeTextDataType Build(object rawValue, ColumnInfo columnInfo, ArrangeContext context, ColumnValueFormatterRegistry formatterRegistry)
        {
            var fullText = rawValue?.ToString() ?? string.Empty;
            var fullLength = fullText.Length;
            var maxLength = LargeTextPreviewLengthPolicy.Normalize(context?.LargeTextPreviewLength ?? LargeTextPreviewLengthPolicy.DefaultLength);
            var truncatedSuffix = context?.TruncatedText ?? string.Empty;

            if (formatterRegistry == null || !formatterRegistry.TryFormat(rawValue, columnInfo, context, out var formatted))
            {
                var previewResult = LargeTextPreviewBuilder.Build(fullText, maxLength, truncatedSuffix);

                return new LargeTextDataType
                (
                    previewResult.DisplayText,
                    previewResult.PreviewText,
                    previewResult.FullLength,
                    previewResult.IsTruncated,
                    () => fullText
                );
            }

            var previewText = formatted?.ToString() ?? string.Empty;
            var isTruncated = fullLength > maxLength;
            var displayText = isTruncated ? TextPreviewFormatter.FormatText(maxLength, fullLength) : string.Empty;

            previewText = FormatterTextHelper.TrimToMaxLength(previewText, maxLength, out bool isFormattedPreviewTruncated);

            if (isFormattedPreviewTruncated && !string.IsNullOrEmpty(truncatedSuffix))
            {
                previewText = string.Concat(previewText, truncatedSuffix);
            }

            return new LargeTextDataType
            (
                displayText,
                previewText,
                fullLength,
                isTruncated,
                () => fullText
            );
        }
    }
}