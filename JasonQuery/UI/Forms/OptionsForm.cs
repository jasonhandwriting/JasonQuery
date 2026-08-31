using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Events;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Update;
using JasonLibrary.UI.Controls;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.QueryEngine.Formatters;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VisualStyle = Crownwood.Magic.Common.VisualStyle;

namespace JasonQuery.UI.Forms
{
    public sealed partial class OptionsForm : Form
    {
        public event ValueUpdatedEventHandler ValueUpdated;
        private int _pid; //for Auto Complete, Pid
        private string _panelColorSelectedName = string.Empty;
        private List<object> _lstPanelTabColor; //Tab
        private List<object> _lstPanelQueryEditorColor; //Query Editor
        private List<object> _lstPanelGridColor; //Grid
        private List<object> _lstc1Grid;
        private List<object> _lstFindTextBox;
        private List<object> _lstFindNextButton;
        private List<object> _lstFindPreviousButton;
        private List<object> _lstFindEditor;
        private List<object> _lstFindGroup;
        private ContextMenuStrip _cMenu = new ContextMenuStrip();
        private ContextMenuStrip _gMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _editableEditorContextMenu = new ContextMenuStrip();
        private readonly ContextMenuStrip _readOnlyEditorContextMenu = new ContextMenuStrip();
        private readonly ContextMenu _nullMenu = new ContextMenu();
        private readonly ToolTip _toolTip1 = new ToolTip();
        private DataTable _dtAutoReplaceInfo;
        private DataRow _rowAutoReplaceInfo;
        private DataTable _dtVisualStyle;
        private string _languageText = string.Empty;
        private List<string> _lstGridHeaderAutoReplace = new List<string>();
        private Color _colorEditorFocused = Color.LightGoldenrodYellow;
        private Color _colorEditorUnfocused = SystemColors.Control;

        private const int BOOKMARK_MARGIN = 1; //Conventionally the symbol margin
        private const int BOOKMARK_MARKER = 3; //Arbitrary. Any valid index would work.

        private bool _isFormLoadFinished = false; //表單是否載入完畢 (避免觸發事件)
        private bool _isApplyAndClose; //使用是否按下「套用&關閉」？

        private readonly List<string> _lstMenuGrid = new List<string>();

        private bool _isCtrlKeyDown;
        private int _totalDelta;

        private enum c1GridID
        {
            //AcInfo = 0,
            //ArInfo = 1,
            VisualStyle = 0
        }

        private int _rowHeight; //original row height
        private int _recSelWidth; //orignal record selector width
        private float _fontSize; //original font size

        private readonly List<Point> _modifiedList = new List<Point>();
        private readonly SqlFormatterCoordinator _sqlFormatterCoordinator = new SqlFormatterCoordinator();
        private IReadOnlyList<SqlFormatterEngineChoice> _sqlFormatterEngineChoices = new List<SqlFormatterEngineChoice>().AsReadOnly();
        private static readonly int[] _sqlFormatterIndentSizes = { 2, 4, 8 };
        private static readonly int[] _sqlFormatterBlankLinesBetweenStatements = { 0, 1, 2, 3, 4 };
        private static readonly int[] _sqlFormatterListItemsPerLine = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private readonly ScintillaEditor[] _editableOptionEditors;
        private readonly ScintillaEditor[] _readOnlyOptionEditors;

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public OptionsForm()
        {
            InitializeComponent();

            _editableOptionEditors = new[]
            {
                editorAutoReplace,
                editorOperatorKeywords,
                editorBuiltInFunctions,
                editorBuiltInKeywords,
                editorUserDefinedKeywords,
                editorSqlToCode,
                editorSqlFormatter
            };

            _readOnlyOptionEditors = new[]
            {
                editorSqlToCodePreview,
                editorSqlFormatterPreview
            };

            InitializeOptionEditorContextMenus();

            EnableFormDoubleBuffering();
            HideMainOptionTabsDuringLoading();

            InitializeDefaultSelectedTabs();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void EnableFormDoubleBuffering()
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

        private void HideMainOptionTabsDuringLoading()
        {
            c1DockingTab.Visible = false;
            c1DockingTab4.Visible = false;
        }

        private void ShowMainOptionTabsAfterLoading()
        {
            InitializeDefaultSelectedTabs();

            c1DockingTab4.Visible = true;
            c1DockingTab4.Invalidate(true);
            c1DockingTab4.Update();

            c1DockingTab.Visible = true;
            c1DockingTab.Invalidate(true);
            c1DockingTab.Update();
        }

        private void InitializeDefaultSelectedTabs()
        {
            c1DockingTab.SuspendLayout();
            c1DockingTab4.SuspendLayout();

            try
            {
                c1DockingTab.SelectedTab = tabGlobal;
                c1DockingTab4.SelectedTab = tabGlobalSettings;
            }
            finally
            {
                c1DockingTab4.ResumeLayout(false);
                c1DockingTab.ResumeLayout(false);
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            var isLayoutSuspended = false;

            try
            {
                SuspendLayout();
                isLayoutSuspended = true;

                ApplyLocalizationSetting();

                //20250214 路徑鎖定，不讓使用者變更
                btnBrowseBackupPath.Enabled = false;
                btnClear3.Enabled = false;

                GridHelper.SetGridVisualStyle(c1GridAutoReplaceInfo, 10);
                GridHelper.SetGridVisualStyle(c1GridVisualStyle, 10);

                txtSpecifiedSQLFile1.Text = MyGlobal.SpecifiedSqlFile1;
                txtSpecifiedSQLFile2.Text = MyGlobal.SpecifiedSqlFile2;

                chkAskMeBeforeOpenUnsavedFiles.Checked = AppConfigHelper.AskBeforeOpenUnsavedFiles;

                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'EditorConfig'");
                sbSql.Append("   AND AttributeName = 'SortByColumnName'");

                var sql = sbSql.ToString();
                var dtTemp = JasonQueryRepository.ExecQuery(sql);
                var isChecked = dtTemp.Rows.Count > 0 && dtTemp.Rows[0]["AttributeValue"].ToString() == "1";

                chkSortByColumnName.Checked = isChecked; //重啟後才會生效，故此處要直接撈取 DB 的值

                //20250205 直接設定為 true，且使用者不可變更 (因為 0.90 版本開始，啟動速度加快了，故此處改為一律顯示)
                chkShowColumnInfo.Checked = true; //重啟後才會生效，故此處要直接撈取 DB 的值
                chkShowColumnInfo.Enabled = false;

                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'EditorConfig'");
                sbSql.Append("   AND AttributeName = 'DefaultTabSchemaBrowser'");

                sql = sbSql.ToString();
                dtTemp = JasonQueryRepository.ExecQuery(sql);

                isChecked = dtTemp.Rows.Count > 0 && dtTemp.Rows[0]["AttributeValue"].ToString() == "1";
                chkDefaultTabSchemaInformation.Checked = isChecked; //重啟後才會生效，故此處要直接撈取 DB 的值

                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine("   AND MPID = 0");
                sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                sbSql.Append("   AND AttributeName = 'BackupFile'");

                sql = sbSql.ToString(); //20250830 BackupFile 屬於 Global 性質，MPid = "0"
                dtTemp = JasonQueryRepository.ExecQuery(sql);

                isChecked = dtTemp.Rows.Count == 0 || dtTemp.Rows[0]["AttributeValue"].ToString() != "0";
                chkEnableBackup.Checked = isChecked; //重啟後才會生效，故此處要直接撈取 DB 的值
                chkAskMeBeforeOpenUnsavedFiles.Checked = AppConfigHelper.AskBeforeOpenUnsavedFiles;
                txtBackupPath.Text = AppConfigHelper.BackupPath;
                chkAutoListMembers.Checked = MyGlobal.IsAutoListMembers;
                chkSavePoint.Checked = MyGlobal.ShouldSavePoint;

                if (DatabaseSqlExecutor.CurrentDataSource != DataSourceType.PostgreSql)
                {
                    chkSavePoint.Enabled = false;
                }

                #region Copy Settings... 收集下拉清單
                btnCopySettings.Visible = false;
                btnCopySettings.Items.Clear();

                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM DBInfo");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.Append($"   AND PID <> {JasonQueryRepository.DbMotherPid}");

                sql = sbSql.ToString();
                dtTemp = JasonQueryRepository.ExecQuery(sql);

                if (dtTemp?.Rows.Count > 0)
                {
                    btnCopySettings.Visible = true;

                    var sFrom = LocalizationHelper.GetLanguageString("From", "form", GetType().Name, "msg", "From", "Text");
                    var sTo = LocalizationHelper.GetLanguageString("To", "form", GetType().Name, "msg", "To", "Text");

                    for (var i = 0; i < dtTemp.Rows.Count; i++)
                    {
                        var dr = dtTemp.Rows[i];
                        var sDataSource2 = dr.GetSafeString("DataSource");
                        var sConnectionName2 = dr.GetSafeString("ConnectionName");
                        var dropDownItem = new C1.Win.C1Input.DropDownItem();

                        btnCopySettings.Items.Add(dropDownItem);
                        btnCopySettings.Items[i].Tag = dr.GetSafeString("PID");
                        btnCopySettings.Items[i].Text = $"{sFrom} <{sDataSource2}, {sConnectionName2}>  {sTo} <{DatabaseSqlExecutor.DataSourceDisplayName}, {DatabaseSqlExecutor.DbConnectionName}>";

                        if (i == 0) //只註冊一次即可！
                        {
                            btnCopySettings.DropDownItemClicked += btnCopySettings_DropDownItemClicked;
                        }
                    }
                }
                #endregion

                if (MyLibrary.IsDarkMode)
                {
                    C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";
                    c1ThemeController1.SetTheme(c1GridAutoReplaceInfo, "VS2013Dark");
                    GridHelper.SetGridVisualStyle(c1GridAutoReplaceInfo, 10);
                    c1GridAutoReplaceInfo.BackColor = ColorTranslator.FromHtml("#2D2D30");
                    c1ThemeController1.SetTheme(c1GridVisualStyle, "VS2013Dark");
                    GridHelper.SetGridVisualStyle(c1GridVisualStyle, 10);
                    c1GridVisualStyle.BackColor = ColorTranslator.FromHtml("#2D2D30");

                    ApplyDarkStyler();

                    c1GridVisualStyle.BorderColor = Color.White;
                    c1GridVisualStyle.HeadingStyle.Borders.Color = Color.White;
                    c1GridVisualStyle.HeadingStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHeadingForeColor);
                    c1GridVisualStyle.RowDivider.Color = Color.White;
                    c1GridVisualStyle.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
                    c1GridVisualStyle.HeadingStyle.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
                    c1GridVisualStyle.MarqueeStyle = MarqueeEnum.HighlightCell;

                    MyLibrary.GridVisualStyle = "Office 2010 Black";
                    cboGridVisualStyle.Text = "Office 2010 Black";
                    cboGridVisualStyle.Enabled = false;

                    _colorEditorUnfocused = ColorTranslator.FromHtml("#2D2D30");
                }

                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);
                cboLocalization.Text = LocalizationHelper.Localization;
                cboLocalization.Tag = LocalizationHelper.Localization;

                cboDateFormat.Text = MyLibrary.DateFormat;
                chkShowDatabaseName.Checked = MyLibrary.ShowDatabaseName;
                chkShowVersion.Checked = MyLibrary.ShowVersion;
                chkShowIP.Checked = MyLibrary.ShowIP;

                //記住原值，如果變更語系時，才能正確處理
                MyGlobal.OptionsTabName_Before = MyGlobal.OptionsTabName;
                MyGlobal.SchemaBrowserTabName_Before = MyGlobal.SchemaBrowserTabName;
                MyGlobal.SqlHistoryTabName_Before = MyGlobal.SqlHistoryTabName;
                MyGlobal.CreateTableTabName_Before = MyGlobal.CreateTableTabName; //20241017

                editor.Tag = string.Empty; //multi selection 時判斷用的

                editorSqlToCodePreview.Tag = editor.Text;
                editorSqlToCode.Text = editor.Text;
                var formatterProviderKind = DataSourceTypeMapper.ToDatabaseProviderKind(_currentSourceType);

                editorSqlFormatter.Text = SqlFormatterPreviewSqlCatalog.Get(formatterProviderKind);

                _rowHeight = c1GridVisualStyle.RowHeight;
                _recSelWidth = c1GridVisualStyle.RecordSelectorWidth;
                _fontSize = c1GridVisualStyle.Styles["Normal"].Font.Size;

                _toolTip1.ForeColor = Color.Blue;
                _toolTip1.BackColor = Color.Gray;

                _toolTip1.UseAnimation = true;
                _toolTip1.AutoPopDelay = 5000;
                _toolTip1.InitialDelay = 50;
                _toolTip1.ReshowDelay = 30;

                Cursor = Cursors.WaitCursor;

                grpBuiltInFunctions.Text += $" ({DatabaseSqlExecutor.DataSourceDisplayName})";
                grpBuiltInKeywords.Text += $" ({DatabaseSqlExecutor.DataSourceDisplayName})";

                _toolTip1.SetToolTip(nudMinFragmentLength, "2 ~ 9");
                _toolTip1.SetToolTip(txtRecentFiles, "10 ~ 60");
                _toolTip1.SetToolTip(txtMyFavorite, "10 ~ 60");

                nudMinFragmentLength.ContextMenu = _nullMenu;
                txtRecentFiles.ContextMenu = _nullMenu;
                txtMyFavorite.ContextMenu = _nullMenu;

                _lstPanelTabColor = new List<object>
                {
                    pnlOptionsTabActiveForeColor,
                    pnlOptionsTabActiveBackColor,
                    pnlOptionsTabInactiveForeColor
                };

                _lstPanelQueryEditorColor = new List<object>
                {
                    pnlToolstripBackground,
                    pnlEditorBackground,
                    pnlCurrentLineBackground,
                    pnlSelectedTextBackground,
                    pnlErrorLineBackground,
                    pnlBookmarkBackground,
                    pnlComments,
                    pnlIdentifier,
                    pnlNumber,
                    pnlOperatorSymbol,
                    pnlOperatorKeywords,
                    pnlString,
                    pnlCharacter,
                    pnlBuiltInFunctions,
                    pnlBuiltInKeywords,
                    pnlUserDefinedKeywords,
                    pnlWhiteSpace,
                    pnlUserTables,
                    pnlUserFunctions,
                    pnlHighlightForeColor
                };

                _lstPanelGridColor = new List<object>
                {
                    pnlNullValueForeColor,
                    pnlGridHeadingForeColor,
                    pnlGridEvenRowForeColor,
                    pnlGridEvenRowBackColor,
                    pnlGridOddRowForeColor,
                    pnlGridOddRowBackColor,
                    pnlGridHighlightForeColor,
                    pnlGridHighlightBackColor,
                    pnlGridSelectedForeColor,
                    pnlGridSelectedBackColor
                };

                _lstc1Grid = new List<object>
                {
                    c1GridAutoReplaceInfo,
                    c1GridVisualStyle
                };

                _lstFindTextBox = new List<object>
                {
                    txtFindOperatorKeywords,
                    txtFindBuiltInFunctions,
                    txtFindBuiltInKeywords,
                    txtFindUserDefinedKeywords
                };

                _lstFindNextButton = new List<object>
                {
                    btnNextOperatorKeywords,
                    btnNextBuiltInFunctions,
                    btnNextBuiltInKeywords,
                    btnNextUserDefinedKeywords
                };

                _lstFindPreviousButton = new List<object>
                {
                    btnPreviousOperatorKeywords,
                    btnPreviousBuiltInFunctions,
                    btnPreviousBuiltInKeywords,
                    btnPreviousUserDefinedKeywords
                };

                _lstFindEditor = new List<object>
                {
                    editorOperatorKeywords,
                    editorBuiltInFunctions,
                    editorBuiltInKeywords,
                    editorUserDefinedKeywords
                };

                _lstFindGroup = new List<object>
                {
                    grpFindOperatorKeywords,
                    grpFindBuiltInFunctions,
                    grpFindBuiltInKeywords,
                    grpFindUserDefinedKeywords
                };

                c1GridVisualStyle.MouseWheel += c1GridVisualStyle_MouseWheel;

                c1GridVisualStyle.KeyDown += Detect_KeyDown;
                c1GridVisualStyle.KeyUp += Detect_KeyUp;

                //Query Editor Color 套用
                #region Query Editor Color 套用，並將 Color 的值存入 Tag
                pnlToolstripBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                pnlToolstripBackground.Tag = MyLibrary.ColorToolstripBackground;
                lblEditorBackground.Tag = MyLibrary.ColorToolstripBackground; //當使用者按下 Close 時，用 .Tag 的顏色來還原
                _toolTip1.SetToolTip(pnlToolstripBackground, $"{MyLibrary.ColorToolstripBackground} {GetRgbColorCode(MyLibrary.ColorToolstripBackground)}");

                _colorEditorFocused = pnlToolstripBackground.BackColor;

                pnlEditorBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
                pnlEditorBackground.Tag = MyLibrary.ColorEditorBackground;
                lblEditorBackground.Tag = MyLibrary.ColorEditorBackground;
                _toolTip1.SetToolTip(pnlEditorBackground, $"{MyLibrary.ColorEditorBackground} {GetRgbColorCode(MyLibrary.ColorEditorBackground)}");

                pnlCurrentLineBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                pnlCurrentLineBackground.Tag = MyLibrary.ColorCurrentLineBackground;
                lblCurrentLineBackground.Tag = MyLibrary.ColorCurrentLineBackground;
                _toolTip1.SetToolTip(pnlCurrentLineBackground, $"{MyLibrary.ColorCurrentLineBackground} {GetRgbColorCode(MyLibrary.ColorCurrentLineBackground)}");

                pnlSelectedTextBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground);
                pnlSelectedTextBackground.Tag = MyLibrary.ColorSelectedTextBackground;
                lblSelectedTextBackground.Tag = MyLibrary.ColorSelectedTextBackground;
                _toolTip1.SetToolTip(pnlSelectedTextBackground, $"{MyLibrary.ColorSelectedTextBackground} {GetRgbColorCode(MyLibrary.ColorSelectedTextBackground)}");

                pnlErrorLineBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorErrorLineBackground);
                pnlErrorLineBackground.Tag = MyLibrary.ColorErrorLineBackground;
                lblErrorLineBackground.Tag = MyLibrary.ColorErrorLineBackground;
                _toolTip1.SetToolTip(pnlErrorLineBackground, $"{MyLibrary.ColorErrorLineBackground} {GetRgbColorCode(MyLibrary.ColorErrorLineBackground)}");

                pnlBookmarkBackground.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorBookmarkBackground);
                pnlBookmarkBackground.Tag = MyLibrary.ColorBookmarkBackground;
                lblBookmarkBackground.Tag = MyLibrary.ColorBookmarkBackground;
                _toolTip1.SetToolTip(pnlBookmarkBackground, $"{MyLibrary.ColorBookmarkBackground} {GetRgbColorCode(MyLibrary.ColorBookmarkBackground)}");

                pnlComments.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorComments);
                pnlComments.Tag = MyLibrary.ColorComments;
                lblComments.Tag = MyLibrary.ColorComments;
                _toolTip1.SetToolTip(pnlComments, $"{MyLibrary.ColorComments} {GetRgbColorCode(MyLibrary.ColorComments)}");

                pnlIdentifier.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorTextIdentifier);
                pnlIdentifier.Tag = MyLibrary.ColorTextIdentifier;
                lblIdentifier.Tag = MyLibrary.ColorTextIdentifier;
                _toolTip1.SetToolTip(pnlIdentifier, $"{MyLibrary.ColorTextIdentifier} {GetRgbColorCode(MyLibrary.ColorTextIdentifier)}");

                pnlBuiltInKeywords.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorBuiltInKeywords);
                pnlBuiltInKeywords.Tag = MyLibrary.ColorBuiltInKeywords;
                lblBuiltInKeywords.Tag = MyLibrary.ColorBuiltInKeywords;
                _toolTip1.SetToolTip(pnlBuiltInKeywords, $"{MyLibrary.ColorBuiltInKeywords} {GetRgbColorCode(MyLibrary.ColorBuiltInKeywords)}");

                pnlUserDefinedKeywords.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorUserDefinedKeywords);
                pnlUserDefinedKeywords.Tag = MyLibrary.ColorUserDefinedKeywords;
                lblUserDefinedKeywords.Tag = MyLibrary.ColorUserDefinedKeywords;
                _toolTip1.SetToolTip(pnlUserDefinedKeywords, $"{MyLibrary.ColorUserDefinedKeywords} {GetRgbColorCode(MyLibrary.ColorUserDefinedKeywords)}");

                pnlNumber.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorNumber);
                pnlNumber.Tag = MyLibrary.ColorNumber;
                lblNumber.Tag = MyLibrary.ColorNumber;
                _toolTip1.SetToolTip(pnlNumber, $"{MyLibrary.ColorNumber} {GetRgbColorCode(MyLibrary.ColorNumber)}");

                pnlOperatorSymbol.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorOperatorSymbol);
                pnlOperatorSymbol.Tag = MyLibrary.ColorOperatorSymbol;
                lblOperatorSymbol.Tag = MyLibrary.ColorOperatorSymbol;
                _toolTip1.SetToolTip(pnlOperatorSymbol, $"{MyLibrary.ColorOperatorSymbol} {GetRgbColorCode(MyLibrary.ColorOperatorSymbol)}");

                pnlOperatorKeywords.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorOperatorKeywords);
                pnlOperatorKeywords.Tag = MyLibrary.ColorOperatorKeywords;
                lblOperatorKeywords.Tag = MyLibrary.ColorOperatorKeywords;
                _toolTip1.SetToolTip(pnlOperatorKeywords, $"{MyLibrary.ColorOperatorKeywords} {GetRgbColorCode(MyLibrary.ColorOperatorKeywords)}");

                pnlString.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorString);
                pnlString.Tag = MyLibrary.ColorString;
                lblString.Tag = MyLibrary.ColorString;
                _toolTip1.SetToolTip(pnlString, $"{MyLibrary.ColorString} {GetRgbColorCode(MyLibrary.ColorString)}");

                pnlCharacter.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorCharacter);
                pnlCharacter.Tag = MyLibrary.ColorCharacter;
                lblCharacter.Tag = MyLibrary.ColorCharacter;
                _toolTip1.SetToolTip(pnlCharacter, $"{MyLibrary.ColorCharacter} {GetRgbColorCode(MyLibrary.ColorCharacter)}");

                pnlBuiltInFunctions.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorBuiltInFunctions);
                pnlBuiltInFunctions.Tag = MyLibrary.ColorBuiltInFunctions;
                lblBuiltInFunctions.Tag = MyLibrary.ColorBuiltInFunctions;
                _toolTip1.SetToolTip(pnlBuiltInFunctions, $"{MyLibrary.ColorBuiltInFunctions} {GetRgbColorCode(MyLibrary.ColorBuiltInFunctions)}");

                pnlWhiteSpace.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace);
                pnlWhiteSpace.Tag = MyLibrary.ColorWhiteSpace;
                lblWhiteSpace.Tag = MyLibrary.ColorWhiteSpace;
                _toolTip1.SetToolTip(pnlWhiteSpace, $"{MyLibrary.ColorWhiteSpace} {GetRgbColorCode(MyLibrary.ColorWhiteSpace)}");

                pnlUserFunctions.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorUserDefinedFunctionsTriggers);
                pnlUserFunctions.Tag = MyLibrary.ColorUserDefinedFunctionsTriggers;
                lblUserFunctions.Tag = MyLibrary.ColorUserDefinedFunctionsTriggers;
                _toolTip1.SetToolTip(pnlUserFunctions, $"{MyLibrary.ColorUserDefinedFunctionsTriggers} {GetRgbColorCode(MyLibrary.ColorUserDefinedFunctionsTriggers)}");

                pnlUserTables.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorUserDefinedTablesViews);
                pnlUserTables.Tag = MyLibrary.ColorUserDefinedTablesViews;
                lblUserTables.Tag = MyLibrary.ColorUserDefinedTablesViews;
                _toolTip1.SetToolTip(pnlUserTables, $"{MyLibrary.ColorUserDefinedTablesViews} {GetRgbColorCode(MyLibrary.ColorUserDefinedTablesViews)}");

                //Query Editor 頁籤：Highlight
                pnlHighlightForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.HighlightColorForeColor);
                pnlHighlightForeColor.Tag = MyLibrary.HighlightColorForeColor;
                lblHighlightColorForeColor.Tag = MyLibrary.HighlightColorForeColor;
                _toolTip1.SetToolTip(pnlHighlightForeColor, $"{MyLibrary.HighlightColorForeColor} {GetRgbColorCode(MyLibrary.HighlightColorForeColor)}");

                UIHelper.SelectC1ComboBoxItemByText(cboHighlightStyle, MyLibrary.HighlightColorStyle);
                lblHighlightColorStyle.Tag = MyLibrary.HighlightColorStyle;
                UIHelper.SelectC1ComboBoxItemByText(cboHighlightOutlineAlpha, MyLibrary.HighlightColorOutlineAlpha);
                lblHighlightColorOutlineAlpha.Tag = MyLibrary.HighlightColorOutlineAlpha;
                UIHelper.SelectC1ComboBoxItemByText(cboHighlightAlpha, MyLibrary.HighlightColorAlpha);
                lblHighlightColorAlpha.Tag = MyLibrary.HighlightColorAlpha;

                //Query Editor 頁籤：Preferences
                cboEditorFontPicker.Value = MyLibrary.QueryEditorFontName; //指定的字型若之後被使用者移除了，也不會出現錯誤

                lblEditorFontName.Tag = MyLibrary.QueryEditorFontName;

                UIHelper.SelectC1ComboBoxItemByText(cboEditorFontSize, MyLibrary.QueryEditorFontSizeText);
                lblEditorFontSize.Tag = MyLibrary.QueryEditorFontSizeText;

                UIHelper.SelectC1ComboBoxItemByText(cboEditorZoom, MyLibrary.QueryEditorZoomText);
                lblEditorZoom.Tag = MyLibrary.QueryEditorZoomText;

                chkWordWrap.Checked = MyLibrary.WordWrap;
                chkWordWrap.Tag = chkWordWrap.Checked ? "1" : "0";
                btnWordWrap.Visible = !chkWordWrap.Checked;
                btnWordWrap2.Visible = chkWordWrap.Checked;

                chkStart.Checked = MyLibrary.WordWrapVisualFlags_Start;
                chkStart.Tag = chkStart.Checked ? "1" : "0";
                chkEnd.Checked = MyLibrary.WordWrapVisualFlags_End;
                chkEnd.Tag = chkEnd.Checked ? "1" : "0";
                chkMargin.Checked = MyLibrary.WordWrapVisualFlags_Margin;
                chkMargin.Tag = chkMargin.Checked ? "1" : "0";
                cboIndentMode.Text = MyGlobal.WordWrapIndentMode;
                cboIndentMode.Tag = MyGlobal.WordWrapIndentMode;

                chkBold.Checked = MyLibrary.KeywordFontBold;
                chkBold.Tag = chkBold.Checked ? "1" : "0";

                chkCopyAsHTML.Checked = MyLibrary.CopyAsHTML;
                chkCopyAsHTML.Tag = chkCopyAsHTML.Checked ? "1" : "0";

                chkShowAllCharacters.Checked = MyLibrary.ShowAllCharacters;
                chkShowAllCharacters.Tag = chkShowAllCharacters.Checked ? "1" : "0";
                btnShowAllCharacters.Visible = !chkShowAllCharacters.Checked;
                btnShowAllCharacters2.Visible = chkShowAllCharacters.Checked;

                chkShowSaveAsButton.Checked = true; //20260504 改為一律顯示，並且使用者不可變更
                chkShowSaveAsButton.Tag = chkShowSaveAsButton.Checked ? "1" : "0";
                btnSaveAs.Visible = chkShowSaveAsButton.Checked;

                chkShowIndentGuide.Checked = MyLibrary.ShowIndentGuide;
                chkShowIndentGuide.Tag = chkShowIndentGuide.Checked ? "1" : "0";
                btnShowIndentGuide.Visible = !chkShowIndentGuide.Checked;
                btnShowIndentGuide2.Visible = chkShowIndentGuide.Checked;

                chkEntireBlankRowAsEmptyRow4SelectBlock.Enabled = false;
                chkEntireBlankRowAsEmptyRow4SelectBlock.Checked = MyLibrary.EntireBlankRowAsEmptyRow;
                chkEntireBlankRowAsEmptyRow4SelectBlock.Tag = chkEntireBlankRowAsEmptyRow4SelectBlock.Checked ? "1" : "0";
                chkHighlightSelection.Checked = MyLibrary.HighlightSelection;
                chkHighlightSelection.Tag = chkHighlightSelection.Checked ? "1" : "0";
                #endregion

                editor.ViewEol = MyLibrary.ShowAllCharacters;
                editor.ViewWhitespace = MyLibrary.ShowAllCharacters ? ScintillaNET.WhitespaceMode.VisibleAlways : ScintillaNET.WhitespaceMode.Invisible;
                editor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editor.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                editor.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                editor.WrapMode = MyLibrary.WordWrap ? ScintillaNET.WrapMode.Word : ScintillaNET.WrapMode.None;
                editor.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

                editorAutoReplace.ViewEol = MyLibrary.ShowAllCharacters;
                editorAutoReplace.ViewWhitespace = MyLibrary.ShowAllCharacters ? ScintillaNET.WhitespaceMode.VisibleAlways : ScintillaNET.WhitespaceMode.Invisible;
                editorAutoReplace.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorAutoReplace.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
                editorAutoReplace.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                editorAutoReplace.WrapMode = MyLibrary.WordWrap ? ScintillaNET.WrapMode.Word : ScintillaNET.WrapMode.None;
                editorAutoReplace.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);

                editorSqlToCode.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorSqlToCode.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));

                editorSqlFormatter.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorSqlFormatter.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));

                editorSqlFormatterPreview.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorSqlFormatterPreview.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));

                //載入 Auto Complete 設定
                #region 載入 Auto Complete 設定
                chkEnableAutoComplete.Checked = MyLibrary.EnableAutoComplete;
                nudMinFragmentLength.Value = MyLibrary.AutoCompleteMinFragmentLength;
                chkFirstCharChecking.Value = MyLibrary.AutoCompleteFirstCharChecking;
                chkBuiltInKeywords.Checked = MyLibrary.AutoCompleteBuiltInKeywords;
                chkBuiltInFunctions.Checked = MyLibrary.AutoCompleteBuiltInFunctions;
                chkUserDefinedKeywords.Checked = MyLibrary.AutoCompleteUserDefinedKeywords;
                chkUserDefinedFunctions.Checked = MyLibrary.AutoCompleteUserDefinedFunctions;
                chkUserDefinedTables.Checked = MyLibrary.AutoCompleteUserDefinedTables;
                chkUserDefinedTriggers.Checked = MyLibrary.AutoCompleteUserDefinedTriggers;
                chkUserDefinedViews.Checked = MyLibrary.AutoCompleteUserDefinedViews;
                #endregion

                //載入 Auto Replace 設定
                #region 載入 Auto Replace 設定
                chkEnableAutoReplace.Checked = MyLibrary.EnableAutoReplace;
                #endregion

                //載入 Data Grid 設定
                #region 載入 Data Grid 設定
                cboResultCopyQuotingWith.Text = MyLibrary.GridQuotationMarks;
                cboResultCopyFieldSeparator.Text = MyLibrary.GridFieldSeparator;
                rdoMaximized.Checked = AppConfigHelper.IsMainFormMaximized;
                rdoNormal.Checked = !AppConfigHelper.IsMainFormMaximized;

                chkShowColumnType.Checked = MyLibrary.GridShowColumnDataType;

                //////c1DockingTab.SelectedTab = tabDataGrid;
                chkShowFilterRow.Checked = MyLibrary.GridShowFilterRow;
                //////c1DockingTab.SelectedTab = tabGlobal;

                chkShowGroupingRow.Checked = MyLibrary.GridShowGroupingRow;
                chkResize.Checked = MyLibrary.GridResize;
                lblMaxWidth.Enabled = chkResize.Checked;
                cboMaxWidth.Enabled = chkResize.Checked;

                pnlNullValueForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor);
                pnlNullValueForeColor.Tag = MyLibrary.GridNullShowColor;
                lblNullValueForeColor.Tag = MyLibrary.GridNullShowColor;
                _toolTip1.SetToolTip(pnlNullValueForeColor, $"{MyLibrary.GridNullShowColor} {GetRgbColorCode(MyLibrary.GridNullShowColor)}");

                chkShowColumnComment.Checked = MyLibrary.GridShowColumnComment;
                chkRawDataMode.Checked = MyLibrary.GridRawDataMode;

                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'GridConfig'");
                sbSql.Append("   AND AttributeName = 'NullShowAs'");

                sql = sbSql.ToString();
                dtTemp = JasonQueryRepository.ExecQuery(sql);

                if (dtTemp == null || dtTemp.Rows.Count == 0)
                {
                    cboNullShowAs.Text = MyLibrary.GridNullShowAs;
                }
                else
                {
                    cboNullShowAs.Text = dtTemp.Rows[0]["AttributeValue"].ToString(); //20260505 改為重啟後才會生效，故此處要直接撈取 DB 的值
                }

                chkPagedQuery.Checked = MyLibrary.GridPagingQuery;
                cboRowsPerPage.Text = MyLibrary.GridRowsPerPage;
                chkAppendQueryResult.Checked = MyLibrary.GridAppendingQueries;

                chkSetFocusAfterQuery.Checked = MyLibrary.GridSetFocusAfterQuery;

                cboGridVisualStyle.Text = MyLibrary.GridVisualStyle;
                cboGridFontPicker.Value = MyLibrary.GridFontName;
                cboGridFontSize.Text = MyLibrary.GridFontSizeText;

                pnlGridHeadingForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridHeadingForeColor);
                pnlGridHeadingForeColor.Tag = MyLibrary.GridHeadingForeColor;
                _toolTip1.SetToolTip(pnlGridHeadingForeColor, $"{MyLibrary.GridHeadingForeColor} {GetRgbColorCode(MyLibrary.GridHeadingForeColor)}");

                pnlGridEvenRowForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
                pnlGridEvenRowForeColor.Tag = MyLibrary.GridEvenRowForeColor;
                _toolTip1.SetToolTip(pnlGridEvenRowForeColor, $"{MyLibrary.GridEvenRowForeColor} {GetRgbColorCode(MyLibrary.GridEvenRowForeColor)}");

                pnlGridEvenRowBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                pnlGridEvenRowBackColor.Tag = MyLibrary.GridEvenRowBackColor;
                _toolTip1.SetToolTip(pnlGridEvenRowBackColor, $"{MyLibrary.GridEvenRowBackColor} {GetRgbColorCode(MyLibrary.GridEvenRowBackColor)}");

                pnlGridOddRowForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
                pnlGridOddRowForeColor.Tag = MyLibrary.GridOddRowForeColor;
                _toolTip1.SetToolTip(pnlGridOddRowForeColor, $"{MyLibrary.GridOddRowForeColor} {GetRgbColorCode(MyLibrary.GridOddRowForeColor)}");

                pnlGridOddRowBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
                pnlGridOddRowBackColor.Tag = MyLibrary.GridOddRowBackColor;
                _toolTip1.SetToolTip(pnlGridOddRowBackColor, $"{MyLibrary.GridOddRowBackColor} {GetRgbColorCode(MyLibrary.GridOddRowBackColor)}");

                AlternatingRowColorSetting();

                pnlGridHighlightForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightForeColor);
                pnlGridHighlightForeColor.Tag = MyLibrary.GridHighlightForeColor;
                _toolTip1.SetToolTip(pnlGridHighlightForeColor, $"{MyLibrary.GridHighlightForeColor} {GetRgbColorCode(MyLibrary.GridHighlightForeColor)}");

                pnlGridHighlightBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridHighlightBackColor);
                pnlGridHighlightBackColor.Tag = MyLibrary.GridHighlightBackColor;
                _toolTip1.SetToolTip(pnlGridHighlightBackColor, $"{MyLibrary.GridHighlightBackColor} {GetRgbColorCode(MyLibrary.GridHighlightBackColor)}");

                pnlGridSelectedForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
                pnlGridSelectedForeColor.Tag = MyLibrary.GridSelectedForeColor;
                _toolTip1.SetToolTip(pnlGridSelectedForeColor, $"{MyLibrary.GridSelectedForeColor} {GetRgbColorCode(MyLibrary.GridSelectedForeColor)}");

                pnlGridSelectedBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
                pnlGridSelectedBackColor.Tag = MyLibrary.GridSelectedBackColor;
                _toolTip1.SetToolTip(pnlGridSelectedBackColor, $"{MyLibrary.GridSelectedBackColor} {GetRgbColorCode(MyLibrary.GridSelectedBackColor)}");

                //20260202 Store display strategy settings for large text fields
                LoadLargeTextPreviewLengthSettings();
                #endregion

                //載入 Keywords 設定
                #region 載入 Keywords 設定
                editorOperatorKeywords.Text = MyLibrary.KeywordsOperatorKeywords;
                editorOperatorKeywords.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                editorBuiltInFunctions.Text = MyLibrary.KeywordsBuiltInFunctions;
                editorBuiltInFunctions.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                editorBuiltInKeywords.Text = MyLibrary.KeywordsBuiltInKeywords;
                editorBuiltInKeywords.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                editorUserDefinedKeywords.Text = MyLibrary.KeywordsUserDefinedKeywords;
                editorUserDefinedKeywords.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
                #endregion

                //載入 SQL To Code 設定
                #region 載入 SQL To Code 設定
                txtSqlVariableName.Text = MyLibrary.SqlToCodeSqlVariableName;
                txtSqlVariableName.Tag = MyLibrary.SqlToCodeSqlVariableName;
                txtStringBuilderVariableName.Text = MyLibrary.SqlToCodeStringBuilderVariableName;
                txtStringBuilderVariableName.Tag = MyLibrary.SqlToCodeStringBuilderVariableName;
                #endregion

                //載入 SQL Formatter 設定
                #region 載入 SQL Formatter 設定
                LoadSqlFormatterLayoutChoices();
                txtMaxWidth.Text = MyLibrary.SqlFormatterMaxLineWidth.ToString();
                chkConvertCaseForKeywords.Checked = MyLibrary.SqlFormatterConvertCaseForKeywords;

                switch (MyLibrary.SqlFormatterConvertCaseForKeywordsCase)
                {
                    case 2:
                        {
                            rdoLowerCase.Checked = true;
                            break;
                        }
                    default:
                        {
                            rdoUpperCase.Checked = true;
                            break;
                        }
                }

                LoadSqlFormatterEngineChoices();
                #endregion

                //載入 Global 設定
                #region 載入 Global 設定
                rdoCheckOnly.Checked = MyLibrary.CheckForUpdate;

                switch (MyLibrary.CheckForUpdateValue)
                {
                    case 0:
                        {
                            rdoCheckForUpdates0.Checked = true;
                            break;
                        }
                    case 1:
                        {
                            rdoCheckForUpdates1.Checked = true;
                            break;
                        }
                    default:
                        {
                            rdoCheckForUpdates7.Checked = true;
                            break;
                        }
                }

                LoadUpdateMetadataSourceSettings();

                chkDarkMode.Checked = MyLibrary.IsDarkMode;
                chkDarkMode.Tag = MyLibrary.IsDarkMode ? "1" : "0";

                if (AppConfigHelper.IsChangeColorThemeNeedRestart)
                {
                    chkDarkMode.Enabled = false; //避免連續切換色彩佈景主題
                    btnHelp_DarkMode.Visible = true;
                }

                if (MyLibrary.TabStyle == "IDE")
                {
                    rdoIDE.Checked = true;
                }
                else
                {
                    rdoPlain.Checked = true;
                }

                switch (MyLibrary.TabAppearance)
                {
                    case "MultiDocument":
                        {
                            rdoMultiDocument.Checked = true;
                            break;
                        }
                    case "MultiForm":
                        {
                            rdoMultiForm.Checked = true;
                            break;
                        }
                    default:
                        {
                            rdoMultiBox.Checked = true;
                            break;
                        }
                }

                chkTabBold.Checked = MyGlobal.IsTabBold;
                chkShrinkPages.Checked = MyGlobal.IsTabShrinkPages;
                chkShowArrows.Checked = MyGlobal.IsTabShowArrows;
                chkHoverSelect.Checked = MyGlobal.IsTabHoverSelect;
                chkMultiLine.Checked = MyGlobal.IsTabMultiLine;
                chkPendingWarning.Checked = MyGlobal.IsPendingTransactionWarning;

                txtRecentFiles.Text = MyLibrary.RecentFilesQty.ToString();
                txtMyFavorite.Text = MyLibrary.MyFavoriteQty.ToString();

                rdoCommitRollbackIconStyle1.Checked = MyGlobal.CommitRollbackIcon == 1;
                rdoCommitRollbackIconStyle2.Checked = MyGlobal.CommitRollbackIcon == 2;
                rdoCommitRollbackIconStyle3.Checked = MyGlobal.CommitRollbackIcon == 3;
                rdoCommitRollbackIconStyle4.Checked = MyGlobal.CommitRollbackIcon == 4;
                rdoCommitRollbackIconStyle5.Checked = MyGlobal.CommitRollbackIcon == 5;
                rdoCommitRollbackIconStyle6.Checked = MyGlobal.CommitRollbackIcon == 6;

                pnlOptionsTabActiveForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor);
                pnlOptionsTabActiveForeColor.Tag = MyLibrary.ColorOptionsTabActiveForeColor;
                lblOptionsTabActiveForeColor.Tag = MyLibrary.ColorOptionsTabActiveForeColor;
                _toolTip1.SetToolTip(pnlOptionsTabActiveForeColor, $"{MyLibrary.ColorOptionsTabActiveForeColor} {GetRgbColorCode(MyLibrary.ColorOptionsTabActiveForeColor)}");

                pnlOptionsTabActiveBackColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor);
                pnlOptionsTabActiveBackColor.Tag = MyLibrary.ColorOptionsTabActiveBackColor;
                lblOptionsTabActiveBackColor.Tag = MyLibrary.ColorOptionsTabActiveBackColor;
                _toolTip1.SetToolTip(pnlOptionsTabActiveBackColor, $"{MyLibrary.ColorOptionsTabActiveBackColor} {GetRgbColorCode(MyLibrary.ColorOptionsTabActiveBackColor)}");

                pnlOptionsTabInactiveForeColor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor);
                pnlOptionsTabInactiveForeColor.Tag = MyLibrary.ColorOptionsTabInactiveForeColor;
                lblOptionsTabInactiveForeColor.Tag = MyLibrary.ColorOptionsTabInactiveForeColor;
                _toolTip1.SetToolTip(pnlOptionsTabInactiveForeColor, $"{MyLibrary.ColorOptionsTabInactiveForeColor} {GetRgbColorCode(MyLibrary.ColorOptionsTabInactiveForeColor)}");

                UIHelper.SetDockingTabColor(c1DockingTab, pnlOptionsTabActiveBackColor.BackColor, pnlOptionsTabActiveForeColor.BackColor, pnlOptionsTabInactiveForeColor.BackColor);
                UIHelper.SetDockingTabColor(c1DockingTab1, pnlOptionsTabActiveBackColor.BackColor, pnlOptionsTabActiveForeColor.BackColor, pnlOptionsTabInactiveForeColor.BackColor);

                if (MyLibrary.IsDarkMode)
                {
                    c1ThemeController1.SetTheme(c1DockingTab, "VS2013Dark");
                }
                #endregion

                #region 套用 tabExmaple 外觀
                tabExample.BackColor = ColorTranslator.FromHtml(MyGlobal.TabBackColor);
                tabExample.ForeColor = ColorTranslator.FromHtml(MyGlobal.TabActiveForeColor);
                tabExample.TextInactiveColor = ColorTranslator.FromHtml(MyGlobal.TabInactiveForeColor);
                tabExample.BoldSelectedPage = true;
                tabExample.ShrinkPagesToFit = chkShrinkPages.Checked;
                tabExample.ShowArrows = chkShowArrows.Checked;
                tabExample.HoverSelect = chkHoverSelect.Checked;
                tabExample.Multiline = chkMultiLine.Checked;
                tabExample.PositionTop = true;
                tabExample.ShowClose = true;

                tabExample.Style = MyLibrary.TabStyle == "IDE" ? VisualStyle.IDE : VisualStyle.Plain;

                switch (MyLibrary.TabAppearance)
                {
                    case "MultiDocument":
                        {
                            tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiDocument;
                            break;
                        }
                    case "MultiForm":
                        {
                            tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiForm;
                            break;
                        }
                    default:
                        {
                            tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiBox;
                            break;
                        }
                }
                #endregion

                //載入雜項設定
                if (!string.IsNullOrEmpty(MyLibrary.DefaultDirectory) && Directory.Exists(MyLibrary.DefaultDirectory)) //Default Directory
                {
                    rdoFavoriteDirectory.Checked = true;
                    txtFavoriteDirectory.Text = MyLibrary.DefaultDirectory;
                }
                else
                {
                    rdoDefaultDirectory.Checked = true;
                }

                cboGridVisualStyle.Text = MyLibrary.GridVisualStyle;

                if (!MyLibrary.IsDarkMode)
                {
                    ChangeVisualStyle(false); //bPreview=true, 表示只要針對「c1GridVisualStyle」作用即可
                }

                _rowHeight = c1GridVisualStyle.RowHeight;
                lstLanguage.SelectedIndex = 0;

                HighlightPreview();
                btnClearHighlightsGrid.Enabled = false;

                //外觀套用
                ApplySqlStyler();
                editorIndicator.ReadOnly = false;
                editorIndicator.Text = "SELECT * FROM Empoyee";
                editorIndicator.ReadOnly = true;
                SetSquiggle(false, string.Empty, 14, 7);
                ApplyIndicatorAppearance(MyLibrary.ColorBookmarkBackground);

                //////InitializeDefaultSelectedTabs();

                _isFormLoadFinished = true;
                //////c1DockingTab4.SelectedTab = tabGlobalSettings;

                PreviewSqlToCode();
                PreviewSqlFormatter();
                CreateVisualStyleInfo();
                UpdateMainFormTabVisualStyle();
            }
            catch (Exception ex)
            {
                if (isLayoutSuspended)
                {
                    ResumeLayout(true);
                    isLayoutSuspended = false;
                }

                ShowMainOptionTabsAfterLoading();

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (isLayoutSuspended)
                {
                    ResumeLayout(true);
                }

                if (!c1DockingTab.Visible)
                {
                    ShowMainOptionTabsAfterLoading();
                }

                Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Controls.Clear();
            _editableEditorContextMenu.Dispose();
            _readOnlyEditorContextMenu.Dispose();
            base.OnFormClosed(e);
        }

        private void ApplyLocalizationSetting()
        {
            LocalizationHelper.ApplyLanguageInfo(this);

            RebuildOptionEditorContextMenus();

            chkShowDatabaseName.Text = chkShowDatabaseName.Text.Replace("{DatabaseName}", $"({DatabaseSqlExecutor.DataSourceDisplayName})");
            grpSqlStatementFormatter.Text = $"{grpSqlStatementFormatter.Text} ({DatabaseSqlExecutor.DataSourceDisplayName})";

            ApplyLocalizedLayout();
            ApplyLocalizedControlColor();
            ApplyInitialValue();

            _languageText = LocalizationHelper.GetLanguageString("Display 'Start' flag on a wrapped line", "form", GetType().Name, "object", "chkStart", "ToolTipText");
            _toolTip1.SetToolTip(chkStart, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Display 'End' flag on a wrapped line", "form", GetType().Name, "object", "chkEnd", "ToolTipText");
            _toolTip1.SetToolTip(chkEnd, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Display 'Margin' flag on a wrapped line", "form", GetType().Name, "object", "chkMargin", "ToolTipText");
            _toolTip1.SetToolTip(chkMargin, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Determines how wrapped sublines are indented", "form", GetType().Name, "object", "cboIndentMode", "ToolTipText");
            _toolTip1.SetToolTip(cboIndentMode, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Find Operator Keywords (Ctrl+F)", "form", GetType().Name, "object", "picOperatorKeywords", "ToolTipText");
            _toolTip1.SetToolTip(picOperatorKeywords, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Find Built-In Functions (Ctrl+F)", "form", GetType().Name, "object", "picBuiltInFunctions", "ToolTipText");
            _toolTip1.SetToolTip(picBuiltInFunctions, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Find Built-In Keywords (Ctrl+F)", "form", GetType().Name, "object", "picBuiltInKeywords", "ToolTipText");
            _toolTip1.SetToolTip(picBuiltInKeywords, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Find User-Defined Keywords (Ctrl+F)", "form", GetType().Name, "object", "picUserDefinedKeywords", "ToolTipText");
            _toolTip1.SetToolTip(picUserDefinedKeywords, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Operator Keywords, Built-in Keywords, User-defined Keywords", "form", GetType().Name, "object", "chkBold", "ToolTipText");
            _toolTip1.SetToolTip(chkBold, _languageText);

            _languageText = LocalizationHelper.GetLanguageString("Browse File", "form", GetType().Name, "object", "btnSpecifiedSQLFile1", "ToolTipText");
            _toolTip1.SetToolTip(btnSpecifiedSqlFile1, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Browse File", "form", GetType().Name, "object", "btnSpecifiedSQLFile2", "ToolTipText");
            _toolTip1.SetToolTip(btnSpecifiedSqlFile2, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Clear", "form", GetType().Name, "object", "btnClear1", "ToolTipText");
            _toolTip1.SetToolTip(btnClear1, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Clear", "form", GetType().Name, "object", "btnClear2", "ToolTipText");
            _toolTip1.SetToolTip(btnClear2, _languageText);

            _languageText = LocalizationHelper.GetLanguageString("Select Company Update Folder...", "form", GetType().Name, "object", "btnBrowseLocalFolder", "ToolTipText");
            _toolTip1.SetToolTip(btnBrowseLocalFolder, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Open Company Update Folder", "form", GetType().Name, "object", "btnLocalFolderOpenFolder", "ToolTipText");
            _toolTip1.SetToolTip(btnLocalFolderOpenFolder, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("About Company Update Folder", "form", GetType().Name, "object", "btnHelp_LocalUpdateFolder", "ToolTipText");
            _toolTip1.SetToolTip(btnHelp_LocalUpdateFolder, _languageText);

            _cMenu = new ContextMenuStrip();
            _gMenu = new ContextMenuStrip();

            _cMenu.Items.Add("Copy");
            ((ToolStripMenuItem)_cMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.C;

            _cMenu.Items[0].Click += delegate
            {
                if (chkCopyAsHTML.Checked)
                {
                    editor.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editor.Copy();
                }
            };

            _languageText = LocalizationHelper.GetLanguageString("Cell Viewer", "form", GetType().Name, "menugrid", "CellViewer", "Text");
            _lstMenuGrid.Add(_languageText);
            _lstMenuGrid.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menugrid", "SelectAll", "Text");
            _lstMenuGrid.Add(_languageText);
            _lstMenuGrid.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Export all data to Excel", "form", GetType().Name, "menugrid", "ExportAllDataToExcel", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Export all data to Excel without color", "form", GetType().Name, "menugrid", "ExportAllDataToExcelWithoutColor", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Export all data to CSV", "form", GetType().Name, "menugrid", "ExportAllDataToCSV", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Export all data to File (as Insert script)", "form", GetType().Name, "menugrid", "ExportAllDataToFile", "Text");
            _lstMenuGrid.Add(_languageText);
            _lstMenuGrid.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menugrid", "Copy", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Copy with Column Name(s)", "form", GetType().Name, "menugrid", "CopyWithColumnName", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Copy Column Name(s)", "form", GetType().Name, "menugrid", "CopyColumnName", "Text");
            _lstMenuGrid.Add(_languageText);
            _lstMenuGrid.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Freeze Column", "form", GetType().Name, "menugrid", "FreezeColumn", "Text");
            _lstMenuGrid.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Unfreeze Column", "form", GetType().Name, "menugrid", "UnfreezeColumn", "Text");
            _lstMenuGrid.Add(_languageText);

            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.CellViewer]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.Dash0]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.SelectAll]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.Dash1]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.ExportToExcel]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.ExportToCsv]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.ExportToFile]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.Dash2]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.Copy]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.CopyWithColumnNames]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.CopyColumnNames]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.Dash3]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.FreezeColumn]);
            _gMenu.Items.Add(_lstMenuGrid[MenuColumn.UnfreezeColumn]);

            _gMenu.Items[MenuColumn.CellViewer].Click += delegate
            {
                CellViewer();
            };

            ((ToolStripMenuItem)_gMenu.Items[MenuColumn.SelectAll]).ShortcutKeys = Keys.Control | Keys.A;

            _gMenu.Items[MenuColumn.SelectAll].Click += delegate
            {
                for (var i = 0; i < c1GridVisualStyle.Splits[0].Rows.Count; i++)
                {
                    c1GridVisualStyle.SelectedRows.Add(i);
                }
            };

            _gMenu.Items[MenuColumn.ExportToExcel].Click += delegate
            {
                ExportToExcel();
            };

            _gMenu.Items[MenuColumn.ExportToCsv].Click += delegate
            {
                ArrangeDataForAllData("EXPORTTOCSV");
            };

            _gMenu.Items[MenuColumn.ExportToFile].Click += delegate
            {
                ArrangeData("EXPORTTOFILE");
            };

            _gMenu.Items[MenuColumn.Copy].Click += delegate
            {
                ArrangeData("COPY");
            };

            _gMenu.Items[MenuColumn.CopyWithColumnNames].Click += delegate
            {
                ArrangeData("COPYWITHCOLUMNNAMES");
            };

            _gMenu.Items[MenuColumn.CopyColumnNames].Click += delegate
            {
                ArrangeData("COPYCOLUMNNAMES");
            };

            _gMenu.Items[MenuColumn.FreezeColumn].Click += delegate
            {
                FrozenColumn();
            };

            _gMenu.Items[MenuColumn.UnfreezeColumn].Click += delegate
            {
                FrozenColumn(false);
            };

            CreateAndGetAutoReplaceInfoTable();
        }

        private void LoadLargeTextPreviewLengthSettings()
        {
            cboLargeTextPreviewLength.Items.Clear();

            foreach (var length in LargeTextPreviewLengthPolicy.GetAllowedLengths())
            {
                cboLargeTextPreviewLength.Items.Add(length.ToString());
            }

            var normalizedLength = LargeTextPreviewLengthPolicy.Normalize(AppConfigHelper.LargeTextPreviewLength);

            AppConfigHelper.LargeTextPreviewLength = normalizedLength;
            cboLargeTextPreviewLength.Text = normalizedLength.ToString();
        }

        private void InitializeOptionEditorContextMenus()
        {
            _editableEditorContextMenu.Opening += OptionEditorContextMenu_Opening;
            _readOnlyEditorContextMenu.Opening += OptionEditorContextMenu_Opening;

            foreach (var optionEditor in _editableOptionEditors)
            {
                optionEditor.ContextMenuStrip = _editableEditorContextMenu;
            }

            foreach (var previewEditor in _readOnlyOptionEditors)
            {
                previewEditor.ReadOnly = true;
                previewEditor.ContextMenuStrip = _readOnlyEditorContextMenu;
            }
        }

        private void RebuildOptionEditorContextMenus()
        {
            _editableEditorContextMenu.Items.Clear();
            _readOnlyEditorContextMenu.Items.Clear();

            AddOptionEditorMenuItem(_editableEditorContextMenu, "Undo", "Undo", OptionEditorCommand.Undo, "Ctrl+Z", "Undo 16x16.ico");
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Redo", "Redo", OptionEditorCommand.Redo, "Ctrl+Y", "Redo 16x16.ico");
            _editableEditorContextMenu.Items.Add(new ToolStripSeparator());
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Cut", "Cut", OptionEditorCommand.Cut, "Ctrl+X", "Cut 16x16.ico");
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Copy", "Copy", OptionEditorCommand.Copy, "Ctrl+C", "Copy 16x16.ico");
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Paste", "Paste", OptionEditorCommand.Paste, "Ctrl+V", "Paste 16x16.ico");
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Delete", "Delete", OptionEditorCommand.Delete, string.Empty, "Delete 16x16.ico");
            _editableEditorContextMenu.Items.Add(new ToolStripSeparator());
            AddOptionEditorMenuItem(_editableEditorContextMenu, "Select All", "SelectAll", OptionEditorCommand.SelectAll, "Ctrl+A", "Select All 16x16.ico");

            AddOptionEditorMenuItem(_readOnlyEditorContextMenu, "Copy", "Copy", OptionEditorCommand.Copy, "Ctrl+C", "Copy 16x16.ico");
            _readOnlyEditorContextMenu.Items.Add(new ToolStripSeparator());
            AddOptionEditorMenuItem(_readOnlyEditorContextMenu, "Select All", "SelectAll", OptionEditorCommand.SelectAll, "Ctrl+A", "Select All 16x16.ico");

            ApplyOptionEditorContextMenuAppearance(_editableEditorContextMenu);
            ApplyOptionEditorContextMenuAppearance(_readOnlyEditorContextMenu);
        }

        private void AddOptionEditorMenuItem(ContextMenuStrip contextMenu, string defaultText, string localizationKey,
                                             OptionEditorCommand command, string shortcutKeyDisplayString, string iconFileName)
        {
            var menuText = LocalizationHelper.GetLanguageString
            (
                defaultText,
                "form",
                GetType().Name,
                "menueditor",
                localizationKey,
                "Text"
            );

            var menuItem = new ToolStripMenuItem(menuText)
            {
                Tag = command,
                Image = IconManager.GetImage(MyGlobal.IconLibrary, iconFileName),
                ShortcutKeyDisplayString = shortcutKeyDisplayString
            };

            menuItem.Click += OptionEditorContextMenuItem_Click;
            contextMenu.Items.Add(menuItem);
        }

        private static void ApplyOptionEditorContextMenuAppearance(ContextMenuStrip contextMenu)
        {
            if (!MyLibrary.IsDarkMode)
            {
                return;
            }

            contextMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
            contextMenu.ForeColor = Color.White;
            contextMenu.RenderMode = ToolStripRenderMode.System;
        }

        private void OptionEditorContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(sender is ContextMenuStrip contextMenu) || !(contextMenu.SourceControl is ScintillaEditor optionEditor))
            {
                e.Cancel = true;
                return;
            }

            var hasSelection = optionEditor.SelectionStart != optionEditor.SelectionEnd;

            foreach (ToolStripItem item in contextMenu.Items)
            {
                if (!(item is ToolStripMenuItem menuItem) || !(menuItem.Tag is OptionEditorCommand command))
                {
                    continue;
                }

                switch (command)
                {
                    case OptionEditorCommand.Undo:
                        {
                            menuItem.Enabled = !optionEditor.ReadOnly && optionEditor.CanUndo;
                            break;
                        }
                    case OptionEditorCommand.Redo:
                        {
                            menuItem.Enabled = !optionEditor.ReadOnly && optionEditor.CanRedo;
                            break;
                        }
                    case OptionEditorCommand.Cut:
                    case OptionEditorCommand.Delete:
                        {
                            menuItem.Enabled = !optionEditor.ReadOnly && hasSelection;
                            break;
                        }
                    case OptionEditorCommand.Copy:
                        {
                            menuItem.Enabled = hasSelection;
                            break;
                        }
                    case OptionEditorCommand.Paste:
                        {
                            menuItem.Enabled = !optionEditor.ReadOnly && optionEditor.CanPaste;
                            break;
                        }
                    case OptionEditorCommand.SelectAll:
                        {
                            menuItem.Enabled = optionEditor.TextLength > 0;
                            break;
                        }
                }
            }
        }

        private static void OptionEditorContextMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem menuItem) || !(menuItem.Tag is OptionEditorCommand command) ||
                !(menuItem.Owner is ContextMenuStrip contextMenu) || !(contextMenu.SourceControl is ScintillaEditor optionEditor))
            {
                return;
            }

            ExecuteOptionEditorCommand(optionEditor, command);
        }

        private static void ExecuteOptionEditorCommand(ScintillaEditor optionEditor, OptionEditorCommand command)
        {
            switch (command)
            {
                case OptionEditorCommand.Undo:
                    {
                        if (!optionEditor.ReadOnly && optionEditor.CanUndo)
                        {
                            optionEditor.Undo();
                        }

                        break;
                    }
                case OptionEditorCommand.Redo:
                    {
                        if (!optionEditor.ReadOnly && optionEditor.CanRedo)
                        {
                            optionEditor.Redo();
                        }

                        break;
                    }
                case OptionEditorCommand.Cut:
                    {
                        if (!optionEditor.ReadOnly && optionEditor.SelectionStart != optionEditor.SelectionEnd)
                        {
                            optionEditor.Cut();
                        }

                        break;
                    }
                case OptionEditorCommand.Copy:
                    {
                        if (optionEditor.SelectionStart != optionEditor.SelectionEnd)
                        {
                            optionEditor.Copy();
                        }

                        break;
                    }
                case OptionEditorCommand.Paste:
                    if (!optionEditor.ReadOnly && optionEditor.CanPaste)
                    {
                        optionEditor.Paste();
                    }

                    break;
                case OptionEditorCommand.Delete:
                    {
                        if (!optionEditor.ReadOnly && optionEditor.SelectionStart != optionEditor.SelectionEnd)
                        {
                            optionEditor.Clear();
                        }

                        break;
                    }
                case OptionEditorCommand.SelectAll:
                    {
                        if (optionEditor.TextLength > 0)
                        {
                            optionEditor.SelectAll();
                        }

                        break;
                    }
            }
        }

        private void ApplyLocalizedControlColor()
        {
            lblGlobalOverview.ForeColor = MyLibrary.IsDarkMode ? Color.Yellow : Color.Green;
            lblLargeTextNote.ForeColor = Color.DarkRed;
        }

        private void ApplyLocalizedLayout()
        {
            SuspendLayout();

            try
            {
                //Global, Data Grid, Auto Complete, Safety
                ControlLayoutHelper.PlaceRightOf(lblStarShowDatabaseName, chkShowDatabaseName, -5);
                ControlLayoutHelper.PlaceRightOf(lblStarShowVersion, chkShowVersion, -5);
                ControlLayoutHelper.PlaceRightOf(lblStarShowIP, chkShowIP, -5);

                ControlLayoutHelper.PlaceRightOf(btnHelp_ColumnComment, chkShowColumnComment, -3);
                ControlLayoutHelper.PlaceRightOf(btnHelp_RawDataMode, chkRawDataMode, -3);
                ControlLayoutHelper.PlaceRightOf(btnHelp_AppendQueryResult, chkAppendQueryResult, -3);

                ControlLayoutHelper.PlaceRightOf(lblStarSortByColumnName, chkSortByColumnName, -5);
                ControlLayoutHelper.PlaceRightOf(lblStarShowColumnInfo, chkShowColumnInfo, -5);
                ControlLayoutHelper.PlaceRightOf(btnHelp_ShowColumnInfo, lblStarShowColumnInfo, 3);

                ControlLayoutHelper.PlaceRightOf(btnHelp_AutoListMembers, chkAutoListMembers, 3);
                ControlLayoutHelper.PlaceRightOf(btnHelp_SavePoint, chkSavePoint, 3);
                ControlLayoutHelper.PlaceRightOf(lblStarDirection, chkDirection, 5);
                ControlLayoutHelper.PlaceRightOf(btnHelp_Symbol, lblSymbolTips, 3);

                ControlLayoutHelper.PlaceRightOf(btnHelp_DisconnectAfterSelect, chkDisconnectAfterSelect, 0);
                ControlLayoutHelper.PlaceRightOf(btnHelp_PendingWarning, chkPendingWarning, 0);

                ControlLayoutHelper.ArrangeHorizontalKeepTop
                (
                    0,
                    lblPreviewLength,
                    cboLargeTextPreviewLength,
                    lblCharacters
                );

                ControlLayoutHelper.PlaceRightOf(cboNullShowAs, lblNullValueShowAs, 1);
                ControlLayoutHelper.PlaceRightOf(lblStarNullValueStyle, cboNullShowAs, 0);

                //GroupBox 標題星號
                SetGroupBoxTitleStarLayout(grpMainFormTabVisualStyle, lblStarMainFormTab, -10, true);
                SetGroupBoxTitleStarLayout(grpHighlightStyle, lblStarHighlight, -10, false);
                SetGroupBoxTitleStarLayout(grpDataGridColor, lblStarGridColor, -10, false);

                lblLength.Text = grpIndicate.Text.TrimEnd();
                lblStarIndicate.Location = new Point(lblLength.Width + 2, lblStarIndicate.Top);
                lblStarIndicate.Visible = false;

                lblCommitRollbackIcon.Text = grpCommitRollbackIcon.Text;
                ControlLayoutHelper.PlaceRightOf(lblStarCommitRollbackIcon, lblCommitRollbackIcon, 1);

                ControlLayoutHelper.PlaceRightOf(btnHelp_DarkMode, chkDarkMode, 3);

                //Backup GroupBox：CheckBox 疊在 GroupBox 邊框上
                ControlLayoutHelper.AdjustGroupBoxBorderForCheckbox(grpBackup, chkEnableBackup, 7);

                ControlLayoutHelper.PlaceRightOf(lblStarBackup, chkEnableBackup, -4);
                ControlLayoutHelper.PlaceRightOf(txtBackupPath, lblBackupPath, 0);
                ControlLayoutHelper.PlaceRightOf(btnBrowseBackupPath, txtBackupPath, 6);
                ControlLayoutHelper.PlaceRightOf(btnBackupPathOpenFolder, btnBrowseBackupPath, 6);
                ControlLayoutHelper.PlaceRightOf(btnClear3, btnBackupPathOpenFolder, 6);

                //Update Information Source
                ControlLayoutHelper.PlaceRightOf(btnHelp_LocalUpdateFolder, rdoUpdateSourceLocal, 3);
                ControlLayoutHelper.PlaceRightOf(txtLocalFolder, btnHelp_LocalUpdateFolder, 5);
                txtLocalFolder.Width = grpUpdateInformationSource.Width - txtLocalFolder.Left - btnBrowseLocalFolder.Width - btnLocalFolderOpenFolder.Width - 24;
                ControlLayoutHelper.PlaceRightOf(btnBrowseLocalFolder, txtLocalFolder, 5);
                ControlLayoutHelper.PlaceRightOf(btnLocalFolderOpenFolder, btnBrowseLocalFolder, 5);

                //Auto Complete GroupBox：CheckBox 疊在 GroupBox 邊框上
                ControlLayoutHelper.AdjustGroupBoxBorderForCheckbox(grpAutoComplete, chkEnableAutoComplete, 7);

                ControlLayoutHelper.PlaceRightOf(lblStarAutoComplete, chkEnableAutoComplete, -4);
                ControlLayoutHelper.PlaceRightOf(btnHelp_EnableAutoComplete, lblStarAutoComplete, 5);

                //Auto Replace GroupBox：CheckBox 疊在 GroupBox 邊框上
                ControlLayoutHelper.AdjustGroupBoxBorderForCheckbox(grpAutoReplace, chkEnableAutoReplace, 7);

                ControlLayoutHelper.PlaceRightOf(lblStarAutoReplace, chkEnableAutoReplace, -4);
                lblStarAutoReplace.Visible = false;
                ControlLayoutHelper.PlaceRightOf(btnHelp_EnableAutoReplace, chkEnableAutoReplace, 5);

                //Options Tab Appearance
                ControlLayoutHelper.PlaceRightOf(pnlOptionsTabActiveForeColor, lblOptionsTabActiveForeColor, 3);
                ControlLayoutHelper.PlaceRightOf(lblOptionsTabActiveBackColor, pnlOptionsTabActiveForeColor, 30);
                ControlLayoutHelper.PlaceRightOf(pnlOptionsTabActiveBackColor, lblOptionsTabActiveBackColor, 3);
                ControlLayoutHelper.PlaceRightOf(lblOptionsTabInactiveForeColor, pnlOptionsTabActiveBackColor, 30);
                ControlLayoutHelper.PlaceRightOf(pnlOptionsTabInactiveForeColor, lblOptionsTabInactiveForeColor, 3);

                //Open SQL File
                txtSpecifiedSQLFile1.Size = new Size(grpOpenSqlFile.Width - lblFile1.Width - btnSpecifiedSqlFile1.Width - btnClear1.Width - 40, 21);
                ControlLayoutHelper.PlaceRightOf(txtSpecifiedSQLFile1, lblFile1, 2);
                ControlLayoutHelper.PlaceRightOf(btnSpecifiedSqlFile1, txtSpecifiedSQLFile1, 5);
                ControlLayoutHelper.PlaceRightOf(btnClear1, btnSpecifiedSqlFile1, 5);

                txtSpecifiedSQLFile2.Size = new Size(grpOpenSqlFile.Width - lblFile2.Width - btnSpecifiedSqlFile2.Width - btnClear2.Width - 40, 21);
                ControlLayoutHelper.PlaceRightOf(txtSpecifiedSQLFile2, lblFile2, 2);
                ControlLayoutHelper.PlaceRightOf(btnSpecifiedSqlFile2, txtSpecifiedSQLFile2, 5);
                ControlLayoutHelper.PlaceRightOf(btnClear2, btnSpecifiedSqlFile2, 5);

                //Auto Complete
                ControlLayoutHelper.PlaceRightOf(nudMinFragmentLength, lblMinFragmentLength, 0);

                //Data Grid
                ControlLayoutHelper.PlaceRightOf(cboMaxWidth, lblMaxWidth, 1);
                ControlLayoutHelper.PlaceRightOf(pnlNullValueForeColor, lblNullValueForeColor, 3);
                ControlLayoutHelper.PlaceRightOf(cboRowsPerPage, lblRowsPerPage, 1);
                ControlLayoutHelper.PlaceRightOf(btnHelp_PagedQuery, chkPagedQuery, 1);

                //SQL To Code
                ControlLayoutHelper.PlaceRightOf(txtSqlVariableName, lblSqlVariableName, 1);
                ControlLayoutHelper.PlaceRightOf(lblStringBuilderVariableName, txtSqlVariableName, 30);
                ControlLayoutHelper.PlaceRightOf(txtStringBuilderVariableName, lblStringBuilderVariableName, 1);

                //Global
                ControlLayoutHelper.PlaceRightOf(cboDateFormat, lblDateFormat, 1);
                ControlLayoutHelper.PlaceRightOf(cboLocalization, lblLocalization, 1);
                ControlLayoutHelper.PlaceRightOf(txtRecentFiles, lblRecentFiles, 3);
                ControlLayoutHelper.PlaceRightOf(txtMyFavorite, lblMyFavorite, 3);

                //Indicator, Bookmark
                ControlLayoutHelper.PlaceRightOf(pnlErrorLineBackground, lblErrorLineBackground, 3);
                ControlLayoutHelper.PlaceRightOf(pnlBookmarkBackground, lblBookmarkBackground, 3);
                ControlLayoutHelper.PlaceRightOf(lblBookmarkStyle, pnlErrorLineBackground, 33);
                ControlLayoutHelper.PlaceRightOf(cboBookmarkStyle, lblBookmarkStyle, 3);

                editorIndicator.Location = new Point(lblBookmarkStyle.Left + 3, editorIndicator.Top);

                ControlLayoutHelper.PlaceRightOf(cboTabWidth, lblTabWidth, 3);
                ControlLayoutHelper.PlaceRightOf(lblStarShowIndentGuide, chkShowIndentGuide, -4);
                ControlLayoutHelper.PlaceRightOf(lblStarTabWidth, cboTabWidth, 3);

                //Find Grid, Save As Encoding, Indent Mode
                cboFindGrid.Left = lblFindGrid.Width + 3;
                ControlLayoutHelper.PlaceRightOf(cboSaveAsEncoding, chkSaveAsEncoding, 0);
                ControlLayoutHelper.PlaceRightOf(cboIndentMode, lblIndentMode, 3);
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ApplyInitialValue()
        {
            cboMaxWidth.Text = GridHelper.MaxWidth.ToString(); //最大寬度的設定值

            //按下 Enter 鍵後的移動方向
            UIHelper.SetC1ComboBoxItemsFromDictionary(cboDirection, MyGlobal.dicDirection);
            cboDirection.Text = GridHelper.Direction;

            //定義縮排模式的設定值
            UIHelper.SetC1ComboBoxItemsFromDictionary(cboIndentMode, MyGlobal.dicWordWrapIndentMode);
            cboIndentMode.Text = MyGlobal.WordWrapIndentMode;

            cboTabWidth.Text = MyGlobal.TabWidth.ToString();
            txtIndentWord.Text = MyGlobal.TabWidth.ToString();

            //定義書籤樣式的設定值
            UIHelper.SetC1ComboBoxItemsFromDictionary(cboBookmarkStyle, MyGlobal.dicBookmarkStyle);
            cboBookmarkStyle.Text = MyGlobal.BookmarkStyle;

            //Begin:定義調整列高方式的設定值
            UIHelper.SetC1ComboBoxItemsFromDictionary(cboGridRowHeightResizing, MyGlobal.dicRowSizing);
            cboGridRowHeightResizing.Text = MyGlobal.RowSize;

            if (TextHelper.GetKeyFromDictionary(MyGlobal.dicRowSizing, MyGlobal.RowSize) == "AllRows")
            {
                c1GridAutoReplaceInfo.AllowRowSizing = RowSizingEnum.AllRows;
                c1GridVisualStyle.AllowRowSizing = RowSizingEnum.AllRows;
            }
            else
            {
                c1GridAutoReplaceInfo.AllowRowSizing = RowSizingEnum.IndividualRows;
                c1GridVisualStyle.AllowRowSizing = RowSizingEnum.IndividualRows;
            }
            //End:定義調整列高方式的設定值
        }

        private void SetGroupBoxTitleStarLayout(Control groupBox, Control starLabel, int offsetX, bool appendSpacesToGroupBoxText)
        {
            var text = groupBox.Text.TrimEnd();

            lblLength.Text = text;

            if (appendSpacesToGroupBoxText)
            {
                groupBox.Text = $"{text}    ";
            }

            starLabel.Location = new Point(groupBox.Left + lblLength.Width + offsetX, starLabel.Top);
        }


        private void ApplySqlStyler(string controlName = "all")
        {
            SqlStyler.ColorEditorBackground = TextHelper.GetSafeString(pnlEditorBackground.Tag);
            SqlStyler.ColorTextIdentifier = TextHelper.GetSafeString(pnlIdentifier.Tag);
            SqlStyler.ColorComments = TextHelper.GetSafeString(pnlComments.Tag);
            SqlStyler.ColorNumber = TextHelper.GetSafeString(pnlNumber.Tag);
            SqlStyler.ColorString = TextHelper.GetSafeString(pnlString.Tag);
            SqlStyler.ColorCharacter = TextHelper.GetSafeString(pnlCharacter.Tag);
            SqlStyler.ColorOperatorSymbol = TextHelper.GetSafeString(pnlOperatorSymbol.Tag);
            SqlStyler.ColorUserDefinedTablesViews = TextHelper.GetSafeString(pnlUserTables.Tag);
            SqlStyler.ColorUserDefinedFunctionsTriggers = TextHelper.GetSafeString(pnlUserFunctions.Tag);
            SqlStyler.ColorOperatorKeywords = TextHelper.GetSafeString(pnlOperatorKeywords.Tag);
            SqlStyler.ColorBuiltInFunctions = TextHelper.GetSafeString(pnlBuiltInFunctions.Tag);
            SqlStyler.ColorBuiltInKeywords = TextHelper.GetSafeString(pnlBuiltInKeywords.Tag);
            SqlStyler.ColorUserDefinedKeywords = TextHelper.GetSafeString(pnlUserDefinedKeywords.Tag);

            SqlStyler.IsKeywordFontBold = chkBold.Checked;
            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = editorOperatorKeywords.Text;
            SqlStyler.KeywordsBuiltInFunctions = editorBuiltInFunctions.Text;
            SqlStyler.KeywordsBuiltInKeywords = editorBuiltInKeywords.Text;
            SqlStyler.KeywordsUserDefinedKeywords = editorUserDefinedKeywords.Text;

            if (controlName == "all" || controlName == "editor")
            {
                editor.Styler = new SqlStyler();
            }

            if (controlName == "all" || controlName == "editorAutoReplace")
            {
                editorAutoReplace.Styler = new SqlStyler();
            }

            if (controlName == "all" || controlName == "editorSqlToCode")
            {
                editorSqlToCode.Styler = new SqlStyler();
            }

            if (controlName == "all" || controlName == "editorSqlFormatter")
            {
                editorSqlFormatter.Styler = new SqlStyler();
            }

            if (controlName == "all" || controlName == "editorSqlFormatterPreview")
            {
                editorSqlFormatterPreview.Styler = new SqlStyler();
            }

            if (controlName == "all" || controlName == "editorIndicator")
            {
                editorIndicator.Styler = new SqlStyler();
            }
        }

        private void ApplyDarkStyler()
        {
            SqlStyler.ColorEditorBackground = "#2D2D30";
            SqlStyler.ColorTextIdentifier = "#FFFFFF";
            SqlStyler.ColorComments = "#FFFFFF";
            SqlStyler.ColorNumber = "#FFFFFF";
            SqlStyler.ColorString = "#FFFFFF";
            SqlStyler.ColorCharacter = "#FFFFFF";
            SqlStyler.ColorOperatorSymbol = "#FFFFFF";
            SqlStyler.ColorUserDefinedTablesViews = "#FFFFFF";
            SqlStyler.ColorUserDefinedFunctionsTriggers = "#FFFFFF";
            SqlStyler.ColorOperatorKeywords = "#FFFFFF";
            SqlStyler.ColorBuiltInFunctions = "#FFFFFF";
            SqlStyler.ColorBuiltInKeywords = "#FFFFFF";
            SqlStyler.ColorUserDefinedKeywords = "#FFFFFF";

            SqlStyler.IsKeywordFontBold = false;
            SqlStyler.KeywordsUserDefinedTables = string.Empty;
            SqlStyler.KeywordsUserDefinedViews = string.Empty;
            SqlStyler.KeywordsUserDefinedFunctions = string.Empty;
            SqlStyler.KeywordsUserDefinedTriggers = string.Empty;
            SqlStyler.KeywordsOperatorKeywords = string.Empty;
            SqlStyler.KeywordsBuiltInFunctions = string.Empty;
            SqlStyler.KeywordsBuiltInKeywords = string.Empty;
            SqlStyler.KeywordsUserDefinedKeywords = string.Empty;

            editorAutoReplace.Styler = new SqlStyler();
            editorSqlToCodePreview.Styler = new SqlStyler();
            editorOperatorKeywords.Styler = new SqlStyler();
            editorBuiltInFunctions.Styler = new SqlStyler();
            editorBuiltInKeywords.Styler = new SqlStyler();
            editorUserDefinedKeywords.Styler = new SqlStyler();
        }

        private void chkWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            bool bValue;

            if (!chkWordWrap.Checked)
            {
                bValue = false;
                editor.WrapMode = ScintillaNET.WrapMode.None;
            }
            else
            {
                bValue = true;
                editor.WrapMode = ScintillaNET.WrapMode.Word;
                editor.WrapVisualFlags = (chkStart.Checked ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (chkEnd.Checked ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (chkMargin.Checked ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
            }

            chkStart.Enabled = bValue;
            chkEnd.Enabled = bValue;
            chkMargin.Enabled = bValue;

            btnWordWrap.Visible = !chkWordWrap.Checked;
            btnWordWrap2.Visible = chkWordWrap.Checked;
        }

        private void WordWrapFlags_CheckedChanged(object sender, EventArgs e)
        {
            editor.WrapVisualFlags = (chkStart.Checked ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (chkEnd.Checked ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (chkMargin.Checked ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
        }

        private void chkBold_CheckedChanged(object sender, EventArgs e)
        {
            ApplySqlStyler("editor");
        }

        private void chkCopyAsHTML_CheckedChanged(object sender, EventArgs e)
        {
            //do nothing (透過程式判斷)
        }

        private void chkShowAllCharacters_CheckedChanged(object sender, EventArgs e)
        {
            editor.ViewEol = chkShowAllCharacters.Checked;
            editor.ViewWhitespace = chkShowAllCharacters.Checked ? ScintillaNET.WhitespaceMode.VisibleAlways : ScintillaNET.WhitespaceMode.Invisible;

            btnShowAllCharacters.Visible = !chkShowAllCharacters.Checked;
            btnShowAllCharacters2.Visible = chkShowAllCharacters.Checked;
        }

        private void pnlSelectedClick(object sender, EventArgs e)
        {
            if (!(sender is Panel pnlSelected))
            {
                return;
            }

            _panelColorSelectedName = pnlSelected.Name;
            ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ApplyQueryEditorColor);
        }

        private void ApplyQueryEditorColor(Color selectedColor)
        {
            var e = new { Color = selectedColor, HexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor) };

            for (var i = 0; i < _lstPanelQueryEditorColor.Count; i++)
            {
                if (((Panel)_lstPanelQueryEditorColor[i]).Name != _panelColorSelectedName)
                {
                    continue;
                }

                ((Panel)_lstPanelQueryEditorColor[i]).BackColor = e.Color;
                ((Panel)_lstPanelQueryEditorColor[i]).Tag = e.HexColor;
                _toolTip1.SetToolTip((Panel)_lstPanelQueryEditorColor[i], $"{e.HexColor} (R:{e.Color.R}, G:{e.Color.G}, B:{e.Color.B})");

                switch (((Panel)_lstPanelQueryEditorColor[i]).Name)
                {
                    case "pnlToolstripBackground":
                        {
                            pnlToolstripBackground.Tag = e.HexColor;
                            _colorEditorFocused = pnlToolstripBackground.BackColor;
                            break;
                        }
                    case "pnlEditorBackground":
                        {
                            pnlEditorBackground.Tag = e.HexColor;
                            break;
                        }
                    case "pnlCurrentLineBackground":
                        {
                            editor.CaretLineBackColor = e.Color;
                            pnlCurrentLineBackground.Tag = e.HexColor;
                            break;
                        }
                    case "pnlSelectedTextBackground":
                        {
                            editor.SetSelectionBackColor(true, e.Color);
                            pnlSelectedTextBackground.Tag = e.HexColor;
                            break;
                        }
                    case "pnlErrorLineBackground":
                        {
                            SetSquiggle(false, e.HexColor, 14, 7); //變更 Error Line 顏色
                            pnlErrorLineBackground.Tag = e.HexColor;
                            break;
                        }
                    case "pnlBookmarkBackground":
                        {
                            ApplyIndicatorAppearance(e.HexColor); //變更 Bookmark 顏色
                            pnlBookmarkBackground.Tag = e.HexColor;
                            break;
                        }
                    case "pnlComments":
                        {
                            pnlComments.Tag = e.HexColor;
                            break;
                        }
                    case "pnlIdentifier":
                        {
                            pnlIdentifier.Tag = e.HexColor;
                            break;
                        }
                    case "pnlNumber":
                        {
                            pnlNumber.Tag = e.HexColor;
                            break;
                        }
                    case "pnlOperatorSymbol":
                        {
                            pnlOperatorSymbol.Tag = e.HexColor;
                            break;
                        }
                    case "pnlOperatorKeywords":
                        {
                            pnlOperatorKeywords.Tag = e.HexColor;
                            break;
                        }
                    case "pnlString":
                        {
                            pnlString.Tag = e.HexColor;
                            break;
                        }
                    case "pnlCharacter":
                        {
                            pnlCharacter.Tag = e.HexColor;
                            break;
                        }
                    case "pnlBuiltInFunctions":
                        {
                            pnlBuiltInFunctions.Tag = e.HexColor;
                            break;
                        }
                    case "pnlBuiltInKeywords":
                        {
                            pnlBuiltInKeywords.Tag = e.HexColor;
                            break;
                        }
                    case "pnlUserDefinedKeywords":
                        {
                            pnlUserDefinedKeywords.Tag = e.HexColor;
                            break;
                        }
                    case "pnlWhiteSpace":
                        {
                            editor.SetWhitespaceForeColor(true, e.Color);
                            pnlWhiteSpace.Tag = e.HexColor;
                            break;
                        }
                    case "pnlUserTables":
                        {
                            pnlUserTables.Tag = e.HexColor;
                            break;
                        }
                    case "pnlUserFunctions":
                        {
                            pnlUserFunctions.Tag = e.HexColor;
                            break;
                        }
                    case "pnlHighlightForeColor":
                        {
                            pnlHighlightForeColor.Tag = e.HexColor;
                            HighlightPreview();
                            break;
                        }
                }

                ApplySqlStyler("editor");

                break;
            }
        }

        private void HighlightPreview()
        {
            if (string.IsNullOrEmpty(editor.Text) || string.IsNullOrEmpty(cboHighlightStyle.Text) || string.IsNullOrEmpty(cboHighlightOutlineAlpha.Text) || string.IsNullOrEmpty(cboHighlightAlpha.Text))
            {
                return;
            }

            //Indicators 0-7 could be in use by a lexer, so we'll use indicator 8 to highlight words.
            const int num = 8;

            //Remove all uses of our indicator
            editor.IndicatorCurrent = num;
            editor.IndicatorClearRange(0, editor.TextLength);

            //Update indicator appearance
            UpdateHighlightPreview(cboHighlightStyle.Text, num);

            editor.Indicators[num].Under = true;
            editor.Indicators[num].ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlHighlightForeColor.Tag));
            editor.Indicators[num].OutlineAlpha = Convert.ToInt16(cboHighlightOutlineAlpha.Text);
            editor.Indicators[num].Alpha = Convert.ToInt16(cboHighlightAlpha.Text);

            //Search the document
            editor.TargetStart = 0;
            editor.TargetEnd = editor.TextLength;
            editor.SearchFlags = ScintillaNET.SearchFlags.None;

            var matches = Regex.Matches(editor.Text, "Employee");

            foreach (Match m in matches)
            {
                //Mark the search results with the current indicator
                editor.IndicatorFillRange(m.Index, "Employee".Length);
            }
        }

        private void UpdateHighlightPreview(string sStyle, int num)
        {
            //Update indicator appearance
            switch (sStyle)
            {
                case "Box":
                    {
                        editor.Indicators[num].Style = ScintillaNET.IndicatorStyle.Box;
                        break;
                    }
                case "CompositionThick":
                    {
                        editor.Indicators[num].Style = ScintillaNET.IndicatorStyle.CompositionThick;
                        break;
                    }
                case "Dash":
                    {
                        editor.Indicators[num].Style = ScintillaNET.IndicatorStyle.Dash;
                        break;
                    }
                case "Diagonal":
                    {
                        editor.Indicators[num].Style = ScintillaNET.IndicatorStyle.Diagonal;
                        break;
                    }
                case "StraightBox":
                    {
                        editor.Indicators[num].Style = ScintillaNET.IndicatorStyle.StraightBox;
                        break;
                    }
            }
        }

        private void pnlSelectedGridClick(object sender, EventArgs e)
        {
            if (!(sender is Panel pnlSelected))
            {
                return;
            }

            _panelColorSelectedName = pnlSelected.Name;
            ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ApplyGridColor);
        }

        private void ApplyGridColor(Color selectedColor)
        {
            var e = new { Color = selectedColor, HexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor) };

            for (var i = 0; i < _lstPanelGridColor.Count; i++)
            {
                if (((Panel)_lstPanelGridColor[i]).Name != _panelColorSelectedName)
                {
                    continue;
                }

                ((Panel)_lstPanelGridColor[i]).BackColor = e.Color;
                ((Panel)_lstPanelGridColor[i]).Tag = e.HexColor;
                _toolTip1.SetToolTip((Panel)_lstPanelGridColor[i], $"{e.HexColor} (R:{e.Color.R}, G:{e.Color.G}, B:{e.Color.B})");

                switch (((Panel)_lstPanelGridColor[i]).Name)
                {
                    case "pnlNullValueForeColor":
                        {
                            pnlNullValueForeColor.Tag = e.HexColor;
                            CreateVisualStyleInfo();
                            break;
                        }
                    case "pnlGridHeadingForeColor":
                        {
                            pnlGridHeadingForeColor.Tag = e.HexColor;
                            c1GridVisualStyle.HeadingStyle.ForeColor = e.Color;
                            break;
                        }
                    case "pnlGridEvenRowForeColor":
                        {
                            pnlGridEvenRowForeColor.Tag = e.HexColor;
                            AlternatingRowColorSetting();
                            break;
                        }
                    case "pnlGridEvenRowBackColor":
                        {
                            pnlGridEvenRowBackColor.Tag = e.HexColor;
                            AlternatingRowColorSetting();
                            break;
                        }
                    case "pnlGridOddRowForeColor":
                        {
                            pnlGridOddRowForeColor.Tag = e.HexColor;
                            AlternatingRowColorSetting();
                            break;
                        }
                    case "pnlGridOddRowBackColor":
                        {
                            pnlGridOddRowBackColor.Tag = e.HexColor;
                            AlternatingRowColorSetting();
                            break;
                        }
                    case "pnlGridHighlightForeColor":
                        {
                            pnlGridHighlightForeColor.Tag = e.HexColor;

                            if (!string.IsNullOrWhiteSpace(cboFindGrid.Text))
                            {
                                btnHighlightAllGrid.PerformClick();
                            }

                            break;
                        }
                    case "pnlGridHighlightBackColor":
                        {
                            pnlGridHighlightBackColor.Tag = e.HexColor;

                            if (!string.IsNullOrWhiteSpace(cboFindGrid.Text))
                            {
                                btnHighlightAllGrid.PerformClick();
                            }

                            break;
                        }
                    case "pnlGridSelectedForeColor":
                        {
                            pnlGridSelectedForeColor.Tag = e.HexColor;
                            c1GridVisualStyle.SelectedStyle.ForeColor = e.Color;
                            break;
                        }
                    case "pnlGridSelectedBackColor":
                        {
                            pnlGridSelectedBackColor.Tag = e.HexColor;
                            c1GridVisualStyle.SelectedStyle.BackColor = e.Color;
                            break;
                        }
                }

                break;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CancelApplyAndCloseOptionsForm();

            //20191013 關閉 - 要透過 MainForm 才行, 否則 SubForm 關了, TabControl 沒關！
            //Dispose vs Close 的比較 → https://www.alwaysgetbetter.com/blog/2008/04/04/c-formclose-vs-formdispose/
            //this.Dispose();
            //this.Close();

            TransferValueToMainForm("CloseOptionsTab`");
        }

        private void CancelApplyAndCloseOptionsForm()
        {
            //使用者可能有調整顏色，但直接按 Close 關閉 (不儲存)，以免新開啟的 SQL Editor 會套用到不相干的設定值

            //Query Editor 頁籤：Editor Color
            MyLibrary.ColorEditorBackground = TextHelper.GetSafeString(lblEditorBackground.Tag);
            MyLibrary.ColorCurrentLineBackground = TextHelper.GetSafeString(lblCurrentLineBackground.Tag);
            MyLibrary.ColorSelectedTextBackground = TextHelper.GetSafeString(lblSelectedTextBackground.Tag);
            MyLibrary.ColorComments = TextHelper.GetSafeString(lblComments.Tag);
            MyLibrary.ColorTextIdentifier = TextHelper.GetSafeString(lblIdentifier.Tag);
            MyLibrary.ColorBuiltInKeywords = TextHelper.GetSafeString(lblBuiltInKeywords.Tag);
            MyLibrary.ColorUserDefinedKeywords = TextHelper.GetSafeString(lblUserDefinedKeywords.Tag);
            MyLibrary.ColorNumber = TextHelper.GetSafeString(lblNumber.Tag);
            MyLibrary.ColorOperatorSymbol = TextHelper.GetSafeString(lblOperatorSymbol.Tag);
            MyLibrary.ColorOperatorKeywords = TextHelper.GetSafeString(lblOperatorKeywords.Tag);
            MyLibrary.ColorString = TextHelper.GetSafeString(lblString.Tag);
            MyLibrary.ColorCharacter = TextHelper.GetSafeString(lblCharacter.Tag);
            MyLibrary.ColorBuiltInFunctions = TextHelper.GetSafeString(lblBuiltInFunctions.Tag);
            MyLibrary.ColorWhiteSpace = TextHelper.GetSafeString(lblWhiteSpace.Tag);
            MyLibrary.ColorUserDefinedTablesViews = TextHelper.GetSafeString(lblUserTables.Tag);
            MyLibrary.ColorUserDefinedFunctionsTriggers = TextHelper.GetSafeString(lblUserFunctions.Tag);

            //Query Editor 頁籤：Highlight (Apply & Close 不變更全域變數的值，故此處不用還原)
            //MyLibrary.HighlightColorForeColor = TextHelper.GetSafeString(lblHighlightColorForeColor.Tag);
            //MyLibrary.HighlightColorStyle = TextHelper.GetSafeString(lblHighlightColorStyle.Tag);
            //MyLibrary.HighlightColorOutlineAlpha = TextHelper.GetSafeString(lblHighlightColorOutlineAlpha.Tag);
            //MyLibrary.HighlightColorAlpha = TextHelper.GetSafeString(lblHighlightColorAlpha.Tag);

            //Query Editor 頁籤：Preferences
            MyLibrary.QueryEditorFontName = TextHelper.GetSafeString(lblEditorFontName.Tag);
            MyLibrary.SetQueryEditorFontSizeFromText(TextHelper.GetSafeString(lblEditorFontSize.Tag)); //20260531 修改取值方法
            MyLibrary.SetQueryEditorZoomFromText(TextHelper.GetSafeString(lblEditorZoom.Tag)); //20260531 修改取值方法
            MyLibrary.KeywordFontBold = TextHelper.GetSafeString(chkBold.Tag) == "1";
            MyLibrary.CopyAsHTML = TextHelper.GetSafeString(chkCopyAsHTML.Tag) == "1";
            MyLibrary.ShowAllCharacters = TextHelper.GetSafeString(chkShowAllCharacters.Tag) == "1";
            MyLibrary.EntireBlankRowAsEmptyRow = TextHelper.GetSafeString(chkEntireBlankRowAsEmptyRow4SelectBlock.Tag) == "1";
            MyLibrary.HighlightSelection = TextHelper.GetSafeString(chkHighlightSelection.Tag) == "1";

            //Auto Replace 頁籤：直接關閉，不需任何動作 (重啟程式才會生效)
            //Data Grid 頁籤：直接關閉，不需任何動作 (重啟程式才會生效)
            //Keywords 頁籤：直接關閉，不需任何動作 (重啟程式才會生效)

            //SQL To Code 頁籤：
            MyLibrary.SqlToCodeSqlVariableName = TextHelper.GetSafeString(txtSqlVariableName.Tag);
            MyLibrary.SqlToCodeStringBuilderVariableName = TextHelper.GetSafeString(txtStringBuilderVariableName.Tag);

            if (TextHelper.GetSafeString(cboLocalization.Tag) == cboLocalization.Text)
            {
                return;
            }

            LocalizationHelper.Localization = TextHelper.GetSafeString(cboLocalization.Tag);
            TransferValueToMainForm("ReloadLocalization`");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //取得顏色的 HTML 代碼
            var mycolor = Color.YellowGreen;
            var strcol = ColorTranslator.ToHtml(Color.FromArgb(mycolor.ToArgb()));

            #region demo
            //////ToolStripMenuItem tsMenuItem40 = new ToolStripMenuItem();

            //////tsMenuItem40.Name = "tsMenuItem40";
            //////tsMenuItem40.Size = new Size(152, 22);
            //////tsMenuItem40.Text = "Test40";
            //////tsMenuItem40.Click += new EventHandler(tsMenuItem40_Click);

            //////ToolStripMenuItem tsProdMenuItem11 = new ToolStripMenuItem();

            //////tsProdMenuItem11.Name = "tsProdMenuItem11";
            //////tsProdMenuItem11.Size = new Size(152, 22);
            //////tsProdMenuItem11.Text = "Prod11";
            //////tsProdMenuItem11.Click += new EventHandler(tsProdMenuItem11_Click);

            //////tsCopyFrom.DropDownItems.AddRange(new ToolStripItem[] { tsMenuItem40, tsProdMenuItem11 });
            #endregion

            btnCopySettings.Items.Clear();

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM DBInfo");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.Append($"   AND PID <> {JasonQueryRepository.DbMotherPid}");

            var sql = sbSql.ToString();
            var dtTemp = JasonQueryRepository.ExecQuery(sql);

            if (dtTemp.Rows.Count <= 0)
            {
                return;
            }

            pnlCopySettings.Visible = true;

            for (var i = 0; i < dtTemp.Rows.Count; i++)
            {
                var dr = dtTemp.Rows[i];
                var sDataSource2 = dr.GetSafeString("DataSource");
                var sConnectionName2 = dr.GetSafeString("ConnectionName");
                var dropDownItem = new C1.Win.C1Input.DropDownItem();

                btnCopySettings.Items.Add(dropDownItem);
                btnCopySettings.Items[i].Tag = "123";
                btnCopySettings.Items[i].Text = $"From \"({sDataSource2}){sConnectionName2}\" To \"({DatabaseSqlExecutor.DataSourceDisplayName}){DatabaseSqlExecutor.DbConnectionName}\"";
                btnCopySettings.DropDownItemClicked += btnCopySettings_DropDownItemClicked;

                //以下是 VS 的 DropDownItems 使用範例
                //tsCopyFrom.DropDownItems.Add("tsMenuItem" + i.ToString());
                //tsCopyFrom.DropDownItems[i].AccessibleName = "from";
                //tsCopyFrom.DropDownItems[i].AccessibleDescription = "to";
                //tsCopyFrom.DropDownItems[i].AccessibleDefaultActionDescription = string.Empty;
                //tsCopyFrom.DropDownItems[i].Text = "Copy settings from \"" + dtTemp.Rows[i]["ConnectionName"] + "\" to \"" + DBState.sDBConnectionName + "\"";
                //tsCopyFrom.DropDownItems[i].Click += new EventHandler(tsMenuItem_Click);
            }

            //tsCopyFrom.DropDownItems.Add("tsProdMenuItem11");
            ////tsCopyFrom.DropDownItems[1].Size = new Size(152, 22);
            //tsCopyFrom.DropDownItems[1].Text = "Copy settings from \"Prod11\" to \"那個\"";
            //tsCopyFrom.DropDownItems[1].Click += new EventHandler(tsMenuItem_Click);

            //editor.WrapVisualFlags = (chkStart.Checked ? ScintillaNET.WrapVisualFlags.Start : ScintillaNET.WrapVisualFlags.None) | (chkEnd.Checked ? ScintillaNET.WrapVisualFlags.End : ScintillaNET.WrapVisualFlags.None) | (chkMargin.Checked ? ScintillaNET.WrapVisualFlags.Margin : ScintillaNET.WrapVisualFlags.None);
        }

        private void btnCopySettings_DropDownItemClicked(object sender, C1.Win.C1Input.DropDownItemClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.ClickedItem.Text))
            {
                return;
            }

            _languageText = LocalizationHelper.GetLanguageString("Are you sure you want to copy all settings?", "Global", "Global", "msg", "CopySettingsFrom", "Text");

            var parts = e.ClickedItem.Text.Split(new[] { " " }, StringSplitOptions.None);
            var sTemp = parts.Aggregate(string.Empty, (current, t) => $"{current}{t}\r\n");

            _languageText += $"\r\n\r\n{sTemp}";

            var result = MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            var sbSql = new StringBuilder();

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.Append("   AND AttributeKey <> 'GlobalConfig'");

            var sql = sbSql.ToString();

            //刪除本身的設定
            JasonQueryRepository.ExecNonQuery(sql);

            //複製選定的 Profile 設定
            sbSql.Clear();
            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine("      (MPID, DomainUser, AttributeKey, AttributeName, AttributeValue, AttributeDate, AttributeText, AttributeText2)");
            sbSql.AppendLine($"SELECT {JasonQueryRepository.DbMotherPid} AS MPID, DomainUser, AttributeKey, AttributeName, AttributeValue, AttributeDate, AttributeText, AttributeText2");
            sbSql.AppendLine("  FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {TextHelper.GetSafeString(e.ClickedItem.Tag)}");
            sbSql.Append("   AND [AttributeKey] <> 'GlobalConfig';");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            _languageText = LocalizationHelper.GetLanguageString("The settings have been copied. Please restart JasonQuery!", "Global", "Global", "msg", "CopyOK", "Text");
            MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            var temp1 = LocalizationHelper.GetLanguageString("All settings have been changed.", "Global", "Global", "msg", "SettingsChanged", "Text");
            var temp2 = LocalizationHelper.GetLanguageString("You need to restart JasonQuery!", "Global", "Global", "msg", "RequireToRestart", "Text");

            MyGlobal.RequireToRestart = $"{temp1}\r\n{temp2}";
            btnApply.Enabled = false;
            btnRestoreDefaults.Enabled = false;
            btnCopySettings.Enabled = false;
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            _languageText = LocalizationHelper.GetLanguageString("Are you sure you want to restore all default settings except \"Global\"? ", "Global", "Global", "msg", "RestoreDefault", "Text");

            var result = MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            var sbSql = new StringBuilder();

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.Append("   AND AttributeKey <> 'GlobalConfig'");

            var sql = sbSql.ToString();

            JasonQueryRepository.ExecNonQuery(sql);

            btnApply.Enabled = false;
            pnlCopySettings.Enabled = false;

            _languageText = LocalizationHelper.GetLanguageString("The settings have been restored. Please restart JasonQuery!", "Global", "Global", "msg", "RestoreOK", "Text");
            MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            var sTemp1 = LocalizationHelper.GetLanguageString("All settings have been changed.", "Global", "Global", "msg", "SettingsChanged", "Text");
            var sTemp2 = LocalizationHelper.GetLanguageString("You need to restart JasonQuery!", "Global", "Global", "msg", "RequireToRestart", "Text");

            MyGlobal.RequireToRestart = $"{sTemp1}\r\n{sTemp2}";
            btnApply.Enabled = false;
            btnRestoreDefaults.Enabled = false;
            btnCopySettings.Enabled = false;
        }

        private void editor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            editor.ContextMenuStrip = _cMenu;

            if (MyLibrary.IsDarkMode)
            {
                _cMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _cMenu.ForeColor = Color.White;
                _cMenu.RenderMode = ToolStripRenderMode.System;
                //_cMenu.ShowImageMargin = false;
            }

            _cMenu.Show(editor, new Point(e.X, e.Y));
        }

        private void cboWordWrapIndentMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            var sMode = TextHelper.GetKeyFromDictionary(MyGlobal.dicWordWrapIndentMode, cboIndentMode.Text);

            switch (sMode)
            {
                case "Fixed":
                    {
                        editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Fixed;
                        break;
                    }
                case "Indent":
                    {
                        editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Indent;
                        break;
                    }
                default:
                    {
                        editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
                        break;
                    }
            }
        }

        private void editor_ZoomChanged(object sender, EventArgs e)
        {
            //以下，當放大縮小後，即時調整 line number 的寬度，避免因為放大時，line number 最左側的數字會看不見
            var iStart = editor.SelectionStart;

            editor.Text += "\r\n";
            editor.Text = editor.Text.Substring(0, editor.Text.Length - 2);
            editor.SelectionStart = iStart;
            editor.ScrollCaret();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (!ValidateUpdateMetadataSettings())
            {
                return;
            }

            Cursor = Cursors.WaitCursor;

            btnRestoreDefaults.Enabled = false;
            btnCopySettings.Enabled = false;
            btnApply.Enabled = false;
            btnClose.Enabled = false;

            if (!MyLibrary.IsDarkMode && chkDarkMode.Checked)
            {
                pnlGridHeadingForeColor.Tag = "#FFFFFF";
                pnlGridEvenRowForeColor.Tag = "#FFFFFF";
                pnlGridEvenRowBackColor.Tag = "#0F243E";
                pnlGridOddRowForeColor.Tag = "#FFFFFF";
                pnlGridOddRowBackColor.Tag = "#262626";
                pnlNullValueForeColor.Tag = "#FFFF00";

                pnlOptionsTabActiveForeColor.Tag = "#000000";
                pnlOptionsTabActiveBackColor.Tag = "#E6FFFF";

                pnlToolstripBackground.Tag = "#3F3F3F";
                pnlEditorBackground.Tag = "#2D2D30";
                pnlCurrentLineBackground.Tag = "#7F7F7F";
                pnlSelectedTextBackground.Tag = "#639ACE";
                pnlComments.Tag = "#00FF00";
                pnlIdentifier.Tag = "#FFFFFF";
                pnlNumber.Tag = "#FF00FF";
                pnlOperatorSymbol.Tag = "#FF00FF";
                pnlOperatorKeywords.Tag = "#00FFFF";
                pnlString.Tag = "#FFFF00";
                pnlCharacter.Tag = "#FFFF00";
                pnlBuiltInFunctions.Tag = "#FFC000";
                pnlBuiltInKeywords.Tag = "#00FFFF";
                pnlUserDefinedKeywords.Tag = "#00FFFF";
                pnlWhiteSpace.Tag = "#7030A0";
                pnlHighlightForeColor.Tag = "#FFFF00";
                pnlUserTables.Tag = "#9BBB59";
                pnlUserFunctions.Tag = "#9BBB59";

                cboGridVisualStyle.Text = @"Office 2010 Black";
            }
            else if (MyLibrary.IsDarkMode && !chkDarkMode.Checked)
            {
                pnlGridHeadingForeColor.Tag = "#000000";
                pnlGridEvenRowForeColor.Tag = "#000000";
                pnlGridEvenRowBackColor.Tag = "#FFFFFF";
                pnlGridOddRowForeColor.Tag = "#000000";
                pnlGridOddRowBackColor.Tag = "#FFFFC1";
                pnlNullValueForeColor.Tag = "#0000FF";

                pnlOptionsTabActiveForeColor.Tag = "#000000";
                pnlOptionsTabActiveBackColor.Tag = "#E6FFFF";
                pnlOptionsTabInactiveForeColor.Tag = "#595959";

                pnlToolstripBackground.Tag = "#E3FDCA";
                pnlEditorBackground.Tag = "#FFFFFF";
                pnlCurrentLineBackground.Tag = "#FFFFE0";
                pnlSelectedTextBackground.Tag = "#ADD8E6";
                pnlComments.Tag = "#008000";
                pnlIdentifier.Tag = "#000000";
                pnlNumber.Tag = "#800000";
                pnlOperatorSymbol.Tag = "#800000";
                pnlOperatorKeywords.Tag = "#366092";
                pnlString.Tag = "#FF0000";
                pnlCharacter.Tag = "#FF0000";
                pnlBuiltInFunctions.Tag = "#FF00FF";
                pnlBuiltInKeywords.Tag = "#0000FF";
                pnlUserDefinedKeywords.Tag = "#0000FF";
                pnlWhiteSpace.Tag = "#00FFFF";
                pnlHighlightForeColor.Tag = "#000000";
                pnlUserTables.Tag = "#808000";
                pnlUserFunctions.Tag = "#808000";

                cboGridVisualStyle.Text = @"Office 2010 Blue";
            }

            _isApplyAndClose = true;

            //儲存設定：General
            #region 儲存設定：General
            JasonQueryRepository.UpdateSetting("GeneralConfig", "DarkMode", chkDarkMode.Checked ? "1" : "0");
            MyLibrary.IsDarkMode = chkDarkMode.Checked;

            JasonQueryRepository.UpdateSetting("GeneralConfig", "SpecifiedSQLFile1", txtSpecifiedSQLFile1.Text);
            JasonQueryRepository.UpdateSetting("GeneralConfig", "SpecifiedSQLFile2", txtSpecifiedSQLFile2.Text);
            #endregion

            //儲存設定：Query Editor
            #region 儲存設定：Query Editor
            JasonQueryRepository.UpdateSetting("EditorConfig", "ToolstripBackground", TextHelper.GetSafeString(pnlToolstripBackground.Tag));
            MyLibrary.ColorToolstripBackground = TextHelper.GetSafeString(pnlToolstripBackground.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "EditorBackground", TextHelper.GetSafeString(pnlEditorBackground.Tag));
            MyLibrary.ColorEditorBackground = TextHelper.GetSafeString(pnlEditorBackground.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "CurrentLineBackground", TextHelper.GetSafeString(pnlCurrentLineBackground.Tag));
            MyLibrary.ColorCurrentLineBackground = TextHelper.GetSafeString(pnlCurrentLineBackground.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "SelectedTextBackground", TextHelper.GetSafeString(pnlSelectedTextBackground.Tag));
            MyLibrary.ColorSelectedTextBackground = TextHelper.GetSafeString(pnlSelectedTextBackground.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "ErrorLineBackground", TextHelper.GetSafeString(pnlErrorLineBackground.Tag));
            JasonQueryRepository.UpdateSetting("EditorConfig", "BookmarkBackground", TextHelper.GetSafeString(pnlBookmarkBackground.Tag));

            JasonQueryRepository.UpdateSetting("EditorConfig", "BookmarkStyle", TextHelper.GetKeyFromDictionary(MyGlobal.dicWordWrapIndentMode, cboBookmarkStyle.Text));

            JasonQueryRepository.UpdateSetting("EditorConfig", "Comments", TextHelper.GetSafeString(pnlComments.Tag));
            MyLibrary.ColorComments = TextHelper.GetSafeString(pnlComments.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "TextIdentifier", TextHelper.GetSafeString(pnlIdentifier.Tag));
            MyLibrary.ColorTextIdentifier = TextHelper.GetSafeString(pnlIdentifier.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "BuiltInKeywords", TextHelper.GetSafeString(pnlBuiltInKeywords.Tag));
            MyLibrary.ColorBuiltInKeywords = TextHelper.GetSafeString(pnlBuiltInKeywords.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "UserDefinedKeywords", TextHelper.GetSafeString(pnlUserDefinedKeywords.Tag));
            MyLibrary.ColorUserDefinedKeywords = TextHelper.GetSafeString(pnlUserDefinedKeywords.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "Number", TextHelper.GetSafeString(pnlNumber.Tag));
            MyLibrary.ColorNumber = TextHelper.GetSafeString(pnlNumber.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "OperatorSymbol", TextHelper.GetSafeString(pnlOperatorSymbol.Tag));
            MyLibrary.ColorOperatorSymbol = TextHelper.GetSafeString(pnlOperatorSymbol.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "OperatorKeywords", TextHelper.GetSafeString(pnlOperatorKeywords.Tag));
            MyLibrary.ColorOperatorKeywords = TextHelper.GetSafeString(pnlOperatorKeywords.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "String", TextHelper.GetSafeString(pnlString.Tag));
            MyLibrary.ColorString = TextHelper.GetSafeString(pnlString.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "Character", TextHelper.GetSafeString(pnlCharacter.Tag));
            MyLibrary.ColorCharacter = TextHelper.GetSafeString(pnlCharacter.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "BuiltinFunctions", TextHelper.GetSafeString(pnlBuiltInFunctions.Tag));
            MyLibrary.ColorBuiltInFunctions = TextHelper.GetSafeString(pnlBuiltInFunctions.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "BuiltInKeywords", TextHelper.GetSafeString(pnlBuiltInKeywords.Tag));
            MyLibrary.ColorBuiltInKeywords = TextHelper.GetSafeString(pnlBuiltInKeywords.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "WhiteSpace", TextHelper.GetSafeString(pnlWhiteSpace.Tag));
            MyLibrary.ColorWhiteSpace = TextHelper.GetSafeString(pnlWhiteSpace.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "UserDefinedTables", TextHelper.GetSafeString(pnlUserTables.Tag));
            MyLibrary.ColorUserDefinedTablesViews = TextHelper.GetSafeString(pnlUserTables.Tag);

            JasonQueryRepository.UpdateSetting("EditorConfig", "UserDefinedFunctions", TextHelper.GetSafeString(pnlUserFunctions.Tag));
            MyLibrary.ColorUserDefinedFunctionsTriggers = TextHelper.GetSafeString(pnlUserFunctions.Tag);

            //Query Editor 頁籤：Highlight
            //20191016 Highlight 不要動態變更，故不變更全域變數的值
            JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightForeColor", TextHelper.GetSafeString(pnlHighlightForeColor.Tag));
            JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightStyle", cboHighlightStyle.Text);
            JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightOutlineAlpha", cboHighlightOutlineAlpha.Text);
            JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightAlpha", cboHighlightAlpha.Text);

            //Query Editor 頁籤：Preferences
            JasonQueryRepository.UpdateSetting("EditorConfig", "EditorFontName", cboEditorFontPicker.Text);
            MyLibrary.QueryEditorFontName = cboEditorFontPicker.Text;

            JasonQueryRepository.UpdateSetting("EditorConfig", "EditorFontSize", cboEditorFontSize.Text);
            MyLibrary.SetQueryEditorFontSizeFromText(cboEditorFontSize.Text); //20260531 修改取值方法

            JasonQueryRepository.UpdateSetting("EditorConfig", "EditorZoom", cboEditorZoom.Text);
            MyLibrary.SetQueryEditorZoomFromText(cboEditorZoom.Text); //20260531 修改取值方法

            JasonQueryRepository.UpdateSetting("EditorConfig", "WordWrap", chkWordWrap.Checked ? "1" : "0");
            MyLibrary.WordWrap = chkWordWrap.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "WordWrapVisualFlags_Start", chkStart.Checked ? "1" : "0");
            MyLibrary.WordWrapVisualFlags_Start = chkStart.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "WordWrapVisualFlags_End", chkEnd.Checked ? "1" : "0");
            MyLibrary.WordWrapVisualFlags_End = chkEnd.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "WordWrapVisualFlags_Margin", chkMargin.Checked ? "1" : "0");
            MyLibrary.WordWrapVisualFlags_Margin = chkMargin.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "WordWrapIndentMode", cboIndentMode.Text);
            MyGlobal.WordWrapIndentMode = TextHelper.GetKeyFromDictionary(MyGlobal.dicWordWrapIndentMode, cboIndentMode.Text);

            JasonQueryRepository.UpdateSetting("EditorConfig", "TabWidth", cboTabWidth.Text);
            MyGlobal.TabWidth = Convert.ToInt16(cboTabWidth.Text);

            JasonQueryRepository.UpdateSetting("EditorConfig", "KeywordFontBold", chkBold.Checked ? "1" : "0");
            MyLibrary.KeywordFontBold = chkBold.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "CopyAsHTML", chkCopyAsHTML.Checked ? "1" : "0");
            MyLibrary.CopyAsHTML = chkCopyAsHTML.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "ShowAllCharacters", chkShowAllCharacters.Checked ? "1" : "0");
            MyLibrary.ShowAllCharacters = chkShowAllCharacters.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "ShowSaveAsButton", chkShowSaveAsButton.Checked ? "1" : "0");
            MyLibrary.ShowSaveAsButton = chkShowSaveAsButton.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "ShowIndentGuide", chkShowIndentGuide.Checked ? "1" : "0");
            MyLibrary.ShowIndentGuide = chkShowIndentGuide.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "EntireBlankRowAsEmptyRow4SelectBlock", chkEntireBlankRowAsEmptyRow4SelectBlock.Checked ? "1" : "0");
            MyLibrary.EntireBlankRowAsEmptyRow = chkEntireBlankRowAsEmptyRow4SelectBlock.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightSelection", chkHighlightSelection.Checked ? "1" : "0");
            MyLibrary.HighlightSelection = chkHighlightSelection.Checked;

            JasonQueryRepository.UpdateSetting("EditorConfig", "SortByColumnName", chkSortByColumnName.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("EditorConfig", "ShowColumnInfo", chkShowColumnInfo.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("EditorConfig", "DefaultTabSchemaBrowser", chkDefaultTabSchemaInformation.Checked ? "1" : "0");

            JasonQueryRepository.UpdateSetting("EditorConfig", "AutoListMembers", chkAutoListMembers.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("EditorConfig", "SavePoint", chkSavePoint.Checked ? "1" : "0");
            #endregion

            //儲存設定：Auto Complete
            #region 儲存設定：Auto Complete
            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "EnableAutoComplete2", chkEnableAutoComplete.Checked ? "1" : "0");
            MyLibrary.EnableAutoComplete = chkEnableAutoComplete.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "MinFragmentLength2", nudMinFragmentLength.Text);
            MyLibrary.AutoCompleteMinFragmentLength = (int)nudMinFragmentLength.Value;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "FirstCharChecking2", chkFirstCharChecking.Checked ? "1" : "0");
            MyLibrary.AutoCompleteFirstCharChecking = chkFirstCharChecking.Checked;

            //Built-In Keywords && Functions
            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "BuiltInKeywords2", chkBuiltInKeywords.Checked ? "1" : "0");
            MyLibrary.AutoCompleteBuiltInKeywords = chkBuiltInKeywords.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "BuiltInFunctions2", chkBuiltInFunctions.Checked ? "1" : "0");
            MyLibrary.AutoCompleteBuiltInFunctions = chkBuiltInFunctions.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "UserDefinedKeywords2", chkUserDefinedKeywords.Checked ? "1" : "0");
            MyLibrary.AutoCompleteUserDefinedKeywords = chkUserDefinedKeywords.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "UserDefinedFunctions2", chkUserDefinedFunctions.Checked ? "1" : "0");
            MyLibrary.AutoCompleteUserDefinedFunctions = chkUserDefinedFunctions.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "UserDefinedTables2", chkUserDefinedTables.Checked ? "1" : "0");
            MyLibrary.AutoCompleteUserDefinedTables = chkUserDefinedTables.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "UserDefinedTriggers2", chkUserDefinedTriggers.Checked ? "1" : "0");
            MyLibrary.AutoCompleteUserDefinedTriggers = chkUserDefinedTriggers.Checked;

            JasonQueryRepository.UpdateSetting("AutoCompleteConfig", "UserDefinedViews2", chkUserDefinedViews.Checked ? "1" : "0");
            MyLibrary.AutoCompleteUserDefinedViews = chkUserDefinedViews.Checked;
            #endregion

            //儲存設定：Auto Replace
            #region 儲存設定：Auto Replace
            JasonQueryRepository.UpdateSetting("AutoReplaceConfig", "EnableAutoReplace", chkEnableAutoReplace.Checked ? "1" : "0");
            MyLibrary.EnableAutoReplace = chkEnableAutoReplace.Checked;

            var sbSql = new StringBuilder();

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'AutoReplaceConfig'");
            sbSql.Append("   AND AttributeName = 'AutoReplace'");

            var sql = sbSql.ToString();
            var temp = string.Empty;

            //刪除所有的 Auto Replace，再重新 Insert/Update
            JasonQueryRepository.ExecNonQuery(sql);

            foreach (DataRow oRow in _dtAutoReplaceInfo.Rows)
            {
                var sKeyword = oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].ToString();
                var sReplacement = oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]].ToString();

                temp = $"{sKeyword}{MyGlobal.Separator3s}{sReplacement}";
                JasonQueryRepository.UpdateSetting("AutoReplaceConfig", "AutoReplace", temp, true);
            }
            #endregion

            //儲存設定：Data Grid
            #region 儲存設定：Data Grid
            JasonQueryRepository.UpdateSetting("GridConfig", "QuotingWith", cboResultCopyQuotingWith.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "ShowColumnDataType", chkShowColumnType.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "ShowFilterRow", chkShowFilterRow.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "ShowGroupingRow", chkShowGroupingRow.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "Resize", chkResize.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "MaxWidth", cboMaxWidth.Text);
            int.TryParse(cboMaxWidth.Text, out GridHelper.MaxWidth);
            JasonQueryRepository.UpdateSetting("GridConfig", "ShowColumnComment", chkShowColumnComment.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "RawDataMode", chkRawDataMode.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "NullShowAs", cboNullShowAs.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "PagingQuery", chkPagedQuery.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "RowsPerPage", cboRowsPerPage.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "AppendingQueries", chkAppendQueryResult.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "Direction", TextHelper.GetKeyFromDictionary(MyGlobal.dicDirection, cboDirection.Text));
            JasonQueryRepository.UpdateSetting("GridConfig", "SetFocusAfterQuery", chkSetFocusAfterQuery.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GridConfig", "NullShowColor", TextHelper.GetSafeString(pnlNullValueForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "VisualStyle", cboGridVisualStyle.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "FontName", cboGridFontPicker.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "FontSize", cboGridFontSize.Text);
            JasonQueryRepository.UpdateSetting("GridConfig", "RowResizing", TextHelper.GetKeyFromDictionary(MyGlobal.dicRowSizing, cboGridRowHeightResizing.Text));

            //重新啟動，設定才會生效
            JasonQueryRepository.UpdateSetting("GridConfig", "HeadingForeColor", TextHelper.GetSafeString(pnlGridHeadingForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "EvenRowForeColor", TextHelper.GetSafeString(pnlGridEvenRowForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "EvenRowBackColor", TextHelper.GetSafeString(pnlGridEvenRowBackColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "OddRowForeColor", TextHelper.GetSafeString(pnlGridOddRowForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "OddRowBackColor", TextHelper.GetSafeString(pnlGridOddRowBackColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "HighlightForeColor", TextHelper.GetSafeString(pnlGridHighlightForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "HighlightBackColor", TextHelper.GetSafeString(pnlGridHighlightBackColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "SelectedForeColor", TextHelper.GetSafeString(pnlGridSelectedForeColor.Tag));
            JasonQueryRepository.UpdateSetting("GridConfig", "SelectedBackColor", TextHelper.GetSafeString(pnlGridSelectedBackColor.Tag));
            #endregion

            //儲存設定：Keywords
            #region 儲存設定：Keywords
            temp = string.IsNullOrWhiteSpace(editorOperatorKeywords.Text) ? string.Empty : $"{editorOperatorKeywords.Text.Trim().ToLower()} ";
            JasonQueryRepository.UpdateSetting("KeywordsConfig", "OperatorKeywords", temp, false, true);
            MyLibrary.KeywordsOperatorKeywords = editorOperatorKeywords.Text;

            temp = string.IsNullOrWhiteSpace(editorBuiltInFunctions.Text) ? string.Empty : $"{editorBuiltInFunctions.Text.Trim().ToLower()} ";
            JasonQueryRepository.UpdateSetting("KeywordsConfig", "BuiltInFunctions", temp, false, true);
            MyLibrary.KeywordsBuiltInFunctions = editorBuiltInFunctions.Text;

            temp = string.IsNullOrWhiteSpace(editorBuiltInKeywords.Text) ? string.Empty : $"{editorBuiltInKeywords.Text.Trim().ToLower()} ";
            JasonQueryRepository.UpdateSetting("KeywordsConfig", "BuiltInKeywords", temp, false, true);
            MyLibrary.KeywordsBuiltInKeywords = editorBuiltInKeywords.Text;

            temp = string.IsNullOrWhiteSpace(editorUserDefinedKeywords.Text) ? string.Empty : $"{editorUserDefinedKeywords.Text.Trim().ToLower()} ";
            JasonQueryRepository.UpdateSetting("KeywordsConfig", "UserDefinedKeywords", temp, false, true);
            MyLibrary.KeywordsUserDefinedKeywords = editorUserDefinedKeywords.Text;
            #endregion

            //儲存設定：SQL to Code
            #region 儲存設定：SQL to Code
            JasonQueryRepository.UpdateSetting("SQL2CodeConfig", "VariableName", txtSqlVariableName.Text);
            MyLibrary.SqlToCodeSqlVariableName = txtSqlVariableName.Text;
            JasonQueryRepository.UpdateSetting("SQL2CodeConfig", "StringBuilderVariableName", txtStringBuilderVariableName.Text);
            MyLibrary.SqlToCodeStringBuilderVariableName = txtStringBuilderVariableName.Text;
            #endregion

            //儲存設定：SQL Formatter
            #region 儲存設定：SQL Formatter
            var indentSize = GetSelectedSqlFormatterIndentSize();

            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "IndentSize", indentSize.ToString());
            MyLibrary.SqlFormatterIndentSize = indentSize;

            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "MaxLineWidth", txtMaxWidth.Text);
            MyLibrary.SqlFormatterMaxLineWidth = Convert.ToInt16(txtMaxWidth.Text);

            var blankLinesBetweenStatements = GetSelectedSqlFormatterBlankLinesBetweenStatements();

            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "BlankLinesBetweenStatements", blankLinesBetweenStatements.ToString());
            MyLibrary.SqlFormatterBlankLinesBetweenStatements = blankLinesBetweenStatements;

            var listItemsPerLine = GetSelectedSqlFormatterListItemsPerLine();

            JasonQueryRepository.UpdateSetting
            (
                SqlFormatterSettingsContract.SectionName,
                SqlFormatterSettingsContract.ListItemsPerLineSettingName,
                listItemsPerLine.ToString()
            );

            MyLibrary.SqlFormatterListItemsPerLine = listItemsPerLine;

            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "ConvertCaseForKeywords", chkConvertCaseForKeywords.Checked ? "1" : "0");
            MyLibrary.SqlFormatterConvertCaseForKeywords = chkConvertCaseForKeywords.Checked;

            var selectedCase = 1;

            if (rdoLowerCase.Checked)
            {
                selectedCase = 2;
            }
            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "ConvertCaseForKeywordsCase", selectedCase.ToString());
            MyLibrary.SqlFormatterConvertCaseForKeywordsCase = selectedCase;

            var selectedEngineKind = GetSelectedSqlFormatterEngineKind();

            JasonQueryRepository.UpdateSetting("SQLFormatterConfig", "EngineKind", selectedEngineKind.ToString());
            MyLibrary.SqlFormatterEngine = selectedEngineKind;
            #endregion

            //儲存設定：Global
            #region 儲存設定：Global
            //檢查更新
            JasonQueryRepository.UpdateSetting("GlobalConfig", "EnableCheckForUpdate", rdoCheckOnly.Checked ? "1" : "0");

            var updateMetadataSource = GetSelectedUpdateMetadataSource();
            var updateMetadataLocalFolder = txtLocalFolder.Text.Trim();

            JasonQueryRepository.UpdateSetting
            (
                UpdateMetadataSettingsContract.SectionName,
                UpdateMetadataSettingsContract.SourceSettingName,
                updateMetadataSource.ToString()
            );

            JasonQueryRepository.UpdateSetting
            (
                UpdateMetadataSettingsContract.SectionName,
                UpdateMetadataSettingsContract.LocalFolderSettingName,
                updateMetadataLocalFolder
            );

            MyLibrary.UpdateMetadataSource = updateMetadataSource;
            MyLibrary.UpdateMetadataLocalFolder = updateMetadataLocalFolder;

            if (rdoCheckForUpdates0.Checked)
            {
                selectedCase = 0;
            }
            else if (rdoCheckForUpdates1.Checked)
            {
                selectedCase = 1;
            }
            else
            {
                selectedCase = 7;
            }

            JasonQueryRepository.UpdateSetting("GlobalConfig", "CheckForUpdateDays", selectedCase.ToString());

            if (rdoMultiDocument.Checked)
            {
                temp = "MultiDocument";
            }
            else if (rdoMultiForm.Checked)
            {
                temp = "MultiForm";
            }
            else
            {
                temp = "MultiBox";
            }

            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabAppearance", temp);

            JasonQueryRepository.UpdateSetting("GlobalConfig", "RecentFilesQty", txtRecentFiles.Text);
            MyLibrary.RecentFilesQty = Convert.ToInt16(txtRecentFiles.Text);
            JasonQueryRepository.UpdateSetting("GlobalConfig", "MyFavoriteQty", txtMyFavorite.Text);
            MyLibrary.MyFavoriteQty = Convert.ToInt16(txtMyFavorite.Text);

            //202411129 儲存 Icon Style (重新啟動 JasonQuery 後才會生效，故此處以變數處理)
            var icon = "1";

            if (rdoCommitRollbackIconStyle2.Checked)
            {
                icon = "2";
            }
            else if (rdoCommitRollbackIconStyle3.Checked)
            {
                icon = "3";
            }
            else if (rdoCommitRollbackIconStyle4.Checked)
            {
                icon = "4";
            }
            else if (rdoCommitRollbackIconStyle5.Checked) //20260619 新增 Icon Style 5、6 新的圖示樣式
            {
                icon = "5";
            }
            else if (rdoCommitRollbackIconStyle6.Checked)
            {
                icon = "6";
            }

            JasonQueryRepository.UpdateSetting("GlobalConfig", "CommitRollbackIcon", icon);
            JasonQueryRepository.UpdateSetting("GlobalConfig", "BackupFile", chkEnableBackup.Checked ? "1" : "0");
            AppConfigHelper.IsBackupFile = chkEnableBackup.Checked;
            JasonQueryRepository.UpdateSetting("GlobalConfig", "AskMeBeforeOpenUnsavedFiles", chkAskMeBeforeOpenUnsavedFiles.Checked ? "1" : "0");
            AppConfigHelper.AskBeforeOpenUnsavedFiles = chkAskMeBeforeOpenUnsavedFiles.Checked;
            JasonQueryRepository.UpdateSetting("GlobalConfig", "BackupPath", txtBackupPath.Text);
            AppConfigHelper.BackupPath = txtBackupPath.Text;

            if (!AppConfigHelper.BackupPath.EndsWith("\\", StringComparison.Ordinal))
            {
                AppConfigHelper.BackupPath += "\\";
            }

            if (!Directory.Exists(AppConfigHelper.BackupPath))
            {
                try
                {
                    Directory.CreateDirectory(AppConfigHelper.BackupPath);
                }
                catch (Exception)
                {
                    AppConfigHelper.IsBackupFile = false;
                }
            }

            JasonQueryRepository.UpdateSetting("GlobalConfig", "DateFormat", cboDateFormat.Text);
            MyLibrary.DateFormat = cboDateFormat.Text;
            JasonQueryRepository.UpdateSetting("GlobalConfig", "ShowDatabaseName", chkShowDatabaseName.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "ShowVersion", chkShowVersion.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "ShowIP", chkShowIP.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "MainFormMaximized", rdoMaximized.Checked ? "1" : "0");

            JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabActiveForeColor", TextHelper.GetSafeString(pnlOptionsTabActiveForeColor.Tag));
            MyLibrary.ColorOptionsTabActiveForeColor = TextHelper.GetSafeString(pnlOptionsTabActiveForeColor.Tag);
            JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabActiveBackColor", TextHelper.GetSafeString(pnlOptionsTabActiveBackColor.Tag));
            MyLibrary.ColorOptionsTabActiveBackColor = TextHelper.GetSafeString(pnlOptionsTabActiveBackColor.Tag);
            JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabInactiveForeColor", TextHelper.GetSafeString(pnlOptionsTabInactiveForeColor.Tag));
            MyLibrary.ColorOptionsTabInactiveForeColor = TextHelper.GetSafeString(pnlOptionsTabInactiveForeColor.Tag);

            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabStyle", rdoIDE.Checked ? "IDE" : "Plain");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabBold", chkTabBold.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabShrinkPages", chkShrinkPages.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabShowArrows", chkShowArrows.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabHoverSelect", chkHoverSelect.Checked ? "1" : "0");
            JasonQueryRepository.UpdateSetting("GlobalConfig", "TabMultiLine", chkMultiLine.Checked ? "1" : "0");

            if (MyLibrary.IsDarkMode && TextHelper.GetSafeString(chkDarkMode.Tag) == "0" || MyLibrary.IsDarkMode && TextHelper.GetSafeString(chkDarkMode.Tag) != "1")
            {
                AppConfigHelper.IsChangeColorThemeNeedRestart = true;
            }

            if (!MyLibrary.IsDarkMode && TextHelper.GetSafeString(chkDarkMode.Tag) == "1")
            {
                _languageText = LocalizationHelper.GetLanguageString("Changing the color theme from dark to normal requires restarting JasonQuery for the best display.", "form", GetType().Name, "msg", "ChangeColorTheme", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (MyLibrary.IsDarkMode && TextHelper.GetSafeString(chkDarkMode.Tag) == "0")
            {
                _languageText = LocalizationHelper.GetLanguageString("Changing the color theme from normal to dark requires restarting JasonQuery for the best display.", "form", GetType().Name, "msg", "ChangeColorThemeN2D", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            //20260620 改為全域變數
            JasonQueryRepository.UpdateSetting("GlobalConfig", "PendingTransactionIdleWarningEnabled", chkPendingWarning.Checked ? "1" : "0");
            MyGlobal.IsPendingTransactionWarning = chkPendingWarning.Checked;

            //20260125 Store display strategy settings for large text
            int.TryParse(cboLargeTextPreviewLength.Text, out var largeTextPreviewLength);

            largeTextPreviewLength = LargeTextPreviewLengthPolicy.Normalize(largeTextPreviewLength);
            AppConfigHelper.LargeTextPreviewLength = largeTextPreviewLength;
            cboLargeTextPreviewLength.Text = largeTextPreviewLength.ToString();

            JasonQueryRepository.UpdateSetting("GlobalConfig", "LargeTextPreviewLength", largeTextPreviewLength.ToString());

            TransferValueToMainForm("ReloadQueryEditorSetting`");

            //判斷語系檔案是否存在
            if (CheckLocalizationFileExist())
            {
                //判斷是否有變更語系
                if (cboLocalization.Text != TextHelper.GetSafeString(cboLocalization.Tag))
                {
                    JasonQueryRepository.UpdateSetting("GlobalConfig", "Localization", cboLocalization.Text);

                    LocalizationHelper.Localization = cboLocalization.Text;
                }
            }

            TransferValueToMainForm("ReloadLocalization`");
            #endregion

            Cursor = Cursors.Default;

            //20191013 關閉Tab - 要透過 MainForm 關閉才行, 否則 SubForm 關了, TabControl 沒關！
            TransferValueToMainForm("CloseOptionsTab`");
        }

        private void TransferValueToMainForm(string sValue)
        {
            var valueArgs = new ValueUpdatedEventArgs(sValue);

            ValueUpdated(this, valueArgs);
        }

        private void chkEnableGroupBox_CheckedChanged(object sender, EventArgs e)
        {
            //讓 CheckBox 可以完美控制 GroupBox 的 Enable/Disable
            if (c1DockingTab.SelectedTab == tabAutoComplete)
            {
                ManageCheckGroupBox(chkEnableAutoComplete, grpAutoComplete);
            }
            else if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                ManageCheckGroupBox(chkEnableAutoReplace, grpAutoReplace);
            }
        }

        private static void ManageCheckGroupBox(CheckBox chk, Control grp)
        {
            if (chk.Parent == grp)
            {
                grp.Parent.Controls.Add(chk);

                chk.Location = new Point(chk.Left + grp.Left, chk.Top + grp.Top);

                chk.BringToFront();
            }

            grp.Enabled = chk.Checked;
        }

        private void c1GridAutoReplaceInfo_RowColChange(object sender, RowColChangeEventArgs e)
        {
            var iCurrentRow = c1GridAutoReplaceInfo.Row;

            btnSaveAutoReplace.Tag = c1GridAutoReplaceInfo.Columns["PID"].CellValue(iCurrentRow).ToString();
            txtKeyword.ReadOnly = false;
            editorAutoReplace.ReadOnly = false;
            txtKeyword.Text = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].CellValue(iCurrentRow).ToString();
            txtKeyword.Tag = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].CellValue(iCurrentRow).ToString();
            editorAutoReplace.Text = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]].CellValue(iCurrentRow).ToString();
            editorAutoReplace.Tag = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]].CellValue(iCurrentRow).ToString();
            txtKeyword.ReadOnly = true;
            editorAutoReplace.ReadOnly = true;

            SetControlEnabled(true); //顯示內容，但不可編輯 (除非按下「Add/Edit」)
        }

        private void CreateVisualStyleInfo()
        {
            var isShowColumnsDataType = chkShowColumnType.Checked;
            var nullShowAs = string.Equals(cboNullShowAs.Text, "NONE", StringComparison.OrdinalIgnoreCase) ? string.Empty : cboNullShowAs.Text;

            if (!_isFormLoadFinished)
            {
                return;
            }

            _languageText = LocalizationHelper.GetLanguageString("name", "form", GetType().Name, "gridheader", "name", "Text");

            var sGender = LocalizationHelper.GetLanguageString("gender", "form", GetType().Name, "gridheader", "gender", "Text");
            var sAge = LocalizationHelper.GetLanguageString("age", "form", GetType().Name, "gridheader", "age", "Text");
            var sBirthday = LocalizationHelper.GetLanguageString("birthday", "form", GetType().Name, "gridheader", "birthday", "Text");
            var sWeight = LocalizationHelper.GetLanguageString("weight", "form", GetType().Name, "gridheader", "weight", "Text");
            var sHeight = LocalizationHelper.GetLanguageString("height", "form", GetType().Name, "gridheader", "height", "Text");
            var sBlood = LocalizationHelper.GetLanguageString("blood group", "form", GetType().Name, "gridheader", "bloodgroup", "Text");
            var sEMail = LocalizationHelper.GetLanguageString("e-mail", "form", GetType().Name, "gridheader", "e-mail", "Text");
            var sEducation = LocalizationHelper.GetLanguageString("education", "form", GetType().Name, "gridheader", "education", "Text");
            var sAddress = LocalizationHelper.GetLanguageString("address", "form", GetType().Name, "gridheader", "address", "Text");

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        var sTemp01 = isShowColumnsDataType ? "\nVARCHAR(20)" : string.Empty;
                        var sTemp02 = isShowColumnsDataType ? "\nCHAR(2)" : string.Empty;
                        var sTemp03 = isShowColumnsDataType ? "\nINTEGER" : string.Empty;
                        var sTemp04 = isShowColumnsDataType ? "\nDATE" : string.Empty;
                        var sTemp05 = isShowColumnsDataType ? "\nVARCHAR(100)" : string.Empty;

                        _languageText = $"{_languageText.ToUpper()}{sTemp01}";
                        sGender = $"{sGender.ToUpper()}{sTemp02}";
                        sAge = $"{sAge.ToUpper()}{sTemp03}";
                        sBirthday = $"{sBirthday.ToUpper()}{sTemp04}";
                        sWeight = $"{sWeight.ToUpper()}{sTemp01}";
                        sHeight = $"{sHeight.ToUpper()}{sTemp01}";
                        sBlood = $"{sBlood.ToUpper()}{sTemp02}";
                        sEMail = $"{sEMail.ToUpper()}{sTemp01}";
                        sEducation = $"{sEducation.ToUpper()}{sTemp01}";
                        sAddress = $"{sAddress.ToUpper()}{sTemp05}";
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        _languageText += isShowColumnsDataType ? "\ncharacter varying(20)" : string.Empty;
                        sGender += isShowColumnsDataType ? "\ncharacter(2)" : string.Empty;
                        sAge += isShowColumnsDataType ? "\ninteger" : string.Empty;
                        sBirthday += isShowColumnsDataType ? "\ntime with time zone" : string.Empty;
                        sWeight += isShowColumnsDataType ? "\ncharacter varying(20)" : string.Empty;
                        sHeight += isShowColumnsDataType ? "\ncharacter varying(20)" : string.Empty;
                        sBlood += isShowColumnsDataType ? "\ncharacter(2)" : string.Empty;
                        sEMail += isShowColumnsDataType ? "\ncharacter varying(20)" : string.Empty;
                        sEducation += isShowColumnsDataType ? "\ncharacter varying(20)" : string.Empty;
                        sAddress += isShowColumnsDataType ? "\ncharacter varying(100)" : string.Empty;
                        break;
                    }
                default:
                    {
                        _languageText += isShowColumnsDataType ? "\nvarchar(20)" : string.Empty;
                        sGender += isShowColumnsDataType ? "\nchar(2)" : string.Empty;
                        sAge += isShowColumnsDataType ? "\ninteger" : string.Empty;
                        sBirthday += isShowColumnsDataType ? "\ndate" : string.Empty;
                        sWeight += isShowColumnsDataType ? "\nvarchar(20)" : string.Empty;
                        sHeight += isShowColumnsDataType ? "\nvarchar(20)" : string.Empty;
                        sBlood += isShowColumnsDataType ? "\nchar(2)" : string.Empty;
                        sEMail += isShowColumnsDataType ? "\nvarchar(20)" : string.Empty;
                        sEducation += isShowColumnsDataType ? "\nvarchar(20)" : string.Empty;
                        sAddress += isShowColumnsDataType ? "\nvarchar(100)" : string.Empty;
                        break;
                    }
            }

            _dtVisualStyle = new DataTable();

            _dtVisualStyle.Columns.Add(_languageText);
            _dtVisualStyle.Columns.Add(sGender);
            _dtVisualStyle.Columns.Add(sAge, typeof(int));
            _dtVisualStyle.Columns.Add(sBirthday);
            _dtVisualStyle.Columns.Add(sWeight);
            _dtVisualStyle.Columns.Add(sHeight);
            _dtVisualStyle.Columns.Add(sBlood);
            _dtVisualStyle.Columns.Add(sEMail);
            _dtVisualStyle.Columns.Add(sEducation);
            _dtVisualStyle.Columns.Add(sAddress);

            var rowVSInfo = _dtVisualStyle.NewRow();

            rowVSInfo[_languageText] = "Mary";
            rowVSInfo[sGender] = "F";
            rowVSInfo[sAge] = "21";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1991/12/31").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "49kg";
            rowVSInfo[sHeight] = "162cm";
            rowVSInfo[sBlood] = "B";
            rowVSInfo[sEMail] = "mary@gmail.com";
            rowVSInfo[sEducation] = "Master of Law";
            rowVSInfo[sAddress] = "505 N. Brand Blvd Suite 1450 Glendale CA 91203 USA";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Lucky";
            rowVSInfo[sGender] = "M";
            rowVSInfo[sAge] = "18";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1988/5/25").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "64kg";
            rowVSInfo[sHeight] = "159cm";
            rowVSInfo[sBlood] = nullShowAs;
            rowVSInfo[sEMail] = "lucky.huang@pchome.com.tw";
            rowVSInfo[sEducation] = "Doctor of Philosophy";
            rowVSInfo[sAddress] = "Rm. 50705, 15F.-2, No. 155-12, Aly. 2022, Ln. 1155, Qingda Dongyuan, Sec. 25, Guangfu Rd., East Dist., Hsinchu City 30035, Taiwan (R.O.C.)";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Jack";
            rowVSInfo[sGender] = "M";
            rowVSInfo[sAge] = "28";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1997/10/10").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "69kg";
            rowVSInfo[sHeight] = "166cm";
            rowVSInfo[sBlood] = "AB";
            rowVSInfo[sEMail] = "jack@ibm.com";
            rowVSInfo[sEducation] = "Bachelor of Engineering";
            rowVSInfo[sAddress] = nullShowAs;
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Peter";
            rowVSInfo[sGender] = "M";
            rowVSInfo[sAge] = "24";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1998/3/24").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "87kg";
            rowVSInfo[sHeight] = "182cm";
            rowVSInfo[sBlood] = "B";
            rowVSInfo[sEMail] = "peter@dell.com";
            rowVSInfo[sEducation] = "Master of Business Administration";
            rowVSInfo[sAddress] = "15 Grand Rue 11800 Laure Minervois France";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Jelic";
            rowVSInfo[sGender] = "M";
            rowVSInfo[sAge] = "31";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1992/1/2").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "51kg";
            rowVSInfo[sHeight] = "162cm";
            rowVSInfo[sBlood] = "A";
            rowVSInfo[sEMail] = "jelic@hotmail.com";
            rowVSInfo[sEducation] = nullShowAs;
            rowVSInfo[sAddress] = "3723 HR Bilthoven The Netherlands";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Sue";
            rowVSInfo[sGender] = "F";
            rowVSInfo[sAge] = "19";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1994/12/13").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = nullShowAs;
            rowVSInfo[sHeight] = "173cm";
            rowVSInfo[sBlood] = "O";
            rowVSInfo[sEMail] = "sue@test.com";
            rowVSInfo[sEducation] = "Doctor of Engineering";
            rowVSInfo[sAddress] = "Box 179, Millersville, SI 21108 Japan";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Moly";
            rowVSInfo[sGender] = "F";
            rowVSInfo[sAge] = "27";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1997/4/7").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "55kg";
            rowVSInfo[sHeight] = "167cm";
            rowVSInfo[sBlood] = "O";
            rowVSInfo[sEMail] = "moly@yahoo.com";
            rowVSInfo[sEducation] = "Master of Fine Arts";
            rowVSInfo[sAddress] = "7700 Gateway Blvd. Newark, CC 94560 Vietnam";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            rowVSInfo = _dtVisualStyle.NewRow();
            rowVSInfo[_languageText] = "Jenie";
            rowVSInfo[sGender] = "F";
            rowVSInfo[sAge] = "38";
            rowVSInfo[sBirthday] = Convert.ToDateTime("1992/8/31").ToString(cboDateFormat.Text);
            rowVSInfo[sWeight] = "60kg";
            rowVSInfo[sHeight] = nullShowAs;
            rowVSInfo[sBlood] = "AB";
            rowVSInfo[sEMail] = "jenie@hp.com";
            rowVSInfo[sEducation] = "Bachelor of Arts in Music";
            rowVSInfo[sAddress] = "4700 NW Camas Meadows Drive Camas, WA 98607 USA";
            _dtVisualStyle.Rows.Add(rowVSInfo);

            c1GridVisualStyle.DataSource = _dtVisualStyle;
            c1GridVisualStyle.Splits[0].ColumnCaptionHeight = isShowColumnsDataType ? 45 : 25;

            //Grid's 選取顏色
            c1GridVisualStyle.SelectedStyle.ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridSelectedForeColor.Tag));
            c1GridVisualStyle.SelectedStyle.BackColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridSelectedBackColor.Tag));

            c1GridVisualStyle.HeadingStyle.ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridHeadingForeColor.Tag));

            if (chkResize.Checked)
            {
                GridHelper.ResizeGridColumnWidth(c1GridVisualStyle);
            }

            //變更 Cell = NULL 的前景顏色 (不能使用 FetchCellStyle 事件，因為會變成整列都變色)
            if (string.IsNullOrWhiteSpace(nullShowAs))
            {
                return;
            }

            var colorNull = new Style
            {
                ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlNullValueForeColor.Tag))
            };

            for (var i = 0; i < c1GridVisualStyle.Columns.Count; i++)
            {
                c1GridVisualStyle.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, nullShowAs);
            }
        }

        private void CreateAndGetAutoReplaceInfoTable()
        {
            _lstGridHeaderAutoReplace = new List<string>();
            _lstGridHeaderAutoReplace.Add("PID");
            _languageText = LocalizationHelper.GetLanguageString("Keyword", "form", GetType().Name, "gridheader", "Keyword", "Text");
            _lstGridHeaderAutoReplace.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Replacement", "form", GetType().Name, "gridheader", "Replacement", "Text");
            _lstGridHeaderAutoReplace.Add(_languageText);

            _dtAutoReplaceInfo = new DataTable();
            _dtAutoReplaceInfo.Columns.Add(_lstGridHeaderAutoReplace[AutoReplaceColumn.Pid]);
            _dtAutoReplaceInfo.Columns.Add(_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]);
            _dtAutoReplaceInfo.Columns.Add(_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]);

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT PID, AttributeValue FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'AutoReplaceConfig'");
            sbSql.Append("   AND AttributeName = 'AutoReplace'");

            var sql = sbSql.ToString();
            var dt = JasonQueryRepository.ExecQuery(sql);

            foreach (DataRow dr in dt?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var pid = dr.GetSafeString("PID");
                var attributeValue = dr.GetSafeString("AttributeValue");

                int.TryParse(pid, out var pidValue);

                _rowAutoReplaceInfo = _dtAutoReplaceInfo.NewRow();
                _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Pid]] = pid;

                var info = attributeValue.Split(new[] { MyGlobal.Separator3s }, StringSplitOptions.None);

                _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]] = info[0];
                _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]] = info[1];

                _pid = _pid < pidValue ? pidValue : _pid;
                _dtAutoReplaceInfo.Rows.Add(_rowAutoReplaceInfo);
            }

            btnEditAutoReplace.Enabled = _dtAutoReplaceInfo.Rows.Count > 0;
            btnDeleteAutoReplace.Enabled = _dtAutoReplaceInfo.Rows.Count > 0;

            c1GridAutoReplaceInfo.DataSource = _dtAutoReplaceInfo;

            foreach (C1DisplayColumn col in c1GridAutoReplaceInfo.Splits[0].DisplayColumns)
            {
                var name = col.Name;

                if (string.Equals(name, "PID", StringComparison.OrdinalIgnoreCase))
                {
                    col.Visible = false;
                    col.Frozen = true;
                }
                else if (name == _lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword])
                {
                    try
                    {
                        col.AutoSize();
                    }
                    catch (Exception)
                    {
                        col.Width = 500;
                    }

                    if (col.Width > 500)
                    {
                        col.Width = 500;
                    }
                }
                else
                {
                    col.Width = 500;
                }
            }

            c1GridAutoReplaceInfo.AllowRowSizing = RowSizingEnum.IndividualRows;
            c1GridAutoReplaceInfo.Splits[0].ColumnCaptionHeight = 25;

            for (var r = 0; r < c1GridAutoReplaceInfo.Splits[0].Rows.Count; r++)
            {
                c1GridAutoReplaceInfo.Splits[0].Rows[r].AutoSize();

                if (c1GridAutoReplaceInfo.Splits[0].Rows[r].Height > 48)
                {
                    c1GridAutoReplaceInfo.Splits[0].Rows[r].Height = 48; //最多顯示 3 列資料
                }

                c1GridAutoReplaceInfo.Splits[0].Rows[r].Height += 4;
            }
        }

        private bool CheckAutoReplaceInfoExist()
        {
            return _dtAutoReplaceInfo.Rows.Cast<DataRow>().Any(row => row[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].ToString() == txtKeyword.Text);
        }

        private void chkShowFilterRow_CheckedChanged(object sender, EventArgs e)
        {
            if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                c1GridAutoReplaceInfo.FilterBar = chkShowFilterRowAutoReplace.Checked;
            }
            else if (c1DockingTab.SelectedTab == tabDataGrid)
            {
                c1GridVisualStyle.FilterBar = chkShowFilterRow.Checked;
                CreateVisualStyleInfo();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var sTemp = string.Empty;

            _pid++;

            if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                sTemp = "AutoReplace";
                grpModifyDefinitionAutoReplace.Tag = "ADD";
                txtKeyword.ReadOnly = false;
                txtKeyword.Text = string.Empty;
                txtKeyword.Tag = string.Empty;
                editorAutoReplace.ReadOnly = false;
                editorAutoReplace.Text = string.Empty;
                editorAutoReplace.Tag = string.Empty;
                txtKeyword.Focus();
            }

            var btnBox = Controls.Find($"btnSave{sTemp}", true).FirstOrDefault() as Button;

            btnBox.Tag = _pid.ToString();

            SetControlEnabled(false);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                grpModifyDefinitionAutoReplace.Tag = "EDIT";
                txtKeyword.ReadOnly = false;
                editorAutoReplace.ReadOnly = false;
                txtKeyword.Focus();
            }

            SetControlEnabled(false);
        }

        private void btnDeleteAutoReplace_Click(object sender, EventArgs e)
        {
            var sAlias = TextHelper.GetSafeString(txtKeyword.Tag);

            foreach (DataRow oRow in _dtAutoReplaceInfo.Rows)
            {
                if (oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].ToString() != sAlias)
                {
                    continue;
                }

                _dtAutoReplaceInfo.Rows.Remove(oRow);
                txtKeyword.ReadOnly = false;
                editorAutoReplace.ReadOnly = false;

                if (_dtAutoReplaceInfo?.Rows.Count > 0)
                {
                    var iCurrentRow = c1GridAutoReplaceInfo.Row;

                    btnSaveAutoReplace.Tag = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Pid]].CellValue(iCurrentRow).ToString();
                    txtKeyword.Text = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].CellValue(iCurrentRow).ToString();
                    txtKeyword.Tag = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]].CellValue(iCurrentRow).ToString();
                    editorAutoReplace.Text = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]].CellValue(iCurrentRow).ToString();
                    editorAutoReplace.Tag = c1GridAutoReplaceInfo.Columns[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]].CellValue(iCurrentRow).ToString();
                    txtKeyword.ReadOnly = true;
                    editorAutoReplace.ReadOnly = true;
                }
                else
                {
                    txtKeyword.Text = string.Empty;
                    editorAutoReplace.Text = string.Empty;
                    txtKeyword.ReadOnly = true;
                    editorAutoReplace.ReadOnly = true;
                    btnEditAutoReplace.Enabled = false;
                    btnDeleteAutoReplace.Enabled = false;
                }

                break;
            }
        }

        private void btnSaveAutoReplace_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKeyword.Text))
            {
                _languageText = LocalizationHelper.GetLanguageString("Please input \"Keyword\"!", "form", GetType().Name, "msg", "InputKeyword", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtKeyword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(editorAutoReplace.Text))
            {
                _languageText = LocalizationHelper.GetLanguageString("Please input \"Replacement\"!", "form", GetType().Name, "msg", "InputReplacement", "Text");
                MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                editorAutoReplace.Focus();
                return;
            }

            if (TextHelper.GetSafeString(grpModifyDefinitionAutoReplace.Tag) == "ADD")
            {
                if (!CheckAutoReplaceInfoExist())
                {
                    _rowAutoReplaceInfo = _dtAutoReplaceInfo.NewRow();
                    _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Pid]] = TextHelper.GetSafeString(btnSaveAutoReplace.Tag);
                    _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]] = txtKeyword.Text.Trim();
                    _rowAutoReplaceInfo[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]] = editorAutoReplace.Text.Trim();
                    _dtAutoReplaceInfo.Rows.Add(_rowAutoReplaceInfo);

                    var iRow = _dtAutoReplaceInfo.Rows.Count - 1;

                    c1GridAutoReplaceInfo.Row = iRow;
                }
                else
                {
                    _languageText = LocalizationHelper.GetLanguageString("The Keyword already exists!", "form", GetType().Name, "msg", "ReplacementKeywordExist", "Text");
                    MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            if (TextHelper.GetSafeString(grpModifyDefinitionAutoReplace.Tag) == "EDIT")
            {
                foreach (DataRow oRow in _dtAutoReplaceInfo.Rows)
                {
                    if (oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Pid]].ToString() != TextHelper.GetSafeString(btnSaveAutoReplace.Tag))
                    {
                        continue;
                    }

                    oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Keyword]] = txtKeyword.Text.Trim();
                    oRow[_lstGridHeaderAutoReplace[AutoReplaceColumn.Replacement]] = editorAutoReplace.Text.Trim();

                    break;
                }
            }

            txtKeyword.Tag = txtKeyword.Text.Trim();
            editorAutoReplace.Tag = editorAutoReplace.Text.Trim();

            SetControlEnabled(true);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (c1DockingTab.SelectedTab != tabAutoReplace)
            {
                return;
            }

            txtKeyword.Text = string.Empty;
            editorAutoReplace.Text = string.Empty;
            txtKeyword.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                if (TextHelper.GetSafeString(grpModifyDefinitionAutoReplace.Tag) == "EDIT")
                {
                    txtKeyword.Text = TextHelper.GetSafeString(txtKeyword.Tag);
                    editorAutoReplace.Text = TextHelper.GetSafeString(editorAutoReplace.Tag);
                }
                else
                {
                    txtKeyword.Text = string.Empty;
                    editorAutoReplace.Text = string.Empty;
                }
            }

            SetControlEnabled(true);

            editorAutoReplace.Focus();
            btnAddAutoReplace.Focus();
        }

        private void SetControlEnabled(bool value)
        {
            if (c1DockingTab.SelectedTab == tabAutoReplace)
            {
                txtKeyword.ReadOnly = value;
                editorAutoReplace.ReadOnly = value;
                btnAddAutoReplace.Enabled = value;
                btnEditAutoReplace.Enabled = value;
                btnDeleteAutoReplace.Enabled = value;
                btnSaveAutoReplace.Enabled = !value;
                btnCancelAutoReplace.Enabled = !value;
                btnClearAutoReplace.Enabled = !value;
                c1GridAutoReplaceInfo.Enabled = value;
            }

            btnRestoreDefaults.Enabled = value;
            btnCopySettings.Enabled = value;
            btnApply.Enabled = value;
            btnClose.Enabled = value;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void nudMinFragmentLength_Leave(object sender, EventArgs e)
        {
            //數字如果被使用者用 Delete or Backspace 刪除了，數值還是等於原數值，但畫面上是顯示空值；透過以下方式，將 numMinFragmentLength 顯示成原數值
            if (nudMinFragmentLength.Value == 9)
            {
                nudMinFragmentLength.Value = 8;
                nudMinFragmentLength.UpButton();
            }
            else if (nudMinFragmentLength.Value >= 2)
            {
                nudMinFragmentLength.Value += 1;
                nudMinFragmentLength.DownButton();
            }
        }

        private void HighlightPreview_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            HighlightPreview();
        }

        private void ZoomGrid()
        {
            var pcnt = 1;

            if (_fontSize == 0)
            {
                _fontSize = 12;
            }

            ((C1TrueDBGrid)_lstc1Grid[(int)c1GridID.VisualStyle]).RowHeight = _rowHeight * pcnt + 5;
            ((C1TrueDBGrid)_lstc1Grid[(int)c1GridID.VisualStyle]).Splits[0].ColumnCaptionHeight = _rowHeight * pcnt + 12;
            ((C1TrueDBGrid)_lstc1Grid[(int)c1GridID.VisualStyle]).RecordSelectorWidth = _recSelWidth * pcnt;
            ((C1TrueDBGrid)_lstc1Grid[(int)c1GridID.VisualStyle]).Styles["Normal"].Font = new Font(c1GridVisualStyle.Styles["Normal"].Font.FontFamily, _fontSize * pcnt);

            foreach (C1DisplayColumn col in ((C1TrueDBGrid)_lstc1Grid[(int)c1GridID.VisualStyle]).Splits[0].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 500;
                }

                if (col.Width > 500)
                {
                    col.Width = 500;
                }
            }
        }

        private void AlternatingRowColorSetting()
        {
            c1GridVisualStyle.AlternatingRows = true;
            c1GridVisualStyle.OddRowStyle.ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridOddRowForeColor.Tag));
            c1GridVisualStyle.OddRowStyle.BackColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridOddRowBackColor.Tag));
            c1GridVisualStyle.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridEvenRowForeColor.Tag));
            c1GridVisualStyle.EvenRowStyle.BackColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridEvenRowBackColor.Tag));
        }

        private void cboEditorFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            MyLibrary.SetQueryEditorFontSizeFromText(cboEditorFontSize.Text);
            ApplySqlStyler("editor");
        }

        private void cboEditorZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            //MyLibrary.QueryEditorZoom = cboEditorZoom.Text;
            //editor.Zoom = Convert.ToInt16(cboEditorZoom.Text);
        }

        private void cboGridFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            float.TryParse(cboGridFontSize.Text, out var iSize);

            if (iSize == 0)
            {
                iSize = 12;
            }

            c1GridVisualStyle.Font = new Font(cboGridFontPicker.Text, iSize, FontStyle.Regular, GraphicsUnit.Point);
            AutoSizeGrid();
            c1GridVisualStyle.Refresh();
        }

        private void AutoSizeGrid()
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            foreach (C1DisplayColumn col in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 500;
                }

                if (col.Width > 500)
                {
                    col.Width = 500;
                }
            }

            c1GridVisualStyle.Refresh();
        }

        private void ChangeVisualStyle(bool bPreview = true) //bPreview=true, 表示只要針對「c1GridVisualStyle」作用即可
        {
            var sStyle = MyLibrary.IsDarkMode ? "Office 2010 Black" : cboGridVisualStyle.Text;

            switch (sStyle)
            {
                case "Office 2007 Blue":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Blue;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Blue;
                        }

                        break;
                    }
                case "Office 2007 Silver":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Silver;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Silver;
                        }

                        break;
                    }
                case "Office 2007 Black":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Black;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2007Black;
                        }

                        break;
                    }
                case "Office 2010 Blue":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
                        }

                        break;
                    }
                case "Office 2010 Silver":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Silver;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Silver;
                        }

                        break;
                    }
                case "Office 2010 Black":
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Black;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Black;
                        }

                        break;
                    }
                default:
                    {
                        c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;

                        if (!bPreview)
                        {
                            c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
                        }

                        break;
                    }
            }
        }

        private void cboNullShowAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateVisualStyleInfo();
        }

        private void chkShowColumnDataType_CheckedChanged(object sender, EventArgs e)
        {
            CreateVisualStyleInfo();
        }

        private void c1GridVisualStyle_MouseDown(object sender, MouseEventArgs e)
        {
            //20220218 取消右鍵選單 (此處的右鍵選單，功能與主畫面差異太大，故直接取消)
        }


        private void Detect_KeyUp(object sender, KeyEventArgs e)
        {
            _isCtrlKeyDown = e.Control;
        }

        private void Detect_KeyDown(object sender, KeyEventArgs e)
        {
            _isCtrlKeyDown = e.Control;
        }

        private void c1GridVisualStyle_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_isCtrlKeyDown)
            {
                return;
            }

            _totalDelta += e.Delta;

            var fValue = 1 + (float)(SystemInformation.MouseWheelScrollLines * _totalDelta) / 3600;

            if (fValue > 1.7 || fValue < 0.5)
            {
                //
            }
            else
            {
                Zoom(fValue);
            }
        }

        private void Zoom(float pcnt)
        {
            if (_fontSize == 0)
            {
                _fontSize = 12;
            }

            c1GridVisualStyle.RowHeight = (int)(_rowHeight * pcnt) - 5;
            c1GridVisualStyle.Splits[0].ColumnCaptionHeight = (int)(_rowHeight * pcnt) + 12;
            c1GridVisualStyle.RecordSelectorWidth = (int)(_recSelWidth * pcnt);
            c1GridVisualStyle.Styles["Normal"].Font = new Font(c1GridVisualStyle.Styles["Normal"].Font.FontFamily, _fontSize * pcnt);

            foreach (C1DisplayColumn col in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 500;
                }

                col.Width = Math.Min(500, col.Width);
            }
        }

        private void FrozenColumn(bool bFrozen = true)
        {
            for (var i = 0; i < c1GridVisualStyle.Splits[0].DisplayColumns.Count; i++)
            {
                c1GridVisualStyle.Splits[0].DisplayColumns[i].Frozen = false;
            }

            if (!bFrozen)
            {
                return;
            }

            c1GridVisualStyle.Splits[0].DisplayColumns[c1GridVisualStyle.Col].Frozen = true;
            _gMenu.Items[MenuColumn.UnfreezeColumn].Enabled = true;
        }

        private void ArrangeData(string sMode)
        {
            var i = 0;
            var selCol = c1GridVisualStyle.SelectedCols.Count;
            var sData = string.Empty;
            var sColumnName = string.Empty;
            var sDataType = string.Empty;
            var bCopy = false;
            var bActiveCell = true; //是否為「只點選單一個 cell，並沒有『選取範圍』」?
            var sQuotingWith = cboResultCopyQuotingWith.Text == "None" ? string.Empty : cboResultCopyQuotingWith.Text;
            var sFieldSeparator = ",";

            foreach (int row in c1GridVisualStyle.SelectedRows)
            {
                var vr = c1GridVisualStyle.Splits[0].Rows[row];
                string sTemp;

                if (selCol == 0) //整列選取
                {
                    bActiveCell = false;

                    foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                    {
                        var sCaption = column.Caption;
                        var sCellText = column.CellText(vr.DataRowIndex);
                        var sDataTypeNameUpper = column.DataType.Name.ToUpper();
                        string[] splitters = { "\r\n", "\r", "\n" };
                        var parts = sCaption.Split(splitters, 2, StringSplitOptions.None);
                        var sTemp1 = parts[0];
                        var sTemp2 = parts.Length > 1 ? parts[1] : string.Empty;

                        if (i == 0)
                        {
                            //收集 Column Name & Data Type
                            sColumnName += $"{sTemp1}{sFieldSeparator}";
                            sDataType += string.IsNullOrEmpty(sTemp2) ? string.Empty : $"{sTemp2}{sFieldSeparator}";
                        }

                        switch (sDataTypeNameUpper)
                        {
                            case "STRING":
                                {
                                    sData += $"{sQuotingWith}{sCellText}{sQuotingWith}{sFieldSeparator}";
                                    break;
                                }
                            case "DATETIME":
                                {
                                    sTemp = Convert.ToDateTime(sCellText).ToString(cboDateFormat.Text);
                                    sData += $"{sQuotingWith}{sTemp}{sQuotingWith}{sFieldSeparator}";
                                    break;
                                }
                            default:
                                {
                                    sData += $"{sCellText}{sFieldSeparator}";
                                    break;
                                }
                        }
                    }

                    i++;
                }
                else //非整列選取 (選取區塊)
                {
                    foreach (C1DataColumn column in c1GridVisualStyle.SelectedCols)
                    {
                        bActiveCell = false;

                        var sCaption = column.Caption;
                        var sCellText = column.CellText(vr.DataRowIndex);
                        var sDataTypeNameUpper = column.DataType.Name.ToUpper();
                        string[] splitters = { "\r\n", "\r", "\n" };
                        var parts = sCaption.Split(splitters, 2, StringSplitOptions.None);
                        var sTemp1 = parts[0];
                        var sTemp2 = parts.Length > 1 ? parts[1] : string.Empty;

                        if (i == 0)
                        {
                            //收集 Column Name & Data Type
                            sColumnName += $"{sTemp1}{sFieldSeparator}";
                            sDataType += string.IsNullOrEmpty(sTemp2) ? string.Empty : $"{sTemp2}{sFieldSeparator}";
                        }

                        switch (sDataTypeNameUpper)
                        {
                            case "STRING":
                                {
                                    sData += $"{sQuotingWith}{sCellText}{sQuotingWith}{sFieldSeparator}";
                                    break;
                                }
                            case "DATETIME":
                                {
                                    sTemp = Convert.ToDateTime(sCellText).ToString(cboDateFormat.Text);
                                    sData += $"{sQuotingWith}{sTemp}{sQuotingWith}{sFieldSeparator}";
                                    break;
                                }
                            default:
                                {
                                    sData += $"{sCellText}{sFieldSeparator}";
                                    break;
                                }
                        }
                    }

                    i++;
                }

                if (!string.IsNullOrEmpty(sData) && sData.Length > sFieldSeparator.Length)
                {
                    sData = sData.Substring(0, sData.Length - sFieldSeparator.Length) + "\r\n";
                }
            }

            if (!string.IsNullOrEmpty(sColumnName) && sColumnName.Length > sFieldSeparator.Length)
            {
                sColumnName = sColumnName.Substring(0, sColumnName.Length - sFieldSeparator.Length);
            }

            if (!string.IsNullOrEmpty(sDataType) && sDataType.Length > sFieldSeparator.Length) //MyLibrary.GridShowColumnDataType
            {
                sDataType = sDataType.Substring(0, sDataType.Length - sFieldSeparator.Length);
            }

            var sLineBreak = string.IsNullOrEmpty(sDataType) ? string.Empty : $"\r\n{sDataType}";

            switch (sMode)
            {
                case "COPY":
                    {
                        bCopy = true;

                        if (bActiveCell)
                        {
                            i = 0;

                            foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                            {
                                if (i == c1GridVisualStyle.Col)
                                {
                                    var sDataTypeName = column.DataType.Name;

                                    if (new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "STRING", "DATETIME" }.Contains(sDataTypeName))
                                    {
                                        var sTemp = c1GridVisualStyle[c1GridVisualStyle.Splits[0].Rows[c1GridVisualStyle.Row].DataRowIndex, c1GridVisualStyle.Col].ToString();

                                        sData = $"{sQuotingWith}{sTemp}{sQuotingWith}";
                                    }
                                    else
                                    {
                                        sData = c1GridVisualStyle[c1GridVisualStyle.Splits[0].Rows[c1GridVisualStyle.Row].DataRowIndex, c1GridVisualStyle.Col].ToString();
                                    }

                                    break;
                                }

                                i++;
                            }
                        }

                        sData = sData.TrimEnd('\r', '\n');
                        break;
                    }
                case "COPYWITHCOLUMNNAMES":
                    {
                        bCopy = true;
                        sData = $"{sColumnName}{sLineBreak}\r\n{sData}";
                        break;
                    }
                case "COPYCOLUMNNAMES":
                    {
                        bCopy = true;
                        sData = $"{sColumnName}{sLineBreak}";
                        break;
                    }
                case "EXPORTTOCSV":
                    {
                        sData = $"{sColumnName}{sLineBreak}\r\n{sData}";
                        break;
                    }
            }

            if (bCopy)
            {
                Clipboard.SetDataObject(sData, false);
            }
            else
            {
                var messageAllFiles = LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text");
                var messageCsvFile = LocalizationHelper.GetLanguageString("CSV file", "Global", "Global", "msg", "CSVFile", "Text");
                var sf = new SaveFileDialog();

                if (string.Equals(sMode, "EXPORTTOCSV", StringComparison.OrdinalIgnoreCase))
                {
                    sf.Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text");
                    sf.Filter = @"CSV file (*.csv)|*.csv|All files (*.*)|*.*".Replace("All files", messageAllFiles).Replace("CSV file", messageCsvFile);
                }
                else
                {
                    sf.Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", messageAllFiles).Replace("Query file", messageCsvFile);
                }

                if (sf.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (string.Equals(sMode, "EXPORTTOCSV", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                    {
                        sf.FileName += ".csv";
                    }

                    TextEngine.WriteContentToFile(sData, sf.FileName, TextEncodes.Default);
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                    {
                        sf.FileName += ".sql";
                    }

                    TextEngine.WriteContentToFile(sData, sf.FileName, TextEncodes.UTF8);
                }
            }
        }

        private void ArrangeDataForAllData(string sMode)
        {
            var i = 0;
            var sData = string.Empty;
            var sColumnName = string.Empty;
            var sDataType = string.Empty;
            var sQuotingWith = cboResultCopyQuotingWith.Text == "None" ? string.Empty : cboResultCopyQuotingWith.Text;
            var sFieldSeparator = ",";

            for (var iRow = 0; iRow < c1GridVisualStyle.Splits[0].Rows.Count; iRow++)
            {
                var vr = c1GridVisualStyle.Splits[0].Rows[iRow];

                foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                {
                    var sCaption = column.Caption;
                    var sCellText = column.CellText(vr.DataRowIndex);
                    var sDataTypeNameUpper = column.DataType.Name.ToUpper();
                    string[] splitters = { "\r\n", "\r", "\n" };
                    var parts = sCaption.Split(splitters, 2, StringSplitOptions.None);
                    var sTemp1 = parts[0];
                    var sTemp2 = parts.Length > 1 ? parts[1] : string.Empty;

                    if (i == 0)
                    {
                        //收集 Column Name & Data Type
                        sColumnName += $"{sTemp1}{sFieldSeparator}";
                        sDataType += string.IsNullOrEmpty(sTemp2) ? string.Empty : $"{sTemp2}{sFieldSeparator}";
                    }

                    switch (sDataTypeNameUpper)
                    {
                        case "STRING":
                        case "DATETIME":
                            {
                                sData += $"{sQuotingWith}{sCellText}{sQuotingWith}{sFieldSeparator}";
                                break;
                            }
                        default:
                            {
                                sData += $"{sCellText}{sFieldSeparator}";
                                break;
                            }
                    }
                }

                i++;

                if (!string.IsNullOrEmpty(sData))
                {
                    sData = sData.Substring(0, sData.Length - sFieldSeparator.Length) + "\r\n";
                }
            }

            sData = sData.TrimEnd('\r', '\n');

            if (!string.IsNullOrEmpty(sColumnName) && sColumnName.Length > sFieldSeparator.Length)
            {
                sColumnName = sColumnName.Substring(0, sColumnName.Length - sFieldSeparator.Length);
            }

            if (chkShowColumnType.Checked && !string.IsNullOrEmpty(sDataType) && sDataType.Length > sFieldSeparator.Length) //MyLibrary.GridShowColumnDataType
            {
                sDataType = sDataType.Substring(0, sDataType.Length - sFieldSeparator.Length);
            }

            switch (sMode)
            {
                case "EXPORTTOCSV":
                    {
                        var sTemp1 = string.IsNullOrEmpty(sDataType) ? string.Empty : $"\r\n{sDataType}";

                        sData = $"{sColumnName}{sTemp1}\r\n{sData}";
                        break;
                    }
            }

            var messageAllFiles = LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text");
            var messageCsvFile = LocalizationHelper.GetLanguageString("CSV file", "Global", "Global", "msg", "CSVFile", "Text");
            var messageQueryFile = LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text");
            var temp = string.Equals(sMode, "EXPORTTOCSV", StringComparison.OrdinalIgnoreCase) ? @"CSV file (*.csv)|*.csv|All files (*.*)|*.*" : @"Query file (*.sql)|*.sql|All files (*.*)|*.*";

            temp = temp.Replace("All files", messageAllFiles).Replace("Query file", messageQueryFile).Replace("CSV file", messageCsvFile);

            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                Filter = temp
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return; //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
            }

            if (string.Equals(sMode, "EXPORTTOCSV", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                {
                    sf.FileName += ".csv";
                }

                TextEngine.WriteContentToFile(sData, sf.FileName, TextEncodes.Default);
            }
            else
            {
                if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                {
                    sf.FileName += ".sql";
                }

                TextEngine.WriteContentToFile(sData, sf.FileName, TextEncodes.UTF8);
            }
        }

        private void tmrMother2Child_Tick(object sender, EventArgs e)
        {
            //由 TabControl 關閉 Options Form
            if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp) && MyGlobal.GlobalTemp.StartsWith("CloseOptionsTab", StringComparison.Ordinal))
            {
                var sTemp2 = MyGlobal.GlobalTemp;

                MyGlobal.GlobalTemp = string.Empty; //避免重複觸發！

                //從 MainForm 傳來的關閉指令，要先「取消套用，再告訴 MainForm 關閉 Options Form」，否則全域變數會被套用到
                CancelApplyAndCloseOptionsForm();

                //20191013 關閉 - 要透過 MainForm 才行, 否則 SubForm 關了, TabControl 沒關！
                TransferValueToMainForm(sTemp2);
            }

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

            if (!_isApplyAndClose)
            {
                ApplyLocalizationSetting();
            }
        }

        private void txtSqlVariableName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSqlVariableName.Text))
            {
                txtSqlVariableName.Text = @"sql";
            }

            PreviewSqlToCode();
        }

        private void txtSqlVariableName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSqlVariableName.Text))
            {
                txtSqlVariableName.Text = @"sql";
            }

            PreviewSqlToCode();
        }

        private void txtStringBuilderVariableName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(txtStringBuilderVariableName.Text))
            {
                txtStringBuilderVariableName.Text = @"sbSql";
            }

            if (rdoStyle4.Enabled)
            {
                rdoStyle4.Checked = true;
            }
        }

        private void txtStringBuilderVariableName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStringBuilderVariableName.Text))
            {
                txtStringBuilderVariableName.Text = @"sbSql";
            }

            PreviewSqlToCode();
        }

        private void lstLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sTemp = string.Empty;

            rdoStyle3.Enabled = false;
            rdoStyle4.Enabled = false;
            lblStringBuilderVariableName.Visible = false;
            txtStringBuilderVariableName.Visible = false;

            switch (lstLanguage.SelectedItem.ToString())
            {
                case "C#":
                    {
                        rdoStyle3.Enabled = true;
                        rdoStyle4.Enabled = true;
                        lblStringBuilderVariableName.Visible = true;
                        txtStringBuilderVariableName.Visible = true;
                        break;
                    }
                case "VB.Net":
                    {
                        rdoStyle3.Enabled = true;

                        if (rdoStyle4.Checked)
                        {
                            rdoStyle1.Checked = true;
                        }

                        break;
                    }
                default:
                    {
                        if (rdoStyle3.Checked || rdoStyle4.Checked)
                        {
                            rdoStyle1.Checked = true;
                        }

                        break;
                    }
            }

            switch (lstLanguage.SelectedItem.ToString())
            {
                case "C#":
                    {
                        sTemp = LocalizationHelper.GetLanguageString("Using + operator", "form", GetType().Name, "msg", "C#Style1", "Text");
                        lblStyle1.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character \r\n", "form", GetType().Name, "msg", "C#Style2", "Text");
                        lblStyle2.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character Enviroment.NewLine", "form", GetType().Name, "msg", "C#Style3", "Text");
                        lblStyle3.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("StringBuilder", "form", GetType().Name, "msg", "C#Style4", "Text");
                        lblStyle4.Text = sTemp;
                        break;
                    }
                case "VB.Net":
                    {
                        sTemp = LocalizationHelper.GetLanguageString("Using & operator", "form", GetType().Name, "msg", "VB.NetStyle1", "Text");
                        lblStyle1.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character VbCrLf", "form", GetType().Name, "msg", "VB.NetStyle2", "Text");
                        lblStyle2.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character Enviroment.NewLine", "form", GetType().Name, "msg", "VB.NetStyle3", "Text");
                        lblStyle3.Text = sTemp;
                        break;
                    }
                case "VB6/VBA":
                    {
                        sTemp = LocalizationHelper.GetLanguageString("Using & operator", "form", GetType().Name, "msg", "VB6/VBAStyle1", "Text");
                        lblStyle1.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character VbCrLf", "form", GetType().Name, "msg", "VB6/VBAStyle2", "Text");
                        lblStyle2.Text = sTemp;
                        lblStyle3.Text = string.Empty;
                        break;
                    }
                case "Delphi6":
                    {
                        sTemp = LocalizationHelper.GetLanguageString("Using + operator", "form", GetType().Name, "msg", "Delphi6Style1", "Text");
                        lblStyle1.Text = sTemp;
                        sTemp = LocalizationHelper.GetLanguageString("New line character #13#10", "form", GetType().Name, "msg", "Delphi6Style2", "Text");
                        lblStyle2.Text = sTemp;
                        lblStyle3.Text = string.Empty;
                        break;
                    }
            }

            PreviewSqlToCode();
        }

        private void rdoStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rdo)
            {
                grpStyle.Tag = TextHelper.GetSafeString(rdo.Tag).Substring(6, 1);
            }

            PreviewSqlToCode();
        }

        private void editorSqlStatement_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(editorSqlToCode.Text))
            {
                editorSqlToCode.Text = TextHelper.GetSafeString(editorSqlToCodePreview.Tag);
            }
        }

        private void PreviewSqlToCode()
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            var text = string.Empty;
            var language = lstLanguage.SelectedItem.ToString();
            var style = TextHelper.GetSafeString(grpStyle.Tag);

            switch (language, style)
            {
                case ("C#", "1"):
                    {
                        text = TextHelper.TransferSqlToCode_CSharpStyle1(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("C#", "2"):
                    {
                        text = TextHelper.TransferSqlToCode_CSharpStyle2(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("C#", "3"):
                    {
                        text = TextHelper.TransferSqlToCode_CSharpStyle3(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("C#", "4"):
                    {
                        text = TextHelper.TransferSqlToCode_CSharpStyle4(editorSqlToCode.Text, txtSqlVariableName.Text, txtStringBuilderVariableName.Text, false);
                        break;
                    }
                case ("VB.Net", "1"):
                    {
                        text = TextHelper.TransferSqlToCode_VBNet1(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("VB.Net", "2"):
                    {
                        text = TextHelper.TransferSqlToCode_VBNet2(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("VB.Net", "3"):
                    {
                        text = TextHelper.TransferSqlToCode_VBNet3(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("VB6/VBA", "1"):
                    {
                        text = TextHelper.TransferSqlToCode_VB6VBA1(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("VB6/VBA", "2"):
                    {
                        text = TextHelper.TransferSqlToCode_VB6VBA2(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("Delphi6", "1"):
                    {
                        text = TextHelper.TransferSqlToCode_Delphi61(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
                case ("Delphi6", "2"):
                    {
                        text = TextHelper.TransferSqlToCode_Delphi62(editorSqlToCode.Text, txtSqlVariableName.Text, false);
                        break;
                    }
            }

            editorSqlToCodePreview.ReadOnly = false;
            editorSqlToCodePreview.Text = text;
            editorSqlToCodePreview.ReadOnly = true;
        }

        private void txtMaxWidth_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtMaxWidth_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaxWidth.Text) || int.Parse(txtMaxWidth.Text) < 100)
            {
                txtMaxWidth.Text = @"99";
            }
        }

        private void txtMaxWidth_Enter(object sender, EventArgs e)
        {
            txtMaxWidth.SelectionStart = 0;
            txtMaxWidth.SelectionLength = txtMaxWidth.TextLength;
        }

        private void PreviewSqlFormatter()
        {
            if (!_isFormLoadFinished || string.IsNullOrWhiteSpace(editorSqlFormatter.Text))
            {
                return;
            }

            var previewPosition = editorSqlFormatterPreview.SelectionStart;
            var maxLineWidth = int.TryParse(txtMaxWidth.Text, out var parsedMaxLineWidth)
                               ? parsedMaxLineWidth
                               : SqlFormatOptions.DefaultMaxLineWidth;

            var keywordCaseValue = rdoLowerCase.Checked ? 2 : 1;

            var options = EditorSqlFormatterOptionsFactory.Create
            (
                GetSelectedSqlFormatterIndentSize(),
                maxLineWidth,
                GetSelectedSqlFormatterBlankLinesBetweenStatements(),
                chkConvertCaseForKeywords.Checked,
                keywordCaseValue,
                GetSelectedSqlFormatterListItemsPerLine()
            );

            var providerKind = DataSourceTypeMapper.ToDatabaseProviderKind(_currentSourceType);

            var result = _sqlFormatterCoordinator.Format
            (
                editorSqlFormatter.Text,
                providerKind,
                GetSelectedSqlFormatterEngineKind(),
                options
            );

            editorSqlFormatterPreview.ReadOnly = false;
            editorSqlFormatterPreview.Text = result.FormattedSql;
            editorSqlFormatterPreview.ReadOnly = true;

            editorSqlFormatterPreview.SelectionStart = Math.Min
            (
                previewPosition,
                editorSqlFormatterPreview.TextLength
            );

            editorSqlFormatterPreview.ScrollCaret();

            if (result.Success)
            {
                lblSqlFormatterPreviewStatus.Text = string.Empty;
            }
            else
            {
                lblSqlFormatterPreviewStatus.ForeColor = Color.DarkRed;
                lblSqlFormatterPreviewStatus.Text = result.ErrorMessage;
            }
        }

        private void LoadSqlFormatterEngineChoices()
        {
            var providerKind = DataSourceTypeMapper.ToDatabaseProviderKind(_currentSourceType);

            _sqlFormatterEngineChoices = SqlFormatterEnginePreferenceResolver.GetChoices(providerKind);
            cboSqlFormatterEngine.Items.Clear();

            foreach (var choice in _sqlFormatterEngineChoices)
            {
                cboSqlFormatterEngine.Items.Add(choice.DisplayName);
            }

            var selectedEngineKind = SqlFormatterEnginePreferenceResolver.GetEffectiveEngine
            (
                providerKind,
                MyLibrary.SqlFormatterEngine
            );

            var selectedIndex = _sqlFormatterEngineChoices.Select((choice, index) => new { choice, index })
                                                          .Where(item => item.choice.Kind == selectedEngineKind)
                                                          .Select(item => item.index)
                                                          .DefaultIfEmpty(_sqlFormatterEngineChoices.Count > 0 ? 0 : -1)
                                                          .First();

            cboSqlFormatterEngine.SelectedIndex = selectedIndex;
            ApplySqlFormatterEngineOptionState();
        }

        private void LoadSqlFormatterLayoutChoices()
        {
            cboSqlFormatterIndentSize.Items.Clear();

            foreach (var indentSize in _sqlFormatterIndentSizes)
            {
                cboSqlFormatterIndentSize.Items.Add(indentSize);
            }

            cboSqlFormatterIndentSize.SelectedItem = MyLibrary.SqlFormatterIndentSize;

            if (cboSqlFormatterIndentSize.SelectedIndex < 0)
            {
                cboSqlFormatterIndentSize.SelectedItem = 4;
            }

            cboSqlFormatterBlankLines.Items.Clear();

            foreach (var blankLines in _sqlFormatterBlankLinesBetweenStatements)
            {
                cboSqlFormatterBlankLines.Items.Add(blankLines);
            }

            cboSqlFormatterBlankLines.SelectedItem = MyLibrary.SqlFormatterBlankLinesBetweenStatements;

            if (cboSqlFormatterBlankLines.SelectedIndex < 0)
            {
                cboSqlFormatterBlankLines.SelectedItem = 1;
            }

            cboSqlFormatterListItemsPerLine.Items.Clear();

            foreach (var itemsPerLine in _sqlFormatterListItemsPerLine)
            {
                cboSqlFormatterListItemsPerLine.Items.Add(itemsPerLine);
            }

            cboSqlFormatterListItemsPerLine.SelectedItem = MyLibrary.SqlFormatterListItemsPerLine;

            if (cboSqlFormatterListItemsPerLine.SelectedIndex < 0)
            {
                cboSqlFormatterListItemsPerLine.SelectedItem = SqlFormatOptions.DefaultListItemsPerLine;
            }
        }

        private int GetSelectedSqlFormatterIndentSize()
        {
            return cboSqlFormatterIndentSize.SelectedItem is int indentSize ? indentSize : 4;
        }

        private int GetSelectedSqlFormatterBlankLinesBetweenStatements()
        {
            return cboSqlFormatterBlankLines.SelectedItem is int blankLines ? blankLines : 1;
        }

        private int GetSelectedSqlFormatterListItemsPerLine()
        {
            return cboSqlFormatterListItemsPerLine.SelectedItem is int itemsPerLine ? itemsPerLine : SqlFormatOptions.DefaultListItemsPerLine;
        }

        private SqlFormatterEngineKind GetSelectedSqlFormatterEngineKind()
        {
            var selectedIndex = cboSqlFormatterEngine.SelectedIndex;

            return selectedIndex >= 0 && selectedIndex < _sqlFormatterEngineChoices.Count
                   ? _sqlFormatterEngineChoices[selectedIndex].Kind
                   : SqlFormatterEngineKind.Unknown;
        }

        private void ApplySqlFormatterEngineOptionState()
        {
            var selectedIndex = cboSqlFormatterEngine.SelectedIndex;

            var selectedChoice = selectedIndex >= 0 && selectedIndex < _sqlFormatterEngineChoices.Count
                                 ? _sqlFormatterEngineChoices[selectedIndex]
                                 : null;

            lblMaxWidth2.Enabled = selectedChoice?.SupportsMaxLineWidth == true;
            txtMaxWidth.Enabled = selectedChoice?.SupportsMaxLineWidth == true;
            lblSqlFormatterListItemsPerLine.Enabled = selectedChoice?.SupportsListItemsPerLine == true;
            cboSqlFormatterListItemsPerLine.Enabled = selectedChoice?.SupportsListItemsPerLine == true;
        }

        private void cboSqlFormatterEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySqlFormatterEngineOptionState();
            PreviewSqlFormatter();
        }

        private void cboSqlFormatterLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreviewSqlFormatter();
        }

        private void SqlFormat_TextChanged(object sender, EventArgs e)
        {
            PreviewSqlFormatter();
        }

        private void SqlFormatter_CheckedChanged(object sender, EventArgs e)
        {
            PreviewSqlFormatter();
        }

        private void SqlFormatter2_CheckedChanged(object sender, EventArgs e)
        {
            if (chkConvertCaseForKeywords.Checked)
            {
                PreviewSqlFormatter();
            }
        }

        private void editor_MouseClick(object sender, MouseEventArgs e)
        {
            //停用：會造成輸入文字時的誤刪困擾 (加上區塊選取+執行，會造成選取文字的 SQL 指令包含了其他的選取文字)
            //if (chkHighlightSelection.Checked)
            //{
            //    HighlightSelection(true);
            //}
        }

        private void HighlightSelection(bool bMouseClick = false)
        {
            var iPos = editor.CurrentPosition;
            var iWordStart = editor.WordStartPosition(iPos, true);
            var iWordEnd = editor.WordEndPosition(iPos, true);
            var sTextUpper = editor.Text.ToUpper();
            var sWord = editor.GetTextRange(iWordStart, iWordEnd - iWordStart);
            var bValue = true;

            editor.AdditionalCaretsBlink = false; //選取的字串，最前面會不會閃爍
            editor.AdditionalCaretsVisible = false; //選取的字串，最前面的 | 要不要顯示

            //如果選取文字包含了換行符號或空白，不用 multi-select！
            if (editor.SelectedText.Any(c => c == '\r' || c == '\n' || c == ' '))
            {
                bValue = false;
            }

            //for Mouse Click，如果沒有選取文字，需要重新處理 multi selection
            //20190320 此處還有 BUG 需要調整：左右鍵移動時，在單字移動，例如 word，游標在 d 按左鍵移動，此時不會選取 (忽略 bMouseClick，會造成左右鍵失效)
            if (string.IsNullOrEmpty(editor.SelectedText) && bMouseClick)
            {
                editor.Tag = string.Empty;
            }

            if (string.IsNullOrEmpty(sWord) || !bValue || TextHelper.GetSafeString(editor.Tag) == sWord)
            {
                return;
            }

            editor.Tag = sWord;

            var iMainSelection = 0;
            var matches = Regex.Matches(sTextUpper, sWord.ToUpper());

            editor.MultipleSelection = true;

            foreach (Match m in matches)
            {
                if (iPos >= m.Index && iPos - m.Index <= sWord.Length)
                {
                    //記住游標所在處的單字
                    iMainSelection = m.Index;
                }
                else
                {
                    editor.AddSelection(m.Index, m.Index + sWord.Length);
                }
            }

            //20250906 模仿 Notepad++ 將游標停留在單字的最後方
            editor.AddSelection(iMainSelection + sWord.Length, iMainSelection);
        }

        private void cboFindGrid_KeyUp(object sender, KeyEventArgs e)
        {
            var bValue = !string.IsNullOrWhiteSpace(cboFindGrid.Text);

            btnFindNextGrid.Enabled = bValue;
            btnFindPreviousGrid.Enabled = bValue;
            btnCountGrid.Enabled = bValue;
            btnHighlightAllGrid.Enabled = bValue;
            btnClearHighlightsGrid.Enabled = bValue;
        }

        private void cboFindGrid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 13 || string.IsNullOrWhiteSpace(cboFindGrid.Text))
            {
                return;
            }

            btnFindNextGrid.PerformClick();
        }

        private void cboFindGrid_DropDown(object sender, EventArgs e)
        {
            //LoadFindListGrid();
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

            cboFindGrid.SelectedIndex = 0;
            cboFindGrid.Tag = HighlightCount().ToString(); //統計出現次數，但不顯示
        }

        private void cboFindGrid_TextChanged(object sender, EventArgs e)
        {
            var bValue = !string.IsNullOrWhiteSpace(cboFindGrid.Text);

            foreach (C1DisplayColumn cd in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                cd.OwnerDraw = bValue;
            }

            btnFindNextGrid.Enabled = bValue;
            btnFindPreviousGrid.Enabled = bValue;
            btnCountGrid.Enabled = bValue;
            btnHighlightAllGrid.Enabled = bValue;
            btnClearHighlightsGrid.Enabled = bValue;

            if (bValue)
            {
                //統計出現次數，但不顯示
                cboFindGrid.Tag = HighlightCount().ToString();
            }
        }

        private void btnFindNextGrid_Click(object sender, EventArgs e)
        {
            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                MessageBoxHelper.ShowNearCursor($"Can't find the text \"{cboFindGrid.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!FindNextGrid())
            {
                return;
            }

            //往下找不到資料，而且不是在第一格位置，再從頭開始找一次
            if (c1GridVisualStyle.Row != 0 && c1GridVisualStyle.Col == 0)
            {
                FindNextGrid(true);
            }
        }

        private bool FindNextGrid(bool bFindAgain = false)
        {
            var sSearchText = cboFindGrid.Text;
            var iCurrentRowStart = c1GridVisualStyle.Row;
            var iCurrentColStart = c1GridVisualStyle.Col;
            var iFindRow = 0;
            var iFindCol = 0;
            var bFind = false;
            bool bResult;

            if (bFindAgain)
            {
                iCurrentRowStart = 0;
                iCurrentColStart = 0;
            }

            if (string.IsNullOrEmpty(sSearchText))
            {
                return false;
            }

            for (var iRow = iCurrentRowStart; iRow < c1GridVisualStyle.Splits[0].Rows.Count; iRow++)
            {
                var vr = c1GridVisualStyle.Splits[0].Rows[iRow];
                var iCol = 0;

                foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                {
                    var cellText = column.CellText(vr.DataRowIndex);

                    if (!string.IsNullOrEmpty(cellText) && cellText.IndexOf(cboFindGrid.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (iRow == iCurrentRowStart) //游標所在列，尋找下一個，要略過此格！
                        {
                            if (iCol > iCurrentColStart)
                            {
                                iFindRow = iRow;
                                iFindCol = iCol;

                                bFind = true;
                                break;
                            }
                        }
                        else
                        {
                            iFindRow = iRow;
                            iFindCol = iCol;

                            bFind = true;
                            break;
                        }
                    }

                    iCol++;
                }

                if (bFind)
                {
                    break;
                }
            }

            if (bFind)
            {
                c1GridVisualStyle.Row = iFindRow;
                c1GridVisualStyle.Col = iFindCol;
                c1GridVisualStyle.Select();
                bResult = false;
            }
            else
            {
                bResult = true;
            }

            return bResult;
        }

        private void btnFindPreviousGrid_Click(object sender, EventArgs e)
        {
            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                MessageBoxHelper.ShowNearCursor($"Can't find the text \"{cboFindGrid.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (FindPreviousGrid())
            {
                //往上找不到資料，再從底部開始找一次
                FindPreviousGrid(true);
            }
        }

        private bool FindPreviousGrid(bool bFindAgain = false)
        {
            var sSearchText = cboFindGrid.Text;
            var iCurrentRowStart = c1GridVisualStyle.Row;
            var iCurrentColStart = c1GridVisualStyle.Col;
            var iFindRow = 0;
            var iFindCol = 0;
            var bFind = false;
            bool bResult;

            if (bFindAgain)
            {
                iCurrentRowStart = c1GridVisualStyle.Splits[0].Rows.Count - 1;
                iCurrentColStart = c1GridVisualStyle.Splits[0].DisplayColumns.Count - 1;
            }

            if (string.IsNullOrEmpty(sSearchText))
            {
                return false;
            }

            for (var iRow = iCurrentRowStart; iRow >= 0; iRow--)
            {
                var vr = c1GridVisualStyle.Splits[0].Rows[iRow];
                var iCol = 0;
                var iCount = c1GridVisualStyle.Splits[0].DisplayColumns.Count - 1;

                for (var jj = iCount; jj >= 0; jj--)
                {
                    var cellText = c1GridVisualStyle.Columns[jj].CellValue(iRow).ToString();

                    if (cellText.IndexOf(cboFindGrid.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (iRow == iCurrentRowStart) //游標所在列
                        {
                            if (!bFindAgain && jj < iCurrentColStart || bFindAgain && jj <= iCurrentColStart)
                            {
                                iFindRow = iRow;
                                iFindCol = jj;

                                bFind = true;
                                break;
                            }
                        }
                        else
                        {
                            iFindRow = iRow;
                            iFindCol = jj;

                            bFind = true;
                            break;
                        }
                    }

                    iCol++;
                }

                if (bFind)
                {
                    break;
                }
            }

            if (bFind)
            {
                c1GridVisualStyle.Row = iFindRow;
                c1GridVisualStyle.Col = iFindCol;
                c1GridVisualStyle.Select();
                bResult = false;
            }
            else
            {
                bResult = true;
            }

            return bResult;
        }

        private void btnClearHighlightsGrid_Click(object sender, EventArgs e)
        {
            //Disable OwnerDraw property for each column
            foreach (C1DisplayColumn cd in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                cd.OwnerDraw = false;
            }

            _modifiedList.Clear();
            c1GridVisualStyle.ClearCellStyle(CellStyleFlag.AllCells);
        }

        private void c1GridVisualStyle_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            //var dr = (DataRowView)c1GridVisualStyle[c1GridVisualStyle.RowBookmark(e.Row)];

            //if ((int)dr[1] <= 20)
            //{
            //    //e.CellStyle.BackColor = Color.Blue;
            //}
            //else if (dr[5].ToString() == "O"))
            //{
            //    //e.CellStyle.BackColor = Color.Green;
            //}
        }

        private void btnHighlightAllGrid_Click(object sender, EventArgs e)
        {
            var bFind = false;

            btnClearHighlightsGrid.PerformClick();

            if (TextHelper.GetSafeString(cboFindGrid.Tag) == "0")
            {
                MessageBoxHelper.ShowNearCursor($"Can't find the text \"{cboFindGrid.Text}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            for (var iRow = 0; iRow < c1GridVisualStyle.Splits[0].Rows.Count; iRow++)
            {
                var iCol = 0;
                var vr = c1GridVisualStyle.Splits[0].Rows[iRow];

                foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                {
                    var cellText = column.CellText(vr.DataRowIndex);

                    if (!string.IsNullOrEmpty(cellText) && cellText.IndexOf(cboFindGrid.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bFind = true;
                        _modifiedList.Add(new Point(iRow, iCol));
                    }

                    iCol++;
                }
            }

            //Enable OwnerDraw property for each column
            if (!bFind)
            {
                return;
            }

            foreach (C1DisplayColumn cd in c1GridVisualStyle.Splits[0].DisplayColumns)
            {
                cd.OwnerDraw = true;
            }
        }

        private void c1GridVisualStyle_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
        {
            var mc = new Point(e.Row, e.Col);

            if (!_modifiedList.Contains(mc))
            {
                return;
            }

            e.Style.ForeColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridHighlightForeColor.Tag));
            e.Style.BackColor = ColorTranslator.FromHtml(TextHelper.GetSafeString(pnlGridHighlightBackColor.Tag));
        }

        private void btnCountGrid_Click(object sender, EventArgs e)
        {
            HighlightCount(true);
        }

        private int HighlightCount(bool showAlertOnError = false)
        {
            var count = 0;

            for (var row = 0; row < c1GridVisualStyle.Splits[0].Rows.Count; row++)
            {
                var vr = c1GridVisualStyle.Splits[0].Rows[row];

                count += c1GridVisualStyle.Columns.Cast<C1DataColumn>().Count(col1 => col1.CellText(vr.DataRowIndex).Length != col1.CellText(vr.DataRowIndex).ToUpper().Replace(cboFindGrid.Text.ToUpper(), string.Empty).Length);
            }

            if (!showAlertOnError)
            {
                return count;
            }

            var temp1 = LocalizationHelper.GetLanguageString("Find What:", "form", GetType().Name, "msg", "FindWhat", "Text");
            var temp2 = LocalizationHelper.GetLanguageString("Count:", "form", GetType().Name, "msg", "Count", "Text");
            var temp3 = LocalizationHelper.GetLanguageString("matches.", "form", GetType().Name, "msg", "matches", "Text");

            MessageBoxHelper.ShowNearCursor($"{temp1} {cboFindGrid.Text}\r\n\r\n{temp2} {count} {temp3}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return count;
        }

        private void Keywords_LeaveCheck(object sender, EventArgs e)
        {
            if (!(sender is ScintillaEditor keywords))
            {
                return;
            }

            keywords.Text = Regex.Replace(keywords.Text.Replace("\r\n", " "), @"\s+", " ");
        }

        private void ShowOperatorKeywords(object sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                FindKeywords(Convert.ToInt16(TextHelper.GetSafeString(pic.Tag)), true);
            }
        }

        private void HideOperatorKeywords(object sender, EventArgs e)
        {
            if (sender is ToolStripButton btn)
            {
                FindKeywords(Convert.ToInt16(TextHelper.GetSafeString(btn.Tag)));
            }
        }

        private void FindKeywords(int iKey, bool bFormButton = false)
        {
            var iFocus = -1; //目前 Focus 在哪一個區塊？

            for (var i = 0; i <= 3; i++)
            {
                if (!((GroupBox)_lstFindGroup[i]).Visible)
                {
                    continue;
                }

                iFocus = i;
                break;
            }

            if (iFocus != -1)
            {
                FindKeywords2(iFocus); //先 Show 再 Hide
            }

            if (!(iKey >= 0 && iKey <= 3))
            {
                return;
            }

            if (iFocus != -1 && !bFormButton)
            {
                return;
            }

            if (iFocus != iKey)
            {
                FindKeywords2(iKey);
            }
        }

        private void FindKeywords2(int iKey)
        {
            int iWidth;
            var iHeight = ((ScintillaEditor)_lstFindEditor[iKey]).Height;

            if (((GroupBox)_lstFindGroup[iKey]).Visible)
            {
                iWidth = ((ScintillaEditor)_lstFindEditor[iKey]).Width + 205;
                ((ScintillaEditor)_lstFindEditor[iKey]).Size = new Size(iWidth, iHeight);
                ((GroupBox)_lstFindGroup[iKey]).Visible = false;
                ((ScintillaEditor)_lstFindEditor[iKey]).Focus();
            }
            else
            {
                iWidth = ((ScintillaEditor)_lstFindEditor[iKey]).Width - 205;
                ((ScintillaEditor)_lstFindEditor[iKey]).Size = new Size(iWidth, iHeight);
                ((GroupBox)_lstFindGroup[iKey]).Visible = true;
                ((ToolStripTextBox)_lstFindTextBox[iKey]).Focus();
            }
        }

        private void FindKeywords_TextChanged(object sender, EventArgs e)
        {
            if (!(sender is ToolStripTextBox txt))
            {
                return;
            }

            int i = Convert.ToInt16(TextHelper.GetSafeString(txt.Tag));
            var bValue = !string.IsNullOrWhiteSpace(((ToolStripTextBox)_lstFindTextBox[i]).Text);

            ((ToolStripButton)_lstFindNextButton[i]).Enabled = bValue;
            ((ToolStripButton)_lstFindPreviousButton[i]).Enabled = bValue;
        }

        private void FindNextKeywords_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton btn))
            {
                return;
            }

            int i = Convert.ToInt16(TextHelper.GetSafeString(btn.Tag));
            var sText = ((ScintillaEditor)_lstFindEditor[i]).Text.Trim();
            var sSearchText = ((ToolStripTextBox)_lstFindTextBox[i]).Text.Trim();

            if (string.IsNullOrEmpty(sText) || string.IsNullOrEmpty(sSearchText))
            {
                return;
            }

            if (FindCount(sText, sSearchText) == 0)
            {
                MessageBoxHelper.ShowNearCursor($"Can't find the text \"{sSearchText}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!FindNext(sText, sSearchText, i))
            {
                if (FindNext(sText, sSearchText, i, true))
                {
                    ((ScintillaEditor)_lstFindEditor[i]).Focus();
                }
            }
            else
            {
                ((ScintillaEditor)_lstFindEditor[i]).Focus();
            }
        }

        private bool FindNext(string sAllText, string sSearchText, int ii, bool bFindAgain = false)
        {
            bool bResult;
            var iStartOriginal = ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart;
            var iEndOriginal = ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd;

            if (bFindAgain)
            {
                iStartOriginal = 0;
                iEndOriginal = 0;
            }

            var iStart = 0;
            var iEnd = 0;
            var bMatch = false;
            var array = sAllText.ToCharArray();

            for (var i = iEndOriginal; i < array.Length; i++)
            {
                var letter = array[i];

                if (!string.Equals(letter.ToString(), sSearchText.Substring(0, 1), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                iStart = i;
                bMatch = true;

                for (var j = 1; j < sSearchText.Length; j++)
                {
                    i++;

                    try
                    {
                        letter = array[i];

                        if (j < sSearchText.Length && string.Equals(letter.ToString(), sSearchText.Substring(j, 1), StringComparison.CurrentCultureIgnoreCase))
                        {
                            //
                        }
                        else
                        {
                            bMatch = false;
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        bMatch = false;
                        break;
                    }
                }

                if (!bMatch)
                {
                    continue;
                }

                iEnd = i;
                break;
            }

            if (iStart >= 0 && (bMatch && iEnd >= 0 || iEnd > 0))
            {
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart = iStart;
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd = iEnd + 1;
                ((ScintillaEditor)_lstFindEditor[ii]).ScrollCaret();
                bResult = true;
            }
            else
            {
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart = iStartOriginal;
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd = iEndOriginal;
                bResult = false;
            }

            return bResult;
        }

        private void FindPreviousKeywords_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripButton btn))
            {
                return;
            }

            int i = Convert.ToInt16(TextHelper.GetSafeString(btn.Tag));
            var sText = ((ScintillaEditor)_lstFindEditor[i]).Text.Trim();
            var sSearchText = ((ToolStripTextBox)_lstFindTextBox[i]).Text.Trim();

            if (string.IsNullOrEmpty(sText) || string.IsNullOrEmpty(sSearchText))
            {
                return;
            }

            if (FindCount(sText, sSearchText) == 0)
            {
                MessageBoxHelper.ShowNearCursor($"Can't find the text \"{sSearchText}\"", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!FindPrevious(sText, sSearchText, i))
            {
                if (FindPrevious(sText, sSearchText, i, true))
                {
                    ((ScintillaEditor)_lstFindEditor[i]).Focus();
                }
            }
            else
            {
                ((ScintillaEditor)_lstFindEditor[i]).Focus();
            }
        }

        private bool FindPrevious(string sAllText, string sSearchText, int ii, bool bFindAgain = false)
        {
            bool bResult;

            if (string.IsNullOrEmpty(sSearchText))
            {
                return false;
            }

            var iStartOriginal = ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart;
            var iEndOriginal = ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd;

            if (bFindAgain)
            {
                iStartOriginal = sAllText.Length;
                iEndOriginal = sAllText.Length;
            }

            var iStart = 0;
            var iEnd = 0;
            var bMatch = false;
            var array = sAllText.ToCharArray();

            if (iStartOriginal - sSearchText.Length <= 0)
            {
                return false;
            }

            for (var i = iStartOriginal; i >= 0; i--)
            {
                if (i - 1 < 0)
                {
                    return false;
                }

                var letter = array[i - 1];

                //最後一個字母符合
                if (!string.Equals(letter.ToString(), sSearchText.Substring(sSearchText.Length - 1, 1), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                iEnd = i - 1;
                bMatch = true;

                var iCount = sSearchText.Length - 1;

                for (var j = iCount; j > 0; j--)
                {
                    i--;

                    try
                    {
                        letter = array[i - 1];

                        if (j < sSearchText.Length && string.Equals(letter.ToString(), sSearchText.Substring(j - 1, 1), StringComparison.CurrentCultureIgnoreCase))
                        {
                            //bMatch = true;
                        }
                        else
                        {
                            bMatch = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                if (!bMatch)
                {
                    continue;
                }

                iStart = i - 1;
                break;
            }

            if (iStart >= 0 && ((bMatch && iEnd >= 0) || (iEnd > 0)))
            {
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart = iStart;
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd = iEnd + 1;
                ((ScintillaEditor)_lstFindEditor[ii]).ScrollCaret();
                bResult = true;
            }
            else
            {
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionStart = iStartOriginal;
                ((ScintillaEditor)_lstFindEditor[ii]).SelectionEnd = iEndOriginal;
                bResult = false;
            }

            return bResult;
        }

        private static int FindCount(string sText, string sFind)
        {
            var i = 0;

            try
            {
                var matches = Regex.Matches(sText.ToUpper(), sFind.ToUpper());

                i += matches.Cast<Match>().Count();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return i;
        }

        private void FindNextKeywords_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(sender is ToolStripTextBox txt))
            {
                return;
            }

            int i = Convert.ToInt16(TextHelper.GetSafeString(txt.Tag));

            if (e.KeyChar == 13 && !string.IsNullOrWhiteSpace(((ToolStripTextBox)_lstFindTextBox[i]).Text))
            {
                ((ToolStripButton)_lstFindNextButton[i]).PerformClick();
            }
        }

        private void editor_DoubleClick(object sender, ScintillaNET.DoubleClickEventArgs e)
        {
            HighlightSelection(true);
        }

        private void ApplyIndicatorAppearance(string sColor) //設定 Bookmark 樣式
        {
            var sStyle = string.Empty;
            var margin = editorIndicator.Margins[BOOKMARK_MARGIN];

            margin.Width = 15;
            margin.Mask = 0;
            margin.Sensitive = true;
            margin.Type = ScintillaNET.MarginType.Symbol;
            margin.Mask = ScintillaNET.Marker.MaskAll;
            margin.Cursor = ScintillaNET.MarginCursor.Arrow;

            var marker = editorIndicator.Markers[BOOKMARK_MARKER];

            sStyle = TextHelper.GetKeyFromDictionary(MyGlobal.dicBookmarkStyle, cboBookmarkStyle.Text);

            switch (sStyle)
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

            marker.SetBackColor(ColorTranslator.FromHtml(sColor));
            marker.SetForeColor(Color.Transparent);
            editorIndicator.Lines[editorIndicator.CurrentLine].MarkerAdd(BOOKMARK_MARKER);
            editorIndicator.Margins[0].Width = 0;
        }

        private void SetSquiggle(bool bClearOnly, string sColorErrorLineBackground, int iPos = 0, int iLength = 0)
        {
            //波浪底線
            const int iSquiggleNum = 11;

            editorIndicator.IndicatorCurrent = iSquiggleNum;
            editorIndicator.IndicatorClearRange(0, editorIndicator.TextLength);

            if (bClearOnly)
            {
                return;
            }

            if (string.IsNullOrEmpty(sColorErrorLineBackground))
            {
                sColorErrorLineBackground = MyLibrary.ColorErrorLineBackground;
            }

            editorIndicator.Indicators[iSquiggleNum].ForeColor = ColorTranslator.FromHtml(sColorErrorLineBackground);
            editorIndicator.Indicators[iSquiggleNum].Style = ScintillaNET.IndicatorStyle.Squiggle;
            editorIndicator.IndicatorFillRange(iPos, iLength);
        }

        private void cboBookmarkStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isFormLoadFinished)
            {
                ApplyIndicatorAppearance(TextHelper.GetSafeString(pnlBookmarkBackground.Tag)); //變更 Bookmark Style
            }
        }

        private void cboDateFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isFormLoadFinished)
            {
                CreateVisualStyleInfo();
            }
        }

        private void chkResize_CheckedChanged(object sender, EventArgs e)
        {
            lblMaxWidth.Enabled = chkResize.Checked;
            cboMaxWidth.Enabled = chkResize.Checked;
            CreateVisualStyleInfo();
        }

        private void chkPagedQuery_CheckedChanged(object sender, EventArgs e)
        {
            lblRowsPerPage.Enabled = chkPagedQuery.Checked;
            cboRowsPerPage.Enabled = chkPagedQuery.Checked;
        }

        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            CreateVisualStyleInfo();
        }

        private void cboMaxSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateVisualStyleInfo();
        }

        private void chkShowStreamlinedName_CheckedChanged(object sender, EventArgs e)
        {
            CreateVisualStyleInfo();
        }

        private void ExportToExcel()
        {
            using (var form = new ExportToFileForm())
            {
                var header = string.Empty;
                var age = LocalizationHelper.GetLanguageString("age", "form", GetType().Name, "gridheader", "age", "Text");
                var dtData = new DataTable();
                var dt = c1GridVisualStyle.GetDataTableSourceOrNull();

                foreach (C1DataColumn column in c1GridVisualStyle.Columns)
                {
                    header = column.Caption;
                    dtData.Columns.Add(header, header == age ? typeof(int) : typeof(string));
                }

                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    var rowData = dtData.NewRow();

                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        header = dt.Columns[j].ColumnName;

                        if (dt.Rows[i][dt.Columns[j].ColumnName].ToString() == MyLibrary.GridNullShowAs)
                        {
                            rowData[header] = DBNull.Value;
                        }
                        else
                        {
                            rowData[header] = dt.Rows[i][dt.Columns[j].ColumnName];
                        }
                    }

                    dtData.Rows.Add(rowData);
                }

                form.dtData = dtData;
                form.FontName = c1GridVisualStyle.Font.Name;
                form.FontSize = c1GridVisualStyle.Font.Size;
                form.ShowDialog();
            }
        }

        private void CellViewer()
        {
            string columnName;
            var columnType = string.Empty;
            var cellText = c1GridVisualStyle[c1GridVisualStyle.Row, c1GridVisualStyle.Col].ToString();
            var temp = c1GridVisualStyle.Splits[0].DisplayColumns[c1GridVisualStyle.Col].ToString();
            int index = temp.IndexOf('\n');

            if (index >= 0)
            {
                columnName = temp.Substring(0, index);
                columnType = temp.Substring(index + 1);
            }
            else
            {
                columnName = temp;
            }

            using (var form = new CellViewerForm())
            {
                form.ColumnName = columnName;
                form.ColumnType = columnType;
                form.CellText = cellText;
                form.ShowDialog();
            }
        }

        private void c1GridVisualStyle_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var iRow = c1GridVisualStyle.RowContaining(e.Y);

            if (iRow != -1)
            {
                CellViewer();
            }
        }

        private void LocationObject()
        {
            int height = Convert.ToInt16(Math.Floor((double)txtHeightCode.Height / 2));

            grpSqlStatementCode.Size = new Size(grpSqlStatementCode.Width, height);
            grpPreviewSql.Size = new Size(grpSqlStatementCode.Width, height);
            grpPreviewSql.Location = new Point(grpPreviewSql.Left, grpSqlStatementCode.Top + grpSqlStatementCode.Height + 4);
        }

        private void cboLocalization_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            if (!CheckLocalizationFileExist(true))
            {
                return;
            }

            if (LocalizationHelper.Localization == cboLocalization.Text)
            {
                return;
            }

            LocalizationHelper.Localization = cboLocalization.Text;
            TransferValueToMainForm("ReloadLocalization`");
        }

        private bool CheckLocalizationFileExist(bool showAlertOnError = false)
        {
            var result = true;
            var temp = TextHelper.GetValueFromDictionary(LocalizationHelper.LocalizationMap, cboLocalization.Text);
            var fileName = $@"{Application.StartupPath}\localization\{temp}";

            if (!File.Exists(fileName))
            {
                result = false;
            }

            if (!showAlertOnError || result)
            {
                return result;
            }

            var message = LocalizationHelper.GetLanguageString("Localization file not found!", "Global", "Global", "msg", "LocalizationNotFound", "Text");

            MessageBox.Show($"{message}\r\n\r\n{fileName}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

        private void cboGridRowHeightResizing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            var sRowSizing = TextHelper.GetKeyFromDictionary(MyGlobal.dicRowSizing, cboGridRowHeightResizing.Text);

            c1GridVisualStyle.AllowRowSizing = sRowSizing == "AllRows" ? RowSizingEnum.AllRows : RowSizingEnum.IndividualRows;
        }

        private void c1DockingTab_SizeChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            LocationObject();
        }

        private void c1DockingTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            LocationObject();
        }

        private void c1DockingTab_SelectedTabChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            LocationObject();
        }

        private void timerTitle_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(MyGlobal.GlobalTemp))
            {
                return;
            }

            timerTitle.Enabled = false;

            var sTitle = MyGlobal.GlobalTemp.Split(new[] { ";" }, StringSplitOptions.None);

            MyGlobal.GlobalTemp = string.Empty;

            for (var i = 0; i < sTitle.Length; i++)
            {
                if (i > 19)
                {
                    break;
                }

                tabExample.TabPages[i].Title = sTitle[i];
            }

            if (sTitle.Length < 20)
            {
                for (var i = 19; i >= sTitle.Length; i--)
                {
                    //tabExample.TabPages.RemoveAt(i);
                    tabExample.TabPages[i].Title = $"Sample Page No.{i + 1}";
                }
            }

            tabExample.TabPages[0].Selected = true;
        }

        private void UpdateMainFormTabVisualStyle()
        {
            try
            {
                if (rdoMultiDocument.Checked)
                {
                    tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiDocument;
                }
                else if (rdoMultiForm.Checked)
                {
                    tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiForm;
                }
                else
                {
                    tabExample.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiBox;
                }

                tabExample.Style = rdoIDE.Checked ? VisualStyle.IDE : VisualStyle.Plain;
                tabExample.BoldSelectedPage = chkTabBold.Checked;
                tabExample.ShrinkPagesToFit = chkShrinkPages.Checked;
                tabExample.ShowArrows = chkShowArrows.Checked;
                tabExample.HoverSelect = chkHoverSelect.Checked;
                tabExample.Multiline = chkMultiLine.Checked;
                tabExample.PositionTop = true;
                tabExample.ShowClose = true;
                tabExample.BorderStyle = BorderStyle.None;
            }
            catch
            {
                //20250725 tabExample.Appearance 所引發的例外錯誤必須忽略它 (來源為 MagicLibrary's TabControl.cs，暫時無解)
            }
        }

        private void chkTabVisualStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            if (sender is CheckBox chk)
            {
                switch (chk.Name)
                {
                    case "chkShrinkPages":
                        {
                            if (chkShowArrows.Checked)
                            {
                                chkShowArrows.Checked = !chkShrinkPages.Checked;
                            }

                            break;
                        }
                    case "chkShowArrows":
                        {
                            if (chkShrinkPages.Checked)
                            {
                                chkShrinkPages.Checked = !chkShowArrows.Checked;
                            }

                            break;
                        }
                    case "chkHoverSelect":
                        {
                            if (chkMultiLine.Checked)
                            {
                                chkMultiLine.Checked = !chkHoverSelect.Checked;
                            }

                            break;
                        }
                    case "chkMultiLine":
                        {
                            if (chkHoverSelect.Checked)
                            {
                                chkHoverSelect.Checked = !chkMultiLine.Checked;
                            }

                            break;
                        }
                }
            }

            UpdateMainFormTabVisualStyle();
        }

        private void cboEditorFontPicker_TextChanged(object sender, EventArgs e)
        {
            MyLibrary.QueryEditorFontName = cboEditorFontPicker.Text;
            ApplySqlStyler("editor");
        }

        private void cboGridFontPicker_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(cboGridFontSize.Text, out var iSize);

            if (iSize == 0)
            {
                iSize = 12;
            }

            c1GridVisualStyle.Font = new Font(cboGridFontPicker.Text, iSize, FontStyle.Regular, GraphicsUnit.Point);
            AutoSizeGrid();
            c1GridVisualStyle.Refresh();
        }

        private void pnlOptionsTabClick(object sender, EventArgs e)
        {
            if (!(sender is Panel pnlSelected))
            {
                return;
            }

            _panelColorSelectedName = pnlSelected.Name;
            ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ApplyOptionsTabColor);
        }

        private void ApplyOptionsTabColor(Color selectedColor)
        {
            var e = new { Color = selectedColor, HexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor) };

            for (var i = 0; i < _lstPanelTabColor.Count; i++)
            {
                if (((Panel)_lstPanelTabColor[i]).Name != _panelColorSelectedName)
                {
                    continue;
                }

                ((Panel)_lstPanelTabColor[i]).BackColor = e.Color;
                ((Panel)_lstPanelTabColor[i]).Tag = e.HexColor;
                _toolTip1.SetToolTip((Panel)_lstPanelTabColor[i], $"{e.HexColor} (R:{e.Color.R}, G:{e.Color.G}, B:{e.Color.B})");

                UIHelper.SetDockingTabColor(c1DockingTab1, pnlOptionsTabActiveBackColor.BackColor, pnlOptionsTabActiveForeColor.BackColor, pnlOptionsTabInactiveForeColor.BackColor);

                break;
            }
        }

        private void txtRecentFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtRecentFiles_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtRecentFiles_Leave(object sender, EventArgs e)
        {
            int.TryParse(txtRecentFiles.Text, out var i);

            if (i < 10 || i > 60)
            {
                txtRecentFiles.Text = @"20";
            }
        }

        private void txtMyFavorite_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtMyFavorite_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtMyFavorite_Leave(object sender, EventArgs e)
        {
            int.TryParse(txtMyFavorite.Text, out var i);

            if (i < 10 || i > 60)
            {
                txtMyFavorite.Text = @"20";
            }
        }

        private void editor_Enter(object sender, EventArgs e)
        {
            tsEditor.BackColor = _colorEditorFocused;
        }

        private void editor_Leave(object sender, EventArgs e)
        {
            tsEditor.BackColor = _colorEditorUnfocused;
        }

        private void cboGridVisualStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridHelper.ChangeGridVisualStyle(c1GridVisualStyle, cboGridVisualStyle.Text);
        }

        private void txtRecentFiles_MouseClick(object sender, MouseEventArgs e)
        {
            txtRecentFiles.SelectionStart = 0;
            txtRecentFiles.SelectionLength = 2;
        }

        private void txtMyFavorite_MouseClick(object sender, MouseEventArgs e)
        {
            txtMyFavorite.SelectionStart = 0;
            txtMyFavorite.SelectionLength = 2;
        }

        private void chkShowSaveAsButton_CheckedChanged(object sender, EventArgs e)
        {
            btnSaveAs.Visible = chkShowSaveAsButton.Checked;
        }

        private void Style_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMainFormTabVisualStyle();
        }

        private void CheckForUpdates(object sender, EventArgs e)
        {
            var enabled = !rdoDonotCheck.Checked;

            grpCheckOnly.Enabled = enabled;
            grpUpdateInformationSource.Enabled = enabled;
        }

        private void LoadUpdateMetadataSourceSettings()
        {
            var sourceValue = JasonQueryRepository.GetSettingValue
            (
                UpdateMetadataSettingsContract.SectionName,
                UpdateMetadataSettingsContract.SourceSettingName,
                UpdateMetadataSettingsContract.DefaultSource.ToString()
            );

            var localFolder = JasonQueryRepository.GetSettingValue
            (
                UpdateMetadataSettingsContract.SectionName,
                UpdateMetadataSettingsContract.LocalFolderSettingName,
                string.Empty
            );

            MyLibrary.UpdateMetadataSource = UpdateMetadataSettingsContract.ParseSource(sourceValue);
            MyLibrary.UpdateMetadataLocalFolder = localFolder;
            txtLocalFolder.Text = MyLibrary.UpdateMetadataLocalFolder;

            switch (MyLibrary.UpdateMetadataSource)
            {
                case UpdateMetadataSourceKind.GitHub:
                    {
                        rdoUpdateSourceOfficialWebsite.Checked = true;
                        break;
                    }
                case UpdateMetadataSourceKind.LocalFolder:
                    {
                        rdoUpdateSourceLocal.Checked = true;
                        break;
                    }
                default:
                    {
                        rdoUpdateSourceOfficialWebsite.Checked = true;
                        break;
                    }
            }

            UpdateMetadataSourceControlState();
        }

        private UpdateMetadataSourceKind GetSelectedUpdateMetadataSource()
        {
            return rdoUpdateSourceLocal.Checked ? UpdateMetadataSourceKind.LocalFolder : UpdateMetadataSourceKind.OfficialWebsite;
        }

        private void UpdateMetadataSource_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.Checked)
            {
                UpdateMetadataSourceControlState();
            }
        }

        private void txtLocalFolder_TextChanged(object sender, EventArgs e)
        {
            UpdateMetadataSourceControlState();
        }

        private void UpdateMetadataSourceControlState()
        {
            var localFolderSelected = rdoUpdateSourceLocal.Checked;

            txtLocalFolder.Enabled = localFolderSelected;
            btnBrowseLocalFolder.Enabled = localFolderSelected;
            btnLocalFolderOpenFolder.Enabled = localFolderSelected && !string.IsNullOrWhiteSpace(txtLocalFolder.Text);
        }

        private bool ValidateUpdateMetadataSettings()
        {
            if (GetSelectedUpdateMetadataSource() != UpdateMetadataSourceKind.LocalFolder)
            {
                return true;
            }

            var localFolder = txtLocalFolder.Text.Trim();

            if (string.IsNullOrWhiteSpace(localFolder))
            {
                ShowInvalidUpdateFolderMessage("Please select a company update folder.", "SelectCompanyUpdateFolder");
                return false;
            }

            if (!Directory.Exists(localFolder))
            {
                ShowInvalidUpdateFolderMessage
                (
                    $"The company update folder does not exist:\r\n\r\n{localFolder}",
                    "CompanyUpdateFolderNotFound",
                    localFolder
                );

                return false;
            }

            var metadataPath = UpdateSourceResolver.ResolveMetadata(UpdateMetadataSourceKind.LocalFolder, localFolder).Value;

            if (!File.Exists(metadataPath))
            {
                ShowInvalidUpdateFolderMessage
                (
                    $"The update metadata file was not found:\r\n\r\n{metadataPath}",
                    "UpdateMetadataFileNotFound",
                    metadataPath
                );

                return false;
            }

            return true;
        }

        private void ShowInvalidUpdateFolderMessage(string fallbackMessage, string messageId, string path = "")
        {
            var message = LocalizationHelper.GetLanguageString(fallbackMessage, "form", GetType().Name, "msg", messageId, "Text");

            if (!string.IsNullOrWhiteSpace(path) && !message.Contains(path))
            {
                message = $"{message}\r\n\r\n{path}";
            }

            c1DockingTab.SelectedTab = tabGlobal;
            c1DockingTab4.SelectedTab = tabUpdateSettings;
            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtLocalFolder.Focus();
        }

        private void btnSpecifiedSQLFile_Click(object sender, EventArgs e)
        {
            var btn = sender as C1.Win.C1Input.C1Button;
            var sTag = TextHelper.GetSafeString(btn.Tag);

            var of = new OpenFileDialog
            {
                Multiselect = false,
                Title = LocalizationHelper.GetLanguageString("Open File", "Global", "Global", "msg", "OpenFile", "Text"),
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (of.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            switch (sTag)
            {
                case "1":
                    {
                        txtSpecifiedSQLFile1.Text = of.FileName;
                        break;
                    }
                case "2":
                    {
                        txtSpecifiedSQLFile2.Text = of.FileName;
                        break;
                    }
            }
        }

        private void btnClearFile_Click(object sender, EventArgs e)
        {
            var btn = sender as C1.Win.C1Input.C1Button;
            var sTag = TextHelper.GetSafeString(btn.Tag);

            switch (sTag)
            {
                case "1":
                    {
                        txtSpecifiedSQLFile1.Text = string.Empty;
                        break;
                    }
                case "2":
                    {
                        txtSpecifiedSQLFile2.Text = string.Empty;
                        break;
                    }
            }
        }

        private void chkShowIndentGuide_CheckedChanged(object sender, EventArgs e)
        {
            editor.IndentationGuides = chkShowIndentGuide.Checked ? ScintillaNET.IndentView.LookBoth : ScintillaNET.IndentView.None;

            btnShowIndentGuide.Visible = !chkShowIndentGuide.Checked;
            btnShowIndentGuide2.Visible = chkShowIndentGuide.Checked;
        }

        private void c1GridVisualStyle_Enter(object sender, EventArgs e)
        {
            tsGrid.BackColor = _colorEditorFocused;
        }

        private void c1GridVisualStyle_Leave(object sender, EventArgs e)
        {
            tsGrid.BackColor = _colorEditorUnfocused;
        }

        private void btnHelp_ColumnComment_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This feature is only effective when querying a single table.", "Global", "Global", "msg", "Help_ColumnName", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_RawDataMode_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("\"Raw Data Mode\" significantly improves data display performance.\r\n\r\nWhen enabled, query results are displayed exactly as returned by the database, without any additional formatting or processing.\r\n\r\nAs a result, the following display-related settings will not be applied:\\r\\n1. Date formatting\\r\\n2. Null value style\\r\\n3. Column type display\\r\\n4. Column comment display\\r\\n5. Auto-fit column width\r\n\r\nNote:\\r\\nIf the query result contains large text or binary data, this mode may be temporarily disabled for the current query to prevent performance or stability issues.", "Global", "Global", "msg", "Help_RawDataMode", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_AppendQueryResult_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This feature is enabled by default.\r\n\r\nWhen Paging is enabled and a query completes successfully, pressing \"Next Page\" will re-execute the previous query to retrieve the next set of rows.\r\n\r\nIf enabled, the new results will be appended to the existing results.\r\nIf disabled, only the latest page of results will be displayed.", "form", GetType().Name, "msg", "Help_AppendQueryResult", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_EnableAutoComplete_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When this option is enabled, JasonQuery analyzes your SQL input while typing and automatically displays a list of matching members when the conditions are met.\r\n\r\nThe minimum fragment length controls how many characters are required before Auto Complete is triggered, helping to avoid excessive popups.\r\n\r\nThe options below allow you to specify which types of members are included in Auto Complete, such as built-in keywords, functions, or user-defined database objects.", "form", GetType().Name, "msg", "Help_AutoComplete", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_EnableAutoReplace_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Auto Replace allows you to bind frequently used query fragments to short keywords.\r\n\r\nAfter typing a keyword and pressing the space key, JasonQuery automatically expands it into the predefined replacement.\r\n\r\nThis feature is useful for:\r\n• Frequently used query templates\r\n• Repetitive SQL structures\r\n• Quickly inserting long statements or clauses\r\n\r\nYou can press Ctrl+Z at any time to undo the replacement.", "form", GetType().Name, "msg", "Help_AutoReplace", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Symbol_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Insert the ^ symbol in the replacement text.\r\nAfter Auto Replace is applied, the cursor will be positioned at ^, and the symbol will be removed automatically.\r\n\r\nThis is useful for quickly continuing SQL input after expansion.", "form", GetType().Name, "msg", "Help_Symbol", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_PagedQuery_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When enabled, JasonQuery executes queries in paging mode, returning a limited number of rows per execution.\\r\\n\\r\\nAfter a query completes successfully, you can retrieve the next page of data by clicking the \"Next Page\" button or by navigating to the last row in the result grid.", "form", GetType().Name, "msg", "Help_PagedQuery", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DarkMode_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("You cannot change this setting unless you restart JasonQuery.", "Global", "Global", "msg", "Help_RequestToRestart", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_SelectCurrentSqlBlock_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This option controls how SQL blocks are selected when using Ctrl+Enter.\r\n\r\nIf enabled, lines containing only whitespace are treated as part of the current SQL block, and JasonQuery continues selecting the full SQL statement.\r\n\r\nIf disabled, selection stops when a blank line is encountered.", "form", GetType().Name, "msg", "Help_SelectCurrentSqlBlock", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_ShowColumnInfo_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Display column information for Tables and Views in the Query Editor's \"Schema Information\".\r\n\r\nWhen enabled, JasonQuery retrieves all column names and data types while loading the schema information.\r\n\r\nIf the database contains a large number of Tables or Views, the schema initialization time may increase significantly.\r\n\r\nIf you only need to browse object names, disabling this option can improve loading performance.\r\n\r\nYou can change the setting from [Tools] > [Options] > [Query Editor].", "Global", "Global", "msg", "Help_ShowColumnInfo", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_AutoListMembers_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When enabled, JasonQuery analyzes the current SQL context while you type and automatically lists available database objects or column members.\r\n\r\nFor example:\r\n• After typing an alias followed by a dot (\".\"), members of the corresponding table, view, or subquery are listed.\r\n• After keywords such as FROM, UPDATE, DELETE, or INSERT INTO, tables or views matching the current input are suggested.\r\n\r\nTo provide accurate suggestions, JasonQuery may query schema information from the database during SQL editing.", "form", GetType().Name, "msg", "Help_AutoListMembers", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_UseSavePoint_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("In PostgreSQL, JasonQuery automatically creates a savepoint before executing subqueries required for \"Auto List Members\".\r\n\r\nIf a subquery execution fails, for example when the subquery is incomplete or cannot be executed independently, JasonQuery restores the savepoint to prevent affecting the current transaction state.\r\n\r\nIf the subquery executes successfully, the savepoint is automatically released.\r\n\r\nThis mechanism applies only to PostgreSQL, as any SQL error in PostgreSQL causes the current transaction to enter a failed state.\r\n\r\nThis setting affects only helper queries during SQL editing and does not impact user-executed SQL statements.", "form", GetType().Name, "msg", "Help_UseSavePoint", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DisconnectAfterSelect_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("[Disconnect immediately after SELECT queries]\r\n\r\nAfter a simple SELECT query is executed, JasonQuery immediately disconnects from the database to avoid holding unnecessary connections.\r\n\r\nIf there is a pending transaction, JasonQuery keeps the connection open so you can still execute Commit or Rollback.", "form", GetType().Name, "msg", "Help_DisconnectAfterSelect", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_PendingWarning_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("[Pending transaction warning]\r\n\r\nWhen JasonQuery detects a pending transaction, it shows \"Not yet committed or rolled back!\" at the bottom of the window, followed by an elapsed-time timer.\r\n\r\nJasonQuery shows the first reminder dialog 5 minutes after the pending transaction starts.\r\n\r\nIf the transaction is still not committed or rolled back after you click OK, JasonQuery will show the reminder again every 5 minutes.", "form", GetType().Name, "msg", "Help_PendingWarning", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lblStyle_Click(object sender, EventArgs e)
        {
            var label = sender as Label;
            var tag = TextHelper.GetSafeString(label.Tag);

            switch (tag)
            {
                case "1":
                    {
                        rdoStyle1.Checked = true;
                        break;
                    }
                case "2":
                    {
                        rdoStyle2.Checked = true;
                        break;
                    }
                case "3":
                    {
                        rdoStyle3.Checked = true;
                        break;
                    }
            }
        }

        private void txtMaxWidth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Back)
            {
                return;
            }

            if (string.IsNullOrEmpty(txtMaxWidth.Text) || int.Parse(txtMaxWidth.Text) < 100)
            {
                txtMaxWidth.Text = @"99";
            }
        }

        private void btnBrowseBackupPath_Click(object sender, EventArgs e)
        {
            using (var vBrowseFolder = new FolderBrowserDialog())
            {
                vBrowseFolder.ShowNewFolderButton = true;
                vBrowseFolder.Description = LocalizationHelper.GetLanguageString("Select Backup Folder...", "form", GetType().Name, "msg", "SelectBackupFolder", "Text");
                MyGlobal.GlobalTempDialog = "BrowseForFolder";

                var result = vBrowseFolder.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(vBrowseFolder.SelectedPath))
                {
                    var selectedPath = vBrowseFolder.SelectedPath;
                    var temp = selectedPath.EndsWith(@"\", StringComparison.Ordinal) ? string.Empty : @"\";

                    txtBackupPath.Text = $"{selectedPath}{temp}";
                    txtBackupPath.Focus();
                    txtBackupPath.SelectionStart = txtBackupPath.Text.Length;
                }
            }
        }

        private void btnBackupPathOpenFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", txtBackupPath.Text); //以檔案總管開啟指定路徑
        }

        private void btnBrowseLocalFolder_Click(object sender, EventArgs e)
        {
            using (var browseFolder = new FolderBrowserDialog())
            {
                browseFolder.ShowNewFolderButton = false;

                browseFolder.Description = LocalizationHelper.GetLanguageString
                (
                    "Select Company Update Folder...",
                    "form",
                    GetType().Name,
                    "msg",
                    "SelectCompanyUpdateFolderTitle",
                    "Text"
                );

                if (Directory.Exists(txtLocalFolder.Text))
                {
                    browseFolder.SelectedPath = txtLocalFolder.Text;
                }

                MyGlobal.GlobalTempDialog = "BrowseForFolder";

                if (browseFolder.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(browseFolder.SelectedPath))
                {
                    return;
                }

                rdoUpdateSourceLocal.Checked = true;
                txtLocalFolder.Text = browseFolder.SelectedPath;
                txtLocalFolder.Focus();
                txtLocalFolder.SelectionStart = txtLocalFolder.Text.Length;
                UpdateMetadataSourceControlState();
            }
        }

        private void btnLocalFolderOpenFolder_Click(object sender, EventArgs e)
        {
            var localFolder = txtLocalFolder.Text.Trim();

            if (!Directory.Exists(localFolder))
            {
                ShowInvalidUpdateFolderMessage
                (
                    $"The company update folder does not exist:\r\n\r\n{localFolder}",
                    "CompanyUpdateFolderNotFound",
                    localFolder
                );
                return;
            }

            Process.Start("explorer.exe", localFolder); //以檔案總管開啟指定路徑
        }

        private void btnHelp_LocalUpdateFolder_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString
            (
                "Company Update Folder is intended for computers that cannot access external update websites.\r\n\r\nIT administrator deployment steps:\r\n1. Download JasonQuery-Company-Update-v<version>.zip from https://jasonquery.org/JasonQueryUpdate/. For example: JasonQuery-Company-Update-v0.95.0.zip.\r\n2. Extract the ZIP to a local folder or UNC network share.\r\n3. Grant JasonQuery users read-only access to the folder.\r\n4. In JasonQuery, select [Tools] > [Options] > [Update Settings] > [Company Update Folder] and specify the full path used in step 2.\r\n\r\nKeep these extracted files together:\r\n• jasonquery-update.json\r\n• JasonQuery64.zip\r\n• README-Company-Update.txt\r\n\r\nLocal disk paths and UNC paths such as \\\\server\\share\\JasonQueryUpdate are supported. JasonQuery reads update metadata and packages only from the selected folder and does not connect to external update websites. The company offline update package contains Production updates only.",
                "form",
                GetType().Name,
                "msg",
                "Help_CompanyUpdateFolder",
                "Text"
            );

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cboDirection_SelectedIndexChanged(object sender, EventArgs e)
        {
            var direction = TextHelper.GetKeyFromDictionary(MyGlobal.dicDirection, cboDirection.Text);

            GridHelper.SetGridInteractionDirection(c1GridAutoReplaceInfo, direction);
            GridHelper.SetGridInteractionDirection(c1GridVisualStyle, direction);
        }

        private void btnClear3_Click(object sender, EventArgs e)
        {
            txtBackupPath.Text = string.Empty;
        }

        /// <summary>
        /// 根據指定控制項，找出剛好超過指定寬度的空白字串 (由控制項 TextSize 決定)
        /// </summary>
        /// <param name="targetWidth">目標寬度(像素)</param>
        /// <param name="measuringControl">用來測量寬度的 Control，例如 Label</param>
        /// <param name="iMaxLength">最大空白字元數，預設 100</param>
        /// <returns>剛好達到目標寬度的空白字串</returns>
        private static string GetFittingSpaceString(int targetWidth, Control measuringControl, int iMaxLength = 100)
        {
            var sb = new StringBuilder(iMaxLength);

            for (int i = 1; i <= iMaxLength; i++)
            {
                sb.Append(' ');
                measuringControl.Text = sb.ToString();

                if (measuringControl.Width > targetWidth)
                {
                    return sb.ToString();
                }
            }

            return sb.ToString();
        }

        private static string GetRgbColorCode(string sHtmlColorCode)
        {
            var color = ColorTranslator.FromHtml(sHtmlColorCode);

            return $"(R:{color.R}, G:{color.G}, B:{color.B})";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (TryProcessOptionEditorDeleteKey(keyData) || TryProcessOptionEditorShortcut(keyData))
            {
                return true;
            }

            var iKey = 0;

            switch (keyData)
            {
                case Keys.Control | Keys.F: //Ctrl+F
                    {
                        if (c1DockingTab.SelectedTab == tabKeywords)
                        {
                            if (editorOperatorKeywords.Focused || grpFindOperatorKeywords.Visible)
                            {
                                iKey = 0;
                            }
                            else if (editorBuiltInFunctions.Focused || grpFindBuiltInFunctions.Visible)
                            {
                                iKey = 1;
                            }
                            else if (editorBuiltInKeywords.Focused || grpFindBuiltInKeywords.Visible)
                            {
                                iKey = 2;
                            }
                            else if (editorUserDefinedKeywords.Focused || grpFindUserDefinedKeywords.Visible)
                            {
                                iKey = 3;
                            }

                            FindKeywords(iKey, true);
                        }

                        return true;
                    }
                case Keys.Control | Keys.M: //Ctrl+M
                    {
                        if (c1DockingTab.SelectedTab == tabDataGrid && c1GridVisualStyle.Focused)
                        {
                            c1GridVisualStyle.Row = 0;
                            c1GridVisualStyle.Col = 6;
                            c1GridVisualStyle.Select(); //Focus 切換到指定的 Cell
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.A: //Ctrl+A
                    {
                        if (c1DockingTab.SelectedTab == tabDataGrid && c1GridVisualStyle.Focused)
                        {
                            c1GridVisualStyle.SelectedRows.Clear();

                            for (var i = 0; i < c1GridVisualStyle.Splits[0].Rows.Count; i++)
                            {
                                c1GridVisualStyle.SelectedRows.Add(i);
                            }

                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.C: //Ctrl+C
                case Keys.Control | Keys.Insert: //20231020 Ctrl+INS
                    {
                        if (c1DockingTab.SelectedTab == tabAutoReplace)
                        {
                            if (txtKeyword.Focused)
                            {
                                txtKeyword.Copy();
                                return true;
                            }

                            if (editorAutoReplace.Focused)
                            {
                                editorAutoReplace.Copy();
                                return true;
                            }
                        }
                        else if (c1DockingTab.SelectedTab == tabQueryEditor && editor.Focused)
                        {
                            if (chkCopyAsHTML.Checked)
                            {
                                editor.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                                return true;
                            }
                            else
                            {
                                editor.Copy();
                                return true;
                            }
                        }
                        else if (c1DockingTab.SelectedTab == tabDataGrid)
                        {
                            if (c1GridVisualStyle.Focused)
                            {
                                ArrangeData("COPY");
                                return true;
                            }
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
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool TryProcessOptionEditorDeleteKey(Keys keyData)
        {
            if (keyData != Keys.Delete)
            {
                return false;
            }

            var optionEditor = _editableOptionEditors.FirstOrDefault(item => item.Focused);

            //editor 有獨立的 Ctrl+C, Copy as HTML 邏輯，因此只需在處理 Delete 時額外納入
            if (optionEditor == null && editor.Focused)
            {
                optionEditor = editor;
            }

            if (optionEditor == null)
            {
                return false;
            }

            if (!optionEditor.ReadOnly)
            {
                optionEditor.Clear();
            }

            return true;
        }

        private bool TryProcessOptionEditorShortcut(Keys keyData)
        {
            var optionEditor = _editableOptionEditors.FirstOrDefault(item => item.Focused);

            if (optionEditor != null)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.Y:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.Redo);
                            return true;
                        }
                    case Keys.Control | Keys.Z:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.Undo);
                            return true;
                        }
                    case Keys.Control | Keys.X:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.Cut);
                            return true;
                        }
                    case Keys.Control | Keys.C:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.Copy);
                            return true;
                        }
                    case Keys.Control | Keys.V:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.Paste);
                            return true;
                        }
                    case Keys.Control | Keys.A:
                        {
                            ExecuteOptionEditorCommand(optionEditor, OptionEditorCommand.SelectAll);
                            return true;
                        }
                }

                return false;
            }

            var previewEditor = _readOnlyOptionEditors.FirstOrDefault(item => item.Focused);

            if (previewEditor == null)
            {
                return false;
            }

            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    {
                        ExecuteOptionEditorCommand(previewEditor, OptionEditorCommand.Copy);
                        return true;
                    }
                case Keys.Control | Keys.A:
                    {
                        ExecuteOptionEditorCommand(previewEditor, OptionEditorCommand.SelectAll);
                        return true;
                    }
                case Keys.Control | Keys.Y:
                case Keys.Control | Keys.Z:
                case Keys.Control | Keys.X:
                case Keys.Control | Keys.V:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static class AutoReplaceColumn
        {
            public const int Pid = 0;
            public const int Keyword = 1;
            public const int Replacement = 2;
        }

        private enum OptionEditorCommand
        {
            Undo,
            Redo,
            Cut,
            Copy,
            Paste,
            Delete,
            SelectAll
        }

        private static class MenuColumn
        {
            public const int CellViewer = 0;
            public const int Dash0 = 1;
            public const int SelectAll = 2;
            public const int Dash1 = 3;
            public const int ExportToExcel = 4;
            public const int ExportToCsv = 5;
            public const int ExportToFile = 6;
            public const int Dash2 = 7;
            public const int Copy = 8;
            public const int CopyWithColumnNames = 9;
            public const int CopyColumnNames = 10;
            public const int Dash3 = 11;
            public const int FreezeColumn = 12;
            public const int UnfreezeColumn = 13;
        }
    }
}
