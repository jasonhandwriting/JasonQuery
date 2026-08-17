using System;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.Oracle
{
    internal static class OracleErrorCodeParser
    {
        public static string Normalize(string errorCode, string errorMessage)
        {
            var result = NormalizeDigits(errorCode);

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            if (string.IsNullOrEmpty(errorMessage))
            {
                return string.Empty;
            }

            var match = Regex.Match
            (
                errorMessage,
                @"ORA-(?<code>\d{5})",
                RegexOptions.IgnoreCase
            );

            return match.Success ? match.Groups["code"].Value : string.Empty;
        }

        public static bool Is(string normalizedCode, string expectedCode)
        {
            return string.Equals
            (
                NormalizeDigits(normalizedCode),
                NormalizeDigits(expectedCode),
                StringComparison.Ordinal
            );
        }

        private static string NormalizeDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var match = Regex.Match(value, @"(?<code>\d{1,5})");

            if (!match.Success)
            {
                return string.Empty;
            }

            if (!int.TryParse(match.Groups["code"].Value, out var numericCode))
            {
                return string.Empty;
            }

            return numericCode.ToString("00000");
        }
    }
}