using System;
using System.Globalization;
using System.Linq;

namespace JasonLibrary.Core.Update
{
    public sealed class UpdateVersionComparer
    {
        private const int ComparablePartCount = 4;

        public int Compare(string left, string right)
        {
            if (!TryParse(left, out var leftParts, out _))
            {
                throw new FormatException($"Invalid version: {left}");
            }

            if (!TryParse(right, out var rightParts, out _))
            {
                throw new FormatException($"Invalid version: {right}");
            }

            for (var i = 0; i < ComparablePartCount; i++)
            {
                var result = leftParts[i].CompareTo(rightParts[i]);

                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }

        public bool TryNormalize(string value, out string normalizedVersion)
        {
            return TryParse(value, out _, out normalizedVersion);
        }

        private static bool TryParse(string value, out int[] parts, out string normalizedVersion)
        {
            parts = new int[ComparablePartCount];
            normalizedVersion = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var numericValue = value.Trim();

            if (numericValue.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                numericValue = numericValue.Substring(1);
            }

            var suffixIndex = numericValue.IndexOfAny(new[] { '-', '+' });

            if (suffixIndex >= 0)
            {
                numericValue = numericValue.Substring(0, suffixIndex);
            }

            var sourceParts = numericValue.Split('.');

            if (sourceParts.Length == 0 || sourceParts.Length > ComparablePartCount)
            {
                return false;
            }

            for (var i = 0; i < sourceParts.Length; i++)
            {
                if (!int.TryParse(sourceParts[i], NumberStyles.None, CultureInfo.InvariantCulture, out parts[i]) || parts[i] < 0)
                {
                    return false;
                }
            }

            normalizedVersion = string.Join(".", parts.Take(Math.Max(3, sourceParts.Length)));
            return true;
        }
    }
}
