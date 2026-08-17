using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Data.Formatters;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private enum QueryResultCopyMode
        {
            Copy,
            CopyAsQueryCondition,
            CopyWithColumnNames,
            CopyColumnNames
        }

        private sealed class QueryResultGridSelectedRow
        {
            public int RowIndex { get; set; }
            public List<C1DataColumn> Columns { get; set; } = new List<C1DataColumn>();
            public bool IsFirstSelectedRow { get; set; }
        }

        private sealed class QueryResultCopyOptions
        {
            public QueryResultCopyMode Mode { get; set; }
            public string FieldSeparator { get; set; } = string.Empty;
            public string QuotationMarks { get; set; } = string.Empty;
            public int SelectedColumnCount { get; set; }
        }

        private sealed class QueryResultCopyContent
        {
            public string ColumnNames { get; set; } = string.Empty;
            public string ColumnTypes { get; set; } = string.Empty;
            public string Data { get; set; } = string.Empty;
            public bool IsActiveCell { get; set; }
        }

        private sealed class QueryResultGridSummary
        {
            public long Sum { get; set; }
            public int NonEmptyCellCount { get; set; }
            public int NumericCellCount { get; set; }
        }

        private void ArrangeData(QueryResultCopyMode copyMode)
        {
            var c1Grid = GetWhichGrid();
            var copyOptions = CreateQueryResultCopyOptions(c1Grid, copyMode);
            var copyContent = BuildQueryResultCopyContent(c1Grid, copyOptions);
            var clipboardText = BuildQueryResultClipboardText(copyContent, copyOptions);
            var languageText = LocalizationHelper.GetLanguageString("Data has been copied to the clipboard!", "form", GetType().Name, "msg", "CopyOK", "Text");

            TextHelper.CopyTextToClipboard(clipboardText, "ArrangeData(01)");
            SetFormStatusBarInfo(languageText, Color.Blue);
        }

        private QueryResultCopyOptions CreateQueryResultCopyOptions(C1TrueDBGrid c1Grid, QueryResultCopyMode copyMode)
        {
            var isCopyAsQueryCondition = copyMode == QueryResultCopyMode.CopyAsQueryCondition;

            var resultCopyQuotationMarks = TextHelper.GetSafeString(mnuResultCopyQuotingWith.Tag);
            var quotationMarks = string.Equals(resultCopyQuotationMarks, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : resultCopyQuotationMarks;

            var fieldSeparator = TextHelper.GetSafeString(mnuResultCopyFieldSeparator.Tag);

            if (isCopyAsQueryCondition)
            {
                quotationMarks = "'";
                fieldSeparator = ",";
            }

            return new QueryResultCopyOptions
            {
                Mode = copyMode,
                FieldSeparator = fieldSeparator,
                QuotationMarks = quotationMarks,
                SelectedColumnCount = c1Grid.SelectedCols.Count
            };
        }

        private QueryResultCopyContent BuildQueryResultCopyContent(C1TrueDBGrid c1Grid, QueryResultCopyOptions options)
        {
            var selectedRows = GetQueryResultGridSelectedRows(c1Grid);

            if (selectedRows.Count == 0)
            {
                return BuildActiveCellCopyContent(c1Grid, options);
            }

            var columnNames = new StringBuilder();
            var columnTypes = new StringBuilder();
            var data = new StringBuilder();

            foreach (var selectedRow in selectedRows)
            {
                var rowData = new StringBuilder();

                foreach (var column in selectedRow.Columns)
                {
                    if (!_columnInfoCollector.TryGet(column.DataField, out var columnInfo))
                    {
                        continue;
                    }

                    if (selectedRow.IsFirstSelectedRow)
                    {
                        AppendSeparated(columnNames, column.DataField, options.FieldSeparator);
                        AppendSeparated(columnTypes, columnInfo.BaseDataType, options.FieldSeparator);
                    }

                    var cellText = NormalizeCopiedCellText(column.CellText(selectedRow.RowIndex));
                    var copiedValue = FormatCellValueForCopy(cellText, columnInfo, options);

                    AppendSeparated(rowData, copiedValue, options.FieldSeparator);
                }

                if (rowData.Length > 0)
                {
                    data.Append(rowData).AppendLine();
                }
            }

            return new QueryResultCopyContent
            {
                ColumnNames = columnNames.ToString(),
                ColumnTypes = columnTypes.ToString(),
                Data = data.ToString(),
                IsActiveCell = false
            };
        }

        private QueryResultCopyContent BuildActiveCellCopyContent(C1TrueDBGrid c1Grid, QueryResultCopyOptions options)
        {
            var columnIndex = 0;

            foreach (C1DataColumn column in c1Grid.Columns)
            {
                if (columnIndex != c1Grid.Col)
                {
                    columnIndex++;
                    continue;
                }

                var cellText = NormalizeCopiedCellText(column.CellText(c1Grid.Row));
                var copiedValue = cellText;

                if (_columnInfoCollector != null && _columnInfoCollector.TryGet(column.DataField, out var columnInfo))
                {
                    copiedValue = FormatCellValueForCopy(cellText, columnInfo, options);
                }

                return new QueryResultCopyContent
                {
                    ColumnNames = column.DataField,
                    ColumnTypes = string.Empty,
                    Data = copiedValue,
                    IsActiveCell = true
                };
            }

            return new QueryResultCopyContent
            {
                IsActiveCell = true
            };
        }

        private static string NormalizeCopiedCellText(string cellText)
        {
            return string.Equals(cellText, MyLibrary.GridNullShowAs, StringComparison.Ordinal) ? "null" : cellText;
        }

        private static string FormatCellValueForCopy(string cellText, dynamic columnInfo, QueryResultCopyOptions options)
        {
            switch (columnInfo.CategoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                case CategoryDataTypeKind.DateTime:
                    {
                        return $"{options.QuotationMarks}{cellText}{options.QuotationMarks}";
                    }
                default:
                    {
                        if (columnInfo.SpecialDataTypeKind == SpecialDataTypeKind.NString)
                        {
                            var nStringPrefix = options.Mode == QueryResultCopyMode.CopyAsQueryCondition ? "N" : string.Empty;

                            return $"{nStringPrefix}{options.QuotationMarks}{cellText}{options.QuotationMarks}";
                        }

                        return cellText;
                    }
            }
        }

        private string BuildQueryResultClipboardText(QueryResultCopyContent content, QueryResultCopyOptions options)
        {
            switch (options.Mode)
            {
                case QueryResultCopyMode.Copy:
                    {
                        return content.Data.TrimEnd('\r', '\n');
                    }
                case QueryResultCopyMode.CopyAsQueryCondition:
                    {
                        return BuildQueryConditionText(content, options);
                    }
                case QueryResultCopyMode.CopyWithColumnNames:
                    {
                        return BuildCopyWithColumnNamesText(content);
                    }
                case QueryResultCopyMode.CopyColumnNames:
                    {
                        return BuildCopyColumnNamesText(content);
                    }
                default:
                    {
                        return content.Data.TrimEnd('\r', '\n');
                    }
            }
        }

        private static string BuildCopyWithColumnNamesText(QueryResultCopyContent content)
        {
            var columnTypes = string.IsNullOrEmpty(content.ColumnTypes) ? string.Empty : $"\r\n{content.ColumnTypes}";

            return $"{content.ColumnNames}{columnTypes}\r\n{content.Data}";
        }

        private static string BuildCopyColumnNamesText(QueryResultCopyContent content)
        {
            var columnTypes = string.IsNullOrEmpty(content.ColumnTypes) ? string.Empty : $"\r\n{content.ColumnTypes}";

            return $"{content.ColumnNames}{columnTypes}";
        }

        private static string BuildQueryConditionText(QueryResultCopyContent content, QueryResultCopyOptions options)
        {
            var data = content.Data.TrimEnd('\r', '\n');
            var distinct = BuildDistinctLines(data);
            var hasMultipleColumns = content.ColumnNames.IndexOf(options.FieldSeparator, StringComparison.Ordinal) >= 0;
            var shouldUseTupleFormat = !content.IsActiveCell && (options.SelectedColumnCount > 1 || options.SelectedColumnCount == 0) && hasMultipleColumns;

            if (shouldUseTupleFormat)
            {
                return $"(({distinct.Replace("\r\n", "),(")}))";
            }

            return $"({distinct.Replace("\r\n", ",")})";
        }

        private static string BuildDistinctLines(string text)
        {
            var parts = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var distinctSet = new HashSet<string>(StringComparer.Ordinal);
            var sbDistinct = new StringBuilder();

            foreach (var part in parts)
            {
                if (distinctSet.Add(part))
                {
                    sbDistinct.Append(part).AppendLine();
                }
            }

            return sbDistinct.ToString().TrimEnd('\r', '\n');
        }

        private List<QueryResultGridSelectedRow> GetQueryResultGridSelectedRows(C1TrueDBGrid c1Grid)
        {
            var result = new List<QueryResultGridSelectedRow>();
            var isWholeColumnSelection = c1Grid.SelectedRows.Count == 0 && c1Grid.SelectedCols.Count > 0;

            if (isWholeColumnSelection)
            {
                var rowCount = c1Grid.Splits[_splitsIndex].Rows.Count;
                var selectedColumns = ToColumnList(c1Grid.SelectedCols);

                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    result.Add(new QueryResultGridSelectedRow
                    {
                        RowIndex = rowIndex,
                        Columns = selectedColumns,
                        IsFirstSelectedRow = rowIndex == 0
                    });
                }

                return result;
            }

            var isFirstSelectedRow = true;

            foreach (int selectedRowIndex in c1Grid.SelectedRows)
            {
                var dataRowIndex = c1Grid.Splits[_splitsIndex].Rows[selectedRowIndex].DataRowIndex;
                var selectedColumns = c1Grid.SelectedCols.Count == 0 ? ToColumnList(c1Grid.Columns) : ToColumnList(c1Grid.SelectedCols);

                result.Add(new QueryResultGridSelectedRow
                {
                    RowIndex = dataRowIndex,
                    Columns = selectedColumns,
                    IsFirstSelectedRow = isFirstSelectedRow
                });

                isFirstSelectedRow = false;
            }

            return result;
        }

        private static List<C1DataColumn> ToColumnList(IEnumerable columns)
        {
            var result = new List<C1DataColumn>();

            foreach (C1DataColumn column in columns)
            {
                result.Add(column);
            }

            return result;
        }

        private static void AppendSeparated(StringBuilder builder, string value, string separator)
        {
            if (builder.Length > 0)
            {
                builder.Append(separator);
            }

            builder.Append(value);
        }

        private void CalculateCells()
        {
            var c1Grid = GetWhichGrid();

            if (!c1Grid.HasDataTableRows())
            {
                return;
            }

            var summary = CalculateQueryResultGridSummary(c1Grid);

            ApplyQueryResultGridSummary(summary);
        }

        private QueryResultGridSummary CalculateQueryResultGridSummary(C1TrueDBGrid c1Grid)
        {
            var summary = new QueryResultGridSummary();
            var selectedRows = GetQueryResultGridSelectedRows(c1Grid);

            foreach (var selectedRow in selectedRows)
            {
                foreach (var column in selectedRow.Columns)
                {
                    var cellText = column.CellText(selectedRow.RowIndex);

                    if (!string.IsNullOrEmpty(cellText))
                    {
                        summary.NonEmptyCellCount++;
                    }

                    if (long.TryParse(cellText, out var value))
                    {
                        summary.Sum += value;
                        summary.NumericCellCount++;
                    }
                }
            }

            return summary;
        }

        private void ApplyQueryResultGridSummary(QueryResultGridSummary summary)
        {
            var showCount = summary.NonEmptyCellCount > 0;
            var showNumericSummary = summary.NumericCellCount > 0;

            lblSummaryValue.Text = DataSizeFormatter.FormatInt(summary.Sum);
            lblCountValue.Text = $"{summary.NonEmptyCellCount}";
            lblAverageValue.Text = CalculateAverageText(summary);

            lblAverage.Visible = showNumericSummary;
            lblAverageValue.Visible = showNumericSummary;
            lblCount.Visible = showCount;
            lblCountValue.Visible = showCount;
            lblSummary.Visible = showNumericSummary;
            lblSummaryValue.Visible = showNumericSummary;
            lblSeparator1.Visible = showNumericSummary;
            lblSeparator2.Visible = showNumericSummary;
            lblSeparator3.Visible = showNumericSummary || showCount;
        }

        private static string CalculateAverageText(QueryResultGridSummary summary)
        {
            if (summary.NumericCellCount == 0)
            {
                return "0";
            }

            var averageValue = (decimal)summary.Sum / summary.NumericCellCount;

            return DataSizeFormatter.FormatDecimal(averageValue);
        }

        private void c1TrueDBGrid1_KeyUp(object sender, KeyEventArgs e)
        {
            if (!ShouldRefreshQueryResultGridSummary(e))
            {
                return;
            }

            CalculateCells();
        }

        private static bool ShouldRefreshQueryResultGridSummary(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Tab:
                case Keys.Enter:
                case Keys.Space:
                    {
                        return true;
                    }
                case Keys.A:
                    {
                        return e.Control;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}