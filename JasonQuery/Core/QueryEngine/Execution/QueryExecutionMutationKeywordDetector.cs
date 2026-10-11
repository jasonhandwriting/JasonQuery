using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.QueryEngine.Execution
{
    /// <summary>
    /// Lexical detection only. The QueryForm consumer continues to own Query/NonQuery routing.
    /// This intentionally keeps the legacy UPDATE/DELETE/INSERT policy; it does not parse CTE grammar.
    /// </summary>
    internal static class QueryExecutionMutationKeywordDetector
    {
        private const SqlTokenizerOptions CompatibilityOptions = SqlTokenizerOptions.DisableNestedBlockComments | SqlTokenizerOptions.MySqlDashCommentWithoutWhitespace | SqlTokenizerOptions.MySqlBacktickIdentifierAllowsBackslashEscape | SqlTokenizerOptions.MySqlHashStartsCommentInsideWord;

        internal static bool ContainsMutationKeyword(DataSourceType dataSourceType, string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return false;
            }

            var result = SqlTokenizer.Tokenize(sql, dataSourceType, CompatibilityOptions);
            var hasMutationWord = false;

            foreach (var token in result.Tokens)
            {
                if (token.Kind == SqlTokenKind.Word && (token.IsWord("UPDATE") || token.IsWord("DELETE") || token.IsWord("INSERT")))
                {
                    hasMutationWord = true;
                    break;
                }
            }

            if (!hasMutationWord)
            {
                return false;
            }

            //Retain the old consumer's space-delimited matching rule (" UPDATE ", etc.).
            //Reconstruct only the visible text, with protected tokens omitted rather than replaced. This avoids broadening Query/NonQuery routing for x.UPDATE or @UPDATE.
            var visible = new StringBuilder(sql.Length);
            var cursor = 0;

            foreach (var token in result.Tokens)
            {
                if (!token.SuppressesKeywordMatching)
                {
                    continue;
                }

                if (token.Start < cursor)
                {
                    continue;
                }

                visible.Append(sql, cursor, token.Start - cursor);
                cursor = token.EndExclusive;
            }

            visible.Append(sql, cursor, sql.Length - cursor);

            var visibleUpper = Regex.Replace(visible.ToString().ToUpper(), @"\s+", " ");

            return visibleUpper.Contains(" UPDATE ") || visibleUpper.Contains(" DELETE ") || visibleUpper.Contains(" INSERT ");
        }
    }
}
