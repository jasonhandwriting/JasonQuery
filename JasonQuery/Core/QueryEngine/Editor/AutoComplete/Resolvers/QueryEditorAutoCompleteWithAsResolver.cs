using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal static class QueryEditorAutoCompleteWithAsResolver
    {
        private const SqlTokenizerOptions FragmentTokenizerOptions = SqlTokenizerOptions.DisableNestedBlockComments;

        /// <summary>
        /// 搜尋 With xxx AS 子查詢
        /// 第一組才會帶 With，所以，搜尋時要以 xxx AS 為主
        /// </summary>
        /// <param name="sql">傳入要搜尋的 SQL</param>
        /// <param name="start">從哪一個位置開始往後搜尋</param>
        /// <returns></returns>
        public static string GetAutoCompleteSqlForWithAs(string sql, int start)
        {
            if (string.IsNullOrEmpty(sql) || start < 0 || start >= sql.Length)
            {
                return string.Empty;
            }

            var tokenizationResult = SqlTokenizer.Tokenize(sql, DataSourceType.None, FragmentTokenizerOptions);
            var openIndex = FindTokenIndexAtPosition(tokenizationResult, start, "(");

            if (openIndex < 0)
            {
                return string.Empty;
            }

            var openToken = tokenizationResult.Tokens[openIndex];

            for (var index = openIndex + 1; index < tokenizationResult.Tokens.Count; index++)
            {
                var token = tokenizationResult.Tokens[index];

                if (!token.IsSymbol(")") || token.Depth != openToken.Depth)
                {
                    continue;
                }

                if (token.Start - start <= 0)
                {
                    return string.Empty;
                }

                return sql.Substring(openToken.EndExclusive, token.Start - openToken.EndExclusive).Trim();
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
