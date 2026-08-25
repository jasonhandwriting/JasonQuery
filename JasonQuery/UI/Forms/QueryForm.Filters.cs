using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Text;
using System;
using System.Data;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void C1TrueDBGrid_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                ApplyGridFilter
                (
                    GetWhichGrid(),
                    e.Condition,
                    shouldClearGridHighlights: true,
                    useColumnTag: false
                );
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void C1GridSqlNavigator_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                ApplyGridFilter
                (
                    c1GridSqlNavigator,
                    e.Condition,
                    shouldClearGridHighlights: false,
                    useColumnTag: true
                );
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void ApplyGridFilter(C1TrueDBGrid grid, string condition, bool shouldClearGridHighlights, bool useColumnTag)
        {
            if (grid?.DataSource == null)
            {
                return;
            }

            var dataView = ((DataTable)grid.DataSource).DefaultView;

            if (dataView.RowFilter == condition)
            {
                return;
            }

            if (shouldClearGridHighlights)
            {
                ClearGridHighlightsBeforeFilter();
            }

            dataView.RowFilter = BuildWildcardFilterCondition(grid, condition, useColumnTag);
        }

        private void ClearGridHighlightsBeforeFilter()
        {
            if (TextHelper.GetSafeString(btnHighlightAllGrid.Tag) != "1")
            {
                return;
            }

            //20230824 如果使用者有 Highlight 關鍵字，要先清空，否則會有殘留問題
            btnHighlightAllGrid.Tag = "0";
            btnClearHighlightsGrid.PerformClick();
        }

        private string BuildWildcardFilterCondition(C1TrueDBGrid grid, string condition, bool useColumnTag)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return condition;
            }

            var normalizedCondition = condition;
            var count = grid.Splits[_splitsIndex].DisplayColumns.Count;

            for (var columnIndex = 0; columnIndex < count; columnIndex++)
            {
                var token = useColumnTag ? TextHelper.GetSafeString(grid.Columns[columnIndex].Tag) : grid.Columns[columnIndex].Caption;

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                var columnToken = $"[{token}]";

                if (!normalizedCondition.Contains(columnToken))
                {
                    continue;
                }

                var tokenIndex = normalizedCondition.IndexOf(columnToken, StringComparison.Ordinal);

                if (tokenIndex < 0)
                {
                    continue;
                }

                var quoteIndex = normalizedCondition.IndexOf('\'', tokenIndex);

                if (quoteIndex < 0)
                {
                    continue;
                }

                normalizedCondition = normalizedCondition.Insert(quoteIndex + 1, "*");
            }

            return normalizedCondition;
        }
    }
}
