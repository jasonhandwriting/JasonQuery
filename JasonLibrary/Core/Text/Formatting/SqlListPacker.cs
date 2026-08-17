using SQL.Formatter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonLibrary.Core.Text.Formatting
{
    internal static class SqlListPacker
    {
        private static readonly HashSet<string> PackableClauses = new HashSet<string>
        (
            new[] { "SELECT", "GROUP BY", "ORDER BY", "ORDER SIBLINGS BY" },
            StringComparer.OrdinalIgnoreCase
        );

        public static string Apply(AbstractFormatter formatter, string formattedSql, int itemsPerLine, int maxLineWidth)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (itemsPerLine < 1 || itemsPerLine > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsPerLine));
            }

            if (maxLineWidth < 20 || maxLineWidth > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLineWidth));
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
            var currentLineLength = 0;

            Token previousToken = null;

            for (var index = 0; index < tokens.Count; index++)
            {
                var token = tokens[index];
                var whitespace = replacements.TryGetValue(token.WhitespaceStart, out var pendingReplacement)
                                 ? pendingReplacement.Value
                                 : token.WhitespaceBefore;

                currentLineLength = GetLineLengthAfter(currentLineLength, whitespace);

                var tokenStartColumn = currentLineLength;

                currentLineLength = GetLineLengthAfter(currentLineLength, token.Value);

                var keyword = IsReservedKeyword(token) ? GetKeyword(token) : string.Empty;

                if (IsSelectKeyword(token, keyword, previousToken))
                {
                    queryContexts[nestingLevel] = new QueryContext("SELECT", index);
                }
                else if (queryContexts.TryGetValue(nestingLevel, out var clauseContext) && IsClauseBoundary(token) && !IsQualifiedNamePart(previousToken))
                {
                    clauseContext.Reset(keyword, index);
                }

                if (queryContexts.TryGetValue(nestingLevel, out var currentContext)
                    && PackableClauses.Contains(currentContext.CurrentClause)
                    && index > currentContext.ClauseTokenIndex
                    && currentContext.ContinuationIndent < 0
                    && token.Value != ","
                    && !IsComment(token))
                {
                    currentContext.ContinuationIndent = tokenStartColumn;
                }

                if (queryContexts.TryGetValue(nestingLevel, out var listContext) && PackableClauses.Contains(listContext.CurrentClause) && token.Value == ",")
                {
                    var nextIndex = index + 1;

                    if (CanMergeNextItem(tokens, nextIndex, nestingLevel, listContext.ItemsOnCurrentLine, itemsPerLine, currentLineLength, maxLineWidth))
                    {
                        ReplaceWhitespace(replacements, tokens[nextIndex], " ");
                        listContext.ItemsOnCurrentLine++;
                    }
                    else
                    {
                        listContext.ItemsOnCurrentLine = 1;

                        if (nextIndex < tokens.Count && listContext.ContinuationIndent >= 0 && !ContainsLineBreak(tokens[nextIndex].WhitespaceBefore))
                        {
                            ReplaceWhitespace
                            (
                                replacements,
                                tokens[nextIndex],
                                "\n" + new string(' ', listContext.ContinuationIndent)
                            );
                        }
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

            foreach (var whitespaceReplacement in replacements.Values.OrderByDescending(item => item.Start))
            {
                result.Remove(whitespaceReplacement.Start, whitespaceReplacement.Length);
                result.Insert(whitespaceReplacement.Start, whitespaceReplacement.Value);
            }

            return result.ToString();
        }

        private static bool CanMergeNextItem(IReadOnlyList<Token> tokens, int nextIndex, int nestingLevel, int itemsOnCurrentLine,
                                             int itemsPerLine, int currentLineLength, int maxLineWidth)
        {
            if (itemsOnCurrentLine >= itemsPerLine || nextIndex >= tokens.Count || IsComment(tokens[nextIndex]))
            {
                return false;
            }

            return TryGetSingleLineItemLength(tokens, nextIndex, nestingLevel, out var itemLength)
                   && currentLineLength + 1 + itemLength <= maxLineWidth;
        }

        private static bool TryGetSingleLineItemLength(IReadOnlyList<Token> tokens, int startIndex, int startNestingLevel, out int itemLength)
        {
            itemLength = 0;

            var nestingLevel = startNestingLevel;

            for (var index = startIndex; index < tokens.Count; index++)
            {
                var token = tokens[index];

                if (index > startIndex && nestingLevel == startNestingLevel && (token.Value == "," || IsClauseBoundary(token)))
                {
                    break;
                }

                if (IsComment(token) || ContainsLineBreak(token.Value) || (index > startIndex && ContainsLineBreak(token.WhitespaceBefore)))
                {
                    return false;
                }

                if (index > startIndex)
                {
                    itemLength += token.WhitespaceBefore.Length;
                }

                itemLength += token.Value.Length;

                if (token.Type == TokenTypes.OPEN_PAREN)
                {
                    nestingLevel++;
                }
                else if (token.Type == TokenTypes.CLOSE_PAREN && nestingLevel > startNestingLevel)
                {
                    nestingLevel--;
                }
            }

            return itemLength > 0;
        }

        private static int GetLineLengthAfter(int currentLineLength, string value)
        {
            var lastLineBreak = Math.Max(value.LastIndexOf('\r'), value.LastIndexOf('\n'));

            return lastLineBreak < 0 ? currentLineLength + value.Length : value.Length - lastLineBreak - 1;
        }

        private static bool ContainsLineBreak(string value)
        {
            return value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0;
        }

        private static bool IsClauseBoundary(Token token)
        {
            return token.Type == TokenTypes.RESERVED_TOP_LEVEL || token.Type == TokenTypes.RESERVED_TOP_LEVEL_NO_INDENT;
        }

        private static bool IsSelectKeyword(Token token, string keyword, Token previousToken)
        {
            return IsReservedKeyword(token)
                   && string.Equals(keyword, "SELECT", StringComparison.OrdinalIgnoreCase)
                   && !IsQualifiedNamePart(previousToken);
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

        private static string GetKeyword(Token token)
        {
            return string.Join
            (
                " ",
                token.Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
            ).ToUpperInvariant();
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
            public QueryContext(string currentClause, int clauseTokenIndex)
            {
                CurrentClause = currentClause;
                ClauseTokenIndex = clauseTokenIndex;
                ItemsOnCurrentLine = 1;
                ContinuationIndent = -1;
            }

            public string CurrentClause { get; set; }

            public int ClauseTokenIndex { get; private set; }

            public int ItemsOnCurrentLine { get; set; }

            public int ContinuationIndent { get; set; }

            public void Reset(string currentClause, int clauseTokenIndex)
            {
                CurrentClause = currentClause;
                ClauseTokenIndex = clauseTokenIndex;
                ItemsOnCurrentLine = 1;
                ContinuationIndent = -1;
            }
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