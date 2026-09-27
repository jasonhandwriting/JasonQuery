using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Internal.Security;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class JasonQueryDbSecurityForm : Form
    {
        private const string DefaultLocalization = "English";
        private const int LabelControlSpacing = 2;

        private readonly string _databaseFilePath;
        private readonly JasonQueryDbSecurityMetadataStore _metadataStore;
        private readonly JasonQueryDbDpapiKeyProtector _databaseKeyProtector;
        private readonly JasonQueryDbSecurityTransitionManager _transitionManager;
        private readonly JasonQueryDbRecoveryEnrollmentManager _recoveryEnrollmentManager;

        private JasonQueryDbSecurityMetadata _currentMetadata;
        private bool _isApplyingLocalization;

        public JasonQueryDbSecurityForm()
        {
            InitializeComponent();

            _databaseFilePath = Path.GetFullPath(JasonQueryRepository.DbFileName);

            var metadataFilePath = Path.Combine(Path.GetDirectoryName(_databaseFilePath), JasonQueryDbSecurityConstants.MetadataFileName);

            _metadataStore = new JasonQueryDbSecurityMetadataStore(metadataFilePath);
            _databaseKeyProtector = new JasonQueryDbDpapiKeyProtector();

            var journalStore = new JasonQueryDbSecurityTransitionJournalStore
            (
                JasonQueryDbSecurityTransitionManager.GetJournalFilePath(_databaseFilePath)
            );

            _transitionManager = new JasonQueryDbSecurityTransitionManager
            (
                _metadataStore,
                _databaseKeyProtector,
                metadata => StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create(metadata),
                journalStore
            );

            _recoveryEnrollmentManager = new JasonQueryDbRecoveryEnrollmentManager
            (
                _metadataStore,
                _databaseKeyProtector,
                metadata => StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create(metadata)
            );

            btnApply.DialogResult = DialogResult.None;
            CancelButton = btnClose;

            rdoWindowsProtected.CheckedChanged += ProtectionRadio_CheckedChanged;
            rdoCustomPassword.CheckedChanged += ProtectionRadio_CheckedChanged;
            txtCurrentPassword.TextChanged += PasswordTextChanged;
            txtNewCustomPassword.TextChanged += PasswordTextChanged;
            txtConfirmPassword.TextChanged += PasswordTextChanged;
            btnApply.Click += btnApply_Click;
            btnClose.Click += btnClose_Click;
            cboLocalization.SelectedIndexChanged += cboLocalization_SelectedIndexChanged;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                _isApplyingLocalization = true;

                var initialLocalization = string.IsNullOrWhiteSpace(LocalizationHelper.Localization) ? DefaultLocalization : LocalizationHelper.Localization;

                LocalizationHelper.Localization = initialLocalization;
                LocalizationHelper.LoadLocalizationXML();
                LocalizationHelper.ApplyLanguageInfo(this);
                ApplyMessageBoxLocalization();
                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);

                cboLocalization.Text = initialLocalization;

                LoadCurrentState();
                ApplyLocalizedLayout();
                ApplyVisualState();
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
                Close();
            }
            finally
            {
                _isApplyingLocalization = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ClearPasswordInputs();
            base.OnFormClosed(e);
        }

        private void cboLocalization_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isApplyingLocalization || string.IsNullOrWhiteSpace(cboLocalization.Text))
            {
                return;
            }

            try
            {
                if (LocalizationHelper.Localization == cboLocalization.Text || !CheckLocalizationFileExist(true))
                {
                    return;
                }

                _isApplyingLocalization = true;

                LocalizationHelper.Localization = cboLocalization.Text;
                LocalizationHelper.LoadLocalizationXML();
                LocalizationHelper.ApplyLanguageInfo(this);
                ApplyMessageBoxLocalization();

                ApplyCurrentProtectionText();
                UpdateInputState();
                ApplyLocalizedLayout();
                ApplyVisualState();
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }
            finally
            {
                _isApplyingLocalization = false;
            }
        }

        private void LoadCurrentState()
        {
            if (!_metadataStore.Exists)
            {
                throw new InvalidDataException(GetFormMessage("MetadataMissing", "Database security metadata is missing."));
            }

            _currentMetadata = _metadataStore.Load();

            if (_currentMetadata.Mode == JasonQueryDbSecurityMode.WindowsCurrentUser)
            {
                rdoWindowsProtected.Checked = true;
            }
            else if (_currentMetadata.Mode == JasonQueryDbSecurityMode.CustomPassword)
            {
                rdoCustomPassword.Checked = true;
            }
            else
            {
                throw new InvalidDataException(GetFormMessage("UnsupportedMode", "The current database security mode is not supported."));
            }

            ApplyCurrentProtectionText();
            UpdateInputState();
        }

        private void ApplyCurrentProtectionText()
        {
            if (_currentMetadata == null)
            {
                lblCurrentProtectionValue.Text = string.Empty;
                return;
            }

            if (_currentMetadata.Mode == JasonQueryDbSecurityMode.WindowsCurrentUser)
            {
                lblCurrentProtectionValue.Text = GetFormMessage("ProtectionWindows", "Windows Protected");
                return;
            }

            if (_currentMetadata.Mode == JasonQueryDbSecurityMode.CustomPassword)
            {
                lblCurrentProtectionValue.Text = GetFormMessage("ProtectionCustom", "Custom Password");
                return;
            }

            lblCurrentProtectionValue.Text = string.Empty;
        }

        private void ProtectionRadio_CheckedChanged(object sender, EventArgs e)
        {
            var radio = sender as RadioButton;

            if (radio != null && !radio.Checked)
            {
                return;
            }

            UpdateInputState();
        }

        private void PasswordTextChanged(object sender, EventArgs e)
        {
            UpdateApplyButton();
        }

        private void UpdateInputState()
        {
            var currentIsCustom = _currentMetadata != null && _currentMetadata.Mode == JasonQueryDbSecurityMode.CustomPassword;
            var targetIsCustom = rdoCustomPassword.Checked;

            lblCurrentPassword.Visible = currentIsCustom;
            txtCurrentPassword.Visible = currentIsCustom;

            lblNewCustomPassword.Visible = targetIsCustom;
            txtNewCustomPassword.Visible = targetIsCustom;
            lblConfirmPassword.Visible = targetIsCustom;
            txtConfirmPassword.Visible = targetIsCustom;

            if (targetIsCustom)
            {
                lblInfo1.Text = GetFormObjectText
                (
                    "lblInfo1",
                    "Custom passwords cannot be recovered by JasonQuery."
                );

                lblInfo2.Text = GetFormObjectText
                (
                    "lblInfo2",
                    "Enter the password at startup. Back up JasonQuery.db and JasonQuery.security.json together."
                );

                lblInfo1.Visible = true;
                lblInfo2.Visible = true;
            }
            else
            {
                lblInfo1.Text = GetFormMessage
                (
                    "WindowsProtectionInfo",
                    "Windows Protected is tied to your current Windows user profile. " +
                    "Create and securely store a Recovery Key so JasonQuery.db can be recovered after moving computers, " +
                    "changing Windows user profiles, or reinstalling Windows."
                );

                lblInfo1.Visible = true;
                lblInfo2.Visible = false;
            }

            UpdateRecoveryUiState();
            ApplyLocalizedLayout();
            UpdateApplyButton();
        }

        private void UpdateApplyButton()
        {
            if (_currentMetadata == null)
            {
                btnApply.Enabled = false;
                return;
            }

            var currentIsCustom = _currentMetadata.Mode == JasonQueryDbSecurityMode.CustomPassword;
            var targetIsCustom = rdoCustomPassword.Checked;

            if (currentIsCustom && string.IsNullOrEmpty(txtCurrentPassword.Text))
            {
                btnApply.Enabled = false;
                return;
            }

            if (targetIsCustom)
            {
                btnApply.Enabled = !string.IsNullOrEmpty(txtNewCustomPassword.Text) && string.Equals(txtNewCustomPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal);
                return;
            }

            btnApply.Enabled = currentIsCustom;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            btnApply.Enabled = false;

            try
            {
                var currentMetadata = _metadataStore.Load();
                var currentDatabasePassword = ResolveAndVerifyCurrentDatabasePassword(currentMetadata);

                JasonQueryDbSecurityBootstrapResult result;

                if (rdoCustomPassword.Checked)
                {
                    if (string.IsNullOrEmpty(txtNewCustomPassword.Text))
                    {
                        var message = GetFormMessage("NewPasswordRequired", "Please enter a new custom password.");

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtNewCustomPassword.Focus();
                        return;
                    }

                    if (!string.Equals(txtNewCustomPassword.Text, txtConfirmPassword.Text, StringComparison.Ordinal))
                    {
                        var message = GetFormMessage("PasswordMismatch", "The new password and confirmation do not match.");

                        MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtConfirmPassword.Focus();
                        return;
                    }

                    var confirmation = MessageBox.Show
                    (
                        GetFormMessage
                        (
                            "ConfirmCustomPassword",
                            "JasonQuery cannot recover a custom password.\r\n\r\n" +
                            "You will need this password every time JasonQuery starts.\r\n\r\n" +
                            "Back up or move JasonQuery.db together with JasonQuery.security.json.\r\n\r\n" +
                            "Continue?"
                        ),
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2
                    );

                    if (confirmation != DialogResult.Yes)
                    {
                        return;
                    }

                    JasonQueryRepository.ReleaseRuntimeDatabaseHandleForSecurityTransition();

                    result = _transitionManager.ChangeToCustomPassword
                    (
                        _databaseFilePath,
                        currentDatabasePassword,
                        txtNewCustomPassword.Text
                    );
                }
                else
                {
                    var confirmation = MessageBox.Show
                    (
                        GetFormMessage
                        (
                            "ConfirmWindowsProtected",
                            "Protect JasonQuery.db for the current Windows user?\r\n\r\n" +
                            "Windows Protected is tied to the current Windows user profile. " +
                            "After this change, create and securely store a Recovery Key from this window so the database can be recovered " +
                            "after moving computers, changing Windows user profiles, or reinstalling Windows.\r\n\r\n" +
                            "The custom password will no longer be required after this change."
                        ),
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2
                    );

                    if (confirmation != DialogResult.Yes)
                    {
                        return;
                    }

                    JasonQueryRepository.ReleaseRuntimeDatabaseHandleForSecurityTransition();

                    result = _transitionManager.ChangeToWindowsCurrentUser
                    (
                        _databaseFilePath,
                        currentDatabasePassword
                    );
                }

                ApplyRuntimeResult(result);
                ClearPasswordInputs();
                LoadCurrentState();

                var successMessage = GetFormMessage("UpdateSuccess", "JasonQuery.db security was updated successfully.");

                MessageBox.Show(successMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var updateFailed = GetFormMessage("UpdateFailed", "JasonQuery.db security could not be updated.");
                var message = string.IsNullOrWhiteSpace(ex.Message) ? updateFailed : $"{updateFailed}\r\n\r\n{ex.Message}";

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ClearPasswordInputs();
                UpdateApplyButton();
            }
        }

        private string ResolveAndVerifyCurrentDatabasePassword(JasonQueryDbSecurityMetadata metadata)
        {
            string databasePassword;

            if (metadata.Mode == JasonQueryDbSecurityMode.WindowsCurrentUser)
            {
                databasePassword = JasonQueryRepository.DbConnectionPassword;
            }
            else if (metadata.Mode == JasonQueryDbSecurityMode.CustomPassword)
            {
                if (string.IsNullOrEmpty(txtCurrentPassword.Text))
                {
                    throw new InvalidOperationException(GetFormMessage("EnterCurrentPassword", "Enter the current custom password."));
                }

                var bootstrapper = new JasonQueryDbSecurityBootstrapper(_metadataStore, _databaseKeyProtector);
                var resolved = bootstrapper.ResolveCustomPassword(metadata, txtCurrentPassword.Text);

                databasePassword = resolved.DatabasePassword;
            }
            else
            {
                throw new InvalidOperationException(GetFormMessage("UnsupportedMode", "The current database security mode is not supported."));
            }

            var database = StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create(metadata);

            if (!database.CanOpen(_databaseFilePath, databasePassword))
            {
                throw new InvalidOperationException(GetFormMessage("CurrentPasswordIncorrect", "The current password is incorrect."));
            }

            return databasePassword;
        }

        private void ApplyRuntimeResult(JasonQueryDbSecurityBootstrapResult result)
        {
            JasonQueryRepository.DbConnectionPassword = result.DatabasePassword;

            if (!JasonQueryRepository.CheckCurrentDatabasePassword())
            {
                throw new InvalidDataException
                (
                    GetFormMessage
                    (
                        "ReopenFailed",
                        "JasonQuery.db could not be reopened after updating database security."
                    )
                );
            }

            JasonQueryDbSecurityRuntime.SetV2(result.Metadata.Mode);
        }

        private void UpdateRecoveryUiState()
        {
            var available = _currentMetadata != null && _currentMetadata.Mode == JasonQueryDbSecurityMode.WindowsCurrentUser && rdoWindowsProtected.Checked;

            lblRecoveryStatus.Visible = available;
            btnManageRecoveryKey.Visible = available;

            if (!available)
            {
                return;
            }

            var configured = _currentMetadata.Recovery != null;

            lblRecoveryStatus.Text = configured ? GetFormMessage("RecoveryConfigured", "Recovery Key: Configured") : GetFormMessage("RecoveryNotConfigured", "Recovery Key: Not configured");

            btnManageRecoveryKey.Text = GetFormMessage
            (
                "ManageRecoveryKey",
                "Manage Recovery Key..."
            );
        }

        private void btnManageRecoveryKey_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new JasonQueryDbRecoveryKeyForm(_recoveryEnrollmentManager, _databaseFilePath))
                {
                    form.ShowDialog(this);
                }

                LoadCurrentState();
            }
            catch (Exception ex)
            {
                var prefix = GetFormMessage("RecoveryManagementFailed", "Recovery Key management could not be opened.");
                var message = string.IsNullOrWhiteSpace(ex.Message) ? prefix : prefix + "\r\n\r\n" + ex.Message;

                MessageBox.Show
                (
                    message,
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ClearPasswordInputs()
        {
            txtCurrentPassword.Clear();
            txtNewCustomPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ClearPasswordInputs();
            Close();
        }

        private void ApplyLocalizedLayout()
        {
            lblCurrentProtectionValue.Left = lblCurrentProtection.Right + LabelControlSpacing;
            txtCurrentPassword.Left = lblCurrentPassword.Right + LabelControlSpacing;
            txtNewCustomPassword.Left = lblNewCustomPassword.Right + LabelControlSpacing;
            txtConfirmPassword.Left = lblConfirmPassword.Right + LabelControlSpacing;
        }

        private void ApplyVisualState()
        {
            lblCurrentProtectionValue.ForeColor = Color.DarkBlue;
            lblInfo1.ForeColor = Color.FromArgb(192, 0, 0);
            lblInfo2.ForeColor = Color.FromArgb(192, 0, 0);
            Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
        }

        private string GetFormMessage(string id, string defaultText)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", id, "Text");
        }

        private string GetFormObjectText(string id, string defaultText)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "object", id, "Text");
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

        private static void ApplyMessageBoxLocalization()
        {
            MessageBoxManager.Unregister();

            MessageBoxManager.OK = LocalizationHelper.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");
            MessageBoxManager.Cancel = LocalizationHelper.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");
            MessageBoxManager.Abort = LocalizationHelper.GetLanguageString("&Abort", "Global", "Global", "messagebox", "Abort", "Text");
            MessageBoxManager.Retry = LocalizationHelper.GetLanguageString("&Retry", "Global", "Global", "messagebox", "Retry", "Text");
            MessageBoxManager.Ignore = LocalizationHelper.GetLanguageString("&Ignore", "Global", "Global", "messagebox", "Ignore", "Text");
            MessageBoxManager.Yes = LocalizationHelper.GetLanguageString("&Yes", "Global", "Global", "messagebox", "Yes", "Text");
            MessageBoxManager.No = LocalizationHelper.GetLanguageString("&No", "Global", "Global", "messagebox", "No", "Text");
            MessageBoxManager.CreateNewFolder = LocalizationHelper.GetLanguageString("&Make New Folder", "Global", "Global", "messagebox", "MakeNewFolder", "Text");
            MessageBoxManager.FolderBrowserDialogTitle = LocalizationHelper.GetLanguageString("Browse For Folder", "Global", "Global", "messagebox", "FolderBrowserDialogTitle", "Text");

            MessageBoxManager.Register();
        }

        private static void ShowUnexpectedError(Exception ex)
        {
            var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
