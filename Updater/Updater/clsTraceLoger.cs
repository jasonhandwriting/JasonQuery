using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace Updater
{
    public static class TraceLogger
    {
        public static void LogStage(string sStage)
        {
            var sNow = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            var sTraceInfo = GetCallerInfo();
            var sLogLine = $"{sNow} | {sTraceInfo} | {sStage}";

            try
            {
                File.AppendAllText(MyGlobal.sLogFilename, sLogLine + Environment.NewLine);
            }
            catch
            {
                //忽略存檔錯誤，避免中斷原有的功能
            }
        }

        public static string GetCallerInfo()
        {
            var stackTrace = new StackTrace(true); //true = 要抓取檔案和行號資訊

            for (int i = 1; i < stackTrace.FrameCount; i++) //從第1層開始抓 (跳過自己)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                var declaringType = method.DeclaringType;

                if (declaringType != typeof(TraceLogger))
                {
                    var sClassName = declaringType?.Name ?? "UnknownClass";
                    var sMethodName = method.Name;
                    var sFileName = Path.GetFileName(frame.GetFileName()) ?? "UnknownFile";
                    int iLineNumber = frame.GetFileLineNumber();

                    return $"{sMethodName} | {sFileName}: Line {iLineNumber}";
                }
            }

            return "UnknownCaller";
        }

        public static void LogInfo(string sInfo)
        {
            var sNow = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            var sLogLine = $"{sNow} | {sInfo}";

            try
            {
                File.AppendAllText(MyGlobal.sLogFilename, sLogLine + Environment.NewLine);
            }
            catch
            {
                //忽略存檔錯誤，避免中斷原有的功能
            }
        }
    }
}