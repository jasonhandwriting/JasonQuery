using JasonQuery.Core.Config;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void tmrBackup_Tick(object sender, EventArgs e)
        {
            try
            {
                //存檔後就取消 Timer；下次使用者異動內容，會再由 CheckEditorContent() 啟用備份
                tmrBackup.Enabled = false;

                if (!TryEnsureBackupDirectory())
                {
                    return;
                }

                File.WriteAllText(GetBackupFileFullName(), editor.Text, Encoding.UTF8);

                //此處不可調整檔案異動的時間判定，否則，針對已存檔的情況，會一直出現誤判 (如果是已存檔的情況，直接以使用者儲存後的檔案時間為依據，檢查是否有被外部程式變更過即可)
                //btnSaveRed.Tag = File.GetLastWriteTime(MyGlobal.sBackupPath + _backupFileName).ToString("yyyy/MM/dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool TryEnsureBackupDirectory()
        {
            if (Directory.Exists(AppConfigHelper.BackupPath))
            {
                return true;
            }

            try
            {
                //使用者可能中途變更備份路徑
                Directory.CreateDirectory(AppConfigHelper.BackupPath);
                return true;
            }
            catch (Exception)
            {
                AppConfigHelper.IsBackupFile = false;
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

            ApplyOpenFileTagFromDefaultAction();

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

        private void ApplyOpenFileTagFromDefaultAction()
        {
            if (string.IsNullOrEmpty(AccessibleDefaultActionDescription))
            {
                return;
            }

            if (AccessibleDefaultActionDescription.StartsWith("OPEN", StringComparison.Ordinal))
            {
                Tag = AccessibleDefaultActionDescription.Substring(6);
            }
        }

        private void ApplyRestoredBackupFileState(string backupFileName)
        {
            if (string.Equals(BackupRealFullFileName, "FileNotSavedYet", StringComparison.Ordinal))
            {
                TransferValueToMainForm($"UpdateTabInfo`{TextHelper.GetSafeString(Tag)}");

                btnSave.Tag = null;
                btnSaveRed.Tag = File.GetLastWriteTime(backupFileName).ToString(DateTimeFormat);
                return;
            }

            var tabTitlePrefix = TextHelper.GetSafeString(Tag).StartsWith("*", StringComparison.Ordinal)
                                 ? "*"
                                 : string.Empty;

            TransferValueToMainForm($"UpdateTabInfo`{tabTitlePrefix}{BackupRealFullFileName}");

            btnSave.Tag = BackupRealFullFileName;
            btnSaveRed.Tag = File.GetLastWriteTime(BackupRealFullFileName).ToString(DateTimeFormat);
        }

        private void RestoreBackupEditorPosition()
        {
            editor.EmptyUndoBuffer();
            SetEditorPositionSafely(BackupCurrentPosition);
        }

        private void MarkRestoredBackupAsModifiedIfNeeded()
        {
            if (!TextHelper.GetSafeString(Tag).StartsWith("*", StringComparison.Ordinal))
            {
                return;
            }

            //讓 editor 重新進入可復原狀態，保留「這份內容是異動狀態」的語意
            editor.AppendText(MyGlobal.Separator00A1);
            editor.Text = editor.Text.Substring(0, editor.Text.Length - 1);

            SetEditorPositionSafely(BackupCurrentPosition);

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
                UpdateExistingBackupFileInfo(
                    escapedDomainUser,
                    escapedBackupFileName,
                    currentPosition,
                    escapedTabTitle,
                    escapedSavedFullFileName,
                    now);

                return;
            }

            InsertBackupFileInfo(
                escapedDomainUser,
                escapedBackupFileName,
                currentPosition,
                escapedTabTitle,
                escapedSavedFullFileName,
                now);
        }

        private bool BackupFileInfoExists(string escapedDomainUser, string escapedBackupFileName)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{escapedDomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'BackupFilename'");
            sbSql.Append($"   AND AttributeValue = '{escapedBackupFileName}'");

            var dtBackupFile = JasonQueryRepository.ExecQuery(sbSql.ToString());

            return dtBackupFile?.Rows.Count > 0;
        }

        private void UpdateExistingBackupFileInfo(
            string escapedDomainUser,
            string escapedBackupFileName,
            int currentPosition,
            string escapedTabTitle,
            string escapedSavedFullFileName,
            string now)
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

            JasonQueryRepository.ExecNonQuery(sbSql.ToString(), false);
        }

        private void InsertBackupFileInfo(
            string escapedDomainUser,
            string escapedBackupFileName,
            int currentPosition,
            string escapedTabTitle,
            string escapedSavedFullFileName,
            string now)
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

            JasonQueryRepository.ExecNonQuery(sbSql.ToString(), false);
        }

        private static string EscapeSqlText(string value)
        {
            return TextHelper.GetSafeString(value).Replace("'", "''");
        }
    }
}
