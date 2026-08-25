using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table
{
    internal static class MySqlTableColumnInfoSqlBuilder
    {
        public static string BuildPrimaryKeyInfo(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Primary Key Information");

            sbSql.AppendLine("SELECT Column_Name, Constraint_Name AS ConstraintInfo");
            sbSql.AppendLine("  FROM `Information_Schema`.`Key_Column_Usage`");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaNode}'");
            sbSql.AppendLine($"   AND Table_Name = '{schemaName}'");
            sbSql.AppendLine("   AND Constraint_Name = 'PRIMARY'");
            sbSql.Append(" ORDER BY Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildColumnInfo(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

            sbSql.AppendLine("SELECT '1' AS \" \", Column_Name, Ordinal_Position AS Column_ID, Column_Type AS TypeName,");
            sbSql.AppendLine("       Column_Type AS DataTypeName, Column_Type AS DataType, Character_Maximum_Length AS ColumnSize,");
            sbSql.AppendLine("       Numeric_Scale AS NumericScale, Numeric_Precision AS NumericPrecision");
            sbSql.AppendLine("  FROM `Information_Schema`.`Columns`");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaNode}'");
            sbSql.AppendLine($"   AND Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildColumnInfoIncludeDefaultValue(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

            sbSql.AppendLine("SELECT Table_Schema AS SchemaName, 'Tables' AS SchemaType, Table_Name AS TableName, Column_Name AS ColumnName,");
            sbSql.AppendLine("       Ordinal_Position AS ColumnID, SUBSTR(Is_Nullable, 1, 1) AS Nullable,");
            sbSql.AppendLine("       Column_Default AS DefaultValue, Column_Type AS DataType, Column_Key AS ColumnKey,");
            sbSql.AppendLine("       Column_Type AS ColumnType, Extra, Column_Comment AS Comments, Character_Maximum_Length AS ColumnSize,");
            sbSql.AppendLine("       Numeric_Precision AS NumericPrecision, Numeric_Scale AS NumericScale");
            sbSql.AppendLine("  FROM `Information_Schema`.`Columns`");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaNode}'");
            sbSql.AppendLine($"   AND Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY Ordinal_Position;");

            return sbSql.ToString();
        }
    }
}
