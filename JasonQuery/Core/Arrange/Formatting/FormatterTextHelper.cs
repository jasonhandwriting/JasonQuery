using System;

namespace JasonQuery.Core.Arrange.Formatting
{
    public static class FormatterTextHelper
    {
        public static string TrimToMaxLength(string text, int maxLength, out bool isTruncated)
        {
            if (string.IsNullOrEmpty(text))
            {
                isTruncated = false;
                return string.Empty;
            }

            maxLength = Math.Max(0, maxLength);

            if (text.Length <= maxLength)
            {
                isTruncated = false;
                return text;
            }

            isTruncated = true;
            return text.Substring(0, maxLength);
        }
    }
}