using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Function
{
    internal static class MySqlFunctionMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Information");

            sbSql.AppendLine("SELECT Routine_Schema AS DbName, Routine_Name AS FunctionName, Routine_Type AS Type,");
            sbSql.AppendLine("       Data_Type AS ReturnType, Routine_Definition AS Definition");
            sbSql.AppendLine("  FROM Information_Schema.Routines");
            sbSql.AppendLine($" WHERE Routine_Schema = {databaseName}");
            sbSql.AppendLine("   AND Routine_Type = 'FUNCTION'");
            sbSql.Append(" ORDER BY Routine_Schema, Routine_Name");

            return sbSql.ToString();
        }
    }
}