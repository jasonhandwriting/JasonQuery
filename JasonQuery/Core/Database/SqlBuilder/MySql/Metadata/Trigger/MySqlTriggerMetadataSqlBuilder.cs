using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Trigger
{
    internal static class MySqlTriggerMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Information");

            sbSql.AppendLine("SELECT Trigger_Schema AS DbName, Trigger_Name AS TriggerName, Created AS Create_Date");
            sbSql.AppendLine("  FROM Information_Schema.Triggers cc");
            sbSql.AppendLine($" WHERE Trigger_Schema = {databaseName}");
            sbSql.Append(" ORDER BY Trigger_Schema, Trigger_Name");

            return sbSql.ToString();
        }
    }
}
