using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Updater;

namespace Updater.Core
{
    internal interface IUpdaterWorkerUi
    {
        void ReportStatus(string status, string instruction);

        void ShowMessage(string message, string title, MessageBoxIcon icon);
    }

    internal static class UpdaterWorker
    {
        public static int Run(string planPath, IUpdaterWorkerUi workerUi = null)
        {
            UpdaterWorkerPlan plan = null;

            try
            {
                plan = UpdaterWorkerPlan.Load(planPath);
                ConfigureLogging(plan.WorkspaceRoot);
                LoadLocalization(plan);
                ReportStatus(workerUi, "Preparing the update. Please wait.", "WorkerPreparingUpdate");
                WaitForParent(plan.ParentProcessId);

                TraceLogger.LogInfo($"Begin update transaction: {plan.Request.InstalledVersion} -> {plan.Request.TargetVersion}");
                TraceLogger.LogInfo($"Payload root: {plan.PayloadRoot}");
                TraceLogger.LogInfo($"Installation root: {plan.InstallationRoot}");
                TraceLogger.LogInfo($"Backup root: {plan.BackupRoot}");

                var transaction = new FileUpdateTransaction();

                var result = transaction.Execute
                (
                    plan.PayloadRoot,
                    plan.InstallationRoot,
                    plan.BackupRoot,
                    plan.Request.InstalledVersion,
                    plan.Request.TargetVersion,
                    stage => ReportTransactionStage(workerUi, stage)
                );

                TraceLogger.LogInfo($"Update transaction completed. Updated files: {result.UpdatedFileCount}");
                TraceLogger.LogInfo($"Backup manifest: {result.BackupManifestPath}");

                ReportStatus(workerUi, "Finalizing the update and organizing old backups.", "WorkerFinalizingUpdate");
                ApplyBackupRetention(result);
                ShowSuccess(plan, result, workerUi);

                if (plan.LaunchJasonQuery)
                {
                    LaunchJasonQuery(plan.InstallationRoot, workerUi);
                }

                return 0;
            }
            catch (UpdateTransactionException ex)
            {
                ConfigureFallbackLogging(plan);
                TraceLogger.LogInfo($"Update transaction failed: {ex.UpdateException}");

                if (ex.RecoveryException != null)
                {
                    TraceLogger.LogInfo($"Automatic recovery failed: {ex.RecoveryException}");
                }
                else
                {
                    TraceLogger.LogInfo("Automatic recovery completed successfully.");
                    ApplyBackupRetention(ex.BackupManifestPath);
                }

                ShowTransactionFailure(ex, workerUi);

                return ex.RecoverySucceeded ? 2 : 3;
            }
            catch (Exception ex)
            {
                ConfigureFallbackLogging(plan);
                TraceLogger.LogInfo($"Updater worker failed: {ex}");

                var title = "JasonQuery Updater";

                var message = GetText
                (
                    "The update could not be started. No update files were intentionally applied.",
                    "UpdateWorkerFailed"
                );

                var details = ex is ProtectedUpdatePathException protectedPathException
                              ? $"{GetText("The update package contains a protected application file:", "ProtectedApplicationFile")}\r\n{protectedPathException.RelativePath}"
                              : ex.Message;

                ShowMessage
                (
                    workerUi,
                    $"{message}\r\n\r\n{details}\r\n\r\n{GetLogLocationText()}",
                    title,
                    MessageBoxIcon.Error
                );

                return 1;
            }
        }

        private static void ShowSuccess(UpdaterWorkerPlan plan, UpdateTransactionResult result, IUpdaterWorkerUi workerUi)
        {
            var completed = GetText("Update completed!", "UpdateCompleted");
            var backup = GetText("A verified backup of the previous version was saved at:", "VerifiedBackupLocation");
            var closeInstruction = plan.LaunchJasonQuery
                                   ? GetText("Press OK to close the Updater and launch JasonQuery.", "PressOKAndLaunchJasonQuery")
                                   : GetText("Press OK to close the Updater.", "PressOK");

            ShowMessage
            (
                workerUi,
                $"{completed}\r\n\r\n{backup}\r\n{Path.GetDirectoryName(result.BackupManifestPath)}\r\n\r\n{closeInstruction}",
                "JasonQuery Updater",
                MessageBoxIcon.Information
            );
        }

        private static void ShowTransactionFailure(UpdateTransactionException exception, IUpdaterWorkerUi workerUi)
        {
            var failed = GetText("Update failed!", "UpdateFailed");
            var recovery = exception.RecoverySucceeded
                           ? GetText("The previous version was restored automatically and verified with SHA-256.", "AutomaticRecoveryCompleted")
                           : GetText("Automatic recovery could not be completed. Do not start JasonQuery until the installation has been repaired.", "AutomaticRecoveryFailed");
            var backup = GetText("Verified backup location:", "VerifiedBackupLocationShort");
            var details = exception.UpdateException?.Message ?? exception.Message;

            if (exception.RecoveryException != null)
            {
                details += $"\r\n{exception.RecoveryException.Message}";
            }

            ShowMessage
            (
                workerUi,
                $"{failed}\r\n\r\n{recovery}\r\n\r\n{details}\r\n\r\n{backup}\r\n{Path.GetDirectoryName(exception.BackupManifestPath)}\r\n\r\n{GetLogLocationText()}",
                "JasonQuery Updater",
                exception.RecoverySucceeded ? MessageBoxIcon.Warning : MessageBoxIcon.Error
            );
        }

        private static string GetLogLocationText()
        {
            var label = GetText("Updater log:", "UpdaterLogLocation");

            return $"{label}\r\n{MyGlobal.sLogFilename}";
        }

        private static void WaitForParent(int parentProcessId)
        {
            try
            {
                using (var parent = Process.GetProcessById(parentProcessId))
                {
                    if (!parent.WaitForExit(60000))
                    {
                        throw new TimeoutException("The Updater user interface did not exit within 60 seconds.");
                    }
                }
            }
            catch (ArgumentException)
            {
                //The parent process exited before the worker opened it.
            }

            Thread.Sleep(250);
        }

        private static void LaunchJasonQuery(string installationRoot, IUpdaterWorkerUi workerUi)
        {
            var executablePath = Path.Combine(installationRoot, "JasonQuery.exe");

            try
            {
                Process.Start
                (
                    new ProcessStartInfo(executablePath)
                    {
                        WorkingDirectory = installationRoot,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                );
            }
            catch (Exception ex)
            {
                var message = GetText
                (
                    "The update completed, but JasonQuery could not be launched automatically.",
                    "LaunchJasonQueryFailed"
                );

                ShowMessage
                (
                    workerUi,
                    $"{message}\r\n\r\n{ex.Message}",
                    "JasonQuery Updater",
                    MessageBoxIcon.Warning
                );
            }
        }

        private static void ApplyBackupRetention(UpdateTransactionResult result)
        {
            ApplyBackupRetention(result.BackupManifestPath);
        }

        private static void ApplyBackupRetention(string backupManifestPath)
        {
            try
            {
                var currentBackupRoot = Path.GetDirectoryName(backupManifestPath);
                var backupParent = Path.GetDirectoryName(currentBackupRoot);

                var retentionResult = UpdateBackupRetentionPolicy.Prune
                (
                    backupParent,
                    currentBackupRoot,
                    UpdateBackupRetentionPolicy.DefaultMaximumVerifiedBackups
                );

                foreach (var deletedDirectory in retentionResult.DeletedDirectories)
                {
                    TraceLogger.LogInfo($"Deleted old verified update backup: {deletedDirectory}");
                }

                foreach (var failedDirectory in retentionResult.FailedDirectories)
                {
                    TraceLogger.LogInfo($"Could not delete old verified update backup: {failedDirectory}");
                }
            }
            catch (Exception ex)
            {
                //Retention maintenance must not change a completed update or recovery into a failure.
                TraceLogger.LogInfo($"Update backup retention was skipped: {ex.Message}");
            }
        }

        private static void ReportTransactionStage(IUpdaterWorkerUi workerUi, UpdateTransactionStage stage)
        {
            switch (stage)
            {
                case UpdateTransactionStage.CreatingVerifiedBackup:
                    {
                        ReportStatus
                        (
                            workerUi,
                            "Creating and verifying a backup of the previous version.",
                            "WorkerCreatingBackup"
                        );

                        break;
                    }
                case UpdateTransactionStage.ApplyingAndVerifyingUpdate:
                    {
                        ReportStatus
                        (
                            workerUi,
                            "Installing and verifying the update files.",
                            "WorkerApplyingUpdate"
                        );

                        break;
                    }
                case UpdateTransactionStage.RestoringAndVerifyingPreviousVersion:
                    {
                        ReportStatus
                        (
                            workerUi,
                            "The update failed. Restoring and verifying the previous version.",
                            "WorkerRestoringBackup"
                        );

                        break;
                    }
            }
        }

        private static void ReportStatus(IUpdaterWorkerUi workerUi, string defaultText, string id)
        {
            if (workerUi == null)
            {
                return;
            }

            var instruction = GetText
            (
                "Do not close this window or start JasonQuery until the update is complete.",
                "WorkerDoNotClose"
            );

            try
            {
                workerUi.ReportStatus(GetText(defaultText, id), instruction);
            }
            catch
            {
                // Progress reporting must never interrupt an update transaction.
            }
        }

        private static void ShowMessage(IUpdaterWorkerUi workerUi, string message, string title, MessageBoxIcon icon)
        {
            if (workerUi != null)
            {
                workerUi.ShowMessage(message, title, icon);
                return;
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

        private static void LoadLocalization(UpdaterWorkerPlan plan)
        {
            try
            {
                MyGlobal.sLocalization = plan.Request.LocalizationCode;
                MyGlobal.sXmlFilename = plan.Request.LocalizationFile;
                MyGlobal.sEnvironment = plan.Request.Environment;

                var localizationPath = Path.Combine
                (
                    plan.InstallationRoot,
                    "localization",
                    plan.Request.LocalizationFile
                );

                if (File.Exists(localizationPath))
                {
                    MyGlobal.dtLocalization = MyGlobal.XmlToDataTable(localizationPath);
                }

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
            catch
            {
                //Worker messages fall back to their English defaults.
            }
        }

        private static string GetText(string defaultText, string id)
        {
            return MyGlobal.GetLanguageString(defaultText, "form", "UpdaterForm", "msg", id, "Text");
        }

        private static void ConfigureLogging(string workspaceRoot)
        {
            Directory.CreateDirectory(workspaceRoot);
            MyGlobal.sLogFilename = Path.Combine(workspaceRoot, "UpdaterWorker.log");
        }

        private static void ConfigureFallbackLogging(UpdaterWorkerPlan plan)
        {
            if (!string.IsNullOrWhiteSpace(MyGlobal.sLogFilename))
            {
                return;
            }

            try
            {
                ConfigureLogging(plan?.WorkspaceRoot ?? Path.GetTempPath());
            }
            catch
            {
                MyGlobal.sLogFilename = Path.Combine(Path.GetTempPath(), "JasonQuery-UpdaterWorker.log");
            }
        }
    }
}
