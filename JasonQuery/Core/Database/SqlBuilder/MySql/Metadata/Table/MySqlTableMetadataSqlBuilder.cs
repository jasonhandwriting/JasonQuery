using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table
{
    internal static class MySqlTableMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information");

            sbSql.AppendLine("SELECT Table_Schema AS DbName, Table_Name AS TableName, Table_Rows AS RowCount,");
            sbSql.AppendLine("       Create_Time AS Create_Date, Update_Time AS Modify_Date");
            sbSql.AppendLine("  FROM Information_Schema.Tables");
            sbSql.AppendLine($" WHERE Table_Schema = {databaseName}");
            sbSql.AppendLine("   AND Table_Type = 'BASE TABLE'");
            sbSql.Append(" ORDER BY Table_Schema, Table_Name");

            return sbSql.ToString();
        }
    }
}