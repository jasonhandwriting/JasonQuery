using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table
{
    internal static class PostgreSqlTableColumnInfoSqlBuilder
    {
        public static string BuildPrimaryKeyInfo(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column and Primary Key Information");

            sbSql.AppendLine("SELECT ee.Constraint_Schema AS SchemaName, ee.Table_Name AS TableName, ee.Column_Name,");
            sbSql.AppendLine("       LOWER(SUBSTR(ss.Constraint_Type, 1, 1)) || ', ' || ss.Constraint_Name AS ConstraintInfo");
            sbSql.AppendLine("  FROM Information_Schema.Key_Column_Usage ee, Information_Schema.Table_Constraints ss");
            sbSql.AppendLine(" WHERE ee.Constraint_Catalog = ss.Constraint_Catalog");
            sbSql.AppendLine("   AND ee.Constraint_Schema = ss.Constraint_Schema");
            sbSql.AppendLine("   AND ee.Table_Schema = ss.Table_Schema");
            sbSql.AppendLine("   AND ee.Table_Name = ss.Table_Name");
            sbSql.AppendLine("   AND ee.Constraint_Name = ss.Constraint_Name");
            sbSql.AppendLine("   AND ss.Constraint_Type <> 'CHECK'");
            sbSql.AppendLine($"   AND ee.Constraint_Schema = '{schemaNode}'");
            sbSql.AppendLine($"   AND ee.Table_Name = '{schemaName}'");
            sbSql.Append(" ORDER BY ee.Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildTableColumnInfo(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column Information");

            //取得 Table 的所有指定欄位, 按照原始順序
            sbSql.AppendLine("SELECT '1' AS \" \", cs.Column_Name AS Column_Name, cs.Ordinal_Position AS Column_ID, LOWER(cs.Data_Type) AS TypeName,");
            sbSql.AppendLine("       LOWER(cs.Data_Type) AS DataTypeName, LOWER(cs.Data_Type) AS DataType, cs.Character_Maximum_Length AS ColumnSize,");
            sbSql.AppendLine("       cs.Numeric_Scale AS NumericScale, cs.Numeric_Precision AS NumericPrecision");
            sbSql.AppendLine("  FROM pg_catalog.pg_tables ts, Information_Schema.Columns cs");
            sbSql.AppendLine(" WHERE ts.SchemaName <> 'pg_catalog'");
            sbSql.AppendLine("   AND ts.SchemaName <> 'information_schema'");
            sbSql.AppendLine("   AND ts.SchemaName = cs.Table_Schema");
            sbSql.AppendLine("   AND ts.TableName = cs.Table_Name");
            sbSql.AppendLine($"   AND ts.SchemaName = '{schemaNode}'");
            sbSql.AppendLine($"   AND ts.TableName = '{schemaName}'");
            sbSql.Append(" ORDER BY cs.Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildViewColumnInfoProbeSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Retrieve the first record of the View to obtain its field information");

            sbSql.Append($"SELECT * FROM {schemaNode}.{schemaName} WHERE 1 = 2");

            return sbSql.ToString();
        }

        public static string BuildColumnInfoIncludeDefaultValue(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Info (Include Default Value)");

            sbSql.AppendLine("SELECT ts.schemaname AS \"SchemaName\",");
            sbSql.AppendLine("       'Tables' AS \"SchemaType\",");
            sbSql.AppendLine("       ts.tablename AS \"TableName\",");
            sbSql.AppendLine("       cs.column_name AS \"ColumnName\",");
            sbSql.AppendLine("       a.attnum AS \"ColumnID\",");
            sbSql.AppendLine("       SUBSTR(cs.is_nullable, 1, 1) AS \"Nullable\",");
            sbSql.AppendLine("       cs.column_default AS \"DefaultValue\",");
            sbSql.AppendLine("       LOWER(cs.data_type) AS \"DataType\",");
            sbSql.AppendLine("       cs.character_maximum_length AS \"DataLength\",");
            sbSql.AppendLine("       cs.numeric_precision || ',' || cs.numeric_scale AS \"Scale\",");
            sbSql.AppendLine("       pg_catalog.col_description(c.oid, a.attnum) AS \"Comments\"");
            sbSql.AppendLine("  FROM pg_catalog.pg_tables ts");
            sbSql.AppendLine("  JOIN information_schema.columns cs");
            sbSql.AppendLine("    ON cs.table_schema = ts.schemaname");
            sbSql.AppendLine("   AND cs.table_name = ts.tablename");
            sbSql.AppendLine("  JOIN pg_catalog.pg_namespace n");
            sbSql.AppendLine("    ON n.nspname = ts.schemaname");
            sbSql.AppendLine("  JOIN pg_catalog.pg_class c");
            sbSql.AppendLine("    ON c.relnamespace = n.oid");
            sbSql.AppendLine("   AND c.relname = ts.tablename");
            sbSql.AppendLine("  JOIN pg_catalog.pg_attribute a");
            sbSql.AppendLine("    ON a.attrelid = c.oid");
            sbSql.AppendLine("   AND a.attname = cs.column_name");
            sbSql.AppendLine("   AND a.attnum > 0");
            sbSql.AppendLine("   AND NOT a.attisdropped");
            sbSql.AppendLine($" WHERE ts.schemaname = '{schemaNode}'");
            sbSql.AppendLine($"   AND ts.tablename = '{schemaName}'");
            sbSql.Append(" ORDER BY a.attnum;");

            return sbSql.ToString();
        }
    }
}