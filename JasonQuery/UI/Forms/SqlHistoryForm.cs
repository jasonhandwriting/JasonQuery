using C1.Win.C1TrueDBGrid;
using C1.Win.C1TrueDBGrid.Excel;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Events;
using JasonLibrary.UI.Controls;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.Formatters;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Editor.FindAndReplace;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SqlHistoryForm : Form
    {
        private DataTable _dtSqlHistoryInfo;
        private DataRow _rowDbInfo;
        private DataTable _dt;
        public event ValueUpdatedEventHandler ValueUpdated;
        private ContextMenuStrip _cMenuGrid = new ContextMenuStrip(); //Grid
        private ContextMenuStrip _cMenuMessage = new ContextMenuStrip(); //Editor - Message
        private ContextMenuStrip _cMenuSql = new ContextMenuStrip(); //Editor - Sql
        private List<string> _lstGridHeader = new List<string>();

        private FindAndReplace myFindReplace; //20230629
        private int _rowHeight; //original row height
        private int _recSelWidth; //oringal record selector width
        private float _fontSize; //original font size

        private bool _isCtrlKeyDown;
        private int _totalDelta;
        private bool _isFormLoadFinished; //表單是否載入完畢 (避免觸發事件)
        private int _iSelectedCount;

        private Dictionary<string, string> _dicHistoryPeriod = new Dictionary<string, string>();

        public SqlHistoryForm()
        {
            InitializeComponent();
        }

        private void ApplyLocalizationSetting()
        {
            LocalizationHelper.ApplyLanguageInfo(this);

            var toolTip1 = new ToolTip
            {
                ForeColor = Color.Blue,
                BackColor = Color.Gray,
                AutoPopDelay = 5000
            };

            var languageText = string.Empty;

            languageText = LocalizationHelper.GetLanguageString("Search SQL History", "form", GetType().Name, "object", "btnSearch", "ToolTipText");
            toolTip1.SetToolTip(btnSearch, languageText);
            languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "object", "btnSelectAll", "ToolTipText");
            toolTip1.SetToolTip(btnSelectAll, languageText);
            languageText = LocalizationHelper.GetLanguageString("Unselect All", "form", GetType().Name, "object", "btnUnselectAll", "ToolTipText");
            toolTip1.SetToolTip(btnUnselectAll, languageText);
            languageText = LocalizationHelper.GetLanguageString("Delete Selected Records", "form", GetType().Name, "object", "btnDelete", "ToolTipText");
            toolTip1.SetToolTip(btnDelete, languageText);

            _isFormLoadFinished = false;

            //以下顏色要重新指定，否則 CheckBox 底色會不一樣
            toolStrip1.BackColor = SystemColors.Control;
            chkShowFilterRow.BackColor = toolStrip1.BackColor;
            toolStrip2.BackColor = SystemColors.Control;
            chkCopyAsHTML.BackColor = toolStrip2.BackColor;
            pnlHideCheckBox.BackColor = Color.White;

            if (MyLibrary.IsDarkMode)
            {
                C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";

                chkShowFilterRow.Visible = !MyLibrary.IsDarkMode;
                chkShowFilterRow.Visible = MyLibrary.IsDarkMode;
                chkCopyAsHTML.Visible = !MyLibrary.IsDarkMode;
                chkCopyAsHTML.Visible = MyLibrary.IsDarkMode;

                toolStrip1.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkCopyAsHTML.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                chkShowFilterRow.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                toolStrip2.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                cboDataSource.BackColor = ColorTranslator.FromHtml("#707070");
                cboConnectionName.BackColor = ColorTranslator.FromHtml("#707070");
                cboHistoryPeriod.BackColor = ColorTranslator.FromHtml("#707070");

                c1GridSqlHistory.FilterBarStyle.BackColor = ColorTranslator.FromHtml("#2D2D30");
                c1GridSqlHistory.FilterBarStyle.ForeColor = Color.White;
                pnlHideCheckBox.BackColor = ColorTranslator.FromHtml("#2D2D30");
            }

            _cMenuGrid = new ContextMenuStrip();

            //右鍵選單
            //20230909 新增：將游標所在列的 SQL 內容貼至查詢編輯器中，並加上圖示！
            languageText = LocalizationHelper.GetLanguageString("Paste Current Row SQL Statement To QueryEditor", "form", GetType().Name, "menugrid", "PasteSqlToQueryEditor", "Text");
            _cMenuGrid.Items.Add(languageText);

            _cMenuGrid.Items[0].Click += delegate
            {
                var dt = c1GridSqlHistory.GetDataTableSourceOrNull();
                var iCurrentRow = c1GridSqlHistory.Row;
                var sqlStatement = dt.Rows[iCurrentRow][MapColumn.SqlStatement].ToString();

                MyGlobal.GlobalTemp = $"PasteFromSchemaBrowser`{sqlStatement}\r\n";
            };

            _cMenuGrid.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste2Editor_Current 16x16.ico");

            _cMenuGrid.Items.Add("-");

            languageText = LocalizationHelper.GetLanguageString("Export All Data to File", "form", GetType().Name, "menugrid", "ExportAllDataToFile", "Text");
            _cMenuGrid.Items.Add(languageText);

            _cMenuGrid.Items[2].Click += delegate
            {
                ExportToFile();
            };

            _cMenuGrid.Items[2].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Export 16x16.ico");

            _cMenuSql = new ContextMenuStrip();

            //Begin:右鍵選單
            languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
            _cMenuSql.Items.Add(languageText);
            ((ToolStripMenuItem)_cMenuSql.Items[0]).ShortcutKeys = (Keys.Control | Keys.A);

            _cMenuSql.Items[0].Click += delegate
            {
                editorSql.SelectionStart = 0;
                editorSql.SelectionEnd = editorSql.Text.Length;
                //editorCellViewer.SelectAll();
            };

            _cMenuSql.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            _cMenuSql.Items.Add("-");

            languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
            _cMenuSql.Items.Add(languageText);
            ((ToolStripMenuItem)_cMenuSql.Items[2]).ShortcutKeys = (Keys.Control | Keys.C);

            _cMenuSql.Items[2].Click += delegate
            {
                if (chkCopyAsHTML.Checked)
                {
                    Clipboard.SetDataObject(" ", false);
                    editorSql.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editorSql.Copy();
                }
            };

            _cMenuSql.Items[2].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            //End:右鍵選單

            _cMenuMessage = new ContextMenuStrip();

            //Begin:右鍵選單
            languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text");
            _cMenuMessage.Items.Add(languageText);
            ((ToolStripMenuItem)_cMenuMessage.Items[0]).ShortcutKeys = (Keys.Control | Keys.A);

            _cMenuMessage.Items[0].Click += delegate
            {
                editorMessage.SelectionStart = 0;
                editorMessage.SelectionEnd = editorMessage.Text.Length;
            };

            _cMenuMessage.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            _cMenuMessage.Items.Add("-");

            languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text");
            _cMenuMessage.Items.Add(languageText);
            ((ToolStripMenuItem)_cMenuMessage.Items[2]).ShortcutKeys = (Keys.Control | Keys.C);

            _cMenuMessage.Items[2].Click += delegate
            {
                if (chkCopyAsHTML.Checked)
                {
                    Clipboard.SetDataObject(" ", false);
                    editorMessage.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editorMessage.Copy();
                }
            };

            _cMenuMessage.Items[2].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            //End:右鍵選單

            _dicHistoryPeriod = new Dictionary<string, string>();

            //Begin:定義區間的設定值
            var s1 = LocalizationHelper.GetLanguageString("within 7 days", "form", GetType().Name, "dropdownlist", "HistoryPeriod_within7days", "Text");

            _dicHistoryPeriod.Add("within 7 days", s1);

            languageText = LocalizationHelper.GetLanguageString("within 14 days", "form", GetType().Name, "dropdownlist", "HistoryPeriod_within14days", "Text");
            _dicHistoryPeriod.Add("within 14 days", languageText);

            languageText = LocalizationHelper.GetLanguageString("within 30 days", "form", GetType().Name, "dropdownlist", "HistoryPeriod_within30days", "Text");
            _dicHistoryPeriod.Add("within 30 days", languageText);

            languageText = LocalizationHelper.GetLanguageString("before 30 days", "form", GetType().Name, "dropdownlist", "HistoryPeriod_before30days", "Text");
            _dicHistoryPeriod.Add("before 30 days", languageText);

            languageText = LocalizationHelper.GetLanguageString("All", "form", GetType().Name, "dropdownlist", "HistoryPeriod_All", "Text");
            _dicHistoryPeriod.Add("All", languageText);

            UIHelper.SetC1ComboBoxItemsFromDictionary(cboHistoryPeriod, _dicHistoryPeriod);
            cboHistoryPeriod.Text = s1;
            //End:定義區間的設定值

            if (_isFormLoadFinished)
            {
                CreateAndGetSqlHistoryInfoTable();
            }

            c1GridSqlHistory.AllowRowSizing = TextHelper.GetKeyFromDictionary(MyGlobal.dicRowSizing, MyGlobal.RowSize) == "AllRows" ? RowSizingEnum.AllRows : RowSizingEnum.IndividualRows;

            //套用 Editor 外觀
            ApplyEditorSetting();
            ApplySqlStyler();

            //套用 Grid 外觀
            //20211030 以下兩個 17 必須是固定值，否則每次「寬、高」都會變動，造成「CheckBox 遮蓋失效」
            _rowHeight = 17;
            _recSelWidth = 17;
            _fontSize = MyLibrary.GridFontSize;

            GridVisualStyle();
            GridFontAndBackColor();
            GridZoom();

            c1GridSqlHistory.AllowFilter = false;
            c1GridSqlHistory.Filter += C1TrueDBGrid_Filter;

            cboConnectionName.Text = DatabaseSqlExecutor.DbConnectionName;
            //End:取得此使用者所有的 Connection Name 設定值

            cboDataSource.Location = new Point(lblDataSource.Left + lblDataSource.Width, cboDataSource.Top);
            lblConnectionName.Location = new Point(cboDataSource.Left + cboDataSource.Width + 20, lblConnectionName.Top);
            cboConnectionName.Location = new Point(lblConnectionName.Left + lblConnectionName.Width, cboConnectionName.Top);
            lblHistoryPeriod.Location = new Point(cboConnectionName.Left + cboConnectionName.Width + 20, lblHistoryPeriod.Top);
            cboHistoryPeriod.Location = new Point(lblHistoryPeriod.Left + lblHistoryPeriod.Width, cboHistoryPeriod.Top);
            picSeparator1.Location = new Point(cboHistoryPeriod.Left + cboHistoryPeriod.Width + 10, picSeparator1.Top);
            btnSearch.Location = new Point(picSeparator1.Left + picSeparator1.Width - 2, btnSearch.Top);
            btnSelectAll.Location = new Point(btnSearch.Left + btnSearch.Width + 10, btnSelectAll.Top);
            btnUnselectAll.Location = new Point(btnSelectAll.Left + btnSelectAll.Width + 10, btnUnselectAll.Top);
            btnDelete.Location = new Point(btnUnselectAll.Left + btnUnselectAll.Width + 10, btnDelete.Top);
            picSeparator2.Location = new Point(btnDelete.Left + btnDelete.Width + 10, picSeparator2.Top);
            chkShowFilterRow.Location = new Point(picSeparator2.Left + picSeparator2.Width - 2, chkShowFilterRow.Top);
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                //20260726 統一圖示風格
                btnSelectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
                btnUnselectAll.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Unselect All 16x16.ico");

                ApplyLocalizationSetting();

                myFindReplace = new FindAndReplace();
                myFindReplace.Scintilla = editorSql;

                //Start:取得下拉選單項目
                cboDataSource.Items.Add("*");

                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT DISTINCT DataSource FROM DBInfo");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.Append(" ORDER BY DataSource");

                var sql = sbSql.ToString();
                var dtData = JasonQueryRepository.ExecQuery(sql);

                for (var i = 0; i < dtData.Rows.Count; i++)
                {
                    cboDataSource.Items.Add(dtData.Rows[i]["DataSource"].ToString());
                }

                cboDataSource.SelectedIndex = 0;
                //End:取得下拉選單項目

                chkCopyAsHTML.Checked = MyLibrary.CopyAsHTML;

                btnWordWrap.Visible = !MyLibrary.WordWrap;
                btnWordWrap2.Visible = !btnWordWrap.Visible;
                editorSql.WrapMode = !MyLibrary.WordWrap ? ScintillaNET.WrapMode.None : ScintillaNET.WrapMode.Word;
                editorSql.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

                //Message 不要折行，因為有可能包含「錯誤定位點」
                //editorMessage.WrapMode = !MyLibrary.WordWrap ? ScintillaNET.WrapMode.None : ScintillaNET.WrapMode.Word;
                //editorMessage.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

                //Begin:取得此使用者所有的 Connection Name 設定值
                cboConnectionName.Items.Add("*");

                sbSql.Clear();
                sbSql.AppendLine("SELECT ConnectionName AS DisplayName, PID AS DisplayValue");
                sbSql.AppendLine("  FROM DBInfo");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.Append(" ORDER BY ConnectionName");

                sql = sbSql.ToString();
                dtData = JasonQueryRepository.ExecQuery(sql);

                for (var i = 0; i < dtData.Rows.Count; i++)
                {
                    cboConnectionName.Items.Add(dtData.Rows[i]["DisplayName"].ToString());
                }

                CreateAndGetSqlHistoryInfoTable();

                //設定放大縮小功能
                c1GridSqlHistory.MouseWheel += c1GridSqlHistory_MouseWheel;

                _isFormLoadFinished = true;

                btnSearch.PerformClick();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void C1TrueDBGrid_Filter(object sender, FilterEventArgs e)
        {
            var dataView = (c1GridSqlHistory.DataSource as DataTable)?.DefaultView;

            if (dataView == null || dataView.RowFilter == e.Condition)
            {
                return;
            }

            var sCondition = e.Condition;

            if (sCondition.Length != 0)
            {
                sCondition = e.Condition;

                var iCount = c1GridSqlHistory.Splits[0].DisplayColumns.Count;

                for (var i = 0; i < iCount; i++)
                {
                    var sCaption = c1GridSqlHistory.Columns[i].Caption;

                    if (!sCondition.Contains($"[{sCaption}]"))
                    {
                        continue;
                    }

                    var iParamIndex = sCondition.IndexOf('\'', sCondition.IndexOf($"[{sCaption}]", StringComparison.Ordinal)) + 1;

                    sCondition = sCondition.Insert(iParamIndex, "*");
                }
            }

            dataView.RowFilter = sCondition;
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            btnWordWrap.Visible = editorSql.WrapMode == ScintillaNET.WrapMode.Word;
            btnWordWrap2.Visible = !btnWordWrap.Visible;
            editorSql.WrapMode = editorSql.WrapMode == ScintillaNET.WrapMode.Word ? ScintillaNET.WrapMode.None : ScintillaNET.WrapMode.Word;
            editorSql.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

            //Message 不要折行，因為有可能包含「錯誤定位點」
            //editorMessage.WrapMode = editorSql.WrapMode;
            //editorMessage.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSql.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            btnShowAllCharacters.Visible = !btnShowAllCharacters.Visible;
            btnShowAllCharacters2.Visible = !btnShowAllCharacters.Visible;
            editorSql.ViewEol = !btnShowAllCharacters.Visible;
            editorSql.ViewWhitespace = btnShowAllCharacters.Visible ? ScintillaNET.WhitespaceMode.Invisible : ScintillaNET.WhitespaceMode.VisibleAlways;
            editorMessage.ViewEol = !btnShowAllCharacters.Visible;
            editorMessage.ViewWhitespace = btnShowAllCharacters.Visible ? ScintillaNET.WhitespaceMode.Invisible : ScintillaNET.WhitespaceMode.VisibleAlways;
        }

        private void CreateAndGetSqlHistoryInfoTable()
        {
            var dateCondition = string.Empty;
            var dbTypeCondition = string.Empty;
            var connectionNameCondition = string.Empty;
            var enabled = false;

            Cursor = Cursors.WaitCursor;
            _dt = new DataTable();

            switch (TextHelper.GetKeyFromDictionary(_dicHistoryPeriod, cboHistoryPeriod.Text))
            {
                case "within 7 days":
                    {
                        var temp = DateTime.Today.AddDays(-7).ToString("yyyy/MM/dd");

                        dateCondition = $"\r\n   AND yy.ExecutionDate > '{temp}'";
                        break;
                    }
                case "within 14 days":
                    {
                        var temp = DateTime.Today.AddDays(-14).ToString("yyyy/MM/dd");

                        dateCondition = $"\r\n   AND yy.ExecutionDate > '{temp}'";
                        break;
                    }
                case "within 30 days":
                    {
                        var temp = DateTime.Today.AddDays(-30).ToString("yyyy/MM/dd");

                        dateCondition = $"\r\n   AND yy.ExecutionDate > '{temp}'";
                        break;
                    }
                case "before 30 days":
                    {
                        var temp = DateTime.Today.AddDays(-30).ToString("yyyy/MM/dd");

                        dateCondition = $"\r\n   AND yy.ExecutionDate < '{temp}'";
                        break;
                    }
            }

            _lstGridHeader = new List<string> { " ", "PID", "MPID" };

            var languageText = string.Empty;

            languageText = LocalizationHelper.GetLanguageString("Data Source", "form", GetType().Name, "gridheader", "DataSource", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Connection Name", "form", GetType().Name, "gridheader", "ConnectionName", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Operation Object", "form", GetType().Name, "gridheader", "OperationObject", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Seq. No", "form", GetType().Name, "gridheader", "SeqNo", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Execute Date", "form", GetType().Name, "gridheader", "ExecutionDate", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Execute Time", "form", GetType().Name, "gridheader", "ExecutionTime", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Query Time", "form", GetType().Name, "gridheader", "QueryTime", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Rows", "form", GetType().Name, "gridheader", "Rows", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Result", "form", GetType().Name, "gridheader", "Result", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("Message", "form", GetType().Name, "gridheader", "Message", "Text");
            _lstGridHeader.Add(languageText);
            languageText = LocalizationHelper.GetLanguageString("SQL Statement", "form", GetType().Name, "gridheader", "SQLStatement", "Text");
            _lstGridHeader.Add(languageText);

            _dtSqlHistoryInfo = new DataTable();

            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.Select]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.Pid]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.MPid]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.DataSource]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.ConnectionName]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.OperationObject]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.SeqNo]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.ExecutionDate]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.ExecutionTime]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.QueryTime]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.Rows]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.Result]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.Message]);
            _dtSqlHistoryInfo.Columns.Add(_lstGridHeader[MapColumn.SqlStatement]);

            //判斷 DB Type
            if (cboDataSource.Text != "*")
            {
                dbTypeCondition = $"\r\n   AND oo.DataSource = '{cboDataSource.Text}'";
            }

            //判斷 Connection Name
            if (cboConnectionName.Text != "*")
            {
                connectionNameCondition = $"\r\n   AND oo.ConnectionName = '{cboConnectionName.Text}'";
            }

            //預設，取 7 天內的資料
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT yy.PID, oo.PID AS MPID, oo.DataSource, oo.ConnectionName, yy.O1, yy.O2, yy.ExecutionDate,");
            sbSql.AppendLine("       yy.ExecutionTime, yy.QueryTime, yy.Rows, yy.Result, yy.Message, yy.SQL");
            sbSql.AppendLine("  FROM SqlHistory yy, DBInfo oo");
            sbSql.AppendLine($" WHERE oo.PID = yy.MPID{dbTypeCondition}");
            sbSql.AppendLine($"   AND oo.DomainUser = '{MyGlobal.DomainUser}'{connectionNameCondition}{dateCondition}");
            sbSql.Append(" ORDER BY yy.ExecutionDate DESC");

            var sql = sbSql.ToString();

            _dt = JasonQueryRepository.ExecQuery(sql);

            if (_dt?.Rows.Count > 0)
            {
                for (var row = 0; row < _dt.Rows.Count; row++)
                {
                    _rowDbInfo = _dtSqlHistoryInfo.NewRow();
                    _rowDbInfo[_lstGridHeader[MapColumn.Select]] = "0";
                    _rowDbInfo[_lstGridHeader[MapColumn.Pid]] = _dt.Rows[row]["PID"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.MPid]] = _dt.Rows[row]["MPID"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.DataSource]] = _dt.Rows[row]["DataSource"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.ConnectionName]] = _dt.Rows[row]["ConnectionName"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.OperationObject]] = _dt.Rows[row]["O1"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.SeqNo]] = _dt.Rows[row]["O2"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.ExecutionDate]] = Convert.ToDateTime(_dt.Rows[row]["ExecutionDate"].ToString()).ToString($"{MyLibrary.DateFormat} HH:mm:ss.fff");
                    _rowDbInfo[_lstGridHeader[MapColumn.ExecutionTime]] = _dt.Rows[row]["ExecutionTime"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.QueryTime]] = _dt.Rows[row]["QueryTime"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.Rows]] = _dt.Rows[row]["Rows"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.Result]] = _dt.Rows[row]["Result"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.Message]] = _dt.Rows[row]["Message"].ToString();
                    _rowDbInfo[_lstGridHeader[MapColumn.SqlStatement]] = _dt.Rows[row]["SQL"].ToString();
                    _dtSqlHistoryInfo.Rows.Add(_rowDbInfo);
                }
            }

            c1GridSqlHistory.DataSource = _dtSqlHistoryInfo;

            foreach (C1DisplayColumn col in c1GridSqlHistory.Splits[0].DisplayColumns)
            {
                var name = col.Name;

                if (new HashSet<string> { "PID", "MPID" }.Contains(name))
                {
                    col.Visible = false;
                    col.Frozen = true;
                }
                else
                {
                    try
                    {
                        col.AutoSize();
                    }
                    catch (Exception)
                    {
                        col.Width = 2000;
                    }

                    if (name == _lstGridHeader[MapColumn.Select])
                    {
                        col.AllowSizing = false;
                        col.Frozen = true;
                    }
                    else if (name == _lstGridHeader[MapColumn.Message])
                    {
                        col.Width = 200;
                    }
                    else if (name == _lstGridHeader[MapColumn.SqlStatement])
                    {
                        col.Width = 600;
                    }
                }
            }

            SetCheckBox(" ");
            c1GridSqlHistory.Splits[0].DisplayColumns[" "].Style.HorizontalAlignment = AlignHorzEnum.Center;

            if (c1GridSqlHistory.RowCount > 0)
            {
                enabled = true;
            }

            btnSelectAll.Enabled = enabled;
            btnUnselectAll.Enabled = enabled;
            btnDelete.Enabled = false;

            Cursor = Cursors.Default;
        }

        private void TransferValueToMainForm(string value)
        {
            //使用時機：
            //選定某一個SQL，按下右鍵，傳送至「SQL Editor」
            //使用方式如下範例：
            //uTransferValueToMainForm("TransferSelectSQL`" + "要傳送的 SQL 內容");

            var valueArgs = new ValueUpdatedEventArgs(value);

            ValueUpdated(this, valueArgs);
        }

        private void chkShowFilterRow_Click(object sender, EventArgs e)
        {
            c1GridSqlHistory.FilterBar = chkShowFilterRow.Checked;
            pnlHideCheckBox.Visible = chkShowFilterRow.Checked;
        }

        private void c1GridSqlHistory_RowColChange(object sender, RowColChangeEventArgs e)
        {
            editorMessage.ReadOnly = false;
            editorMessage.Text = c1GridSqlHistory.Columns[_lstGridHeader[MapColumn.Message]].CellValue(c1GridSqlHistory.Row).ToString();
            editorMessage.ReadOnly = true;
            editorMessage.SelectionStart = 0;
            editorMessage.ScrollCaret();

            editorSql.ReadOnly = false;
            editorSql.Text = c1GridSqlHistory.Columns[_lstGridHeader[MapColumn.SqlStatement]].CellValue(c1GridSqlHistory.Row).ToString();
            editorSql.ReadOnly = true;
            editorSql.SelectionStart = 0;
            editorSql.ScrollCaret();
        }

        private void ApplyEditorSetting()
        {
            editorSql.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorSql.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorSql.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
            editorSql.Zoom = Convert.ToInt16(MyLibrary.QueryEditorZoom);

            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editorSql.Styler = new SqlStyler();
        }

        private void GridVisualStyle()
        {
            GridHelper.SetGridVisualStyle(c1GridSqlHistory);
            c1GridSqlHistory.Splits[0].ColumnCaptionHeight = 25;
        }

        private void GridFontAndBackColor()
        {
            _fontSize = 12;

            //字型 + 字體大小
            c1GridSqlHistory.Font = new Font(MyLibrary.GridFontName, _fontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1GridSqlHistory.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1GridSqlHistory.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1GridSqlHistory.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1GridSqlHistory.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1GridSqlHistory.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridSqlHistory.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        private void GridZoom()
        {
            float.TryParse("0.9", out var pcnt);

            _fontSize = _fontSize == 0 ? 12 : _fontSize;

            //adjust row height
            c1GridSqlHistory.RowHeight = (int)(_rowHeight * pcnt) + 5;

            //標題列的高度
            c1GridSqlHistory.Splits[0].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 7;

            //and recordselector width
            c1GridSqlHistory.RecordSelectorWidth = (int)(_recSelWidth * pcnt);

            //adjust font sizes.  Normal is the root style so changing its sizes adjust all other styles
            c1GridSqlHistory.Styles["Normal"].Font = new Font(c1GridSqlHistory.Styles["Normal"].Font.FontFamily, _fontSize * pcnt);
        }

        private void c1GridSqlHistory_MouseWheel(object sender, MouseEventArgs e)
        {
            //The amount by which we adjust scale per wheel click.
            //const float scalePerDelta = 10f / 120;

            //Update the drawing based upon the mouse wheel scrolling.
            //float imageScale = e.Delta * scalePerDelta;

            if (!_isCtrlKeyDown)
            {
                return;
            }

            _totalDelta += e.Delta;

            var fValue = 1 + ((float)(SystemInformation.MouseWheelScrollLines * _totalDelta) / 3600);

            if (fValue > 1.7 || fValue < 0.5)
            {
                //
            }
            else
            {
                zoom(fValue);
            }
        }

        private void zoom(float pcnt)
        {
            _fontSize = _fontSize == 0 ? 12 : _fontSize;
            c1GridSqlHistory.RowHeight = (int)(_rowHeight * pcnt) + 5;
            c1GridSqlHistory.Splits[0].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 12;
            c1GridSqlHistory.RecordSelectorWidth = (int)(_recSelWidth * pcnt);
            c1GridSqlHistory.Styles["Normal"].Font = new Font(c1GridSqlHistory.Styles["Normal"].Font.FontFamily, _fontSize * pcnt);

            GridHelper.ResizeGridColumnWidth(c1GridSqlHistory);
        }

        private void Query_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            //切換 Focus，避免滑鼠滾輪滾動，又再次觸發查詢
            chkShowFilterRow.Focus();
            CreateAndGetSqlHistoryInfoTable();
            c1GridSqlHistory.Focus();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            CreateAndGetSqlHistoryInfoTable();
        }

        private void SetCheckBox(string sColumn)
        {
            //該欄位以 CheckBox 動態呈現
            var items = c1GridSqlHistory.Columns[sColumn].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.CheckBox;

            //now associate underlying db values with the checked state
            items.Values.Clear();
            items.Values.Add(new ValueItem("0", false)); //unchecked
            items.Values.Add(new ValueItem("1", true));  //checked

            //指定哪一個 Column 要套用 FetchCellStyle
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.ConnectionName]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.DataSource]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.ExecutionDate]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.ExecutionTime]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.Message]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.QueryTime]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.Rows]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.Result]].FetchStyle = true;
            c1GridSqlHistory.Splits[0].DisplayColumns[_lstGridHeader[MapColumn.SqlStatement]].FetchStyle = true;
        }

        private void c1GridSqlHistory_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            if (e.Col > 0) //除了 CheckBox，其餘皆 Lock
            {
                e.CellStyle.Locked = true;
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            var sValue = sender is C1.Win.C1Input.C1Button btn && TextHelper.GetSafeString(btn.Tag) == "SelectAll" ? "1" : "0";
            var iCurrentRow = c1GridSqlHistory.Row;
            var iCurrentCol = c1GridSqlHistory.Col;

            try
            {
                for (var j = c1GridSqlHistory.RowCount - 1; j >= 0; j--)
                {
                    c1GridSqlHistory[j, 0] = sValue;
                }

                CheckAndCountSelected();

                c1GridSqlHistory.Row = iCurrentRow;
                c1GridSqlHistory.Col = iCurrentCol;
                c1GridSqlHistory.Select();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var temp1 = LocalizationHelper.GetLanguageString("All selected records will be deleted!", "form", GetType().Name, "msg", "AllWillBeDeleted", "Text");
                var temp2 = LocalizationHelper.GetLanguageString("Are you sure you want to continue?", "form", GetType().Name, "msg", "WantToContinue", "Text");

                if (MessageBox.Show($"{temp1}\r\n\r\n{temp2}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;

                    var sbPid = new StringBuilder();

                    for (var j = c1GridSqlHistory.RowCount - 1; j >= 0; j--)
                    {
                        var select = c1GridSqlHistory[j, MapColumn.Select].ToString();
                        var pid = c1GridSqlHistory[j, MapColumn.Pid].ToString();

                        if (select == "1")
                        {
                            sbPid.Append(pid);
                            sbPid.Append(",");
                        }
                    }

                    //移除最後一個逗號
                    if (sbPid.Length > 0 && sbPid[sbPid.Length - 1] == ',')
                    {
                        sbPid.Length--; //直接縮短長度，效能最好
                    }

                    var pids = sbPid.ToString();
                    var sbSql = new StringBuilder();

                    sbSql.AppendLine("DELETE FROM SqlHistory");
                    sbSql.Append($" WHERE PID IN ({pids});");

                    var sql = sbSql.ToString();

                    //Batch Delete SQL History
                    var message = JasonQueryRepository.BatchDeleteRecord(sql);

                    btnSearch.PerformClick();

                    if (string.IsNullOrEmpty(message))
                    {
                        temp1 = LocalizationHelper.GetLanguageString("All selected records have been deleted!", "form", GetType().Name, "msg", "AllHaveBeenDeleted", "Text");
                        MessageBoxHelper.ShowNearCursor(temp1, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBoxHelper.ShowNearCursor($"{MyGlobal.AnErrorHasOccurred}\r\n\r\n{message}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                JasonQueryRepository.ExecNonQuery("VACUUM");
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch.Focus();
                Cursor = Cursors.Default;
            }
        }

        private void CheckAndCountSelected()
        {
            var iCurrentRow = c1GridSqlHistory.Row;
            var iCurrentCol = c1GridSqlHistory.Col;

            _iSelectedCount = 0;

            for (var j = c1GridSqlHistory.RowCount - 1; j >= 0; j--)
            {
                if (c1GridSqlHistory[j, 0].ToString() == "1")
                {
                    _iSelectedCount++;
                }
            }

            c1GridSqlHistory.Row = iCurrentRow;
            c1GridSqlHistory.Col = iCurrentCol;
            c1GridSqlHistory.Select(); //Focus 切換到指定的 Cell

            btnDelete.Enabled = _iSelectedCount > 0;
        }

        private void c1GridSqlHistory_AfterColUpdate(object sender, ColEventArgs e)
        {
            //使用者手動勾選
            CheckAndCountSelected();
        }

        private void editorSql_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _cMenuSql.Items[0].Enabled = !string.IsNullOrEmpty(editorSql.Text);

            //Copy: 判斷是否有選取文字，決定功能表項目可不可用
            _cMenuSql.Items[2].Enabled = !string.IsNullOrEmpty(editorSql.SelectedText);

            editorSql.ContextMenuStrip = _cMenuSql;

            if (MyLibrary.IsDarkMode)
            {
                _cMenuSql.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenuSql.ForeColor = Color.White;
                _cMenuSql.RenderMode = ToolStripRenderMode.System;
                //_cMenuSql.ShowImageMargin = false;
            }

            _cMenuSql.Show(editorSql, new Point(e.X, e.Y));
        }

        private void btnSelectAll2_Click(object sender, EventArgs e)
        {
            if (editorSql.Focused)
            {
                editorSql.SelectionStart = 0;
                editorSql.SelectionEnd = editorSql.Text.Length;
            }
            else
            {
                editorMessage.SelectionStart = 0;
                editorMessage.SelectionEnd = editorSql.Text.Length;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (editorSql.Focused)
            {
                if (chkCopyAsHTML.Checked)
                {
                    Clipboard.SetDataObject(string.Empty, false);
                    editorSql.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editorSql.Copy();
                }
            }
            else
            {
                if (chkCopyAsHTML.Checked)
                {
                    Clipboard.SetDataObject(string.Empty, false);
                    editorMessage.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editorMessage.Copy();
                }
            }
        }

        private void tmrMother2Child_Tick(object sender, EventArgs e)
        {
            //是否為 Reload Localization 套用？
            if (string.IsNullOrEmpty(MyGlobal.InfoFromReloadLocalization) || !MyGlobal.InfoFromReloadLocalization.StartsWith("ReloadLocalization`", StringComparison.Ordinal))
            {
                return;
            }

            var sTemp = MyGlobal.InfoFromReloadLocalization.Replace("ReloadLocalization`", string.Empty);

            sTemp = sTemp.Split(';')[0];

            if (sTemp != AccessibleDescription)
            {
                return;
            }

            MyGlobal.InfoFromReloadLocalization = MyGlobal.InfoFromReloadLocalization.Replace($"{AccessibleDescription};", string.Empty);

            if (MyGlobal.InfoFromReloadLocalization == "ReloadLocalization`")
            {
                MyGlobal.InfoFromReloadLocalization = string.Empty;
            }

            ApplyLocalizationSetting();
            CreateAndGetSqlHistoryInfoTable();
            _isFormLoadFinished = true;
        }

        private void c1GridSqlHistory_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _cMenuGrid.Items[0].Enabled = c1GridSqlHistory.RowCount > 0;

            c1GridSqlHistory.ContextMenuStrip = _cMenuGrid;

            if (MyLibrary.IsDarkMode)
            {
                _cMenuGrid.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenuGrid.ForeColor = Color.White;
                _cMenuGrid.RenderMode = ToolStripRenderMode.System;
                //_gMenu.ShowImageMargin = false;
            }

            _cMenuGrid.Show(c1GridSqlHistory, new Point(e.X, e.Y));
        }

        private void ExportToFile()
        {
            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                Filter = @"Excel files (*.xlsx)|*.xlsx"
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
            {
                sf.FileName += ".xlsx";
            }

            try
            {
                c1GridSqlHistory.SaveExcel(sf.FileName, MyLibrary.GridSheetName);
                Process.Start(sf.FileName);
            }
            catch (Exception ex) //20250816 此處沒有取得行號
            {
                MessageBox.Show($"{MyGlobal.AnErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void editorMessage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            //Select All
            _cMenuMessage.Items[0].Enabled = !string.IsNullOrEmpty(editorMessage.Text);

            //Copy: 判斷是否有選取文字，決定功能表項目可不可用
            _cMenuMessage.Items[2].Enabled = !string.IsNullOrEmpty(editorMessage.SelectedText);

            editorMessage.ContextMenuStrip = _cMenuMessage;

            if (MyLibrary.IsDarkMode)
            {
                _cMenuMessage.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenuMessage.ForeColor = Color.White;
                _cMenuMessage.RenderMode = ToolStripRenderMode.System;
                //_fMenu.ShowImageMargin = false;
            }

            _cMenuMessage.Show(editorMessage, new Point(e.X, e.Y));
        }

        private void ApplySqlStyler()
        {
            editorMessage.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorMessage.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorMessage.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editorMessage.Styler = new SqlStyler();
        }

        private void btnCompact_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            var message = string.Empty;
            long fileLengthOld = new FileInfo(JasonQueryRepository.DbFileName).Length;
            long fileLengthNew = 0;

            try
            {
                JasonQueryRepository.ExecNonQuery("VACUUM");
                fileLengthNew = new FileInfo(JasonQueryRepository.DbFileName).Length;
            }
            catch (Exception)
            {
                message = $"Cannot access the file '{JasonQueryRepository.DbFileName}' because it is being used by another process.";
                MessageBox.Show(message, @"Error compacting file...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            message = $"'JasonQuery.db' compacted successfully!\r\n\r\nSize before compacting: {DataSizeFormatter.FormatInt(fileLengthOld)}\r\nSize after compacting: {DataSizeFormatter.FormatInt(fileLengthNew)}";
            MessageBox.Show(message, @"Compact OK!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            btnCompact.Enabled = false;
        }

        private void editorSql_Enter(object sender, EventArgs e)
        {
            myFindReplace.Scintilla = (ScintillaEditor)sender; //20230629
            MyGlobal.GlobalTemp5 = "CanPasteN"; //20230704
        }

        private void editorMessage_Enter(object sender, EventArgs e)
        {
            myFindReplace.Scintilla = (ScintillaEditor)sender; //20230629
            MyGlobal.GlobalTemp5 = "CanPasteN"; //20230704
        }

        //20230909 滑鼠定位在游標所在列
        private void c1GridSqlHistory_MouseUp(object sender, MouseEventArgs e)
        {
            var displayColIndex = c1GridSqlHistory.ColContaining(e.X);
            var displayRowIndex = c1GridSqlHistory.RowContaining(e.Y);

            if (displayColIndex == -1 && displayRowIndex == -1)
            {
                return;
            }

            c1GridSqlHistory.Row = displayRowIndex;
            c1GridSqlHistory.Col = displayColIndex;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C: //Ctrl+C
                case Keys.Control | Keys.Insert: //20231020 Ctrl+INS
                    {
                        if (editorSql.Focused)
                        {
                            if (chkCopyAsHTML.Checked)
                            {
                                Clipboard.SetDataObject(" ", false);
                                editorSql.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                            }
                            else
                            {
                                editorSql.Copy();
                            }

                            return true;
                        }

                        if (editorMessage.Focused)
                        {
                            editorMessage.Copy(); //20260716 補上 editorMessage 的 Ctrl+C 快捷鍵
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
                        myFindReplace.ShowFind();
                        return true;
                    }
                case Keys.Control | Keys.H: //Ctrl+H 取代
                    {
                        myFindReplace.ShowReplace();
                        return true;
                    }
                case Keys.F3:
                    {
                        if (editorSql.Focused || editorMessage.Focused)
                        {
                            myFindReplace.Window.FindNext(true);
                            return true;
                        }

                        break;
                    }
                case Keys.Shift | Keys.F3:
                    {
                        if (editorSql.Focused || editorMessage.Focused)
                        {
                            myFindReplace.Window.FindNext(false);
                            return true;
                        }

                        break;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static class MapColumn
        {
            public const int Select = 0;
            public const int Pid = 1;
            public const int MPid = 2;
            public const int DataSource = 3;
            public const int ConnectionName = 4;
            public const int OperationObject = 5;
            public const int SeqNo = 6;
            public const int ExecutionDate = 7;
            public const int ExecutionTime = 8;
            public const int QueryTime = 9;
            public const int Rows = 10;
            public const int Result = 11;
            public const int Message = 12;
            public const int SqlStatement = 13;
        }
    }
}
