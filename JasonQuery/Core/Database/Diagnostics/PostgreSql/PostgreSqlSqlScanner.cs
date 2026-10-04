using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Collections.Generic;
using System.Text;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    internal static class PostgreSqlSqlScanner
    {
        private const SqlTokenizerOptions DiagnosticsTokenizerOptions = SqlTokenizerOptions.None;

        public static string NormalizeSql(string sql)
        {
            return TrimSqlTerminator(RemoveSqlComments(sql)).Trim();
        }

        public static void SkipWhiteSpace(string text, ref int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }
        }

        public static bool ReadKeyword(string text, ref int index, string keyword)
        {
            SkipWhiteSpace(text, ref index);

            if (!IsKeywordAt(text, index, keyword))
            {
                return false;
            }

            index += keyword.Length;
            return true;
        }

        public static int FindKeywordAtTopLevel(string text, string keyword, int startIndex)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
            {
                return -1;
            }

            var tokenizationResult = Tokenize(text);
            var firstIndex = Math.Max(0, startIndex);

            for (var index = firstIndex; index <= text.Length - keyword.Length; index++)
            {
                if (!IsKeywordAt(text, index, keyword))
                {
                    continue;
                }

                var token = tokenizationResult.FindTokenContaining(index);

                if (token == null || token.Depth != 0 || token.SuppressesKeywordMatching)
                {
                    continue;
                }

                return index;
            }

            return -1;
        }

        private static int LegacyFindKeywordAtTopLevel(string text, string keyword, int startIndex)
        {
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = startIndex; i <= text.Length - keyword.Length; i++)
            {
                var ch = text[i];

                if (inSingleQuote)
                {
                    if (ch == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '\'')
                    {
                        inSingleQuote = false;
                    }

                    continue;
                }

                if (inDoubleQuote)
                {
                    if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inDoubleQuote = false;
                    }

                    continue;
                }

                if (ch == '\'')
                {
                    inSingleQuote = true;
                    continue;
                }

                if (ch == '"')
                {
                    inDoubleQuote = true;
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (depth != 0)
                {
                    continue;
                }

                if (IsKeywordAt(text, i, keyword))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool TryReadQualifiedTableName(string text, ref int index, out string schemaName, out string tableName)
        {
            schemaName = string.Empty;
            tableName = string.Empty;

            if (!TryReadQualifiedNameParts(text, ref index, out var parts))
            {
                return false;
            }

            if (parts.Count == 1)
            {
                tableName = parts[0];
                return true;
            }

            schemaName = parts[parts.Count - 2];
            tableName = parts[parts.Count - 1];
            return true;
        }

        public static bool TryReadQualifiedColumnName(string text, ref int index, out string columnName)
        {
            columnName = string.Empty;

            if (!TryReadQualifiedNameParts(text, ref index, out var parts))
            {
                return false;
            }

            columnName = parts[parts.Count - 1];
            return true;
        }

        public static bool TryReadQualifiedNameParts(string text, ref int index, out List<string> parts)
        {
            parts = new List<string>();

            if (!TryReadIdentifier(text, ref index, out var firstPart))
            {
                return false;
            }

            parts.Add(firstPart);

            while (true)
            {
                SkipWhiteSpace(text, ref index);

                if (index >= text.Length || text[index] != '.')
                {
                    break;
                }

                index++;
                SkipWhiteSpace(text, ref index);

                if (!TryReadIdentifier(text, ref index, out var nextPart))
                {
                    return false;
                }

                parts.Add(nextPart);
            }

            return true;
        }

        public static bool TryReadIdentifier(string text, ref int index, out string identifier)
        {
            identifier = string.Empty;

            SkipWhiteSpace(text, ref index);

            if (index >= text.Length)
            {
                return false;
            }

            if (text[index] == '"')
            {
                return TryReadQuotedIdentifier(text, ref index, out identifier);
            }

            var start = index;

            while (index < text.Length && IsIdentifierPart(text[index]))
            {
                index++;
            }

            if (index == start)
            {
                return false;
            }

            identifier = text.Substring(start, index - start).ToLowerInvariant();
            return true;
        }

        public static bool TryReadParenthesizedContent(string text, ref int index, out string content)
        {
            content = string.Empty;

            SkipWhiteSpace(text, ref index);

            if (index >= text.Length || text[index] != '(')
            {
                return false;
            }

            var tokenizationResult = Tokenize(text);
            var openToken = FindTokenStartingAt(tokenizationResult, index);

            if (openToken == null || !openToken.IsSymbol("("))
            {
                return false;
            }

            foreach (var token in tokenizationResult.Tokens)
            {
                if (token.Start <= openToken.Start)
                {
                    continue;
                }

                if (!token.IsSymbol(")") || token.Depth != openToken.Depth)
                {
                    continue;
                }

                content = text.Substring(openToken.EndExclusive, token.Start - openToken.EndExclusive);
                index = token.EndExclusive;
                return true;
            }

            return false;
        }

        private static bool LegacyTryReadParenthesizedContent(string text, ref int index, out string content)
        {
            content = string.Empty;

            SkipWhiteSpace(text, ref index);

            if (index >= text.Length || text[index] != '(')
            {
                return false;
            }

            var start = index + 1;
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = index; i < text.Length; i++)
            {
                var ch = text[i];

                if (inSingleQuote)
                {
                    if (ch == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '\'')
                    {
                        inSingleQuote = false;
                    }

                    continue;
                }

                if (inDoubleQuote)
                {
                    if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inDoubleQuote = false;
                    }

                    continue;
                }

                if (ch == '\'')
                {
                    inSingleQuote = true;
                    continue;
                }

                if (ch == '"')
                {
                    inDoubleQuote = true;
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;

                    if (depth == 0)
                    {
                        content = text.Substring(start, i - start);
                        index = i + 1;
                        return true;
                    }

                    if (depth < 0)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public static List<string> SplitTopLevelComma(string text)
        {
            var result = new List<string>();
            var start = 0;
            var tokenizationResult = Tokenize(text);

            foreach (var token in tokenizationResult.Tokens)
            {
                if (!token.IsSymbol(",") || token.Depth != 0)
                {
                    continue;
                }

                result.Add(text.Substring(start, token.Start - start).Trim());
                start = token.EndExclusive;
            }

            result.Add(text.Substring(start).Trim());
            return result;
        }

        private static List<string> LegacySplitTopLevelComma(string text)
        {
            var result = new List<string>();
            var start = 0;
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (inSingleQuote)
                {
                    if (ch == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '\'')
                    {
                        inSingleQuote = false;
                    }

                    continue;
                }

                if (inDoubleQuote)
                {
                    if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inDoubleQuote = false;
                    }

                    continue;
                }

                if (ch == '\'')
                {
                    inSingleQuote = true;
                    continue;
                }

                if (ch == '"')
                {
                    inDoubleQuote = true;
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (ch == ',' && depth == 0)
                {
                    result.Add(text.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            result.Add(text.Substring(start).Trim());

            return result;
        }

        public static int FindTopLevelEqualSign(string text)
        {
            var tokenizationResult = Tokenize(text);

            foreach (var token in tokenizationResult.Tokens)
            {
                if (token.IsSymbol("=") && token.Depth == 0)
                {
                    return token.Start;
                }
            }

            return -1;
        }

        private static int LegacyFindTopLevelEqualSign(string text)
        {
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (inSingleQuote)
                {
                    if (ch == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '\'')
                    {
                        inSingleQuote = false;
                    }

                    continue;
                }

                if (inDoubleQuote)
                {
                    if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    if (ch == '"')
                    {
                        inDoubleQuote = false;
                    }

                    continue;
                }

                if (ch == '\'')
                {
                    inSingleQuote = true;
                    continue;
                }

                if (ch == '"')
                {
                    inDoubleQuote = true;
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (ch == '=' && depth == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool TryParseSingleQuotedStringLiteral(string text, out string value)
        {
            value = string.Empty;

            var temp = text.Trim();

            if (temp.Length < 2 || temp[0] != '\'')
            {
                return false;
            }

            var tokenizationResult = Tokenize(temp);

            if (tokenizationResult.Tokens.Count != 1)
            {
                return false;
            }

            var token = tokenizationResult.Tokens[0];

            if (token.Kind != SqlTokenKind.StringLiteral || token.Start != 0 || token.EndExclusive != temp.Length || token.Text.Length < 2 || token.Text[0] != '\'' || token.Text[token.Text.Length - 1] != '\'')
            {
                return false;
            }

            value = token.Text.Substring(1, token.Text.Length - 2).Replace("''", "'");
            return true;
        }

        private static bool LegacyTryParseSingleQuotedStringLiteral(string text, out string value)
        {
            value = string.Empty;

            var temp = text.Trim();

            if (temp.Length < 2 || temp[0] != '\'')
            {
                return false;
            }

            var sb = new StringBuilder();

            for (var i = 1; i < temp.Length; i++)
            {
                var ch = temp[i];

                if (ch == '\'' && i + 1 < temp.Length && temp[i + 1] == '\'')
                {
                    sb.Append('\'');
                    i++;
                    continue;
                }

                if (ch == '\'')
                {
                    var remaining = temp.Substring(i + 1).Trim();

                    if (remaining.Length > 0)
                    {
                        return false;
                    }

                    value = sb.ToString();
                    return true;
                }

                sb.Append(ch);
            }

            return false;
        }

        private static bool TryReadQuotedIdentifier(string text, ref int index, out string identifier)
        {
            identifier = string.Empty;

            if (index >= text.Length || text[index] != '"')
            {
                return false;
            }

            var tokenizationResult = Tokenize(text);
            var token = FindTokenStartingAt(tokenizationResult, index);

            if (token == null || token.Kind != SqlTokenKind.DelimitedIdentifier || token.Text.Length < 2 || token.Text[0] != '"' || token.Text[token.Text.Length - 1] != '"')
            {
                return false;
            }

            identifier = token.Text.Substring(1, token.Text.Length - 2).Replace("\"\"", "\"");
            index = token.EndExclusive;
            return true;
        }

        private static bool LegacyTryReadQuotedIdentifier(string text, ref int index, out string identifier)
        {
            identifier = string.Empty;

            if (index >= text.Length || text[index] != '"')
            {
                return false;
            }

            var sb = new StringBuilder();

            index++;

            while (index < text.Length)
            {
                var ch = text[index];

                if (ch == '"' && index + 1 < text.Length && text[index + 1] == '"')
                {
                    sb.Append('"');
                    index += 2;
                    continue;
                }

                if (ch == '"')
                {
                    index++;
                    identifier = sb.ToString();
                    return true;
                }

                sb.Append(ch);
                index++;
            }

            return false;
        }

        private static SqlTokenizationResult Tokenize(string text)
        {
            return SqlTokenizer.Tokenize(text, DataSourceType.PostgreSql, DiagnosticsTokenizerOptions);
        }

        private static SqlToken FindTokenStartingAt(SqlTokenizationResult tokenizationResult, int start)
        {
            if (tokenizationResult == null || start < 0)
            {
                return null;
            }

            foreach (var token in tokenizationResult.Tokens)
            {
                if (token.Start == start)
                {
                    return token;
                }

                if (token.Start > start)
                {
                    break;
                }
            }

            return null;
        }

        internal static string NormalizeSqlLegacyForParityTest(string sql)
        {
            return TrimSqlTerminator(LegacyRemoveSqlComments(sql)).Trim();
        }

        internal static int FindKeywordAtTopLevelLegacyForParityTest(string text, string keyword, int startIndex)
        {
            return LegacyFindKeywordAtTopLevel(text, keyword, startIndex);
        }

        internal static bool TryReadParenthesizedContentLegacyForParityTest(string text, ref int index, out string content)
        {
            return LegacyTryReadParenthesizedContent(text, ref index, out content);
        }

        internal static List<string> SplitTopLevelCommaLegacyForParityTest(string text)
        {
            return LegacySplitTopLevelComma(text);
        }

        internal static int FindTopLevelEqualSignLegacyForParityTest(string text)
        {
            return LegacyFindTopLevelEqualSign(text);
        }

        internal static bool TryParseSingleQuotedStringLiteralLegacyForParityTest(string text, out string value)
        {
            return LegacyTryParseSingleQuotedStringLiteral(text, out value);
        }

        private static bool IsKeywordAt(string text, int index, string keyword)
        {
            if (index < 0 || index + keyword.Length > text.Length)
            {
                return false;
            }

            if (!string.Equals(text.Substring(index, keyword.Length), keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var before = index == 0 ? '\0' : text[index - 1];
            var afterIndex = index + keyword.Length;
            var after = afterIndex >= text.Length ? '\0' : text[afterIndex];
            var beforeOk = before == '\0' || !IsIdentifierPart(before);
            var afterOk = after == '\0' || !IsIdentifierPart(after);

            return beforeOk && afterOk;
        }

        private static bool IsIdentifierPart(char ch)
        {
            return char.IsLetterOrDigit(ch) || ch == '_' || ch == '$';
        }

        private static string TrimSqlTerminator(string sql)
        {
            return sql.Trim().TrimEnd(';', '\r', '\n', ' ', '\t');
        }

        private static string RemoveSqlComments(string sql)
        {
            var sb = new StringBuilder();
            var tokenizationResult = Tokenize(sql);
            var sourceIndex = 0;

            foreach (var token in tokenizationResult.Tokens)
            {
                if (!token.IsComment)
                {
                    continue;
                }

                if (token.Start > sourceIndex)
                {
                    sb.Append(sql.Substring(sourceIndex, token.Start - sourceIndex));
                }

                sb.Append(' ');
                sourceIndex = token.EndExclusive;
            }

            if (sourceIndex < sql.Length)
            {
                sb.Append(sql.Substring(sourceIndex));
            }

            return sb.ToString();
        }

        private static string LegacyRemoveSqlComments(string sql)
        {
            var sb = new StringBuilder();
            var inSingleQuote = false;
            var inDoubleQuote = false;

            for (var i = 0; i < sql.Length; i++)
            {
                var ch = sql[i];

                if (inSingleQuote)
                {
                    sb.Append(ch);

                    if (ch == '\'' && i + 1 < sql.Length && sql[i + 1] == '\'')
                    {
                        sb.Append(sql[i + 1]);
                        i++;

                        continue;
                    }

                    if (ch == '\'')
                    {
                        inSingleQuote = false;
                    }

                    continue;
                }

                if (inDoubleQuote)
                {
                    sb.Append(ch);

                    if (ch == '"' && i + 1 < sql.Length && sql[i + 1] == '"')
                    {
                        sb.Append(sql[i + 1]);
                        i++;

                        continue;
                    }

                    if (ch == '"')
                    {
                        inDoubleQuote = false;
                    }

                    continue;
                }

                if (ch == '\'')
                {
                    inSingleQuote = true;
                    sb.Append(ch);

                    continue;
                }

                if (ch == '"')
                {
                    inDoubleQuote = true;
                    sb.Append(ch);

                    continue;
                }

                if (ch == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
                {
                    while (i < sql.Length && sql[i] != '\r' && sql[i] != '\n')
                    {
                        i++;
                    }

                    sb.Append(' ');
                    continue;
                }

                if (ch == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
                {
                    i += 2;

                    while (i + 1 < sql.Length && !(sql[i] == '*' && sql[i + 1] == '/'))
                    {
                        i++;
                    }

                    i++;
                    sb.Append(' ');

                    continue;
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
