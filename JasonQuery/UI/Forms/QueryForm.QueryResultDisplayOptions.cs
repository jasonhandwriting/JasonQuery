using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void chkSize_Click(object sender, EventArgs e)
        {
            try
            {
                var c1Grid = GetQueryResultGridForDisplayOption();

                ApplyQueryResultDisplayOptionFocusState();

                if (!CanApplyQueryResultAutoSizeOnChecked(c1Grid))
                {
                    return;
                }

                GridHelper.ResizeGridColumnWidth(c1Grid);
                c1Grid.Refresh();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void chkShowColumnType_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var c1Grid = GetQueryResultGridForDisplayOption();

                ApplyQueryResultDisplayOptionFocusState();

                if (!CanApplyQueryResultColumnCaptionChange(c1Grid))
                {
                    return;
                }

                ApplyQueryResultColumnCaptions(c1Grid);
                ApplyQueryResultCaptionChangedState(c1Grid);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void chkShowColumnComments_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var c1Grid = GetQueryResultGridForDisplayOption();

                ApplyQueryResultDisplayOptionFocusState();

                if (!CanApplyQueryResultColumnCaptionChange(c1Grid))
                {
                    return;
                }

                ApplyQueryResultColumnCaptions(c1Grid);
                ApplyQueryResultCaptionChangedState(c1Grid);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnAutoSort_Click(object sender, EventArgs e)
        {
            C1TrueDBGrid c1Grid = null;

            try
            {
                c1Grid = GetQueryResultGridForDisplayOption();

                ApplyQueryResultDisplayOptionFocusState();

                if (!CanApplyQueryResultColumnNameAutoSort(c1Grid, out var dtData))
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;
                c1Grid.Cursor = Cursors.WaitCursor;

                var currentRow = c1Grid.Row;
                var currentCol = c1Grid.Col;
                var sortAscending = _isNextQueryResultColumnNameSortAscending;

                var dtSortedData = CreateColumnNameSortedDataTable(dtData, sortAscending);

                c1Grid.DataSource = dtSortedData;
                c1Grid.Show();

                _isNextQueryResultColumnNameSortAscending = !sortAscending;

                ApplyQueryResultColumnCaptionsIfAvailable(c1Grid);
                ApplyQueryResultGridNullDisplayStyle(c1Grid);
                ApplyQueryResultCaptionChangedState(c1Grid);
                RestoreQueryResultGridPosition(c1Grid, currentRow, currentCol);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Default;

                if (c1Grid != null)
                {
                    c1Grid.Cursor = Cursors.Default;
                }
            }
        }

        private void chkRawDataMode_Click(object sender, EventArgs e)
        {
            ApplyQueryResultDisplayOptionFocusState();
        }

        private void chkRawDataMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var c1Grid = GetQueryResultGridForDisplayOption();

                ApplyQueryResultDisplayOptionFocusState();
                ApplyRawDataModeDependentControls(c1Grid);

                if (!CanApplyQueryResultColumnCaptionChange(c1Grid, allowRawDataMode: true))
                {
                    return;
                }

                ApplyQueryResultColumnCaptions(c1Grid);
                ApplyQueryResultCaptionChangedState(c1Grid, allowAutoResize: !chkRawDataMode.Checked);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private C1TrueDBGrid GetQueryResultGridForDisplayOption()
        {
            var c1Grid = GetWhichGrid();

            if (c1Grid != null)
            {
                return c1Grid;
            }

            c1DockingTab1.SelectedTab = tabDataGrid;

            return GetWhichGrid();
        }

        private void ApplyQueryResultDisplayOptionFocusState()
        {
            SetGridToolStripBackColor(true);

            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void ApplyRawDataModeDependentControls(C1TrueDBGrid c1Grid)
        {
            var isRawDataMode = chkRawDataMode.Checked;
            var hasRows = HasQueryResultRows(c1Grid);

            chkShowColumnType.Enabled = !isRawDataMode;
            chkShowColumnComments.Enabled = !isRawDataMode;

            //Raw Data Mode 不套用 Auto-fit column width
            chkSize.Enabled = hasRows && !isRawDataMode;
        }

        private bool HasQueryResultRows(C1TrueDBGrid c1Grid)
        {
            if (c1Grid == null)
            {
                return false;
            }

            if (c1Grid.Splits.Count <= _splitsIndex)
            {
                return false;
            }

            return c1Grid.Splits[_splitsIndex].Rows.Count > 0;
        }

        private bool CanApplyQueryResultColumnCaptionChange(C1TrueDBGrid c1Grid, bool allowRawDataMode = false)
        {
            if (!_isFormLoadFinished)
            {
                return false;
            }

            if (_isBusy || btnCancelQuery.Enabled)
            {
                return false;
            }

            if (c1Grid == null)
            {
                return false;
            }

            if (_columnInfoCollector == null)
            {
                return false;
            }

            if (!allowRawDataMode && chkRawDataMode.Checked)
            {
                return false;
            }

            return c1Grid.Columns.Count > 0;
        }

        private void ApplyQueryResultColumnCaptions(C1TrueDBGrid c1Grid)
        {
            var hasAnyColumnComment = false;

            foreach (C1DataColumn column in c1Grid.Columns)
            {
                var columnName = ResolveGridColumnName(column);

                if (string.IsNullOrEmpty(columnName))
                {
                    continue;
                }

                if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    continue;
                }

                var baseDataType = TextHelper.GetSafeString(columnInfo.BaseDataType);
                var columnComment = TextHelper.GetSafeString(columnInfo.ColumnComment);

                if (!string.IsNullOrWhiteSpace(columnComment))
                {
                    hasAnyColumnComment = true;
                }

                column.Caption = BuildQueryResultColumnCaption(columnName, baseDataType, columnComment);
            }

            _currentQueryResultHasColumnComments = hasAnyColumnComment;

            if (!chkRawDataMode.Checked && chkShowColumnComments.Checked && !hasAnyColumnComment)
            {
                SetEditorStatusBarInfo(_sqlWithoutCommentMessage, Color.DarkRed);
            }
        }

        private string ResolveGridColumnName(C1DataColumn column)
        {
            var columnName = TextHelper.GetSafeString(column.DataField);

            if (!string.IsNullOrEmpty(columnName) && _columnInfoCollector.TryGet(columnName, out _))
            {
                return columnName;
            }

            columnName = GetCaptionFirstLine(column.Caption);

            if (!string.IsNullOrEmpty(columnName) && _columnInfoCollector.TryGet(columnName, out _))
            {
                return columnName;
            }

            return columnName;
        }

        private string BuildQueryResultColumnCaption(string columnName, string baseDataType, string columnComment)
        {
            var captionLines = new List<string>
            {
                columnName
            };

            if (!chkRawDataMode.Checked && chkShowColumnType.Checked && !string.IsNullOrWhiteSpace(baseDataType))
            {
                captionLines.Add(baseDataType);
            }

            if (!chkRawDataMode.Checked && chkShowColumnComments.Checked && !string.IsNullOrWhiteSpace(columnComment))
            {
                foreach (var commentLine in GetColumnCommentCaptionLines(columnComment))
                {
                    captionLines.Add(commentLine);
                }
            }

            return string.Join("\r\n", captionLines);
        }

        private static IEnumerable<string> GetColumnCommentCaptionLines(string columnComment)
        {
            var lines = NormalizeCaptionLineBreaks(columnComment).Split('\n');

            foreach (var line in lines)
            {
                var value = line.TrimEnd('\r');

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                yield return value;
            }
        }

        private static string GetCaptionFirstLine(string caption)
        {
            var normalizedCaption = NormalizeCaptionLineBreaks(caption);
            var parts = normalizedCaption.Split('\n');

            return parts.Length == 0 ? string.Empty : parts[0].TrimEnd('\r');
        }

        private static string NormalizeCaptionLineBreaks(string caption)
        {
            return TextHelper.GetSafeString(caption)
                             .Replace("\r\n", "\n")
                             .Replace("\r", "\n");
        }

        private void ApplyQueryResultCaptionChangedState(C1TrueDBGrid c1Grid, bool allowAutoResize = true)
        {
            ApplyResultGridHeadingStyle(c1Grid);
            GridZoom(c1Grid);

            if (allowAutoResize && chkSize.Checked)
            {
                GridHelper.ResizeGridColumnWidth(c1Grid);
            }

            c1Grid.Refresh();
        }

        private bool CanApplyQueryResultAutoSizeOnChecked(C1TrueDBGrid c1Grid)
        {
            if (!chkSize.Checked)
            {
                return false;
            }

            if (!_isFormLoadFinished)
            {
                return false;
            }

            if (_isBusy || btnCancelQuery.Enabled)
            {
                return false;
            }

            if (c1Grid == null)
            {
                return false;
            }

            if (c1Grid.Splits.Count <= _splitsIndex)
            {
                return false;
            }

            if (c1Grid.Splits[_splitsIndex].Rows.Count <= 0)
            {
                return false;
            }

            return c1Grid.Columns.Count > 0;
        }

        private bool CanApplyQueryResultColumnNameAutoSort(C1TrueDBGrid c1Grid, out DataTable dtData)
        {
            dtData = null;

            if (!_isFormLoadFinished)
            {
                return false;
            }

            if (_isBusy || btnCancelQuery.Enabled)
            {
                return false;
            }

            if (c1Grid == null)
            {
                return false;
            }

            if (c1Grid.Splits.Count <= _splitsIndex)
            {
                return false;
            }

            if (c1Grid.Splits[_splitsIndex].Rows.Count <= 0)
            {
                return false;
            }

            dtData = c1Grid.GetDataTableSourceOrNull();

            if (dtData == null)
            {
                return false;
            }

            if (dtData.Rows.Count <= 0)
            {
                return false;
            }

            return dtData.Columns.Count > 0;
        }

        private static DataTable CreateColumnNameSortedDataTable(DataTable sourceData, bool sortAscending)
        {
            var sortedColumnNames = GetColumnNamesSortedByName(sourceData, sortAscending);

            return sourceData.DefaultView.ToTable(false, sortedColumnNames);
        }

        private static string[] GetColumnNamesSortedByName(DataTable sourceData, bool sortAscending)
        {
            var columns = sourceData.Columns.Cast<DataColumn>();

            if (sortAscending)
            {
                return columns.OrderBy(c => c.ColumnName, StringComparer.OrdinalIgnoreCase)
                              .ThenBy(c => c.ColumnName, StringComparer.Ordinal)
                              .Select(c => c.ColumnName)
                              .ToArray();
            }

            return columns.OrderByDescending(c => c.ColumnName, StringComparer.OrdinalIgnoreCase)
                          .ThenByDescending(c => c.ColumnName, StringComparer.Ordinal)
                          .Select(c => c.ColumnName)
                          .ToArray();
        }

        private void ApplyQueryResultColumnCaptionsIfAvailable(C1TrueDBGrid c1Grid)
        {
            if (_columnInfoCollector == null)
            {
                return;
            }

            if (chkRawDataMode.Checked)
            {
                return;
            }

            ApplyQueryResultColumnCaptions(c1Grid);
        }

        private void ApplyQueryResultGridNullDisplayStyle(C1TrueDBGrid c1Grid)
        {
            if (string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var colorNull = new Style
            {
                ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
            };

            for (var i = 0; i < c1Grid.Columns.Count; i++)
            {
                c1Grid.Splits[_splitsIndex].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
            }
        }

        private void RestoreQueryResultGridPosition(C1TrueDBGrid c1Grid, int row, int col)
        {
            if (c1Grid.Columns.Count > 0)
            {
                c1Grid.Col = Math.Max(0, Math.Min(col, c1Grid.Columns.Count - 1));
            }

            if (c1Grid.Splits.Count > _splitsIndex && c1Grid.Splits[_splitsIndex].Rows.Count > 0)
            {
                c1Grid.Row = Math.Max(0, Math.Min(row, c1Grid.Splits[_splitsIndex].Rows.Count - 1));
            }

            c1Grid.Select();
        }

        private bool ShouldShowColumnTypeInHeader()
        {
            return !chkRawDataMode.Checked && chkShowColumnType.Checked;
        }

        private bool ShouldShowColumnCommentInHeader()
        {
            return !chkRawDataMode.Checked
                   && chkShowColumnComments.Checked
                   && _currentQueryResultHasColumnComments;
        }

        private bool ShouldUseMultiLineQueryResultHeader()
        {
            return ShouldShowColumnTypeInHeader() || ShouldShowColumnCommentInHeader();
        }

        private int GetQueryResultHeaderLineCount()
        {
            var lineCount = 1;

            if (ShouldShowColumnTypeInHeader())
            {
                lineCount++;
            }

            if (ShouldShowColumnCommentInHeader())
            {
                lineCount++;
            }

            return lineCount;
        }

        private void btnHelp_RawDataMode_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Raw Data Mode significantly improves data display performance.\r\n\r\nWhen enabled, query results are displayed exactly as returned by the database, without any additional formatting or processing.\r\n\r\nAs a result, the following display-related settings will not be applied:\r\n1. Date formatting\r\n2. Null value style\r\n3. Column type display\r\n4. Column comment display\r\n5. Auto-fit column width\r\n\r\nNote:\r\nIf the query result contains large text or binary data, this mode may be temporarily disabled for the current query to prevent performance or stability issues.", "Global", "Global", "msg", "Help_RawDataMode", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
