using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private string CaptureCopyDataSourceDisplayText()
        {
            return GetSelectedDataSourceDisplayText();
        }

        private void RestoreCopiedDataSource(string dataSourceDisplayText)
        {
            StartNewConnectionProfile(false);

            if (!string.Equals(GetSelectedDataSourceDisplayText(), dataSourceDisplayText, StringComparison.Ordinal))
            {
                SetSelectedDataSourceAndApply(dataSourceDisplayText, false);
            }
        }

        private void CaptureTag(Control control, string value)
        {
            if (control == null)
            {
                return;
            }

            control.Tag = value;
        }

        private void RestoreTagToText(Control control)
        {
            if (control == null)
            {
                return;
            }
            if (!control.Visible)
            { }
            control.Text = TextHelper.GetSafeString(control.Tag);
        }

        private void RestoreTagToChecked(CheckBox checkBox)
        {
            if (checkBox == null)
            {
                return;
            }

            checkBox.Checked = TextHelper.GetSafeString(checkBox.Tag) == "True";
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckData(false))
                {
                    return;
                }

                switch (GetSelectedDataSourceType())
                {
                    case DataSourceType.Oracle:
                        {
                            CopyOracle();
                            _lstPicLogo[0].Visible = true;
                            txtSupportInfo.Text = _lstSupportInfo[0];
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            CopyPostgreSql();
                            _lstPicLogo[1].Visible = true;
                            txtSupportInfo.Text = _lstSupportInfo[1];
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            CopySqlServer();
                            _lstPicLogo[2].Visible = true;
                            txtSupportInfo.Text = _lstSupportInfo[2];
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            CopyMySql();
                            _lstPicLogo[3].Visible = true;
                            txtSupportInfo.Text = _lstSupportInfo[3];
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CopyOracle()
        {
            var dataSourceDisplayText = CaptureCopyDataSourceDisplayText();

            CaptureTag(txtConnectionName_Oracle, $"{txtConnectionName_Oracle.Text}(Copied)");
            CaptureTag(txtServer_Oracle, txtServer_Oracle.Text);
            CaptureTag(txtPort_Oracle, txtPort_Oracle.Text);
            CaptureTag(txtRemark_Oracle, txtRemark_Oracle.Text);
            CaptureTag(txtUserID_Oracle, txtUserID_Oracle.Text);
            CaptureTag(txtPassword_Oracle, txtPassword_Oracle.Text);
            CaptureTag(cboConnectAs_Oracle, cboConnectAs_Oracle.Text);
            CaptureTag(txtSID_Oracle, txtSID_Oracle.Text);
            CaptureTag(chkDirectMode_Oracle, chkDirectMode_Oracle.Checked.ToString());
            CaptureTag(chkSavePasswords, chkSavePasswords.Checked.ToString());

            RestoreCopiedDataSource(dataSourceDisplayText);

            RestoreTagToText(txtConnectionName_Oracle);
            RestoreTagToText(txtServer_Oracle);
            RestoreTagToText(txtPort_Oracle);
            RestoreTagToText(txtRemark_Oracle);
            RestoreTagToText(txtUserID_Oracle);
            RestoreTagToText(txtPassword_Oracle);
            RestoreTagToText(cboConnectAs_Oracle);

            RestoreTagToChecked(chkDirectMode_Oracle);

            if (chkDirectMode_Oracle.Checked)
            {
                RestoreTagToText(txtSID_Oracle);
            }

            RestoreTagToChecked(chkSavePasswords);

            txtConnectionName_Oracle.Focus();
        }

        private void CopyPostgreSql()
        {
            var dataSourceDisplayText = CaptureCopyDataSourceDisplayText();

            CaptureTag(txtConnectionName_PostgreSQL, $"{txtConnectionName_PostgreSQL.Text}(Copied)");
            CaptureTag(txtServer_PostgreSQL, txtServer_PostgreSQL.Text);
            CaptureTag(cboDatabase_PostgreSQL, cboDatabase_PostgreSQL.Text);
            CaptureTag(txtPort_PostgreSQL, txtPort_PostgreSQL.Text);
            CaptureTag(txtRemark_PostgreSQL, txtRemark_PostgreSQL.Text);
            CaptureTag(txtUserID_PostgreSQL, txtUserID_PostgreSQL.Text);
            CaptureTag(txtPassword_PostgreSQL, txtPassword_PostgreSQL.Text);
            CaptureTag(chkUnicode_PostgreSQL, chkUnicode_PostgreSQL.Checked.ToString());
            CaptureTag(chkAutoRollback_PostgreSQL, chkAutoRollback_PostgreSQL.Checked.ToString());
            CaptureTag(chkSavePasswords, chkSavePasswords.Checked.ToString());

            RestoreCopiedDataSource(dataSourceDisplayText);

            RestoreTagToText(txtConnectionName_PostgreSQL);
            RestoreTagToText(txtServer_PostgreSQL);
            RestoreTagToText(cboDatabase_PostgreSQL);
            RestoreTagToText(txtPort_PostgreSQL);
            RestoreTagToText(txtRemark_PostgreSQL);
            RestoreTagToText(txtUserID_PostgreSQL);
            RestoreTagToText(txtPassword_PostgreSQL);
            RestoreTagToChecked(chkUnicode_PostgreSQL);
            RestoreTagToChecked(chkAutoRollback_PostgreSQL);
            RestoreTagToChecked(chkSavePasswords);

            txtConnectionName_PostgreSQL.Focus();
        }

        private void CopySqlServer()
        {
            var dataSourceDisplayText = CaptureCopyDataSourceDisplayText();

            CaptureTag(txtConnectionName_SQLServer, $"{txtConnectionName_SQLServer.Text}(Copied)");
            CaptureTag(txtServer_SQLServer, txtServer_SQLServer.Text);
            CaptureTag(cboDatabase_SQLServer, cboDatabase_SQLServer.Text);
            CaptureTag(txtRemark_SQLServer, txtRemark_SQLServer.Text);
            CaptureTag(txtUserID_SQLServer, txtUserID_SQLServer.Text);
            CaptureTag(txtPassword_SQLServer, txtPassword_SQLServer.Text);
            CaptureTag(chkSavePasswords, chkSavePasswords.Checked.ToString());

            RestoreCopiedDataSource(dataSourceDisplayText);

            RestoreTagToText(txtConnectionName_SQLServer);
            RestoreTagToText(txtServer_SQLServer);
            RestoreTagToText(cboDatabase_SQLServer);
            RestoreTagToText(txtRemark_SQLServer);
            RestoreTagToText(txtUserID_SQLServer);
            RestoreTagToText(txtPassword_SQLServer);
            RestoreTagToChecked(chkSavePasswords);

            txtConnectionName_SQLServer.Focus();
        }

        private void CopyMySql()
        {
            var dataSourceDisplayText = CaptureCopyDataSourceDisplayText();

            CaptureTag(txtConnectionName_MySQL, $"{txtConnectionName_MySQL.Text}(Copied)");
            CaptureTag(txtServer_MySQL, txtServer_MySQL.Text);
            CaptureTag(cboDatabase_MySQL, cboDatabase_MySQL.Text);
            CaptureTag(txtRemark_MySQL, txtRemark_MySQL.Text);
            CaptureTag(txtUserID_MySQL, txtUserID_MySQL.Text);
            CaptureTag(txtPassword_MySQL, txtPassword_MySQL.Text);
            CaptureTag(chkSavePasswords, chkSavePasswords.Checked.ToString());

            RestoreCopiedDataSource(dataSourceDisplayText);

            RestoreTagToText(txtConnectionName_MySQL);
            RestoreTagToText(txtServer_MySQL);
            RestoreTagToText(cboDatabase_MySQL);
            RestoreTagToText(txtRemark_MySQL);
            RestoreTagToText(txtUserID_MySQL);
            RestoreTagToText(txtPassword_MySQL);
            RestoreTagToChecked(chkSavePasswords);

            txtConnectionName_MySQL.Focus();
        }
    }
}
