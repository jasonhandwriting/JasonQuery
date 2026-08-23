using JasonLibrary.Core;
using JasonLibrary.Core.Update;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class UpdateForm : Form
    {
        public bool IsCheckOnStartup { get; set; }

        public bool UpdateCheckCompletedSuccessfully { get; private set; }

        private readonly MessageForm _messageForm = new MessageForm();
        private const string EnvironmentProd = "PROD";
        private const string EnvironmentTest = "TEST";
        private UpdateReleaseSelection _selectedRelease;

        public UpdateForm()
        {
            InitializeComponent();
        }

        private async void Form_Load(object sender, EventArgs e)
        {
            try
            {
                Hide();

                LocalizationHelper.ApplyLanguageInfo(this, false);

                var toolTip1 = new ToolTip
                {
                    AutoPopDelay = 5000
                };

                var manualUpdateHelpToolTip = LocalizationHelper.GetLanguageString("How to update manually", "form", GetType().Name, "object", "btnHelp_HowToUpdate", "ToolTipText");

                toolTip1.SetToolTip(btnHelp_HowToUpdate, manualUpdateHelpToolTip);

                ConfigureMetadataLink
                (
                    UpdateSourceResolver.ResolveMetadata
                    (
                        MyLibrary.UpdateMetadataSource,
                        MyLibrary.UpdateMetadataLocalFolder
                    )
                );

                lnkDownloadJasonQuery64.Enabled = false;
                lnkDownloadJasonQuery64Test.Enabled = false;
                btnUpdateNow.Enabled = false;

                lblLength.Text = grpDownloadInfo.Text;
                grpDownloadInfo.Text += "   ";
                btnHelp_HowToUpdate.Location = new Point(grpDownloadInfo.Left + lblLength.Width - 8, btnHelp_HowToUpdate.Top);

                if (IsCheckOnStartup)
                {
                    //顯示 MessageForm
                    _messageForm.TopLevel = true;

                    var updateCheckCaption = LocalizationHelper.GetLanguageString("Check for Updates", "form", GetType().Name, "msg", "CheckforUpdatesTitle", "Text");

                    _messageForm.Caption = updateCheckCaption;

                    var updateCheckInformation = LocalizationHelper.GetLanguageString("check for updates on startup...", "form", GetType().Name, "msg", "CheckforUpdatesInfo", "Text");

                    _messageForm.Info = updateCheckInformation;
                    _messageForm.StartPosition = FormStartPosition.CenterScreen;
                    _messageForm.IsNeedToMovePosition = true; //20231111 加入此變數，MessageForm 顯示於螢幕中央時，視窗的位置再往上調整一些，如果有錯誤發生時，MessageBox 才不會剛好擋住 MessageForm！
                    _messageForm.Show();
                    _messageForm.Refresh();

                    await CheckForUpdatesAsync();
                }
                else
                {
                    Show();
                }
            }
            catch (Exception ex)
            {
                CloseStartupMessage();

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (IsCheckOnStartup)
                {
                    Close();
                }
                else
                {
                    Show();
                }
            }
        }

        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            var source = MyLibrary.UpdateMetadataSource;
            UpdateContentLocation metadataLocation = null;

            try
            {
                Cursor = Cursors.WaitCursor;
                btnCheckForUpdates.Enabled = false;
                btnUpdateNow.Enabled = false;
                UpdateCheckCompletedSuccessfully = false;
                _selectedRelease = null;

                var localFolder = MyLibrary.UpdateMetadataLocalFolder;

                metadataLocation = UpdateSourceResolver.ResolveMetadata(source, localFolder);

                var installedChannel = UpdateChannelResolver.Resolve(AppConfigHelper.LocalVersion);
                UpdateMetadataManifest manifest;

                using (var provider = new UpdateMetadataProvider($"JasonQuery/{AppConfigHelper.LocalVersion}"))
                {
                    manifest = await provider.LoadAsync(source, localFolder, CancellationToken.None);
                }

                var selector = new UpdateReleaseSelector();

                _selectedRelease = selector.FindUpdate
                (
                    manifest,
                    AppConfigHelper.LocalVersion
                );

                ConfigureMetadataLink(metadataLocation);

                ConfigurePackageLink
                (
                    lnkDownloadJasonQuery64,
                    source,
                    localFolder,
                    selector.FindLatest(manifest, UpdateChannel.Production)
                );

                ConfigurePackageLink
                (
                    lnkDownloadJasonQuery64Test,
                    source,
                    localFolder,
                    selector.FindLatest(manifest, UpdateChannel.Test)
                );

                var displayChannel = _selectedRelease?.Channel ?? installedChannel;
                var versionDescription = GetVersionDescription(displayChannel);

                btnCheckForUpdates.Visible = false;
                lblInfo.Visible = true;
                lblInfo2.Visible = true;
                btnUpdateNow.Enabled = _selectedRelease != null && source == UpdateMetadataSourceKind.OfficialWebsite;
                UpdateCheckCompletedSuccessfully = true;

                CloseStartupMessage();

                if (_selectedRelease != null)
                {
                    lblInfo.Text = LocalizationHelper.GetLanguageString("A new version of JasonQuery is available:", "form", GetType().Name, "object", "lblInfoHasNewVersion", "Text");
                    lblInfo2.Text = $"{_selectedRelease.Version} {versionDescription}";

                    lblInfo.ForeColor = Color.DarkGreen;
                    lblInfo2.ForeColor = Color.DarkGreen;

                    if (IsCheckOnStartup)
                    {
                        Show();
                    }
                }
                else
                {
                    if (IsCheckOnStartup)
                    {
                        Cursor = Cursors.Default;

                        //啟動時沒有新版本，直接關閉 UpdateForm
                        Hide();
                        Close();
                        return;
                    }

                    lblInfo.Text = LocalizationHelper.GetLanguageString("You are using the latest version of JasonQuery.", "form", GetType().Name, "object", "lblInfoLatest", "Text");
                    lblInfo2.Text = $"{AppConfigHelper.LocalVersion} {versionDescription}";
                }
            }
            catch (Exception ex)
            {
                CloseStartupMessage();

                var message = BuildUpdateCheckFailureMessage(ex, source, metadataLocation);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (IsCheckOnStartup)
                {
                    Hide();
                    Close();
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    Cursor = Cursors.Default;
                    btnCheckForUpdates.Enabled = true;
                }
            }
        }

        private string BuildUpdateCheckFailureMessage(Exception exception, UpdateMetadataSourceKind source, UpdateContentLocation metadataLocation)
        {
            var methodLabel = GetUpdateMessage("Update check method:", "UpdateCheckMethodLabel");
            var method = GetUpdateCheckMethod(source);

            var summary = GetUpdateMessage
            (
                "JasonQuery could not check for updates. Please verify the update source and try again.",
                "UpdateCheckFailed"
            );

            var reason = GetUpdateCheckFailureReason(exception, source);
            var sourceLabel = GetUpdateMessage("Update information source:", "UpdateInformationSourceLabel");
            var sourceValue = metadataLocation?.Value;

            if (exception is UpdateMetadataLoadException loadException && !string.IsNullOrWhiteSpace(loadException.Location))
            {
                sourceValue = loadException.Location;
            }

            if (string.IsNullOrWhiteSpace(sourceValue))
            {
                return $"{methodLabel}\r\n{method}\r\n\r\n{summary}\r\n\r\n{reason}";
            }

            return $"{methodLabel}\r\n{method}\r\n\r\n{summary}\r\n\r\n{reason}\r\n\r\n{sourceLabel}\r\n{sourceValue}";
        }

        private string GetUpdateCheckMethod(UpdateMetadataSourceKind source)
        {
            switch (source)
            {
                case UpdateMetadataSourceKind.GitHub:
                    {
                        return GetUpdateMessage("GitHub Releases", "UpdateCheckMethodGitHub");
                    }
                case UpdateMetadataSourceKind.LocalFolder:
                    {
                        return GetUpdateMessage("Company Update Folder", "UpdateCheckMethodLocalFolder");
                    }
                case UpdateMetadataSourceKind.OfficialWebsite:
                default:
                    {
                        return GetUpdateMessage("JasonQuery Website", "UpdateCheckMethodOfficialWebsite");
                    }
            }
        }

        private string GetUpdateCheckFailureReason(Exception exception, UpdateMetadataSourceKind source)
        {
            if (exception is UpdateMetadataLoadException loadException)
            {
                string reason;

                switch (loadException.FailureKind)
                {
                    case UpdateMetadataFailureKind.NotFound:
                        {
                            reason = source == UpdateMetadataSourceKind.GitHub
                                     ? GetUpdateMessage("The selected GitHub Releases source was not found.", "UpdateSourceNotFound")
                                     : GetUpdateMessage("The JasonQuery update information file was not found at the selected source.", "UpdateFileNotFound");
                            break;
                        }
                    case UpdateMetadataFailureKind.AccessDenied:
                        {
                            reason = GetUpdateMessage("Access to the update information source was denied.", "UpdateSourceAccessDenied");
                            break;
                        }
                    case UpdateMetadataFailureKind.Timeout:
                        {
                            reason = GetUpdateMessage("The update information source did not respond in time.", "UpdateSourceTimeout");
                            break;
                        }
                    case UpdateMetadataFailureKind.Network:
                        {
                            reason = GetUpdateMessage("JasonQuery could not connect to the update information source.", "UpdateSourceNetworkError");
                            break;
                        }
                    case UpdateMetadataFailureKind.ServerError:
                        {
                            reason = GetUpdateMessage("The update server is temporarily unavailable.", "UpdateSourceServerError");
                            break;
                        }
                    case UpdateMetadataFailureKind.ReadError:
                        {
                            reason = GetUpdateMessage("JasonQuery could not read the update information file.", "UpdateFileReadError");
                            break;
                        }
                    default:
                        {
                            reason = GetUpdateMessage("The update source returned an unexpected HTTP response.", "UpdateSourceHttpError");
                            break;
                        }
                }

                if (loadException.StatusCode.HasValue)
                {
                    reason += $" (HTTP {(int)loadException.StatusCode.Value})";
                }

                return reason;
            }

            if (exception is NotSupportedException)
            {
                return GetUpdateMessage("The update information format is not supported by this version of JasonQuery.", "UpdateFormatNotSupported");
            }

            if (exception is FormatException)
            {
                return GetUpdateMessage("The update information file contains invalid JSON or data.", "UpdateContentInvalid");
            }

            return GetUpdateMessage("An unexpected error occurred while checking for updates.", "UpdateUnexpectedError");
        }

        private string GetUpdateMessage(string defaultText, string id)
        {
            return LocalizationHelper.GetLanguageString(defaultText, "form", GetType().Name, "msg", id, "Text");
        }

        private void ConfigureMetadataLink(UpdateContentLocation location)
        {
            lnkCheck.Text = location.Value;
            lnkCheck.Tag = location;
            lnkCheck.Enabled = true;
        }

        private static void ConfigurePackageLink(LinkLabel link, UpdateMetadataSourceKind source, string localFolder, UpdateReleaseSelection selection)
        {
            link.Tag = null;
            link.Enabled = false;

            if (selection == null)
            {
                return;
            }

            link.Text = selection.Asset.Name;

            var location = UpdateSourceResolver.ResolvePackage(source, localFolder, selection.Asset);

            if (location.IsLocalFile && !File.Exists(location.Value))
            {
                return;
            }

            link.Tag = location;
            link.Enabled = true;
        }

        private void lnkCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkCheck.LinkVisited = true;
            OpenContentLocation(lnkCheck);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lnkDownloadJasonQuery64_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkDownloadJasonQuery64.LinkVisited = true;
            OpenContentLocation(lnkDownloadJasonQuery64);
        }

        private void lnkDownloadJasonQuery64Test_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkDownloadJasonQuery64Test.LinkVisited = true;
            OpenContentLocation(lnkDownloadJasonQuery64Test);
        }

        private void btnHelp_HowToUpdate_Click(object sender, EventArgs e)
        {
            var manualUpdateTitle = LocalizationHelper.GetLanguageString("How to manually update JasonQuery to the latest version:", "form", GetType().Name, "msg", "HowToUpdate0", "Text") + "\r\n\r\n";
            var downloadPackageStep = LocalizationHelper.GetLanguageString("1. Download the latest JasonQuery64.zip", "form", GetType().Name, "msg", "HowToUpdate1", "Text") + "\r\n";
            var closeJasonQueryStep = LocalizationHelper.GetLanguageString("2. Close all open JasonQuery", "form", GetType().Name, "msg", "HowToUpdate2", "Text") + "\r\n";
            var extractPackageStep = LocalizationHelper.GetLanguageString("3. Unzip \"JasonQuery x64\" or \"JasonQuery x86\" to the folder where JasonQuery is currently located (overwrite all, JasonQuery64.zip does not include JasonQuery.db)", "form", GetType().Name, "msg", "HowToUpdate3", "Text") + "\r\n";
            var runJasonQueryStep = LocalizationHelper.GetLanguageString("4. Run JasonQuery", "form", GetType().Name, "msg", "HowToUpdate4", "Text");

            MessageBox.Show($"{manualUpdateTitle}{downloadPackageStep}{closeJasonQueryStep}{extractPackageStep}{runJasonQueryStep}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            var executeName = $@"{Application.StartupPath}\Updater.exe";

            if (File.Exists(executeName))
            {
                var environment = ResolveUpdateEnvironment(_selectedRelease?.Channel ?? UpdateChannelResolver.Resolve(AppConfigHelper.LocalVersion));

                var infoExe = new ProcessStartInfo
                {
                    FileName = executeName,
                    WorkingDirectory = $@"{Application.StartupPath}\",
                    Arguments = $"{LocalizationHelper.LocalizationCode}|{LocalizationHelper.XmlFileName}|{environment}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(infoExe);

                if (string.IsNullOrWhiteSpace(DatabaseSqlExecutor.DbConnectionString))
                {
                    Environment.Exit(0);
                }

                Close();
            }
            else
            {
                var fileNotFoundMessage = LocalizationHelper.GetLanguageString("File not found:", "form", GetType().Name, "msg", "UpdaterNotFound", "Text");

                MessageBox.Show($"{fileNotFoundMessage}\r\n\r\n{executeName}", AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ResolveUpdateEnvironment(UpdateChannel channel)
        {
            return channel == UpdateChannel.Test ? EnvironmentTest : EnvironmentProd;
        }

        private string GetVersionDescription(UpdateChannel channel)
        {
            return channel == UpdateChannel.Test
                   ? LocalizationHelper.GetLanguageString("(Test Version)", "form", GetType().Name, "msg", "TestVersion", "Text")
                   : LocalizationHelper.GetLanguageString("(Release Version)", "form", GetType().Name, "msg", "ProductionVersion", "Text");
        }

        private void OpenContentLocation(LinkLabel link)
        {
            if (!(link.Tag is UpdateContentLocation location))
            {
                return;
            }

            try
            {
                Process.Start
                (
                    new ProcessStartInfo
                    {
                        FileName = location.Value,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show
                (
                    message,
                    AppConfigHelper.JasonQueryVersion,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                );
            }
        }

        private void CloseStartupMessage()
        {
            if (!_messageForm.IsDisposed)
            {
                _messageForm.Hide();
                _messageForm.Close();
            }
        }
    }
}