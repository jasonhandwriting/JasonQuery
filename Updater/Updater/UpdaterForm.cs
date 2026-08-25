using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Updater.Core;

namespace Updater
{
    public partial class UpdaterForm : Form
    {
        private const int MfByCommand = 0x00000000;
        private const int MfGrayed = 0x00000001;
        private const int ScClose = 0xF060;

        private readonly ToolTip _toolTip = new ToolTip
        {
            ForeColor = Color.Blue,
            BackColor = Color.Gray,
            AutoPopDelay = 5000
        };

        private CancellationTokenSource _downloadCancellation;
        private UpdaterRequest _request;
        private UpdateWorkspace _workspace;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr windowHandle, bool revert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr menuHandle, uint itemId, uint enableFlags);

        public UpdaterForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            EnableMenuItem(GetSystemMenu(Handle, false), ScClose, MfByCommand | MfGrayed);
            ConfigureLog();
            LoadLocalizationXml(MyGlobal.sXmlFilename);
            ConfigureCulture();
            RegisterMessageBoxManager();
            InitializeStatusDisplay();

            MyGlobal.ApplyLanguageInfo(this, false);

            lblStep2.Tag = lblStep2.Text;
            lblStep3.Tag = lblStep3.Text;

            txtTo.Text = Application.StartupPath + Path.DirectorySeparatorChar;
            txtTo.ReadOnly = true;
            txtUpdateSource.ReadOnly = true;
            btnBrowseLocalFolder.Visible = false;

            rdoJasonQueryOfficial.AutoCheck = false;
            rdoGitHubOfficial.AutoCheck = false;
            rdoLocal.AutoCheck = false;
            rdoRelease.AutoCheck = false;
            rdoPreview.AutoCheck = false;

            _request = UpdaterRuntime.Request;

            if (_request == null)
            {
                DisableUnverifiedUpdate();
            }
            else
            {
                ConfigureRequest(_request);
            }

            CheckJasonQueryExecuted();
            tmrCheck.Enabled = true;
        }

        private void ConfigureRequest(UpdaterRequest request)
        {
            request.Validate();

            rdoJasonQueryOfficial.Checked = string.Equals(request.SourceKind, "OfficialWebsite", StringComparison.OrdinalIgnoreCase);
            rdoGitHubOfficial.Checked = string.Equals(request.SourceKind, "GitHub", StringComparison.OrdinalIgnoreCase);
            rdoLocal.Checked = string.Equals(request.SourceKind, "LocalFolder", StringComparison.OrdinalIgnoreCase);
            txtUpdateSource.Text = request.PackageLocation;

            rdoRelease.Checked = string.Equals(request.Environment, "PROD", StringComparison.OrdinalIgnoreCase);
            rdoPreview.Checked = string.Equals(request.Environment, "TEST", StringComparison.OrdinalIgnoreCase);

            UpdatePackageLabels(request.PackageName);

            var updateToolTip = GetText
            (
                "Install the selected JasonQuery version after verifying its size and SHA-256.",
                "VerifiedUpdateToolTip",
                "ToolTipText"
            ) + $"\r\n{request.TargetVersion}";

            var downloadToolTip = GetText
            (
                "Open the selected update package location:",
                "OpenPackageLocationToolTip",
                "ToolTipText"
            ) + $"\r\n{request.PackageLocation}";

            _toolTip.SetToolTip(btnUpdate, updateToolTip);
            _toolTip.SetToolTip(btnDownload, downloadToolTip);

            btnUpdate.Enabled = true;
            btnDownload.Enabled = true;
        }

        private void DisableUnverifiedUpdate()
        {
            btnUpdate.Enabled = false;
            btnDownload.Enabled = false;
            grpVersion.Enabled = false;
            grpUpdateSource.Enabled = false;

            var instruction = GetText
            (
                "Please start Updater from JasonQuery after selecting and verifying an available update.",
                "StartUpdaterFromJasonQuery"
            );

            var details = string.IsNullOrWhiteSpace(UpdaterRuntime.StartupError) ? string.Empty : $"\r\n\r\n{UpdaterRuntime.StartupError}";

            MessageBox.Show
            (
                instruction + details,
                "JasonQuery Updater",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            await UpdateFileAsync();
        }

        private async Task UpdateFileAsync()
        {
            if (_request == null)
            {
                DisableUnverifiedUpdate();
                return;
            }

            if (!ConfirmAndCloseJasonQuery())
            {
                return;
            }

            SetBusyState(true);
            _downloadCancellation = new CancellationTokenSource();

            try
            {
                _workspace = UpdateWorkspace.Create(_request.PackageName);
                txtFrom.Text = _workspace.Root + Path.DirectorySeparatorChar;

                await AcquirePackageAsync(_request, _workspace.PackagePath, _downloadCancellation.Token);
                UpdatePackageVerifier.Verify(_workspace.PackagePath, _request.ExpectedSize, _request.ExpectedDigest);

                picStep2.Image = picChecked.Image;
                pbDownloadStatus.Value = 100;
                btnCancel.Enabled = false;

                TraceLogger.LogInfo($"Verified update package: {_workspace.PackagePath}");
                TraceLogger.LogInfo($"Expected size: {_request.ExpectedSize}");
                TraceLogger.LogInfo($"Expected digest: {_request.ExpectedDigest}");

                var payloadRoot = SafeZipExtractor.ExtractAndResolvePayloadRoot
                (
                    _workspace.PackagePath,
                    _workspace.ExtractionRoot
                );

                picStep3.Image = picChecked.Image;
                TraceLogger.LogInfo($"Extracted payload root: {payloadRoot}");

                StartWorker(payloadRoot);
                Close();
            }
            catch (OperationCanceledException)
            {
                TraceLogger.LogInfo("Update package acquisition was cancelled by the user.");
                ResetAfterFailure();
            }
            catch (Exception ex)
            {
                TraceLogger.LogInfo($"Updater preparation failed: {ex}");
                ShowPreparationFailure(ex);
                ResetAfterFailure();
            }
            finally
            {
                _downloadCancellation?.Dispose();
                _downloadCancellation = null;

                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private async Task AcquirePackageAsync(UpdaterRequest request, string destinationPath, CancellationToken cancellationToken)
        {
            var partialPath = destinationPath + ".partial";

            TryDeleteFile(partialPath);
            TryDeleteFile(destinationPath);

            if (request.PackageIsLocal)
            {
                if (!File.Exists(request.PackageLocation))
                {
                    throw new FileNotFoundException("The selected company update package was not found.", request.PackageLocation);
                }

                cancellationToken.ThrowIfCancellationRequested();
                File.Copy(request.PackageLocation, partialPath, false);
                cancellationToken.ThrowIfCancellationRequested();
                pbDownloadStatus.Value = 100;
            }
            else
            {
                await DownloadFileAsync(new Uri(request.PackageLocation), partialPath, cancellationToken);
            }

            File.Move(partialPath, destinationPath);
        }

        private Task DownloadFileAsync(Uri source, string destinationPath, CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            var client = new WebClient();
            CancellationTokenRegistration registration = default(CancellationTokenRegistration);

            client.DownloadProgressChanged += (sender, args) =>
            {
                pbDownloadStatus.Value = Math.Max(pbDownloadStatus.Minimum, Math.Min(pbDownloadStatus.Maximum, args.ProgressPercentage));
            };

            client.DownloadFileCompleted += (sender, args) =>
            {
                registration.Dispose();
                client.Dispose();

                if (args.Cancelled)
                {
                    completion.TrySetCanceled();
                }
                else if (args.Error != null)
                {
                    completion.TrySetException(args.Error);
                }
                else
                {
                    completion.TrySetResult(null);
                }
            };

            registration = cancellationToken.Register(client.CancelAsync);

            try
            {
                client.DownloadFileAsync(source, destinationPath);
            }
            catch (Exception ex)
            {
                registration.Dispose();
                client.Dispose();
                completion.TrySetException(ex);
            }

            return completion.Task;
        }

        private void StartWorker(string payloadRoot)
        {
            var backupRoot = UpdateWorkspace.CreateBackupRoot(_request.InstalledVersion, _request.TargetVersion);

            var plan = new UpdaterWorkerPlan
            {
                ParentProcessId = Process.GetCurrentProcess().Id,
                InstallationRoot = Application.StartupPath,
                WorkspaceRoot = _workspace.Root,
                PayloadRoot = payloadRoot,
                BackupRoot = backupRoot,
                LaunchJasonQuery = chkLaunchJQ.Checked,
                Request = _request
            };

            File.Copy(Application.ExecutablePath, _workspace.WorkerExecutablePath, false);
            UpdaterWorkerPlan.Write(_workspace.WorkerPlanPath, plan);

            TraceLogger.LogInfo($"Start temporary Updater worker: {_workspace.WorkerExecutablePath}");
            TraceLogger.LogInfo($"Worker plan: {_workspace.WorkerPlanPath}");

            var process = Process.Start
            (
                new ProcessStartInfo
                {
                    FileName = _workspace.WorkerExecutablePath,
                    Arguments = $"--apply-plan \"{_workspace.WorkerPlanPath}\"",
                    WorkingDirectory = _workspace.Root,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            );

            if (process == null)
            {
                throw new InvalidOperationException("The temporary Updater worker could not be started.");
            }
        }

        private bool ConfirmAndCloseJasonQuery()
        {
            if (string.IsNullOrEmpty(CheckJasonQueryExecuted()))
            {
                return true;
            }

            var message = GetText
            (
                "Automatically close all executing JasonQuery?",
                "CloseJQAutomatically"
            ) + "\r\n\r\n" + GetText
            (
                "Please confirm whether you need to save or execute the \"Commit/Rollback\" command before pressing the \"Yes\" button!",
                "CloseJQAutomatically2"
            );

            if (MessageBox.Show(message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return false;
            }

            var targetPath = Path.Combine(Application.StartupPath, "JasonQuery.exe");

            foreach (var process in Process.GetProcessesByName("JasonQuery"))
            {
                try
                {
                    if (string.Equals(process.MainModule?.FileName, targetPath, StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit(10000);
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.LogInfo($"Could not close JasonQuery process {process.Id}: {ex.Message}");
                }
                finally
                {
                    process.Dispose();
                }
            }

            if (!string.IsNullOrEmpty(CheckJasonQueryExecuted()))
            {
                var failed = GetText
                (
                    "JasonQuery is still running. Close it before starting the update.",
                    "JasonQueryStillRunning"
                );

                MessageBox.Show(failed, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private string CheckJasonQueryExecuted()
        {
            var titles = string.Empty;

            try
            {
                foreach (var process in Process.GetProcessesByName("JasonQuery"))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.ProcessName)
                            && string.Equals(Path.GetDirectoryName(process.MainModule?.FileName), Application.StartupPath, StringComparison.OrdinalIgnoreCase))
                        {
                            titles += process.MainWindowTitle + "\r\n";
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                titles = $"JasonQuery x64 ({ex.Message})\r\n";
            }

            titles = titles.TrimEnd('\r', '\n');

            txtList.ReadOnly = false;
            txtList.Text = titles;
            txtList.ReadOnly = true;

            var stopped = string.IsNullOrEmpty(titles);

            picStep1.Image = stopped ? picChecked.Image : picUnchecked.Image;
            lblStep1.ForeColor = stopped ? Color.Black : Color.DarkRed;

            return titles;
        }

        private void SetBusyState(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            tmrCheck.Enabled = !busy;
            btnCancel.Enabled = busy;
            btnUpdate.Enabled = !busy && _request != null;
            btnDownload.Enabled = !busy && _request != null;
            btnClose.Enabled = !busy;
            chkLaunchJQ.Enabled = !busy;
        }

        private void ResetAfterFailure()
        {
            pbDownloadStatus.Value = 0;
            picStep2.Image = picUnchecked.Image;
            picStep3.Image = picUnchecked.Image;
            picStep4.Image = picUnchecked.Image;
            SetBusyState(false);
        }

        private void ShowPreparationFailure(Exception exception)
        {
            var title = GetText("Update failed!", "UpdateFailed");
            string summary;

            if (exception is UpdatePackageVerificationException verificationException)
            {
                switch (verificationException.FailureKind)
                {
                    case UpdatePackageVerificationFailureKind.SizeMismatch:
                        {
                            summary = GetText("The downloaded package size does not match the update metadata.", "PackageSizeMismatch");
                            break;
                        }
                    case UpdatePackageVerificationFailureKind.DigestMismatch:
                        {
                            summary = GetText("The downloaded package SHA-256 does not match the update metadata.", "PackageDigestMismatch");
                            break;
                        }
                    default:
                        {
                            summary = GetText("The selected update package was not found.", "PackageNotFound");
                            break;
                        }
                }
            }
            else if (exception is InvalidDataException)
            {
                summary = GetText("The update package is damaged or contains an unsafe archive path.", "PackageInvalid");
            }
            else if (exception is FileNotFoundException)
            {
                summary = GetText("The selected update package was not found.", "PackageNotFound");
            }
            else
            {
                summary = GetText("The update package could not be prepared.", "PackagePreparationFailed");
            }

            MessageBox.Show
            (
                $"{summary}\r\n\r\n{exception.Message}\r\n\r\n{GetText("Updater log:", "UpdaterLogLocation")}\r\n{MyGlobal.sLogFilename}",
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        private void UpdatePackageLabels(string packageName)
        {
            var step2Template = Convert.ToString(lblStep2.Tag, CultureInfo.InvariantCulture) ?? lblStep2.Text;
            var step3Template = Convert.ToString(lblStep3.Tag, CultureInfo.InvariantCulture) ?? lblStep3.Text;

            GetRichText(txtStep2, step2Template, packageName);
            GetRichText(txtStep3, step3Template, packageName);
        }

        private static void GetRichText(RichTextBox textBox, string template, string packageName)
        {
            using (var font = new Font("Microsoft JhengHei", 9))
            {
                textBox.Text = string.Empty;

                var parts = template.Split(new[] { "JasonQuery.zip" }, StringSplitOptions.None);

                textBox.AppendText(parts[0].Trim() + " ", Color.Black, font);

                if (parts.Length > 1)
                {
                    textBox.AppendText(packageName, Color.Blue, font);
                    textBox.AppendText(" " + parts[1].Trim(), Color.Black, font);
                }
                else
                {
                    textBox.AppendText(packageName, Color.Blue, font);
                }

                textBox.Refresh();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _downloadCancellation?.Cancel();
            btnCancel.Enabled = false;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (_request == null)
            {
                return;
            }

            try
            {
                if (_request.PackageIsLocal)
                {
                    Process.Start("explorer.exe", $"/select,\"{_request.PackageLocation}\"");
                }
                else
                {
                    Process.Start
                    (
                        new ProcessStartInfo
                        {
                            FileName = _request.PackageLocation,
                            UseShellExecute = true
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tmrCheck_Tick(object sender, EventArgs e)
        {
            CheckJasonQueryExecuted();
        }

        private void rdoUpdateProduction_CheckedChanged(object sender, EventArgs e)
        {
            //The version is selected and integrity-checked by JasonQuery.
        }

        private void rdoUpdateBeta_CheckedChanged(object sender, EventArgs e)
        {
            //The version is selected and integrity-checked by JasonQuery.
        }

        private void frmUpdater_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBoxManager.Unregister();
        }

        private void InitializeStatusDisplay()
        {
            picStep1.Image = picUnchecked.Image;
            picStep2.Image = picUnchecked.Image;
            picStep3.Image = picUnchecked.Image;
            picStep4.Image = picUnchecked.Image;

            lblStep1.ForeColor = Color.Black;
            lblStep2.ForeColor = Color.Black;
            lblStep3.ForeColor = Color.Black;
            lblStep4.ForeColor = Color.Black;

            txtFrom.ReadOnly = true;
            pbDownloadStatus.Value = 0;
            btnCancel.Enabled = false;
        }

        private static void ConfigureLog()
        {
            var logPath = Path.Combine(Application.StartupPath, "Log");

            try
            {
                Directory.CreateDirectory(logPath);
            }
            catch
            {
                logPath = Application.StartupPath;
            }

            MyGlobal.sLogFilename = Path.Combine(logPath, "Updater.log");

            if (!File.Exists(MyGlobal.sLogFilename))
            {
                return;
            }

            try
            {
                File.Delete(MyGlobal.sLogFilename);
            }
            catch
            {
                MyGlobal.sLogFilename = Path.Combine(logPath, $"Updater_{DateTime.Now:yyyyMMddHHmmss}.log");
            }
        }

        private static void LoadLocalizationXml(string xmlFileName)
        {
            if (!string.IsNullOrWhiteSpace(xmlFileName))
            {
                var xmlPath = Path.Combine(Application.StartupPath, "localization", xmlFileName);

                if (File.Exists(xmlPath))
                {
                    MyGlobal.dtLocalization = MyGlobal.XmlToDataTable(xmlPath);
                }
            }

            MyGlobal.sAnUnexpectedErrorHasOccurred = MyGlobal.GetLanguageString("An unexpected error has occurred.", "Global", "Global", "msg", "AnUnexpectedErrorHasOccurred", "Text");
            MyGlobal.sStackTrace = MyGlobal.GetLanguageString("Stack Trace:", "Global", "Global", "msg", "StackTrace", "Text");
        }

        private static void ConfigureCulture()
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(MyGlobal.sLocalization);
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(MyGlobal.sLocalization);
            }
            catch
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }
        }

        private static void RegisterMessageBoxManager()
        {
            MessageBoxManager.OK = MyGlobal.GetLanguageString("&OK", "Global", "Global", "messagebox", "OK", "Text");
            MessageBoxManager.Cancel = MyGlobal.GetLanguageString("&Cancel", "Global", "Global", "messagebox", "Cancel", "Text");
            MessageBoxManager.Abort = MyGlobal.GetLanguageString("&Abort", "Global", "Global", "messagebox", "Abort", "Text");
            MessageBoxManager.Retry = MyGlobal.GetLanguageString("&Retry", "Global", "Global", "messagebox", "Retry", "Text");
            MessageBoxManager.Ignore = MyGlobal.GetLanguageString("&Ignore", "Global", "Global", "messagebox", "Ignore", "Text");
            MessageBoxManager.Yes = MyGlobal.GetLanguageString("&Yes", "Global", "Global", "messagebox", "Yes", "Text");
            MessageBoxManager.No = MyGlobal.GetLanguageString("&No", "Global", "Global", "messagebox", "No", "Text");
            MessageBoxManager.Register();
        }

        private string GetText(string defaultText, string id, string attribute = "Text")
        {
            return MyGlobal.GetLanguageString(defaultText, "form", Name, "msg", id, attribute);
        }

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                //A unique workspace will be used if an old file cannot be deleted.
            }
        }
    }

    public static class RichTextBoxColorExtensions
    {
        public static void AppendText(this RichTextBox textBox, string text, Color color, Font font, bool isNewLine = false)
        {
            textBox.SuspendLayout();
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.SelectionColor = color;
            textBox.SelectionFont = font;
            textBox.AppendText(isNewLine ? $"{text}{Environment.NewLine}" : text);
            textBox.SelectionColor = textBox.ForeColor;
            textBox.ScrollToCaret();
            textBox.ResumeLayout();
        }
    }
}
