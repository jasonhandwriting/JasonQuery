using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void ApplyLocalizationSetting(bool reloadDbInfoGrid = true)
        {
            LocalizationHelper.ApplyLanguageInfo(this);

            _languageText = LocalizationHelper.GetLanguageString("Create a desktop shortcut", "form", GetType().Name, "object", "btnCreateShortcut", "ToolTipText");
            _toolTip1.SetToolTip(btnCreateShortcut, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Export password encrypted all connection information", "form", GetType().Name, "object", "btnExport", "ToolTipText");
            _toolTip1.SetToolTip(btnExport, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Import all connection information encrypted with password", "form", GetType().Name, "object", "btnImport", "ToolTipText");
            _toolTip1.SetToolTip(btnImport, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("You can copy all the settings, including AutoReplace, from [Tools] > [Options] > [Copy Settings...].", "form", GetType().Name, "object", "btnCopy", "ToolTipText");
            _toolTip1.SetToolTip(btnCopy, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Browse existing database file on local computer", "form", GetType().Name, "object", "btnBrowse", "ToolTipText");
            _toolTip1.SetToolTip(btnBrowseFile_SQLite, _languageText);
            _languageText = LocalizationHelper.GetLanguageString("Auto rollback when SQL execution fails", "form", GetType().Name, "object", "chkAutoRollback_PostgreSQL", "ToolTipText");
            _toolTip1.SetToolTip(chkAutoRollback_PostgreSQL, _languageText);

            chkPooling_SQLServer.Location = new Point(btnHelp_Pooling_SQLServer.Left - chkPooling_SQLServer.Width, chkPooling_SQLServer.Top);
            chkExcludeNativeObject_SQLServer.Location = new Point(btnHelp_ExcludeNativeDatabase_SQLServer.Left - chkExcludeNativeObject_SQLServer.Width, chkExcludeNativeObject_SQLServer.Top);

            chkPooling_PostgreSQL.Location = new Point(btnHelp_Pooling_PostgreSQL.Left - chkPooling_PostgreSQL.Width, chkPooling_PostgreSQL.Top);
            chkUnicode_PostgreSQL.Location = new Point(btnHelp_Unicode_PostgreSQL.Left - chkUnicode_PostgreSQL.Width, chkUnicode_PostgreSQL.Top);
            chkAutoRollback_PostgreSQL.Location = new Point(btnHelp_AutoRollback_PostgreSQL.Left - chkAutoRollback_PostgreSQL.Width, chkAutoRollback_PostgreSQL.Top);

            chkPooling_MySQL.Location = new Point(btnHelp_Pooling_MySQL.Left - chkPooling_MySQL.Width, chkPooling_MySQL.Top);
            chkUnicode_MySQL.Location = new Point(btnHelp_Unicode_MySQL.Left - chkUnicode_MySQL.Width, chkUnicode_MySQL.Top);

            chkUnicode_Oracle.Location = new Point(btnHelp_DirectMode.Left - chkUnicode_Oracle.Width, chkUnicode_Oracle.Top);
            chkDirectMode_Oracle.Location = new Point(btnHelp_DirectMode.Left - chkDirectMode_Oracle.Width, chkDirectMode_Oracle.Top);
            chkPooling_Oracle.Location = new Point(btnHelp_Pooling_Oracle.Left - chkPooling_Oracle.Width, chkPooling_Oracle.Top);

            btnHelp_PasswordEncrypt.Location = new Point(chkSavePasswords.Left + chkSavePasswords.Width - 3, btnHelp_PasswordEncrypt.Top);
            chkDarkMode.Location = new Point(btnHelp_PasswordEncrypt.Left + btnHelp_PasswordEncrypt.Width + 18, chkDarkMode.Top);
            btnHelp_DarkMode.Location = new Point(chkDarkMode.Left + chkDarkMode.Width - 3, btnHelp_DarkMode.Top);

            chkLog.Checked = AppConfigHelper.HasGenerateLogFile;
            btnHelp_Log.Location = new Point(chkLog.Left + chkLog.Width - 3, btnHelp_Log.Top);
            chkLog.Enabled = !AppConfigHelper.HasGenerateLogFile; //20250708 如果是從批次檔呼叫 JasonQuery，開啟 Log Mode 並鎖定 chkLog

            btnHelp_QueryTimeout_Oracle.Location = new Point(lblSeconds_Oracle.Left + lblSeconds_Oracle.Width, btnHelp_QueryTimeout_Oracle.Top);
            btnHelp_QueryTimeout_PostgreSQL.Location = new Point(lblSeconds_PostgreSQL.Left + lblSeconds_PostgreSQL.Width, btnHelp_QueryTimeout_PostgreSQL.Top);
            btnHelp_QueryTimeout_SQLServer.Location = new Point(lblSeconds_SQLServer.Left + lblSeconds_SQLServer.Width, btnHelp_QueryTimeout_SQLServer.Top);
            btnHelp_QueryTimeout_MySQL.Location = new Point(lblSeconds_MySQL.Left + lblSeconds_MySQL.Width, btnHelp_QueryTimeout_MySQL.Top);

            c1GridDBInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;

            if (reloadDbInfoGrid)
            {
                ReloadDbInfoGrid();
            }
            else
            {
                RefreshDbInfoGridCaptions();
            }

            pnlBackColor.Location = new Point(lblBackColor.Left + lblBackColor.Width + 3, pnlBackColor.Top);
            pnlActiveForeColor.Location = new Point(lblActiveForeColor.Left + lblActiveForeColor.Width + 3, pnlActiveForeColor.Top);
            pnlInactiveForeColor.Location = new Point(lblInactiveForeColor.Left + lblInactiveForeColor.Width + 3, pnlInactiveForeColor.Top);

            lblMainFormIconStyle.Text = grpMainFormIconStyle.Text;
            grpMainFormIconStyle.Text += @"    ";
            btnHelp_MainFormIconStyle.Location = new Point(lblMainFormIconStyle.Left + lblMainFormIconStyle.Width + 1, btnHelp_MainFormIconStyle.Top);

            //20250409 變更語系時，同時變更它的顏色
            lblPrompt.ForeColor = Color.FromArgb(192, 0, 0); //暗紅色

            Refresh();
        }

        private void cboLocalization_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LocalizationHelper.Localization == cboLocalization.Text || !CheckLocalizationFileExist(true))
                {
                    return;
                }

                JasonQueryRepository.UpdateSetting("GlobalConfig", "Localization", cboLocalization.Text);

                LocalizationHelper.Localization = cboLocalization.Text;
                LocalizationHelper.LoadLocalizationXML();
                ApplyLocalizationSetting(false);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool CheckLocalizationFileExist(bool showAlertOnError = false)
        {
            var result = true;
            var temp = TextHelper.GetValueFromDictionary(LocalizationHelper.LocalizationMap, cboLocalization.Text);
            var fileName = $@"{Application.StartupPath}\localization\{temp}";

            if (!File.Exists(fileName))
            {
                result = false;
            }

            if (!showAlertOnError || result)
            {
                return result;
            }

            var message = LocalizationHelper.GetLanguageString("Localization file not found!", "Global", "Global", "msg", "LocalizationNotFound", "Text");

            MessageBox.Show($"{message}\r\n\r\n{fileName}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }
    }
}