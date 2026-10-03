using C1.Win.C1Input;
using C1.Win.C1Themes;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Events;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.UI.Controls;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Schema;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Editor.FindAndReplace;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.Services;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static JasonQuery.UI.Forms.CellEditorForm;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm : Form
    {
        public event ValueUpdatedEventHandler ValueUpdated;

        private ContextMenuStrip _nullContextMenu = new ContextMenuStrip(); //20240821 通用型 null 右鍵選單 (不出現選單)
        private ContextMenuStrip _sqlPaneContextMenu = new ContextMenuStrip(); //Editor's menu
        private ContextMenuStrip _sqlPreviewContextMenu = new ContextMenuStrip(); //Editor's menu
        private ContextMenuStrip _gridContextMenu = new ContextMenuStrip(); //C1TrueDBGrid's menu
        private ContextMenuStrip _schemaBrowserContextMenu = new ContextMenuStrip(); //SchemaBrowser Grid 右鍵選單
        private bool _isFormLoadFinished = false; //表單是否載入完畢 (避免觸發事件)
        private bool _isColumnResizing; //是否正在執行 c1TrueDBGrid1_ColResize 事件
        private bool _isColumnAutoResizing; //是否正在「自動調整欄寬」？
        private bool _isMouseDoubleClick = true; //是否由 MouseDoubleClick 觸發？初始值要設為 true，因為 Form_Load 會觸發一次
        private bool _isExpandingOrCollapsing; //是否正在執行 Expand / Collapse ?
        private bool _shouldSaveSplitter; //判斷是否需要 SaveSplitter 寬／高度值
        private string _languageText = string.Empty;
        private string _confirmExitTableEditMessage = string.Empty;
        private bool _isDataGridFilterBarEditing;

        private const int ConstraintIconColumnIndex = 0;
        private const string ConstraintInfoColumnName = "ConstraintInfo";

        private FindAndReplace _findAndReplace; //20230629 改寫搜尋、取代功能

        //20260322 表單層級的全域變數 (Grid Style/Export 皆可使用)
        private ColumnInfoCollector _columnInfoCollector;

        //20240911 使用者透過 CellEditor 變更後的新內容
        private string _cellEditorResult = null;

        //20240531 判斷並標記需要變更 BackColor 的儲存格位置集合
        private List<Point> _modifiedCells = new List<Point>(); //有異動過資料的儲存格位置
        private List<Point> _deletedRows = new List<Point>(); //被刪除的列 (整列的儲存格位置)
        private List<Point> _newRows = new List<Point>(); //插入列或複製列 (整列的儲存格位置)

        //20240531 使用者搜尋的關鍵字匹配儲存格
        private List<Point> _searchResultCells = new List<Point>();

        //20240531 原查詢結果最後插入一個欄位，欄位名稱以變數控制 (避免使用者使用了相同的欄位名稱，引發例外錯誤)
        private string _identifyColumnName = "JQ1231_I_D_E_N_T_1_F_Y_1321QJ";

        private Point _currentCellPosition = new Point(-1, -1); //20240531 for 提示用：顯示異動前後值
        private string _currentModifiedCellTipText = string.Empty; //20260608
        private string _selectedTableName = string.Empty; //使用者選定了哪一個 Table

        private DataTable _dtTableData; //選擇指定 Table，將它的內容儲存至此 DataTable
        private DataTable _dtOriginalTableData; //選擇指定 Table，將它的原始內容儲存至此 DataTable (供後續比對用的)
        private DataTable _dtRawSchemaTable; //選擇指定 Table，將它的 Schema 結構儲存至此 DataTable
        private DataTable _dtStructuredSchemaTable; //整理過的 Table Schema 訊息，綁定到 c1GridStructure
        private DataTable _dtSqlServerPrimaryKeyTable; //20240818 針對 SQL Server，記錄當次選擇的 Table 的主鍵
        private DataTable _dtMySqlPrimaryKeyTable; //20240820 針對 MySQL，記錄當次選擇的 Table 的主鍵

        private string _sqlPreviewHint1 = string.Empty;
        private string _sqlPreviewHint2 = string.Empty;

        private ToolTip _toolTip1 = new ToolTip();
        private string _panelColorSelectedName = string.Empty;
        private List<Control> _colorPanelControls;

        //20240531 編輯模式相關變數
        private int _editingRowIndex = -1;
        private int _editingColumnIndex = -1;
        private bool _isInEditMode = false;

        //20240714 使用者選中的物件行索引 (Row Index)
        private int _userSelectedDisplayRowIndex = 0;

        private string _currentOperationObjectName = string.Empty;

        //20250708 記住雙擊 (Double Click) 取得的字串內容
        private string _selectedTextOnDoubleClickSqlPane = string.Empty;
        private string _selectedTextOnDoubleClickSqlPreview = string.Empty;

        //20260704 重構「雙擊 (Double Click) 字串內容的寫法」
        private bool _isSqlPaneHighlightSelectionCopyMode;
        private bool _isSqlPreviewHighlightSelectionCopyMode;

        public int DisplayRowIndex { get; set; } = 0;
        public string SchemaNode { get; set; }
        public string SchemaType { get; set; }
        public string SchemaName { get; set; }
        public string SchemaDbo { get; set; }
        public string ObjectId { get; set; }
        public string AccessibleDescriptionString { get; set; }

        private Dictionary<string, C1TrueDBGrid> _gridMap;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(IntPtr classname, string title);

        //尋找並關閉 MessageBox
        #region
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_CLOSE = 0x10;

        [DllImport("user32.dll")]
        private static extern void MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool rePaint);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle rect);
        #endregion

        private const string _dateTimeFormat = "yyyyMMddHHmmssfff";
        private HashSet<string> _operationModeNewClone = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "NEW", "CLONE" };

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public SchemaBrowserForm()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;

            _gridMap = new Dictionary<string, C1TrueDBGrid>
            {
                { "tabTableStructure", c1GridStructure },
                { "tabView100RowsTop", c1Grid100RowsTop },
                { "tabData", c1GridData }
            };
        }

        private void ShowExceptionMessage(Exception ex)
        {
            ExceptionDialogService.Show(this, ex);
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                //20260726 統一圖示風格
                btnSelectAllSqlPane.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnSelectAllTableStructure.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnSelectAllTop100.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnSelectAllData.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnSelectAllSqlPreview.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

                if (DisplayRowIndex != -1)
                {
                    //載入左右分割的百分比
                    LoadSplitterData("LL/RR");
                }

                //篩選功能
                c1GridData.AllowFilter = false;
                c1GridData.Filter += c1GridData_Filter;

                tabSqlPreview.TabForeColor = Color.DarkRed;
                tabSqlPreview.TabBackColor = Color.LightGoldenrodYellow;
                tabSqlPreview.TabForeColorSelected = Color.DarkRed;
                tabSqlPreview.TabBackColorSelected = Color.LightGoldenrodYellow;

                //20240530 預設不顯示左側「指定 Table 欄位的Grid」
                splitContainer3.Panel1Collapsed = true;

                //強制改為 3 (預設會強制等於 4)
                splitContainer1.SplitterWidth = 3;
                splitContainer3.SplitterWidth = 3;

                //no delay for showing, very long time to hide
                c1SuperTooltip1.AutomaticDelay = 0;
                c1SuperTooltip1.AutoPopDelay = int.MaxValue;
                c1SuperTooltip1.IsBalloon = true;

                //20240525 隱藏左半側的上半部，跨 Schema 先不處理！
                splitContainer2.Panel1Collapsed = true; //左半側全部隱藏

                btnFilterData.Tag = string.Empty;
                lblFindData.Tag = string.Empty;

                //20240413 以下這幾個物件，每次在開發介面開啟時，它們的高度會一直增加，故改成用程式碼自動調整高度
                editorSqlPane.Size = new Size(editorSqlPane.Width, tabSchemaBrowser.Height - tsSqlPane.Height - 29);
                c1GridStructure.Size = new Size(c1GridStructure.Width, tabSchemaBrowser.Height - tsTableStructure.Height - 29);
                c1Grid100RowsTop.Size = new Size(c1Grid100RowsTop.Width, tabSchemaBrowser.Height - tsView100RowsTop.Height - 29);

                //20231105 由 QueryEditor 所開啟的 SchemaBrowser，AccessibleDescription 會是 null
                if (string.IsNullOrEmpty(AccessibleDescription))
                {
                    AccessibleDescription = DateTime.Now.ToString(_dateTimeFormat);
                }

                _findAndReplace = new FindAndReplace();
                _findAndReplace.Scintilla = editorSqlPane;

                UIHelper.SetDockingTabColor(tabSchemaBrowser, ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor), ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor), ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor));

                if (MyLibrary.IsDarkMode)
                {
                    c1ThemeController1.SetTheme(tabSchemaBrowser, "VS2013Dark");
                }

                _colorPanelControls = new List<Control>
                {
                    pnlNewRowForeColor,
                    pnlNewRowBackColor,
                    pnlDeletedRowForeColor,
                    pnlDeletedRowBackColor,
                    pnlChangedCellForeColor,
                    pnlChangedCellBackColor
                };

                pnlNewRowForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorNewRowForeColor);
                pnlNewRowBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorNewRowBackColor);
                pnlDeletedRowForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorDeletedRowForeColor);
                pnlDeletedRowBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorDeletedRowBackColor);
                pnlChangedCellForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorChangedCellForeColor);
                pnlChangedCellBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorChangedCellBackColor);

                ApplyLocalizationSetting();
                ApplyGridInteractionDirection();

                var confirmExitTableEdit = LocalizationHelper.GetLanguageString("The data has been changed.\r\n\r\nDo you want to discard all changes?", "form", GetType().Name, "msg", "ConfirmExitTableEdit", "Text");
                var space35 = new string(' ', 35);

                _confirmExitTableEditMessage = $"{confirmExitTableEdit}{space35}"; //後面加上空白，避免資料表名稱較長，顯示不完全
                lblFilter.Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);

                btnCommitData.Enabled = AppConfigHelper.IsNotCommitYet;
                btnRollbackData.Enabled = AppConfigHelper.IsNotCommitYet;

                if (DisplayRowIndex == -1)
                {
                    splitContainer1.Panel1Collapsed = true; //左半側全部隱藏
                    DisplaySchemaInfo(DisplayRowIndex); //單獨開啟 SchemaBrowser

                    var tableNamePrefix = lblTableOrViewName.Text.EndsWith(":", StringComparison.Ordinal) ? $"{lblTableOrViewName.Text} " : lblTableOrViewName.Text; //判斷冒號後面是否要帶一個空白 (減少壓迫感)

                    Text += $" - {tableNamePrefix}{lblTableName02.Text}";
                }
                else
                {
                    //載入左右分割的百分比
                    LoadSplitterData("L/R");

                    if (AppConfigHelper.HasMultiOpenSchemaBrowser)
                    {
                        GridHelper.UpdateSchemaData(c1GridSchemaBrowser);
                    }
                    else
                    {
                        RefreshSchema();
                    }
                }

                var iconSuffix = (MyLibrary.CommitRollbackIcon >= 2 && MyLibrary.CommitRollbackIcon <= 6) ? $"{MyLibrary.CommitRollbackIcon} " : string.Empty;
                var commitIconName = $"Commit {iconSuffix}16x16.ico";
                var rollbackIconName = $"Rollback {iconSuffix}16x16.ico";

                btnCommitData.Image = IconManager.GetImage(MyGlobal.IconLibrary, commitIconName);
                btnRollbackData.Image = IconManager.GetImage(MyGlobal.IconLibrary, rollbackIconName);
                btnCommitSqlPreview.Image = IconManager.GetImage(MyGlobal.IconLibrary, commitIconName);
                btnRollbackSqlPreview.Image = IconManager.GetImage(MyGlobal.IconLibrary, rollbackIconName);

                //20250909 圖示的外觀品質會莫名變差，故此處重新載入
                btnFindNextData.Image = IconManager.GetImage(MyGlobal.IconLibrary, "FindNext 16x16.ico");
                btnFindPreviousData.Image = IconManager.GetImage(MyGlobal.IconLibrary, "FindPrevious 16x16.ico");
                btnHighlightData.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Highlight 16x16.ico");


                _isFormLoadFinished = true;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DisplayRowIndex == -1 && (_modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0))
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
                            e.Cancel = true;
                            break;
                        }
                }
            }
        }

        private void ApplyLocalizationSetting()
        {
            Cursor = Cursors.WaitCursor;

            //以下顏色要重新指定
            grpEditedColors.BackColor = Color.Transparent;
            tsSqlPane.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsTableStructure.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsView100RowsTop.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsData.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsColumnFilter.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsSqlPreview.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            chkCopyAsHTML.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            chkShowFilterRowData.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsSchemaBrowser.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            btnExpandCollapse.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            lblFilter.BackColor = tsSchemaBrowser.BackColor;

            LocalizationHelper.ApplyLanguageInfo(this);

            _sqlPreviewHint1 = LocalizationHelper.GetLanguageString("-- This change script was automatically generated by JasonQuery. After clicking the \"Apply\" button, it is recommended to verify the result first.", "form", GetType().Name, "msg", "SqlPreviewHint1", "Text");
            _sqlPreviewHint2 = LocalizationHelper.GetLanguageString("-- Finally, press the \"Commit\" or \"Rollback\" button as needed.", "form", GetType().Name, "msg", "SqlPreviewHint2", "Text");
            _currentOperationObjectName = LocalizationHelper.GetLanguageString("Edit Table Data", "Global", "Global", "msg", "EditTableData", "Text");

            pnlNewRowForeColor.Location = new Point(lblNewRowForeColor.Left + lblNewRowForeColor.Width + 3, pnlNewRowForeColor.Top);
            pnlNewRowBackColor.Location = new Point(lblNewRowBackColor.Left + lblNewRowBackColor.Width + 3, pnlNewRowBackColor.Top);
            pnlDeletedRowForeColor.Location = new Point(lblDeletedRowForeColor.Left + lblDeletedRowForeColor.Width + 3, pnlDeletedRowForeColor.Top);
            pnlDeletedRowBackColor.Location = new Point(lblDeletedRowBackColor.Left + lblDeletedRowBackColor.Width + 3, pnlDeletedRowBackColor.Top);
            pnlChangedCellForeColor.Location = new Point(lblChangedCellForeColor.Left + lblChangedCellForeColor.Width + 3, pnlChangedCellForeColor.Top);
            pnlChangedCellBackColor.Location = new Point(lblChangedCellBackColor.Left + lblChangedCellBackColor.Width + 3, pnlChangedCellBackColor.Top);

            Tag = Text;
            tabData.Tag = tabData.Text;
            lblPosition2.Text = lblColumnFilterData.Text;
            txtColumnFilter.Location = new Point(lblPosition2.Left + lblPosition2.Width + 3, txtColumnFilter.Top);
            txtColumnFilter.Size = new Size(c1GridColumns.Width - lblPosition2.Width - 4, 21);
            ApplyLocalizedGridColumnCaptions(c1GridColumns, "gridheader"); //20241031

            AutoResizeGridColumnWidthForColumns();
            ArrangeDataFindControls();

            txtSchemaFilter.Location = new Point(lblFilter.Left + lblFilter.Width + 1, txtSchemaFilter.Top);
            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblFilter.Left - lblFilter.Width - 1, 21);
            AutoResizeGridColumnWidth();
            cboSchema.Location = new Point(lblSchema.Left + lblSchema.Width + 2, cboSchema.Top);
            splitContainer2.IsSplitterFixed = true;

            if (!MyLibrary.IsDarkMode)
            {
                lblFilter.ForeColor = ColorTranslator.FromHtml("#000000");
            }
            else
            {
                C1ThemeController.ApplicationTheme = "VS2013Dark";
                c1ThemeController1.SetTheme(c1GridSchemaBrowser, "VS2013Dark");
                c1GridSchemaBrowser.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1GridStructure, "VS2013Dark");
                c1GridStructure.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1Grid100RowsTop, "VS2013Dark");
                c1Grid100RowsTop.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1GridData, "VS2013Dark");
                c1GridData.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1GridColumns, "VS2013Dark");
                c1GridColumns.BackColor = ColorTranslator.FromHtml("#2D2D30");

                lblFilter.ForeColor = ColorTranslator.FromHtml("#FFFFFF");
                lblFilter.BackColor = ColorTranslator.FromHtml("#3F3F3f");

                //以下顏色要重新指定
                tsSqlPane.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkCopyAsHTML.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsSchemaBrowser.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                txtSchemaFilter.BackColor = ColorTranslator.FromHtml("#2D2D30");

                lblTableName01.ForeColor = Color.Yellow;
                lblTableName02.ForeColor = Color.Yellow;

                txtSchemaFilter.Location = new Point(131, 1);
                lblFilter.Location = new Point(90, 4);
            }

            _sqlPaneContextMenu = new ContextMenuStrip();

            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
            _sqlPaneContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPaneContextMenu.Items[SqlPaneColumn.SelectAll]).ShortcutKeys = Keys.Control | Keys.A;

            _sqlPaneContextMenu.Items[SqlPaneColumn.SelectAll].Click += delegate
            {
                editorSqlPane.SelectionStart = 0;
                editorSqlPane.SelectionEnd = editorSqlPane.Text.Length;
            };

            _sqlPaneContextMenu.Items[SqlPaneColumn.SelectAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
            _sqlPaneContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPaneContextMenu.Items[SqlPaneColumn.Copy]).ShortcutKeys = Keys.Control | Keys.C;

            _sqlPaneContextMenu.Items[SqlPaneColumn.Copy].Click += delegate
            {
                CopySqlPane();
            };

            _sqlPaneContextMenu.Items[SqlPaneColumn.Copy].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy2Script 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Save As", "form", GetType().Name, "menueditor", "SaveAs", "Text");
            _sqlPaneContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPaneContextMenu.Items[SqlPaneColumn.SaveAs]).ShortcutKeys = Keys.F12;

            _sqlPaneContextMenu.Items[SqlPaneColumn.SaveAs].Click += delegate
            {
                btnSaveAsSqlPane.PerformClick();
            };

            _sqlPaneContextMenu.Items[SqlPaneColumn.SaveAs].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Save As 16x16.ico");

            //----------------------------------//

            _sqlPreviewContextMenu = new ContextMenuStrip();

            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPreviewContextMenu.Items[SqlPreviewColumn.SelectAll]).ShortcutKeys = Keys.Control | Keys.A;

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SelectAll].Click += delegate
            {
                editorSqlPreview.SelectionStart = 0;
                editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SelectAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPreviewContextMenu.Items[SqlPreviewColumn.Copy]).ShortcutKeys = Keys.Control | Keys.C;

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Copy].Click += delegate
            {
                CopySqlPreviewSelection("SqlPreviewContextMenuCopy");
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Copy].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy2Script 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Save As", "form", GetType().Name, "menueditor", "SaveAs", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);
            ((ToolStripMenuItem)_sqlPreviewContextMenu.Items[SqlPreviewColumn.SaveAs]).ShortcutKeys = Keys.F12;

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SaveAs].Click += delegate
            {
                SaveAsSqlPreview();
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.SaveAs].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Save As 16x16.ico");

            _sqlPreviewContextMenu.Items.Add("-");

            _languageText = LocalizationHelper.GetLanguageString("Apply Edit", "form", GetType().Name, "menueditor", "ApplyEdit", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Apply].Click += delegate
            {
                SaveAsSqlPreview();
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Apply].Image = IconManager.GetImage(MyGlobal.IconLibrary, "ApplyEdit 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Cancel Edit", "form", GetType().Name, "menueditor", "CancelEdit", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Cancel].Click += delegate
            {
                SaveAsSqlPreview();
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Cancel].Image = IconManager.GetImage(MyGlobal.IconLibrary, "CancelEdit 16x16.ico");

            _sqlPreviewContextMenu.Items.Add("-");

            _languageText = LocalizationHelper.GetLanguageString("Commit", "form", GetType().Name, "menueditor", "Commit", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Commit].Click += delegate
            {
                SaveAsSqlPreview();
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Commit].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Commit 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Rollback", "form", GetType().Name, "menueditor", "Rollback", "Text");
            _sqlPreviewContextMenu.Items.Add(_languageText);

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Rollback].Click += delegate
            {
                SaveAsSqlPreview();
            };

            _sqlPreviewContextMenu.Items[SqlPreviewColumn.Rollback].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Rollback 16x16.ico");

            chkCopyAsHTML.Checked = true; //20250709 改為 true，不抓取 MyLibrary.CopyAsHTML 值

            Refresh();

            editorSqlPane.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorSqlPane.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? WrapVisualFlags.Start : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? WrapVisualFlags.End : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? WrapVisualFlags.Margin : WrapVisualFlags.None);

            if (MyLibrary.WordWrap)
            {
                btnWordWrapSqlPane.Visible = false;
                btnWordWrap2SqlPane.Visible = true;
                editorSqlPane.WrapMode = WrapMode.Word;
            }
            else
            {
                btnWordWrapSqlPane.Visible = true;
                btnWordWrap2SqlPane.Visible = false;
                editorSqlPane.WrapMode = WrapMode.None;
            }

            editorSqlPane.ViewWhitespace = WhitespaceMode.Invisible;

            ApplyEditorSetting();
            GridHelper.SetGridVisualStyle(c1GridSchemaBrowser, 10);
            GridHelper.SetGridVisualStyle(c1GridStructure, 10);
            GridHelper.SetGridVisualStyle(c1Grid100RowsTop, 10);
            GridHelper.SetGridVisualStyle(c1GridData, 10); //20231003
            GridHelper.SetGridVisualStyle(c1GridColumns, 10); //20240530

            GridHelper.SetGridVisualStyle(c1GridSchemaBrowser);
            GridHelper.SetGridVisualStyle(c1GridStructure);
            GridHelper.SetGridVisualStyle(c1Grid100RowsTop);
            GridHelper.SetGridVisualStyle(c1GridData);
            GridHelper.SetGridVisualStyle(c1GridColumns);

            //20260607 依語系內容變更欄位名稱
            ApplyLocalizedGridColumnCaptions(c1GridStructure, "gridheader");
            ApplyLocalizedGridColumnCaptions(c1GridColumns, "gridheader");

            GridFontAndBackColor();
            GridZoom();

            //切換語系時，LocalizationHelper 可能會將 c1GridData 的多行 Caption 還原為單行
            //須在 GridZoom() 之後重算高度，避免使用舊的 RowHeight
            RefreshGridDataColumnCaptionsAndHeight();
            GridHelper.SetGridHeaderLine(c1GridData);
            GridHelper.ApplyGridHeadingStyle(_columnInfoCollector, c1GridData);

            Cursor = Cursors.Default;
        }

        private void ApplyLocalizedGridColumnCaptions(C1TrueDBGrid grid, string gridHeader)
        {
            foreach (C1DataColumn column in grid.Columns)
            {
                var localizationId = column.DataField;

                if (string.IsNullOrEmpty(localizationId))
                {
                    continue;
                }

                column.Caption = LocalizationHelper.GetLanguageString
                (
                    column.Caption,
                    "form",
                    GetType().Name,
                    gridHeader,
                    localizationId,
                    "Text"
                );
            }
        }
        private void CreateTableSchemaTable() //點選到 Tables 時，顯示在右側的 Grid
        {
            _dtStructuredSchemaTable = new DataTable();
            _dtStructuredSchemaTable.Columns.Add(" ");
            _dtStructuredSchemaTable.Columns.Add("ColumnName");
            _dtStructuredSchemaTable.Columns.Add("ID");
            _dtStructuredSchemaTable.Columns.Add("DataType"); //完整的欄位型別
            _dtStructuredSchemaTable.Columns.Add("ConstraintInfo");
            _dtStructuredSchemaTable.Columns.Add("Nullable");

            if (IsMySql)
            {
                _dtStructuredSchemaTable.Columns.Add("AutoInc");
            }

            _dtStructuredSchemaTable.Columns.Add("Default");
            _dtStructuredSchemaTable.Columns.Add("Comments");
        }

        private void ConnectToDatabase()
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        MyGlobal.OracleReader.ConnectTo();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        MyGlobal.PostgreSqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        MyGlobal.SqlServerReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        MyGlobal.MySqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
            }
        }

        private void TransferValueToMainForm(string value)
        {
            if (DisplayRowIndex == -1) //20231004 如果是從 SQL Editor 觸發而開啟的，此處無法傳遞訊息給 Main Form (會引發例外錯誤)
            {
                return;
            }

            var valueArgs = new ValueUpdatedEventArgs(value);

            ValueUpdated(this, valueArgs);
        }

        private void tmrMother2Child_Tick(object sender, EventArgs e)
        {
            var temp = string.Empty;

            //是否為主表單通知要自動中斷連線？
            if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp) && MyGlobal.GlobalTemp.StartsWith("AutoDisconnect`", StringComparison.Ordinal))
            {
                temp = MyGlobal.GlobalTemp.Replace("AutoDisconnect`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.GlobalTemp == "AutoDisconnect`")
                    {
                        MyGlobal.GlobalTemp = string.Empty;
                    }

                    DisconnectDatabase();
                }
            }
            else if (!string.IsNullOrEmpty(MyGlobal.InfoFromMDIForm) && MyGlobal.InfoFromMDIForm.StartsWith("ExecuteCommitRollback`", StringComparison.Ordinal)) //是否為「按下 Commit / Rollback 按鈕」，或「執行 Commit / Rollback 指令」？
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("ExecuteCommitRollback`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "ExecuteCommitRollback`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    _modifiedCells.Clear();
                    _deletedRows.Clear();
                    _newRows.Clear();

                    ClearDirectBinaryChanges();

                    _searchResultCells.Clear();

                    //20240717 更新資料！
                    DisplaySchemaInfo2();

                    MarkTransactionAsCompleted();

                    btnApplyEditData.Enabled = false;
                    btnApplyEditSqlPreview.Enabled = false;

                    if (tabSqlPreview.TabVisible)
                    {
                        tabSqlPreview.TabVisible = false;
                    }

                    if (tabData.TabVisible)
                    {
                        tabSchemaBrowser.SelectedTab = tabData;
                    }

                    DisconnectDatabase(); //共用同一個 Connection，所以，Commit/Rollback 之後，每一個 Query Editor 的 lblNotCommitYet.Text 都要清空
                }
            }
            else if (!string.IsNullOrEmpty(MyGlobal.InfoFromMDIForm) && MyGlobal.InfoFromMDIForm.StartsWith("UpdateCommitRollbackButton`", StringComparison.Ordinal)) //如果是 nonquery, 執行無錯誤, 更新 Commit / Rollback 按鈕
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("UpdateCommitRollbackButton`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "UpdateCommitRollbackButton`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    MarkTransactionAsPending();
                }
            }
            else if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp) && MyGlobal.GlobalTemp.StartsWith("CloseSchemaBrowserForm`", StringComparison.Ordinal)) //由 TabControl 關閉 SchemaBrowser Form (關閉單一 Form)
            {
                temp = MyGlobal.GlobalTemp.Replace("CloseSchemaBrowserForm`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp != AccessibleDescription)
                {
                    return;
                }

                //20240801 檢查是否有編輯資料？
                if (_modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0)
                {
                    //處理過程中：先把關鍵字串重新命名，以免重複觸發！
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", $"+{AccessibleDescription}+;");

                    var result = MessageBox.Show(_confirmExitTableEditMessage, tabData.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    switch (result)
                    {
                        case DialogResult.Yes: //使用者選擇「放棄修改」，直接關閉
                            {
                                _modifiedCells.Clear();
                                _deletedRows.Clear();
                                _newRows.Clear();
                                _searchResultCells.Clear();

                                //20240823 判斷是主畫面觸發關閉
                                if (MyGlobal.CommitRollbackCheck == 0)
                                {
                                    //等於 0，主畫面要求關閉(關閉整個 JasonQuery)，回答 YES，就直接關閉了
                                    MyGlobal.GlobalTemp = string.Empty;
                                }
                                else
                                {
                                    //此處有可能是關閉右側所有視窗，故需要逐一詢問
                                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"+{AccessibleDescription}+;", $"{AccessibleDescription};").Replace($"{AccessibleDescription};", string.Empty);

                                    if (MyGlobal.GlobalTemp == "CloseSchemaBrowserForm`")
                                    {
                                        MyGlobal.GlobalTemp = string.Empty;
                                    }
                                }

                                break;
                            }
                        case DialogResult.No: //使用者選擇「保留修改」，不關閉
                            {
                                MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"+{AccessibleDescription}+;", $"{AccessibleDescription}|CANCEL;");
                                return;
                            }
                    }
                }
                else
                {
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", string.Empty);
                }

                return;
            }
            else if (!string.IsNullOrEmpty(MyGlobal.InfoFromMDIForm) && MyGlobal.InfoFromMDIForm.StartsWith("UpdateSchemaBrowserInformation`", StringComparison.Ordinal)) //20241011 按下 Refresh 更新 SchemaBrowser 資訊
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("UpdateSchemaBrowserInformation`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "UpdateSchemaBrowserInformation`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    GridHelper.UpdateSchemaData(c1GridSchemaBrowser);
                    txtSchemaFilter.Text = "*";
                    txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblFilter.Left - lblFilter.Width - 1, 21);
                    AutoResizeGridColumnWidth();
                }
                else if (MyGlobal.InfoFromMDIForm == "UpdateSchemaBrowserInformation`")
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp6) && MyGlobal.GlobalTemp6.StartsWith("UpdateSchemaBrowserInformation`", StringComparison.Ordinal)) //20250204 在 QueryForm 更新 Schema 資訊
            {
                temp = MyGlobal.GlobalTemp6.Replace("UpdateSchemaBrowserInformation`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp6 = string.Empty; //觸發一次即可，後續再由「MyGlobal.sInfoFromMDIForm = "UpdateSchemaInformation`"」接手

                    btnRefresh.PerformClick();
                }
            }

            //是否為 Reload Localization 套用？
            if (string.IsNullOrEmpty(MyGlobal.InfoFromReloadLocalization) || !MyGlobal.InfoFromReloadLocalization.StartsWith("ReloadLocalization`", StringComparison.Ordinal))
            {
                return;
            }

            temp = MyGlobal.InfoFromReloadLocalization.Replace("ReloadLocalization`", string.Empty);
            temp = temp.Split(';')[0];

            if (temp != AccessibleDescription)
            {
                return;
            }

            MyGlobal.InfoFromReloadLocalization = MyGlobal.InfoFromReloadLocalization.Replace($"{AccessibleDescription};", string.Empty);

            if (MyGlobal.InfoFromReloadLocalization == "ReloadLocalization`")
            {
                MyGlobal.InfoFromReloadLocalization = string.Empty;
            }

            ApplyLocalizationSetting();
            ApplyGridInteractionDirection();
        }

        private void btnGridExportToFile_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }

        private void ExportToFile()
        {
            var sheetName = lblTableName01.Text;

            _gridMap.TryGetValue(tabSchemaBrowser.SelectedTab.Name, out var c1Grid);

            using (var form = new ExportToFileForm())
            {
                var dtData = c1Grid.CopyDataTableSourceOrEmpty();

                dtData = GridHelper.ReplaceColumnNameByLanguageInfo(dtData, GetType().Name);

                //20240724 移除識別欄位
                var columns = dtData.Columns;

                DataTableColumnHelper.SafeRemoveColumn(dtData, MyGlobal.Row_Id_PK_JQ);
                DataTableColumnHelper.SafeRemoveColumn(dtData, _identifyColumnName);

                form.Title = tabSchemaBrowser.SelectedTab.Text;
                form.dtData = dtData;
                form.SheetName = sheetName;
                form.FontName = c1Grid.Font.Name;
                form.FontSize = c1Grid.Font.Size;
                form.ShowDialog();
            }
        }

        private void btnGridSelectAll_Click(object sender, EventArgs e)
        {
            _gridMap.TryGetValue(tabSchemaBrowser.SelectedTab.Name, out var c1Grid);

            c1Grid.SelectedRows.Clear();

            for (var i = 0; i < c1Grid.Splits[0].Rows.Count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }
        }

        private void btnGridCopy_Click(object sender, EventArgs e)
        {
            CopyDataFromDataGrid();
        }

        private void DisconnectDatabase()
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        MyGlobal.OracleReader.Disconnect();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        MyGlobal.PostgreSqlReader.Disconnect();
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        MyGlobal.SqlServerReader.Disconnect();
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        MyGlobal.MySqlReader.Disconnect();
                        break;
                    }
            }
        }

        private string ExecuteQuery100Rows(string sql, int startRow, int pageLength, bool isTop100Rows = true, bool showAlertOnError = false)
        {
            var errorMessage = string.Empty;

            var queryContext = new TraceLogContext
            {
                Category = "SchemaBrowser",
                ObjectType = _currentSchemaBrowserSelection?.SchemaType,
                ObjectName = _currentSchemaBrowserSelection?.SchemaName,
                RequestedRows = pageLength
            };

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var oracleReader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

                        oracleReader.EnsureConnectionOpen();

                        using (TraceLogger.Time("SchemaBrowser", "Query.Total", queryContext))
                        {
                            _dtTableData = oracleReader.ExecuteQueryPaged100Rows(sql, startRow, pageLength, out errorMessage, out _dtRawSchemaTable);
                            queryContext.ReturnedRows = _dtTableData?.Rows.Count;
                            queryContext.ColumnCount = _dtTableData?.Columns.Count;
                        }

                        if (_dtTableData != null && string.IsNullOrEmpty(errorMessage)) //20250304
                        {
                            try
                            {
                                if (pageLength == 0)
                                {
                                    PrepareSchemaColumnInfoCollector(_dtRawSchemaTable);
                                }
                                else
                                {
                                    ArrangeDataTable(isTop100Rows ? c1Grid100RowsTop : c1GridData, _dtTableData, _dtRawSchemaTable, isTop100Rows);
                                }
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }
                            catch (Exception ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }

                            if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }

                        //20231014
                        if (showAlertOnError && !string.IsNullOrEmpty(errorMessage) && !errorMessage.StartsWith("ORA-00942", StringComparison.Ordinal))
                        {
                            MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        var postgreSqlReader = MyGlobal.PostgreSqlReader ?? throw new InvalidOperationException("PostgreSqlReader has not been initialized.");

                        postgreSqlReader.EnsureConnectionOpen();

                        var errorCode = string.Empty;

                        bool isRollback;
                        bool isPermissionDenied;

                        using (TraceLogger.Time("SchemaBrowser", "Query.Total", queryContext))
                        {
                            _dtTableData = postgreSqlReader.ExecuteQueryPaged100Rows(sql, startRow, pageLength, out isRollback, out isPermissionDenied, out errorMessage, out errorCode, out _dtRawSchemaTable);

                            if (isRollback)
                            {
                                errorMessage = string.Empty;
                                errorCode = string.Empty;
                                _dtTableData = MyGlobal.PostgreSqlReader.ExecuteQueryPaged100Rows(sql, startRow, pageLength, out isRollback, out isPermissionDenied, out errorMessage, out errorCode, out _dtRawSchemaTable);
                            }

                            queryContext.ReturnedRows = _dtTableData?.Rows.Count;
                            queryContext.ColumnCount = _dtTableData?.Columns.Count;
                        }

                        if (_dtTableData != null && string.IsNullOrEmpty(errorMessage)) //20250304
                        {
                            try
                            {
                                if (pageLength == 0)
                                {
                                    PrepareSchemaColumnInfoCollector(_dtRawSchemaTable);
                                }
                                else if (isPermissionDenied)
                                {
                                    ArrangeDataTable(isTop100Rows ? c1Grid100RowsTop : c1GridData, _dtRawSchemaTable, _dtRawSchemaTable, isTop100Rows);
                                }
                                else
                                {
                                    ArrangeDataTable(isTop100Rows ? c1Grid100RowsTop : c1GridData, _dtTableData, _dtRawSchemaTable, isTop100Rows);
                                }
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }
                            catch (Exception ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }

                            if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }

                        var temp = string.IsNullOrEmpty(errorCode) ? string.Empty : $"{errorCode}: ";

                        errorMessage = $"{temp}{errorMessage}";

                        //20231014
                        if (showAlertOnError && !string.IsNullOrEmpty(errorMessage) && !errorMessage.StartsWith("42P01:", StringComparison.Ordinal))
                        {
                            MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        var sqlServerReader = MyGlobal.SqlServerReader ?? throw new InvalidOperationException("SqlServerReader has not been initialized.");

                        sqlServerReader.EnsureConnectionOpen();

                        using (TraceLogger.Time("SchemaBrowser", "Query.Total", queryContext))
                        {
                            _dtTableData = sqlServerReader.ExecuteQueryPaged100Rows(sql, startRow, pageLength, out errorMessage, out _dtRawSchemaTable);
                            queryContext.ReturnedRows = _dtTableData?.Rows.Count;
                            queryContext.ColumnCount = _dtTableData?.Columns.Count;
                        }

                        if (_dtTableData != null && string.IsNullOrEmpty(errorMessage)) //20250304
                        {
                            try
                            {
                                if (pageLength == 0)
                                {
                                    PrepareSchemaColumnInfoCollector(_dtRawSchemaTable);
                                }
                                else
                                {
                                    ArrangeDataTable(isTop100Rows ? c1Grid100RowsTop : c1GridData, _dtTableData, _dtRawSchemaTable, isTop100Rows);
                                }
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }
                            catch (Exception ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }

                            if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }

                        //20231014
                        if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                        {
                            MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        var mySqlReader = MyGlobal.MySqlReader ?? throw new InvalidOperationException("MySqlReader has not been initialized.");

                        mySqlReader.EnsureConnectionOpen();

                        using (TraceLogger.Time("SchemaBrowser", "Query.Total", queryContext))
                        {
                            _dtTableData = mySqlReader.ExecuteQueryPaged100Rows(sql, startRow, pageLength, out errorMessage, out _dtRawSchemaTable);
                            queryContext.ReturnedRows = _dtTableData?.Rows.Count;
                            queryContext.ColumnCount = _dtTableData?.Columns.Count;
                        }

                        if (_dtTableData != null && string.IsNullOrEmpty(errorMessage)) //20250304
                        {
                            try
                            {
                                if (pageLength == 0)
                                {
                                    PrepareSchemaColumnInfoCollector(_dtRawSchemaTable);
                                }
                                else
                                {
                                    ArrangeDataTable(isTop100Rows ? c1Grid100RowsTop : c1GridData, _dtTableData, _dtRawSchemaTable, isTop100Rows);
                                }
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }
                            catch (Exception ex)
                            {
                                errorMessage = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";
                            }

                            if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }

                        //20231014
                        if (showAlertOnError && !string.IsNullOrEmpty(errorMessage))
                        {
                            MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
            }

            //20240414 顯示標題列的分隔線
            GridHelper.SetGridHeaderLine(c1GridStructure);
            GridHelper.SetGridHeaderLine(c1Grid100RowsTop);
            GridHelper.SetGridHeaderLine(c1GridData);
            return errorMessage;
        }

        private void CopyDataFromDataGrid()
        {
            var data = string.Empty;
            var columnName = string.Empty;
            var dataType = string.Empty;
            var isActiveCell = true; //是否為「只點選單一個 cell，並沒有『選取範圍』」?

            _gridMap.TryGetValue(tabSchemaBrowser.SelectedTab.Name, out var c1Grid);

            var selectedColsCount = c1Grid.SelectedCols.Count;
            var quotingWith = string.Empty;
            var fieldSeparator = ",";
            var isSelectedWholeColumn = c1Grid.SelectedRows.Count == 0 && selectedColsCount > 0;

            if (isSelectedWholeColumn) //整欄選取
            {
                isActiveCell = false;

                var sbColumnName = new StringBuilder();
                var sbDataType = new StringBuilder();
                var sbData = new StringBuilder();
                var rowsCount = c1Grid.Splits[0].Rows.Count;

                for (var row = 0; row < rowsCount; row++)
                {
                    var sbRowData = new StringBuilder();

                    foreach (var col in c1Grid.SelectedCols)
                    {
                        var columnKey = col.ToString();
                        var caption = c1Grid.Columns[columnKey].DataField.Replace("\r\n", "\n");
                        string[] splitters = { "\r\n", "\r", "\n" };
                        var parts = caption.Split(splitters, 4, StringSplitOptions.None);
                        var temp1 = parts[0];
                        var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                        if (row == 0)
                        {
                            //收集 Column Name & Data Type
                            sbColumnName.Append(temp1).Append(fieldSeparator);

                            if (!string.IsNullOrEmpty(temp2))
                            {
                                sbDataType.Append(temp2).Append(fieldSeparator);
                            }
                        }

                        sbRowData.Append(quotingWith)
                                 .Append(c1Grid.Columns[columnKey].CellText(row))
                                 .Append(quotingWith)
                                 .Append(fieldSeparator);
                    }

                    if (sbRowData.Length > 0)
                    {
                        //移除末尾的分隔符
                        sbRowData.Length -= fieldSeparator.Length;
                        sbData.AppendLine(sbRowData.ToString());
                    }
                }

                data = sbData.ToString();
                columnName = sbColumnName.ToString();
                dataType = sbDataType.ToString();
            }
            else //非整欄選取
            {
                var sbColumnName = new StringBuilder();
                var sbDataType = new StringBuilder();
                var sbData = new StringBuilder();
                var i = 0;

                foreach (int row in c1Grid.SelectedRows)
                {
                    var vr = c1Grid.Splits[0].Rows[row];

                    if (selectedColsCount == 0) //整列選取
                    {
                        isActiveCell = false;

                        foreach (C1DataColumn column in c1Grid.Columns)
                        {
                            var caption = column.DataField;
                            string[] splitters = { "\r\n", "\r", "\n" };
                            var parts = caption.Split(splitters, 4, StringSplitOptions.None);
                            var temp1 = parts[0];
                            var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                            if (i == 0) //收集 Column Name & Data Type
                            {
                                sbColumnName.Append(temp1).Append(fieldSeparator);

                                if (!string.IsNullOrEmpty(temp2))
                                {
                                    sbDataType.Append(temp2).Append(fieldSeparator);
                                }
                            }

                            sbData.Append(column.CellText(vr.DataRowIndex)).Append(fieldSeparator);
                        }

                        i++;
                    }
                    else //非整列選取 (選取區塊)
                    {
                        foreach (C1DataColumn column in c1Grid.SelectedCols)
                        {
                            isActiveCell = false;

                            var caption = column.DataField;
                            string[] splitters = { "\r\n", "\r", "\n" };
                            var parts = caption.Split(splitters, 4, StringSplitOptions.None);
                            var temp1 = parts[0];
                            var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                            if (i == 0) //收集 Column Name & Data Type
                            {
                                sbColumnName.Append(temp1).Append(fieldSeparator);

                                if (!string.IsNullOrEmpty(temp2))
                                {
                                    sbDataType.Append(temp2).Append(fieldSeparator);
                                }
                            }

                            sbData.Append(column.CellText(vr.DataRowIndex)).Append(fieldSeparator);
                        }

                        i++;
                    }

                    if (sbData.Length > 0)
                    {
                        //移除末尾的分隔符
                        sbData.Length -= fieldSeparator.Length;
                        sbData.AppendLine();
                    }
                }

                data = sbData.ToString();
                columnName = sbColumnName.ToString();
                dataType = sbDataType.ToString();
            }

            if (!string.IsNullOrEmpty(columnName) && columnName.EndsWith(fieldSeparator, StringComparison.Ordinal))
            {
                var temp = columnName.Substring(0, columnName.Length - fieldSeparator.Length);

                columnName = $"{temp}\r\n";
            }

            if (!string.IsNullOrEmpty(dataType) && dataType.EndsWith(fieldSeparator, StringComparison.Ordinal))
            {
                var temp = dataType.Substring(0, dataType.Length - fieldSeparator.Length);

                dataType = $"{temp}\r\n";
            }

            if (isActiveCell)
            {
                var row = c1Grid.Splits[0].Rows[c1Grid.Row].DataRowIndex;

                data = c1Grid[row, c1Grid.Col].ToString();
            }

            TextHelper.CopyTextToClipboard($"{columnName}{dataType}{data}", "CopyDataFromDataGrid(01)");

            var copySelectedBlock = LocalizationHelper.GetLanguageString("The selected block has been copied to the clipboard!", "Global", "Global", "msg", "CopySelectedBlock", "Text");

            MessageBox.Show(copySelectedBlock, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ArrangeDataTable(C1TrueDBGrid c1Grid, DataTable dtData, DataTable dtSchemaTable, bool isTop = true)
        {
            var traceContext = new TraceLogContext
            {
                Category = "SchemaBrowser",
                ObjectType = _currentSchemaBrowserSelection?.SchemaType,
                ObjectName = _currentSchemaBrowserSelection?.SchemaName,
                ReturnedRows = dtData?.Rows.Count,
                ColumnCount = dtData?.Columns.Count,
                Message = isTop ? "ViewData" : "TableData"
            };

            using (TraceLogger.Time("SchemaBrowser", "Grid.ArrangeDataTable", traceContext))
            {
                PrepareSchemaColumnInfoCollector(dtSchemaTable);

                var context = new ArrangeContext
                {
                    SourceData = dtData,
                    SchemaTable = dtSchemaTable,
                    ShowColumnType = true,
                    ShowColumnComments = true,
                    ShowColumnDefaultValue = true,
                    columnInfoCollector = _columnInfoCollector,
                    DateFormat = MyLibrary.DateFormat,
                    DateTimeFormat = $"{MyLibrary.DateFormat} HH:mm:ss",
                    NullDisplayText = string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs,
                    LargeTextPreviewLength = AppConfigHelper.LargeTextPreviewLength,
                    TruncatedText = LocalizationHelper.GetLanguageString("…(truncated)", "Global", "Global", "msg", "Truncated…", "Text")
                };

                var strategy = ArrangeStrategyFactory.Create(_currentSourceType);

                //收集 context 訊息 (執行對應的 ArrangeStrategy)
                strategy.Execute(context);

                _dtTableData = context.SortedData.Copy();

                //20240604 for 編輯資料用，末欄新增一個識別欄位
                if (!isTop)
                {
                    for (var i = 0; i < _dtTableData.Columns.Count; i++)
                    {
                        _dtTableData.Columns[i].AllowDBNull = true;
                        _dtTableData.Columns[i].ReadOnly = false;
                    }

                    DataTableColumnHelper.SafeAddColumn(_dtTableData, _identifyColumnName);
                }

                c1Grid.DataSource = _dtTableData;
                _dtOriginalTableData = _dtTableData.Copy();

                if (!isTop)
                {
                    foreach (C1DisplayColumn col in c1Grid.Splits[0].DisplayColumns)
                    {
                        var columnName = col.DataColumn.DataField;

                        if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                        {
                            col.Visible = false; //20231003 隱藏 ROWID_PK_JQ
                            col.Frozen = true;
                        }

                        if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            col.Visible = false; //20240604 隱藏識別欄位
                            col.Frozen = true;
                            break;
                        }
                    }
                }

                GridHelper.ResizeGridColumnWidth(c1Grid);
            }
        }

        private void PrepareSchemaColumnInfoCollector(DataTable dtSchemaTable)
        {
            DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "Comment");

            if (_currentSourceType == DataSourceType.PostgreSql)
            {
                DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "UsedProviderFallback");
            }

            if (_currentSourceType == DataSourceType.MySql)
            {
                DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "UsedProviderFallback");
                DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "DataType2");
            }

            _columnInfoCollector = SchemaColumnInfoBuilder.Build(_currentSourceType, dtSchemaTable);
        }

        private void c1Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
            }
        }

        private void SelectAll()
        {
            if (_isInEditMode)
            {
                return; //20240726 編輯模式，禁止全選，否則會引發例外錯誤
            }

            var c1Grid = GetCurrentGridOrNull();

            if (c1Grid == null)
            {
                return;
            }

            c1Grid.SelectedRows.Clear();

            for (var i = 0; i < c1Grid.Splits[0].Rows.Count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }
        }

        private void c1Grid_MouseClick(object sender, MouseEventArgs e)
        {
            _gridMap.TryGetValue(tabSchemaBrowser.SelectedTab.Name, out var c1Grid);

            var row = c1Grid.RowContaining(e.Y);
            var col = c1Grid.ColContaining(e.X);

            if (c1GridData.Focused && _isInEditMode && (row != _editingRowIndex || col != _editingColumnIndex))
            {
                UpdateEditMode(); //20250815 如果是編輯模式，且滑鼠點擊了其他單元格，則取消編輯模式
            }
        }

        private void c1Grid_MouseDown(object sender, MouseEventArgs e)
        {
            var c1Grid = GetCurrentGridOrNull();

            if (c1Grid == null)
            {
                return;
            }

            var row = c1Grid.RowContaining(e.Y);
            var col = c1Grid.ColContaining(e.X);

            if (ReferenceEquals(c1Grid, c1GridData))
            {
                _isDataGridFilterBarEditing = IsMouseOnDataGridFilterBar(row, col, e.Y);

                if (_isDataGridFilterBarEditing)
                {
                    _isInEditMode = false;
                    tsData.Enabled = true;
                    return;
                }
            }

            if (_isInEditMode)
            {
                if (ReferenceEquals(c1Grid, c1GridData))
                {
                    var isDifferentCell = row != _editingRowIndex || col != _editingColumnIndex;

                    if (row >= 0 && col >= 0 && isDifferentCell)
                    {
                        //一般內嵌編輯時，滑鼠點到其他 Cell，提交當前的輸入值
                        CommitDataGridInlineEditBeforeCellNavigation();

                        try
                        {
                            c1GridData.SelectedCols.Clear();
                            c1GridData.SelectedRows.Clear();
                            c1GridData.Row = row;
                            c1GridData.Col = col;
                            c1GridData.Select();
                        }
                        catch
                        {
                            //忽略篩選或特殊 Grid 狀態下 Row/Col 設定失敗
                        }

                        //此處不要 return，讓後續正常更新按鈕狀態
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            if (_gridContextMenu != null)
            {
                _gridContextMenu.Dispose();
            }

            _gridContextMenu = new ContextMenuStrip();

            if (c1GridData.Focused)
            {
                if (_isInEditMode && (row == -1 || row != _editingRowIndex || col != _editingColumnIndex))
                {
                    UpdateEditMode(); //20250815 如果是編輯模式，且滑鼠點擊了其他單元格，則取消編輯模式
                }

                var isCornerSelected = row == -1 && col == -1;

                //20241003 兩者同時為 -1，表示滑鼠點在 Grid 右側(無法識別是哪一列)或是點在右下角空白區塊
                if (isCornerSelected)
                {
                    c1Grid.SelectedCols.Clear();
                    c1Grid.SelectedRows.Clear();
                    c1Grid.Select();
                    return;
                }

                //20240429 iCol = -1，表示是滑鼠點在最左側那一行
                //20240914 iCol = -1，也可能是滑鼠點在查詢結果的最右側空白處
                if (col == -1)
                {
                    row = c1Grid.Row;
                    col = c1Grid.Col;
                    c1Grid.Row = row;
                    c1Grid.Col = col;
                    c1Grid.Select();
                    return;
                }

                //20240519 iRow = -1，表示是在篩選列 (或標題列)
                if (row == -1)
                {
                    if (c1Grid.Row >= 0)
                    {
                        //20240914 c1GridData.Row >= 0，表示滑鼠點在查詢結果的最下方空白處
                        row = c1Grid.Row;
                        tsData.Enabled = true;
                        chkShowFilterRowData.Enabled = true;
                    }
                    else
                    {
                        tsData.Enabled = false;
                        chkShowFilterRowData.Enabled = false;
                        return;
                    }
                }
                else
                {
                    tsData.Enabled = true;
                    chkShowFilterRowData.Enabled = true;
                }

                UpdateDataCellEditCommandStatus(row, col); //20260529 重構
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var enabled = c1Grid.HasDataTableRows();

            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menugrid2", "SelectAll", "Text");
            _gridContextMenu.Items.Add(_languageText);

            _gridContextMenu.Items[GridColumn.SelectAll].Click += delegate
            {
                SelectAll();
            };

            _gridContextMenu.Items[GridColumn.SelectAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
            _gridContextMenu.Items[GridColumn.SelectAll].Enabled = enabled;
            ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.SelectAll]).ShortcutKeyDisplayString = "Ctrl+A";

            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menugrid2", "Copy", "Text");
            _gridContextMenu.Items.Add(_languageText);

            _gridContextMenu.Items[GridColumn.Copy].Click += delegate
            {
                CopyDataFromDataGrid();
            };

            _gridContextMenu.Items[GridColumn.Copy].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy2Script 16x16.ico");
            _gridContextMenu.Items[GridColumn.Copy].Enabled = enabled;
            ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.Copy]).ShortcutKeyDisplayString = "Ctrl+C";

            _gridContextMenu.Items.Add("-");

            _languageText = LocalizationHelper.GetLanguageString("Export to File", "form", GetType().Name, "menugrid2", "ExportToFile", "Text");
            _gridContextMenu.Items.Add(_languageText);

            _gridContextMenu.Items[GridColumn.ExportToFile].Click += delegate
            {
                ExportToFile();
            };

            _gridContextMenu.Items[GridColumn.ExportToFile].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Export 16x16.ico");
            _gridContextMenu.Items[GridColumn.ExportToFile].Enabled = enabled;

            //20241003 新增功能表 for 資料編輯
            if (c1GridData.Focused)
            {
                _gridContextMenu.Items.Add("-");

                _languageText = LocalizationHelper.GetLanguageString("Edit Cell", "form", GetType().Name, "menugrid2", "EditCell", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.EditCell].Click += delegate
                {
                    EditCell();
                };

                _gridContextMenu.Items[GridColumn.EditCell].Image = IconManager.GetImage(MyGlobal.IconLibrary, "EditCell 16x16.ico");
                _gridContextMenu.Items[GridColumn.EditCell].Enabled = btnEditCellData.Enabled;
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.EditCell]).ShortcutKeyDisplayString = "F2";

                _languageText = LocalizationHelper.GetLanguageString("Edit Cell with Edit Form", "form", GetType().Name, "menugrid2", "EditCellWithEditForm", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.EditCellWithEditForm].Click += delegate
                {
                    EditCellWithEditForm();
                };

                _gridContextMenu.Items[GridColumn.EditCellWithEditForm].Image = IconManager.GetImage(MyGlobal.IconLibrary, "EditCellWithEditForm 16x16.ico");
                _gridContextMenu.Items[GridColumn.EditCellWithEditForm].Enabled = CheckEditCellWithEditForm(row, col, false);
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.EditCellWithEditForm]).ShortcutKeyDisplayString = "Shift+Enter"; //20250114 修改快捷鍵 → Shift + Enter

                _languageText = LocalizationHelper.GetLanguageString("Set to Null", "form", GetType().Name, "menugrid2", "SetNull", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.SetNull].Click += delegate
                {
                    SetToNull();
                };

                _gridContextMenu.Items[GridColumn.SetNull].Image = IconManager.GetImage(MyGlobal.IconLibrary, "SetNull 16x16.ico");
                _gridContextMenu.Items[GridColumn.SetNull].Enabled = btnSetNullData.Enabled;
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.SetNull]).ShortcutKeyDisplayString = "Alt+S";

                _languageText = LocalizationHelper.GetLanguageString("Insert New Row", "form", GetType().Name, "menugrid2", "InsertNewRow", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.InsertNewRow].Click += delegate
                {
                    InsertNewRow();
                };

                _gridContextMenu.Items[GridColumn.InsertNewRow].Image = IconManager.GetImage(MyGlobal.IconLibrary, "InsertNewRow 16x16.ico");
                _gridContextMenu.Items[GridColumn.InsertNewRow].Enabled = btnInsertNewRowData.Enabled;
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.InsertNewRow]).ShortcutKeyDisplayString = "Alt+Insert";

                _languageText = LocalizationHelper.GetLanguageString("Duplicate Current Row", "form", GetType().Name, "menugrid2", "DuplicateCurrentRow", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.DuplicateCurrentRow].Click += delegate
                {
                    DuplicateCurrentRow();
                };

                _gridContextMenu.Items[GridColumn.DuplicateCurrentRow].Image = IconManager.GetImage(MyGlobal.IconLibrary, "DuplicateCurrentRow 16x16.ico");
                _gridContextMenu.Items[GridColumn.DuplicateCurrentRow].Enabled = btnDuplicateCurrentRowData.Enabled;
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.DuplicateCurrentRow]).ShortcutKeyDisplayString = "Ctrl+Alt+Insert";

                _languageText = LocalizationHelper.GetLanguageString("Delete Current Row", "form", GetType().Name, "menugrid2", "DeleteCurrentRow", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.DeleteCurrentRow].Click += delegate
                {
                    DeleteCurrentRow();
                };

                _gridContextMenu.Items[GridColumn.DeleteCurrentRow].Image = IconManager.GetImage(MyGlobal.IconLibrary, "DeleteCurrentRow 16x16.ico");
                _gridContextMenu.Items[GridColumn.DeleteCurrentRow].Enabled = btnDeleteCurrentRowData.Enabled;
                ((ToolStripMenuItem)_gridContextMenu.Items[GridColumn.DeleteCurrentRow]).ShortcutKeyDisplayString = "Alt+Delete";

                _gridContextMenu.Items.Add("-");

                _languageText = LocalizationHelper.GetLanguageString("Sql Preview", "form", GetType().Name, "menugrid2", "SqlPreview", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.SqlPreview].Click += delegate
                {
                    tabSchemaBrowser.SelectedTab = tabSqlPreview;
                };

                _gridContextMenu.Items[GridColumn.SqlPreview].Image = IconManager.GetImage(MyGlobal.IconLibrary, "SQL Preview 16x16.ico");
                _gridContextMenu.Items[GridColumn.SqlPreview].Enabled = btnSqlPreviewData.Enabled;

                _gridContextMenu.Items.Add("-");

                _languageText = LocalizationHelper.GetLanguageString("Apply All Changes", "form", GetType().Name, "menugrid2", "ApplyEdit", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.ApplyEdit].Click += delegate
                {
                    ApplyEdit();
                };

                _gridContextMenu.Items[GridColumn.ApplyEdit].Image = IconManager.GetImage(MyGlobal.IconLibrary, "ApplyEdit 16x16.ico");
                _gridContextMenu.Items[GridColumn.ApplyEdit].Enabled = btnApplyEditData.Enabled;

                _gridContextMenu.Items.Add("-");

                _languageText = LocalizationHelper.GetLanguageString("Commit", "form", GetType().Name, "menugrid2", "Commit", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.Commit].Click += delegate
                {
                    Commit();
                };

                var iconSuffix = (MyLibrary.CommitRollbackIcon >= 2 && MyLibrary.CommitRollbackIcon <= 6) ? MyLibrary.CommitRollbackIcon.ToString() : string.Empty;
                var commitIconName = $"Commit {iconSuffix} 16x16.ico";
                var rollbackIconName = $"Rollback {iconSuffix} 16x16.ico";

                _gridContextMenu.Items[GridColumn.Commit].Image = IconManager.GetImage(MyGlobal.IconLibrary, commitIconName);
                _gridContextMenu.Items[GridColumn.Commit].Enabled = btnCommitData.Enabled;

                _languageText = LocalizationHelper.GetLanguageString("Rollback", "form", GetType().Name, "menugrid2", "Rollback", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.Rollback].Click += delegate
                {
                    Rollback();
                };

                _gridContextMenu.Items[GridColumn.Commit].Image = IconManager.GetImage(MyGlobal.IconLibrary, rollbackIconName);
                _gridContextMenu.Items[GridColumn.Rollback].Enabled = btnRollbackData.Enabled;

                _gridContextMenu.Items.Add("-");

                _languageText = LocalizationHelper.GetLanguageString("Refresh", "form", GetType().Name, "menugrid2", "Refresh", "Text");
                _gridContextMenu.Items.Add(_languageText);

                _gridContextMenu.Items[GridColumn.Refresh].Click += delegate
                {
                    DisplaySchemaInfo2();
                };

                _gridContextMenu.Items[GridColumn.Refresh].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Refresh 16x16.ico");
                _gridContextMenu.Items[GridColumn.Refresh].Enabled = true;
            }

            if (MyLibrary.IsDarkMode)
            {
                _gridContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _gridContextMenu.ForeColor = Color.White;
                _gridContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            c1Grid.ContextMenuStrip = _gridContextMenu;
            _gridContextMenu.Show(c1Grid, new Point(e.X, e.Y));
        }

        private bool IsGridDataColumnUpdateSupported(int col)
        {
            if (col < 0 || col >= c1GridData.Columns.Count)
            {
                return false;
            }

            var columnName = c1GridData.Columns[col].DataField;

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            return _columnInfoCollector != null
                   && _columnInfoCollector.TryGet(columnName, out var columnInfo)
                   && IsTableDataColumnEditable(columnInfo);
        }

        private void c1GridData_AfterColUpdate(object sender, ColEventArgs e)
        {
            if (!IsVisibleDataGridDataRow(c1GridData.Row))
            {
                _isInEditMode = false;
                tsData.Enabled = true;
                return;
            }

            RefreshTableEditStatusAfterCellValueChanged();
        }

        private void RefreshTableEditStatusAfterCellValueChanged()
        {
            var editedRowIndex = _editingRowIndex;
            var editedColumnIndex = _editingColumnIndex;

            _isInEditMode = false;
            tsData.Enabled = true;

            if (editedColumnIndex >= 0 && editedColumnIndex < c1GridData.Splits[0].DisplayColumns.Count)
            {
                c1GridData.Splits[0].DisplayColumns[editedColumnIndex].AutoSize();
            }

            RefreshTableEditStateMarkers();
            CheckButtonsStatus();
            UpdateDataCellEditCommandStatus(editedRowIndex, editedColumnIndex);

            //Oracle 的自訂欄位 Editor 會在 Leave 事件中直接將值寫回 DataRow，並呼叫 c1GridData_AfterColUpdate
            //統一在所有編輯路徑共用的完成處理中，針對寫回後的實際值進行驗證
            if (_currentSourceType == DataSourceType.Oracle)
            {
                TryValidateOracleRequiredCellAfterUpdate(editedRowIndex, editedColumnIndex);
            }
        }

        private void c1GridData_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            try
            {
                var row = c1GridData.Row;
                var col = c1GridData.Col;

                if (_isDataGridFilterBarEditing)
                {
                    _isInEditMode = false;
                    tsData.Enabled = true;
                    return;
                }

                if (!IsVisibleDataGridDataRow(row))
                {
                    _isInEditMode = false;
                    tsData.Enabled = true;
                    return;
                }

                //需要透過 CellEditorForm / CellViewerForm 處理的欄位，不允許 C1 內嵌編輯器先啟動
                if (ShouldUseExternalCellDialog(row, col))
                {
                    e.Cancel = true;
                    _isInEditMode = false;
                    tsData.Enabled = true;
                    return;
                }

                _editingRowIndex = row;
                _editingColumnIndex = col;

                _isInEditMode = true;
                tsData.Enabled = false;
                ClearModifiedCellTip(c1GridData);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool ShouldUseExternalCellDialog(int row, int col)
        {
            if (row < 0 || col < 0 || col >= c1GridData.Columns.Count)
            {
                return false;
            }

            var columnName = c1GridData.Columns[col].DataField;

            if (string.IsNullOrEmpty(columnName))
            {
                return false;
            }

            if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return false;
            }

            //返回 true 代表「不允許 C1Grid 直接內嵌編輯，必須改用外部 CellEditorForm 編輯 或 CellViewerForm 檢視」
            switch (columnInfo.CategoryDataTypeKind)
            {
                case CategoryDataTypeKind.LargeBinary:
                case CategoryDataTypeKind.LargeText:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private void c1GridData_Filter(object sender, FilterEventArgs e)
        {
            var dt = c1GridData.GetDataTableSourceOrNull();

            if (dt == null)
            {
                return;
            }

            var dataView = dt.DefaultView;
            var condition = BuildDataGridFilterCondition(dt, e.Condition);

            if (dataView.RowFilter == condition)
            {
                return;
            }

            dataView.RowFilter = condition;

            _isDataGridFilterBarEditing = false;
            _isInEditMode = false;
            tsData.Enabled = true;

            BeginInvoke
            (
                new Action
                (
                    () =>
                    {
                        ApplyDataGridFilterResultState();
                    }
                )
            );
        }

        private string BuildDataGridFilterCondition(DataTable dt, string condition)
        {
            condition ??= string.Empty;

            if (condition.Length == 0 || dt == null)
            {
                return condition;
            }

            var result = condition;
            var count = dt.Columns.Count;

            for (var i = 0; i < count; i++)
            {
                var caption = dt.Columns[i].Caption;

                if (!result.Contains($"[{caption}]"))
                {
                    continue;
                }

                var parameterIndex = result.IndexOf('\'', result.IndexOf($"[{caption}]", StringComparison.Ordinal)) + 1;

                if (parameterIndex > 0 && parameterIndex <= result.Length)
                {
                    result = result.Insert(parameterIndex, "*");
                }
            }

            return result;
        }

        private void ApplyDataGridFilterResultState()
        {
            var hasVisibleRows = HasVisibleDataGridRows();

            cboFind.Enabled = hasVisibleRows;

            if (!hasVisibleRows)
            {
                ClearCurrentDataCellCommandStatus();
            }
            else
            {
                EnsureDataGridCurrentCellVisible();
            }

            RefreshTableEditStateMarkers();
            CheckButtonsStatus();
        }

        private void ClearCurrentDataCellCommandStatus()
        {
            btnSetNullData.Enabled = false;
            btnEditCellData.Enabled = false;
            btnEditCellWithEditFormData.Enabled = false;

            btnTopData.Enabled = false;
            btnPreviousData.Enabled = false;
            btnNextData.Enabled = false;
            btnLastData.Enabled = false;

            btnDuplicateCurrentRowData.Enabled = false;
            btnDeleteCurrentRowData.Enabled = false;

            c1SuperTooltip1.SetToolTip(c1GridData, string.Empty);
        }

        private void EnsureDataGridCurrentCellVisible()
        {
            try
            {
                if (c1GridData.Row < 0 || c1GridData.Row >= GetVisibleDataGridRowCount())
                {
                    c1GridData.Row = 0;
                }

                if (c1GridData.Col < 0 && c1GridData.Columns.Count > 0)
                {
                    c1GridData.Col = 0;
                }
            }
            catch
            {
                //忽略 C1TrueDBGrid 篩選後 Row/Col 還原期間的例外
            }
        }

        private void c1GridData_KeyUp(object sender, KeyEventArgs e)
        {
            var row = c1GridData.Row;
            var col = c1GridData.Col;

            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return;
            }

            if (!IsVisibleDataGridDataRow(row))
            {
                _isInEditMode = false;
                UpdateDataCellEditCommandStatus(row, col);
                return;
            }

            if (_isInEditMode && e.KeyCode == Keys.Enter)
            {
                UpdateEditMode();
            }
            else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                if (_isInEditMode)
                {
                    if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                    {
                        //20250815 如果是編輯模式，移動到其他單元格，則取消編輯模式
                        if (row != _editingRowIndex || col != _editingColumnIndex)
                        {
                            UpdateEditMode();
                        }
                    }
                    else
                    {
                        UpdateEditMode();
                    }
                }

                UpdateDataCellEditCommandStatus(row, col);
            }
        }

        private void c1GridData_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var row = c1GridData.RowContaining(e.Y);
            var col = c1GridData.ColContaining(e.X);

            if (row < 0 || col < 0)
            {
                return;
            }

            BeginInvoke
            (
                new Action
                (
                    () =>
                    {
                        OpenCellEditorFormFromDataGrid(row, col, true);
                    }
                )
            );
        }

        private void OpenCellEditorFormFromDataGrid(int row, int col, bool closeInlineEditorFirst)
        {
            if (row < 0 || col < 0)
            {
                return;
            }

            if (col >= c1GridData.Columns.Count)
            {
                return;
            }

            try
            {
                c1GridData.Row = row;
                c1GridData.Col = col;
            }
            catch
            {
                return;
            }

            if (closeInlineEditorFirst)
            {
                CloseDataGridInlineEditorBeforeCellEditorForm();
            }

            CheckEditCellWithEditForm(row, col);
        }

        private void CloseDataGridInlineEditorBeforeCellEditorForm()
        {
            if (!_isInEditMode && !c1GridData.EditActive)
            {
                return;
            }

            CancelDataGridInlineEditWithoutCommit();
        }

        private void c1GridData_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as C1TrueDBGrid;

            if (grid == null)
            {
                return;
            }

            //20260714 由 MouseMove 主動更新「異動前／後」C1SuperTooltip，FetchCellTips 僅保留為備援。
            UpdateModifiedCellTipFromMousePosition(grid, e.X, e.Y);
        }

        private void c1GridData_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return;
            }

            var row = c1GridData.RowContaining(e.Y);
            var col = c1GridData.ColContaining(e.X);

            if (row < 0 || col < 0)
            {
                return;
            }

            if (!IsVisibleDataGridDataRow(row))
            {
                return;
            }

            try
            {
                c1GridData.SelectedCols.Clear();
                c1GridData.SelectedRows.Clear();
                c1GridData.Row = row;
                c1GridData.Col = col;
            }
            catch
            {
                //忽略篩選或特殊 Grid 狀態下 Row/Col 設定失敗
            }
        }

        /// <summary>
        /// 依欄位型態，判定是否為「鎖定、不可編輯」的欄位
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns>是否為「鎖定、不可編輯」的欄位</returns>
        private bool CheckLockedColumn_PostgreSql(string columnType, out bool isBlob) //20240910 新增 bBlob 變數
        {
            var bValue = false;
            var parts = columnType.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var dataType = parts.Length > 1 ? parts[1] : string.Empty;

            if (columnType.IndexOf("\r\n", StringComparison.Ordinal) == -1)
            {
                dataType = columnType;
            }

            if (dataType.IndexOf('(') >= 0)
            {
                dataType = dataType.Substring(0, dataType.IndexOf('('));
            }

            isBlob = false;

            switch (dataType)
            {
                case "serial":
                case "bigserial":
                case "smallserial":
                case "aclitem":
                case "aclitem[]":
                case "bytea[]": //20240817 目前不知如何處理 bytea 陣列，直接列為不可編輯
                case "box":
                case "box[]":
                case "cid":
                case "cid[]":
                case "cidr":
                case "cidr[]":
                case "circle":
                case "circle[]":
                case "gtsvector":
                case "gtsvector[]":
                case "int2vector":
                case "int2vector[]":
                case "interval":
                case "interval[]":
                case "json":
                case "json[]":
                case "jsonb":
                case "jsonb[]":
                case "jsonpath":
                case "jsonpath[]":
                case "line":
                case "line[]":
                case "lseg":
                case "lseg[]":
                //case "macaddr": //20241003 視為文字
                //case "macaddr[]": //20241003 視為文字，範例 update abc set c64_macaddr_ar='{01:02:03:04:05:06,02:03:04:05:06:07}' where c01_char='1'
                //case "macaddr8": //20241003 視為文字
                //case "macaddr8[]": //20241003 視為文字，範例 update abc set c66_macaddr8_ar='{"08:00:27:03:fb:19","08:00:27:62:35:51"}' where c01_char='1' (是否有雙引號不影響)
                case "money":
                case "money[]":
                case "name":
                case "name[]":
                case "oid":
                case "oid[]":
                case "oidvector":
                case "oidvector[]":
                case "path":
                case "path[]":
                case "pg_brin_bloom_summary":
                case "pg_brin_minmax_multi_summary":
                case "pg_dependencies":
                case "pg_lsn":
                case "pg_lsn[]":
                case "pg_mcv_list":
                case "pg_ndistinct":
                case "pg_node_tree":
                case "pg_snapshot":
                case "pg_snapshot[]":
                case "point":
                case "point[]":
                case "polygon":
                case "polygon[]":
                case "refcursor":
                case "refcursor[]":
                case "regclass":
                case "regclass[]":
                case "regcollation":
                case "regcollation[]":
                case "regconfig":
                case "regconfig[]":
                case "regdictionary":
                case "regdictionary[]":
                case "regnamespace":
                case "regnamespace[]":
                case "regoper":
                case "regoper[]":
                case "regoperator":
                case "regoperator[]":
                case "regproc":
                case "regproc[]":
                case "regprocedure":
                case "regprocedure[]":
                case "regrole":
                case "regrole[]":
                case "regtype":
                case "regtype[]":
                case "tsquery":
                case "tsquery[]":
                case "tid":
                case "tid[]":
                //case "tsmultirange":
                //case "tsmultirange[]":
                //case "tsrange":
                //case "tsrange[]":
                case "tstzrange": //tstzrange('2023-04-01 12:00:00+00','2023-04-02 12:00:00+00','[]')
                case "tstzrange[]": //'[2023-01-01 12:00:00+00,2023-01-02 12:00:00+00)'
                case "tstzmultirange": //'[["2023-04-01 12:00:00+00", "2023-04-01 13:00:00+00"),("2023-04-02 14:00:00+00", "2023-04-02 15:00:00+00")]'::tstzmultirange
                case "tstzmultirange[]": //'[tstzrange(timestamp'2023-01-01 10:00:00',timestamp'2023-01-01 11:00:00','[)'),tstzrange(timestamp'2023-01-02 12:00:00',timestamp'2023-01-02 13:00:00','[)')]'::tstzmultirange[]
                case "tsvector":
                case "tsvector[]":
                case "txid_snapshot":
                case "txid_snapshot[]":
                case "uuid":
                case "uuid[]":
                case "xid":
                case "xid[]":
                case "xid8":
                case "xid8[]":
                case "xml":
                case "xml[]":
                    {
                        bValue = true;
                        break;
                    }
                case "bytea":
                    {
                        isBlob = true;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return bValue;
        }

        private void c1GridData_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
        {
            var pp = new Point(e.Col, e.Row);

            if (_modifiedCells.Contains(pp) || IsDirectBinaryCell(e.Row, e.Col))
            {
                e.Style.ForeColor = ColorTranslator.FromHtml(MyLibrary.ColorChangedCellForeColor);
                e.Style.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorChangedCellBackColor);
            }
            else if (_deletedRows.Contains(pp))
            {
                e.Style.Font = new Font(MyLibrary.GridFontName, 10, FontStyle.Strikeout);
                e.Style.ForeColor = ColorTranslator.FromHtml(MyLibrary.ColorDeletedRowForeColor);
                e.Style.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorDeletedRowBackColor);
            }
            else if (_newRows.Contains(pp))
            {
                e.Style.ForeColor = ColorTranslator.FromHtml(MyLibrary.ColorNewRowForeColor);
                e.Style.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorNewRowBackColor);
            }
            else if (_searchResultCells.Contains(pp)) //20240518 搜尋的優先權調整最低 (優先顯示異動→刪除→新增，最後才是搜尋)
            {
                e.Style.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightForeColor);
                e.Style.BackColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightBackColor);
            }
        }

        private void c1GridData_RowColChange(object sender, RowColChangeEventArgs e)
        {
            if (!_isFormLoadFinished || _dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return;
            }

            try
            {
                if (!HasVisibleDataGridRows())
                {
                    CheckButtonsStatus();
                    return;
                }

                if (!IsVisibleDataGridDataRow(c1GridData.Row))
                {
                    CheckButtonsStatus();
                    return;
                }

                var row = GetDataRowFromGridDisplayRow(c1GridData, c1GridData.Row);

                if (row == null)
                {
                    CheckButtonsStatus();
                    return;
                }

                //20250829 如果是 NEW 之後再被使用者刪除，不處理
                if (row.RowState == DataRowState.Detached)
                {
                    return;
                }

                var dtData = c1GridData.GetDataTableSourceOrNull();

                //20240502 切換 Schema 時，會是 null
                if (dtData == null)
                {
                    return;
                }

                var columnName = c1GridData.Columns[c1GridData.Col].DataField;

                if (string.IsNullOrEmpty(columnName))
                {
                    return;
                }

                if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    return;
                }

                if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
                {
                    //BLOB / bytea / binary 等等的二進制欄位必須透過專用視窗處理，不允許 C1TrueDBGrid 直接內嵌編輯
                    c1GridData.AllowUpdate = false;
                    c1GridData.AllowUpdateOnBlur = false;
                }
                else
                {
                    c1GridData.AllowUpdate = true;
                    c1GridData.AllowUpdateOnBlur = true;
                }

                //檢查是否被刪除了，是的話，Lock 整列，避免使用者異動
                var deleteValue = row.GetSafeString(_identifyColumnName);

                if (!string.Equals(deleteValue, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    for (var i = 0; i < dtData.Columns.Count; i++)
                    {
                        try
                        {
                            c1GridData.Splits[0].DisplayColumns[i].Locked = false;
                        }
                        catch
                        {
                            //20240430 遇到特殊欄位狀態時忽略
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                CheckButtonsStatus();
            }
        }

        private void c1GridStructure_FetchRowStyle(object sender, FetchRowStyleEventArgs e)
        {
            if (!TryGetGridCellText(c1GridStructure, ConstraintInfoColumnName, e.Row, out var text))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);

            if (text.StartsWith("P", StringComparison.OrdinalIgnoreCase) || text.IndexOf(", P", StringComparison.OrdinalIgnoreCase) > 0)
            {
                e.CellStyle.ForeColor = Color.Purple;
            }
            else
            {
                e.CellStyle.ForeColor = Color.Blue;
            }
        }

        private void c1GridStructure_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            if (e.Col != ConstraintIconColumnIndex)
            {
                return;
            }

            if (!TryGetGridCellText(c1GridStructure, ConstraintInfoColumnName, e.Row, out var text))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var image = GetConstraintIcon(text);

            if (image == null)
            {
                return;
            }

            e.CellStyle.ForegroundImage = image;
            e.CellStyle.ForeGroundPicturePosition = ForeGroundPicturePositionEnum.PictureOnly;
        }

        private static bool TryGetGridCellText(C1TrueDBGrid grid, string columnName, int rowIndex, out string text)
        {
            text = string.Empty;

            if (grid == null || rowIndex < 0)
            {
                return false;
            }

            C1DataColumn column = null;

            foreach (C1DataColumn item in grid.Columns)
            {
                if (string.Equals(item.DataField, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    column = item;
                    break;
                }
            }

            if (column == null)
            {
                return false;
            }

            text = column.CellText(rowIndex);
            return true;
        }

        private Image GetConstraintIcon(string constraintInfo)
        {
            if (string.IsNullOrWhiteSpace(constraintInfo))
            {
                return null;
            }

            if (IsSqlServer)
            {
                if (constraintInfo.IndexOf(", P", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Primary Key 16x16.ico");
                }
                else if (constraintInfo.IndexOf(", F", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Foreign Key 16x16.ico");
                }
                else if (constraintInfo.IndexOf(", U", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Unique 16x16.ico");
                }
                else if (constraintInfo.IndexOf(", C", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Check 16x16.ico");
                }
            }
            else
            {
                if (constraintInfo.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Primary Key 16x16.ico");
                }
                else if (constraintInfo.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Foreign Key 16x16.ico");
                }
                else if (constraintInfo.StartsWith("U", StringComparison.OrdinalIgnoreCase))
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Unique 16x16.ico");
                }
                else if (constraintInfo.StartsWith("C", StringComparison.OrdinalIgnoreCase))
                {
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Check 16x16.ico");
                }
            }

            switch (constraintInfo[0].ToString().ToUpperInvariant())
            {
                case "P":
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Primary Key 16x16.ico");

                case "F":
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Foreign Key 16x16.ico");

                case "U":
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Unique 16x16.ico");

                case "C":
                    return IconManager.GetImage(MyGlobal.IconLibrary, "Constraint Check 16x16.ico");

                default:
                    return null;
            }
        }

        private void txtSchemaFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                QuerySchema();
            }
        }

        private void c1Grid100RowsTop_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var row = c1Grid100RowsTop.RowContaining(e.Y);

            if (row != -1)
            {
                CellViewer(c1Grid100RowsTop);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SchemaBrowserFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SchemaBrowserFormHeight", Size.Height.ToString());
        }

        private void UpdateSqlHistory(int rows, string result, string message, string sql, string seqNo = "")
        {
            var execTime = string.Empty;
            var queryTime = string.Empty;

            JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), execTime, queryTime, rows, result, message, sql, _currentOperationObjectName, seqNo);
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            using (var form = new SchemaFilterDialog())
            {
                var schemaColumnTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                foreach (DataRow dr in _dtStructuredSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var columnName = dr.GetSafeString("ColumnName");
                    var dataType = dr.GetSafeString("DataType");

                    schemaColumnTypes.Add(columnName, dataType);
                }

                form.SchemaColumnTypes = schemaColumnTypes;
                form.SqlCondition = TextHelper.GetSafeString(btnFilterData.Tag);
                form.ShowDialog();

                //傳回來的 SQL Condition
                if (form.IsOKButton)
                {
                    //20240714 若判斷使用者有異動資料，要詢問使用者
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

                    if (string.IsNullOrEmpty(form.SqlCondition))
                    {
                        btnFilterData.Visible = true;
                        btnFilterRedData.Visible = false;
                    }
                    else
                    {
                        btnFilterData.Visible = false;
                        btnFilterRedData.Visible = true;
                    }

                    btnFilterData.Tag = form.SqlCondition.Trim();

                    //更新 Filter 條件
                    GetSetTableFilter(_selectedTableName, "UPDATE", TextHelper.GetSafeString(btnFilterData.Tag));

                    //20240608 更新此 Table 的過濾條件
                    if (!TextHelper.IsNullOrEmptyTag(btnFilterData.Tag))
                    {
                        btnFilterData.Visible = false;
                        btnFilterRedData.Visible = true;
                    }
                    else
                    {
                        btnFilterData.Visible = true;
                        btnFilterRedData.Visible = false;
                        btnFilterData.Tag = string.Empty;
                    }

                    RefreshData(form.SqlCondition);

                    //20240608 依查詢筆數，決定 cboFind.Enabled
                    cboFind.Enabled = c1GridData.HasDataTableRows();
                }
            }
        }

        private void btnRefreshData_Click(object sender, EventArgs e)
        {
            DisplaySchemaInfo2();
        }

        private void RefreshData(string sqlCondition)
        {
            var currentRow = c1GridData.Row;
            var currentCol = c1GridData.Col;
            var message = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");

            var form = new MessageForm
            {
                Info = message,
                BackColor = Color.LightYellow,
                ClientSize = new Size(220, 70),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                IsNeedToMovePosition = true, //20231111 加入此變數，MessageForm 顯示於螢幕中央時，視窗的位置再往上調整一些，如果有錯誤發生時，MessageBox 才不會剛好擋住 MessageForm！
                TopLevel = true //20250118 改用 TopLevel，只在 JasonQuery 最上層顯示
            };

            form.Show();
            form.Refresh();

            try
            {
                Cursor = Cursors.WaitCursor;

                //查詢前 500筆資料
                var sql = BuildTableDataPreviewSql(SchemaNode, SchemaDbo, SchemaName, sqlCondition);

                if (!string.IsNullOrEmpty(sql))
                {
                    ExecuteQuery100Rows(sql, 0, 500, false);

                    SetGridFormat();

                    //20240714 自動調整欄寬
                    GridHelper.ResizeGridColumnWidth(c1GridData);
                    GridHelper.SetGridHeaderLine(c1GridData);

                    c1GridData.Row = currentRow;
                    c1GridData.Col = currentCol;
                    c1GridData.Select();
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                form.Dispose();
                Cursor = Cursors.Default;
            }
        }

        private void CellEditor(C1TrueDBGrid c1Grid)
        {
            _cellEditorResult = null; //清空，確保不殘留前一次的值

            if (c1Grid == null || c1Grid.Row < 0 || c1Grid.Col < 0)
            {
                return;
            }

            var columnName = c1Grid.Columns[c1Grid.Col].DataField;

            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }

            if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return;
            }

            //20241009 取得目前儲存格的座標
            _editingRowIndex = c1GridData.Row;
            _editingColumnIndex = c1GridData.Col;

            var originalCellText = GetCellEditorOriginalText(c1Grid, c1Grid.Row, c1Grid.Col);

            using (var form = new CellEditorForm())
            {
                form.ColumnName = columnName;
                form.ColumnType = columnInfo.BaseDataType;
                form.OriginalCellText = originalCellText;
                form.Result = originalCellText;
                form.CanSetNull = columnInfo.IsNullable;
                form.LengthTotal = columnInfo.ColumnSize;
                form.LengthSet = columnInfo.ColumnSize;
                form.ColumnInformation = columnInfo;

                PrepareCellEditorBinaryMode(form, c1Grid, c1Grid.Row, c1Grid.Col, columnInfo);

                form.CellValueChanged += CellEditorForm_CellValueChanged;
                form.BinaryValueApplied += CellEditorForm_BinaryValueApplied;

                //20250426 改寫 Width/Height 取值方法
                var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "CellEditorFormWidth", "CellEditorFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                form.ShowDialog();

                form.CellValueChanged -= CellEditorForm_CellValueChanged;
                form.BinaryValueApplied -= CellEditorForm_BinaryValueApplied;
            }

            c1Grid.Row = _editingRowIndex;
            c1Grid.Col = _editingColumnIndex;
            c1Grid.Select();
        }

        private void CellEditorForm_CellValueChanged(object sender, CellEditorValueChangedEventArgs e)
        {
            ApplyCellEditorValueToGrid(e.Value);
        }

        private void ApplyCellEditorValueToGrid(string value)
        {
            if (_dtTableData == null || _dtTableData.Rows.Count == 0)
            {
                return;
            }

            if (_editingRowIndex < 0 || _editingColumnIndex < 0)
            {
                return;
            }

            if (_editingColumnIndex >= c1GridData.Columns.Count)
            {
                return;
            }

            var columnName = c1GridData.Columns[_editingColumnIndex].DataField;

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return;
            }

            var dataRow = GetDataRowFromGridDisplayRow(c1GridData, _editingRowIndex);

            if (dataRow == null || dataRow.Table == null || !dataRow.Table.Columns.Contains(columnName))
            {
                return;
            }

            if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo)
                && IsReadOnlyLargeTextColumn(columnInfo))
            {
                //防禦性保護：LargeText 不應進入 CellEditorForm，也不接受其 Apply 回寫
                //JasonQuery 所自動產生的異動 SQL 若包含大型長文字，可能意外引發字串 literal 長度限制與安全問題
                //甚至可能會讓使用者誤以為這是 JasonQuery 的 bug
                return;
            }

            var newValue = value ?? string.Empty;

            dataRow[columnName] = newValue;
            dataRow.EndEdit();

            _cellEditorResult = newValue;

            ClearModifiedCellTip(c1GridData); //避免同一個 Cell 沿用修改前建立的 Tooltip 快取

            RefreshTableEditStatusAfterCellValueChanged();
            RefreshDataGridDisplayAfterCellEditorUpdate();
        }

        private void SelectColor_Click(object sender, EventArgs e)
        {
            if (!(sender is Panel pnlSelected))
            {
                return;
            }

            _panelColorSelectedName = pnlSelected.Name;
            ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ApplySelectedTabColor);
        }

        private void ApplySelectedTabColor(Color selectedColor)
        {
            var hexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor);

            foreach (var t in _colorPanelControls.Where(t => ((Panel)t).Name == _panelColorSelectedName))
            {
                ((Panel)t).BackColor = selectedColor;
                ((Panel)t).Tag = hexColor;

                var caption = $"{hexColor} (R:{selectedColor.R}, G:{selectedColor.G}, B:{selectedColor.B})";

                _toolTip1.SetToolTip((Panel)t, caption);

                switch (((Panel)t).Name)
                {
                    case "pnlNewRowForeColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_NewRowForeColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorNewRowForeColor = hexColor;
                            break;
                        }
                    case "pnlNewRowBackColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_NewRowBackColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorNewRowBackColor = hexColor;
                            break;
                        }
                    case "pnlDeletedRowForeColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_DeletedRowForeColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorDeletedRowForeColor = hexColor;
                            break;
                        }
                    case "pnlDeletedRowBackColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_DeletedRowBackColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorDeletedRowBackColor = hexColor;
                            break;
                        }
                    case "pnlChangedCellForeColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_ChangedCellForeColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorChangedCellForeColor = hexColor;
                            break;
                        }
                    case "pnlChangedCellBackColor":
                        {
                            JasonQueryRepository.UpdateSetting("GlobalConfig", "EditTableData_ChangedCellBackColor", TextHelper.GetSafeString(((Panel)t).Tag));
                            MyLibrary.ColorChangedCellBackColor = hexColor;
                            break;
                        }
                }

                RefreshTableEditStateMarkers();
                break;
            }
        }

        private void chkShowFilterRow_CheckedChanged(object sender, EventArgs e)
        {
            c1GridData.FilterBar = chkShowFilterRowData.Checked;
        }

        private void btnShowColumns_Click(object sender, EventArgs e)
        {
            splitContainer3.Panel1Collapsed = !splitContainer3.Panel1Collapsed;
        }

        private void txtColumnFilter_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                GridColumnFilterHelper.ApplyColumnNameFilterOnEnter(c1GridColumns, txtColumnFilter, e);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void txtColumnFilter_MouseClick(object sender, MouseEventArgs e)
        {
            GridColumnFilterHelper.SelectAllText(txtColumnFilter);
        }

        private void c1GridColumns_MouseUp(object sender, MouseEventArgs e)
        {
            var displayRowIndex = c1GridColumns.RowContaining(e.Y);

            if (displayRowIndex == -1) //Exclude row headers
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            GridColumnNavigatorHelper.PositionColumn(c1GridColumns, c1GridData, displayRowIndex);
        }

        private void cboFind_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadFindList("Grid", cboFind);
        }

        private void cboFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 || string.IsNullOrWhiteSpace(cboFind.Text))
            {
                return;
            }

            UpdateFindList();

            cboFind.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            btnFindNextData.PerformClick();
        }

        private void cboFind_KeyUp(object sender, KeyEventArgs e)
        {
            var enabled = !string.IsNullOrWhiteSpace(cboFind.Text);

            btnFindNextData.Enabled = enabled;
            btnFindPreviousData.Enabled = enabled;
            btnCountData.Enabled = enabled;
            btnHighlightData.Enabled = enabled;
            btnClearHighlightData.Enabled = enabled;
        }

        private void cboFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFind.Text))
            {
                return;
            }

            btnFindNextData.Enabled = true;
            btnFindPreviousData.Enabled = true;
            btnCountData.Enabled = true;
            btnHighlightData.Enabled = true;
            btnClearHighlightData.Enabled = true;

            cboFind.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
        }

        private void cboFind_TextChanged(object sender, EventArgs e)
        {
            var enabled = false;

            if (!string.IsNullOrWhiteSpace(cboFind.Text))
            {
                enabled = true;
                cboFind.Tag = string.Empty;
            }

            btnFindNextData.Enabled = enabled;
            btnFindPreviousData.Enabled = enabled;
            btnCountData.Enabled = enabled;
            btnHighlightData.Enabled = enabled;
            btnClearHighlightData.Enabled = enabled;
        }

        private int CountGrid(bool showAlertOnError = false)
        {
            var count = GridHelper.CountGridOccurrence(c1GridData, cboFind.Text, 0);

            if (!showAlertOnError)
            {
                return count;
            }

            var temp1 = LocalizationHelper.GetLanguageString("Find What:", "form", GetType().Name, "msg", "FindWhat", "Text");
            var temp2 = LocalizationHelper.GetLanguageString("Count:", "form", GetType().Name, "msg", "Count", "Text");
            var temp3 = LocalizationHelper.GetLanguageString("matches.", "form", GetType().Name, "msg", "matches", "Text");

            MessageBoxHelper.ShowNearCursor($"{temp1} {cboFind.Text}\r\n\r\n{temp2} {count} {temp3}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return count;
        }

        private void UpdateFindList() //判斷是否要更新「搜尋清單」
        {
            if (TextHelper.GetSafeString(lblFindData.Tag) == cboFind.Text)
            {
                return;
            }

            if (cboFind.Items.Count > 0 && cboFind.Text == cboFind.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFind.Text);
            }

            lblFindData.Tag = cboFind.Text;
        }

        private void LoadFindList(string sFun, C1ComboBox objComboBox)
        {
            objComboBox.Items.Clear();

            try
            {
                var i = 0;
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = 'FindList_EditTable_{sFun}'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtFindList = JasonQueryRepository.ExecQuery(sql);

                if (dtFindList.Rows.Count <= 0)
                {
                    return;
                }

                for (var row = 0; row < dtFindList.Rows.Count; row++)
                {
                    if (i > 20)
                    {
                        break;
                    }

                    objComboBox.Items.Add(dtFindList.Rows[row]["AttributeValue"].ToString());
                    i++;
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void SaveFindList(string function, string findText)
        {
            findText = findText.Replace("'", "''");

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey = 'FindList_EditTable_{function}'");
            sbSql.Append($"   AND AttributeValue = '{findText}'");

            var sql = sbSql.ToString();
            var dtFindList = JasonQueryRepository.ExecQuery(sql);

            if (dtFindList?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = 'FindList_EditTable_{function}'");
                sbSql.Append($"   AND AttributeValue = '{findText}'");

                sql = sbSql.ToString();
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'FindList_EditTable_{function}', '{findText}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
            }

            JasonQueryRepository.ExecNonQuery(sql);

            //Reload
            LoadFindList(function, cboFind);
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFind.Text))
            {
                cboFind.Text = string.Empty;
                return;
            }

            if (TextHelper.IsNullOrEmptyTag(cboFind.Tag))
            {
                cboFind.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFind.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");
                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFind.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindList();

            if (!FindNext())
            {
                return;
            }

            //往下找不到資料，而且不是在第一格位置，再從頭開始找一次
            if (c1GridData.Row != 0 && c1GridData.Col != 0)
            {
                FindNext(true);
            }
        }

        private bool FindNext(bool isFindAgain = false)
        {
            var searchText = cboFind.Text;
            var findRow = 0;
            var findCol = 0;
            var isFound = false;
            bool result;
            var currentRowStart = c1GridData.Row;
            var currentColStart = c1GridData.Col;

            if (isFindAgain)
            {
                currentRowStart = 0;
                currentColStart = 0;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return false;
            }

            if (cboFind.Items.Count > 0 && cboFind.Text == cboFind.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFind.Text);
            }

            for (var row = currentRowStart; row < c1GridData.Splits[0].Rows.Count; row++)
            {
                var vr = c1GridData.Splits[0].Rows[row];
                var col = 0;

                foreach (C1DataColumn column in c1GridData.Columns)
                {
                    col++;

                    //忽略前後兩個欄位！
                    if (col == 1 || col == c1GridData.Columns.Count)
                    {
                        continue;
                    }

                    var cellText = column.CellText(vr.DataRowIndex);
                    var findText = cboFind.Text;

                    if (cellText.IndexOf(findText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (row == currentRowStart) //游標所在列，尋找下一個，要略過此格！
                        {
                            if (col - 1 > currentColStart)
                            {
                                findRow = row;
                                findCol = col - 1;
                                isFound = true;
                                break;
                            }
                        }
                        else
                        {
                            findRow = row;
                            findCol = col - 1;
                            isFound = true;
                            break;
                        }
                    }
                }

                if (isFound)
                {
                    break;
                }
            }

            if (isFound)
            {
                c1GridData.Row = findRow;
                c1GridData.Col = findCol;
                c1GridData.Select(); //Focus 切換到指定的 Cell
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void btnFindPrevious_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFind.Text))
            {
                cboFind.Text = string.Empty;
                return;
            }

            if (TextHelper.IsNullOrEmptyTag(cboFind.Tag))
            {
                cboFind.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFind.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");
                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFind.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindList();

            if (FindPrevious())
            {
                //往上找不到資料，再從底部開始找一次
                FindPrevious(true);
            }
        }

        private bool FindPrevious(bool isFindAgain = false)
        {
            var searchText = cboFind.Text;
            var currentRowStart = c1GridData.Row;
            var currentColStart = c1GridData.Col;
            var findRow = 0;
            var findCol = 0;
            var isFound = false;
            bool result;

            if (isFindAgain)
            {
                currentRowStart = c1GridData.Splits[0].Rows.Count - 1;
                currentColStart = c1GridData.Splits[0].DisplayColumns.Count - 1;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return false;
            }

            if (cboFind.Items.Count > 0 && cboFind.Text == cboFind.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFind.Text);
            }

            for (var row = currentRowStart; row >= 0; row--)
            {
                var count = c1GridData.Splits[0].DisplayColumns.Count - 1;

                for (var i = count; i >= 0; i--)
                {
                    //忽略前後兩個欄位！
                    if (i == 0 || i == c1GridData.Columns.Count - 1)
                    {
                        continue;
                    }

                    var cellText = c1GridData.Columns[i].CellValue(row).ToString();
                    var findText = cboFind.Text;

                    if (cellText.IndexOf(findText, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        continue;
                    }

                    if (row == currentRowStart) //游標所在列
                    {
                        if ((isFindAgain || i >= currentColStart) && (!isFindAgain || i > currentColStart))
                        {
                            continue;
                        }

                        findRow = row;
                        findCol = i;
                        isFound = true;
                        break;
                    }

                    findRow = row;
                    findCol = i;
                    isFound = true;
                    break;
                }

                if (isFound)
                {
                    break;
                }
            }

            if (isFound)
            {
                c1GridData.Row = findRow;
                c1GridData.Col = findCol;
                c1GridData.Select(); //Focus 切換到指定的 Cell
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void btnCount_Click(object sender, EventArgs e)
        {
            var count = GridHelper.CountGridOccurrence(c1GridData, cboFind.Text, 0);
            var temp1 = LocalizationHelper.GetLanguageString("Find What:", "form", GetType().Name, "msg", "FindWhat", "Text");
            var temp2 = LocalizationHelper.GetLanguageString("Count:", "form", GetType().Name, "msg", "Count", "Text");
            var temp3 = LocalizationHelper.GetLanguageString("matches.", "form", GetType().Name, "msg", "matches", "Text");

            MessageBoxHelper.ShowNearCursor($"{temp1} {cboFind.Text}\r\n\r\n{temp2} {count} {temp3}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHighlight_Click(object sender, EventArgs e)
        {
            btnClearHighlightData.PerformClick();

            var tag = TextHelper.GetSafeString(cboFind.Tag);

            if (string.IsNullOrEmpty(tag))
            {
                cboFind.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFind.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");
                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFind.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindList();

            _searchResultCells.Clear();

            DataTable dt = null;

            //判斷是否有篩選
            if (c1GridData.FocusedSplit.Rows[c1GridData.Row].RowType == RowTypeEnum.DataRow)
            {
                var dr = (DataRowView)c1GridData[c1GridData.RowBookmark(c1GridData.Row)];

                //取得篩選後的數據
                dt = dr.DataView[0].Row.Table.DefaultView.ToTable();
            }
            else
            {
                dt = c1GridData.GetDataTableSourceOrNull();
            }

            var findText = cboFind.Text;
            int rowsCount = dt.Rows.Count;
            int start = 1;
            int end = dt.Columns.Count - 1;

            for (int row = 0; row < rowsCount; row++)
            {
                var dr = dt.Rows[row];

                for (int col = start; col < end; col++) //忽略第一欄及最後一欄！
                {
                    var value = dr.GetSafeString(col);

                    if (value.IndexOf(findText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _searchResultCells.Add(new Point(col, row));
                    }
                }
            }

            if (_searchResultCells.Count == 0)
            {
                return;
            }

            btnHighlightData.Tag = "1";

            foreach (C1DisplayColumn col in c1GridData.Splits[0].DisplayColumns)
            {
                col.OwnerDraw = true;
            }

            RefreshTableEditStateMarkers();
        }

        private void btnClearHighlight_Click(object sender, EventArgs e)
        {
            foreach (C1DisplayColumn col in c1GridData.Splits[0].DisplayColumns)
            {
                col.OwnerDraw = false;
            }

            btnHighlightData.Tag = "0";
            _searchResultCells.Clear();
            RefreshTableEditStateMarkers();
        }

        private string GetSetTableFilter(string tableName, string mode = "SELECT", string filter = "")
        {
            var result = string.Empty;
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.Append($"   AND AttributeKey = '{tableName}_Filter'");

            var sql = sbSql.ToString();
            var dtFilter = JasonQueryRepository.ExecQuery(sql);

            filter = filter.Replace("'", "''");

            if (dtFilter?.Rows.Count > 0)
            {
                if (mode == "SELECT")
                {
                    result = dtFilter.Rows[0].GetSafeString(0);
                }
                else
                {
                    sbSql.Clear();
                    sbSql.AppendLine("UPDATE SystemConfig");
                    sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}', AttributeValue = '{filter}'");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.Append($"   AND AttributeKey = '{tableName}_Filter'");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }
            }
            else
            {
                if (mode != "SELECT")
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                    sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, '{tableName}_Filter', '{filter}', '{MyGlobal.DateTimeNow()}')");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }
            }

            return result;
        }

        private void btnSelectAllSqlPreview_Click(object sender, EventArgs e)
        {
            SelectAllSqlPreview();
        }

        private void SelectAllSqlPreview()
        {
            editorSqlPreview.SelectionStart = 0;
            editorSqlPreview.SelectionEnd = editorSqlPreview.Text.Length;
        }

        private void btnCopySqlPreview_Click(object sender, EventArgs e)
        {
            CopySqlPreview();
        }

        private void CopySqlPreview()
        {
            if (string.IsNullOrEmpty(editorSqlPreview.Text))
            {
                return;
            }

            CopySqlPreviewSelection("CopySqlPreview");
        }

        private void btnSaveAsSqlPreview_Click(object sender, EventArgs e)
        {
            SaveAsSqlPreview();
        }

        private void SaveAsSqlPreview()
        {
            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                FileName = string.Empty,
                RestoreDirectory = true,
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return; //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
            }

            if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
            {
                sf.FileName += ".sql";
            }

            try
            {
                TextEngine.WriteContentToFile(editorSqlPreview.Text, sf.FileName, TextEncodes.UTF8);
            }
            catch (Exception ex)
            {
                var temp = LocalizationHelper.GetLanguageString("Please check if this file is opened in another program.", "form", GetType().Name, "msg", "SaveFailedCheck", "Text");

                MessageBox.Show($"{temp}\r\n\r\n{sf.FileName}\r\n\r\n{ex.Message}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWordWrapSqlPreview_Click(object sender, EventArgs e)
        {
            WordWrapSqlPreview();
        }

        private void WordWrapSqlPreview()
        {
            btnWordWrapSqlPreview.Visible = editorSqlPreview.WrapMode == WrapMode.Word;
            btnWordWrap2SqlPreview.Visible = !btnWordWrapSqlPreview.Visible;
            editorSqlPreview.WrapMode = editorSqlPreview.WrapMode == WrapMode.Word ? WrapMode.None : WrapMode.Word;
            editorSqlPreview.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? WrapVisualFlags.Start : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? WrapVisualFlags.End : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? WrapVisualFlags.Margin : WrapVisualFlags.None);

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSqlPreview.ScrollCaret();
        }

        private void btnShowAllCharactersSqlPreview_Click(object sender, EventArgs e)
        {
            ShowallCharactersSqlPreview();
        }

        private void ShowallCharactersSqlPreview()
        {
            btnShowAllCharactersSqlPreview.Visible = editorSqlPreview.ViewEol;
            btnShowAllCharacters2SqlPreview.Visible = !btnShowAllCharactersSqlPreview.Visible;
            editorSqlPreview.ViewEol = !editorSqlPreview.ViewEol;
            editorSqlPreview.ViewWhitespace = btnShowAllCharactersSqlPreview.Visible ? ScintillaNET.WhitespaceMode.Invisible : ScintillaNET.WhitespaceMode.VisibleAlways;
        }

        private void btnZoomInSqlPreview_Click(object sender, EventArgs e)
        {
            editorSqlPreview.ZoomIn();
        }

        private void btnZoomOutSqlPreview_Click(object sender, EventArgs e)
        {
            editorSqlPreview.ZoomOut();
        }

        private void btnHelp_ColumnName_Click(object sender, EventArgs e)
        {
            _languageText = LocalizationHelper.GetLanguageString("After clicking on the column name, you can quickly switch to the specified column.", "form", GetType().Name, "msg", "Help_ColumnName", "Text");
            MessageBoxHelper.ShowNearCursor(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnShowAllCharactersSqlPane_Click(object sender, EventArgs e)
        {
            if (editorSqlPane.ViewEol)
            {
                btnShowAllCharactersSqlPane.Visible = true;
                btnShowAllCharacters2SqlPane.Visible = false;
                editorSqlPane.ViewEol = false;
                editorSqlPane.ViewWhitespace = ScintillaNET.WhitespaceMode.Invisible;
            }
            else
            {
                btnShowAllCharactersSqlPane.Visible = false;
                btnShowAllCharacters2SqlPane.Visible = true;
                editorSqlPane.ViewEol = true;
                editorSqlPane.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            }
        }

        private void btnZoomInSqlPane_Click(object sender, EventArgs e)
        {
            editorSqlPane.ZoomIn();
        }

        private void btnZoomOutSqlPane_Click(object sender, EventArgs e)
        {
            editorSqlPane.ZoomOut();
        }

        private void editorSqlPane_DoubleClick(object sender, DoubleClickEventArgs e)
        {
            HighlightSelection_SqlPane(true);
        }

        private void editorSqlPreview_DoubleClick(object sender, DoubleClickEventArgs e)
        {
            HighlightSelection_SqlPreview(true);
        }

        private void HighlightSelection_SqlPane(bool isMouseClick = false)
        {
            HighlightSqlEditorSelection(editorSqlPane, ClearSqlPaneHighlightSelectionCopyState,
                                        word =>
                                        {
                                            _selectedTextOnDoubleClickSqlPane = word;
                                            _isSqlPaneHighlightSelectionCopyMode = true;
                                        });
        }

        private void HighlightSelection_SqlPreview(bool isMouseClick = false)
        {
            HighlightSqlEditorSelection(editorSqlPreview, ClearSqlPreviewHighlightSelectionCopyState,
                                        word =>
                                        {
                                            _selectedTextOnDoubleClickSqlPreview = word;
                                            _isSqlPreviewHighlightSelectionCopyMode = true;
                                        });
        }

        private C1TrueDBGrid GetCurrentGridOrNull()
        {
            return GridHelper.GetFocusedGridOrNull(c1GridStructure, c1Grid100RowsTop, c1GridData);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var isActiveCell = true; //是否為「只點選單一個 cell，並沒有『選取範圍』」?
            var focusedControl = this.ActiveControl;

            switch (keyData)
            {
                case Keys.Control | Keys.C: //Ctrl+C
                case Keys.Control | Keys.Insert: //20231020 Ctrl+Insert
                    {
                        //判斷要針對哪一個元件進行「複製」
                        if (c1GridStructure.Focused || c1Grid100RowsTop.Focused || c1GridData.Focused)
                        {
                            CopyDataFromDataGrid();
                            return true;
                        }
                        else if (c1GridSchemaBrowser.Focused)
                        {
                            var c1Grid = c1GridSchemaBrowser;
                            var currentRow = c1Grid.Row;
                            var i = 0;
                            var selectedColsCount = c1Grid.SelectedCols.Count;
                            var selectedRowsCount = c1Grid.SelectedRows.Count;
                            var vr = c1Grid.Splits[0].Rows[currentRow];
                            var temp = string.Empty;

                            //判斷是不是節點
                            if (selectedRowsCount == 0 && ((c1Grid.Splits[0].Rows[currentRow].RowType == RowTypeEnum.CollapsedGroupRow) || (c1Grid.Splits[0].Rows[currentRow].RowType == RowTypeEnum.ExpandedGroupRow)))
                            {
                                isActiveCell = false;
                                temp = ((GroupRow)vr).GroupedText;

                                var tempValue = temp.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                                if (tempValue >= 0)
                                {
                                    temp = temp.Substring(0, tempValue);
                                }
                            }
                            else
                            {
                                foreach (int row in c1Grid.SelectedRows)
                                {
                                    if (selectedColsCount == 0) //整列選取
                                    {
                                        isActiveCell = false;

                                        foreach (C1DataColumn column in c1Grid.Columns)
                                        {
                                            vr = c1Grid.Splits[0].Rows[row];

                                            var caption = column.Caption;
                                            var text = column.CellText(vr.DataRowIndex);

                                            switch (i)
                                            {
                                                case 0 when caption == "SchemaType":
                                                case 0 when caption == "SchemaName":
                                                    {
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        if (caption != "SchemaObject" && caption != "SchemaNode" && caption != "SchemaType" && caption != "SchemaName")
                                                        {
                                                            temp += $"{text}, ";
                                                        }

                                                        break;
                                                    }
                                            }
                                        }

                                        i++;
                                    }
                                    else
                                    {
                                        vr = c1Grid.Splits[0].Rows[row];

                                        //使用者可能會同時選取「節點」、「非節點」
                                        if (c1Grid.Splits[0].Rows[row].RowType == RowTypeEnum.CollapsedGroupRow || c1Grid.Splits[0].Rows[row].RowType == RowTypeEnum.ExpandedGroupRow)
                                        {
                                            temp = $"{((GroupRow)vr).GroupedText}, ";
                                        }
                                        else
                                        {
                                            foreach (C1DataColumn column in c1Grid.SelectedCols)
                                            {
                                                isActiveCell = false;

                                                if (string.IsNullOrWhiteSpace(column.ToString()))
                                                {
                                                    temp += $"{c1Grid[vr.DataRowIndex, column.DataField]}, ";
                                                }
                                                else
                                                {
                                                    temp += $"{c1Grid[vr.DataRowIndex, column.ToString()]}, ";
                                                }
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(temp))
                                    {
                                        temp = $"{temp.TrimEnd(',', ' ')}\r\n";
                                    }
                                }
                            }

                            if (isActiveCell)
                            {
                                temp = c1Grid[c1Grid.Splits[0].Rows[c1Grid.Row].DataRowIndex, c1Grid.Col].ToString();

                                if (c1GridSchemaBrowser.Focused)
                                {
                                    temp = lblSchemaName2.Text;
                                }

                                var tempValue = temp.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                                if (c1Grid == c1GridSchemaBrowser && tempValue >= 0)
                                {
                                    temp = temp.Substring(0, tempValue);
                                }
                            }

                            Clipboard.SetDataObject(temp, false);
                            return true;
                        }
                        else if (editorSqlPane.Focused)
                        {
                            CopySqlPane();
                            return true;
                        }
                        else if (editorSqlPreview.Focused)
                        {
                            CopySqlPreview();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.N: //Ctrl+N
                    {
                        TransferValueToMainForm("CreateNewTab`");
                        return true;
                    }
                case Keys.Control | Keys.O: //Ctrl+O
                    {
                        //20191005 按快速鍵，改為「在新的頁籤開啟」
                        TransferValueToMainForm("CreateNewTab`OPENFILE");
                        return true;
                    }
                case Keys.Control | Keys.F: //Ctrl+F 尋找
                    {
                        _findAndReplace.ShowFind();
                        return true;
                    }
                case Keys.Control | Keys.H: //Ctrl+H 取代
                    {
                        _findAndReplace.ShowReplace();
                        return true;
                    }
                case Keys.F2: //20240911 for 資料編輯
                    {
                        if (c1GridData.Focused)
                        {
                            btnEditCellData.PerformClick();
                            return true;
                        }

                        break;
                    }
                case Keys.Shift | Keys.Enter: //20241006 for 資料編輯(開啟編輯視窗), 20250114 修改快捷鍵 → Shift + Enter
                    {
                        if (c1GridData.Focused)
                        {
                            EditCellWithEditForm();
                            return true;
                        }

                        break;
                    }
                case Keys.F3:
                    {
                        if (editorSqlPane.Focused)
                        {
                            _findAndReplace.Window.FindNext(true);
                            return true;
                        }

                        break;
                    }
                case Keys.Shift | Keys.F3:
                    {
                        if (editorSqlPane.Focused)
                        {
                            _findAndReplace.Window.FindNext(false);
                            return true;
                        }

                        break;
                    }
                case Keys.F12:
                    {
                        if (tabSchemaBrowser.SelectedTab.Name == nameof(tabSqlPane))
                        {
                            SaveAsSqlPane();
                            return true;
                        }
                        else if (tabSchemaBrowser.SelectedTab.Name == nameof(tabSqlPreview))
                        {
                            SaveAsSqlPreview();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.Home: //20231024 Ctrl+Home
                    {
                        if (c1GridStructure.Focused)
                        {
                            c1GridStructure.Row = 0;
                            c1GridStructure.Col = 0;
                            c1GridStructure.Select();
                            return true;
                        }
                        else if (c1Grid100RowsTop.Focused)
                        {
                            c1Grid100RowsTop.Row = 0;
                            c1Grid100RowsTop.Col = 0;
                            c1Grid100RowsTop.Select();
                            return true;
                        }
                        else if (c1GridData.Focused)
                        {
                            c1GridData.Row = 0;
                            c1GridData.Col = 0;
                            c1GridData.Select();
                            return true;
                        }
                        else if (c1GridColumns.Focused)
                        {
                            c1GridColumns.Row = 0;
                            c1GridColumns.Col = 0;
                            c1GridColumns.Select();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.End: //20231024 Ctrl+End
                    {
                        if (c1GridStructure.Focused)
                        {
                            c1GridStructure.Row = c1GridStructure.Splits[0].Rows.Count - 1;
                            c1GridStructure.Col = c1GridStructure.Splits[0].DisplayColumns.Count - 1;
                            c1GridStructure.Select();
                            return true;
                        }
                        else if (c1Grid100RowsTop.Focused)
                        {
                            c1Grid100RowsTop.Row = c1Grid100RowsTop.Splits[0].Rows.Count - 1;
                            c1Grid100RowsTop.Col = c1Grid100RowsTop.Splits[0].DisplayColumns.Count - 1;
                            c1Grid100RowsTop.Select();
                            return true;
                        }
                        else if (c1GridData.Focused)
                        {
                            c1GridData.Row = c1GridData.Splits[0].Rows.Count - 1;
                            c1GridData.Col = c1GridData.Splits[0].DisplayColumns.Count - 1;
                            c1GridData.Select();
                            return true;
                        }
                        else if (c1GridColumns.Focused)
                        {
                            c1GridColumns.Row = c1GridColumns.Splits[0].Rows.Count - 1;
                            c1GridColumns.Col = c1GridColumns.Splits[0].DisplayColumns.Count - 1;
                            c1GridColumns.Select();
                            return true;
                        }

                        break;
                    }
                case Keys.Alt | Keys.S: //20241006 Set to Null
                    {
                        if (btnSetNullData.Enabled)
                        {
                            SetToNull();
                            return true;
                        }

                        break;
                    }
                case Keys.Alt | Keys.Insert: //20240718 插入新列
                    {
                        if (tabSchemaBrowser.SelectedTab == tabData && btnInsertNewRowData.Enabled)
                        {
                            btnInsertNewRowData.PerformClick();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.Alt | Keys.Insert: //20240718 複製目前列
                    {
                        if (tabSchemaBrowser.SelectedTab == tabData && btnDuplicateCurrentRowData.Enabled)
                        {
                            btnDuplicateCurrentRowData.PerformClick();
                            return true;
                        }

                        break;
                    }
                case Keys.Alt | Keys.Delete: //20240718 刪除目前列
                    {
                        if (tabSchemaBrowser.SelectedTab == tabData && btnDeleteCurrentRowData.Enabled)
                        {
                            btnDeleteCurrentRowData.PerformClick();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.A: //20250710 加入 Ctrl+A
                    {
                        return false;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static class SqlPaneColumn
        {
            public const int SelectAll = 0;
            public const int Copy = 1;
            public const int SaveAs = 2;
        }

        private static class SqlPreviewColumn
        {
            public const int SelectAll = 0;
            public const int Copy = 1;
            public const int SaveAs = 2;
            public const int Dash1 = 3;
            public const int Apply = 4;
            public const int Cancel = 5;
            public const int Dash3 = 6;
            public const int Commit = 7;
            public const int Rollback = 8;
        }

        private static class GridColumn
        {
            public const int SelectAll = 0;
            public const int Copy = 1;
            public const int Dash1 = 2;
            public const int ExportToFile = 3;
            public const int Dash2 = 4;
            public const int EditCell = 5;
            public const int EditCellWithEditForm = 6;
            public const int SetNull = 7;
            public const int InsertNewRow = 8;
            public const int DuplicateCurrentRow = 9;
            public const int DeleteCurrentRow = 10;
            public const int Dash3 = 11;
            public const int SqlPreview = 12;
            public const int Dash4 = 13;
            public const int ApplyEdit = 14;
            public const int Dash5 = 15;
            public const int Commit = 16;
            public const int Rollback = 17;
            public const int Dash6 = 18;
            public const int Refresh = 19;
        }
    }
}
