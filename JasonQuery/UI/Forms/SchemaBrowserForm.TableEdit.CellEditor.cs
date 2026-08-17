using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.QueryEngine.Types;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private bool IsTableDataColumnEditable(ColumnInfo columnInfo)
        {
            if (columnInfo == null)
            {
                return false;
            }

            //20260713 LargeText 一律唯讀優先！即使特定資料庫型別目前被標記為允許 UPDATE，也不允許透過一般 CellEditorForm 或 Grid Editor 改寫大型文字內容
            if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
            {
                return false;
            }

            return columnInfo.IsUpdateValueSupported;
        }

        private bool IsReadOnlyLargeTextColumn(ColumnInfo columnInfo)
        {
            return columnInfo != null && columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText;
        }

        private string GetCellEditorOriginalText(C1TrueDBGrid c1Grid, int row, int col)
        {
            if (c1Grid == null || row < 0 || col < 0)
            {
                return string.Empty;
            }

            var value = c1Grid[row, col];

            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            if (value is LargeTextDataType largeText)
            {
                return largeText.LoadContent();
            }

            return value.ToString();
        }

        private DataRow GetDataRowFromGridDisplayRow(C1TrueDBGrid grid, int displayRowIndex)
        {
            if (grid == null || displayRowIndex < 0)
            {
                return null;
            }

            try
            {
                var dataRowView = grid.GetDataBoundItem(displayRowIndex) as DataRowView;

                if (dataRowView != null && IsUsableTableDataRow(dataRowView.Row))
                {
                    return dataRowView.Row;
                }
            }
            catch
            {
                //特殊狀態下，C1TrueDBGrid 可能無法取得 DataBoundItem，改用 fallback
            }

            if (_dtTableData == null || displayRowIndex >= _dtTableData.Rows.Count)
            {
                return null;
            }

            var fallbackRow = _dtTableData.Rows[displayRowIndex];

            return IsUsableTableDataRow(fallbackRow) ? fallbackRow : null;
        }

        private void RefreshDataGridDisplayAfterCellEditorUpdate()
        {
            try
            {
                var currencyManager = BindingContext[c1GridData.DataSource] as CurrencyManager;

                if (currencyManager != null)
                {
                    currencyManager.EndCurrentEdit();
                    currencyManager.Refresh();
                }
            }
            catch
            {
                //忽略 binding refresh 期間的例外，後面仍會用 Grid repaint
            }

            try
            {
                c1GridData.Row = _editingRowIndex;
                c1GridData.Col = _editingColumnIndex;
            }
            catch
            {
                //忽略 row/col 還原失敗
            }

            c1GridData.Invalidate();
            c1GridData.Refresh();
            c1GridData.Update();
        }

        private void CancelDataGridInlineEditWithoutCommit() //只用於準備開 CellEditorForm / CellViewerForm 前，取消 C1 的內嵌 editor
        {
            try
            {
                if (c1GridData.EditActive)
                {
                    try
                    {
                        c1GridData.Focus();

                        //C1TrueDBGrid 沒有 CancelUpdate()
                        //用 ESC 取消目前內嵌編輯器，避免 EditActive = false 直接提交目前值
                        SendKeys.SendWait("{ESC}");
                    }
                    catch
                    {
                        //忽略 ESC 取消期間的例外
                    }
                }

                try
                {
                    var currencyManager = BindingContext[c1GridData.DataSource] as CurrencyManager;

                    if (currencyManager != null)
                    {
                        currencyManager.CancelCurrentEdit();
                        currencyManager.Refresh();
                    }
                }
                catch
                {
                    //忽略 binding cancel 期間的例外
                }
            }
            finally
            {
                _isInEditMode = false;
                tsData.Enabled = true;

                c1GridData.Invalidate();
                c1GridData.Refresh();
                c1GridData.Update();
            }
        }

        private void CommitDataGridInlineEditBeforeCellNavigation() //用於一般滑鼠切換 Cell，提交使用者剛輸入的值
        {
            try
            {
                if (c1GridData.EditActive)
                {
                    c1GridData.EditActive = false;
                }

                try
                {
                    var currencyManager = BindingContext[c1GridData.DataSource] as CurrencyManager;

                    if (currencyManager != null)
                    {
                        currencyManager.EndCurrentEdit();
                        currencyManager.Refresh();
                    }
                }
                catch
                {
                    //忽略 binding commit 期間的例外，後面仍會重整狀態
                }
            }
            finally
            {
                _isInEditMode = false;
                tsData.Enabled = true;

                RefreshTableEditStateMarkers();
                CheckButtonsStatus();

                c1GridData.Invalidate();
                c1GridData.Refresh();
                c1GridData.Update();
            }
        }

        private bool IsUsableTableDataRow(DataRow row)
        {
            if (row == null)
            {
                return false;
            }

            if (row.RowState == DataRowState.Detached)
            {
                return false;
            }

            if (row.Table == null)
            {
                return false;
            }

            if (_dtTableData == null)
            {
                return false;
            }

            return _dtTableData.Rows.IndexOf(row) >= 0;
        }
    }
}