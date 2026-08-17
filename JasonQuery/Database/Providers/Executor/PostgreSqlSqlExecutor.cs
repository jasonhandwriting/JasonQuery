using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Reflection;

namespace JasonQuery.Database.Providers.Executor
{
    internal class PostgreSqlSqlExecutor
    {
        public static DataTable GetPostgreSqlCatalogTypeInfo(IEnumerable<dynamic> tables)
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            var className = currentMethod.DeclaringType.Name;
            var methodName = currentMethod.Name;

            //Composition of screening conditions ( (n.nspname = 'public' AND c.relname = 'customerinfo') OR (...) )
            var tableFilters = string.Join(" OR ", tables.Select(t => $"(n.nspname = '{t.SchemaName}' AND c.relname = '{t.TableName}')"));

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---PostgreSQL Type Catalog Resolution");

            sbSql.AppendLine("SELECT n.nspname AS SchemaName, c.relname AS TableName, a.attname AS ColumnName,");
            sbSql.AppendLine("       format_type(a.atttypid, a.atttypmod) AS TrueTypeName");
            sbSql.AppendLine("  FROM pg_attribute a");
            sbSql.AppendLine("       JOIN pg_class c ON a.attrelid = c.oid");
            sbSql.AppendLine("       JOIN pg_namespace n ON c.relnamespace = n.oid");
            sbSql.AppendLine(" WHERE a.attnum > 0 AND NOT a.attisdropped");
            sbSql.Append($"   AND ({tableFilters})");

            var sql = sbSql.ToString();
            var dtData = new DataTable();

            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtData);
            return dtData;
        }
    }
}