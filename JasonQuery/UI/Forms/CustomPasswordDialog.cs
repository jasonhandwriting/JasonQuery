using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class CustomPasswordDialog : Form
    {
        public CustomPasswordDialog()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this);
                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);

                cboLocalization.Text = LocalizationHelper.Localization;
                cboLocalization.Tag = LocalizationHelper.Localization;
                lblInfo.ForeColor = Color.Maroon;
                Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtEncryptPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V || e.Control && e.KeyCode == Keys.Space || e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(txtEncryptPassword.Text))
            {
                btnOK.PerformClick();
            }
        }

        private void txtEncryptPassword_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var count = txtEncryptPassword.Text.Length - 1;

                for (var i = count; i >= 0; i--)
                {
                    if (!TextHelper.IsEngAlphabetOrNumberOrSpecialCharacters(txtEncryptPassword.Text.Substring(i, 1), " "))
                    {
                        txtEncryptPassword.Text = txtEncryptPassword.Text.Replace(txtEncryptPassword.Text.Substring(i, 1), string.Empty);
                        txtEncryptPassword.SelectionStart = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtEncryptPassword.Text))
                {
                    var message = LocalizationHelper.GetLanguageString("Please enter password.", "form", GetType().Name, "msg", "NonePassword", "Text");

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtEncryptPassword.Focus();
                    return;
                }

                if (JasonQueryRepository.CheckDBPassword(txtEncryptPassword.Text))
                {
                    JasonQueryRepository.DbConnectionPassword = $"{JasonQueryRepository.DbConnectionPasswordPrefix}{txtEncryptPassword.Text}{JasonQueryRepository.DbConnectionPasswordSuffix}";
                    Close();
                }
                else
                {
                    var message = LocalizationHelper.GetLanguageString("Wrong password!", "form", GetType().Name, "msg", "WrongPassword", "Text") + "\r\n";

                    message += LocalizationHelper.GetLanguageString("You must re-enter your password.", "form", GetType().Name, "msg", "ReEnter", "Text");

                    MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtEncryptPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboLocalization_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (LocalizationHelper.Localization == cboLocalization.Text || !CheckLocalizationFileExist(true))
                {
                    return;
                }

                LocalizationHelper.Localization = cboLocalization.Text;
                LocalizationHelper.LoadLocalizationXML();
                LocalizationHelper.ApplyLanguageInfo(this);

                lblInfo.ForeColor = Color.Maroon;
                Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool CheckLocalizationFileExist(bool showAlertOnError = false)
        {
            try
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
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return false;
        }

        private void btnEncryptPasswordView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                txtEncryptPassword.PasswordChar = '\0';
            }
        }

        private void btnEncryptPasswordView_MouseUp(object sender, MouseEventArgs e)
        {
            txtEncryptPassword.PasswordChar = '*';
            txtEncryptPassword.Focus();
        }
    }
}
