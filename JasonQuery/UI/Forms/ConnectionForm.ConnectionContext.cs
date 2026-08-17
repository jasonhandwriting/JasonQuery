using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private sealed class ConnectionContextFormValues
        {
            public DataSourceType DataSourceType { get; set; }
            public string DataSourceDisplayText { get; set; }
            public string ConnectionTitle { get; set; }
            public string ConnectionName { get; set; }
            public string Server { get; set; }
            public int Port { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string DatabaseName { get; set; }
            public string OracleSid { get; set; }
            public string OracleConnectAs { get; set; }
            public bool UseAutoRollback { get; set; }
            public bool UseDirectMode { get; set; }
            public bool UseConnectionPooling { get; set; }
            public bool UseUnicode { get; set; }
            public bool ExcludeNativeDatabase { get; set; }
            public int QueryTimeoutSeconds { get; set; }
        }

        private void ApplyConnectionContextFromForm(ConnectionContextFormValues values)
        {
            ApplyConnectionContextCommonUiState();

            DatabaseSqlExecutor.CurrentDataSource = values.DataSourceType;
            DatabaseSqlExecutor.DataSourceDisplayName = values.DataSourceDisplayText;

            DatabaseSqlExecutor.ClearDbServerVersion();

            DatabaseSqlExecutor.DbConnectionString = string.Empty;

            DatabaseSqlExecutor.DbConnectionTitle = values.ConnectionTitle;
            DatabaseSqlExecutor.DbConnectionName = values.ConnectionName;
            DatabaseSqlExecutor.DbConnectionServer = values.Server;
            DatabaseSqlExecutor.DbConnectionPort = values.Port;

            DatabaseSqlExecutor.DbUser = values.UserName;
            DatabaseSqlExecutor.DbPassword = values.Password;

            DatabaseSqlExecutor.DatabaseName = values.DatabaseName ?? string.Empty;
            DatabaseSqlExecutor.OracleSid = values.OracleSid ?? string.Empty;
            DatabaseSqlExecutor.OracleConnectAs = values.OracleConnectAs ?? string.Empty;

            DatabaseSqlExecutor.UseAutoRollback = values.UseAutoRollback;
            DatabaseSqlExecutor.UseDirectMode = values.UseDirectMode;
            DatabaseSqlExecutor.UseConnectionPooling = values.UseConnectionPooling;
            DatabaseSqlExecutor.UseUnicode = values.UseUnicode;
            DatabaseSqlExecutor.ExcludeNativeDatabase = values.ExcludeNativeDatabase;
            DatabaseSqlExecutor.QueryTimeoutSeconds = values.QueryTimeoutSeconds;
        }

        private void UpdateConnectionContextFromForm_Oracle()
        {
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText();

            ApplyConnectionContextFromForm
            (
                new ConnectionContextFormValues
                {
                    DataSourceType = DataSourceType.Oracle,
                    DataSourceDisplayText = dataSourceDisplayText,
                    ConnectionTitle = $"({dataSourceDisplayText}) ({txtConnectionName_Oracle.Text}) {txtUserID_Oracle.Text}@{txtServer_Oracle.Text}:{txtPort_Oracle.Text}",
                    ConnectionName = txtConnectionName_Oracle.Text,
                    Server = txtServer_Oracle.Text,
                    Port = Convert.ToInt32(txtPort_Oracle.Text),
                    UserName = txtUserID_Oracle.Text,
                    Password = txtPassword_Oracle.Text,
                    DatabaseName = string.Empty,
                    OracleSid = txtSID_Oracle.Text,
                    OracleConnectAs = string.Empty,
                    UseAutoRollback = false,
                    UseDirectMode = chkDirectMode_Oracle.Checked,
                    UseConnectionPooling = chkPooling_Oracle.Checked,
                    UseUnicode = chkUnicode_Oracle.Checked,
                    ExcludeNativeDatabase = false,
                    QueryTimeoutSeconds = Convert.ToInt32(nudQueryTimeout_Oracle.Value)
                }
            );
        }

        private void UpdateConnectionContextFromForm_PostgreSql()
        {
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText();

            ApplyConnectionContextFromForm
            (
                new ConnectionContextFormValues
                {
                    DataSourceType = DataSourceType.PostgreSql,
                    DataSourceDisplayText = dataSourceDisplayText,
                    ConnectionTitle = $"({dataSourceDisplayText}) ({txtConnectionName_PostgreSQL.Text}) {cboDatabase_PostgreSQL.Text} on {txtUserID_PostgreSQL.Text}@{txtServer_PostgreSQL.Text}:{txtPort_PostgreSQL.Text}",
                    ConnectionName = txtConnectionName_PostgreSQL.Text,
                    Server = txtServer_PostgreSQL.Text,
                    Port = Convert.ToInt32(txtPort_PostgreSQL.Text),
                    UserName = txtUserID_PostgreSQL.Text,
                    Password = txtPassword_PostgreSQL.Text,
                    DatabaseName = cboDatabase_PostgreSQL.Text,
                    OracleSid = string.Empty,
                    OracleConnectAs = string.Empty,
                    UseAutoRollback = chkAutoRollback_PostgreSQL.Checked,
                    UseDirectMode = false,
                    UseConnectionPooling = chkPooling_PostgreSQL.Checked,
                    UseUnicode = chkUnicode_PostgreSQL.Checked,
                    ExcludeNativeDatabase = false,
                    QueryTimeoutSeconds = Convert.ToInt32(nudQueryTimeout_PostgreSQL.Value)
                }
            );
        }

        private void UpdateConnectionContextFromForm_SqlServer()
        {
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText();
            var databaseName = cboDatabase_SQLServer.Text;
            var databaseTitlePart = string.IsNullOrEmpty(databaseName) ? string.Empty : $"{databaseName} ";

            ApplyConnectionContextFromForm
            (
                new ConnectionContextFormValues
                {
                    DataSourceType = DataSourceType.SqlServer,
                    DataSourceDisplayText = dataSourceDisplayText,
                    ConnectionTitle = $"({dataSourceDisplayText}) ({txtConnectionName_SQLServer.Text}) {databaseTitlePart}on {txtUserID_SQLServer.Text}@{txtServer_SQLServer.Text}",
                    ConnectionName = txtConnectionName_SQLServer.Text,
                    Server = txtServer_SQLServer.Text,
                    Port = Convert.ToInt32(txtPort_SQLServer.Text),
                    UserName = txtUserID_SQLServer.Text,
                    Password = txtPassword_SQLServer.Text,
                    DatabaseName = databaseName,
                    OracleSid = string.Empty,
                    OracleConnectAs = string.Empty,
                    UseAutoRollback = false,
                    UseDirectMode = false,
                    UseConnectionPooling = chkPooling_SQLServer.Checked,
                    UseUnicode = false,
                    ExcludeNativeDatabase = chkExcludeNativeObject_SQLServer.Checked,
                    QueryTimeoutSeconds = Convert.ToInt32(nudQueryTimeout_SQLServer.Value)
                }
            );
        }

        private void UpdateConnectionContextFromForm_MySql()
        {
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText();
            var databaseName = cboDatabase_MySQL.Text;
            var databaseTitlePart = string.IsNullOrEmpty(databaseName) ? string.Empty : $"{databaseName} ";

            ApplyConnectionContextFromForm
            (
                new ConnectionContextFormValues
                {
                    DataSourceType = DataSourceType.MySql,
                    DataSourceDisplayText = dataSourceDisplayText,
                    ConnectionTitle = $"({dataSourceDisplayText}) ({txtConnectionName_MySQL.Text}) {databaseTitlePart}on {txtUserID_MySQL.Text}@{txtServer_MySQL.Text}",
                    ConnectionName = txtConnectionName_MySQL.Text,
                    Server = txtServer_MySQL.Text,
                    Port = Convert.ToInt32(txtPort_MySQL.Text),
                    UserName = txtUserID_MySQL.Text,
                    Password = txtPassword_MySQL.Text,
                    DatabaseName = databaseName,
                    OracleSid = string.Empty,
                    OracleConnectAs = string.Empty,
                    UseAutoRollback = false,
                    UseDirectMode = false,
                    UseConnectionPooling = chkPooling_MySQL.Checked,
                    UseUnicode = chkUnicode_MySQL.Checked,
                    ExcludeNativeDatabase = false,
                    QueryTimeoutSeconds = Convert.ToInt32(nudQueryTimeout_MySQL.Value)
                }
            );
        }

        private void ApplyConnectionContextCommonUiState()
        {
            JasonQueryRepository.DbMotherPid = lblPID.Text;
            MyGlobal.TabBackColor = TextHelper.GetSafeString(pnlBackColor.Tag);
            MyGlobal.TabActiveForeColor = TextHelper.GetSafeString(pnlActiveForeColor.Tag);
            MyGlobal.TabInactiveForeColor = TextHelper.GetSafeString(pnlInactiveForeColor.Tag);
            AppConfigHelper.SupportInfo = txtSupportInfo.Text;
        }

        private void ResetTransientConnectionState()
        {
            DatabaseSqlExecutor.CurrentDataSource = DataSourceType.None;
            DatabaseSqlExecutor.DataSourceDisplayName = string.Empty;

            MyGlobal.TabBackColor = string.Empty;
            MyGlobal.TabActiveForeColor = string.Empty;
            MyGlobal.TabInactiveForeColor = string.Empty;
        }
    }
}