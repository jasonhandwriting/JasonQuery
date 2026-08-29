using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Events;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Update;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Transactions;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TabPage = Crownwood.Magic.Controls.TabPage;

//快速提示(關鍵字)
//LoadConnectionForm 關鍵字如右「檢查是否有暫存檔案需要開啟(還原)」

namespace JasonQuery.UI.Forms
{
    public partial class MainForm : Form
    {
        private QueryForm _queryForm;
        private SchemaBrowserForm _schemaBrowserForm;
        private SqlHistoryForm _sqlHistoryForm;
        private OptionsForm _optionsForm;
        private ContextMenuStrip _cMenu = new ContextMenuStrip();
        private readonly ToolTip _toolTip1 = new ToolTip();
        private string _tabToolTip = string.Empty;
        private HashSet<string> _MenuItems;
        private string _languageText = string.Empty;
        private bool _isMenuEnable = true;
        private bool _isFormLoadFinished = false; //表單是否載入完畢 (避免觸發事件)
        private int _changeFormSizeManually;
        private int _connectionFormChangeLocalization = -1;
        private readonly Queue<string> _mruList = new Queue<string>();
        private List<string> _lstGridHeader = new List<string>();
        private int _mouseMove = -1;
        private DataTable _dtDatabase;
        private DateTime? _pendingTransactionWarningStartTime;
        private DateTime? _pendingTransactionWarningNextTime; //按下 OK 後，間隔 5 分鐘後再跳一次提示訊息，避免使用者忘記 Commit 或 Rollback
        private string _pendingTransactionMessage = string.Empty;
        private static HashSet<string> SpecialTabName;

        private enum UpdateCheckScheduleOperation
        {
            Query,
            MarkCompleted
        }

        //20250602 簡化資料庫判斷方式
        private DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializePendingTransactionStatus()
        {
            _pendingTransactionMessage = LocalizationHelper.GetLanguageString("Not yet committed or rolled back!", "form", GetType().Name, "msg", "CommitPrompt", "Text");
            SetPendingTransactionStatus(AppConfigHelper.IsNotCommitYet); //20260704 此處的簽名帶入 AppConfigHelper.IsNotCommitYet，切換語系時才不會誤判！
        }

        private PendingTransactionReminderState GetPendingTransactionReminderState()
        {
            return new PendingTransactionReminderState
            {
                IsPending = AppConfigHelper.IsNotCommitYet,
                PendingSince = _pendingTransactionWarningStartTime,
                NextWarningTime = _pendingTransactionWarningNextTime
            };
        }

        private void ApplyPendingTransactionReminderState(PendingTransactionReminderState state)
        {
            if (state == null)
            {
                state = new PendingTransactionReminderState();
            }

            AppConfigHelper.IsNotCommitYet = state.IsPending;
            _pendingTransactionWarningStartTime = state.PendingSince;
            _pendingTransactionWarningNextTime = state.NextWarningTime;
        }

        private void SetPendingTransactionStatus(bool isPending)
        {
            var state = PendingTransactionReminderPolicy.SetPending
                        (
                            GetPendingTransactionReminderState(),
                            isPending,
                            DateTime.Now,
                            MyGlobal.PendingTransactionWarningIntervalMilliseconds
                        );

            ApplyPendingTransactionReminderState(state);

            spNotCommitYet.Visible = state.IsPending;
            lblNotCommitYet.Visible = state.IsPending;
            lblNotCommitYetTime.Visible = state.IsPending;

            if (state.IsPending)
            {
                lblNotCommitYet.Text = _pendingTransactionMessage;
                UpdatePendingTransactionElapsedTime();

                tmrPendingTransactionIdleCheck.Interval = 1000;
                tmrPendingTransactionIdleCheck.Enabled = true;
            }
            else
            {
                lblNotCommitYet.Text = string.Empty;
                lblNotCommitYetTime.Text = string.Empty;
                tmrPendingTransactionIdleCheck.Enabled = false;
            }
        }

        private void UpdatePendingTransactionElapsedTime()
        {
            var state = GetPendingTransactionReminderState();

            if (!state.IsPending || !state.PendingSince.HasValue)
            {
                lblNotCommitYetTime.Text = string.Empty;
                return;
            }

            var elapsed = PendingTransactionReminderPolicy.GetElapsed(state, DateTime.Now);

            lblNotCommitYetTime.Text = elapsed.TotalHours >= 1 ? elapsed.ToString(@"hh\:mm\:ss") : elapsed.ToString(@"mm\:ss");
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //20250622 for LOG file，載入設定值前，使用預設的日期格式
                MyLibrary.DateFormat = string.Empty;

                //20250413 暗紅色
                lblPrompt4NewConnection.ForeColor = Color.FromArgb(192, 0, 0);

                //20250615 log 的路徑及檔名
                #region
                var logPath = Path.Combine(Application.StartupPath, "log");

                if (!Directory.Exists(logPath))
                {
                    try
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    catch
                    {
                        logPath = Application.StartupPath;
                    }
                }

                AppConfigHelper.LogFileName = $@"{logPath}\JasonQuery.log";

                if (File.Exists(AppConfigHelper.LogFileName))
                {
                    try
                    {
                        File.Delete(AppConfigHelper.LogFileName);
                    }
                    catch
                    {
                        var now = $"{DateTime.Now:yyyyMMddHHmmss}";

                        AppConfigHelper.LogFileName = $@"{logPath}\JasonQuery_{now}.log";
                    }
                }
                #endregion

                SystemEvents.SessionEnding += SystemEvents_SessionEnding;

                var isCreateDbFile = false; //是否為新建立的 JasonQuery.db (判斷後續主畫面呈現的位置)
                var dbFilePath = Path.Combine(Application.StartupPath, "JasonQuery.db");

                if (!File.Exists(dbFilePath))
                {
                    using (TraceLogger.Time("Create JasonQuery.db"))
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("JasonQuery.Files.JasonQuery.db"))
                    using (var fileStream = new FileStream(dbFilePath, FileMode.CreateNew))
                    {
                        stream.CopyTo(fileStream);
                    }

                    isCreateDbFile = true;
                }

                var noneExistFileList = string.Empty;

                using (TraceLogger.Time("Check that all necessary dll files exist"))
                {
                    noneExistFileList = CheckFileExists();
                }

                var beVerb = " is";

                if (!string.IsNullOrEmpty(noneExistFileList))
                {
                    if ((noneExistFileList.Length - noneExistFileList.Replace("\r\n", string.Empty).Length) == 2)
                    {
                        beVerb = "s are"; //如果有換行符號，就表示有多個 dll 檔案找不到
                    }

                    _languageText = $"The program can't start because the following file{beVerb} missing from your computer.\r\n\r\nTry reinstalling the program to fix this problem.";
                    MessageBox.Show($"{_languageText}\r\n\r\n{noneExistFileList}", @"JasonQuery - System Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(Environment.ExitCode);
                }

                using (TraceLogger.Time("Load IconLibrary.dll"))
                {
                    //20250413 載入 IconLibrary
                    string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IconLibrary.dll");

                    MyGlobal.IconLibrary = Assembly.LoadFrom(dllPath);
                }

                //新增語系時，此處要維護
                LocalizationHelper.LocalizationList = "English;english.xml`Chinese (Traditional) - 中文繁體;chinese-cht.xml`Chinese (Simplified) - 中文简体;chinese-chs.xml";
                JasonQueryRepository.DbFileName = dbFilePath;
                JasonQueryRepository.DbConnectionString = $"Data Source={JasonQueryRepository.DbFileName};Version=3;New=False;Compress=True;";

                var versionInfo = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule?.FileName);

                //20240807 針對測試版本，MAJOR.MINOR.RELEASE.REVISION，使用第 3 個 RELEASE 版號，做為「測試版本的識別碼」
                AppConfigHelper.LocalVersion = versionInfo.FileVersion.Replace(".0.0", string.Empty).Replace(".0", string.Empty);

                var version = !MyLibrary.ShowVersion ? string.Empty : $" {AppConfigHelper.LocalVersion}";

                //尚未登入前，標題列顯示的內容
                Text = $"{Tag}{version}";
                AppConfigHelper.JasonQueryVersion = $"{Tag} {AppConfigHelper.LocalVersion}";

                if (JasonQueryRepository.CheckDBPassword(string.Empty))
                {
                    //do nothing
                }
                else
                {
                    using (TraceLogger.Time("Load Localization XML file"))
                    {
                        LocalizationHelper.LoadLocalizationXML();
                    }

                    using (TraceLogger.Time("Apply Localization"))
                    {
                        ApplyLocalization();
                    }

                    using (var form = new CustomPasswordDialog())
                    {
                        form.ShowDialog();
                    }
                }

                using (TraceLogger.Time("Load Global Setting"))
                {
                    LoadGlobalSetting();
                }

                if (AppConfigHelper.IsMainFormMaximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (isCreateDbFile)
                    {
                        Location = (Point)new Size(400, 150);
                    }
                    else
                    {
                        Location = (Point)new Size(AppConfigHelper.MainFormLocationX, AppConfigHelper.MainFormLocationY);
                    }

                    WindowState = FormWindowState.Normal;
                    ClientSize = new Size(AppConfigHelper.MainFormWidth - 16, AppConfigHelper.MainFormHeight - 38);
                }

                using (TraceLogger.Time("Load Localization XML file"))
                {
                    LocalizationHelper.LoadLocalizationXML();
                }

                using (TraceLogger.Time("Apply Localization"))
                {
                    ApplyLocalization();
                }

                #region 判斷是否要檢查新版本
                if (MyLibrary.CheckForUpdate)
                {
                    var isNeedToCheckUpdate = MyLibrary.CheckForUpdateValue == 0 || VerifyCheckForUpdate(UpdateCheckScheduleOperation.Query);

                    if (isNeedToCheckUpdate)
                    {
                        using (var form = new UpdateForm { IsCheckOnStartup = true })
                        {
                            form.ShowDialog();

                            if (form.UpdateCheckCompletedSuccessfully)
                            {
                                VerifyCheckForUpdate(UpdateCheckScheduleOperation.MarkCompleted);
                            }
                        }
                    }
                }
                #endregion

                lblDomainUser.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if (MyLibrary.ShowIP)
                {
                    btnIP.Text = MyGlobal.GetIPAddress();
                }
                else
                {
                    btnIP.Visible = false;
                    spDomainUser.Visible = false;
                }

                using (TraceLogger.Time("Enable Close Tab Menu"))
                {
                    EnableCloseTabMenu(false);
                }

                using (TraceLogger.Time("Load Connection Form"))
                {
                    LoadConnectionForm();
                }

                AppConfigHelper.JasonQueryVersion = $"{Tag} {AppConfigHelper.LocalVersion}, {DatabaseSqlExecutor.DatabaseVersionDisplayText}";

                AppConfigHelper.MainFormLeft = Left;
                AppConfigHelper.MainFormTop = Top;

                _isFormLoadFinished = true;

                using (TraceLogger.Time("Update Tab List"))
                {
                    UpdateTabList();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ApplyLocalization()
        {
            MessageBoxManager.Unregister();

            _languageText = LocalizationHelper.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");
            MessageBoxManager.OK = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");
            MessageBoxManager.Cancel = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Abort", "Global", "Global", "messagebox", "Abort", "Text");
            MessageBoxManager.Abort = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Retry", "Global", "Global", "messagebox", "Retry", "Text");
            MessageBoxManager.Retry = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Ignore", "Global", "Global", "messagebox", "Ignore", "Text");
            MessageBoxManager.Ignore = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Yes", "Global", "Global", "messagebox", "Yes", "Text");
            MessageBoxManager.Yes = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&No", "Global", "Global", "messagebox", "No", "Text");
            MessageBoxManager.No = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("&Make New Folder", "Global", "Global", "messagebox", "MakeNewFolder", "Text");
            MessageBoxManager.CreateNewFolder = _languageText;
            _languageText = LocalizationHelper.GetLanguageString("Browse For Folder", "Global", "Global", "messagebox", "FolderBrowserDialogTitle", "Text");
            MessageBoxManager.FolderBrowserDialogTitle = _languageText;
            MessageBoxManager.Register();

            var myItems = GetItems(mnuMainForm);

            var Check3DotText = new HashSet<string>
            {
                "mnuNewConnection", "mnuOptions", "mnuSchemaBrowser", "mnuSqlHistory", "mnuFileSplitter", "mnuWelcome", "mnuCheckForUpdatesManually", "mnuUpdateNow", "mnuReleaseNotes", "mnuReportBugs", "mnuAbout"
            };

            foreach (var item in myItems)
            {
                var name = item.Name;

                if (_MenuItems != null && _MenuItems.Contains(name)) //後面要自動加上 ... 的功能表項目
                {
                    item.Enabled = _isMenuEnable;
                }
                else
                {
                    if (name == "mnuOpenMethod")
                    {
                        item.Enabled = false; //20240206 有時會碰到 mnuOpenMethod 莫名變成 Enabled = true，故強制切換為 false
                    }
                    else
                    {
                        if (name != "mnuMyFavorite" && name != "mnuRecentFiles") //20250831 忽略這兩個選單
                        {
                            item.Enabled = !_isMenuEnable;
                        }
                    }
                }

                var text = LocalizationHelper.GetLanguageString(item.Text, "form", GetType().Name, "menu", name, "Text");
                var points = Check3DotText.Contains(name) ? "..." : string.Empty;

                item.Text = $"{text}{points}";
                item.ToolTipText = LocalizationHelper.GetLanguageString(item.ToolTipText, "form", GetType().Name, "menu", name, "ToolTipText");
                item.ShortcutKeyDisplayString = LocalizationHelper.GetLanguageString(item.ShortcutKeyDisplayString, "form", GetType().Name, "menu", name, "ShortcutKeyDisplayString");

                var textTrimEnd = item.Text.TrimEnd('.');

                switch (name)
                {
                    case "mnuCreateTable":
                        {
                            switch (_currentSourceType)
                            {
                                case DataSourceType.Oracle: //20240408
                                    {
                                        item.Enabled = true;
                                        break;
                                    }
                                default:
                                    {
                                        item.Enabled = false;
                                        break;
                                    }
                            }

                            MyGlobal.CreateTableTabName = textTrimEnd;
                            break;
                        }
                    case "mnuImportTableData":
                        {
                            switch (_currentSourceType)
                            {
                                case DataSourceType.Oracle: //未完成
                                    {
                                        item.Enabled = false;
                                        break;
                                    }
                                default:
                                    {
                                        item.Enabled = false;
                                        break;
                                    }
                            }

                            break;
                        }
                    case "mnuOptions":
                        {
                            MyGlobal.OptionsTabName = textTrimEnd;
                            break;
                        }
                    case "mnuSchemaBrowser":
                        {
                            MyGlobal.SchemaBrowserTabName = textTrimEnd;
                            break;
                        }
                    case "mnuSqlHistory":
                        {
                            MyGlobal.SqlHistoryTabName = textTrimEnd;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            LocalizationHelper.LocalizationMap = new Dictionary<string, string>();

            var comboBoxString = LocalizationHelper.LocalizationList.Split(new[] { "`" }, StringSplitOptions.None);

            foreach (var t in comboBoxString)
            {
                LocalizationHelper.LocalizationMap.Add(t.Split(';')[0], t.Split(';')[1]);
            }

            MyGlobal.dicWordWrapIndentMode = new Dictionary<string, string>();

            _languageText = LocalizationHelper.GetLanguageString("Fixed", "form", "OptionsForm", "dropdownlist", "IndentMode_Fixed", "Text");
            MyGlobal.dicWordWrapIndentMode.Add("Fixed", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Same", "form", "OptionsForm", "dropdownlist", "IndentMode_Same", "Text");
            MyGlobal.dicWordWrapIndentMode.Add("Same", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Indent", "form", "OptionsForm", "dropdownlist", "IndentMode_Indent", "Text");
            MyGlobal.dicWordWrapIndentMode.Add("Indent", _languageText);

            MyGlobal.dicDirection = new Dictionary<string, string>();

            _languageText = LocalizationHelper.GetLanguageString("Down", "form", "OptionsForm", "dropdownlist", "Down", "Text");
            MyGlobal.dicDirection.Add("Down", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Right", "form", "OptionsForm", "dropdownlist", "Right", "Text");
            MyGlobal.dicDirection.Add("Right", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Up", "form", "OptionsForm", "dropdownlist", "Up", "Text");
            MyGlobal.dicDirection.Add("Up", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Left", "form", "OptionsForm", "dropdownlist", "Left", "Text");
            MyGlobal.dicDirection.Add("Left", _languageText);

            MyGlobal.dicRowSizing = new Dictionary<string, string>();

            _languageText = LocalizationHelper.GetLanguageString("All Rows", "form", "OptionsForm", "dropdownlist", "RowHeightResizing_AllRows", "Text");
            MyGlobal.dicRowSizing.Add("AllRows", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Individual Rows", "form", "OptionsForm", "dropdownlist", "RowHeightResizing_IndividualRows", "Text");
            MyGlobal.dicRowSizing.Add("IndividualRows", _languageText);

            MyGlobal.dicBookmarkStyle = new Dictionary<string, string>();

            _languageText = LocalizationHelper.GetLanguageString("Arrow", "form", "OptionsForm", "dropdownlist", "BookmarkStyle_Arrow", "Text");
            MyGlobal.dicBookmarkStyle.Add("Arrow", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Circle", "form", "OptionsForm", "dropdownlist", "BookmarkStyle_Circle", "Text");
            MyGlobal.dicBookmarkStyle.Add("Circle", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("RoundRect", "form", "OptionsForm", "dropdownlist", "BookmarkStyle_RoundRect", "Text");
            MyGlobal.dicBookmarkStyle.Add("RoundRect", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("ShortArrow", "form", "OptionsForm", "dropdownlist", "BookmarkStyle_ShortArrow", "Text");
            MyGlobal.dicBookmarkStyle.Add("ShortArrow", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("SmallRect", "form", "OptionsForm", "dropdownlist", "BookmarkStyle_SmallRect", "Text");
            MyGlobal.dicBookmarkStyle.Add("SmallRect", _languageText);

            MyGlobal.dicCsvDelimiters = new Dictionary<string, string>();

            _languageText = LocalizationHelper.GetLanguageString("Tab", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Tab", "Text");
            MyGlobal.dicCsvDelimiters.Add("Tab", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Semicolon", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Semicolon", "Text");
            MyGlobal.dicCsvDelimiters.Add("Semicolon", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Comma", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Comma", "Text");
            MyGlobal.dicCsvDelimiters.Add("Comma", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Space", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Space", "Text");
            MyGlobal.dicCsvDelimiters.Add("Space", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Colon", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Colon", "Text");
            MyGlobal.dicCsvDelimiters.Add("Colon", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Slash", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Slash", "Text");
            MyGlobal.dicCsvDelimiters.Add("Slash", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Backslash", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Backslash", "Text");
            MyGlobal.dicCsvDelimiters.Add("Backslash", _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Pipe", "form", "ExportToFileForm", "dropdownlist", "Delimiters_Pipe", "Text");
            MyGlobal.dicCsvDelimiters.Add("Pipe", _languageText);

            _lstGridHeader = new List<string>();

            _languageText = LocalizationHelper.GetLanguageString("Close", "form", GetType().Name, "menu", "Close", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all windows", "form", GetType().Name, "menu", "CloseAll", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all windows except for this one", "form", GetType().Name, "menu", "CloseAllButThis", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all windows on the left", "form", GetType().Name, "menu", "CloseAllLeft", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all windows on the right", "form", GetType().Name, "menu", "CloseAllRight", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all saved file(s)", "form", GetType().Name, "menu", "CloseAllSavedFiles", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Close all unsaved file(s)", "form", GetType().Name, "menu", "CloseAllUnsavedFiles", "Text");
            _lstGridHeader.Add(_languageText);
            _lstGridHeader.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("RenameTab", "form", GetType().Name, "menu", "RenameTab", "Text");
            _lstGridHeader.Add(_languageText);
            _lstGridHeader.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("&New SQL Editor", "form", GetType().Name, "menu", "NewSQLEditor", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("&Open File(s)", "form", GetType().Name, "menu", "OpenFiles", "Text");
            _lstGridHeader.Add(_languageText);
            _lstGridHeader.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Add to \"My Favorite\"", "form", GetType().Name, "menu", "AddToMyFavorite", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Remove from \"My Favorite\"", "form", GetType().Name, "menu", "RemoveFromMyFavorite", "Text");
            _lstGridHeader.Add(_languageText);
            _lstGridHeader.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Open Containing Folder in Explorer", "form", GetType().Name, "menu", "OpenFolder", "Text");
            _lstGridHeader.Add(_languageText);
            _lstGridHeader.Add("-");
            _languageText = LocalizationHelper.GetLanguageString("Copy Full File Path to Clipboard", "form", GetType().Name, "menu", "CopyFullFilePath", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Copy File Name to Clipboard", "form", GetType().Name, "menu", "CopyFileName", "Text");
            _lstGridHeader.Add(_languageText);
            _languageText = LocalizationHelper.GetLanguageString("Copy Current Dir. Path to Clipboard", "form", GetType().Name, "menu", "CopyCurrentPath", "Text");
            _lstGridHeader.Add(_languageText);

            //20240222 將 ContextMenu 改為 ContextMenuStrip
            _cMenu = new ContextMenuStrip();

            _cMenu.Items.Add(_lstGridHeader[MapColumn.Close]);

            _cMenu.Items[MapColumn.Close].Click += delegate
            {
                CloseIt(null, null);
            };

            _cMenu.Items[MapColumn.Close].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Close 16x16.ico");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAll]);

            _cMenu.Items[MapColumn.CloseAll].Click += delegate
            {
                var tabPagesCount0 = tabControl1.TabPages.Count - 1;

                for (var i = tabPagesCount0; i >= 0; i--)
                {
                    CloseTabIndex(i);
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items[MapColumn.CloseAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Close All 16x16.ico");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAllButThis]);

            _cMenu.Items[MapColumn.CloseAllButThis].Click += delegate
            {
                var tabPagesCount0 = tabControl1.TabPages.Count;
                var tabPagesCount1 = tabControl1.TabPages.Count - 1;
                var title = tabControl1.SelectedTab.Title;

                for (var i = tabPagesCount1; i >= 0; i--)
                {
                    var text = tabControl1.TabPages[i].Title;

                    if (!string.Equals(title, text, StringComparison.Ordinal))
                    {
                        CloseTabIndex(i);
                    }
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAllLeft]);

            _cMenu.Items[MapColumn.CloseAllLeft].Click += delegate
            {
                var isSpecifiedTab = false;
                var tabPagesCount0 = tabControl1.TabPages.Count;
                var tabPagesCount1 = tabControl1.TabPages.Count - 1;
                var title = tabControl1.SelectedTab.Title;

                for (var i = tabPagesCount1; i >= 0; i--)
                {
                    var text = tabControl1.TabPages[i].Title;

                    if (string.Equals(title, text, StringComparison.Ordinal))
                    {
                        isSpecifiedTab = true;
                        continue;
                    }

                    if (isSpecifiedTab)
                    {
                        CloseTabIndex(i);
                    }
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAllRight]);

            _cMenu.Items[MapColumn.CloseAllRight].Click += delegate
            {
                var tabPagesCount0 = tabControl1.TabPages.Count;
                var tabPagesCount1 = tabControl1.TabPages.Count - 1;
                var title = tabControl1.SelectedTab.Title;

                for (var i = tabPagesCount1; i >= 0; i--)
                {
                    var text = tabControl1.TabPages[i].Title;

                    if (string.Equals(title, text, StringComparison.Ordinal))
                    {
                        break;
                    }

                    CloseTabIndex(i);
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAllSavedFiles]);

            _cMenu.Items[MapColumn.CloseAllSavedFiles].Click += delegate
            {
                var tabPagesCount0 = tabControl1.TabPages.Count;
                var tabPagesCount1 = tabControl1.TabPages.Count - 1;

                for (var i = tabPagesCount1; i >= 0; i--)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var isSaved = !(title.StartsWith("*", StringComparison.Ordinal) || SpecialTabName.Contains(title));

                    if (isSaved)
                    {
                        CloseTabIndex(i);
                    }
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CloseAllUnsavedFiles]);

            _cMenu.Items[MapColumn.CloseAllUnsavedFiles].Click += delegate
            {
                var tabPagesCount0 = tabControl1.TabPages.Count;
                var tabPagesCount1 = tabControl1.TabPages.Count - 1;

                for (var i = tabPagesCount1; i >= 0; i--)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var isUnsaved = !(!title.StartsWith("*", StringComparison.Ordinal) || SpecialTabName.Contains(title));

                    if (isUnsaved)
                    {
                        CloseTabIndex(i);
                    }
                }

                var tabPagesCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                if (tabPagesCount0 > 0 && tabPagesCount0 != tabPagesCount)
                {
                    UpdateTabList();
                    return;
                }

                //自動再開啟一個空白的 Tab
                CreateNewTab("Query", CheckTabNameExist());
            };

            _cMenu.Items.Add("-");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.RenameTab]);

            _cMenu.Items[MapColumn.RenameTab].Click += delegate
            {
                var tabTitle = tabControl1.SelectedTab.Title.Replace("*", string.Empty);
                var accessibleDescription = tabControl1.SelectedTab.AccessibleDescription;

                using (var form = new RenameTabDialog())
                {
                    var x = Cursor.Position.X - 30;
                    var y = Cursor.Position.Y - 72;

                    form.Location = new Point(x, y);
                    form.AccessibleDescriptionString = accessibleDescription;
                    form.TabTitle = tabTitle;
                    form.IsStar = tabControl1.SelectedTab.Title.StartsWith("*", StringComparison.Ordinal);
                    form.ShowDialog();
                }
            };

            _cMenu.Items[MapColumn.RenameTab].Image = IconManager.GetImage(MyGlobal.IconLibrary, "RenameTab 16x16.ico"); //20241221

            _cMenu.Items.Add("-");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.NewSQLEditor]);

            _cMenu.Items[MapColumn.NewSQLEditor].Click += delegate
            {
                CreateNewTab("Query", CheckTabNameExist()); //Tab頁籤的右鍵選單
            };

            _cMenu.Items[MapColumn.NewSQLEditor].Image = IconManager.GetImage(MyGlobal.IconLibrary, "New File 16x16.ico");
            ((ToolStripMenuItem)_cMenu.Items[MapColumn.NewSQLEditor]).ShortcutKeys = Keys.Control | Keys.N;

            _cMenu.Items.Add(_lstGridHeader[MapColumn.OpenFile]);

            _cMenu.Items[MapColumn.OpenFile].Click += delegate
            {
                OpenFile();
            };

            _cMenu.Items[MapColumn.OpenFile].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Open 16x16.ico");
            ((ToolStripMenuItem)_cMenu.Items[MapColumn.OpenFile]).ShortcutKeys = Keys.Control | Keys.O;

            _cMenu.Items.Add("-");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.AddToMyFavorite]);

            _cMenu.Items[MapColumn.AddToMyFavorite].Click += delegate
            {
                AddToMyFavoriteFiles(_cMenu.Items[MapColumn.AddToMyFavorite], null);
            };

            _cMenu.Items[MapColumn.AddToMyFavorite].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Add 16x16.ico");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.RemoveFromMyFavorite]);

            _cMenu.Items[MapColumn.RemoveFromMyFavorite].Click += delegate
            {
                RemoveFromMyFavoriteFiles(_cMenu.Items[MapColumn.RemoveFromMyFavorite], null);
            };

            _cMenu.Items[MapColumn.RemoveFromMyFavorite].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Remove 16x16.ico");

            _cMenu.Items.Add("-");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.OpenFolder]);

            _cMenu.Items[MapColumn.OpenFolder].Click += delegate
            {
                OpenFolder(_cMenu.Items[MapColumn.OpenFolder], null);
            };

            _cMenu.Items[MapColumn.OpenFolder].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Open Path 16x16.ico");

            _cMenu.Items.Add("-");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CopyFullFilePath]);

            _cMenu.Items[MapColumn.CopyFullFilePath].Click += delegate
            {
                CopyFullFilePath(_cMenu.Items[MapColumn.CopyFullFilePath], null);
            };

            _cMenu.Items[MapColumn.CopyFullFilePath].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste 16x16.ico");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CopyFileName]);

            _cMenu.Items[MapColumn.CopyFileName].Click += delegate
            {
                CopyFileName(_cMenu.Items[MapColumn.CopyFileName], null);
            };

            _cMenu.Items[MapColumn.CopyFileName].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste Filename 16x16.ico");

            _cMenu.Items.Add(_lstGridHeader[MapColumn.CopyCurrentPath]);

            _cMenu.Items[MapColumn.CopyCurrentPath].Click += delegate
            {
                CopyCurrentPath(_cMenu.Items[MapColumn.CopyCurrentPath], null);
            };

            _cMenu.Items[MapColumn.CopyCurrentPath].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paste Path 16x16.ico");

            if (string.IsNullOrEmpty(MyGlobal.TabBackColor))
            {
                //20250413 提示使用者
                lblPrompt4NewConnection.Text = LocalizationHelper.GetLanguageString("You can create or select database connection from [File] > [New Connection]!", "form", GetType().Name, "object", "lblPrompt4NewConnection", "Text");

                return;
            }

            tabControl1.BackColor = ColorTranslator.FromHtml(MyGlobal.TabBackColor);
            tabControl1.ForeColor = ColorTranslator.FromHtml(MyGlobal.TabActiveForeColor);
            tabControl1.TextInactiveColor = ColorTranslator.FromHtml(MyGlobal.TabInactiveForeColor);
            tabControl1.ShrinkPagesToFit = MyGlobal.IsTabShrinkPages;
            tabControl1.ShowArrows = MyGlobal.IsTabShowArrows;
            tabControl1.HoverSelect = MyGlobal.IsTabHoverSelect;
            tabControl1.Multiline = MyGlobal.IsTabMultiLine;
            tabControl1.ShowClose = !mnuNewConnection.Enabled;

            tabControl1.Style = MyLibrary.TabStyle == "IDE" ? Crownwood.Magic.Common.VisualStyle.IDE : Crownwood.Magic.Common.VisualStyle.Plain;

            switch (MyLibrary.TabAppearance)
            {
                case "MultiDocument":
                    {
                        tabControl1.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiDocument;
                        break;
                    }
                case "MultiForm":
                    {
                        tabControl1.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiForm;
                        break;
                    }
                default:
                    {
                        tabControl1.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiBox;
                        break;
                    }
            }

            tabControl1.PositionTop = true;
            tabControl1.BoldSelectedPage = true;
            tabControl1.BorderStyle = BorderStyle.None;

            //20220802
            if (IsPostgreSql)
            {
                btnAutoRollbackOnErrorOn.Text = LocalizationHelper.GetLanguageString("Auto Rollback on error", "form", GetType().Name, "object", "btnAutoRollbackOnError", "Text");
                btnAutoRollbackOnErrorOff.Text = LocalizationHelper.GetLanguageString("Auto Rollback on error", "form", GetType().Name, "object", "btnAutoRollbackOnError", "Text");

                if (DatabaseSqlExecutor.UseAutoRollback)
                {
                    btnAutoRollbackOnErrorOn.Visible = true;
                }
                else
                {
                    btnAutoRollbackOnErrorOff.Visible = true;
                }

                spAutoRollbackOnError.Visible = true;
            }

            lblAutoCommit.Text = LocalizationHelper.GetLanguageString("Auto Commit is off", "form", GetType().Name, "object", "lblAutoCommit", "Text");

            //20240314 變更提示
            switch (LocalizationHelper.LocalizationCode)
            {
                case "zh-TW":
                case "zh-CN":
                    {
                        mnuReleaseNotes.ToolTipText = LocalizationHelper.GetLanguageString("Use default browser to open the \"releasenotes.html\"", "form", GetType().Name, "menu", "mnuReleaseNotes", "ToolTipText").Replace("releasenotes.html", "releasenotes_cht.html");
                        break;
                    }
            }

            #region 20240507 把這段從 Form_Load 移到此處，每次切換語系時，才會重新載入
            //新增語系時，此處要維護
            switch (LocalizationHelper.Localization)
            {
                case "Chinese (Traditional) - 中文繁體":
                    {
                        _languageText = "zh-TW";
                        break;
                    }
                case "Chinese (Simplified) - 中文简体":
                    {
                        _languageText = "zh-CN";
                        break;
                    }
                default: //English
                    {
                        _languageText = "en-US";
                        break;
                    }
            }

            LocalizationHelper.LocalizationCode = _languageText;

            var cultureInfo = new CultureInfo(_languageText)
            {
                DateTimeFormat =
                {
                    ShortDatePattern = MyLibrary.DateFormat,
                    ShortTimePattern = "HH:mm:ss",
                    LongDatePattern = MyLibrary.DateFormat,
                    LongTimePattern = "HH:mm:ss"
                }
            };

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            #endregion

            SetSpecialTabName();
            InitializePendingTransactionStatus();
        }

        private void SetSpecialTabName()
        {
            //20250615 依語系變化，取得最新的值
            SpecialTabName = new HashSet<string>
            {
                MyGlobal.OptionsTabName, MyGlobal.SchemaBrowserTabName, MyGlobal.SqlHistoryTabName, MyGlobal.CreateTableTabName
            };
        }

        private static string CheckFileExists()
        {
            var sbResult = new StringBuilder();

            if (!File.Exists($"{Application.StartupPath}\\C1.C1Excel.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.C1Excel.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.C1Zip.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.C1Zip.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.Bitmap.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.Bitmap.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1Command.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1Command.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1DX.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1DX.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1Input.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1Input.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1Ribbon.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1Ribbon.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1Themes.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1Themes.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1Themes.Extended.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1Themes.Extended.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1TrueDBGrid.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1TrueDBGrid.4.5.2.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\C1.Win.C1TrueDBGrid.Excel.4.5.2.dll"))
            {
                sbResult.AppendLine("C1.Win.C1TrueDBGrid.Excel.4.5.2.dll");
            }
            if (!File.Exists($"{Application.StartupPath}\\Devart.Data.dll"))
            {
                sbResult.AppendLine("Devart.Data.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\Devart.Data.Oracle.dll"))
            {
                sbResult.AppendLine("Devart.Data.Oracle.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\Devart.Data.PostgreSql.dll"))
            {
                sbResult.AppendLine("Devart.Data.PostgreSql.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\Devart.Data.SqlServer.dll"))
            {
                sbResult.AppendLine("Devart.Data.SqlServer.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\Devart.Data.MySql.dll"))
            {
                sbResult.AppendLine("Devart.Data.MySql.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\JasonLibrary.dll"))
            {
                sbResult.AppendLine("JasonLibrary.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\IconLibrary.dll"))
            {
                sbResult.AppendLine("IconLibrary.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\MagicLibrary.dll"))
            {
                sbResult.AppendLine("MagicLibrary.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\Microsoft.SqlServer.TransactSql.ScriptDom.dll"))
            {
                sbResult.AppendLine("Microsoft.SqlServer.TransactSql.ScriptDom.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\SQL.Formatter.dll"))
            {
                sbResult.AppendLine("SQL.Formatter.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\ScintillaNET.dll"))
            {
                sbResult.AppendLine("ScintillaNET.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\SQLite.Interop.dll"))
            {
                sbResult.AppendLine("SQLite.Interop.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\System.Data.SQLite.dll"))
            {
                sbResult.AppendLine("System.Data.SQLite.dll");
            }

            if (!File.Exists($"{Application.StartupPath}\\System.Threading.Tasks.Extensions.dll"))
            {
                sbResult.AppendLine("System.Threading.Tasks.Extensions.dll");
            }

            return sbResult.ToString();
        }

        private void btnAutoRollbackOnError_Click(object sender, EventArgs e)
        {
            btnAutoRollbackOnErrorOn.Visible = !btnAutoRollbackOnErrorOn.Visible;
            btnAutoRollbackOnErrorOff.Visible = !btnAutoRollbackOnErrorOn.Visible;

            DatabaseSqlExecutor.UseAutoRollback = btnAutoRollbackOnErrorOn.Visible;
        }

        private string GetLanguage(CultureInfo culture)
        {
            if (!string.Equals(culture.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase))
            {
                return "English";
            }

            var cultureName = culture.Name;

            //判斷繁體中文
            var traditionalSuffixes = new[] { "hant", "tw", "hk", "mo" };
            var isTraditional = traditionalSuffixes.Any(suffix => cultureName.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) >= 0);

            if (isTraditional)
            {
                return "Chinese (Traditional) - 中文繁體"; //此處使用 LocalizationHelper.sLocalizationList 變數的定義
            }

            //判斷簡體中文 (新加坡大部份使用簡體中文)
            var simplifiedSuffixes = new[] { "hant", "tw", "hk", "mo" };
            var isSimplified = simplifiedSuffixes.Any(suffix => cultureName.IndexOf(suffix, StringComparison.OrdinalIgnoreCase) >= 0);

            if (isSimplified)
            {
                return "Chinese (Simplified) - 中文简体"; //此處使用 LocalizationHelper.sLocalizationList 變數的定義
            }

            return "English";
        }

        private void LoadGlobalSetting(bool bChangeLocalization = false)
        {
            #region 載入 Global 設定值
            //檢查是否有此帳號的 Global 設定值，若沒有，則 Insert 預設值
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
            sbSql.Append("   AND MPID IS NOT NULL");

            var sql = sbSql.ToString();
            var dtData = JasonQueryRepository.ExecQuery(sql);

            //建立 Dictionary<Name,Value> 加速取值，若有重複則取第一筆
            var configTable = (dtData.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                .GroupBy
                 (
                     row => row.GetSafeString("AttributeName"), StringComparer.OrdinalIgnoreCase
                 )
                .ToDictionary
                 (
                     g => g.Key,
                     g => g.First().GetSafeString("AttributeValue"),
                     StringComparer.OrdinalIgnoreCase
                 );

            //需要 Insert 資料 (並回傳是否為新插入)
            bool EnsureInsert(string sName, string sValue)
            {
                if (!configTable.ContainsKey(sName))
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine("       (DomainUser, AttributeKey, AttributeName, AttributeValue, AttributeDate)");
                    sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', 'GlobalConfig', '{sName}', '{sValue}', '{MyGlobal.DateTimeNow()}')");

                    var sqlInsert = sbSql.ToString();

                    JasonQueryRepository.ExecNonQuery(sqlInsert);

                    configTable[sName] = sValue; //同步放入字典，供後續查詢
                    return true;
                }

                return false;
            }

            //取值 - 字串
            string GetString(string sName, string defaultValue = "") =>
                   configTable.TryGetValue(sName, out var value) ? value : defaultValue;

            //取值 - 布林值
            bool GetBool(string name, bool defaultValue) =>
                 configTable.TryGetValue(name, out var value) ? (value == "1") : defaultValue;

            //取值 - 數值
            int GetInt(string name, int defaultValue) =>
                configTable.TryGetValue(name, out var value) && int.TryParse(value, out var i) ? i : defaultValue;

            if (!bChangeLocalization)
            {
                //取得語系設定
                LocalizationHelper.Localization = GetString("Localization");

                //20250406 如果是第一次執行 JasonQuery，依作業系統的語系決定 LocalizationHelper.sLocalization 的預設值
                if (string.IsNullOrEmpty(LocalizationHelper.Localization))
                {
                    CultureInfo osCulture = CultureInfo.InstalledUICulture;
                    CultureInfo userCulture = CultureInfo.CurrentUICulture;
                    string osLanguage = GetLanguage(osCulture); //取得作業系統安裝時的預設 UI 語言
                    string userLanguage = GetLanguage(userCulture); //取得目前使用者所設定的 UI 語言 (如果使用者有更改過區域設置，這裡會不同於 osCulture)

                    LocalizationHelper.Localization = osLanguage;
                }
            }

            bool isNewInsert = EnsureInsert("EnableCheckForUpdate", "1");

            MyLibrary.CheckForUpdate = isNewInsert ? false : GetBool("EnableCheckForUpdate", true);

            //CheckForUpdateDays：若新插入則預設 7，並取值
            EnsureInsert("CheckForUpdateDays", "7"); //20200715 此處要寫入記錄，否則第一次啟動時找不到 JasonQuery.db，產生 JasonQuery.db 後，馬上就會檢查更新
            MyLibrary.CheckForUpdateValue = GetInt("CheckForUpdateDays", 7);

            MyLibrary.UpdateMetadataSource = UpdateMetadataSettingsContract.ParseSource
            (
                GetString
                (
                    UpdateMetadataSettingsContract.SourceSettingName,
                    UpdateMetadataSettingsContract.DefaultSource.ToString()
                )
            );

            MyLibrary.UpdateMetadataLocalFolder = GetString
            (
                UpdateMetadataSettingsContract.LocalFolderSettingName,
                string.Empty
            );

            AppConfigHelper.IsBackupFile = GetBool("BackupFile", true);
            AppConfigHelper.AskBeforeOpenUnsavedFiles = GetBool("AskMeBeforeOpenUnsavedFiles", false);

            var temp01 = Path.Combine(Application.StartupPath, "backup");

            //備份路徑
            AppConfigHelper.BackupPath = $"{temp01}{Path.DirectorySeparatorChar}";

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

            MyLibrary.DateFormat = GetString("DateFormat");
            MyLibrary.ShowDatabaseName = GetBool("ShowDatabaseName", true);
            MyLibrary.ShowVersion = GetBool("ShowVersion", true);
            MyLibrary.ShowIP = GetBool("ShowIP", true);

            AppConfigHelper.IsMainFormMaximized = GetBool("MainFormMaximized", false);
            AppConfigHelper.MainFormLocationX = GetInt("MainFormLocationX", 1);
            AppConfigHelper.MainFormLocationY = GetInt("MainFormLocationY", 1);
            AppConfigHelper.MainFormWidth = GetInt("MainFormWidth", 1024);
            AppConfigHelper.MainFormHeight = GetInt("MainFormHeight", 768);
            AppConfigHelper.LargeTextPreviewLength = GetInt("LargeTextPreviewLength", 50);

            MyLibrary.RecentFilesQty = GetInt("RecentFilesQty", 20);
            MyLibrary.MyFavoriteQty = GetInt("MyFavoriteQty", 20);

            //Commit/Rollback 圖示
            var iconValue = GetInt("CommitRollbackIcon", 1);

            MyGlobal.CommitRollbackIcon = (iconValue >= 1 && iconValue <= 6) ? iconValue : 1;

            if (MyLibrary.CommitRollbackIcon == -1)
            {
                MyLibrary.CommitRollbackIcon = MyGlobal.CommitRollbackIcon; //20240724 每次重啟才會變更
            }

            MyLibrary.ColorOptionsTabActiveForeColor = GetString("OptionsTabActiveForeColor");
            MyLibrary.ColorOptionsTabActiveBackColor = GetString("OptionsTabActiveBackColor");
            MyLibrary.ColorOptionsTabInactiveForeColor = GetString("OptionsTabInactiveForeColor");
            MyLibrary.TabStyle = GetString("TabStyle");
            MyLibrary.TabAppearance = GetString("TabAppearance");
            MyGlobal.IsTabBold = GetBool("TabBold", true);
            MyGlobal.IsTabShrinkPages = GetBool("TabShrinkPages", true);
            MyGlobal.IsTabShowArrows = GetBool("TabShowArrows", false);
            MyGlobal.IsTabHoverSelect = GetBool("TabHoverSelect", false);
            MyGlobal.IsTabMultiLine = GetBool("TabMultiLine", false);
            #endregion

            #region 載入 EditTableData 設定值
            MyLibrary.ColorNewRowForeColor = GetString("EditTableData_NewRowForeColor");
            MyLibrary.ColorNewRowBackColor = GetString("EditTableData_NewRowBackColor");
            MyLibrary.ColorDeletedRowForeColor = GetString("EditTableData_DeletedRowForeColor");
            MyLibrary.ColorDeletedRowBackColor = GetString("EditTableData_DeletedRowBackColor");
            MyLibrary.ColorChangedCellForeColor = GetString("EditTableData_ChangedCellForeColor");
            MyLibrary.ColorChangedCellBackColor = GetString("EditTableData_ChangedCellBackColor");
            #endregion

            //20260620 將中斷連線改為 Pending transaction warning
            MyGlobal.IsPendingTransactionWarning = GetBool("PendingTransactionIdleWarningEnabled", true);
        }

        private void LoadConnectionForm()
        {
            string temp;
            var isConnectToDatabase = true;

            //20250413 提示訊息
            picArrow.Visible = false;
            lblPrompt4NewConnection.Visible = false;

            using (var form = new ConnectionForm())
            {
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowDialog();

                if (_connectionFormChangeLocalization > -1)
                {
                    _connectionFormChangeLocalization = -2;
                }
                else if (_connectionFormChangeLocalization == -2)
                {
                    return;
                }
            }

            Application.UseWaitCursor = true;

            if (DatabaseSqlExecutor.CurrentDataSource != DataSourceType.None)
            {
                _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;

                var connectTo = ConnectToDatabase(); //嘗試連線資料庫

                if (!string.IsNullOrEmpty(connectTo))
                {
                    var sb = new StringBuilder();

                    _languageText = LocalizationHelper.GetLanguageString("Error connecting to the server:", "Global", "Global", "msg", "ErrorConnectingToTheServer", "Text");
                    sb.AppendLine(_languageText);
                    sb.AppendLine();
                    _languageText = LocalizationHelper.GetLanguageString("Connection Name:", "form", "ConnectionForm", "object", "lblConnectionName", "Text");
                    sb.AppendLine($"{_languageText} {DatabaseSqlExecutor.DbConnectionName}");
                    _languageText = LocalizationHelper.GetLanguageString("Server:", "form", "ConnectionForm", "object", "lblServer", "Text");
                    sb.AppendLine($"{_languageText} {DatabaseSqlExecutor.DbConnectionServer}");
                    sb.AppendLine();
                    sb.AppendLine(connectTo);

                    MessageBox.Show(sb.ToString(), AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isConnectToDatabase = false;
                }
            }

            if (DatabaseSqlExecutor.CurrentDataSource == DataSourceType.None || !isConnectToDatabase)
            {
                EnableCloseTabMenu(false);
                _isMenuEnable = true; //只能使用「基本功能」裡面的項目

                //20250613 改為 HashSet
                _MenuItems = new HashSet<string>
                {
                    "mnuFile", "mnuNewConnection", "mnuExit", "mnuHelp", "mnuWelcome", "mnuCheckForUpdatesManually", "mnuUpdateNow", "mnuReleaseNotes", "mnuReportBugs", "mnuAbout", "mnuAssistant", "mnuWindowsExplorer", "mnuNotepad", "mnuCalculator", "mnuPaint", "mnuColorPicker", "mnuDesktop", "mnuTemporary", "mnuStartupAllUser", "mnuStartupPersonal", "mnuJasonQueryLocated", "mnuAsciiTable", "mnuBlobViewer", "mnuFileSplitter"
                };

                //20250413 提示訊息
                pnlArrow.Visible = true;
                picArrow.Visible = true;
                lblPrompt4NewConnection.Visible = true;

                //20250413
                tabControl1.Visible = false;
            }
            else
            {
                //20250413 提示訊息
                pnlArrow.Visible = false;

                //20250413
                tabControl1.Visible = true;

                using (TraceLogger.Time("Load Default Setting"))
                {
                    //載入各項預設值
                    LoadDefaultSetting();
                }

                _isMenuEnable = false; //使用者有指定連線

                //20250613 改為 HashSet
                _MenuItems = new HashSet<string>
                {
                    "mnuNewConnection", "mnuOpenConnection"
                };

                var titleTemp = MyLibrary.ShowDatabaseName ? DatabaseSqlExecutor.DbConnectionTitle : DatabaseSqlExecutor.DbConnectionTitle.Replace($"({DatabaseSqlExecutor.DataSourceDisplayName}/MariaDB) ", string.Empty).Replace($"({DatabaseSqlExecutor.DataSourceDisplayName}) ", string.Empty);
                var localVersion = !MyLibrary.ShowVersion ? string.Empty : $" {AppConfigHelper.LocalVersion}";

                Text = $"{titleTemp} - {Tag}{localVersion}";

                EnableCloseTabMenu(true);

                _toolTip1.ForeColor = Color.Blue;
                _toolTip1.BackColor = Color.Gray;
                _toolTip1.UseAnimation = true;
                _toolTip1.AutoPopDelay = 3600;
                _toolTip1.InitialDelay = 1000;
                _toolTip1.ReshowDelay = 500;

                temp = string.Empty;

                var sql = string.Empty;
                var sbSql = new StringBuilder();
                var rawDbVersion = string.Empty;
                DataTable dtTemp;

                DatabaseSqlExecutor.ClearDbServerVersion();

                //20220102 針對每個資料庫，視需要取得版號
                //20220309 取得每個資料庫的版號，並顯示在主畫面的狀態列上
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            SqlTraceHelper.AppendHeader(sbSql, "---Get all Database Name");

                            sbSql.Append("SELECT * FROM Product_Component_Version");

                            sql = sbSql.ToString();
                            dtTemp = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql);

                            if (dtTemp?.Rows.Count > 0)
                            {
                                var version = dtTemp.Rows[0].GetSafeString("Version");

                                DatabaseSqlExecutor.DbServerVersion = version;
                                rawDbVersion = version;
                            }

                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            //20220805 取得所有 database name
                            SqlTraceHelper.AppendHeader(sbSql, "---Get all Database Name");

                            sbSql.AppendLine("SELECT datname FROM pg_database WHERE datname <> 'template0'");

                            sql = sbSql.ToString();
                            _dtDatabase = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql);

                            sbSql.Clear();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Database Version");

                            //server_version 供狀態列顯示；server_version_num 供程式進行版本能力判斷。
                            sbSql.AppendLine("SELECT current_setting('server_version') AS \"Server_Version\",");
                            sbSql.Append("       current_setting('server_version_num') AS \"Server_Version_Num\";");

                            sql = sbSql.ToString();
                            dtTemp = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql);

                            if (dtTemp?.Rows.Count > 0)
                            {
                                var row = dtTemp.Rows[0];

                                rawDbVersion = row.GetSafeString("Server_Version");

                                DatabaseSqlExecutor.DbServerVersion = ResolvePostgreSqlCompatibilityVersion
                                (
                                    row.GetSafeString("Server_Version_Num"),
                                    rawDbVersion
                                );
                            }

                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            //20220808 取得所有 Database Name
                            SqlTraceHelper.AppendHeader(sbSql, "---Get all Database Name");

                            sbSql.AppendLine("SELECT Name FROM master.sys.databases ORDER BY Name;");

                            sql = sbSql.ToString();

                            //20220810 此處列出全部的 DB，因為使用者可能需要切換至系統資料庫。
                            _dtDatabase = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql);

                            sbSql.Clear();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Database Version");

                            //20210623 取得 SQL Server 版號
                            sbSql.Append
                            (
                                "SELECT CONVERT(nvarchar(128), " +
                                "SERVERPROPERTY('ProductVersion')) AS ProductVersion"
                            );

                            sql = sbSql.ToString();
                            dtTemp = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql);

                            if (dtTemp?.Rows.Count > 0)
                            {
                                var productVersion = dtTemp.Rows[0].GetSafeString("ProductVersion");

                                DatabaseSqlExecutor.SetSqlServerVersion(productVersion);

                                rawDbVersion = DatabaseSqlExecutor.SqlServerVersion.ProductVersion;
                            }

                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            //20220808 取得所有 Database Name
                            SqlTraceHelper.AppendHeader(sbSql, "---Get all Database Name");

                            sbSql.AppendLine("SELECT Schema_Name AS Name");
                            sbSql.AppendLine("  FROM Information_Schema.Schemata");
                            sbSql.Append(" ORDER BY Schema_Name");

                            sql = sbSql.ToString();
                            _dtDatabase = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql);

                            sbSql.Clear();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Database Version");

                            sbSql.Append("SELECT Version() AS Version;");

                            sql = sbSql.ToString();
                            dtTemp = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql);

                            if (dtTemp?.Rows.Count > 0)
                            {
                                rawDbVersion = dtTemp.Rows[0].GetSafeString("Version");
                            }

                            break;
                        }
                    case DataSourceType.None:
                        {
                            break;
                        }
                }

                DatabaseSqlExecutor.SetDatabaseServerVersion(_currentSourceType, rawDbVersion);

                spDBVersion.Visible = !string.IsNullOrWhiteSpace(DatabaseSqlExecutor.DatabaseVersionDisplayText);
                lblDBVersion.Text = DatabaseSqlExecutor.DatabaseVersionDisplayText;

                //20201125 MainForm 在此處中斷連線 (少佔用一個連線數，也省去中斷連線時釋放的問題)
                DisconnectDatabase(true);

                var isSpecifiedSqlFile = false; //是否有開啟選項裡面指定的 SQL 檔案

                if (!string.IsNullOrEmpty(MyGlobal.SpecifiedSqlFile1) && File.Exists(MyGlobal.SpecifiedSqlFile1)) //20240301 檢查指定檔案是否存在
                {
                    isSpecifiedSqlFile = true;
                    temp = CheckTabNameExist(); //取得下一個空白編號
                    CreateNewTab("Query", temp, MyGlobal.SpecifiedSqlFile1);
                }

                if (!string.IsNullOrEmpty(MyGlobal.SpecifiedSqlFile2) && !CheckTabNameExist(MyGlobal.SpecifiedSqlFile2) && File.Exists(MyGlobal.SpecifiedSqlFile2)) //20240301 檢查指定檔案是否存在
                {
                    isSpecifiedSqlFile = true;
                    temp = CheckTabNameExist(); //取得下一個空白編號
                    CreateNewTab("Query", temp, MyGlobal.SpecifiedSqlFile2);
                }

                //20250320 統計開啟了幾個備份檔案
                var fileOpenedCount = 0;

                //20240302 檢查是否有暫存檔案需要開啟(還原)
                //20250830 判斷是否有開啟檔案備案功能
                if (AppConfigHelper.IsBackupFile)
                {
                    var temp2 = $"{MyGlobal.DomainUser.Replace("\\", "@")}{MyGlobal.Separator00A1}{JasonQueryRepository.DbMotherPid}{MyGlobal.Separator00A1}";
                    var likeContent = temp2.Replace("'", "''");

                    sbSql.Clear();
                    sbSql.AppendLine("SELECT * FROM SystemConfig");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
                    sbSql.AppendLine($"   AND AttributeValue LIKE '{likeContent}%'");
                    sbSql.Append(" ORDER BY AttributeValue");

                    sql = sbSql.ToString();

                    var dtBackup = JasonQueryRepository.ExecQuery(sql);

                    if (dtBackup?.Rows.Count > 0)
                    {
                        var result = true;

                        if (AppConfigHelper.AskBeforeOpenUnsavedFiles)
                        {
                            var message = LocalizationHelper.GetLanguageString("Do you want to open \"All unsaved files\" ?\r\nThere are {QTY} file(s) in total.", "form", GetType().Name, "msg", "AskMeBeforeOpenUnsavedFiles", "Text").Replace("{QTY}", dtBackup.Rows.Count.ToString());

                            if (MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            {
                                result = false;
                            }
                        }

                        if (result)
                        {
                            foreach (DataRow dr in dtBackup?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                            {
                                var tabTitle = dr.GetSafeString("AttributeText"); //TabTile(主畫面的頁籤要顯示的文字，若已存檔是檔名；若未存檔則是 *SQL Editor 1 之類的
                                var attributeValue = dr.GetSafeString("AttributeValue");
                                var fileNameBackup = $"{AppConfigHelper.BackupPath}{attributeValue}"; //暫存檔的完整路徑+檔案名稱
                                var fileNameSaved = dr.GetSafeString("AttributeText2"); //使用者實際存檔的完整路徑+檔案名稱(此變數若為空值，表示使用者當初並未儲檔)
                                var backupCurrentPosition = dr.GetSafeString("AttributeName"); //暫存檔內容的當時游標所在位置

                                int.TryParse(backupCurrentPosition, out var backupCurrentPosition2); //如果是空值，轉換為數字 0

                                if (File.Exists(fileNameBackup))
                                {
                                    CreateNewTab("Query3", tabTitle, fileNameBackup, fileNameSaved, backupCurrentPosition2.ToString()); //開啟備份的暫存檔案，但實際要指向已存檔的完整檔案(若當初未存檔，則是空值)

                                    isSpecifiedSqlFile = true;
                                    fileOpenedCount++;
                                }
                                else
                                {
                                    //暫存檔案已不存在，將此筆 Record 刪除，避免下次又重複開啟此暫存檔案
                                    MyGlobal.DeleteBackupFileInfo(fileNameBackup);
                                }
                            }
                        }
                    }
                }

                if (!isSpecifiedSqlFile)
                {
                    var tab = CheckTabNameExist();

                    if (!CheckTabNameExist(tab))
                    {
                        CreateNewTab("Query", tab); //連線後，開啟第一個空白頁籤
                    }
                }
            }

            ApplyLocalization();

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        LoadPostgreSqlDatabase();
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        LoadSqlServerDatabase();
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        LoadMySqlDatabase();
                        break;
                    }
                case DataSourceType.None:
                    {
                        break;
                    }
            }

            Application.UseWaitCursor = false;
        }

        private static string ResolvePostgreSqlCompatibilityVersion(string serverVersionNumber, string rawVersion)
        {
            const int postgreSql11VersionNumber = 110000;

            if
            (
                int.TryParse
                (
                    serverVersionNumber,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var parsedVersionNumber
                )
            )
            {
                return parsedVersionNumber >= postgreSql11VersionNumber ? ">=11" : "<=10";
            }

            //防禦性 fallback：若 server_version_num 無法取得，改由一般版本資訊判斷。
            var versionInfo = DatabaseServerVersionInfo.Create(DataSourceType.PostgreSql, rawVersion);

            return versionInfo.MajorVersion >= 11 ? ">=11" : "<=10";
        }

        private string ConnectToDatabase()
        {
            var result = string.Empty;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        result = MyGlobal.OracleReader.ConnectTo();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        result = MyGlobal.PostgreSqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        result = MyGlobal.SqlServerReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        result = MyGlobal.MySqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);
                        break;
                    }
                case DataSourceType.None:
                    {
                        break;
                    }
            }

            return result;
        }

        private void LoadDefaultSetting()
        {
            Cursor = Cursors.WaitCursor;

            //查出所有指定的 AttributeKey
            var keys = new string[]
            {
                "GeneralConfig", "EditorConfig", "AutoCompleteConfig", "AutoReplaceConfig", "GridConfig",
                "SQLFormatterConfig", "SQL2CodeConfig", "GenerateSQLConfig", "KeywordsConfig"
            };

            var selectCondition = $"'{string.Join("', '", keys)}'";
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine($"   AND AttributeKey IN ({selectCondition})");
            sbSql.Append("   AND AttributeName <> 'MainFormIconStyle'"); //20241207 此處要排除 AttributeName = 'MainFormIconStyle'，否則導入新的連線設定時，不會自動建立關鍵字等相關設定值 (但後續要再單獨撈取此設定)

            var sql = sbSql.ToString();
            var dtData = JasonQueryRepository.ExecQuery(sql);

            #region 檢查是否有此連線的一般設定值，若沒有，則新增此連線一般設定值的預設值
            if (dtData == null || dtData.Rows.Count == 0)
            {
                //新增預設關鍵字
                AddBuiltInFunctionsKeywords();
                AddBuiltInKeywords();

                if (MyLibrary.IsDarkMode)
                {
                    #region for DarkMode Color
                    JasonQueryRepository.UpdateSetting("GridConfig", "HeadingForeColor", "#000000");
                    JasonQueryRepository.UpdateSetting("GridConfig", "EvenRowForeColor", "#000000");
                    JasonQueryRepository.UpdateSetting("GridConfig", "EvenRowBackColor", "#FFFFFF");
                    JasonQueryRepository.UpdateSetting("GridConfig", "OddRowForeColor", "#000000");
                    JasonQueryRepository.UpdateSetting("GridConfig", "OddRowBackColor", "#FFFFC1");
                    JasonQueryRepository.UpdateSetting("GridConfig", "NullShowColor", "#FFFF00");

                    JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabActiveForeColor", "#000000");
                    JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabActiveBackColor", "#E6FFFF");
                    JasonQueryRepository.UpdateSetting("GlobalConfig", "OptionsTabInactiveForeColor", "#A5A5A5"); //20210120 深色模式，這裡調亮一點

                    JasonQueryRepository.UpdateSetting("EditorConfig", "ToolstripBackground", "#E3FDCA");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "EditorBackground", "#FFFFFF");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "CurrentLineBackground", "#FFFFE0");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "SelectedTextBackground", "#ADD8E6");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "Comments", "#008000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "TextIdentifier", "#000000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "Number", "#800000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "OperatorSymbol", "#800000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "OperatorKeywords", "#366092");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "String", "#FF0000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "Character", "#FF0000");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "BuiltinFunctions", "#FF00FF");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "BuiltInKeywords", "#0000FF");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "UserDefinedKeywords", "#0000FF");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "WhiteSpace", "#00FFFF");
                    JasonQueryRepository.UpdateSetting("EditorConfig", "HighlightForeColor", "#000000");
                    #endregion
                }

                //新增 OperatorKeyword
                AddOperatorKeywords();

                //AutoReplace 範例
                #region AutoReplace 範例
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'scf{MyGlobal.Separator3s}select count(*) from')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'sf{MyGlobal.Separator3s}select * from')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'sfe{MyGlobal.Separator3s}select * from employee')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.AppendLine($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'sfew{MyGlobal.Separator3s}select * from employee");
                sbSql.Append("where name like ''J^%''')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                switch (_currentSourceType)
                {
                    case DataSourceType.PostgreSql:
                    case DataSourceType.MySql:
                        {
                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'l1{MyGlobal.Separator3s}limit 1')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'stf{MyGlobal.Separator3s}select top 100 * from')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);
                            break;
                        }
                    default:
                        {
                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'r1{MyGlobal.Separator3s}rownum<=1')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);

                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'AutoReplaceConfig', 'AutoReplace', 'w1{MyGlobal.Separator3s}where rownum<=1')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);
                            break;
                        }
                }
                #endregion

                //Generate SQL 預設值
                #region Generate SQL 預設值
                switch (_currentSourceType)
                {
                    case DataSourceType.PostgreSql:
                    case DataSourceType.MySql:
                        {
                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'GenerateSQLConfig', 'ConvertCase', 'UpperKeywords')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);
                            break;
                        }
                    default: //Oracle
                        {
                            sbSql.Clear();
                            sbSql.AppendLine("INSERT INTO SystemConfig");
                            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'GenerateSQLConfig', 'ConvertCase', 'UpperAll')");

                            sql = sbSql.ToString();
                            JasonQueryRepository.ExecNonQuery(sql);
                            break;
                        }
                }

                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'GenerateSQLConfig', 'Numbers', '5')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                //20230915 初始值改為「將架構瀏覽器設為預設頁籤」，避免初次使用的使用者找不到在哪設定！
                JasonQueryRepository.UpdateSetting("EditorConfig", "DefaultTabSchemaBrowser", "1");
                #endregion

                //重新再 Select 一次
                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"  AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine($"  AND AttributeKey IN ({selectCondition})");
                sbSql.Append("  AND AttributeName <> 'MainFormIconStyle'");

                sql = sbSql.ToString();
                dtData = JasonQueryRepository.ExecQuery(sql);
            }
            #endregion

            //建立雙層字典：Outer=AttributeKey, Inner=AttributeName→AttributeValue
            var attributeValueMap = (dtData.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                .GroupBy
                 (
                     r => r.GetSafeString("AttributeKey"), StringComparer.OrdinalIgnoreCase
                 )
                .ToDictionary
                 (
                     g => g.Key,
                     g => g.GroupBy
                            (
                                r => r.GetSafeString("AttributeName"), StringComparer.OrdinalIgnoreCase
                            )
                           .ToDictionary
                            (
                                gg => gg.Key,
                                gg => gg.First().GetSafeString("AttributeValue"), StringComparer.OrdinalIgnoreCase
                            ), StringComparer.OrdinalIgnoreCase
                 );

            //建立雙層字典：Outer=AttributeKey, Inner=AttributeName→AttributeText
            var attributeTextMap = (dtData.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                .GroupBy
                 (
                     r => r.GetSafeString("AttributeKey"), StringComparer.OrdinalIgnoreCase
                 )
                .ToDictionary
                 (
                     g => g.Key,
                     g => g.GroupBy
                            (
                                r => r.GetSafeString("AttributeName"), StringComparer.OrdinalIgnoreCase
                            )
                           .ToDictionary
                            (
                                gg => gg.Key,
                                gg => gg.First().GetSafeString("AttributeText"), StringComparer.OrdinalIgnoreCase
                            ), StringComparer.OrdinalIgnoreCase
                 );

            //取值 - 字串 AttributeValue
            string GetStringValue(string section, string name, string defaultValue = "")
                   => attributeValueMap.TryGetValue(section, out var sec) && sec.TryGetValue(name, out var value) ? value : defaultValue;

            //取值 - 字串 AttributeText
            string GetStringText(string section, string name, string defaultValue = "")
                   => attributeTextMap.TryGetValue(section, out var sec) && sec.TryGetValue(name, out var value) ? value : defaultValue;

            //取值 - 布林值
            bool GetBool(string section, string name, bool defaultValue = false)
            {
                if (attributeValueMap.TryGetValue(section, out var sec) && sec.ContainsKey(name))
                {
                    return sec[name] == "1";
                }

                return defaultValue;
            }

            //取值 - 數值
            int GetInt(string section, string name, int defaultValue = 0)
                => int.TryParse(GetStringValue(section, name, defaultValue.ToString()), out var i) ? i : defaultValue;

            //載入設定值
            LoadGeneralConfig();
            LoadQueryEditorConfig();
            LoadAutoCompleteConfig();
            LoadAutoReplaceConfig();
            LoadGridConfig();
            LoadKeywordsConfig();
            LoadSQL2CodeConfig();
            LoadSQLFormatterMetaConfig();
            LoadGenerateSQLConfig();

            //處理 MainFormIconStyle
            LoadMainFormIconStyle();

            //載入最近使用清單與我的最愛
            LoadRecentList();
            LoadMyFavoriteFiles();

            Cursor = Cursors.Default;

            void LoadGeneralConfig()
            {
                MyLibrary.IsDarkMode = GetBool("GeneralConfig", "DarkMode", false);

                if (MyLibrary.IsDarkMode)
                {
                    C1.Win.C1Themes.C1ThemeController.ApplicationTheme = "VS2013Dark";
                    c1ThemeController1.SetTheme(tabControl1, "VS2013Dark");
                    c1ThemeController1.SetTheme(mnuMainForm, "ExpressionLight");
                    c1ThemeController1.SetTheme(c1StatusBar1, "ExpressionLight");
                }

                MyGlobal.SpecifiedSqlFile1 = GetStringValue("GeneralConfig", "SpecifiedSQLFile1");
                MyGlobal.SpecifiedSqlFile2 = GetStringValue("GeneralConfig", "SpecifiedSQLFile2");
            }

            void LoadQueryEditorConfig()
            {
                MyLibrary.ColorToolstripBackground = GetStringValue("EditorConfig", "ToolstripBackground");
                MyLibrary.ColorEditorBackground = GetStringValue("EditorConfig", "EditorBackground");
                MyLibrary.ColorCurrentLineBackground = GetStringValue("EditorConfig", "CurrentLineBackground");
                MyLibrary.ColorSelectedTextBackground = GetStringValue("EditorConfig", "SelectedTextBackground");
                MyLibrary.ColorErrorLineBackground = GetStringValue("EditorConfig", "ErrorLineBackground");
                MyLibrary.ColorBookmarkBackground = GetStringValue("EditorConfig", "BookmarkBackground");
                MyLibrary.BookmarkStyle = GetStringValue("EditorConfig", "BookmarkStyle");
                MyGlobal.BookmarkStyle = TextHelper.GetValueFromDictionary(MyGlobal.dicBookmarkStyle, MyLibrary.BookmarkStyle);
                MyLibrary.ColorComments = GetStringValue("EditorConfig", "Comments");
                MyLibrary.ColorTextIdentifier = GetStringValue("EditorConfig", "TextIdentifier");
                MyLibrary.ColorBuiltInKeywords = GetStringValue("EditorConfig", "BuiltInKeywords");
                MyLibrary.ColorUserDefinedKeywords = GetStringValue("EditorConfig", "UserDefinedKeywords");
                MyLibrary.ColorNumber = GetStringValue("EditorConfig", "Number");
                MyLibrary.ColorOperatorSymbol = GetStringValue("EditorConfig", "OperatorSymbol");
                MyLibrary.ColorOperatorKeywords = GetStringValue("EditorConfig", "OperatorKeywords");
                MyLibrary.ColorString = GetStringValue("EditorConfig", "String");
                MyLibrary.ColorCharacter = GetStringValue("EditorConfig", "Character");
                MyLibrary.ColorBuiltInFunctions = GetStringValue("EditorConfig", "BuiltinFunctions");
                MyLibrary.ColorWhiteSpace = GetStringValue("EditorConfig", "WhiteSpace");
                MyLibrary.ColorUserDefinedTablesViews = GetStringValue("EditorConfig", "UserDefinedTables");
                MyLibrary.ColorUserDefinedFunctionsTriggers = GetStringValue("EditorConfig", "UserDefinedFunctions");

                //Query Editor 頁籤：載入 Highlight 設定
                MyLibrary.HighlightColorForeColor = GetStringValue("EditorConfig", "HighlightForeColor");
                MyLibrary.HighlightColorStyle = GetStringValue("EditorConfig", "HighlightStyle");
                MyLibrary.HighlightColorOutlineAlpha = GetStringValue("EditorConfig", "HighlightOutlineAlpha");
                MyLibrary.HighlightColorAlpha = GetStringValue("EditorConfig", "HighlightAlpha");

                //Query Editor 頁籤：載入 Preferences 設定
                MyLibrary.QueryEditorFontName = GetStringValue("EditorConfig", "EditorFontName");
                MyLibrary.SetQueryEditorFontSizeFromText(GetStringValue("EditorConfig", "EditorFontSize")); //20260531 修改取值方法
                MyLibrary.SetQueryEditorZoomFromText(GetStringValue("EditorConfig", "EditorZoom")); //20260531 修改取值方法
                MyLibrary.WordWrap = GetBool("EditorConfig", "WordWrap", false);
                MyLibrary.WordWrapVisualFlags_Start = GetBool("EditorConfig", "WordWrapVisualFlags_Start", false);
                MyLibrary.WordWrapVisualFlags_End = GetBool("EditorConfig", "WordWrapVisualFlags_End", true);
                MyLibrary.WordWrapVisualFlags_Margin = GetBool("EditorConfig", "WordWrapVisualFlags_Margin", false);
                MyLibrary.WordWrapIndentMode = GetStringValue("EditorConfig", "WordWrapIndentMode");
                MyGlobal.WordWrapIndentMode = TextHelper.GetValueFromDictionary(MyGlobal.dicWordWrapIndentMode, MyLibrary.WordWrapIndentMode);
                MyLibrary.KeywordFontBold = GetBool("EditorConfig", "KeywordFontBold", false);
                MyLibrary.CopyAsHTML = GetBool("EditorConfig", "CopyAsHTML", true);
                MyLibrary.ShowAllCharacters = GetBool("EditorConfig", "ShowAllCharacters", false);
                MyLibrary.ShowSaveAsButton = true; //20260504 改為固定值 true (省去計算 QueryForm 的 txtIndentWord 的位置)
                MyLibrary.ShowIndentGuide = GetBool("EditorConfig", "ShowIndentGuide", false);
                MyGlobal.TabWidth = GetInt("EditorConfig", "TabWidth", 4);
                //MyLibrary.EntireBlankRowAsEmptyRow = GetBool("EditorConfig", "EntireBlankRowAsEmptyRow4SelectBlock", false);
                MyLibrary.EntireBlankRowAsEmptyRow = true; //20250211 改為固定值 true (不勾選有 bug，暫時不花時間修復)
                MyLibrary.HighlightSelection = GetBool("EditorConfig", "HighlightSelection", true);
                MyGlobal.IsDefaultTabSchemaBrowser = GetBool("EditorConfig", "DefaultTabSchemaBrowser", true);
                MyGlobal.IsSortByColumnName = GetBool("EditorConfig", "SortByColumnName", false);
                MyGlobal.IsAutoListMembers = GetBool("EditorConfig", "AutoListMembers", true);
                MyGlobal.ShouldSavePoint = IsPostgreSql && GetBool("EditorConfig", "SavePoint", true);
                AppConfigHelper.IsAfterPasteFocusOnQueryEditor = GetBool("EditorConfig", "AfterPasteFocusOnQueryEditor", true);
            }

            void LoadAutoCompleteConfig()
            {
                MyLibrary.EnableAutoComplete = GetBool("AutoCompleteConfig", "EnableAutoComplete2", true);
                MyLibrary.AutoCompleteMinFragmentLength = GetInt("AutoCompleteConfig", "MinFragmentLength2", 2);
                MyLibrary.AutoCompleteFirstCharChecking = GetBool("AutoCompleteConfig", "FirstCharChecking2", true);
                MyLibrary.AutoCompleteBuiltInKeywords = GetBool("AutoCompleteConfig", "BuiltInKeywords2", true); //20201231 Built-In Keywords & Functions, 共用同一個變數
                MyLibrary.AutoCompleteBuiltInFunctions = GetBool("AutoCompleteConfig", "BuiltInFunctions2", true);
                MyLibrary.AutoCompleteUserDefinedKeywords = GetBool("AutoCompleteConfig", "UserDefinedKeywords2", true);
                MyLibrary.AutoCompleteUserDefinedFunctions = GetBool("AutoCompleteConfig", "UserDefinedFunctions2", true);
                MyLibrary.AutoCompleteUserDefinedTables = GetBool("AutoCompleteConfig", "UserDefinedTables2", true);
                MyLibrary.AutoCompleteUserDefinedTriggers = GetBool("AutoCompleteConfig", "UserDefinedTriggers2", true);
                MyLibrary.AutoCompleteUserDefinedViews = GetBool("AutoCompleteConfig", "UserDefinedViews2", true);
            }

            void LoadAutoReplaceConfig()
            {
                //載入 Auto Replace 設定值 (此處只要載入「是否啟用」即可，其他的在 QueryForm 確認／載入)
                MyLibrary.EnableAutoReplace = GetBool("AutoReplaceConfig", "EnableAutoReplace", true);
            }

            void LoadGridConfig()
            {
                //載入 Grid 設定值
                MyLibrary.GridQuotationMarks = GetStringValue("GridConfig", "QuotingWith");
                MyLibrary.GridFieldSeparator = GetStringValue("GridConfig", "FieldSeparator");
                MyLibrary.GridShowColumnDataType = GetBool("GridConfig", "ShowColumnDataType", false);
                MyLibrary.GridShowFilterRow = GetBool("GridConfig", "ShowFilterRow", false);
                MyLibrary.GridShowGroupingRow = GetBool("GridConfig", "ShowGroupingRow", false);
                MyLibrary.GridResize = GetBool("GridConfig", "Resize", true); //20230915 預設值變更為「自動調整欄寬」

                #region 取得欄位寬度設定值
                var maxWidth = GetStringValue("GridConfig", "MaxWidth", "500");

                int.TryParse(maxWidth, out GridHelper.MaxWidth);

                if (GridHelper.MaxWidth == 0)
                {
                    GridHelper.MaxWidth = 500;
                }
                #endregion

                MyGlobal.IsPreviewCLOBData = false; //20260115 預設值變更為 false (避免內容太多，顯示時引發例外錯誤) GetBool("GridConfig", "PreviewCLOBData", false);
                MyLibrary.GridShowColumnComment = GetBool("GridConfig", "ShowColumnComment", false);
                MyLibrary.GridRawDataMode = GetBool("GridConfig", "RawDataMode", false);
                MyLibrary.GridNullShowAs = string.IsNullOrEmpty(MyLibrary.GridNullShowAs) ? GetStringValue("GridConfig", "NullShowAs") : MyLibrary.GridNullShowAs; //20260505 改為只在啟動時取得，之後從選項中變更則不再更新
                MyLibrary.GridNullShowColor = GetStringValue("GridConfig", "NullShowColor");
                MyLibrary.GridPagingQuery = GetBool("GridConfig", "PagingQuery", true); //預設啟用「分頁查詢」功能
                MyLibrary.GridRowsPerPage = GetStringValue("GridConfig", "RowsPerPage", "500");
                MyLibrary.GridAppendingQueries = GetBool("GridConfig", "AppendingQueries", true); //預設啟用「附加查詢」功能
                MyLibrary.GridSetFocusAfterQuery = GetBool("GridConfig", "SetFocusAfterQuery", false);
                MyLibrary.GridVisualStyle = GetStringValue("GridConfig", "VisualStyle");
                MyLibrary.SetGridZoomFromText(GetStringValue("GridConfig", "Zoom")); //20260531 修改取值方法
                MyLibrary.GridFontName = GetStringValue("GridConfig", "FontName");
                MyLibrary.SetGridFontSizeFromText(GetStringValue("GridConfig", "FontSize")); //20260531 修改取值方法
                MyLibrary.GridSheetName = GetStringValue("GridConfig", "SheetName");
                MyLibrary.GridHeadingForeColor = GetStringValue("GridConfig", "HeadingForeColor");
                MyLibrary.GridEvenRowForeColor = GetStringValue("GridConfig", "EvenRowForeColor");
                MyLibrary.GridEvenRowBackColor = GetStringValue("GridConfig", "EvenRowBackColor");
                MyLibrary.GridOddRowForeColor = GetStringValue("GridConfig", "OddRowForeColor");
                MyLibrary.GridOddRowBackColor = GetStringValue("GridConfig", "OddRowBackColor");
                MyLibrary.GridHighlightForeColor = GetStringValue("GridConfig", "HighlightForeColor");
                MyLibrary.GridHighlightBackColor = GetStringValue("GridConfig", "HighlightBackColor");
                MyLibrary.GridSelectedForeColor = GetStringValue("GridConfig", "SelectedForeColor");
                MyLibrary.GridSelectedBackColor = GetStringValue("GridConfig", "SelectedBackColor");
                MyLibrary.RowSizing = GetStringValue("GridConfig", "RowResizing");
                MyGlobal.RowSize = TextHelper.GetValueFromDictionary(MyGlobal.dicRowSizing, MyLibrary.RowSizing);
                MyLibrary.GridInteractionDirection = GetStringValue("GridConfig", "Direction");
                GridHelper.Direction = TextHelper.GetValueFromDictionary(MyGlobal.dicDirection, MyLibrary.GridInteractionDirection);
                MyLibrary.GridExcelFileName = GetStringValue("GridConfig", "ExcelFilename");
                MyLibrary.GridExcelSaveAsType = GetStringValue("GridConfig", "ExcelSaveAsType", "Excel 2007 (*.xlsx)");
                MyLibrary.GridExcelWorksheetName = GetStringValue("GridConfig", "ExcelWorksheetName", "data");
                MyLibrary.GridCSVDelimiters = GetStringValue("GridConfig", "CSVDelimiters", "Comma");
                MyGlobal.CsvDelimiters = TextHelper.GetValueFromDictionary(MyGlobal.dicCsvDelimiters, MyLibrary.GridCSVDelimiters);
                MyLibrary.GridEncoding = GetStringValue("GridConfig", "Encoding", "UTF-8");
                MyLibrary.GridExcelAutoOpen = GetBool("GridConfig", "ExcelAutoOpen", true);
                MyLibrary.GridExcelAutoColumnResize = GetBool("GridConfig", "ExcelAutoColumnResize", true);
                MyLibrary.GridExcelCustom = GetBool("GridConfig", "ExcelCustom", false);
                MyLibrary.GridExcelHeadingBackColor = GetStringValue("GridConfig", "ExcelHeadingBackColor", "LightSkyBlue");
                MyLibrary.GridExcelEvenRowBackColor = GetStringValue("GridConfig", "ExcelEvenRowBackColor", "White");
                MyLibrary.GridExcelOddRowBackColor = GetStringValue("GridConfig", "ExcelOddRowBackColor", "LightYellow");
                MyLibrary.GridExcelFontName = GetStringValue("GridConfig", "ExcelFontName", "Consolas");
                MyLibrary.GridExcelFontSize = GetStringValue("GridConfig", "ExcelFontSize", "12");
                MyLibrary.GridExcelRowHeight = GetStringValue("GridConfig", "ExcelRowHeight", "20");
            }

            void LoadKeywordsConfig()
            {
                //載入 Keywords 設定值
                MyLibrary.KeywordsOperatorKeywords = GetStringText("KeywordsConfig", "OperatorKeywords");
                MyLibrary.KeywordsUserDefinedKeywords = GetStringText("KeywordsConfig", "UserDefinedKeywords");
                MyLibrary.KeywordsBuiltInFunctions = GetStringText("KeywordsConfig", "BuiltInFunctions");
                MyLibrary.KeywordsBuiltInKeywords = GetStringText("KeywordsConfig", "BuiltInKeywords");

                //20250525 因 bug 因素，導致關鍵字可能被清空，此處再次檢查並視結果新增預設關鍵字
                if (string.IsNullOrEmpty(MyLibrary.KeywordsOperatorKeywords))
                {
                    AddOperatorKeywords(); //LoadDefaultSetting, 因空值而主動添加
                }

                if (string.IsNullOrEmpty(MyLibrary.KeywordsBuiltInFunctions))
                {
                    AddBuiltInFunctionsKeywords(); //LoadDefaultSetting, 因空值而主動添加
                }

                if (string.IsNullOrEmpty(MyLibrary.KeywordsBuiltInKeywords))
                {
                    AddBuiltInKeywords(); //LoadDefaultSetting, 因空值而主動添加
                }
            }

            void LoadSQL2CodeConfig()
            {
                //載入 SQL to Code 設定值
                MyLibrary.SqlToCodeSqlVariableName = GetStringValue("SQL2CodeConfig", "SqlVariableName");

                if (string.IsNullOrEmpty(MyLibrary.SqlToCodeSqlVariableName))
                {
                    MyLibrary.SqlToCodeSqlVariableName = GetStringValue("SQL2CodeConfig", "VariableName");

                    if (string.IsNullOrEmpty(MyLibrary.SqlToCodeSqlVariableName))
                    {
                        JasonQueryRepository.UpdateSetting("SQL2CodeConfig", "SqlVariableName", "sql");
                    }
                    else
                    {
                        JasonQueryRepository.UpdateSetting("SQL2CodeConfig", "SqlVariableName", MyLibrary.SqlToCodeSqlVariableName);
                    }
                }

                MyLibrary.SqlToCodeStringBuilderVariableName = GetStringValue("SQL2CodeConfig", "StringBuilderVariableName");
            }

            void LoadSQLFormatterMetaConfig()
            {
                //載入 SQL Formatter 設定值
                MyLibrary.SqlFormatterIndentSize = GetInt("SQLFormatterConfig", "IndentSize", 4);
                MyLibrary.SqlFormatterMaxLineWidth = GetInt("SQLFormatterConfig", "MaxLineWidth", 999);
                MyLibrary.SqlFormatterBlankLinesBetweenStatements = GetInt("SQLFormatterConfig", "BlankLinesBetweenStatements", 1);
                MyLibrary.SqlFormatterListItemsPerLine = GetInt(SqlFormatterSettingsContract.SectionName, SqlFormatterSettingsContract.ListItemsPerLineSettingName, SqlFormatOptions.DefaultListItemsPerLine);
                MyLibrary.SqlFormatterConvertCaseForKeywords = GetBool("SQLFormatterConfig", "ConvertCaseForKeywords", true);
                MyLibrary.SqlFormatterConvertCaseForKeywordsCase = GetInt("SQLFormatterConfig", "ConvertCaseForKeywordsCase", 1);

                var providerKind = DataSourceTypeMapper.ToDatabaseProviderKind(_currentSourceType);

                MyLibrary.SqlFormatterEngine = SqlFormatterEnginePreferenceResolver.Parse
                (
                    providerKind,
                    GetStringValue("SQLFormatterConfig", "EngineKind", SqlFormatterEngineKind.Unknown.ToString())
                );
            }

            void LoadGenerateSQLConfig()
            {
                //載入 Generate SQL 預設值
                MyLibrary.GenerateSqlConvertCase = GetStringValue("GenerateSQLConfig", "ConvertCase");
                MyLibrary.GenerateSqlNumbers = GetInt("GenerateSQLConfig", "Numbers", 5);
            }

            void LoadMainFormIconStyle()
            {
                //20241207 最後要再單獨撈取 Icon 的設定值
                sbSql.Clear();
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'GeneralConfig'");
                sbSql.Append("   AND AttributeName = 'MainFormIconStyle'");

                sql = sbSql.ToString();

                var dtTemp = JasonQueryRepository.ExecQuery(sql);

                int.TryParse(dtTemp?.Rows.Count > 0 ? dtTemp.Rows[0]["AttributeValue"].ToString() : "0", out AppConfigHelper.MainFormIconStyle); //20240210 預設值為第 0 組圖示

                if (AppConfigHelper.MainFormIconStyle < 0 || AppConfigHelper.MainFormIconStyle > 9)
                {
                    AppConfigHelper.MainFormIconStyle = 0;
                }

                //20240107 根據資料庫顯示不同的圖示 (主要體現在主畫面、工作列)
                var iconMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Oracle", "Oracle" },
                        { "PostgreSQL", "PostgreSQL" },
                        { "SQL Server", "SQL Server" },
                        { "MySQL", "MySQL" },
                        { "MySQL/MariaDB", "MySQL" },
                        { "SQLite", "SQLite" },
                        { "SQLCipher", "SQLite" }
                    };

                if (iconMapping.TryGetValue(DatabaseSqlExecutor.DataSourceDisplayName, out var iconKey))
                {
                    var iconName = $"{iconKey} {AppConfigHelper.MainFormIconStyle} 16x16.ico";

                    Icon = IconManager.GetIcon(MyGlobal.IconLibrary, iconName);
                }
            }
        }

        private void AddBuiltInFunctionsKeywords()
        {
            var sql = string.Empty;
            var sbSql = new StringBuilder();
            var keywordsBuiltInFunctions = string.Empty;
            var postgreSqlBuiltInFunctionsKeywords = "abbrev abs acos age area array_agg array_append array_cat array_dims array_fill array_length array_lower array_ndims array_prepend array_to_string array_upper ascii asin atan atan2 avg bit_and bit_length bit_or bool_and bool_or box broadcast btrim cbrt ceil ceiling center char_length chr circle clock_timestamp coalesce col_description concat concat_ws convert convert_from convert_to corr cos cot count covar_pop covar_samp cume_dist current_catalog current_database current_date current_query current_schema current_schemas current_setting current_time current_timestamp current_user currval cursor_to_xml cursor_to_xmlschema database_to_xml database_to_xml_and_xmlschema database_to_xmlschema date_part date_trunc decode degrees dense_rank diameter div encode enum_first enum_last enum_range every exists exp extract family first_value floor format format_type generate_series generate_subscripts get_bit get_byte get_current_ts_config greatest has_any_column_privilege has_column_privilege has_database_privilege has_foreign_data_wrapper_privilege has_function_privilege has_language_privilege has_schema_privilege has_sequence_privilege has_server_privilege has_table_privilege has_tablespace_privilege height host hostmask inet_client_addr inet_client_port inet_server_addr inet_server_port initcap isclosed isfinite isopen justify_days justify_hours justify_interval lag last_value lastval lead least left length ln localtime localtimestamp log lower lpad lseg ltrim masklen max md5 min mod netmask network nextval now npoints nth_value ntile nullif numnode obj_description octet_length overlay path pclose percent_rank pg_advisory_lock pg_advisory_lock_shared pg_advisory_unlock pg_advisory_unlock_all pg_advisory_unlock_shared pg_advisory_xact_lock pg_advisory_xact_lock_shared pg_backend_pid pg_cancel_backend pg_client_encoding pg_collation_is_visible pg_column_size pg_conf_load_time pg_conversion_is_visible pg_create_restore_point pg_current_xlog_insert_location pg_current_xlog_location pg_database_size pg_describe_object pg_function_is_visible pg_get_constraintdef pg_get_expr pg_get_function_arguments pg_get_function_identity_arguments pg_get_function_result pg_get_functiondef pg_get_indexdef pg_get_keywords pg_get_ruledef pg_get_serial_sequence pg_get_triggerdef pg_get_userbyid pg_get_viewdef pg_has_role pg_indexes_size pg_is_in_recovery pg_is_other_temp_schema pg_is_xlog_replay_paused pg_last_xact_replay_timestamp pg_last_xlog_receive_location pg_last_xlog_replay_location pg_listening_channels pg_ls_dir pg_my_temp_schema pg_opclass_is_visible pg_operator_is_visible pg_options_to_table pg_postmaster_start_time pg_read_binary_file pg_read_file pg_relation_filenode pg_relation_filepath pg_relation_size pg_reload_conf pg_rotate_logfile pg_size_pretty pg_sleep pg_start_backup pg_stat_file pg_stop_backup pg_switch_xlog pg_table_is_visible pg_table_size pg_tablespace_databases pg_tablespace_size pg_terminate_backend pg_total_relation_size pg_try_advisory_lock pg_try_advisory_lock_shared pg_try_advisory_xact_lock pg_try_advisory_xact_lock_shared pg_ts_config_is_visible pg_ts_dict_is_visible pg_ts_parser_is_visible pg_ts_template_is_visible pg_type_is_visible pg_typeof pg_xlog_replay_pause pg_xlog_replay_resume pg_xlogfile_name pg_xlogfile_name_offset pi plainto_tsquery point polygon popen position power query_to_xml query_to_xml_and_xmlschema query_to_xmlschema querytree quote_ident quote_literal quote_nullable radians radius random rank regexp_matches regexp_replace regexp_split_to_array regexp_split_to_table regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_sxy regr_syy repeat replace reverse right round row_number rpad rtrim schema_to_xml schema_to_xml_and_xmlschema schema_to_xmlschema session_user set_bit set_byte set_config set_masklen setseed setval setweight shobj_description sign sin split_part sqrt statement_timestamp stddev stddev_pop stddev_samp string_agg string_to_array strip strpos substr substring sum table_to_xml table_to_xml_and_xmlschema table_to_xmlschema tan text timeofday to_ascii to_char to_date to_hex to_number to_timestamp to_tsquery to_tsvector transaction_timestamp translate trim trunc ts_debug ts_headline ts_lexize ts_parse ts_rank ts_rank_cd ts_rewrite ts_stat ts_token_type tsvector_update_trigger tsvector_update_trigger_column txid_current txid_current_snapshot txid_snapshot_xip txid_snapshot_xmax txid_snapshot_xmin txid_visible_in_snapshot unnest upper user var_pop var_samp variance version width width_bucket xip_list xmax xmin xml_is_well_formed xml_is_well_formed_content xml_is_well_formed_document xmlagg xmlcomment xmlconcat xmlelement xmlexists xmlforest xmlpi xmlroot xpath ";
            var keywordsMySqlBuiltInFunctions = "abbrev abs acos age area array_agg array_append array_cat array_dims array_fill array_length array_lower array_ndims array_prepend array_to_string array_upper ascii asin atan atan2 avg bit_and bit_length bit_or bool_and bool_or box broadcast btrim cbrt ceil ceiling center char_length chr circle clock_timestamp coalesce col_description concat concat_ws convert convert_from convert_to corr cos cot count covar_pop covar_samp cume_dist current_catalog current_database current_date current_query current_schema current_schemas current_setting current_time current_timestamp current_user currval cursor_to_xml cursor_to_xmlschema database_to_xml database_to_xml_and_xmlschema database_to_xmlschema date_part date_trunc decode degrees dense_rank diameter div encode enum_first enum_last enum_range every exists exp extract family first_value floor format format_type generate_series generate_subscripts get_bit get_byte get_current_ts_config greatest has_any_column_privilege has_column_privilege has_database_privilege has_foreign_data_wrapper_privilege has_function_privilege has_language_privilege has_schema_privilege has_sequence_privilege has_server_privilege has_table_privilege has_tablespace_privilege height host hostmask inet_client_addr inet_client_port inet_server_addr inet_server_port initcap isclosed isfinite isopen justify_days justify_hours justify_interval lag last_value lastval lead least left length ln localtime localtimestamp log lower lpad lseg ltrim masklen max md5 min mod netmask network nextval now npoints nth_value ntile nullif numnode obj_description octet_length overlay path pclose percent_rank pg_advisory_lock pg_advisory_lock_shared pg_advisory_unlock pg_advisory_unlock_all pg_advisory_unlock_shared pg_advisory_xact_lock pg_advisory_xact_lock_shared pg_backend_pid pg_cancel_backend pg_client_encoding pg_collation_is_visible pg_column_size pg_conf_load_time pg_conversion_is_visible pg_create_restore_point pg_current_xlog_insert_location pg_current_xlog_location pg_database_size pg_describe_object pg_function_is_visible pg_get_constraintdef pg_get_expr pg_get_function_arguments pg_get_function_identity_arguments pg_get_function_result pg_get_functiondef pg_get_indexdef pg_get_keywords pg_get_ruledef pg_get_serial_sequence pg_get_triggerdef pg_get_userbyid pg_get_viewdef pg_has_role pg_indexes_size pg_is_in_recovery pg_is_other_temp_schema pg_is_xlog_replay_paused pg_last_xact_replay_timestamp pg_last_xlog_receive_location pg_last_xlog_replay_location pg_listening_channels pg_ls_dir pg_my_temp_schema pg_opclass_is_visible pg_operator_is_visible pg_options_to_table pg_postmaster_start_time pg_read_binary_file pg_read_file pg_relation_filenode pg_relation_filepath pg_relation_size pg_reload_conf pg_rotate_logfile pg_size_pretty pg_sleep pg_start_backup pg_stat_file pg_stop_backup pg_switch_xlog pg_table_is_visible pg_table_size pg_tablespace_databases pg_tablespace_size pg_terminate_backend pg_total_relation_size pg_try_advisory_lock pg_try_advisory_lock_shared pg_try_advisory_xact_lock pg_try_advisory_xact_lock_shared pg_ts_config_is_visible pg_ts_dict_is_visible pg_ts_parser_is_visible pg_ts_template_is_visible pg_type_is_visible pg_typeof pg_xlog_replay_pause pg_xlog_replay_resume pg_xlogfile_name pg_xlogfile_name_offset pi plainto_tsquery point polygon popen position power query_to_xml query_to_xml_and_xmlschema query_to_xmlschema querytree quote_ident quote_literal quote_nullable radians radius random rank regexp_matches regexp_replace regexp_split_to_array regexp_split_to_table regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_sxy regr_syy repeat replace reverse right round row_number rpad rtrim schema_to_xml schema_to_xml_and_xmlschema schema_to_xmlschema session_user set_bit set_byte set_config set_masklen setseed setval setweight shobj_description sign sin split_part sqrt statement_timestamp stddev stddev_pop stddev_samp string_agg string_to_array strip strpos substr substring sum table_to_xml table_to_xml_and_xmlschema table_to_xmlschema tan text timeofday to_ascii to_char to_date to_hex to_number to_timestamp to_tsquery to_tsvector transaction_timestamp translate trim trunc ts_debug ts_headline ts_lexize ts_parse ts_rank ts_rank_cd ts_rewrite ts_stat ts_token_type tsvector_update_trigger tsvector_update_trigger_column txid_current txid_current_snapshot txid_snapshot_xip txid_snapshot_xmax txid_snapshot_xmin txid_visible_in_snapshot unnest upper user var_pop var_samp variance version width width_bucket xip_list xmax xmin xml_is_well_formed xml_is_well_formed_content xml_is_well_formed_document xmlagg xmlcomment xmlconcat xmlelement xmlexists xmlforest xmlpi xmlroot xpath ";
            var keywordsSqlServerBuiltInFunctions = "abs acos add_months appendchildxml ascii asciistr asin atan atan2 avg bfilename bin_to_num bitand cardinality case cast ceil chartorowid chr cluster_id cluster_probability cluster_set coalesce collect compose concat convert corr corr_k corr_s cos cosh count covar_pop covar_samp cume_dist current_date current_timestamp cv dbtimezone decode decompose deletexml dense_rank depth deref dump empty_blob empty_clob existsnode exp extract extractvalue feature_id feature_set feature_value first first_value floor from_tz greatest group_id grouping grouping_id hextoraw initcap insertchildxml insertxmlbefore instr instr2 instr4 instrb instrc iteration_number lag last last_day last_value lead least len ln lnnvl localtimestamp log lower lpad ltrim make_ref max median min mod months_between nanvl nchr new_time next_day nls_charset_decl_len nls_charset_id nls_charset_name nls_initcap nls_lower nls_upper nlssort nth_value ntile nullif numtodsinterval numtoyminterval nvl nvl2 ora_hash path percent_rank percentile_cont percentile_disc power powermultiset powermultiset_by_cardinality prediction prediction_cost prediction_details prediction_probability prediction_set presentnnv presentv previous rank ratio_to_report rawtohex rawtonhex ref reftohex regexp_count regexp_instr regexp_replace regexp_substr regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_syy remainder replace round row_number rowidtochar rowidtonchar rownum rpad rtrim scn_to_timestamp sessiontimezone sign sin sinh soundex sqrt stats_binomial_test stats_crosstab stats_f_test stats_ks_test stats_mode stats_mw_test stats_one_way_anova stats_t_test_indep stats_t_test_indepu stats_t_test_one stats_t_test_paired stats_wsr_test stddev stddev_pop stddev_samp substring sum sys_connect_by_path sys_context sys_dburigen sys_extract_utc sys_guid sys_typeid sys_xmlagg sys_xmlgen sysdate systimestamp tan tanh timestamp_to_scn to_binary_double to_binary_float to_char to_clob to_date to_dsinterval to_lob to_multi_byte to_nclob to_number to_single_byte to_timestamp to_timestamp_tz to_yminterval translate treat trim trunc tz_offset uid unistr updatexml upper user userenv value var_pop var_samp variance vsize width_bucket xmlagg xmlcdata xmlcolattval xmlcomment xmlconcat xmlforest xmlparse xmlpi xmlquery xmlroot xmlsequence xmlserialize xmltable xmltransform ";
            var keywordsOracleBuiltInFunctions = "ABS ACOS ADD_MONTHS APPENDCHILDXML ASCII ASCIISTR ASIN ATAN ATAN2 AVG BFILENAME BIN_TO_NUM BITAND CARDINALITY CASE CAST CEIL CHARTOROWID CHR CLUSTER_ID CLUSTER_PROBABILITY CLUSTER_SET COALESCE COLLECT COMPOSE CONCAT CONVERT CORR CORR_K CORR_S COS COSH COUNT COVAR_POP COVAR_SAMP CUME_DIST CURRENT_DATE CURRENT_TIMESTAMP CV DBTIMEZONE DECODE DECOMPOSE DELETEXML DENSE_RANK DEPTH DEREF DUMP EMPTY_BLOB EMPTY_CLOB EXISTSNODE EXP EXTRACT EXTRACTVALUE FEATURE_ID FEATURE_SET FEATURE_VALUE FIRST FIRST_VALUE FLOOR FROM_TZ GREATEST GROUP_ID GROUPING GROUPING_ID HEXTORAW INITCAP INSERTCHILDXML INSERTXMLBEFORE INSTR INSTR2 INSTR4 INSTRB INSTRC ITERATION_NUMBER LAG LAST LAST_DAY LAST_VALUE LEAD LEAST LENGTH LENGTH2 LENGTH4 LENGTHB LENGTHC LISTAGG LN LNNVL LOCALTIMESTAMP LOG LOWER LPAD LTRIM MAKE_REF MAX MEDIAN MIN MOD MONTHS_BETWEEN NANVL NCHR NEW_TIME NEXT_DAY NLS_CHARSET_DECL_LEN NLS_CHARSET_ID NLS_CHARSET_NAME NLS_INITCAP NLS_LOWER NLS_UPPER NLSSORT NTH_VALUE NTILE NULLIF NUMTODSINTERVAL NUMTOYMINTERVAL NVL NVL2 ORA_HASH PATH PERCENT_RANK PERCENTILE_CONT PERCENTILE_DISC POWER POWERMULTISET POWERMULTISET_BY_CARDINALITY PREDICTION PREDICTION_COST PREDICTION_DETAILS PREDICTION_PROBABILITY PREDICTION_SET PRESENTNNV PRESENTV PREVIOUS RANK RATIO_TO_REPORT RAWTOHEX RAWTONHEX REF REFTOHEX REGEXP_COUNT REGEXP_INSTR REGEXP_REPLACE REGEXP_SUBSTR REGR_AVGX REGR_AVGY REGR_COUNT REGR_INTERCEPT REGR_R2 REGR_SLOPE REGR_SXX REGR_SYY REMAINDER REPLACE ROUND ROW_NUMBER ROWIDTOCHAR ROWIDTONCHAR ROWNUM RPAD RTRIM SCN_TO_TIMESTAMP SESSIONTIMEZONE SIGN SIN SINH SOUNDEX SQRT STATS_BINOMIAL_TEST STATS_CROSSTAB STATS_F_TEST STATS_KS_TEST STATS_MODE STATS_MW_TEST STATS_ONE_WAY_ANOVA STATS_T_TEST_INDEP STATS_T_TEST_INDEPU STATS_T_TEST_ONE STATS_T_TEST_PAIRED STATS_WSR_TEST STDDEV STDDEV_POP STDDEV_SAMP SUBSTR SUM SYS_CONNECT_BY_PATH SYS_CONTEXT SYS_DBURIGEN SYS_EXTRACT_UTC SYS_GUID SYS_TYPEID SYS_XMLAGG SYS_XMLGEN SYSDATE SYSTIMESTAMP TAN TANH TIMESTAMP_TO_SCN TO_BINARY_DOUBLE TO_BINARY_FLOAT TO_CHAR TO_CLOB TO_DATE TO_DSINTERVAL TO_LOB TO_MULTI_BYTE TO_NCLOB TO_NUMBER TO_SINGLE_BYTE TO_TIMESTAMP TO_TIMESTAMP_TZ TO_YMINTERVAL TRANSLATE TREAT TRIM TRUNC TZ_OFFSET UID UNISTR UPDATEXML UPPER USER USERENV VALUE VAR_POP VAR_SAMP VARIANCE VSIZE WIDTH_BUCKET XMLAGG XMLCDATA XMLCOLATTVAL XMLCOMMENT XMLCONCAT XMLFOREST XMLPARSE XMLPI XMLQUERY XMLROOT XMLSEQUENCE XMLSERIALIZE XMLTABLE XMLTRANSFORM ";

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'KeywordsConfig'");
            sbSql.Append("   AND AttributeName = 'BuiltInFunctions'");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            switch (_currentSourceType)
            {
                case DataSourceType.PostgreSql: //20231017 部份關鍵字不一樣，獨立出來
                    {
                        keywordsBuiltInFunctions = postgreSqlBuiltInFunctionsKeywords;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        keywordsBuiltInFunctions = keywordsMySqlBuiltInFunctions;
                        break;
                    }
                case DataSourceType.SqlServer: //20200308 部份關鍵字不一樣，獨立出來，並改為小寫
                    {
                        keywordsBuiltInFunctions = keywordsSqlServerBuiltInFunctions;
                        break;
                    }
                default: //Oracle
                    {
                        keywordsBuiltInFunctions = keywordsOracleBuiltInFunctions;
                        break;
                    }
            }

            sbSql.Clear();
            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeText, AttributeDate)");
            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'KeywordsConfig', 'BuiltInFunctions', '{keywordsBuiltInFunctions}', '{MyGlobal.DateTimeNow()}')");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            MyLibrary.KeywordsBuiltInFunctions = keywordsBuiltInFunctions;
        }

        private void AddBuiltInKeywords()
        {
            var sql = string.Empty;
            var sbSql = new StringBuilder();
            var builtInKeywords = string.Empty;
            var oracleBuiltInKeywords = "ABORT ACCEPT ACCESS ADD ADMIN AFTER ALL ALLOCATE ALTER ANALYZE AND ANY ARCHIVE ARCHIVELOG ARRAY ARRAYLEN AS ASC ASSERT ASSIGN AT AUDIT AUTHORIZATION AVG AVG BACKUP BASE_TABLE BECOME BEFORE BEGIN BETWEEN BINARY_INTEGER BLOCK BODY BOOLEAN BY CACHE CANCEL CASCADE CASE CHANGE CHAR CHAR_BASE CHARACTER CHECK CHECKPOINT CLOSE CLUSTER CLUSTERS COBOL COLAUTH COLUMN COLUMNS COMMENT COMMIT COMPILE COMPRESS CONNECT CONSTANT CONSTRAINT CONSTRAINTS CONTENTS CONTINUE CONTROLFILE COUNT CRASH CREATE CURRENT CURRVAL CURSOR CYCLE DATA_BASE DATABASE DATAFILE DATE DBA DEBUGOFF DEBUGON DEC DECIMAL DECLARE DEFAULT DEFINITION DELAY DELETE DELTA DESC DIGITS DISABLE DISMOUNT DISPOSE DISTINCT DO DOUBLE DROP DUMP EACH ELSE ELSIF ENABLE END ENTRY ESCAPE EVENTS EXCEPT EXCEPTION EXCEPTION_INIT EXCEPTIONS EXCLUSIVE EXEC EXECUTE EXISTS EXIT EXPLAIN EXTENT EXTERNALLY FETCH FILE FLOAT FLUSH FOR FORCE FOREIGN FORM FORTRAN FOUND FREELIST FREELISTS FROM FUNCTION GENERIC GO GOTO GRANT GROUP GROUPING GROUPS HAVING IDENTIFIED IF IMMEDIATE IN INCLUDING INCREMENT INDEX INDEXES INDICATOR INITIAL INITRANS INSERT INSTANCE INT INTEGER INTERSECT INTO IS KEY LANGUAGE LAYER LEVEL LIKE LIMITED LINK LISTS LOCK LOGFILE LONG LOOP MANAGE MANUAL MAX MAXDATAFILES MAXEXTENTS MAXINSTANCES MAXLOGFILES MAXLOGHISTORY MAXLOGMEMBERS MAXTRANS MAXVALUE MIN MINEXTENTS MINUS MINVALUE MLSLABEL MOD MODE MODIFY MODULE MOUNT NATURAL NEW NEXT NEXTVAL NOARCHIVELOG NOAUDIT NOCACHE NOCOMPRESS NOCYCLE NOMAXVALUE NOMINVALUE NONE NOORDER NORESETLOGS NORMAL NOSORT NOT NOTFOUND NOWAIT NULL NUMBER NUMBER_BASE NUMERIC OF OFF OFFLINE OLD ON ONLINE ONLY OPEN OPTIMAL OPTION OR ORDER OTHERS OUT OWN PACKAGE PARALLEL PARTITION PCTFREE PCTINCREASE PCTUSED PLAN PLI POSITIVE PRAGMA PRECISION PRIMARY PRIOR PRIVATE PRIVILEGES PROCEDURE PROFILE PUBLIC QUOTA RAISE RANGE RAW READ REAL RECORD RECOVER REFERENCES REFERENCING RELEASE REMR RENAME RESETLOGS RESOURCE RESTRICTED RETURN REUSE REVERSE REVOKE ROLE ROLES ROLLBACK ROW ROWID ROWLABEL ROWNUM ROWS ROWTYPE RUN SAVEPOINT SCHEMA SCN SECTION SEGMENT SELECT SEPARATE SEQUENCE SESSION SET SHARE SHARED SIZE SMALLINT SNAPSHOT SOME SORT SPACE SQL SQLBUF SQLCODE SQLERRM SQLERROR SQLSTATE START STATEMENT STATEMENT_ID STATISTICS STDDEV STOP STORAGE SUBTYPE SUCCESSFUL SUM SWITCH SYNONYM SYSDATE SYSTEM TABAUTH TABLE TABLES TABLESPACE TASK TEMPORARY TERMINATE THEN THREAD TIME TO TRACING TRANSACTION TRIGGER TRIGGERS TRUNCATE TYPE UID UNDER UNION UNIQUE UNLIMITED UNTIL UPDATE USE USER USING VALIDATE VALUES VARCHAR VARCHAR2 VARIANCE VIEW VIEWS WHEN WHENEVER WHERE WHILE WITH WITHIN WORK WRITE XOR FALSE TRUE ";
            var postgreSqlBuiltInKeywordsKeywords = "abort absent absolute access according action add admin after aggregate all allocate also alter always analyse analyze and any are array array_max_cardinality as asc asensitive assertion assignment asymmetric at atomic attribute attributes authorization backward base64 before begin begin_frame begin_partition bernoulli between bigint binary bit blob blocked bom boolean both breadth by bytea cache call called cardinality cascade cascaded case cast catalog catalog_name chain char character characteristics characters character_length character_set_catalog character_set_name character_set_schema check checkpoint class class_origin clob close cluster cobol collate collation collation_catalog collation_name collation_schema collect column columns column_name command_function command_function_code comment comments commit committed concurrently condition condition_number configuration conflict connect connection connection_name constraint constraints constraint_catalog constraint_name constraint_schema constructor contains content continue control conversion copy corresponding cost create cross csv cube current current_default_transform_group current_path current_role current_row current_transform_group_for_type cursor cursor_name cycle data database datalink date datetime_interval_code datetime_interval_precision day db deallocate dec decimal declare default defaults deferrable deferred defined definer degree delete delimiter delimiters depends depth deref derived desc describe descriptor deterministic diagnostics dictionary disable discard disconnect dispatch distinct dlnewcopy dlpreviouscopy dlurlcomplete dlurlcompleteonly dlurlcompletewrite dlurlpath dlurlpathonly dlurlpathwrite dlurlscheme dlurlserver dlvalue do document domain double drop dynamic dynamic_function dynamic_function_code each element else empty enable encoding encrypted end end-exec end_frame end_partition enforced enum equals escape event except exception exclude excluding exclusive exec execute explain expression extension external false fetch file filter final first flag float following for force foreign fortran forward found frame_row free freeze from fs full function functions fusion general generated get global go goto grant granted group grouping groups handler having header hex hierarchy hold hour id identity if ignore ilike immediate immediately immutable implementation implicit import in including increment indent index indexes indicator inherit inherits initially inline inner inout input insensitive insert instance instantiable instead int integer integrity intersect intersection interval into invoker is isnull isolation join key key_member key_type label language large last lateral leading leakproof level library like like_regex limit link listen load local location locator lock locked logged map mapping match matched materialized maxvalue max_cardinality member merge message_length message_octet_length message_text method minute minvalue mode modifies module month more move multiset mumps name names namespace national natural nchar nclob nesting new next nfc nfd nfkc nfkd nil no none normalize normalized not nothing notify notnull nowait null nullable nulls number numeric object occurrences_regex octets of off offset oids old on only open operator option options or order ordering ordinality others out outer output over overlaps overriding owned owner pad parallel parameter parameter_mode parameter_name parameter_ordinal_position parameter_specific_catalog parameter_specific_name parameter_specific_schema parser partial partition pascal passing passthrough password percent percentile_cont percentile_disc period permission placing plans pli policy portion position_regex precedes preceding precision prepare prepared preserve primary prior privileges procedural procedure program public quote range read reads real reassign recheck recovery recursive ref references referencing refresh reindex relative release rename repeatable replica requiring reset respect restart restore restrict result return returned_cardinality returned_length returned_octet_length returned_sqlstate returning returns revoke role rollback rollup routine routine_catalog routine_name routine_schema row rows row_count rule savepoint scale schema schema_name scope scope_catalog scope_name scope_schema scroll search second section security select selective self sensitive sequence sequences serializable server server_name session set setof sets share show similar simple size skip smallint snapshot some source space specific specifictype specific_name sql sqlcode sqlerror sqlexception sqlstate sqlwarning stable standalone start state statement static statistics stdin stdout storage strict structure style subclass_origin submultiset substring_regex succeeds symmetric sysid system system_time system_user table tables tablesample tablespace table_name temp template temporary then ties time timestamp timezone_hour timezone_minute to token top_level_count trailing transaction transactions_committed transactions_rolled_back transaction_active transform transforms translate_regex translation treat trigger trigger_catalog trigger_name trigger_schema trim_array true truncate trusted type types uescape unbounded uncommitted under unencrypted union unique unknown unlink unlisten unlogged unnamed until untyped update uri usage use user_defined_type_catalog user_defined_type_code user_defined_type_name user_defined_type_schema using vacuum valid validate validator value values value_of varbinary varchar variadic varying verbose versioning view views volatile when whenever where whitespace window with within without work wrapper write xml xmlattributes xmlbinary xmlcast xmldeclaration xmldocument xmliterate xmlnamespaces xmlparse xmlquery xmlschema xmlserialize xmltable xmltext xmlvalidate year yes zone ";
            var sqlServerBuiltInKeywords = "abort accept access add admin after all allocate alter analyze and any archive archivelog array arraylen as asc assert assign at audit authorization avg avg backup base_table become before begin between binary binary_integer block body boolean by cache cancel cascade case change char char_base character check checkpoint close cluster clusters cobol colauth column columns comment commit compile compress connect constant constraint constraints contents continue controlfile count crash create current currval cursor cycle data_base database datafile date dba debugoff debugon dec decimal declare default definition delay delete delta desc digits disable dismount dispose distinct do double drop dump each else elsif enable end entry escape events except exception exception_init exceptions exclusive exec execute exists exit explain extent externally fetch file float flush for force foreign form fortran found freelist freelists from function generic go goto grant group grouping groups having identified if immediate in including increment index indexes indicator initial initrans insert instance int integer intersect into is key language layer level like limited link lists lock logfile long loop manage manual max maxdatafiles maxextents maxinstances maxlogfiles maxloghistory maxlogmembers maxtrans maxvalue min minextents minus minvalue mlslabel mod mode modify module mount natural new next nextval noarchivelog noaudit nocache nocompress nocycle nomaxvalue nominvalue none noorder noresetlogs normal nosort not notfound nowait null number number_base numeric of off offline old on online only open optimal option or order others out own package parallel partition pctfree pctincrease pctused plan pli positive pragma precision primary prior private privileges procedure profile public quota raise range raw read real record recover references referencing release remr rename resetlogs resource restricted return reuse reverse revoke role roles rollback row rowid rowlabel rownum rows rowtype run savepoint schema scn section segment select separate sequence session set share shared size smallint snapshot some sort space sql sqlbuf sqlcode sqlerrm sqlerror sqlstate start statement statement_id statistics stddev stop storage subtype successful sum switch synonym sysdate system tabauth table tables tablespace task temporary terminate then thread time to tracing transaction trigger triggers truncate type uid under union unique unlimited until update use user using validate values varbinary varchar varchar2 variance view views when whenever where while with within work write xor false true ";
            var mySqlBuiltInKeywords = "abort absent absolute access according action add admin after aggregate all allocate also alter always analyse analyze and any are array array_max_cardinality as asc asensitive assertion assignment asymmetric at atomic attribute attributes authorization backward base64 before begin begin_frame begin_partition bernoulli between bigint binary bit blob blocked bom boolean both breadth by cache call called cardinality cascade cascaded case cast catalog catalog_name chain char character characteristics characters character_length character_set_catalog character_set_name character_set_schema check checkpoint class class_origin clob close cluster cobol collate collation collation_catalog collation_name collation_schema collect column columns column_name command_function command_function_code comment comments commit committed concurrently condition condition_number configuration conflict connect connection connection_name constraint constraints constraint_catalog constraint_name constraint_schema constructor contains content continue control conversion copy corresponding cost create cross csv cube current current_default_transform_group current_path current_role current_row current_transform_group_for_type cursor cursor_name cycle data database datalink date datetime_interval_code datetime_interval_precision day db deallocate dec decimal declare default defaults deferrable deferred defined definer degree delete delimiter delimiters depends depth deref derived desc describe descriptor deterministic diagnostics dictionary disable discard disconnect dispatch distinct dlnewcopy dlpreviouscopy dlurlcomplete dlurlcompleteonly dlurlcompletewrite dlurlpath dlurlpathonly dlurlpathwrite dlurlscheme dlurlserver dlvalue do document domain double drop dynamic dynamic_function dynamic_function_code each element else empty enable encoding encrypted end end-exec end_frame end_partition enforced enum equals escape event except exception exclude excluding exclusive exec execute explain expression extension external false fetch file filter final first flag float following for force foreign fortran forward found frame_row free freeze from fs full function functions fusion general generated get global go goto grant granted group grouping groups handler having header hex hierarchy hold hour id identity if ignore ilike immediate immediately immutable implementation implicit import in including increment indent index indexes indicator inherit inherits initially inline inner inout input insensitive insert instance instantiable instead int integer integrity intersect intersection interval into invoker is isnull isolation join key key_member key_type label language large last lateral leading leakproof level library like like_regex limit link listen load local location locator lock locked logged map mapping match matched materialized maxvalue max_cardinality member merge message_length message_octet_length message_text method minute minvalue mode modifies module month more move multiset mumps name names namespace national natural nchar nclob nesting new next nfc nfd nfkc nfkd nil no none normalize normalized not nothing notify notnull nowait null nullable nulls number numeric object occurrences_regex octets of off offset oids old on only open operator option options or order ordering ordinality others out outer output over overlaps overriding owned owner pad parallel parameter parameter_mode parameter_name parameter_ordinal_position parameter_specific_catalog parameter_specific_name parameter_specific_schema parser partial partition pascal passing passthrough password percent percentile_cont percentile_disc period permission placing plans pli policy portion position_regex precedes preceding precision prepare prepared preserve primary prior privileges procedural procedure program public quote range read reads real reassign recheck recovery recursive ref references referencing refresh reindex relative release rename repeatable replica requiring reset respect restart restore restrict result return returned_cardinality returned_length returned_octet_length returned_sqlstate returning returns revoke role rollback rollup routine routine_catalog routine_name routine_schema row rows row_count rule savepoint scale schema schema_name scope scope_catalog scope_name scope_schema scroll search second section security select selective self sensitive sequence sequences serializable server server_name session set setof sets share show similar simple size skip smallint snapshot some source space specific specifictype specific_name sql sqlcode sqlerror sqlexception sqlstate sqlwarning stable standalone start state statement static statistics stdin stdout storage strict structure style subclass_origin submultiset substring_regex succeeds symmetric sysid system system_time system_user table tables tablesample tablespace table_name temp template temporary then ties time timestamp timezone_hour timezone_minute to token top_level_count trailing transaction transactions_committed transactions_rolled_back transaction_active transform transforms translate_regex translation treat trigger trigger_catalog trigger_name trigger_schema trim_array true truncate trusted type types uescape unbounded uncommitted under unencrypted union unique unknown unlink unlisten unlogged unnamed until untyped update uri usage use user_defined_type_catalog user_defined_type_code user_defined_type_name user_defined_type_schema using vacuum valid validate validator value values value_of varbinary varchar variadic varying verbose versioning view views volatile when whenever where whitespace window with within without work wrapper write xml xmlattributes xmlbinary xmlcast xmldeclaration xmldocument xmliterate xmlnamespaces xmlparse xmlquery xmlschema xmlserialize xmltable xmltext xmlvalidate year yes zone ";

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'KeywordsConfig'");
            sbSql.Append("   AND AttributeName = 'BuiltInKeywords'");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            switch (_currentSourceType)
            {
                case DataSourceType.PostgreSql: //20231017 部份關鍵字不一樣，獨立出來
                    {
                        builtInKeywords = postgreSqlBuiltInKeywordsKeywords;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        builtInKeywords = mySqlBuiltInKeywords;
                        break;
                    }
                case DataSourceType.SqlServer: //20200308 部份關鍵字不一樣，獨立出來，並改為小寫
                    {
                        builtInKeywords = sqlServerBuiltInKeywords;
                        break;
                    }
                default: //Oracle
                    {
                        builtInKeywords = oracleBuiltInKeywords;
                        break;
                    }
            }

            sbSql.Clear();
            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeText, AttributeDate)");
            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'KeywordsConfig', 'BuiltInKeywords', '{builtInKeywords}', '{MyGlobal.DateTimeNow()}')");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            MyLibrary.KeywordsBuiltInKeywords = builtInKeywords;
        }

        private void AddOperatorKeywords()
        {
            var sql = string.Empty;
            var sbSql = new StringBuilder();
            var operatorKeywords = "all and any between cross exists in inner is join left like not null or outer pivot right some unpivot ( ) *";

            sbSql.AppendLine("DELETE FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'KeywordsConfig'");
            sbSql.Append("   AND AttributeName = 'OperatorKeywords'");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            sbSql.Clear();
            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeText, AttributeDate)");
            sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'KeywordsConfig', 'OperatorKeywords', '{operatorKeywords}', '{MyGlobal.DateTimeNow()}')");

            sql = sbSql.ToString();
            JasonQueryRepository.ExecNonQuery(sql);

            MyLibrary.KeywordsOperatorKeywords = operatorKeywords;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBoxManager.Unregister();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //記錄位置
            if (Location.X > 0)
            {
                JasonQueryRepository.UpdateSetting("GlobalConfig", "MainFormLocationX", Location.X.ToString());
            }

            if (Location.Y > 0)
            {
                JasonQueryRepository.UpdateSetting("GlobalConfig", "MainFormLocationY", Location.Y.ToString());
            }
        }

        private static List<ToolStripMenuItem> GetItems(ToolStrip menuStrip)
        {
            var items = new List<ToolStripMenuItem>();

            foreach (ToolStripMenuItem i in menuStrip.Items)
            {
                GetMenuItems(i, items);
            }

            return items;
        }

        private static void GetMenuItems(ToolStripMenuItem item, ICollection<ToolStripMenuItem> items)
        {
            items.Add(item);

            foreach (ToolStripItem i in item.DropDownItems)
            {
                if (i is ToolStripMenuItem menuItem)
                {
                    GetMenuItems(menuItem, items);
                }
            }
        }

        private void CloseIt(object sender, EventArgs e) //在 Tab 上按右鍵選單選擇「關閉」
        {
            CloseTab();
        }

        private void tabControl1_ClosePressed(object sender, EventArgs e) //按下 tabControl 的右上角 X
        {
            CloseTab();
        }

        private void CloseTab()
        {
            try
            {
                var isCancelClose = false;
                var isCloseOptionsForm = false; //要關閉的是否為 Options Form？
                var isCloseSchemaBrowserForm = false; //要關閉的是否為 SchemaBrowser Form？
                var tabPagesCount = tabControl1.TabPages.Count;

                if (tabPagesCount > 0)
                {
                    var title = tabControl1.SelectedTab.Title;
                    var tabAccessibleDescription = tabControl1.SelectedTab.AccessibleDescription;
                    var tabAccessibleName = tabControl1.SelectedTab.AccessibleName;

                    if (title.StartsWith("*", StringComparison.Ordinal))
                    {
                        //關閉 Query Form !
                        MyGlobal.GlobalTemp = $"CloseQueryForm`{tabAccessibleDescription};";

                        while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                        {
                            if (MyGlobal.GlobalTemp.Contains($"{tabAccessibleDescription}|CANCEL;"))
                            {
                                isCancelClose = true;
                                break;
                            }

                            Application.DoEvents();
                        }

                        if (!isCancelClose)
                        {
                            //20240301 關閉頁籤：同時刪除備份的對應記錄，下次重啟 JasonQuery 時，不用再開啟此檔案
                            MyGlobal.DeleteBackupFileInfo(tabAccessibleName);

                            tabControl1.SelectedTab.Dispose();
                            tabControl1.TabPages.Remove(tabControl1.SelectedTab);

                            MyGlobal.ClearMemory();
                            UpdateMainMenuTitle();
                        }
                    }
                    else
                    {
                        if (title == MyGlobal.SchemaBrowserTabName)
                        {
                            //此處可能需要關閉連線
                            MyGlobal.GlobalTemp = "NoSplit"; //不要觸發 Split event
                            MyGlobal.FormSchemaBrowserKey = string.Empty;
                            mnuSchemaBrowser.Enabled = true;
                            isCloseSchemaBrowserForm = true;

                            var iSchemaBrowserTabCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => theTab.Title == MyGlobal.SchemaBrowserTabName);

                            if (iSchemaBrowserTabCount == 1)
                            {
                                //20231011 關閉最後一個 SchemaBrowser 才要取消勾選！
                                mnuSchemaBrowser.Checked = false;
                                AppConfigHelper.HasMultiOpenSchemaBrowser = false;
                            }

                            //20240801 關閉 SchemaBrowser Form !
                            MyGlobal.GlobalTemp = $"CloseSchemaBrowserForm`{tabAccessibleDescription};";

                            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp) && MyGlobal.GlobalTemp.Contains($"`{tabAccessibleDescription}"))
                            {
                                if (MyGlobal.GlobalTemp.Contains($"{tabAccessibleDescription}|CANCEL;"))
                                {
                                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{tabAccessibleDescription}|CANCEL;", string.Empty);
                                    isCancelClose = true;
                                    break;
                                }

                                Application.DoEvents();
                            }

                            if (!isCancelClose)
                            {
                                MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{tabAccessibleDescription};", string.Empty);
                                tabControl1.SelectedTab.Dispose();
                                tabControl1.TabPages.Remove(tabControl1.SelectedTab);

                                MyGlobal.ClearMemory();
                                UpdateMainMenuTitle();
                            }

                            if (MyGlobal.GlobalTemp == "CloseSchemaBrowserForm`")
                            {
                                MyGlobal.GlobalTemp = string.Empty;
                            }
                        }
                        else if (tabControl1.SelectedTab.Title == MyGlobal.OptionsTabName)
                        {
                            mnuOptions.Enabled = true;
                            mnuOptions.Checked = false;
                            isCloseOptionsForm = true;

                            //Options Form 要用特殊方法關閉
                            MyGlobal.GlobalTemp = "CloseOptionsTab`"; //通知 Options Form 取消套用，再由 Options Form 回傳關閉 Tab 的指令
                        }
                        else if (tabControl1.SelectedTab.Title == MyGlobal.SqlHistoryTabName)
                        {
                            mnuSqlHistory.Enabled = true;
                            mnuSqlHistory.Checked = false;
                        }
                        else if (tabControl1.SelectedTab.Title == MyGlobal.CreateTableTabName)
                        {
                            mnuCreateTable.Enabled = true;
                            mnuCreateTable.Checked = false;
                        }
                        else
                        {
                            //此 Query Form 不需要存檔，直接關閉即可!
                            MyGlobal.GlobalTemp = $"CloseQueryForm`{tabAccessibleDescription};";

                            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                            {
                                Application.DoEvents();
                            }
                        }

                        if (!isCloseOptionsForm && !isCloseSchemaBrowserForm)
                        {
                            //20240301 關閉頁籤：同時刪除備份的對應記錄，下次重啟 JasonQuery 時，不用再開啟此檔案
                            MyGlobal.DeleteBackupFileInfo(tabControl1.SelectedTab.AccessibleName);

                            tabControl1.SelectedTab.Dispose();
                            tabControl1.TabPages.Remove(tabControl1.SelectedTab);

                            MyGlobal.ClearMemory();
                            UpdateMainMenuTitle();
                        }

                        if (MyGlobal.GlobalTemp == "NoSplit")
                        {
                            MyGlobal.GlobalTemp = string.Empty;
                        }
                    }
                }

                var tabPagesCount2 = tabControl1.TabPages.Cast<TabPage>().Count(theTab => SpecialTabName.Contains(theTab.Title));

                tabPagesCount = tabControl1.TabPages.Count; //重新抓取一次

                if (tabPagesCount != 0 && tabPagesCount != tabPagesCount2)
                {
                    UpdateTabList();
                    return;
                }

                //20190928 改為：如果把最後一個 Tab 關閉，自動再開啟一個空白的 Tab
                var result = CheckTabNameExist();

                if (!CheckTabNameExist(result))
                {
                    CreateNewTab("Query", result);
                }

                UpdateTabList();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CloseTabIndex(int tabIndex)
        {
            var isCancelClose = false;
            var isCloseOptionsForm = false; //要關閉的是否為 Options Form？
            var isCloseSchemaBrowserForm = false; //要關閉的是否為 SchemaBrowser Form？
            var tabAccessibleDescription = tabControl1.TabPages[tabIndex].AccessibleDescription;
            var tabPagesCount = tabControl1.TabPages.Count;

            try
            {
                if (tabPagesCount > 0)
                {
                    var title = tabControl1.TabPages[tabIndex].Title;

                    if (title.StartsWith("*", StringComparison.Ordinal))
                    {
                        //關閉 Query Form !
                        MyGlobal.GlobalTemp = $"CloseQueryForm`{tabAccessibleDescription};";

                        while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                        {
                            if (MyGlobal.GlobalTemp.Contains($"{tabAccessibleDescription}|CANCEL;"))
                            {
                                isCancelClose = true;
                                break;
                            }

                            Application.DoEvents();
                        }

                        if (isCancelClose)
                        {
                            return;
                        }

                        //關閉頁籤：同時刪除備份的對應記錄，下次重啟 JasonQuery 時，不用再開啟此檔案
                        MyGlobal.DeleteBackupFileInfo(tabControl1.TabPages[tabIndex].AccessibleName);

                        tabControl1.TabPages[tabIndex].Dispose();
                        tabControl1.TabPages.Remove(tabControl1.TabPages[tabIndex]);

                        MyGlobal.ClearMemory();
                        UpdateMainMenuTitle();
                    }
                    else
                    {
                        if (title == MyGlobal.SchemaBrowserTabName)
                        {
                            //此處可能需要關閉連線
                            MyGlobal.GlobalTemp = "NoSplit"; //不要觸發 Split event
                            MyGlobal.FormSchemaBrowserKey = string.Empty;
                            mnuSchemaBrowser.Enabled = true;
                            isCloseSchemaBrowserForm = true;

                            var iSchemaBrowserTabCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => theTab.Title == MyGlobal.SchemaBrowserTabName);

                            if (iSchemaBrowserTabCount == 1)
                            {
                                //20231011 關閉最後一個 SchemaBrowser 才要取消勾選！
                                mnuSchemaBrowser.Checked = false;
                                AppConfigHelper.HasMultiOpenSchemaBrowser = false;
                            }

                            //20240801 關閉 SchemaBrowser Form !
                            MyGlobal.GlobalTemp = $"CloseSchemaBrowserForm`{tabAccessibleDescription};";

                            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp) && MyGlobal.GlobalTemp.Contains($"`{tabAccessibleDescription}"))
                            {
                                if (MyGlobal.GlobalTemp.Contains($"{tabAccessibleDescription}|CANCEL;"))
                                {
                                    MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{tabAccessibleDescription}|CANCEL;", string.Empty);
                                    isCancelClose = true;
                                    break;
                                }

                                Application.DoEvents();
                            }

                            if (!isCancelClose)
                            {
                                MyGlobal.GlobalTemp = MyGlobal.GlobalTemp.Replace($"{tabAccessibleDescription};", string.Empty);
                                tabControl1.TabPages[tabIndex].Dispose();
                                tabControl1.TabPages.Remove(tabControl1.TabPages[tabIndex]);

                                MyGlobal.ClearMemory();
                                UpdateMainMenuTitle();
                            }

                            if (MyGlobal.GlobalTemp == "CloseSchemaBrowserForm`")
                            {
                                MyGlobal.GlobalTemp = string.Empty;
                            }
                        }
                        else if (title == MyGlobal.OptionsTabName)
                        {
                            mnuOptions.Enabled = true;
                            mnuOptions.Checked = false;
                            isCloseOptionsForm = true;

                            //Options Form 要用特殊方法關閉
                            MyGlobal.GlobalTemp = $"CloseOptionsTab`{tabIndex}"; //通知 Options Form 取消套用，再由 Options Form 回傳關閉 Tab 的指令
                        }
                        else if (title == MyGlobal.SqlHistoryTabName)
                        {
                            mnuSqlHistory.Enabled = true;
                            mnuSqlHistory.Checked = false;
                        }
                        else if (title == MyGlobal.CreateTableTabName)
                        {
                            mnuCreateTable.Enabled = true;
                            mnuCreateTable.Checked = false;
                        }
                        else
                        {
                            //此 Query Form 不需要存檔，直接關閉即可!
                            MyGlobal.GlobalTemp = $"CloseQueryForm`{tabAccessibleDescription};";

                            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                            {
                                Application.DoEvents();
                            }
                        }

                        if (!isCloseOptionsForm && !isCloseSchemaBrowserForm)
                        {
                            //20240301 關閉頁籤：同時刪除備份的對應記錄，下次重啟 JasonQuery 時，不用再開啟此檔案
                            MyGlobal.DeleteBackupFileInfo(tabControl1.TabPages[tabIndex].AccessibleName);

                            tabControl1.TabPages[tabIndex].Dispose();
                            tabControl1.TabPages.Remove(tabControl1.TabPages[tabIndex]);

                            MyGlobal.ClearMemory();
                            UpdateMainMenuTitle();
                        }

                        if (MyGlobal.GlobalTemp == "NoSplit")
                        {
                            MyGlobal.GlobalTemp = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                tabPagesCount = tabControl1.TabPages.Count;

                if (tabPagesCount == 0) //20250903 如果把最後一個 Tab 關閉，自動再開啟一個空白的 Tab
                {
                    var result = CheckTabNameExist();

                    if (!CheckTabNameExist(result))
                    {
                        CreateNewTab("Query", result);
                    }
                }

                UpdateTabList();
            }
        }

        private void CreateNewTab(string functionName, string tabTitle, string optional = "", string optional2 = "", string optional3 = "")
        {
            try
            {
                Control controlToAdd = null;
                var accessibleDescription = $"{DateTime.Now:yyyyMMddHHmmssfff}";

                //20240301 記錄備份的訊息
                var accessibleName = $"{MyGlobal.DomainUser.Replace("\\", "@")}{MyGlobal.Separator00A1}{JasonQueryRepository.DbMotherPid}{MyGlobal.Separator00A1}{accessibleDescription}";

                switch (functionName)
                {
                    case "Query":
                    case "Query1":
                    case "Query2":
                    case "Query3":
                    case "SQLEditor":
                        {
                            _queryForm = new QueryForm
                            {
                                MdiParent = this,
                                Tag = tabTitle,
                                AccessibleDefaultActionDescription = string.Empty
                            };

                            if (functionName == "SQLEditor")
                            {
                                _queryForm.AccessibleDefaultActionDescription = $"SQL:{optional}";
                            }
                            else if (new HashSet<string> { "Query", "Query1", "Query2" }.Contains($"{functionName}"))
                            {
                                if (!string.IsNullOrEmpty(optional))
                                {
                                    switch (functionName)
                                    {
                                        case "Query1":
                                            {
                                                _queryForm.AccessibleDefaultActionDescription = $"OPEN1:{optional}";
                                                break;
                                            }
                                        case "Query2":
                                            {
                                                _queryForm.AccessibleDefaultActionDescription = $"OPEN2:{optional}";
                                                break;
                                            }
                                        default:
                                            {
                                                _queryForm.AccessibleDefaultActionDescription = $"OPEN0:{optional}";
                                                break;
                                            }
                                    }
                                }
                            }

                            _queryForm.AccessibleDescription = accessibleDescription;
                            _queryForm.AccessibleName = accessibleName;

                            if (functionName == "Query3")
                            {
                                var originalAccessibleName = Path.GetFileName(optional);

                                accessibleDescription = originalAccessibleName.Split(new[] { MyGlobal.Separator00A1 }, StringSplitOptions.None)[2];
                                _queryForm.AccessibleDescription = accessibleDescription; //延用原本的時間戳記
                                _queryForm.AccessibleName = originalAccessibleName; //延用原本的 AccessibleName
                                _queryForm.BackupRealFullFileName = !string.IsNullOrEmpty(optional2) ? optional2 : "FileNotSavedYet"; //使用者實際存檔的完整路徑+檔案名稱(此變數若為空值，表示使用者當初並未儲檔)【sFileNameSaved】
                                int.TryParse(optional3, out var backupCurrentPosition);
                                _queryForm.BackupCurrentPosition = backupCurrentPosition;
                            }

                            _queryForm.ValueUpdated += ValueUpdated;
                            controlToAdd = _queryForm;
                            break;
                        }
                    case "":
                        {
                            break;
                        }
                    default:
                        {
                            if (functionName == MyGlobal.SchemaBrowserTabName)
                            {
                                _schemaBrowserForm = new SchemaBrowserForm
                                {
                                    MdiParent = this,
                                    Tag = tabTitle,
                                    AccessibleDescription = accessibleDescription,
                                    AccessibleDefaultActionDescription = optional
                                };

                                MyGlobal.FormSchemaBrowserKey = accessibleDescription;
                                _schemaBrowserForm.ValueUpdated += ValueUpdated;
                                controlToAdd = _schemaBrowserForm;
                            }
                            else if (functionName == MyGlobal.SqlHistoryTabName)
                            {
                                _sqlHistoryForm = new SqlHistoryForm
                                {
                                    MdiParent = this,
                                    Tag = tabTitle,
                                    AccessibleDescription = accessibleDescription
                                };

                                MyGlobal.FormSqlHistoryKey = accessibleDescription;
                                _sqlHistoryForm.ValueUpdated += ValueUpdated;
                                controlToAdd = _sqlHistoryForm;
                            }
                            else if (functionName == MyGlobal.CreateTableTabName)
                            {
                                switch (_currentSourceType)
                                {
                                    case DataSourceType.Oracle:
                                        {
                                            var oracleCreateTableWizardForm = new OracleCreateTableWizardForm
                                            {
                                                MdiParent = this,
                                                Tag = tabTitle,
                                                AccessibleDescription = accessibleDescription
                                            };

                                            MyGlobal.FormCreateTableKey = accessibleDescription;
                                            oracleCreateTableWizardForm.ValueUpdated += ValueUpdated;
                                            controlToAdd = oracleCreateTableWizardForm;

                                            break;
                                        }
                                    case DataSourceType.PostgreSql:
                                    case DataSourceType.SqlServer:
                                    case DataSourceType.MySql:
                                        {
                                            break;
                                        }
                                    case DataSourceType.None:
                                        {
                                            break;
                                        }
                                }
                            }
                            else if (functionName == MyGlobal.OptionsTabName)
                            {
                                _optionsForm = new OptionsForm
                                {
                                    MdiParent = this,
                                    Tag = tabTitle,
                                    AccessibleDescription = accessibleDescription
                                };

                                MyGlobal.FormOptionsKey = accessibleDescription;
                                _optionsForm.ValueUpdated += ValueUpdated;
                                controlToAdd = _optionsForm;
                            }

                            break;
                        }
                }

                var tabPage = new TabPage(tabTitle, controlToAdd, null, 0)
                {
                    Selected = true,
                    Tag = tabTitle,
                    AccessibleDescription = accessibleDescription,
                    AccessibleName = accessibleName
                };

                //20240203 頁籤加上圖示
                if (functionName == MyGlobal.SchemaBrowserTabName)
                {
                    tabPage.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Schema Browser 16x16.ico");
                }
                else if (functionName == MyGlobal.SqlHistoryTabName)
                {
                    tabPage.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL History 16x16.ico"); //
                }
                else if (functionName == MyGlobal.CreateTableTabName)
                {
                    tabPage.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Create Table 16x16.ico");
                }
                else if (functionName == MyGlobal.OptionsTabName)
                {
                    tabPage.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "Options 16x16.ico");
                }
                else
                {
                    tabPage.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL Editor Blue 16x16.ico");
                }

                tabControl1.TabPages.Add(tabPage);

                tsMainMenuToolBar.Visible = false;
                EnableCloseTabMenu(true);

                if (_isFormLoadFinished)
                {
                    UpdateTabList();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string CheckTabNameExist()
        {
            var numbers = new List<int>();

            foreach (TabPage tab in tabControl1.TabPages)
            {
                var sTitle = tab.Title.TrimStart('*').ToUpper();

                if (sTitle.StartsWith("SQL EDITOR", StringComparison.OrdinalIgnoreCase))
                {
                    //移除標題中的 SQL Editor 與 .SQL
                    //20250815 不處理 .SQL (忽略已存檔的頁籤，避免使用者存檔為「SQL Editor 155.sql」，取得 156)
                    var sNumPart = sTitle.Replace("SQL EDITOR", string.Empty)
                                         //.Replace(".SQL", string.Empty)
                                         .Trim();

                    if (int.TryParse(sNumPart, out var iNum))
                    {
                        numbers.Add(iNum);
                    }
                }
            }

            int iNextNumber = numbers.Count > 0 ? numbers.Max() + 1 : 1;

            return $"SQL Editor {iNextNumber}";
        }

        private bool CheckTabNameExist(string sTabName)
        {
            try
            {
                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var sValue = TextHelper.GetSafeString(theTab.Tag).Replace(@"*", string.Empty);

                    //20190930 改用 full path + filename 判斷是否已存在，因為有可能是同檔名但不同路徑
                    if (sValue != sTabName)
                    {
                        continue;
                    }

                    theTab.Selected = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return false;
        }

        private void tabControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CreateNewTab("Query", CheckTabNameExist()); //Tab頁籤，雙擊左鍵
        }

        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (tabControl1.TabPages.Count > 0)
                {
                    EnableCloseTabMenu(true, true);
                }
                else
                {
                    return;
                }

                _cMenu.Show(tabControl1, new Point(e.X, e.Y));
            }
            else
            {
                UpdateMainMenuTitle();
            }
        }

        private void UpdateMainMenuTitle()
        {
            foreach (TabPage theTab in tabControl1.TabPages)
            {
                var sTitle = theTab.Title;
                var accessibleDescription = theTab.AccessibleDescription;

                if (!theTab.Selected)
                {
                    continue;
                }

                //20190909 點選到的 Tab，檢查檔案是否有被外部程式異動內容，或是被刪除了
                if (SpecialTabName.Contains(sTitle))
                {
                    MyGlobal.CheckFileFromMDIForm = $"`{accessibleDescription}`";
                }
            }
        }

        private void EnableCloseTabMenu(bool bEnable, bool bCheckFavorite = false)
        {
            var bSaved = false;
            var sFullFileName = string.Empty;

            _cMenu.Items[MapColumn.Close].Enabled = bEnable;
            tabControl1.ShowClose = bEnable;

            _cMenu.Items[MapColumn.NewSQLEditor].Enabled = true;

            if (!bCheckFavorite)
            {
                return;
            }

            foreach (TabPage theTab in tabControl1.TabPages)
            {
                if (!theTab.Selected)
                {
                    continue;
                }

                var sTitle = theTab.Title;
                var sValue1 = theTab.Title.Replace("*", string.Empty);
                var sValue2 = TextHelper.GetSafeString(theTab.Tag).Replace("*", string.Empty);

                //兩者不相等，表示有存檔過了
                if (sValue1 == sValue2)
                {
                    continue;
                }

                //略過以下幾個
                if (SpecialTabName.Contains(sTitle))
                {
                    continue;
                }

                bSaved = true;
                sFullFileName = TextHelper.GetSafeString(theTab.Tag);
            }

            var bValue = false;

            if (bSaved)
            {
                var bExist = CheckExist4MyFavoriteFiles(sFullFileName);

                _cMenu.Items[MapColumn.AddToMyFavorite].Enabled = !bExist;
                _cMenu.Items[MapColumn.RemoveFromMyFavorite].Enabled = bExist;
                bValue = true;
            }
            else
            {
                _cMenu.Items[MapColumn.AddToMyFavorite].Enabled = false;
                _cMenu.Items[MapColumn.RemoveFromMyFavorite].Enabled = false;
            }

            _cMenu.Items[MapColumn.AddToMyFavorite].Tag = sFullFileName;
            _cMenu.Items[MapColumn.RemoveFromMyFavorite].Tag = sFullFileName;
            _cMenu.Items[MapColumn.OpenFolder].Enabled = bValue;
            _cMenu.Items[MapColumn.OpenFolder].Tag = sFullFileName;
            _cMenu.Items[MapColumn.CopyFullFilePath].Enabled = bValue;
            _cMenu.Items[MapColumn.CopyFullFilePath].Tag = sFullFileName;
            _cMenu.Items[MapColumn.CopyFileName].Enabled = bValue;
            _cMenu.Items[MapColumn.CopyFileName].Tag = sFullFileName;
            _cMenu.Items[MapColumn.CopyCurrentPath].Enabled = bValue;
            _cMenu.Items[MapColumn.CopyCurrentPath].Tag = sFullFileName;

            //20241217 判斷 Rename Tab 選單的 Enabled 狀態
            //_cMenu.Items[MyColumnMap.RenameTab].Enabled = string.IsNullOrEmpty(sFullFileName);
        }

        private static bool CheckExist4MyFavoriteFiles(string fullFileName)
        {
            bool result;
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'MyFavoriteFiles'");
            sbSql.Append($"   AND AttributeText = '{fullFileName.Replace("'", "''")}'");

            var sql = sbSql.ToString();
            var dtTemp = JasonQueryRepository.ExecQuery(sql);

            result = dtTemp?.Rows.Count > 0;
            return result;
        }

        private void AddToMyFavoriteFiles(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            var sFileName = TextHelper.GetSafeString(mnuItem.Tag);

            try
            {
                //去掉最前面的 * 符號
                sFileName = sFileName.TrimStart('*');

                var sAliasName = string.Empty;
                var sTitle = LocalizationHelper.GetLanguageString("Add to \"My Favorite\"", "form", GetType().Name, "menu", "AddToMyFavorite", "Text");
                var sPromptText = LocalizationHelper.GetLanguageString("alias", "form", GetType().Name, "msg", "AliasName", "Text");
                var sPromptText2 = LocalizationHelper.GetLanguageString("file", "form", GetType().Name, "msg", "FileName", "Text");

                if (AliasName(sTitle, sPromptText, sPromptText2, Cursor.Position.X, Cursor.Position.Y, sFileName, ref sAliasName) != DialogResult.OK)
                {
                    return;
                }

                SaveMyFavoriteFiles(sFileName, false, false, sAliasName);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static DialogResult AliasName(string sTitle, string sPromptText, string sPromptText2, int iX, int iY, string sFileName, ref string sAliasName)
        {
            var form = new Form();
            var lblAliasName = new Label();
            var lblFileName = new Label();
            var txtAliasName = new TextBox();
            var txtFileName = new TextBox();
            var btnOk = new C1.Win.C1Input.C1Button();
            var btnCancel = new C1.Win.C1Input.C1Button();

            form.Text = sTitle;
            form.ClientSize = new Size(400, 215);
            form.Controls.AddRange(new Control[] { lblAliasName, lblFileName, txtAliasName, txtFileName, btnOk, btnCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);

            lblAliasName.Text = sPromptText;
            txtAliasName.Text = string.Empty;

            lblFileName.Text = sPromptText2;
            txtFileName.Text = sFileName;
            txtFileName.ReadOnly = true;

            var sName = LocalizationHelper.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");

            btnOk.Text = sName;
            sName = LocalizationHelper.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");
            btnCancel.Text = sName;

            btnOk.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;

            lblAliasName.SetBounds(14, 18, 372, 13);
            lblFileName.SetBounds(14, 85, 372, 13);
            lblAliasName.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Bold, GraphicsUnit.Point, 136);
            lblFileName.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Bold, GraphicsUnit.Point, 136);
            btnOk.SetBounds(215, 160, 75, 37);
            btnCancel.SetBounds(304, 160, 75, 37);

            lblAliasName.AutoSize = true;
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            txtAliasName.SetBounds(16, 39, 150, 20);
            txtFileName.SetBounds(16, 106, 362, 20);

            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(iX - 62, iY - 62);

            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            var dialogResult = form.ShowDialog();

            sAliasName = txtAliasName.Text.Trim();
            return dialogResult;
        }

        private void RemoveFromMyFavoriteFiles(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            RemoveMyFavoriteFiles(TextHelper.GetSafeString(mnuItem.Tag));
        }

        private static void OpenFolder(object sender, EventArgs e) //20191003
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            var sFileName = TextHelper.GetSafeString(mnuItem.Tag);

            //去掉最前面的 * 符號
            sFileName = sFileName.TrimStart('*');

            sFileName = !File.Exists(sFileName) ? Path.GetDirectoryName(sFileName) : $"/select, \"{sFileName}\"";
            Process.Start("explorer.exe", sFileName); //自動移至指定的路徑下的指定檔案
        }

        private static void CopyFullFilePath(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mnuItem)
            {
                Clipboard.SetText(TextHelper.GetSafeString(mnuItem.Tag));
            }
        }

        private static void CopyFileName(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mnuItem)
            {
                Clipboard.SetText(Path.GetFileName(TextHelper.GetSafeString(mnuItem.Tag)));
            }
        }

        private static void CopyCurrentPath(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem mnuItem)
            {
                var sText = TextHelper.GetSafeString(mnuItem.Tag);

                //20240222 複製時，去掉前面的 * 符號
                sText = sText.TrimStart('*');

                Clipboard.SetText(Path.GetDirectoryName(sText));
            }
        }

        private void MyFavoriteFilesClick(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            try
            {
                if (TextHelper.GetSafeString(mnuItem.Tag) == "Empty My Favorite")
                {
                    var message = LocalizationHelper.GetLanguageString("Are you sure you want to empty \"My Favorite\" ?", "form", GetType().Name, "msg", "EmptyMyFavorite", "Text");

                    if (MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    SaveMyFavoriteFiles(TextHelper.GetSafeString(mnuItem.Tag), false, true);
                    LoadMyFavoriteFiles();
                }
                else
                {
                    SaveMyFavoriteFiles(TextHelper.GetSafeString(mnuItem.Tag), true);
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void LoadMyFavoriteFiles()
        {
            var i = 0;

            _mruList.Clear();
            mnuMyFavorite.DropDownItems.Clear();

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT AttributeText, AttributeText2");
            sbSql.AppendLine("  FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'MyFavoriteFiles'");
            sbSql.Append(" ORDER BY AttributeDate DESC");

            var sql = sbSql.ToString();
            var dtMyFavoriteFiles = JasonQueryRepository.ExecQuery(sql);
            var iNum = MyLibrary.MyFavoriteQty - 1; //從 0 開始，要減 1

            foreach (DataRow dr in dtMyFavoriteFiles?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                if (i <= iNum)
                {
                    i++;

                    var sAttributeText = dr.GetSafeString("AttributeText");
                    var sAttributeText2 = dr.GetSafeString("AttributeText2");

                    sAttributeText2 = string.IsNullOrEmpty(sAttributeText2) ? string.Empty : $"```{sAttributeText2}";
                    _mruList.Enqueue($"{sAttributeText}{sAttributeText2}");
                }
                else
                {
                    break;
                }
            }

            var k = 0;

            if (i == 0)
            {
                mnuMyFavorite.Enabled = false;
            }
            else
            {
                mnuMyFavorite.Enabled = true;

                _languageText = LocalizationHelper.GetLanguageString("Empty My Favorite", "form", GetType().Name, "menu", "EmptyMyFavorite", "Text");
                mnuMyFavorite.DropDownItems.Add(_languageText);
                mnuMyFavorite.DropDownItems[k].Tag = "Empty My Favorite";
                mnuMyFavorite.DropDownItems[k].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Delete 16x16.ico");
                mnuMyFavorite.DropDownItems[k].Click += MyFavoriteFilesClick;
                mnuMyFavorite.DropDownItems.Add("-");

                k = 2; //因為前面有加上一個 "Manage My Favorite" and "-"，所以這裡要改成從 2 開始，否則不會觸發 menu event

                foreach (var fileMyFavoriteFiles in _mruList.Select(item => new ToolStripMenuItem(item)))
                {
                    var sTemp = fileMyFavoriteFiles.ToString();
                    var sFullFileName = string.Empty;
                    var sAliasName = string.Empty;

                    if (sTemp.IndexOf("```", StringComparison.Ordinal) == -1)
                    {
                        sFullFileName = sTemp;
                    }
                    else
                    {
                        var sInfo = sTemp.Split(new[] { "```" }, StringSplitOptions.None);

                        sFullFileName = sInfo[0];
                        sAliasName = $"({sInfo[1]}) ";
                    }

                    var sFileName = $"{sAliasName}{Path.GetFileName(sFullFileName)}";

                    //獨立下拉功能表
                    mnuMyFavorite.DropDownItems.Add($"{k - 1}: {sFileName}");
                    mnuMyFavorite.DropDownItems[k].ToolTipText = sFullFileName;
                    mnuMyFavorite.DropDownItems[k].Tag = sFullFileName;
                    mnuMyFavorite.DropDownItems[k].Click += MyFavoriteFilesClick;

                    k++;
                }
            }
        }

        private void SaveMyFavoriteFiles(string sFullFileName, bool bInformChildForm = false, bool bEmptyMyFavorite = false, string sAliasName = "")
        {
            string sql;
            var sbSql = new StringBuilder();

            if (bEmptyMyFavorite)
            {
                sbSql.AppendLine("DELETE FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.Append("   AND AttributeKey = 'MyFavoriteFiles'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);

                mnuMyFavorite.DropDownItems.Clear();
                return;
            }

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'MyFavoriteFiles'");
            sbSql.Append($"   AND AttributeText = '{sFullFileName.Replace("'", "''")}'");

            sql = sbSql.ToString();

            var dtMyFavoriteFiles = JasonQueryRepository.ExecQuery(sql);

            if (dtMyFavoriteFiles?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'MyFavoriteFiles'");
                sbSql.AppendLine($"   AND AttributeText = '{sFullFileName.Replace("'", "''")}'");
                sbSql.Append($"   AND AttributeText2 = '{sAliasName}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeText, AttributeText2, AttributeDate)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'MyFavoriteFiles', '{sFullFileName.Replace("'", "''")}', '{sAliasName}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }

            //Reload
            LoadMyFavoriteFiles();

            //是否需要傳訊息給子表單
            if (!bInformChildForm)
            {
                return;
            }

            if (CheckTabNameExist(sFullFileName))
            {
                return;
            }

            var sTabName = CheckTabNameExist();

            CreateNewTab("Query2", sTabName, sFullFileName); //開啟我的最愛裡面的文件
            MyGlobal.CancelOpenAndCloseTab = sTabName;
        }

        private void RemoveMyFavoriteFiles(string sPath)
        {
            try
            {
                var sbSql = new StringBuilder();

                sPath = sPath.Replace("'", "''");
                sbSql.AppendLine("DELETE FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'MyFavoriteFiles'");
                sbSql.Append($"   AND AttributeText = '{sPath}'");

                var sql = sbSql.ToString();

                JasonQueryRepository.ExecNonQuery(sql);

                //Reload
                LoadMyFavoriteFiles();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tmrTimeAndKeyStatus_Tick(object sender, EventArgs e)
        {
            var sTemp = string.Empty;

            if (MyGlobal.GlobalTemp3.StartsWith("SQLServerSwitchDatabaseByUsing`", StringComparison.Ordinal)) //透過 USE 指令切換資料庫
            {
                var sDatabase = MyGlobal.GlobalTemp3.Replace("SQLServerSwitchDatabaseByUsing`", string.Empty);

                MyGlobal.GlobalTemp3 = string.Empty;

                try
                {
                    LoadSqlServerDatabase(sDatabase);
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                return;
            }

            if (MyGlobal.GlobalTemp3.StartsWith("MySQLSwitchDatabaseByUsing`", StringComparison.Ordinal)) //透過 USE 指令切換資料庫
            {
                var sDatabase = MyGlobal.GlobalTemp3.Replace("MySQLSwitchDatabaseByUsing`", string.Empty);

                MyGlobal.GlobalTemp3 = string.Empty;

                try
                {
                    LoadMySqlDatabase(sDatabase);
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                return;
            }

            if (MyGlobal.GlobalExecuteCommitRollback.StartsWith("ExecuteCommitRollback`", StringComparison.Ordinal)) //SchemaBrowser 傳過來的訊息
            {
                var sb = new StringBuilder();

                MyGlobal.GlobalExecuteCommitRollback = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var sTitle = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    //20241017 略過 'CreateTable'
                    if (!SpecialTabName.Contains(sTitle))
                    {
                        sb.Append(tabAccessibleDescription).Append(";");
                    }

                    //20231004 Schema Brower 允許多開/編輯，如果是 'Schema Browser' 也要處理
                    if (sTitle == MyGlobal.SchemaBrowserTabName)
                    {
                        sb.Append(tabAccessibleDescription).Append(";");
                    }
                }

                MyGlobal.InfoFromMDIForm = $"ExecuteCommitCommitRollback`{sb}";
                return;
            }

            if (MyGlobal.GlobalTemp5.StartsWith("UpdateCommitRollbackButton`", StringComparison.Ordinal)) //20231004 透過獨立開啟的 SchemaBrowser 傳過來的訊息
            {
                var sb = new StringBuilder();

                MyGlobal.GlobalTemp5 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var sTitle = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    //20241017 略過 'CreateTable'
                    if (!SpecialTabName.Contains(sTitle))
                    {
                        sb.Append(tabAccessibleDescription).Append(";");
                    }

                    //20231004 如果是 'Schema Browser' 也要處理
                    if (sTitle == MyGlobal.SchemaBrowserTabName)
                    {
                        sb.Append(tabAccessibleDescription).Append(";");
                    }
                }

                MyGlobal.InfoFromMDIForm = $"UpdateCommitRollbackButton`{sb}";
                return;
            }

            if (MyGlobal.GlobalTemp5.StartsWith("UpdateSchemaInformation`", StringComparison.Ordinal)) //20250204 更新 QueryForm 的 Schema Info
            {
                var sb2 = new StringBuilder();
                var sb3 = new StringBuilder();

                MyGlobal.GlobalTemp5 = string.Empty;
                MyGlobal.GlobalTemp6 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var sTitle = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    //20241017 略過 'CreateTable'
                    if (!SpecialTabName.Contains(sTitle))
                    {
                        sb2.Append(tabAccessibleDescription).Append(";");
                    }

                    if (sTitle == MyGlobal.SchemaBrowserTabName)
                    {
                        sb3.Append(tabAccessibleDescription).Append(";");
                    }
                }

                //先更新 SchemaBrowserForm (透過 MyGlobal.sGlobalTemp4 變數的值，避免無限迴圈
                if (!string.IsNullOrEmpty(MyGlobal.GlobalTemp4))
                {
                    MyGlobal.GlobalTemp4 = string.Empty;
                    MyGlobal.GlobalTemp6 = sb3.Length == 0 ? string.Empty : $"UpdateSchemaBrowserInformation`{sb3}"; //針對 SchemaBrowserForm
                }

                //再更新 QueryForm
                MyGlobal.GlobalTemp5 = $"UpdateSchemaInformation`{sb2}"; //針對 QueryForm
            }
            else if (MyGlobal.GlobalTemp.StartsWith("PasteFromSchemaBrowser`", StringComparison.Ordinal)) //是否為 Schema Browser 要貼上 SQL
            {
                sTemp = MyGlobal.GlobalTemp.Replace("PasteFromSchemaBrowser`", string.Empty);
                MyGlobal.GlobalTemp = string.Empty;
                PasteFromSchemaBrowser(sTemp);
                return;
            }
            else if (MyGlobal.GlobalTemp == "Rename4SchemaBrowser") //是否要修正 Schema Browser's Tab Name?
            {
                MyGlobal.GlobalTemp = string.Empty;

                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var sTitle = theTab.Title;
                    var tabAccessibleDescription = theTab.AccessibleDescription;

                    if (tabAccessibleDescription == MyGlobal.FormSchemaBrowserKey)
                    {
                        theTab.Title = MyGlobal.SchemaBrowserTabName;
                    }

                    if (sTitle == "*SQL Editor")
                    {
                        theTab.Selected = true;
                    }
                }

                return;
            }
            else if (MyGlobal.GlobalTemp.StartsWith("RenameTabMain`", StringComparison.Ordinal)) //20241217 變更頁籤名稱
            {
                var temp = MyGlobal.GlobalTemp.Replace("RenameTabMain`", string.Empty);

                MyGlobal.GlobalTemp = string.Empty;

                var parts = temp.Split(new[] { MyGlobal.Separator }, StringSplitOptions.None);
                var accessibleDescription = parts[0];
                var tabTitle = parts[1];

                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var tabAccessibleDescription = theTab.AccessibleDescription;

                    if (tabAccessibleDescription == accessibleDescription)
                    {
                        theTab.Title = tabTitle;

                        MyGlobal.GlobalTemp = $"RenameTab`{accessibleDescription}{MyGlobal.Separator}{tabTitle}"; //變更 Query Form 的頁籤名稱
                        break;
                    }
                }

                return;
            }
            else if (MyGlobal.GlobalTemp == "RenameTabUpdateTabList") //20241217 更新 Tab List
            {
                MyGlobal.GlobalTemp = string.Empty;
                UpdateTabList();

                return;
            }

            //以下程式碼，可以判斷本程式是否在作用中，並只「呼叫」一次！
            if (!MyGlobal.IsContainsFocusFormOptionsKey && ContainsFocus)
            {
                var sb = new StringBuilder();

                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var sTitle = theTab.Title;
                    var tabAccessibleDescription = theTab.AccessibleDescription;

                    if (SpecialTabName.Contains(sTitle))
                    {
                        continue;
                    }

                    //20190909 此處改成「從其他程式切換至 JasonQuery 時，只針對 Focused Tab 處理」，因為同時處理多個，可能會有 BUG
                    if (!theTab.Selected)
                    {
                        continue;
                    }

                    sb.Append(tabAccessibleDescription).Append(";");
                    break;
                }

                MyGlobal.CheckFileFromMDIForm = $"`{sb}";
            }

            MyGlobal.IsContainsFocusFormOptionsKey = ContainsFocus;
        }

        private void PasteFromSchemaBrowser(string sInfo)
        {
            var bExistSqlEditor = false;

            for (var i = 0; i < tabControl1.TabPages.Count; i++)
            {
                //檢查是否有 'SQL Editor'？若存在，則傳遞 SQL Statement；若不存在，則建立 Tab 後再傳遞 SQL Statement
                if (tabControl1.TabPages[i].Title != "*SQL Editor")
                {
                    continue;
                }

                bExistSqlEditor = true;

                var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                //已存在，要透過全域變數傳值
                MyGlobal.InfoFromMDIForm = $"TransferSelectSQL`{tabAccessibleDescription};{sInfo}";
                break;
            }

            if (!bExistSqlEditor)
            {
                CreateNewTab("SQLEditor", "*SQL Editor", sInfo); //開啟 SQL Editor (從 Schema Browser 傳過來的 SQL 語句)
            }
        }

        internal void ReceiveValueFromChildForm(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            ValueUpdated(this, new ValueUpdatedEventArgs(value));
        }

        private void ValueUpdated(object sender, ValueUpdatedEventArgs e)
        {
            var newValue = e.NewValue;

            if (newValue.StartsWith("SQLServerSwitchOKDatabaseByUseCommand`", StringComparison.Ordinal)) //20220818 使用者透過 USE 指令 切換資料庫，清空其他頁籤的訊息、並更新 Editor's SchemaBrowser 資訊
            {
                var info = newValue.Replace("SQLServerSwitchOKDatabaseByUseCommand`", string.Empty);

                var temp = tabControl1.TabPages.Cast<TabPage>()
                                               .Where
                                                (
                                                    tab => !SpecialTabName.Contains(tab.Title) && tab.AccessibleDescription != info
                                                )
                                               .Aggregate
                                                (
                                                    string.Empty, (current, tab) => $"{current}{tab.AccessibleDescription}`"
                                                );

                if (string.IsNullOrEmpty(temp))
                {
                    return;
                }

                MyGlobal.GlobalTemp4 = $"SQLServerSwitchOKDatabaseFromMainForm{MyGlobal.Separator}{temp}"; //清空其他頁籤的訊息
                MyGlobal.GlobalTemp5 = $"ReloadSchemaInfo`{temp.Replace("`", ";")}"; //更新 Editor's SchemaBrowser 資訊
            }
            else if (newValue.StartsWith("MySQLSwitchOKDatabaseByUseCommand`", StringComparison.Ordinal)) //20220818 使用者透過 USE 指令 切換資料庫，清空其他頁籤的訊息、並更新 Editor's SchemaBrowser 資訊
            {
                var info = newValue.Replace("MySQLSwitchOKDatabaseByUseCommand`", string.Empty);

                var temp = tabControl1.TabPages.Cast<TabPage>()
                                               .Where
                                                (
                                                    tab => !SpecialTabName.Contains(tab.Title) && tab.AccessibleDescription != info
                                                )
                                               .Aggregate
                                                (
                                                    string.Empty, (current, tab) => $"{current}{tab.AccessibleDescription}`"
                                                );

                if (string.IsNullOrEmpty(temp))
                {
                    return;
                }

                MyGlobal.GlobalTemp4 = $"MySQLSwitchOKDatabaseFromMainForm{MyGlobal.Separator}{temp}"; //清空其他頁籤的訊息
                MyGlobal.GlobalTemp5 = $"ReloadSchemaInfo`{temp.Replace("`", ";")}"; //更新 Editor's SchemaBrowser 資訊
            }
            else if (newValue.StartsWith("P`", StringComparison.OrdinalIgnoreCase)) //查詢完畢，從子表單傳過來的訊息
            {
                lblInfo.Text = newValue.Substring(2);
            }
            else if (newValue.StartsWith("CloseQueryForm`", StringComparison.Ordinal))
            {
                var info = newValue.Substring("CloseQueryForm`".Length);

                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var tabAccessibleDescription = theTab.AccessibleDescription;

                    if (tabAccessibleDescription != info)
                    {
                        continue;
                    }

                    tabControl1.TabPages.Remove(theTab);
                    UpdateMainMenuTitle();
                    break;
                }
            }
            else if (newValue.StartsWith("CloseOptionsTab`", StringComparison.Ordinal))
            {
                var bValue = true;
                var tabIndex = newValue.Replace("CloseOptionsTab`", string.Empty);
                var tabTitle = tabControl1.SelectedTab.Title;

                if (string.IsNullOrEmpty(tabIndex) && tabTitle != MyGlobal.OptionsTabName)
                {
                    //選定的頁籤不是「選項」，也沒有帶入 Tab Index：不處理
                    bValue = false;
                }

                if (!bValue)
                {
                    return;
                }

                int.TryParse(tabIndex, out var tabIndexValue);

                try
                {
                    if (tabTitle == MyGlobal.OptionsTabName)
                    {
                        tabControl1.SelectedTab.Dispose();
                        tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                    }
                    else
                    {
                        tabControl1.TabPages[tabIndexValue].Dispose();
                        tabControl1.TabPages.Remove(tabControl1.TabPages[tabIndexValue]);
                    }

                    mnuOptions.Enabled = true;
                    mnuOptions.Checked = false;

                    MyGlobal.ClearMemory();
                    UpdateMainMenuTitle();

                    if (tabControl1.TabPages.Count == 0)
                    {
                        //20191013 如果把最後一個 Tab 關閉，自動再開啟一個空白的 SQL Editor
                        CreateNewTab("Query", CheckTabNameExist()); //關閉選項頁籤後，已沒有任何一個頁籤了，再開啟一個空白的 SQL Editor
                    }
                    else
                    {
                        UpdateTabList();
                    }
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else if (newValue.StartsWith("CloseEmptyTab`", StringComparison.Ordinal))
            {
                //20191014 如果開啟檔案失敗，或是使用者取消開啟檔案，把空白的 Tab 關閉
                if (tabControl1.SelectedTab.Title != MyGlobal.CancelOpenAndCloseTab)
                {
                    return;
                }

                MyGlobal.CancelOpenAndCloseTab = string.Empty;
                tabControl1.SelectedTab.Dispose();
                tabControl1.TabPages.Remove(tabControl1.SelectedTab);

                if (tabControl1.TabPages.Count == 0)
                {
                    CreateNewTab("Query", CheckTabNameExist()); //已沒有任何一個頁籤了，再開啟一個空白的 SQL Editor
                }
            }
            else if (newValue.StartsWith("RemoveFromMyFavoriteLists`", StringComparison.Ordinal))
            {
                RemoveMyFavoriteFiles(newValue.Substring(26)); //確認要刪除 My Favorite List - Query Form 再透過 MainForm
            }
            else if (newValue.StartsWith("RemoveFromRecentFiles`", StringComparison.Ordinal))
            {
                RemoveRecentFiles(newValue.Substring(22));
            }
            else if (newValue.StartsWith("CreateNewTab`", StringComparison.Ordinal))
            {
                if (newValue == "CreateNewTab`") //單純地開啟一個空白的 SQL Editor
                {
                    using (TraceLogger.Time("Update Schema Data"))
                    {
                        CreateNewTab("Query", CheckTabNameExist());
                        MyGlobal.ClearMemory();
                    }
                }
                else
                {
                    //20191005 傳入要開啟的檔名
                    var info = newValue.Substring(13);

                    if (string.Equals(info, "OPENFILE", StringComparison.OrdinalIgnoreCase))
                    {
                        OpenFile();
                    }
                    else if (info.Length > 8 && info.StartsWith("OPENFILE", StringComparison.OrdinalIgnoreCase))
                    {
                        info = info.Substring(9);
                        SaveRecentList(info, true);
                    }
                }
            }
            else if (newValue.StartsWith("CheckExistTab`", StringComparison.Ordinal))
            {
                //20190930 改用 full path + filename 判斷是否已存在，因為有可能是同檔名但不同路徑
                MyGlobal.CheckExistTabResult = !CheckTabNameExist(newValue.Substring(14)) ? "FALSE" : "TRUE";
            }
            else if (newValue.StartsWith("UpdateTabInfo`", StringComparison.Ordinal)) //更新 Tab 資訊
            {
                UpdateTabInfo(newValue.Substring(14));
            }
            else if (newValue.StartsWith("UpdateDatabaseInfo`", StringComparison.Ordinal)) //更新 Database 資訊
            {
                var info = newValue.Substring(19);

                if (string.IsNullOrEmpty(info))
                {
                    return;
                }

                btnDatabase.Visible = true;
                btnDatabase.Text = info;
                spDatabase.Visible = true;
                DatabaseSqlExecutor.DatabaseName = info;
            }
            else if (newValue.StartsWith("UpdateCanUndo`", StringComparison.Ordinal))
            {
                UpdateTabInfo(newValue.Substring(14));
            }
            else if (newValue.StartsWith("UpdateRecentFiles`", StringComparison.Ordinal)) //更新 Recent Files 清單
            {
                SaveRecentList(newValue.Substring(18));
            }
            else if (newValue.StartsWith("ReloadQueryEditorSetting`", StringComparison.Ordinal)) //重新載入 Query Editor 的設定值
            {
                var sbInfo = new StringBuilder();

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //不用略過 'Schema Browser' & 'SQL History'，因為這兩個也要套用！
                    if (title != MyGlobal.OptionsTabName)
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.GlobalTemp5 = $"ReloadQueryEditorSetting`{info}";
                }
                else
                {
                    MyGlobal.GlobalTemp5 = string.Empty;
                }
            }
            else if (newValue.StartsWith("ExecuteCommitRollback`", StringComparison.Ordinal)) //執行 Commit / Rollback 指令：設定按鈕狀態
            {
                SetPendingTransactionStatus(false);
                BroadcastCommitRollbackButtonState(false);
            }
            else if (newValue.StartsWith("UpdateCommitRollbackButton`", StringComparison.Ordinal)) //執行 nonquery 指令：更新按鈕狀態
            {
                SetPendingTransactionStatus(true);
                BroadcastCommitRollbackButtonState(true);
            }
            else if (newValue.StartsWith("DisconnectAfterQueryOnly`", StringComparison.Ordinal)) //單純執行 query 指令，且不需要等待 Commit / Rollback：中斷連線
            {
                if (!AppConfigHelper.IsNotCommitYet)
                {
                    DisconnectDatabase();
                }
            }
            else if (newValue.StartsWith("DisconnectAfterExecuteError`", StringComparison.Ordinal)) //執行指令有錯誤：中斷連線
            {
                var sbInfo = new StringBuilder();

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    if (!SpecialTabName.Contains(title))
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.InfoFromMDIForm = $"DisconnectAfterExecuteError`{info}";
                }
                else
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (newValue.StartsWith("ReloadLocalization`", StringComparison.Ordinal)) //重新載入 Localization 的設定值
            {
                try
                {
                    LoadGlobalSetting(true);
                    LocalizationHelper.LoadLocalizationXML();
                    ApplyLocalization();
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (newValue.Length > 25)
                {
                    _connectionFormChangeLocalization++;

                    try
                    {
                        LoadConnectionForm();
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    try
                    {
                        LoadDefaultSetting();
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    var sbInfo = new StringBuilder();

                    try
                    {
                        for (var i = 0; i < tabControl1.TabPages.Count; i++)
                        {
                            var title = tabControl1.TabPages[i].Title;
                            var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                            sbInfo.Append($"{tabAccessibleDescription};");

                            if (title == MyGlobal.OptionsTabName_Before)
                            {
                                tabControl1.TabPages[i].Title = MyGlobal.OptionsTabName;
                                tabControl1.TabPages[i].Tag = MyGlobal.OptionsTabName;
                                MyGlobal.OptionsTabName_Before = MyGlobal.OptionsTabName;
                            }
                            else if (title == MyGlobal.SchemaBrowserTabName_Before)
                            {
                                tabControl1.TabPages[i].Title = MyGlobal.SchemaBrowserTabName;
                                tabControl1.TabPages[i].Tag = MyGlobal.SchemaBrowserTabName;

                                var schemaBrowserTabCount = tabControl1.TabPages.Cast<TabPage>().Count(theTab => theTab.Title == MyGlobal.SchemaBrowserTabName_Before);

                                if (schemaBrowserTabCount == 0)
                                {
                                    //20231011 最後一個 SchemaBrowser 才要變更此變數的值！
                                    MyGlobal.SchemaBrowserTabName_Before = MyGlobal.SchemaBrowserTabName;
                                }
                            }
                            else if (title == MyGlobal.SqlHistoryTabName_Before)
                            {
                                tabControl1.TabPages[i].Title = MyGlobal.SqlHistoryTabName;
                                tabControl1.TabPages[i].Tag = MyGlobal.SqlHistoryTabName;
                                MyGlobal.SqlHistoryTabName_Before = MyGlobal.SqlHistoryTabName;
                            }
                            else if (title == MyGlobal.CreateTableTabName_Before)
                            {
                                tabControl1.TabPages[i].Title = MyGlobal.CreateTableTabName;
                                tabControl1.TabPages[i].Tag = MyGlobal.CreateTableTabName;
                                MyGlobal.CreateTableTabName_Before = MyGlobal.CreateTableTabName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                        MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }

                    var info = sbInfo.ToString();

                    if (!string.IsNullOrWhiteSpace(info))
                    {
                        MyGlobal.InfoFromReloadLocalization = $"ReloadLocalization`{info}";
                    }
                    else
                    {
                        MyGlobal.InfoFromReloadLocalization = string.Empty;
                    }
                }

                try
                {
                    //20220826 這裡要再重新載入一次，要把已切換的資料庫的 MenuItem 變成 Disable
                    switch (_currentSourceType)
                    {
                        case DataSourceType.Oracle:
                            {
                                break;
                            }
                        case DataSourceType.PostgreSql:
                            {
                                LoadPostgreSqlDatabase();
                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                LoadSqlServerDatabase();
                                break;
                            }
                        case DataSourceType.MySql:
                            {
                                LoadMySqlDatabase();
                                break;
                            }
                        case DataSourceType.None:
                            {
                                break;
                            }
                    }

                    //這裡要再針對頁籤色彩調整一次
                    tabControl1.BackColor = ColorTranslator.FromHtml(MyGlobal.TabBackColor);
                    tabControl1.ForeColor = ColorTranslator.FromHtml(MyGlobal.TabActiveForeColor);
                    tabControl1.TextInactiveColor = ColorTranslator.FromHtml(MyGlobal.TabInactiveForeColor);
                }
                catch (Exception ex)
                {
                    var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else if (newValue.StartsWith("TransferSelectSQL`", StringComparison.Ordinal)) //傳遞 SQL (建立新的 Tab)
            {
                var info = newValue.Split('`')[1];

                PasteFromSchemaBrowser(info);
            }
            else if (newValue.StartsWith("UpdateSchemaInformation`", StringComparison.Ordinal)) //20241013
            {
                var sbInfo = new StringBuilder();
                var temp = newValue.Replace("UpdateSchemaInformation`", string.Empty); //忽略呼叫方

                MyGlobal.GlobalTemp5 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    if (!SpecialTabName.Contains(title) && temp != tabAccessibleDescription)
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.InfoFromMDIForm = $"UpdateSchemaInformation`{info}";
                }
                else
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (newValue.StartsWith("UpdateSchemaInformationRefreshClick`", StringComparison.Ordinal)) //20241011 在 QueryForm 按下 Refresh 更新 Schema 資訊
            {
                var sbInfo = new StringBuilder();
                var temp = newValue.Replace("UpdateSchemaInformationRefreshClick`", string.Empty); //忽略呼叫方

                MyGlobal.GlobalTemp5 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //略過 'Options' & 'Schema Browser' & 'SQL History'
                    if (!SpecialTabName.Contains(title) && temp != tabAccessibleDescription)
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.InfoFromMDIForm = $"UpdateSchemaInformationRefreshClick`{info}";
                }
                else
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (newValue.StartsWith("UpdateSchemaBrowserInformation`", StringComparison.Ordinal)) //20241013
            {
                var sbInfo = new StringBuilder();
                var temp = newValue.Replace("UpdateSchemaBrowserInformation`", string.Empty); //忽略呼叫方

                MyGlobal.GlobalTemp5 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //忽略呼叫方
                    if (title == MyGlobal.SchemaBrowserTabName && temp != tabAccessibleDescription)
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.InfoFromMDIForm = $"UpdateSchemaBrowserInformation`{info}";
                }
                else
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (newValue.StartsWith("UpdateSchemaBrowserInformationRefreshClick`", StringComparison.Ordinal)) //20241011 按下 Refresh 更新 SchemaBrowser 資訊
            {
                var sbInfo = new StringBuilder();
                var temp = newValue.Replace("UpdateSchemaBrowserInformationRefreshClick`", string.Empty); //忽略呼叫方

                MyGlobal.GlobalTemp5 = string.Empty;

                for (var i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    var title = tabControl1.TabPages[i].Title;
                    var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                    //忽略呼叫方
                    if (title == MyGlobal.SchemaBrowserTabName && temp != tabAccessibleDescription)
                    {
                        sbInfo.Append($"{tabAccessibleDescription};");
                    }
                }

                var info = sbInfo.ToString();

                if (!string.IsNullOrWhiteSpace(info))
                {
                    MyGlobal.InfoFromMDIForm = $"UpdateSchemaBrowserInformationRefreshClick`{info}";
                }
                else
                {
                    MyGlobal.InfoFromMDIForm = string.Empty;
                }
            }
            else if (newValue.StartsWith("DoubleClickToSwitchTab`", StringComparison.Ordinal)) //20241208 切換至指定的頁籤
            {
                var accessibleDescription = newValue.Replace("DoubleClickToSwitchTab`", string.Empty);

                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    if (theTab.AccessibleDescription == accessibleDescription)
                    {
                        theTab.Selected = true;
                        break;
                    }
                }
            }
        }

        private void BroadcastCommitRollbackButtonState(bool enabled)
        {
            var descriptions = GetCommitRollbackButtonBroadcastAccessibleDescriptions();

            if (descriptions.Count == 0)
            {
                return;
            }

            var command = enabled ? "UpdateCommitRollbackButton`" : "ExecuteCommitRollback`";

            MyGlobal.InfoFromMDIForm = $"{command}{string.Join(";", descriptions)};";
        }

        private List<string> GetCommitRollbackButtonBroadcastAccessibleDescriptions()
        {
            return tabControl1.TabPages.Cast<TabPage>()
                                       .Where(ShouldBroadcastCommitRollbackButtonState)
                                       .Select(tab => tab.AccessibleDescription)
                                       .Where(description => !string.IsNullOrWhiteSpace(description))
                                       .Distinct()
                                       .ToList();
        }

        private bool ShouldBroadcastCommitRollbackButtonState(TabPage tab)
        {
            if (tab == null)
            {
                return false;
            }

            if (!SpecialTabName.Contains(tab.Title))
            {
                return true; //一般 QueryForm
            }

            return string.Equals(tab.Title, MyGlobal.SchemaBrowserTabName, StringComparison.Ordinal);
        }

        private List<string> GetNormalQueryFormAccessibleDescriptions()
        {
            return tabControl1.TabPages.Cast<TabPage>()
                                       .Where(tab => !SpecialTabName.Contains(tab.Title))
                                       .Select(tab => tab.AccessibleDescription)
                                       .Where(description => !string.IsNullOrWhiteSpace(description))
                                       .Distinct()
                                       .ToList();
        }

        private void UpdateTabInfo(string value)
        {
            var tabIndex = 0;
            var neeUpdate = true;
            var tabTitle = string.Empty;
            var temp = string.Empty;

            for (var i = 0; i < tabControl1.TabPages.Count; i++)
            {
                if (!tabControl1.TabPages[i].Selected)
                {
                    continue;
                }

                var idx = value.IndexOf('`');
                var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                if (idx > 0 && idx == value.LastIndexOf('`'))
                {
                    temp = value.Substring(0, idx); //傳過來的 AccessibleDescription
                    value = value.Substring(idx + 1);
                }

                //如果有指定 Tab 代號，比對是否吻合，避免改錯
                if (!string.IsNullOrEmpty(temp) && temp != tabAccessibleDescription)
                {
                    neeUpdate = false;
                }

                tabTitle = Path.GetFileName(value);

                if (value.StartsWith("*", StringComparison.Ordinal) && !tabTitle.StartsWith("*", StringComparison.Ordinal))
                {
                    tabTitle = $"*{tabTitle}";
                }

                tabIndex = i;
                break;
            }

            if (!neeUpdate)
            {
                return;
            }

            if (value.StartsWith("*", StringComparison.Ordinal) && tabTitle.StartsWith("*", StringComparison.Ordinal))
            {
                tabControl1.TabPages[tabIndex].Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL Editor Red 16x16.ico");
            }
            else
            {
                tabControl1.TabPages[tabIndex].Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL Editor Blue 16x16.ico");
            }

            tabControl1.TabPages[tabIndex].Tag = value;
            tabControl1.TabPages[tabIndex].Title = tabTitle;
            _toolTip1.SetToolTip(tabControl1, value);
        }

        private void tabControl1_MouseMove(object sender, MouseEventArgs e)
        {
            var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
            var tabPagesCount = tabControl1.TabPages.Count;

            for (var i = 0; i < tabPagesCount; i++)
            {
                var tabRect = tabControl1.GetTabRect(i);

                if (!tabRect.IntersectsWith(mouseRect))
                {
                    continue;
                }

                var tag = TextHelper.GetSafeString(tabControl1.TabPages[i].Tag);

                //Tab 名稱有異動，才要再次顯示 ToolTip (否則會有閃爍的問題)
                if (!string.IsNullOrEmpty(_tabToolTip) && _tabToolTip == tag)
                {
                    continue;
                }

                _tabToolTip = tag;
                _toolTip1.SetToolTip(tabControl1, tag);
                break;
            }
        }

        private void tabControl1_SelectionChanged(object sender, EventArgs e)
        {
            var info = string.Empty;
            var tabPagesCount = tabControl1.TabPages.Count;

            for (var i = 0; i < tabPagesCount; i++)
            {
                if (!tabControl1.TabPages[i].Selected)
                {
                    continue;
                }

                info = TextHelper.GetSafeString(tabControl1.TabPages[i].Tag);
                break;
            }

            _toolTip1.SetToolTip(tabControl1, info);
        }

        private void btnNewSqlEditor_Click(object sender, EventArgs e)
        {
            CreateNewTab("Query", CheckTabNameExist()); //開新檔案的按鈕：開啟一個空白的 SQL Editor
        }

        private void LoadRecentList()
        {
            var i = 0;

            _mruList.Clear();
            mnuRecentFiles.DropDownItems.Clear();

            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeText FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'RecentFile'");
                sbSql.Append(" ORDER BY AttributeDate DESC");

                var sql = sbSql.ToString();
                var dtRecent = JasonQueryRepository.ExecQuery(sql);
                var favoriteQty = MyLibrary.MyFavoriteQty - 1; //從 0 開始，要減 1

                foreach (DataRow dr in dtRecent?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    if (i <= favoriteQty) //從 0 開始，要減 1
                    {
                        var attributeText = dr.GetSafeString("AttributeText");

                        i++;
                        _mruList.Enqueue(attributeText);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            var k = 0;

            if (i == 0)
            {
                mnuRecentFiles.Enabled = false;
            }
            else
            {
                mnuRecentFiles.Enabled = true;

                _languageText = LocalizationHelper.GetLanguageString("Empty Recent Files", "form", GetType().Name, "menu", "EmptyRecentFiles", "Text");
                mnuRecentFiles.DropDownItems.Add(_languageText);
                mnuRecentFiles.DropDownItems[k].Tag = "Empty Recent Files";

                mnuRecentFiles.DropDownItems[k].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Delete 16x16.ico");
                mnuRecentFiles.DropDownItems[k].Click += new EventHandler(RecentFileClick);
                mnuRecentFiles.DropDownItems.Add("-");

                k = 2; //因為前面有加上一個 "Empty Recent Files List" and "-"，所以這裡要改成從 2 開始，否則不會觸發 menu event

                foreach (var fileRecent in _mruList.Select(item => new ToolStripMenuItem(item)))
                {
                    //獨立下拉功能表
                    mnuRecentFiles.DropDownItems.Add($"{k - 1}: {Path.GetFileName(fileRecent.ToString())}"); //add the menu to "recent" menu
                    mnuRecentFiles.DropDownItems[k].ToolTipText = fileRecent.ToString();
                    mnuRecentFiles.DropDownItems[k].Tag = fileRecent.ToString();
                    mnuRecentFiles.DropDownItems[k].Click += RecentFileClick;

                    k++;
                }
            }
        }

        private void SaveRecentList(string path, bool needInformChildForm = false, bool shouldEmptyRecentList = false)
        {
            string sql;
            var sbSql = new StringBuilder();

            try
            {
                if (shouldEmptyRecentList)
                {
                    sbSql.AppendLine("DELETE FROM SystemConfig");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.Append("   AND AttributeKey = 'RecentFile'");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);

                    mnuRecentFiles.DropDownItems.Clear();
                    return;
                }

                path = path.Replace("'", "''");
                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'RecentFile'");
                sbSql.Append($"   AND AttributeText = '{path}'");

                sql = sbSql.ToString();

                var dtRecent = JasonQueryRepository.ExecQuery(sql);

                if (dtRecent?.Rows.Count > 0)
                {
                    sbSql.Clear();
                    sbSql.AppendLine("UPDATE SystemConfig");
                    sbSql.AppendLine($"   SET AttributeDate = '{MyGlobal.DateTimeNow()}'");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.AppendLine("   AND AttributeKey = 'RecentFile'");
                    sbSql.Append($"   AND AttributeText = '{path}'");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }
                else
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeText, AttributeDate)");
                    sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'RecentFile', '{path}', '{MyGlobal.DateTimeNow()}')");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                }

                //Reload
                LoadRecentList();

                //是否需要傳訊息給子表單
                if (!needInformChildForm)
                {
                    return;
                }

                if (CheckTabNameExist(path))
                {
                    return;
                }

                var tabName = CheckTabNameExist();

                CreateNewTab("Query1", tabName, path);
                MyGlobal.CancelOpenAndCloseTab = tabName;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void RemoveRecentFiles(string path)
        {
            try
            {
                var sbSql = new StringBuilder();

                path = path.Replace("'", "''");
                sbSql.AppendLine("DELETE FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'RecentFile'");
                sbSql.Append($"   AND AttributeText = '{path}'");

                var sql = sbSql.ToString();

                JasonQueryRepository.ExecNonQuery(sql);
                LoadRecentList();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void RecentFileClick(object sender, EventArgs e)
        {
            var fileName = string.Empty;

            if (sender is ToolStripMenuItem mnuItem)
            {
                fileName = TextHelper.GetSafeString(mnuItem.Tag);
            }

            if (fileName == "Empty Recent Files")
            {
                var message = "Are you sure you want to empty \"Recent Files\" ?";

                message = LocalizationHelper.GetLanguageString(message, "form", GetType().Name, "msg", "EmptyRecentFiles", "Text");

                if (MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                SaveRecentList(fileName, false, true);
                LoadRecentList();
            }
            else
            {
                SaveRecentList(fileName, true);
            }
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(MyGlobal.RequireToRestart))
            {
                MessageBox.Show(MyGlobal.RequireToRestart, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var title = tabControl1.TabPages.Cast<TabPage>().Aggregate(string.Empty, (current, theTab) => current + theTab.Title.Trim().Replace("*", string.Empty) + ";");

            title += MyGlobal.OptionsTabName;
            MyGlobal.GlobalTemp = title;

            if (!CheckTabNameExist(MyGlobal.OptionsTabName))
            {
                CreateNewTab(MyGlobal.OptionsTabName, MyGlobal.OptionsTabName); //從主選單開啟選項頁籤
            }

            //20200825 改成可重複開啟 (第二次開啟，自動切換到該頁籤)
            mnuOptions.Checked = true;
        }

        private void mnuSqlHistory_Click(object sender, EventArgs e)
        {
            if (!CheckTabNameExist(MyGlobal.SqlHistoryTabName))
            {
                CreateNewTab(MyGlobal.SqlHistoryTabName, MyGlobal.SqlHistoryTabName); //從主選單開啟SQL歷史記錄頁籤
            }

            mnuSqlHistory.Checked = true;
        }

        private void mnuSchemaBrowser_Click(object sender, EventArgs e)
        {
            if (IsSqlServer && DatabaseSqlExecutor.DbServerVersion == "2000")
            {
                var temp = LocalizationHelper.GetLanguageString("The function is not yet complete!", "Global", "Global", "msg", "FunctionNotYet", "Text");

                _languageText = $"for SQL Server 2000 (or lower)\r\n{temp}";

                MessageBox.Show(_languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            _toolTip1.AutoPopDelay = 0;

            //20231011 允許重複開啟多個 SchemaBrowser
            AppConfigHelper.HasMultiOpenSchemaBrowser = CheckTabNameExist(MyGlobal.SchemaBrowserTabName);

            CreateNewTab(MyGlobal.SchemaBrowserTabName, MyGlobal.SchemaBrowserTabName); //從主選單開啟架構瀏覽器頁籤

            mnuSchemaBrowser.Checked = true;
            _toolTip1.AutoPopDelay = 5000;
        }

        private void mnuGenerateSqlStatement_Click(object sender, EventArgs e)
        {
            using (var form = new GenerateSqlForm())
            {
                form.ShowDialog();
            }
        }

        private void mnuSchemaSearch_Click(object sender, EventArgs e)
        {
            //using (var form = new SchemaSearchForm())
            //{
            //    var (formWidth, formHeight) = TextHelper.GetFormDimensionSettings(MyGlobal.sDomainUser, "SchemaSearchFormWidth", "SchemaSearchFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

            //    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
            //    form.ShowDialog();
            //}
        }

        private void mnuCreateTable_Click(object sender, EventArgs e)
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                case DataSourceType.PostgreSql:
                    {
                        if (!CheckTabNameExist(MyGlobal.CreateTableTabName))
                        {
                            CreateNewTab(MyGlobal.CreateTableTabName, MyGlobal.CreateTableTabName);
                        }

                        mnuCreateTable.Enabled = false;
                        mnuCreateTable.Checked = true;

                        break;
                    }
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        break;
                    }
                case DataSourceType.None:
                    {
                        break;
                    }
            }
        }

        private void mnuImportTableData_Click(object sender, EventArgs e)
        {
            //var form = new OracleImportTableDataWizardForm();

            //form.ShowInTaskbar = false;
            //form.TopLevel = true; //只在 JasonQuery 最上層顯示
            //form.Show(this); //20250227 此處要加上 this，TopLevel 才會有效果
            //form.Refresh();
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            //if (e.Reason == SessionEndReasons.SystemShutdown) //重新開機
            //if (e.Reason == SessionEndReasons.Logoff) //登出或關機

            using (TraceLogger.Time("BeforeCloseApplication - SystemEvents_SessionEnding"))
            {
                if (BeforeCloseApplication())
                {
                    Close();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            if (m.Msg == WM_SYSCOMMAND && (int)m.WParam == SC_CLOSE) //使用者將 MainForm 整個關閉
            {
                using (TraceLogger.Time("BeforeCloseApplication - WndProc"))
                {
                    if (!BeforeCloseApplication())
                    {
                        return;
                    }
                }
            }

            base.WndProc(ref m);
        }

        private bool BeforeCloseApplication()
        {
            var sb = new StringBuilder();

            sb.Append("CloseQueryFormAndCheckCommit`");

            //判斷是否需要 Commit，由使用者決定
            foreach (TabPage theTab in tabControl1.TabPages)
            {
                var title = theTab.Title;
                var accessibleDescription = theTab.AccessibleDescription;

                if (!SpecialTabName.Contains(title))
                {
                    sb.Append(accessibleDescription).Append(';');
                }
            }

            MyGlobal.GlobalTemp = sb.ToString();

            if (string.IsNullOrEmpty(MyGlobal.GlobalTemp) || MyGlobal.GlobalTemp == "CloseQueryFormAndCheckCommit`") //沒有開啟 Editor，所以不需要 Commit
            {
                MyGlobal.CommitRollbackCheck = 0;
                MyGlobal.GlobalTemp = string.Empty; //沒有開啟 Editor
            }

            while (MyGlobal.CommitRollbackCheck == -1 && !string.IsNullOrEmpty(MyGlobal.GlobalTemp))
            {
                Application.DoEvents();
            }

            if (MyGlobal.CommitRollbackCheck == 2)
            {
                MyGlobal.CommitRollbackCheck = -1; //恢復成初始值

                //使用者取消，不用關閉了
                return false; //這裡要用 return，否則程式還是會被整個關閉
            }

            //20240823 此處設定由 -1 改為 0，方便關閉 Schema Brower 時，判斷是否由 Main Form 關閉的
            MyGlobal.CommitRollbackCheck = 0; //恢復成初始值，因為後續使用者可能「取消存檔」而「不關閉程式」

            //20240802 判斷是否有異動 SchemaBrowser 內容，並詢問是否要繼續「關閉」JasonQuery
            #region 關閉 SchemaBrowser Form !
            sb.Clear();
            sb.Append("CloseSchemaBrowserForm`");

            foreach (TabPage theTab in tabControl1.TabPages)
            {
                var title = theTab.Title;
                var tabAccessibleDescription = theTab.AccessibleDescription;

                if (title == MyGlobal.SchemaBrowserTabName)
                {
                    sb.Append(tabAccessibleDescription).Append(";");
                }
            }

            MyGlobal.GlobalTemp = sb.ToString();

            if (MyGlobal.GlobalTemp == "CloseSchemaBrowserForm`")
            {
                MyGlobal.GlobalTemp = string.Empty; //沒有開啟 SchemaBrowser
            }

            var isCancelClose = false;

            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
            {
                if (MyGlobal.GlobalTemp.Contains("|CANCEL;"))
                {
                    MyGlobal.GlobalTemp = string.Empty;
                    isCancelClose = true;
                    break;
                }

                if (MyGlobal.GlobalTemp == "CloseSchemaBrowserForm`")
                {
                    MyGlobal.GlobalTemp = string.Empty;
                    break;
                }

                Application.DoEvents();
            }

            if (isCancelClose)
            {
                return false;
            }
            #endregion

            //20240302 有啟用備份，不需要再詢問是否要儲存檔案
            if (AppConfigHelper.IsBackupFile)
            {
                return true;
            }

            //20250903 使用者沒有啟用備份，此處用函數判斷是否需要存檔並繼續「關閉」JasonQuery
            return CloseTabBeforeCloseApplication();
        }

        private bool CloseTabBeforeCloseApplication()
        {
            var result = true;
            var tabPagesCount = tabControl1.TabPages.Count - 1;

            try
            {
                if (tabPagesCount > 0)
                {
                    for (var i = tabPagesCount; i >= 0; i--)
                    {
                        var isCancelClose = false;
                        var title = tabControl1.TabPages[i].Title;
                        var tabAccessibleDescription = tabControl1.TabPages[i].AccessibleDescription;

                        if (title.StartsWith("*", StringComparison.Ordinal))
                        {
                            //關閉 Query Form !
                            MyGlobal.GlobalTemp = $"CloseQueryForm`{tabAccessibleDescription};";

                            while (!string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                            {
                                if (MyGlobal.GlobalTemp.Contains($"{tabAccessibleDescription}|CANCEL;"))
                                {
                                    isCancelClose = true;
                                    break;
                                }

                                Application.DoEvents();
                            }

                            if (isCancelClose)
                            {
                                result = false;
                                break;
                            }

                            //關閉頁籤：同時刪除備份的對應記錄，下次重啟 JasonQuery 時，不用再開啟此檔案
                            MyGlobal.DeleteBackupFileInfo(tabControl1.TabPages[i].AccessibleName);

                            tabControl1.TabPages[i].Dispose();
                            tabControl1.TabPages.Remove(tabControl1.TabPages[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result;
        }

        private void mnuNewConnection_Click(object sender, EventArgs e)
        {
            try
            {
                LoadConnectionForm();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void mnuDatabaseConnectionViewMode_Click(object sender, EventArgs e)
        {
            using (var myForm = new ConnectionForm())
            {
                myForm.ShowInTaskbar = false;
                myForm.StartPosition = FormStartPosition.CenterScreen;
                myForm.IsViewMode = true;
                myForm.ShowDialog();
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (BeforeCloseApplication())
            {
                Close();
            }
        }

        private void mnuCompactMDB_Click(object sender, EventArgs e)
        {
            //
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            using (var form = new AboutForm())
            {
                form.ShowDialog();
            }
        }

        private void mnuCheckForUpdatesManually_Click(object sender, EventArgs e)
        {
            using (var form = new UpdateForm())
            {
                form.ShowDialog();
            }
        }

        private void mnuUpdateNow_Click(object sender, EventArgs e)
        {
            var executeName = $@"{Application.StartupPath}\Updater.exe";

            if (File.Exists(executeName))
            {
                var infoExe = new ProcessStartInfo
                {
                    FileName = executeName,
                    WorkingDirectory = $@"{Application.StartupPath}\",
                    Arguments = $"{LocalizationHelper.LocalizationCode}|{LocalizationHelper.XmlFileName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(infoExe);
            }
            else
            {
                var message = LocalizationHelper.GetLanguageString("File not found:", "form", GetType().Name, "msg", "UpdaterNotFound", "Text");

                MessageBox.Show($"{message}\r\n\r\n{executeName}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void mnuNewSqlEditor_Click(object sender, EventArgs e)
        {
            CreateNewTab("Query", CheckTabNameExist()); //從主選單開啟新的 SQL Editor 頁籤
        }

        private void mnuOpenQueryFile_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            var of = new OpenFileDialog
            {
                Multiselect = true,
                Title = LocalizationHelper.GetLanguageString("Open File", "Global", "Global", "msg", "OpenFile", "Text"),
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (of.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                foreach (var fileName in of.FileNames)
                {
                    if (!CheckTabNameExist(fileName))
                    {
                        var tabName = CheckTabNameExist();

                        CreateNewTab("Query", tabName, fileName); //開啟指定檔案
                        MyGlobal.CancelOpenAndCloseTab = tabName;
                    }

                    var startTime = DateTime.Now;

                    while (true)
                    {
                        Application.DoEvents();

                        if (DateTime.Now.Subtract(startTime).Milliseconds >= 150)
                        {
                            break;
                        }

                        Application.DoEvents();
                    }
                }

                UpdateTabList(); //OpenFile
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void mnuFileSplitter_Click(object sender, EventArgs e)
        {
            using (var form = new FileSplitterForm())
            {
                form.ShowDialog();
            }
        }

        private void mnuFile_Save_Click(object sender, EventArgs e)
        {
            GetActiveQueryForm()?.ExecuteSaveCommand();
        }

        private void mnuFile_SaveAs_Click(object sender, EventArgs e)
        {
            GetActiveQueryForm()?.ExecuteSaveAsCommand();
        }

        private QueryForm GetActiveQueryForm()
        {
            return tabControl1.SelectedTab?.Control as QueryForm;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            _changeFormSizeManually++; //經過這裡，才可以判定是使用者改變 Form 大小，而不是按下「放至最大」「縮至最小」按鈕

            if (Left == -32000 && Top == -32000)
            {
                HideAutoCompleteList();
            }
            else
            {
                AppConfigHelper.MainFormLeft = Left;
                AppConfigHelper.MainFormTop = Top;
            }
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            _changeFormSizeManually++;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (_changeFormSizeManually > 1)
            {
                JasonQueryRepository.UpdateSetting("GlobalConfig", "MainFormWidth", Size.Width.ToString());
                JasonQueryRepository.UpdateSetting("GlobalConfig", "MainFormHeight", Size.Height.ToString());
            }

            _changeFormSizeManually = 0;
            AppConfigHelper.MainFormLeft = Left;
            AppConfigHelper.MainFormTop = Top;
        }

        private static bool VerifyCheckForUpdate(UpdateCheckScheduleOperation operation)
        {
            var sql = string.Empty;
            var result = false;
            var sbSql = new StringBuilder();

            if (operation == UpdateCheckScheduleOperation.Query)
            {
                //查詢是否需要更新
                sbSql.AppendLine("SELECT AttributeDate FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                sbSql.Append("   AND AttributeName = 'CheckForUpdateDays'");

                sql = sbSql.ToString();

                var dt = JasonQueryRepository.ExecQuery(sql);

                if (dt?.Rows.Count > 0)
                {
                    try
                    {
                        var ts1 = new TimeSpan(Convert.ToDateTime(dt.Rows[0]["AttributeDate"].ToString()).Ticks);
                        var ts2 = new TimeSpan(DateTime.Now.Ticks);
                        var ts = ts1.Subtract(ts2).Duration();

                        if ((ts.Days * 24 + ts.Hours) >= (MyLibrary.CheckForUpdateValue * 24))
                        {
                            result = true;
                        }
                    }
                    catch (Exception)
                    {
                        //日期格式有錯誤
                        sbSql.Clear();
                        sbSql.AppendLine("UPDATE SystemConfig");
                        sbSql.AppendLine($"   SET AttributeDate = '{DateTime.Now:yyyy/MM/dd 00:00:00}'");
                        sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                        sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                        sbSql.Append("   AND AttributeName = 'CheckForUpdateDays'");

                        sql = sbSql.ToString();
                        JasonQueryRepository.ExecNonQuery(sql);
                        result = true;
                    }
                }
                else
                {
                    sbSql.Clear();
                    sbSql.AppendLine("INSERT INTO SystemConfig");
                    sbSql.AppendLine("       (DomainUser, AttributeKey, AttributeName, AttributeValue, AttributeDate)");
                    sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', 'GlobalConfig', 'CheckForUpdateDays', '{MyLibrary.CheckForUpdateValue}', '{DateTime.Now:yyyy/MM/dd 00:00:00}')");

                    sql = sbSql.ToString();
                    JasonQueryRepository.ExecNonQuery(sql);
                    result = true;
                }
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeDate = '{DateTime.Now:yyyy/MM/dd 00:00:00}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                sbSql.Append("   AND AttributeName = 'CheckForUpdateDays'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }

            return result;
        }

        private void mnuFile_Click(object sender, EventArgs e)
        {
            UpdateFileMenuState();
        }

        private void mnuFile_MouseHover(object sender, EventArgs e)
        {
            UpdateFileMenuState();
        }

        private void UpdateFileMenuState()
        {
            var hasActiveQueryForm = GetActiveQueryForm() != null;

            mnuFile_Save.Enabled = hasActiveQueryForm;
            mnuFile_SaveAs.Enabled = hasActiveQueryForm;
        }

        private void tmrPendingTransactionIdleCheck_Tick(object sender, EventArgs e)
        {
            var decision = PendingTransactionReminderPolicy.Evaluate
                           (
                               GetPendingTransactionReminderState(),
                               DateTime.Now,
                               MyGlobal.IsPendingTransactionWarning,
                               MyGlobal.PendingTransactionWarningIntervalMilliseconds
                           );

            ApplyPendingTransactionReminderState(decision.State);

            if (!decision.State.IsPending)
            {
                SetPendingTransactionStatus(false);
                return;
            }

            UpdatePendingTransactionElapsedTime();

            if (!decision.ShouldShowWarning)
            {
                tmrPendingTransactionIdleCheck.Enabled = decision.ShouldEnableTimer;
                return;
            }

            tmrPendingTransactionIdleCheck.Enabled = false;

            ShowPendingTransactionIdleWarning();
        }

        private void ShowPendingTransactionIdleWarning()
        {
            try
            {
                var message = LocalizationHelper.GetLanguageString("There is a pending transaction that has not been committed or rolled back.\r\n\r\nPlease return to any QueryForm tab and click the Commit or Rollback button.", "form", GetType().Name, "msg", "PendingTransactionIdleWarning", "Text");

                MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (AppConfigHelper.IsNotCommitYet)
                {
                    var state = PendingTransactionReminderPolicy.ScheduleNextWarning
                                (
                                    GetPendingTransactionReminderState(),
                                    DateTime.Now,
                                    MyGlobal.PendingTransactionWarningIntervalMilliseconds
                                );

                    ApplyPendingTransactionReminderState(state);
                    tmrPendingTransactionIdleCheck.Enabled = true;
                }
                else
                {
                    SetPendingTransactionStatus(false);
                }
            }
        }

        private void mnuCloseConnection_Click(object sender, EventArgs e)
        {
            var descriptions = tabControl1.TabPages.Cast<TabPage>().Where(tab => !SpecialTabName.Contains(tab.Title)).Select(tab => tab.AccessibleDescription);
            var temp = $"AutoDisconnect`{string.Join(";", descriptions)};";

            MyGlobal.GlobalTemp = temp;

            mnuNewConnection.Enabled = false;
            mnuOpenConnection.Enabled = true;
            mnuCloseConnection.Enabled = false;

            //20250413 提示使用者
            pnlArrow.Visible = mnuNewConnection.Enabled;
        }

        private void DisconnectDatabase(bool started = false)
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
                case DataSourceType.None:
                    {
                        break;
                    }
            }

            mnuDatabaseConnectionViewMode.Visible = true;

            if (started)
            {
                return;
            }

            mnuNewConnection.Enabled = false;
            mnuOpenConnection.Enabled = true;
            mnuCloseConnection.Enabled = false;

            //20250413 提示使用者
            pnlArrow.Visible = mnuNewConnection.Enabled;
        }

        private void mnuOpenConnection_Click(object sender, EventArgs e)
        {
            string result;

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        result = MyGlobal.OracleReader.ConnectTo();

                        if (!string.IsNullOrEmpty(result))
                        {
                            MessageBox.Show(result, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        result = MyGlobal.PostgreSqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (!string.IsNullOrEmpty(result))
                        {
                            MessageBox.Show(result, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        result = MyGlobal.SqlServerReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (!string.IsNullOrEmpty(result))
                        {
                            MessageBox.Show(result, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        result = MyGlobal.MySqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (!string.IsNullOrEmpty(result))
                        {
                            MessageBox.Show(result, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                        break;
                    }
                case DataSourceType.None:
                    {
                        break;
                    }
            }

            mnuNewConnection.Enabled = false;
            mnuOpenConnection.Enabled = false;
            mnuCloseConnection.Enabled = true;

            //20250413 提示使用者
            pnlArrow.Visible = mnuNewConnection.Enabled;
        }

        private void mnuReleaseNotes_Click(object sender, EventArgs e)
        {
            //20240309 新增繁體中文 Release Notes
            switch (LocalizationHelper.LocalizationCode)
            {
                case "zh-TW":
                case "zh-CN":
                    {
                        Process.Start("https://jasonquery.org/releasenotes_cht.html");
                        break;
                    }
                default: //English
                    {
                        Process.Start("https://jasonquery.org/releasenotes.html");
                        break;
                    }
            }
        }

        private void mnuReportBugs_Click(object sender, EventArgs e)
        {
            Process.Start("https://jasonquery.org/reportbugs.html");
        }

        private void mnuMainForm_MouseEnter(object sender, EventArgs e)
        {
            //20220725 判斷 Editor 是否有顯示下拉清單？
            _mouseMove = 1;
        }

        private void mnuMainForm_MouseLeave(object sender, EventArgs e)
        {
            _mouseMove = -1;
        }

        private void mnuMainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseMove == -1)
            {
                return;
            }

            _mouseMove = -1;

            HideAutoCompleteList();
        }

        private void HideAutoCompleteList() //視窗最小化、滑鼠移到功能表區域
        {
            var sb = new StringBuilder();

            sb.Append("HideAutoCompleteList`");

            foreach (TabPage theTab in tabControl1.TabPages)
            {
                var tabTitle = theTab.Title;
                var tabAccessibleDescription = theTab.AccessibleDescription;

                if (!SpecialTabName.Contains(tabTitle))
                {
                    //隱藏 AutoComplete 下拉清單
                    sb.Append(tabAccessibleDescription).Append(";");
                }
            }

            MyGlobal.GlobalTemp = sb.ToString();
        }

        private void LoadPostgreSqlDatabase()
        {
            mnuSwitchDatabase.Visible = true;
            mnuSwitchDatabase.DropDownItems.Clear();

            if (_dtDatabase == null)
            {
                return;
            }

            for (var i = 0; i < _dtDatabase.Rows.Count; i++)
            {
                var dbName = _dtDatabase.Rows[i].GetSafeString("datname");

                mnuSwitchDatabase.DropDownItems.Add(dbName);
                mnuSwitchDatabase.DropDownItems[i].Tag = dbName;
                mnuSwitchDatabase.DropDownItems[i].Enabled = dbName != DatabaseSqlExecutor.DatabaseName;
                //mnuSwitchDatabase.DropDownItems[i].Name = dbName;

                if (dbName == DatabaseSqlExecutor.DatabaseName)
                {
                    //20220825 指定 icon
                    mnuSwitchDatabase.DropDownItems[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "PostgreSQL 0 16x16.ico");
                    ((ToolStripMenuItem)mnuSwitchDatabase.DropDownItems[i]).Checked = true;
                }

                mnuSwitchDatabase.DropDownItems[i].Click += SwitchDatabase_PostgreSQL;
            }
        }

        private void SwitchDatabase_PostgreSQL(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            if (TextHelper.IsNullOrEmptyTag(mnuItem.Tag))
            {
                return;
            }

            Application.UseWaitCursor = true;
            MyGlobal.CommitRollbackCheck = -1;

            var sisCloseQueryFormAndCheckCommit = new StringBuilder();
            var sbTabsInfo = new StringBuilder();

            sisCloseQueryFormAndCheckCommit.Append("CloseQueryFormAndCheckCommit`");
            sbTabsInfo.Append("ReloadSchemaInfo`"); //重新載入 Schema Info

            try
            {
                //判斷是否需要 Commit，由使用者決定
                foreach (TabPage theTab in tabControl1.TabPages)
                {
                    var tabTitle = theTab.Title;
                    var tabAccessibleDescription = theTab.AccessibleDescription;

                    if (SpecialTabName.Contains(tabTitle))
                    {
                        continue;
                    }

                    sisCloseQueryFormAndCheckCommit.Append(tabAccessibleDescription).Append(";");
                    sbTabsInfo.Append(tabAccessibleDescription).Append(";");
                }

                MyGlobal.GlobalTemp = sisCloseQueryFormAndCheckCommit.ToString();

                if (string.IsNullOrEmpty(MyGlobal.GlobalTemp) || MyGlobal.GlobalTemp == "CloseQueryFormAndCheckCommit`") //沒有開啟 Editor，所以不需要 Commit
                {
                    MyGlobal.CommitRollbackCheck = 0;
                }

                while (MyGlobal.CommitRollbackCheck == -1 && !string.IsNullOrEmpty(MyGlobal.GlobalTemp))
                {
                    Application.DoEvents();
                }

                if (MyGlobal.CommitRollbackCheck == 2)
                {
                    MyGlobal.CommitRollbackCheck = -1; //恢復成初始值

                    //使用者取消，不用關閉了
                    return; //這裡要用 return，否則程式還是會被整個關閉
                }

                var tag = TextHelper.GetSafeString(mnuItem.Tag);

                MyGlobal.CommitRollbackCheck = -1; //恢復成初始值，因為後續使用者可能「取消存檔」而「不關閉程式」
                DatabaseSqlExecutor.DatabaseName = tag;

                var database = TextHelper.GetStringBetween2(DatabaseSqlExecutor.DbConnectionString, ";Database=", ";", true);

                DatabaseSqlExecutor.DbConnectionString = DatabaseSqlExecutor.DbConnectionString.Replace($";Database={database};", $";Database={tag};");
                btnDatabase.Text = tag;

                ConnectToDatabase(); //切換資料庫後，重新連線
                LoadPostgreSqlDatabase(); //切換資料庫後，重新產生功能表資訊

                var c1Grid = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();

                DatabaseSqlExecutor.UpdateSchemaData_PostgreSql(c1Grid, false); //切換資料庫後，重新撈取 Schema Info
                MyGlobal.GlobalTemp = sbTabsInfo.ToString();
                Application.UseWaitCursor = false;

                _languageText = LocalizationHelper.GetLanguageString("The database has been switched to {db} successfully!", "form", GetType().Name, "msg", "SwitchedDatabase", "Text").Replace("{db}", tag);
                MessageBox.Show(_languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void LoadSqlServerDatabase(string database = "")
        {
            mnuSwitchDatabase.Visible = true;
            mnuSwitchDatabase.DropDownItems.Clear();

            if (_dtDatabase == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(database))
            {
                database = DatabaseSqlExecutor.DatabaseName;
            }

            for (var i = 0; i < _dtDatabase.Rows.Count; i++)
            {
                var dbName = _dtDatabase.Rows[i].GetSafeString("name");

                mnuSwitchDatabase.DropDownItems.Add(dbName);
                mnuSwitchDatabase.DropDownItems[i].Tag = dbName;
                mnuSwitchDatabase.DropDownItems[i].Enabled = !string.Equals(dbName, database, StringComparison.CurrentCultureIgnoreCase);

                if (string.Equals(dbName, DatabaseSqlExecutor.DatabaseName, StringComparison.CurrentCultureIgnoreCase)) //使用 USE 指令切換
                {
                    //20220825 指定 icon
                    mnuSwitchDatabase.DropDownItems[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "SQL Server 0 16x16.ico");
                    ((ToolStripMenuItem)mnuSwitchDatabase.DropDownItems[i]).Checked = true;
                }

                mnuSwitchDatabase.DropDownItems[i].Click += SwitchDatabase_SQLServer;
            }
        }

        private void SwitchDatabase_SQLServer(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            var tag = TextHelper.GetSafeString(mnuItem.Tag);

            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            Application.UseWaitCursor = true;

            var tabTitle0 = tabControl1.SelectedTab.Title;
            var accessibleDescription = string.Empty;

            try
            {
                if (tabControl1.TabPages.Count > 0)
                {
                    if (SpecialTabName.Contains(tabTitle0)) //所在頁籤不是 SQL Editor
                    {
                        //找出其中一個 SQL Editor，送出 Use database 指令
                        foreach (TabPage theTab in tabControl1.TabPages)
                        {
                            var tabTitle = theTab.Title;
                            var tabAccessibleDescription = theTab.AccessibleDescription;

                            if (SpecialTabName.Contains(tabTitle))
                            {
                                continue;
                            }

                            accessibleDescription = tabAccessibleDescription;
                            theTab.Selected = true;
                            break;
                        }
                    }
                    else
                    {
                        accessibleDescription = tabControl1.SelectedTab.AccessibleDescription;
                    }
                }

                var temp = string.Join("`", tabControl1.TabPages.Cast<TabPage>()
                                                                .Where
                                                                (
                                                                    tab => !SpecialTabName.Contains(tab.Title) &&
                                                                    tab.AccessibleDescription != accessibleDescription
                                                                )
                                                                .Select
                                                                (
                                                                    tab => tab.AccessibleDescription)
                                                                ) + "`";

                MyGlobal.GlobalTemp = $"SQLServerSwitchDatabaseFromMainForm{MyGlobal.Separator}{accessibleDescription};{tag};{temp}";

                DatabaseSqlExecutor.DatabaseName = tag;
                btnDatabase.Text = tag;

                LoadSqlServerDatabase(); //切換資料庫後，重新產生功能表資訊
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            Application.UseWaitCursor = false;
        }

        private void LoadMySqlDatabase(string database = "")
        {
            mnuSwitchDatabase.Visible = true;
            mnuSwitchDatabase.DropDownItems.Clear();

            if (_dtDatabase == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(database))
            {
                database = DatabaseSqlExecutor.DatabaseName;
            }

            for (var i = 0; i < _dtDatabase.Rows.Count; i++)
            {
                var dbName = _dtDatabase.Rows[i].GetSafeString("name");

                mnuSwitchDatabase.DropDownItems.Add(dbName);
                mnuSwitchDatabase.DropDownItems[i].Tag = dbName;
                mnuSwitchDatabase.DropDownItems[i].Enabled = !string.Equals(dbName, database, StringComparison.CurrentCultureIgnoreCase);

                if (string.Equals(dbName, DatabaseSqlExecutor.DatabaseName, StringComparison.CurrentCultureIgnoreCase)) //使用 USE 指令切換，大小寫會有差異
                {
                    //20220825 指定 icon
                    mnuSwitchDatabase.DropDownItems[i].Image = IconManager.GetImage(MyGlobal.IconLibrary, "MySQL 0 16x16.ico");
                    ((ToolStripMenuItem)mnuSwitchDatabase.DropDownItems[i]).Checked = true;
                }

                mnuSwitchDatabase.DropDownItems[i].Click += SwitchDatabase_MySQL;
            }
        }

        private void SwitchDatabase_MySQL(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem mnuItem))
            {
                return;
            }

            var tag = TextHelper.GetSafeString(mnuItem.Tag);

            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            Application.UseWaitCursor = true;

            var tabTitle0 = tabControl1.SelectedTab.Title;
            var accessibleDescription = string.Empty;

            try
            {
                if (tabControl1.TabPages.Count > 0)
                {
                    if (SpecialTabName.Contains(tabTitle0)) //所在頁籤不是 SQL Editor
                    {
                        //找出其中一個 SQL Editor，送出 Use database 指令
                        foreach (TabPage theTab in tabControl1.TabPages)
                        {
                            var tabTitle = theTab.Title;
                            var tabAccessibleDescription = theTab.AccessibleDescription;

                            if (SpecialTabName.Contains(tabTitle))
                            {
                                continue;
                            }

                            accessibleDescription = tabAccessibleDescription;
                            theTab.Selected = true;
                            break;
                        }
                    }
                    else
                    {
                        accessibleDescription = tabControl1.SelectedTab.AccessibleDescription;
                    }
                }

                var temp = string.Join("`", tabControl1.TabPages.Cast<TabPage>()
                                                                .Where
                                                                (
                                                                    tab => !SpecialTabName.Contains(tab.Title) &&
                                                                    tab.AccessibleDescription != accessibleDescription
                                                                )
                                                                .Select
                                                                (
                                                                    tab => tab.AccessibleDescription)
                                                                ) + "`";

                MyGlobal.GlobalTemp = $"MySQLSwitchDatabaseFromMainForm{MyGlobal.Separator}{accessibleDescription};{tag};{temp}";

                DatabaseSqlExecutor.DatabaseName = tag;
                btnDatabase.Text = tag;

                LoadMySqlDatabase(); //切換資料庫後，重新產生功能表資訊
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            Application.UseWaitCursor = false;
        }

        private void mnuBlobViewer_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new BlobViewerForm())
                {
                    MyLibrary.IsBlobReadOnly = true;
                    form.FileName = string.Empty;
                    form.FieldName = string.Empty;

                    var blobViewerFontSize = 0;
                    var sbSql = new StringBuilder();

                    sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine("   AND AttributeKey = 'GlobalConfig'");
                    sbSql.Append("   AND AttributeName = 'BlobViewerFontSize'");

                    var sql = sbSql.ToString();
                    var dtData = JasonQueryRepository.ExecQuery(sql);

                    if (dtData?.Rows.Count > 0)
                    {
                        int.TryParse(dtData.Rows[0].GetSafeString(0), out blobViewerFontSize);
                    }
                    else
                    {
                        blobViewerFontSize = 10;
                        JasonQueryRepository.UpdateSetting("GlobalConfig", "BlobViewerFontSize", "10");
                    }

                    //20250426 改寫 Width/Height 取值方法
                    var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "BlobViewerFormWidth", "BlobViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

                    form.ClientSize = new Size(formWidth - 16, formHeight - 38);
                    form.FontSize = blobViewerFontSize;
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Assistant_Click(object sender, EventArgs e)
        {
            var applicationName = string.Empty;
            var path = string.Empty;
            var mnu = sender as ToolStripMenuItem;
            var tag = TextHelper.GetSafeString(mnu.Tag);

            switch (tag)
            {
                case "Windows Explorer":
                    {
                        applicationName = "explorer.exe";
                        break;
                    }
                case "Notepad":
                    {
                        applicationName = "notepad.exe";
                        break;
                    }
                case "Calculator":
                    {
                        applicationName = "calc.exe";
                        break;
                    }
                case "Paint":
                    {
                        applicationName = "mspaint.exe";
                        break;
                    }
                case "Desktop":
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    }
                case "Temporary":
                    {
                        path = Path.GetTempPath();
                        break;
                    }
                case "StartupAllUser":
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                        break;
                    }
                case "StartupPersonal":
                    {
                        path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        break;
                    }
                case "JasonQueryLocated":
                    {
                        path = $"/select, {Application.ExecutablePath}";
                        break;
                    }
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path) || tag == "JasonQueryLocated")
                {
                    Process.Start("explorer.exe", path);
                }
                else
                {
                    var message = LocalizationHelper.GetLanguageString("Path not found!", "Global", "Global", "msg", "PathNotFound", "Text");

                    MessageBox.Show($"{message}\r\n\r\n{path}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                Process.Start(applicationName);
            }
        }

        private void mnuAsciiTable_Click(object sender, EventArgs e)
        {
            using (var form = new AsciiTableForm())
            {
                form.ShowDialog();
            }
        }

        private void mnuColorPicker_Click(object sender, EventArgs e)
        {
            using (var form = new ColorConverterDialog())
            {
                form.ShowDialog();
            }
        }

        //20241207 更新 Tab List
        private void UpdateTabList()
        {
            //20250626 預防 SpecialTabName 為 null (在某些操作手順，它會是 null)
            if (SpecialTabName == null)
            {
                SetSpecialTabName();
            }

            var sb = new StringBuilder();

            sb.Append("TabList`");

            MyGlobal.dtTabList = new DataTable();
            MyGlobal.dtTabList.Columns.Add("Tab");
            MyGlobal.dtTabList.Columns.Add("AccessibleDescription");

            foreach (TabPage theTab in tabControl1.TabPages)
            {
                var tabTitle = theTab.Title;
                var tabAccessibleDescription = theTab.AccessibleDescription;

                if (!SpecialTabName.Contains(tabTitle))
                {
                    sb.Append(tabAccessibleDescription).Append("`");
                }

                var row = MyGlobal.dtTabList.NewRow();

                row["Tab"] = tabTitle;
                row["AccessibleDescription"] = tabAccessibleDescription;
                MyGlobal.dtTabList.Rows.Add(row);
            }

            MyGlobal.GlobalTemp6 = sb.ToString();
        }

        private static class MapColumn
        {
            public const int Close = 0;
            public const int CloseAll = 1;
            public const int CloseAllButThis = 2;
            public const int CloseAllLeft = 3;
            public const int CloseAllRight = 4;
            public const int CloseAllSavedFiles = 5;
            public const int CloseAllUnsavedFiles = 6;
            public const int Dash0 = 7;
            public const int RenameTab = 8;
            public const int Dash1 = 9;
            public const int NewSQLEditor = 10;
            public const int OpenFile = 11;
            public const int Dash2 = 12;
            public const int AddToMyFavorite = 13;
            public const int RemoveFromMyFavorite = 14;
            public const int Dash3 = 15;
            public const int OpenFolder = 16;
            public const int Dash4 = 17;
            public const int CopyFullFilePath = 18;
            public const int CopyFileName = 19;
            public const int CopyCurrentPath = 20;
        }
    }
}
