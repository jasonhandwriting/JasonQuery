using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    public partial class UpdaterForm : Form
    {
        private WebClient wc;
        private string _ColorString = "#006CBA";
        private int _iDownloadProgressValue;
        private int _iDownloadStatus = -2; //-1=Continue, 0=Cancel, 1=OK
        private bool _bIs64 = IntPtr.Size == 8 ? true : false;

        //20250712
        private string sTag = string.Empty;
        private string sUpdateProduction = string.Empty;
        private string sUpdateBeta = string.Empty;
        private string sDownloadProduction = string.Empty;
        private string sDownloadBeta = string.Empty;

        private ToolTip toolTip = new ToolTip
        {
            ForeColor = Color.Blue,
            BackColor = Color.Gray,
            AutoPopDelay = 5000
        };

        const int MF_BYCOMMAND = 0x00000000;
        const int MF_GRAYED = 0x00000001;
        const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        public UpdaterForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            //20250627 將「關閉」按鈕設為不可用！
            IntPtr hMenu = GetSystemMenu(Handle, false);

            EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);

            var sLogPath = Application.StartupPath + @"\Log";

            if (!Directory.Exists(sLogPath))
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + @"\Log");
                }
                catch
                {
                    sLogPath = Application.StartupPath;
                }
            }

            MyGlobal.sLogFilename = sLogPath + @"\Updater.log";

            if (File.Exists(MyGlobal.sLogFilename))
            {
                try
                {
                    File.Delete(MyGlobal.sLogFilename);
                }
                catch
                {
                    var sNow = DateTime.Now.ToString("yyyyMMddHHmmss");

                    MyGlobal.sLogFilename = sLogPath + $@"\Updater_{sNow}.log";
                }
            }

            var sTemp = " is";

            rdoPreview.Enabled = _bIs64;

            LoadLocalizationXML(MyGlobal.sXmlFilename);
            InitialLabel();

            //Register MessageBoxManager
            var sLangText = MyGlobal.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");

            MessageBoxManager.OK = sLangText;
            sLangText = MyGlobal.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");
            MessageBoxManager.Cancel = sLangText;
            sLangText = MyGlobal.GetLanguageString("&Abort", "Global", "Global", "messagebox", "Abort", "Text");
            MessageBoxManager.Abort = sLangText;
            sLangText = MyGlobal.GetLanguageString("&Retry", "Global", "Global", "messagebox", "Retry", "Text");
            MessageBoxManager.Retry = sLangText;
            sLangText = MyGlobal.GetLanguageString("&Ignore", "Global", "Global", "messagebox", "Ignore", "Text");
            MessageBoxManager.Ignore = sLangText;
            sLangText = MyGlobal.GetLanguageString("&Yes", "Global", "Global", "messagebox", "Yes", "Text");
            MessageBoxManager.Yes = sLangText;
            sLangText = MyGlobal.GetLanguageString("&No", "Global", "Global", "messagebox", "No", "Text");
            MessageBoxManager.No = sLangText;
            MessageBoxManager.Register();

            try
            {
                //變更語系
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(MyGlobal.sLocalization);
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(MyGlobal.sLocalization);
            }
            catch (Exception)
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("english.xml");
            }

            var sNoneExistFileList = CheckFileExist();

            if (!string.IsNullOrEmpty(sNoneExistFileList))
            {
                if ((sNoneExistFileList.Length - sNoneExistFileList.Replace("\r\n", string.Empty).Length) == 2)
                {
                    sTemp = "s are"; //包含換行符號，表示有多個 dll 檔案找不到
                }
                else
                {
                    sTemp = " is";
                }

                sLangText = "The program can't start because the following file" + sTemp + " missing from your computer.\r\n\r\nTry reinstalling the program to fix this problem.";
                MessageBox.Show(sLangText + "\r\n\r\n" + sNoneExistFileList, @"Updater - System Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            picStep1.Image = picUnchecked.Image;
            picStep2.Image = picUnchecked.Image;
            picStep3.Image = picUnchecked.Image;
            picStep4.Image = picUnchecked.Image;

            txtFrom.Tag = Path.GetTempPath() + @"tmpJQUpdate.tmp\";
            lblFrom.Tag = DateTime.Now.ToString("yyyyMMdd") + @"\";
            txtFrom.Text = txtFrom.Tag + string.Empty + lblFrom.Tag;
            txtFrom.ReadOnly = true;
            txtTo.Text = Application.StartupPath + @"\";
            txtTo.ReadOnly = true;
            
            MyGlobal.ApplyLanguageInfo(this, false);

            lblStep2.Tag = lblStep2.Text;
            lblStep3.Tag = lblStep3.Text;

            CheckJasonQueryExecuted();

            tmrCheck.Enabled = true;

            //20250712 取得語系文字
            sUpdateProduction = MyGlobal.GetLanguageString("Update to the Production version!", "form", Name, "msg", "UpdateProduction", "ToolTipText");
            sUpdateBeta = MyGlobal.GetLanguageString("Update to the Beta version!", "form", Name, "msg", "UpdateBeta", "ToolTipText");
            sDownloadProduction = MyGlobal.GetLanguageString("Use default browser to download the \"Production version\" of JasonQuery zip file!\r\nhttp://www.jasonquery.org/JasonQueryUpdate/JasonQuery64.zip", "form", Name, "msg", "DownloadProduction", "ToolTipText");
            sDownloadBeta = MyGlobal.GetLanguageString("Use default browser to download the \"Beta version\" of JasonQuery zip file!\r\nhttp://www.jasonquery.org/JasonQueryUpdate/JasonQuery64Test.zip", "form", Name, "msg", "DownloadBeta", "ToolTipText");

            rdoRelease.Checked = true;

            //20250924
            if (string.Equals(MyGlobal.sEnvironment, "TEST", StringComparison.OrdinalIgnoreCase))
            {
                rdoPreview.Checked = true;
            }
        }

        private static string CheckFileExist()
        {
            var sResult = string.Empty;

            if (!File.Exists(Application.StartupPath + @"\JasonQuery.exe"))
            {
                sResult += "JasonQuery.exe\r\n";
            }
            
            return sResult;
        }

        private void tmrCheck_Tick(object sender, EventArgs e)
        {
            CheckJasonQueryExecuted();
        }

        private string CheckJasonQueryExecuted()
        {
            var sList = string.Empty;

            try
            {
                //20250527 只列出同一個路徑下的 JasonQuery.exe (使用者可以依不同路徑執行不同版本的 JasonQuery，更新時互不干擾)
                Process[] process = Process.GetProcessesByName("JasonQuery");

                foreach (Process p in process)
                {
                    if (!string.IsNullOrEmpty(p.ProcessName) && Path.GetDirectoryName(p.MainModule.FileName) == Application.StartupPath)
                    {
                        sList = p.MainWindowTitle + "\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                //偵測 JasonQuery 64 bit 執行中，但 Updater.exe 是 32 bit
                sList = "JasonQuery x64 (" + ex.Message + ")\r\n";
            }

            txtList.ReadOnly = false;

            if (!string.IsNullOrEmpty(sList) && sList.Length >= 2)
            {
                sList = sList.Substring(0, sList.Length - 2);
                txtList.Text = sList;
            }
            else
            {
                txtList.Text = string.Empty;
            }

            txtList.ReadOnly = true;

            if (string.IsNullOrEmpty(sList))
            {
                picStep1.Image = picChecked.Image;
                lblStep1.ForeColor = Color.Black;
            }
            else
            {
                picStep1.Image = picUnchecked.Image;
                lblStep1.ForeColor = Color.DarkRed;
            }

            return sList;
        }

        private static void LoadLocalizationXML(string sXmlFilename)
        {
            var sXmlFullFilename = Application.StartupPath + @"\localization\" + sXmlFilename;

            if (File.Exists(sXmlFullFilename))
            {
                MyGlobal.dtLocalization = MyGlobal.XmlToDataTable(sXmlFullFilename);
            }

            //20250527 變更語系時，載入常用說明
            MyGlobal.sAnUnexpectedErrorHasOccurred = MyGlobal.GetLanguageString("An unexpected error has occurred.", "Global", "Global", "msg", "AnUnexpectedErrorHasOccurred", "Text");
            MyGlobal.sStackTrace = MyGlobal.GetLanguageString("Stack Trace:", "Global", "Global", "msg", "StackTrace", "Text");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateFile();
        }

        private void UpdateFile()
        {
            var bError = false;
            var sLangText = string.Empty;

            if (!string.IsNullOrEmpty(CheckJasonQueryExecuted()))
            {
                var sMsg = MyGlobal.GetLanguageString("Automatically close all executing JasonQuery?", "form", Name, "msg", "CloseJQAutomatically", "Text") + "\r\n\r\n";

                sMsg += MyGlobal.GetLanguageString("Please confirm whether you need to save or execute the \"Commit/Rollback\" command before pressing the \"Yes\" button!", "form", Name, "msg", "CloseJQAutomatically2", "Text");

                if (MessageBox.Show(sMsg, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                else
                {
                    var sTargetPath = Application.StartupPath + @"\JasonQuery.exe";
                    var processCollection = Process.GetProcesses();

                    foreach (Process p in processCollection)
                    {
                        try
                        {
                            if (string.Equals(p.ProcessName, "JasonQuery", StringComparison.OrdinalIgnoreCase))
                            {
                                string exePath = p.MainModule.FileName;

                                //20250623 相同路徑的 process 才要 Kill
                                if (string.Equals(exePath, sTargetPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    p.Kill();
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }

                    txtList.Text = string.Empty;
                    picStep1.Image = picChecked.Image;
                }
            }

            Cursor = Cursors.WaitCursor;
            tmrCheck.Enabled = false;

            //20240126 變更下載網址 (000webhostapp 免費空間容易被擋住，造成下載失敗)
            const string sURL = "http://www.jasonquery.org/JasonQueryUpdate/";

            //20240126 直接鎖定 x64 版本 (x86 已不維護了)
            //var sRemoteFilename = "JasonQuery" + (_bIs64 ? "64" : "86") + ".zip";

            var sRemoteFilename = rdoPreview.Checked ? "JasonQuery64Test.zip" : "JasonQuery64.zip";
            var sTargetFilename = "JasonQuery.zip";
            var sTargetFolder = txtFrom.Text;
            var sUpdaterExeHasNewVersion = string.Empty;

            MyGlobal.CheckAndCreateDirectory(sTargetFolder);

            _iDownloadProgressValue = 0;
            _iDownloadStatus = -1;
            btnCancel.Enabled = true;
            btnUpdate.Enabled = false;
            btnDownload.Enabled = false;
            btnClose.Enabled = false;
            chkLaunchJQ.Enabled = false;

            wc = new WebClient();

            using (wc)
            {
                wc.DownloadFileCompleted += wc_Completed;
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;

                wc.DownloadFileAsync(new Uri(sURL + sRemoteFilename), sTargetFolder + sTargetFilename);
            }

            while (true)
            {
                Application.DoEvents();
                pbDownloadStatus.Value = _iDownloadProgressValue;

                if (_iDownloadStatus != -1)
                {
                    break;
                }
            }

            Cursor = Cursors.Default;

            if (_iDownloadStatus == 0)
            {
                btnCancel.Click -= delegate
                {
                    wc.CancelAsync();
                };

                pbDownloadStatus.Value = 0;
                btnCancel.Enabled = false;
                tmrCheck.Enabled = true;

                try
                {
                    if (File.Exists(sTargetFolder + sTargetFilename))
                    {
                        //取消下載，刪除檔案
                        File.Delete(sTargetFolder + sTargetFilename);
                    }
                }
                catch (Exception)
                {
                    //
                }
            }
            else
            {
                btnCancel.Enabled = false;

                var fi = new FileInfo(sTargetFolder + sTargetFilename);

                if (fi.Length == 0)
                {
                    bError = true;
                }
                else
                {
                    picStep2.Image = picChecked.Image;

                    //下載OK：解壓縮並更新檔案
                    var zip = new C1ZipFile();

                    TraceLogger.LogInfo($"Open zip file: {sTargetFolder}{sTargetFilename}");

                    zip.Open(sTargetFolder + sTargetFilename); //如果此處出現「C1.C1Zip.ZipFileException: 'Central dir not found.'」的錯誤，非常大的可能是，壓成 7z 格式了！C1Zip 只能處理 zip 格式！

                    TraceLogger.LogInfo($"zip.Entries.Count: {zip.Entries.Count}");

                    foreach (var ze in zip.Entries)
                    {
                        //get source and destination file names
                        var sFileNameSource = ze.FileName;
                        var sFileNameDestination = $@"{sTargetFolder}\{ze.FileName}";

                        //create destination subdirectory if necessary
                        var sFolderDestination = Path.GetDirectoryName(sFileNameDestination);

                        if (!Directory.Exists(sFolderDestination))
                        {
                            try
                            {
                                TraceLogger.LogInfo($"Create Directory: {sFolderDestination}");
                                Directory.CreateDirectory(sFolderDestination);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }

                        //extract the entry
                        try
                        {
                            TraceLogger.LogInfo($"zip.Entries.Extract: {sFileNameSource}, {sFileNameDestination}");
                            zip.Entries.Extract(sFileNameSource, sFileNameDestination);
                        }
                        catch (Exception ex)
                        {
                            sLangText = MyGlobal.GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");
                            MessageBox.Show(sLangText + "\r\n\r\n" + ex.Message, @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            bError = true;
                        }
                    }

                    TraceLogger.LogInfo("");

                    if (!bError)
                    {
                        picStep3.Image = picChecked.Image;

                        var sSourceFolder = sTargetFolder + @"JasonQuery x" + (_bIs64 ? "64" : "86") + @"\";

                        sTargetFolder = txtTo.Text;
                        TraceLogger.LogInfo($"Source Folder: {sSourceFolder}");

                        var uFile = Directory.GetFiles(sSourceFolder);

                        //複製檔案
                        foreach (var filename in uFile)
                        {
                            var fInfo = new FileInfo(filename);
                            var sFilename = Path.GetFileName(filename);

                            if (!File.Exists(sTargetFolder + sFilename))
                            {
                                try
                                {
                                    var sFileNameDestination = sTargetFolder + sFilename;

                                    TraceLogger.LogInfo($"Copy File: {filename} {sFileNameDestination}");
                                    File.Copy(filename, sFileNameDestination);
                                }
                                catch (Exception ex)
                                {
                                    bError = true;
                                    sLangText = MyGlobal.GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");
                                    MessageBox.Show($"{sLangText}\r\n\r\n{sFilename}\r\n\r\n{ex.Message}", @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                var fInfo2 = new FileInfo(sTargetFolder + sFilename);
                                var sfInfoLastWriteTime = fInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                                var sfInfoLastWriteTime2 = fInfo2.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");

                                if (sfInfoLastWriteTime == sfInfoLastWriteTime2 && fInfo.Length == fInfo2.Length)
                                {
                                    //檔案時間、大小一樣，不處理
                                    TraceLogger.LogInfo($"FileName: {sFilename}");
                                    TraceLogger.LogInfo($"fInfo  LastWriteTime: {sfInfoLastWriteTime}");
                                    TraceLogger.LogInfo($"fInfo2 LastWriteTime: {sfInfoLastWriteTime2}");
                                    TraceLogger.LogInfo($"fInfo  Length: {fInfo.Length}");
                                    TraceLogger.LogInfo($"fInfo2 Length: {fInfo2.Length}");
                                    TraceLogger.LogInfo("");
                                }
                                else
                                {
                                    if (fInfo.Name == "Updater.exe")
                                    {
                                        //Updater.exe 有新版本
                                        sUpdaterExeHasNewVersion = $"@copy \"{filename}\" \"{sTargetFolder}{sFilename}\"\r\n";
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //20250527 避開部份 C1 元件，因為它們被 Updater.exe 鎖定了
                                            if (!sFilename.StartsWith("C1."))
                                            {
                                                File.Copy(filename, sTargetFolder + sFilename, true);
                                                TraceLogger.LogInfo($"Copy OK: {filename} → {sTargetFolder + sFilename}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            bError = true;
                                            TraceLogger.LogInfo($"Copy NG: {filename} → {sTargetFolder + sFilename} {ex.Message}");
                                            sLangText = MyGlobal.GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");
                                            MessageBox.Show($"{sLangText}\r\n\r\n{sFilename}\r\n\r\n{ex.Message}", @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }

                            Application.DoEvents();
                        }

                        sSourceFolder += @"localization\";
                        sTargetFolder += @"localization\";

                        if (!Directory.Exists(sTargetFolder))
                        {
                            try
                            {
                                Directory.CreateDirectory(sTargetFolder);
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }

                        uFile = Directory.GetFiles(sSourceFolder);

                        foreach (var filename in uFile)
                        {
                            var fInfo = new FileInfo(filename);

                            if (!File.Exists(sTargetFolder + Path.GetFileName(filename)))
                            {
                                try
                                {
                                    File.Copy(filename, sTargetFolder + Path.GetFileName(filename));
                                }
                                catch (Exception ex)
                                {
                                    sLangText = MyGlobal.GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");
                                    MessageBox.Show(sLangText + "\r\n\r\n" + Path.GetFileName(filename) + "\r\n\r\n" + ex.Message, @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    bError = true;
                                }
                            }
                            else
                            {
                                var fInfo2 = new FileInfo(sTargetFolder + Path.GetFileName(filename));

                                if (fInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") == fInfo2.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") && fInfo.Length == fInfo2.Length)
                                {
                                    //檔案時間、大小一樣，不處理
                                }
                                else
                                {
                                    try
                                    {
                                        File.Copy(filename, sTargetFolder + Path.GetFileName(filename), true);
                                    }
                                    catch (Exception ex)
                                    {
                                        sLangText = MyGlobal.GetLanguageString("An error has occurred.", "Global", "Global", "msg", "AnErrorHasOccurred", "Text");
                                        MessageBox.Show(sLangText + "\r\n\r\n" + Path.GetFileName(filename) + "\r\n\r\n" + ex.Message, @"JasonQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                        bError = true;
                                    }
                                }
                            }

                            Application.DoEvents();
                        }

                        if (!bError)
                        {
                            picStep4.Image = picChecked.Image;
                            sLangText = MyGlobal.GetLanguageString("Update completed!", "form", Name, "msg", "UpdateCompleted", "Text") + "\r\n\r\n";

                            if (chkLaunchJQ.Checked)
                            {
                                sLangText += MyGlobal.GetLanguageString("Press OK to close the program and automatically launch JasonQuery.", "form", Name, "msg", "PressOKAndLaunchJasonQuery", "Text");
                            }
                            else
                            {
                                sLangText += MyGlobal.GetLanguageString("Press OK to close the program.", "form", Name, "msg", "PressOK", "Text");
                            }

                            MessageBox.Show(sLangText, @"Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            ProcessStartInfo exeInfo;

                            if (chkLaunchJQ.Checked)
                            {
                                exeInfo = new ProcessStartInfo(Application.StartupPath + @"\JasonQuery.exe")
                                {
                                    WorkingDirectory = Application.StartupPath + @"\",
                                    CreateNoWindow = true,
                                    UseShellExecute = false
                                };

                                Process.Start(exeInfo);
                            }

                            //產生批次檔：更新 Updater.exe 並刪除暫存檔案

                            var sBatchContent = "@echo off\r\n@ping localhost -n 5 -w 1000 > nul\r\n"
                                                + sUpdaterExeHasNewVersion
                                                + "@cd \"" + txtFrom.Tag + "\"\r\n"
                                                + "@del " + lblFrom.Tag + "*.* /s /q\r\n"
                                                + "@rd \"" + lblFrom.Tag + "JasonQuery x64\\localization\"\r\n"
                                                + "@rd \"" + lblFrom.Tag + "JasonQuery x64\"\r\n"
                                                + "@rd " + lblFrom.Tag.ToString().Replace("\\", string.Empty) + "\r\n"
                                                + "@del *.bat /q";

                            MyGlobal.WriteContentToFile(sBatchContent, txtFrom.Tag + "update.bat", FileMode.CreateNew);

                            exeInfo = new ProcessStartInfo("cmd.exe", "/c \"" + txtFrom.Tag + "update.bat\"")
                            {
                                WorkingDirectory = txtFrom.Text,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };

                            Process.Start(exeInfo);

                            Close();
                        }
                    }
                }
            }

            Cursor = Cursors.Default;

            if (!bError)
            {
                return;
            }

            _iDownloadStatus = -2;
            btnCancel.Enabled = false;
            btnUpdate.Enabled = true;
            btnDownload.Enabled = true;
            btnClose.Enabled = true;
            chkLaunchJQ.Enabled = true;

            sLangText = MyGlobal.GetLanguageString("Update failed!", "form", Name, "msg", "UpdateFailed", "Text") + "\r\n\r\n" + MyGlobal.GetLanguageString("Please try again or update manually by yourself!", "form", Name, "msg", "FailedTryAgain", "Text") + "\r\n\r\n";

            var sTemp0 = MyGlobal.GetLanguageString("How to manually update JasonQuery to the latest version:", "form", "frmCheckForUpdates", "msg", "HowToUpdate0", "Text") + "\r\n";
            var sTemp1 = MyGlobal.GetLanguageString("1. Download the latest JasonQuery.7z", "form", "frmCheckForUpdates", "msg", "HowToUpdate1", "Text") + "\r\n";
            var sTemp2 = MyGlobal.GetLanguageString("2. Close all open JasonQuery", "form", "frmCheckForUpdates", "msg", "HowToUpdate2", "Text") + "\r\n";
            var sTemp3 = MyGlobal.GetLanguageString("3. Unzip \"JasonQuery x64\" or \"JasonQuery x86\" to the folder where JasonQuery is currently located (overwrite all, JasonQuery.7z does not include JasonQuery.db)", "form", "frmCheckForUpdates", "msg", "HowToUpdate3", "Text") + "\r\n";
            var sTemp4 = MyGlobal.GetLanguageString("4. Run JasonQuery", "form", "frmCheckForUpdates", "msg", "HowToUpdate4", "Text");

            MessageBox.Show(sLangText + sTemp0 + sTemp1 + sTemp2 + sTemp3 + sTemp4, @"Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _iDownloadProgressValue = e.ProgressPercentage;

            if (_iDownloadStatus == 0)
            {
                wc.CancelAsync();
            }
        }

        private void wc_Completed(object sender, AsyncCompletedEventArgs e)
        {
            _iDownloadStatus = e.Cancelled ? 0 : 1;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _iDownloadStatus = 0;
            btnUpdate.Enabled = true;
            btnClose.Enabled = true;
            chkLaunchJQ.Enabled = true;

            InitialLabel();
        }

        private void InitialLabel()
        {
            lblStep1.ForeColor = Color.Black;
            lblStep2.ForeColor = Color.Black;
            lblStep3.ForeColor = Color.Black;
            lblStep4.ForeColor = Color.Black;
        }

        private void frmUpdater_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBoxManager.Unregister();
        }

        private void rdoUpdateProduction_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoRelease.Checked)
            {
                lblStep2.Text = lblStep2.Tag.ToString().Replace("JasonQuery.zip", "JasonQuery" + (_bIs64 ? "64" : "86") + ".zip");
                lblStep3.Text = lblStep3.Tag.ToString().Replace("JasonQuery.zip", "JasonQuery" + (_bIs64 ? "64" : "86") + ".zip");

                GetRichText(txtStep2, lblStep2.Tag.ToString(), "JasonQuery" + (_bIs64 ? "64" : "86") + ".zip");
                GetRichText(txtStep3, lblStep3.Tag.ToString(), "JasonQuery" + (_bIs64 ? "64" : "86") + ".zip");

                //20250712
                toolTip.SetToolTip(btnUpdate, sUpdateProduction);
                toolTip.SetToolTip(btnDownload, sDownloadProduction);
            }
        }

        private void rdoUpdateBeta_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoPreview.Checked)
            {
                lblStep2.Text = lblStep2.Tag.ToString().Replace("JasonQuery.zip", "JasonQuery64Test.zip");
                lblStep3.Text = lblStep3.Tag.ToString().Replace("JasonQuery.zip", "JasonQuery64Test.zip");

                GetRichText(txtStep2, lblStep2.Tag.ToString(), "JasonQuery64Test.zip");
                GetRichText(txtStep3, lblStep3.Tag.ToString(), "JasonQuery64Test.zip");

                //20250712
                toolTip.SetToolTip(btnUpdate, sUpdateBeta);
                toolTip.SetToolTip(btnDownload, sDownloadBeta);
            }
        }

        private void GetRichText(RichTextBox txtInfo, string sText1, string sText2 = "")
        {
            var font = new Font("Microsoft JhengHei", 9);

            txtInfo.Text = string.Empty;

            var parts = sText1.Split(new[] { "JasonQuery.zip" }, StringSplitOptions.None);

            txtInfo.AppendText(parts[0].Trim() + " ", Color.Black, font);

            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                txtInfo.AppendText(sText2.Trim(), Color.Blue, font);
                txtInfo.AppendText(" " + parts[1].Trim(), Color.Black, font);
            }

            txtInfo.Refresh();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (rdoRelease.Checked)
            {
                Process.Start("http://www.jasonquery.org/JasonQueryUpdate/JasonQuery64.zip");
            }
            else
            {
                Process.Start("http://www.jasonquery.org/JasonQueryUpdate/JasonQuery64Test.zip");
            }
        }
    }

    public static class RichTextBoxColorExtensions
    {
        public static void AppendText(this RichTextBox rtb, string text, Color color, Font font, bool isNewLine = false)
        {
            rtb.SuspendLayout();
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;

            rtb.SelectionColor = color;
            rtb.SelectionFont = font;
            rtb.AppendText(isNewLine ? $"{text}{ Environment.NewLine}" : text);
            rtb.SelectionColor = rtb.ForeColor;
            rtb.ScrollToCaret();
            rtb.ResumeLayout();
        }
    }
}