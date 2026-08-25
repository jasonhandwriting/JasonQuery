using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Function
{
    internal static class OracleFunctionSqlBuilder
    {
        public static string BuildGetFunctionInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Function Information");

            sbSql.AppendLine("SELECT Object_Name AS FunctionName, Status, Created");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'FUNCTION'");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUppercase}'");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }
    }
}
