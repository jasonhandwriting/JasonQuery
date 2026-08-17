using JasonQuery.Core.Data;
using System;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript.Index
{
    internal static class OracleIndexCreateScriptSqlBuilder
    {
        public static string BuildGetCreateScriptSql(string ownerName, string indexName)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
            {
                throw new ArgumentNullException(nameof(ownerName));
            }

            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Index Create Script");

            sbSql.AppendLine("SELECT DBMS_METADATA.GET_DDL('INDEX', i.Index_Name, i.Owner) AS Scripts");
            sbSql.AppendLine("  FROM All_Indexes i");
            sbSql.AppendLine($" WHERE i.Owner = '{EscapeSqlLiteral(ownerName)}'");
            sbSql.AppendLine($"   AND i.Index_Name = '{EscapeSqlLiteral(indexName)}'");
            sbSql.AppendLine("   AND i.Index_Type <> 'LOB'");
            sbSql.Append("   AND ROWNUM = 1");

            return sbSql.ToString();
        }

        private static string EscapeSqlLiteral(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}