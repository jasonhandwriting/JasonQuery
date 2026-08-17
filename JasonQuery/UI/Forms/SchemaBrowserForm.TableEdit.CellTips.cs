using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.Core.QueryEngine.Types;
using System;
using System.Data;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private const string ModifiedCellTipTemplate = @"<table><tr><td><b><font color='red' face='Microsoft JhengHei UI' size='3'>{TIPTITLE}</font></b></table>
                                                         <hr noshade size=1 style='margin:1' color=Darker>
                                                         <div style='margin:1 12'><parm>
                                                         <font color='black' face='Microsoft JhengHei UI' size='3'>{CURRENTTITLE}</font><br>
                                                         <b><font color='blue' face='Microsoft JhengHei UI' size='3'>{CURRENTVALUE}</font></b><br><br>
                                                         <font color='black' face='Microsoft JhengHei UI' size='3'>{ORIGINALTITLE}</font><br>
                                                         <b><font color='green' face='Microsoft JhengHei UI' size='3'>{ORIGINALVALUE}</font></b><br>
                                                         </parm></div>";

        private sealed class ModifiedCellTipValue
        {
            public string Text { get; set; } = string.Empty;

            public bool IsNull { get; set; }

            public int DisplayLength
            {
                get
                {
                    return IsNull ? 0 : Text.Length;
                }
            }
        }

        private void c1GridData_FetchCellTips(object sender, FetchCellTipsEventArgs e)
        {
            //保留 C1TrueDBGrid 的 FetchCellTips 作為滑鼠停留事件來源，實際顯示仍交由 C1SuperTooltip，沿用 HTML 格式與既有顯示行為
            e.CellTip = string.Empty;

            if (!(sender is C1TrueDBGrid grid))
            {
                return;
            }

            if (!TryGetCellTipColumnContext(grid, e, out var columnIndex, out var columnName))
            {
                ClearModifiedCellTip(grid);
                return;
            }

            var cellPosition = new Point(columnIndex, e.Row);

            if (!TryBuildModifiedCellTip(e.Row, columnIndex, columnName, out var tip))
            {
                ClearModifiedCellTip(grid);
                return;
            }

            ApplyModifiedCellTip(grid, cellPosition, tip);
        }

        private void UpdateModifiedCellTipFromMousePosition(C1TrueDBGrid grid, int mouseX, int mouseY)
        {
            if (grid == null)
            {
                return;
            }

            var rowIndex = grid.RowContaining(mouseY);
            var columnIndex = grid.ColContaining(mouseX);

            if (!TryGetCellTipColumnName(grid, columnIndex, out var columnName) || !IsVisibleDataGridDataRow(rowIndex))
            {
                ClearModifiedCellTip(grid);
                return;
            }

            var cellPosition = new Point(columnIndex, rowIndex);

            //同一個 Cell 且 Tooltip 內容已建立時，不在每一次 MouseMove 重建文字或重設 Tooltip
            if (_currentCellPosition.Equals(cellPosition)
                && !string.IsNullOrEmpty(_currentModifiedCellTipText))
            {
                return;
            }

            if (!TryBuildModifiedCellTip(rowIndex, columnIndex, columnName, out var tip))
            {
                ClearModifiedCellTip(grid);
                return;
            }

            ApplyModifiedCellTip(grid, cellPosition, tip);
        }

        private static bool TryGetCellTipColumnName(C1TrueDBGrid grid, int columnIndex, out string columnName)
        {
            columnName = string.Empty;

            if (grid == null || columnIndex < 0 || columnIndex >= grid.Columns.Count)
            {
                return false;
            }

            columnName = grid.Columns[columnIndex].DataField ?? string.Empty;

            return !string.IsNullOrWhiteSpace(columnName);
        }

        private bool TryGetCellTipColumnContext(C1TrueDBGrid grid, FetchCellTipsEventArgs e, out int columnIndex, out string columnName)
        {
            columnIndex = -1;
            columnName = string.Empty;

            if (grid == null || e == null)
            {
                return false;
            }

            var dataColumn = e.Column?.DataColumn;

            if (dataColumn != null)
            {
                columnName = dataColumn.DataField ?? string.Empty;

                if (TryFindGridDataColumnIndex(grid, dataColumn, columnName, out columnIndex))
                {
                    return !string.IsNullOrWhiteSpace(columnName);
                }
            }

            if (e.SplitIndex >= 0 && e.SplitIndex < grid.Splits.Count)
            {
                var displayColumns = grid.Splits[e.SplitIndex].DisplayColumns;

                if (e.ColIndex >= 0 && e.ColIndex < displayColumns.Count)
                {
                    dataColumn = displayColumns[e.ColIndex].DataColumn;

                    if (dataColumn != null)
                    {
                        columnName = dataColumn.DataField ?? string.Empty;

                        if (TryFindGridDataColumnIndex(grid, dataColumn, columnName, out columnIndex))
                        {
                            return !string.IsNullOrWhiteSpace(columnName);
                        }
                    }
                }
            }

            if (e.ColIndex < 0 || e.ColIndex >= grid.Columns.Count)
            {
                return false;
            }

            columnIndex = e.ColIndex;
            columnName = grid.Columns[columnIndex].DataField ?? string.Empty;

            return !string.IsNullOrWhiteSpace(columnName);
        }

        private bool TryFindGridDataColumnIndex(C1TrueDBGrid grid, C1DataColumn dataColumn, string columnName, out int columnIndex)
        {
            columnIndex = -1;

            if (grid == null || dataColumn == null)
            {
                return false;
            }

            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var candidateColumn = grid.Columns[index];

                if (ReferenceEquals(candidateColumn, dataColumn)
                    || string.Equals(candidateColumn.DataField, columnName, StringComparison.Ordinal))
                {
                    columnIndex = index;
                    return true;
                }
            }

            return false;
        }

        private void ApplyModifiedCellTip(C1TrueDBGrid grid, Point cellPosition, string tip)
        {
            if (grid == null)
            {
                return;
            }

            if (_currentCellPosition.Equals(cellPosition) && string.Equals(_currentModifiedCellTipText, tip, StringComparison.Ordinal))
            {
                return;
            }

            c1SuperTooltip1.Hide(grid);

            _currentCellPosition = cellPosition;
            _currentModifiedCellTipText = tip;

            c1SuperTooltip1.SetToolTip(grid, tip);
        }

        private void ClearModifiedCellTip(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            if (_currentCellPosition.X < 0 && _currentCellPosition.Y < 0 && string.IsNullOrEmpty(_currentModifiedCellTipText))
            {
                return;
            }

            c1SuperTooltip1.Hide(grid);
            c1SuperTooltip1.SetToolTip(grid, string.Empty);

            _currentCellPosition = new Point(-1, -1);
            _currentModifiedCellTipText = string.Empty;
        }

        private bool TryBuildModifiedCellTip(int rowIndex, int columnIndex, string columnName, out string tip)
        {
            tip = string.Empty;

            if (!IsValidModifiedCellTipPosition(rowIndex, columnIndex, columnName))
            {
                return false;
            }

            var currentRow = GetDataRowFromGridDisplayRow(c1GridData, rowIndex);

            if (!IsUsableTableDataRow(currentRow))
            {
                return false;
            }

            var directBinaryChange = GetDirectBinaryChange(currentRow, columnName);

            if (directBinaryChange != null)
            {
                tip = BuildDirectBinaryCellTip(directBinaryChange);
                return !string.IsNullOrEmpty(tip);
            }

            var cellPosition = new Point(columnIndex, rowIndex);

            if (!_modifiedCells.Contains(cellPosition))
            {
                return false;
            }

            if (IsDeletedDataRow(currentRow))
            {
                return false;
            }

            var rowId = currentRow.GetSafeString(MyGlobal.Row_Id_PK_JQ);

            if (!TryFindOriginalDataRow(rowId, out var originalRow))
            {
                return false;
            }

            var currentValue = CreateModifiedCellTipValue(currentRow, columnName);
            var originalValue = CreateModifiedCellTipValue(originalRow, columnName);

            if (!IsModifiedCellValueDifferent(currentValue, originalValue))
            {
                return false;
            }

            tip = BuildModifiedCellTip(currentValue, originalValue);
            return !string.IsNullOrEmpty(tip);
        }

        private bool IsValidModifiedCellTipPosition(int rowIndex, int columnIndex, string columnName)
        {
            if (rowIndex < 0 || columnIndex < 0 || string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return false;
            }

            if (_dtOriginalTableData == null || _dtOriginalTableData.Rows.Count == 0)
            {
                return false;
            }

            if (!HasVisibleDataGridRows())
            {
                return false;
            }

            if (!IsVisibleDataGridDataRow(rowIndex))
            {
                return false;
            }

            if (columnIndex >= c1GridData.Columns.Count)
            {
                return false;
            }

            if (!_dtTableData.Columns.Contains(columnName) || !_dtOriginalTableData.Columns.Contains(columnName))
            {
                return false;
            }

            if ((_modifiedCells == null || _modifiedCells.Count == 0) && !HasDirectBinaryChanges())
            {
                return false;
            }

            return true;
        }

        private bool IsDeletedDataRow(DataRow currentRow)
        {
            if (currentRow == null)
            {
                return false;
            }

            var identifyValue = currentRow.GetSafeString(_identifyColumnName);

            return string.Equals(identifyValue, "DEL", StringComparison.OrdinalIgnoreCase);
        }

        private bool TryFindOriginalDataRow(string rowId, out DataRow originalRow)
        {
            originalRow = null;

            if (string.IsNullOrEmpty(rowId))
            {
                return false;
            }

            if (_dtOriginalTableData == null || _dtOriginalTableData.Rows.Count == 0)
            {
                return false;
            }

            foreach (DataRow row in _dtOriginalTableData.Rows)
            {
                //Oracle ROWID 區分大小寫，所以必須使用 Ordinal
                if (StringComparer.Ordinal.Equals(row.GetSafeString(MyGlobal.Row_Id_PK_JQ), rowId))
                {
                    originalRow = row;
                    return true;
                }
            }

            return false;
        }

        private ModifiedCellTipValue CreateModifiedCellTipValue(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || string.IsNullOrWhiteSpace(columnName))
            {
                return null;
            }

            if (!row.Table.Columns.Contains(columnName))
            {
                return null;
            }

            var rawValue = row[columnName];
            var text = GetModifiedCellTipText(rawValue);
            var isNull = IsCellNullForTip(rawValue, text);

            return new ModifiedCellTipValue
            {
                Text = text,
                IsNull = isNull
            };
        }

        private string GetModifiedCellTipText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is LargeTextDataType largeText)
            {
                return largeText.LoadContent() ?? string.Empty;
            }

            return Convert.ToString(value) ?? string.Empty;
        }

        private bool IsCellNullForTip(object value, string text)
        {
            if (value == null || value == DBNull.Value)
            {
                return true;
            }

            if (IsGridNullShowAsNone())
            {
                return false;
            }

            return IsGridNullDisplayText(text);
        }

        private bool IsGridNullShowAsNone()
        {
            return string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsGridNullDisplayText(string value)
        {
            if (string.IsNullOrEmpty(MyLibrary.GridNullShowAs))
            {
                return false;
            }

            return string.Equals(value, MyLibrary.GridNullShowAs, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsModifiedCellValueDifferent(
            ModifiedCellTipValue currentValue,
            ModifiedCellTipValue originalValue)
        {
            if (currentValue == null || originalValue == null)
            {
                return false;
            }

            if (currentValue.IsNull != originalValue.IsNull)
            {
                return true;
            }

            return !string.Equals(currentValue.Text, originalValue.Text, StringComparison.Ordinal);
        }

        private string BuildModifiedCellTip(ModifiedCellTipValue currentValue, ModifiedCellTipValue originalValue)
        {
            if (currentValue == null || originalValue == null)
            {
                return string.Empty;
            }

            var modifiedTipTitle = LocalizationHelper.GetLanguageString("The cell has been modified.", "form", GetType().Name, "msg", "ModifiedTipTitle", "Text");
            var modifiedCurrentTitle = LocalizationHelper.GetLanguageString("Current Value:", "form", GetType().Name, "msg", "ModifiedCurrentTitle", "Text");
            var modifiedOriginalTitle = LocalizationHelper.GetLanguageString("Original Value:", "form", GetType().Name, "msg", "ModifiedOriginalTitle", "Text");
            var currentDisplayText = GetModifiedCellTipDisplayText(currentValue);
            var originalDisplayText = GetModifiedCellTipDisplayText(originalValue);
            var tip = ModifiedCellTipTemplate;

            tip = tip.Replace("{TIPTITLE}", EncodeTooltipHtml(modifiedTipTitle));
            tip = tip.Replace("{CURRENTTITLE}", EncodeTooltipHtml(modifiedCurrentTitle.Replace("{LEN}", currentValue.DisplayLength.ToString())));
            tip = tip.Replace("{CURRENTVALUE}", currentDisplayText);
            tip = tip.Replace("{ORIGINALTITLE}", EncodeTooltipHtml(modifiedOriginalTitle.Replace("{LEN}", originalValue.DisplayLength.ToString())));
            tip = tip.Replace("{ORIGINALVALUE}", originalDisplayText);

            return tip;
        }

        private string GetModifiedCellTipDisplayText(ModifiedCellTipValue value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (!value.IsNull)
            {
                return EncodeTooltipHtml(value.Text);
            }

            if (IsGridNullShowAsNone())
            {
                return string.Empty;
            }

            return EncodeTooltipHtml(MyLibrary.GridNullShowAs);
        }

        private string BuildDirectBinaryCellTip(DirectBinaryChange change)
        {
            if (change == null)
            {
                return string.Empty;
            }

            var title = LocalizationHelper.GetLanguageString("The binary value has already been updated in the current transaction.", "form", GetType().Name, "msg", "DirectBinaryUpdatedTipTitle", "Text");
            var fileTitle = LocalizationHelper.GetLanguageString("File", "Global", "Global", "msg", "File", "Text");
            var sizeTitle = LocalizationHelper.GetLanguageString("Size", "Global", "Global", "msg", "Size", "Text");
            var locatorTitle = LocalizationHelper.GetLanguageString("Locator", "form", GetType().Name, "msg", "DirectBinaryLocatorTitle", "Text");
            var size = change.Value == null ? 0 : change.Value.Length;

            return $"<b>{EncodeTooltipHtml(title)}</b><br><br>" +
                   $"{EncodeTooltipHtml(fileTitle)}: {EncodeTooltipHtml(change.SourceFileName)}<br>" +
                   $"{EncodeTooltipHtml(sizeTitle)}: {size:N0} bytes<br>" +
                   $"{EncodeTooltipHtml(locatorTitle)}: {EncodeTooltipHtml(change.LocatorDescription)}";
        }

        private static string EncodeTooltipHtml(string value)
        {
            value = value ?? string.Empty;

            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("\r\n", "<br>")
                        .Replace("\n", "<br>")
                        .Replace("\r", "<br>");
        }
    }
}