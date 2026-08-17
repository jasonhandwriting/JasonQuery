using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.SchemaExplorer;
using System;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private sealed class SchemaExplorerRightClickContext
        {
            public int DisplayRowIndex { get; set; }

            public int GroupLevel { get; set; } = -1;

            public bool IsGroupRow { get; set; }

            public bool IsColumnInfoRow { get; set; }

            public DataSourceType SourceType { get; set; }

            public string SchemaNode { get; set; } = string.Empty;

            public string SchemaType { get; set; } = string.Empty;

            public string SchemaName { get; set; } = string.Empty;

            public string SchemaDbo { get; set; } = string.Empty;

            public string ObjectId { get; set; } = string.Empty;

            public string PackageSpecBody { get; set; } = string.Empty;

            public string TableName { get; set; } = string.Empty;

            public string ColumnType { get; set; } = string.Empty;

            public string TitleCopyToClipboard { get; set; } = string.Empty;

            public string TitlePasteToQueryEditor { get; set; } = string.Empty;

            public int MouseX { get; set; }

            public int MouseY { get; set; }
        }

        private SchemaExplorerRightClickContext CreateSchemaExplorerRightClickContext(SchemaExplorerSelectionInfo selection, int displayRowIndex, int mouseX, int mouseY)
        {
            if (selection == null || selection.Kind == SchemaExplorerSelectionKind.Invalid)
            {
                return null;
            }

            var title1 = LocalizationHelper.GetLanguageString("Copy to Clipboard", "form", "GenerateSqlForm", "object", "btnCopyToClipboard", "Text");
            var title2 = LocalizationHelper.GetLanguageString("Paste to Query Editor", "form", "GenerateSqlForm", "object", "btnPasteToQueryEditor", "Text");

            var context = new SchemaExplorerRightClickContext
            {
                DisplayRowIndex = displayRowIndex,
                GroupLevel = selection.GroupLevel,
                IsGroupRow = selection.IsGroupRow,
                IsColumnInfoRow = false, //在 FillDataRowRightClickContext 中判斷
                SourceType = selection.SourceType,
                SchemaNode = selection.SchemaNode ?? string.Empty,
                SchemaType = selection.SchemaType ?? string.Empty,
                SchemaName = selection.SchemaName ?? string.Empty,
                SchemaDbo = selection.SchemaDbo ?? string.Empty,
                ObjectId = selection.ObjectId ?? string.Empty,
                PackageSpecBody = selection.PackageSpecBody ?? string.Empty,
                TitleCopyToClipboard = title1,
                TitlePasteToQueryEditor = title2,
                MouseX = mouseX,
                MouseY = mouseY
            };

            if (context.IsGroupRow)
            {
                FillGroupRowRightClickContext(context, displayRowIndex);
                return context;
            }

            FillDataRowRightClickContext(context, displayRowIndex);

            return context;
        }

        private void FillGroupRowRightClickContext(SchemaExplorerRightClickContext context, int displayRowIndex)
        {
            var rowReader = new C1SchemaExplorerRowReader(c1GridSchemaBrowser);

            if (context.GroupLevel < 0)
            {
                context.GroupLevel = rowReader.GetGroupLevel(displayRowIndex);
            }

            if (string.IsNullOrWhiteSpace(context.SchemaName))
            {
                context.SchemaName = rowReader.GetGroupedText(displayRowIndex);
            }
        }

        private void FillDataRowRightClickContext(SchemaExplorerRightClickContext context, int displayRowIndex)
        {
            var rowReader = new C1SchemaExplorerRowReader(c1GridSchemaBrowser);
            var row = rowReader.GetDataRow(displayRowIndex);

            if (row == null)
            {
                return;
            }

            var schemaType = row.GetSafeString("SchemaType");
            var separatorIndex = schemaType.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                schemaType = schemaType.Substring(0, separatorIndex);
            }

            if (!string.IsNullOrWhiteSpace(schemaType))
            {
                context.SchemaType = schemaType;
            }

            if (IsTableColumnInfoRow(context, row))
            {
                context.IsColumnInfoRow = true;
                FillTableNameForColumnInfoRightClick(context, row);
            }

            var schemaName = row.GetSafeString("Schema_Browser");
            var separatorIndex2 = schemaName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);
            var columnTypeIndex = schemaName.IndexOf(", ", StringComparison.Ordinal);

            if (separatorIndex2 >= 0)
            {
                schemaName = schemaName.Substring(0, separatorIndex2);
            }
            else if (columnTypeIndex >= 0)
            {
                context.ColumnType = schemaName.Substring(columnTypeIndex + 2);
                schemaName = schemaName.Substring(0, columnTypeIndex);
            }

            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                context.SchemaName = schemaName;
            }

            if (context.SourceType == DataSourceType.PostgreSql || context.SourceType == DataSourceType.MySql)
            {
                context.SchemaNode = row.GetSafeString("SchemaNode");
            }

            if (context.SourceType == DataSourceType.SqlServer)
            {
                context.SchemaNode = row.GetSafeString("SchemaNode");
                context.SchemaDbo = row.GetSafeString("SchemaDbo");
                context.ObjectId = row.GetSafeString("ObjectID");
            }
        }

        private bool IsTableColumnInfoRow(SchemaExplorerRightClickContext context, DataRow row)
        {
            if (context == null || row == null)
            {
                return false;
            }

            if (!MyGlobal.IsShowColumnInfo)
            {
                return false;
            }

            if (!SchemaObjectTypeHelper.Is(context.SchemaType, SchemaObjectNames.Tables))
            {
                return false;
            }

            var schemaName = row.GetSafeString("SchemaName");
            var schemaBrowser = row.GetSafeString("Schema_Browser");

            if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(schemaBrowser))
            {
                return false;
            }

            //顯示欄位資訊時，Table 底下的欄位列通常會有：
            //schemaName = TableName
            //schemaBrowser = ColumnName, DataType
            //所以用 ", " 判斷目前列是否為欄位資訊列
            return schemaBrowser.IndexOf(", ", StringComparison.Ordinal) >= 0;
        }

        private void FillTableNameForColumnInfoRightClick(SchemaExplorerRightClickContext context, DataRow row)
        {
            switch (context.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        context.SchemaNode = DatabaseSqlExecutor.DbUser;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        context.SchemaNode = row.GetSafeString("SchemaNode");
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        context.SchemaNode = row.GetSafeString("SchemaDbo");
                        context.SchemaDbo = row.GetSafeString("SchemaDbo");
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        context.SchemaNode = row.GetSafeString("SchemaNode");
                        break;
                    }
            }

            var tableName = row.GetSafeString("SchemaName");
            var separatorIndex = tableName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                tableName = tableName.Substring(0, separatorIndex);
            }

            if (context.SourceType == DataSourceType.SqlServer && !string.IsNullOrWhiteSpace(context.SchemaNode))
            {
                tableName = tableName.Replace($"{context.SchemaNode}.", string.Empty);
            }

            context.TableName = tableName;
        }

        private void ShowSchemaExplorerRightClickMenu(SchemaExplorerRightClickContext context)
        {
            if (context == null)
            {
                return;
            }

            if (TryShowUseDatabaseContextMenu(context))
            {
                return;
            }

            if (context.IsGroupRow && !SchemaExplorerGroupLevelPolicy.IsObjectGroupLevel(context.SourceType, context.GroupLevel))
            {
                return; //針對主要節點，例如 AliasName / Tables / Functions / Views / Triggers，右鍵不處理
            }

            if (string.IsNullOrWhiteSpace(context.SchemaName))
            {
                return;
            }

            if (context.IsColumnInfoRow) //表示「目前右鍵點到的這一列是 Table 的欄位資訊列」
            {
                ShowCopyOnlyContextMenu(context, true);
                return;
            }

            if (TryShowDatabaseObjectContextMenu(context))
            {
                return;
            }

            ShowCopyOnlyContextMenu(context, !context.IsGroupRow);
        }

        private bool TryShowUseDatabaseContextMenu(SchemaExplorerRightClickContext context)
        {
            if (!context.IsGroupRow || context.GroupLevel != 1)
            {
                return false;
            }

            switch (context.SourceType)
            {
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        if (string.IsNullOrWhiteSpace(context.SchemaName))
                        {
                            return false;
                        }

                        UIHelper.GenerateRightMenuForCopyOnly_SchemaBrowser
                        (
                            _schemaBrowserContextMenu,
                            c1GridSchemaBrowser,
                            context.TitleCopyToClipboard,
                            context.TitlePasteToQueryEditor,
                            $"USE {context.SchemaName};",
                            context.MouseX,
                            context.MouseY
                        );

                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryShowDatabaseObjectContextMenu(SchemaExplorerRightClickContext context)
        {
            var isViewOrTable = SchemaObjectTypeHelper.IsAny(context.SchemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views);

            if (!isViewOrTable)
            {
                return false;
            }

            switch (context.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        UIHelper.GenerateRightMenuForCopy_Oracle
                        (
                            true,
                            c1GridSchemaBrowser,
                            _schemaBrowserContextMenu,
                            editorSqlPane,
                            AccessibleDescription,
                            context.SchemaType,
                            context.TitleCopyToClipboard,
                            context.TitlePasteToQueryEditor,
                            context.SchemaName,
                            context.MouseX,
                            context.MouseY
                        );

                        return true;
                    }
                case DataSourceType.PostgreSql:
                    {
                        UIHelper.GenerateRightMenuForCopy_PostgreSql
                        (
                            true,
                            c1GridSchemaBrowser,
                            _schemaBrowserContextMenu,
                            editorSqlPane,
                            AccessibleDescription,
                            context.SchemaNode,
                            context.SchemaType,
                            context.TitleCopyToClipboard,
                            context.TitlePasteToQueryEditor,
                            context.SchemaName,
                            context.MouseX,
                            context.MouseY
                        );

                        return true;
                    }
                case DataSourceType.SqlServer:
                    {
                        UIHelper.GenerateRightMenuForCopy_SqlServer
                        (
                            true,
                            c1GridSchemaBrowser,
                            _schemaBrowserContextMenu,
                            editorSqlPane,
                            AccessibleDescription,
                            context.SchemaNode,
                            context.SchemaDbo,
                            context.SchemaType,
                            context.TitleCopyToClipboard,
                            context.TitlePasteToQueryEditor,
                            context.SchemaName,
                            context.MouseX,
                            context.MouseY,
                            context.ObjectId
                        );

                        return true;
                    }
                case DataSourceType.MySql:
                    {
                        UIHelper.GenerateRightMenuForCopy_MySql
                        (
                            true,
                            c1GridSchemaBrowser,
                            _schemaBrowserContextMenu,
                            editorSqlPane,
                            AccessibleDescription,
                            context.SchemaNode,
                            context.SchemaType,
                            context.TitleCopyToClipboard,
                            context.TitlePasteToQueryEditor,
                            context.SchemaName,
                            context.MouseX,
                            context.MouseY
                        );

                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private void ShowCopyOnlyContextMenu(SchemaExplorerRightClickContext context, bool isDataRowContext)
        {
            var schemaName = GetCopyOnlySchemaName(context);

            UIHelper.GenerateRightMenuForCopyOnly
            (
                true,
                _schemaBrowserContextMenu,
                c1GridSchemaBrowser,
                editorSqlPane,
                context.TitleCopyToClipboard,
                context.TitlePasteToQueryEditor,
                schemaName,
                context.MouseX,
                context.MouseY,
                isDataRowContext,
                context.SchemaNode,
                context.TableName,
                context.ColumnType,
                DatabaseSqlExecutor.DatabaseName,
                context.SchemaType,
                context.PackageSpecBody,
                context.SchemaDbo
            );
        }

        private string GetCopyOnlySchemaName(SchemaExplorerRightClickContext context)
        {
            var schemaName = context.SchemaName ?? string.Empty;

            if (context.SourceType == DataSourceType.PostgreSql &&
                SchemaObjectTypeHelper.StartsWithAny(context.SchemaType, SchemaObjectNames.Functions, SchemaObjectNames.Triggers))
            {
                var index = schemaName.IndexOf('(');

                if (index >= 0 && schemaName.IndexOf(')') >= 0)
                {
                    schemaName = schemaName.Substring(0, index);
                }
            }

            return schemaName;
        }

        private void c1GridSchemaBrowser_ColResize(object sender, ColResizeEventArgs e)
        {
            if (_isColumnAutoResizing)
            {
                return;
            }

            _isColumnResizing = true;
        }

        private void c1GridSchemaBrowser_Expand(object sender, BandEventArgs e)
        {
            if (_isExpandingOrCollapsing)
            {
                return;
            }

            AutoResizeGridColumnWidth();

            Thread.Sleep(50);

            tmrMouseDoubleClick.Enabled = true;
        }

        private void c1GridSchemaBrowser_FetchRowStyle(object sender, FetchRowStyleEventArgs e)
        {
            try
            {
                var data = c1GridSchemaBrowser.GetDataBoundItem(e.Row);
                var name = ((DataRowView)data).Row.GetSafeString("SchemaObject");

                if (name == DatabaseSqlExecutor.DbConnectionName)
                {
                    e.CellStyle.ForeColor = MyLibrary.IsDarkMode ? Color.Yellow : Color.Blue;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridSchemaBrowser_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    {
                        e.Handled = true;
                        CheckNodeThenCollapsedOrExpanded();
                        break;
                    }
                case Keys.Up:
                case Keys.Down:
                    {
                        e.Handled = true;
                        _userSelectedDisplayRowIndex = c1GridSchemaBrowser.Row;

                        if (e.KeyCode == Keys.Down)
                        {
                            _userSelectedDisplayRowIndex++;
                        }

                        if (e.KeyCode == Keys.Up)
                        {
                            _userSelectedDisplayRowIndex--;
                        }

                        DisplaySchemaInfo(_userSelectedDisplayRowIndex);
                        break;
                    }
            }
        }

        private void c1GridSchemaBrowser_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _isMouseDoubleClick = true;
            CheckNodeThenCollapsedOrExpanded();
        }

        private void c1GridSchemaBrowser_MouseDown(object sender, MouseEventArgs e)
        {
            var displayRowIndex = c1GridSchemaBrowser.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return; //Exclude row headers
            }

            if (e.Button != MouseButtons.Right)
            {
                return; //左鍵寫在 MouseUp()；如果寫在 MouseDown() 會有問題
            }

            _schemaBrowserContextMenu = new ContextMenuStrip();
            c1GridSchemaBrowser.ContextMenuStrip = _schemaBrowserContextMenu;

            try
            {
                c1GridSchemaBrowser.Row = displayRowIndex;
            }
            catch
            {
                //忽略 C1TrueDBGrid 在特殊列或資料重繫結期間可能發生的 Row 設定失敗
            }

            var selection = ResolveSchemaExplorerSelection(displayRowIndex);
            var context = CreateSchemaExplorerRightClickContext(selection, displayRowIndex, e.X, e.Y);

            if (context == null)
            {
                return;
            }

            ShowSchemaExplorerRightClickMenu(context);
        }

        private void c1GridSchemaBrowser_MouseUp(object sender, MouseEventArgs e)
        {
            _userSelectedDisplayRowIndex = c1GridSchemaBrowser.RowContaining(e.Y);

            if (_userSelectedDisplayRowIndex == -1) //Exclude row headers
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            DisplaySchemaInfo2();
        }

        private void CheckNodeThenCollapsedOrExpanded()
        {
            var currentRow = c1GridSchemaBrowser.Row;

            if (_isColumnResizing)
            {
                _isColumnResizing = false;
                return;
            }

            switch (c1GridSchemaBrowser.Splits[0].Rows[currentRow].RowType)
            {
                //判斷是不是節點
                case RowTypeEnum.CollapsedGroupRow:
                    {
                        c1GridSchemaBrowser.ExpandGroupRow(currentRow);
                        AutoResizeGridColumnWidth();
                        break;
                    }
                case RowTypeEnum.ExpandedGroupRow:
                    {
                        c1GridSchemaBrowser.CollapseGroupRow(currentRow);
                        break;
                    }
            }
        }

        private void tmrMouseDoubleClick_Tick(object sender, EventArgs e)
        {
            tmrMouseDoubleClick.Enabled = false;

            if (!_isMouseDoubleClick)
            {
                return; //如果是 Form_Load 或是 MouseDoubleClick 事件，不處理
            }

            _isMouseDoubleClick = false;
        }

        private void mnuExpandAll_Click(object sender, EventArgs e)
        {
            ExpandAll();
        }

        private void mnuCollapseAll_Click(object sender, EventArgs e)
        {
            CollapseAll();
        }

        private async void ExpandAll()
        {
            Application.UseWaitCursor = true;

            await Task.Run
            (() =>
            {
                for (var i = 0; i < c1GridSchemaBrowser.Splits[0].Rows.Count; i++)
                {
                    if (c1GridSchemaBrowser.Splits[0].Rows[i].RowType != RowTypeEnum.DataRow)
                    {
                        c1GridSchemaBrowser.ExpandGroupRow(i);
                    }

                    Application.DoEvents();
                }
            });

            c1GridSchemaBrowser.Focus();
            c1GridSchemaBrowser.Cursor = Cursors.Default;
            Application.UseWaitCursor = false;
        }

        private async void CollapseAll()
        {
            Application.UseWaitCursor = true;

            await Task.Run
            (() =>
            {
                for (var i = 0; i < c1GridSchemaBrowser.Splits[0].Rows.Count; i++)
                {
                    if (c1GridSchemaBrowser.Splits[0].Rows[i].RowType != RowTypeEnum.DataRow)
                    {
                        c1GridSchemaBrowser.CollapseGroupRow(i);
                    }
                }
            });

            c1GridSchemaBrowser.Focus();
            c1GridSchemaBrowser.Cursor = Cursors.Default;
            Application.UseWaitCursor = false;

            //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
            //參考 MyGlobal.UpdateSchemaData()
            c1GridSchemaBrowser.ExpandGroupRow(0); //展開 0 的結點
        }

        private void QuerySchema()
        {
            var filter = string.Empty;
            var dtSchema = c1GridSchemaBrowser.GetDataTableSourceOrNull();

            if (dtSchema == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSchemaFilter.Text))
            {
                txtSchemaFilter.Text = @"*";
            }

            if (!txtSchemaFilter.Text.EndsWith("*", StringComparison.Ordinal))
            {
                txtSchemaFilter.Text += @"*";
            }

            var schemaFilter = NormalizeSchemaFilterText(txtSchemaFilter.Text);

            try
            {
                //20230810 修正：當使用者在搜尋字串中間有用到 % 符號時，變更 sFilter 寫法 (Wildcard characters are not allowed in the middle of a string. For example, 'te*xt' is not allowed.)
                if (MyGlobal.IsShowColumnInfo) //表示「目前有開啟欄位資訊顯示」
                {
                    if (schemaFilter.StartsWith("%", StringComparison.Ordinal) && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 3)
                    {
                        var schemaName = string.Empty;
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (i == 0)
                            {
                                schemaName = $"SchemaName LIKE '%{parts[i]}%'";
                                schemaBrowser = $"Schema_Browser LIKE '%{parts[i]}%'";
                            }
                            else
                            {
                                schemaName += $" AND SchemaName LIKE '%{parts[i]}%'";
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[i]}%'";
                            }
                        }

                        filter = $"({schemaName}) OR ({schemaBrowser})";
                    }
                    else if (!schemaFilter.StartsWith("%", StringComparison.Ordinal) && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 2)
                    {
                        var schemaName = string.Empty;
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (i == 0)
                            {
                                schemaName = $"SchemaName LIKE '{parts[i]}%'";
                                schemaBrowser = $"Schema_Browser LIKE '{parts[i]}%'";
                            }
                            else
                            {
                                schemaName += $" AND SchemaName LIKE '%{parts[i]}%'";
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[i]}%'";
                            }
                        }

                        filter = $"({schemaName}) OR ({schemaBrowser})";
                    }
                    else
                    {
                        filter = $"(SchemaName LIKE '{schemaFilter}' OR Schema_Browser LIKE '{schemaFilter}')";
                    }
                }
                else
                {
                    if (schemaFilter.StartsWith("%", StringComparison.Ordinal) && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 3)
                    {
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (i == 0)
                            {
                                schemaBrowser = $"Schema_Browser LIKE '%{parts[i]}%'";
                            }
                            else
                            {
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[i]}%'";
                            }
                        }

                        filter = schemaBrowser;
                    }
                    else if (!schemaFilter.StartsWith("%", StringComparison.Ordinal) && schemaFilter.Length - schemaFilter.Replace("%", string.Empty).Length >= 2)
                    {
                        var schemaBrowser = string.Empty;
                        var parts = schemaFilter.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                        for (var i = 0; i < parts.Length; i++)
                        {
                            if (i == 0)
                            {
                                schemaBrowser = $"Schema_Browser LIKE '{parts[i]}%'";
                            }
                            else
                            {
                                schemaBrowser += $" AND Schema_Browser LIKE '%{parts[i]}%'";
                            }
                        }

                        filter = schemaBrowser;
                    }
                    else
                    {
                        filter = $"Schema_Browser LIKE '{schemaFilter}'";
                    }
                }

                dtSchema.DefaultView.RowFilter = filter;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
                if (c1GridSchemaBrowser.Splits.Count > 0 && c1GridSchemaBrowser.Splits[0].Rows.Count > 0)
                {
                    c1GridSchemaBrowser.ExpandGroupRow(0);
                }

                c1GridSchemaBrowser.Focus();
            }
        }

        private static string NormalizeSchemaFilterText(string text)
        {
            var value = (text ?? string.Empty).Trim()
                                              .Replace("%", "*")
                                              .Replace("'", "''");

            while (value.Contains("**"))
            {
                value = value.Replace("**", "*");
            }

            return value.Replace("*", "%");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSchema();

            MyGlobal.GlobalTemp4 = string.Empty;

            txtSchemaFilter.Text = "*";
            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblFilter.Left - lblFilter.Width - 1, 21);
            AutoResizeGridColumnWidth();

            //傳遞資訊至 MainForm，更新其他 QueryForm 的 Schema 資訊
            TransferValueToMainForm($"UpdateSchemaBrowserInformationRefreshClick`{AccessibleDescription}");
        }

        private void RefreshSchema()
        {
            if (_modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0)
            {
                var result = MessageBox.Show(_confirmExitTableEditMessage, tabData.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes: //使用者選擇「放棄修改」
                        {
                            _modifiedCells.Clear();
                            _deletedRows.Clear();
                            _newRows.Clear();
                            _searchResultCells.Clear();
                            break;
                        }
                    case DialogResult.No: //使用者選擇「保留修改」
                        {
                            return;
                        }
                }
            }

            tabTableStructure.TabVisible = false;
            tabView100RowsTop.TabVisible = false;
            tabData.TabVisible = false;
            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;
            editorSqlPane.ReadOnly = false;
            editorSqlPane.Text = string.Empty;
            editorSqlPane.ReadOnly = true;
            c1GridSchemaBrowser.Focus();

            //20240714 Refresh，避免影像殘留
            tabSchemaBrowser.Refresh();

            Cursor = Cursors.WaitCursor;

            var form = new MessageForm();
            var message = string.Empty;

            if (MyGlobal.IsShowColumnInfo)
            {
                message = LocalizationHelper.GetLanguageString("Getting schema information (include all column information of Tables) ...", "Global", "Global", "msg", "GetSchemaInfoIncludeColumn", "Text");
            }
            else
            {
                message = LocalizationHelper.GetLanguageString("Getting schema information...", "Global", "Global", "msg", "GetSchemaInfo", "Text");
            }

            form.Caption = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");
            form.TopLevel = true; //20250118 改用 TopLevel，只在 JasonQuery 最上層顯示
            form.Info = message;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.IsNeedToMovePosition = true; //20231111 加入此變數，MessageForm 顯示於螢幕中央時，視窗的位置再往上調整一些，如果有錯誤發生時，MessageBox 才不會剛好擋住 MessageForm！
            form.Show();
            form.Refresh();

            ConnectToDatabase();

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        DatabaseSqlExecutor.UpdateSchemaData_Oracle(c1GridSchemaBrowser, true);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        DatabaseSqlExecutor.UpdateSchemaData_PostgreSql(c1GridSchemaBrowser, true, true);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        DatabaseSqlExecutor.UpdateSchemaData_SqlServer(c1GridSchemaBrowser, true, true);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        DatabaseSqlExecutor.UpdateSchemaData_MySql(c1GridSchemaBrowser, true);
                        break;
                    }
            }

            txtSchemaFilter.Text = "*";
            form.Dispose();
            Cursor = Cursors.Default;
        }
    }
}