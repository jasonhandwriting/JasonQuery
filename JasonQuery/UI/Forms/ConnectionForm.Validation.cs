using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Localization;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private bool CheckData(bool skipPasswordCheck, bool showAlertOnError = true, bool isDatabaseRequired = false)
        {
            var result = false;

            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        result = CheckData_Oracle(skipPasswordCheck);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        result = CheckData_PostgreSQL(skipPasswordCheck, showAlertOnError);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        result = CheckData_SqlServer(skipPasswordCheck, showAlertOnError, isDatabaseRequired);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        result = CheckData_MySql(skipPasswordCheck, showAlertOnError, isDatabaseRequired);
                        break;
                    }
            }

            return result;
        }

        private bool FailValidation(string defaultText, string messageKey, Control focusControl, bool showMessage = true)
        {
            if (!showMessage)
            {
                return false;
            }

            _languageText = LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", messageKey, "Text");
            MessageBox.Show(_languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (focusControl != null)
            {
                focusControl.Focus();
            }

            return false;
        }

        private bool IsDataSourceNotSelected()
        {
            return GetSelectedDataSourceType() == DataSourceType.None;
        }

        private bool CheckData_Oracle(bool skipPasswordCheck)
        {
            if (IsDataSourceNotSelected())
            {
                return FailValidation("Please select data source.", "InfoCheckDataSource", cboDataSource);
            }

            if (string.IsNullOrWhiteSpace(txtConnectionName_Oracle.Text))
            {
                return FailValidation("Please enter connection name.", "InfoCheckConnection", txtConnectionName_Oracle);
            }

            if (string.IsNullOrWhiteSpace(txtServer_Oracle.Text))
            {
                return FailValidation("Please enter server name.", "InfoCheckServer", txtServer_Oracle);
            }

            if (chkDirectMode_Oracle.Checked && string.IsNullOrWhiteSpace(txtSID_Oracle.Text))
            {
                return FailValidation("Please enter SID.", "InfoCheckSID", txtSID_Oracle);
            }

            if (string.IsNullOrWhiteSpace(txtPort_Oracle.Text))
            {
                return FailValidation("Please enter port number.", "InfoCheckPort", txtPort_Oracle);
            }

            if (string.IsNullOrWhiteSpace(txtUserID_Oracle.Text))
            {
                return FailValidation("Please enter user name.", "InfoCheckUser", txtUserID_Oracle);
            }

            if (!skipPasswordCheck && string.IsNullOrWhiteSpace(txtPassword_Oracle.Text))
            {
                return FailValidation("Please enter password.", "InfoCheckPassword", txtPassword_Oracle);
            }

            return true;
        }

        private bool CheckData_PostgreSQL(bool skipPasswordCheck, bool showAlertOnError)
        {
            if (IsDataSourceNotSelected())
            {
                return FailValidation("Please select data source.", "InfoCheckDataSource", cboDataSource, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtConnectionName_PostgreSQL.Text))
            {
                return FailValidation("Please enter connection name.", "InfoCheckConnection", txtConnectionName_PostgreSQL, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtServer_PostgreSQL.Text))
            {
                return FailValidation("Please enter server name.", "InfoCheckServer", txtServer_PostgreSQL, showAlertOnError);
            }

            if (showAlertOnError && string.IsNullOrWhiteSpace(cboDatabase_PostgreSQL.Text))
            {
                return FailValidation("Please enter database name.", "InfoCheckDatabase", cboDatabase_PostgreSQL, true);
            }

            if (string.IsNullOrWhiteSpace(txtPort_PostgreSQL.Text))
            {
                return FailValidation("Please enter port number.", "InfoCheckPort", txtPort_PostgreSQL, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtUserID_PostgreSQL.Text))
            {
                return FailValidation("Please enter user name.", "InfoCheckUser", txtUserID_PostgreSQL, showAlertOnError);
            }

            if (!skipPasswordCheck && string.IsNullOrWhiteSpace(txtPassword_PostgreSQL.Text))
            {
                return FailValidation("Please enter password.", "InfoCheckPassword", txtPassword_PostgreSQL, showAlertOnError);
            }

            return true;
        }

        private bool CheckData_SqlServer(bool skipPasswordCheck, bool showAlertOnError, bool isDatabaseRequired)
        {
            if (IsDataSourceNotSelected())
            {
                return FailValidation("Please select data source.", "InfoCheckDataSource", cboDataSource, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtConnectionName_SQLServer.Text))
            {
                return FailValidation("Please enter connection name.", "InfoCheckConnection", txtConnectionName_SQLServer, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtServer_SQLServer.Text))
            {
                return FailValidation("Please enter server name.", "InfoCheckServer", txtServer_SQLServer, showAlertOnError);
            }

            if (isDatabaseRequired && string.IsNullOrWhiteSpace(cboDatabase_SQLServer.Text))
            {
                return FailValidation("Please enter database name.", "InfoCheckDatabase", cboDatabase_SQLServer, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtUserID_SQLServer.Text))
            {
                return FailValidation("Please enter user name.", "InfoCheckUser", txtUserID_SQLServer, showAlertOnError);
            }

            if (!skipPasswordCheck && string.IsNullOrWhiteSpace(txtPassword_SQLServer.Text))
            {
                return FailValidation("Please enter password.", "InfoCheckPassword", txtPassword_SQLServer, showAlertOnError);
            }

            return true;
        }

        private bool CheckData_MySql(bool skipPasswordCheck, bool showAlertOnError, bool isDatabaseRequired)
        {
            if (IsDataSourceNotSelected())
            {
                return FailValidation("Please select data source.", "InfoCheckDataSource", cboDataSource, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtConnectionName_MySQL.Text))
            {
                return FailValidation("Please enter connection name.", "InfoCheckConnection", txtConnectionName_MySQL, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtServer_MySQL.Text))
            {
                return FailValidation("Please enter server name.", "InfoCheckServer", txtServer_MySQL, showAlertOnError);
            }

            if (isDatabaseRequired && string.IsNullOrWhiteSpace(cboDatabase_MySQL.Text))
            {
                return FailValidation("Please enter database name.", "InfoCheckDatabase", cboDatabase_MySQL, showAlertOnError);
            }

            if (string.IsNullOrWhiteSpace(txtUserID_MySQL.Text))
            {
                return FailValidation("Please enter user name.", "InfoCheckUser", txtUserID_MySQL, showAlertOnError);
            }

            if (!skipPasswordCheck && string.IsNullOrWhiteSpace(txtPassword_MySQL.Text))
            {
                return FailValidation("Please enter password.", "InfoCheckPassword", txtPassword_MySQL, showAlertOnError);
            }

            return true;
        }
    }
}
