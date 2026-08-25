using System;

namespace JasonQuery.Core.QueryEngine.Types
{
    public sealed class LargeBinaryDataType
    {
        public string DisplayText { get; }
        public string PreviewText { get; }
        public int Length { get; }
        public bool IsTruncated { get; }
        public bool IsNull { get; }

        private readonly Func<byte[]> _contentLoader;
        private byte[] _cached;
        private bool _isLoaded;

        public LargeBinaryDataType(string displayText, string previewText, int length, bool isTruncated, Func<byte[]> fullContent, bool isNull = false)
        {
            DisplayText = displayText ?? string.Empty;
            PreviewText = previewText ?? string.Empty;
            Length = length;
            IsTruncated = isTruncated;
            IsNull = isNull;
            _contentLoader = fullContent ?? (() => Array.Empty<byte>());
        }

        public static LargeBinaryDataType CreateNull(string nullText)
        {
            return new LargeBinaryDataType
            (
                nullText ?? string.Empty,
                string.Empty,
                0,
                false,
                () => Array.Empty<byte>(),
                true
            );
        }

        public byte[] LoadContent()
        {
            if (_isLoaded)
            {
                return _cached;
            }

            var content = _contentLoader();

            _cached = content ?? Array.Empty<byte>();
            _isLoaded = true;

            return _cached;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(PreviewText) ? DisplayText : string.Concat(DisplayText, PreviewText);
        }
    }
}
