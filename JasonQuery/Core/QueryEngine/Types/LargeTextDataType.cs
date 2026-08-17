using System;

namespace JasonQuery.Core.QueryEngine.Types
{
    public sealed class LargeTextDataType
    {
        public string DisplayText { get; } //(CLOB)(50,000)
        public string PreviewText { get; } //xxxxxxxx...(truncated)
        public int Length { get; } //原始完整長度
        public bool IsTruncated { get; }
        public bool IsNull { get; }

        private readonly Func<string> _contentLoader;
        private string _cached;
        private bool _isLoaded;

        public LargeTextDataType(string displayText, string previewText, int length, bool isTruncated, Func<string> fullContent, bool isNull = false)
        {
            DisplayText = displayText ?? string.Empty;
            PreviewText = previewText ?? string.Empty;
            Length = length;
            IsTruncated = isTruncated;
            IsNull = isNull;
            _contentLoader = fullContent ?? (() => string.Empty);
        }

        public static LargeTextDataType CreateNull(string nullText)
        {
            return new LargeTextDataType
            (
                nullText ?? string.Empty,
                string.Empty,
                0,
                false,
                () => string.Empty,
                true
            );
        }

        public string LoadContent()
        {
            if (_isLoaded)
            {
                return _cached;
            }

            var content = _contentLoader();

            _cached = content ?? string.Empty;
            _isLoaded = true;

            return _cached;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(PreviewText) ? DisplayText : string.Concat(DisplayText, PreviewText);
        }
    }
}