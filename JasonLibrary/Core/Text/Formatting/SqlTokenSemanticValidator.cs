using JasonLibrary.Core.Database.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace JasonLibrary.Core.Text.Formatting
{
    public static class SqlTokenSemanticValidator
    {
        public static SqlTokenSemanticValidationResult Validate(DatabaseProviderKind providerKind, string originalSql, string formattedSql)
        {
            return Validate(providerKind, originalSql, formattedSql, Array.Empty<int>());
        }

        public static SqlTokenSemanticValidationResult Validate(DatabaseProviderKind providerKind, string originalSql, string formattedSql, IEnumerable<int> caseChangeAuthorizedTokenIndexes)
        {
            if (originalSql == null)
            {
                throw new ArgumentNullException(nameof(originalSql));
            }

            if (formattedSql == null)
            {
                throw new ArgumentNullException(nameof(formattedSql));
            }

            if (caseChangeAuthorizedTokenIndexes == null)
            {
                throw new ArgumentNullException(nameof(caseChangeAuthorizedTokenIndexes));
            }

            var authorizedIndexes = new HashSet<int>(caseChangeAuthorizedTokenIndexes);

            if (!SqlSemanticTokenizer.TryTokenize(providerKind, originalSql, out var originalTokens, out var originalError))
            {
                return SqlTokenSemanticValidationResult.Unsafe
                (
                    -1,
                    string.Empty,
                    string.Empty,
                    "The original SQL could not be tokenized safely: " + originalError
                );
            }

            if (!SqlSemanticTokenizer.TryTokenize(providerKind, formattedSql, out var formattedTokens, out var formattedError))
            {
                return SqlTokenSemanticValidationResult.Unsafe
                (
                    -1,
                    string.Empty,
                    string.Empty,
                    "The formatted SQL could not be tokenized safely: " + formattedError
                );
            }

            var commonCount = Math.Min(originalTokens.Count, formattedTokens.Count);

            for (var index = 0; index < commonCount; index++)
            {
                var originalToken = originalTokens[index];
                var formattedToken = formattedTokens[index];

                if (!AreEquivalent(index, originalToken, formattedToken, authorizedIndexes))
                {
                    return SqlTokenSemanticValidationResult.Unsafe
                    (
                        index,
                        originalToken.DisplayValue,
                        formattedToken.DisplayValue,
                        string.Format
                        (
                            CultureInfo.InvariantCulture,
                            "SQL token {0} changed from {1} to {2}.",
                            index,
                            originalToken.Describe(),
                            formattedToken.Describe()
                        )
                    );
                }
            }

            if (originalTokens.Count != formattedTokens.Count)
            {
                var index = commonCount;
                var originalToken = index < originalTokens.Count ? originalTokens[index].DisplayValue : "<end>";
                var formattedToken = index < formattedTokens.Count ? formattedTokens[index].DisplayValue : "<end>";

                return SqlTokenSemanticValidationResult.Unsafe
                (
                    index,
                    originalToken,
                    formattedToken,
                    string.Format
                    (
                        CultureInfo.InvariantCulture,
                        "SQL token count changed from {0} to {1}; the first difference is at token {2}.",
                        originalTokens.Count,
                        formattedTokens.Count,
                        index
                    )
                );
            }

            return SqlTokenSemanticValidationResult.Safe();
        }

        private static bool AreEquivalent(int tokenIndex, SqlSemanticToken originalToken, SqlSemanticToken formattedToken, ISet<int> caseChangeAuthorizedTokenIndexes)
        {
            if (originalToken.Kind != formattedToken.Kind)
            {
                return false;
            }

            if (string.Equals(originalToken.ComparisonValue, formattedToken.ComparisonValue, StringComparison.Ordinal))
            {
                return true;
            }

            return originalToken.Kind == SqlSemanticTokenKind.Word
                   && string.Equals(originalToken.ComparisonValue, formattedToken.ComparisonValue, StringComparison.OrdinalIgnoreCase)
                   && caseChangeAuthorizedTokenIndexes.Contains(tokenIndex);
        }
    }

    internal enum SqlSemanticTokenKind
    {
        Word,
        QuotedIdentifier,
        StringLiteral,
        NumericLiteral,
        Parameter,
        Operator,
        Punctuation,
        Comment
    }

    internal sealed class SqlSemanticToken
    {
        public SqlSemanticToken(SqlSemanticTokenKind kind, string value, int startIndex, int length, string comparisonValue = null)
        {
            Kind = kind;
            DisplayValue = value ?? string.Empty;
            ComparisonValue = Canonicalize(comparisonValue ?? DisplayValue);
            StartIndex = startIndex;
            Length = length;
        }

        public SqlSemanticTokenKind Kind { get; }

        public string DisplayValue { get; }

        public string ComparisonValue { get; }

        public int StartIndex { get; }

        public int Length { get; }

        public int EndIndex => StartIndex + Length;

        public string Describe()
        {
            var value = DisplayValue.Replace("\r", "\\r").Replace("\n", "\\n");

            if (value.Length > 80)
            {
                value = value.Substring(0, 77) + "...";
            }

            return Kind + " '" + value + "'";
        }

        private static string Canonicalize(string value)
        {
            return value.Replace("\r\n", "\n")
                        .Replace("\r", "\n")
                        .Replace("\t", "    ");
        }
    }

    internal static class SqlSemanticTokenizer
    {
        private static readonly string[] MultiCharacterOperators =
        {
            "!~~*", "#>>", "->>", "!~~", "!~*", "~~*", "<=>", "::", "->", "#>", "#-", "||", "&&",
            "<=", ">=", "<>", "!=", "!<", "!>", ":=", "=>", "+=", "-=", "*=", "/=", "%=", "&=", "^=",
            "|=", "<<", ">>", "~~", "!~", "~*", "@>", "<@", "?|", "?&", ".."
        };

        public static bool TryTokenize(DatabaseProviderKind providerKind, string sql,
                                       out List<SqlSemanticToken> tokens, out string errorMessage)
        {
            tokens = new List<SqlSemanticToken>();
            errorMessage = string.Empty;

            for (var index = 0; index < sql.Length;)
            {
                var character = sql[index];

                if (char.IsWhiteSpace(character))
                {
                    index++;
                    continue;
                }

                if (StartsWith(sql, index, "--")
                    && (providerKind != DatabaseProviderKind.MySql || index + 2 >= sql.Length || char.IsWhiteSpace(sql[index + 2])))
                {
                    ReadLineComment(sql, ref index, tokens);
                    continue;
                }

                if (providerKind == DatabaseProviderKind.MySql && character == '#')
                {
                    ReadLineComment(sql, ref index, tokens);
                    continue;
                }

                if (StartsWith(sql, index, "/*"))
                {
                    if (!TryReadBlockComment(sql, ref index, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (providerKind == DatabaseProviderKind.PostgreSql && character == '$'
                    && TryReadDollarQuotedLiteral(sql, ref index, tokens, out var dollarError))
                {
                    if (dollarError.Length > 0)
                    {
                        errorMessage = dollarError;
                        return false;
                    }

                    continue;
                }

                if (providerKind == DatabaseProviderKind.Oracle
                    && (character == 'q' || character == 'Q')
                    && index + 2 < sql.Length && sql[index + 1] == '\'')
                {
                    if (!TryReadOracleQuotedLiteral(sql, ref index, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (TryGetSingleQuotePrefix(providerKind, sql, index, out var quoteIndex, out var usesBackslashEscapes))
                {
                    if (!TryReadSingleQuotedLiteral(sql, index, quoteIndex, usesBackslashEscapes,
                                                    ref index, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '\'')
                {
                    var backslashEscapes = providerKind == DatabaseProviderKind.MySql;

                    if (!TryReadSingleQuotedLiteral(sql, index, index, backslashEscapes, ref index, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '"')
                {
                    if (!TryReadDelimitedToken(sql, ref index, '"', '"', "\"\"", SqlSemanticTokenKind.QuotedIdentifier, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '[' && providerKind == DatabaseProviderKind.SqlServer)
                {
                    if (!TryReadDelimitedToken(sql, ref index, '[', ']', "]]", SqlSemanticTokenKind.QuotedIdentifier, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (character == '`'
                    && (providerKind == DatabaseProviderKind.MySql || providerKind == DatabaseProviderKind.Sqlite))
                {
                    if (!TryReadDelimitedToken(sql, ref index, '`', '`', "``", SqlSemanticTokenKind.QuotedIdentifier, tokens, out errorMessage))
                    {
                        return false;
                    }

                    continue;
                }

                if (TryReadMultiCharacterOperator(sql, ref index, tokens))
                {
                    continue;
                }

                if (IsParameterStart(sql, index))
                {
                    ReadParameter(sql, ref index, tokens);
                    continue;
                }

                if (char.IsDigit(character) || (character == '.' && index + 1 < sql.Length && char.IsDigit(sql[index + 1])))
                {
                    ReadNumber(sql, ref index, tokens);
                    continue;
                }

                if (IsWordStart(character))
                {
                    ReadWord(sql, ref index, tokens);
                    continue;
                }

                var kind = IsOperatorCharacter(character) ? SqlSemanticTokenKind.Operator : SqlSemanticTokenKind.Punctuation;

                tokens.Add(new SqlSemanticToken(kind, character.ToString(), index, 1));
                index++;
            }

            return true;
        }

        private static bool TryGetSingleQuotePrefix(DatabaseProviderKind providerKind, string sql, int index, out int quoteIndex, out bool usesBackslashEscapes)
        {
            quoteIndex = -1;
            usesBackslashEscapes = false;

            if (index + 1 < sql.Length && sql[index + 1] == '\'')
            {
                var prefix = char.ToUpperInvariant(sql[index]);

                if (prefix == 'N' || prefix == 'X' || prefix == 'B' || (prefix == 'E' && providerKind == DatabaseProviderKind.PostgreSql))
                {
                    quoteIndex = index + 1;
                    usesBackslashEscapes = prefix == 'E' || providerKind == DatabaseProviderKind.MySql;
                    return true;
                }
            }

            if (providerKind == DatabaseProviderKind.PostgreSql && index + 2 < sql.Length
                && (sql[index] == 'U' || sql[index] == 'u') && sql[index + 1] == '&' && sql[index + 2] == '\'')
            {
                quoteIndex = index + 2;
                return true;
            }

            return false;
        }

        private static bool TryReadSingleQuotedLiteral(string sql, int tokenStart, int quoteIndex, bool usesBackslashEscapes, ref int index,
                                                       ICollection<SqlSemanticToken> tokens, out string errorMessage)
        {
            for (var cursor = quoteIndex + 1; cursor < sql.Length; cursor++)
            {
                if (usesBackslashEscapes && sql[cursor] == '\\' && cursor + 1 < sql.Length)
                {
                    cursor++;
                    continue;
                }

                if (sql[cursor] != '\'')
                {
                    continue;
                }

                if (cursor + 1 < sql.Length && sql[cursor + 1] == '\'')
                {
                    cursor++;
                    continue;
                }

                index = cursor + 1;
                var displayValue = sql.Substring(tokenStart, index - tokenStart);
                var prefixLength = quoteIndex - tokenStart;
                var comparisonValue = prefixLength == 0
                                      ? displayValue
                                      : displayValue.Substring(0, prefixLength).ToUpperInvariant() + displayValue.Substring(prefixLength);

                tokens.Add
                (
                    new SqlSemanticToken
                    (
                        SqlSemanticTokenKind.StringLiteral,
                        displayValue,
                        tokenStart,
                        index - tokenStart,
                        comparisonValue
                    )
                );

                errorMessage = string.Empty;
                return true;
            }

            errorMessage = "Unterminated single-quoted literal at character " + tokenStart + ".";
            return false;
        }

        private static bool TryReadOracleQuotedLiteral(string sql, ref int index, ICollection<SqlSemanticToken> tokens, out string errorMessage)
        {
            var tokenStart = index;
            var openingDelimiter = sql[index + 2];
            var closingDelimiter = GetOracleClosingDelimiter(openingDelimiter);

            for (var cursor = index + 3; cursor + 1 < sql.Length; cursor++)
            {
                if (sql[cursor] == closingDelimiter && sql[cursor + 1] == '\'')
                {
                    index = cursor + 2;

                    var displayValue = sql.Substring(tokenStart, index - tokenStart);
                    var comparisonValue = char.ToUpperInvariant(displayValue[0]) + displayValue.Substring(1);

                    tokens.Add
                    (
                        new SqlSemanticToken
                        (
                            SqlSemanticTokenKind.StringLiteral,
                            displayValue,
                            tokenStart,
                            index - tokenStart,
                            comparisonValue
                        )
                    );

                    errorMessage = string.Empty;
                    return true;
                }
            }

            errorMessage = "Unterminated Oracle alternative-quoted literal at character " + tokenStart + ".";
            return false;
        }

        private static char GetOracleClosingDelimiter(char openingDelimiter)
        {
            switch (openingDelimiter)
            {
                case '[': return ']';
                case '{': return '}';
                case '(': return ')';
                case '<': return '>';
                default: return openingDelimiter;
            }
        }

        private static bool TryReadDollarQuotedLiteral(string sql, ref int index, ICollection<SqlSemanticToken> tokens, out string errorMessage)
        {
            var tagEnd = index + 1;

            while (tagEnd < sql.Length && sql[tagEnd] != '$')
            {
                if (!(char.IsLetterOrDigit(sql[tagEnd]) || sql[tagEnd] == '_'))
                {
                    errorMessage = string.Empty;
                    return false;
                }

                tagEnd++;
            }

            if (tagEnd >= sql.Length)
            {
                errorMessage = string.Empty;
                return false;
            }

            if (tagEnd > index + 1 && char.IsDigit(sql[index + 1]))
            {
                errorMessage = string.Empty;
                return false;
            }

            var delimiter = sql.Substring(index, tagEnd - index + 1);
            var closingIndex = sql.IndexOf(delimiter, tagEnd + 1, StringComparison.Ordinal);

            if (closingIndex < 0)
            {
                errorMessage = "Unterminated PostgreSQL dollar-quoted literal at character " + index + ".";
                return true;
            }

            var tokenStart = index;

            index = closingIndex + delimiter.Length;

            tokens.Add
            (
                new SqlSemanticToken
                (
                    SqlSemanticTokenKind.StringLiteral,
                    sql.Substring(tokenStart, index - tokenStart),
                    tokenStart,
                    index - tokenStart
                )
            );

            errorMessage = string.Empty;

            return true;
        }

        private static bool TryReadDelimitedToken(string sql, ref int index, char openingDelimiter,
                                                  char closingDelimiter, string escapedClosingDelimiter,
                                                  SqlSemanticTokenKind kind, ICollection<SqlSemanticToken> tokens,
                                                  out string errorMessage)
        {
            var tokenStart = index;
            var cursor = index + 1;

            while (cursor < sql.Length)
            {
                if (sql[cursor] != closingDelimiter)
                {
                    cursor++;
                    continue;
                }

                if (escapedClosingDelimiter.Length == 2 && cursor + 1 < sql.Length
                    && sql[cursor] == escapedClosingDelimiter[0] && sql[cursor + 1] == escapedClosingDelimiter[1])
                {
                    cursor += 2;
                    continue;
                }

                index = cursor + 1;

                tokens.Add
                (
                    new SqlSemanticToken
                    (
                        kind,
                        sql.Substring(tokenStart, index - tokenStart),
                        tokenStart,
                        index - tokenStart
                    )
                );

                errorMessage = string.Empty;
                return true;
            }

            errorMessage = "Unterminated " + openingDelimiter + " delimited token at character " + tokenStart + ".";
            return false;
        }

        private static void ReadLineComment(string sql, ref int index, ICollection<SqlSemanticToken> tokens)
        {
            var tokenStart = index;

            while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
            {
                index++;
            }

            tokens.Add
            (
                new SqlSemanticToken
                (
                    SqlSemanticTokenKind.Comment,
                    sql.Substring(tokenStart, index - tokenStart),
                    tokenStart,
                    index - tokenStart
                )
            );
        }

        private static bool TryReadBlockComment(string sql, ref int index, ICollection<SqlSemanticToken> tokens, out string errorMessage)
        {
            var tokenStart = index;
            var depth = 1;
            index += 2;

            while (index < sql.Length)
            {
                if (StartsWith(sql, index, "/*"))
                {
                    depth++;
                    index += 2;
                    continue;
                }

                if (StartsWith(sql, index, "*/"))
                {
                    depth--;
                    index += 2;

                    if (depth == 0)
                    {
                        tokens.Add
                        (
                            new SqlSemanticToken
                            (
                                SqlSemanticTokenKind.Comment,
                                sql.Substring(tokenStart, index - tokenStart),
                                tokenStart,
                                index - tokenStart
                            )
                        );

                        errorMessage = string.Empty;
                        return true;
                    }

                    continue;
                }

                index++;
            }

            errorMessage = "Unterminated block comment at character " + tokenStart + ".";
            return false;
        }

        private static bool TryReadMultiCharacterOperator(string sql, ref int index, ICollection<SqlSemanticToken> tokens)
        {
            string bestMatch = null;

            foreach (var candidate in MultiCharacterOperators)
            {
                if ((bestMatch == null || candidate.Length > bestMatch.Length) && StartsWith(sql, index, candidate))
                {
                    bestMatch = candidate;
                }
            }

            if (bestMatch == null)
            {
                return false;
            }

            tokens.Add(new SqlSemanticToken(SqlSemanticTokenKind.Operator, bestMatch, index, bestMatch.Length));
            index += bestMatch.Length;
            return true;
        }

        private static bool IsParameterStart(string sql, int index)
        {
            var character = sql[index];

            if (character == '?')
            {
                return true;
            }

            if (character != '@' && character != ':' && character != '$')
            {
                return false;
            }

            return index + 1 < sql.Length && (IsWordPart(sql[index + 1]) || (character == '@' && sql[index + 1] == '@'));
        }

        private static void ReadParameter(string sql, ref int index, ICollection<SqlSemanticToken> tokens)
        {
            var tokenStart = index++;

            if (sql[tokenStart] == '?' && (index >= sql.Length || !char.IsDigit(sql[index])))
            {
                tokens.Add(new SqlSemanticToken(SqlSemanticTokenKind.Parameter, "?", tokenStart, 1));
                return;
            }

            while (index < sql.Length && (IsWordPart(sql[index]) || sql[index] == '@'))
            {
                index++;
            }

            tokens.Add
            (
                new SqlSemanticToken
                (
                    SqlSemanticTokenKind.Parameter,
                    sql.Substring(tokenStart, index - tokenStart),
                    tokenStart,
                    index - tokenStart
                )
            );
        }

        private static void ReadNumber(string sql, ref int index, ICollection<SqlSemanticToken> tokens)
        {
            var tokenStart = index;

            if (index + 1 < sql.Length && sql[index] == '0' && (sql[index + 1] == 'x' || sql[index + 1] == 'X'))
            {
                index += 2;

                while (index < sql.Length && Uri.IsHexDigit(sql[index]))
                {
                    index++;
                }
            }
            else
            {
                var hasExponent = false;

                index++;

                while (index < sql.Length)
                {
                    var character = sql[index];

                    if (char.IsDigit(character) || character == '.')
                    {
                        index++;
                        continue;
                    }

                    if (!hasExponent && (character == 'e' || character == 'E'))
                    {
                        hasExponent = true;
                        index++;

                        if (index < sql.Length && (sql[index] == '+' || sql[index] == '-'))
                        {
                            index++;
                        }

                        continue;
                    }

                    break;
                }
            }

            tokens.Add
            (
                new SqlSemanticToken
                (
                    SqlSemanticTokenKind.NumericLiteral,
                    sql.Substring(tokenStart, index - tokenStart),
                    tokenStart,
                    index - tokenStart
                )
            );
        }

        private static void ReadWord(string sql, ref int index, ICollection<SqlSemanticToken> tokens)
        {
            var tokenStart = index++;

            while (index < sql.Length && IsWordPart(sql[index]))
            {
                index++;
            }

            tokens.Add
            (
                new SqlSemanticToken
                (
                    SqlSemanticTokenKind.Word,
                    sql.Substring(tokenStart, index - tokenStart),
                    tokenStart,
                    index - tokenStart
                )
            );
        }

        private static bool IsWordStart(char character)
        {
            return char.IsLetter(character) || character == '_' || character >= 128;
        }

        private static bool IsWordPart(char character)
        {
            return char.IsLetterOrDigit(character) || character == '_' || character == '$' || character == '#' || character >= 128;
        }

        private static bool IsOperatorCharacter(char character)
        {
            return "+-*/%=<>!|&^~".IndexOf(character) >= 0;
        }

        private static bool StartsWith(string value, int index, string candidate)
        {
            return index + candidate.Length <= value.Length
                   && string.CompareOrdinal(value, index, candidate, 0, candidate.Length) == 0;
        }
    }
}
