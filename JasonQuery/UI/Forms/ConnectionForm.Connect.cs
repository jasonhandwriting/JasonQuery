using Devart.Data.MySql;
using Devart.Data.PostgreSql;
using Devart.Data.SqlServer;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using System;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //20241207 連線時，避免多觸發一次 CheckedChanged 事件，誤更新 Icon 設定值
                _isConnecting = true;

                MyGlobal.IsShowColumnInfo = chkShowColumnInfo.Checked; //先記住使用者是否有勾選

                //針對「不儲存密碼」，先記住密碼內容
                _password1 = GetCurrentPasswordText();

                if (!SaveConnectData())
                {
                    return;
                }

                //針對「不儲存密碼」，此處還原密碼內容
                ApplyCurrentDataSourceForConnect();

                _password2 = _password1;
                SetCurrentPasswordText(_password2);

                if (!TraceLogger.SetEnabled(chkLog.Checked) && chkLog.Checked)
                {
                    chkLog.Checked = false;

                    var message = LocalizationHelper.GetLanguageString("Unable to create the runtime log file. JasonQuery will continue without runtime logging.", "form", GetType().Name, "msg", "LogFileCreateFailed", "Text");

                    if (!string.IsNullOrWhiteSpace(TraceLogger.LastWriteError))
                    {
                        message += $"\r\n\r\n{TraceLogger.LastWriteError}";
                    }

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                chkShowColumnInfo.Checked = MyGlobal.IsShowColumnInfo; //還原使用者是否有勾選
                ConnectToDatabase();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CheckData(false, true, true))
                {
                    return;
                }

                ConnectToDatabase(true);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                //以下還原，才不會引發錯誤 & 變更 Tab Color
                ResetTransientConnectionState();
            }
        }

        private void ApplyCurrentDataSourceForConnect()
        {
            var dataSourceType = GetSelectedDataSourceType();

            DatabaseSqlExecutor.CurrentDataSource = dataSourceType;

            //if (dataSourceType != DataSourceType.Sqlite)
            //{
            //    DatabaseSqlExecutor.QueryTimeoutSeconds = GetCurrentQueryTimeoutSeconds();
            //}
        }

        private string GetCurrentPasswordText()
        {
            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        return txtPassword_Oracle.Text;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return txtPassword_PostgreSQL.Text;
                    }
                case DataSourceType.SqlServer:
                    {
                        return txtPassword_SQLServer.Text;
                    }
                case DataSourceType.MySql:
                    {
                        return txtPassword_MySQL.Text;
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private void SetCurrentPasswordText(string password)
        {
            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        txtPassword_Oracle.Text = password;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        txtPassword_PostgreSQL.Text = password;
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        txtPassword_SQLServer.Text = password;
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        txtPassword_MySQL.Text = password;
                        break;
                    }
            }
        }

        private int GetCurrentQueryTimeoutSeconds()
        {
            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        return (int)nudQueryTimeout_Oracle.Value;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return (int)nudQueryTimeout_PostgreSQL.Value;
                    }
                case DataSourceType.SqlServer:
                    {
                        return (int)nudQueryTimeout_SQLServer.Value;
                    }
                case DataSourceType.MySql:
                    {
                        return (int)nudQueryTimeout_MySQL.Value;
                    }
                default:
                    {
                        return DatabaseSqlExecutor.QueryTimeoutSeconds;
                    }
            }
        }

        private void ConnectToDatabase(bool isTestMode = false)
        {
            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        ConnectToDatabase_Oracle(isTestMode);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        ConnectToDatabase_PostgreSql(isTestMode);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        ConnectToDatabase_SqlServer(isTestMode);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        ConnectToDatabase_MySql(isTestMode);
                        break;
                    }
            }
        }

        private void ConnectToDatabase_Oracle(bool isTestMode = false)
        {
            UpdateConnectionContextFromForm_Oracle();

            var connectionString = string.Empty;

            connectionString = string.Format(connectionString, txtServer_Oracle.Text, txtPort_Oracle.Text, txtUserID_Oracle.Text, txtPassword_Oracle.Text);
            DatabaseSqlExecutor.DbConnectionString = connectionString;

            ApplyOracleConnectAsOption();

            if (TryHandleConnectOrTestMode
            (
                isTestMode,
                () => ExecuteStandardConnectionTest(() => ConnectToDatabase2(), DatabaseSqlExecutor.DbConnectionServer)
            ))
            {
                return;
            }
        }

        private void ConnectToDatabase_PostgreSql(bool isTestMode = false, bool isUpdateDatabaseList = false)
        {
            UpdateConnectionContextFromForm_PostgreSql();

            var builder = new PgSqlConnectionStringBuilder
            {
                UserId = txtUserID_PostgreSQL.Text,
                Password = txtPassword_PostgreSQL.Text,
                PersistSecurityInfo = chkSavePasswords.Checked,
                Host = txtServer_PostgreSQL.Text,
                Port = Convert.ToInt16(txtPort_PostgreSQL.Text),
                Pooling = chkPooling_PostgreSQL.Checked,
                Database = cboDatabase_PostgreSQL.Text,
                ConnectionTimeout = 30,
                DefaultCommandTimeout = 0,
                Unicode = chkUnicode_PostgreSQL.Checked
            };

            DatabaseSqlExecutor.DbConnectionString = builder.ConnectionString;

            if (TryHandleConnectOrTestMode
            (
                isTestMode,
                () => ExecuteDatabaseListConnectionTest
                (
                    () => ConnectToDatabase2(isUpdateDatabaseList),
                    DatabaseSqlExecutor.DbConnectionServer,
                    isUpdateDatabaseList
                )
            ))
            {
                return;
            }
        }

        private void ConnectToDatabase_SqlServer(bool isTestMode = false, bool isUpdateDatabaseList = false)
        {
            UpdateConnectionContextFromForm_SqlServer();

            var builder = new SqlConnectionStringBuilder();
            {
                builder.IntegratedSecurity = false; //false: 表示採用「SQL Server Authentication」
                builder.UserID = txtUserID_SQLServer.Text;
                builder.Password = txtPassword_SQLServer.Text;
                builder.PersistSecurityInfo = chkSavePasswords.Checked;
                builder.DataSource = $"{txtServer_SQLServer.Text},{txtPort_SQLServer.Text}";
                builder.ConnectTimeout = 30;
                builder.InitialCatalog = GetDatabaseNameForConnection(cboDatabase_SQLServer.Text, isUpdateDatabaseList);
                builder.Pooling = chkPooling_SQLServer.Checked;
            }
            ;

            DatabaseSqlExecutor.DbConnectionString = builder.ConnectionString;

            if (TryHandleConnectOrTestMode
            (
                isTestMode,
                () => ExecuteDatabaseListConnectionTest
                (
                    () => ConnectToDatabase2(isUpdateDatabaseList),
                    $"{DatabaseSqlExecutor.DbConnectionServer},{txtPort_SQLServer.Text}",
                    isUpdateDatabaseList,
                    true
                )
            ))
            {
                return;
            }
        }

        private void ConnectToDatabase_MySql(bool isTestMode = false, bool isUpdateDatabaseList = false)
        {
            UpdateConnectionContextFromForm_MySql();

            var builder = new MySqlConnectionStringBuilder
            {
                UserId = txtUserID_MySQL.Text,
                Password = txtPassword_MySQL.Text,
                PersistSecurityInfo = chkSavePasswords.Checked,
                Host = txtServer_MySQL.Text,
                Port = Convert.ToInt16(txtPort_MySQL.Text),
                Pooling = chkPooling_MySQL.Checked,
                Database = GetDatabaseNameForConnection(cboDatabase_MySQL.Text, isUpdateDatabaseList),
                ConnectionTimeout = 30,
                Unicode = chkUnicode_MySQL.Checked,
                Protocol = MySqlProtocol.Tcp
            };

            DatabaseSqlExecutor.DbConnectionString = builder.ConnectionString;

            if (TryHandleConnectOrTestMode
            (
                isTestMode,
                () => ExecuteDatabaseListConnectionTest
                (
                    () => ConnectToDatabase2(isUpdateDatabaseList),
                    $"{DatabaseSqlExecutor.DbConnectionServer},{txtPort_MySQL.Text}",
                    isUpdateDatabaseList
                )
            ))
            {
                return;
            }
        }

        private string ConnectToDatabase2(bool isUpdateDatabaseList = false)
        {
            var result = string.Empty;
            var sbSql = new StringBuilder();

            switch (DatabaseSqlExecutor.CurrentDataSource)
            {
                case DataSourceType.Oracle:
                    {
                        result = MyGlobal.OracleReader.ConnectTo();

                        if (string.IsNullOrEmpty(result))
                        {
                            MyGlobal.OracleReader.Disconnect();
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        result = MyGlobal.PostgreSqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (string.IsNullOrEmpty(result))
                        {
                            if (isUpdateDatabaseList)
                            {
                                SqlTraceHelper.AppendHeader(sbSql, "---Get Database Name for ComboBox list");

                                sbSql.AppendLine("SELECT datname AS Name FROM pg_database");
                                sbSql.AppendLine(" WHERE datistemplate = false");
                                sbSql.Append(" ORDER BY datname");

                                var sql = sbSql.ToString();
                                var dtData = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql);

                                if (dtData != null)
                                {
                                    _dtDatabaseListInfo_PostgreSql = dtData.Copy();
                                    UpdateComboBox(dtData, cboDatabase_PostgreSQL);
                                }
                                else
                                {
                                    cboDatabase_PostgreSQL.Items.Clear();
                                    _dtDatabaseListInfo_PostgreSql = null;
                                }
                            }

                            MyGlobal.PostgreSqlReader.Disconnect();
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        result = MyGlobal.SqlServerReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (string.IsNullOrEmpty(result))
                        {
                            if (isUpdateDatabaseList)
                            {
                                SqlTraceHelper.AppendHeader(sbSql, "---Get Database Name for ComboBox list");

                                sbSql.AppendLine("SELECT Name FROM master.sys.databases");
                                sbSql.Append(" ORDER BY Name");

                                var sql = sbSql.ToString();
                                var dtData = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                                if (dtData != null)
                                {
                                    _dtDatabaseListInfo_SqlServer = dtData.Copy();
                                    UpdateComboBox(dtData, cboDatabase_SQLServer);
                                }
                                else
                                {
                                    cboDatabase_SQLServer.Items.Clear();
                                    _dtDatabaseListInfo_SqlServer = null;
                                }
                            }

                            MyGlobal.SqlServerReader.Disconnect();
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        result = MyGlobal.MySqlReader.ConnectTo(DatabaseSqlExecutor.DbConnectionString);

                        if (string.IsNullOrEmpty(result))
                        {
                            if (isUpdateDatabaseList)
                            {
                                SqlTraceHelper.AppendHeader(sbSql, "---Get Database Name for ComboBox list");

                                sbSql.AppendLine("SELECT Schema_Name AS Name");
                                sbSql.AppendLine("  FROM Information_Schema.schemata");
                                sbSql.Append(" ORDER BY Schema_Name");

                                var sql = sbSql.ToString();
                                var dtData = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);

                                if (dtData != null)
                                {
                                    _dtDatabaseListInfo_MySql = dtData.Copy();
                                    UpdateComboBox(dtData, cboDatabase_MySQL);
                                }
                                else
                                {
                                    cboDatabase_MySQL.Items.Clear();
                                    _dtDatabaseListInfo_MySql = null;
                                }
                            }

                            MyGlobal.MySqlReader.Disconnect();
                        }

                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return result;
        }

        private void ShowTestConnectionSucceededMessage()
        {
            var message = LocalizationHelper.GetLanguageString("Test connection succeeded.", "Global", "Global", "msg", "TestOK", "Text");

            MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildTestConnectionFailedMessage(string connectTo, string serverDisplayText, bool includeTestFailedTitle, bool appendSqlServerTcpHint = false)
        {
            var message = string.Empty;

            if (includeTestFailedTitle)
            {
                _languageText = LocalizationHelper.GetLanguageString("Test connection failed", "Global", "Global", "msg", "TestNG", "Text");
                message = $"{_languageText}\r\n\r\n";
            }

            _languageText = LocalizationHelper.GetLanguageString("Error connecting to the server:", "Global", "Global", "msg", "ErrorConnectingToTheServer", "Text");
            message += $"{_languageText}\r\n\r\n";

            _languageText = LocalizationHelper.GetLanguageString("Connection Name:", "form", "ConnectionForm", "object", "lblConnectionName", "Text");
            message += $"{_languageText} {DatabaseSqlExecutor.DbConnectionName}\r\n";

            _languageText = LocalizationHelper.GetLanguageString("Server:", "form", "ConnectionForm", "object", "lblServer", "Text");
            message += $"{_languageText} {serverDisplayText}\r\n\r\n{connectTo}";

            if (appendSqlServerTcpHint && connectTo.IndexOf("(provider: TCP Provider, error: 0", StringComparison.Ordinal) >= 0)
            {
                var temp = LocalizationHelper.GetLanguageString("Please double check that the TCP/IP protocol is enabled and the port number is correct.", "form", "ConnectionForm", "msg", "ServerTCPIPPortIssue", "Text");

                message += $"\r\n{temp}";
            }

            return message;
        }

        private void ShowTestConnectionFailedMessage(string connectTo, string serverDisplayText, bool includeTestFailedTitle, bool appendSqlServerTcpHint = false)
        {
            var message = BuildTestConnectionFailedMessage(connectTo, serverDisplayText, includeTestFailedTitle, appendSqlServerTcpHint);

            MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool TryHandleConnectOrTestMode(bool isTestMode, Action testAction)
        {
            if (!isTestMode)
            {
                Close();
                return true;
            }

            testAction?.Invoke();
            return false;
        }

        private void ExecuteStandardConnectionTest(Func<string> connectAction, string serverDisplayText)
        {
            var connectTo = connectAction();

            if (!string.IsNullOrEmpty(connectTo))
            {
                ShowTestConnectionFailedMessage(connectTo, serverDisplayText, true);
            }
            else
            {
                ShowTestConnectionSucceededMessage();
            }
        }

        private void ExecuteDatabaseListConnectionTest(Func<string> connectAction, string serverDisplayText, bool isUpdateDatabaseList, bool appendSqlServerTcpHint = false)
        {
            var connectTo = connectAction();

            if (!string.IsNullOrEmpty(connectTo))
            {
                ShowTestConnectionFailedMessage(connectTo, serverDisplayText, !isUpdateDatabaseList, appendSqlServerTcpHint);
            }
            else
            {
                if (isUpdateDatabaseList)
                {
                    return;
                }

                ShowTestConnectionSucceededMessage();
            }
        }

        private void ApplyOracleConnectAsOption()
        {
            switch (cboConnectAs_Oracle.Text)
            {
                case "SYSDBA":
                    {
                        DatabaseSqlExecutor.OracleConnectAs = "SysDba";
                        DatabaseSqlExecutor.DbConnectionString = string.Concat(DatabaseSqlExecutor.DbConnectionString, "DBA Privilege=SYSDBA;");
                        break;
                    }
                case "SYSOPER":
                    {
                        DatabaseSqlExecutor.OracleConnectAs = "SysOper";
                        DatabaseSqlExecutor.DbConnectionString = string.Concat(DatabaseSqlExecutor.DbConnectionString, "DBA Privilege=SYSOPER;");
                        break;
                    }
                default:
                    {
                        DatabaseSqlExecutor.OracleConnectAs = string.Empty;
                        break;
                    }
            }
        }

        private string GetDatabaseNameForConnection(string databaseName, bool isUpdateDatabaseList)
        {
            return isUpdateDatabaseList ? string.Empty : databaseName;
        }
    }
}
