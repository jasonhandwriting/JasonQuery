using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.View
{
    internal sealed class SqlServerViewColumnMetadataSqlBuilder
    {
        public string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Column Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName,");
            sbSql.AppendLine("       SCHEMA_NAME(v.Schema_ID) AS SchemaDbo,");
            sbSql.AppendLine("       v.Name AS ViewName,");
            sbSql.AppendLine("       c.Column_ID AS OrdinalPosition,");
            sbSql.AppendLine("       c.Name AS ColumnName,");
            sbSql.AppendLine("       t.Name + CASE WHEN t.Name IN ('char', 'varchar', 'binary', 'varbinary')");
            sbSql.AppendLine("                          THEN '(' + CASE WHEN c.Max_Length = -1");
            sbSql.AppendLine("                                               THEN 'max'");
            sbSql.AppendLine("                                          ELSE CAST(c.Max_Length AS VARCHAR(5))");
            sbSql.AppendLine("                                          END + ')'");
            sbSql.AppendLine("                     WHEN t.Name IN ('nchar', 'nvarchar')");
            sbSql.AppendLine("                          THEN '(' + CASE WHEN c.Max_Length = -1");
            sbSql.AppendLine("                                               THEN 'max'");
            sbSql.AppendLine("                                          ELSE CAST(c.Max_Length / 2 AS VARCHAR(5))");
            sbSql.AppendLine("                                          END + ')'");
            sbSql.AppendLine("                     WHEN t.Name IN ('decimal', 'numeric')");
            sbSql.AppendLine("                          THEN '(' + CAST(c.Precision AS VARCHAR(5)) + ',' + CAST(c.Scale AS VARCHAR(5)) + ')'");
            sbSql.AppendLine("                     ELSE ''");
            sbSql.AppendLine("                END AS ColumnType");
            sbSql.AppendLine($"  FROM {databaseName}.sys.Views v");
            sbSql.AppendLine($"       JOIN {databaseName}.sys.Columns c ON v.Object_ID = c.Object_ID");
            sbSql.AppendLine($"       JOIN {databaseName}.sys.Types t ON c.User_Type_ID = t.User_Type_ID");
            sbSql.AppendLine(" WHERE t.Is_User_Defined = 0");
            sbSql.Append(" ORDER BY DbName, SchemaDbo, ViewName, OrdinalPosition;");

            return sbSql.ToString();
        }
    }
}