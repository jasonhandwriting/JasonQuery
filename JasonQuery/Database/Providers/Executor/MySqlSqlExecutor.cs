using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Reflection;

namespace JasonQuery.Database.Providers.Executor
{
    internal class MySqlSqlExecutor
    {
        public static DataTable GetMySqlCatalogTypeInfo(IEnumerable<dynamic> tables)
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            var className = currentMethod.DeclaringType.Name;
            var methodName = currentMethod.Name;

            //Composition of screening conditions ( (n.nspname = 'public' AND c.relname = 'customerinfo') OR (...) )
            var tableFilters = string.Join(" OR ", tables.Select(t => $"(TABLE_SCHEMA = '{t.SchemaName}' AND TABLE_NAME = '{t.TableName}')"));

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---MySQL Type Catalog Resolution");

            sbSql.AppendLine("SELECT TABLE_SCHEMA AS SchemaName, TABLE_NAME AS TableName,");
            sbSql.AppendLine("       COLUMN_NAME AS ColumnName, COLUMN_TYPE AS TrueTypeName");
            sbSql.AppendLine("  FROM Information_Schema.Columns");
            sbSql.AppendLine(" WHERE 1 = 1");
            sbSql.Append($"   AND ({tableFilters})");

            var sql = sbSql.ToString();
            var dtData = new DataTable();

            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtData);
            return dtData;
        }
    }
}