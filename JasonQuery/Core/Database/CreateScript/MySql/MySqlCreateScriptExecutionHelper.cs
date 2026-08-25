using System;
using System.Data;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal static class MySqlCreateScriptExecutionHelper
    {
        public static DataTable ExecuteSql(string sql, Func<string, DataTable> executeQuery)
        {

            return executeQuery(sql);
        }

        public static string BuildUnsupportedPackageText()
        {
            return "-- Package create script is not supported in the current MySQL stage-1 dispatcher.";
        }
    }
}
