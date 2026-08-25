namespace JasonQuery.Core.QueryEngine.Formatters
{
    internal sealed class LargeTextPreviewResult
    {
        public string DisplayText { get; set; }
        public string PreviewText { get; set; }
        public int FullLength { get; set; }
        public int PreviewLength { get; set; }
        public bool IsTruncated { get; set; }
    }
}
