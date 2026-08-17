using JasonLibrary.Core.Text;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void CheckFileDateTimeAndExist()
        {
            try
            {
                if (!TryGetSavedFileName(out var fileName))
                {
                    return;
                }

                if (!File.Exists(fileName))
                {
                    HandleWatchedFileDeleted(fileName);
                    return;
                }

                HandleWatchedFileModified(fileName);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool TryGetSavedFileName(out string fileName)
        {
            fileName = TextHelper.GetSafeString(btnSave.Tag);

            return !string.IsNullOrEmpty(fileName);
        }

        private void HandleWatchedFileDeleted(string fileName)
        {
            var displayFileName = Path.GetFileName(fileName);
            var thisFileDoesntExist = LocalizationHelper.GetLanguageString("This file doesn't exist anymore.", "form", GetType().Name, "msg", "ThisFileDoesntExist", "Text");
            var keepThisFileInEditor = LocalizationHelper.GetLanguageString("Keep this file in editor?", "form", GetType().Name, "msg", "KeepThisFileInEditor", "Text");
            var result = MessageBox.Show($"{thisFileDoesntExist}\r\n{displayFileName}\r\n\r\n{keepThisFileInEditor}", _languageText, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                WriteFile(fileName);
                editor.Focus();
            }
            else
            {
                TransferValueToMainForm($"CloseQueryForm`{AccessibleDescription}");
            }

            ClearFileWatchBackupState();
        }

        private void HandleWatchedFileModified(string fileName)
        {
            var savedDateTime = TextHelper.GetSafeString(btnSaveRed.Tag);

            if (string.IsNullOrEmpty(savedDateTime))
            {
                return;
            }

            var currentLastWriteTime = GetFileLastWriteTimeText(fileName);

            if (string.Equals(savedDateTime, currentLastWriteTime, StringComparison.Ordinal))
            {
                return;
            }

            if (ConfirmReloadExternallyModifiedFile(fileName))
            {
                ReloadWatchedFile(fileName);
            }
            else
            {
                MarkWatchedFileAsExternallyModified(fileName);
            }

            //無論使用者選「要或不要更新」，一律更新時間戳記。否則外部檔案沒有再次更新時，下次檢查仍會重複詢問
            btnSaveRed.Tag = GetFileLastWriteTimeText(fileName);

            editor.Focus();
        }

        private bool ConfirmReloadExternallyModifiedFile(string fileName)
        {
            var reloadMessage = editor.CanUndo
                                ? LocalizationHelper.GetLanguageString("Do you want to reload it and lose the changes made in JasonQuery?", "form", GetType().Name, "msg", "Reload1", "Text")
                                : LocalizationHelper.GetLanguageString("Do you want to reload it?", "form", GetType().Name, "msg", "Reload2", "Text");

            var modifiedByAnotherProgram = LocalizationHelper.GetLanguageString("This file has been modified by another program.", "form", GetType().Name, "msg", "ModifiedByAnotherProgram", "Text");

            return MessageBox.Show($"{fileName}\r\n\r\n{modifiedByAnotherProgram}\r\n{reloadMessage}", _languageText, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
        }

        private void ReloadWatchedFile(string fileName)
        {
            if (!LoadFile(fileName, out _))
            {
                return;
            }

            ClearFileWatchBackupState();

            TransferValueToMainForm($"UpdateTabInfo`{fileName}");

            //儲存「存檔點」
            editor.EmptyUndoBuffer();

            btnSaveRed.Visible = false;
            btnSave.Visible = true;
        }

        private void MarkWatchedFileAsExternallyModified(string fileName)
        {
            TransferValueToMainForm($"UpdateTabInfo`*{fileName}");

            btnSaveRed.Visible = true;
            btnSave.Visible = false;
        }

        private void ClearFileWatchBackupState()
        {
            tmrBackup.Enabled = false;
            MyGlobal.DeleteBackupFileInfo(AccessibleName);
        }

        private static string GetFileLastWriteTimeText(string fileName)
        {
            return File.GetLastWriteTime(fileName).ToString(DateTimeFormat);
        }
    }
}