using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void SplitButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var clickedItem = (e.ClickedItem.Tag ?? Tag)?.ToString() ?? string.Empty;

            try
            {
                switch (clickedItem)
                {
                    case "mnuExpandAll":
                        {
                            for (var rowIndex = 0; rowIndex < c1GridSchemaBrowser.Splits[0].Rows.Count; rowIndex++)
                            {
                                if (c1GridSchemaBrowser.Splits[0].Rows[rowIndex].RowType != RowTypeEnum.DataRow)
                                {
                                    c1GridSchemaBrowser.ExpandGroupRow(rowIndex);
                                }
                            }

                            break;
                        }
                    case "mnuCollapseAll":
                        {
                            for (var rowIndex = 0; rowIndex < c1GridSchemaBrowser.Splits[0].Rows.Count; rowIndex++)
                            {
                                if (c1GridSchemaBrowser.Splits[0].Rows[rowIndex].RowType != RowTypeEnum.DataRow)
                                {
                                    c1GridSchemaBrowser.CollapseGroupRow(rowIndex);
                                }
                            }

                            //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
                            c1GridSchemaBrowser.ExpandGroupRow(0); //展開 0 的結點
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }

            Cursor = Cursors.Default;
        }

        private void UpdateSchemaInformation(bool isRefreshButton = false, bool isLocation = false)
        {
            Application.UseWaitCursor = true;

            var form = new MessageForm();

            if (isRefreshButton)
            {
                var message = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");

                form.Caption = message;

                if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1)
                {
                    message = LocalizationHelper.GetLanguageString("Getting schema information (include all column information of Tables) ...", "Global", "Global", "msg", "GetSchemaInfoIncludeColumn", "Text");
                    form.Info = message;

                    if (isLocation) //視窗顯示的位置靠近游標所在位置
                    {
                        var x = Cursor.Position.X + 25;
                        var y = Cursor.Position.Y;

                        form.Location = new Point(x, y);
                        form.Size = new Size(form.Width, form.Height + 20);
                    }
                    else
                    {
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.IsNeedToMovePosition = true; //20220706 加入此變數，螢幕置中後，視窗的位置再往上調整一些，當有錯誤訊息時，MessageBox 才不會被蓋住！
                    }

                    form.TopLevel = true; //20220720 改用 TopLevel，只在 JasonQuery 最上層顯示
                    form.Show(this);
                    form.Refresh(); //20231104 Refresh 方法要放在 Show 方法之後，Label 才會即時顯示出來
                }
            }

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        using (TraceLogger.Time("Call DatabaseHelper.UpdateSchemaData_Oracle"))
                        {
                            DatabaseSqlExecutor.UpdateSchemaData_Oracle(c1GridSchemaBrowser);
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        using (TraceLogger.Time("Call DatabaseHelper.UpdateSchemaData_PostgreSql"))
                        {
                            DatabaseSqlExecutor.UpdateSchemaData_PostgreSql(c1GridSchemaBrowser);
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        using (TraceLogger.Time("Call DatabaseHelper.UpdateSchemaData_SqlServer"))
                        {
                            DatabaseSqlExecutor.UpdateSchemaData_SqlServer(c1GridSchemaBrowser, false);
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        using (TraceLogger.Time("Call DatabaseHelper.UpdateSchemaData_MySql"))
                        {
                            DatabaseSqlExecutor.UpdateSchemaData_MySql(c1GridSchemaBrowser);
                        }

                        break;
                    }
            }

            form.Dispose();
            Application.UseWaitCursor = false;
        }

        private void UpdateSchemaInfo()
        {
            if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo == -1) //20241115 判斷架構瀏覽器是否要顯示欄位訊息
            {
                MyGlobal.ShowColumnInfo = 1;

                try
                {
                    using (TraceLogger.Time("Update Schema Information, call UpdateSchemaInformation()"))
                    {
                        UpdateSchemaInformation(true, false);
                    }

                    ResetSchemaFilterUi();

                    //傳遞資訊至 MainForm，更新其他 QueryForm 的 Schema 資訊
                    TransferValueToMainForm($"UpdateSchemaInformation`{AccessibleDescription}");

                    //20250911 傳遞資訊至 MainForm，更新其他 QueryForm 的 QueryEditor Setting 資訊 (此處在更新 Schema 訊息後再套用編輯器的關鍵字高亮設定)
                    TransferValueToMainForm("ReloadQueryEditorSetting`");

                    MyGlobal.ClearMemory();
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            var isLocation = true;

            if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo == -1)
            {
                isLocation = false;
                MyGlobal.ShowColumnInfo = 1;
            }

            try
            {
                using (TraceLogger.Time("Update Schema Information, call UpdateSchemaInformation()"))
                {
                    UpdateSchemaInformation(true, isLocation);
                }

                ResetSchemaFilterUi();

                //傳遞資訊至 MainForm，更新其他 QueryForm 的 Schema 資訊
                TransferValueToMainForm($"UpdateSchemaInformationRefreshClick`{AccessibleDescription}");
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnHelp_SchemaFilter_Click(object sender, EventArgs e)
        {
            var title = LocalizationHelper.GetLanguageString("Schema Filter Overview", "form", GetType().Name, "msg", "Help_SchemaFilterTitle", "Text");
            var message = LocalizationHelper.GetLanguageString("The Schema Filter allows you to search database objects using keywords and display the results in a tree structure.\r\n\r\nSupported object types include the following:\r\n- Functions\r\n- Packages\r\n- Procedures\r\n- Tables\r\n- Views\r\n\r\nFor Tables and Views, the filter also applies to:\r\n- Column names\r\n- Data types\r\n\r\nIf the keyword matches either one, the table or view will be included in the result.\r\n\r\nThis feature is especially useful when you know a column name but are not sure which tables or views reference it.", "form", GetType().Name, "msg", "Help_SchemaFilter", "Text");

            MessageBoxHelper.ShowNearCursor(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetSchemaFilterUi()
        {
            txtSchemaFilter.Text = "*";
            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);
            AutoResizeGridColumnWidthForSchema();
        }

        private void txtSchemaFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && e.Control) //20250905 加入 Ctrl+Z 判斷，否則使用者按下 Ctrl+Z 會傳入「i」到 editor！
            {
                txtSchemaFilter.Undo();
                txtSchemaFilter.ClearUndo();

                e.Handled = true;
            }
        }

        private void txtSchemaFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                QuerySchema();
            }
        }

        private void QuerySchema()
        {
            var filterCondition = string.Empty;
            var dtSchema = c1GridSchemaBrowser.GetDataTableSourceOrNull();

            if (string.IsNullOrWhiteSpace(txtSchemaFilter.Text))
            {
                txtSchemaFilter.Text = @"*";
            }

            if (!txtSchemaFilter.Text.EndsWith("*", StringComparison.Ordinal))
            {
                txtSchemaFilter.Text += @"*";
            }

            var schemaFilter = txtSchemaFilter.Text.Trim()
                                                   .Replace("%", "*")
                                                   .Replace("**", "*")
                                                   .Replace("'", "''")
                                                   .Replace("*", "%");

            try
            {
                //20230810 修正：當使用者在搜尋字串中間有用到 % 符號時，變更 sFilter 寫法 (Wildcard characters are not allowed in the middle of a string. For example, 'te*xt' is not allowed.)
                if (MyGlobal.IsShowColumnInfo)
                {
                    if (schemaFilter.StartsWith("%", StringComparison.Ordinal)
                        && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 3)
                    {
                        var schemaName = string.Empty;
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var index = 0; index < parts.Length; index++)
                        {
                            if (index == 0)
                            {
                                schemaName = $"SchemaName LIKE '%{parts[index]}%'";
                                schemaBrowser = $"Schema_Browser LIKE '%{parts[index]}%'";
                            }
                            else
                            {
                                schemaName += $" AND SchemaName LIKE '%{parts[index]}%'";
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[index]}%'";
                            }
                        }

                        filterCondition = $"({schemaName}) OR ({schemaBrowser})";
                    }
                    else if (!schemaFilter.StartsWith("%", StringComparison.Ordinal)
                             && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 2)
                    {
                        var schemaName = string.Empty;
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var index = 0; index < parts.Length; index++)
                        {
                            if (index == 0)
                            {
                                schemaName = $"SchemaName LIKE '{parts[index]}%'";
                                schemaBrowser = $"Schema_Browser LIKE '{parts[index]}%'";
                            }
                            else
                            {
                                schemaName += $" AND SchemaName LIKE '%{parts[index]}%'";
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[index]}%'";
                            }
                        }

                        filterCondition = $"({schemaName}) OR ({schemaBrowser})";
                    }
                    else
                    {
                        filterCondition = $"(SchemaName LIKE '{schemaFilter}' OR Schema_Browser LIKE '{schemaFilter}')";
                    }
                }
                else
                {
                    if (schemaFilter.StartsWith("%", StringComparison.Ordinal)
                        && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 3)
                    {
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var index = 0; index < parts.Length; index++)
                        {
                            if (index == 0)
                            {
                                schemaBrowser = $"Schema_Browser LIKE '%{parts[index]}%'";
                            }
                            else
                            {
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[index]}%'";
                            }
                        }

                        filterCondition = schemaBrowser;
                    }
                    else if (!schemaFilter.StartsWith("%", StringComparison.Ordinal)
                             && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 2)
                    {
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var index = 0; index < parts.Length; index++)
                        {
                            if (index == 0)
                            {
                                schemaBrowser = $"Schema_Browser LIKE '{parts[index]}%'";
                            }
                            else
                            {
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[index]}%'";
                            }
                        }

                        filterCondition = schemaBrowser;
                    }
                    else
                    {
                        filterCondition = $"Schema_Browser LIKE '{schemaFilter}'";
                    }
                }

                dtSchema.DefaultView.RowFilter = filterCondition;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
                c1GridSchemaBrowser.ExpandGroupRow(0);
                c1GridSchemaBrowser.Focus();
            }
        }

        private void c1GridSchemaBrowser_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor("SchemaBrowser");
        }

        private void c1GridSchemaBrowser_MouseUp(object sender, MouseEventArgs e)
        {
            var displayRowIndex = c1GridSchemaBrowser.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return;
            }

            //切換：當滑鼠點到 Grid 最右邊的空白處時，一樣會有點選的效果，但 Focus 列並不會同時切換，所以此處要強制切換
            c1GridSchemaBrowser.Row = displayRowIndex;
        }

        private void mnuExpandAll_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        private void mnuCollapseAll_Click(object sender, EventArgs e)
        {
            CollapseAll();
        }

        private async void ExpandAll() //20250523 改為 async/await 寫法
        {
            Application.UseWaitCursor = true;

            var rowCount = c1GridSchemaBrowser.Splits[0].Rows.Count;

            await Task.Run
            (() =>
            {
                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    if (c1GridSchemaBrowser.Splits[0].Rows[rowIndex] != null
                        && c1GridSchemaBrowser.Splits[0].Rows[rowIndex].RowType != RowTypeEnum.DataRow)
                    {
                        c1GridSchemaBrowser.ExpandGroupRow(rowIndex);
                    }
                }
            });

            c1GridSchemaBrowser.Focus();
            c1GridSchemaBrowser.Cursor = Cursors.Default;
            Application.UseWaitCursor = false;
        }

        private async void CollapseAll()
        {
            Application.UseWaitCursor = true;

            var rowCount = c1GridSchemaBrowser.Splits[0].Rows.Count;

            await Task.Run
            (() =>
            {
                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    if (c1GridSchemaBrowser.Splits[0].Rows[rowIndex] != null
                        && c1GridSchemaBrowser.Splits[0].Rows[rowIndex].RowType != RowTypeEnum.DataRow)
                    {
                        c1GridSchemaBrowser.CollapseGroupRow(rowIndex);
                    }
                }
            });

            c1GridSchemaBrowser.Focus();
            c1GridSchemaBrowser.Cursor = Cursors.Default;
            Application.UseWaitCursor = false;

            //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
            //參考 MyGlobal.UpdateSchemaData()
            c1GridSchemaBrowser.ExpandGroupRow(0);
        }

        private void AutoResizeGridColumnWidthForSchema()
        {
            _isColumnAutoResizing = true;

            if ((DataTable)c1GridSchemaBrowser.DataSource == null)
            {
                return;
            }

            var isShowColumn = MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1;
            var widthOffset = 0;
            var columnIndex = 3 + (isShowColumn ? 1 : 0);

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        columnIndex--;
                        widthOffset = 85 + (isShowColumn ? 22 : 0);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        widthOffset = 115 + (isShowColumn ? -13 : -26);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        widthOffset = 105 + (isShowColumn ? 2 : -10);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        widthOffset = 120 + (isShowColumn ? -10 : -23);
                        break;
                    }
            }

            c1GridSchemaBrowser.Splits[0].DisplayColumns[columnIndex].Width = splitContainer1.Panel1.Width - widthOffset;
            c1GridSchemaBrowser.Refresh();
            _isColumnAutoResizing = false;
        }
    }
}