using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Index
{
    internal sealed class SqlServerIndexMetadataSqlBuilder
    {
        public string Build(string databaseName, string excludeIsMsShippedClause)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Index Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName, SCHEMA_NAME(t.Schema_ID) AS Schema_Dbo, i.Name, t.Name AS tblname, u.Name AS uname, i.Object_ID,");
            sbSql.AppendLine("       g.Name AS GroupName, i.Is_Padded, s.No_Recompute, i.Ignore_Dup_Key, i.Allow_Row_Locks,");
            sbSql.AppendLine("       i.Allow_Page_Locks, t.Create_Date, t.Modify_Date");
            sbSql.AppendLine($"  FROM {databaseName}.sys.Indexes i");
            sbSql.AppendLine($"       LEFT OUTER JOIN {databaseName}.sys.Data_Spaces g ON i.Data_Space_ID = g.Data_Space_ID");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Objects t ON t.Object_ID = i.Object_ID");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Schemas u ON t.Schema_ID = u.Schema_ID");
            sbSql.AppendLine($"       LEFT OUTER JOIN {databaseName}.sys.Stats s ON s.Object_ID = i.Object_ID AND s.Stats_ID = i.Index_ID");
            sbSql.AppendLine(" WHERE i.Index_ID > 0");
            sbSql.AppendLine("   AND t.Type = 'U'");
            sbSql.AppendLine($"   AND u.Name = 'dbo'{excludeIsMsShippedClause}");
            sbSql.Append(" ORDER BY i.Name, i.Index_ID;");

            return sbSql.ToString();
        }
    }
}