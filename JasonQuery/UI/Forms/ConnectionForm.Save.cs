using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.ConnectionCredentials;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Data.SQLite;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private bool SaveConnectData(bool isPureSave = false)
        {
            var result = false;

            try
            {
                switch (GetSelectedDataSourceType())
                {
                    case DataSourceType.Oracle:
                        {
                            result = SaveConnectData_Oracle(isPureSave);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            result = SaveConnectData_PostgreSql(isPureSave);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            result = SaveConnectData_SqlServer(isPureSave);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            result = SaveConnectData_MySql(isPureSave);
                            break;
                        }
                }

                //指定哪一個 Column 要套用 FetchCellStyle (這裡要重新指定一次，並 Refresh，才會顯示顏色，而不是顯示顏色代碼)
                c1GridDBInfo.Splits[0].DisplayColumns[_gridHeaderMap[menu.TabBackColor]].FetchStyle = true;
                c1GridDBInfo.Refresh();

                //20240210 此處必須更新，否則 MainForm 載入設定值，會恢復成原本的值
                JasonQueryRepository.DbMotherPid = lblPID.Text;
                JasonQueryRepository.UpdateSetting("GeneralConfig", "MainFormIconStyle", _selectedMainFormIconStyleIndex.ToString());
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return result;
        }

        private void MoveToNewestDbInfoRow(out int pid, out int rowIndex)
        {
            pid = 0;
            rowIndex = 0;

            if (_dtDbInfo == null || _dtDbInfo.Rows.Count == 0 || c1GridDBInfo.RowCount == 0)
            {
                return;
            }

            var maxPid = 0;
            var maxIndex = 0;

            for (var i = 0; i < _dtDbInfo.Rows.Count; i++)
            {
                var pidValue = c1GridDBInfo.Columns["PID"].CellValue(i)?.ToString();

                if (int.TryParse(pidValue, out int currentPid))
                {
                    if (currentPid > maxPid)
                    {
                        maxPid = currentPid;
                        maxIndex = i;
                    }
                }
            }

            pid = maxPid;
            rowIndex = maxIndex;

            lblPID.Text = pid.ToString();
            c1GridDBInfo.Row = rowIndex; //指標切換到剛剛新增的那一筆的列數
        }

        private static void ExecuteConnectionProfileWrite(string sql, string storedPassword)
        {
            JasonQueryRepository.ExecNonQuery
            (
                sql,
                new[]
                {
                    new SQLiteParameter("@Password", storedPassword)
                }
            );
        }

        private bool SaveConnectData_Oracle(bool isPureSave = false)
        {
            var result = false;
            string sql;

            if (!CheckData(isPureSave))
            {
                return false;
            }

            var storedPassword = ConnectionCredentialStorageContract.ToV2StoredValue
                                 (
                                     chkSavePasswords.Checked ? txtPassword_Oracle.Text : null
                                 );

            var sbSql = new StringBuilder();
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText().Replace("'", "''");
            var unicode = chkUnicode_Oracle.Checked ? "1" : "0";
            var remark = txtRemark_Oracle.Text.Replace("'", "''");
            var pooling = chkPooling_Oracle.Checked ? "1" : "0";

            if (!string.IsNullOrEmpty(lblPID.Text))
            {
                var currentRow = c1GridDBInfo.Row; //記住目前在第幾列

                sql = "UPDATE DBInfo\r\n";
                sql += $"   SET DataSource = '{dataSourceDisplayText}', DomainUser = '{lblDomainUser.Text}', ConnectionName = '{txtConnectionName_Oracle.Text}',\r\n";
                sql += $"       Server = '{txtServer_Oracle.Text}', SID = '{txtSID_Oracle.Text}', DirectMode = '{(chkDirectMode_Oracle.Checked ? "1" : "0")}', Database = '', Port = '{txtPort_Oracle.Text}',\r\n";
                sql += $"       TabBackColor = '{pnlBackColor.Tag}', TabActiveForeColor = '{pnlActiveForeColor.Tag}', TabInactiveForeColor = '{pnlInactiveForeColor.Tag}', User = '{txtUserID_Oracle.Text}', ConnectAs = '{cboConnectAs_Oracle.Text}', \r\n";
                sql += "       Password = @Password,\r\n";
                sql += $"       AutoRollback = '0', Unicode = '{unicode}', Remarks = '{remark}', O1 = '{pooling}', O2 = '', O3 = '{nudQueryTimeout_Oracle.Value}'";

                if (!isPureSave)
                {
                    sql += $", LastConnect = '{MyGlobal.DateTimeNow()}'\r\n";
                }

                sql += $" WHERE PID = {lblPID.Text}";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                c1GridDBInfo.Row = !isPureSave ? 0 : currentRow;
                result = true;
            }
            else //Add Mode
            {
                sql = "INSERT INTO DBInfo\r\n";
                sql += "       (DataSource, DomainUser, ConnectionName,\r\n";
                sql += "        Server, SID, DirectMode, Database, Port,\r\n";
                sql += "        TabBackColor, TabActiveForeColor, TabInactiveForeColor, User, ConnectAs,\r\n";
                sql += "        Password,\r\n";
                sql += "        AutoRollback, Unicode, Remarks, O1, O2, O3, O4, O5, O6";

                if (!isPureSave)
                {
                    sql += ", LastConnect";
                }

                sql += ")\r\n";
                sql += $"VALUES ('{dataSourceDisplayText}', '{lblDomainUser.Text}', '{txtConnectionName_Oracle.Text}',\r\n";
                sql += $"        '{txtServer_Oracle.Text}', '{txtSID_Oracle.Text}', '{(chkDirectMode_Oracle.Checked ? "1" : "0")}', '', '{txtPort_Oracle.Text}',\r\n";
                sql += $"        '{pnlBackColor.Tag}', '{pnlActiveForeColor.Tag}', '{pnlInactiveForeColor.Tag}', '{txtUserID_Oracle.Text}', '{cboConnectAs_Oracle.Text}',\r\n";
                sql += "        @Password,\r\n";
                sql += $"        '0', '{unicode}', '{remark}', '{pooling}', '', '{nudQueryTimeout_Oracle.Value}', '', '', ''";

                if (!isPureSave)
                {
                    sql += $", '{MyGlobal.DateTimeNow()}'";
                }

                sql += ")";

                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                //找到 PID 數值最大的，就是剛剛新增的資料
                MoveToNewestDbInfoRow(out _, out _);

                result = true;
            }

            return result;
        }

        private bool SaveConnectData_PostgreSql(bool isPureSave = false)
        {
            var result = false;
            string sql;

            if (!CheckData(isPureSave))
            {
                return false;
            }

            var storedPassword = ConnectionCredentialStorageContract.ToV2StoredValue
                                 (
                                     chkSavePasswords.Checked ? txtPassword_PostgreSQL.Text : null
                                 );

            var sbSql = new StringBuilder();
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText().Replace("'", "''");

            if (!string.IsNullOrEmpty(lblPID.Text))
            {
                var currentRow = c1GridDBInfo.Row; //記住目前在第幾列
                var autoRollback = chkAutoRollback_PostgreSQL.Checked ? "1" : "0";
                var unicode = chkUnicode_PostgreSQL.Checked ? "1" : "0";
                var remark = txtRemark_PostgreSQL.Text.Replace("'", "''");
                var pooling = chkPooling_PostgreSQL.Checked ? "1" : "0";

                sql = "UPDATE DBInfo\r\n";
                sql += $"   SET DataSource = '{dataSourceDisplayText}', DomainUser = '{lblDomainUser.Text}', ConnectionName = '{txtConnectionName_PostgreSQL.Text}',\r\n";
                sql += $"       Server = '{txtServer_PostgreSQL.Text}', SID = '', DirectMode = '0', Database = '{cboDatabase_PostgreSQL.Text}', Port = '{txtPort_PostgreSQL.Text}',\r\n";
                sql += $"       TabBackColor = '{pnlBackColor.Tag}', TabActiveForeColor = '{pnlActiveForeColor.Tag}', TabInactiveForeColor = '{pnlInactiveForeColor.Tag}', User = '{txtUserID_PostgreSQL.Text}', ConnectAs = '',\r\n";
                sql += "       Password = @Password,\r\n";
                sql += $"       AutoRollback = '{autoRollback}', Unicode = '{unicode}', Remarks = '{remark}', O1 = '{pooling}', O2 = '', O3 = '{nudQueryTimeout_PostgreSQL.Value}'";

                if (!isPureSave)
                {
                    sql += $", LastConnect = '{MyGlobal.DateTimeNow()}' \r\n";
                }

                sql += $" WHERE PID = {lblPID.Text}";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                c1GridDBInfo.Row = !isPureSave ? 0 : currentRow;
                result = true;
            }
            else //Add Mode
            {
                sql = "INSERT INTO DBInfo\r\n";
                sql += "       (DataSource, DomainUser, ConnectionName,\r\n";
                sql += "        Server, SID, DirectMode, Database, Port,\r\n";
                sql += "        TabBackColor, TabActiveForeColor, TabInactiveForeColor, User, ConnectAs,\r\n";
                sql += "        Password,\r\n";
                sql += "        AutoRollback, Unicode, Remarks, O1, O2, O3, O4, O5, O6";

                if (!isPureSave)
                {
                    sql += ", LastConnect";
                }

                var autoRollback = chkAutoRollback_PostgreSQL.Checked ? "1" : "0";
                var unicode = chkUnicode_PostgreSQL.Checked ? "1" : "0";
                var remark = txtRemark_PostgreSQL.Text.Replace("'", "''");
                var pooling = chkPooling_PostgreSQL.Checked ? "1" : "0";

                sql += ")\r\n";
                sql += $"VALUES ('{dataSourceDisplayText}', '{lblDomainUser.Text}', '{txtConnectionName_PostgreSQL.Text}',\r\n";
                sql += $"        '{txtServer_PostgreSQL.Text}', '', '0', '{cboDatabase_PostgreSQL.Text}', '{txtPort_PostgreSQL.Text}',\r\n";
                sql += $"        '{pnlBackColor.Tag}', '{pnlActiveForeColor.Tag}', '{pnlInactiveForeColor.Tag}', '{txtUserID_PostgreSQL.Text}', '',\r\n";
                sql += "        @Password,\r\n";
                sql += $"        '{autoRollback}', '{unicode}', '{remark}', '{pooling}', '', '{nudQueryTimeout_PostgreSQL.Value}', '', '', ''";

                if (!isPureSave)
                {
                    sql += $", '{MyGlobal.DateTimeNow()}'";
                }

                sql += ")";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                //找到 PID 數值最大的，就是剛剛新增的資料
                MoveToNewestDbInfoRow(out _, out _);

                result = true;
            }

            return result;
        }

        private bool SaveConnectData_SqlServer(bool isPureSave = false)
        {
            var result = false;
            string sql;

            if (!CheckData(isPureSave))
            {
                return false;
            }

            var storedPassword = ConnectionCredentialStorageContract.ToV2StoredValue
                                 (
                                     chkSavePasswords.Checked ? txtPassword_SQLServer.Text : null
                                 );

            var sbSql = new StringBuilder();
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText().Replace("'", "''");

            if (!string.IsNullOrEmpty(lblPID.Text))
            {
                var currentRow = c1GridDBInfo.Row; //記住目前在第幾列
                var remark = txtRemark_SQLServer.Text.Replace("'", "''");
                var pooling = chkPooling_SQLServer.Checked ? "1" : "0";
                var excludeNativeObject = chkExcludeNativeObject_SQLServer.Checked ? "1" : "0";

                sql = "UPDATE DBInfo\r\n";
                sql += $"   SET DataSource = '{dataSourceDisplayText}', DomainUser = '{lblDomainUser.Text}', ConnectionName = '{txtConnectionName_SQLServer.Text}',\r\n";
                sql += $"       Server = '{txtServer_SQLServer.Text}', SID = '', DirectMode = '0', Database = '{cboDatabase_SQLServer.Text}', Port = '{txtPort_SQLServer.Text}',\r\n";
                sql += $"       TabBackColor = '{pnlBackColor.Tag}', TabActiveForeColor = '{pnlActiveForeColor.Tag}', TabInactiveForeColor = '{pnlInactiveForeColor.Tag}', User = '{txtUserID_SQLServer.Text}', ConnectAs = '',\r\n";
                sql += "       Password = @Password,\r\n";
                sql += $"       AutoRollback = '0', Unicode = '0', Remarks = '{remark}', O1 = '{pooling}', O2 = '{excludeNativeObject}', O3 = '{nudQueryTimeout_SQLServer.Value}'";

                if (!isPureSave)
                {
                    sql += $", LastConnect = '{MyGlobal.DateTimeNow()}'\r\n";
                }

                sql += $" WHERE PID = {lblPID.Text}";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                c1GridDBInfo.Row = !isPureSave ? 0 : currentRow;
                result = true;
            }
            else //Add Mode
            {
                sql = "INSERT INTO DBInfo\r\n";
                sql += "       (DataSource, DomainUser, ConnectionName,\r\n";
                sql += "        Server, SID, DirectMode, Database, Port,\r\n";
                sql += "        TabBackColor, TabActiveForeColor, TabInactiveForeColor, User, ConnectAs,\r\n";
                sql += "        Password,\r\n";
                sql += "        AutoRollback, Unicode, Remarks, O1, O2, O3, O4, O5, O6";

                if (!isPureSave)
                {
                    sql += ", LastConnect";
                }

                var remark = txtRemark_SQLServer.Text.Replace("'", "''");
                var pooling = chkPooling_SQLServer.Checked ? "1" : "0";
                var excludeNativeObject = chkExcludeNativeObject_SQLServer.Checked ? "1" : "0";

                sql += ")\r\n";
                sql += $"VALUES ('{dataSourceDisplayText}', '{lblDomainUser.Text}', '{txtConnectionName_SQLServer.Text}', \r\n";
                sql += $"        '{txtServer_SQLServer.Text}', '', '0', '{cboDatabase_SQLServer.Text}', '{txtPort_SQLServer.Text}', \r\n";
                sql += $"        '{pnlBackColor.Tag}', '{pnlActiveForeColor.Tag}', '{pnlInactiveForeColor.Tag}', '{txtUserID_SQLServer.Text}', '', \r\n";
                sql += "        @Password, \r\n";
                sql += $"        '0', '0', '{remark}', '{pooling}', '{excludeNativeObject}', '{nudQueryTimeout_SQLServer.Value}', '', '', ''";

                if (!isPureSave)
                {
                    sql += $", '{MyGlobal.DateTimeNow()}'";
                }

                sql += ")";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                //找到 Pid 數值最大的，就是剛剛新增的資料
                MoveToNewestDbInfoRow(out _, out _);

                result = true;
            }

            return result;
        }

        private bool SaveConnectData_MySql(bool isPureSave = false)
        {
            var result = false;
            string sql;

            if (!CheckData(isPureSave))
            {
                return false;
            }

            var storedPassword = ConnectionCredentialStorageContract.ToV2StoredValue
                                 (
                                     chkSavePasswords.Checked ? txtPassword_MySQL.Text : null
                                 );

            var sbSql = new StringBuilder();
            var dataSourceDisplayText = GetSelectedDataSourceDisplayText().Replace("'", "''");

            if (!string.IsNullOrEmpty(lblPID.Text))
            {
                var currentRow = c1GridDBInfo.Row; //記住目前在第幾列
                var unicode = chkUnicode_MySQL.Checked ? "1" : "0";
                var remark = txtRemark_MySQL.Text.Replace("'", "''");
                var pooling = chkPooling_MySQL.Checked ? "1" : "0";

                sql = "UPDATE DBInfo\r\n";
                sql += $"   SET DataSource = '{dataSourceDisplayText}', DomainUser = '{lblDomainUser.Text}', ConnectionName = '{txtConnectionName_MySQL.Text}',\r\n";
                sql += $"       Server = '{txtServer_MySQL.Text}', SID = '', DirectMode = '0', Database = '{cboDatabase_MySQL.Text}', Port = '{txtPort_MySQL.Text}',\r\n";
                sql += $"       TabBackColor = '{pnlBackColor.Tag}', TabActiveForeColor = '{pnlActiveForeColor.Tag}', TabInactiveForeColor = '{pnlInactiveForeColor.Tag}', User = '{txtUserID_MySQL.Text}', ConnectAs = '',\r\n";
                sql += "       Password = @Password,\r\n";
                sql += $"       AutoRollback = '0', Unicode = '{unicode}', Remarks = '{remark}', O1 = '{pooling}', O2 = '', O3 = '{nudQueryTimeout_MySQL.Value}'";

                if (!isPureSave)
                {
                    sql += $", LastConnect = '{MyGlobal.DateTimeNow()}'\r\n";
                }

                sql += $" WHERE PID = {lblPID.Text}";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                c1GridDBInfo.Row = !isPureSave ? 0 : currentRow;
                result = true;
            }
            else //Add Mode
            {
                sql = "INSERT INTO DBInfo\r\n";
                sql += "       (DataSource, DomainUser, ConnectionName,\r\n";
                sql += "        Server, SID, DirectMode, Database, Port,\r\n";
                sql += "        TabBackColor, TabActiveForeColor, TabInactiveForeColor, User, ConnectAs,\r\n";
                sql += "        Password,\r\n";
                sql += "        AutoRollback, Unicode, Remarks, O1, O2, O3, O4, O5, O6";

                if (!isPureSave)
                {
                    sql += ", LastConnect";
                }

                var unicode = chkUnicode_MySQL.Checked ? "1" : "0";
                var remark = txtRemark_MySQL.Text.Replace("'", "''");
                var pooling = chkPooling_MySQL.Checked ? "1" : "0";

                sql += ")\r\n";
                sql += $"VALUES ('{dataSourceDisplayText}', '{lblDomainUser.Text}', '{txtConnectionName_MySQL.Text}',\r\n";
                sql += $"        '{txtServer_MySQL.Text}', '', '0', '{cboDatabase_MySQL.Text}', '{txtPort_MySQL.Text}',\r\n";
                sql += $"        '{pnlBackColor.Tag}', '{pnlActiveForeColor.Tag}', '{pnlInactiveForeColor.Tag}', '{txtUserID_MySQL.Text}', '',\r\n";
                sql += "        @Password,\r\n";
                sql += $"        '0', '{unicode}', '{remark}', '{pooling}', '', '{nudQueryTimeout_MySQL.Value}', '', '', ''";

                if (!isPureSave)
                {
                    sql += $", '{MyGlobal.DateTimeNow()}'";
                }

                sql += ")";
                ExecuteConnectionProfileWrite(sql, storedPassword);

                //重新載入 DBInfo 資料
                ReloadDbInfoGrid();

                //找到 Pid 數值最大的，就是剛剛新增的資料
                MoveToNewestDbInfoRow(out _, out _);

                result = true;
            }

            return result;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _password1 = _password2;

                var showColumnInfoChecked = chkShowColumnInfo.Checked;

                if (SaveConnectData(true))
                {
                    btnCopy.Enabled = true;
                }

                _password2 = _password1;
                chkShowColumnInfo.Checked = showColumnInfoChecked;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
