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
        internal bool CanCheckWatchedFile => _isFormLoadFinished && !_isBusy && !IsDisposed && !Disposing
                                             && HasSavedFile && editor != null && !editor.IsDisposed && !editor.Disposing;

        internal void CheckFileDateTimeAndExist()
        {
            if (!CanCheckWatchedFile)
            {
                return;
            }

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
            fileName = SavedFilePath;

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
                if (!WriteFile(fileName))
                {
                    return;
                }

                editor.Focus();
            }
            else
            {
                //The host may dispose this editor synchronously. Finish recovery cleanup first.
                ClearFileWatchBackupState();

                TransferValueToMainForm($"CloseQueryForm`{AccessibleDescription}");

                return;
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
                if (!ReloadWatchedFile(fileName, out var wasCancelled))
                {
                    if (!wasCancelled)
                    {
                        return;
                    }

                    //Declining the large-file warning also means retaining the editor.
                    MarkWatchedFileAsExternallyModified(fileName);
                }
            }
            else
            {
                MarkWatchedFileAsExternallyModified(fileName);
            }

            //Acknowledge only the timestamp observed before confirmation.
            btnSaveRed.Tag = currentLastWriteTime;

            editor.Focus();
        }

        private bool ConfirmReloadExternallyModifiedFile(string fileName)
        {
            var reloadMessage = IsEditorDirty
                                ? LocalizationHelper.GetLanguageString("Do you want to reload it and lose the changes made in JasonQuery?", "form", GetType().Name, "msg", "Reload1", "Text")
                                : LocalizationHelper.GetLanguageString("Do you want to reload it?", "form", GetType().Name, "msg", "Reload2", "Text");

            var modifiedByAnotherProgram = LocalizationHelper.GetLanguageString("This file has been modified by another program.", "form", GetType().Name, "msg", "ModifiedByAnotherProgram", "Text");

            return MessageBox.Show($"{fileName}\r\n\r\n{modifiedByAnotherProgram}\r\n{reloadMessage}", _languageText, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
        }

        private bool ReloadWatchedFile(string fileName, out bool wasCancelled)
        {
            if (!LoadFile(fileName, out _, out wasCancelled))
            {
                return false;
            }

            ClearFileWatchBackupState();

            //Only a successful reload resolves retained external changes.
            _requiresSaveWithoutUndo = false;

            editor.EmptyUndoBuffer();

            RefreshEditorDirtyState();
            RefreshEditorCommandButtonsState();

            return true;
        }

        private void MarkWatchedFileAsExternallyModified(string fileName)
        {
            _requiresSaveWithoutUndo = true;

            RefreshEditorDirtyState();
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
