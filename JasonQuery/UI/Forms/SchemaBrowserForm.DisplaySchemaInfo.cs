using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void DisplaySchemaInfo2()
        {
            if (_modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0)
            {
                var result = MessageBox.Show(_confirmExitTableEditMessage, tabData.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes: //使用者選擇「放棄修改」
                        {
                            break;
                        }
                    case DialogResult.No: //使用者選擇「保留修改」
                        {
                            return;
                        }
                }
            }

            DisplaySchemaInfo(_userSelectedDisplayRowIndex);
        }

        private void DisplaySchemaInfo(int displayRowIndex)
        {
            Cursor = Cursors.WaitCursor;

            MessageForm form = null;
            var message = string.Empty;

            try
            {
                PrepareDisplaySchemaInfoStart();

                form = CreateAndShowSchemaInfoLoadingForm();
                cboFind.Text = string.Empty;

                var selectedTabName = GetCurrentSchemaBrowserTabName();

                ClearTableEditTrackingState();

                var selection = ResolveSchemaExplorerSelection(displayRowIndex);
                var canDisplayObject = DisplaySchemaExplorerSelection(selection);

                if (canDisplayObject)
                {
                    ScrollDataGridToLeft();
                }
                else
                {
                    ClearSchemaObjectDisplayPane();
                }

                RestoreSchemaBrowserSelectedTab(selectedTabName);
            }
            catch (Exception ex)
            {
                message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                ApplyGridNullDisplayStyle();
                DisposeSchemaInfoLoadingForm(form);
                ResetDisplaySchemaInfoUiState();
            }
        }

        private void PrepareDisplaySchemaInfoStart()
        {
            //20240602 此處先隱藏，畫面顯示效果較佳
            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;
        }

        private MessageForm CreateAndShowSchemaInfoLoadingForm()
        {
            var message = LocalizationHelper.GetLanguageString("Retrieving information about the specified object, please wait…", "form", GetType().Name, "msg", "PleaseWait4GetInfo", "Text");

            var form = new MessageForm
            {
                Info = message,
                BackColor = Color.LightYellow,
                ClientSize = new Size(550, 70),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                IsNeedToMovePosition = true, //20231111 加入此變數，MessageForm 顯示於螢幕中央時，視窗的位置再往上調整一些，如果有錯誤發生時，MessageBox 才不會剛好擋住 MessageForm！
                TopLevel = true //20250118 改用 TopLevel，只在 JasonQuery 最上層顯示
            };

            form.Show();
            form.Refresh();

            return form;
        }

        private string GetCurrentSchemaBrowserTabName()
        {
            return tabSchemaBrowser.SelectedTab == null ? string.Empty : tabSchemaBrowser.SelectedTab.Name;
        }

        private void ClearTableEditTrackingState()
        {
            _modifiedCells.Clear();
            _deletedRows.Clear();
            _newRows.Clear();

            ClearDirectBinaryChanges();

            _searchResultCells.Clear();
        }

        private void ClearSchemaObjectDisplayPane()
        {
            tabTableStructure.TabVisible = false;
            tabView100RowsTop.TabVisible = false;
            tabData.TabVisible = false;
            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;

            editorSqlPane.ReadOnly = false;
            editorSqlPane.Text = string.Empty;
            editorSqlPane.ReadOnly = true;
            editorSqlPane.Focus();
        }

        private void ScrollDataGridToLeft()
        {
            var dtTemp = c1GridData.GetDataTableSourceOrNull();

            //20240916 模擬按下 HOME/END，控制水平 ScrollBar 移到最左側
            if (dtTemp?.Rows.Count > 0)
            {
                SendKeys.SendWait("{END}");
                c1GridData.ScrollGrid(0, 0); //只用這個指令，ScrollBar 並不會完全移到最左側
                SendKeys.SendWait("{HOME}");
            }
        }

        private void RestoreSchemaBrowserSelectedTab(string selectedTabName)
        {
            switch (selectedTabName)
            {
                case "tabSqlPane":
                    {
                        tabSchemaBrowser.SelectedTab = tabSqlPane;
                        break;
                    }
                case "tabTableStructure":
                    {
                        if (tabTableStructure.TabVisible)
                        {
                            tabSchemaBrowser.SelectedTab = tabTableStructure;
                        }

                        break;
                    }
                case "tabView100RowsTop":
                    {
                        if (tabView100RowsTop.TabVisible)
                        {
                            tabSchemaBrowser.SelectedTab = tabView100RowsTop;
                        }

                        break;
                    }
                case "tabData":
                    {
                        if (tabData.TabVisible)
                        {
                            tabSchemaBrowser.SelectedTab = tabData;
                        }

                        break;
                    }
                case "tabSettings":
                    {
                        if (tabSettings.TabVisible)
                        {
                            tabSchemaBrowser.SelectedTab = tabData;
                        }

                        break;
                    }
                case "tabSqlPreview":
                    {
                        if (tabSqlPreview.TabVisible)
                        {
                            tabSchemaBrowser.SelectedTab = tabSqlPreview;
                        }

                        break;
                    }
            }
        }

        private void ApplyGridNullDisplayStyle()
        {
            if (string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var colorNull = new C1.Win.C1TrueDBGrid.Style
            {
                ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
            };

            for (var i = 0; i < c1GridStructure.Columns.Count; i++)
            {
                //套用「使用者指定的 NULL」顯示格式
                c1GridStructure.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
            }

            for (var i = 0; i < c1GridData.Columns.Count; i++)
            {
                //套用「使用者指定的 NULL」顯示格式
                c1GridData.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
            }
        }

        private void DisposeSchemaInfoLoadingForm(MessageForm form)
        {
            if (form == null)
            {
                return;
            }

            try
            {
                form.Dispose();
            }
            catch
            {
                //忽略等待視窗關閉期間的 UI 釋放例外
            }
        }

        private void ResetDisplaySchemaInfoUiState()
        {
            Cursor = Cursors.Default;
            c1GridSchemaBrowser.Cursor = Cursors.Default;

            lblTableName01.ForeColor = Color.Blue; //20260523 強制變更顏色
            lblTableName02.ForeColor = Color.Blue;

            UpdateEditCellWithEditFormButtonStatus();
        }

        private void UpdateEditCellWithEditFormButtonStatus()
        {
            //20241009 判斷並調整按鈕的狀態
            if (c1GridData.HasDataTableRows())
            {
                btnEditCellWithEditFormData.Enabled = CheckEditCellWithEditForm(0, 1, false);
            }
            else
            {
                btnEditCellWithEditFormData.Enabled = false;
            }
        }
    }
}