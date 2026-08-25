using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        /// <summary>
        /// 只檢查目前 Cell 的編輯能力
        /// 例如：
        ///  - 目前欄位是否 IsUpdateValueSupported
        ///  - 目前欄位是否 IsNullable
        ///  - 目前列是否 DEL
        ///  - 目前值是否已經是 NULL 顯示值
        ///  - Edit Cell / Edit Form / Set Null 是否可用
        ///  - Grid 欄位是否 Locked
        /// </summary>
        private void UpdateDataCellEditCommandStatus()
        {
            UpdateDataCellEditCommandStatus(c1GridData.Row, c1GridData.Col);
        }

        private void UpdateDataCellEditCommandStatus(int row, int col)
        {
            btnSetNullData.Enabled = false;
            btnEditCellData.Enabled = false;
            btnEditCellWithEditFormData.Enabled = false;

            try
            {
                if (!CanUpdateDataCellEditCommandStatus(row, col))
                {
                    return;
                }

                var columnName = c1GridData.Columns[col].DataField;

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    return;
                }

                var currentRow = GetDataRowFromGridDisplayRow(c1GridData, row);

                if (currentRow == null || currentRow.Table == null || !currentRow.Table.Columns.Contains(columnName))
                {
                    return;
                }

                var isEditable = false;
                var isNullable = false;

                if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    isEditable = IsTableDataColumnEditable(columnInfo);
                    isNullable = columnInfo.IsNullable;
                }

                c1GridData.Splits[0].DisplayColumns[col].Locked = !isEditable;
                btnEditCellData.Enabled = isEditable;
                btnEditCellWithEditFormData.Enabled = CheckEditCellWithEditForm(row, col, false);

                if (!isEditable || !isNullable)
                {
                    btnSetNullData.Enabled = false;
                    return;
                }

                var operationMode = currentRow.GetSafeString(_identifyColumnName);

                if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    btnSetNullData.Enabled = false;
                    return;
                }

                var cellValue = currentRow[columnName]?.ToString() ?? string.Empty;

                btnSetNullData.Enabled = !string.Equals(cellValue, MyLibrary.GridNullShowAs, StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                btnSetNullData.Enabled = false;
                btnEditCellData.Enabled = false;
                btnEditCellWithEditFormData.Enabled = false;

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool CanUpdateDataCellEditCommandStatus(int row, int col)
        {
            if (_isInEditMode)
            {
                return false;
            }

            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return false;
            }

            if (!HasVisibleDataGridRows())
            {
                return false;
            }

            if (!IsVisibleDataGridDataRow(row))
            {
                return false;
            }

            if (col < 0 || col >= c1GridData.Columns.Count)
            {
                return false;
            }

            if (!_dtTableData.Columns.Contains(_identifyColumnName))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 檢查整體 Table Data 編輯狀態
        /// 例如：
        ///  - 是否有資料
        ///  - 上一筆 / 下一筆 / 首筆 / 末筆
        ///  - Insert / Duplicate / Delete
        ///  - Apply / Cancel / Commit / Rollback
        ///  - 搜尋按鈕
        ///  - 是否顯示 SQL Preview
        /// </summary>
        private void CheckButtonsStatus()
        {
            var currentRow = c1GridData.Row;
            var currentCol = c1GridData.Col;
            var visibleRowCount = GetVisibleDataGridRowCount();
            var hasVisibleRows = HasVisibleDataGridRows();

            if (_dtTableData == null || _dtTableData.Rows.Count == 0 || !hasVisibleRows)
            {
                ClearCurrentDataCellCommandStatus();

                btnSelectAllData.Enabled = false;
                btnCopyData.Enabled = false;
                btnExportToFileData.Enabled = false;
                btnDuplicateCurrentRowData.Enabled = false;
                btnDeleteCurrentRowData.Enabled = false;

                btnTopData.Enabled = false;
                btnPreviousData.Enabled = false;
                btnNextData.Enabled = false;
                btnLastData.Enabled = false;

                btnSqlPreviewData.Enabled = _modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0;
                btnApplyEditData.Enabled = btnSqlPreviewData.Enabled;

                SetCommitRollbackButtonsEnabled(AppConfigHelper.IsNotCommitYet);

                lblInfo.Text = string.Empty;
                return;
            }

            var isFound = !string.IsNullOrEmpty(cboFind.Text) && cboFind.Enabled;
            var row = hasVisibleRows ? GetDataRowFromGridDisplayRow(c1GridData, currentRow) : null;
            var isDeletedRow = row != null && row.RowState == DataRowState.Detached;

            btnSelectAllSqlPane.Enabled = true;
            btnCopySqlPane.Enabled = true;
            btnFilterData.Enabled = true;
            btnInsertNewRowData.Enabled = true;
            chkShowFilterRowData.Enabled = true;

            btnFindNextData.Enabled = isFound;
            btnFindPreviousData.Enabled = isFound;
            btnCountData.Enabled = isFound;
            btnHighlightData.Enabled = isFound;
            btnClearHighlightData.Enabled = isFound;

            try
            {
                if (!hasVisibleRows || row == null)
                {
                    btnSelectAllSqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.Text);
                    btnCopySqlPane.Enabled = !string.IsNullOrEmpty(editorSqlPane.SelectedText);

                    btnEditCellData.Enabled = false;
                    btnEditCellWithEditFormData.Enabled = false;
                    btnDuplicateCurrentRowData.Enabled = false;
                    btnDeleteCurrentRowData.Enabled = false;
                    btnSetNullData.Enabled = false;
                    btnSelectAllData.Enabled = false;
                    btnCopyData.Enabled = false;
                    btnExportToFileData.Enabled = false;
                    btnTopData.Enabled = false;
                    btnPreviousData.Enabled = false;
                    btnNextData.Enabled = false;
                    btnLastData.Enabled = false;

                    //仍保留篩選列勾選能力，否則使用者篩到 0 筆時反而不容易復原
                    chkShowFilterRowData.Enabled = true;

                    return;
                }

                btnSelectAllData.Enabled = true;
                btnCopyData.Enabled = true;
                btnExportToFileData.Enabled = true;
                btnDuplicateCurrentRowData.Enabled = true;
                btnDeleteCurrentRowData.Enabled = true;

                btnPreviousData.Enabled = currentRow > 0;
                btnTopData.Enabled = currentRow > 0;
                btnNextData.Enabled = currentRow < visibleRowCount - 1;
                btnLastData.Enabled = currentRow < visibleRowCount - 1;

                var operationMode = isDeletedRow ? "DELETED" : row.GetSafeString(_identifyColumnName);

                btnEditCellData.Enabled = !isDeletedRow && IsGridDataColumnUpdateSupported(currentCol);
                btnEditCellWithEditFormData.Enabled = !isDeletedRow && CheckEditCellWithEditForm(currentRow, currentCol, false);

                if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    btnEditCellData.Enabled = false;
                    btnEditCellWithEditFormData.Enabled = false;
                    btnDeleteCurrentRowData.Enabled = false;
                    btnDuplicateCurrentRowData.Enabled = false;
                    btnSetNullData.Enabled = false;
                }
                else if (_operationModeNewClone.Contains(operationMode))
                {
                    //插入或複製，可以被刪除
                    btnDuplicateCurrentRowData.Enabled = false;
                }
                else if (string.Equals(operationMode, "DELETED", StringComparison.OrdinalIgnoreCase))
                {
                    btnEditCellData.Enabled = false;
                    btnEditCellWithEditFormData.Enabled = false;
                    btnDeleteCurrentRowData.Enabled = false;
                    btnDuplicateCurrentRowData.Enabled = false;
                    btnSetNullData.Enabled = false;
                    btnSelectAllData.Enabled = false;
                    btnCopyData.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                //檢查是否有任何異動
                var hasStagedTableChanges = HasPendingTableEditChanges();
                var hasDirectBinaryChanges = HasDirectBinaryChanges();
                var hasPreviewContent = hasStagedTableChanges || hasDirectBinaryChanges;

                tabSqlPreview.TabVisible = hasPreviewContent;
                btnSqlPreviewData.Enabled = hasPreviewContent;

                //Binary 檔案寫入已經直接執行 UPDATE SQL 了，不能再透過 Apply/Cancel 處理。
                btnApplyEditData.Enabled = hasStagedTableChanges;
                btnApplyEditSqlPreview.Enabled = hasStagedTableChanges;
                btnCancelEditData.Enabled = hasStagedTableChanges;
                btnCancelEditSqlPreview.Enabled = hasStagedTableChanges;

                //20260704 先以 AppConfigHelper.IsNotCommitYet 變數判斷按鈕是否可用 (使用者可能在 QueryForm 執行異動 SQL)
                SetCommitRollbackButtonsEnabled(AppConfigHelper.IsNotCommitYet);

                lblInfo.Text = string.Empty;

                UpdateDataCellEditCommandStatus(); //20260529 判斷目前 Cell 的 Edit / Set Null / Lock 狀態
            }
        }

        private void MarkTransactionAsPending()
        {
            AppConfigHelper.IsNotCommitYet = true;
            SetCommitRollbackButtonsEnabled(true);
        }

        private void MarkTransactionAsCompleted()
        {
            AppConfigHelper.IsNotCommitYet = false;
            SetCommitRollbackButtonsEnabled(false);
        }

        private void SetCommitRollbackButtonsEnabled(bool enabled)
        {
            btnCommitData.Enabled = enabled;
            btnRollbackData.Enabled = enabled;
            btnCommitSqlPreview.Enabled = enabled;
            btnRollbackSqlPreview.Enabled = enabled;
        }
    }
}
