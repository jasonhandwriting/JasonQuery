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
                if (Save())
                {
                    btnSave.Visible = true;
                    btnSaveRed.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool Save()
        {
            var fileName = TextHelper.GetSafeString(btnSave.Tag);

            if (string.IsNullOrEmpty(fileName)) //尚未存檔過！
            {
                if (!SaveAs())
                {
                    return false;
                }

                MyGlobal.DeleteBackupFileInfo(AccessibleName); //20240303 存檔完畢，將備份記錄刪除 (使用者再次異動檔案內容，會再重新記錄)
                return true;
            }

            WriteFile(fileName);

            MyGlobal.DeleteBackupFileInfo(AccessibleName); //20240303 存檔完畢，將備份記錄刪除 (使用者再次異動檔案內容，會再重新記錄)
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
                    MessageBox.Show($"SaveAs()\r\n\r\n{fileName}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (!WriteFile(fileName))
                {
                    return false;
                }

                //20240301 記住完整檔名
                Tag = fileName;

                TransferValueToMainForm($"UpdateTabInfo`{fileName}");

                btnSave.Tag = fileName;
                btnSaveRed.Tag = File.GetLastWriteTime(fileName).ToString(DateTimeFormat);

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
                FileName = Path.GetFileName((btnSave.Tag ?? Tag)?.ToString() ?? string.Empty).Replace("*", string.Empty),
                RestoreDirectory = true,
                Filter = BuildSaveFileDialogFilter()
            };
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

        private bool WriteFile(string fileName)
        {
            try
            {
                File.WriteAllText(fileName, editor.Text, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return false;
            }

            ApplySavedFileState(fileName);
            return true;
        }

        private void ApplySavedFileState(string fileName)
        {
            Tag = fileName;

            //傳資訊到母表單，更新 Tab 資訊
            TransferValueToMainForm($"UpdateTabInfo`{fileName}");

            btnSave.Tag = fileName;
            btnSaveRed.Tag = File.GetLastWriteTime(fileName).ToString(DateTimeFormat);

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
