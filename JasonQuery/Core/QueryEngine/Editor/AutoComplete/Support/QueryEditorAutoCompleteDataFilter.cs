using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteDataFilter
    {
        private enum AutoCompleteMatchBucket
        {
            Exact = 0,
            StartsWith = 1,
            BoundaryContains = 2,
            Contains = 3,
            NoMatch = int.MaxValue
        }

        private sealed class FilterRowInfo
        {
            public DataRow Row { get; set; }

            public int OriginalIndex { get; set; }

            public string MatchText { get; set; }

            public AutoCompleteMatchBucket MatchBucket { get; set; }

            public int MatchIndex { get; set; }

            public int TextLength { get; set; }

            public string TieBreakerKey { get; set; }
        }

        /// <summary>
        /// Period AutoComplete：以 ColumnName 做關鍵字篩選與排序。
        /// </summary>
        public static DataTable FilterPeriodByKeyword(DataTable source, string keyword)
        {
            return FilterByKeyword
            (
                source,
                keyword,
                "ColumnName",
                fallbackToAllWhenNoMatch: false,
                tieBreakerColumns: new[] { "ColumnName", "DataType", "BaseTableName" }
            );
        }

        /// <summary>
        /// Space AutoComplete：自動判斷主要比對欄位。
        /// 優先使用 SchemaName，其次 ColumnName。
        /// </summary>
        public static DataTable FilterSpaceByKeyword(DataTable source, string keyword)
        {
            var matchColumnName = ResolveSpaceMatchColumnName(source);

            if (string.IsNullOrWhiteSpace(matchColumnName))
            {
                return source == null ? new DataTable() : source.Copy();
            }

            return FilterByKeyword
            (
                source,
                keyword,
                matchColumnName,
                fallbackToAllWhenNoMatch: false,
                tieBreakerColumns: new[] { "SchemaName", "ColumnName", "SchemaType", "DataType", "BaseTableName" }
            );
        }

        /// <summary>
        /// 通用關鍵字篩選：
        /// 1. Exact
        /// 2. StartsWith
        /// 3. BoundaryContains
        /// 4. Contains
        ///
        /// 同一 Bucket 內排序：
        /// 1. MatchIndex
        /// 2. Length
        /// 3. Alphabetical
        /// 4. TieBreaker
        /// 5. OriginalIndex
        /// </summary>
        public static DataTable FilterByKeyword(DataTable source, string keyword, string matchColumnName,
                                                bool fallbackToAllWhenNoMatch = false, IEnumerable<string> tieBreakerColumns = null)
        {
            if (source == null)
            {
                return new DataTable();
            }

            if (string.IsNullOrWhiteSpace(matchColumnName) || !source.Columns.Contains(matchColumnName))
            {
                return source.Copy();
            }

            var normalizedKeyword = (keyword ?? string.Empty).Trim();

            if (normalizedKeyword.Length == 0)
            {
                return source.Copy();
            }

            var tieBreakerColumnList = (tieBreakerColumns ?? Enumerable.Empty<string>())
                                       .Where(c => !string.IsNullOrWhiteSpace(c))
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToList();

            var matchedRows = source.AsEnumerable()
                                    .Select((row, index) =>
                                     {
                                         var matchText = row.GetSafeString(matchColumnName);
                                         var matchInfo = GetMatchInfo(matchText, normalizedKeyword);

                                         return new FilterRowInfo
                                         {
                                             Row = row,
                                             OriginalIndex = index,
                                             MatchText = matchText,
                                             MatchBucket = matchInfo.Bucket,
                                             MatchIndex = matchInfo.MatchIndex,
                                             TextLength = matchText?.Length ?? int.MaxValue,
                                             TieBreakerKey = BuildTieBreakerKey(row, tieBreakerColumnList)
                                         };
                                     })
                                    .Where(x => x.MatchBucket != AutoCompleteMatchBucket.NoMatch)
                                    .OrderBy(x => (int)x.MatchBucket)
                                    .ThenBy(x => x.MatchIndex)
                                    .ThenBy(x => x.TextLength)
                                    .ThenBy(x => x.MatchText, StringComparer.OrdinalIgnoreCase)
                                    .ThenBy(x => x.TieBreakerKey, StringComparer.OrdinalIgnoreCase)
                                    .ThenBy(x => x.OriginalIndex)
                                    .ToList();

            if (matchedRows.Count == 0)
            {
                return fallbackToAllWhenNoMatch ? source.Copy() : source.Clone();
            }

            var result = source.Clone();

            foreach (var item in matchedRows)
            {
                result.ImportRow(item.Row);
            }

            return result;
        }

        private static string ResolveSpaceMatchColumnName(DataTable source)
        {
            if (source == null)
            {
                return string.Empty;
            }

            if (source.Columns.Contains("SchemaName"))
            {
                return "SchemaName";
            }

            if (source.Columns.Contains("ColumnName"))
            {
                return "ColumnName";
            }

            return string.Empty;
        }

        private static (AutoCompleteMatchBucket Bucket, int MatchIndex) GetMatchInfo(string text, string keyword)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            {
                return (AutoCompleteMatchBucket.NoMatch, int.MaxValue);
            }

            if (string.Equals(text, keyword, StringComparison.OrdinalIgnoreCase))
            {
                return (AutoCompleteMatchBucket.Exact, 0);
            }

            if (text.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return (AutoCompleteMatchBucket.StartsWith, 0);
            }

            var containsIndex = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

            if (containsIndex < 0)
            {
                return (AutoCompleteMatchBucket.NoMatch, int.MaxValue);
            }

            //20260424 keyword 長度 >= 2 才啟用 BoundaryContains
            var canUseBoundaryContains = keyword.Length >= 2;

            if (canUseBoundaryContains && IsBoundaryMatch(text, containsIndex))
            {
                return (AutoCompleteMatchBucket.BoundaryContains, containsIndex);
            }

            return (AutoCompleteMatchBucket.Contains, containsIndex);
        }

        private static bool IsBoundaryMatch(string text, int matchIndex)
        {
            if (string.IsNullOrEmpty(text) || matchIndex <= 0 || matchIndex >= text.Length)
            {
                return false;
            }

            var previousChar = text[matchIndex - 1];
            var currentChar = text[matchIndex];

            //snake_case / schema.name / 非識別字元邊界
            if (!char.IsLetterOrDigit(previousChar) || previousChar == '_')
            {
                return true;
            }

            //CamelCase / PascalCase 邊界
            if (char.IsLetter(previousChar) && char.IsLetter(currentChar) && char.IsLower(previousChar) && char.IsUpper(currentChar))
            {
                return true;
            }

            return false;
        }

        private static string BuildTieBreakerKey(DataRow row, IReadOnlyList<string> columns)
        {
            if (row == null || columns == null || columns.Count == 0)
            {
                return string.Empty;
            }

            var values = new string[columns.Count];

            for (var i = 0; i < columns.Count; i++)
            {
                var columnName = columns[i];

                values[i] = row.Table.Columns.Contains(columnName) ? row.GetSafeString(columnName) : string.Empty;
            }

            return string.Join("\u001F", values);
        }
    }
}
