using JasonLibrary.Core;
using JasonQuery.Core.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace JasonQuery.Core.Logging
{
    public static class TraceLogger
    {
        public static void LogStage(string stage)
        {
            if (!AppConfigHelper.HasGenerateLogFile)
            {
                return;
            }

            var dateTimeFormat = $"{MyLibrary.DateFormat} HH:mm:ss.fff";
            var now = DateTime.Now.ToString(dateTimeFormat);
            var traceInfo = GetCallerInfo();
            var logLine = $"{now} | {traceInfo} | {stage}";

            try
            {
                File.AppendAllText(AppConfigHelper.LogFileName, $"{logLine}{Environment.NewLine}");
            }
            catch
            {
                //忽略存檔錯誤，避免中斷原有的功能
            }
        }

        public static IDisposable Time(string label,
                                       [CallerMemberName] string callerName = "",
                                       [CallerFilePath] string callerFile = "",
                                       [CallerLineNumber] int callerLine = 0)
        {
            return new TraceTimer(label, callerName, callerFile, callerLine);
        }

        public static string GetCallerInfo()
        {
            var stackTrace = new StackTrace(true); //true = 要抓取檔案和行號資訊

            for (var i = 1; i < stackTrace.FrameCount; i++) //從第1層開始抓 (跳過自己)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                var declaringType = method.DeclaringType;

                if (declaringType != typeof(TraceLogger))
                {
                    //var className = declaringType?.Name ?? "UnknownClass";
                    var methodName = method.Name;
                    var fileName = Path.GetFileName(frame.GetFileName()) ?? "UnknownFile";
                    int lineNumber = frame.GetFileLineNumber();

                    return $"{methodName} | {fileName}: Line {lineNumber}";
                }
            }

            return "UnknownCaller";
        }

        public static string GetStackTraceContent(string stackTrace)
        {
            var result = string.Empty;
            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    result += $"{value}\r\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.TrimEnd('\r', '\n');
            }
            else
            {
                result = stackTrace;
            }

            return result.Trim();
        }

        public static string GetStackTraceMessageAndContent(string stackTrace, string message, bool isErrorMessageOnly = false)
        {
            var result = string.Empty;
            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    result += $"{value.Trim()}\r\n\r\n";
                }
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.TrimEnd('\r', '\n');
            }
            else
            {
                result = stackTrace;
            }

            var messageFinal = string.Empty;

            if (isErrorMessageOnly)
            {
                messageFinal = $"{message}\r\n\r\n{result}";
            }
            else
            {
                messageFinal = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{message}\r\n\r\n{MyGlobal.PleaseTryAgain}\r\n\r\n{result}";
            }

            return messageFinal;
        }

        public static string BuildExceptionDisplayMessage2(Exception ex, bool errorMessageOnly = false)
        {
            if (ex == null)
            {
                return MyGlobal.AnUnexpectedErrorHasOccurred;
            }

            var sourceInfo = ExtractSourceLocationsFromStackTrace(ex.StackTrace);
            var message = ex.Message ?? string.Empty;

            if (errorMessageOnly)
            {
                return string.Format("{0}\r\n\r\n{1}", message, sourceInfo);
            }

            return string.Format("{0}\r\n\r\n{1}\r\n\r\n{2}\r\n\r\n{3}", MyGlobal.AnUnexpectedErrorHasOccurred, message,
                                 MyGlobal.PleaseTryAgain, sourceInfo);
        }

        private static string ExtractSourceLocationsFromStackTrace(string stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
            {
                return string.Empty;
            }

            var parts = stackTrace.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                var value = parts[i];

                if (value.Contains("\\") && value.Contains(".cs"))
                {
                    sb.AppendLine(value.Trim());
                    sb.AppendLine();
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString().TrimEnd('\r', '\n');
            }

            return stackTrace;
        }

        private sealed class TraceTimer : IDisposable
        {
            private readonly Stopwatch stopwatch;
            private readonly string sTraceInfo;

            public TraceTimer(string label, string callerName, string callerFile, int callerLine)
            {
                var fileName = Path.GetFileName(callerFile);

                sTraceInfo = $"{callerName}() | {fileName}: Line {callerLine} | [{label}]";
                stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                stopwatch.Stop();

                if (!AppConfigHelper.HasGenerateLogFile)
                {
                    return;
                }

                var elapsedMs = stopwatch.ElapsedMilliseconds;
                var now = DateTime.Now.ToString($"{MyLibrary.DateFormat} HH:mm:ss.fff");
                var formattedTime = FormatElapsedTime(elapsedMs);
                var memoryUsage = GetMemoryUsage(); //獲取記憶體使用量
                var message = $"{now} | {sTraceInfo} - Finished in {formattedTime} | Memory Usage: {memoryUsage}";

                try
                {
                    File.AppendAllText(AppConfigHelper.LogFileName, $"{message}{Environment.NewLine}");
                }
                catch
                {
                    //忽略存檔錯誤，避免中斷原有的功能
                }
            }

            private string FormatElapsedTime(long ms)
            {
                long sec = ms / 1000;
                long remain = ms % 1000;

                return sec > 0 ? $"{sec}s {remain}ms" : $"{remain}ms";
            }

            //獲取當前 JasonQuery 的記憶體使用量 (單位：MB)
            private string GetMemoryUsage()
            {
                var process = Process.GetCurrentProcess();
                long memoryUsageBytes = process.PrivateMemorySize64;
                double memoryUsageMB = memoryUsageBytes / (1024.0 * 1024.0); //轉換為 MB

                return $"{memoryUsageMB:F2} MB";  //格式化為 2 位小數的 MB
            }
        }
    }
}
