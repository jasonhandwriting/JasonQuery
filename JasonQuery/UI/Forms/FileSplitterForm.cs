using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class FileSplitterForm : Form
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public FileSplitterForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                cboSizeLimit.SelectedIndex = 0;

                LocalizationHelper.ApplyLanguageInfo(this, false);

                //20190925 針對 TextBox, 顯示 Hint 功能 (只是顯示，不會改變 .Text 值)
                var originalFile = LocalizationHelper.GetLanguageString("drag file here!", "form", GetType().Name, "object", "txtOriginalFile", "ToolTipText");
                var destinationFolder = LocalizationHelper.GetLanguageString("drag folder here!", "form", GetType().Name, "object", "txtDestinationFolder", "ToolTipText");

                SendMessage(txtOriginalFile.Handle, EM_SETCUEBANNER, 0, originalFile);
                SendMessage(txtDestinationFolder.Handle, EM_SETCUEBANNER, 0, destinationFolder);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtSourceFile_DragDrop(object sender, DragEventArgs e)
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

            if (File.Exists(a.GetValue(0).ToString()))
            {
                txtOriginalFile.Text = a.GetValue(0).ToString();

                var fi = new FileInfo(a.GetValue(0).ToString());

                lblFileSize.Text = $@"{fi.Length:N0} Bytes";
            }
            else
            {
                txtOriginalFile.Text = string.Empty;
                lblFileSize0.Text = string.Empty;
                lblFileSize.Text = string.Empty;
            }
        }

        private void txtSourceFile_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            var of = new OpenFileDialog
            {
                Multiselect = false,
                Title = LocalizationHelper.GetLanguageString("Select a large text file...", "form", GetType().Name, "msg", "SelectLargeFile", "Text"),
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (of.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var fileName = of.FileName;

            if (File.Exists(fileName))
            {
                txtOriginalFile.Text = fileName;

                var fi = new FileInfo(fileName);

                lblFileSize.Text = $@"{fi.Length:N0} Bytes";
            }
            else
            {
                txtOriginalFile.Text = string.Empty;
                lblFileSize0.Text = string.Empty;
                lblFileSize.Text = string.Empty;
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var browseFolder = new FolderBrowserDialog())
            {
                browseFolder.ShowNewFolderButton = true;
                browseFolder.Description = LocalizationHelper.GetLanguageString("Select Destination Folder...", "form", GetType().Name, "msg", "SelectDestinationFolder", "Text");
                MyGlobal.GlobalTempDialog = "BrowseForFolder";

                var result = browseFolder.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(browseFolder.SelectedPath))
                {
                    var selectedPath = browseFolder.SelectedPath;
                    var temp = selectedPath.EndsWith(@"\", StringComparison.Ordinal) ? string.Empty : @"\";

                    txtDestinationFolder.Text = $"{selectedPath}{temp}";
                }
            }
        }

        private void txtDestinationFolder_DragDrop(object sender, DragEventArgs e)
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

            //判斷是否為目錄
            if (Directory.Exists(a.GetValue(0).ToString()))
            {
                txtDestinationFolder.Text = a.GetValue(0) + (a.GetValue(0).ToString().Substring(a.GetValue(0).ToString().Length - 1, 1) == @"\" ? string.Empty : @"\");
            }
            else
            {
                txtDestinationFolder.Text = string.Empty;
            }
        }

        private void txtDestinationFolder_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtSplitSize_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtSplitSize_Leave(object sender, EventArgs e)
        {
            int.TryParse(txtSplitSize.Text, out var splitSize);
            txtSplitSize.Text = splitSize > 0 ? splitSize.ToString() : string.Empty;
        }

        private string CheckLargeFileContent(string fileName)
        {
            var endOfLineStyle = string.Empty;
            var encode = TextEncodingDetector.GetTextEncode(fileName, ref endOfLineStyle);

            if (encode == "ERROR") //指定的檔案被鎖定，開啟失敗
            {
                return "ERROR";
            }

            if (!encode.StartsWith("Binary", StringComparison.Ordinal))
            {
                return encode;
            }

            var languageText = LocalizationHelper.GetLanguageString("It seems to be a binary file.", "form", GetType().Name, "msg", "SeemsBinaryFile", "Text");

            var temp = $"{languageText}\r\n\r\n";

            if (encode.Length > 6) //可辨識的 binary file
            {
                var temp2 = LocalizationHelper.GetLanguageString("It seems to be a", "form", GetType().Name, "msg", "SeemsBeA", "Text");
                var temp3 = LocalizationHelper.GetLanguageString("file.", "form", GetType().Name, "msg", "File.", "Text");
                var temp4 = encode.Substring(7, encode.Length - 7);

                temp = $"{temp2} {temp4} {temp3}\r\n\r\n";
            }

            languageText = LocalizationHelper.GetLanguageString("The following file could not be opened because it contains characters that could not be interpreted.", "form", GetType().Name, "msg", "FileContainsBinaryData", "Text");
            MessageBox.Show($"{languageText}\r\n\r\n{temp}{fileName}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return "ERROR";
        }

        private bool CheckSplitCondition()
        {
            var fileSizeLimit = "000000";

            if (string.IsNullOrEmpty(txtOriginalFile.Text) || !File.Exists(txtOriginalFile.Text))
            {
                var languageText = LocalizationHelper.GetLanguageString("Please select an existing source file.", "form", GetType().Name, "msg", "SelectSource", "Text");

                MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                btnBrowseFile.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtDestinationFolder.Text))
            {
                var languageText = LocalizationHelper.GetLanguageString("You must indicate a destination folder for your piece files.", "form", GetType().Name, "msg", "SelectFolder", "Text");

                MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                btnBrowseFolder.Focus();
                return false;
            }

            if (!Directory.Exists(txtDestinationFolder.Text))
            {
                var languageText = LocalizationHelper.GetLanguageString("Please select an existing destination folder.", "form", GetType().Name, "msg", "SelectExistingFolder", "Text");

                MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                btnBrowseFolder.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtSplitSize.Text))
            {
                var languageText = LocalizationHelper.GetLanguageString("Please define the size of a piece file.", "form", GetType().Name, "msg", "DefineSize", "Text");

                MessageBox.Show(languageText, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtSplitSize.Focus();
                return false;
            }

            if (cboSizeLimit.Text == "KBytes")
            {
                fileSizeLimit = "000";
            }

            double.TryParse(lblFileSize.Text.Replace(",", string.Empty).Replace(" Bytes", string.Empty), out var length1);
            double.TryParse($"{txtSplitSize.Text}{fileSizeLimit}", out var lenght2);

            if (!(length1 < lenght2))
            {
                return true;
            }

            var languageText2 = LocalizationHelper.GetLanguageString("Please decrease the size of blocked pieces so it is lower than the size of the original file.", "form", GetType().Name, "msg", "DecreaseSize", "Text");

            MessageBox.Show(languageText2, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            txtSplitSize.Focus();
            return false;
        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            var fileSize = 0;
            var fileIndex = 1;
            var fileSizeLimit = Convert.ToInt32($"{txtSplitSize.Text}000000");
            var list = new List<string>();
            var fileName = txtOriginalFile.Text;

            if (!CheckSplitCondition())
            {
                return;
            }

            var encode = CheckLargeFileContent(fileName);

            if (cboSizeLimit.Text == "KBytes")
            {
                fileSizeLimit = Convert.ToInt32($"{txtSplitSize.Text}000");
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileNameExtension = fileName.Replace(Path.GetDirectoryName(fileName), string.Empty)
                                            .Replace(fileNameWithoutExtension, string.Empty)
                                            .Replace(@"\", string.Empty);
            var temp0 = lblFileSize.Text.Replace(",", string.Empty)
                                         .Replace(" Bytes", string.Empty);

            double.TryParse(temp0, out var fileSizeTemp);

            var fileSizeLimitTemp = (double)fileSizeLimit;

            //計算分割檔的預計數量 Math.Ceiling(target);
            int.TryParse(Math.Ceiling(fileSizeTemp / fileSizeLimitTemp).ToString(), out var fileMaxIndex);

            if (encode == "ERROR")
            {
                return;
            }

            //確認是否已經有存在已分割過的檔案名稱 (是否要繼續？)
            string temp1;
            string temp2;

            foreach (var file in Directory.GetFiles(txtDestinationFolder.Text))
            {
                temp1 = file;
                int.TryParse(Path.GetFileNameWithoutExtension(temp1).Replace(fileNameWithoutExtension, string.Empty).Replace(".", string.Empty), out var iTemp);

                if (iTemp <= 0 || iTemp >= fileMaxIndex)
                {
                    continue;
                }

                var languageText = LocalizationHelper.GetLanguageString("Parts of the file have been found in the destination folder.", "form", GetType().Name, "msg", "FileBeenFound", "Text");

                temp2 = LocalizationHelper.GetLanguageString("Some of the them will be deleted!", "form", GetType().Name, "msg", "SomeWillBeDelete", "Text");

                var temp3 = LocalizationHelper.GetLanguageString("Continue anyway?", "form", GetType().Name, "msg", "ContinueAnyway", "Text");
                var result0 = MessageBox.Show($"{languageText}\r\n{temp2}\r\n\r\n{temp3}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result0 == DialogResult.No)
                {
                    return;
                }

                break;
            }

            Cursor = Cursors.WaitCursor;
            progressBar1.Visible = true;
            progressBar1.Value = 0;

            try
            {
                using (var streamReader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string[] lines;
                    string contents;
                    var baseValue = 1;

                    int.TryParse(streamReader.Length.ToString(), out var fileLength);

                    if (fileLength == 0) //超過 int 限制
                    {
                        baseValue = 500;
                        int.TryParse((streamReader.Length / baseValue).ToString(), out fileLength);
                    }

                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = fileLength;

                    fileLength = 0;

                    using (var reader = new StreamReader(streamReader))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            list.Add(line);

                            fileSize += line.Length;
                            fileLength += (int)Math.Truncate((double)(line.Length / baseValue));

                            progressBar1.Value = fileLength;

                            if (fileSize <= fileSizeLimit)
                            {
                                continue;
                            }

                            lines = list.ToArray();
                            contents = string.Join("\r\n", lines);

                            var temp4 = fileIndex.ToString("D" + fileMaxIndex.ToString().Length);

                            temp1 = $"{txtDestinationFolder.Text}{fileNameWithoutExtension}.{temp4}{fileNameExtension}";

                            try
                            {
                                if (File.Exists(temp1))
                                {
                                    File.Delete(temp1);
                                }

                                TextEngine.WriteContentToFile(contents, temp1, TextEncodes.UTF8);
                            }
                            catch (Exception)
                            {
                                var languageText = LocalizationHelper.GetLanguageString("Cannot access the file below because it is being used by another process.", "form", GetType().Name, "msg", "UsedByAnother", "Text");

                                temp2 = LocalizationHelper.GetLanguageString("Skip this file and continue anyway?", "form", GetType().Name, "msg", "SkipAndContinue", "Text");

                                var result1 = MessageBox.Show($"{languageText}\r\n\r\n{temp1}\r\n\r\n{temp2}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (result1 == DialogResult.No)
                                {
                                    return;
                                }
                            }

                            //還原初始值，重新產生下一個檔案
                            fileSize = 0;
                            fileIndex++;
                            list = new List<string>();
                        }
                    }

                    //最後未超過 iFileLimit 的部份，要再寫入檔案
                    lines = list.ToArray();
                    contents = string.Join("\r\n", lines);

                    var temp5 = fileIndex.ToString("D" + fileMaxIndex.ToString().Length);

                    temp1 = $"{txtDestinationFolder.Text}{fileNameWithoutExtension}.{temp5}{fileNameExtension}";

                    try
                    {
                        if (File.Exists(temp1))
                        {
                            File.Delete(temp1);
                        }

                        TextEngine.WriteContentToFile(contents, temp1, TextEncodes.UTF8);
                    }
                    catch (Exception)
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Cannot access the file below because it is being used by another process.", "form", GetType().Name, "msg", "UsedByAnother", "Text");

                        temp2 = LocalizationHelper.GetLanguageString("Skip this file and continue anyway?", "form", GetType().Name, "msg", "SkipAndContinue", "Text");

                        var result2 = MessageBox.Show($"{languageText}\r\n\r\n{temp1}\r\n\r\n{temp2}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result2 == DialogResult.No)
                        {
                            return;
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
                progressBar1.Visible = false;
            }

            Cursor = Cursors.Default;

            var languageText2 = LocalizationHelper.GetLanguageString("Splitting completed.", "form", GetType().Name, "msg", "SplittingCompleted", "Text");

            temp2 = LocalizationHelper.GetLanguageString("Would you like to open the destination folder?", "form", GetType().Name, "msg", "OpenDestinationFolder", "Text");

            var result = MessageBox.Show($"{languageText2}\r\n\r\n{temp2}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result != DialogResult.Yes)
            {
                return;
            }

            var temp6 = 1.ToString("D" + fileMaxIndex.ToString().Length);

            temp1 = $"{txtDestinationFolder.Text}{fileNameWithoutExtension}.{temp6}{fileNameExtension}";

            if (File.Exists(temp1))
            {
                temp1 = $"/select, {temp1}";
                Process.Start("explorer.exe", temp1); //自動移至指定的路徑下的指定檔案
            }
            else
            {
                var languageText = LocalizationHelper.GetLanguageString("The following file could not be found.", "form", GetType().Name, "msg", "FollowingFileNotFound", "Text");

                temp2 = LocalizationHelper.GetLanguageString("Please check the file name and try again.", "form", GetType().Name, "msg", "CheckFileAndTry", "Text");
                MessageBox.Show($"{languageText}\r\n{temp2}\r\n\r\n{temp1}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
