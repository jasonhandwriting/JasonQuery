using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class JasonQueryDbRecoveryKeyForm : Form
    {
        private readonly JasonQueryDbRecoveryEnrollmentManager _manager;
        private readonly string _databaseFilePath;

        public JasonQueryDbRecoveryKeyForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this);

                ReloadStatus();

                lblRecoveryStatus.ForeColor = Color.DarkBlue;
            }
            catch (Exception ex)
            {
                ShowUnexpectedError(ex);
            }
        }

        public JasonQueryDbRecoveryKeyForm(JasonQueryDbRecoveryEnrollmentManager manager, string databaseFilePath) : this()
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new ArgumentException("A database file path is required.", nameof(databaseFilePath));
            }

            _databaseFilePath = Path.GetFullPath(databaseFilePath);

            ReloadStatus();
        }

        private void btnCreateOrRegenerate_Click(object sender, EventArgs e)
        {
            var configured = _manager.IsRecoveryConfigured();

            if (configured)
            {
                var confirmation = MessageBox.Show
                (
                    GetMessage
                    (
                        "RegenerateConfirm",
                        "Regenerating the Recovery Key will make the previous Recovery Key invalid as soon as the new key is enabled.\r\n\r\nContinue?"
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
            }

            string exportedFilePath = null;
            var committed = false;

            try
            {
                using (var draft = _manager.PrepareEnrollment(_databaseFilePath))
                using (var dialog = new SaveFileDialog())
                {
                    dialog.AddExtension = true;
                    dialog.CheckPathExists = true;
                    dialog.DefaultExt = "jqrecovery";
                    dialog.Filter = "JasonQuery Recovery Key (*.jqrecovery)|*.jqrecovery";
                    dialog.FileName = "JasonQuery-Recovery-" + draft.KeyId + JasonQueryDbRecoveryKeyFile.FileExtension;
                    dialog.OverwritePrompt = false;
                    dialog.RestoreDirectory = true;
                    dialog.Title = GetMessage("SaveDialogTitle", "Save JasonQuery Recovery Key");

                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    exportedFilePath = dialog.FileName;

                    if (File.Exists(exportedFilePath))
                    {
                        MessageBox.Show
                        (
                            GetMessage
                            (
                                "ExistingFile",
                                "The selected file already exists. Choose a new file name so an existing Recovery Key is never overwritten."
                            ),
                            AppConfigHelper.MessageBoxCaption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        exportedFilePath = null;
                        return;
                    }

                    JasonQueryDbRecoveryKeyFile.SaveAndVerify
                    (
                        exportedFilePath,
                        draft.RecoveryKeyText,
                        draft.KeyId
                    );

                    var enableConfirmation = MessageBox.Show
                    (
                        GetMessage
                        (
                            "EnableAfterSave",
                            "The Recovery Key file was saved and verified.\r\n\r\n" +
                            "Keep this file in a secure location separate from JasonQuery.db and JasonQuery.security.json.\r\n\r\n" +
                            "Enable this Recovery Key now?"
                        ),
                        AppConfigHelper.MessageBoxCaption,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2
                    );

                    if (enableConfirmation != DialogResult.Yes)
                    {
                        JasonQueryDbRecoveryKeyFile.TryDelete(exportedFilePath);
                        exportedFilePath = null;
                        return;
                    }

                    _manager.CommitEnrollment(_databaseFilePath, draft);
                    committed = true;
                }

                ReloadStatus();

                MessageBox.Show
                (
                    GetMessage
                    (
                        "EnrollmentSuccess",
                        "The Recovery Key is now enabled.\r\n\r\n" +
                        "If this was a regeneration, the previous Recovery Key is no longer valid."
                    ),
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                if (!committed)
                {
                    JasonQueryDbRecoveryKeyFile.TryDelete(exportedFilePath);
                }

                ShowOperationError(ex);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
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

        private void ShowOperationError(Exception ex)
        {
            var prefix = GetMessage("OperationFailed", "The Recovery Key operation could not be completed.");
            var message = string.IsNullOrWhiteSpace(ex.Message) ? prefix : prefix + "\r\n\r\n" + ex.Message;

            MessageBox.Show
            (
                message,
                AppConfigHelper.MessageBoxCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            var confirmation = MessageBox.Show
            (
                GetMessage
                (
                    "DisableConfirm",
                    "Disable the Recovery Key?\r\n\r\n" +
                    "After this change, the existing .jqrecovery file can no longer recover this JasonQuery.db."
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

            try
            {
                _manager.DisableRecovery(_databaseFilePath);
                ReloadStatus();

                MessageBox.Show
                (
                    GetMessage
                    (
                        "DisableSuccess",
                        "The Recovery Key was disabled. The database encryption key was not changed."
                    ),
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                ShowOperationError(ex);
            }
        }

        private void ReloadStatus()
        {
            var configured = _manager.IsRecoveryConfigured();

            lblRecoveryStatus.Text = configured ? GetMessage("StatusConfigured", "Recovery Key: Configured") : GetMessage("StatusNotConfigured", "Recovery Key: Not configured");
            btnCreateOrRegenerate.Text = configured ? GetMessage("RegenerateButton", "Regenerate Recovery Key...") : GetMessage("CreateButton", "Create Recovery Key...");

            btnDisable.Enabled = configured;
            lblRecoveryStatus.ForeColor = Color.DarkBlue;
        }

        private static void ShowUnexpectedError(Exception ex)
        {
            var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
