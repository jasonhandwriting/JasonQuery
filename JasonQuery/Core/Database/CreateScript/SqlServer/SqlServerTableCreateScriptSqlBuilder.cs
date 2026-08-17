using JasonQuery.Core.Data;
using JasonQuery.Core.Database.CreateScript.SqlServer;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Table
{
    internal static class SqlServerTableCreateScriptSqlBuilder
    {
        public static string BuildTableObjectInfoSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Object Info");

            sbSql.AppendLine($"SELECT o.* FROM {request.SchemaNode}.sys.Objects o");
            sbSql.AppendLine(" WHERE Type = 'U'");
            sbSql.AppendLine("   AND Is_Ms_Shipped = 0");
            sbSql.AppendLine($"   AND SCHEMA_NAME(Schema_ID) = '{request.SchemaDbo}'");
            sbSql.Append($"   AND Name = '{request.SchemaName}';");

            return sbSql.ToString();
        }

        public static string BuildTableConstraintSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Constraint Info");

            sbSql.AppendLine("SELECT col.Column_Name, col.Ordinal_Position, con.Constraint_Name, con.Constraint_Type, NULL AS Check_Clause");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.Information_Schema.Key_Column_Usage AS col");
            sbSql.AppendLine($"       JOIN {request.SchemaNode}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS con");
            sbSql.AppendLine("         ON col.Constraint_Name = con.Constraint_Name");
            sbSql.AppendLine("        AND col.Table_Schema = con.Table_Schema");
            sbSql.AppendLine("        AND col.Table_Name = con.Table_Name");
            sbSql.AppendLine($" WHERE col.Table_Schema = '{request.SchemaDbo}'");
            sbSql.AppendLine($"   AND col.Table_Name = '{request.SchemaName}'");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine("SELECT NULL AS Column_Name, NULL AS Ordinal_Position, con.Constraint_Name, con.Constraint_Type, chk.Check_Clause");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.Information_Schema.Table_Constraints AS con");
            sbSql.AppendLine($"       JOIN {request.SchemaNode}.Information_Schema.Check_Constraints AS chk");
            sbSql.AppendLine("         ON con.Constraint_Name = chk.Constraint_Name");
            sbSql.AppendLine($" WHERE con.Table_Schema = '{request.SchemaDbo}'");
            sbSql.AppendLine($"   AND con.Table_Name = '{request.SchemaName}'");
            sbSql.Append(" ORDER BY Constraint_Name, Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildColumnCommentSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column Comments");

            sbSql.AppendLine("SELECT c.Name AS \"Column_Name\", prop.Value AS \"Comment\"");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Extended_Properties AS prop");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.All_Objects o ON prop.Major_ID = o.Object_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Schemas s ON o.Schema_ID = s.Schema_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Columns AS c ON prop.Major_ID = c.Object_ID AND prop.Minor_ID = c.Column_ID");
            sbSql.AppendLine(" WHERE prop.Name = 'MS_Description'");
            sbSql.AppendLine($"   AND s.Name = '{request.SchemaDbo}'");
            sbSql.Append($"   AND o.Name = '{request.SchemaName}';");

            return sbSql.ToString();
        }

        public static string BuildColumnInfoSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column Info");

            sbSql.AppendLine($"SELECT * FROM {request.SchemaNode}.Information_Schema.Columns");
            sbSql.AppendLine($" WHERE Table_Schema = '{request.SchemaDbo}'");
            sbSql.AppendLine($"   AND Table_Name = '{request.SchemaName}'");
            sbSql.Append(" ORDER BY Ordinal_Position");

            return sbSql.ToString();
        }

        public static string BuildPrimaryKeyWithSql(SqlServerCreateScriptRequest request, string constraintName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Primary Key Info");

            var optimizeForSequentialKeyColumn = request.SupportsOptimizeForSequentialKey
                                                 ? "i.optimize_for_sequential_key " + "AS Optimize_For_Sequential_Key"
                                                 : "CAST(0 AS bit) " + "AS Optimize_For_Sequential_Key";

            sbSql.AppendLine("SELECT i.Name, t.Name AS tblname, " + "u.Name AS uname, i.Object_ID, " + "g.Name AS GroupName, i.Is_Padded,");
            sbSql.AppendLine("       s.No_Recompute, i.Ignore_Dup_Key, " + "i.Allow_Row_Locks, i.Allow_Page_Locks,");
            sbSql.AppendLine($"       {optimizeForSequentialKeyColumn}");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       LEFT OUTER JOIN " + $"{request.SchemaNode}.sys.Data_Spaces g " + "ON i.Data_Space_ID = g.Data_Space_ID");
            sbSql.AppendLine($"       INNER JOIN " + $"{request.SchemaNode}.sys.Objects t " + "ON t.Object_ID = i.Object_ID");
            sbSql.AppendLine($"       INNER JOIN " + $"{request.SchemaNode}.sys.Schemas u " + "ON t.Schema_ID = u.Schema_ID");
            sbSql.AppendLine($"       LEFT OUTER JOIN " + $"{request.SchemaNode}.sys.Stats s " + "ON s.Object_ID = i.Object_ID " + "AND s.Stats_ID = i.Index_ID");
            sbSql.AppendLine(" WHERE i.Index_ID > 0");
            sbSql.AppendLine("   AND t.Type = 'U'");
            sbSql.Append($"   AND i.Name = '{constraintName}'");

            return sbSql.ToString();
        }

        public static string BuildPrimaryKeyColumnOrderSql(SqlServerCreateScriptRequest request, string constraintName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Primary Key Column Order");

            sbSql.AppendLine("SELECT i.Name, ic.Column_ID, ic.Is_Descending_Key");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       JOIN {request.SchemaNode}.sys.Index_Columns ic ON i.Index_ID = ic.Index_ID AND i.Object_ID = ic.Object_ID");
            sbSql.AppendLine($" WHERE i.Name = '{constraintName}'");
            sbSql.Append(" ORDER BY ic.Key_Ordinal");

            return sbSql.ToString();
        }

        public static string BuildDefaultConstraintSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Default Constraint Info");

            sbSql.AppendLine("SELECT dc.Name AS DefaultConstraintName,");
            sbSql.AppendLine("       t.Name AS TableName,");
            sbSql.AppendLine("       c.Name AS ColumnName,");
            sbSql.AppendLine("       dc.Definition AS DefaultValue");
            sbSql.AppendLine("  FROM sys.Default_Constraints dc");
            sbSql.AppendLine("       INNER JOIN sys.Columns c ON dc.Parent_Object_ID = c.Object_ID");
            sbSql.AppendLine("                               AND dc.Parent_Column_ID = c.Column_ID");
            sbSql.AppendLine("       INNER JOIN sys.Tables t ON dc.Parent_Object_ID = t.Object_ID");
            sbSql.AppendLine($" WHERE t.Schema_ID = SCHEMA_ID('{request.SchemaDbo}')");
            sbSql.AppendLine($"   AND t.Name = '{request.SchemaName}'");
            sbSql.Append(" ORDER BY dc.Name;");

            return sbSql.ToString();
        }

        public static string BuildForeignKeySql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Foreign Key Info");

            sbSql.AppendLine("SELECT fk.Name AS ForeignKeyName,");
            sbSql.AppendLine("       tp.Name AS ThisTable,");
            sbSql.AppendLine("       cp.Name AS ThisColumn,");
            sbSql.AppendLine("       tr.Name AS ReferencedTable,");
            sbSql.AppendLine("       cr.Name AS ReferencedColumn,");
            sbSql.AppendLine("       fk.Update_Referential_Action_Desc AS OnUpdateRule,");
            sbSql.AppendLine("       fk.Delete_Referential_Action_Desc AS OnDeleteRule");
            sbSql.AppendLine("  FROM sys.Foreign_Keys fk");
            sbSql.AppendLine("       INNER JOIN sys.Foreign_Key_Columns fkc ON fk.Object_ID = fkc.Constraint_Object_ID");
            sbSql.AppendLine("       INNER JOIN sys.Tables tp ON fkc.Parent_Object_ID = tp.Object_ID");
            sbSql.AppendLine("       INNER JOIN sys.Columns cp ON fkc.Parent_Object_ID = cp.Object_ID AND fkc.Parent_Column_ID = cp.Column_ID");
            sbSql.AppendLine("       INNER JOIN sys.Tables tr ON fkc.Referenced_Object_ID = tr.Object_ID");
            sbSql.AppendLine("       INNER JOIN sys.Columns cr ON fkc.Referenced_Object_ID = cr.Object_ID AND fkc.Referenced_Column_ID = cr.Column_ID");
            sbSql.AppendLine($" WHERE tp.Schema_ID = SCHEMA_ID('{request.SchemaDbo}')");
            sbSql.AppendLine($"   AND tp.Name = '{request.SchemaName}'");
            sbSql.Append(" ORDER BY fk.Name, fkc.Constraint_Column_ID");

            return sbSql.ToString();
        }

        public static string BuildTableCommentSql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Comment Info");

            sbSql.AppendLine("SELECT ep.Value AS Comment");
            sbSql.AppendLine("  FROM sys.Tables t");
            sbSql.AppendLine("       INNER JOIN sys.Extended_Properties ep ON t.Object_ID = ep.Major_ID");
            sbSql.AppendLine($" WHERE t.Name = '{request.SchemaName}'");
            sbSql.AppendLine("   AND ep.Class = 1");
            sbSql.Append("   AND ep.Minor_ID = 0;");

            return sbSql.ToString();
        }

        public static string BuildNonClusteredIndexSummarySql(SqlServerCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Non-Clustered Index Summary");

            sbSql.AppendLine("SELECT i.Name, t.Name AS tblname, i.Index_ID, u.Name AS uName, i.Object_ID,");
            sbSql.AppendLine("       g.Name AS GroupName, i.Is_Padded, s.No_Recompute, i.Ignore_Dup_Key,");
            sbSql.AppendLine("       i.Allow_Row_Locks, i.Allow_Page_Locks");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       LEFT OUTER JOIN {request.SchemaNode}.sys.Data_Spaces g ON i.Data_Space_ID = g.Data_Space_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Objects t ON t.Object_ID = i.Object_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Schemas u ON t.Schema_ID = u.Schema_ID");
            sbSql.AppendLine($"       LEFT OUTER JOIN {request.SchemaNode}.sys.Stats s ON s.Object_ID = i.Object_ID AND s.Stats_ID = i.Index_ID");
            sbSql.AppendLine(" WHERE i.Index_ID > 0");
            sbSql.AppendLine("   AND t.Type = 'U'");
            sbSql.AppendLine("   AND i.Type_Desc = 'NONCLUSTERED'");
            sbSql.AppendLine($"   AND u.Name = '{request.SchemaDbo}'");
            sbSql.AppendLine($"   AND t.Name = '{request.SchemaName}'");
            sbSql.Append(" ORDER BY i.Index_ID");

            return sbSql.ToString();
        }

        public static string BuildNonClusteredIndexColumnSql(SqlServerCreateScriptRequest request, string objectId, string indexName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Non-Clustered Index Column Info");

            sbSql.AppendLine("SELECT tab.Name Table_Name, ix.Name Index_Name,");
            sbSql.AppendLine("       col.Name Index_Column_Name, ixc.Index_Column_ID");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes ix");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Index_Columns ixc ON ix.Object_ID = ixc.Object_ID AND ix.Index_ID = ixc.Index_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Columns col ON ix.Object_ID = col.Object_ID AND ixc.Column_ID = col.Column_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Tables tab ON ix.Object_ID = tab.Object_ID");
            sbSql.AppendLine($" WHERE ix.Object_ID = {objectId}");
            sbSql.AppendLine("   AND ix.Type_Desc = 'NONCLUSTERED'");
            sbSql.AppendLine($"   AND ix.Name = '{indexName}'");
            sbSql.Append(" ORDER BY ixc.Index_ID, ixc.Index_Column_ID");

            return sbSql.ToString();
        }

        public static string BuildIndexSortDirectionSql(SqlServerCreateScriptRequest request, string indexName, string indexColumnId)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Index Sort Direction Info");

            sbSql.AppendLine("SELECT i.Name, ic.Index_Column_ID, ic.Is_Descending_Key");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       JOIN {request.SchemaNode}.sys.Index_Columns ic ON i.Index_ID = ic.Index_ID");
            sbSql.AppendLine("        AND i.Object_ID = ic.Object_ID");
            sbSql.AppendLine($"        AND i.Name = '{indexName}'");
            sbSql.Append($"        AND ic.Index_Column_ID = {indexColumnId}");

            return sbSql.ToString();
        }
    }
}