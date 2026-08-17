using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.PostgreSql.CreateScript
{
    internal static class PostgreSqlTableCreateScriptSqlBuilder
    {
        public static string BuildTableDefinitionSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Creation Script");

            //取得指定的 Table Create Script
            sbSql.AppendLine("SELECT FORMAT(E'CREATE %sTABLE IF NOT EXISTS %s%I\n(\n%s#$@', CASE c.relpersistence WHEN 't' THEN 'temporary ' ELSE '' END, CASE c.relpersistence WHEN 't' THEN '' ELSE n.nspname || '.' END,");
            sbSql.AppendLine("       c.relname, STRING_AGG(FORMAT(E'    %I %s%s', a.AttName, pg_catalog.FORMAT_TYPE(a.atttypid, a.atttypmod), CASE WHEN a.attnotnull THEN ' not null' ELSE '' end), E',\n' ORDER BY a.attnum)) AS Sql, c.oid");
            sbSql.AppendLine("  FROM pg_catalog.pg_class c");
            sbSql.AppendLine("       JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace");
            sbSql.AppendLine("       JOIN pg_catalog.pg_attribute a ON a.attrelid = c.oid AND a.attnum > 0");
            sbSql.AppendLine("       JOIN pg_catalog.pg_type t ON a.atttypid = t.oid");
            sbSql.AppendLine($" WHERE n.nspname = '{schemaNode}'");
            sbSql.AppendLine($"   AND c.relname = '{schemaName}'");
            sbSql.Append(" GROUP BY c.oid, c.relname, c.relpersistence, n.nspname;");

            return sbSql.ToString();
        }

        public static string BuildTableConstraintSql(string oid)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Constraint Information");

            sbSql.AppendLine("SELECT conrelid::regclass AS Table_From, conname, PG_GET_CONSTRAINTDEF(oid) AS \"info\"");
            sbSql.AppendLine("  FROM pg_constraint pp");
            sbSql.AppendLine(" WHERE pp.contype IN ('f', 'p', 'u', 'c')");
            sbSql.AppendLine($"   AND pp.conrelid = {oid}");
            sbSql.Append(" ORDER BY CASE pp.contype WHEN 'p' THEN 1 WHEN 'u' THEN 2 WHEN 'f' THEN 3 WHEN 'c' THEN 4 END");

            return sbSql.ToString();
        }

        public static string BuildTableOwnerSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Owner Information");

            //20220726 取得 Table Owner Info
            sbSql.AppendLine("SELECT TableOwner FROM pg_tables");
            sbSql.AppendLine($" WHERE SchemaName = '{schemaNode}'");
            sbSql.Append($"   AND TableName = '{schemaName}';");

            return sbSql.ToString();
        }

        public static string BuildTableGrantSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Grant Information");

            sbSql.AppendLine("SELECT REPLACE(grant_info, 'DELETE, INSERT, REFERENCES, SELECT, TRIGGER, TRUNCATE, UPDATE', 'ALL') AS Grant_Info, Owner_Info");
            sbSql.AppendLine("  FROM (SELECT FORMAT('GRANT %s ON TABLE %I.%I TO %I%s;', STRING_AGG(Privilege_Type, ', '), Table_Schema, Table_Name, Grantee,");
            sbSql.AppendLine("               CASE ");
            sbSql.AppendLine("                   WHEN Is_Grantable = 'YES'");
            sbSql.AppendLine("                   THEN ' WITH GRANT OPTION'");
            sbSql.AppendLine("                   ELSE ''");
            sbSql.AppendLine("               END) AS Grant_Info, Owner_Info");
            sbSql.AppendLine("          FROM (SELECT tg.Table_Schema, tg.Table_Name, tg.Grantee, tg.Privilege_Type, tg.Is_Grantable, t.TableOwner AS Owner_Info");
            sbSql.AppendLine("                  FROM Information_Schema.Role_Table_Grants tg");
            sbSql.AppendLine("                       JOIN pg_tables t ON t.SchemaName = tg.Table_Schema AND t.TableName = tg.Table_Name");
            sbSql.AppendLine($"                 WHERE tg.Table_Schema = '{schemaNode}'");
            sbSql.AppendLine($"                   AND tg.Table_Name = '{schemaName}'");
            sbSql.AppendLine("                 ORDER BY tg.Grantee, tg.Privilege_Type");
            sbSql.AppendLine("               ) ss");
            sbSql.AppendLine(" GROUP BY Table_Schema, Table_Name, Grantee, Is_Grantable, Owner_Info");
            sbSql.Append(" ORDER BY Grant_Info) tt;");

            return sbSql.ToString();
        }

        public static string BuildTableNonUniqueIndexSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Non-Unique Index Information");

            sbSql.AppendLine("SELECT t.relname AS TableName, i.relname AS IndexName, am.amname AS IndexType,");
            sbSql.AppendLine("       PG_GET_INDEXDEF(ix.indexrelid) AS IndexDefinition");
            sbSql.AppendLine("  FROM pg_index ix");
            sbSql.AppendLine("       JOIN pg_class t ON t.oid = ix.indrelid");
            sbSql.AppendLine("       JOIN pg_class i ON i.oid = ix.indexrelid");
            sbSql.AppendLine("       JOIN pg_am am ON i.relam = am.oid");
            sbSql.AppendLine("       JOIN pg_namespace n ON n.oid = t.relnamespace");
            sbSql.AppendLine($" WHERE n.nspname = '{schemaNode}'");
            sbSql.AppendLine($"   AND t.relname = '{schemaName}'");
            sbSql.AppendLine("   AND ix.indisunique = false --true:UNIQUE, false:NON-UNIQUE");
            sbSql.Append(" ORDER BY t.relname, i.relname;");

            return sbSql.ToString();
        }

        public static string BuildTableCommentSql(string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Comment Information");

            //20220726 取得 Table Comment Info
            sbSql.Append($"SELECT OBJ_DESCRIPTION('{schemaNode}.{schemaName}'::regclass);");

            return sbSql.ToString();
        }

        public static string BuildColumnCommentSql(string databaseName, string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comment Information");

            //20220726 取得 Column Comment Info
            sbSql.AppendLine("SELECT c.Column_Name, d.Description");
            sbSql.AppendLine("  FROM Information_Schema.Columns c");
            sbSql.AppendLine("       INNER JOIN pg_class c1 ON c.Table_Name = c1.relname");
            sbSql.AppendLine("       INNER JOIN pg_catalog.pg_namespace n ON c.Table_Schema = n.nspname AND c1.relnamespace = n.oid");
            sbSql.AppendLine("       LEFT JOIN pg_catalog.pg_description d ON d.objsubid = c.Ordinal_Position AND d.objoid = c1.oid");
            sbSql.AppendLine($" WHERE c.Table_Catalog = '{databaseName}'");
            sbSql.AppendLine($"   AND c.Table_Name = '{schemaName}'");
            sbSql.AppendLine($"   AND c.Table_Schema = '{schemaNode}'");
            sbSql.Append("   AND d.Description IS NOT NULL");

            return sbSql.ToString();
        }

        public static string BuildTableTriggerSql(string databaseName, string schemaNode, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Trigger Information");

            sbSql.AppendLine("SELECT Trigger_Name, Action_Timing, STRING_AGG(Event_Manipulation,' OR ') AS Event_Manipulation,");
            sbSql.AppendLine("       Action_Orientation, Action_Statement");
            sbSql.AppendLine("  FROM Information_Schema.Triggers ss");
            sbSql.AppendLine($" WHERE Trigger_Catalog = '{databaseName}'");
            sbSql.AppendLine($"   AND Trigger_Schema = '{schemaNode}'");
            sbSql.AppendLine($"   AND Event_Object_Table = '{schemaName}'");
            sbSql.AppendLine(" GROUP BY Trigger_Name, Action_Timing, Action_Orientation, Action_Statement");
            sbSql.Append(" ORDER BY Trigger_Name, Event_Manipulation");

            return sbSql.ToString();
        }
    }
}