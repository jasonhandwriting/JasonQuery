using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table
{
    internal static class PostgreSqlTableMetadataSqlBuilder
    {
        public static string Build(bool sortByColumnName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information (include ColumnType)");

            sbSql.AppendLine("SELECT ts.SchemaName AS DbName,");
            sbSql.AppendLine("       'Tables' AS SchemaType,");
            sbSql.AppendLine("       ts.TableName AS TableName,");
            sbSql.AppendLine("       cs.Column_Name AS ColumnName,");
            sbSql.AppendLine("       cs.Ordinal_Position AS ColumnID,");
            sbSql.AppendLine("       SUBSTR(cs.Is_Nullable, 1, 1) AS Nullable,");
            sbSql.AppendLine("       cs.Column_Default AS DefaultValue,");
            sbSql.AppendLine("       LOWER(cs.Data_Type) AS DataType,");
            sbSql.AppendLine("       cs.Character_Maximum_Length AS DataLength,");
            sbSql.AppendLine("       cs.Numeric_Precision || ',' || cs.Numeric_Scale AS Scale,");
            sbSql.AppendLine("       pg_catalog.FORMAT_TYPE(a.atttypid, a.atttypmod) AS ColumnType");
            sbSql.AppendLine("  FROM pg_catalog.pg_tables ts");
            sbSql.AppendLine("       JOIN Information_Schema.Columns cs");
            sbSql.AppendLine("         ON ts.SchemaName = cs.Table_Schema");
            sbSql.AppendLine("        AND ts.TableName = cs.Table_Name");
            sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_namespace n");
            sbSql.AppendLine("         ON n.nspname = ts.SchemaName");
            sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_class c");
            sbSql.AppendLine("         ON c.relnamespace = n.oid");
            sbSql.AppendLine("        AND c.relname = ts.TableName");
            sbSql.AppendLine("        AND c.relkind = 'r'");
            sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_attribute a");
            sbSql.AppendLine("         ON a.attrelid = c.oid");
            sbSql.AppendLine("        AND a.attname = cs.Column_Name");
            sbSql.AppendLine("        AND a.attnum > 0");
            sbSql.AppendLine("        AND NOT a.attisdropped");
            sbSql.AppendLine(" WHERE ts.SchemaName NOT IN ('pg_catalog', 'information_schema')");

            if (sortByColumnName)
            {
                sbSql.Append(" ORDER BY ts.SchemaName, ts.TableName, cs.Column_Name");
            }
            else
            {
                sbSql.Append(" ORDER BY ts.SchemaName, ts.TableName, cs.Ordinal_Position");
            }

            return sbSql.ToString();
        }
    }
}