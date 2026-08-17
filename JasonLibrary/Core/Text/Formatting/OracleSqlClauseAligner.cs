using SQL.Formatter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonLibrary.Core.Text.Formatting
{
    internal static class OracleSqlClauseAligner
    {
        private static readonly HashSet<string> ListClauses = new HashSet<string>
        (
            new[] { "SELECT", "FROM", "GROUP BY", "ORDER BY", "ORDER SIBLINGS BY" },
            StringComparer.OrdinalIgnoreCase
        );

        public static string Apply(AbstractFormatter formatter, string formattedSql)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (string.IsNullOrEmpty(formattedSql))
            {
                return formattedSql;
            }

            var tokens = formatter.Tokenizer().Tokenize(formattedSql).ToList();

            if (tokens.Count == 0)
            {
                return formattedSql;
            }

            var replacements = new Dictionary<int, WhitespaceReplacement>();
            var queryContexts = new Dictionary<int, QueryContext>();
            var nestingLevel = 0;

            Token previousToken = null;

            for (var index = 0; index < tokens.Count; index++)
            {
                var token = tokens[index];
                var keyword = IsReservedKeyword(token) ? GetKeyword(token) : string.Empty;

                if (IsSelectKeyword(token, keyword, previousToken))
                {
                    var baseIndent = GetTrailingIndentLength(token.WhitespaceBefore);
                    var context = new QueryContext(baseIndent, "SELECT");

                    queryContexts[nestingLevel] = context;

                    ReplaceWhitespace
                    (
                        replacements,
                        token,
                        BuildSelectWhitespace(token, previousToken, baseIndent)
                    );

                    MergeClauseWithFirstContent
                    (
                        tokens,
                        index,
                        replacements,
                        GetListContentIndent(context)
                    );
                }
                else if (queryContexts.TryGetValue(nestingLevel, out var context) && IsReservedKeyword(token) && !IsQualifiedNamePart(previousToken))
                {
                    if (TryGetClauseIndent(keyword, out var clauseIndent))
                    {
                        context.CurrentClause = keyword;
                        context.PendingBetweenCount = 0;

                        ReplaceWhitespace
                        (
                            replacements,
                            token,
                            "\n" + new string(' ', context.BaseIndent + clauseIndent)
                        );

                        MergeClauseWithFirstContent
                        (
                            tokens,
                            index,
                            replacements,
                            context.BaseIndent + clauseIndent + keyword.Length + 1
                        );
                    }
                    else if (IsJoinKeyword(keyword))
                    {
                        context.CurrentClause = "JOIN";
                        context.PendingBetweenCount = 0;

                        ReplaceWhitespace
                        (
                            replacements,
                            token,
                            "\n" + new string(' ', context.BaseIndent + 2)
                        );

                        MergeClauseWithFirstContent
                        (
                            tokens,
                            index,
                            replacements,
                            context.BaseIndent + 2 + keyword.Length + 1
                        );
                    }
                    else if (string.Equals(keyword, "ON", StringComparison.OrdinalIgnoreCase))
                    {
                        context.CurrentClause = "ON";
                        context.PendingBetweenCount = 0;

                        ReplaceWhitespace
                        (
                            replacements,
                            token,
                            "\n" + new string(' ', context.BaseIndent + 4)
                        );

                        MergeClauseWithFirstContent
                        (
                            tokens,
                            index,
                            replacements,
                            context.BaseIndent + 7
                        );
                    }
                    else if (string.Equals(keyword, "BETWEEN", StringComparison.OrdinalIgnoreCase) && IsConditionClause(context.CurrentClause))
                    {
                        context.PendingBetweenCount++;
                    }
                    else if (string.Equals(keyword, "AND", StringComparison.OrdinalIgnoreCase) && IsConditionClause(context.CurrentClause))
                    {
                        if (context.PendingBetweenCount > 0)
                        {
                            context.PendingBetweenCount--;
                            ReplaceWhitespace(replacements, token, " ");
                        }
                        else
                        {
                            ReplaceWhitespace
                            (
                                replacements,
                                token,
                                "\n" + new string(' ', context.BaseIndent + 3)
                            );

                            MergeClauseWithFirstContent
                            (
                                tokens,
                                index,
                                replacements,
                                context.BaseIndent + 7
                            );
                        }
                    }
                    else if (string.Equals(keyword, "OR", StringComparison.OrdinalIgnoreCase) && IsConditionClause(context.CurrentClause))
                    {
                        ReplaceWhitespace
                        (
                            replacements,
                            token,
                            "\n" + new string(' ', context.BaseIndent + 4)
                        );

                        MergeClauseWithFirstContent
                        (
                            tokens,
                            index,
                            replacements,
                            context.BaseIndent + 7
                        );
                    }
                    else if (token.Type == TokenTypes.RESERVED_TOP_LEVEL || token.Type == TokenTypes.RESERVED_TOP_LEVEL_NO_INDENT)
                    {
                        context.CurrentClause = keyword;
                        context.PendingBetweenCount = 0;
                    }
                }

                if (queryContexts.TryGetValue(nestingLevel, out var listContext) && ListClauses.Contains(listContext.CurrentClause) && token.Value == ",")
                {
                    var nextIndex = index + 1;

                    if (nextIndex < tokens.Count && !IsComment(tokens[nextIndex]))
                    {
                        ReplaceWhitespace
                        (
                            replacements,
                            tokens[nextIndex],
                            "\n" + new string(' ', GetListContentIndent(listContext))
                        );
                    }
                }

                if (token.Type == TokenTypes.OPEN_PAREN)
                {
                    nestingLevel++;
                }
                else if (token.Type == TokenTypes.CLOSE_PAREN && nestingLevel > 0)
                {
                    queryContexts.Remove(nestingLevel);
                    nestingLevel--;
                }

                previousToken = token;
            }

            if (replacements.Count == 0)
            {
                return formattedSql;
            }

            var result = new StringBuilder(formattedSql);

            foreach (var replacement in replacements.Values.OrderByDescending(item => item.Start))
            {
                result.Remove(replacement.Start, replacement.Length);
                result.Insert(replacement.Start, replacement.Value);
            }

            return result.ToString();
        }

        private static void MergeClauseWithFirstContent(IReadOnlyList<Token> tokens, int clauseIndex,
                                                        IDictionary<int, WhitespaceReplacement> replacements,
                                                        int continuationIndent)
        {
            var contentIndex = clauseIndex + 1;

            if (contentIndex >= tokens.Count)
            {
                return;
            }

            var contentToken = tokens[contentIndex];

            if (IsComment(contentToken))
            {
                if (!IsOracleHint(contentToken))
                {
                    contentIndex++;

                    if (contentIndex < tokens.Count && !IsComment(tokens[contentIndex]))
                    {
                        var lineBreak = EndsWithLineBreak(contentToken.Value) ? string.Empty : "\n";

                        ReplaceWhitespace
                        (
                            replacements,
                            tokens[contentIndex],
                            lineBreak + new string(' ', continuationIndent)
                        );
                    }

                    return;
                }

                ReplaceWhitespace(replacements, contentToken, " ");
                contentIndex++;

                if (contentIndex >= tokens.Count || IsComment(tokens[contentIndex]))
                {
                    return;
                }

                contentToken = tokens[contentIndex];
            }

            ReplaceWhitespace(replacements, contentToken, " ");

            if (IsSelectModifier(contentToken) && contentIndex + 1 < tokens.Count && !IsComment(tokens[contentIndex + 1]))
            {
                ReplaceWhitespace(replacements, tokens[contentIndex + 1], " ");
            }
        }

        private static bool EndsWithLineBreak(string value)
        {
            return !string.IsNullOrEmpty(value) && (value.EndsWith("\r", StringComparison.Ordinal) || value.EndsWith("\n", StringComparison.Ordinal));
        }

        private static int GetListContentIndent(QueryContext context)
        {
            return context.BaseIndent + GetClauseLeadingIndent(context.CurrentClause) + context.CurrentClause.Length + 1;
        }

        private static int GetClauseLeadingIndent(string keyword)
        {
            return TryGetClauseIndent(keyword, out var indent) ? indent : 0;
        }

        private static bool TryGetClauseIndent(string keyword, out int indent)
        {
            switch (keyword)
            {
                case "FROM":
                    {
                        indent = 2;
                        return true;
                    }
                case "WHERE":
                case "GROUP BY":
                case "ORDER BY":
                case "ORDER SIBLINGS BY":
                    {
                        indent = 1;
                        return true;
                    }
                case "HAVING":
                    {
                        indent = 0;
                        return true;
                    }
                default:
                    {
                        indent = 0;
                        return false;
                    }
            }
        }

        private static bool IsConditionClause(string clause)
        {
            return string.Equals(clause, "WHERE", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(clause, "HAVING", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(clause, "ON", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsJoinKeyword(string keyword)
        {
            return string.Equals(keyword, "JOIN", StringComparison.OrdinalIgnoreCase) || keyword.EndsWith(" JOIN", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSelectKeyword(Token token, string keyword, Token previousToken)
        {
            return IsReservedKeyword(token)
                   && string.Equals(keyword, "SELECT", StringComparison.OrdinalIgnoreCase)
                   && !IsQualifiedNamePart(previousToken);
        }

        private static bool IsSelectModifier(Token token)
        {
            var keyword = GetKeyword(token);

            return string.Equals(keyword, "ALL", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(keyword, "DISTINCT", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(keyword, "UNIQUE", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReservedKeyword(Token token)
        {
            switch (token.Type)
            {
                case TokenTypes.RESERVED:
                case TokenTypes.RESERVED_NEWLINE:
                case TokenTypes.RESERVED_TOP_LEVEL:
                case TokenTypes.RESERVED_TOP_LEVEL_NO_INDENT:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool IsQualifiedNamePart(Token previousToken)
        {
            return previousToken != null && previousToken.Value == ".";
        }

        private static bool IsComment(Token token)
        {
            return token != null && (token.Type == TokenTypes.LINE_COMMENT || token.Type == TokenTypes.BLOCK_COMMENT);
        }

        private static bool IsOracleHint(Token token)
        {
            return token.Value.TrimStart().StartsWith("/*+", StringComparison.Ordinal);
        }

        private static string GetKeyword(Token token)
        {
            return string.Join
            (
                " ",
                token.Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            ).ToUpperInvariant();
        }

        private static string BuildSelectWhitespace(Token token, Token previousToken, int baseIndent)
        {
            var lineBreakCount = CountLineBreaks(token.WhitespaceBefore);

            if (previousToken == null)
            {
                return lineBreakCount == 0 ? new string(' ', baseIndent) : new string('\n', lineBreakCount) + new string(' ', baseIndent);
            }

            return new string('\n', Math.Max(1, lineBreakCount)) + new string(' ', baseIndent);
        }

        private static int CountLineBreaks(string value)
        {
            var count = 0;

            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] == '\r')
                {
                    count++;

                    if (index + 1 < value.Length && value[index + 1] == '\n')
                    {
                        index++;
                    }
                }
                else if (value[index] == '\n')
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetTrailingIndentLength(string whitespace)
        {
            var lastLineBreak = Math.Max(whitespace.LastIndexOf('\r'), whitespace.LastIndexOf('\n'));

            return whitespace.Length - lastLineBreak - 1;
        }

        private static void ReplaceWhitespace(IDictionary<int, WhitespaceReplacement> replacements, Token token, string value)
        {
            replacements[token.WhitespaceStart] = new WhitespaceReplacement
            (
                token.WhitespaceStart,
                token.WhitespaceLength,
                value
            );
        }

        private sealed class QueryContext
        {
            public QueryContext(int baseIndent, string currentClause)
            {
                BaseIndent = baseIndent;
                CurrentClause = currentClause;
            }

            public int BaseIndent { get; }

            public string CurrentClause { get; set; }

            public int PendingBetweenCount { get; set; }
        }

        private sealed class WhitespaceReplacement
        {
            public WhitespaceReplacement(int start, int length, string value)
            {
                Start = start;
                Length = length;
                Value = value;
            }

            public int Start { get; }

            public int Length { get; }

            public string Value { get; }
        }
    }
}