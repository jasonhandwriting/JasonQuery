using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.Database;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class DatabaseSecurityStartupErrorForm : Form
    {
        private const string DefaultLocalization = "English";

        private readonly DatabaseSecurityStartupErrorKind _errorKind;

        public DatabaseSecurityStartupErrorForm(DatabaseSecurityStartupErrorKind errorKind)
        {
            _errorKind = errorKind;
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.Localization = DefaultLocalization;
                LocalizationHelper.LoadLocalizationXML();
                LocalizationHelper.ApplyLanguageInfo(this);
                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);

                cboLocalization.Text = DefaultLocalization;

                ApplyErrorKindLayout();
                ApplyVisualState();
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
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

                ApplyErrorKindLayout();
                ApplyVisualState();
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ApplyErrorKindLayout()
        {
            var isMissingSecurityInformation = _errorKind == DatabaseSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword;
            var isWindowsCurrentUserKeyUnavailable = _errorKind == DatabaseSecurityStartupErrorKind.WindowsCurrentUserKeyUnavailable;

            lblMissingReason.Visible = isMissingSecurityInformation;
            lblMissingRecovery.Visible = isMissingSecurityInformation;
            lblGeneralReason.Visible = !isMissingSecurityInformation;
            lblGeneralRecovery.Visible = !isMissingSecurityInformation;
            lblNoMigration.Visible = isMissingSecurityInformation;

            if (isWindowsCurrentUserKeyUnavailable)
            {
                lblGeneralReason.Text = GetFormMessage
                (
                    "WindowsCurrentUserKeyUnavailableReason",
                    "The Windows-protected key for JasonQuery.db could not be unlocked. " +
                    "This can happen after moving JasonQuery to another computer, using a different Windows user profile, " +
                    "or reinstalling Windows."
                );

                lblGeneralRecovery.Text = GetFormMessage
                (
                    "WindowsCurrentUserKeyUnavailableRecovery",
                    "If the original Windows environment is still available, open JasonQuery there and switch " +
                    "JasonQuery.db Security to Custom Password before moving it. " +
                    "Always back up or move JasonQuery.db together with JasonQuery.security.json."
                );
            }
        }

        private string GetFormMessage(string id, string defaultText)
        {
            return LocalizationHelper.GetLanguageString
            (
                defaultText,
                "form",
                GetType().Name,
                "msg",
                id,
                "Text"
            );
        }

        private void ApplyVisualState()
        {
            lblTitle.ForeColor = Color.Maroon;
            lblNoMigration.ForeColor = Color.DarkGreen;
            Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
        }

        private bool CheckLocalizationFileExist(bool showAlertOnError = false)
        {
            try
            {
                var result = true;
                var xmlFileName = TextHelper.GetValueFromDictionary(LocalizationHelper.LocalizationMap, cboLocalization.Text);
                var fileName = Path.Combine(Application.StartupPath, "localization", xmlFileName);

                if (!File.Exists(fileName))
                {
                    result = false;
                }

                if (!showAlertOnError || result)
                {
                    return result;
                }

                var message = LocalizationHelper.GetLanguageString("Localization file not found!", "Global", "Global", "msg", "LocalizationNotFound", "Text");

                MessageBox.Show($"{message}\r\n{fileName}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }

            return false;
        }

        private static void ShowUnexpectedError(Exception ex)
        {
            var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
