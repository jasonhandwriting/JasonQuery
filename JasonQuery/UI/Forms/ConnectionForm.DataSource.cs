using IconLibrary;
using JasonLibrary.Core.Database.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private sealed class DataSourceComboItem
        {
            public DataSourceType DataSourceType { get; set; }
            public string DisplayText { get; set; }
            public string IconPrefix { get; set; }

            public override string ToString()
            {
                return DisplayText ?? string.Empty;
            }
        }

        private void ConfigureDataSourceComboBox()
        {
            cboDataSource.Items.Clear();

            //順序要和 _lstPicLogo / _lstSupportInfo 保持一致
            cboDataSource.Items.Add
            (
                new DataSourceComboItem
                {
                    DataSourceType = DataSourceType.Oracle,
                    DisplayText = "Oracle",
                    IconPrefix = "Oracle"
                }
            );

            cboDataSource.Items.Add
            (
                new DataSourceComboItem
                {
                    DataSourceType = DataSourceType.PostgreSql,
                    DisplayText = "PostgreSQL",
                    IconPrefix = "PostgreSQL"
                }
            );

            cboDataSource.Items.Add
            (
                new DataSourceComboItem
                {
                    DataSourceType = DataSourceType.SqlServer,
                    DisplayText = "SQL Server",
                    IconPrefix = "SQL Server"
                }
            );

            cboDataSource.Items.Add
            (
                new DataSourceComboItem
                {
                    DataSourceType = DataSourceType.MySql,
                    DisplayText = "MySQL/MariaDB",
                    IconPrefix = "MySQL"
                }
            );

            //cboDataSource.Items.Add
            //(
            //    new DataSourceComboItem
            //    {
            //        DataSourceType = DataSourceType.Sqlite,
            //        DisplayText = "SQLite",
            //        IconPrefix = "SQLite"
            //    }
            //);

            cboDataSource.SelectedIndex = -1;
            cboDataSource.Text = string.Empty;
        }

        private DataSourceComboItem GetSelectedDataSourceItem()
        {
            if (cboDataSource.SelectedIndex >= 0 && cboDataSource.SelectedIndex < cboDataSource.Items.Count)
            {
                return cboDataSource.Items[cboDataSource.SelectedIndex] as DataSourceComboItem;
            }

            foreach (var item in cboDataSource.Items)
            {
                var comboItem = item as DataSourceComboItem;

                if (comboItem != null && string.Equals(comboItem.DisplayText, cboDataSource.Text, StringComparison.Ordinal))
                {
                    return comboItem;
                }
            }

            return null;
        }

        private DataSourceType GetSelectedDataSourceType()
        {
            var item = GetSelectedDataSourceItem();

            if (item != null)
            {
                return item.DataSourceType;
            }

            return DataSourceTypeHelper.Parse(cboDataSource.Text);
        }

        private string GetSelectedDataSourceDisplayText()
        {
            var item = GetSelectedDataSourceItem();

            if (item != null)
            {
                return item.DisplayText;
            }

            return cboDataSource.Text;
        }

        private string GetSelectedDataSourceIconPrefix()
        {
            var item = GetSelectedDataSourceItem();

            if (item != null)
            {
                return item.IconPrefix;
            }

            return cboDataSource.Text.Replace("/MariaDB", string.Empty);
        }

        private bool TryGetSelectedProviderKind(out DatabaseProviderKind providerKind)
        {
            switch (GetSelectedDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        providerKind = DatabaseProviderKind.Oracle;
                        return true;
                    }
                case DataSourceType.PostgreSql:
                    {
                        providerKind = DatabaseProviderKind.PostgreSql;
                        return true;
                    }
                case DataSourceType.SqlServer:
                    {
                        providerKind = DatabaseProviderKind.SqlServer;
                        return true;
                    }
                case DataSourceType.MySql:
                    {
                        providerKind = DatabaseProviderKind.MySql;
                        return true;
                    }
                default:
                    {
                        providerKind = default(DatabaseProviderKind);
                        return false;
                    }
            }
        }

        private void SetSelectedDataSource(string storedValue)
        {
            if (string.IsNullOrWhiteSpace(storedValue))
            {
                cboDataSource.SelectedIndex = -1;
                cboDataSource.Text = string.Empty;
                return;
            }

            for (var i = 0; i < cboDataSource.Items.Count; i++)
            {
                var item = cboDataSource.Items[i] as DataSourceComboItem;

                if (item != null && string.Equals(item.DisplayText, storedValue, StringComparison.Ordinal))
                {
                    cboDataSource.SelectedIndex = i;
                    return;
                }
            }

            var dataSourceType = DataSourceTypeHelper.Parse(storedValue);

            if (dataSourceType != DataSourceType.None)
            {
                for (var i = 0; i < cboDataSource.Items.Count; i++)
                {
                    var item = cboDataSource.Items[i] as DataSourceComboItem;

                    if (item != null && item.DataSourceType == dataSourceType)
                    {
                        cboDataSource.SelectedIndex = i;
                        return;
                    }
                }
            }

            cboDataSource.SelectedIndex = -1;
            cboDataSource.Text = storedValue;
        }

        private string GetCurrentGridSelectedDataSource()
        {
            try
            {
                if (!c1GridDBInfo.HasDataTableRows())
                {
                    return string.Empty;
                }

                if (c1GridDBInfo.Row < 0 || c1GridDBInfo.Row >= c1GridDBInfo.RowCount)
                {
                    return string.Empty;
                }

                return GetCellValue(menu.DataSource, c1GridDBInfo.Row);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetDefaultDataSourceForNewConnection()
        {
            var currentDataSource = GetCurrentGridSelectedDataSource();

            if (!string.IsNullOrWhiteSpace(currentDataSource))
            {
                return currentDataSource;
            }

            if (!string.IsNullOrWhiteSpace(_lastSelectedDataSourceForNewConnection))
            {
                return _lastSelectedDataSourceForNewConnection;
            }

            return DefaultDataSourceForNewConnection;
        }

        private void SetSelectedDataSourceAndApply(string dataSourceDisplayText, bool showDefaultDataSourcePrompt)
        {
            _suppressDataSourceTextChanged = true;

            try
            {
                SetSelectedDataSource(dataSourceDisplayText);
            }
            finally
            {
                _suppressDataSourceTextChanged = false;
            }

            ApplyDataSourceChanged(showDefaultDataSourcePrompt);
        }

        private void FocusConnectionNameByDataSource(DataSourceType dataSourceType)
        {
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        txtConnectionName_Oracle.Focus();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        txtConnectionName_PostgreSQL.Focus();
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        txtConnectionName_SQLServer.Focus();
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        txtConnectionName_MySQL.Focus();
                        break;
                    }
                default:
                    {
                        cboDataSource.Focus();
                        break;
                    }
            }
        }

        private void StartNewConnectionProfile(bool showDefaultDataSourcePrompt)
        {
            foreach (var t in _lstPicLogo)
            {
                t.Visible = false;
            }

            var defaultDataSource = GetDefaultDataSourceForNewConnection();

            lblPID.Text = string.Empty;
            lblDomainUser.Text = MyGlobal.DomainUser;
            cboDataSource.Enabled = true;
            chkSavePasswords.Checked = true; //20230915 新建立的連線，預設值改為 true
            chkShowColumnInfo.Checked = true; //20230915 新建立的連線，預設值改為 true
            chkDarkMode.Checked = false; //20230915 新建立的連線，一律是一般模式
            txtSupportInfo.Text = string.Empty;
            btnDelete.Enabled = false;
            btnCopy.Enabled = false;

            var rng = new Random(Guid.NewGuid().GetHashCode());
            var i = rng.Next(0, 8);

            switch (i)
            {
                case 0:
                    {
                        pnlBackColor.BackColor = Color.YellowGreen;
                        break;
                    }
                case 1:
                    {
                        pnlBackColor.BackColor = Color.LightBlue;
                        break;
                    }
                case 2:
                    {
                        pnlBackColor.BackColor = Color.LightPink;
                        break;
                    }
                case 3:
                    {
                        pnlBackColor.BackColor = Color.LightSeaGreen;
                        break;
                    }
                case 4:
                    {
                        pnlBackColor.BackColor = Color.LightSalmon;
                        break;
                    }
                case 5:
                    {
                        pnlBackColor.BackColor = Color.Plum;
                        break;
                    }
                case 6:
                    {
                        pnlBackColor.BackColor = Color.Orange;
                        break;
                    }
                default:
                    {
                        pnlBackColor.BackColor = Color.Gold;
                        break;
                    }
            }

            pnlBackColor.Tag = ColorTranslator.ToHtml(Color.FromArgb(pnlBackColor.BackColor.ToArgb()));
            pnlActiveForeColor.BackColor = Color.Black;
            pnlActiveForeColor.Tag = ColorTranslator.ToHtml(Color.FromArgb(pnlActiveForeColor.BackColor.ToArgb()));
            pnlInactiveForeColor.BackColor = ColorTranslator.FromHtml("#7F7F7F");
            pnlInactiveForeColor.Tag = ColorTranslator.ToHtml(Color.FromArgb(pnlInactiveForeColor.BackColor.ToArgb()));

            _toolTip1.SetToolTip(pnlBackColor, $"{pnlBackColor.Tag} (R:{pnlBackColor.BackColor.R}, G:{pnlBackColor.BackColor.G}, B:{pnlBackColor.BackColor.B})");
            _toolTip1.SetToolTip(pnlActiveForeColor, $"{pnlActiveForeColor.Tag} (R:{pnlActiveForeColor.BackColor.R}, G:{pnlActiveForeColor.BackColor.G}, B:{pnlActiveForeColor.BackColor.B})");
            _toolTip1.SetToolTip(pnlInactiveForeColor, $"{pnlInactiveForeColor.Tag} (R:{pnlInactiveForeColor.BackColor.R}, G:{pnlInactiveForeColor.BackColor.G}, B:{pnlInactiveForeColor.BackColor.B})");

            tabExample.BackColor = pnlBackColor.BackColor;
            tabExample.ForeColor = pnlActiveForeColor.BackColor;
            tabExample.TextInactiveColor = pnlInactiveForeColor.BackColor;

            pnlPostgreSQL.Visible = false;
            pnlOracle.Visible = false;
            pnlSQLServer.Visible = false;
            pnlMySQL.Visible = false;
            pnlSQLite.Visible = false;
            picDatabase.Visible = false;
            grpMainFormIconStyle.Enabled = false;

            SetSelectedDataSourceAndApply(defaultDataSource, showDefaultDataSourcePrompt);
            _lastSelectedDataSourceForNewConnection = GetSelectedDataSourceDisplayText();

            FocusConnectionNameByDataSource(GetSelectedDataSourceType());
        }

        private void cboDataSource_TextChanged(object sender, EventArgs e)
        {
            if (_suppressDataSourceTextChanged)
            {
                return;
            }

            try
            {
                ApplyDataSourceChanged(false);

                if (GetSelectedDataSourceType() != DataSourceType.None)
                {
                    _lastSelectedDataSourceForNewConnection = GetSelectedDataSourceDisplayText();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ApplyDataSourceChanged(bool showDefaultDataSourcePrompt)
        {
            var dataSourceType = GetSelectedDataSourceType();
            var displayText = GetSelectedDataSourceDisplayText();
            var iconPrefix = GetSelectedDataSourceIconPrefix();

            pnlOracle.Visible = dataSourceType == DataSourceType.Oracle;
            pnlPostgreSQL.Visible = dataSourceType == DataSourceType.PostgreSql;
            pnlSQLServer.Visible = dataSourceType == DataSourceType.SqlServer;
            pnlMySQL.Visible = dataSourceType == DataSourceType.MySql;
            pnlSQLite.Visible = false;

            btnTest.Enabled = true;
            btnSave.Enabled = true;
            picDatabase.Visible = true;

            if (dataSourceType == DataSourceType.None && string.IsNullOrEmpty(displayText))
            {
                return;
            }

            txtSupportInfo.Text = string.Empty;

            //20240209 根據資料庫顯示不同的圖示
            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        InitialDataSource_Oracle();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        InitialDataSource_PostgreSql();
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        InitialDataSource_SqlServer();
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        InitialDataSource_MySql();
                        break;
                    }
            }

            for (var i = 0; i <= 9; i++)
            {
                var iconName = $"{iconPrefix} {i} 16x16.ico";

                _lstPicIconStyle[i].Image = IconManager.GetIcon16x16(MyGlobal.IconLibrary, iconName).ToBitmap(); //20250417 此處必須使用 GetIcon16x16 讀取 16x16 的 icon 內容，並轉為 Bitmap，否則會引發 GDI+ 的錯誤
            }

            grpMainFormIconStyle.Enabled = true;
            lblPrompt.Visible = showDefaultDataSourcePrompt;
        }

        private void ApplyInitialDataSourceUi(Control connectionNameControl)
        {
            foreach (var t in _lstPicLogo)
            {
                t.Visible = false;
            }

            if (cboDataSource.SelectedIndex >= 0 && cboDataSource.SelectedIndex < _lstPicLogo.Count && cboDataSource.SelectedIndex < _lstSupportInfo.Count)
            {
                _lstPicLogo[cboDataSource.SelectedIndex].Visible = true;
                txtSupportInfo.Text = _lstSupportInfo[cboDataSource.SelectedIndex];
            }

            cboDataSource.Focus();
            connectionNameControl?.Focus();
        }

        private void InitialDataSource_Oracle()
        {
            txtConnectionName_Oracle.Text = string.Empty;
            txtServer_Oracle.Text = string.Empty;
            txtPort_Oracle.Text = @"1521";
            txtRemark_Oracle.Text = string.Empty;
            txtUserID_Oracle.Text = string.Empty;
            txtPassword_Oracle.Text = string.Empty;
            cboConnectAs_Oracle.SelectedIndex = 0;
            chkDirectMode_Oracle.Checked = false;
            lblSID_Oracle.Visible = chkDirectMode_Oracle.Checked;
            txtSID_Oracle.Visible = true;
            txtSID_Oracle.Text = string.Empty;
            txtSID_Oracle.Visible = chkDirectMode_Oracle.Checked;
            btnHelp_SID_Oracle.Visible = chkDirectMode_Oracle.Checked;
            chkUnicode_Oracle.Checked = true;
            chkPooling_Oracle.Checked = false;

            ApplyInitialDataSourceUi(txtConnectionName_Oracle);
        }

        private void InitialDataSource_PostgreSql()
        {
            txtConnectionName_PostgreSQL.Text = string.Empty;
            txtServer_PostgreSQL.Text = string.Empty;
            cboDatabase_PostgreSQL.Items.Clear();
            cboDatabase_PostgreSQL.Text = string.Empty;
            txtPort_PostgreSQL.Text = @"5432";
            txtRemark_PostgreSQL.Text = string.Empty;
            txtUserID_PostgreSQL.Text = string.Empty;
            txtPassword_PostgreSQL.Text = string.Empty;
            chkUnicode_PostgreSQL.Checked = true;
            chkAutoRollback_PostgreSQL.Checked = true;
            chkPooling_PostgreSQL.Checked = false;

            ApplyInitialDataSourceUi(txtConnectionName_PostgreSQL);
        }

        private void InitialDataSource_SqlServer()
        {
            txtConnectionName_SQLServer.Text = string.Empty;
            txtServer_SQLServer.Text = string.Empty;
            cboDatabase_SQLServer.Items.Clear();
            cboDatabase_SQLServer.Text = string.Empty;
            txtRemark_SQLServer.Text = string.Empty;
            txtUserID_SQLServer.Text = string.Empty;
            txtPassword_SQLServer.Text = string.Empty;
            txtPort_SQLServer.Text = @"1433";
            chkPooling_SQLServer.Checked = false;
            chkExcludeNativeObject_SQLServer.Checked = true;

            ApplyInitialDataSourceUi(txtConnectionName_SQLServer);
        }

        private void InitialDataSource_MySql()
        {
            txtConnectionName_MySQL.Text = string.Empty;
            txtServer_MySQL.Text = string.Empty;
            cboDatabase_MySQL.Items.Clear();
            cboDatabase_MySQL.Text = string.Empty;
            txtRemark_MySQL.Text = string.Empty;
            txtUserID_MySQL.Text = string.Empty;
            txtPassword_MySQL.Text = string.Empty;
            txtPort_MySQL.Text = @"3306";
            chkPooling_MySQL.Checked = false;
            chkUnicode_MySQL.Checked = true;

            ApplyInitialDataSourceUi(txtConnectionName_MySQL);
        }
    }
}
