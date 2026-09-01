using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class EncryptionMethodForm : Form
    {
        private int _width = 0;

        public EncryptionMethodForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

                _width = Math.Max(Math.Max(500, lblChangeToDefaultPassword.Width + 70), lblCaution0.Width + 45);
                lblOldPassword2.Location = new Point(rdoDefaultPassword.Left + rdoDefaultPassword.Width + 25, lblOldPassword2.Top);
                txtOldPassword2.Location = new Point(lblOldPassword2.Left + lblOldPassword2.Width, txtOldPassword2.Top);
                txtCustomPassword.Location = new Point(rdoCustomPassword.Left + rdoCustomPassword.Width - 2, txtCustomPassword.Top);
                btnCustomPasswordView.Location = new Point(txtCustomPassword.Left + txtCustomPassword.Width + 6, btnCustomPasswordView.Top);
                btnHelp_Password.Location = new Point(btnCustomPasswordView.Left + btnCustomPasswordView.Width + 5, btnHelp_Password.Top);
                txtOldPassword.Location = new Point(lblOldPassword.Left + lblOldPassword.Width, txtOldPassword.Top);
                txtNewPassword.Location = new Point(lblNewPassword.Left + lblNewPassword.Width, txtNewPassword.Top);
                txtConfirmNewPassword.Location = new Point(lblConfirmNewPassword.Left + lblConfirmNewPassword.Width, txtConfirmNewPassword.Top);
                lblEncryptionMothed.Location = new Point(lblTitle.Left + lblTitle.Width - 1, lblEncryptionMothed.Top);

                if (JasonQueryRepository.DbConnectionPassword == "ytec1688")
                {
                    rdoChangeCustomPassword.Enabled = false;
                    lblOldPassword.Enabled = false;
                    txtOldPassword.Enabled = false;
                    lblNewPassword.Enabled = false;
                    txtNewPassword.Enabled = false;
                    lblConfirmNewPassword.Enabled = false;
                    txtConfirmNewPassword.Enabled = false;
                    btnApply.Enabled = false;
                    rdoDefaultPassword.Enabled = false;
                    lblOldPassword2.Enabled = false;
                    txtOldPassword2.Enabled = false;
                    txtCustomPassword.Focus();
                    lblEncryptionMothed.Text = LocalizationHelper.GetLanguageString("Default Password", "form", GetType().Name, "msg", "DefaultPassword", "Text");
                    Size = new Size(_width, 280);
                }
                else
                {
                    rdoCustomPassword.Enabled = false;
                    txtCustomPassword.Text = JasonQueryRepository.DbConnectionPassword;
                    txtCustomPassword.Enabled = false;
                    btnCustomPasswordView.Visible = false;
                    txtOldPassword.Text = string.Empty;
                    txtOldPassword.Tag = JasonQueryRepository.DbConnectionPassword.Substring(JasonQueryRepository.DbConnectionPasswordPrefix.Length, JasonQueryRepository.DbConnectionPassword.Length - 27);
                    txtOldPassword.Enabled = true;
                    txtOldPassword2.Text = string.Empty;
                    txtOldPassword2.Tag = JasonQueryRepository.DbConnectionPassword.Substring(JasonQueryRepository.DbConnectionPasswordPrefix.Length, JasonQueryRepository.DbConnectionPassword.Length - 27);
                    lblChangeToDefaultPassword.Visible = true;
                    Size = new Size(_width, 355);
                    lblEncryptionMothed.Text = LocalizationHelper.GetLanguageString("Custom Password", "form", GetType().Name, "msg", "CustomPassword", "Text");
                    rdoChangeCustomPassword.Checked = true;
                    txtNewPassword.Focus();
                    btnApply.Enabled = true;
                    txtOldPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void rdoPassword_CheckedChanged(object sender, EventArgs e)
        {
            var rdo = sender as RadioButton;

            if (!rdo.Checked)
            {
                return; //避免執行兩次 (第一次會是「前一次」的選項，且為「未核取」，忽略它！)
            }

            var enabled = true;

            switch (rdo.Name)
            {
                case "rdoDefaultPassword":
                    {
                        Size = new Size(_width, 280);

                        if (JasonQueryRepository.DbConnectionPassword == "ytec1688")
                        {
                            enabled = false;
                        }
                        else
                        {
                            txtOldPassword2.Focus();
                        }

                        break;
                    }
                case "rdoCustomPassword":
                case "rdoChangeCustomPassword":
                    {
                        lblCaution0.Visible = true;
                        lblCaution1.Visible = true;
                        lblCaution2.Visible = true;
                        Size = new Size(_width, 355);

                        if (rdo.Name == nameof(rdoCustomPassword))
                        {
                            txtCustomPassword.Focus();
                        }
                        else
                        {
                            txtOldPassword.Focus();
                        }

                        break;
                    }
            }

            btnApply.Enabled = enabled;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V || e.Control && e.KeyCode == Keys.Space || e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            var textBox = sender as C1.Win.C1Input.C1TextBox;
            var length = textBox.Text.Length - 1;

            try
            {
                for (var i = length; i >= 0; i--)
                {
                    if (!TextHelper.IsEngAlphabetOrNumberOrSpecialCharacters(textBox.Text.Substring(i, 1), " "))
                    {
                        textBox.Text = textBox.Text.Replace(textBox.Text.Substring(i, 1), string.Empty);
                        textBox.SelectionStart = i;
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

        private void txtCustomPassword_Enter(object sender, EventArgs e)
        {
            rdoCustomPassword.Checked = true;
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            rdoChangeCustomPassword.Checked = true;
        }

        private void txtOldPassword2_Enter(object sender, EventArgs e)
        {
            rdoDefaultPassword.Checked = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (rdoCustomPassword.Checked && string.IsNullOrEmpty(txtCustomPassword.Text))
                {
                    var languageText = LocalizationHelper.GetLanguageString("Please enter your password.", "form", GetType().Name, "msg", "NonePassword", "Text");

                    MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCustomPassword.Focus();
                    return;
                }
                else if (rdoChangeCustomPassword.Checked)
                {
                    if (string.IsNullOrEmpty(txtOldPassword.Text))
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Please enter your old custom password.", "form", GetType().Name, "msg", "NoneOldPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtOldPassword.Focus();
                        return;
                    }
                    else if (string.IsNullOrEmpty(txtNewPassword.Text))
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Please enter your new password.", "form", GetType().Name, "msg", "NoneNewPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtNewPassword.Focus();
                        return;
                    }
                    else if (string.IsNullOrEmpty(txtConfirmNewPassword.Text))
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Please enter your confirm password.", "form", GetType().Name, "msg", "NoneConfirmNewPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtConfirmNewPassword.Focus();
                        return;
                    }
                    else if (txtNewPassword.Text != txtConfirmNewPassword.Text)
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Please make sure your passwords match.", "form", GetType().Name, "msg", "NoneMatchConfirmNewPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtNewPassword.Focus();
                        return;
                    }
                }

                if (rdoDefaultPassword.Checked && JasonQueryRepository.DbConnectionPassword == "ytec1688")
                {
                    Close();
                }
                else if (rdoDefaultPassword.Checked && JasonQueryRepository.DbConnectionPassword != "ytec1688")
                {
                    //檢查是否有輸入舊密碼
                    if (string.IsNullOrEmpty(txtOldPassword2.Text))
                    {
                        var languageText = LocalizationHelper.GetLanguageString("Please enter your old custom password.", "form", GetType().Name, "msg", "NoneOldPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtOldPassword2.Focus();
                        return;
                    }

                    //檢查舊密碼是否輸入正確
                    if (txtOldPassword2.Text != TextHelper.GetSafeString(txtOldPassword2.Tag))
                    {
                        //舊密碼不正確
                        var languageText = LocalizationHelper.GetLanguageString("The old password is incorrect. Try again.", "form", GetType().Name, "msg", "ErrorOldPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtOldPassword2.Focus();
                    }
                    else
                    {
                        //密碼改成預設值
                        var result = JasonQueryRepository.ResetDBPassword(JasonQueryRepository.DbConnectionPassword, string.Empty, true);

                        if (string.IsNullOrEmpty(result))
                        {
                            //密碼變更成功
                            var languageText = LocalizationHelper.GetLanguageString("Your password has been changed.", "form", GetType().Name, "msg", "OKPasswordChanged1", "Text");

                            MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            //密碼變更失敗
                            var languageText = LocalizationHelper.GetLanguageString("Password change failed!", "form", GetType().Name, "msg", "ErrorPasswordChanged", "Text");

                            MessageBox.Show($"{languageText}\r\n\r\n{result}", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
                else if (rdoCustomPassword.Checked)
                {
                    //密碼由預設值改成自訂密碼
                    var result = JasonQueryRepository.ResetDBPassword(JasonQueryRepository.DbConnectionPassword, txtCustomPassword.Text, false);

                    if (string.IsNullOrEmpty(result))
                    {
                        var temp1 = LocalizationHelper.GetLanguageString("Your password has been changed.", "form", GetType().Name, "msg", "OKPasswordChanged1", "Text");
                        var temp2 = LocalizationHelper.GetLanguageString("Please use the new password to start JasonQuery next time.", "form", GetType().Name, "msg", "OKPasswordChanged2", "Text");

                        //密碼變更成功
                        var languageText = $"{temp1}\r\n\r\n{temp2}";

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Close();
                    }
                    else
                    {
                        //密碼變更失敗
                        var languageText = LocalizationHelper.GetLanguageString("Password change failed!", "form", GetType().Name, "msg", "ErrorPasswordChanged", "Text");

                        MessageBox.Show($"{languageText}\r\n\r\n{result}", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else if (rdoChangeCustomPassword.Checked)
                {
                    if (txtOldPassword.Text == TextHelper.GetSafeString(txtOldPassword.Tag))
                    {
                        //變更自訂密碼
                        var result = JasonQueryRepository.ResetDBPassword(JasonQueryRepository.DbConnectionPassword, txtNewPassword.Text, false);

                        if (string.IsNullOrEmpty(result))
                        {
                            var temp1 = LocalizationHelper.GetLanguageString("Your password has been changed.", "form", GetType().Name, "msg", "OKPasswordChanged1", "Text");
                            var temp2 = LocalizationHelper.GetLanguageString("Please use the new password to start JasonQuery next time.", "form", GetType().Name, "msg", "OKPasswordChanged2", "Text");

                            //密碼變更成功
                            var languageText = $"{temp1}\r\n\r\n{temp2}";

                            MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            //密碼變更失敗
                            var languageText = LocalizationHelper.GetLanguageString("Password change failed!", "form", GetType().Name, "msg", "ErrorPasswordChanged", "Text");

                            MessageBox.Show($"{languageText}\r\n\r\n{result}", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        //舊密碼不正確
                        var languageText = LocalizationHelper.GetLanguageString("The old password is incorrect. Try again.", "form", GetType().Name, "msg", "ErrorOldPassword", "Text");

                        MessageBox.Show(languageText, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtOldPassword.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void tmrNewPassword_Tick(object sender, EventArgs e)
        {
            txtNewPassword.Focus();
            tmrNewPassword.Enabled = false;
        }

        private void btnCustomPasswordView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                txtCustomPassword.PasswordChar = '\0';
            }
        }

        private void btnCustomPasswordView_MouseUp(object sender, MouseEventArgs e)
        {
            txtCustomPassword.PasswordChar = '*';
            txtCustomPassword.Focus();
        }

        private void btnHelp_Password_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("A valid password can contain the following characters:", "form", GetType().Name, "msg", "PasswordHelp1", "Text") + "\r\n\r\n";

            message += LocalizationHelper.GetLanguageString("Lowercase characters a-z", "form", GetType().Name, "msg", "PasswordHelp2", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Uppercase characters A-Z", "form", GetType().Name, "msg", "PasswordHelp3", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Numbers 0-9", "form", GetType().Name, "msg", "PasswordHelp4", "Text") + "\r\n";
            message += LocalizationHelper.GetLanguageString("Special characters", "form", GetType().Name, "msg", "PasswordHelp5", "Text").Trim() + " `~!@#$%^&*()_-+=[{]}|\\;:'\",<.>/?";

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
