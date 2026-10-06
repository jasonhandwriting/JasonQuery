using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.QueryEngine.Editor.Analysis
{
    internal static class QueryEditorSqlNormalizer
    {
        private const SqlTokenizerOptions NormalizerTokenizerOptions = SqlTokenizerOptions.RecognizeForeignDelimitedIdentifiers | SqlTokenizerOptions.DisableNestedBlockComments;

        /// <summary>
        /// 將整段 SQL 整理成一行 SQL，以利後續判斷用 (將註解一併移除)
        /// </summary>
        /// <param name="sql">完整 SQL 區塊</param>
        /// <param name="toUpperCase">是否轉成大寫</param>
        /// <param name="appendTrailingSpace">是否在最後補一個空白</param>
        /// <returns>整理完畢的 SQL 子句</returns>
        public static string GetSingleLineSql(string sql, bool toUpperCase = true, bool appendTrailingSpace = false)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            sql = RemoveCommentsUsingSharedTokenizer(sql);

            var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);

            TrimShortLines(parts);

            return NormalizeSingleLineSql(parts, toUpperCase, appendTrailingSpace);
        }

        internal static string GetSingleLineSqlLegacyForParityTest(string sql, bool toUpperCase = true, bool appendTrailingSpace = false)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            sql = RemoveBlockCommentsLegacy(sql);

            var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);

            RemoveSingleLineCommentsAndTrimLegacy(parts);

            return NormalizeSingleLineSql(parts, toUpperCase, appendTrailingSpace);
        }

        private static string RemoveCommentsUsingSharedTokenizer(string sql)
        {
            var tokenizationResult = SqlTokenizer.Tokenize
            (
                sql,
                DataSourceType.None,
                NormalizerTokenizerOptions
            );

            var result = new StringBuilder(sql.Length);
            var copyStart = 0;

            for (var i = 0; i < tokenizationResult.Tokens.Count; i++)
            {
                var token = tokenizationResult.Tokens[i];

                if (!token.IsComment)
                {
                    continue;
                }

                if (token.Start > copyStart)
                {
                    result.Append(sql, copyStart, token.Start - copyStart);
                }

                copyStart = Math.Max(copyStart, token.EndExclusive);
            }

            if (copyStart < sql.Length)
            {
                result.Append(sql, copyStart, sql.Length - copyStart);
            }

            return result.ToString();
        }

        private static void TrimShortLines(string[] parts)
        {
            if (parts == null)
            {
                return;
            }

            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length < 2)
                {
                    parts[i] = parts[i].Trim();
                }
            }
        }

        private static string NormalizeSingleLineSql(string[] parts, bool toUpperCase, bool appendTrailingSpace)
        {
            var sql = RebuildSingleLineSql(parts, 300);

            sql = Regex.Replace(sql, @"\r\n|\r|\n", " ");
            sql = sql.Replace(",", " , ").Replace("(", " ( ").Replace(")", " ) ");
            sql = Regex.Replace(sql, @"\s+", " ").Trim().TrimEnd(';');

            if (toUpperCase)
            {
                sql = sql.ToUpper();
            }

            if (appendTrailingSpace)
            {
                sql = string.Concat(sql, " ");
            }

            return sql;
        }

        private static string RemoveBlockCommentsLegacy(string sql)
        {
            for (var i = 0; i < 100; i++)
            {
                var indexOfStart = sql.IndexOf("/*", StringComparison.Ordinal);
                var indexOfEnd = sql.IndexOf("*/", StringComparison.Ordinal);

                if (indexOfStart == -1 || indexOfEnd == -1)
                {
                    break;
                }

                if (indexOfEnd > indexOfStart)
                {
                    var temp1 = sql.Substring(0, indexOfStart);
                    var temp2 = sql.Substring(indexOfEnd + 2, sql.Length - indexOfEnd - 2);

                    sql = string.Concat(temp1, temp2);
                }
            }

            return sql;
        }

        private static void RemoveSingleLineCommentsAndTrimLegacy(string[] parts)
        {
            if (parts == null)
            {
                return;
            }

            for (var i = 0; i < parts.Length; i++)
            {
                var index = parts[i].IndexOf("--", StringComparison.Ordinal);

                if (parts[i].Length < 2)
                {
                    parts[i] = parts[i].Trim();
                }
                else if (parts[i].StartsWith("--", StringComparison.Ordinal))
                {
                    parts[i] = string.Empty;
                }
                else if (index >= 0)
                {
                    parts[i] = parts[i].Substring(0, index);
                }
            }
        }

        private static string RebuildSingleLineSql(string[] parts, int maxLineIndex)
        {
            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length && i <= maxLineIndex; i++)
            {
                if (string.IsNullOrEmpty(parts[i]))
                {
                    continue;
                }

                sb.Append(parts[i]).Append(" ");
            }

            return sb.ToString();
        }
    }
}
