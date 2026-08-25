using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table
{
    internal static class MySqlTableColumnMetadataSqlBuilder
    {
        public static string Build(string databaseName, bool sortByColumnName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get all Column Information of the Specified Table");

            sbSql.AppendLine("SELECT Table_Schema AS DbName, Table_Name AS TableName, Column_Name AS ColumnName, Data_Type AS DataType, Character_Maximum_Length,");
            sbSql.AppendLine("       Numeric_Precision, Numeric_Scale, Column_Type AS ColumnType, Ordinal_Position AS OrdinalPosition");
            sbSql.AppendLine("  FROM Information_Schema.Columns");
            sbSql.AppendLine($" WHERE Table_Schema = {databaseName}");

            if (sortByColumnName)
            {
                sbSql.Append(" ORDER BY Table_Schema, Table_Name, Column_Name");
            }
            else
            {
                sbSql.Append(" ORDER BY Table_Schema, Table_Name, Ordinal_Position");
            }

            return sbSql.ToString();
        }
    }
}
