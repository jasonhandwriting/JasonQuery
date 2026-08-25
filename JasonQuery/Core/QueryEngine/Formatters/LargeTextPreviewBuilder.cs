using System;

namespace JasonQuery.Core.QueryEngine.Formatters
{
    internal static class LargeTextPreviewBuilder
    {
        public static LargeTextPreviewResult Build(string value, int requestedLength, string truncatedText)
        {
            var fullText = value ?? string.Empty;
            var previewLength = LargeTextPreviewLengthPolicy.Normalize(requestedLength);
            var isTruncated = fullText.Length > previewLength;
            var previewText = isTruncated ? fullText.Substring(0, previewLength) : fullText;

            if (isTruncated && !string.IsNullOrEmpty(truncatedText))
            {
                previewText = string.Concat(previewText, truncatedText);
            }

            return new LargeTextPreviewResult
            {
                DisplayText = isTruncated ? TextPreviewFormatter.FormatText(previewLength, fullText.Length) : string.Empty,
                PreviewText = previewText,
                FullLength = fullText.Length,
                PreviewLength = previewLength,
                IsTruncated = isTruncated
            };
        }
    }
}
