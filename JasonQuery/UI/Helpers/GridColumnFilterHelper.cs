using C1.Win.C1TrueDBGrid;
using JasonLibrary.UI.Controls;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Helpers
{
    public static class GridColumnFilterHelper
    {
        private const string DefaultColumnName = "ColumnName";

        public static void ApplyColumnNameFilterOnEnter(C1TrueDBGrid grid, TextBox textBox, KeyEventArgs e)
        {
            if (e == null || e.KeyData != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            ApplyColumnNameFilter(grid, textBox);
            grid?.Focus();
        }

        public static void ApplyColumnNameFilter(C1TrueDBGrid grid, TextBox textBox)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            var dtColumn = grid.GetDataTableSourceOrNull();

            if (dtColumn == null || !dtColumn.Columns.Contains(DefaultColumnName))
            {
                return;
            }

            var filterText = NormalizeFilterText(textBox.Text);

            textBox.Text = filterText;
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;

            dtColumn.DefaultView.RowFilter = BuildColumnNameRowFilter(filterText);
        }

        public static void SelectAllText(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.TextLength;
        }

        private static string NormalizeFilterText(string text)
        {
            var filterText = (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                return "*";
            }

            filterText = filterText.Replace("%", "*");

            while (filterText.Contains("**"))
            {
                filterText = filterText.Replace("**", "*");
            }

            if (!filterText.EndsWith("*", StringComparison.Ordinal))
            {
                filterText += "*";
            }

            return filterText;
        }

        private static string BuildColumnNameRowFilter(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText) || filterText == "*")
            {
                return string.Empty;
            }

            var columnFilter = filterText.Replace("*", "%");
            var percentCount = columnFilter.Count(c => c == '%');

            if (columnFilter.StartsWith("%", StringComparison.Ordinal) && percentCount >= 3)
            {
                var parts = SplitFilterParts(columnFilter);

                return BuildContainsAllFilter(parts);
            }

            if (!columnFilter.StartsWith("%", StringComparison.Ordinal) && percentCount >= 2)
            {
                var parts = SplitFilterParts(columnFilter);

                return BuildPrefixAndContainsFilter(parts);
            }

            return BuildLikeFilter(columnFilter);
        }

        private static string[] SplitFilterParts(string columnFilter)
        {
            return columnFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string BuildContainsAllFilter(string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append(BuildLikeFilter($"%{parts[i]}%"));
            }

            return sb.ToString();
        }

        private static string BuildPrefixAndContainsFilter(string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" AND ");
                }

                var pattern = i == 0 ? $"{parts[i]}%" : $"%{parts[i]}%";

                sb.Append(BuildLikeFilter(pattern));
            }

            return sb.ToString();
        }

        private static string BuildLikeFilter(string pattern)
        {
            return $"[ColumnName] LIKE '{EscapeRowFilterValue(pattern)}'";
        }

        private static string EscapeRowFilterValue(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
