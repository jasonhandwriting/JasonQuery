using JasonQuery.Core.Data;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Procedure
{
    internal static class MySqlProcedureMetadataSqlBuilder
    {
        public static string Build(string databaseName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Information");

            sbSql.AppendLine("SELECT Routine_Schema AS DbName, Routine_Name AS ProcedureName,");
            sbSql.AppendLine("       Routine_Type AS Type, Routine_Definition AS Definition");
            sbSql.AppendLine("  FROM Information_Schema.Routines");
            sbSql.AppendLine($" WHERE Routine_Schema = {databaseName}");
            sbSql.AppendLine("   AND Routine_Type = 'PROCEDURE'");
            sbSql.Append(" ORDER BY Routine_Schema, Routine_Name");

            return sbSql.ToString();
        }
    }
}