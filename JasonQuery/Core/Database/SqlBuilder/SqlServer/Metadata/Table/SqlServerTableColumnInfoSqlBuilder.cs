using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal static class SqlServerTableColumnInfoSqlBuilder
    {
        public static string BuildPrimaryKeyInfo(string schemaNode, string schemaDbo, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column and Primary Key Information");

            sbSql.AppendLine("SELECT col.Column_Name, col.Ordinal_Position, con.Constraint_Name, con.Constraint_Type");
            sbSql.AppendLine($"  FROM {schemaNode}.Information_Schema.Key_Column_Usage AS col, {schemaNode}.Information_Schema.Table_Constraints AS con");
            sbSql.AppendLine($" WHERE col.Table_Schema = '{schemaDbo}' AND col.Table_Name = '{schemaName}'");
            sbSql.AppendLine("   AND col.Table_Schema = con.Table_Schema");
            sbSql.AppendLine("   AND col.Table_Name = con.Table_Name");
            sbSql.AppendLine("   AND col.Constraint_Name = con.Constraint_Name");
            sbSql.AppendLine("   AND con.Constraint_Type = 'PRIMARY KEY'");
            sbSql.Append(" ORDER BY col.Ordinal_Position;");

            return sbSql.ToString();
        }

        public static string BuildTableColumnInfo(string schemaNode, string schemaDbo, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column Information");

            sbSql.AppendLine("SELECT '1' AS \" \", Column_Name, Ordinal_Position AS Column_ID, Data_Type AS TypeName,");
            sbSql.AppendLine("       Data_Type AS DataTypeName, Data_Type AS DataType, Character_Maximum_Length AS ColumnSize,");
            sbSql.AppendLine("       Numeric_Scale AS NumericScale, Numeric_Precision AS NumericPrecision");
            sbSql.AppendLine($"  FROM {schemaNode}.Information_Schema.Columns");
            sbSql.AppendLine($" WHERE Table_Name = '{schemaName}'");
            sbSql.AppendLine($"   AND Table_Schema = '{schemaDbo}'");
            sbSql.Append(" ORDER BY Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildColumnInfo(string schemaNode, string schemaDbo, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

            //取得所有欄位資訊 (包含 DefaultValue)    //u.Table_Catalog AS BaseSchemaNode, u.Table_Schema AS BaseSchemaName, u.Table_Name AS BaseTableName, 
            sbSql.AppendLine($"SELECT u.*, u.Column_Name AS ColumnName, u.Is_Nullable AS Nullable, u.Column_Default AS DefaultValue FROM {schemaNode}.Information_Schema.Columns u");
            sbSql.AppendLine($" WHERE Table_Schema = '{schemaDbo}'");
            sbSql.AppendLine($"   AND Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY Ordinal_Position;");

            return sbSql.ToString();
        }
    }
}
