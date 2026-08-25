using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Trigger
{
    internal static class OracleTriggerSqlBuilder
    {
        public static string BuildGetTriggerInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Information");

            sbSql.AppendLine("SELECT Object_Name AS TriggerName, Status, Created");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'TRIGGER'");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUppercase}'");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }
    }
}
