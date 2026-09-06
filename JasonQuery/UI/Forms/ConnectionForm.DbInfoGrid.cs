using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Database.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.Legacy;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private enum menu
        {
            Pid = 0,
            DomainUser,
            DataSource,
            ConnectionName,
            Server,
            Sid,
            DirectMode,
            Database,
            ConnectAs,
            Port,
            UserId,
            Password,
            LastConnect,
            TabBackColor,
            TabActiveForeColor,
            TabInactiveForeColor,
            Unicode,
            AutoRollback,
            Pooling,
            ExcludeNativeDatabases,
            QueryTimeout,
            Remarks,
            DatabaseFile,
            DatabaseType,
            WithPassword
        }

        private static readonly Dictionary<menu, string> _gridHeaderMap = new Dictionary<menu, string>
        {
            { menu.Pid, "PID" },
            { menu.DomainUser, "DomainUser" },
            { menu.DataSource, "DataSource" },
            { menu.ConnectionName, "ConnectionName" },
            { menu.Server, "Server" },
            { menu.Sid, "SID" },
            { menu.DirectMode, "DirectMode" },
            { menu.Database, "Database" },
            { menu.ConnectAs, "ConnectAs" },
            { menu.Port, "Port" },
            { menu.UserId, "UserID" },
            { menu.Password, "Password" },
            { menu.LastConnect, "LastConnect" },
            { menu.TabBackColor, "TabBackColor" },
            { menu.TabActiveForeColor, "TabActiveForeColor" },
            { menu.TabInactiveForeColor, "TabInactiveForeColor" },
            { menu.Unicode, "Unicode" },
            { menu.AutoRollback, "AutoRollback" },
            { menu.Pooling, "Pooling" },
            { menu.ExcludeNativeDatabases, "ExcludeNativeDatabases" },
            { menu.QueryTimeout, "QueryTimeout" },
            { menu.Remarks, "Remarks" },
            { menu.DatabaseFile, "DatabaseFile" },
            { menu.DatabaseType, "DatabaseType" },
            { menu.WithPassword, "WithPassword" }
        };

        private static readonly HashSet<string> _hiddenDbInfoFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            _gridHeaderMap[menu.Pid],
            _gridHeaderMap[menu.DirectMode],
            _gridHeaderMap[menu.Sid],
            _gridHeaderMap[menu.ConnectAs],
            _gridHeaderMap[menu.Database],
            _gridHeaderMap[menu.DomainUser],
            _gridHeaderMap[menu.Password],
            _gridHeaderMap[menu.TabActiveForeColor],
            _gridHeaderMap[menu.TabInactiveForeColor],
            _gridHeaderMap[menu.Unicode],
            _gridHeaderMap[menu.AutoRollback],
            _gridHeaderMap[menu.Pooling],
            _gridHeaderMap[menu.ExcludeNativeDatabases],
            _gridHeaderMap[menu.QueryTimeout],
            _gridHeaderMap[menu.DatabaseFile],
            _gridHeaderMap[menu.DatabaseType],
            _gridHeaderMap[menu.WithPassword]
        };

        private int CreateAndGetDbInfoTable()
        {
            _dtDbInfo = new DataTable();

            foreach (var columnName in _gridHeaderMap.Values)
            {
                _dtDbInfo.Columns.Add(columnName);
            }

            var sql = $"SELECT * FROM DBInfo WHERE DomainUser = '{MyGlobal.DomainUser}' ORDER BY LastConnect DESC";
            var dt = JasonQueryRepository.ExecQuery(sql);

            if (dt?.Rows.Count > 0)
            {
                foreach (DataRow dr in dt?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var lastConnect = dr.GetSafeString("LastConnect");

                    lastConnect = string.IsNullOrEmpty(lastConnect) ? string.Empty : Convert.ToDateTime(lastConnect).ToString($"{MyLibrary.DateFormat} HH:mm:ss");

                    _rowDbInfo = _dtDbInfo.NewRow();
                    _rowDbInfo[_gridHeaderMap[menu.Pid]] = dr.GetSafeString("PID", "0");
                    _rowDbInfo[_gridHeaderMap[menu.DomainUser]] = dr.GetSafeString("DomainUser");
                    _rowDbInfo[_gridHeaderMap[menu.ConnectionName]] = dr.GetSafeString("ConnectionName");
                    _rowDbInfo[_gridHeaderMap[menu.DataSource]] = dr.GetSafeString("DataSource");
                    _rowDbInfo[_gridHeaderMap[menu.Server]] = dr.GetSafeString("Server");
                    _rowDbInfo[_gridHeaderMap[menu.Sid]] = dr.GetSafeString("SID");
                    _rowDbInfo[_gridHeaderMap[menu.DirectMode]] = dr.GetSafeString("DirectMode");
                    _rowDbInfo[_gridHeaderMap[menu.Database]] = dr.GetSafeString("Database");
                    _rowDbInfo[_gridHeaderMap[menu.ConnectAs]] = dr.GetSafeString("ConnectAs");
                    _rowDbInfo[_gridHeaderMap[menu.Port]] = dr.GetSafeString("Port");
                    _rowDbInfo[_gridHeaderMap[menu.UserId]] = dr.GetSafeString("User");
                    _rowDbInfo[_gridHeaderMap[menu.Password]] = dr.GetSafeString("Password");
                    _rowDbInfo[_gridHeaderMap[menu.LastConnect]] = lastConnect;
                    _rowDbInfo[_gridHeaderMap[menu.TabBackColor]] = dr.GetSafeString("TabBackColor");
                    _rowDbInfo[_gridHeaderMap[menu.TabActiveForeColor]] = dr.GetSafeString("TabActiveForeColor");
                    _rowDbInfo[_gridHeaderMap[menu.TabInactiveForeColor]] = dr.GetSafeString("TabInactiveForeColor");
                    _rowDbInfo[_gridHeaderMap[menu.Unicode]] = dr.GetSafeString("Unicode");
                    _rowDbInfo[_gridHeaderMap[menu.AutoRollback]] = dr.GetSafeString("AutoRollback");
                    _rowDbInfo[_gridHeaderMap[menu.Pooling]] = dr.GetSafeString("O1");
                    _rowDbInfo[_gridHeaderMap[menu.ExcludeNativeDatabases]] = dr.GetSafeString("O2");
                    _rowDbInfo[_gridHeaderMap[menu.QueryTimeout]] = dr.GetSafeString("O3");
                    _rowDbInfo[_gridHeaderMap[menu.Remarks]] = dr.GetSafeString("Remarks");
                    _rowDbInfo[_gridHeaderMap[menu.DatabaseFile]] = dr.GetSafeString("O4");
                    _rowDbInfo[_gridHeaderMap[menu.DatabaseType]] = dr.GetSafeString("O5");
                    _rowDbInfo[_gridHeaderMap[menu.WithPassword]] = dr.GetSafeString("O6");
                    _dtDbInfo.Rows.Add(_rowDbInfo);
                }

            }

            c1GridDBInfo.DataSource = _dtDbInfo;

            ApplyDbInfoGridColumnLayout();

            c1GridDBInfo.RowHeight = 21;
            c1GridDBInfo.Splits[0].ColumnCaptionHeight = 24;
            c1GridDBInfo.Refresh();

            return dt.Rows.Count;
        }

        private int ReloadDbInfoGrid()
        {
            var rowCount = CreateAndGetDbInfoTable();

            ApplyDbInfoTabColorFetchStyle(rowCount);
            ApplyDbInfoActionButtonState(rowCount);

            return rowCount;
        }

        private void ApplyDbInfoTabColorFetchStyle(int rowCount)
        {
            if (rowCount <= 0)
            {
                return;
            }

            //指定哪一個 Column 要套用 FetchCellStyle (重新指定一次，才會顯示顏色，而不是顯示顏色代碼)
            c1GridDBInfo.Splits[0].DisplayColumns[_gridHeaderMap[menu.TabBackColor]].FetchStyle = true;
            c1GridDBInfo.Refresh();
        }

        private void ApplyDbInfoActionButtonState(int rowCount)
        {
            var hasConnectionProfiles = rowCount > 0;

            btnExport.Enabled = hasConnectionProfiles;
            btnSave.Enabled = hasConnectionProfiles;
            btnTest.Enabled = hasConnectionProfiles;
            btnDelete.Enabled = hasConnectionProfiles;
        }

        private void RefreshDbInfoGridCaptions()
        {
            if (c1GridDBInfo.DataSource == null)
            {
                return;
            }

            GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridDBInfo, Name);
            c1GridDBInfo.Refresh();
        }

        private string GetCellValue(menu menu, int row)
        {
            return c1GridDBInfo.Columns[_gridHeaderMap[menu]].CellValue(row).ToString();
        }

        private void c1GridDBInfo_RowColChange(object sender, RowColChangeEventArgs e)
        {
            if (c1GridDBInfo.RowCount == 0)
            {
                return;
            }

            try
            {
                var currentRow = c1GridDBInfo.Row;
                var columnDataSource = GetCellValue(menu.DataSource, currentRow);

                cboDataSource.Enabled = true;
                SetSelectedDataSource(columnDataSource);
                cboDataSource.Enabled = false;

                var dataSourceType = GetSelectedDataSourceType();

                //if (dataSourceType == DataSourceType.Sqlite)
                //{
                //    //Grid_RowColChange_SQLite();
                //}
                //else
                {
                    DatabaseProviderKind providerKind;

                    if (TryGetSelectedProviderKind(out providerKind))
                    {
                        Grid_RowColChange(providerKind);
                    }
                }

                //20240210 讀取主畫面圖示的樣式
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT * FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {lblPID.Text}");
                sbSql.AppendLine("   AND AttributeKey = 'GeneralConfig'");
                sbSql.Append("   AND AttributeName = 'MainFormIconStyle'");

                var sql = sbSql.ToString();
                var dtTemp = JasonQueryRepository.ExecQuery(sql);

                int.TryParse(dtTemp.Rows.Count > 0 ? dtTemp.Rows[0].GetSafeString("AttributeValue") : DefaultMainFormIconStyleIndex.ToString(), out AppConfigHelper.MainFormIconStyle);

                if (AppConfigHelper.MainFormIconStyle < 0 || AppConfigHelper.MainFormIconStyle >= _lstRdoIconStyle.Count)
                {
                    AppConfigHelper.MainFormIconStyle = DefaultMainFormIconStyleIndex;
                }

                ApplyMainFormIconStyleSelection(AppConfigHelper.MainFormIconStyle, !_isConnecting);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Grid_RowColChange(DatabaseProviderKind providerKind)
        {
            switch (providerKind)
            {
                case DatabaseProviderKind.Oracle:
                    {
                        Grid_RowColChange_Oracle();
                        break;
                    }
                case DatabaseProviderKind.PostgreSql:
                    {
                        _dtDatabaseListInfo_PostgreSql = null; //切換時要清空！
                        Grid_RowColChange_PostgreSql();
                        break;
                    }
                case DatabaseProviderKind.SqlServer:
                    {
                        _dtDatabaseListInfo_SqlServer = null; //切換時要清空！
                        Grid_RowColChange_SqlServer();
                        break;
                    }
                case DatabaseProviderKind.MySql:
                    {
                        _dtDatabaseListInfo_MySql = null; //切換時要清空！
                        Grid_RowColChange_MySql();
                        break;
                    }
            }
        }

        private void GetSetBasis(int currentRow)
        {
            lblPID.Text = GetCellValue(menu.Pid, currentRow);
            lblDomainUser.Text = GetCellValue(menu.DomainUser, currentRow);

            btnDelete.Enabled = true;
            btnCopy.Enabled = true;
            chkDarkMode.Checked = GetDarkModeValue();
            chkShowColumnInfo.Checked = true; //20250205 固定為顯示欄位資訊 (因為 0.90 版本開始，已改為不會在啟動時讀取/整理欄位資訊，啟動速度加快了，故此處改為一律顯示)

            var tabBackColor = GetCellValue(menu.TabBackColor, currentRow);
            var tabActiveForeColor = GetCellValue(menu.TabActiveForeColor, currentRow);
            var tabInactiveForColor = GetCellValue(menu.TabInactiveForeColor, currentRow);

            pnlBackColor.BackColor = ColorTranslator.FromHtml(tabBackColor);
            pnlBackColor.Tag = tabBackColor;
            pnlActiveForeColor.BackColor = ColorTranslator.FromHtml(tabActiveForeColor);
            pnlActiveForeColor.Tag = tabActiveForeColor;
            pnlInactiveForeColor.BackColor = ColorTranslator.FromHtml(tabInactiveForColor);
            pnlInactiveForeColor.Tag = tabInactiveForColor;

            _toolTip1.SetToolTip(pnlBackColor, $"{pnlBackColor.Tag} (R:{pnlBackColor.BackColor.R}, G:{pnlBackColor.BackColor.G}, B:{pnlBackColor.BackColor.B})");
            _toolTip1.SetToolTip(pnlActiveForeColor, $"{pnlActiveForeColor.Tag} (R:{pnlActiveForeColor.BackColor.R}, G:{pnlActiveForeColor.BackColor.G}, B:{pnlActiveForeColor.BackColor.B})");
            _toolTip1.SetToolTip(pnlInactiveForeColor, $"{pnlInactiveForeColor.Tag} (R:{pnlInactiveForeColor.BackColor.R}, G:{pnlInactiveForeColor.BackColor.G}, B:{pnlInactiveForeColor.BackColor.B})");

            tabExample.BackColor = pnlBackColor.BackColor;
            tabExample.ForeColor = pnlActiveForeColor.BackColor;
            tabExample.TextInactiveColor = pnlInactiveForeColor.BackColor;
        }

        private bool GetDarkModeValue()
        {
            var value = false;
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {lblPID.Text}");
            sbSql.AppendLine("   AND AttributeKey = 'GeneralConfig'");
            sbSql.Append("   AND AttributeName = 'DarkMode'");

            var sql = sbSql.ToString();
            var dtTemp = JasonQueryRepository.ExecQuery(sql);

            if (dtTemp.Rows.Count > 0 && dtTemp.Rows[0]["AttributeValue"].ToString() == "1")
            {
                value = true;
            }

            return value;
        }

        private void ShowWrongPasswordMessage()
        {
            _languageText = LocalizationHelper.GetLanguageString("Wrong password!", "form", GetType().Name, "msg", "WrongPassword", "Text");

            var message = $"{_languageText}\r\n\r\n";

            _languageText = LocalizationHelper.GetLanguageString("Could not resolve your password correctly.", "form", GetType().Name, "msg", "Resolve", "Text");
            message += $"{_languageText}\r\n";

            _languageText = LocalizationHelper.GetLanguageString("You must re-enter your password.", "form", GetType().Name, "msg", "ReEnter", "Text");
            message += _languageText;

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RestorePasswordFromGrid(string encryptedPassword, Control passwordControl)
        {
            var passwordResult = string.Empty;

            //這裡改為各別判斷，才不會出現「先勾再取消」的「特殊狀況」
            chkSavePasswords.Checked = !string.IsNullOrEmpty(encryptedPassword);

            if (!string.IsNullOrEmpty(encryptedPassword))
            {
                passwordResult = LegacyConnectionCredentialSecurity.Unprotect(encryptedPassword, MyGlobal.DomainUser);
            }

            if (string.IsNullOrEmpty(encryptedPassword) && TextHelper.IsNullOrEmptyTag(passwordControl.Tag))
            {
                passwordControl.Text = string.Empty;
                passwordControl.Focus();
            }
            else if (string.IsNullOrEmpty(passwordResult) && !string.IsNullOrEmpty(encryptedPassword))
            {
                passwordControl.Text = string.Empty;
                ShowWrongPasswordMessage();
                passwordControl.Focus();
            }
            else
            {
                var tag = TextHelper.GetSafeString(passwordControl.Tag);

                passwordControl.Text = string.IsNullOrEmpty(passwordResult) ? tag : passwordResult;
            }
        }

        private void Grid_RowColChange_Oracle()
        {
            var currentRow = c1GridDBInfo.Row;

            //20250607 將通用的程式碼抽取至函數
            GetSetBasis(currentRow);

            var connectionName = GetCellValue(menu.ConnectionName, currentRow);

            //Text & Tag：當 Update Mode 時，如果 Connection Name 有變更，檢查是否已經有重複的 Connection Name 存在了
            txtConnectionName_Oracle.Text = connectionName;
            txtConnectionName_Oracle.Tag = connectionName;

            txtServer_Oracle.Text = GetCellValue(menu.Server, currentRow);
            UIHelper.SetC1TextBoxText(txtSID_Oracle, GetCellValue(menu.Sid, currentRow));
            chkDirectMode_Oracle.Checked = GetCellValue(menu.DirectMode, currentRow) == "1";
            txtPort_Oracle.Text = GetCellValue(menu.Port, currentRow);
            txtUserID_Oracle.Text = GetCellValue(menu.UserId, currentRow);

            var connectAs = GetCellValue(menu.ConnectAs, currentRow);

            if (cboConnectAs_Oracle.Items.Contains(connectAs))
            {
                UIHelper.SetC1ComboboxTextIgnoreVisible(cboConnectAs_Oracle, connectAs);
            }
            else
            {
                cboConnectAs_Oracle.SelectedIndex = -1;
            }

            txtRemark_Oracle.Text = GetCellValue(menu.Remarks, currentRow);
            chkPooling_Oracle.Checked = GetCellValue(menu.Pooling, currentRow) == "1";
            chkUnicode_Oracle.Checked = GetCellValue(menu.Unicode, currentRow) == "1";

            int queryTimeoutValue = 0;
            var queryTimeout = GetCellValue(menu.QueryTimeout, currentRow);

            if (string.IsNullOrEmpty(queryTimeout))
            {
                queryTimeoutValue = 30;
            }
            else
            {
                int.TryParse(queryTimeout, out queryTimeoutValue);
            }

            nudQueryTimeout_Oracle.Value = queryTimeoutValue;

            var password = GetCellValue(menu.Password, currentRow);

            RestorePasswordFromGrid(password, txtPassword_Oracle);

            cboDataSource.Focus();
            txtConnectionName_Oracle.Focus();
        }

        private void Grid_RowColChange_PostgreSql()
        {
            var currentRow = c1GridDBInfo.Row;

            //20250607 將通用的程式碼抽取至函數
            GetSetBasis(currentRow);

            var connectionName = GetCellValue(menu.ConnectionName, currentRow);

            //Text & Tag：當 Update Mode 時，如果 Connection Name 有變更，檢查是否已經有重複的 Connection Name 存在了
            txtConnectionName_PostgreSQL.Text = connectionName;
            txtConnectionName_PostgreSQL.Tag = connectionName;

            txtServer_PostgreSQL.Text = GetCellValue(menu.Server, currentRow);
            txtPort_PostgreSQL.Text = GetCellValue(menu.Port, currentRow);
            cboDatabase_PostgreSQL.Text = GetCellValue(menu.Database, currentRow);
            cboDatabase_PostgreSQL.BackColor = _colorEssentialField;

            chkPooling_PostgreSQL.Checked = GetCellValue(menu.Pooling, currentRow) == "1";
            chkUnicode_PostgreSQL.Checked = GetCellValue(menu.Unicode, currentRow) == "1";
            chkAutoRollback_PostgreSQL.Checked = GetCellValue(menu.AutoRollback, currentRow) == "1";

            txtUserID_PostgreSQL.Text = GetCellValue(menu.UserId, currentRow);
            txtRemark_PostgreSQL.Text = GetCellValue(menu.Remarks, currentRow);

            int queryTimeoutValue = 0;
            var queryTimeout = GetCellValue(menu.QueryTimeout, currentRow);

            if (string.IsNullOrEmpty(queryTimeout))
            {
                queryTimeoutValue = 30;
            }
            else
            {
                int.TryParse(queryTimeout, out queryTimeoutValue);
            }

            nudQueryTimeout_PostgreSQL.Value = queryTimeoutValue;

            var password = GetCellValue(menu.Password, currentRow);

            RestorePasswordFromGrid(password, txtPassword_PostgreSQL);

            cboDataSource.Focus();
            txtConnectionName_PostgreSQL.Focus();
        }

        private void Grid_RowColChange_SqlServer()
        {
            var currentRow = c1GridDBInfo.Row;

            //20250607 將通用的程式碼抽取至函數
            GetSetBasis(currentRow);

            var connectionName = GetCellValue(menu.ConnectionName, currentRow);

            //Text & Tag：當 Update Mode 時，如果 Connection Name 有變更，檢查是否已經有重複的 Connection Name 存在了
            txtConnectionName_SQLServer.Text = connectionName;
            txtConnectionName_SQLServer.Tag = connectionName;

            txtServer_SQLServer.Text = c1GridDBInfo.Columns[_gridHeaderMap[menu.Server]].CellValue(currentRow).ToString();
            cboDatabase_SQLServer.Text = c1GridDBInfo.Columns[_gridHeaderMap[menu.Database]].CellValue(currentRow).ToString();
            txtPort_SQLServer.Text = c1GridDBInfo.Columns[_gridHeaderMap[menu.Port]].CellValue(currentRow).ToString();
            txtPort_SQLServer.Text = txtPort_SQLServer.Text == @"0" ? @"1433" : txtPort_SQLServer.Text;

            txtUserID_SQLServer.Text = GetCellValue(menu.UserId, currentRow);
            txtRemark_SQLServer.Text = GetCellValue(menu.Remarks, currentRow);

            chkPooling_SQLServer.Checked = GetCellValue(menu.Pooling, currentRow) == "1";

            var excludeNativeDatabases = GetCellValue(menu.ExcludeNativeDatabases, currentRow);

            chkExcludeNativeObject_SQLServer.Checked = excludeNativeDatabases == "1" || string.IsNullOrEmpty(excludeNativeDatabases);

            int queryTimeoutValue = 0;
            var queryTimeout = GetCellValue(menu.QueryTimeout, currentRow);

            if (string.IsNullOrEmpty(queryTimeout))
            {
                queryTimeoutValue = 30;
            }
            else
            {
                int.TryParse(queryTimeout, out queryTimeoutValue);
            }

            nudQueryTimeout_SQLServer.Value = queryTimeoutValue;

            var password = GetCellValue(menu.Password, currentRow);

            RestorePasswordFromGrid(password, txtPassword_SQLServer);

            cboDataSource.Focus();
            txtConnectionName_SQLServer.Focus();
        }

        private void Grid_RowColChange_MySql()
        {
            var currentRow = c1GridDBInfo.Row;

            //20250607 將通用的程式碼抽取至函數
            GetSetBasis(currentRow);

            var connectionName = GetCellValue(menu.ConnectionName, currentRow);

            //Text & Tag：當 Update Mode 時，如果 Connection Name 有變更，檢查是否已經有重複的 Connection Name 存在了
            txtConnectionName_MySQL.Text = connectionName;
            txtConnectionName_MySQL.Tag = connectionName;

            txtServer_MySQL.Text = GetCellValue(menu.Server, currentRow);
            cboDatabase_MySQL.Text = GetCellValue(menu.Database, currentRow);
            txtPort_MySQL.Text = GetCellValue(menu.Port, currentRow);
            txtPort_MySQL.Text = txtPort_MySQL.Text == @"0" ? @"3306" : txtPort_MySQL.Text;

            txtUserID_MySQL.Text = GetCellValue(menu.UserId, currentRow);
            txtRemark_MySQL.Text = GetCellValue(menu.Remarks, currentRow);

            chkPooling_MySQL.Checked = GetCellValue(menu.Pooling, currentRow) == "1";
            chkUnicode_MySQL.Checked = GetCellValue(menu.Unicode, currentRow) == "1";

            int queryTimeoutValue = 0;
            var queryTimeout = GetCellValue(menu.QueryTimeout, currentRow);

            if (string.IsNullOrEmpty(queryTimeout))
            {
                queryTimeoutValue = 30;
            }
            else
            {
                int.TryParse(queryTimeout, out queryTimeoutValue);
            }

            nudQueryTimeout_MySQL.Value = queryTimeoutValue;

            var password = GetCellValue(menu.Password, currentRow);

            RestorePasswordFromGrid(password, txtPassword_MySQL);

            cboDataSource.Focus();
            txtConnectionName_MySQL.Focus();
        }

        private void c1GridDBInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var row = c1GridDBInfo.RowContaining(e.Y);

            if (row != -1)
            {
                c1GridDBInfo.Enabled = false;
                btnConnect.PerformClick();
            }
        }

        private void c1GridDBInfo_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            var color = c1GridDBInfo.Columns[(int)menu.TabBackColor].CellText(e.Row);

            e.CellStyle.ForeColor = ColorTranslator.FromHtml(color);
            e.CellStyle.BackColor = ColorTranslator.FromHtml(color);
        }

        private void ApplyDbInfoGridColumnLayout()
        {
            GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridDBInfo, Name);

            foreach (C1DisplayColumn displayColumn in c1GridDBInfo.Splits[0].DisplayColumns)
            {
                var dataColumn = displayColumn.DataColumn;

                if (dataColumn == null)
                {
                    continue;
                }

                var dataField = dataColumn.DataField;

                if (_hiddenDbInfoFields.Contains(dataField))
                {
                    displayColumn.Visible = false;
                    displayColumn.Frozen = true;
                }
                else if (string.Equals(dataField, _gridHeaderMap[menu.Remarks], StringComparison.OrdinalIgnoreCase))
                {
                    displayColumn.Width = 150;
                }
                else
                {
                    try
                    {
                        displayColumn.AutoSize();
                    }
                    catch (Exception)
                    {
                        displayColumn.Width = 500;
                    }

                    if (displayColumn.Width > 500)
                    {
                        displayColumn.Width = 500;
                    }
                }

                displayColumn.Style.VerticalAlignment = AlignVertEnum.Center;
            }
        }
    }
}
