using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Procedure
{
    internal static class OracleProcedureSqlBuilder
    {
        public static string BuildGetProcedureInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Information");

            sbSql.AppendLine("SELECT Object_Name AS ProcedureName, Object_Type AS ObjectType, Status");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'PROCEDURE'");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUppercase}'");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }
    }
}
