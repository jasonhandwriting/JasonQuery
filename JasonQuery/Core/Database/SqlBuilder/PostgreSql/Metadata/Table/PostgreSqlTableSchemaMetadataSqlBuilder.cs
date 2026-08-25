using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table
{
    internal static class PostgreSqlTableSchemaMetadataSqlBuilder
    {
        public static string Build(bool sortByColumnName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Schema Information");

            sbSql.AppendLine("WITH table_list AS");
            sbSql.AppendLine("(");
            sbSql.AppendLine("    SELECT n.nspname AS DbName,");
            sbSql.AppendLine("           c.oid AS TableOid,");
            sbSql.AppendLine("           c.relname AS TableName,");
            sbSql.AppendLine("           COUNT(*) OVER (PARTITION BY n.nspname) AS TableCountInSchema,");
            sbSql.AppendLine("           GREATEST(c.reltuples, 0)::bigint AS RowCount");
            sbSql.AppendLine("      FROM pg_catalog.pg_class c");
            sbSql.AppendLine("           JOIN pg_catalog.pg_namespace n");
            sbSql.AppendLine("             ON n.oid = c.relnamespace");
            sbSql.AppendLine("     WHERE c.relkind = 'r'");
            sbSql.AppendLine("       AND n.nspname NOT IN ('pg_catalog', 'information_schema')");
            sbSql.AppendLine(")");
            sbSql.AppendLine("SELECT t.DbName,");
            sbSql.AppendLine("       t.TableName,");
            sbSql.AppendLine("       t.TableCountInSchema,");
            sbSql.AppendLine("       t.RowCount,");
            sbSql.AppendLine("       a.attname AS ColumnName,");
            sbSql.AppendLine("       pg_catalog.format_type(a.atttypid, a.atttypmod) AS ColumnType,");
            sbSql.AppendLine("       LOWER(");
            sbSql.AppendLine("           CASE COALESCE(bt.typname, tp.typname)");
            sbSql.AppendLine("                WHEN 'bpchar' THEN 'character'");
            sbSql.AppendLine("                WHEN 'varchar' THEN 'character varying'");
            sbSql.AppendLine("                WHEN 'int8' THEN 'bigint'");
            sbSql.AppendLine("                WHEN 'int4' THEN 'integer'");
            sbSql.AppendLine("                WHEN 'int2' THEN 'smallint'");
            sbSql.AppendLine("                WHEN 'bool' THEN 'boolean'");
            sbSql.AppendLine("                WHEN 'float4' THEN 'real'");
            sbSql.AppendLine("                WHEN 'float8' THEN 'double precision'");
            sbSql.AppendLine("                WHEN 'timetz' THEN 'time with time zone'");
            sbSql.AppendLine("                WHEN 'timestamp' THEN 'timestamp without time zone'");
            sbSql.AppendLine("                WHEN 'timestamptz' THEN 'timestamp with time zone'");
            sbSql.AppendLine("                WHEN 'varbit' THEN 'bit varying'");
            sbSql.AppendLine("                ELSE COALESCE(bt.typname, tp.typname)");
            sbSql.AppendLine("           END");
            sbSql.AppendLine("       ) AS DataType,");
            sbSql.AppendLine("       CASE");
            sbSql.AppendLine("            WHEN COALESCE(bt.typname, tp.typname) IN ('bpchar', 'varchar')");
            sbSql.AppendLine("             AND a.atttypmod > 4");
            sbSql.AppendLine("            THEN (a.atttypmod - 4)::text");
            sbSql.AppendLine("            ELSE NULL");
            sbSql.AppendLine("       END AS DataLength,");
            sbSql.AppendLine("       CASE");
            sbSql.AppendLine("            WHEN COALESCE(bt.typname, tp.typname) = 'numeric'");
            sbSql.AppendLine("             AND a.atttypmod > 0");
            sbSql.AppendLine("            THEN (((a.atttypmod - 4) >> 16) & 65535)::text");
            sbSql.AppendLine("                 || ',' ||");
            sbSql.AppendLine("                 ((a.atttypmod - 4) & 65535)::text");
            sbSql.AppendLine("            ELSE NULL");
            sbSql.AppendLine("       END AS Scale");
            sbSql.AppendLine("  FROM table_list t");
            sbSql.AppendLine("       JOIN pg_catalog.pg_attribute a");
            sbSql.AppendLine("         ON a.attrelid = t.TableOid");
            sbSql.AppendLine("       JOIN pg_catalog.pg_type tp");
            sbSql.AppendLine("         ON tp.oid = a.atttypid");
            sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_type bt");
            sbSql.AppendLine("         ON tp.typtype = 'd'");
            sbSql.AppendLine("        AND bt.oid = tp.typbasetype");
            sbSql.AppendLine(" WHERE a.attnum > 0");
            sbSql.AppendLine("   AND NOT a.attisdropped");

            if (sortByColumnName)
            {
                sbSql.Append(" ORDER BY t.DbName, t.TableName, a.attname");
            }
            else
            {
                sbSql.Append(" ORDER BY t.DbName, t.TableName, a.attnum");
            }

            return sbSql.ToString();
        }
    }
}
