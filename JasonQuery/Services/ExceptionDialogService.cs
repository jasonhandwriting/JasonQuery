using JasonQuery.Core.Config;
using JasonQuery.Infrastructure.Diagnostics;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Services
{
    internal static class ExceptionDialogService
    {
        public static string BuildMessage(Exception ex)
        {
            return ExceptionMessageFormatter.BuildExceptionDisplayMessage(ex, true);
        }

        public static void Show(Exception ex)
        {
            var message = ExceptionMessageFormatter.BuildExceptionDisplayMessage(ex);

            MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void Show(IWin32Window owner, Exception ex, bool errorMessageOnly = false)
        {
            var message = ExceptionMessageFormatter.BuildExceptionDisplayMessage(ex, errorMessageOnly);

            MessageBox.Show(owner, message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}