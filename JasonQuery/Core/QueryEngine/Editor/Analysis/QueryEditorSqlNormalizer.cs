using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.QueryEngine.Editor.Analysis
{
    internal static class QueryEditorSqlNormalizer
    {
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

            sql = RemoveBlockComments(sql);

            var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);

            RemoveSingleLineCommentsAndTrim(parts);

            sql = RebuildSingleLineSql(parts, 300);

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

        private static string RemoveBlockComments(string sql)
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

        private static void RemoveSingleLineCommentsAndTrim(string[] parts)
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