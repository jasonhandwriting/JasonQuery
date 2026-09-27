using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Security;
using JasonQuery.UI.Helpers;
using System;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class JasonQueryDbPasswordDialog : Form
    {
        private const string DefaultLocalization = "English";
        private const int LabelControlSpacing = 2;

        private readonly JasonQueryDbSecurityBootstrapper _bootstrapper;
        private readonly JasonQueryDbSecurityMetadata _metadata;
        private readonly JasonQueryDbLegacySecurityMigrator _legacyMigrator;
        private readonly string _databaseFilePath;
        private readonly Func<string, bool> _storageMigrationCustomPasswordValidator;
        private readonly Func<string, bool> _currentDatabasePasswordValidator;

        private bool _isApplyingLocalization;

        public JasonQueryDbPasswordDialog(JasonQueryDbSecurityBootstrapper bootstrapper, JasonQueryDbSecurityMetadata metadata, string databaseFilePath,
                                      Func<string, bool> currentDatabasePasswordValidator)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _databaseFilePath = databaseFilePath ?? throw new ArgumentNullException(nameof(databaseFilePath));
            _currentDatabasePasswordValidator = currentDatabasePasswordValidator ?? throw new ArgumentNullException(nameof(currentDatabasePasswordValidator));

            InitializeDialog();
        }

        public JasonQueryDbPasswordDialog(JasonQueryDbLegacySecurityMigrator legacyMigrator, string databaseFilePath)
        {
            _legacyMigrator = legacyMigrator ?? throw new ArgumentNullException(nameof(legacyMigrator));
            _databaseFilePath = databaseFilePath ?? throw new ArgumentNullException(nameof(databaseFilePath));

            InitializeDialog();
        }

        public JasonQueryDbPasswordDialog(Func<string, bool> storageMigrationCustomPasswordValidator)
        {
            _storageMigrationCustomPasswordValidator = storageMigrationCustomPasswordValidator ?? throw new ArgumentNullException(nameof(storageMigrationCustomPasswordValidator));

            InitializeDialog();
        }

        public string DatabasePassword { get; private set; }

        public string CustomPassword { get; private set; }

        private void InitializeDialog()
        {
            InitializeComponent();

            btnOK.DialogResult = DialogResult.None;
            AcceptButton = btnOK;
            CancelButton = btnExit;

            btnOK.Click += btnOK_Click;
            btnExit.Click += btnExit_Click;
            txtCustomPassword.KeyDown += txtCustomPassword_KeyDown;
            cboLocalization.SelectedIndexChanged += cboLocalization_SelectedIndexChanged;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                _isApplyingLocalization = true;

                LocalizationHelper.Localization = DefaultLocalization;
                LocalizationHelper.LoadLocalizationXML();
                LocalizationHelper.ApplyLanguageInfo(this);
                ApplyMessageBoxLocalization();
                UIHelper.SetC1ComboBoxItemsFromDictionary(cboLocalization, LocalizationHelper.LocalizationMap, true);

                cboLocalization.Text = DefaultLocalization;

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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtCustomPassword.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            txtCustomPassword.Clear();

            if (DialogResult != DialogResult.OK)
            {
                DatabasePassword = null;
                CustomPassword = null;
            }

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

        private void txtCustomPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            btnOK.PerformClick();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomPassword.Text))
            {
                var message = GetFormMessage("NonePassword", "Please enter the custom password.");

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCustomPassword.Focus();

                return;
            }

            if (_storageMigrationCustomPasswordValidator != null)
            {
                ResolveStorageMigrationCustomPassword();
                return;
            }

            if (_legacyMigrator != null)
            {
                ResolveLegacyCustomPassword();
                return;
            }

            try
            {
                var result = _bootstrapper.ResolveCustomPassword(_metadata, txtCustomPassword.Text);

                if (!_currentDatabasePasswordValidator(result.DatabasePassword))
                {
                    ShowWrongPassword();
                    return;
                }

                DatabasePassword = result.DatabasePassword;
                CustomPassword = null;
                txtCustomPassword.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception)
            {
                ShowWrongPassword();
            }
        }

        private void ResolveStorageMigrationCustomPassword()
        {
            try
            {
                if (!_storageMigrationCustomPasswordValidator(txtCustomPassword.Text))
                {
                    ShowWrongPassword();
                    return;
                }

                CustomPassword = txtCustomPassword.Text;
                DatabasePassword = null;
                txtCustomPassword.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception)
            {
                ShowWrongPassword();
            }
        }

        private void ResolveLegacyCustomPassword()
        {
            try
            {
                if (!_legacyMigrator.IsCustomPasswordValid(_databaseFilePath, txtCustomPassword.Text))
                {
                    ShowWrongPassword();
                    return;
                }

                CustomPassword = txtCustomPassword.Text;
                DatabasePassword = null;
                txtCustomPassword.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception)
            {
                ShowWrongPassword();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            txtCustomPassword.Clear();
            DatabasePassword = null;
            CustomPassword = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ShowWrongPassword()
        {
            var message = GetFormMessage("WrongPassword", "The custom password is incorrect.");

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtCustomPassword.SelectAll();
            txtCustomPassword.Focus();
        }

        private void ApplyLocalizedLayout()
        {
            txtCustomPassword.Left = lblCustomPassword.Right + LabelControlSpacing;
        }

        private void ApplyVisualState()
        {
            Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
        }

        private string GetFormMessage(string id, string defaultText)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", id, "Text");
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
