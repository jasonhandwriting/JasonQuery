using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal sealed class SqlServerTableRowCountSqlBuilder
    {
        public string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Row Count Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName, SCHEMA_NAME(t.Schema_ID) AS SchemaName, t.Name AS TableName, SUM(p.Rows) AS Rows");
            sbSql.AppendLine($"  FROM {databaseName}.sys.Tables t");
            sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.Partitions p ON t.Object_ID = p.Object_ID");
            sbSql.AppendLine(" WHERE p.Index_ID <= 1");
            sbSql.AppendLine("   AND t.Is_Ms_Shipped = 0");
            sbSql.AppendLine(" GROUP BY SCHEMA_NAME(t.Schema_ID), t.Name;");

            return sbSql.ToString();
        }
    }
}