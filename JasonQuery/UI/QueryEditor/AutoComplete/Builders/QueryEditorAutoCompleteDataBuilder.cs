using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Text;
using System;
using System.Data;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Builders
{
    internal sealed class QueryEditorAutoCompleteDataBuilder
    {
        public DataTable BuildPeriodAutoCompleteData(DataTable source, C1TrueDBGrid grid, string condition)
        {
            return BuildAutoCompleteData
            (
                source,
                grid,
                condition,
                "ColumnName",
                "[ColumnName] LIKE '*'",
                string.Empty,
                "Sort, ColumnName, DataType"
            );
        }

        public DataTable BuildSpaceAutoCompleteData(DataTable source, C1TrueDBGrid grid, string condition)
        {
            return BuildAutoCompleteData
            (
                source,
                grid,
                condition,
                "ColumnName",
                "[ColumnName] LIKE '*'",
                string.Empty,
                "Sort, ColumnName, DataType"
            );
        }

        public DataTable BuildAutoCompleteData(DataTable source, C1TrueDBGrid grid, string condition, string keywordColumnName,
                                               string fallbackFilter, string preSortExpression, string finalSortExpression)
        {
            if (source == null)
            {
                return new DataTable();
            }

            var dataView = source.DefaultView;
            var normalizedCondition = NormalizeAutoCompleteFilterCondition(grid, condition);

            dataView.Sort = string.Empty;
            dataView.RowFilter = normalizedCondition;

            if (dataView.Count == 0)
            {
                dataView.RowFilter = fallbackFilter;
            }

            if (!string.IsNullOrEmpty(preSortExpression))
            {
                dataView.Sort = preSortExpression;
            }

            var dtSorted = dataView.ToTable();

            if (dtSorted.Rows.Count == 0)
            {
                return dtSorted;
            }

            ApplyStartsWithPrioritySort(dtSorted, normalizedCondition, keywordColumnName);

            var sortedView = dtSorted.DefaultView;

            sortedView.Sort = finalSortExpression;

            var dtResult = sortedView.ToTable();

            if (dtResult.Columns.Contains("Sort"))
            {
                dtResult.Columns.Remove("Sort");
            }

            return dtResult;
        }

        public string NormalizeAutoCompleteFilterCondition(C1TrueDBGrid grid, string condition0)
        {
            var condition = condition0 ?? string.Empty;

            if (string.IsNullOrEmpty(condition) || grid == null)
            {
                return condition;
            }

            var count = GetAutoCompleteDisplayColumnCount(grid);

            for (var i = 0; i < count; i++)
            {
                var caption = grid.Columns[i].Caption;
                var columnToken = string.Format("[{0}]", caption);

                if (!condition.Contains(columnToken))
                {
                    continue;
                }

                var columnIndex = condition.IndexOf(columnToken, StringComparison.Ordinal);

                if (columnIndex < 0)
                {
                    continue;
                }

                var quoteIndex = condition.IndexOf('\'', columnIndex);

                if (quoteIndex < 0)
                {
                    continue;
                }

                condition = condition.Insert(quoteIndex + 1, "*");
                condition = condition.Replace("**", "*");
            }

            condition = condition.Replace("LIKE '", "LIKE '*").Replace("**", "*");

            return condition;
        }

        public int GetAutoCompleteDisplayColumnCount(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return 0;
            }

            if (grid.Splits != null && grid.Splits.Count > 0 && grid.Splits[0].DisplayColumns != null)
            {
                return grid.Splits[0].DisplayColumns.Count;
            }

            return grid.Columns.Count;
        }

        public void ApplyStartsWithPrioritySort(DataTable dt, string condition, string keywordColumnName)
        {
            if (dt == null)
            {
                return;
            }

            if (!dt.Columns.Contains(keywordColumnName))
            {
                return;
            }

            if (!dt.Columns.Contains("Sort"))
            {
                dt.Columns.Add("Sort", typeof(int));
            }

            var filterKeyword = ExtractAutoCompleteFilterKeyword(condition);
            var priority = -1000;

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var dr = dt.Rows[i];
                var text = dr.GetSafeString(keywordColumnName);

                if (!string.IsNullOrEmpty(filterKeyword) && text.StartsWith(filterKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    dr["Sort"] = priority;
                    priority++;
                }
                else
                {
                    dr["Sort"] = i;
                }
            }
        }

        public string ExtractAutoCompleteFilterKeyword(string condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return string.Empty;
            }

            return TextHelper.GetStringBetween(condition, "'", "'")
                             .Replace("*", string.Empty)
                             .Trim();
        }
    }
}
