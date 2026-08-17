using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Package
{
    internal static class OraclePackageSqlBuilder
    {
        public static string BuildGetPackageInfoSql(string ownerUppercase)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Package Information");

            sbSql.AppendLine("SELECT Object_Name AS PackageName, Object_Type AS ObjectType, Status");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type IN ('PACKAGE', 'PACKAGE BODY')");
            sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUppercase}'");
            sbSql.AppendLine(" ORDER BY Object_Name,");
            sbSql.Append("          CASE WHEN Object_Type = 'PACKAGE' THEN 0 ELSE 1 END");

            return sbSql.ToString();
        }
    }
}