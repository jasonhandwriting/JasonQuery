using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Updater.Core;

namespace Updater
{
    internal static class Program
    {
        private const string ApplicationGuid = "{JQUPDATE12-C91D-1231-1688-B2D7E22D1F1D}";
        private const string RequestArgument = "--request";
        private const string WorkerPlanArgument = "--apply-plan";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr windowHandle);

        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (TryGetArgumentValue(args, WorkerPlanArgument, out var workerPlanPath))
            {
                using (var progressForm = new UpdaterWorkerProgressForm(workerPlanPath))
                {
                    Application.Run(progressForm);
                    Environment.ExitCode = progressForm.WorkerExitCode;
                }

                return;
            }

            using (var mutex = new Mutex(false, "Global\\" + ApplicationGuid, out var isNewInstance))
            {
                if (!isNewInstance)
                {
                    BringExistingInstanceToFront();
                    return;
                }

                LoadRequest(args);

                Application.Run(new UpdaterForm());
            }
        }

        private static void LoadRequest(string[] args)
        {
            if (TryGetArgumentValue(args, RequestArgument, out var requestPath))
            {
                UpdaterRuntime.RequestPath = requestPath;

                try
                {
                    var request = UpdaterRequest.Load(requestPath);

                    UpdaterRuntime.Request = request;
                    MyGlobal.sLocalization = request.LocalizationCode;
                    MyGlobal.sXmlFilename = request.LocalizationFile;
                    MyGlobal.sEnvironment = request.Environment;
                }
                catch (Exception ex)
                {
                    UpdaterRuntime.StartupError = ex.Message;
                }
                finally
                {
                    TryDeleteRequestFile(requestPath);
                }

                return;
            }

            //Compatibility: retain localization from the legacy combined argument, but do not perform an unverified legacy download.
            if (args == null || args.Length == 0)
            {
                return;
            }

            var legacyArgument = args[0].Replace("``", " ");
            var values = legacyArgument.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length >= 2)
            {
                MyGlobal.sLocalization = values[0];
                MyGlobal.sXmlFilename = values[1];
                MyGlobal.sEnvironment = values.Length > 2 ? values[2] : string.Empty;
            }
        }

        private static bool TryGetArgumentValue(string[] args, string argumentName, out string value)
        {
            value = string.Empty;

            if (args == null)
            {
                return false;
            }

            for (var index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    value = args[index + 1];
                    return !string.IsNullOrWhiteSpace(value);
                }
            }

            return false;
        }

        private static void TryDeleteRequestFile(string requestPath)
        {
            try
            {
                File.Delete(requestPath);
            }
            catch
            {
                // Stale request files are harmless and contain no credentials.
            }
        }

        private static void BringExistingInstanceToFront()
        {
            var current = Process.GetCurrentProcess();

            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id == current.Id)
                {
                    continue;
                }

                SetForegroundWindow(process.MainWindowHandle);
                break;
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private static void HandleException(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            var stackTrace = exception.StackTrace ?? string.Empty;
            var sourceLines = string.Empty;
            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (var part in parts)
            {
                if (part.Contains("\\") && part.Contains(".cs:"))
                {
                    sourceLines += part + "\r\n";
                }
            }

            if (!string.IsNullOrEmpty(sourceLines))
            {
                sourceLines = sourceLines.Substring(0, sourceLines.Length - 2);
            }

            var displayedStackTrace = string.IsNullOrEmpty(sourceLines) ? stackTrace : sourceLines;
            var message = $"{MyGlobal.sAnUnexpectedErrorHasOccurred}\r\n{exception.Message}\r\n\r\n{MyGlobal.sStackTrace}\r\n{displayedStackTrace}";

            MessageBox.Show(message, "JasonQuery Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    internal sealed class UpdaterWorkerProgressForm : Form, IUpdaterWorkerUi
    {
        private readonly string _workerPlanPath;
        private readonly Label _statusLabel;
        private readonly Label _instructionLabel;
        private bool _workerCompleted;

        public UpdaterWorkerProgressForm(string workerPlanPath)
        {
            _workerPlanPath = workerPlanPath;

            Text = "JasonQuery Updater";
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(520, 155);
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;

            var layout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(22, 20, 22, 18),
                RowCount = 3
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _statusLabel = new Label
            {
                AutoEllipsis = true,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font(Font, FontStyle.Bold),
                Text = "..."
            };

            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                MarqueeAnimationSpeed = 30,
                Margin = new Padding(0, 12, 0, 8),
                Style = ProgressBarStyle.Marquee
            };

            _instructionLabel = new Label
            {
                AutoEllipsis = true,
                AutoSize = true,
                Dock = DockStyle.Fill,
                ForeColor = SystemColors.GrayText,
                Text = string.Empty
            };

            layout.Controls.Add(_statusLabel, 0, 0);
            layout.Controls.Add(progressBar, 0, 1);
            layout.Controls.Add(_instructionLabel, 0, 2);

            Controls.Add(layout);
            Shown += UpdaterWorkerProgressForm_Shown;
            FormClosing += UpdaterWorkerProgressForm_FormClosing;
        }

        public int WorkerExitCode { get; private set; } = 1;

        public void ReportStatus(string status, string instruction)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, string>(ReportStatus), status, instruction);
                return;
            }

            _statusLabel.Text = status ?? string.Empty;
            _instructionLabel.Text = instruction ?? string.Empty;
        }

        public void ShowMessage(string message, string title, MessageBoxIcon icon)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, string, MessageBoxIcon>(ShowMessage), message, title, icon);
                return;
            }

            MessageBox.Show(this, message, title, MessageBoxButtons.OK, icon);
        }

        private async void UpdaterWorkerProgressForm_Shown(object sender, EventArgs e)
        {
            Activate();
            BringToFront();

            try
            {
                WorkerExitCode = await Task.Run(() => UpdaterWorker.Run(_workerPlanPath, this));
            }
            finally
            {
                _workerCompleted = true;
                Close();
            }
        }

        private void UpdaterWorkerProgressForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_workerCompleted)
            {
                e.Cancel = true;
            }
        }
    }
}
