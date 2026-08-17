using JasonQuery.Core.Database.Connection;
using System;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal static class DdlInputSafetyValidator
    {
        private static readonly Regex UnquotedIdentifierRegex =
                new Regex(@"^[\p{L}_][\p{L}\p{Nd}_$#]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex CustomTypeCharacterRegex =
                new Regex(@"^[\p{L}\p{Nd}_$#\.\s,""`\[\]\(\)]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex UnsafeTypeClauseRegex =
                new Regex(@"\b(DEFAULT|NULL|NOT\s+NULL|CONSTRAINT|PRIMARY|UNIQUE|REFERENCES|CHECK|GENERATED|IDENTITY|COMMENT|COLLATE|CHARACTER\s+SET)\b",
                          RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool IsSafeSingleIdentifier(string value, DataSourceType dataSourceType)
        {
            if (!IsSupportedDataSourceType(dataSourceType))
            {
                return false;
            }

            var text = (value ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text) || ContainsStatementDelimiterOrComment(text))
            {
                return false;
            }

            if (UnquotedIdentifierRegex.IsMatch(text))
            {
                return true;
            }

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        return IsValidDelimitedIdentifier(text, '"', "\"\"");
                    }
                case DataSourceType.SqlServer:
                    {
                        return IsValidBracketIdentifier(text);
                    }
                case DataSourceType.MySql:
                    {
                        return IsValidDelimitedIdentifier(text, '`', "``");
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsSafeCustomType(string value)
        {
            var text = (value ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text)
                || text.Length > 200
                || ContainsStatementDelimiterOrComment(text)
                || !CustomTypeCharacterRegex.IsMatch(text)
                || UnsafeTypeClauseRegex.IsMatch(text)
                || !HasBalancedParentheses(text))
            {
                return false;
            }

            return true;
        }

        public static bool IsSafeSqlExpression(string value)
        {
            var text = (value ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text)
                || text.Length > 500
                || ContainsStatementDelimiterOrComment(text)
                || !HasBalancedParentheses(text)
                || !HasBalancedSingleQuotes(text))
            {
                return false;
            }

            return true;
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

        private static bool ContainsStatementDelimiterOrComment(string value)
        {
            return value.IndexOf(';') >= 0
                   || value.IndexOf("--", StringComparison.Ordinal) >= 0
                   || value.IndexOf("/*", StringComparison.Ordinal) >= 0
                   || value.IndexOf("*/", StringComparison.Ordinal) >= 0
                   || value.IndexOf('\r') >= 0
                   || value.IndexOf('\n') >= 0
                   || value.IndexOf('\0') >= 0;
        }

        private static bool IsValidDelimitedIdentifier(string value, char delimiter, string escapedDelimiter)
        {
            if (value.Length < 2 || value[0] != delimiter || value[value.Length - 1] != delimiter)
            {
                return false;
            }

            var inner = value.Substring(1, value.Length - 2);

            if (string.IsNullOrEmpty(inner))
            {
                return false;
            }

            var delimiterText = delimiter.ToString();
            var normalized = inner.Replace(escapedDelimiter, string.Empty);

            return normalized.IndexOf(delimiterText, StringComparison.Ordinal) < 0;
        }

        private static bool IsValidBracketIdentifier(string value)
        {
            if (value.Length < 2 || value[0] != '[' || value[value.Length - 1] != ']')
            {
                return false;
            }

            var inner = value.Substring(1, value.Length - 2);

            if (string.IsNullOrEmpty(inner))
            {
                return false;
            }

            return inner.Replace("]]", string.Empty).IndexOf(']') < 0;
        }

        private static bool HasBalancedParentheses(string value)
        {
            var depth = 0;
            var insideSingleQuote = false;

            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];

                if (current == '\'')
                {
                    if (insideSingleQuote && i + 1 < value.Length && value[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    insideSingleQuote = !insideSingleQuote;
                    continue;
                }

                if (insideSingleQuote)
                {
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                }
                else if (current == ')')
                {
                    depth--;

                    if (depth < 0)
                    {
                        return false;
                    }
                }
            }

            return depth == 0;
        }

        private static bool HasBalancedSingleQuotes(string value)
        {
            var insideSingleQuote = false;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != '\'')
                {
                    continue;
                }

                if (insideSingleQuote && i + 1 < value.Length && value[i + 1] == '\'')
                {
                    i++;
                    continue;
                }

                insideSingleQuote = !insideSingleQuote;
            }

            return !insideSingleQuote;
        }
    }
}