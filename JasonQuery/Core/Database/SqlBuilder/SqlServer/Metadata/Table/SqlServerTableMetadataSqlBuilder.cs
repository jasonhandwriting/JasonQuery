using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal sealed class SqlServerTableMetadataSqlBuilder
    {
        public string Build(string databaseName, string excludeIsMsShippedClause)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName, SCHEMA_NAME(Schema_ID) AS Schema_Dbo, o.*");
            sbSql.AppendLine($"  FROM {databaseName}.sys.All_Objects o");
            sbSql.AppendLine($" WHERE o.Type = 'U'{excludeIsMsShippedClause}");
            sbSql.Append(" ORDER BY Name;");

            return sbSql.ToString();
        }
    }
}
