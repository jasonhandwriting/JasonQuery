using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table
{
    internal sealed class SqlServerTableColumnMetadataSqlBuilder
    {
        public string Build(string databaseName, string sortByColumnName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Column Information");

            sbSql.AppendLine($"SELECT '{databaseName}' AS DbName, o.Table_Schema AS SchemaDbo, o.*");
            sbSql.AppendLine($"  FROM {databaseName}.Information_Schema.Columns o");
            sbSql.AppendLine($" WHERE o.Table_Catalog = '{databaseName}'");
            sbSql.Append($" ORDER BY o.Table_Name, {sortByColumnName};");

            return sbSql.ToString();
        }
    }
}
