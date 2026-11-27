using JasonQuery.Core.Config;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.QueryEditor;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void tmrBackup_Tick(object sender, EventArgs e)
        {
            TryPersistEditorBackup();
        }

        internal bool TryPersistEditorBackup()
        {
            try
            {
                //Stop the pending timer; the same method is used during normal application exit.
                tmrBackup.Enabled = false;

                if (!AppConfigHelper.IsBackupFile || !IsEditorDirty)
                {
                    return true;
                }

                //CreateDirectory is also safe when the directory already exists.
                Directory.CreateDirectory(AppConfigHelper.BackupPath);

                //Only publish recovery metadata after the backup content has been written.
                File.WriteAllText(GetBackupFileFullName(), editor.Text, Encoding.UTF8);
                UpdateEditorBackupInfoIfNeeded(IsEditorDirty);

                //Do not change the external-file timestamp or clear the editor's dirty state.
                return true;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return false;
            }
        }

        private string GetBackupFileFullName()
        {
            return Path.Combine(AppConfigHelper.BackupPath, _backupFileName);
        }

        private void RestoreBackupFileIfNeeded()
        {
            EnableEditorAfterFormLoad();

            if (string.IsNullOrEmpty(BackupRealFullFileName))
            {
                return;
            }

            var backupFileName = GetBackupFileFullName();

            if (!LoadFile(backupFileName, out _))
            {
                return;
            }

            ApplyRestoredBackupFileState(backupFileName);
            RestoreBackupEditorPosition();
            MarkRestoredBackupAsModifiedIfNeeded();
        }

        private void EnableEditorAfterFormLoad()
        {
            c1DockingTab2.Enabled = true;
            editor.Enabled = true;
            editor.Focus();
        }

        private void ApplyRestoredBackupFileState(string backupFileName)
        {
            var savedFilePath = string.Equals(BackupRealFullFileName, "FileNotSavedYet", StringComparison.Ordinal) ? string.Empty : BackupRealFullFileName;

            _editorTabState = QueryEditorTabState.Restore(BuildEditorTabTitle(_restoreBackupWasDirty), savedFilePath, out _restoreBackupWasDirty);
            btnSaveRed.Tag = File.GetLastWriteTime(HasSavedFile ? SavedFilePath : backupFileName).ToString(DateTimeFormat);

            PublishEditorTabState(_restoreBackupWasDirty);
        }

        private void RestoreBackupEditorPosition()
        {
            editor.EmptyUndoBuffer();

            SetEditorPositionSafely(BackupCurrentPosition);
        }

        private void MarkRestoredBackupAsModifiedIfNeeded()
        {
            var wasDirty = _restoreBackupWasDirty;

            _restoreBackupWasDirty = false;

            if (!wasDirty)
            {
                return;
            }

            //Recovered content requires saving even with an empty undo buffer.
            _requiresSaveWithoutUndo = true;

            using (TraceLogger.Time("Check Editor Content"))
            {
                CheckEditorContent();
            }

            tmrBackup.Enabled = false;
        }

        private void SetEditorPositionSafely(int position)
        {
            var safePosition = Math.Max(0, Math.Min(position, editor.TextLength));

            editor.SelectionStart = safePosition;
            editor.CurrentPosition = safePosition;
            editor.ScrollCaret();
        }

        /// <summary>
        /// 更新此筆資料庫連線所儲存的暫存檔訊息。
        /// </summary>
        /// <param name="backupFileName">對應 SystemConfig.AttributeValue：備份檔案名稱。</param>
        /// <param name="currentPosition">對應 SystemConfig.AttributeName：備份時的 editor 游標位置。</param>
        /// <param name="tabTitle">對應 SystemConfig.AttributeText：當下主畫面頁籤顯示文字。</param>
        /// <param name="savedFullFileName">對應 SystemConfig.AttributeText2：已儲存檔案完整路徑；未存檔則為空值。</param>
        private void UpdateBackupFileInfo(string backupFileName, int currentPosition, string tabTitle = "", string savedFullFileName = "")
        {
            var escapedDomainUser = EscapeSqlText(MyGlobal.DomainUser);
            var escapedBackupFileName = EscapeSqlText(backupFileName);
            var escapedTabTitle = EscapeSqlText(tabTitle);
            var escapedSavedFullFileName = EscapeSqlText(savedFullFileName);
            var now = MyGlobal.DateTimeNow();

            if (BackupFileInfoExists(escapedDomainUser, escapedBackupFileName))
            {
                UpdateExistingBackupFileInfo(escapedDomainUser, escapedBackupFileName, currentPosition, escapedTabTitle, escapedSavedFullFileName, now);

                return;
            }

            InsertBackupFileInfo(escapedDomainUser, escapedBackupFileName, currentPosition, escapedTabTitle, escapedSavedFullFileName, now);
        }

        private void UpdateExistingBackupTabTitle(string tabTitle)
        {
            if (!AppConfigHelper.IsBackupFile || !IsEditorDirty || string.IsNullOrEmpty(_backupFileName) || !File.Exists(GetBackupFileFullName()))
            {
                return;
            }

            //Rename changes only the title of an existing recovery record.
            //Do not insert a record or advance the saved caret/path without a backup write.
            var sbSql = new StringBuilder();

            sbSql.AppendLine("UPDATE SystemConfig");
            sbSql.AppendLine($"   SET AttributeText = '{EscapeSqlText(tabTitle)}'");
            sbSql.AppendLine($" WHERE DomainUser = '{EscapeSqlText(MyGlobal.DomainUser)}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
            sbSql.Append($"   AND AttributeValue = '{EscapeSqlText(_backupFileName)}'");

            JasonQueryRepository.ExecNonQuery(sbSql.ToString(), false);
        }

        private bool BackupFileInfoExists(string escapedDomainUser, string escapedBackupFileName)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{escapedDomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
            sbSql.Append($"   AND AttributeValue = '{escapedBackupFileName}'");

            var dtBackupFile = JasonQueryRepository.ReadBackupMetadataOrThrow(sbSql.ToString());

            return dtBackupFile?.Rows.Count > 0;
        }

        private void UpdateExistingBackupFileInfo(string escapedDomainUser, string escapedBackupFileName, int currentPosition, string escapedTabTitle, string escapedSavedFullFileName, string now)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("UPDATE SystemConfig");
            sbSql.AppendLine($"   SET AttributeDate = '{now}',");
            sbSql.AppendLine($"       AttributeName = '{currentPosition}',");
            sbSql.AppendLine($"       AttributeText = '{escapedTabTitle}',");
            sbSql.AppendLine($"       AttributeText2 = '{escapedSavedFullFileName}'");
            sbSql.AppendLine($" WHERE DomainUser = '{escapedDomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
            sbSql.Append($"   AND AttributeValue = '{escapedBackupFileName}'");

            JasonQueryRepository.WriteBackupMetadataOrThrow(sbSql.ToString());
        }

        private void InsertBackupFileInfo(string escapedDomainUser, string escapedBackupFileName, int currentPosition, string escapedTabTitle, string escapedSavedFullFileName, string now)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("INSERT INTO SystemConfig");
            sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeValue, AttributeName, AttributeText, AttributeText2, AttributeDate)");
            sbSql.Append("VALUES ");
            sbSql.Append($"('{escapedDomainUser}', ");
            sbSql.Append($"{JasonQueryRepository.DbMotherPid}, ");
            sbSql.Append("'BackupFilename', ");
            sbSql.Append($"'{escapedBackupFileName}', ");
            sbSql.Append($"'{currentPosition}', ");
            sbSql.Append($"'{escapedTabTitle}', ");
            sbSql.Append($"'{escapedSavedFullFileName}', ");
            sbSql.Append($"'{now}')");

            JasonQueryRepository.WriteBackupMetadataOrThrow(sbSql.ToString());
        }

        private static string EscapeSqlText(string value)
        {
            return TextHelper.GetSafeString(value).Replace("'", "''");
        }
    }
}
