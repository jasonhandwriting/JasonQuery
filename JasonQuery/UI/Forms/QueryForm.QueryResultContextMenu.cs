using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.Display.Columns;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool _isGridContextMenuBuilt;

        private void EnsureGridContextMenusCreated()
        {
            if (_isGridContextMenuBuilt)
            {
                return;
            }

            if (_gridContextMenu == null)
            {
                _gridContextMenu = new ContextMenuStrip();
            }
            else
            {
                _gridContextMenu.Items.Clear();
            }

            if (_gridHeaderContextMenu == null)
            {
                _gridHeaderContextMenu = new ContextMenuStrip();
            }
            else
            {
                _gridHeaderContextMenu.Items.Clear();
            }

            BuildGridHeaderContextMenu();
            BuildGridContextMenu();

            _isGridContextMenuBuilt = true;
        }

        private void BuildGridHeaderContextMenu()
        {
            _gridHeaderContextMenu.Items.Add(string.Empty);
            _gridHeaderContextMenu.Items.Add("-");
            _gridHeaderContextMenu.Items.Add(string.Empty);
            _gridHeaderContextMenu.Items.Add("-");
            _gridHeaderContextMenu.Items.Add(string.Empty);
            _gridHeaderContextMenu.Items.Add(string.Empty);

            _gridHeaderContextMenu.Items[2].Click += (sender, e) =>
            {
                TextHelper.CopyTextToClipboard(TextHelper.GetSafeString(_gridHeaderContextMenu.Items[2].Tag), "ApplyGridMenu(HeaderCopyColumnName)");
            };

            _gridHeaderContextMenu.Items[4].Click += (sender, e) =>
            {
                int.TryParse(TextHelper.GetSafeString(_gridHeaderContextMenu.Items[0].Tag), out var columnIndex);

                SortDataGrid(columnIndex);
            };

            _gridHeaderContextMenu.Items[5].Click += (sender, e) =>
            {
                int.TryParse(TextHelper.GetSafeString(_gridHeaderContextMenu.Items[0].Tag), out var columnIndex);

                SortDataGrid(columnIndex, true);
            };
        }

        private void BuildGridContextMenu()
        {
            _gridContextMenu.Items.Add(string.Empty); //ShowSqlStatement
            _gridContextMenu.Items.Add(string.Empty); //CellViewer
            _gridContextMenu.Items.Add(string.Empty); //SingleRecordViewer
            _gridContextMenu.Items.Add(string.Empty); //ShowColumns
            _gridContextMenu.Items.Add("-");          //Dash0
            _gridContextMenu.Items.Add(string.Empty); //SelectAll
            _gridContextMenu.Items.Add("-");          //Dash1
            _gridContextMenu.Items.Add(string.Empty); //ZoomInGrid
            _gridContextMenu.Items.Add(string.Empty); //ZoomOutGrid
            _gridContextMenu.Items.Add("-");          //Dash2
            _gridContextMenu.Items.Add(string.Empty); //ExportAllDataToFile
            _gridContextMenu.Items.Add(string.Empty); //ExportAllDataToFileScript
            _gridContextMenu.Items.Add("-");          //Dash3
            _gridContextMenu.Items.Add(string.Empty); //CopyAllDataToClipboard
            _gridContextMenu.Items.Add(string.Empty); //CopyAllDataToClipboardCurrentRow
            _gridContextMenu.Items.Add("-");          //Dash7
            _gridContextMenu.Items.Add(string.Empty); //Copy
            _gridContextMenu.Items.Add(string.Empty); //CopyAsQueryCondition
            _gridContextMenu.Items.Add(string.Empty); //CopyWithColumnNames
            _gridContextMenu.Items.Add(string.Empty); //CopyColumnNames
            _gridContextMenu.Items.Add("-");          //Dash8
            _gridContextMenu.Items.Add(string.Empty); //FreezeColumn
            _gridContextMenu.Items.Add(string.Empty); //UnfreezeColumn

            ApplyGridContextMenuImages();
            BindGridContextMenuEvents();

            GetGridMenuItem(GridColumn.Copy).ShortcutKeys = Keys.Control | Keys.C;

            //初始值為 false: 使用過 Frozen 後再啟用
            _gridContextMenu.Items[GridColumn.UnfreezeColumn].Enabled = false;
        }

        private void ApplyGridContextMenuImages()
        {
            SetGridMenuImage(GridColumn.ShowSqlStatement, "Show SQL Statement 16x16.ico");
            SetGridMenuImage(GridColumn.CellViewer, "CellViewer 16x16.ico");
            SetGridMenuImage(GridColumn.SingleRecordViewer, "Literature 16x16.ico");
            SetGridMenuImage(GridColumn.ShowColumns, "Show Column 16x16.ico");
            SetGridMenuImage(GridColumn.SelectAll, "Select All 16x16.ico");
            SetGridMenuImage(GridColumn.ZoomInGrid, "Zoom In 16x16.ico");
            SetGridMenuImage(GridColumn.ZoomOutGrid, "Zoom Out 16x16.ico");
            SetGridMenuImage(GridColumn.ExportAllDataToFile, "Export 16x16.ico");
            SetGridMenuImage(GridColumn.CopyAllDataToClipboard, "Copy2Script 16x16.ico");
            SetGridMenuImage(GridColumn.CopyAllDataToClipboardCurrentRow, "Copy2Script Current 16x16.ico");
            SetGridMenuImage(GridColumn.Copy, "Copy 16x16.ico");
            SetGridMenuImage(GridColumn.CopyAsQueryCondition, "CopyIn 16x16.ico");
            SetGridMenuImage(GridColumn.FreezeColumn, "Freeze 16x16.ico");
            SetGridMenuImage(GridColumn.UnfreezeColumn, "Unfreeze 16x16.ico");
        }

        private void SetGridMenuImage(int menuIndex, string iconName)
        {
            _gridContextMenu.Items[menuIndex].Image = IconManager.GetImage(MyGlobal.IconLibrary, iconName);
        }

        private void BindGridContextMenuEvents()
        {
            _gridContextMenu.Items[GridColumn.ShowSqlStatement].Click += (sender, e) => ShowSqlStatement();
            _gridContextMenu.Items[GridColumn.CellViewer].Click += (sender, e) => CellViewer();
            _gridContextMenu.Items[GridColumn.SingleRecordViewer].Click += (sender, e) => SingleRecordViewer();

            _gridContextMenu.Items[GridColumn.ShowColumns].Click += (sender, e) =>
            {
                splitContainer4.Panel1Collapsed = !splitContainer4.Panel1Collapsed;
            };

            _gridContextMenu.Items[GridColumn.SelectAll].Click += (sender, e) =>
            {
                c1TrueDBGrid1.SelectedRows.Clear();

                for (var i = 0; i < c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count; i++)
                {
                    c1TrueDBGrid1.SelectedRows.Add(i);
                }
            };

            _gridContextMenu.Items[GridColumn.ZoomInGrid].Click += (sender, e) => ZoomObject("ZoomInGrid");
            _gridContextMenu.Items[GridColumn.ZoomOutGrid].Click += (sender, e) => ZoomObject("ZoomOutGrid");
            _gridContextMenu.Items[GridColumn.ExportAllDataToFile].Click += (sender, e) => ExportToFile();

            BindGridExportScriptMenu();
            BindGridCopyScriptMenu();
            BindGridCopyCurrentRowScriptMenu();

            BindGridCopyMenuClick(GridColumn.Copy, QueryResultCopyMode.Copy);
            BindGridCopyMenuClick(GridColumn.CopyAsQueryCondition, QueryResultCopyMode.CopyAsQueryCondition);
            BindGridCopyMenuClick(GridColumn.CopyWithColumnNames, QueryResultCopyMode.CopyWithColumnNames);
            BindGridCopyMenuClick(GridColumn.CopyColumnNames, QueryResultCopyMode.CopyColumnNames);

            _gridContextMenu.Items[GridColumn.FreezeColumn].Click += (sender, e) => FrozenColumn();
            _gridContextMenu.Items[GridColumn.UnfreezeColumn].Click += (sender, e) => FrozenColumn(false);
        }

        private void BindGridExportScriptMenu()
        {
            var menuItem = GetGridMenuItem(GridColumn.ExportAllDataToFileScript);

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[0].Click += (sender, e) =>
            {
                ArrangeDataForAllData("ExportAllDataToFileScript", true);
            };

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[1].Click += (sender, e) =>
            {
                ArrangeDataForAllData("ExportAllDataToFileScript");
            };
        }

        private void BindGridCopyScriptMenu()
        {
            var menuItem = GetGridMenuItem(GridColumn.CopyAllDataToClipboard);

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[0].Click += (sender, e) =>
            {
                ArrangeDataForAllData("CopyAllDataToClipboard", true);
            };

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[1].Click += (sender, e) =>
            {
                ArrangeDataForAllData("CopyAllDataToClipboard");
            };
        }

        private void BindGridCopyCurrentRowScriptMenu()
        {
            var menuItem = GetGridMenuItem(GridColumn.CopyAllDataToClipboardCurrentRow);

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[0].Click += (sender, e) =>
            {
                ArrangeDataForAllData("CopyAllDataToClipboardCurrentRow", true);
            };

            menuItem.DropDownItems.Add(string.Empty);
            menuItem.DropDownItems[1].Click += (sender, e) =>
            {
                ArrangeDataForAllData("CopyAllDataToClipboardCurrentRow");
            };
        }

        private void BindGridCopyMenuClick(int menuIndex, QueryResultCopyMode copyMode)
        {
            _gridContextMenu.Items[menuIndex].Click += (sender, e) => ArrangeData(copyMode);
        }

        private void ApplyGridContextMenuLocalization()
        {
            ApplyGridHeaderContextMenuLocalization();
            ApplyGridBodyContextMenuLocalization();
            ApplyGridDateTimeConversionDropDownLocalization();
        }

        private void ApplyGridHeaderContextMenuLocalization()
        {
            _gridHeaderContextMenu.Items[0].Text = string.Empty;
            _gridHeaderContextMenu.Items[1].Text = "-";
            _gridHeaderContextMenu.Items[2].Text = GetGridMenuText("Copy Column Name", "CopyColumnName");
            _gridHeaderContextMenu.Items[3].Text = "-";
            _gridHeaderContextMenu.Items[4].Text = GetGridMenuText("Sort", "Sort");
            _gridHeaderContextMenu.Items[5].Text = GetGridMenuText("Sort by NUMBER", "SortByNumber");
        }

        private void ApplyGridBodyContextMenuLocalization()
        {
            SetGridMenuText(GridColumn.ShowSqlStatement, "SQL Statement Viewer", "ShowSqlStatement");
            SetGridMenuText(GridColumn.CellViewer, "Cell Viewer", "CellViewer");
            SetGridMenuText(GridColumn.SingleRecordViewer, "Single Record Viewer", "SingleRecordViewer");
            SetGridMenuText(GridColumn.ShowColumns, "Show/Hide Columns", "ShowColumns");
            SetGridMenuText(GridColumn.Dash0, "-", string.Empty);
            SetGridMenuText(GridColumn.SelectAll, "Select All", "SelectAll");
            SetGridMenuText(GridColumn.Dash1, "-", string.Empty);
            SetGridMenuText(GridColumn.ZoomInGrid, "Zoom In (Ctrl+Mouse Wheel Up)", "ZoomInGrid");
            SetGridMenuText(GridColumn.ZoomOutGrid, "Zoom In (Ctrl+Mouse Wheel Down)", "ZoomOutGrid");
            SetGridMenuText(GridColumn.Dash2, "-", string.Empty);
            SetGridMenuText(GridColumn.ExportAllDataToFile, "Export All Data to File (Excel/CSV...)", "ExportAllDataToFile");
            SetGridMenuText(GridColumn.ExportAllDataToFileScript, "Export All Data to File (as \"Insert Into\" Script)", "ExportAllDataToFileScript");
            SetGridMenuText(GridColumn.Dash3, "-", string.Empty);
            SetGridMenuText(GridColumn.CopyAllDataToClipboard, "Export All Data to Clipboard (as \"Insert Into\" Script)", "CopyAllDataToClipboard");
            SetGridMenuText(GridColumn.CopyAllDataToClipboardCurrentRow, "Export Current Row Data to Clipboard (as \"Insert Into\" Script)", "CopyAllDataToClipboardCurrentRow");
            SetGridMenuText(GridColumn.Dash7, "-", string.Empty);
            SetGridMenuText(GridColumn.Copy, "Copy", "Copy");
            SetGridMenuText(GridColumn.CopyAsQueryCondition, "Copy as Query Condition", "CopyAsQueryCondition");
            SetGridMenuText(GridColumn.CopyWithColumnNames, "Copy with Column Name(s)", "CopyWithColumnName");
            SetGridMenuText(GridColumn.CopyColumnNames, "Copy Column Name(s)", "CopyColumnName");
            SetGridMenuText(GridColumn.Dash8, "-", string.Empty);
            SetGridMenuText(GridColumn.FreezeColumn, "Freeze Column", "FreezeColumn");
            SetGridMenuText(GridColumn.UnfreezeColumn, "Unfreeze Column", "UnfreezeColumn");
        }

        private void ApplyGridDateTimeConversionDropDownLocalization()
        {
            SetGridDateTimeConversionDropDownText(GridColumn.ExportAllDataToFileScript);
            SetGridDateTimeConversionDropDownText(GridColumn.CopyAllDataToClipboard);
            SetGridDateTimeConversionDropDownText(GridColumn.CopyAllDataToClipboardCurrentRow);
        }

        private void SetGridDateTimeConversionDropDownText(int menuIndex)
        {
            var menuItem = GetGridMenuItem(menuIndex);

            menuItem.DropDownItems[0].Text = GetGridMessageText("DateTime field(s) will be converted using TO_DATE", "DateTime2ToDate");
            menuItem.DropDownItems[1].Text = GetGridMessageText("DateTime field(s) will be converted to STRING", "DateTime2String");
        }

        private void SetGridMenuText(int menuIndex, string defaultText, string key)
        {
            _gridContextMenu.Items[menuIndex].Text = string.IsNullOrEmpty(key) ? defaultText : GetGridMenuText(defaultText, key);
        }

        private string GetGridMenuText(string defaultText, string key)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "menugrid", key, "Text");
        }

        private string GetGridMessageText(string defaultText, string key)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", key, "Text");
        }

        private ToolStripMenuItem GetGridMenuItem(int menuIndex)
        {
            return (ToolStripMenuItem)_gridContextMenu.Items[menuIndex];
        }
    }
}