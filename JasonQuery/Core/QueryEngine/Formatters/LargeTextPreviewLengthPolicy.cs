using System;

namespace JasonQuery.Core.QueryEngine.Formatters
{
    internal static class LargeTextPreviewLengthPolicy
    {
        public const int DefaultLength = 50;
        public const int RawDataModeLength = 50;

        private static readonly int[] _allowedLengths = { 50, 100, 200, 500, 1000 };

        public static bool IsSupported(int length)
        {
            return Array.IndexOf(_allowedLengths, length) >= 0;
        }

        public static int Normalize(int length)
        {
            return IsSupported(length) ? length : DefaultLength;
        }

        public static int[] GetAllowedLengths()
        {
            return (int[])_allowedLengths.Clone();
        }
    }
}
