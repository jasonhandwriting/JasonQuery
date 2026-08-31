using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Forms;
using JasonQuery.UI.Helpers;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace JasonQuery
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            MyGlobal.PendingExternalOpenFileName = string.Empty;

            if (args.Length >= 1)
            {
                var arg1 = args[0];

                if (string.Equals(arg1, "LOG", StringComparison.OrdinalIgnoreCase)) //20250615 新增 LOG 模式，逐步加入 LOG 供除錯、找問題、追查執行時間用
                {
                    AppConfigHelper.HasGenerateLogFile = true;
                }
                else
                {
                    //只取第一個參數即可 (只有在命令列模式才有機會在執行檔後面帶多個參數，例如 JasonQuery.exe c:\temp\123.sql c:\temp\456.sql)
                    //如果是透過檔案總案在檔名上快按兩次左鍵(呼叫 JasonQuery.exe)，一次選多個檔案，就會執行多個 JasonQuery.exe (一對一)
                    MyGlobal.PendingExternalOpenFileName = arg1;

                    if (!File.Exists(MyGlobal.PendingExternalOpenFileName))
                    {
                        MyGlobal.PendingExternalOpenFileName = string.Empty;
                    }
                }
            }

            TraceLogger.Initialize(Application.StartupPath);
            TraceLogger.SetContextProvider(CreateTraceLogContext);
            TraceLogger.SetEnabled(AppConfigHelper.HasGenerateLogFile);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //20250514 註冊並處理全域的異常捕捉
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            //20260712 掛載閒置事件
            Application.Idle += Application_Idle;

            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                Application.Idle -= Application_Idle;
                TraceLogger.Shutdown();
            }
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            //此處只作為低優先級檢查時機；MemoryHelper 會限制檢查及 Full GC 的頻率
            MemoryHelper.ClearMemory();
        }

        private static TraceLogContext CreateTraceLogContext()
        {
            return new TraceLogContext
            {
                DatabaseType = DatabaseSqlExecutor.CurrentDataSource == DataSourceType.None ? string.Empty : DatabaseSqlExecutor.CurrentDataSource.ToString(),
                DatabaseVersion = DatabaseSqlExecutor.DatabaseVersionDisplayText,
                ConnectionName = DatabaseSqlExecutor.DbConnectionName
            };
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

            TraceLogger.LogError("UnhandledException", ex);

            //20250526 將錯誤訊息中，有包含 "xxxx.cs" 的訊息顯示出來
            var result = TraceLogger.GetStackTraceContent(ex.StackTrace);
            var message = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n{ex.Message}\r\n\r\n{MyGlobal.StackTrace}\r\n{result}";

            MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
