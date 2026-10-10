using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal static class QueryEditorAutoCompleteSubqueryResolver
    {
        private const SqlTokenizerOptions FragmentTokenizerOptions = SqlTokenizerOptions.DisableNestedBlockComments;

        /// <summary>
        /// 搜尋 With xxx AS 子查詢
        /// </summary>
        /// <param name="sql">傳入要搜尋的 SQL</param>
        /// <param name="start">從哪一個位置開始往後搜尋</param>
        /// <returns></returns>
        public static string GetAutoCompleteSqlForSubquery(string sql, int start)
        {
            if (string.IsNullOrEmpty(sql) || start < 0 || start >= sql.Length)
            {
                return string.Empty;
            }

            var tokenizationResult = SqlTokenizer.Tokenize(sql, DataSourceType.None, FragmentTokenizerOptions);
            var closeIndex = FindTokenIndexAtPosition(tokenizationResult, start, ")");

            if (closeIndex < 0)
            {
                return string.Empty;
            }

            var closeToken = tokenizationResult.Tokens[closeIndex];

            for (var index = closeIndex - 1; index >= 0; index--)
            {
                var token = tokenizationResult.Tokens[index];

                if (!token.IsSymbol("(") || token.Depth != closeToken.Depth)
                {
                    continue;
                }

                if (token.Start <= 0 || start - token.Start - 1 <= 0)
                {
                    return string.Empty;
                }

                return sql.Substring(token.EndExclusive, start - token.EndExclusive).Trim();
            }

            return string.Empty;
        }

        private static int FindTokenIndexAtPosition(SqlTokenizationResult tokenizationResult, int position, string symbol)
        {
            if (tokenizationResult == null)
            {
                return -1;
            }

            for (var index = 0; index < tokenizationResult.Tokens.Count; index++)
            {
                var token = tokenizationResult.Tokens[index];

                if (token.Start == position && token.IsSymbol(symbol))
                {
                    return index;
                }

                if (token.Start > position)
                {
                    break;
                }
            }

            return -1;
        }
    }
}
