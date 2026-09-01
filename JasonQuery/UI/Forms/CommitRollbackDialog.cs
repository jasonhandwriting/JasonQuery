using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class CommitRollbackDialog : Form
    {
        public string Result { get; set; } //COMMIT 或 ROLLBACK，其他皆視為「取消」
        public string Mode { get; set; } = string.Empty; //BLOB：使用者編輯 Blob 欄位；EDIT：使用者編輯了其他欄位的資料
        public string ColumnType { get; set; } //for SQL Server

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public CommitRollbackDialog()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

                btnCommit.Location = new Point(btnCancel.Left - btnCommit.Width - btnRollback.Width - 50, btnCommit.Top);
                btnRollback.Location = new Point(btnCommit.Left + btnCommit.Width + 15, btnRollback.Top);

                Text = $"{AppConfigHelper.MessageBoxCaption} - {Text}";
                lblConnection.Text += DatabaseSqlExecutor.DbConnectionTitle;
                lblInfo.Text = lblInfo.Text.Replace("{DBType}", DatabaseSqlExecutor.DataSourceDisplayName);
                btnCancel.Focus();

                //20231030 修飾 PostgreSQL/SQL Server 的提示
                if (new HashSet<string> { "BLOB", "EDIT" }.Contains(Mode))
                {
                    var info = string.Empty;

                    if (Mode == "BLOB")
                    {
                        info = LocalizationHelper.GetLanguageString("You have saved the specified file into the BLOB field.\r\nJasonQuery has detected that you have uncommitted changes.", "form", GetType().Name, "object", "lblInfo_BlobConfirm", "Text");
                    }
                    else
                    {
                        info = LocalizationHelper.GetLanguageString("You have changed data in some fields.\r\nJasonQuery has detected that you have uncommitted changes.", "form", GetType().Name, "object", "lblInfo_EditConfirm", "Text");
                    }

                    switch (_currentSourceType)
                    {
                        case DataSourceType.PostgreSql:
                            {
                                if (string.Equals(Mode, "BLOB", StringComparison.OrdinalIgnoreCase))
                                {
                                    info = info.Replace("BLOB", "bytea");
                                }

                                break;
                            }
                        case DataSourceType.SqlServer:
                            {
                                if (string.Equals(Mode, "BLOB", StringComparison.OrdinalIgnoreCase) && ColumnType.StartsWith("BINARY", StringComparison.OrdinalIgnoreCase))
                                {
                                    info = info.Replace("BLOB", "binary");
                                }
                                else if (string.Equals(Mode, "BLOB", StringComparison.OrdinalIgnoreCase) && ColumnType.StartsWith("VARBINARY", StringComparison.OrdinalIgnoreCase))
                                {
                                    info = info.Replace("BLOB", "varbinary");
                                }

                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    lblInfo.Text = info;
                }

                //20240724 變更 Commit/Rollback 圖示樣示
                var iconSuffix = (MyLibrary.CommitRollbackIcon >= 2 && MyLibrary.CommitRollbackIcon <= 6) ? $"{MyLibrary.CommitRollbackIcon} " : string.Empty;
                var commitIconName = $"Commit {iconSuffix}16x16.ico";
                var rollbackIconName = $"Rollback {iconSuffix}16x16.ico";

                btnCommit.Image = IconManager.GetImage(MyGlobal.IconLibrary, commitIconName);
                btnRollback.Image = IconManager.GetImage(MyGlobal.IconLibrary, rollbackIconName);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            Result = "COMMIT";
        }

        private void btnRollback_Click(object sender, EventArgs e)
        {
            Result = "ROLLBACK";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
