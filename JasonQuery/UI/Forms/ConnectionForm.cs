using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Infrastructure.Windows;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm : Form
    {
        public bool IsViewMode { get; set; } = false; //20240505 是否為檢視模式
        public bool IsApplicationExitRequested { get; private set; }

        private DataTable _dtDbInfo;
        private DataTable _dtDatabaseListInfo_PostgreSql; //database 欄位下拉清單 (各自獨立，避免相互干擾)
        private DataTable _dtDatabaseListInfo_SqlServer; //database 欄位下拉清單 (各自獨立，避免相互干擾)
        private DataTable _dtDatabaseListInfo_MySql; //database 欄位下拉清單 (各自獨立，避免相互干擾)
        private DataRow _rowDbInfo;
        private List<Control> _lstPicLogo;
        private List<RadioButton> _lstRdoIconStyle;
        private List<PictureBox> _lstPicIconStyle;

        //20260805 重構：記住使用者選中哪一個 Icon Style
        private const int DefaultMainFormIconStyleIndex = 0;
        private int _selectedMainFormIconStyleIndex = DefaultMainFormIconStyleIndex;

        private List<Control> _lstPanelTabColor;
        private List<string> _lstSupportInfo = new List<string>();
        private bool _isFormLoadFinished; //表單是否載入完畢 (避免觸發事件)
        private ToolTip _toolTip1 = new ToolTip();
        private string _panelColorSelectedName = string.Empty;
        private string _languageText = string.Empty;
        private string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private Color _colorEssentialField = Color.LightYellow;
        private Color _colorOptionalField = ColorTranslator.FromHtml("#EAF2FF");
        private string _password1;
        private string _password2;
        private bool _isComboBoxKeypressForDatabase = false;
        private bool _isKeyPressTab = false; //記住是否按下 Tab 鍵 (KeyUp 可以偵測 Tab，但 Tab 鍵已被提前觸發了，故不在 KeyUp 攔截)
        private bool _isKeyPressEsc = false; //記住是否按下 ESC 鍵
        private bool _isKeyPressDelete = false; //記住是否按下 Delete 鍵
        private bool _isConnecting = false; //20241207 是否正在連線中
        private bool _suppressDataSourceTextChanged;
        private string _lastSelectedDataSourceForNewConnection = string.Empty;

        private const string DefaultDataSourceForNewConnection = "Oracle";

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName,
                                                   [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName,
                                                   ref int length);

        public ConnectionForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                tabPage1.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL Editor Blue 16x16.ico");
                tabPage2.Icon = IconManager.GetIcon(MyGlobal.IconLibrary, "SQL Editor Red 16x16.ico");

                pnlOracle.Location = new Point(8, 46);
                pnlPostgreSQL.Location = new Point(8, 46);
                pnlSQLServer.Location = new Point(8, 46);
                pnlMySQL.Location = new Point(8, 46);
                pnlSQLite.Location = new Point(8, 46);

                c1GridPostgreSQL.Splits[0].RecordSelectors = false;
                c1GridPostgreSQL.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                GridHelper.SetGridVisualStyle(c1GridPostgreSQL, 10);

                c1GridSQLServer.Splits[0].RecordSelectors = false;
                c1GridSQLServer.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                GridHelper.SetGridVisualStyle(c1GridSQLServer, 10);

                c1GridMySQL.Splits[0].RecordSelectors = false;
                c1GridMySQL.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
                GridHelper.SetGridVisualStyle(c1GridMySQL, 10);

                if (IntPtr.Size == 8) //8:x64；非8:x86
                {
                    cboDatabaseType_SQLite.Items.Add("SQLCipher(Community)");
                }

                cboDatabaseType_SQLite.Items.Add("System.Data.SQLite RC4");

                _password1 = string.Empty;

                cboDataSource.BackColor = _colorEssentialField;

                txtConnectionName_Oracle.BackColor = _colorEssentialField;
                txtServer_Oracle.BackColor = _colorEssentialField;
                txtPort_Oracle.BackColor = _colorEssentialField;
                txtUserID_Oracle.BackColor = _colorEssentialField;
                txtPassword_Oracle.BackColor = _colorEssentialField;
                cboConnectAs_Oracle.BackColor = _colorEssentialField;

                txtConnectionName_PostgreSQL.BackColor = _colorEssentialField;
                txtServer_PostgreSQL.BackColor = _colorEssentialField;
                cboDatabase_PostgreSQL.BackColor = _colorEssentialField;
                txtPort_PostgreSQL.BackColor = _colorEssentialField;
                txtUserID_PostgreSQL.BackColor = _colorEssentialField;
                txtPassword_PostgreSQL.BackColor = _colorEssentialField;

                txtConnectionName_SQLServer.BackColor = _colorEssentialField;
                txtServer_SQLServer.BackColor = _colorEssentialField;
                txtUserID_SQLServer.BackColor = _colorEssentialField;
                txtPassword_SQLServer.BackColor = _colorEssentialField;

                txtConnectionName_MySQL.BackColor = _colorEssentialField;
                txtServer_MySQL.BackColor = _colorEssentialField;
                txtUserID_MySQL.BackColor = _colorEssentialField;
                txtPassword_MySQL.BackColor = _colorEssentialField;

                txtConnectionName_SQLite.BackColor = _colorEssentialField;
                txtFile_SQLite.BackColor = _colorEssentialField;

                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);
                cboLocalization.Text = LocalizationHelper.Localization;
                cboLocalization.Tag = LocalizationHelper.Localization;

                _lstPanelTabColor = new List<Control>
                {
                    pnlBackColor,
                    pnlActiveForeColor,
                    pnlInactiveForeColor
                };

                btnCreateShortcut.Enabled = !WindowsShortcutHelper.DesktopShortcutExists("JasonQuery");

                _lstPicLogo = new List<Control>
                {
                    picOracle,
                    picPostgreSQL,
                    picSQLServer,
                    picMySQL,
                    picSQLite
                };

                _lstRdoIconStyle = new List<RadioButton>
                {
                    rdoIconStyle0,
                    rdoIconStyle1,
                    rdoIconStyle2,
                    rdoIconStyle3,
                    rdoIconStyle4,
                    rdoIconStyle5,
                    rdoIconStyle6,
                    rdoIconStyle7,
                    rdoIconStyle8,
                    rdoIconStyle9
                };

                _lstPicIconStyle = new List<PictureBox>
                {
                    picIconStyle0,
                    picIconStyle1,
                    picIconStyle2,
                    picIconStyle3,
                    picIconStyle4,
                    picIconStyle5,
                    picIconStyle6,
                    picIconStyle7,
                    picIconStyle8,
                    picIconStyle9
                };

                ConfigureDataSourceComboBox(); //20260415 重構
                InitializeSupportInfo();
                ApplyLocalizationSetting(true);
                chkLog.Checked = TraceLogger.IsEnabled;

                if (IsViewMode)
                {
                    Text = LocalizationHelper.GetLanguageString(Text, "form", GetType().Name, "object", "thisViewMode", "Text");
                    pnlConnect.Enabled = false;
                    cboLocalization.Enabled = false;
                    btnUpdateNow.Enabled = false;
                    btnDatabaseSecurity.Enabled = false;
                    btnExit.Enabled = false;
                }

                lblDomainUser.Text = MyGlobal.DomainUser;

                //判斷是否透過檔案總管呼叫 JasonQuery.exe 並開啟指定的 sql 檔案
                if (!string.IsNullOrEmpty(MyGlobal.PendingExternalOpenFileName))
                {
                    txtOpenSpecifiedFile.Text = MyGlobal.PendingExternalOpenFileName;

                    grpOpenFile.Visible = true;
                    grpOpenFile.ForeColor = Color.DarkGreen;

                    //20231229 改為動態調整位置及大小
                    c1GridDBInfo.Location = new Point(10, c1GridDBInfo.Top + grpOpenFile.Height + 5);
                    c1GridDBInfo.Size = new Size(c1GridDBInfo.Width, c1GridDBInfo.Height - grpOpenFile.Height - 5);
                }

                if (!IsViewMode)
                {
                    //Form_Load 還原以下初始值，避免連線有誤時，再次開啟連線視窗又馬上關閉時，又再嘗試連線而引發錯誤
                    ResetTransientConnectionState();
                }

                //20260711 重新製作圖示，避免侵犯版權；改從 IconLibrary 取得圖示
                picOracle.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Oracle 48x48.ico");
                picPostgreSQL.Image = IconManager.GetImage(MyGlobal.IconLibrary, "PostgreSQL 48x48.ico");
                picSQLServer.Image = IconManager.GetImage(MyGlobal.IconLibrary, "SQL Server 48x48.ico");
                picMySQL.Image = IconManager.GetImage(MyGlobal.IconLibrary, "MySQL 48x48.ico");
                picSQLite.Image = IconManager.GetImage(MyGlobal.IconLibrary, "SQLite 48x48.ico");

                _isFormLoadFinished = true;

                if (c1GridDBInfo.HasDataTableRows())
                {
                    btnConnect.Focus();
                }
                else
                {
                    StartNewConnectionProfile(true);
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void InitializeSupportInfo()
        {
            var oracle = "JasonQuery for Oracle is designed for standard Oracle Database connections. The underlying Devart provider supports Oracle 8.0 and later, including newer releases such as Oracle 23c and 26ai. Other Oracle-compatible environments may work, but they have not been officially validated in JasonQuery.";
            var postgreSql = "JasonQuery for PostgreSQL is designed for standard PostgreSQL server connections. The underlying Devart provider supports PostgreSQL server versions 8 through 18. Other PostgreSQL-compatible services may work, but they have not been officially validated in JasonQuery.";
            var sqlServer = "JasonQuery for SQL Server is designed for standard SQL Server connections. The underlying Devart provider supports SQL Server 2000 through 2025, including Express editions.";
            var mySql = "JasonQuery for MySQL is designed for MySQL 3.23 through 9.x. MariaDB and Percona may be compatible, but they have not yet been fully tested in JasonQuery.";
            var sqlite = "JasonQuery for SQLite is designed for standard SQLite connections. The underlying Devart provider supports SQLite engine version 3 and later."; //JasonQuery for SQLite supports SQLite engine version 3 and later.

            _lstSupportInfo.Add(oracle);
            _lstSupportInfo.Add(postgreSql);
            _lstSupportInfo.Add(sqlServer);
            _lstSupportInfo.Add(mySql);
            _lstSupportInfo.Add(sqlite);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                StartNewConnectionProfile(false);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void GetDeleteConnectionSummary(out string connectionName, out string serverOrFile, out string database, out bool isSqlite)
        {
            connectionName = string.Empty;
            serverOrFile = string.Empty;
            database = string.Empty;
            isSqlite = false;

            _languageText = LocalizationHelper.GetLanguageString("Database:", "form", GetType().Name, "object", "lblDatabase", "Text");

            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        connectionName = txtConnectionName_Oracle.Text;
                        serverOrFile = txtServer_Oracle.Text;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        connectionName = txtConnectionName_PostgreSQL.Text;
                        serverOrFile = txtServer_PostgreSQL.Text;
                        database = $"{_languageText} {cboDatabase_PostgreSQL.Text}";
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        connectionName = txtConnectionName_SQLServer.Text;
                        serverOrFile = txtServer_SQLServer.Text;
                        database = $"{_languageText} {cboDatabase_SQLServer.Text}";
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        connectionName = txtConnectionName_MySQL.Text;
                        serverOrFile = txtServer_MySQL.Text;
                        database = $"{_languageText} {cboDatabase_MySQL.Text}";
                        break;
                    }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                GetDeleteConnectionSummary(out var connectionName, out var serverOrFile, out var database, out var isSqlite);

                var temp = LocalizationHelper.GetLanguageString("Are you sure you want to delete the record below?", "form", GetType().Name, "msg", "DeleteRecord", "Text");

                temp += "\r\n" + LocalizationHelper.GetLanguageString("All related records will be deleted.", "form", GetType().Name, "msg", "AllRelatedRecords", "Text");

                _languageText = LocalizationHelper.GetLanguageString("Connection Name:", "form", GetType().Name, "object", "lblConnectionName", "Text");
                temp += $"\r\n\r\n{_languageText} {connectionName}";

                if (isSqlite)
                {
                    _languageText = LocalizationHelper.GetLanguageString("Database File:", "form", GetType().Name, "object", "lblDatabaseFile_SQLite", "Text");
                }
                else
                {
                    _languageText = LocalizationHelper.GetLanguageString("Server:", "form", GetType().Name, "object", "lblServer", "Text");
                }

                temp += $"\r\n{_languageText} {serverOrFile}";

                if (!string.IsNullOrEmpty(database))
                {
                    temp += $"\r\n{database}";
                }

                if (MessageBox.Show(temp, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

                var sbSql = new StringBuilder();

                sbSql.AppendLine($"DELETE FROM SqlHistory WHERE MPID = {lblPID.Text};");
                sbSql.AppendLine($"DELETE FROM SystemConfig WHERE MPID = {lblPID.Text};");
                sbSql.AppendLine($"DELETE FROM DBInfo WHERE PID = {lblPID.Text}");

                JasonQueryRepository.ExecNonQuery(sbSql.ToString());

                //重新載入 DBInfo 資料
                var rowCount = ReloadDbInfoGrid();

                //20260415 刪除後，若沒有任何資料，則自動觸發「新增一筆空白資料」
                if (rowCount == 0)
                {
                    btnNew.PerformClick();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            IsApplicationExitRequested = true;
            Close();
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
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

                //這裡不能用「Application.Exit()」這個關閉方法，因為會有關閉延遲的問題，導致 Updater.exe 在偵測時，會出現「存取被拒」的狀況
                Environment.Exit(0);
            }
            else
            {
                var message = LocalizationHelper.GetLanguageString("File not found:", "form", GetType().Name, "msg", "UpdaterNotFound", "Text");

                MessageBox.Show($"{message}\r\n\r\n{executeName}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkDirectMode_CheckedChanged(object sender, EventArgs e)
        {
            var isDirectMode = chkDirectMode_Oracle.Checked;

            lblSID_Oracle.Visible = isDirectMode;
            txtSID_Oracle.Visible = isDirectMode;
            btnHelp_SID_Oracle.Visible = isDirectMode;
            txtSID_Oracle.BackColor = isDirectMode ? _colorEssentialField : _colorOptionalField;

            if (_isFormLoadFinished && isDirectMode)
            {
                txtSID_Oracle.Focus();
            }
        }

        private void btnCreateShortcut_Click(object sender, EventArgs e)
        {
            var shortcutName = "JasonQuery";
            var targetFilePath = Path.Combine(Application.StartupPath, "JasonQuery.exe");

            var success = WindowsShortcutHelper.TryCreateDesktopShortcut(shortcutName, targetFilePath, Application.StartupPath,
                                                                         shortcutName, targetFilePath, out var errorMessage);

            if (!success)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                return;
            }

            btnCreateShortcut.Enabled = false;

            var message = LocalizationHelper.GetLanguageString("A shortcut has been created on the desktop!", "form", GetType().Name, "msg", "Shortcut", "Text");

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnBrowseFile_Sqlite_Click(object sender, EventArgs e)
        {
            //待建置
        }

        private void CreateOrNewCheckedChanged(object sender, EventArgs e)
        {
            //ChangeSQLitePasswordState();
        }

        private void btnRemoveOrEncrypt_Click(object sender, EventArgs e)
        {
            //待建置
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (var form = new ConnectionExportForm())
            {
                form.dtDbInfo = c1GridDBInfo.GetDataTableSourceOrNull();
                form.ShowDialog();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new ConnectionImportForm())
                {
                    form.ShowDialog();
                }

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnDatabaseSecurity_Click(object sender, EventArgs e)
        {
            using (var form = new JasonQueryDbSecurityForm())
            {
                form.ShowDialog();
            }
        }
    }
}
