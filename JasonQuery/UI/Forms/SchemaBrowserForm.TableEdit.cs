using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void tabData_Enter(object sender, EventArgs e)
        {
            //20240707 切換到 c1GridData 會重複觸發 tabData_Enter，故以下先解除綁定，再重新綁定
            tabData.Enter -= new EventHandler(tabData_Enter);
            c1GridData.Focus(); //切換到 c1GridData，當滑鼠移至異動處時，才會有提示效果
            tabData.Enter += new EventHandler(tabData_Enter);
        }

        private void UpdateEditMode()
        {
            _isInEditMode = false;
            c1GridData.EditActive = false;
            tsData.Enabled = true;

            CheckButtonsStatus();
        }

        private bool CheckEditCellWithEditForm(int row, int col, bool shouldOpenForm = true)
        {
            var result = false;

            if (row < 0 || col < 0 || col >= c1GridData.Columns.Count)
            {
                return false;
            }

            var columnName = c1GridData.Columns[col]?.DataField ?? string.Empty;

            if (string.IsNullOrEmpty(columnName))
            {
                return false;
            }

            try
            {
                if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    return false;
                }

                var currentRow = GetDataRowFromGridDisplayRow(c1GridData, row);

                if (!IsUsableTableDataRow(currentRow))
                {
                    return false;
                }

                if (!currentRow.Table.Columns.Contains(_identifyColumnName))
                {
                    return false;
                }

                var operationMode = currentRow.GetSafeString(_identifyColumnName);
                var canOpenEditor = CanOpenCellEditorForm(columnInfo);

                //Binary 檔案模式會直接執行 UPDATE SQL；尚未儲存至資料庫的 NEW/CLONE 新資料不適用
                if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary && _operationModeNewClone.Contains(operationMode))
                {
                    return false;
                }

                //針對「完全不支援 CellEditorForm」的欄位，且是 NEW 資料列，不處理
                if (!canOpenEditor && string.Equals(operationMode, "NEW", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var dataType = columnInfo.BaseDataType;

                if (string.IsNullOrEmpty(dataType))
                {
                    return false;
                }

                if (IsShortDirectEditDataType(columnInfo))
                {
                    return false;
                }

                if (canOpenEditor)
                {
                    result = true;

                    if (shouldOpenForm)
                    {
                        CellEditor(c1GridData);
                    }

                    return result;
                }

                //不可編輯的欄位仍可透過 CellViewerForm 檢視完整內容
                result = true;

                if (shouldOpenForm)
                {
                    CellViewer(c1GridData);
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result;
        }

        private bool IsShortDirectEditDataType(ColumnInfo columnInfo)
        {
            if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.DateTime || columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.Number)
            {
                return true;
            }

            return columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.String && columnInfo.ColumnSize < 10;
        }

        private bool CanOpenCellEditorForm(ColumnInfo columnInfo)
        {
            if (columnInfo == null)
            {
                return false;
            }

            //LargeText 一律由 CellViewerForm 唯讀顯示，不進入 CellEditorForm
            if (IsReadOnlyLargeTextColumn(columnInfo))
            {
                return false;
            }

            //LargeBinary 由 CellEditorForm 的檔案模式處理；不允許把 Alias 當一般文字編輯
            if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
            {
                return true;
            }

            return IsTableDataColumnEditable(columnInfo);
        }

        private void btnApplyEdit_Click(object sender, EventArgs e)
        {
            ApplyEdit();
        }

        private void ApplyEdit()
        {
            if (tabSchemaBrowser.SelectedTab.Name != "tabSqlPreview") //選中的不是 SQL 預覽頁籤，呼叫 GenerateSqlPreview() 產生最新的 SQL 預覽內容
            {
                GenerateSqlPreview();
            }

            if (string.IsNullOrEmpty(editorSqlPreview.Text))
            {
                return;
            }

            var hasSavePoint = false;
            var seqNo = $"{DateTime.Now:MMdd_ffffff}"; //20240723 識別碼，識別此為同一次的操作
            var sql = string.Empty;
            var errorMessage = string.Empty;
            var errorMessageFinal = string.Empty;
            var savePointResult = string.Empty;
            var savePointName = $"jqcc1688ccqj{DateTime.Now:HHmmssff}";
            var applyEditMsg = LocalizationHelper.GetLanguageString("(View/Edit) Table Data => Apply All Changes", "form", GetType().Name, "msg", "Data2ApplyEdit", "Text");
            var createSavePoint = LocalizationHelper.GetLanguageString("Create Save Point", "form", GetType().Name, "msg", "CreateSavePoint", "Text");
            var restoreSavePoint = LocalizationHelper.GetLanguageString("Error occurred, execute ROLLBACK statement.", "form", GetType().Name, "msg", "RestoreSavePoint", "Text");
            var noneSavePoint = LocalizationHelper.GetLanguageString("No errors occurred, no rollback is required.", "form", GetType().Name, "msg", "NoneSavePoint", "Text");

            //直接寫入的 Binary 變更只供預覽說明，不能當成 SQL 再執行一次。
            var executablePreviewText = RemoveDirectBinaryPreviewNotes(editorSqlPreview.Text);

            //20260704 去除提示文字後逐一處理 SQL Statement
            var parts = executablePreviewText.Replace(_sqlPreviewHint1, string.Empty).Replace(_sqlPreviewHint2, string.Empty).Trim('\r', '\n').Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var shouldCreateSavePoint = ShouldCreateApplyEditSavePoint(parts.Length);

            if (shouldCreateSavePoint)
            {
                //20240716 建立 Save Point
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            sql = $"SAVEPOINT {savePointName}";
                            savePointResult = MyGlobal.OracleReader.SavePoint(savePointName);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            sql = $"savepoint {savePointName}";
                            savePointResult = MyGlobal.PostgreSqlReader.SavePoint(savePointName);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            sql = $"save transaction {savePointName}";
                            savePointResult = MyGlobal.SqlServerReader.SavePoint(savePointName);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            sql = $"savepoint {savePointName}";
                            savePointResult = MyGlobal.MySqlReader.SavePoint(savePointName);
                            break;
                        }
                }

                hasSavePoint = string.IsNullOrEmpty(savePointResult);

                if (!hasSavePoint)
                {
                    UpdateSqlHistory(0, "Error", $"{createSavePoint}\r\n{savePointResult}\r\n\r\n{applyEditMsg}", sql, seqNo);

                    MessageBox.Show
                    (
                        $"{createSavePoint}\r\n\r\n{savePointResult}",
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation
                    );

                    return;
                }

                //儲存 Save Point 執行記錄
                UpdateSqlHistory(0, "Complete", $"--{createSavePoint}\r\n{applyEditMsg}", sql, seqNo);
            }

            for (var i = 0; i < parts.Length; i++)
            {
                var parts2 = parts[i].Split(new[] { ";\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                sql = parts2[0];

                var remark = parts2.Length > 1 ? parts2[1] : string.Empty;

                //執行 SQL Statement
                int affectedRows = 0;

                errorMessage = string.Empty;

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            affectedRows = MyGlobal.OracleReader.ExecuteSingleNonQuery(sql, out errorMessage);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            affectedRows = MyGlobal.PostgreSqlReader.ExecuteSingleNonQuery(sql, out errorMessage);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            affectedRows = MyGlobal.SqlServerReader.ExecuteSingleNonQuery(sql, out errorMessage);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            affectedRows = MyGlobal.MySqlReader.ExecuteSingleNonQuery(sql, out errorMessage);
                            break;
                        }
                }

                errorMessageFinal = $"{errorMessage}\r\n\r\n";

                var tempErrorMessage = string.IsNullOrEmpty(errorMessage) ? string.Empty : $"{errorMessage}\r\n\r\n";

                //儲存 SQL 歷史記錄
                UpdateSqlHistory(affectedRows, "Complete", $"{tempErrorMessage}{remark}", sql, seqNo);

                //有錯誤，不再繼續執行 SQL！
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    break;
                }
            }

            //按下「套用」後，無論成功或失敗，把「取消」按鈕停用，使用者後續只選擇「Rollback」或「Commit」
            btnCancelEditData.Enabled = false;
            btnCancelEditSqlPreview.Enabled = false;

            errorMessageFinal = errorMessageFinal.TrimEnd('\r', '\n');

            //有錯誤！
            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show($"{MyGlobal.AnErrorHasOccurred}\r\n\r\n{errorMessageFinal}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (hasSavePoint) //20260705 有成功建立 Save Point，才執行 Rollback To Save Point
                {
                    var rollbackSql = string.Empty;
                    var rollbackPointResult = string.Empty;

                    //20240716 有錯誤，還原 Save Point
                    switch (_currentSourceType)
                    {
                        case DataSourceType.Oracle: //Oracle 不用執行 Release Point (Commit/Rollback 後，會自動釋放)
                            {
                                rollbackSql = $"ROLLBACK TO SAVEPOINT {savePointName}";
                                rollbackPointResult = MyGlobal.OracleReader.RollbackPoint(savePointName);
                                break;
                            }
                        case DataSourceType.PostgreSql:
                            {
                                rollbackSql = $"rollback to savepoint {savePointName}";
                                rollbackPointResult = MyGlobal.PostgreSqlReader.RollbackPoint(savePointName);
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                rollbackSql = $"rollback transaction {savePointName}";
                                rollbackPointResult = MyGlobal.SqlServerReader.RollbackPoint(savePointName);
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                rollbackSql = $"rollback to savepoint {savePointName}";
                                rollbackPointResult = MyGlobal.MySqlReader.RollbackPoint(savePointName);
                                break;
                            }
                    }

                    //儲存 Rollback To Save Point 執行記錄
                    if (!string.IsNullOrEmpty(rollbackSql))
                    {
                        var rollbackPointMessage = string.IsNullOrEmpty(rollbackPointResult) ? restoreSavePoint : $"{restoreSavePoint}\r\n{rollbackPointResult}";

                        UpdateSqlHistory(0, string.IsNullOrEmpty(rollbackPointResult) ? "Complete" : "Error", $"{rollbackPointMessage}\r\n{applyEditMsg}", rollbackSql, seqNo);
                    }
                }
            }
            else
            {
                UpdateSqlHistory(0, "Complete", $"--{noneSavePoint}", string.Empty, seqNo); //儲存 Save Point 執行記錄

                MarkTransactionAsPending();
            }

            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;

            //20240724 由 Main Form 控制每個表單的 btnCommit/btnRollback 按鈕狀態
            TransferValueToMainForm("UpdateCommitRollbackButton`");
        }

        private bool ShouldCreateApplyEditSavePoint(int statementCount)
        {
            return AppConfigHelper.IsNotCommitYet || statementCount > 1;
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            CancelPendingTableEditChanges();
        }

        private void CancelPendingTableEditChanges()
        {
            if (_dtOriginalTableData == null)
            {
                return;
            }

            if (!HasPendingTableEditChanges())
            {
                return;
            }

            try
            {
                if (_isInEditMode)
                {
                    CancelDataGridInlineEditWithoutCommit();
                }

                Cursor = Cursors.WaitCursor;

                _dtTableData = _dtOriginalTableData.Copy();
                c1GridData.DataSource = _dtTableData;

                _modifiedCells.Clear();
                _deletedRows.Clear();
                _newRows.Clear();
                _searchResultCells.Clear();

                ClearSqlPreviewAfterCancelEdit();

                SetGridFormat();

                GridHelper.ResizeGridColumnWidth(c1GridData);
                GridHelper.SetGridHeaderLine(c1GridData);

                HideInternalDataGridColumns();
                ApplyNullDisplayStyleToDataGrid();

                RefreshTableEditStateMarkers();
                CheckButtonsStatus();
                UpdateDataCellEditCommandStatus();

                c1GridData.Invalidate();
                c1GridData.Refresh();
                c1GridData.Update();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            Commit();
        }

        private void Commit()
        {
            var hadDirectBinaryChanges = HasDirectBinaryChanges();
            var message = string.Empty;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        message = DatabaseTransactionActionHelper.TryCommitAndDisconnect
                        (
                            MyGlobal.OracleReader,
                            reader => reader.GetState(),
                            reader => reader.Commit(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.PostgreSql:
                    {
                        message = DatabaseTransactionActionHelper.TryCommitAndDisconnect
                        (
                            MyGlobal.PostgreSqlReader,
                            reader => reader.GetState(),
                            reader => reader.Commit(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.SqlServer:
                    {
                        message = DatabaseTransactionActionHelper.TryCommitAndDisconnect
                        (
                            MyGlobal.SqlServerReader,
                            reader => reader.GetState(),
                            reader => reader.Commit(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.MySql:
                    {
                        message = DatabaseTransactionActionHelper.TryCommitAndDisconnect
                        (
                            MyGlobal.MySqlReader,
                            reader => reader.GetState(),
                            reader => reader.Commit(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }
            }

            message += MyGlobal.CommitRollbackCheck == -1 ? string.Empty : "\r\n(when user closes JasonQuery)";

            UpdateSqlHistory(0, "Complete", message, string.Empty);

            ClearDirectBinaryChanges();
            MarkTransactionAsCompleted();

            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;
            tabSqlPreview.TabVisible = false;
            tabSchemaBrowser.SelectedTab = tabData;

            //Binary 的 UPDATE 已經不屬於一般 staged edit；Commit 後要明確重查資料。
            if (hadDirectBinaryChanges)
            {
                DisplaySchemaInfo2();
            }
            else
            {
                btnCancelEditData.PerformClick();
            }

            //btnCancelEditData.PerformClick() 可能重刷 Grid / 狀態，這裡再保險設一次
            MarkTransactionAsCompleted();

            //傳遞資訊至 MainForm，更新所有 QueryForm 的 Commit/Rollbak 按鈕狀態
            if (DisplayRowIndex == -1) //獨立開啟的 SchemaBrowser
            {
                MyGlobal.GlobalExecuteCommitRollback = "ExecuteCommitRollback`";
            }
            else //在 Main Form 內開啟的 SchemaBrowser
            {
                //由 Main Form 控制每個表單的 btnCommit/btnRollback 按鈕狀態
                TransferValueToMainForm("ExecuteCommitRollback`");
            }

            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            Rollback();
        }

        private void Rollback()
        {
            var hadDirectBinaryChanges = HasDirectBinaryChanges();
            var message = string.Empty;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        message = DatabaseTransactionActionHelper.TryRollbackAndDisconnect
                        (
                            MyGlobal.OracleReader,
                            reader => reader.GetState(),
                            reader => reader.Rollback(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.PostgreSql:
                    {
                        message = DatabaseTransactionActionHelper.TryRollbackAndDisconnect
                        (
                            MyGlobal.PostgreSqlReader,
                            reader => reader.GetState(),
                            reader => reader.Rollback(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.SqlServer:
                    {
                        message = DatabaseTransactionActionHelper.TryRollbackAndDisconnect
                        (
                            MyGlobal.SqlServerReader,
                            reader => reader.GetState(),
                            reader => reader.Rollback(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }

                case DataSourceType.MySql:
                    {
                        message = DatabaseTransactionActionHelper.TryRollbackAndDisconnect
                        (
                            MyGlobal.MySqlReader,
                            reader => reader.GetState(),
                            reader => reader.Rollback(),
                            reader => reader.Disconnect()
                        );

                        break;
                    }
            }

            message += MyGlobal.CommitRollbackCheck == -1 ? string.Empty : "\r\n(when user closes JasonQuery)";

            UpdateSqlHistory(0, "Complete", message, string.Empty);

            ClearDirectBinaryChanges();
            MarkTransactionAsCompleted();

            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;
            tabSqlPreview.TabVisible = false;

            if (tabData.TabVisible)
            {
                tabSchemaBrowser.SelectedTab = tabData;
            }

            if (hadDirectBinaryChanges)
            {
                DisplaySchemaInfo2();
            }

            //傳遞資訊至 MainForm，更新所有 QueryForm 的 Commit/Rollbak 按鈕狀態
            if (DisplayRowIndex == -1) //獨立開啟的 SchemaBrowser
            {
                MyGlobal.InfoFromMDIForm = $"ExecuteCommitRollback`{AccessibleDescription};";
            }
            else //在 Main Form 內開啟的 SchemaBrowser
            {
                //由 Main Form 控制每個表單的 btnCommit/btnRollback 按鈕狀態
                TransferValueToMainForm("ExecuteCommitRollback`");
            }

            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;

            //保險再設一次
            MarkTransactionAsCompleted();
        }

        private void btnEditCell_Click(object sender, EventArgs e)
        {
            EditCell();
        }

        private void EditCell()
        {
            var isViewerOnlyColumn = TryGetGridColumnEditorDefinition(c1GridData.Col, out var editorDefinition)
                                     && (editorDefinition.Metadata.ContentKind == GridColumnContentKind.Binary
                                     || editorDefinition.Metadata.ContentKind == GridColumnContentKind.LargeText);

            if (!isViewerOnlyColumn && c1GridData.Col >= 0 && c1GridData.Col < c1GridData.Columns.Count)
            {
                var columnName = c1GridData.Columns[c1GridData.Col].DataField;

                isViewerOnlyColumn = _columnInfoCollector != null
                                     && _columnInfoCollector.TryGet(columnName, out var columnInfo)
                                     && (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary
                                     || IsReadOnlyLargeTextColumn(columnInfo));
            }

            if (isViewerOnlyColumn)
            {
                CellViewer(c1GridData);
            }
            else
            {
                c1GridData.Focus();
                c1GridData.EditActive = true;

                _editingRowIndex = c1GridData.Row;
                _editingColumnIndex = c1GridData.Col;

                var dt = c1GridData.GetDataTableSourceOrNull();

                _isInEditMode = true;
                tsData.Enabled = false;
                ClearModifiedCellTip(c1GridData);
            }
        }

        private void btnEditCellWithEditForm_Click(object sender, EventArgs e)
        {
            EditCellWithEditForm();
        }

        private void EditCellWithEditForm()
        {
            var row = c1GridData.Row;
            var col = c1GridData.Col;

            c1GridData.Focus();
            OpenCellEditorFormFromDataGrid(row, col, true);
        }

        private void btnSetNull_Click(object sender, EventArgs e)
        {
            SetToNull();
        }

        private void SetToNull()
        {
            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return;
            }

            if (!IsVisibleDataGridDataRow(c1GridData.Row))
            {
                return;
            }

            var currentRow = GetDataRowFromGridDisplayRow(c1GridData, c1GridData.Row);

            if (currentRow == null || currentRow.Table == null)
            {
                return;
            }

            if (!IsGridDataColumnUpdateSupported(c1GridData.Col))
            {
                return;
            }

            var columnName = c1GridData.Columns[c1GridData.Col].DataField;

            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            if (!currentRow.Table.Columns.Contains(columnName))
            {
                return;
            }

            var nullDisplayText = string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs;

            currentRow[columnName] = nullDisplayText;
            currentRow.EndEdit();

            RefreshTableEditStateMarkers();
            CheckButtonsStatus();
            UpdateDataCellEditCommandStatus(c1GridData.Row, c1GridData.Col);
            c1GridData.Invalidate();
            c1GridData.Refresh();
        }

        private void btnDuplicateCurrentRow_Click(object sender, EventArgs e)
        {
            DuplicateCurrentRow();
        }

        private void DuplicateCurrentRow()
        {
            try
            {
                if (_dtTableData == null || _dtTableData.Rows.Count == 0)
                {
                    return;
                }

                if (!IsVisibleDataGridDataRow(c1GridData.Row))
                {
                    return;
                }

                var sourceRow = GetDataRowFromGridDisplayRow(c1GridData, c1GridData.Row);

                if (sourceRow == null)
                {
                    return;
                }

                var rowId = sourceRow.GetSafeString(0);

                if (string.IsNullOrEmpty(rowId))
                {
                    return;
                }

                var duplicatedRow = DuplicateDataRow(rowId);
                var sourceRowIndex = _dtTableData.Rows.IndexOf(sourceRow);
                var insertIndex = sourceRowIndex >= 0 ? sourceRowIndex + 1 : _dtTableData.Rows.Count;
                var duplicatedRowId = duplicatedRow.GetSafeString(0);

                _dtTableData.Rows.InsertAt(duplicatedRow, insertIndex);

                SelectVisibleDataGridRowByRowId(duplicatedRowId, c1GridData.Row + 1);

                RefreshTableEditStateMarkers();
                CheckButtonsStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow DuplicateDataRow(string rowId)
        {
            var row = _dtTableData.NewRow();

            //20240601 改用 Linq 進行「區分大小寫」的查詢
            var dtRow = _dtTableData.AsEnumerable().FirstOrDefault(row2 => StringComparer.Ordinal.Equals(row2.Field<string>(MyGlobal.Row_Id_PK_JQ), rowId));

            for (var i = 1; i < _dtTableData.Columns.Count; i++)
            {
                row[i] = dtRow != null ? dtRow[i] : DBNull.Value;
            }

            row[0] = DateTime.Now.ToString(_dateTimeFormat);
            row[_identifyColumnName] = "CLONE";
            return row;
        }

        private void SelectVisibleDataGridRowByRowId(string rowId, int fallbackDisplayRowIndex)
        {
            var displayRowIndex = FindVisibleDataGridRowIndexByRowId(rowId);

            if (displayRowIndex < 0)
            {
                displayRowIndex = fallbackDisplayRowIndex;
            }

            var visibleRowCount = GetVisibleDataGridRowCount();

            if (visibleRowCount <= 0)
            {
                return;
            }

            if (displayRowIndex < 0)
            {
                displayRowIndex = 0;
            }

            if (displayRowIndex >= visibleRowCount)
            {
                displayRowIndex = visibleRowCount - 1;
            }

            try
            {
                c1GridData.Row = displayRowIndex;
            }
            catch
            {
                //忽略篩選狀態下 row 設定失敗
            }
        }

        private int FindVisibleDataGridRowIndexByRowId(string rowId)
        {
            if (string.IsNullOrEmpty(rowId))
            {
                return -1;
            }

            var visibleRowCount = GetVisibleDataGridRowCount();

            for (var displayRowIndex = 0; displayRowIndex < visibleRowCount; displayRowIndex++)
            {
                if (!IsVisibleDataGridDataRow(displayRowIndex))
                {
                    continue;
                }

                var row = GetDataRowFromGridDisplayRow(c1GridData, displayRowIndex);

                if (row == null)
                {
                    continue;
                }

                if (StringComparer.Ordinal.Equals(row.GetSafeString(MyGlobal.Row_Id_PK_JQ), rowId))
                {
                    return displayRowIndex;
                }
            }

            return -1;
        }

        private void btnDeleteCurrentRow_Click(object sender, EventArgs e)
        {
            DeleteCurrentRow();
        }

        private void DeleteCurrentRow()
        {
            try
            {
                if (_dtTableData == null || _dtTableData.Rows.Count == 0)
                {
                    return;
                }

                if (!IsVisibleDataGridDataRow(c1GridData.Row))
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;

                var currentDisplayRowIndex = c1GridData.Row;
                var currentRow = GetDataRowFromGridDisplayRow(c1GridData, currentDisplayRowIndex);

                if (currentRow == null || currentRow.Table == null)
                {
                    return;
                }

                var rowId = currentRow.GetSafeString(MyGlobal.Row_Id_PK_JQ);

                if (string.IsNullOrEmpty(rowId))
                {
                    return;
                }

                var operationMode = currentRow.GetSafeString(_identifyColumnName);

                if (_operationModeNewClone.Contains(operationMode))
                {
                    //新增或複製的資料尚未真正套用到資料庫，直接從 DataTable 移除即可
                    _dtTableData.Rows.Remove(currentRow);

                    RefreshDataGridBindingAfterRowRemoved();
                }
                else
                {
                    currentRow[_identifyColumnName] = "DEL";

                    var originalRow = FindOriginalTableDataRowByRowId(rowId);

                    //使用者可能已經變更內容；刪除時先還原成原值，讓後續 Delete SQL 的 where 提示資訊維持正確
                    if (originalRow != null)
                    {
                        RestoreDataRowValuesFromOriginal(currentRow, originalRow);
                    }

                    currentRow.EndEdit();
                }

                RefreshTableEditStateMarkers();
                SelectNearestVisibleDataGridRow(currentDisplayRowIndex);
                CheckButtonsStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RefreshDataGridBindingAfterRowRemoved()
        {
            try
            {
                var currencyManager = BindingContext[c1GridData.DataSource] as CurrencyManager;

                if (currencyManager != null)
                {
                    currencyManager.Refresh();
                }
            }
            catch
            {
                //忽略 binding refresh 期間的例外
            }

            c1GridData.Invalidate();
            c1GridData.Refresh();
            c1GridData.Update();
        }

        private DataRow FindOriginalTableDataRowByRowId(string rowId)
        {
            if (string.IsNullOrEmpty(rowId) || _dtOriginalTableData == null)
            {
                return null;
            }

            return _dtOriginalTableData.AsEnumerable()
                                       .FirstOrDefault(row => StringComparer.Ordinal.Equals(row.GetSafeString(MyGlobal.Row_Id_PK_JQ), rowId));
        }

        private void RestoreDataRowValuesFromOriginal(DataRow targetRow, DataRow originalRow)
        {
            if (targetRow == null || originalRow == null)
            {
                return;
            }

            for (var i = 1; i < _dtTableData.Columns.Count - 1; i++)
            {
                var columnName = _dtTableData.Columns[i].ColumnName;

                if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (targetRow.Table.Columns.Contains(columnName) && originalRow.Table.Columns.Contains(columnName))
                {
                    targetRow[columnName] = originalRow[columnName];
                }
            }
        }

        private void SelectNearestVisibleDataGridRow(int preferredDisplayRowIndex)
        {
            var visibleRowCount = GetVisibleDataGridRowCount();

            if (visibleRowCount <= 0)
            {
                return;
            }

            var rowIndex = preferredDisplayRowIndex;

            if (rowIndex >= visibleRowCount)
            {
                rowIndex = visibleRowCount - 1;
            }

            if (rowIndex < 0)
            {
                rowIndex = 0;
            }

            try
            {
                c1GridData.Row = rowIndex;
            }
            catch
            {
                //忽略篩選狀態下 row 設定失敗
            }
        }

        private bool HasPendingTableEditChanges()
        {
            return _modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0;
        }

        private void SetApplyCancelButtonsEnabled(bool enabled)
        {
            btnApplyEditData.Enabled = enabled;
            btnApplyEditSqlPreview.Enabled = enabled;
            btnCancelEditData.Enabled = enabled;
            btnCancelEditSqlPreview.Enabled = enabled;
        }

        private void ClearSqlPreviewAfterCancelEdit()
        {
            editorSqlPreview.ReadOnly = false;
            editorSqlPreview.Text = string.Empty;
            editorSqlPreview.ReadOnly = true;

            tabSqlPreview.TabVisible = false;

            btnSqlPreviewData.Enabled = false;
            btnApplyEditData.Enabled = false;
            btnApplyEditSqlPreview.Enabled = false;
            btnCancelEditData.Enabled = false;
            btnCancelEditSqlPreview.Enabled = false;
        }

        private void HideInternalDataGridColumns()
        {
            foreach (C1DisplayColumn col in c1GridData.Splits[0].DisplayColumns)
            {
                var columnName = col.Name.Split(new[] { "\r\n" }, StringSplitOptions.None)[0];

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                {
                    col.Visible = false;
                    col.Frozen = true;
                }

                if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    col.Visible = false;
                    col.Frozen = true;
                    break;
                }
            }
        }

        private void ApplyNullDisplayStyleToDataGrid()
        {
            if (string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var colorStyle = new Style
            {
                ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
            };

            for (var i = 0; i < c1GridData.Columns.Count; i++)
            {
                c1GridData.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorStyle, MyLibrary.GridNullShowAs);
            }
        }
    }
}
