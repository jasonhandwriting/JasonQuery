using JasonQuery.Core.Data;
using JasonQuery.Core.Database.CreateScript.SqlServer;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.CreateScript.Index
{
    internal static class SqlServerIndexCreateScriptSqlBuilder
    {
        public static string BuildIndexObjectIdSql(SqlServerCreateScriptRequest request, string tableName, string indexName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, $"---Get Index Object ID");

            sbSql.AppendLine("SELECT ix.Object_ID AS ObjectID");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes ix");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Tables tab ON ix.Object_ID = tab.Object_ID");
            sbSql.AppendLine($" WHERE tab.Name = '{tableName}'");
            sbSql.Append($"   AND ix.Name = '{indexName}'");

            return sbSql.ToString();
        }

        public static string BuildSingleIndexSummarySql(SqlServerCreateScriptRequest request, string tableName, string indexName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, $"---Get Index Summary Info");

            sbSql.AppendLine("SELECT i.Name, t.Name AS tblname, i.Index_ID, i.Type_Desc, u.Name AS uname, i.Object_ID,");
            sbSql.AppendLine("       g.Name AS GroupName, i.Is_Padded, s.No_Recompute, i.Ignore_Dup_Key, i.Allow_Row_Locks,");
            sbSql.AppendLine("       i.Allow_Page_Locks, t.Create_Date, t.Modify_Date");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       LEFT OUTER JOIN {request.SchemaNode}.sys.Data_Spaces g ON i.Data_Space_ID = g.Data_Space_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Objects t ON t.Object_ID = i.Object_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Schemas u ON t.Schema_ID = u.Schema_ID");
            sbSql.AppendLine($"       LEFT OUTER JOIN {request.SchemaNode}.sys.Stats s ON s.Object_ID = i.Object_ID AND s.Stats_ID = i.Index_ID");
            sbSql.AppendLine(" WHERE i.Index_ID > 0");
            sbSql.AppendLine("   AND t.Type = 'U'");
            sbSql.AppendLine($"   AND u.Name = '{request.SchemaDbo}'");
            sbSql.AppendLine($"   AND t.Name = '{tableName}'");
            sbSql.Append($"   AND i.Name = '{indexName}'");

            return sbSql.ToString();
        }

        public static string BuildIndexColumnSql(SqlServerCreateScriptRequest request, string objectID, string indexName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, $"---Get Index Column Info");

            sbSql.AppendLine("SELECT tab.Name Table_Name, ix.Name Index_Name,");
            sbSql.AppendLine("       col.Name Index_Column_Name, ixc.Index_Column_ID");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes ix");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Index_Columns ixc ON ix.Object_ID = ixc.Object_ID AND ix.Index_ID = ixc.Index_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Columns col ON ix.Object_ID = col.Object_ID AND ixc.Column_ID = col.Column_ID");
            sbSql.AppendLine($"       INNER JOIN {request.SchemaNode}.sys.Tables tab ON ix.Object_ID = tab.Object_ID");
            sbSql.AppendLine($" WHERE ix.Object_ID = {objectID}");
            sbSql.AppendLine($"   AND ix.Name = '{indexName}'");
            sbSql.Append(" ORDER BY ixc.Index_ID, ixc.Index_Column_ID");

            return sbSql.ToString();
        }

        public static string BuildIndexSortDirectionSql(SqlServerCreateScriptRequest request, string indexName, string indexColumnID)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, $"---Get Index Sort Direction Info");

            sbSql.AppendLine("SELECT i.Name, ic.Index_Column_ID, ic.Is_Descending_Key");
            sbSql.AppendLine($"  FROM {request.SchemaNode}.sys.Indexes i");
            sbSql.AppendLine($"       JOIN {request.SchemaNode}.sys.Index_Columns ic ON i.Index_ID = ic.Index_ID");
            sbSql.AppendLine("        AND i.Object_ID = ic.Object_ID");
            sbSql.AppendLine($"        AND i.Name = '{indexName}'");
            sbSql.Append($"        AND ic.Index_Column_ID = {indexColumnID}");

            return sbSql.ToString();
        }
    }
}