using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAs();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void btnSaveRed_Click(object sender, EventArgs e) //按下工具列上的紅色 Save 按鈕
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool Save()
        {
            if (!HasSavedFile)
            {
                return SaveAs();
            }

            if (!WriteFile(SavedFilePath))
            {
                return false;
            }

            MyGlobal.DeleteBackupFileInfo(AccessibleName);

            return true;
        }

        private bool SaveAs()
        {
            using (var saveFileDialog = CreateQuerySaveFileDialog())
            {
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false; //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
                }

                return SaveAs(saveFileDialog.FileName);
            }
        }

        private bool SaveAs(string fileName)
        {
            try
            {
                fileName = EnsureSqlFileExtension(fileName);

                if (IsFileAlreadyOpenedForSaveAs(fileName))
                {
                    var message = LocalizationHelper.GetLanguageString("The file is already opened in the JasonQuery.", "form", GetType().Name, "msg", "OpenInJasonQuery", "Text");

                    MessageBox.Show(this, $"{message}\r\n\r\n{fileName}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return false;
                }

                if (!WriteFile(fileName, true))
                {
                    return false;
                }

                MyGlobal.DeleteBackupFileInfo(AccessibleName);

                return true;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);

                return false;
            }
        }

        private SaveFileDialog CreateQuerySaveFileDialog()
        {
            return new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                FileName = GetSuggestedSaveAsFileName(),
                RestoreDirectory = true,
                Filter = BuildSaveFileDialogFilter()
            };
        }

        private string GetSuggestedSaveAsFileName()
        {
            if (HasSavedFile)
            {
                return Path.GetFileName(SavedFilePath);
            }

            //An alias is display text, not a path supplied to the save dialog.
            var fileName = string.Join("_", EditorTabDisplayName.Split(Path.GetInvalidFileNameChars())).TrimEnd('.', ' ');

            return string.IsNullOrWhiteSpace(fileName) ? "SQL Editor" : fileName;
        }

        private string BuildSaveFileDialogFilter()
        {
            return @"Query file (*.sql)|*.sql|All files (*.*)|*.*"
                   .Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text"))
                   .Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"));
        }

        private static string EnsureSqlFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                return $"{fileName}.sql";
            }

            return fileName;
        }

        private bool IsFileAlreadyOpenedForSaveAs(string fileName)
        {
            MyGlobal.CheckExistTabResult = string.Empty;

            //傳資訊到母表單，檢查 Tab 資訊，此檔案是否已被 JasonQuery 開啟了
            TransferValueToMainForm($"CheckExistTab`{fileName}");

            var isFileAlreadyOpened = string.Equals(MyGlobal.CheckExistTabResult, "TRUE", StringComparison.Ordinal);

            MyGlobal.CheckExistTabResult = string.Empty;

            return isFileAlreadyOpened;
        }

        private bool WriteFile(string fileName, bool rebindFileIdentity = false)
        {
            if (!rebindFileIdentity && !MatchesSavedFilePath(fileName))
            {
                ShowExceptionMessage(new InvalidOperationException("Save cancelled: the target path does not match the current saved file identity."));

                return false;
            }

            try
            {
                File.WriteAllText(fileName, editor.Text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);

                return false;
            }

            ApplySavedFileState(fileName, rebindFileIdentity);

            return true;
        }

        private void ApplySavedFileState(string fileName, bool rebindFileIdentity)
        {
            if (rebindFileIdentity)
            {
                _editorTabState.ApplySuccessfulSaveAs(fileName);
            }

            btnSaveRed.Tag = File.GetLastWriteTime(fileName).ToString(DateTimeFormat);

            _requiresSaveWithoutUndo = false;
            editor.EmptyUndoBuffer();
            RefreshEditorDirtyState();
            RefreshEditorCommandButtonsState();

            //傳資訊到母表單，更新 Recent Files 資訊
            TransferValueToMainForm($"UpdateRecentFiles`{fileName}");
        }

        internal bool ExecuteSaveCommand()
        {
            try
            {
                return Save();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);

                return false;
            }
        }

        internal bool ExecuteSaveAsCommand()
        {
            try
            {
                return SaveAs();
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);

                return false;
            }
        }
    }
}
