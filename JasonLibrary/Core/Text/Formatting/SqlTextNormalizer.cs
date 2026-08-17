using SQL.Formatter.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace JasonLibrary.Core.Text.Formatting
{
    public static class SqlTextNormalizer
    {
        private const string FourSpaces = "    ";

        public static string Normalize(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            return sql.Replace("\r\n", "\n")
                      .Replace("\r", "\n")
                      .Replace("\n", "\r\n")
                      .Replace("\t", FourSpaces)
                      .TrimEnd('\r', '\n');
        }
    }

    internal static class SqlStatementSpacingNormalizer
    {
        private static readonly HashSet<string> StatementStartWords = new HashSet<string>
        (
            new[]
            {
                "ALTER", "CALL", "COMMENT", "CREATE", "DECLARE", "DELETE", "DROP", "EXEC", "EXECUTE",
                "GRANT", "INSERT", "MERGE", "REVOKE", "SELECT", "TRUNCATE", "UPDATE", "WITH"
            },
            StringComparer.OrdinalIgnoreCase
        );

        private static readonly HashSet<string> StatementContinuationWords = new HashSet<string>
        (
            new[] { "EXCEPT", "EXCEPT ALL", "INTERSECT", "INTERSECT ALL", "MINUS", "UNION", "UNION ALL" },
            StringComparer.OrdinalIgnoreCase
        );

        public static string Apply(AbstractFormatter formatter, string formattedSql, int linesBetweenStatements)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (string.IsNullOrEmpty(formattedSql))
            {
                return formattedSql;
            }

            var tokens = formatter.Tokenizer().Tokenize(formattedSql);
            var boundaries = new List<WhitespaceSpan>();
            var nestingLevel = 0;
            var currentStatementStart = string.Empty;

            Token previousToken = null;

            foreach (Token token in tokens)
            {
                var followsExplicitSeparator = nestingLevel == 0 && previousToken?.Value == ";";
                var isStatementStart = nestingLevel == 0 && IsStatementStart(token);
                var isImplicitRepeatedStatement = isStatementStart
                                                  && currentStatementStart.Length > 0
                                                  && string.Equals(currentStatementStart, token.Value, StringComparison.OrdinalIgnoreCase)
                                                  && ContainsLineBreak(token.WhitespaceBefore)
                                                  && !IsStatementContinuation(previousToken);

                if (followsExplicitSeparator || isImplicitRepeatedStatement)
                {
                    boundaries.Add(new WhitespaceSpan(token.WhitespaceStart, token.WhitespaceLength));
                }

                if (followsExplicitSeparator)
                {
                    currentStatementStart = string.Empty;
                }

                if (isStatementStart && (currentStatementStart.Length == 0 || isImplicitRepeatedStatement))
                {
                    currentStatementStart = token.Value;
                }

                if (token.Type == TokenTypes.OPEN_PAREN)
                {
                    nestingLevel++;
                }
                else if (token.Type == TokenTypes.CLOSE_PAREN && nestingLevel > 0)
                {
                    nestingLevel--;
                }

                previousToken = token;
            }

            if (boundaries.Count == 0)
            {
                return formattedSql;
            }

            var separator = new string('\n', Math.Max(1, linesBetweenStatements));
            var result = new StringBuilder(formattedSql);

            for (var index = boundaries.Count - 1; index >= 0; index--)
            {
                var boundary = boundaries[index];
                result.Remove(boundary.Start, boundary.Length);
                result.Insert(boundary.Start, separator);
            }

            return result.ToString();
        }

        private static bool IsStatementStart(Token token)
        {
            return token.Type == TokenTypes.RESERVED_TOP_LEVEL && StatementStartWords.Contains(token.Value);
        }

        private static bool IsStatementContinuation(Token token)
        {
            return token != null && StatementContinuationWords.Contains(token.Value);
        }

        private static bool ContainsLineBreak(string value)
        {
            return value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0;
        }

        private sealed class WhitespaceSpan
        {
            public WhitespaceSpan(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public int Start { get; }

            public int Length { get; }
        }
    }
}