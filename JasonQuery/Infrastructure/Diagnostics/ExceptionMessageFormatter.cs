using JasonQuery.Core.Config;
using System;
using System.Text;

namespace JasonQuery.Infrastructure.Diagnostics
{
    internal static class ExceptionMessageFormatter
    {
        public static string BuildExceptionDisplayMessage(Exception ex, bool errorMessageOnly = false)
        {
            if (ex == null)
            {
                return MyGlobal.AnUnexpectedErrorHasOccurred;
            }

            var message = ex.Message ?? string.Empty;
            var sourceInfo = ExtractSourceLocationsFromStackTrace(ex.StackTrace);

            if (errorMessageOnly)
            {
                if (string.IsNullOrWhiteSpace(sourceInfo))
                {
                    return message;
                }

                return string.Format("{0}\r\n\r\n{1}", message, sourceInfo);
            }

            if (string.IsNullOrWhiteSpace(sourceInfo))
            {
                return string.Format
                (
                    "{0}\r\n\r\n{1}\r\n\r\n{2}",
                    MyGlobal.AnUnexpectedErrorHasOccurred,
                    message,
                    MyGlobal.PleaseTryAgain
                );
            }

            return string.Format
            (
                "{0}\r\n\r\n{1}\r\n\r\n{2}\r\n\r\n{3}",
                MyGlobal.AnUnexpectedErrorHasOccurred,
                message,
                MyGlobal.PleaseTryAgain,
                sourceInfo
            );
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

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

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
    }
}
