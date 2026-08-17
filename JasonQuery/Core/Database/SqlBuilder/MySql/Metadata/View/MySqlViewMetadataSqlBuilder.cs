using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.View
{
    internal static class MySqlViewMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT Table_Schema AS DbName, Table_Name AS ViewName, View_Definition");
            sbSql.AppendLine("  FROM Information_Schema.Views");
            sbSql.AppendLine($" WHERE Table_Schema = {databaseName}");
            sbSql.Append(" ORDER BY Table_Schema, Table_Name");

            return sbSql.ToString();
        }
    }
}