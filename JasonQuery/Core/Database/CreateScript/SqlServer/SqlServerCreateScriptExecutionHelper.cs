using System;
using System.Data;

namespace JasonQuery.Core.Database.CreateScript.SqlServer
{
    internal static class SqlServerCreateScriptExecutionHelper
    {
        public static DataTable ExecuteAndLog(string sql, Func<string, DataTable> executeQuery)
        {
            return executeQuery?.Invoke(sql);
        }

        public static string RemoveWrappingParentheses(string text, int maxRounds)
        {
            var result = text ?? string.Empty;

            for (var i = 0; i < maxRounds; i++)
            {
                if (result.Length > 2 && result.StartsWith("(", StringComparison.Ordinal) && result.EndsWith(")", StringComparison.Ordinal))
                {
                    result = result.Substring(1, result.Length - 2);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static string BuildUnsupportedPackageText()
        {
            return "-- SQL Server does not have an Oracle-style native PACKAGE schema object.\r\n" +
                   "-- If you meant SSIS packages, those belong to Integration Services, not database schema objects.";
        }
    }
}
