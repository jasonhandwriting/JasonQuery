using JasonLibrary.Core.Database.Enums;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SQL.Formatter.Core;
using SQL.Formatter.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JasonLibrary.Core.Text.Formatting.Engines
{
    public sealed class MicrosoftScriptDomSqlFormatterEngine : ISqlFormatterEngine
    {
        public SqlFormatterEngineKind Kind => SqlFormatterEngineKind.MicrosoftScriptDom;

        public bool Supports(DatabaseProviderKind providerKind)
        {
            return providerKind == DatabaseProviderKind.SqlServer;
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
                    "The formatter request engine does not match Microsoft SQL ScriptDOM."
                );
            }

            if (!Supports(request.ProviderKind))
            {
                return SqlFormatResult.Failed
                (
                    Kind,
                    request.Sql,
                    "Microsoft SQL ScriptDOM supports SQL Server only."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return SqlFormatResult.Succeeded(Kind, SqlTextNormalizer.Normalize(request.Sql));
            }

            try
            {
                var parser = new TSql170Parser(true);

                TSqlFragment fragment;
                IList<ParseError> parseErrors;

                using (var reader = new StringReader(request.Sql))
                {
                    fragment = parser.Parse(reader, out parseErrors);
                }

                if (parseErrors.Count > 0)
                {
                    return SqlFormatResult.Failed(Kind, request.Sql, BuildParseErrorMessage(parseErrors));
                }

                var generator = new Sql170ScriptGenerator(CreateGeneratorOptions(request.Options));

                generator.GenerateScript(fragment, out var formattedSql);

                var scriptDomFormattedSql = SqlTextNormalizer.Normalize(formattedSql);
                var listPackerFormatter = CreateListPackerFormatter(request.Options);

                formattedSql = SqlStatementSpacingNormalizer.Apply
                (
                    listPackerFormatter,
                    scriptDomFormattedSql,
                    request.Options.LinesBetweenStatements
                );

                formattedSql = SqlListPacker.Apply
                (
                    listPackerFormatter,
                    formattedSql,
                    request.Options.ListItemsPerLine,
                    request.Options.MaxLineWidth
                );

                formattedSql = SqlTextNormalizer.Normalize(formattedSql);

                var validation = SqlTokenSemanticValidator.Validate
                (
                    DatabaseProviderKind.SqlServer,
                    scriptDomFormattedSql,
                    formattedSql
                );

                if (!validation.IsSafe)
                {
                    return SqlFormatResult.Failed
                    (
                        Kind,
                        request.Sql,
                        "Microsoft SQL ScriptDOM list layout failed token safety validation: " + validation.ErrorMessage
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
                    "Microsoft SQL ScriptDOM could not format the SQL: " + exception.Message
                );
            }
        }

        private static SqlScriptGeneratorOptions CreateGeneratorOptions(SqlFormatOptions options)
        {
            return new SqlScriptGeneratorOptions
            {
                AlignClauseBodies = false,
                AlignSetClauseItem = false,
                AsKeywordOnOwnLine = false,
                IncludeSemicolons = true,
                IndentationSize = options.IndentSize,
                KeywordCasing = ResolveKeywordCasing(options.KeywordCase),
                MultilineSelectElementsList = true,
                MultilineSetClauseItems = true,
                NewLineAfterJoinKeyword = false,
                NewLineBeforeFromClause = true,
                NewLineBeforeGroupByClause = true,
                NewLineBeforeHavingClause = true,
                NewLineBeforeJoinClause = true,
                NewLineBeforeOnClause = true,
                NewLineBeforeOrderByClause = true,
                NewLineBeforeWhereClause = true,
                NumNewlinesAfterStatement = Math.Max(1, options.LinesBetweenStatements),
                PreserveComments = true,
                SqlVersion = SqlVersion.Sql170
            };
        }

        private static AbstractFormatter CreateListPackerFormatter(SqlFormatOptions options)
        {
            var configuration = FormatConfig.Builder()
                                            .Indent(options.GetIndentString())
                                            .MaxColumnLength(options.MaxLineWidth)
                                            .Build();

            return new TSqlFormatter(configuration);
        }

        private static KeywordCasing ResolveKeywordCasing(SqlFormatterKeywordCase keywordCase)
        {
            switch (keywordCase)
            {
                case SqlFormatterKeywordCase.Lower:
                    {
                        return KeywordCasing.Lowercase;
                    }
                case SqlFormatterKeywordCase.Proper:
                    {
                        return KeywordCasing.PascalCase;
                    }
                case SqlFormatterKeywordCase.Preserve:
                case SqlFormatterKeywordCase.Upper:
                default:
                    {
                        // ScriptDOM regenerates the syntax tree and cannot preserve mixed keyword casing.
                        return KeywordCasing.Uppercase;
                    }
            }
        }

        private static string BuildParseErrorMessage(IEnumerable<ParseError> parseErrors)
        {
            return "Microsoft SQL ScriptDOM rejected the SQL: " + string.Join
                   (
                       " | ",
                       parseErrors.Select
                       (
                           error => string.Format
                           (
                               CultureInfo.InvariantCulture,
                               "{0} at line {1}, column {2}: {3}",
                               error.Number,
                               error.Line,
                               error.Column,
                               error.Message
                           )
                       )
                   );
        }
    }
}
