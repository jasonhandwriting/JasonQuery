using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Sql.Lexing
{
    internal static class SqlTokenizer
    {
        private const SqlTokenizerOptions DefaultOptions = SqlTokenizerOptions.RecognizeForeignDelimitedIdentifiers | SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape | SqlTokenizerOptions.MySqlDoubleQuotedTextAllowsBackslashEscape;

        public static SqlTokenizationResult Tokenize(string sql, DataSourceType dataSourceType)
        {
            return Tokenize(sql, dataSourceType, DefaultOptions);
        }

        public static SqlTokenizationResult Tokenize(string sql, DataSourceType dataSourceType, SqlTokenizerOptions options)
        {
            var tokens = new List<SqlToken>();

            if (string.IsNullOrEmpty(sql))
            {
                return new SqlTokenizationResult(tokens);
            }

            var index = 0;
            var depth = 0;

            while (index < sql.Length)
            {
                if (char.IsWhiteSpace(sql[index]))
                {
                    index++;
                    continue;
                }

                if (TryReadLineComment(sql, dataSourceType, options, ref index, depth, tokens))
                {
                    continue;
                }

                if (TryReadBlockComment(sql, options, ref index, depth, tokens))
                {
                    continue;
                }

                if (dataSourceType == DataSourceType.PostgreSql
                    && !HasOption(options, SqlTokenizerOptions.DisablePostgreSqlDollarQuotedText)
                    && TryReadPostgreSqlDollarQuotedText(sql, ref index, depth, tokens))
                {
                    continue;
                }

                if (dataSourceType == DataSourceType.Oracle
                    && !HasOption(options, SqlTokenizerOptions.DisableOracleAlternativeQuotedText)
                    && TryReadOracleAlternativeQuotedText(sql, ref index, depth, tokens))
                {
                    continue;
                }

                if (sql[index] == '\'')
                {
                    ReadDelimitedToken
                    (
                        sql,
                        ref index,
                        depth,
                        tokens,
                        SqlTokenKind.StringLiteral,
                        '\'',
                        allowDoubledClosingDelimiter: true,
                        allowBackslashEscape:
                            dataSourceType == DataSourceType.MySql
                            && !HasOption(options, SqlTokenizerOptions.DisableMySqlSingleQuotedStringBackslashEscape)
                    );

                    continue;
                }

                if (TryReadDelimitedIdentifier(sql, dataSourceType, options, ref index, depth, tokens))
                {
                    continue;
                }

                if (IsWordCharacter(sql[index], dataSourceType, options))
                {
                    var start = index;

                    index++;

                    while (index < sql.Length && IsWordCharacter(sql[index], dataSourceType, options))
                    {
                        index++;
                    }

                    AddToken(tokens, SqlTokenKind.Word, sql, start, index, depth);
                    continue;
                }

                if (sql[index] == '(')
                {
                    AddToken(tokens, SqlTokenKind.Symbol, sql, index, index + 1, depth);
                    depth++;
                    index++;

                    continue;
                }

                if (sql[index] == ')')
                {
                    depth = Math.Max(0, depth - 1);
                    AddToken(tokens, SqlTokenKind.Symbol, sql, index, index + 1, depth);
                    index++;

                    continue;
                }

                AddToken(tokens, SqlTokenKind.Symbol, sql, index, index + 1, depth);
                index++;
            }

            return new SqlTokenizationResult(tokens);
        }

        private static bool TryReadLineComment(string sql, DataSourceType dataSourceType, SqlTokenizerOptions options, ref int index, int depth, List<SqlToken> tokens)
        {
            var start = index;
            var isComment = false;

            if (dataSourceType == DataSourceType.MySql && sql[index] == '#')
            {
                isComment = true;
                index++;
            }
            else if (index + 1 < sql.Length && sql[index] == '-' && sql[index + 1] == '-')
            {
                if (dataSourceType != DataSourceType.MySql || HasOption(options, SqlTokenizerOptions.MySqlDashCommentWithoutWhitespace) || index + 2 >= sql.Length || char.IsWhiteSpace(sql[index + 2]) || char.IsControl(sql[index + 2]))
                {
                    isComment = true;
                    index += 2;
                }
            }

            if (!isComment)
            {
                return false;
            }

            while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
            {
                index++;
            }

            AddToken(tokens, SqlTokenKind.LineComment, sql, start, index, depth);
            return true;
        }

        private static bool TryReadBlockComment(string sql, SqlTokenizerOptions options, ref int index, int depth, List<SqlToken> tokens)
        {
            if (index + 1 >= sql.Length || sql[index] != '/' || sql[index + 1] != '*')
            {
                return false;
            }

            var start = index;
            var blockDepth = 1;
            var allowNestedComments = !HasOption(options, SqlTokenizerOptions.DisableNestedBlockComments);

            index += 2;

            while (index < sql.Length && blockDepth > 0)
            {
                if (allowNestedComments && index + 1 < sql.Length && sql[index] == '/' && sql[index + 1] == '*')
                {
                    blockDepth++;
                    index += 2;
                    continue;
                }

                if (index + 1 < sql.Length && sql[index] == '*' && sql[index + 1] == '/')
                {
                    blockDepth--;
                    index += 2;
                    continue;
                }

                index++;
            }

            AddToken(tokens, SqlTokenKind.BlockComment, sql, start, index, depth);
            return true;
        }

        private static bool TryReadPostgreSqlDollarQuotedText(string sql, ref int index, int depth, List<SqlToken> tokens)
        {
            if (sql[index] != '$')
            {
                return false;
            }

            var delimiterEnd = index + 1;

            while (delimiterEnd < sql.Length && (char.IsLetterOrDigit(sql[delimiterEnd]) || sql[delimiterEnd] == '_'))
            {
                delimiterEnd++;
            }

            if (delimiterEnd >= sql.Length || sql[delimiterEnd] != '$')
            {
                return false;
            }

            var start = index;
            var delimiter = sql.Substring(start, delimiterEnd - start + 1);
            var contentStart = delimiterEnd + 1;
            var closingPosition = sql.IndexOf(delimiter, contentStart, StringComparison.Ordinal);

            index = closingPosition >= 0 ? closingPosition + delimiter.Length : sql.Length;

            AddToken(tokens, SqlTokenKind.StringLiteral, sql, start, index, depth);
            return true;
        }

        private static bool TryReadOracleAlternativeQuotedText(string sql, ref int index, int depth, List<SqlToken> tokens)
        {
            if (index + 2 >= sql.Length || (sql[index] != 'q' && sql[index] != 'Q') || sql[index + 1] != '\'')
            {
                return false;
            }

            var start = index;
            var openingDelimiter = sql[index + 2];
            var closingDelimiter = GetOracleClosingDelimiter(openingDelimiter);
            var searchIndex = index + 3;
            var endExclusive = sql.Length;

            while (searchIndex + 1 < sql.Length)
            {
                if (sql[searchIndex] == closingDelimiter && sql[searchIndex + 1] == '\'')
                {
                    endExclusive = searchIndex + 2;
                    break;
                }

                searchIndex++;
            }

            index = endExclusive;
            AddToken(tokens, SqlTokenKind.StringLiteral, sql, start, index, depth);

            return true;
        }

        private static bool TryReadDelimitedIdentifier(string sql, DataSourceType dataSourceType, SqlTokenizerOptions options,
                                                       ref int index, int depth, List<SqlToken> tokens)
        {
            var openingDelimiter = sql[index];
            char closingDelimiter;

            switch (openingDelimiter)
            {
                case '"':
                    {
                        closingDelimiter = '"';
                        break;
                    }
                case '[':
                    {
                        if (dataSourceType != DataSourceType.SqlServer
                            && !HasOption(options, SqlTokenizerOptions.RecognizeForeignDelimitedIdentifiers))
                        {
                            return false;
                        }

                        closingDelimiter = ']';
                        break;
                    }
                case '`':
                    {
                        if (dataSourceType != DataSourceType.MySql
                            && !HasOption(options, SqlTokenizerOptions.RecognizeForeignDelimitedIdentifiers))
                        {
                            return false;
                        }

                        closingDelimiter = '`';
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            var allowBackslashEscape = false;

            if (dataSourceType == DataSourceType.MySql && openingDelimiter == '`')
            {
                allowBackslashEscape = HasOption(options, SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape);
            }
            else if (dataSourceType == DataSourceType.MySql && openingDelimiter == '"')
            {
                allowBackslashEscape = HasOption(options, SqlTokenizerOptions.MySqlDoubleQuotedTextAllowsBackslashEscape);
            }

            ReadDelimitedToken
            (
                sql,
                ref index,
                depth,
                tokens,
                SqlTokenKind.DelimitedIdentifier,
                closingDelimiter,
                allowDoubledClosingDelimiter: true,
                allowBackslashEscape: allowBackslashEscape
            );

            return true;
        }

        private static void ReadDelimitedToken(string sql, ref int index, int depth, List<SqlToken> tokens,
                                               SqlTokenKind kind, char closingDelimiter,
                                               bool allowDoubledClosingDelimiter, bool allowBackslashEscape)
        {
            var start = index;
            index++;

            while (index < sql.Length)
            {
                if (allowBackslashEscape && sql[index] == '\\' && index + 1 < sql.Length)
                {
                    index += 2;
                    continue;
                }

                if (sql[index] != closingDelimiter)
                {
                    index++;
                    continue;
                }

                if (allowDoubledClosingDelimiter && index + 1 < sql.Length && sql[index + 1] == closingDelimiter)
                {
                    index += 2;
                    continue;
                }

                index++;
                break;
            }

            AddToken(tokens, kind, sql, start, index, depth);
        }

        private static char GetOracleClosingDelimiter(char openingDelimiter)
        {
            switch (openingDelimiter)
            {
                case '[':
                    {
                        return ']';
                    }
                case '(':
                    {
                        return ')';
                    }
                case '{':
                    {
                        return '}';
                    }
                case '<':
                    {
                        return '>';
                    }
                default:
                    {
                        return openingDelimiter;
                    }
            }
        }

        private static bool IsWordCharacter(char value, DataSourceType dataSourceType, SqlTokenizerOptions options)
        {
            if (value == '#' && dataSourceType == DataSourceType.MySql && HasOption(options, SqlTokenizerOptions.MySqlHashStartsCommentInsideWord))
            {
                return false;
            }

            return char.IsLetterOrDigit(value) || value == '_' || value == '$' || value == '#';
        }

        private static bool HasOption(SqlTokenizerOptions options, SqlTokenizerOptions value)
        {
            return (options & value) == value;
        }

        private static void AddToken(List<SqlToken> tokens, SqlTokenKind kind, string sql, int start, int endExclusive, int depth)
        {
            tokens.Add
            (
                new SqlToken
                (
                    kind,
                    sql.Substring(start, endExclusive - start),
                    start,
                    endExclusive,
                    depth
                )
            );
        }
    }
}
