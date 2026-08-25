using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool LoadFile(string fileName, out bool isLargeFile)
        {
            isLargeFile = false;

            var encodingName = string.Empty;
            var endOfLineStyle = string.Empty;

            try
            {
                if (!File.Exists(fileName))
                {
                    ShowOpenFileNotFoundMessage(fileName);
                    return false;
                }

                if (!TryConfirmOpenLargeFile(fileName, out isLargeFile))
                {
                    return false;
                }

                Application.UseWaitCursor = true;
                editor.Text = string.Empty;

                var loadResult = TextFileLoadHelper.Load(fileName, IsBackupFile(fileName));

                encodingName = loadResult.EncodingName;
                endOfLineStyle = loadResult.EndOfLineStyle;

                ApplyFileLoadEncodingStatus(encodingName, endOfLineStyle);

                if (!loadResult.IsSuccess)
                {
                    if (loadResult.IsBinaryFile)
                    {
                        ShowBinaryFileOpenFailedMessage(fileName, loadResult.BinaryFileType);
                    }

                    return false;
                }

                ApplyLoadedFileText(loadResult.Text);

                return true;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return false;
            }
            finally
            {
                ApplyFileLoadEncodingStatus(encodingName, endOfLineStyle);
                ResetFileLoadExecutionStatus();

                Application.UseWaitCursor = false;
            }
        }

        private bool TryConfirmOpenLargeFile(string fileName, out bool isLargeFile)
        {
            isLargeFile = false;

            var fileLength = new FileInfo(fileName).Length;

            if (!IsLargeFile(fileLength, out var fileLimitAlarm))
            {
                return true;
            }

            if (!ConfirmOpenLargeFile(fileName, fileLimitAlarm))
            {
                return false;
            }

            isLargeFile = true;
            return true;
        }

        private static bool IsLargeFile(long fileLength, out int fileLimitAlarm)
        {
            int fileLimit;

            if (Environment.Is64BitProcess)
            {
                fileLimitAlarm = 300;
                fileLimit = 303000000;
            }
            else
            {
                fileLimitAlarm = 50;
                fileLimit = 53000000;
            }

            return fileLength > fileLimit;
        }

        private bool ConfirmOpenLargeFile(string fileName, int fileLimitAlarm)
        {
            var openLargeFile1 = LocalizationHelper.GetLanguageString("JasonQuery may not open this file because it is larger than {qty} MB.", "form", GetType().Name, "msg", "OpenLargeFile1", "Text");
            var openLargeFile2 = LocalizationHelper.GetLanguageString("Do you still want to open this file?", "form", GetType().Name, "msg", "OpenLargeFile2", "Text");
            var openLargeFile3 = LocalizationHelper.GetLanguageString("(You may get the exception error of 'System.OutOfMemoryException'.)", "form", GetType().Name, "msg", "OpenLargeFile3", "Text");
            var title = LocalizationHelper.GetLanguageString("Open File", "form", GetType().Name, "msg", "OpenFile", "Text");
            var message = $"{openLargeFile1}\r\n\r\n{fileName}\r\n\r\n".Replace("{qty}", fileLimitAlarm.ToString());

            message = $"{message}{openLargeFile2}\r\n{openLargeFile3}";

            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private static bool IsBackupFile(string fileName)
        {
            return fileName.Contains(BuildBackupFileNameMarker());
        }

        private static string BuildBackupFileNameMarker()
        {
            return ($"{MyGlobal.DomainUser.Replace("\\", "@")}{MyGlobal.Separator00A1}{JasonQueryRepository.DbMotherPid}{MyGlobal.Separator00A1}").Replace("'", "''");
        }

        private void ApplyLoadedFileText(string text)
        {
            editor.Text = text;
            editor.SelectionStart = 0;
            editor.SelectionEnd = 0;
            editor.ScrollCaret();
        }

        private void ApplyFileLoadEncodingStatus(string encodingName, string endOfLineStyle)
        {
            lblEncode.Text = encodingName;
            lblEndOfLineStyle.Text = endOfLineStyle;
        }

        private void ResetFileLoadExecutionStatus()
        {
            lblExecTime.Text = $"{_execTime} 00:00.000";
            lblQueryTime.Text = $"{_queryTime} 00:00.000";
            lblRows.Text = $"0 {_rows}";
        }

        private void ShowOpenFileNotFoundMessage(string fileName)
        {
            var title = LocalizationHelper.GetLanguageString("Open File", "form", GetType().Name, "msg", "OpenFile", "Text");
            var message = LocalizationHelper.GetLanguageString("This file doesn't exist anymore.", "form", GetType().Name, "msg", "ThisFileDoesntExist", "Text");

            MessageBox.Show($"{message}\r\n{fileName}", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowBinaryFileOpenFailedMessage(string fileName, string binaryFileType)
        {
            var openFileFailed1 = LocalizationHelper.GetLanguageString("It seems to be a binary file.", "form", GetType().Name, "msg", "OpenFileFailed1", "Text");
            var binaryFileMessage = $"{openFileFailed1}\r\n\r\n";

            if (!string.IsNullOrEmpty(binaryFileType))
            {
                var openFileFailed2 = LocalizationHelper.GetLanguageString($"It seems to be a {binaryFileType} file.", "form", GetType().Name, "msg", "OpenFileFailed2", "Text");

                binaryFileMessage = $"{openFileFailed2}\r\n\r\n";
                binaryFileMessage = binaryFileMessage.Replace("{0}", binaryFileType);
            }

            var openFileFailed3 = LocalizationHelper.GetLanguageString("The following file could not be opened because it contains characters that could not be interpreted.", "form", GetType().Name, "msg", "OpenFileFailed3", "Text");
            var title = LocalizationHelper.GetLanguageString("Open File", "form", GetType().Name, "msg", "OpenFile", "Text");
            var message = $"{openFileFailed3}\r\n\r\n{binaryFileMessage}{fileName}";

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
