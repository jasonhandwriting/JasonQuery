using System;
using System.IO;
using System.Runtime.InteropServices;

namespace JasonQuery.Infrastructure.Windows
{
    internal static class WindowsShortcutHelper
    {
        public static bool TryCreateDesktopShortcut(string shortcutName, string targetFilePath, string workingDirectory,
                                                    string description, string iconLocation, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(shortcutName))
            {
                errorMessage = "Shortcut name is empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetFilePath))
            {
                errorMessage = "Target file path is empty.";
                return false;
            }

            if (!File.Exists(targetFilePath))
            {
                errorMessage = $"Target file does not exist.\r\n{targetFilePath}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(targetFilePath);
            }

            if (string.IsNullOrWhiteSpace(iconLocation))
            {
                iconLocation = targetFilePath;
            }

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

            object shellObject = null;
            object shortcutObject = null;

            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");

                if (shellType == null)
                {
                    errorMessage = "WScript.Shell is not available.";
                    return false;
                }

                shellObject = Activator.CreateInstance(shellType);

                dynamic shell = shellObject;

                shortcutObject = shell.CreateShortcut(shortcutPath);

                dynamic shortcut = shortcutObject;

                shortcut.TargetPath = targetFilePath;
                shortcut.Arguments = string.Empty;
                shortcut.WorkingDirectory = workingDirectory;
                shortcut.WindowStyle = 1;
                shortcut.Description = description ?? shortcutName;
                shortcut.IconLocation = iconLocation;
                shortcut.Save();

                return File.Exists(shortcutPath);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                ReleaseComObject(shortcutObject);
                ReleaseComObject(shellObject);
            }
        }

        public static bool DesktopShortcutExists(string shortcutName)
        {
            if (string.IsNullOrWhiteSpace(shortcutName))
            {
                return false;
            }

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

            return File.Exists(shortcutPath);
        }

        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null)
            {
                return;
            }

            try
            {
                if (Marshal.IsComObject(comObject))
                {
                    Marshal.FinalReleaseComObject(comObject);
                }
            }
            catch
            {
                //釋放 COM 物件失敗時，不覆蓋原本的建立結果
            }
        }
    }
}
