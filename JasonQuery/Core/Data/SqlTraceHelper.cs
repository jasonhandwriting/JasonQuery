using JasonQuery.Core.Config;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace JasonQuery.Core.Data
{
    internal static class SqlTraceHelper
    {
        public static string BuildHeaderNewLine(string description, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            string sourceName = string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath);

            return $"{description}{MyGlobal.Separator3s}{sourceName},{methodName}(){MyGlobal.Separator7}\r\n";
        }

        public static void AppendHeader(StringBuilder sbSql, string sDescription, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
        {
            if (sbSql == null)
            {
                return;
            }

            string sourceName = string.IsNullOrEmpty(filePath) ? "Unknown" : Path.GetFileNameWithoutExtension(filePath);

            sbSql.AppendLine($"{sDescription}{MyGlobal.Separator3s}{sourceName},{methodName}(){MyGlobal.Separator7}");
        }
    }
}
