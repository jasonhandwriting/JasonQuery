using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.View
{
    internal static class MySqlViewColumnMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information (Including Column Name and Type)");

            sbSql.AppendLine("SELECT v.Table_Schema AS DbName, v.Table_Name AS ViewName, c.Column_Name AS ColumnName,");
            sbSql.AppendLine("       c.Column_Type AS ColumnType, c.Ordinal_Position AS OrdinalPosition");
            sbSql.AppendLine("  FROM Information_Schema.Views AS v");
            sbSql.AppendLine("       JOIN Information_Schema.Columns AS c ON c.Table_Schema = v.Table_Schema");
            sbSql.AppendLine("        AND c.Table_Name = v.Table_Name");
            sbSql.AppendLine($" WHERE v.Table_Schema = {databaseName}");
            sbSql.Append(" ORDER BY v.Table_Schema, v.Table_Name, c.Ordinal_Position");

            return sbSql.ToString();
        }
    }
}