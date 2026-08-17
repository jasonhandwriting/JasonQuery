using JasonQuery.Core.Database.Connection;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.DdlPreview
{
    internal enum DdlNewIdentifierCaseMode
    {
        NormalizeDatabaseDefault,
        PreserveInput
    }

    /// <summary>
    /// Applies database-specific identifier delimiters while preserving the
    /// semantic difference between existing metadata names and new names
    /// entered by the user.
    /// </summary>
    internal static class DdlIdentifierQuotingPolicy
    {
        private static readonly Regex UnquotedIdentifierRegex =
                new Regex(@"^[\p{L}_][\p{L}\p{Nd}_$#]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string QuoteExistingIdentifier(DataSourceType dataSourceType, string identifier)
        {
            return TryQuoteExistingIdentifier(dataSourceType, identifier, out string quotedIdentifier) ? quotedIdentifier : string.Empty;
        }

        public static bool TryQuoteExistingIdentifier(DataSourceType dataSourceType, string identifier, out string quotedIdentifier)
        {
            quotedIdentifier = string.Empty;

            if (!TryGetExistingIdentifierValue(dataSourceType, identifier, out string identifierValue))
            {
                return false;
            }

            quotedIdentifier = QuoteIdentifierValue(dataSourceType, identifierValue);

            return !string.IsNullOrEmpty(quotedIdentifier);
        }

        public static string QuoteNewIdentifier(DataSourceType dataSourceType, string identifier)
        {
            return QuoteNewIdentifier(dataSourceType, identifier, DdlNewIdentifierCaseMode.NormalizeDatabaseDefault);
        }

        public static string QuoteNewIdentifier(DataSourceType dataSourceType, string identifier, DdlNewIdentifierCaseMode caseMode)
        {
            return TryQuoteNewIdentifier(dataSourceType, identifier, caseMode, out string quotedIdentifier) ? quotedIdentifier : string.Empty;
        }

        public static bool TryQuoteNewIdentifier(DataSourceType dataSourceType, string identifier, out string quotedIdentifier)
        {
            return TryQuoteNewIdentifier(dataSourceType, identifier, DdlNewIdentifierCaseMode.NormalizeDatabaseDefault, out quotedIdentifier);
        }

        public static bool TryQuoteNewIdentifier(DataSourceType dataSourceType, string identifier, DdlNewIdentifierCaseMode caseMode, out string quotedIdentifier)
        {
            quotedIdentifier = string.Empty;

            if (!TryGetNewIdentifierValue(dataSourceType, identifier, caseMode, out string identifierValue))
            {
                return false;
            }

            quotedIdentifier = QuoteIdentifierValue(dataSourceType, identifierValue);

            return !string.IsNullOrEmpty(quotedIdentifier);
        }

        public static string QuoteExistingQualifiedName(DataSourceType dataSourceType, params string[] identifierParts)
        {
            if (identifierParts == null || identifierParts.Length == 0)
            {
                return string.Empty;
            }

            var quotedParts = new string[identifierParts.Length];

            for (var i = 0; i < identifierParts.Length; i++)
            {
                if (!TryQuoteExistingIdentifier(dataSourceType, identifierParts[i], out quotedParts[i]))
                {
                    return string.Empty;
                }
            }

            return string.Join(".", quotedParts);
        }

        public static string GetExistingIdentifierValue(DataSourceType dataSourceType, string identifier)
        {
            return TryGetExistingIdentifierValue(dataSourceType, identifier, out string identifierValue) ? identifierValue : string.Empty;
        }

        public static string GetNewIdentifierValue(DataSourceType dataSourceType, string identifier)
        {
            return GetNewIdentifierValue(dataSourceType, identifier, DdlNewIdentifierCaseMode.NormalizeDatabaseDefault);
        }

        public static string GetNewIdentifierValue(DataSourceType dataSourceType, string identifier, DdlNewIdentifierCaseMode caseMode)
        {
            return TryGetNewIdentifierValue(dataSourceType, identifier, caseMode, out string identifierValue) ? identifierValue : string.Empty;
        }

        public static bool TryGetExistingIdentifierValue(DataSourceType dataSourceType, string identifier, out string identifierValue)
        {
            identifierValue = string.Empty;

            if (!IsSupportedDataSourceType(dataSourceType) || string.IsNullOrEmpty(identifier) || ContainsUnsupportedControlCharacter(identifier))
            {
                return false;
            }

            var trimmedText = identifier.Trim();

            if (TryReadDelimitedIdentifier(dataSourceType, trimmedText, out identifierValue))
            {
                return true;
            }

            //Existing names originate from database metadata. Preserve the exact value, including case, spaces and delimiter characters.
            identifierValue = identifier;

            return identifierValue.Length > 0;
        }

        public static bool TryGetNewIdentifierValue(DataSourceType dataSourceType, string identifier, out string identifierValue)
        {
            return TryGetNewIdentifierValue(dataSourceType, identifier, DdlNewIdentifierCaseMode.NormalizeDatabaseDefault, out identifierValue);
        }

        public static bool TryGetNewIdentifierValue(DataSourceType dataSourceType, string identifier, DdlNewIdentifierCaseMode caseMode, out string identifierValue)
        {
            identifierValue = string.Empty;

            if (!IsSupportedDataSourceType(dataSourceType))
            {
                return false;
            }

            var text = (identifier ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text) || ContainsUnsafeNewIdentifierSyntax(text))
            {
                return false;
            }

            if (TryReadDelimitedIdentifier(dataSourceType, text, out identifierValue))
            {
                return true;
            }

            if (StartsOrEndsWithAnyIdentifierDelimiter(text) || !UnquotedIdentifierRegex.IsMatch(text))
            {
                identifierValue = string.Empty;
                return false;
            }

            identifierValue = caseMode == DdlNewIdentifierCaseMode.PreserveInput
                              ? text
                              : NormalizeUnquotedNewIdentifier(dataSourceType, text);

            return !string.IsNullOrEmpty(identifierValue);
        }

        public static bool AreExistingAndNewEquivalent(DataSourceType dataSourceType, string existingIdentifier, string newIdentifier)
        {
            return AreExistingAndNewEquivalent(dataSourceType, existingIdentifier, newIdentifier, DdlNewIdentifierCaseMode.NormalizeDatabaseDefault);
        }

        public static bool AreExistingAndNewEquivalent(DataSourceType dataSourceType, string existingIdentifier, string newIdentifier, DdlNewIdentifierCaseMode caseMode)
        {
            if (!TryGetExistingIdentifierValue(dataSourceType, existingIdentifier, out string existingValue)
                || !TryGetNewIdentifierValue(dataSourceType, newIdentifier, caseMode, out string newValue))
            {
                return false;
            }

            return string.Equals(existingValue, newValue, StringComparison.Ordinal);
        }

        private static string QuoteIdentifierValue(DataSourceType dataSourceType, string identifierValue)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return "\""
                               + identifierValue.Replace("\"", "\"\"")
                               + "\"";
                    }
                case DataSourceType.SqlServer:
                    {
                        return "["
                               + identifierValue.Replace("]", "]]")
                               + "]";
                    }
                case DataSourceType.MySql:
                    {
                        return "`"
                               + identifierValue.Replace("`", "``")
                               + "`";
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static bool IsSupportedDataSourceType(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return true;
                    }
                case DataSourceType.None:
                default:
                    {
                        return false;
                    }
            }
        }

        private static string NormalizeUnquotedNewIdentifier(DataSourceType dataSourceType, string identifier)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return identifier.ToUpperInvariant();
                    }
                case DataSourceType.PostgreSql:
                    {
                        return identifier.ToLowerInvariant();
                    }
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return identifier;
                    }
                case DataSourceType.None:
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private static bool TryReadDelimitedIdentifier(DataSourceType dataSourceType, string text, out string identifierValue)
        {
            identifierValue = string.Empty;

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return TryUnescapeDelimitedIdentifier(text, '"', out identifierValue);
                    }
                case DataSourceType.SqlServer:
                    {
                        return TryUnescapeBracketIdentifier(text, out identifierValue);
                    }
                case DataSourceType.MySql:
                    {
                        return TryUnescapeDelimitedIdentifier(text, '`', out identifierValue);
                    }
                case DataSourceType.None:
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool TryUnescapeDelimitedIdentifier(string text, char delimiter, out string identifierValue)
        {
            identifierValue = string.Empty;

            if (string.IsNullOrEmpty(text) || text.Length < 2 || text[0] != delimiter || text[text.Length - 1] != delimiter)
            {
                return false;
            }

            var inner = text.Substring(1, text.Length - 2);

            if (string.IsNullOrEmpty(inner))
            {
                return false;
            }

            var result = new StringBuilder(inner.Length);

            for (var i = 0; i < inner.Length; i++)
            {
                var current = inner[i];

                if (current != delimiter)
                {
                    result.Append(current);
                    continue;
                }

                if (i + 1 >= inner.Length || inner[i + 1] != delimiter)
                {
                    return false;
                }

                result.Append(delimiter);
                i++;
            }

            identifierValue = result.ToString();
            return identifierValue.Length > 0;
        }

        private static bool TryUnescapeBracketIdentifier(string text, out string identifierValue)
        {
            identifierValue = string.Empty;

            if (string.IsNullOrEmpty(text) || text.Length < 2 || text[0] != '[' || text[text.Length - 1] != ']')
            {
                return false;
            }

            var inner = text.Substring(1, text.Length - 2);

            if (string.IsNullOrEmpty(inner))
            {
                return false;
            }

            var result = new StringBuilder(inner.Length);

            for (var i = 0; i < inner.Length; i++)
            {
                var current = inner[i];

                if (current != ']')
                {
                    result.Append(current);
                    continue;
                }

                if (i + 1 >= inner.Length || inner[i + 1] != ']')
                {
                    return false;
                }

                result.Append(']');
                i++;
            }

            identifierValue = result.ToString();
            return identifierValue.Length > 0;
        }

        private static bool StartsOrEndsWithAnyIdentifierDelimiter(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var first = value[0];
            var last = value[value.Length - 1];

            return first == '"'
                   || first == '['
                   || first == '`'
                   || last == '"'
                   || last == ']'
                   || last == '`';
        }

        private static bool ContainsUnsafeNewIdentifierSyntax(string value)
        {
            return value.IndexOf(';') >= 0
                   || value.IndexOf("--", StringComparison.Ordinal) >= 0
                   || value.IndexOf("/*", StringComparison.Ordinal) >= 0
                   || value.IndexOf("*/", StringComparison.Ordinal) >= 0
                   || ContainsUnsupportedControlCharacter(value);
        }

        private static bool ContainsUnsupportedControlCharacter(string value)
        {
            return value.IndexOf('\r') >= 0
                   || value.IndexOf('\n') >= 0
                   || value.IndexOf('\0') >= 0;
        }
    }
}