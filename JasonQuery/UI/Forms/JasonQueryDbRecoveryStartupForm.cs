using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class JasonQueryDbRecoveryStartupForm : Form
    {
        private const string DefaultLocalization = "English";

        private readonly JasonQueryDbRecoveryStartupManager _recoveryManager;
        private readonly string _databaseFilePath;

        private bool _isApplyingLocalization;

        /// <summary>
        /// Required by the WinForms Designer.
        /// Production code must use the constructor that supplies the recovery manager and database path.
        /// </summary>
        public JasonQueryDbRecoveryStartupForm()
        {
            InitializeComponent();

            txtRecoveryKey.ReadOnly = true;
        }

        public JasonQueryDbRecoveryStartupForm(JasonQueryDbRecoveryStartupManager recoveryManager, string databaseFilePath) : this()
        {
            _recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));

            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            _databaseFilePath = databaseFilePath;
        }

        public JasonQueryDbSecurityBootstrapResult RecoveryResult { get; private set; }

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

                ApplyLocalizedFormCaption();
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            btnBrowseFile.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            txtRecoveryKey.Clear();

            if (DialogResult != DialogResult.OK)
            {
                RecoveryResult = null;
            }

            base.OnFormClosed(e);
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

        private void ApplyLocalizedFormCaption()
        {
            Text = $"JasonQuery {AppConfigHelper.LocalVersion} - {Text}";
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
                ApplyLocalizedFormCaption();
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

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = GetMessage("OpenDialogTitle", "Select JasonQuery Recovery Key");
                dialog.Filter = "*.jqrecovery|*.jqrecovery";
                dialog.DefaultExt = "jqrecovery";
                dialog.AddExtension = true;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Multiselect = false;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                txtRecoveryKey.Text = dialog.FileName;
                txtRecoveryKey.SelectionStart = 0;
                txtRecoveryKey.SelectionLength = 0;
                btnRecover.Focus();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            RecoveryResult = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnRecover_Click(object sender, EventArgs e)
        {
            var recoveryKeyFilePath = txtRecoveryKey.Text.Trim();
            string recoveryKeyText = null;

            if (string.IsNullOrWhiteSpace(recoveryKeyFilePath))
            {
                MessageBox.Show
                (
                    GetMessage("RecoveryKeyRequired", "Select a Recovery Key file."),
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                btnBrowseFile.Focus();
                return;
            }

            if (!File.Exists(recoveryKeyFilePath))
            {
                MessageBox.Show
                (
                    GetMessage("FileLoadFailed", "The Recovery Key file could not be loaded.") + $"\r\n\r\n{recoveryKeyFilePath}",
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                );

                txtRecoveryKey.Clear();
                btnBrowseFile.Focus();
                return;
            }

            try
            {
                EnsureRecoveryDependenciesInitialized();
                SetRecoveryControlsEnabled(false);

                recoveryKeyText = File.ReadAllText(recoveryKeyFilePath, Encoding.UTF8).Trim();

                RecoveryResult = _recoveryManager.Recover(_databaseFilePath, recoveryKeyText);

                txtRecoveryKey.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                RecoveryResult = null;

                ShowError
                (
                    GetMessage
                    (
                        "RecoveryFailed",
                        "JasonQuery could not recover the key for JasonQuery.db. Verify that the selected Recovery Key file belongs to this JasonQuery.db."
                    ),
                    ex
                );
            }
            finally
            {
                recoveryKeyText = null;

                if (!IsDisposed && DialogResult != DialogResult.OK)
                {
                    SetRecoveryControlsEnabled(true);
                    btnBrowseFile.Focus();
                }
            }
        }

        private void EnsureRecoveryDependenciesInitialized()
        {
            if (_recoveryManager == null || string.IsNullOrWhiteSpace(_databaseFilePath))
            {
                throw new InvalidOperationException("The database Recovery form was not initialized with its runtime dependencies.");
            }
        }

        private void SetRecoveryControlsEnabled(bool enabled)
        {
            txtRecoveryKey.Enabled = enabled;
            btnBrowseFile.Enabled = enabled;
            btnRecover.Enabled = enabled;
            btnExit.Enabled = enabled;
            cboLocalization.Enabled = enabled;
        }

        private void ShowError(string prefix, Exception ex)
        {
            var message = string.IsNullOrWhiteSpace(ex?.Message) ? prefix : prefix + "\r\n\r\n" + ex.Message;

            MessageBox.Show
            (
                message,
                AppConfigHelper.MessageBoxCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private string GetMessage(string id, string defaultText)
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
    }
}
