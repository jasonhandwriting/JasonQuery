using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class UpdateForm : Form
    {
        public bool IsCheckOnStartup { get; set; }

        private readonly MessageForm _messageForm = new MessageForm();
        private const string EnvironmentProd = "PROD";
        private const string EnvironmentTest = "TEST";

        public UpdateForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                Hide();

                LocalizationHelper.ApplyLanguageInfo(this, false);

                var toolTip1 = new ToolTip
                {
                    AutoPopDelay = 5000
                };

                var languageText = string.Empty;

                languageText = LocalizationHelper.GetLanguageString("Use JasonQuery to download the file directly", "form", GetType().Name, "msg", "DownloadWithJasonQuery", "Text");
                toolTip1.SetToolTip(lnkDownloadJasonQuery64, languageText);

                languageText = LocalizationHelper.GetLanguageString("How to update manually", "form", GetType().Name, "object", "btnHelp_HowToUpdate", "ToolTipText");
                toolTip1.SetToolTip(btnHelp_HowToUpdate, languageText);

                lblLength.Text = grpDownloadInfo.Text;
                grpDownloadInfo.Text += "   ";
                btnHelp_HowToUpdate.Location = new Point(grpDownloadInfo.Left + lblLength.Width - 8, btnHelp_HowToUpdate.Top);

                if (IsCheckOnStartup)
                {
                    //顯示 MessageForm
                    _messageForm.TopLevel = true;

                    var message = string.Empty;

                    message = LocalizationHelper.GetLanguageString("Check for Updates", "form", GetType().Name, "msg", "CheckforUpdatesTitle", "Text");
                    _messageForm.Caption = message;
                    message = LocalizationHelper.GetLanguageString("check for updates on startup...", "form", GetType().Name, "msg", "CheckforUpdatesInfo", "Text");
                    _messageForm.Info = message;
                    _messageForm.StartPosition = FormStartPosition.CenterScreen;
                    _messageForm.IsNeedToMovePosition = true; //20231111 加入此變數，MessageForm 顯示於螢幕中央時，視窗的位置再往上調整一些，如果有錯誤發生時，MessageBox 才不會剛好擋住 MessageForm！
                    _messageForm.Show();
                    _messageForm.Refresh();

                    CheckForUpdates1();
                }
                else
                {
                    Show();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            CheckForUpdates1();
        }

        private void CheckForUpdates1()
        {
            //20240315 此處不能用 https 網址，否則 CheckForUpdates2 函數會引發例外錯誤 (無法建立 SSL 通道)
            const string address = "http://www.jasonquery.org/JasonQueryUpdate/";

            try
            {
                Cursor = Cursors.WaitCursor;

                var fileName = Path.GetFileName(Path.GetTempFileName()).Replace(".", string.Empty);
                var targetFileName = $"{fileName}_jq_{DateTime.Now:yyyyMMddHHmmss}.txt";

                //是否為預覽版本
                //sResult 若為空值，表示目前使用的版本為最新的
                //sResult 若有兩個小數點，且最後不是 .0，表示目前使用的版本為測試版
                var result = CheckForUpdates2(AppConfigHelper.LocalVersion, address, "jq.txt", targetFileName);

                var isBetaVersion = IsBetaVersion(AppConfigHelper.LocalVersion);
                var versionDescript = LocalizationHelper.GetLanguageString("(Production Version)", "form", GetType().Name, "msg", "ProductionVersion", "Text");

                if (isBetaVersion)
                {
                    versionDescript = LocalizationHelper.GetLanguageString("(Beta Version)", "form", GetType().Name, "msg", "BetaVersion", "Text");
                }

                btnCheckForUpdates.Visible = false;
                lblInfo.Visible = true;
                lblInfo2.Visible = true;

                _messageForm.Hide();
                _messageForm.Close();

                if (!string.IsNullOrEmpty(result))
                {
                    lblInfo.Text = LocalizationHelper.GetLanguageString("A new version of JasonQuery is available:", "form", GetType().Name, "object", "lblInfoHasNewVersion", "Text");
                    lblInfo2.Text = $"{result} {versionDescript}";

                    lblInfo.ForeColor = Color.DarkGreen;
                    lblInfo2.ForeColor = Color.DarkGreen;

                    if (IsCheckOnStartup)
                    {
                        Show();
                    }
                }
                else
                {
                    if (IsCheckOnStartup)
                    {
                        Cursor = Cursors.Default;

                        //先 Hide() 再 Dispose()，UpdateForm 不會有感覺，很順！
                        Hide();
                        Dispose();
                    }

                    lblInfo.Text = LocalizationHelper.GetLanguageString("You are using the latest version of JasonQuery.", "form", GetType().Name, "object", "lblInfoLatest", "Text");
                    lblInfo2.Text = $"{AppConfigHelper.LocalVersion}{versionDescript}";
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static string CheckForUpdates2(string localVersion, string address, string fileName, string targetFileName)
        {
            var path = Path.GetTempPath();

            //取得網路所發佈的版號
            var versionInfo = CheckForUpdates.GetUpdateInfo(address, fileName, path, targetFileName);

            try
            {
                File.Delete($"{path}{targetFileName}");
            }
            catch (Exception)
            {
                //do nothing
            }

            if (string.IsNullOrEmpty(versionInfo))
            {
                //20250510 無法取得網路上的版號，返回 0 (可能原因：網路不通、jq.txt 的網址連不上)
                return string.Empty;
            }

            //20250410 等於 1，表示網路上版本較新！
            return TextHelper.CompareVersions(versionInfo, localVersion) == 1 ? versionInfo : string.Empty;
        }

        private void lnkCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkCheck.LinkVisited = true;

            Process.Start("http://www.jasonquery.org/JasonQueryUpdate/jq.txt");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lnkDownloadJasonQuery64_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkDownloadJasonQuery64.LinkVisited = true;

            Process.Start("http://www.jasonquery.org/JasonQueryUpdate/JasonQuery64.zip");
        }

        private void lnkDownloadJasonQuery64Test_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkDownloadJasonQuery64Test.LinkVisited = true;

            Process.Start("http://www.jasonquery.org/JasonQueryUpdate/JasonQuery64Test.zip");
        }

        private void btnHelp_HowToUpdate_Click(object sender, EventArgs e)
        {
            var temp0 = LocalizationHelper.GetLanguageString("How to manually update JasonQuery to the latest version:", "form", GetType().Name, "msg", "HowToUpdate0", "Text") + "\r\n\r\n";
            var temp1 = LocalizationHelper.GetLanguageString("1. Download the latest JasonQuery.7z", "form", GetType().Name, "msg", "HowToUpdate1", "Text") + "\r\n";
            var temp2 = LocalizationHelper.GetLanguageString("2. Close all open JasonQuery", "form", GetType().Name, "msg", "HowToUpdate2", "Text") + "\r\n";
            var temp3 = LocalizationHelper.GetLanguageString("3. Unzip \"JasonQuery x64\" or \"JasonQuery x86\" to the folder where JasonQuery is currently located (overwrite all, JasonQuery.7z does not include JasonQuery.db)", "form", GetType().Name, "msg", "HowToUpdate3", "Text") + "\r\n";
            var temp4 = LocalizationHelper.GetLanguageString("4. Run JasonQuery", "form", GetType().Name, "msg", "HowToUpdate4", "Text");

            MessageBox.Show($"{temp0}{temp1}{temp2}{temp3}{temp4}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            var executeName = $@"{Application.StartupPath}\Updater.exe";

            if (File.Exists(executeName))
            {
                var environment = ResolveUpdateEnvironment(AppConfigHelper.LocalVersion);

                var infoExe = new ProcessStartInfo
                {
                    FileName = executeName,
                    WorkingDirectory = $@"{Application.StartupPath}\",
                    Arguments = $"{LocalizationHelper.LocalizationCode}|{LocalizationHelper.XmlFileName}|{environment}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(infoExe);

                if (string.IsNullOrWhiteSpace(DatabaseSqlExecutor.DbConnectionString))
                {
                    Environment.Exit(0);
                }

                Close();
            }
            else
            {
                var temp = LocalizationHelper.GetLanguageString("File not found:", "form", GetType().Name, "msg", "UpdaterNotFound", "Text");

                MessageBox.Show($"{temp}\r\n\r\n{executeName}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ResolveUpdateEnvironment(string version)
        {
            return IsBetaVersion(version) ? EnvironmentTest : EnvironmentProd;
        }

        private static bool IsBetaVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            return version.Split('.').Length == 3 && !version.EndsWith(".0", StringComparison.Ordinal);
        }
    }
}