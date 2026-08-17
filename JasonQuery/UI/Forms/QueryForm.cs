using C1.Win.C1Command;
using C1.Win.C1Input;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Events;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Core.Text;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.UI.Controls;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Formatters;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Data.Value;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.Transactions;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.QueryEngine.Formatters;
using JasonQuery.Core.Schema;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Display.Columns;
using JasonQuery.Editor.FindAndReplace;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Math = System.Math;

//快速提示
//UpdateBackupFileInfo 可以變更頁簽的名稱 (只針對未存檔的)
//自動取代寫在 editor_KeyUp()，尋找關鍵字「//AutoReplace」
//改寫 Width/Height 取值方法

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm : Form
    {
        public event ValueUpdatedEventHandler ValueUpdated;

        public string BackupRealFullFileName { get; set; } = string.Empty; //20240302 使用者實際儲存的完整路徑+檔案名稱
        public int BackupCurrentPosition { get; set; } = 0; //20240302 備份檔案當時的游標位置

        private static readonly int[] _periodPopupFetchStyleColumns = { 0, 2 };
        private static readonly int[] _spacePopupFetchStyleColumns = { 0 };

        private ColumnInfoCollector _columnInfoCollector; //20260102 改為表單層級變數
        private ColumnDisplayContext _currentDisplayContext; //20260114 記住使用者勾選 ShowColumnType 及 ShowColumnComment 的狀態
        private bool _currentQueryResultHasColumnComments; //20260612 目前查詢結果是否包含欄位註釋，用於標題列高度與顯示選項判斷
        private bool _isNextQueryResultColumnNameSortAscending = true; //20260612 下一次「依欄位名稱排序」是否使用正向排序

        private int _queryIndex;
        private string _unfreezeColumnName = string.Empty;
        private string _queryEditorFontSize = string.Empty;
        private bool _isMainContentShown;
        private int _queryResultOptionsToolbarLayoutRequestId;

        //20260404 改用 Point? 來記錄游標位置，取代原本的 X,Y 字串變數
        private Point? _autoCompleteMousePosition; //觸發 AutoComplete 下拉清單時的 X, Y 座標

        private string _backupFileName = string.Empty; //20240301 for 定期備份的暫存檔名
        private int _sqlNavigatorRowHeight; //20241214 記錄 SQL Navigator 的 RowHeight

        private string _sqlWithoutCommentMessage = string.Empty;
        private string _sqlCannotGetColumnInfoMessage = string.Empty;

        private bool _isColumnAutoResizing; //是否正在「自動調整欄寬」？
        private int _lastRowOffset;
        private int _lastTimeOffset;
        private int _welcomeCountdownSeconds = 120;

        private bool _isNextPageQuery;
        private int _nextPageCol;
        private int _nextPageRow;
        private DataTable _dtNextPage;
        private double _nextPageScale = 1;
        private int _splitsIndex = 0; //預留給未來如果要「支援多個查詢 SQL」，目前只有一個，所以固定為 0
        private bool _isHighlightSelectionCopyMode; //20260703 HighlightSelection 建立的多重選取狀態，Copy 時只複製一個單詞
        private string _selectedTextOnDoubleClick = string.Empty; //20221102 記住 Double Click 的字串

        private FindAndReplace _findAndReplace; //20230629 加入 FindAndReplace 功能
        private Thread _threadQuery;

        private delegate void BindDatagrid();

        private int _shiftMouseLeftDownX = -1;
        private int _shiftMouseLeftDownY = -1;

        private Color _toolstripFocused = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
        private Color _toolstripUnfocused = SystemColors.Control;

        private string _languageText = string.Empty;
        private string _dataGridTabName = string.Empty;
        private string _dataGridTabNameOriginal = string.Empty;
        private string _originalTabName = string.Empty;
        private int _queryTextParametersStart; //含參數的 SQL，該段落的SQL的起始位置
        private string _queryTextParametersMapping = string.Empty; //此參數要呈現在 SQL History 的訊息欄
        private string _queryTextParametersPositionMapping = string.Empty; //此參數供錯誤定位用的

        private ContextMenuStrip _queryEditorContextMenu = new ContextMenuStrip(); //Query Editor 右鍵選單
        private ContextMenuStrip _gridContextMenu = new ContextMenuStrip(); //Grid 右鍵選單
        private ContextMenuStrip _gridHeaderContextMenu = new ContextMenuStrip(); //Grid 右鍵選單 (標題列 - 排序用)
        private ContextMenuStrip _messageEditorContextMenu = new ContextMenuStrip(); //Message Editor 右鍵選單
        private ContextMenuStrip _schemaBrowserContextMenu = new ContextMenuStrip(); //SchemaBrowser Grid 右鍵選單


        private readonly DataTable _dtNullTable = new DataTable(); //查詢前的空 Table，供 Grid 顯示使用
        private DataTable _dtExportedSchemaTable; //承接查詢時所儲存的 SchemaTable
        private DataTable _dtSqlNavigatorTable;

        private string _queryStatus = string.Empty; //20191212 記錄查詢狀態
        private string _sqlWhenError = string.Empty; //20200220 記錄發生錯誤時的 SQL

        private bool _isGridZooming;
        private bool _isBusy;
        private bool _isSplitterSaveRequired; //判斷是否需要 SaveSplitter 寬／高度值

        private List<Point> _searchList = new List<Point>();
        private List<string> _gridHeaderAutoReplacements = new List<string>();

        private DataTable _dtPeriodAutoCompleteTable = new DataTable();
        private DataTable _dtSpaceAutoCompleteTable = new DataTable();
        private DataTable _dtAutoReplaceInfoTable = new DataTable();
        private int _compoundKeyCtrlShift; //for Ctrl + Shift + ?

        private string _columnKeywordName = "Keyword";
        private string _columnReplacementName = "Replacement";
        private Dictionary<string, string> _autoReplacements = new Dictionary<string, string>();
        private string _operationObject = string.Empty;

        [DllImport("user32")]
        private static extern bool SetCursorPos(int x, int y);

        private Devart.Data.Oracle.OracleDataReader _oracleDataReader;
        private Devart.Data.PostgreSql.PgSqlDataReader _postgreSqlDataReader;
        private Devart.Data.SqlServer.SqlDataReader _sqlServerDataReader;
        private Devart.Data.MySql.MySqlDataReader _mySqlDataReader;

        private const int SquiggleNumber = 11; //20191224 波浪底線
        private const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";
        private const int BookmarkMargin = 1; //Conventionally the symbol margin
        private const int BookmarkMarker = 3; //Arbitrary. Any valid index would work

        private bool _isFormLoadFinished = false; //表單是否載入完畢 (避免觸發事件)
        private int _queryCount;
        private DateTime _startTime;
        private DateTime _endTime;

        private int _rowHeight; //original row height
        private int _recSelWidth; //original record selector width
        private float _fontSize; //original font size

        private DataRow _rowAutoReplaceInfo;

        private string _editorLength = string.Empty;
        private string _editorLines = string.Empty;
        private string _editorLn = string.Empty;
        private string _editorCol = string.Empty;
        private string _editorPos = string.Empty;
        private string _editorSel = string.Empty;
        private string _execTime = string.Empty;
        private string _queryTime = string.Empty;
        private string _rows = string.Empty;
        private bool _isCtrlKeyDown;
        private int _totalDelta;

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public QueryForm()
        {
            InitializeComponent();

            EnableOpeningPaintOptimization();
            InitializeDefaultSelectedTabs();
            HideMainContentDuringOpening();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;

            InitializeEditorServices();
            InitializeAutoCompleteComposition();
            InitializeKeyCommandContext();
        }

        private void EnableOpeningPaintOptimization()
        {
            DoubleBuffered = true;

            SetStyle
            (
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            UpdateStyles();
        }

        private void InitializeDefaultSelectedTabs()
        {
            c1DockingTab1.SuspendLayout();
            c1DockingTab2.SuspendLayout();

            try
            {
                c1DockingTab1.SelectedTab = tabMessage;

                if (MyGlobal.IsDefaultTabSchemaBrowser)
                {
                    c1DockingTab2.SelectedTab = tabSchemaInformation;
                }
                else if (MyLibrary.EnableAutoReplace)
                {
                    c1DockingTab2.SelectedTab = tabAutoReplace;
                }
                else
                {
                    c1DockingTab2.SelectedTab = tabTabList;
                }
            }
            finally
            {
                c1DockingTab2.ResumeLayout(false);
                c1DockingTab1.ResumeLayout(false);
            }
        }

        private void HideMainContentDuringOpening()
        {
            splitContainer1.Visible = false;
        }

        private void ShowMainContentAfterOpening()
        {
            if (_isMainContentShown)
            {
                return;
            }

            InitializeDefaultSelectedTabs();

            splitContainer1.Visible = true;

            splitContainer1.PerformLayout();
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.PerformLayout();

            splitContainer3.PerformLayout();
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.PerformLayout();

            LoadSplitterData("U/D");

            splitContainer3.PerformLayout();
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.PerformLayout();

            ApplyEditorPanelLayout();
            ApplyQueryResultOptionsToolbarLayout();

            splitContainer1.Invalidate(true);
            splitContainer1.Update();

            _isMainContentShown = true;
        }

        private void ApplyEditorPanelLayout()
        {
            var panel = splitContainer3.Panel1;

            if (panel.ClientSize.Width <= 0 || panel.ClientSize.Height <= 0)
            {
                return;
            }

            c1StatusBar2.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            lblInfoEditor.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            c1StatusBar2.SetBounds(0, panel.ClientSize.Height - c1StatusBar2.Height, panel.ClientSize.Width, c1StatusBar2.Height);
            lblInfoEditor.Location = new Point(3, c1StatusBar2.Top + 4);
            editor.SetBounds(-1, editor.Top, panel.ClientSize.Width + 2, Math.Max(1, c1StatusBar2.Top - editor.Top + 1));

            c1GridAutoCompleteForAll.BringToFront();
        }

        private void ShowExceptionMessage(Exception ex)
        {
            ExceptionDialogService.Show(this, ex);
        }

        private void Form_Leave(object sender, EventArgs e)
        {
            HideAutoCompleteGrid();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            var isLayoutSuspended = false;

            try
            {
                SuspendOpeningLayout();
                isLayoutSuspended = true;

                InitializeFormLoadBaseUi();
                InitializeFormLoadEditorAndIcons();
                InitializeFormLoadToolbarAndOptions();
                InitializeFormLoadFindAutoReplaceAndGrid();
                InitializeFormLoadSchemaAndConnectionState();
                RestoreBackupFileIfNeeded();
                FinalizeFormLoadLayoutAndEditorSettings();
                ShowEditorMessageWelcomeIfNeeded();
            }
            catch (Exception ex)
            {
                if (isLayoutSuspended)
                {
                    ResumeOpeningLayout();
                    isLayoutSuspended = false;
                }

                ShowMainContentAfterOpening();
                ShowExceptionMessage(ex);
            }
            finally
            {
                if (isLayoutSuspended)
                {
                    ResumeOpeningLayout();
                }

                ShowMainContentAfterOpening();

                _isFormLoadFinished = true;

                ApplyEditorFocusedVisualState();
            }
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void SuspendOpeningLayout()
        {
            SuspendLayout();

            splitContainer1.SuspendLayout();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();

            splitContainer2.SuspendLayout();
            splitContainer2.Panel1.SuspendLayout();

            splitContainer3.SuspendLayout();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();

            splitContainer4.SuspendLayout();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();

            c1DockingTab1.SuspendLayout();
            c1DockingTab2.SuspendLayout();

            tabMessage.SuspendLayout();
            tabDataGrid.SuspendLayout();
            tabAutoReplace.SuspendLayout();
            tabSchemaInformation.SuspendLayout();
            tabTabList.SuspendLayout();
            tabSqlNavigator.SuspendLayout();

            pnlEditorMessageWelcome.SuspendLayout();
        }

        private void ResumeOpeningLayout()
        {
            pnlEditorMessageWelcome.ResumeLayout(false);

            tabSqlNavigator.ResumeLayout(false);
            tabTabList.ResumeLayout(false);
            tabSchemaInformation.ResumeLayout(false);
            tabAutoReplace.ResumeLayout(false);
            tabDataGrid.ResumeLayout(false);
            tabMessage.ResumeLayout(false);

            c1DockingTab2.ResumeLayout(false);
            c1DockingTab1.ResumeLayout(false);

            splitContainer4.Panel2.ResumeLayout(false);
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.ResumeLayout(false);

            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.ResumeLayout(false);

            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.ResumeLayout(false);

            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.ResumeLayout(false);

            ResumeLayout(true);
        }

        private int GetIndentSize()
        {
            int.TryParse(txtIndentWord.Text, out var indentSize);
            return indentSize;
        }

        private void BuildAutoCompleteMenu(bool enabled = true)
        {
            if (!enabled)
            {
                MyGlobal.dtAutoCompleteForAll = null;
            }
        }

        private void ApplyEditorSetting()
        {
            editor.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editor.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

            editorMessage.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorMessage.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);

            //設定 WordWrap 按鈕
            var value = !MyLibrary.WordWrap;

            btnWordWrap.Visible = value;
            btnWordWrap2.Visible = !value;

            editor.WrapMode = value ? ScintillaNET.WrapMode.None : ScintillaNET.WrapMode.Word;
            editor.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None)
                                      | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None)
                                      | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

            //設定 ShowAllCharacters 按鈕
            btnShowAllCharacters.Visible = !MyLibrary.ShowAllCharacters;
            btnShowAllCharacters2.Visible = MyLibrary.ShowAllCharacters;
            editor.ViewEol = MyLibrary.ShowAllCharacters;
            editor.ViewWhitespace = MyLibrary.ShowAllCharacters ? ScintillaNET.WhitespaceMode.VisibleAlways : ScintillaNET.WhitespaceMode.Invisible;

            editor.Zoom = Convert.ToInt16(MyLibrary.QueryEditorZoom);

            ApplySqlStyler();

            //<--Begin, 設定 Bookmark 樣式
            var margin = editor.Margins[BookmarkMargin];

            //editor.Margins[BOOKMARK_MARGIN].Width = 10;
            margin.Width = 15;
            margin.Mask = 0;
            margin.Sensitive = true;
            margin.Type = ScintillaNET.MarginType.Symbol;
            margin.Mask = ScintillaNET.Marker.MaskAll;
            margin.Cursor = ScintillaNET.MarginCursor.Arrow;

            var marker = editor.Markers[BookmarkMarker];
            var style = TextHelper.GetKeyFromDictionary(MyGlobal.dicBookmarkStyle, MyGlobal.BookmarkStyle);

            switch (style)
            {
                case "Arrow":
                    {
                        marker.Symbol = ScintillaNET.MarkerSymbol.Arrow; //箭頭
                        break;
                    }
                case "Circle":
                    {
                        marker.Symbol = ScintillaNET.MarkerSymbol.Circle; //圓形
                        break;
                    }
                case "RoundRect":
                    {
                        marker.Symbol = ScintillaNET.MarkerSymbol.RoundRect; //圓角矩形
                        break;
                    }
                case "SmallRect":
                    {
                        marker.Symbol = ScintillaNET.MarkerSymbol.SmallRect; //正方形
                        break;
                    }
                default:
                    {
                        marker.Symbol = ScintillaNET.MarkerSymbol.ShortArrow; //小箭頭
                        break;
                    }
            }

            marker.SetBackColor(ColorTranslator.FromHtml(MyLibrary.ColorBookmarkBackground));
            marker.SetForeColor(Color.Transparent);
            //-->End, 設定 Bookmark 樣式
        }

        private void AddComment() //加 -- 註解
        {
            try
            {
                _editorCommentService.AddComment();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void RemoveComment() //去除 -- 註解
        {
            try
            {
                _editorCommentService.RemoveComment();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void ApplyGridMenu()
        {
            EnsureGridContextMenusCreated();
            ApplyGridContextMenuLocalization();
        }

        private void SortDataGrid(int col2, bool isByNumber = false)
        {
            var sortMode = string.Empty;
            var tag = string.Empty;
            var c1Grid = GetWhichGrid();
            var newCol = $"{DateTime.Now:yyyyMMddHHmmssfff}"; //DataTable 最後面新增的暫時欄位名稱
            var dc = c1Grid.Splits[_splitsIndex].DisplayColumns[col2]; //dc.DataColumn.DataType.Name
            var currentRow = c1Grid.Row;
            var currentCol = c1Grid.Col;

            Thread.Sleep(12);

            //20250122 Sleep 後再重新取值，避免 newCol 與 newCol0 兩個變數的值一模一樣
            var newCol0 = $"{DateTime.Now:yyyyMMddHHmmssfff}"; //使用者選定要排序的欄位，其原始欄位名稱(因為有可能包含換行符號或特殊符號，造成排序異常，故需要一個臨時的名稱)

            try
            {
                Cursor = Cursors.WaitCursor;

                var sortDown = IconManager.GetBitmap(MyGlobal.IconLibrary, "SortDn.bmp");
                var sortUp = IconManager.GetBitmap(MyGlobal.IconLibrary, "SortUp.bmp");

                sortDown.MakeTransparent(Color.Red);
                sortUp.MakeTransparent(Color.Red);

                //clear all sort indicators
                foreach (C1DisplayColumn col in c1Grid.Splits[_splitsIndex].DisplayColumns)
                {
                    tag += $"{col.DataColumn.Tag};";
                    col.HeadingStyle.ForegroundImage = null;
                }

                switch (dc.DataColumn.FilterWatermark)
                {
                    case "":
                    case "None":
                    case "Desc":
                        {
                            sortMode = "Asc";
                            break;
                        }
                    default:
                        {
                            sortMode = "Desc";
                            break;
                        }
                }

                var condition = string.Empty;
                var dt = c1Grid.CopyDataTableSourceOrEmpty();
                var oldCol = dt.Columns[currentCol].ColumnName;

                dt.Columns[currentCol].ColumnName = newCol0;

                if (isByNumber) //以數字排序，轉成 double 後再排序
                {
                    dt.Columns.Add(newCol, typeof(double));

                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        double.TryParse(dt.Rows[i][currentCol].ToString(), out var dValue);

                        if (dt.Rows[i][currentCol].ToString() == MyLibrary.GridNullShowAs)
                        {
                            dValue = -1;
                        }

                        dt.Rows[i][dt.Columns.Count - 1] = dValue;
                    }

                    condition = $"{newCol} {sortMode}, {newCol0} {sortMode}"; //有些非數字的值，依原值排序
                }
                else //一般排序，為避免欄位標題包含換行符號或特殊字元(尤其是別國的語言)造成 DataTable 排序異常，故先以 sNewCol 排序，後面再置換回來
                {
                    condition = $"{newCol0} {sortMode}";
                }

                var dv = dt.DefaultView;

                dv.Sort = condition;

                var dtSorted = dv.ToTable();

                if (isByNumber)
                {
                    dtSorted.Columns.Remove(newCol);
                }

                dtSorted.Columns[currentCol].ColumnName = oldCol; //不論是一般排序 or 數字排序，都換回原本的名稱
                c1Grid.DataSource = dtSorted;
                dv.Dispose();

                dc = c1Grid.Splits[_splitsIndex].DisplayColumns[currentCol];
                dc.DataColumn.FilterWatermark = sortMode;
                dc.HeadingStyle.ForegroundImage = sortMode == "Asc" ? sortUp : sortDown;
                dc.HeadingStyle.ForeGroundPicturePosition = ForeGroundPicturePositionEnum.RightOfText;

                if (!string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
                {
                    var colorNull = new Style
                    {
                        ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
                    };

                    for (var i = 0; i < c1Grid.Columns.Count; i++)
                    {
                        //套用「使用者指定的 NULL」顯示格式
                        c1Grid.Splits[_splitsIndex].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
                    }
                }

                var x = 0;
                var parts = tag.Split(new[] { ";" }, StringSplitOptions.None);

                foreach (C1DisplayColumn col in c1Grid.Splits[_splitsIndex].DisplayColumns)
                {
                    //還原原本記錄在 Tag 的 DataType，「複製成查詢條件」／「匯出至 Excel」才不會有問題
                    col.DataColumn.Tag = parts[x];
                    x++;
                }

                if (chkSize.Checked)
                {
                    GridHelper.ResizeGridColumnWidth(c1Grid);
                    c1Grid.Refresh();
                }

                //20251118 排序後，套用標題顏色
                ApplyResultGridHeadingStyle(c1TrueDBGrid1, true);

                c1Grid.Row = currentRow;
                c1Grid.Col = currentCol;
                c1Grid.Select();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ConvertSQLFormatter()
        {
            try
            {
                var options = EditorSqlFormatterOptionsFactory.Create
                (
                    MyLibrary.SqlFormatterIndentSize,
                    MyLibrary.SqlFormatterMaxLineWidth,
                    MyLibrary.SqlFormatterBlankLinesBetweenStatements,
                    MyLibrary.SqlFormatterConvertCaseForKeywords,
                    MyLibrary.SqlFormatterConvertCaseForKeywordsCase,
                    MyLibrary.SqlFormatterListItemsPerLine
                );

                var formatted = _editorSqlFormatterService.TryFormatSelection
                (
                    _currentSourceType,
                    MyLibrary.SqlFormatterEngine,
                    options,
                    out var errorMessage
                );

                if (!formatted && !string.IsNullOrEmpty(errorMessage))
                {
                    SetEditorStatusBarInfo(errorMessage, Color.DarkRed);
                }

                var start = editor.SelectionStart;
                var end = editor.SelectionEnd;

                //20260804 修正排版後的選取，並將游標定位在整段SQL選取處的最前方
                editor.SetSelection(start, end);
                editor.ScrollCaret();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        /// <summary>
        /// 轉換大小寫
        /// </summary>
        /// <param name="bUpper">是否轉換為大寫：是→轉大寫；否→轉小寫</param>
        private void ConvertSelectionCase(bool toUpperCase)
        {
            try
            {
                _editorTextCaseService.ConvertSelectionCase(toUpperCase);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        /// <summary>
        /// 判斷是否為 With 子句的 SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static bool IsWithSql(string sql) //判斷 With 子句屬於 Select 還是 Update/Delete/Insert？
        {
            var isResult = true;
            var tempSql = string.Empty;
            var selectedTextUpper = sql.ToUpper();

            selectedTextUpper = Regex.Replace(selectedTextUpper, @"\s+", " "); //重複出現的空白，全部置換成一個空白

            var isDoubleQuotation = false; //是否為雙引號起始？
            var isSingleQuotation = false; //是否為單引號起始？
            var bSingleComment = false; //是否為單列註解？
            var isParagraphComment = false; //是否為段落註解？
            var array = selectedTextUpper.ToCharArray();

            for (var i = 0; i < array.Length; i++)
            {
                var letter = array[i];

                if (letter == '\"' || letter == '\'')
                {
                    switch (letter)
                    {
                        case '\"' when isDoubleQuotation:
                            {
                                isDoubleQuotation = false;
                                break;
                            }
                        case '\"':
                            {
                                if (!isSingleQuotation)
                                {
                                    isDoubleQuotation = true;
                                }

                                break;
                            }
                        case '\'' when isSingleQuotation:
                            {
                                isSingleQuotation = false;
                                break;
                            }
                        case '\'':
                            {
                                if (!isDoubleQuotation)
                                {
                                    isSingleQuotation = true;
                                }

                                break;
                            }
                    }
                }

                if (!isSingleQuotation && !isDoubleQuotation && i >= 1 && letter == '*' && array[i - 1] == '/')
                {
                    if (!isParagraphComment)
                    {
                        isParagraphComment = true;
                    }
                }

                if (!isSingleQuotation && !isDoubleQuotation && letter == '*' && i + 1 <= array.GetUpperBound(0) && array[i + 1] == '/')
                {
                    if (isParagraphComment)
                    {
                        isParagraphComment = false;
                    }
                }

                if (!isSingleQuotation && !isDoubleQuotation && i >= 1 && letter == '-' && array[i - 1] == '-')
                {
                    if (!bSingleComment)
                    {
                        bSingleComment = true;
                    }
                }

                if (bSingleComment && (letter == '\r' || letter == '\n'))
                {
                    bSingleComment = false;
                }

                if (isParagraphComment && letter == '*' && i + 1 < array.Length && array[i + 1] == '/')
                {
                    isParagraphComment = false;
                }

                if (!isDoubleQuotation && !isSingleQuotation && !bSingleComment && !isParagraphComment)
                {
                    tempSql += letter.ToString();
                }
            }

            //換行符號置換成空白
            tempSql = tempSql.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

            //檢查剩下來的 SQL，如果有異動指令，就判定為「非查詢」
            if (tempSql.Contains(" UPDATE ") || tempSql.Contains(" DELETE ") || tempSql.Contains(" INSERT "))
            {
                isResult = false;
            }

            return isResult;
        }

        private void editorMessage_Enter(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
            _findAndReplace.Scintilla = (ScintillaEditor)sender;
            MyGlobal.GlobalTemp5 = "CanPasteN";
        }

        private void SetGridToolStripBackColor(bool isColor)
        {
            ChangeBackColor(isColor ? _toolstripFocused : _toolstripUnfocused);
        }

        private void Detect_KeyDown(object sender, KeyEventArgs e)
        {
            _isCtrlKeyDown = e.Control;
        }

        private void ZoomGrid(float pcnt)
        {
            var c1Grid = GetWhichGrid();

            _fontSize = _fontSize == 0 ? 12 : _fontSize;
            c1Grid.RowHeight = (int)(_rowHeight * pcnt) + 5;

            var headerLineCount = GetQueryResultHeaderLineCount(); //20260612 重構

            if (headerLineCount > 1)
            {
                c1Grid.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) * headerLineCount + 11;
            }
            else
            {
                c1Grid.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 12;
            }

            //20240801 變更標題列的字體
            c1Grid.HeadingStyle.Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, _fontSize * pcnt, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.CaptionStyle.Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, _fontSize * pcnt, FontStyle.Regular, GraphicsUnit.Point);

            //20251118 改寫成 ApplyGridHeadingStyle 函數
            ApplyResultGridHeadingStyle(c1TrueDBGrid1);

            c1Grid.RecordSelectorWidth = (int)(_recSelWidth * pcnt);
            c1Grid.Styles["Normal"].Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, _fontSize * pcnt, FontStyle.Regular, GraphicsUnit.Point);

            if (!chkSize.Checked)
            {
                return;
            }

            foreach (C1DisplayColumn col in c1Grid.Splits[_splitsIndex].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 2000;
                }
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            NextPage();
        }

        private void NextPage(int col = -1, int row = -1)
        {
            if (btnCancelQuery.Enabled)
            {
                return; //「正在執行查詢指令」，忽略！
            }

            Application.UseWaitCursor = true;
            c1DockingTab1.Enabled = false;

            _nextPageCol = col == -1 ? c1TrueDBGrid1.Col : col;
            _nextPageRow = row == -1 ? c1TrueDBGrid1.Row : row;

            _isNextPageQuery = true; //分頁查詢！
            btnCancelQuery.Tag = string.Empty;
            MyGlobal.ExecuteNonQuerySqlHistoryScript = string.Empty;

            var c1Grid = GetWhichGrid();
            var sql = c1Grid.AccessibleDescription;

            _dtNextPage = c1Grid.GetDataTableSourceOrNull();

            if (string.IsNullOrEmpty(sql))
            {
                return;
            }

            _nextPageScale += 0.5;
            ExecuteQuery(sql, _isNextPageQuery);
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            _isNextPageQuery = false;
            _lastTimeOffset = 0;

            lblRows.Text = $"0 {_rows}";
            lblSummaryValue.Text = "0";
            lblCountValue.Text = "0";
            lblAverageValue.Text = "0";
            c1TrueDBGrid1.DataSource = _dtNullTable;

            c1GridAutoReplaceInfo.Cursor = Cursors.Default;

            //正常查詢，從 第0頁 開始
            btnNextPage.Tag = "0";

            //正常查詢，ToolTip 恢復為「下一頁」
            btnNextPage.ToolTip = LocalizationHelper.GetLanguageString("Next Page", "form", GetType().Name, "statusbarobject", "btnNextPage", "ToolTipText");

            _dtNextPage = null;

            _nextPageScale = 1;
            btnCancelQuery.Tag = string.Empty;
            MyGlobal.ExecuteNonQuerySqlHistoryScript = string.Empty;
            ExecuteQuery();
        }

        private void ExecuteQuery(string sql = "", bool isNextPage = false, bool isIndicator = true)
        {
            try
            {
                InitializeQueryExecutionState();

                var textContext = ResolveQueryExecutionText(sql);

                //20260501 將查詢前的處理整理成 context
                var executionContext = CreateQueryExecutionContext(textContext, isNextPage);
                var runContext = CreateQueryExecutionPipelineRunContext(executionContext);

                if (!ShouldContinueQueryExecutionPipelineRun(runContext))
                {
                    return;
                }

                PrepareQueryExecutionBaseState(runContext.QueryText, isNextPage);

                //20260501 Query / NonQuery / Extended / CommitRollback 分析結果包裝
                AnalyzeQueryExecutionPipelineRunKind(runContext);

                if (!runContext.HasExecutableStatement)
                {
                    return; //沒有 SQL 指令 (例如全部是註解)
                }

                if (!ConfirmLockingQueryExecutionIfNeeded(runContext.QueryKind))
                {
                    return;
                }

                //20260501 查詢前 UI 狀態套用, 處理 SQL History
                ApplyQueryExecutionTimerBusyState();
                ApplyBeforeQueryExecutionUiState(runContext.QueryKind, runContext.QueryText);
                ApplyQueryExecutionButtonsBusyState();
                HideEditorMessageWelcomeIfNeeded();
                ApplyQueryExecutionMessageState();

                //20260501 Payload 組裝
                BuildQueryExecutionPipelinePayloadText(runContext);

                //20260501 Bookmark 處理：標記目前 Editor 行，和送出給 Reader 的 payload 無直接依賴
                ApplyQueryExecutionBookmark(isIndicator);

                if (string.IsNullOrEmpty(runContext.PayloadText))
                {
                    ResetLockingQueryExecutionState();
                    return;
                }

                //20260501 確認連線, 註冊 QueryCompleted
                if (!TryPrepareQueryExecutionReader())
                {
                    ResetLockingQueryExecutionState();
                    return;
                }

                //20260501 如果是查詢指令，且符合分頁查詢的條件，則自動加上分頁的 Payload
                AppendQueryExecutionPipelinePagingPayloadIfNeeded(runContext);

                //20260501 建立執行 SQL 並啟動查詢執行緒
                StartQueryExecutionThread(runContext.PayloadText, runContext.QueryKind);
            }
            catch (Exception ex)
            {
                ResetLockingQueryExecutionState();
                ShowExceptionMessage(ex);
            }
        }

        private void btnCancelQuery_Click(object sender, EventArgs e)
        {
            var message = string.Empty;

            _queryStatus = "Cancel";
            btnQuery.Tag = string.Empty;
            btnCancelQuery.Tag = "Cancel";
            lblInfo.Text = LocalizationHelper.GetLanguageString("Execution aborted...", "form", GetType().Name, "msg", "ExecutionAborted", "Text");

            if (!tmrQueryTime.Enabled)
            {
                return;
            }

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            message = MyGlobal.OracleReader.InterruptQuery();
                            message = string.IsNullOrEmpty(message) ? string.Empty : $"InterruptQueryMsg: {message}";
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            message = MyGlobal.PostgreSqlReader.InterruptQuery();
                            message = string.IsNullOrEmpty(message) ? string.Empty : $"InterruptQueryMsg: {message}";
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            message = MyGlobal.SqlServerReader.InterruptQuery();
                            message = string.IsNullOrEmpty(message) ? string.Empty : $"InterruptQueryMsg: {message}";
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            message = MyGlobal.MySqlReader.InterruptQuery();
                            message = string.IsNullOrEmpty(message) ? string.Empty : $"InterruptQueryMsg: {message}";
                            break;
                        }
                }

                if (!string.IsNullOrEmpty(message))
                {
                    throw new InvalidOperationException(message);
                }

                //讓 provider 的 Cancel() 正常結束目前命令，並由 Reader 的 finally 觸發 QueryCompleted
                //Thread.Abort() 可能在 Reader 尚未回傳錯誤與交易結果前中斷執行，造成取消的 NonQuery 被誤判為 TX_PENDING
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void QueryCompleted()
        {
            if (InvokeRequired)
            {
                var d = new BindDatagrid(BindDataGridHandler);

                Invoke(d);
            }
            else
            {
                BindDataGridHandler();
            }

            _threadQuery = null;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        MyGlobal.OracleReader.QueryCompleted -= QueryCompleted;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        MyGlobal.PostgreSqlReader.QueryCompleted -= QueryCompleted;
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        MyGlobal.SqlServerReader.QueryCompleted -= QueryCompleted;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        MyGlobal.MySqlReader.QueryCompleted -= QueryCompleted;
                        break;
                    }
            }

            _queryStatus = "Complete"; //20191212
        }

        private void BindDataGridHandler()
        {
            var loadResult = LoadQueryResultData();
            var dtData = loadResult.Data;
            var dtSchemaTable = loadResult.SchemaTable;

            try
            {
                ApplyQueryTimeDisplay();
                ApplyLoadedQueryResultToGrid(dtData, dtSchemaTable); //查詢，進入 ArrangeDataTable !
                ApplyQueryResultDockingTabState();
                ApplyQueryResultColumnList(dtData); //20241031 收集,顯示,隱藏欄位訊息
                ApplyQueryCancelStatusIfNeeded();
                ApplyLockingQueryCompletionState(loadResult); //20260719 套用 SELECT ... FOR UPDATE 語句的狀態
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Regardless of whether Raw Data Mode is checked, _columnInfoCollector is always processed in advance.
        /// </summary>
        /// <param name="dtSchemaTable"></param>
        /// <returns></returns>
        private bool CanUseRawDataMode(DataTable dtData, DataTable dtSchemaTable)
        {
            //20260319 針對不同資料庫來源類型，建立對應的 ColumnInfoCollector
            _columnInfoCollector = SchemaColumnInfoBuilder.Build(_currentSourceType, dtSchemaTable);

            var columnInfos = _columnInfoCollector.GetAll();
            bool hasRickyColumn = columnInfos.Any(
                                                     c =>
                                                     c.CategoryDataTypeKind == CategoryDataTypeKind.LargeText ||
                                                     c.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary
                                                 );

            if (!hasRickyColumn)
            {
                return true;
            }

            return false;
        }

        private void ArrangeDataTable(C1TrueDBGrid c1Grid, DataTable dtData, DataTable dtSchemaTable)
        {
            try
            {
                if (btnPaginationOn.Visible)
                {
                    int.TryParse(TextHelper.GetSafeString(btnPaginationOn.Tag), out var paginationOn);

                    btnNextPage.Enabled = dtData != null && dtData?.Rows.Count > 0 && dtData.Rows.Count >= paginationOn;

                    if (!btnNextPage.Enabled)
                    {
                        //已經是最後一頁了，變更 ToolTip 為「已經是最後一頁了！」
                        btnNextPage.ToolTip = LocalizationHelper.GetLanguageString("It's the last page!", "form", GetType().Name, "statusbarobject", "btnNextPage_LastPage", "ToolTipText");
                    }
                }

                //針對重複的欄位名稱，後面加上流水號 (目的是要跟 dtData 相匹配)
                dtSchemaTable = DataTableColumnHelper.EnsureUniqueFirstColumnNames(dtSchemaTable);

                //20251003 提前新增 Comment 欄位 (不論是否有勾選顯示註解)
                DataTableColumnHelper.SafeAddColumn(dtSchemaTable, "Comment");

                bool shouldDisableRawDataMode = dtData != null && !CanUseRawDataMode(dtData, dtSchemaTable); //20260219 即使 dtData?.Rows.Count = 0 也要整理！

                if (chkRawDataMode.Checked)
                {
                    if (shouldDisableRawDataMode) //包含大型文字或二進制欄位，需要特別處理
                    {
                        var dtSortedData = new DataTable();

                        //整理 dtSortedData 欄位型態
                        foreach (DataColumn column in dtData.Columns)
                        {
                            var columnName = column.ColumnName;

                            //20260222 In Raw Data Mode, all fields use string data types
                            dtSortedData.Columns.Add(columnName, typeof(string));
                        }

                        var truncated = LocalizationHelper.GetLanguageString("…(truncated)", "Global", "Global", "msg", "Truncated…", "Text");
                        var gridNullShowAs = string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs;

                        //逐筆整理資料
                        for (var i = 0; i < dtData.Rows.Count; i++)
                        {
                            var rowData = dtSortedData.NewRow();

                            for (var j = 0; j < dtData.Columns.Count; j++)
                            {
                                var columnName = dtData.Columns[j].ColumnName;

                                if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
                                {
                                    continue;
                                }

                                object rawValue = dtData.Rows[i][columnName];

                                if (DbValueHelper.IsDbNull(rawValue))
                                {
                                    rowData[columnName] = string.Empty;
                                }
                                else if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary)
                                {
                                    var bytes = rawValue as byte[] ?? Array.Empty<byte>();
                                    var fullBytes = bytes;
                                    var fullLength = fullBytes?.Length ?? 0;

                                    rowData[columnName] = $"({columnInfo.BaseDataType})({fullLength})";
                                }
                                else if (columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText)
                                {
                                    var fullText = rawValue?.ToString() ?? string.Empty;
                                    var previewResult = LargeTextPreviewBuilder.Build(fullText, LargeTextPreviewLengthPolicy.RawDataModeLength, truncated);

                                    rowData[columnName] = $"{previewResult.DisplayText}{previewResult.PreviewText}";
                                }
                                else
                                {
                                    rowData[columnName] = rawValue.ToString();
                                }
                            }

                            dtSortedData.Rows.Add(rowData);

                            if (TextHelper.GetSafeString(btnCancelQuery.Tag) == "Cancel")
                            {
                                break;
                            }
                        }

                        dtData = dtSortedData;

                        var message = LocalizationHelper.GetLanguageString("Because this query contains large text or binary fields, to prevent performance issues, Raw Data Mode may undergo special processing, for example, the content might be condensed into a summary.", "form", GetType().Name, "msg", "RawDataModeDisableOnce", "Text");

                        //RawDataMode + Large Field
                        SetFormStatusBarInfo(message, Color.DarkRed);
                    }

                    if (!_isNextPageQuery || btnAppendingQueriesOff.Visible)
                    {
                        c1Grid.DataSource = dtData;
                    }
                    else
                    {
                        if (dtData?.Rows.Count > 0)
                        {
                            _dtNextPage.Merge(dtData, true);
                        }

                        c1Grid.DataSource = _dtNextPage;
                    }

                    //20240803 針對「原始資料模式」，放大縮小表格
                    GridZoom(c1Grid);

                    c1Grid.Show();
                }
                else
                {
                    var context = new ArrangeContext
                    {
                        SourceData = dtData,
                        SchemaTable = dtSchemaTable,
                        ShowColumnType = chkShowColumnType.Checked,
                        ShowColumnComments = chkShowColumnComments.Checked,
                        LoadColumnCommentsForCellTip = ShouldLoadColumnCommentsForCellTip(),
                        DisplayContext = _currentDisplayContext,
                        columnInfoCollector = _columnInfoCollector,
                        FormatterMode = FormatterMode.Grid,
                        DateFormat = MyLibrary.DateFormat,
                        DateTimeFormat = $"{MyLibrary.DateFormat} HH:mm:ss",
                        NullDisplayText = string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : MyLibrary.GridNullShowAs,
                        LargeTextPreviewLength = AppConfigHelper.LargeTextPreviewLength,
                        IsCancellationRequested = () => TextHelper.GetSafeString(btnCancelQuery.Tag) == "Cancel",
                        TruncatedText = LocalizationHelper.GetLanguageString("…(truncated)", "Global", "Global", "msg", "Truncated…", "Text")
                    };

                    var strategy = ArrangeStrategyFactory.Create(_currentSourceType);

                    strategy.Execute(context);

                    c1Grid.Tag = context.DistinctTableNameViewName;
                    c1Grid.DataSource = context.SortedData;
                    _currentQueryResultHasColumnComments = context.HasAnyComment;

                    if (!_isNextPageQuery || btnAppendingQueriesOff.Visible)
                    {
                        context.totalRows = context.SortedData.Rows.Count;
                        c1Grid.DataSource = context.SortedData;
                    }
                    else
                    {
                        if (dtData?.Rows.Count > 0)
                        {
                            _dtNextPage.Merge(context.SortedData, true);
                        }

                        context.totalRows = _dtNextPage.Rows.Count;
                        c1Grid.DataSource = _dtNextPage;
                    }

                    foreach (C1DataColumn column in c1Grid.Columns)
                    {
                        var columnName = column.Caption;

                        if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
                        {
                            continue;
                        }

                        column.Caption = ColumnHeaderFormatter.Format(columnInfo, context.DisplayContext.Mode); //Show full column name based on user check or not
                    }

                    if (string.IsNullOrEmpty(context.DistinctTableNameViewName))
                    {
                        //20251005 如果無法取得任何的 BaseTableName，顯示額外的訊息
                        SetFormStatusBarInfo(_sqlCannotGetColumnInfoMessage, Color.DarkRed);
                    }
                    else if (chkShowColumnComments.Checked && !context.HasAnyComment)
                    {
                        //20240803 此段 SQL 所有欄位都不包含註解
                        SetEditorStatusBarInfo(_sqlWithoutCommentMessage, Color.DarkRed);
                    }

                    if (context.totalRows == 0)
                    {
                        if (ShouldUseMultiLineQueryResultHeader())
                        {
                            GridZoom(c1Grid); //20240806 沒有任何資料，也要調整標題列
                        }
                        else
                        {
                            //20260606 沒有任何資料，也要調整標題列
                            ApplyResultGridHeadingStyle(c1Grid);
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                _dtExportedSchemaTable = dtSchemaTable;
            }

            //20241013 模擬按下 HOME/END，控制水平 ScrollBar 移到最左側
            #region
            var count1 = c1Grid.Columns.Count;
            var count2 = c1Grid.Splits[_splitsIndex].DisplayColumns.Count;

            if (count1 == count2)
            {
                c1Grid.Col = count1 - 1;
            }
            else
            {
                int columns = Math.Min(count1, count2) - 1;

                columns = columns < 0 ? 0 : columns;
                c1Grid.Col = columns;
            }

            c1Grid.Row = 0;
            c1Grid.Select();
            #endregion

            if (chkSize.Checked)
            {
                GridHelper.ResizeGridColumnWidth(c1Grid);
            }

            c1Grid.Refresh();

            //20240731 調整標題列
            GridZoom(c1Grid);
        }

        private bool ShouldLoadColumnCommentsForCellTip()
        {
            //Raw Data Mode 強調原始與快速顯示，不額外查詢欄位註釋
            if (chkRawDataMode.Checked)
            {
                return false;
            }

            //Header 已經顯示 Comment 時，不需要為 CellTip 額外判斷
            if (chkShowColumnComments.Checked)
            {
                return false;
            }

            return true;
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            if (btnCancelQuery.Enabled)
            {
                //如果「正在執行查詢指令」，忽略 Commit / Rollback 按鈕
                return;
            }

            try
            {
                ExecuteAndApplyTransactionAction(DatabaseTransactionActionKind.Commit);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            if (btnCancelQuery.Enabled)
            {
                //如果「正在執行查詢指令」，忽略 Commit / Rollback 按鈕
                return;
            }

            try
            {
                ExecuteAndApplyTransactionAction(DatabaseTransactionActionKind.Rollback);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private DatabaseTransactionActionResult ExecuteAndApplyTransactionAction(DatabaseTransactionActionKind actionKind)
        {
            var result = ExecuteTransactionAction(actionKind);

            ApplyTransactionActionResult(result);

            return result;
        }

        private DatabaseTransactionActionResult ExecuteTransactionAction(DatabaseTransactionActionKind actionKind)
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return actionKind == DatabaseTransactionActionKind.Commit ? DatabaseTransactionActionHelper.TryCommitAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.OracleReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Commit(),
                                                                                        reader => reader.Disconnect()
                                                                                    )
                                                                                  : DatabaseTransactionActionHelper.TryRollbackAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.OracleReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Rollback(),
                                                                                        reader => reader.Disconnect()
                                                                                    );
                    }
                case DataSourceType.PostgreSql:
                    {
                        return actionKind == DatabaseTransactionActionKind.Commit ? DatabaseTransactionActionHelper.TryCommitAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.PostgreSqlReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Commit(),
                                                                                        reader => reader.Disconnect()
                                                                                    )
                                                                                  : DatabaseTransactionActionHelper.TryRollbackAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.PostgreSqlReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Rollback(),
                                                                                        reader => reader.Disconnect()
                                                                                    );
                    }
                case DataSourceType.SqlServer:
                    {
                        return actionKind == DatabaseTransactionActionKind.Commit ? DatabaseTransactionActionHelper.TryCommitAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.SqlServerReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Commit(),
                                                                                        reader => reader.Disconnect()
                                                                                    )
                                                                                  : DatabaseTransactionActionHelper.TryRollbackAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.SqlServerReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Rollback(),
                                                                                        reader => reader.Disconnect()
                                                                                    );
                    }
                case DataSourceType.MySql:
                    {
                        return actionKind == DatabaseTransactionActionKind.Commit ? DatabaseTransactionActionHelper.TryCommitAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.MySqlReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Commit(),
                                                                                        reader => reader.Disconnect()
                                                                                    )
                                                                                  : DatabaseTransactionActionHelper.TryRollbackAndDisconnectResult
                                                                                    (
                                                                                        MyGlobal.MySqlReader,
                                                                                        reader => reader.GetState(),
                                                                                        reader => reader.Rollback(),
                                                                                        reader => reader.Disconnect()
                                                                                    );
                    }
                default:
                    {
                        return new DatabaseTransactionActionResult
                        {
                            ActionKind = actionKind,
                            Message = "ErrorMsg: Commit or Rollback was not executed because no database type is selected."
                        };
                    }
            }
        }

        private void ApplyTransactionActionResult(DatabaseTransactionActionResult result)
        {
            if (result == null)
            {
                return;
            }

            c1DockingTab1.SelectedTab = tabMessage;

            UpdateMessage(result.Message);

            var closeSuffix = MyGlobal.CommitRollbackCheck == -1 ? string.Empty : "\r\n(when user closes JasonQuery)";
            var historyResult = result.TransactionSucceeded ? "Complete" : "Error";

            UpdateSqlHistory(0, historyResult, $"{result.Message}{closeSuffix}", string.Empty);

            if (result.ShouldClearPendingState)
            {
                //只有 Commit / Rollback 確實成功，才清除 MainForm 的 pending 狀態。Disconnect 失敗不代表交易仍 pending。
                TransferValueToMainForm("ExecuteCommitRollback`");
            }
        }


        private static void CancelCloseAfterTransactionFailure()
        {
            //交易沒有成功完成時，視同取消關閉；保留 Pending、Commit/Rollback 按鈕與提醒計時器。
            MyGlobal.CommitRollbackCheck = 2;
            MyGlobal.GlobalTemp = string.Empty;
        }

        private void TransferValueToMainForm(string value)
        {
            var valueArgs = new ValueUpdatedEventArgs(value);

            ValueUpdated(this, valueArgs);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            TransferValueToMainForm("CreateNewTab`");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            RequestOpenFileInNewTab();
        }

        private void RequestOpenFileInNewTab(string fileName = "")
        {
            try
            {
                var message = string.IsNullOrEmpty(fileName) ? "CreateNewTab`OPENFILE" : $"CreateNewTab`OPENFILE`{fileName}";

                TransferValueToMainForm(message);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void RequestOpenExternalFileInNewTab(string fileName)
        {
            RequestOpenFileInNewTab(fileName);
        }

        private bool HandleAutoCompletePeriodKey(int triggerPositionOverride = 0) //triggerPositionOverride: 透過 Ctrl+J 或重新觸發 Period 傳進來
        {
            return _autoCompleteFacade.HandlePeriod(triggerPositionOverride);
        }

        private bool HandleAutoCompleteSpaceKey(int triggerPositionOverride = 0)
        {
            return _autoCompleteFacade.HandleSpace(triggerPositionOverride);
        }

        private void txtIndentWord_Enter(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripFocused;
            tsDataGrid.BackColor = _toolstripUnfocused;
        }

        private void txtIndentWord_Leave(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;

            if (string.IsNullOrEmpty(txtIndentWord.Text) || txtIndentWord.Text == "0")
            {
                txtIndentWord.Text = @"4";
            }
        }

        private void txtIndentWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtIndentWord_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtIndentWord_KeyUp(object sender, KeyEventArgs e)
        {
            //Editor 上面的垂直虛線，是根據 editor.TabWidth 來呈現的
            if (string.IsNullOrWhiteSpace(txtIndentWord.Text))
            {
                return;
            }

            int.TryParse(txtIndentWord.Text, out var tabWidth);

            editor.TabWidth = tabWidth;
            editor.Focus();
        }

        private void txtIndentWord_MouseClick(object sender, MouseEventArgs e)
        {
            txtIndentWord.SelectionStart = 0;
            txtIndentWord.SelectionLength = 1;
        }

        private void btnComment_Click(object sender, EventArgs e)
        {
            AddComment();
        }

        private void btnRemoveComment_Click(object sender, EventArgs e)
        {
            RemoveComment();
        }

        private void tmrExecTime_Tick(object sender, EventArgs e)
        {
            var execTime = MyGlobal.DateDiff(_startTime, DateTime.Now);

            lblExecTime.Text = $"{_execTime} {execTime}";
        }

        private void tmrCheckIdleTime_Tick(object sender, EventArgs e)
        {
            if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo == -1)
            {
                tmrCheckIdleTime.Enabled = false;

                using (TraceLogger.Time("Update Schema Info, call UpdateSchemaInfo() from tmrCheckIdleTime_Tick()"))
                {
                    UpdateSchemaInfo();
                }
            }
        }

        private void tmrQueryTime_Tick(object sender, EventArgs e)
        {
            var queryTime = MyGlobal.DateDiff(_startTime, DateTime.Now);

            lblQueryTime.Text = $"{_queryTime} {queryTime}";
        }

        private void tmrlblInfo_Tick(object sender, EventArgs e)
        {
            //20201105
            if (btnCancelQuery.Enabled && !tmrExecTime.Enabled && !tmrQueryTime.Enabled)
            {
                btnCancelQuery.Enabled = false; //20200427
            }

            if (lblInfo.Text != TextHelper.GetSafeString(lblInfo.Tag))
            {
                lblInfo.Tag = lblInfo.Text;
                c1StatusBar1.Tag = MyGlobal.DateTimeNow();
            }

            //15 秒後，將 lblInfo.Text 清空！
            if (TextHelper.IsNullOrEmptyTag(c1StatusBar1.Tag))
            {
                return;
            }

            DateTime dtLast = Convert.ToDateTime(TextHelper.GetSafeString(c1StatusBar1.Tag));
            DateTime dtNow = Convert.ToDateTime(MyGlobal.DateTimeNow());
            double diffSeconds = (dtNow - dtLast).TotalSeconds;

            if (diffSeconds < 15)
            {
                return;
            }

            SetFormStatusBarInfo(string.Empty, Color.Black);
            lblInfo.Tag = string.Empty;
            c1StatusBar1.Tag = string.Empty;
        }

        private void tmrlblInfoEditor_Tick(object sender, EventArgs e)
        {
            if (lblInfoEditor.Text != TextHelper.GetSafeString(lblInfoEditor.Tag))
            {
                lblInfoEditor.Tag = lblInfoEditor.Text;
                c1StatusBar2.Tag = MyGlobal.DateTimeNow();
            }

            //15 秒後，將 lblInfo.Text 清空！
            if (TextHelper.IsNullOrEmptyTag(c1StatusBar2.Tag))
            {
                return;
            }

            DateTime dtLast = Convert.ToDateTime(TextHelper.GetSafeString(c1StatusBar2.Tag));
            DateTime dtNow = Convert.ToDateTime(MyGlobal.DateTimeNow());
            double diffSeconds = (dtNow - dtLast).TotalSeconds;

            if (diffSeconds < 15)
            {
                return;
            }

            SetEditorStatusBarInfo(string.Empty, Color.Black);
            lblInfoEditor.Tag = string.Empty;
            c1StatusBar2.Tag = string.Empty;
        }

        private void tmrHideMessageWelcome_Tick(object sender, EventArgs e)
        {
            _welcomeCountdownSeconds--;

            if (_welcomeCountdownSeconds > 0)
            {
                int minutes = _welcomeCountdownSeconds / 60;
                int seconds = _welcomeCountdownSeconds % 60;

                lblWelcomeCountdown.Text = $"{minutes:D2}:{seconds:D2}";
            }
            else
            {
                lblWelcomeCountdown.Text = string.Empty;
                tmrHideMessageWelcome.Stop();

                if (string.IsNullOrEmpty(pnlEditorMessageWelcome.AccessibleDescription))
                {
                    pnlEditorMessageWelcome.Visible = false;
                    pnlEditorMessageWelcome.AccessibleDescription = "Hide";
                }
            }
        }

        //從母表單傳遞資訊至指定的子表單
        private void tmrMother2Child_Tick(object sender, EventArgs e)
        {
            var fileName = string.Empty;
            var temp = string.Empty;
            var temp2 = string.Empty;
            var temp3 = string.Empty;

            if (!string.IsNullOrEmpty(AccessibleDescription) && MyGlobal.CheckFileFromMDIForm.Contains($"`{AccessibleDescription}`"))
            {
                //<--Begin:CheckFileDataTimeAndExist() 之前，要先執行 MyGlobal.sCheckFileFromMDIForm 變數取代
                //判斷是否有相同的 MessageBox 正在顯示中...避免重複顯示
                temp = TextHelper.GetSafeString(btnSave.Tag);
                MyGlobal.CheckFileFromMDIForm = MyGlobal.CheckFileFromMDIForm.Replace($"`{AccessibleDescription}", string.Empty);

                if (MyGlobal.CheckFileFromMDIForm == "`")
                {
                    MyGlobal.CheckFileFromMDIForm = string.Empty;
                }

                if (!string.IsNullOrEmpty(temp)) //有存檔過
                {
                    CheckFileDateTimeAndExist();
                }
                //-->End:CheckFileDataTimeAndExist 之前，要先執行 MyGlobal.sCheckFileFromMDIForm 變數取代

                //這裡要透過全域變數控制「本程式正在作用中」，否則按下「確定」後，從 MessageBox 切換到 Main Form，又會被 Main Form 誤判為 ContainsFocus = true
                MyGlobal.IsContainsFocusFormOptionsKey = true;
            }

            //判斷是否為指定的子表單
            if (!string.IsNullOrEmpty(MyGlobal.OpenFileFromMDIForm) && !string.IsNullOrEmpty(AccessibleDescription) && MyGlobal.OpenFileFromMDIForm.Substring(0, 17) == AccessibleDescription)
            {
                fileName = MyGlobal.OpenFileFromMDIForm.Substring(15);
                MyGlobal.OpenFileFromMDIForm = string.Empty;

                RequestOpenFileInNewTab(fileName);
                return;
            }

            if (MyGlobal.GlobalTemp5.StartsWith("ReloadQueryEditorSetting`", StringComparison.Ordinal)) //是否為 Editor Setting 套用？
            {
                temp = MyGlobal.GlobalTemp5.Replace("ReloadQueryEditorSetting`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp5 = MyGlobal.GlobalTemp5.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.GlobalTemp5 == "ReloadQueryEditorSetting`")
                    {
                        MyGlobal.GlobalTemp5 = string.Empty;
                    }

                    ReloadQueryEditorSetting();
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("ExecuteCommitRollback`", StringComparison.Ordinal)) //是否為「按下 Commit / Rollback 按鈕」，或「執行 Commit / Rollback 指令」？
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

                    AppConfigHelper.IsNotCommitYet = false;

                    DisconnectDatabase(); //共用同一個 Connection，所以，Commit/Rollback 之後，每一個 Query Editor 的 lblNotCommitYet.Text 都要清空
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("UpdateCommitRollbackButton`", StringComparison.Ordinal)) //如果是 nonquery, 執行無錯誤, 更新 Commit / Rollback 按鈕
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

                    ApplyCommitRollbackButtonState(true);
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("DisconnectAfterQueryOnly`", StringComparison.Ordinal)) //如果是單純查詢，且不需要等待 Commit / Rollback，全部「中斷連線」
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("DisconnectAfterQueryOnly`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "DisconnectAfterQueryOnly`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    AppConfigHelper.IsNotCommitYet = false;

                    DisconnectDatabase(); //共用同一個 Connection，所以，Commit/Rollback 之後，每一個 Query Editor 的 lblNotCommitYet.Text 都要清空
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("DisconnectAfterExecuteError`", StringComparison.Ordinal)) //如果是 PostgreSQL，執行指令有錯誤：全部中斷連線
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("DisconnectAfterExecuteError`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "DisconnectAfterExecuteError`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    AppConfigHelper.IsNotCommitYet = false;

                    ApplyCommitRollbackButtonState(false);

                    MyGlobal.PostgreSqlReader.Rollback();
                    MyGlobal.PostgreSqlReader.Disconnect();
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("UpdateSchemaInformation`", StringComparison.Ordinal)) //20241011 更新 Schema 資訊
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("UpdateSchemaInformation`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "UpdateSchemaInformation`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    //20250912 加上提示視窗 MessageForm
                    var form = new MessageForm();

                    form.Caption = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");
                    form.Info = LocalizationHelper.GetLanguageString("Organize schema information…", "Global", "Global", "msg", "OrganizeSchemaInfo", "Text");
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.IsNeedToMovePosition = true;
                    form.TopMost = true; //TopMost 會在「所有的程式」最上層顯示
                    form.TopLevel = true;
                    form.Show(this);
                    form.Refresh();
                    Application.UseWaitCursor = true;

                    using (TraceLogger.Time("Organize schema information"))
                    {
                        GridHelper.UpdateSchemaData(c1GridSchemaBrowser);
                        txtSchemaFilter.Text = "*";
                        txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);
                        AutoResizeGridColumnWidthForSchema();
                    }

                    editor.Focus();
                    form.Dispose();
                    MyGlobal.ClearMemory();

                    Application.UseWaitCursor = false;
                }
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("UpdateSchemaInformationRefreshClick`", StringComparison.Ordinal)) //20241011 在 QueryForm 按下 Refresh 更新 Schema 資訊
            {
                temp = MyGlobal.InfoFromMDIForm.Replace("UpdateSchemaInformationRefreshClick`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromMDIForm = MyGlobal.InfoFromMDIForm.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromMDIForm == "UpdateSchemaInformationRefreshClick`")
                    {
                        MyGlobal.InfoFromMDIForm = string.Empty;
                    }

                    GridHelper.UpdateSchemaData(c1GridSchemaBrowser);
                    txtSchemaFilter.Text = "*";
                    txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);
                    AutoResizeGridColumnWidthForSchema();
                    editor.Focus();
                    MyGlobal.ClearMemory();
                }
            }
            else if (MyGlobal.GlobalTemp5.StartsWith("UpdateSchemaInformation`", StringComparison.Ordinal)) //20250204 在 QueryForm 更新 Schema 資訊
            {
                if (string.IsNullOrEmpty(MyGlobal.GlobalTemp6)) //待 SchemaBrowserForm 更新完畢後，再更新 QueryForm
                {
                    temp = MyGlobal.GlobalTemp5.Replace("UpdateSchemaInformation`", string.Empty);
                    temp = temp.Split(';')[0];

                    if (temp == AccessibleDescription) //確認是否為指定的 Tab
                    {
                        MyGlobal.GlobalTemp5 = string.Empty; //觸發一次即可，後續再由「MyGlobal.sInfoFromMDIForm = "UpdateSchemaInformation`"」接手

                        btnRefresh.PerformClick();
                    }
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith("AutoDisconnect`", StringComparison.Ordinal)) //是否為主表單通知要自動中斷連線？
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
            else if (MyGlobal.InfoFromReloadLocalization.StartsWith("ReloadLocalization`", StringComparison.Ordinal)) //是否為 Reload Localization 套用？
            {
                temp = MyGlobal.InfoFromReloadLocalization.Replace("ReloadLocalization`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.InfoFromReloadLocalization = MyGlobal.InfoFromReloadLocalization.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.InfoFromReloadLocalization == "ReloadLocalization`")
                    {
                        MyGlobal.InfoFromReloadLocalization = string.Empty;
                    }

                    using (TraceLogger.Time("Apply Localization Setting & Reload QueryEditor Setting"))
                    {
                        try
                        {
                            ApplyLocalizationSetting(false);
                            ReloadQueryEditorSetting();
                            ApplyQueryResultOptionsToolbarLayout();
                        }
                        catch (Exception ex)
                        {
                            ShowExceptionMessage(ex);
                        }
                        finally
                        {
                            RequestFocusEditorAfterLocalization();
                        }
                    }
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith($"SQLExecuteErrorPos{MyGlobal.Separator}", StringComparison.Ordinal)) //執行 SQL Statement 出錯了：取得波浪底線要呈現的位置
            {
                DispatchSqlExecuteErrorPositionMessage();
            }
            else if (MyGlobal.GlobalTemp.StartsWith($"GenerateSQLPaste2QueryEditor{MyGlobal.Separator}", StringComparison.Ordinal)) //產生 SQL 指令：貼至 Query Editor
            {
                temp = MyGlobal.GlobalTemp.Replace($"GenerateSQLPaste2QueryEditor{MyGlobal.Separator}", string.Empty);
                temp2 = temp.Split(new[] { MyGlobal.SeparatorPlus1 }, StringSplitOptions.None)[0]; //accessibleDescription
                temp3 = temp.Substring(temp2.Length + MyGlobal.SeparatorPlus1.Length);

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp = string.Empty;

                    Clipboard.SetDataObject(temp3, false);
                    editor.Paste();
                    editor.Focus();
                }
            }

            if (IsSqlServer && MyGlobal.GlobalTemp.StartsWith($"SQLServerSwitchDatabaseFromMainForm{MyGlobal.Separator}", StringComparison.Ordinal)) //20220808 SQL Server，使用者透過功能表切換資料庫
            {
                temp = MyGlobal.GlobalTemp.Replace($"SQLServerSwitchDatabaseFromMainForm{MyGlobal.Separator}", string.Empty);

                var parts = temp.Split(';');

                temp2 = parts[0]; //sAccessibleDescription, 指定哪一個 QueryEditor
                temp3 = parts[1]; //切換哪一個資料庫

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    //20220808 此處只要直接執行 use 指令即可，接著會再呼叫 SQLServerSwitchDatabase 執行後續動作
                    ExecuteQuery($"--Switch database\r\nUSE {temp3}", false, false);
                }
            }

            if (IsSqlServer && MyGlobal.GlobalTemp4.StartsWith($"SQLServerSwitchOKDatabaseFromMainForm{MyGlobal.Separator}", StringComparison.Ordinal)) //20220808 SQL Server，使用者透過功能表切換資料庫完畢，將其他頁籤的「切換資料庫」提示訊息清空
            {
                temp = MyGlobal.GlobalTemp4.Replace($"SQLServerSwitchOKDatabaseFromMainForm{MyGlobal.Separator}", string.Empty);
                temp2 = temp.Split('`')[0]; //sAccessibleDescription, 指定哪一個 QueryEditor

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp4 = MyGlobal.GlobalTemp4.Replace($"{AccessibleDescription}`", string.Empty);

                    SetEditorStatusBarInfo(string.Empty, Color.Black);

                    if (MyGlobal.GlobalTemp4 == $"SQLServerSwitchOKDatabaseFromMainForm{MyGlobal.Separator}")
                    {
                        MyGlobal.GlobalTemp4 = string.Empty;
                    }
                }
            }

            if (IsSqlServer && MyGlobal.GlobalTemp2.StartsWith($"SQLServerSwitchDatabase{MyGlobal.Separator}", StringComparison.Ordinal)) //20220423 SQL Server 使用 USE 指令切換資料庫
            {
                temp = MyGlobal.GlobalTemp2.Replace($"SQLServerSwitchDatabase{MyGlobal.Separator}", string.Empty);

                var parts = temp.Split(';');

                temp2 = parts[0]; //sAccessibleDescription, 識別從哪一個 QueryEditor 傳過來的 SQL
                temp3 = parts[1]; //切換哪一個資料庫

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp2 = string.Empty;

                    if (DatabaseSqlExecutor.DbConnectionString.IndexOf("Initial Catalog", StringComparison.Ordinal) == -1)
                    {
                        DatabaseSqlExecutor.DbConnectionString = DatabaseSqlExecutor.DbConnectionString.Replace("Integrated Security", $"Initial Catalog={temp3};Integrated Security");
                    }
                    else
                    {
                        var temp4 = TextHelper.GetStringBetween2(DatabaseSqlExecutor.DbConnectionString, "Initial Catalog=", ";", false);

                        if (!string.IsNullOrEmpty(temp4))
                        {
                            DatabaseSqlExecutor.DbConnectionString = DatabaseSqlExecutor.DbConnectionString.Replace($"Initial Catalog={temp4};", $"Initial Catalog={temp3};");
                        }
                    }

                    DatabaseSqlExecutor.DatabaseName = temp3;

                    //傳遞資訊至 MainForm，更新 Database 資訊
                    TransferValueToMainForm($"UpdateDatabaseInfo`{DatabaseSqlExecutor.DatabaseName}");

                    //20220803 切換資料庫，清空 Label 訊息
                    SetEditorStatusBarInfo(string.Empty, Color.Black);

                    //20250914 搜尋功能要再改寫，此處先註釋掉！
                    //DatabaseHelper.RefreshDataForSchemaSearch();

                    var form = new MessageForm();
                    var message2 = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");

                    form.Caption = message2;

                    if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1)
                    {
                        message2 = LocalizationHelper.GetLanguageString("Getting schema information (include all column information of Tables) ...", "Global", "Global", "msg", "GetSchemaInfoIncludeColumn", "Text");
                    }
                    else
                    {
                        message2 = LocalizationHelper.GetLanguageString("Getting schema information...", "Global", "Global", "msg", "GetSchemaInfo", "Text");
                    }

                    form.Info = message2;
                    form.IsNeedToMovePosition = true;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.TopLevel = true; //只在 JasonQuery 最上層顯示
                    form.Show(this);
                    form.Refresh();

                    //依切換的資料庫，重新載入一次最新的 Schema Data for SchemaBrowser
                    Application.UseWaitCursor = true;
                    DatabaseSqlExecutor.UpdateSchemaData_SqlServer(c1GridSchemaBrowser, false);
                    DatabaseSqlExecutor.UpdateTableAndViewInfoForAutoComplete_SqlServer(DatabaseSqlExecutor.DatabaseName); //透過 USE 指令 切換資料庫
                    DatabaseSqlExecutor.dtTableAndViews.Merge(DatabaseSqlExecutor.dtDatabaseName); //切換資料庫後，要再合併所有的資料庫名稱，供 USE 指令使用
                    Application.UseWaitCursor = false;

                    form.Dispose();

                    using (TraceLogger.Time("Auto Resize Grid Column Width"))
                    {
                        AutoResizeGridColumnWidth();
                    }

                    TransferValueToMainForm($"SQLServerSwitchOKDatabaseByUseCommand`{AccessibleDescription}");

                    //20220805 切換資料庫，Tag = "NonQuery"，最後會直接切換到「tabMessage」頁籤
                    editorMessage.Tag = "NonQuery";
                }
            }

            if (IsMySql && MyGlobal.GlobalTemp.StartsWith($"MySQLSwitchDatabaseFromMainForm{MyGlobal.Separator}", StringComparison.Ordinal)) //20220808 MySQL，使用者透過功能表切換資料庫
            {
                temp = MyGlobal.GlobalTemp.Replace($"MySQLSwitchDatabaseFromMainForm{MyGlobal.Separator}", string.Empty);

                var parts = temp.Split(';');

                temp2 = parts[0]; //sAccessibleDescription, 指定哪一個 QueryEditor
                temp3 = parts[1]; //切換哪一個資料庫

                var temp4 = parts[2]; //是否有其他頁籤要清空「切換資料庫」的提示訊息？

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp = string.Empty;

                    //20220808 此處只要直接執行 use 指令即可，接著會再呼叫 MySQLSwitchDatabase 執行後續動作
                    ExecuteQuery($"--Switch database\r\nUSE {temp3}", false, false);
                }
            }

            if (IsMySql && MyGlobal.GlobalTemp4.StartsWith($"MySQLSwitchOKDatabaseFromMainForm{MyGlobal.Separator}", StringComparison.Ordinal)) //20220808 MySQL，使用者透過功能表切換資料庫完畢，將其他頁籤的「切換資料庫」提示訊息清空
            {
                temp = MyGlobal.GlobalTemp4.Replace($"MySQLSwitchOKDatabaseFromMainForm{MyGlobal.Separator}", string.Empty);
                temp2 = temp.Split('`')[0]; //sAccessibleDescription, 指定哪一個 QueryEditor

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp4 = MyGlobal.GlobalTemp4.Replace($"{AccessibleDescription}`", string.Empty);

                    SetEditorStatusBarInfo(string.Empty, Color.Black);

                    if (MyGlobal.GlobalTemp4 == $"MySQLSwitchOKDatabaseFromMainForm{MyGlobal.Separator}")
                    {
                        MyGlobal.GlobalTemp4 = string.Empty;
                    }
                }
            }

            if (IsMySql && MyGlobal.GlobalTemp2.StartsWith($"MySQLSwitchDatabase{MyGlobal.Separator}", StringComparison.Ordinal)) //20220515 MySQL 使用 USE 指令切換資料庫，重新連線！
            {
                temp = MyGlobal.GlobalTemp2.Replace($"MySQLSwitchDatabase{MyGlobal.Separator}", string.Empty);

                var parts = temp.Split(';');

                temp2 = parts[0]; //sAccessibleDescription, 識別從哪一個 QueryEditor 傳過來的 SQL
                temp3 = parts[1]; //切換哪一個資料庫

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp2 = string.Empty;

                    if (DatabaseSqlExecutor.DbConnectionString.IndexOf("Database=", StringComparison.Ordinal) == -1)
                    {
                        DatabaseSqlExecutor.DbConnectionString = DatabaseSqlExecutor.DbConnectionString.Replace("Unicode", $"Database={temp3};Unicode");
                    }
                    else
                    {
                        var temp4 = TextHelper.GetStringBetween2(DatabaseSqlExecutor.DbConnectionString, "Database=", ";", false);

                        if (!string.IsNullOrEmpty(temp4))
                        {
                            DatabaseSqlExecutor.DbConnectionString = DatabaseSqlExecutor.DbConnectionString.Replace($"Database={temp4};", $"Database={temp3};");
                        }
                    }

                    DatabaseSqlExecutor.DatabaseName = temp3;

                    //傳遞資訊至 MainForm，更新 Database 資訊
                    TransferValueToMainForm($"UpdateDatabaseInfo`{DatabaseSqlExecutor.DatabaseName}");

                    //20220803 切換資料庫，清空 Label 訊息
                    SetEditorStatusBarInfo(string.Empty, Color.Black);

                    //20250914 搜尋功能要再改寫，此處先註釋掉！
                    //DatabaseHelper.RefreshDataForSchemaSearch();

                    var form = new MessageForm();
                    var messageWait = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");

                    form.Caption = messageWait;

                    if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1)
                    {
                        messageWait = LocalizationHelper.GetLanguageString("Getting schema information (include all column information of Tables) ...", "Global", "Global", "msg", "GetSchemaInfoIncludeColumn", "Text");
                    }
                    else
                    {
                        messageWait = LocalizationHelper.GetLanguageString("Getting schema information...", "Global", "Global", "msg", "GetSchemaInfo", "Text");
                    }

                    form.Info = messageWait;
                    form.IsNeedToMovePosition = true;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.TopLevel = true; //只在 JasonQuery 最上層顯示
                    form.Show(this);
                    form.Refresh();

                    //依切換的資料庫，重新載入一次最新的 Schema Data for SchemaBrowser
                    Application.UseWaitCursor = true;
                    DatabaseSqlExecutor.UpdateSchemaData_MySql(c1GridSchemaBrowser);
                    DatabaseSqlExecutor.UpdateTableAndViewInfoForAutoComplete_MySql(DatabaseSqlExecutor.DatabaseName); //透過 USE 指令 切換資料庫
                    DatabaseSqlExecutor.dtTableAndViews.Merge(DatabaseSqlExecutor.dtDatabaseName); //切換資料庫後，要再合併所有的資料庫名稱，供 USE 指令使用
                    Application.UseWaitCursor = false;

                    form.Dispose();

                    using (TraceLogger.Time("Auto Resize Grid Column Width, MySQLSwitchDatabase"))
                    {
                        AutoResizeGridColumnWidth();
                    }

                    TransferValueToMainForm($"MySQLSwitchOKDatabaseByUseCommand`{AccessibleDescription}");

                    //20220805 切換資料庫，Tag = "NonQuery"，最後會直接切換到「tabMessage」頁籤
                    editorMessage.Tag = "NonQuery";
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith($"SQLExecuteAffected{MyGlobal.Separator}", StringComparison.Ordinal)) //執行「異動的 SQL」，取得異動的筆數
            {
                temp = MyGlobal.GlobalTemp.Replace($"SQLExecuteAffected{MyGlobal.Separator}", string.Empty);

                var parts = temp.Split(';');

                temp2 = parts[0]; //AccessibleDescription, 識別從哪一個 QueryEditor 傳過來的 SQL
                temp3 = parts[1]; //異動的筆數

                if (temp2 == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp = string.Empty;

                    UpdateMessage(temp3);

                    //20240903 如果異動的筆數為 0筆，不更新狀態
                    if (temp3 == "0 row updated.")
                    {
                        editorMessage.Tag = "NonQuery0Row";
                        btnQuery.AccessibleDescription = string.Empty;
                    }
                    else
                    {
                        editorMessage.Tag = "NonQuery";
                    }

                    editorMessage.LineScroll(editorMessage.Lines.Count, 0);

                    var transactionState = parts.Length >= 3 ? parts[2] : "TX_UNCHANGED";

                    switch (transactionState)
                    {
                        case "TX_PENDING":
                            {
                                TransferValueToMainForm("UpdateCommitRollbackButton`");
                                break;
                            }
                        case "TX_CLOSED":
                            {
                                DisconnectDatabase();
                                TransferValueToMainForm("ExecuteCommitRollback`");
                                break;
                            }
                    }

                    //20260720 交易狀態已由 Reader 回傳的 TX_* Token 完整處理，清空此旗標，避免 QueryCompleted() 後段再次把 NonQuery 一律視為 Pending，
                    //造成 Oracle, MySQL implicit-commit DDL 在中斷連線後 Commit/Rollback 按鈕又重新亮起
                    btnQuery.AccessibleDescription = string.Empty;
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith("HideAutoCompleteList`", StringComparison.Ordinal)) //隱藏 AutoComplete 下拉清單
            {
                temp = MyGlobal.GlobalTemp.Replace("HideAutoCompleteList`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    HideAutoCompleteGrid();

                    //將關鍵字移除，以免重複觸發！
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.GlobalTemp == "HideAutoCompleteList`")
                    {
                        MyGlobal.GlobalTemp = string.Empty;
                    }
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith("ReloadSchemaInfo`", StringComparison.Ordinal)) //切換資料庫，重新載入 Schema Info
            {
                temp = MyGlobal.GlobalTemp.Replace("ReloadSchemaInfo`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    using (TraceLogger.Time("Update Schema Data, ReloadSchemaInfo"))
                    {
                        GridHelper.UpdateSchemaData(c1GridSchemaBrowser, true);
                    }

                    using (TraceLogger.Time("Auto Resize Grid Column Width, ReloadSchemaInfo"))
                    {
                        AutoResizeGridColumnWidth();
                    }

                    //將關鍵字移除，以免重複觸發！
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.GlobalTemp == "ReloadSchemaInfo`")
                    {
                        MyGlobal.GlobalTemp = string.Empty;
                    }
                }
            }
            else if (MyGlobal.GlobalTemp5.StartsWith("ReloadSchemaInfo`", StringComparison.Ordinal)) //切換資料庫，重新載入 Schema Info
            {
                temp = MyGlobal.GlobalTemp5.Replace("ReloadSchemaInfo`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    //將關鍵字移除，以免重複觸發！
                    MyGlobal.GlobalTemp5 = MyGlobal.GlobalTemp5.Replace($"{AccessibleDescription};", string.Empty);

                    if (MyGlobal.GlobalTemp5 == "ReloadSchemaInfo`")
                    {
                        MyGlobal.GlobalTemp5 = string.Empty;
                    }

                    using (TraceLogger.Time("Update Schema Data, ReloadSchemaInfo"))
                    {
                        GridHelper.UpdateSchemaData(c1GridSchemaBrowser, true);
                    }

                    using (TraceLogger.Time("Auto Resize Grid Column Width, ReloadSchemaInfo"))
                    {
                        AutoResizeGridColumnWidth();
                    }
                }
            }
            else if (MyGlobal.GlobalTemp6.StartsWith("TabList`", StringComparison.Ordinal)) //20241207 更新 Tab List
            {
                //20241207 先將關鍵字移除，避免所有的處理都集中在非常小的時間段(毫秒級)，導致切換到指定 Cell 時互相干擾)
                //20241208 還是會互相干擾，故取消切換到指定 Cell 的動作
                MyGlobal.GlobalTemp6 = MyGlobal.GlobalTemp6.Replace("TabList`", string.Empty);
                temp = MyGlobal.GlobalTemp6;
                temp = temp.Split('`')[0];

                //確認是否為指定的 Tab
                if (temp != AccessibleDescription)
                {
                    MyGlobal.GlobalTemp6 = $"TabList`{MyGlobal.GlobalTemp6}";
                }
                else
                {
                    //將正要處理的頁籤去除，以免重複觸發！
                    MyGlobal.GlobalTemp6 = MyGlobal.GlobalTemp6.Replace($"{AccessibleDescription}`", string.Empty);

                    c1GridTabList.DataSource = MyGlobal.dtTabList;
                    c1GridTabList.Size = new Size(tabTabList.Width - 2, tabTabList.Height - 2);
                    c1GridTabList.Splits[0].RecordSelectors = false; //20241208 不顯示最左側的指示條
                    c1GridTabList.AllowUpdate = false;

                    foreach (C1DisplayColumn col in c1GridTabList.Splits[0].DisplayColumns)
                    {
                        if (col.Name == "Tab")
                        {
                            col.Width = c1GridTabList.Width - 4;
                        }
                        else if (col.Name == "AccessibleDescription")
                        {
                            col.Visible = false;
                            col.Frozen = true;
                        }
                    }

                    c1GridTabList.Columns["Tab"].Caption = LocalizationHelper.GetLanguageString("Double-click the cell to switch to the tab", "form", GetType().Name, "gridheader", "DoubleClickToSwitchTab", "Text");

                    if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp6))
                    {
                        //如果還有尚未處理的，才要再加上關鍵字
                        MyGlobal.GlobalTemp6 = $"TabList`{MyGlobal.GlobalTemp6}";
                    }
                }
            }

            if (MyGlobal.GlobalTemp.StartsWith("CloseQueryFormAndCheckCommit`", StringComparison.Ordinal)) //檢查是否需要 Commit
            {
                temp = MyGlobal.GlobalTemp.Replace("CloseQueryFormAndCheckCommit`", string.Empty);
                temp = temp.Split(';')[0];

                if (temp == AccessibleDescription) //確認是否為指定的 Tab
                {
                    //把關鍵字串重新命名，以免重複觸發！
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", $"+{AccessibleDescription}+;");

                    if (AppConfigHelper.IsNotCommitYet && MyGlobal.CommitRollbackCheck == -1)
                    {
                        using (var form = new CommitRollbackDialog())
                        {
                            form.Result = string.Empty;
                            form.ShowDialog();

                            //根據傳回來的 Result 決定是否要 Commit/Rollback
                            switch (form.Result)
                            {
                                case "COMMIT":
                                    {
                                        MyGlobal.CommitRollbackCheck = 1;

                                        var result2 = ExecuteAndApplyTransactionAction(DatabaseTransactionActionKind.Commit);

                                        if (!result2.ShouldClearPendingState)
                                        {
                                            CancelCloseAfterTransactionFailure();
                                            return;
                                        }

                                        break;
                                    }
                                case "ROLLBACK":
                                    {
                                        MyGlobal.CommitRollbackCheck = 0;

                                        var result2 = ExecuteAndApplyTransactionAction(DatabaseTransactionActionKind.Rollback);

                                        if (!result2.ShouldClearPendingState)
                                        {
                                            CancelCloseAfterTransactionFailure();
                                            return;
                                        }

                                        break;
                                    }
                                default:
                                    {
                                        //使用者按下取消！
                                        MyGlobal.CommitRollbackCheck = 2; //此處設定為 2(取消)
                                        MyGlobal.GlobalTemp = string.Empty;
                                        return;
                                    }
                            }
                        }
                    }

                    //將關鍵字移除
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"+{AccessibleDescription}+;", string.Empty);

                    if (MyGlobal.GlobalTemp == "CloseQueryFormAndCheckCommit`")
                    {
                        MyGlobal.GlobalTemp = string.Empty;
                    }
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith("CloseQueryForm`", StringComparison.Ordinal)) //由 TabControl 關閉 Query Form (關閉單一 Form)
            {
                try
                {
                    temp = MyGlobal.GlobalTemp.Replace("CloseQueryForm`", string.Empty);
                    temp = temp.Split(';')[0];

                    if (temp != AccessibleDescription)
                    {
                        return;
                    }

                    //處理過程中：先把關鍵字串重新命名，以免重複觸發！
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{AccessibleDescription};", $"+{AccessibleDescription}+;");

                    var isCloseForm = true;

                    //檢查檔案是否需要存檔？
                    if (btnSaveRed.Visible || editor.CanUndo)
                    {
                        temp = (btnSave.Tag ?? Tag)?.ToString() ?? string.Empty;
                        _languageText = LocalizationHelper.GetLanguageString("Save", "form", GetType().Name, "msg", "Save", "Text");
                        temp2 = LocalizationHelper.GetLanguageString("Save file?", "form", GetType().Name, "msg", "SaveFile", "Text");

                        var result0 = MessageBox.Show($"{temp2}\r\n\r\n\"{temp}\"", _languageText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        switch (result0)
                        {
                            case DialogResult.Yes:
                                {
                                    if (!Save()) //在 MainForm 關閉 Tab，詢問是否存檔，使用者選擇「是」要存檔！
                                    {
                                        //存檔出現狀況
                                        return;
                                    }

                                    isCloseForm = true;
                                    break;
                                }
                            case DialogResult.No: //使用者選擇「不要存檔」！
                                {
                                    isCloseForm = true;
                                    break;
                                }
                            default:
                                {
                                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"+{AccessibleDescription}+;", $"{AccessibleDescription}|CANCEL;");
                                    return; //使用者按下取消，告訴 MainForm，此 Tab 不要關閉了！
                                }
                        }

                        //20240306 存檔或不存檔，都要把暫存檔記錄刪除 (下次開啟 JaosnQuery 才不會再次開啟已關閉的檔案)
                        tmrBackup.Enabled = false;
                        MyGlobal.DeleteBackupFileInfo(AccessibleName);
                    }

                    //沒異常或檔案不需要存檔，將關鍵字移除
                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"+{AccessibleDescription}+;", string.Empty);

                    //20231012 不需要中斷連線，否則 Query Editor 有未 Commit/Rollback 會失效！
                    //DisconnectDatabase(); //關閉單一 Query Form

                    if (MyGlobal.GlobalTemp == "CloseQueryForm`")
                    {
                        MyGlobal.GlobalTemp = string.Empty;
                    }

                    if (isCloseForm)
                    {
                        Close(); //20240302 透過 MainForm 關閉 QueryForm，此處必須再執行 Close() 關閉表单，否則 Timer 會一直在啟用狀態 (表單資源未釋放！)
                    }
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }

                return;
            }
            else if (MyGlobal.InfoFromMDIForm.StartsWith("TransferSelectSQL`", StringComparison.Ordinal)) //「不是」第一次開啟 Query Form，要透過全域變數來判斷
            {
                try
                {
                    temp = MyGlobal.InfoFromMDIForm.Replace("TransferSelectSQL`", string.Empty);

                    if (temp.Split(';')[0] == AccessibleDescription) //確認是否為指定的 Tab
                    {
                        temp = temp.Split(';')[1];

                        var i = editor.Text.Length + (string.IsNullOrEmpty(editor.Text) ? 0 : 2);

                        editor.Text = string.IsNullOrEmpty(editor.Text) ? temp : $"{editor.Text}\r\n{temp}";
                        editor.SelectionStart = i;
                        editor.SelectionEnd = i + temp.Length + 2;
                        editor.ScrollCaret();

                        MyGlobal.InfoFromMDIForm = string.Empty;
                        MyGlobal.GlobalTemp = "Rename4SchemaBrowser"; //貼上 SQL 後，要再呼叫主表單變更 Schema Browser's Tab Name (會同時自動切換至 *SQL Editor)
                    }
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }
            else if (MyGlobal.GlobalTemp.StartsWith("RenameTab`", StringComparison.Ordinal)) //20241217 變更頁籤名稱
            {
                temp = MyGlobal.GlobalTemp.Replace("RenameTab`", string.Empty);

                if (temp.Split(new[] { MyGlobal.Separator }, StringSplitOptions.None)[0] == AccessibleDescription) //確認是否為指定的 Tab
                {
                    MyGlobal.GlobalTemp = string.Empty;

                    var tabTitle = temp.Split(new[] { MyGlobal.Separator }, StringSplitOptions.None)[1]; //要變更的頁籤名稱

                    UpdateBackupFileInfo(_backupFileName, editor.CurrentPosition, tabTitle, TextHelper.GetSafeString(btnSave.Tag));

                    MyGlobal.GlobalTemp = "RenameTabUpdateTabList";
                }
            }

            //此處的 AccessibleDescription，指的是 MainForm.cs 裡面的 _queryForm.AccessibleDescription
            //此處的 AccessibleDefaultActionDescription，指的是 MainForm.cs 裡面的 _queryForm.AccessibleDefaultActionDescription
            //第一次開啟 Query Form 時可以判斷 AccessibleDefaultActionDescription，之後的就要透過全域變數才行
            if (!string.IsNullOrEmpty(AccessibleDefaultActionDescription))
            {
                try
                {
                    if (AccessibleDefaultActionDescription.StartsWith("SQL:", StringComparison.Ordinal))
                    {
                        temp = AccessibleDefaultActionDescription.Substring(4, AccessibleDefaultActionDescription.Length - 4);
                        AccessibleDefaultActionDescription = string.Empty;
                        editor.Text = $"{temp}\r\n";
                        editor.SelectionStart = 0;
                        editor.SelectionEnd = temp.Length + 2;

                        #region 20240713 修正 Tab 頁籤及按鈕狀態
                        //傳資訊到母表單，更新 Tab 資訊
                        TransferValueToMainForm($"UpdateTabInfo`{Tag}");

                        btnSave.Tag = null;
                        btnSaveRed.Tag = MyGlobal.DateTimeNow();

                        //更新按鈕狀態
                        CheckEditorContent();
                        #endregion
                    }
                    else if (AccessibleDefaultActionDescription.StartsWith("OPEN", StringComparison.Ordinal))
                    {
                        fileName = AccessibleDefaultActionDescription.Substring(6, AccessibleDefaultActionDescription.Length - 6);

                        temp2 = LocalizationHelper.GetLanguageString("File not found.", "form", GetType().Name, "msg", "FileNotFound", "Text");
                        temp3 = LocalizationHelper.GetLanguageString("Yes: Create it!", "form", GetType().Name, "msg", "YesCreateIt", "Text");

                        var temp5 = LocalizationHelper.GetLanguageString("Cancel: Do nothing!", "form", GetType().Name, "msg", "DoNothing", "Text");
                        var temp4 = string.Empty;
                        var messageText = string.Empty;
                        var messageCaption = string.Empty;

                        if (AccessibleDefaultActionDescription.StartsWith("OPEN1:", StringComparison.Ordinal)) //Recent Files
                        {
                            temp = "1";
                            messageCaption = LocalizationHelper.GetLanguageString("Recent Files", "form", GetType().Name, "msg", "RecentFiles", "Text");
                            temp4 = LocalizationHelper.GetLanguageString("No: Remove the file from Recent Files!", "form", GetType().Name, "msg", "RemoveFromRecentFiles", "Text");
                            messageText = $"{temp2}\r\n\r\n\"{fileName}\"\r\n\r\n{temp3}\r\n{temp4}\r\n{temp5}";
                        }
                        else if (AccessibleDefaultActionDescription.StartsWith("OPEN2:", StringComparison.Ordinal)) //My Favorite
                        {
                            temp = "2";
                            messageCaption = LocalizationHelper.GetLanguageString("My Favorite", "form", GetType().Name, "msg", "MyFavorite", "Text");
                            temp4 = LocalizationHelper.GetLanguageString("No: Remove the file from My Favorite", "form", GetType().Name, "msg", "RemoveFromMyFavorite", "Text");
                            messageText = $"{temp2}\r\n\r\n\"{fileName}\"\r\n\r\n{temp3}\r\n{temp4}\r\n{temp5}";
                        }
                        else //OPEN0: 一般開啟檔案
                        {
                            temp = "0";
                            messageCaption = LocalizationHelper.GetLanguageString("Create new file", "form", GetType().Name, "msg", "CreateNewFile", "Text");
                            temp4 = LocalizationHelper.GetLanguageString("OK: Create it?", "form", GetType().Name, "msg", "OKCreateIt", "Text");
                            messageText = $"{temp2}\r\n\r\n\"{fileName}\"\r\n\r\n{temp4}\r\n{temp5}";
                        }

                        AccessibleDefaultActionDescription = string.Empty;

                        //以下程式碼，是從「Open File」按鈕複製來的！
                        //檢查檔案是否存在，不存在，則詢問是否要建立 (從母表單傳遞過來的 Recent 檔名，有可能不存在/被刪除)
                        if (!File.Exists(fileName))
                        {
                            if (temp == "0")
                            {
                                if (MessageBox.Show(messageText, messageCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                                {
                                    editor.Text = string.Empty;
                                    WriteFile(fileName); //以指定路徑 + 檔名存檔，內容是空白的
                                }
                                else
                                {
                                    //20191029 透過 MainForm 關閉空白的 Tab
                                    TransferValueToMainForm("CloseEmptyTab`");
                                    return;
                                }
                            }
                            else
                            {
                                var dialogResult = MessageBox.Show(messageText, messageCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                                switch (dialogResult)
                                {
                                    case DialogResult.Yes:
                                        {
                                            editor.Text = string.Empty;
                                            WriteFile(fileName); //以指定路徑+檔名存檔，內容是空白的
                                            break;
                                        }
                                    case DialogResult.No:
                                        {
                                            if (temp == "1")
                                            {
                                                //將此檔案從 Recent Files 移除: 透過 MainForm 移除
                                                TransferValueToMainForm($"RemoveFromRecentFiles`{fileName}");
                                            }
                                            else
                                            {
                                                //將此檔案從 My Favorite Lists 移除: 透過 MainForm 移除
                                                TransferValueToMainForm($"RemoveFromMyFavoriteLists`{fileName}");
                                            }

                                            TransferValueToMainForm("CloseEmptyTab`");
                                            return;
                                        }
                                    default:
                                        {
                                            //20191029 透過 MainForm 關閉空白的 Tab
                                            TransferValueToMainForm("CloseEmptyTab`");
                                            return;
                                        }
                                }
                            }
                        }
                        else
                        {
                            MyGlobal.CheckExistTabResult = string.Empty;

                            //傳資訊到母表單，檢查 Tab 資訊，此檔案是否已被開啟了
                            TransferValueToMainForm($"CheckExistTab`{fileName}");

                            while (!string.IsNullOrEmpty(MyGlobal.CheckExistTabResult))
                            {
                                break;
                            }

                            if (MyGlobal.CheckExistTabResult == "TRUE")
                            {
                                MyGlobal.CheckExistTabResult = string.Empty;
                                return;
                            }

                            if (!LoadFile(fileName, out var isLargeFile))
                            {
                                if (!string.IsNullOrEmpty(MyGlobal.CancelOpenAndCloseTab))
                                {
                                    //20191013 關閉 - 要透過 MainForm 才行
                                    TransferValueToMainForm("CloseEmptyTab`");
                                }

                                return;
                            }
                            else
                            {
                                tmrBackup.Enabled = false;
                                MyGlobal.DeleteBackupFileInfo(AccessibleName);
                            }

                            //傳資訊到母表單，更新 Tab 資訊
                            TransferValueToMainForm($"UpdateTabInfo`{fileName}");

                            //傳資訊到母表單，更新 Recent Files 資訊
                            TransferValueToMainForm($"UpdateRecentFiles`{fileName}");

                            btnSave.Tag = fileName;
                            btnSaveRed.Tag = File.GetLastWriteTime(fileName).ToString(DateTimeFormat);
                            //btnSave.Enabled = true;
                            btnSaveAs.Enabled = true;

                            editor.EmptyUndoBuffer();
                            RefreshEditorDirtyState();
                            RefreshEditorCommandButtonsState();
                            RefreshSqlNavigatorPanel();

                            if (isLargeFile)
                            {
                                editor.AppendText("\r\n"); //20190909 加一個換行符號，CanUnDo 就會是 true，(TabControl)檔名前面加上一個星號 (代表檔案未存檔)
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowExceptionMessage(ex);
                }
            }

            //判斷是否有從外部傳進來要開啟的檔案
            if (!string.IsNullOrEmpty(MyGlobal.PendingExternalOpenFileName))
            {
                temp = MyGlobal.PendingExternalOpenFileName;
                MyGlobal.PendingExternalOpenFileName = string.Empty;

                RequestOpenExternalFileInNewTab(temp);
            }

            if (string.IsNullOrEmpty(_queryStatus))
            {
                return;
            }

            //20210529 SQL 指令執行結束, _sQueryStatus == "Complete"
            //可能是沒錯誤正常結束，也可能是有錯誤；都會是 "Complete"

            _endTime = DateTime.Now;
            lblExecTime.Tag = MyGlobal.DateDiff(_startTime, _endTime);
            lblExecTime.Text = $@"{_execTime} {lblExecTime.Tag}";
            lblRows.Text = $"{c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count:N0} {_rows}";
            tmrExecTime.Enabled = false;

            c1TrueDBGrid1.Row = 0;
            c1TrueDBGrid1.Col = 0;
            c1TrueDBGrid1.Select();
            c1TrueDBGrid1.Enabled = true;

            foreach (Control tab in c1DockingTab1.TabPages)
            {
                var tabPage = (C1DockingTabPage)tab;

                foreach (var ctrlTab in tabPage.Controls)
                {
                    if (ctrlTab.GetType().Name != "C1TrueDBGrid")
                    {
                        continue;
                    }

                    if (temp != $"c1TrueDBGrid{_queryIndex}")
                    {
                        continue;
                    }

                    var grid = (C1TrueDBGrid)ctrlTab;

                    GridHelper.SetGridVisualStyle(grid, _fontSize == 0 ? 12 : _fontSize);
                    GridZoom(grid);
                }
            }

            var result = _queryStatus;
            var message = string.Empty;
            var resultTagUpper = TextHelper.GetSafeString(editorMessage.Tag).ToUpper();
            var temp0 = editorMessage.Text.Replace("\r\n", "\r\n--");

            switch (resultTagUpper)
            {
                case "ERROR":
                    {
                        result = "Error";
                        message = editorMessage.Text;

                        if (IsPostgreSql && !DatabaseSqlExecutor.UseAutoRollback)
                        {
                            btnRollback.Enabled = true;
                        }

                        break;
                    }
                //非查詢模式，且沒有發生錯誤
                case "NONQUERY":
                    {
                        message = editorMessage.Text;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //Begin:更新 SQL History
            temp = result == "Error" ? _sqlWhenError : editor.SelectedText;

            if (string.IsNullOrEmpty(temp))
            {
                temp = editor.Text;
            }

            if (!string.IsNullOrEmpty(MyGlobal.ExecuteNonQuerySqlHistoryScript))
            {
                //20201124 不在此處寫入 JasonQuery.db，因為效率太差了 (改在執行當下逐筆寫入)
                MyGlobal.ExecuteNonQuerySqlHistoryScript = string.Empty;
            }
            else
            {
                var newMessage = string.Empty;

                if (result == "Error")
                {
                    newMessage = message;
                }
                else
                {
                    var temp00 = string.IsNullOrEmpty(_queryTextParametersMapping) ? string.Empty : $"\r\n\r\n{_queryTextParametersMapping}";

                    newMessage = string.IsNullOrEmpty(message) ? _queryTextParametersMapping : $"{message}{temp00}";
                }

                UpdateSqlHistory(c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count, result, newMessage, temp);
            }
            //End:更新 SQL History

            _queryStatus = string.Empty;

            //控制按鈕程式碼，要寫在後面
            var value = c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count != 0;

            cboFindGrid.Enabled = value;
            btnExportToFile.Enabled = value;

            var messageTag = TextHelper.GetSafeString(editorMessage.Tag);
            var cancelQueryTag = TextHelper.GetSafeString(btnCancelQuery.Tag);

            if (string.Equals(messageTag, "NONQUERY", StringComparison.OrdinalIgnoreCase) || string.Equals(messageTag, "NONQUERY0ROW", StringComparison.OrdinalIgnoreCase))
            {
                c1TrueDBGrid1.DataSource = _dtNullTable;
                c1DockingTab1.SelectedTab = tabMessage;
            }
            else if (string.Equals(messageTag, "CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                c1DockingTab1.SelectedTab = tabMessage;
            }
            else if (string.Equals(cancelQueryTag, "CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                UpdateMessage("canceling statement due to user request");

                c1TrueDBGrid1.DataSource = _dtNullTable;
                c1DockingTab1.SelectedTab = tabMessage;
            }
            else if (!string.Equals(messageTag, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                //查無任何資料的處理
                if (c1TrueDBGrid1.IsDataTableSourceNullOrEmpty())
                {
                    //查無資料，還是要移到 DataGrid 頁籤
                    c1DockingTab1.SelectedTab = tabDataGrid;

                    GridHelper.ResizeGridColumnWidth(c1TrueDBGrid1);

                    #region 20250526 變更標題列的高度值
                    var pcnt = MyLibrary.GridZoom;

                    if (!chkRawDataMode.Checked && (chkShowColumnType.Checked || (chkShowColumnComments.Checked && _currentQueryResultHasColumnComments)))
                    {
                        int i = 1 + (chkShowColumnType.Checked ? 1 : 0);

                        i += chkShowColumnComments.Checked && _currentQueryResultHasColumnComments ? 1 : 0;
                        c1TrueDBGrid1.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) * i + 11;
                    }
                    else
                    {
                        c1TrueDBGrid1.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 12;
                    }
                    #endregion

                    c1TrueDBGrid1.Refresh();
                }

                GridHelper.SetGridVisualStyle(c1TrueDBGrid1, MyLibrary.GridFontSize);

                #region 20250526 調整標題列垂直置中 (當標題列的內容只有一列時)
                var columns = c1TrueDBGrid1.Columns;

                //取得所有 Caption 中的換行數量 (每個欄位的標題名的 "\r\n" 數量 = 分段數 - 1)
                var lineBreakCounts = columns.Cast<C1DataColumn>()
                                             .Select(col => (col.Caption ?? "").Split(new[] { "\r\n" }, StringSplitOptions.None).Length - 1)
                                             .ToList();

                //檢查是否所有換行符號的數量都一致
                bool isLineBreaksEqual = lineBreakCounts.Distinct().Count() <= 1;

                //取最大值以供後續補足使用
                int maxLineBreaks = lineBreakCounts.DefaultIfEmpty(0).Max();

                //補足每個 Caption 中的換行符號至 iMaxLineBreaks
                if (!isLineBreaksEqual && maxLineBreaks > 1)
                {
                    foreach (C1DataColumn column in columns)
                    {
                        var caption = column.Caption ?? string.Empty;
                        int lineBreaks = caption.Split(new[] { "\r\n" }, StringSplitOptions.None).Length - 1;

                        //補足不足的換行符號
                        for (var i = lineBreaks; i < maxLineBreaks; i++)
                        {
                            caption += "\r\n";
                        }

                        column.Caption = caption;
                    }
                }

                var displayColumns = c1TrueDBGrid1.Splits[_splitsIndex].DisplayColumns;

                foreach (C1DisplayColumn col in displayColumns)
                {
                    //每個欄位的換行符號數量一致，垂直置中！
                    col.HeadingStyle.VerticalAlignment = AlignVertEnum.Center;
                }
                #endregion

                if (AppConfigHelper.IsChangeColorThemeNeedRestart)
                {
                    if (_fontSize == 0)
                    {
                        _fontSize = 12;
                    }

                    c1TrueDBGrid1.Styles["Normal"].Font = new Font(MyLibrary.GridFontName, _fontSize * 1, FontStyle.Regular, GraphicsUnit.Point);
                }

                if (TextHelper.IsNullOrEmptyTag(btnNextPage.Tag))
                {
                    btnNextPage.Tag = "0";
                }

                int.TryParse(TextHelper.GetSafeString(btnNextPage.Tag), out var nextPageTagValue);
                int.TryParse(TextHelper.GetSafeString(btnPaginationOn.Tag), out var paginationOnTagValue);

                btnNextPage.Tag = (nextPageTagValue + paginationOnTagValue).ToString();
            }

            var cancelledNonQueryHandled = TryHandleCancelledNonQueryCompletion(cancelQueryTag);

            if (!cancelledNonQueryHandled && (TextHelper.IsNullOrEmptyTag(editorMessage.Tag)
                || !string.Equals(messageTag, "ERROR", StringComparison.OrdinalIgnoreCase)))
            {
                var result2Upper = btnQuery.AccessibleDescription.ToUpper();

                switch (result2Upper)
                {
                    case "QUERY" when !btnCommit.Enabled && !ShouldPreventAutomaticDisconnectForCompletedQuery():
                        {
                            //單純的查詢、且沒有錯誤、且不需要 Commit / Rollback，全部「中斷連線」
                            TransferValueToMainForm("DisconnectAfterQueryOnly`");
                            break;
                        }
                    case "NONQUERY":
                        {
                            //傳遞資訊至 MainForm，更新每個 QueryForm 的 Commit/Rollbak 按鈕狀態
                            TransferValueToMainForm("UpdateCommitRollbackButton`");
                            break;
                        }
                }
            }
            else if (IsPostgreSql && string.Equals(messageTag, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                //PostgreSQL 執行有錯誤：傳遞資訊至 MainForm，每個 QueryForm 全部中斷連線
                //20220804 PostgreSQL 如果使用者有設定「Auto Rollback on error」，才需要中斷連線！
                if (DatabaseSqlExecutor.UseAutoRollback)
                {
                    TransferValueToMainForm("DisconnectAfterExecuteError`");
                }
            }

            ResetLockingQueryExecutionState();

            _isBusy = false;

            btnQuery.Enabled = true;
            btnExecuteCurrentBlock.Enabled = true;
            btnCancelQuery.Enabled = false;

            if (_isNextPageQuery)
            {
                c1TrueDBGrid1.ScrollGrid(0, c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count);
                _lastRowOffset = c1TrueDBGrid1.Splits[_splitsIndex].VerticalOffset;

                c1TrueDBGrid1.Row = _nextPageRow;
                c1TrueDBGrid1.Col = _nextPageCol;
                c1TrueDBGrid1.Select();

                //記錄目前 scroll bar 的位置
                c1TrueDBGrid1.ScrollGrid(0, _nextPageRow);
                _lastTimeOffset = c1TrueDBGrid1.Splits[_splitsIndex].VerticalOffset;

                //重新再移動一次
                c1TrueDBGrid1.Row = _nextPageRow;
                c1TrueDBGrid1.Col = _nextPageCol;
                c1TrueDBGrid1.Select();

                Application.UseWaitCursor = false;
                c1DockingTab1.Enabled = true;
            }
            else
            {
                c1TrueDBGrid1.ScrollGrid(0, c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count);
                _lastRowOffset = c1TrueDBGrid1.Splits[_splitsIndex].VerticalOffset;
                c1TrueDBGrid1.Row = 0;
                c1TrueDBGrid1.Col = 0;
                c1TrueDBGrid1.Select();
            }

            if (!MyLibrary.GridSetFocusAfterQuery && !_isNextPageQuery)
            {
                editor.Focus();
            }
            else
            {
                c1TrueDBGrid1.Focus(); //20201210
            }

            //20210601 查詢結束後，移動一下滑鼠，讓滑鼠游標不再是 busy
            SetCursorPos(Cursor.Position.X - 1, Cursor.Position.Y - 1);
            SetCursorPos(Cursor.Position.X + 1, Cursor.Position.Y + 1);
        }

        private void RequestFocusEditorAfterLocalization()
        {
            if (!IsHandleCreated || IsDisposed || Disposing)
            {
                return;
            }

            BeginInvoke
            (
                new Action
                (
                    () =>
                    {
                        FocusEditorAfterLocalization();
                    }
                )
            );
        }

        private void FocusEditorAfterLocalization()
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (!editor.CanFocus)
            {
                ApplyEditorFocusedVisualState();
                return;
            }

            ActiveControl = editor;
            editor.Select();
            editor.Focus();

            ApplyEditorFocusedVisualState();
        }

        private void ApplyEditorFocusedVisualState()
        {
            tsEditor.BackColor = _toolstripFocused;
            tsDataGrid.BackColor = _toolstripUnfocused;

            SetGridToolStripBackColor(false);
        }

        private void UpdateSqlHistory(int rows, string result, string message, string sql)
        {
            var execTime = TextHelper.GetSafeString(lblExecTime.Tag);
            var queryTime = TextHelper.GetSafeString(lblQueryTime.Tag);

            //20240723 寫入 SQL 歷史記錄
            JasonQueryRepository.UpdateSqlHistory(JasonQueryRepository.DbMotherPid, MyGlobal.DateTimeNowfff(), execTime, queryTime, rows, result, message, sql, _operationObject);
        }

        private void LoadFindList(string fun, C1ComboBox comboBox)
        {
            var i = 0;

            comboBox.Items.Clear();

            try
            {
                var function = $"FindList_{fun}";
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = '{function}'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtRecent = JasonQueryRepository.ExecQuery(sql);

                if (dtRecent.Rows.Count <= 0)
                {
                    return;
                }

                for (var row = 0; row < dtRecent.Rows.Count; row++)
                {
                    if (i > 20)
                    {
                        break;
                    }

                    comboBox.Items.Add(dtRecent.Rows[row]["AttributeValue"].ToString());

                    i++;
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void SaveFindList(string fun, string findText)
        {
            findText = findText.Replace("'", "''");

            var function = $"FindList_{fun}";
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey = '{function}'");
            sbSql.Append($"   AND AttributeValue = '{findText}'");

            var sql = sbSql.ToString();
            var dtRecent = JasonQueryRepository.ExecQuery(sql);

            if (dtRecent?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = '{function}'");
                sbSql.Append($"   AND AttributeValue = '{findText}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, '{function}', '{findText}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }

            //Reload
            LoadFindList(fun, cboFindGrid);
        }

        private void LoadReplaceList()
        {
            //var i = 0;

            //cboReplaceBox.Items.Clear();

            //try
            //{
            //    var dtReplace = DBCommon.ExecQuery("SELECT AttributeValue FROM SystemConfig WHERE DomainUser = '" + MyGlobal.DomainUser + "' AND MPID = " + DBState.DbMotherPID + " AND AttributeKey = 'ReplaceList_Editor" + "' ORDER BY AttributeDate DESC");

            //    if (dtReplace.Rows.Count <= 0)
            //    {
            //        return;
            //    }

            //    for (var row = 0; row < dtReplace.Rows.Count; row++)
            //    {
            //        if (i > 20)
            //        {
            //            break;
            //        }

            //        cboReplaceBox.Items.Add(dtReplace.Rows[row]["AttributeValue"].ToString());

            //        i++;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ShowExceptionMessage(ex);
            //}
        }

        private void SaveReplaceList(string fun, string replaceText)
        {
            replaceText = replaceText.Replace("'", "''");

            var function = $"ReplaceList_{fun}";
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey = '{function}'");
            sbSql.Append($"   AND AttributeValue = '{replaceText}'");

            var sql = sbSql.ToString();
            var dtRecent = JasonQueryRepository.ExecQuery(sql);

            if (dtRecent?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"   AND AttributeKey = '{function}'");
                sbSql.Append($"   AND AttributeValue = '{replaceText}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeValue, AttributeDate)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, '{function}', '{replaceText}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }

            //Reload
            LoadReplaceList();
        }

        private void splitContainer1_KeyPress(object sender, KeyPressEventArgs e)
        {
            editor.Focus(); //Focus 移回 SQL Editor (Enter 鍵)
        }

        private void splitContainer1_KeyDown(object sender, KeyEventArgs e)
        {
            editor.Focus(); //Focus 移回 SQL Editor (上/下/左/右 鍵)
        }

        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            _isSplitterSaveRequired = true; //使用者手動調整，才要儲存 (Form Size 變動時，並不會觸發 SplitterMoving 事件)
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            try
            {
                HideAutoCompleteGrid();

                if (!_isSplitterSaveRequired)
                {
                    return;
                }

                txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);

                AutoResizeGridColumnWidth();
                AutoResizeGridColumnWidthForSchema();
                ResizeTabListGrid();
                ResizeSqlNavigatorGrid();

                SaveSplitterData("L/R", splitContainer1.SplitterDistance);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                _isSplitterSaveRequired = false;
                editor.Focus();
            }
        }

        private void splitContainer3_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor();
        }

        private void splitContainer3_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            _isSplitterSaveRequired = true; //使用者手動調整，才要儲存 (Form Size 變動時，並不會觸發 SplitterMoving 事件)
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            try
            {
                HideAutoCompleteGrid();

                if (!_isSplitterSaveRequired)
                {
                    return;
                }

                SaveSplitterData("U/D", splitContainer3.SplitterDistance);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                _isSplitterSaveRequired = false;
                editor.Focus();
            }
        }

        private void LoadSplitterData(string splitterKey)
        {
            var attributeName = $"QueryEditor_{splitterKey}";
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
            sbSql.Append($"   AND AttributeName = '{attributeName}'");

            var sql = sbSql.ToString();
            var dtSplitterData = JasonQueryRepository.ExecQuery(sql);

            if (dtSplitterData.Rows.Count <= 0)
            {
                if (splitterKey == "LL/RR")
                {
                    splitContainer4.SplitterDistance = 250; //避免視窗的大小影響到它的寬度
                }

                return;
            }

            int.TryParse(dtSplitterData.Rows[0].GetSafeString("AttributeValue"), out var splitterDistance);

            switch (splitterKey)
            {
                case "L/R":
                    {
                        splitContainer1.SplitterDistance = splitterDistance;
                        break;
                    }
                case "LL/RR":
                    {
                        splitContainer4.SplitterDistance = splitterDistance;
                        break;
                    }
                case "U/D":
                    {
                        splitContainer3.SplitterDistance = splitterDistance;
                        break;
                    }
            }
        }

        private void SaveSplitterData(string splitterKey, int splitterValue)
        {
            if (splitterKey != "U/D" && splitterKey != "L/R" && splitterKey != "LL/RR")
            {
                return;
            }

            var attributeName = $"QueryEditor_{splitterKey}";
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
            sbSql.Append($"   AND AttributeName = '{attributeName}'");

            var sql = sbSql.ToString();
            var dtData = JasonQueryRepository.ExecQuery(sql);

            if (dtData?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeValue = '{splitterValue}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
                sbSql.Append($"   AND AttributeName = '{attributeName}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'SplitterConfig', '{attributeName}', '{splitterValue}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
        }

        private void btnIndent_Click(object sender, EventArgs e)
        {
            Indent();
        }

        private void btnUnIndent_Click(object sender, EventArgs e)
        {
            Unindent();
        }

        private void Indent() //增加縮排
        {
            try
            {
                _editorIndentService.IndentSelection(GetIndentSize());
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void Unindent() //減少縮排
        {
            try
            {
                _editorIndentService.UnindentSelection(GetIndentSize());
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnSelectCurrentBlock_Click(object sender, EventArgs e)
        {
            SelectCurrentBlock();
        }

        private string SelectCurrentBlock(bool select = true)
        {
            try
            {
                return _editorBlockSelectionService.SelectCurrentBlock(select);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return string.Empty;
            }
        }

        private string SelectActiveLine()
        {
            try
            {
                return _editorLineSelectionService.SelectActiveLine();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return string.Empty;
            }
        }

        private void GetNewStartAndEnd_EntireBlankRowAsEmptyRow(int start, int end, ref int start2, ref int end2)
        {
            var lineStart = 0;
            var lineEnd = 0;
            var position = 0;
            var line = string.Empty;
            var text = editor.Text;
            var parts = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var currentLine = editor.CurrentLine;

            for (var i = currentLine; i > 0; i--)
            {
                line = parts[i].Trim();

                if (!string.IsNullOrEmpty(line) || i == currentLine)
                {
                    continue; //如果整列都是空白
                }

                lineStart = i + 1;
                break;
            }

            if (currentLine == parts.Length - 1 || string.IsNullOrEmpty(parts[currentLine]) && currentLine + 1 <= parts.Length - 1 && string.IsNullOrEmpty(parts[currentLine + 1]))
            {
                //【CurrentLine 位於最後一列】，【或是 CurrentLine 為空白列，且 CurrentLine 下一列也是空白列】
                lineEnd = currentLine;
            }
            else
            {
                for (var i = currentLine; i < parts.Length; i++)
                {
                    line = parts[i].Trim();

                    if (string.IsNullOrEmpty(line) && i != currentLine) //如果整列都是空白
                    {
                        lineEnd = i - 1;
                        break;
                    }

                    if (string.IsNullOrEmpty(line) && i == currentLine)
                    {
                        lineEnd = i;
                        break;
                    }

                    lineEnd = i;
                }

                if (lineStart == 0 && lineEnd == 0)
                {
                    lineEnd = 0;
                }
                else if (lineStart >= 0 && lineEnd == 0)
                {
                    lineEnd = parts.Length - 1;
                }
            }

            for (var i = 0; i < parts.Length; i++)
            {
                position += parts[i].Length + 2;

                if (i == lineStart - 1)
                {
                    start2 = position;
                }
                else if (i == lineEnd)
                {
                    end2 = position - 2;
                }
            }
        }

        private void editor_DoubleClick(object sender, ScintillaNET.DoubleClickEventArgs e)
        {
            HighlightSelection(true);
        }

        private void editor_Enter(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            ApplyEditorFocusedVisualState();

            _findAndReplace.Scintilla = (ScintillaEditor)sender;
            MyGlobal.GlobalTemp5 = "CanPasteY";
        }

        private void editor_Leave(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;

            if (c1GridAutoCompleteForPeriod.Visible)
            {
                //焦點離開 Editor，不能直接將 c1GridAutoCompleteForPeriod 隱藏！
                _periodAutoCompleteSession.CaretPosition = editor.CurrentPosition;
            }
            else if (c1GridAutoCompleteForSpace.Visible)
            {
                //焦點離開 Editor，不能直接將 c1GridAutoCompleteForSpace 隱藏！
                _spaceAutoCompleteSession.CaretPosition = editor.CurrentPosition;
            }
            else if (c1GridAutoCompleteForAll.Visible)
            {
                //焦點離開 Editor，不能直接將 c1GridAutoCompleteForAll 隱藏！
                _autoCompleteForAllCaretPosition = editor.CurrentPosition;
            }
        }

        private void editor_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    ClearHighlightSelectionCopyState();
                }

                if (TryHandleEditorLeftMouseDown(e))
                {
                    return;
                }

                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                var context = ResolveEditorRightClickContext();

                ApplyEditorContextMenuState(context);
                ApplyEditorContextMenuDarkModeStyle();
                ShowEditorContextMenu(e);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void editor_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor();
        }

        private void editor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (c1GridAutoCompleteForPeriod.Visible)
                {
                    HidePeriodAutoCompletePopup();
                }
                else if (c1GridAutoCompleteForSpace.Visible)
                {
                    HideSpaceAutoCompletePopup();
                }
                else if (c1GridAutoCompleteForAll.Visible)
                {
                    QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
                }
            }
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            CheckEditorContent();
        }

        private void editor_UpdateUI(object sender, ScintillaNET.UpdateUIEventArgs e)
        {
            UpdateEditorStatusBarLanguage();
            RefreshEditorCommandButtonsState();

            if (e.Change == ScintillaNET.UpdateChange.VScroll || e.Change == ScintillaNET.UpdateChange.HScroll)
            {
                HideAutoCompleteGrid();
            }
        }

        private void editor_ZoomChanged(object sender, EventArgs e)
        {
            //20191016 內容空白時，不用往下執行；否則會被視為「檔案內容有異動」
            if (string.IsNullOrEmpty(editor.Text))
            {
                return;
            }

            try
            {
                var canUndo = editor.CanUndo; //記住一開始是否可以 CanUndo
                var start = editor.SelectionStart; //當放大縮小後，即時調整 line number 的寬度，避免因為放大時，line number 最左側的數字會看不見

                editor.Text = $"{editor.Text}\r\n";
                editor.Text = editor.Text.Substring(0, editor.Text.Length - 2);
                editor.SelectionStart = start;
                editor.ScrollCaret();

                if (canUndo)
                {
                    return;
                }

                editor.EmptyUndoBuffer(); //20191016 如果原本就沒異動過內容，此處要還原狀態
                RefreshEditorDirtyState();
                RefreshEditorCommandButtonsState();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void UpdateEditorStatusBarLanguage()
        {
            var sel = 0;
            var lines = editor.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (editor.SelectionStart != editor.SelectionEnd)
            {
                sel = editor.SelectionEnd - editor.SelectionStart;
            }

            lblEditorLines.Text = $"{_editorLines} {DataSizeFormatter.FormatInt(lines.Length)}";
            lblEditorLength.Text = $"{_editorLength} {DataSizeFormatter.FormatInt(editor.TextLength)}";
            lblEditorLn.Text = $"{_editorLn} {DataSizeFormatter.FormatInt(editor.CurrentLine + 1)}";
            lblEditorCol.Text = $"{_editorCol} {DataSizeFormatter.FormatInt(editor.GetColumn(editor.CurrentPosition) + 1)}";
            lblEditorPos.Text = $"{_editorPos} {DataSizeFormatter.FormatInt(editor.CurrentPosition + 1)}";
            lblEditorSel.Text = $"{_editorSel} {DataSizeFormatter.FormatInt(sel)}";
        }

        private void CreateNullTable()
        {
            _languageText = "data";
            _dtNullTable.Columns.Add(_languageText);
        }

        private void ApplySqlStyler()
        {
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

            editor.Styler = new SqlStyler();

            var tempQueryEditorFontName = MyLibrary.QueryEditorFontName;

            //20260607 editorMessage 會顯示 ^^^ 的錯誤定位，字體固定為 Consolas，避免對齊失效！
            if (tempQueryEditorFontName.Equals("Consolas", StringComparison.OrdinalIgnoreCase))
            {
                editorMessage.Styler = new SqlStyler();
            }
            else
            {
                MyLibrary.QueryEditorFontName = "Consolas";
                editorMessage.Styler = new SqlStyler();
                MyLibrary.QueryEditorFontName = tempQueryEditorFontName;
            }
        }

        private void btnExecuteCurrentBlock_Click(object sender, EventArgs e)
        {
            SelectCurrentBlock();

            var selectedText = editor.SelectedText.Trim();

            if (selectedText.Length > 0) //20250903 有選取文字才要執行
            {
                btnQuery.PerformClick();
            }
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            btnWordWrap.Visible = editor.WrapMode == ScintillaNET.WrapMode.Word;
            btnWordWrap2.Visible = !btnWordWrap.Visible;
            editor.WrapMode = editor.WrapMode == ScintillaNET.WrapMode.Word ? ScintillaNET.WrapMode.None : ScintillaNET.WrapMode.Word;
            editor.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editor.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            btnShowAllCharacters.Visible = editor.ViewEol;
            btnShowAllCharacters2.Visible = !btnShowAllCharacters.Visible;
            editor.ViewEol = !editor.ViewEol;
            editor.ViewWhitespace = (btnShowAllCharacters.Visible) ? ScintillaNET.WhitespaceMode.Invisible : ScintillaNET.WhitespaceMode.VisibleAlways;
        }

        private void btnShowIndentGuide_Click(object sender, EventArgs e)
        {
            btnShowIndentGuide.Visible = editor.IndentationGuides == ScintillaNET.IndentView.LookBoth;
            btnShowIndentGuide2.Visible = !btnShowIndentGuide.Visible;
            editor.IndentationGuides = editor.IndentationGuides == ScintillaNET.IndentView.LookBoth ? ScintillaNET.IndentView.None : ScintillaNET.IndentView.LookBoth;
        }

        //editor 允許從外部拖曳檔案，並直接開啟它
        //20190930 如果拖曳的檔案已被 JasonQuery 開啟了，會自動切換至該 Tab！
        private void InitDragDropFile()
        {
            editor.AllowDrop = true;

            editor.DragEnter += delegate (object sender, DragEventArgs e)
            {
                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            };

            editor.DragDrop += delegate (object sender, DragEventArgs e)
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    return;
                }

                var a = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (a == null)
                {
                    return;
                }

                for (var i = 0; i < a.Length; i++)
                {
                    RequestOpenFileInNewTab(a.GetValue(i).ToString());

                    var startTime = DateTime.Now;

                    while (true)
                    {
                        if (DateTime.Now.Subtract(startTime).Milliseconds >= 150)
                        {
                            break;
                        }
                    }
                }
            };
        }

        private void btnCode2Sql_Click(object sender, EventArgs e)
        {
            var enabled = !string.IsNullOrWhiteSpace(editor.SelectedText);

            mnuCSharp2Sql.Enabled = enabled;
            mnuVB2Sql.Enabled = enabled;
            mnuDelphi2Sql.Enabled = enabled;
        }

        private void mnuCSharp2Sql_Click(object sender, EventArgs e)
        {
            if (editor.CanPaste)
            {
                CodeToSql("C#", "\"", "//");
            }
        }

        private void mnuVB2Sql_Click(object sender, EventArgs e)
        {
            if (editor.CanPaste)
            {
                CodeToSql("VB", "\"", "'");
            }
        }

        private void mnuDelphi2Sql_Click(object sender, EventArgs e)
        {
            if (editor.CanPaste)
            {
                CodeToSql("Delphi", "'", "//");
            }
        }

        private void CodeToSql(string mode, string key, string commentKeyword)
        {
            var result = string.Empty;

            //來源：選取的字串 or 剪貼簿內容
            var originalText = editor.SelectedText;
            var parts = originalText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var position = 0; position < parts.Length; position++)
            {
                var isCrlf = true;
                var line = parts[position];
                var lineTrim = line.Trim();
                int from;

                if (lineTrim.Length >= 2 && lineTrim.Substring(0, 2) == commentKeyword)
                {
                    line = $"--{lineTrim.Substring(2)}";
                }
                else
                {
                    int commentPosition;
                    var comment = string.Empty;

                    //20241224 如果只有一個指定符號：判斷是否有 = 符號，若沒有，最前面加上指定符號
                    if ((mode == "C#" || mode == "VB") && (line.Length - line.Replace(key, string.Empty).Length) == 1)
                    {
                        line = $"{key}{lineTrim}";
                    }

                    if (line.Length - line.Replace(key, string.Empty).Length >= 2) //至少有兩個指定符號：取中間的字串
                    {
                        from = line.IndexOf(key, StringComparison.Ordinal) + key.Length;

                        var to = line.LastIndexOf(key, StringComparison.Ordinal);

                        commentPosition = line.LastIndexOf(commentKeyword, StringComparison.Ordinal);
                        comment = string.Empty;

                        if (commentPosition > 0 && to < commentPosition) //此列有註解
                        {
                            var temp2 = line.Substring(commentPosition + commentKeyword.Length);

                            comment = $" --{temp2}";
                        }

                        var temp = line.Substring(from, to - from);

                        line = $"{temp}{comment}";
                    }
                    else if ((line.Length - line.Replace(key, string.Empty).Length) == 1) //只有一個指定符號：取第一個指定符號後面的字串
                    {
                        from = line.IndexOf(key, StringComparison.Ordinal) + key.Length;
                        commentPosition = line.LastIndexOf(commentKeyword, StringComparison.Ordinal);
                        comment = string.Empty;

                        if (commentPosition > 0 && from < commentPosition) //此列有註解
                        {
                            var temp2 = line.Substring(commentPosition + commentKeyword.Length);

                            comment = $" --{temp2}";
                        }

                        var temp = line.Substring(from, line.Length - from);

                        line = $"{temp}{comment}";
                    }
                }

                if (!string.IsNullOrEmpty(line) && mode == "Delphi" && key == "'")
                {
                    line = line.Replace("''", "'");
                }

                if (!string.IsNullOrEmpty(line) && "`C#`Delphi`".Contains("`" + mode + "`") && lineTrim.Length > commentKeyword.Length && lineTrim.Substring(0, commentKeyword.Length) == commentKeyword)
                {
                    from = line.IndexOf("//", StringComparison.Ordinal);

                    var temp1 = line.Substring(0, from);
                    var temp2 = line.Substring(from + commentKeyword.Length);

                    line = $"{temp1}--{temp2}";
                }

                if (!string.IsNullOrEmpty(line) && mode == "VB" && lineTrim.Length > commentKeyword.Length && lineTrim.Substring(0, commentKeyword.Length) == commentKeyword)
                {
                    //20250218 VB此處不處理！因為它可能是查詢條件值或是 CASE 的值，例如 'xxx'，並不是註解
                }

                if (mode == "C#" && (lineTrim == "{" || lineTrim == "}" || lineTrim.StartsWith("if", StringComparison.Ordinal) || lineTrim.StartsWith("else", StringComparison.Ordinal)))
                {
                    isCrlf = false;
                    line = string.Empty;
                }

                line = line.TrimEnd();
                result += line;

                if (position < parts.Length - 1 && isCrlf) //不是選取區的最後一列，後面加上換行符號
                {
                    result += "\r\n";
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace("\\r\\n", string.Empty);

                switch (mode)
                {
                    case "C#":
                        {
                            result = result.Replace("\\\"", "\"");
                            break;
                        }
                    case "Delphi":
                        {
                            result = result.Replace("'#$D#$A'", "\r\n");
                            break;
                        }
                }
            }

            //判斷使用者的來源是「選取的字串」or「剪貼簿內容」
            originalText = Clipboard.GetText();
            TextHelper.CopyTextToClipboard(result, "ApplyEditorMenu(09)");
            editor.Paste();
            TextHelper.CopyTextToClipboard(originalText, "ApplyEditorMenu(10)");
        }

        private void btnSql2Code_Click(object sender, EventArgs e)
        {
            var bValue = !string.IsNullOrWhiteSpace(editor.SelectedText);

            mnuSql2CSharp.Enabled = bValue;
            mnuSql2VBNet.Enabled = bValue;
            mnuSql2VB6A.Enabled = bValue;
            mnuSql2Delphi.Enabled = bValue;
        }

        private void mnuCSharpStyle1_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_CSharpStyle1(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("C#", GetType().Name);
        }

        private void mnuCSharpStyle2_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_CSharpStyle2(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("C#", GetType().Name);
        }

        private void mnuCSharpStyle3_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_CSharpStyle3(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("C#", GetType().Name);
        }

        private void mnuCSharpStyle4_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_CSharpStyle4(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName, MyLibrary.SqlToCodeStringBuilderVariableName);
            TextHelper.ShowSqlToCodeMessage("C#", GetType().Name, $", \"{MyLibrary.SqlToCodeStringBuilderVariableName}\"");
        }

        private void mnuVBNetStyle1_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_VBNet1(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("VB.Net", GetType().Name);
        }

        private void mnuVBNetStyle2_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_VBNet2(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("VB.Net", GetType().Name);
        }

        private void mnuVBNetStyle3_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_VBNet3(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("VB.Net", GetType().Name);
        }

        private void mnuVB6AStyle1_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_VB6VBA1(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("VB6/VBA", GetType().Name);
        }

        private void mnuVB6AStyle2_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_VB6VBA2(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("VB6/VBA", GetType().Name);
        }

        private void mnuDelphi6Style1_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_Delphi61(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("Delphi6", GetType().Name);
        }

        private void mnuDelphi6Style2_Click(object sender, EventArgs e)
        {
            TextHelper.TransferSqlToCode_Delphi62(editor.SelectedText, MyLibrary.SqlToCodeSqlVariableName);
            TextHelper.ShowSqlToCodeMessage("Delphi6", GetType().Name);
        }

        private void btnHighlightSelection_Click(object sender, EventArgs e)
        {
            //
        }

        private void ApplyQueryResultGridVisualStyle(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            // 只套用目前這個查詢結果 Grid 的 VisualStyle
            GridHelper.SetGridVisualStyle(grid);

            ApplyQueryResultGridCaptionHeight(grid);
        }

        private void ApplyQueryResultGridCaptionHeight(C1TrueDBGrid grid)
        {
            if (grid == null || grid.Splits.Count <= _splitsIndex)
            {
                return;
            }

            var height = 25;

            if (ShouldShowColumnTypeInHeader())
            {
                height += 20;
            }

            if (ShouldShowColumnCommentInHeader())
            {
                height += 20;
            }

            grid.Splits[_splitsIndex].ColumnCaptionHeight = height;
        }

        private void ApplyFixedGridVisualStyles()
        {
            ApplyGridVisualStyle(c1GridAutoReplaceInfo);
            ApplyGridVisualStyle(c1GridSchemaBrowser);
            ApplyGridVisualStyle(c1GridTabList);
            ApplyGridVisualStyle(c1GridSqlNavigator);
        }

        private static void ApplyGridVisualStyle(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            GridHelper.SetGridVisualStyle(grid);
        }

        private GridHeadingCellTipHelper.GridHeadingCellTipOptions CreateGridHeadingCellTipOptions()
        {
            return new GridHeadingCellTipHelper.GridHeadingCellTipOptions
            {
                ShowPrimaryKey = true,
                ShowNotNull = true,
                ShowDataType = !chkShowColumnType.Checked,
                ShowComment = !chkRawDataMode.Checked && !chkShowColumnComments.Checked
            };
        }

        private void ApplyResultGridHeadingStyle(C1TrueDBGrid c1Grid, bool isSetTop = false)
        {
            GridHelper.ApplyGridHeadingStyle(_columnInfoCollector, c1Grid, isSetTop, CreateGridHeadingCellTipOptions());
        }

        private void GridZoom(C1TrueDBGrid c1Grid)
        {
            var pcnt = MyLibrary.GridZoom;

            if (_fontSize == 0)
            {
                _fontSize = 12;
            }

            c1Grid.RowHeight = (int)(_rowHeight * pcnt) + 5;

            var headerLineCount = GetQueryResultHeaderLineCount(); //20260612 重構

            if (headerLineCount > 1)
            {
                c1Grid.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) * headerLineCount + 11;
            }
            else
            {
                c1Grid.Splits[_splitsIndex].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 12;
            }

            c1Grid.RecordSelectorWidth = (int)(_recSelWidth * pcnt);
            c1Grid.Styles["Normal"].Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, _fontSize * pcnt, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void cboFindGrid_BeforeDropDownOpen(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadFindList("Grid", cboFindGrid);
        }

        private void cboFindGrid_Enter(object sender, EventArgs e)
        {
            SetGridToolStripBackColor(true);
        }

        private void cboFindGrid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 || string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                return;
            }

            UpdateFindGridList();

            cboFindGrid.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            btnFindNextGrid.PerformClick();
        }

        private void cboFindGrid_KeyUp(object sender, KeyEventArgs e)
        {
            var enabled = !string.IsNullOrWhiteSpace(cboFindGrid.Text);

            btnFindNextGrid.Enabled = enabled;
            btnFindPreviousGrid.Enabled = enabled;
            btnCountGrid.Enabled = enabled;
            btnHighlightAllGrid.Enabled = enabled;
            btnClearHighlightsGrid.Enabled = enabled;
        }

        private void cboFindGrid_Leave(object sender, EventArgs e)
        {
            SetGridToolStripBackColor(false);
        }

        private void cboFindGrid_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                return;
            }

            btnFindNextGrid.Enabled = true;
            btnFindPreviousGrid.Enabled = true;
            btnCountGrid.Enabled = true;
            btnHighlightAllGrid.Enabled = true;
            btnClearHighlightsGrid.Enabled = true;

            cboFindGrid.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
        }

        private void cboFindGrid_TextChanged(object sender, EventArgs e)
        {
            var enabled = false;

            if (!string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                enabled = true;
                cboFindGrid.Tag = string.Empty; //20201209 每輸入一個字元就統計一次，如果SQL內容很多，就會嚴重 lag。改成每次輸入都清空，按下「Next」/「Previous」/「Highlight」按鈕時再去統計
            }

            btnFindNextGrid.Enabled = enabled;
            btnFindPreviousGrid.Enabled = enabled;
            btnCountGrid.Enabled = enabled;
            btnHighlightAllGrid.Enabled = enabled;
            btnClearHighlightsGrid.Enabled = enabled;
        }

        private int CountGrid(bool showAlertOnError = false)
        {
            var c1Grid = GetWhichGrid();
            var count = GridHelper.CountGridOccurrence(c1Grid, cboFindGrid.Text, _splitsIndex);
            
            if (!showAlertOnError)
            {
                return count;
            }

            var temp1 = LocalizationHelper.GetLanguageString("Find What:", "form", GetType().Name, "msg", "FindWhat", "Text");
            var temp2 = LocalizationHelper.GetLanguageString("Count:", "form", GetType().Name, "msg", "Count", "Text");
            var temp3 = LocalizationHelper.GetLanguageString("matches.", "form", GetType().Name, "msg", "matches", "Text");

            MessageBoxHelper.ShowNearCursor($"{temp1} {cboFindGrid.Text}\r\n\r\n{temp2} {count} {temp3}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return count;
        }

        private void btnFindNextGrid_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                cboFindGrid.Text = string.Empty;
                return;
            }

            if (TextHelper.IsNullOrEmptyTag(cboFindGrid.Tag))
            {
                cboFindGrid.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");

                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFindGrid.Text}\"", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindGridList();

            if (!FindNextGrid())
            {
                return;
            }

            //往下找不到資料，而且不是在第一格位置，就要從頭再找一次
            if (c1TrueDBGrid1.Row != 0 && c1TrueDBGrid1.Col != 0)
            {
                FindNextGrid(true);
            }
        }

        private bool FindNextGrid(bool isFindAgain = false)
        {
            var searchText = cboFindGrid.Text;
            var findRow = 0;
            var findCol = 0;
            var isFound = false;
            bool result;
            var c1Grid = GetWhichGrid();
            var currentRowStart = c1Grid.Row;
            var currentColStart = c1Grid.Col;

            if (isFindAgain)
            {
                currentRowStart = 0;
                currentColStart = 0;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return false;
            }

            if (cboFindGrid.Items.Count > 0 && cboFindGrid.Text == cboFindGrid.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFindGrid.Text);
            }

            var rowCount = c1Grid.Splits[_splitsIndex].Rows.Count;

            for (var row = currentRowStart; row < rowCount; row++)
            {
                var vr = c1Grid.Splits[_splitsIndex].Rows[row];
                var col = 0;

                foreach (C1DataColumn column in c1Grid.Columns)
                {
                    var cellText = column.CellText(vr.DataRowIndex);

                    if (!string.IsNullOrEmpty(cellText) && cellText.IndexOf(cboFindGrid.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (row == currentRowStart) //游標所在列，尋找下一個，要略過此格！
                        {
                            if (col > currentColStart)
                            {
                                findRow = row;
                                findCol = col;
                                isFound = true;
                                break;
                            }
                        }
                        else
                        {
                            findRow = row;
                            findCol = col;
                            isFound = true;
                            break;
                        }
                    }

                    col++;
                }

                if (isFound)
                {
                    break;
                }
            }

            if (isFound)
            {
                c1Grid.Row = findRow;
                c1Grid.Col = findCol;
                c1Grid.Select(); //Focus 切換到指定的 Cell
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void btnFindPreviousGrid_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                cboFindGrid.Text = string.Empty;
                return;
            }

            if (TextHelper.IsNullOrEmptyTag(cboFindGrid.Tag))
            {
                cboFindGrid.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");
                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFindGrid.Text}\"", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindGridList();

            if (FindPreviousGrid())
            {
                //往上找不到資料，就要從底部再找一次
                FindPreviousGrid(true);
            }
        }

        private bool FindPreviousGrid(bool isFindAgain = false)
        {
            var searchText = cboFindGrid.Text;
            var findRow = 0;
            var findCol = 0;
            var isFound = false;
            bool result;
            var c1Grid = GetWhichGrid();
            var currentRowStart = c1Grid.Row;
            var currentColStart = c1Grid.Col;

            if (isFindAgain)
            {
                currentRowStart = c1Grid.Splits[_splitsIndex].Rows.Count - 1;
                currentColStart = c1Grid.Splits[_splitsIndex].DisplayColumns.Count - 1;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return false;
            }

            if (cboFindGrid.Items.Count > 0 && cboFindGrid.Text == cboFindGrid.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFindGrid.Text);
            }

            for (var row = currentRowStart; row >= 0; row--)
            {
                var columnCount = c1Grid.Splits[_splitsIndex].DisplayColumns.Count - 1;

                for (var i = columnCount; i >= 0; i--)
                {
                    var value = c1Grid.Columns[i].CellValue(row).ToString();

                    if (value.IndexOf(cboFindGrid.Text, StringComparison.OrdinalIgnoreCase) < 0)
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
                c1Grid.Row = findRow;
                c1Grid.Col = findCol;
                c1Grid.Select(); //Focus 切換到指定的 Cell
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void btnClearHighlightsGrid_Click(object sender, EventArgs e)
        {
            var c1Grid = GetWhichGrid();

            foreach (C1DisplayColumn cd in c1Grid.Splits[_splitsIndex].DisplayColumns)
            {
                cd.OwnerDraw = false;
            }

            btnHighlightAllGrid.Tag = "0";
            _searchList.Clear();
            c1Grid.ClearCellStyle(CellStyleFlag.AllCells);
        }

        private void btnCountGrid_Click(object sender, EventArgs e)
        {
            CountGrid(true);
        }

        private void btnHighlightAllGrid_Click(object sender, EventArgs e)
        {
            var c1Grid = GetWhichGrid();

            btnClearHighlightsGrid.PerformClick();

            if (TextHelper.IsNullOrEmptyTag(cboFindGrid.Tag))
            {
                cboFindGrid.Tag = CountGrid().ToString(); //統計出現次數，但不顯示
            }

            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Can't find the text", "form", GetType().Name, "msg", "CantFindText", "Text");
                MessageBoxHelper.ShowNearCursor($"{_languageText} \"{cboFindGrid.Text}\"", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            UpdateFindGridList();

            _searchList.Clear();

            DataTable dt = null;

            //判斷是否有篩選
            if (c1Grid.FocusedSplit.Rows[c1Grid.Row].RowType == RowTypeEnum.DataRow)
            {
                var dr = (DataRowView)c1Grid[c1Grid.RowBookmark(c1Grid.Row)];

                //取得篩選後的數據
                dt = dr.DataView[0].Row.Table.DefaultView.ToTable();
            }
            else
            {
                dt = c1Grid.GetDataTableSourceOrNull();
            }

            var findText = cboFindGrid.Text;
            int rowCount = dt.Rows.Count;
            int start = 0;
            int end = dt.Columns.Count;

            for (int row = 0; row < rowCount; row++)
            {
                var dr = dt.Rows[row];

                for (int col = start; col < end; col++)
                {
                    var value = dr.GetSafeString(col);

                    if (value.IndexOf(findText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _searchList.Add(new Point(col, row));
                    }
                }
            }

            if (_searchList.Count == 0)
            {
                return;
            }

            btnHighlightAllGrid.Tag = "1";

            foreach (C1DisplayColumn cd in c1Grid.Splits[_splitsIndex].DisplayColumns)
            {
                cd.OwnerDraw = true;
            }
        }

        private void c1TrueDBGrid1_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
        {
            var mc = new Point(e.Col, e.Row);

            if (!_searchList.Contains(mc))
            {
                return;
            }

            e.Style.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightForeColor);
            e.Style.BackColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightBackColor);
        }

        private void c1TrueDBGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            var c1Grid = GetWhichGrid();
            string[] parts;
            string[] splitters = { "\r\n", "\r", "\n" };

            c1Grid.ContextMenuStrip = null;

            //取得滑鼠所在列的儲存格行列位置
            var row = c1Grid.RowContaining(e.Y);
            var col = c1Grid.ColContaining(e.X);
            var isCornerSelected = false;

            if (row == -1 && col == -1)
            {
                isCornerSelected = !chkShowFilterRow.Checked || e.Y <= c1Grid.Splits[_splitsIndex].ColumnCaptionHeight;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        if (col != -1)
                        {
                            _shiftMouseLeftDownX = col;
                        }

                        if (row != -1)
                        {
                            _shiftMouseLeftDownY = row;
                        }

                        break;
                    }
                case MouseButtons.Right:
                    {
                        if (isCornerSelected) //按下 Grid's 左上角
                        {
                            return;
                        }

                        //20230912 先判斷「iCol == -1」：使用者點到最左側，略過！否則會因為無法取得 Column Name 而觸發例外錯誤
                        if (col == -1)
                        {
                            //20220124 使用者點到最左側，略過！
                            //20220319 使用者點 DataGrid 的灰色地帶，略過！
                            return;
                        }

                        if (row == -1)
                        {
                            var columnName2 = c1Grid.Splits[_splitsIndex].DisplayColumns[col].ToString(); //20210905 c1Grid.Col => iCol, 修正右鍵顯示的欄位不正確問題

                            parts = columnName2.Split(splitters, 2, StringSplitOptions.None);
                            columnName2 = parts[0];

                            _gridHeaderContextMenu.Items[0].Text = columnName2;
                            _gridHeaderContextMenu.Items[0].Tag = col.ToString();
                            _gridHeaderContextMenu.Items[0].Enabled = false;

                            if (c1Grid.SelectedCols.Count <= 1) //直接在 Column Name 上按右鍵，會是等於0
                            {
                                _gridHeaderContextMenu.Items[2].Tag = columnName2;
                            }
                            else
                            {
                                var sbColumnName = new StringBuilder();

                                for (var i = 0; i < c1Grid.SelectedCols.Count; i++)
                                {
                                    var caption = c1Grid.SelectedCols[i].DataField;

                                    sbColumnName.Append($"{caption}, ");
                                }

                                sbColumnName.Length--;
                                sbColumnName.Length--;

                                _gridHeaderContextMenu.Items[2].Tag = sbColumnName.ToString();
                            }

                            var enabled2 = c1Grid.HasDataTableRows();

                            _gridHeaderContextMenu.Items[4].Enabled = enabled2;
                            _gridHeaderContextMenu.Items[5].Enabled = enabled2;

                            c1Grid.ContextMenuStrip = _gridHeaderContextMenu;

                            if (MyLibrary.IsDarkMode)
                            {
                                _gridHeaderContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                                _gridHeaderContextMenu.ForeColor = Color.White;
                                _gridHeaderContextMenu.RenderMode = ToolStripRenderMode.System;
                            }

                            _gridHeaderContextMenu.Show(c1Grid, new Point(e.X, e.Y));

                            return;
                        }

                        GetGridSelectedXY(col, row);

                        var columnName = c1Grid.Splits[_splitsIndex].DisplayColumns[col].ToString(); //20210905 c1Grid.Col => iCol, 修正右鍵顯示的欄位不正確問題

                        parts = columnName.Split(splitters, 2, StringSplitOptions.None);
                        columnName = parts[0];

                        //20250328 改寫 bValue 取值方式
                        bool isDataColumn = string.Equals(columnName, "DATA", StringComparison.OrdinalIgnoreCase);
                        bool isSingleColumn = c1Grid.Columns.Count == 1;
                        bool hasNoRows = c1Grid.Splits[_splitsIndex].Rows.Count == 0;
                        bool hasMultipleColumns = c1Grid.Columns.Count >= 1;
                        var enabled = !((isDataColumn && isSingleColumn && hasNoRows) || (hasMultipleColumns && hasNoRows));

                        var tableOrViewName = TextHelper.GetSafeString(c1TrueDBGrid1.Tag);
                        var insertInto = !string.IsNullOrEmpty(tableOrViewName) && tableOrViewName.IndexOf(',') == -1; //20251003 使用者查詢的 SQL 必須是單一 Table 或 View (最好再把 View 排除掉)

                        _gridContextMenu.Items[GridColumn.ShowSqlStatement].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.CellViewer].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.SingleRecordViewer].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.Dash0].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.SelectAll].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.Dash1].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.ZoomInGrid].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.ZoomOutGrid].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.Dash2].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.ExportAllDataToFile].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.ExportAllDataToFileScript].Enabled = insertInto;
                        _gridContextMenu.Items[GridColumn.Dash3].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.CopyAllDataToClipboard].Enabled = insertInto;
                        _gridContextMenu.Items[GridColumn.CopyAllDataToClipboardCurrentRow].Enabled = insertInto;
                        _gridContextMenu.Items[GridColumn.Dash7].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.Copy].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.CopyAsQueryCondition].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.CopyWithColumnNames].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.CopyColumnNames].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.Dash8].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.FreezeColumn].Enabled = enabled;
                        _gridContextMenu.Items[GridColumn.UnfreezeColumn].Enabled = !string.IsNullOrEmpty(_unfreezeColumnName);

                        var temp1 = !enabled ? string.Empty : $" ({columnName})";
                        var temp2 = string.IsNullOrEmpty(_unfreezeColumnName) ? string.Empty : $" ({_unfreezeColumnName})";

                        _languageText = LocalizationHelper.GetLanguageString("Cell Viewer", "form", GetType().Name, "menugrid", "CellViewer", "Text");
                        _gridContextMenu.Items[GridColumn.CellViewer].Text = $"{_languageText}{temp1}";

                        _languageText = LocalizationHelper.GetLanguageString("Freeze Column", "form", GetType().Name, "menugrid", "FreezeColumn", "Text");
                        _gridContextMenu.Items[GridColumn.FreezeColumn].Text = $"{_languageText}{temp1}";

                        _languageText = LocalizationHelper.GetLanguageString("Unfreeze Column", "form", GetType().Name, "menugrid", "UnfreezeColumn", "Text");
                        _gridContextMenu.Items[GridColumn.UnfreezeColumn].Text = $"{_languageText}{temp2}";

                        c1Grid.ContextMenuStrip = _gridContextMenu;

                        if (MyLibrary.IsDarkMode)
                        {
                            _gridContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                            _gridContextMenu.ForeColor = Color.White;
                            _gridContextMenu.RenderMode = ToolStripRenderMode.System;
                        }

                        _gridContextMenu.Show(c1Grid, new Point(e.X, e.Y));

                        break;
                    }
            }
        }

        private string GetGridSelectedXY(int colX = -1, int rowY = -1)
        {
            var cells = ";";
            var c1Grid = GetWhichGrid();
            var selCol = c1Grid.SelectedCols.Count;

            foreach (int row in c1Grid.SelectedRows)
            {
                var i = 0;

                if (selCol == 0) //整列選取
                {
                    i = 0;

                    foreach (C1DataColumn column in c1Grid.Columns)
                    {
                        cells += $"{i},{row};";
                        i++;
                    }
                }
                else //非整列選取 (選取區塊)
                {
                    foreach (C1DataColumn column in c1Grid.SelectedCols)
                    {
                        i = c1Grid.Columns.Cast<C1DataColumn>().TakeWhile(colX => column.Caption != column.Caption).Count();

                        cells += $"{i},{row};";
                    }
                }
            }

            if (rowY == -1 || colX == -1)
            {
                return cells;
            }

            return cells.IndexOf($";{colX},{rowY};", StringComparison.Ordinal) >= 0 ? "true" : "false";
        }

        private void c1TrueDBGrid1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_isGridZooming)
            {
                return;
            }

            if (!_isCtrlKeyDown)
            {
                return;
            }

            _isCtrlKeyDown = !_isCtrlKeyDown; //20230710 此處強制變更 _isCtrlKeyDown，避免後續使用者沒有按下 Ctrl，只是單純滾動捲軸時會變成 Grid 放大、縮小
            _isBusy = true;
            _isGridZooming = true;
            _totalDelta += e.Delta;

            try
            {
                var floatValue = 1 + (float)(SystemInformation.MouseWheelScrollLines * _totalDelta) / 3600;

                if (floatValue > 1.7 || floatValue < 0.5)
                {
                    //
                }
                else
                {
                    ZoomGrid(floatValue);
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                _isBusy = false;
                _isGridZooming = false;
            }
        }

        private void ArrangeDataForAllData(string mode, bool isToDate = false)
        {
            var i = 0;
            var csvData = string.Empty;
            var insertFields = string.Empty;
            var insertDataResult = string.Empty;
            var columnName = string.Empty;
            var dataType = string.Empty;
            var temp1 = string.Empty;
            var temp2 = string.Empty;
            var tableName = string.Empty;
            var saveAsFileName = string.Empty;
            var titleName = string.Empty;
            var isAutoOpenFile = false;
            var isUpperCase = MyLibrary.SqlFormatterConvertCaseForKeywordsCase == 1;
            var c1Grid = GetWhichGrid();

            if (string.Equals(mode, "ExportAllDataToFileScript", StringComparison.Ordinal))
            {
                tableName = TextHelper.GetSafeString(c1Grid.Tag, "Table_Name");

                var messageTableName = LocalizationHelper.GetLanguageString("Table name", "form", GetType().Name, "msg", "TableName", "Text");
                var messageExportAllDataToFile = LocalizationHelper.GetLanguageString("Export all data to File (as \"Insert Into\" script)", "form", GetType().Name, "menugrid", "ExportAllDataToFile", "Text");
                var title = $"{messageTableName} - {messageExportAllDataToFile}";
                var promptText = LocalizationHelper.GetLanguageString("Please input table name:", "form", GetType().Name, "msg", "InputTableName", "Text");
                var promptText2 = LocalizationHelper.GetLanguageString("After the file is generated, the file will be opened automatically in a new window.", "form", GetType().Name, "msg", "AutoOpenExportedFile", "Text");

                if (InputBox(title, promptText, promptText2, Cursor.Position.X, Cursor.Position.Y, ref tableName, ref isAutoOpenFile) != DialogResult.OK)
                {
                    return;
                }
            }

            if (mode.StartsWith("CopyAllDataToClipboard", StringComparison.Ordinal))
            {
                tableName = TextHelper.GetSafeString(c1Grid.Tag, "Table_Name");
            }
            else
            {
                var sf = new SaveFileDialog();

                var message = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text");

                switch (mode)
                {
                    case "ExportAllDataToCSV":
                        {
                            titleName = LocalizationHelper.GetLanguageString("Export all data to CSV", "form", GetType().Name, "menugrid", "ExportAllDataToCSV", "Text");
                            message += $" - {titleName}";
                            sf.FileName = MyLibrary.GridSheetName;
                            break;
                        }
                    case "ExportAllDataToFileScript":
                        {
                            titleName = LocalizationHelper.GetLanguageString("Export all data to File (as Insert script)", "form", GetType().Name, "menugrid", "ExportAllDataToFileScript", "Text");
                            message += $" - {titleName}";
                            sf.FileName = $"InsertScript_{tableName}";
                            break;
                        }
                }

                sf.Title = message;

                var messageAllFiles = LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text");
                var messageQueryFile = LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text");
                var messageCsvFile = LocalizationHelper.GetLanguageString("CSV file", "Global", "Global", "msg", "CSVFile", "Text");
                var temp = string.Equals(mode, "EXPORTALLDATATOCSV", StringComparison.OrdinalIgnoreCase) ? @"CSV file (*.csv)|*.csv|All files (*.*)|*.*" : @"Query file (*.sql)|*.sql|All files (*.*)|*.*";

                temp = temp.Replace("All files", messageAllFiles).Replace("Query file", messageQueryFile).Replace("CSV file", messageCsvFile);
                sf.Filter = temp;

                if (sf.ShowDialog() == DialogResult.OK) //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
                {
                    saveAsFileName = sf.FileName;
                }
                else
                {
                    return;
                }
            }

            Cursor = Cursors.WaitCursor;

            var quotingWith = string.Empty;

            if (mnuResultCopyQuotingWithDoubleQuoting.Checked)
            {
                quotingWith = "\"";
            }
            else if (mnuResultCopyQuotingWithSingleQuoting.Checked)
            {
                quotingWith = "'";
            }

            var fieldSeparator = string.Empty;

            if (mnuResultCopyFieldSeparatorSemicolon.Checked)
            {
                fieldSeparator = ";";
            }
            else if (mnuResultCopyFieldSeparatorI.Checked)
            {
                fieldSeparator = "|";
            }
            else
            {
                fieldSeparator = ",";
            }

            var currentRowStart = 0;
            var endRow = c1Grid.Splits[_splitsIndex].Rows.Count;

            if (string.Equals(mode, "COPYALLDATATOCLIPBOARDCURRENTROW", StringComparison.OrdinalIgnoreCase))
            {
                currentRowStart = c1Grid.Row;
                endRow = c1Grid.Row + 1;
            }
            else
            {
                MyGlobal.ProgressInsertInto = 0;
                MyGlobal.IsProgressCancel = false;

                var form = new ProgressDialog
                {
                    TitleName = titleName,
                    TotalQty = c1Grid.Splits[_splitsIndex].Rows.Count,
                    TopMost = false,
                    FormBorderStyle = FormBorderStyle.None,
                    ShowInTaskbar = false,
                    Owner = this,
                    StartPosition = FormStartPosition.CenterScreen
                };

                form.Show();
            }

            for (var row = currentRowStart; row < endRow; row++)
            {
                var insertData = string.Empty;
                var vr = c1Grid.Splits[_splitsIndex].Rows[row];

                foreach (C1DataColumn column in c1Grid.Columns)
                {
                    var value = column.CellText(vr.DataRowIndex);
                    var caption = column.DataField;
                    string[] splitters = { "\r\n", "\r", "\n" };
                    var parts = caption.Split(splitters, 2, StringSplitOptions.None);

                    temp1 = parts[0];
                    temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                    if (i == 0)
                    {
                        //收集 Column Name & Data Type
                        columnName += $"{temp1}{fieldSeparator}";
                        dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";

                        insertFields += $"{temp1}, ";
                    }

                    var columnDataTypeUpper = TextHelper.GetSafeString(column.Tag).ToUpper();

                    //20240113 針對 BLOB 相關欄位，一律改為 NULL
                    if ((mode == "ExportAllDataToFileScript" || mode.StartsWith("CopyAllDataToClipboard", StringComparison.Ordinal)) && column.DataField == "blob")
                    {
                        columnDataTypeUpper = "BLOB";
                    }

                    if ((mode == "ExportAllDataToFileScript" || mode.StartsWith("CopyAllDataToClipboard", StringComparison.Ordinal)) && (columnDataTypeUpper == "BLOB" || value == MyLibrary.GridNullShowAs))
                    {
                        insertData += isUpperCase ? "NULL, " : "null, ";
                    }
                    else
                    {
                        switch (columnDataTypeUpper)
                        {
                            case "STRING":
                                {
                                    csvData += $"{quotingWith}{value}{quotingWith}{fieldSeparator}";
                                    insertData += $"'{value.Replace("'", "''")}', ";
                                    break;
                                }
                            case "NCHAR": //20220421 N型態，這幾個欄位型態要特別處理！
                            case "NSTRING":
                            case "NTEXT":
                                {
                                    csvData += $"{quotingWith}{value}{quotingWith}{fieldSeparator}";
                                    insertData += $"N'{value.Replace("'", "''")}', ";
                                    break;
                                }
                            case "DATETIME":
                                {
                                    csvData += $"{quotingWith}{value}{quotingWith}{fieldSeparator}";

                                    if (!isToDate)
                                    {
                                        insertData += $"'{value}', ";
                                    }
                                    else
                                    {
                                        if (isUpperCase)
                                        {
                                            insertData += $"TO_DATE('{value}', '{MyLibrary.DateFormat} HH24:MI:SS'), ";
                                        }
                                        else
                                        {
                                            insertData += $"to_date('{value}', '{MyLibrary.DateFormat} hh24:mi:ss'), ";
                                        }
                                    }

                                    break;
                                }
                            case "BLOB": //20240114
                                {
                                    csvData += $"{quotingWith}{value}{quotingWith}{fieldSeparator}";
                                    insertData += $"'{value.Replace("'", "''")}', ";
                                    break;
                                }
                            default:
                                {
                                    csvData += $"{value}{fieldSeparator}"; //數字相關的
                                    insertData += $"{value}, ";
                                    break;
                                }
                        }
                    }

                    if (MyGlobal.IsProgressCancel)
                    {
                        break;
                    }
                }

                i++;
                MyGlobal.ProgressInsertInto = i;

                if (!string.IsNullOrEmpty(csvData))
                {
                    var temp = csvData.Substring(0, csvData.Length - fieldSeparator.Length);

                    csvData = $"{temp}\r\n";
                }

                var temp11 = insertFields.Substring(0, insertFields.Length - 2);
                var temp22 = insertData.Substring(0, insertData.Length - 2);

                if (isUpperCase)
                {
                    insertDataResult += $"INSERT INTO {tableName} ({temp11})\r\nVALUES ({temp22});\r\n\r\n";
                }
                else
                {
                    insertDataResult += $"insert into {tableName} ({temp11})\r\nvalues ({temp22});\r\n\r\n";
                }

                if (MyGlobal.IsProgressCancel)
                {
                    break;
                }
            }

            csvData = csvData.TrimEnd('\r', '\n');

            if (!string.IsNullOrEmpty(columnName))
            {
                columnName = columnName.Substring(0, columnName.Length - fieldSeparator.Length);
            }

            if (chkShowColumnType.Checked && !string.IsNullOrEmpty(dataType))
            {
                dataType = dataType.Substring(0, dataType.Length - fieldSeparator.Length);
            }

            if (mode == "ExportAllDataToCSV")
            {
                var temp = string.IsNullOrEmpty(dataType) ? string.Empty : $"\r\n{dataType}";

                csvData = $"{columnName}{temp}\r\n{csvData}";
            }

            var message2 = LocalizationHelper.GetLanguageString("has been exported!", "form", GetType().Name, "msg", "ExportOK", "Text");

            if (string.IsNullOrEmpty(Path.GetExtension(saveAsFileName)))
            {
                saveAsFileName += string.Equals(mode, "EXPORTALLDATATOCSV", StringComparison.OrdinalIgnoreCase) ? ".csv" : ".sql";
            }

            if (MyGlobal.IsProgressCancel)
            {
                message2 = LocalizationHelper.GetLanguageString("This operation has been cancelled.", "Global", "Global", "msg", "CancelByUser", "Text");
                SetFormStatusBarInfo(message2, Color.Blue);
                Cursor = Cursors.Default;
                return;
            }

            if (string.Equals(mode, "EXPORTALLDATATOCSV", StringComparison.OrdinalIgnoreCase))
            {
                TextEngine.WriteContentToFile(csvData, saveAsFileName, TextEncodes.Default); //此處用 TextEncode.Default 存檔，Excel 比較不會出現異常 (比如分割位置錯誤)
                SetFormStatusBarInfo($"{saveAsFileName} {message2}", Color.Blue);
            }
            else
            {
                if (insertDataResult.EndsWith("\r\n\r\n", StringComparison.Ordinal))
                {
                    insertDataResult = insertDataResult.Substring(0, insertDataResult.Length - 2);
                }

                if (mode.StartsWith("CopyAllDataToClipboard", StringComparison.Ordinal))
                {
                    TextHelper.CopyTextToClipboard(insertDataResult, "ArrangeDataForAllData1)");

                    message2 = LocalizationHelper.GetLanguageString("Data has been copied to the clipboard!", "form", GetType().Name, "msg", "CopyOK", "Text"); //資料已被複製到剪貼簿。
                    SetFormStatusBarInfo(message2, Color.Blue);
                }
                else if (mode == "ExportAllDataToFileScript")
                {
                    TextEngine.WriteContentToFile(insertDataResult, saveAsFileName, TextEncodes.UTF8);
                    SetFormStatusBarInfo($"{saveAsFileName} {message2}", Color.Blue);

                    if (isAutoOpenFile)
                    {
                        TransferValueToMainForm($"CreateNewTab`OPENFILE`{saveAsFileName}");
                    }
                }
            }

            Cursor = Cursors.Default;
        }

        private void SetSquiggle(bool isClearOnly, int position = 0, int length = 0)
        {
            editor.IndicatorCurrent = SquiggleNumber;
            editor.IndicatorClearRange(0, editor.TextLength);

            if (isClearOnly)
            {
                return;
            }

            editor.Indicators[SquiggleNumber].Style = ScintillaNET.IndicatorStyle.Squiggle;
            editor.Indicators[SquiggleNumber].ForeColor = ColorTranslator.FromHtml(MyLibrary.ColorErrorLineBackground);
            editor.Indicators[SquiggleNumber].Style = ScintillaNET.IndicatorStyle.Squiggle;
            editor.IndicatorFillRange(position, length);
        }

        private void SetSquiggles(string sqlExecuted, string positionString, string errorMessage, out int cursorPostion, out int length)
        {
            cursorPostion = 0;
            length = 0;
            editor.IndicatorCurrent = SquiggleNumber;
            editor.IndicatorClearRange(0, editor.TextLength);
            editor.Indicators[SquiggleNumber].Style = ScintillaNET.IndicatorStyle.Squiggle;
            editor.Indicators[SquiggleNumber].ForeColor = ColorTranslator.FromHtml(MyLibrary.ColorErrorLineBackground);
            editor.Indicators[SquiggleNumber].Style = ScintillaNET.IndicatorStyle.Squiggle;

            int.TryParse(positionString, out var positionValue);

            var isFirst = true;
            var position = positionValue - 1;
            var parts = sqlExecuted.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var parts2 = errorMessage.Split(new[] { MyGlobal.SeparatorPlus3 }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in parts2)
            {
                var parts3 = t.Split(new[] { MyGlobal.SeparatorPlus4 }, StringSplitOptions.None);
                var line = parts3[0];
                var word = parts3[1];

                int.TryParse(line, out var errLine);
                int.TryParse(parts3[2], out var positionTemp);

                if (errLine <= 0 || errLine > parts.Length || parts[errLine - 1].IndexOf(word, StringComparison.Ordinal) == -1)
                {
                    continue;
                }

                var temp0 = 0;
                var temp9 = 0;
                var positionPerErrLine = parts[errLine - 1].IndexOf(word, positionTemp, StringComparison.Ordinal);

                foreach (var y in parts)
                {
                    if (temp0 == errLine - 1)
                    {
                        temp9 += positionPerErrLine;
                        editor.IndicatorFillRange(position + temp9, word.Length);

                        if (isFirst)
                        {
                            isFirst = false;
                            cursorPostion = position + temp9;
                            length = word.Length;
                        }
                    }
                    else
                    {
                        temp9 += y.Length + 2;
                    }

                    temp0++;
                }
            }
        }

        private void UpdateFindGridList() //判斷是否要更新「搜尋清單」
        {
            if (TextHelper.GetSafeString(lblFindGrid.Tag) == cboFindGrid.Text)
            {
                return;
            }

            if (cboFindGrid.Items.Count > 0 && cboFindGrid.Text == cboFindGrid.Items[0].ToString())
            {
                //搜尋的字串是第一個，不用更新
            }
            else
            {
                SaveFindList("Grid", cboFindGrid.Text); //UpdateFindGridList
            }

            lblFindGrid.Tag = cboFindGrid.Text;
        }

        private void chkShowFilterRow_Click(object sender, EventArgs e)
        {
            try
            {
                var c1Grid = GetWhichGrid();

                c1Grid.FilterBar = chkShowFilterRow.Checked;
                SetGridToolStripBackColor(true);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void c1DockingTab1_Enter(object sender, EventArgs e)
        {
            HideAutoCompleteGrid();

            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void c1DockingTab1_Leave(object sender, EventArgs e)
        {
            ChangeBackColor(_toolstripUnfocused);
        }

        private void tabMessage_Enter(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void tabMessage_Leave(object sender, EventArgs e)
        {
            tsDataGrid.BackColor = _toolstripUnfocused;
        }

        private void tabSqlHistory_Enter(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void tabSqlHistory_Leave(object sender, EventArgs e)
        {
            ChangeBackColor(_toolstripUnfocused);
        }

        private void tabDataGrid_Enter(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void tabDataGrid_Leave(object sender, EventArgs e)
        {
            ChangeBackColor(_toolstripUnfocused);
        }

        private void editorMessage_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor();
        }

        private void c1TrueDBGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCursor();
        }

        private void tsEditor_MouseMove(object sender, MouseEventArgs e)
        {
            HideAutoCompleteGrid();
            UpdateCursor();
        }

        private void tsDataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            HideAutoCompleteGrid();
            UpdateCursor();
        }

        private void UpdateCursor(string control = "")
        {
            if (_isBusy)
            {
                Cursor = Cursors.WaitCursor;
            }
            else
            {
                if (Cursor != Cursors.Default)
                {
                    Cursor = Cursors.Default;
                    Application.UseWaitCursor = false;
                    c1TrueDBGrid1.Cursor = Cursors.Default;
                }

                switch (control)
                {
                    case "AutoReplace":
                        {
                            c1GridAutoReplaceInfo.Cursor = Cursors.Default;
                            break;
                        }
                    case "SchemaBrowser":
                        {
                            c1GridSchemaBrowser.Cursor = Cursors.Default;
                            break;
                        }
                }
            }
        }

        private void btnExportToFile_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }

        private void c1TrueDBGrid1_Click(object sender, EventArgs e)
        {
            var c1Grid = GetWhichGrid();

            if ((ModifierKeys & Keys.Shift) == 0)
            {
                return;
            }

            if (_shiftMouseLeftDownX == -1 || _shiftMouseLeftDownY == -1)
            {
                return;
            }

            int temp;
            int y;

            if (_shiftMouseLeftDownY < c1Grid.Row)
            {
                temp = c1Grid.Row;
                c1Grid.Row = _shiftMouseLeftDownY;
                _shiftMouseLeftDownY = temp;
                y = _shiftMouseLeftDownY;
            }
            else
            {
                y = c1Grid.Row;
            }

            var currentRow = c1Grid.Row;
            var rowCount = _shiftMouseLeftDownY - c1Grid.Row + 1;
            int x;

            if (_shiftMouseLeftDownX < c1Grid.Col)
            {
                temp = c1Grid.Col;
                c1Grid.Col = _shiftMouseLeftDownX;
                _shiftMouseLeftDownX = temp;
                x = _shiftMouseLeftDownX;
            }
            else
            {
                x = c1Grid.Col;
            }

            var currentCol = c1Grid.Col;
            var colCount = _shiftMouseLeftDownX - c1Grid.Col + 1;

            c1Grid.Select(currentRow, currentCol, rowCount, colCount, false);
            c1Grid.SetActiveCell(y, x);

            _shiftMouseLeftDownX = -1;
            _shiftMouseLeftDownY = -1;
        }

        private void ExportToFile()
        {
            var c1Grid = GetWhichGrid();

            //20230824 c1Grid1.FocusedSplit.Rows.Count：可能是篩選前的總筆數，也可能是篩選後的總筆數；如果為 0，表示篩選前 或 篩選後的總筆數 = 0
            if (c1Grid.FocusedSplit.Rows.Count == 0)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;

            var dtData = new DataTable();
            var dt = c1Grid.GetDataTableSourceOrNull();
            var sbHeaderGrid = new StringBuilder();

            for (var i = 0; i < c1Grid.Columns.Count; i++)
            {
                var name = c1Grid.Splits[0].DisplayColumns[i].Name;

                sbHeaderGrid.Append($"{name}`");
            }

            var sbHeaderDataTable = new StringBuilder();
            var dtColumnData = new DataTable();

            dtColumnData.Columns.Add("ColumnName");
            dtColumnData.Columns.Add("ColumnType");

            for (var i = 0; i < dt.Columns.Count; i++)
            {
                var columnName = dt.Columns[i].ColumnName;

                if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    continue;
                }

                sbHeaderDataTable.Append($"{columnName}`");

                var type = "string";

                switch (columnInfo.CategoryDataTypeKind)
                {
                    case CategoryDataTypeKind.Number:
                        {
                            type = "numeric";
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                var row = dtColumnData.NewRow();

                row["ColumnName"] = columnName;
                row["ColumnType"] = type;
                dtColumnData.Rows.Add(row);
            }

            //20230823 判斷是否有篩選
            if (c1Grid.FocusedSplit.Rows[c1Grid.Row].RowType == RowTypeEnum.DataRow)
            {
                var dr = (DataRowView)c1Grid[c1Grid.RowBookmark(c1Grid.Row)];

                //取得篩選後的數據
                dt = dr.DataView[0].Row.Table.DefaultView.ToTable();
            }

            dtData = CreateExportDataTableInDisplayOrder(c1Grid, dt);

            //將前 5 筆資料呈現在預覽畫面上
            var dtPreviewData = dtData.Clone(); //複製資料表結構

            //將前 5 筆資料加入預覽資料表
            foreach (DataRow row in dtData.Rows.Cast<DataRow>().Take(5))
            {
                dtPreviewData.Rows.Add(row.ItemArray);
            }

            Cursor = Cursors.Default;

            using (var form = new ExportToFileForm())
            {
                form.dtData = dtData;
                form.dtSchemaTable = _dtExportedSchemaTable;
                form.FontName = c1Grid.Font.Name;
                form.FontSize = c1Grid.Font.Size;
                form.ColumnInfoCollector2 = _columnInfoCollector;
                form.ShowDialog();
            }
        }

        private DataTable CreateExportDataTableInDisplayOrder(C1TrueDBGrid c1Grid, DataTable sourceTable)
        {
            var result = new DataTable();

            if (c1Grid == null || sourceTable == null)
            {
                return result;
            }

            foreach (C1DisplayColumn displayColumn in c1Grid.Splits[_splitsIndex].DisplayColumns)
            {
                var columnName = displayColumn.DataColumn?.DataField;

                if (string.IsNullOrWhiteSpace(columnName) || !sourceTable.Columns.Contains(columnName) || result.Columns.Contains(columnName))
                {
                    continue;
                }

                result.Columns.Add(columnName, GetExportColumnDataType(columnName));
            }

            foreach (DataRow sourceRow in sourceTable.Rows)
            {
                var targetRow = result.NewRow();

                foreach (DataColumn targetColumn in result.Columns)
                {
                    var columnName = targetColumn.ColumnName;
                    var value = sourceRow[columnName];
                    var displayValue = value?.ToString();

                    targetRow[columnName] = displayValue == MyLibrary.GridNullShowAs ? string.Empty : value;
                }

                result.Rows.Add(targetRow);
            }

            return result;
        }

        private Type GetExportColumnDataType(string columnName)
        {
            if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo)
                && columnInfo != null && columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.Number)
            {
                return typeof(decimal);
            }

            return typeof(string);
        }

        private void editorMessage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _messageEditorContextMenu.Items[0].Enabled = !string.IsNullOrEmpty(editorMessage.Text);

            //Copy: 判斷是否有選取文字，決定功能表項目可不可用
            _messageEditorContextMenu.Items[2].Enabled = !string.IsNullOrEmpty(editorMessage.SelectedText);

            editorMessage.ContextMenuStrip = _messageEditorContextMenu;

            if (MyLibrary.IsDarkMode)
            {
                _messageEditorContextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _messageEditorContextMenu.ForeColor = Color.White;
                _messageEditorContextMenu.RenderMode = ToolStripRenderMode.System;
            }

            _messageEditorContextMenu.Show(editorMessage, new Point(e.X, e.Y));
        }

        private void c1DockingTab1_SelectedTabChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            //20201203 此處改成不要帶入 true，因為偶爾會出現「執行異動指令後，觸發 {TAB}，造成指令變成 {TAB}」
            ApplyQueryResultDockingTabState();
        }

        private void SetEditorStatusBarInfo(string message, Color foreColor)
        {
            var backColor = MyLibrary.IsDarkMode ? "#333333" : "#DFE9F5";

            lblInfoEditor.Visible = true;
            lblInfoEditor.Text = message;
            lblInfoEditor.ForeColor = foreColor;
            lblInfoEditor.BackColor = ColorTranslator.FromHtml(backColor);
        }

        private void SetFormStatusBarInfo(string message, Color foreColor)
        {
            var backColor = MyLibrary.IsDarkMode ? "#333333" : "#DFE9F5";

            lblInfo.Visible = true;
            lblInfo.Text = message;
            lblInfo.ForeColor = foreColor;
            lblInfo.BackColor = ColorTranslator.FromHtml(backColor);
        }

        private void c1TrueDBGrid1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var c1Grid = GetWhichGrid();
            var row = c1Grid.RowContaining(e.Y);

            if (row != -1)
            {
                CellViewer();
            }
        }

        private void c1TrueDBGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            var c1Grid = GetWhichGrid();
            var displayColIndex = c1Grid.ColContaining(e.X);
            var displayRowIndex = c1Grid.RowContaining(e.Y);

            if (displayColIndex == -1 && displayRowIndex == -1)
            {
                return;
            }

            c1Grid.Row = displayRowIndex;
            c1Grid.Col = displayColIndex;

            CalculateCells();
        }

        private void UpdateMessage(string message)
        {
            editorMessage.ReadOnly = false;
            editorMessage.Text = message;
            editorMessage.ReadOnly = true;
            editorMessage.IndentationGuides = ScintillaNET.IndentView.None; //20250913 在此處設定為 ScintillaNET.IndentView.None 才有預期的效果

            //20251022 移至最底部
            if (editorMessage.TextLength > 0)
            {
                editorMessage.SelectionStart = editorMessage.TextLength - 1;
                editorMessage.ScrollCaret();
            }
        }

        private void ChangeBackColor(Color color)
        {
            tsDataGrid.BackColor = color;
            chkShowFilterRow.BackColor = color;
            chkSize.BackColor = color;
            chkShowColumnType.BackColor = color;
            chkShowGroupingRow.BackColor = color;
            chkShowColumnComments.BackColor = color;
            chkRawDataMode.BackColor = color;
        }

        private C1TrueDBGrid GetWhichGrid() //預留：如果後續允許一次查詢多個 SQL Statement 時，用它來判斷 C1TrueDBGrid
        {
            var isBreak = false;
            const int queryIndex = 1;
            C1TrueDBGrid c1Grid = null;

            foreach (var ctl in c1DockingTab1.SelectedTab.Controls)
            {
                if (ctl is C1TrueDBGrid grid)
                {
                    c1Grid = grid;
                }
            }

            if (c1Grid != null)
            {
                return c1Grid;
            }

            foreach (Control tab in c1DockingTab1.TabPages)
            {
                var tabPage = (C1DockingTabPage)tab;

                foreach (var ctrlTab in tabPage.Controls)
                {
                    var name = ctrlTab.GetType().Name;

                    if (name != "C1TrueDBGrid")
                    {
                        if (name == "SplitContainer")
                        {
                            return c1TrueDBGrid1;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var temp = ((C1TrueDBGrid)ctrlTab).Name;

                    if (temp != $"c1TrueDBGrid{queryIndex}")
                    {
                        continue;
                    }

                    c1Grid = (C1TrueDBGrid)ctrlTab;
                    isBreak = true;
                    break;
                }

                if (isBreak)
                {
                    break;
                }
            }

            return c1Grid;
        }

        private void chkShowGroupingRow_CheckedChanged(object sender, EventArgs e)
        {
            c1TrueDBGrid1.DataView = chkShowGroupingRow.Checked ? DataViewEnum.GroupBy : DataViewEnum.Normal;
        }

        private void chkShowGroupingRow_Click(object sender, EventArgs e)
        {
            tsEditor.BackColor = _toolstripUnfocused;
            tsDataGrid.BackColor = _toolstripFocused;
            ChangeBackColor(_toolstripFocused);
        }

        private void btnPagination_Click(object sender, EventArgs e)
        {
            btnPaginationOn.Visible = !btnPaginationOn.Visible;
            btnPaginationOff.Visible = !btnPaginationOn.Visible;

            //下一頁按鈕不在此處控制！
            //btnNextPage.Enabled = btnPaginationOn.Visible;
        }

        private void btnAppendingQueries_Click(object sender, EventArgs e)
        {
            btnAppendingQueriesOn.Visible = !btnAppendingQueriesOn.Visible;
            btnAppendingQueriesOff.Visible = !btnAppendingQueriesOn.Visible;
        }

        private void ShowSqlStatement()
        {
            var c1Grid = GetWhichGrid();
            var cellText = c1Grid.AccessibleDescription;

            using (var form = new SqlStatementViewerForm())
            {
                form.CellText = cellText;

                var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "SQLStatementViewerFormWidth", "SQLStatementViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                form.ShowDialog();
            }
        }

        private void c1TrueDBGrid1_Scroll(object sender, C1.Win.C1TrueDBGrid.CancelEventArgs e)
        {
            var oldOffset = c1TrueDBGrid1.Splits[_splitsIndex].VerticalOffset;

            if (!btnNextPage.Enabled || _lastTimeOffset == oldOffset)
            {
                //水平移動
                return;
            }

            if ((oldOffset + (300 * _nextPageScale)) >= _lastRowOffset)
            {
                NextPage(c1TrueDBGrid1.Col, c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count - 1);
            }

            _lastTimeOffset = oldOffset;
        }

        private static DialogResult InputBox(string title, string promptText, string promptText2, int x, int y, ref string value, ref bool autoOpenFile)
        {
            var form = new Form();
            var lblPromptText = new Label();
            var lblTemp = new Label();
            var lblTemp1 = new Label();
            var txtInputBox = new TextBox();
            var btnOK = new C1Button();
            var btnCancel = new C1Button();
            var chkAutoOpenFile = new CheckBox();

            form.Text = title;
            form.ClientSize = new Size(396, 152);
            form.Controls.AddRange(new Control[] { lblPromptText, lblTemp, txtInputBox, btnOK, btnCancel, chkAutoOpenFile });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);

            lblPromptText.Text = promptText;
            txtInputBox.Text = value;
            chkAutoOpenFile.Text = promptText2;
            btnOK.Text = LocalizationHelper.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");
            btnCancel.Text = LocalizationHelper.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");

            btnOK.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;

            lblPromptText.SetBounds(14, 15, 372, 13);
            chkAutoOpenFile.SetBounds(17, 65, 372, 20);
            btnOK.SetBounds(215, 100, 75, 37);
            btnCancel.SetBounds(304, 100, 74, 37);

            lblTemp.SetBounds(12, 65, 372, 13);
            lblTemp.AutoSize = true;
            lblTemp.Text = value;
            lblTemp.Visible = false;

            lblTemp1.SetBounds(0, 0, 372, 13);
            lblTemp1.AutoSize = true;
            lblTemp1.Text = title;
            lblTemp1.Visible = false;

            lblPromptText.AutoSize = true;
            chkAutoOpenFile.AutoSize = true;
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(Math.Max(lblPromptText.Right + 20, Math.Max(chkAutoOpenFile.Right + 20, lblTemp1.Width + 20)), form.ClientSize.Height);
            txtInputBox.SetBounds(15, 36, form.Width - 50, 20);

            txtInputBox.TextChanged += delegate
            {
                btnOK.Enabled = !string.IsNullOrWhiteSpace(txtInputBox.Text);
            };

            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(x - lblTemp.Width - 25, y - 76);

            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = btnOK;
            form.CancelButton = btnCancel;

            var dialogResult = form.ShowDialog();

            value = txtInputBox.Text;
            autoOpenFile = chkAutoOpenFile.Checked;
            return dialogResult;
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

            ApplyCommitRollbackButtonState(false);
        }

        private void mnuResultCopyQuotingWith_Click(object sender, EventArgs e)
        {
            var mnu = sender as ToolStripMenuItem;

            mnuResultCopyQuotingWithNone.Checked = false;
            mnuResultCopyQuotingWithDoubleQuoting.Checked = false;
            mnuResultCopyQuotingWithSingleQuoting.Checked = false;
            mnu.Checked = true;

            var tag = TextHelper.GetSafeString(mnu.Tag);

            switch (tag)
            {
                case "None":
                    {
                        mnuResultCopyQuotingWith.Tag = "None";
                        break;
                    }
                case "\"":
                    {
                        mnuResultCopyQuotingWith.Tag = "\"";
                        break;
                    }
                case "'":
                    {
                        mnuResultCopyQuotingWith.Tag = "'";
                        break;
                    }
            }
        }

        private void mnuResultCopyFieldSeparator_Click(object sender, EventArgs e)
        {
            var mnu = sender as ToolStripMenuItem;

            mnuResultCopyFieldSeparatorComma.Checked = false;
            mnuResultCopyFieldSeparatorSemicolon.Checked = false;
            mnuResultCopyFieldSeparatorI.Checked = false;
            mnu.Checked = true;

            var tag = TextHelper.GetSafeString(mnu.Tag);

            switch (tag)
            {
                case ",":
                    {
                        mnuResultCopyFieldSeparator.Tag = ",";
                        break;
                    }
                case ";":
                    {
                        mnuResultCopyFieldSeparator.Tag = ";";
                        break;
                    }
                case "|":
                    {
                        mnuResultCopyFieldSeparator.Tag = "|";
                        break;
                    }
            }
        }

        private void c1TrueDBGrid1_MouseClick(object sender, MouseEventArgs e)
        {
            var isCornerSelected = false;
            var c1Grid = GetWhichGrid();
            var row = c1Grid.RowContaining(e.Y);
            var col = c1Grid.ColContaining(e.X);

            if (row == -1 && col == -1)
            {
                isCornerSelected = !chkShowFilterRow.Checked || e.Y <= c1Grid.Splits[_splitsIndex].ColumnCaptionHeight;
            }

            if (!isCornerSelected || e.Button != MouseButtons.Left || e.X >= 20)
            {
                return;
            }

            c1Grid.SelectedRows.Clear();

            for (var i = 0; i < c1Grid.Splits[_splitsIndex].Rows.Count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }
        }

        private void c1DockingTab2_TabClick(object sender, EventArgs e)
        {
            switch (c1DockingTab2.SelectedTab.Name)
            {
                case "tabAutoReplace":
                    {
                        c1GridAutoReplaceInfo.Focus();
                        break;
                    }
                case "tabSchemaInformation":
                    {
                        c1GridSchemaBrowser.Focus();
                        break;
                    }
            }
        }

        private void ApplyCommitRollbackButtonState(bool enabled)
        {
            btnCommit.Enabled = enabled;
            btnRollback.Enabled = enabled;
        }

        private void CheckNodeThenCollapsedOrExpanded(bool isMouseClick = false)
        {
            var currentRow = c1GridSchemaBrowser.Row;

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
                case RowTypeEnum.DataRow:
                    {
                        if (isMouseClick)
                        {
                            var schemaType = (c1GridSchemaBrowser.GetDataBoundItem(currentRow) as DataRowView).Row["SchemaType"].ToString();

                            //20250829 針對 Tables，取得其 Column Name
                            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                            {
                                var columnName = (c1GridSchemaBrowser.GetDataBoundItem(currentRow) as DataRowView).Row["Schema_Browser"].ToString();
                                var temp = columnName.IndexOf(", ", StringComparison.Ordinal);

                                if (temp >= 0) //欄位名稱
                                {
                                    columnName = TextHelper.GetSafeSubstring(columnName, 0, temp + 2);
                                }

                                Clipboard.SetDataObject(columnName, false);
                                editor.Paste();

                                if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                                {
                                    editor.Focus();
                                }
                            }
                        }

                        break;
                    }
            }
        }

        private void btnSettingOfFocus_ButtonClick(object sender, EventArgs e)
        {
            mnuFocusOnQueryEditor.Checked = AppConfigHelper.IsAfterPasteFocusOnQueryEditor;
            mnuFocusOnDataGrid.Checked = !AppConfigHelper.IsAfterPasteFocusOnQueryEditor;
        }

        private void mnuFocusOnDataGrid_Click(object sender, EventArgs e)
        {
            mnuFocusOnDataGrid.Checked = !mnuFocusOnDataGrid.Checked;
            mnuFocusOnQueryEditor.Checked = !mnuFocusOnDataGrid.Checked;

            AppConfigHelper.IsAfterPasteFocusOnQueryEditor = mnuFocusOnQueryEditor.Checked;
            JasonQueryRepository.UpdateSetting("EditorConfig", "AfterPasteFocusOnQueryEditor", mnuFocusOnQueryEditor.Checked ? "1" : "0");
        }

        private void mnuFocusOnQueryEditor_Click(object sender, EventArgs e)
        {
            mnuFocusOnQueryEditor.Checked = !mnuFocusOnQueryEditor.Checked;
            mnuFocusOnDataGrid.Checked = !mnuFocusOnQueryEditor.Checked;

            AppConfigHelper.IsAfterPasteFocusOnQueryEditor = mnuFocusOnQueryEditor.Checked;
            JasonQueryRepository.UpdateSetting("EditorConfig", "AfterPasteFocusOnQueryEditor", mnuFocusOnQueryEditor.Checked ? "1" : "0");
        }

        private void ArrangeSchemaDataForCopyPaste(string mode)
        {
            try
            {
                var currentRow = c1GridSchemaBrowser.Row;
                var temp = string.Empty;
                var vr = c1GridSchemaBrowser.Splits[0].Rows[currentRow];

                //判斷是不是節點
                if (c1GridSchemaBrowser.Splits[0].Rows[currentRow].RowType == RowTypeEnum.CollapsedGroupRow || c1GridSchemaBrowser.Splits[0].Rows[currentRow].RowType == RowTypeEnum.ExpandedGroupRow)
                {
                    var level = ((GroupRow)vr).Level;

                    switch (_currentSourceType)
                    {
                        case DataSourceType.Oracle when level <= 1:
                        case DataSourceType.PostgreSql when level <= 2:
                        case DataSourceType.SqlServer when level <= 2:
                        case DataSourceType.MySql when level <= 2:
                            {
                                return; //針對主要節點(例如 AliasName/Tables/Functions/View/Triggers)，不處理
                            }
                        default:
                            {
                                temp = ((GroupRow)vr).GroupedText;
                                break;
                            }
                    }
                }
                else
                {
                    vr = c1GridSchemaBrowser.Splits[0].Rows[currentRow];

                    var schemaType = (c1GridSchemaBrowser.GetDataBoundItem(currentRow) as DataRowView).Row["SchemaType"].ToString();

                    if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                    {
                        //原廠給的建議寫法：use GetDataBoundItem to get the DataRowView associated with the current row and then fetch data from its underlying row
                        temp = (c1GridSchemaBrowser.GetDataBoundItem(currentRow) as DataRowView).Row["Schema_Browser"].ToString();
                    }
                    else
                    {
                        //原廠給的建議寫法：use GetDataBoundItem to get the DataRowView associated with the current row and then fetch data from its underlying row
                        temp = (c1GridSchemaBrowser.GetDataBoundItem(currentRow) as DataRowView).Row["Schema_Browser"].ToString();
                    }

                    if (temp.IndexOf(", ", StringComparison.Ordinal) >= 0)
                    {
                        temp = temp.Split(new[] { ", " }, StringSplitOptions.None)[0];
                    }

                    if (temp.IndexOf(", ", StringComparison.Ordinal) >= 0)
                    {
                        temp = temp.Split(new[] { ", " }, StringSplitOptions.None)[0];
                    }
                }

                if (string.IsNullOrEmpty(temp))
                {
                    return;
                }

                if (temp.IndexOf(MyGlobal.Separator, StringComparison.Ordinal) >= 0)
                {
                    temp = temp.Substring(0, temp.IndexOf(MyGlobal.Separator, StringComparison.Ordinal));
                }

                TextHelper.CopyTextToClipboard(temp, "ArrangeSchemaData4CopyPaste(01)");

                if (!mode.StartsWith("Paste", StringComparison.Ordinal))
                {
                    return;
                }

                switch (mode)
                {
                    case "Paste3":
                        {
                            TextHelper.CopyTextToClipboard(temp, "ArrangeSchemaData4CopyPaste(02)");
                            break;
                        }
                    case "Paste4":
                        {
                            TextHelper.CopyTextToClipboard($"{temp}, ", "ArrangeSchemaData4CopyPaste(03)");
                            break;
                        }
                    case "Paste5":
                        {
                            TextHelper.CopyTextToClipboard($"{temp}, \r\n", "ArrangeSchemaData4CopyPaste(04)");
                            break;
                        }
                    case "Paste6":
                        {
                            TextHelper.CopyTextToClipboard($"{temp}\r\n", "ArrangeSchemaData4CopyPaste(05)");
                            break;
                        }
                }

                editor.Paste();

                if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                {
                    editor.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void c1GridSchemaBrowser_Enter(object sender, EventArgs e)
        {
            tsSchemaBrowser.BackColor = _toolstripFocused;
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
                ShowExceptionMessage(ex);
            }
        }

        private void c1GridSchemaBrowser_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    {
                        e.Handled = true; //true：表示取消原本的左右鍵，不發揮作用 (不會右移或左移)，避免動作重複
                        CheckNodeThenCollapsedOrExpanded();
                        break;
                    }
            }
        }

        private void c1GridSchemaBrowser_Leave(object sender, EventArgs e)
        {
            tsSchemaBrowser.BackColor = _toolstripUnfocused;
        }

        private void c1GridSchemaBrowser_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CheckNodeThenCollapsedOrExpanded(true);
        }

        private void c1GridSchemaBrowser_MouseDown(object sender, MouseEventArgs e)
        {
            var schemaName = string.Empty;
            var tableName = string.Empty; //20241214 for EditColumnForm, 取得 Table Name
            var columnType = string.Empty; //20241214 for EditColumnForm & MySQL, 取得 Column Type
            var displayRowIndex = c1GridSchemaBrowser.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return; //忽略標題列
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _schemaBrowserContextMenu = new ContextMenuStrip();
            c1GridSchemaBrowser.ContextMenuStrip = _schemaBrowserContextMenu;

            var vr = c1GridSchemaBrowser.Splits[0].Rows[displayRowIndex];

            c1GridSchemaBrowser.Row = displayRowIndex;

            var schemaType = string.Empty;
            var schemaType2 = string.Empty;
            var schemaNode = string.Empty;
            var schemaDbo = string.Empty;
            var objectId = string.Empty;
            int level;
            var title1 = LocalizationHelper.GetLanguageString("Copy to Clipboard", "form", "GenerateSqlForm", "object", "btnCopyToClipboard", "Text");
            var title2 = LocalizationHelper.GetLanguageString("Paste to Query Editor", "form", "GenerateSqlForm", "object", "btnPasteToQueryEditor", "Text");
            var packageSpecBody = string.Empty;

            if (c1GridSchemaBrowser.Splits[0].Rows[displayRowIndex].RowType == RowTypeEnum.CollapsedGroupRow || c1GridSchemaBrowser.Splits[0].Rows[displayRowIndex].RowType == RowTypeEnum.ExpandedGroupRow)
            {
                level = ((GroupRow)vr).Level;

                //此寫法可能有問題：原廠的 bug，有時會取到錯誤的值
                schemaType = c1GridSchemaBrowser.Columns["SchemaType"].CellValue(((GroupRow)vr).StartIndex).ToString();

                var j = 2;

                if (IsOracle)
                {
                    j = 1;
                }

                //原廠給的暫時性解法
                for (var i = displayRowIndex - 1; i >= 0; i--)
                {
                    if (!(c1GridSchemaBrowser.Splits[0].Rows[i] is GroupRow groupRow))
                    {
                        continue;
                    }

                    if (groupRow.Level != j)
                    {
                        continue;
                    }

                    schemaType2 = groupRow.GroupedText;
                    break;
                }

                var groupedText = ((GroupRow)vr).GroupedText;
                var dtSchema0 = c1GridSchemaBrowser.GetDataTableSourceOrNull();

                //20220516 判斷 sSchemaType 哪一個是正確的
                if (schemaType != schemaType2 && !string.IsNullOrEmpty(schemaType2))
                {
                    var tempSchemaObject1 = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaObject", ("SchemaType", schemaType), ("SchemaName", groupedText));
                    var tempSchemaObject2 = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaObject", ("SchemaType", schemaType2), ("SchemaName", groupedText));

                    switch (_currentSourceType)
                    {
                        case DataSourceType.Oracle when level > 1:
                        case DataSourceType.PostgreSql when level > 2:
                        case DataSourceType.SqlServer when level > 2:
                        case DataSourceType.MySql when level > 2:
                            {
                                if (string.IsNullOrEmpty(tempSchemaObject1) && !string.IsNullOrEmpty(tempSchemaObject2))
                                {
                                    schemaType = schemaType2;
                                }

                                break;
                            }
                    }
                }

                switch (_currentSourceType)
                {
                    case DataSourceType.PostgreSql when level > 2:
                        {
                            schemaNode = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaNode", ("SchemaType", schemaType), ("SchemaName", groupedText));
                            break;
                        }
                    case DataSourceType.SqlServer when level > 2:
                        {
                            schemaNode = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaNode", ("SchemaType", schemaType), ("SchemaName", groupedText));
                            schemaDbo = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaDbo", ("SchemaType", schemaType), ("SchemaName", groupedText));
                            objectId = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "ObjectID", ("SchemaType", schemaType), ("SchemaName", groupedText));
                            break;
                        }
                    case DataSourceType.MySql when level > 2:
                        {
                            schemaNode = DataTableSearchHelper.FindValueFromDataTable(dtSchema0, "SchemaNode", ("SchemaType", schemaType), ("SchemaName", groupedText));
                            break;
                        }
                }

                var temp01 = schemaType.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                if (temp01 >= 0)
                {
                    schemaType = schemaType.Substring(0, temp01);
                }

                schemaName = groupedText;

                var temp02 = schemaName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                if (temp02 >= 0)
                {
                    if (IsOracle && SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Packages))
                    {
                        packageSpecBody = schemaName.EndsWith("(Spec)", StringComparison.Ordinal) ? "Spec" : "Body";
                    }

                    schemaName = schemaName.Substring(0, temp02);
                }

                switch (_currentSourceType)
                {
                    case DataSourceType.SqlServer when level == 1:
                    case DataSourceType.MySql when level == 1:
                    case DataSourceType.Oracle when level <= 1:
                    case DataSourceType.PostgreSql when level <= 2:
                    case DataSourceType.SqlServer when level <= 2:
                    case DataSourceType.MySql when level <= 2:
                        {
                            return; //針對主要節點(例如 AliasName/Tables/Functions/View/Triggers)，右鍵不處理
                        }
                    case DataSourceType.Oracle:
                        {
                            if (SchemaObjectTypeHelper.StartsWithAny(schemaType, SchemaObjectNames.Functions, SchemaObjectNames.Triggers,
                                                                     SchemaObjectNames.Procedures, SchemaObjectNames.Packages))
                            {
                                UIHelper.GenerateRightMenuForCopyOnly(false, _schemaBrowserContextMenu, c1GridSchemaBrowser, editor, title1, title2, schemaName, e.X, e.Y, false, string.Empty, string.Empty, string.Empty, string.Empty, schemaType, packageSpecBody);
                            }
                            else
                            {
                                UIHelper.GenerateRightMenuForCopy_Oracle(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaType, title1, title2, schemaName, e.X, e.Y, packageSpecBody);
                            }

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            if (SchemaObjectTypeHelper.StartsWithAny(schemaType, SchemaObjectNames.Functions, SchemaObjectNames.Triggers))
                            {
                                UIHelper.GenerateRightMenuForCopyOnly(false, _schemaBrowserContextMenu, c1GridSchemaBrowser, editor, title1, title2, schemaName, e.X, e.Y, false, schemaNode, string.Empty, string.Empty, string.Empty, schemaType);
                            }
                            else
                            {
                                UIHelper.GenerateRightMenuForCopy_PostgreSql(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaType, title1, title2, schemaName, e.X, e.Y);
                            }

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            if (SchemaObjectTypeHelper.StartsWithAny(schemaType, SchemaObjectNames.Functions, SchemaObjectNames.Triggers,
                                                                     SchemaObjectNames.Procedures, SchemaObjectNames.Indexes))
                            {
                                UIHelper.GenerateRightMenuForCopyOnly(false, _schemaBrowserContextMenu, c1GridSchemaBrowser, editor, title1, title2, schemaName, e.X, e.Y, false, schemaNode, string.Empty, string.Empty, string.Empty, schemaType, string.Empty, schemaDbo);
                            }
                            else
                            {
                                UIHelper.GenerateRightMenuForCopy_SqlServer(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaDbo, schemaType, title1, title2, schemaName, e.X, e.Y, objectId);
                            }

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            if (SchemaObjectTypeHelper.StartsWithAny(schemaType, SchemaObjectNames.Functions, SchemaObjectNames.Triggers,
                                                                     SchemaObjectNames.Procedures, SchemaObjectNames.Indexes))
                            {
                                UIHelper.GenerateRightMenuForCopyOnly(false, _schemaBrowserContextMenu, c1GridSchemaBrowser, editor, title1, title2, schemaName, e.X, e.Y, false, schemaNode, string.Empty, string.Empty, string.Empty, schemaType);
                            }
                            else
                            {
                                UIHelper.GenerateRightMenuForCopy_MySql(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaType, title1, title2, schemaName, e.X, e.Y);
                            }

                            break;
                        }
                }
            }
            else //滑鼠所在處並不是節點！
            {
                //20220418：原廠給的改善寫法
                schemaType = (c1GridSchemaBrowser.GetDataBoundItem(displayRowIndex) as DataRowView).Row["SchemaType"].ToString();

                var temp01 = schemaType.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                if (temp01 >= 0)
                {
                    schemaType = schemaType.Substring(0, temp01);
                }

                //20241214 針對 Tables，取得其 Table Name
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                {
                    switch (_currentSourceType)
                    {
                        case DataSourceType.Oracle:
                            {
                                schemaNode = DatabaseSqlExecutor.DbUser;
                                break;
                            }
                        case DataSourceType.PostgreSql:
                            {
                                schemaNode = (c1GridSchemaBrowser.GetDataBoundItem(displayRowIndex) as DataRowView).Row["SchemaNode"].ToString();
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                schemaNode = (c1GridSchemaBrowser.GetDataBoundItem(displayRowIndex) as DataRowView).Row["SchemaDbo"].ToString();
                                break;
                            }
                    }

                    tableName = (c1GridSchemaBrowser.GetDataBoundItem(displayRowIndex) as DataRowView).Row["SchemaName"].ToString();

                    var temp02 = tableName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

                    if (temp02 >= 0)
                    {
                        tableName = tableName.Substring(0, temp02);
                    }

                    if (IsSqlServer)
                    {
                        tableName = tableName.Replace($"{schemaNode}.", string.Empty);
                    }
                }

                schemaName = (c1GridSchemaBrowser.GetDataBoundItem(displayRowIndex) as DataRowView).Row["Schema_Browser"].ToString();

                var temp03 = schemaName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);
                var temp04 = schemaName.IndexOf(", ", StringComparison.Ordinal);

                if (temp03 >= 0)
                {
                    schemaName = schemaName.Substring(0, temp03);
                }
                else if (temp04 >= 0) //欄位名稱
                {
                    //20241214 取得 Column Type
                    columnType = schemaName.Substring(temp04 + 2);

                    schemaName = schemaName.Substring(0, temp04);
                }

                if (string.IsNullOrEmpty(schemaName))
                {
                    //如果使用者有開啟「顯示欄位名稱」，除了「Table」外，其餘的項目，sSchemaName 都會是空值
                    return;
                }

                var isContinue = true;
                var isViewOrTable = SchemaObjectTypeHelper.IsAny(schemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views);

                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle when MyGlobal.IsShowColumnInfo:
                    case DataSourceType.PostgreSql when MyGlobal.IsShowColumnInfo:
                    case DataSourceType.SqlServer when MyGlobal.IsShowColumnInfo:
                    case DataSourceType.MySql when MyGlobal.IsShowColumnInfo:
                        {
                            break; //不是節點，有顯示 Column Info，如果 sSchemaName 不是空值，就顯示右鍵功能表
                        }
                    case DataSourceType.Oracle:
                        {
                            if (isViewOrTable)
                            {
                                isContinue = false;
                                UIHelper.GenerateRightMenuForCopy_Oracle(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaType, title1, title2, schemaName, e.X, e.Y);
                            }

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            schemaNode = c1GridSchemaBrowser.Columns["SchemaNode"].CellValue(vr.DataRowIndex).ToString();

                            if (isViewOrTable)
                            {
                                isContinue = false;
                                UIHelper.GenerateRightMenuForCopy_PostgreSql(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaType, title1, title2, schemaName, e.X, e.Y);
                            }

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            if (isViewOrTable)
                            {
                                isContinue = false;
                                schemaNode = c1GridSchemaBrowser.Columns["SchemaNode"].CellValue(vr.DataRowIndex).ToString();
                                schemaDbo = c1GridSchemaBrowser.Columns["SchemaDbo"].CellValue(vr.DataRowIndex).ToString();
                                objectId = c1GridSchemaBrowser.Columns["ObjectID"].CellValue(vr.DataRowIndex).ToString();

                                UIHelper.GenerateRightMenuForCopy_SqlServer(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaDbo, schemaType, title1, title2, schemaName, e.X, e.Y, objectId);
                            }

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            schemaNode = c1GridSchemaBrowser.Columns["SchemaNode"].CellValue(vr.DataRowIndex).ToString();

                            if (isViewOrTable)
                            {
                                isContinue = false;
                                UIHelper.GenerateRightMenuForCopy_MySql(false, c1GridSchemaBrowser, _schemaBrowserContextMenu, editor, AccessibleDescription, schemaNode, schemaType, title1, title2, schemaName, e.X, e.Y);
                            }

                            break;
                        }
                }

                if (isContinue)
                {
                    //在欄位名稱按右鍵
                    UIHelper.GenerateRightMenuForCopyOnly(false, _schemaBrowserContextMenu, c1GridSchemaBrowser, editor, title1, title2, schemaName, e.X, e.Y, true, schemaNode, tableName, columnType, DatabaseSqlExecutor.DatabaseName, schemaType, packageSpecBody, schemaDbo);
                }
            }
        }

        private void nudQueryTimeout_Enter(object sender, EventArgs e)
        {
            nudQueryTimeout.Select(0, nudQueryTimeout.Text.Length);
        }

        private void nudQueryTimeout_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nudQueryTimeout.Text))
            {
                nudQueryTimeout.UpButton();
                nudQueryTimeout.DownButton();
                nudQueryTimeout.Value = 30;
            }

           
            if (nudQueryTimeout.Value < 30 && nudQueryTimeout.Value != 0) //20250109 允許輸入 0
            {
                nudQueryTimeout.Value = 30;
            }
        }

        private void nudQueryTimeout_MouseClick(object sender, MouseEventArgs e)
        {
            nudQueryTimeout.Select(0, nudQueryTimeout.Text.Length);
        }

        private void Zoom_Click(object sender, EventArgs e)
        {
            var btn = sender as ToolStripButton;
            var tag = TextHelper.GetSafeString(btn?.Tag);

            ZoomObject(tag);
        }

        private void ZoomObject(string tag)
        {
            try
            {
                switch (tag)
                {
                    case "ZoomInEditor":
                    case "ZoomOutEditor":
                        {
                            tsEditor.BackColor = _toolstripFocused;
                            tsDataGrid.BackColor = _toolstripUnfocused;

                            if (tag == "ZoomInEditor")
                            {
                                editor.ZoomIn();
                            }
                            else
                            {
                                editor.ZoomOut();
                            }

                            break;
                        }
                    case "ZoomInGrid":
                    case "ZoomOutGrid":
                        {
                            var c1Grid = GetWhichGrid();

                            c1Grid.Focus();
                            tsEditor.BackColor = _toolstripUnfocused;
                            SetGridToolStripBackColor(true);

                            var delta = tag == "ZoomInGrid" ? 120 : -120;

                            if (_isGridZooming)
                            {
                                return;
                            }

                            _isBusy = true;
                            _isGridZooming = true;
                            _totalDelta += delta;

                            var value = 1 + (float)(SystemInformation.MouseWheelScrollLines * _totalDelta) / 3600;

                            if (value > 1.7 || value < 0.5)
                            {
                                //
                            }
                            else
                            {
                                ZoomGrid(value);
                            }

                            _isBusy = false;
                            _isGridZooming = false;

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnExecuteCurrentLine_Click(object sender, EventArgs e)
        {
            ExecuteCurrentLine();
        }

        private void ExecuteCurrentLine()
        {
            var selectedText = SelectActiveLine();

            selectedText = string.Concat(selectedText.Where(c => !char.IsWhiteSpace(c)));

            if (!string.IsNullOrEmpty(selectedText))
            {
                btnQuery.PerformClick();
            }
        }

        private void btnRemoveTrailingBlanks_Click(object sender, EventArgs e)
        {
            try
            {
                _editorWhitespaceCleanupService.RemoveTrailingBlanks();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnHelp_QueryTimeout_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This setting specifies the maximum time (in seconds) to wait for a server response when executing a query command before generating an error.\r\n\r\nThe timeout is measured from the moment the query command is sent to the server.\r\nIt includes only the waiting time for the server response and does not include the time required to fetch data.\r\n\r\nThis is a connection-level setting and applies to all query editors created using this connection.\r\n\r\nThe timeout value set in a Query Editor toolbar affects only that editor and takes precedence over this setting.\r\n\r\nA value of 0 indicates no time limit.", "Global", "Global", "msg", "Help_QueryTimeout", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSelectCurrentLine_Click(object sender, EventArgs e)
        {
            SelectActiveLine();
        }

        private void tsDataGrid_Enter(object sender, EventArgs e)
        {
            HideAutoCompleteGrid();
        }

        private void c1StatusBar1_MouseMove(object sender, MouseEventArgs e)
        {
            HideAutoCompleteGrid();
        }

        private void c1StatusBar2_MouseMove(object sender, MouseEventArgs e)
        {
            HideAutoCompleteGrid();
        }

        private void AutoResizeGridColumnWidth()
        {
            var width = 0;

            if (c1GridSchemaBrowser.IsDataTableSourceNullOrEmpty())
            {
                return;
            }

            var i = 3 + (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1 ? 1 : 0);

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        i--;
                        width = 96 + (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1 ? 10 : 0);
                        break;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        width = 110 + (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1 ? 10 : 0);
                        break;
                    }
            }

            var widthText = TextHelper.GetSafeString(splitContainer2.Panel1.Width);

            int.TryParse(widthText, out var width2);
            width2 = width2 <= 60 ? 120 : width2;

            c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width = width2 - width;

            if (c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width < 60)
            {
                c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width = 60;
            }

            c1GridSchemaBrowser.Refresh();

            ResizeAutoReplaceReplacementColumn();
        }

        private void HideAutoCompleteGrid(bool value = true)
        {
            if (value && _autoCompleteMousePosition.HasValue && Cursor.Position == _autoCompleteMousePosition.Value) //輸入過程中，若滑鼠沒移動，則忽略
            {
                return;
            }

            HidePeriodAutoCompletePopup();
            HideSpaceAutoCompletePopup();
            QueryEditorAutoCompletePopupController.Hide(c1GridAutoCompleteForAll);
        }

        private void ReloadQueryEditorSetting()
        {
            try
            {
                ApplyEditorSetting();

                if (MyLibrary.EnableAutoReplace)
                {
                    CreateAndGetAutoReplaceInfoTable(_dtAutoReplaceInfoTable);
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private static class AutoReplaceColumn
        {
            public const int Pid = 0;
            public const int Keyword = 1;
            public const int Replacement = 2;
        }

        private static class QueryEditorColumn
        {
            public const int Execute = 0;
            public const int ExecuteCurrentBlock = 1;
            public const int ExecuteCurrentLine = 2;
            public const int Dash2 = 3;
            public const int SchemaBrowser = 4;
            public const int ShowCreateScript = 5;
            public const int SchemaBrowserTable = 6;
            public const int SchemaBrowserView = 7;
            public const int GenerateSql = 8;
            public const int Dash3 = 9;
            public const int TableComment = 10;
            public const int TableDrop = 11;
            public const int TableRename = 12;
            public const int TableTruncate = 13;
            public const int Dash4 = 14;
            public const int Undo = 15;
            public const int Redo = 16;
            public const int Dash5 = 17;
            public const int Cut = 18;
            public const int Copy = 19;
            public const int CopyTo = 20;
            public const int Paste = 21;
            public const int Delete = 22;
            public const int DeleteCurrentLine = 23;
            public const int Dash10 = 24;
            public const int ZoomIn = 25;
            public const int ZoomOut = 26;
            public const int Dash11 = 27;
            public const int FindAndReplace = 28;
            public const int Dash12 = 29;
            public const int SelectAll = 30;
            public const int SelectCurrentBlock = 31;
            public const int SelectCurrentLine = 32;
            public const int Dash13 = 33;
            public const int Code2SQL = 34;
            public const int SQL2Code = 35;
            public const int Dash14 = 36;
            public const int Comment = 37;
            public const int RemoveComment = 38;
            public const int Dash17 = 39;
            public const int Indent = 40;
            public const int Unindent = 41;
            public const int Dash20 = 42;
            public const int UpperCase = 43;
            public const int LowerCase = 44;
            public const int Dash23 = 45;
            public const int SQLFormatter = 46;
        }

        private static class GridColumn2
        {
            public const int ShowSqlStatement = 0;
            public const int CellViewer = 1;
            public const int SingleRecordViewer = 2;
            public const int ShowColumns = 3;
            public const int Dash0 = 4;
            public const int Dash1 = 5;
        }

        private static class GridColumn
        {
            public const int ShowSqlStatement = 0;
            public const int CellViewer = 1;
            public const int SingleRecordViewer = 2;
            public const int ShowColumns = 3;
            public const int Dash0 = 4;
            public const int SelectAll = 5;
            public const int Dash1 = 6;
            public const int ZoomInGrid = 7;
            public const int ZoomOutGrid = 8;
            public const int Dash2 = 9;
            public const int ExportAllDataToFile = 10;
            public const int ExportAllDataToFileScript = 11;
            public const int Dash3 = 12;
            public const int CopyAllDataToClipboard = 13;
            public const int CopyAllDataToClipboardCurrentRow = 14;
            public const int Dash7 = 15;
            public const int Copy = 16;
            public const int CopyAsQueryCondition = 17;
            public const int CopyWithColumnNames = 18;
            public const int CopyColumnNames = 19;
            public const int Dash8 = 20;
            public const int FreezeColumn = 21;
            public const int UnfreezeColumn = 22;
        }
    }
}