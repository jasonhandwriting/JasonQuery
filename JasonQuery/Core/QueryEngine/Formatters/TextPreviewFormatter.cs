using JasonQuery.Core.Data.Formatters;

namespace JasonQuery.Core.QueryEngine.Formatters
{
    internal static class TextPreviewFormatter
    {
        public static string FormatText(long previewBytes, long fullBytes)
        {
            var previewText = DataSizeFormatter.FormatBytes(previewBytes);
            var fullText = DataSizeFormatter.FormatBytes(fullBytes);

            return $"({previewText} of {fullText})";
        }
    }
}
