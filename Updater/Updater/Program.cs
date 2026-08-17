using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Updater
{
    static class Program
    {
        private static bool bNewInstance;
        static string sGuid = "{JQUPDATE12-C91D-1231-1688-B2D7E22D1F1D}";

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (var m = new Mutex(false, "Global\\" + sGuid, out bNewInstance))
            {
                if (bNewInstance)
                {
                    if (args.Length > 0)
                    {
                        var sArg = args[0].ToString().Replace("``", " ");
                        var sArgs = sArg.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                        if (sArgs.Length >= 2)
                        {
                            MyGlobal.sLocalization = sArgs[0].ToString();
                            MyGlobal.sXmlFilename = sArgs.Length > 1 ? sArgs[1].ToString() : string.Empty;
                            MyGlobal.sEnvironment = sArgs.Length > 2 ? sArgs[2].ToString() : string.Empty;
                        }
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    //20250527 註冊並處理全域的異常捕捉
                    Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                    Application.Run(new UpdaterForm());
                }
                else
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

        private static void HandleException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            //20250526 將錯誤訊息中，有包含 "xxxx.cs:" 的訊息顯示出來
            var result = string.Empty;
            var parts = ex.StackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Contains("\\") && parts[i].Contains(".cs:"))
                {
                    result += parts[i] + "\r\n";
                }
            }
            
            if (!string.IsNullOrEmpty(result))
            {
            	result = result.Substring(0, result.Length - 2);
            }

            var message = $"{MyGlobal.sAnUnexpectedErrorHasOccurred}\r\n{ex.Message}\r\n\r\n{MyGlobal.sStackTrace}\r\n{(string.IsNullOrEmpty(result) ? ex.StackTrace : result)}";

            MessageBox.Show(message, "JasonQuery Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}