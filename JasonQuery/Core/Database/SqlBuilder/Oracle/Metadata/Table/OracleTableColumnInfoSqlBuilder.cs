using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table
{
    internal static class OracleTableColumnInfoSqlBuilder
    {
        public static string BuildPrimaryKeyInfo(string schemaName, string dbUserUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column and Primary Key Information");

            sbSql.AppendLine("SELECT cols.Column_Name");
            sbSql.AppendLine("  FROM All_Constraints cons, All_Cons_Columns cols");
            sbSql.AppendLine($" WHERE cols.Table_Name = '{schemaName}'");
            sbSql.AppendLine("   AND cons.Constraint_Type = 'P'");
            sbSql.AppendLine("   AND cons.Constraint_Name = cols.Constraint_Name");
            sbSql.AppendLine("   AND cons.Owner = cols.Owner");
            sbSql.AppendLine($"   AND UPPER(cons.Owner) = '{dbUserUppercase}'");
            sbSql.Append(" ORDER BY cols.Position");

            return sbSql.ToString();
        }

        public static string BuildColumnInfo(string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

            //取得 Table/View 的所有欄位 (按照原始順序)
            sbSql.AppendLine("SELECT '1' AS \" \", Column_Name, Column_ID, Data_Type AS TypeName,");
            sbSql.AppendLine("       Data_Type AS DataTypeName, Data_Type AS DataType, Data_Length AS ColumnSize,");
            sbSql.AppendLine("       Data_Scale AS NumericScale, Data_Precision AS NumericPrecision");
            sbSql.AppendLine("  FROM User_Tab_Columns");
            sbSql.AppendLine($" WHERE Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY Column_ID");

            return sbSql.ToString();
        }

        public static string BuildColumnInfoIncludeDefaultValue(string dbUserUppercase, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Info (including Default Value)");

            sbSql.AppendLine("SELECT 'TABLE' AS Schema_Type, ss.Table_Name AS TableName, ss.Column_Name AS ColumnName,");
            sbSql.AppendLine("       ss.Column_ID AS ColumnID, ss.Data_Type AS DataType, ss.Data_Length AS DataLength, ss.Nullable,");
            sbSql.AppendLine("       ss.Data_Default AS DefaultValue, ss.Data_Precision || ',' || ss.Data_Scale as Scale, cc.Comments");
            sbSql.AppendLine("  FROM User_Tab_Columns ss");
            sbSql.AppendLine("       LEFT JOIN User_Col_Comments cc ON (ss.Column_Name = cc.Column_Name AND ss.Table_Name = cc.Table_Name)");
            sbSql.AppendLine($" WHERE ss.Table_Name IN (SELECT Object_Name FROM All_Objects WHERE Object_Type IN ('TABLE') AND UPPER(Owner) = '{dbUserUppercase}')");
            sbSql.AppendLine($"   AND ss.Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY TO_NUMBER(ss.Column_ID)");

            return sbSql.ToString();
        }
    }
}