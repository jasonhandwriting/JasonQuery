using JasonLibrary.Core.Database.Enums;
using SQL.Formatter.Core;
using SQL.Formatter.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonLibrary.Core.Text.Formatting.Engines
{
    public sealed class HogimnSqlFormatterEngine : ISqlFormatterEngine
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

        public SqlFormatterEngineKind Kind => SqlFormatterEngineKind.Hogimn;

        public bool Supports(DatabaseProviderKind providerKind)
        {
            switch (providerKind)
            {
                case DatabaseProviderKind.Oracle:
                case DatabaseProviderKind.PostgreSql:
                case DatabaseProviderKind.SqlServer:
                case DatabaseProviderKind.MySql:
                case DatabaseProviderKind.Sqlite:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public SqlFormatResult Format(SqlFormatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.EngineKind != Kind)
            {
                return SqlFormatResult.Failed
                (
                    Kind,
                    request.Sql,
                    "The formatter request engine does not match Hogimn SQL Formatter."
                );
            }

            if (!Supports(request.ProviderKind))
            {
                return SqlFormatResult.Failed
                (
                    Kind,
                    request.Sql,
                    "Hogimn SQL Formatter does not support the requested database provider."
                );
            }

            if (request.Options.KeywordCase == SqlFormatterKeywordCase.Proper)
            {
                return SqlFormatResult.Failed
                (
                    Kind,
                    request.Sql,
                    "Hogimn SQL Formatter does not support proper-case keywords."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return SqlFormatResult.Succeeded(Kind, SqlTextNormalizer.Normalize(request.Sql));
            }

            try
            {
                var configuration = CreateConfiguration(request.Options);
                var formatter = CreateFormatter(request.ProviderKind, configuration);

                var caseChangeAuthorizedTokenIndexes = GetCaseChangeAuthorizedTokenIndexes
                (
                    formatter,
                    request.ProviderKind,
                    request.Sql,
                    request.Options.KeywordCase
                );

                var formattedSql = formatter.Format(request.Sql);

                if (formattedSql == null)
                {
                    return SqlFormatResult.Failed
                    (
                        Kind,
                        request.Sql,
                        "Hogimn SQL Formatter returned no formatted SQL."
                    );
                }

                formattedSql = ApplyStatementSpacing
                (
                    formatter,
                    formattedSql,
                    request.Options.LinesBetweenStatements
                );

                if (request.ProviderKind == DatabaseProviderKind.Oracle)
                {
                    formattedSql = OracleSqlClauseAligner.Apply(formatter, formattedSql);
                }

                formattedSql = SqlListPacker.Apply
                (
                    formatter,
                    formattedSql,
                    request.Options.ListItemsPerLine,
                    request.Options.MaxLineWidth
                );

                formattedSql = SqlTextNormalizer.Normalize(formattedSql);

                var validation = SqlTokenSemanticValidator.Validate
                (
                    request.ProviderKind,
                    request.Sql,
                    formattedSql,
                    caseChangeAuthorizedTokenIndexes
                );

                if (!validation.IsSafe)
                {
                    return SqlFormatResult.Failed
                    (
                        Kind,
                        request.Sql,
                        "Hogimn SQL Formatter output failed token safety validation: " + validation.ErrorMessage
                    );
                }

                return SqlFormatResult.Succeeded(Kind, formattedSql);
            }
            catch (Exception exception)
            {
                return SqlFormatResult.Failed
                (
                    Kind,
                    request.Sql,
                    "Hogimn SQL Formatter could not format the SQL: " + exception.Message
                );
            }
        }

        private static AbstractFormatter CreateFormatter(DatabaseProviderKind providerKind, FormatConfig configuration)
        {
            switch (providerKind)
            {
                case DatabaseProviderKind.Oracle:
                    {
                        return new JasonQueryOracleFormatter(configuration);
                    }
                case DatabaseProviderKind.PostgreSql:
                    {
                        return new PostgreSqlFormatter(configuration);
                    }
                case DatabaseProviderKind.SqlServer:
                    {
                        return new TSqlFormatter(configuration);
                    }
                case DatabaseProviderKind.MySql:
                    {
                        return new MySqlFormatter(configuration);
                    }
                case DatabaseProviderKind.Sqlite:
                    {
                        return new StandardSqlFormatter(configuration);
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(providerKind));
                    }
            }
        }

        private static FormatConfig CreateConfiguration(SqlFormatOptions options)
        {
            return FormatConfig.Builder()
                               .Indent(options.GetIndentString())
                               .MaxColumnLength(options.MaxLineWidth)
                               .LinesBetweenQueries(options.LinesBetweenStatements)
                               .Case(ResolveKeywordCase(options.KeywordCase))
                               .Build();
        }

        private static ISet<int> GetCaseChangeAuthorizedTokenIndexes(AbstractFormatter formatter, DatabaseProviderKind providerKind, string sql, SqlFormatterKeywordCase keywordCase)
        {
            var authorizedIndexes = new HashSet<int>();

            if (keywordCase == SqlFormatterKeywordCase.Preserve)
            {
                return authorizedIndexes;
            }

            if (!SqlSemanticTokenizer.TryTokenize(providerKind, sql, out var semanticTokens, out _))
            {
                return authorizedIndexes;
            }

            var semanticIndex = 0;

            foreach (Token formatterToken in formatter.Tokenizer().Tokenize(sql))
            {
                var formatterTokenStart = formatterToken.WhitespaceStart + formatterToken.WhitespaceLength;
                var formatterTokenEnd = formatterTokenStart + formatterToken.Value.Length;

                while (semanticIndex < semanticTokens.Count
                       && semanticTokens[semanticIndex].EndIndex <= formatterTokenStart)
                {
                    semanticIndex++;
                }

                if (!CanChangeCase(formatterToken.Type))
                {
                    continue;
                }

                for (var index = semanticIndex; index < semanticTokens.Count; index++)
                {
                    var semanticToken = semanticTokens[index];

                    if (semanticToken.StartIndex >= formatterTokenEnd)
                    {
                        break;
                    }

                    if (semanticToken.Kind == SqlSemanticTokenKind.Word
                        && semanticToken.StartIndex >= formatterTokenStart
                        && semanticToken.EndIndex <= formatterTokenEnd)
                    {
                        authorizedIndexes.Add(index);
                    }
                }
            }

            return authorizedIndexes;
        }

        private static bool CanChangeCase(TokenTypes tokenType)
        {
            switch (tokenType)
            {
                case TokenTypes.RESERVED:
                case TokenTypes.RESERVED_TOP_LEVEL:
                case TokenTypes.RESERVED_TOP_LEVEL_NO_INDENT:
                case TokenTypes.RESERVED_NEWLINE:
                case TokenTypes.OPEN_PAREN:
                case TokenTypes.CLOSE_PAREN:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static string ApplyStatementSpacing(AbstractFormatter formatter, string formattedSql, int linesBetweenStatements)
        {
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

        private static CaseTypes ResolveKeywordCase(SqlFormatterKeywordCase keywordCase)
        {
            switch (keywordCase)
            {
                case SqlFormatterKeywordCase.Upper:
                    {
                        return CaseTypes.UPPER;
                    }
                case SqlFormatterKeywordCase.Lower:
                    {
                        return CaseTypes.LOWER;
                    }
                case SqlFormatterKeywordCase.Preserve:
                    {
                        return CaseTypes.NONE;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(keywordCase));
                    }
            }
        }

        private sealed class JasonQueryOracleFormatter : PlSqlFormatter
        {
            public JasonQueryOracleFormatter(FormatConfig configuration) : base(configuration)
            {
            }

            public override DialectConfig DoDialectConfig()
            {
                var configuration = base.DoDialectConfig();
                var reservedWords = configuration.ReservedWords
                                                 .Where(word => !IsAliasSensitiveReservedWord(word))
                                                 .ToList();

                return configuration.WithReservedWords(reservedWords);
            }

            private static bool IsAliasSensitiveReservedWord(string word)
            {
                return string.Equals(word, "A", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(word, "C", StringComparison.OrdinalIgnoreCase);
            }
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
