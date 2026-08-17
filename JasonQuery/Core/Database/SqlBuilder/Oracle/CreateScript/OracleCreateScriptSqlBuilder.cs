using JasonQuery.Core.Data;
using JasonQuery.Core.Database.CreateScript.Oracle;
using System;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript
{
    internal static class OracleCreateScriptSqlBuilder
    {
        public static string BuildGetCreateScriptSql(string ownerName, string ddlObjectType, string objectName)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
            {
                throw new ArgumentNullException(nameof(ownerName));
            }

            if (string.IsNullOrWhiteSpace(ddlObjectType))
            {
                throw new ArgumentNullException(nameof(ddlObjectType));
            }

            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Oracle Create Script");

            sbSql.AppendLine("SELECT DBMS_METADATA.GET_DDL");
            sbSql.AppendLine("(");
            sbSql.AppendLine($"    '{EscapeSqlLiteral(ddlObjectType)}',");
            sbSql.AppendLine($"    '{EscapeSqlLiteral(objectName)}',");
            sbSql.AppendLine($"    '{EscapeSqlLiteral(ownerName)}'");
            sbSql.AppendLine(") AS Scripts");
            sbSql.Append("  FROM Dual");

            return sbSql.ToString();
        }

        public static string BuildGetObjectHeaderSql(OracleCreateScriptRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Object Creation Information");

            sbSql.AppendLine("SELECT Object_Name AS ObjectName, Status, Created");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = '{0}'");
            sbSql.AppendLine("   AND UPPER(Owner) = '{1}'");
            sbSql.Append("   AND Object_Name = '{2}'");

            return string.Format
            (
                sbSql.ToString(),
                EscapeSqlLiteral(request.MetadataObjectType),
                EscapeSqlLiteral(request.Owner),
                EscapeSqlLiteral(request.SchemaName)
            );
        }

        public static string BuildGetObjectCreateScriptSql(OracleCreateScriptRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Object Creation Script");

            sbSql.AppendLine("SELECT DBMS_METADATA.GET_DDL('{0}', '{1}', '{2}') AS ScriptText");
            sbSql.Append("  FROM Dual");

            return string.Format
            (
                sbSql.ToString(),
                EscapeSqlLiteral(request.DdlObjectType),
                EscapeSqlLiteral(request.SchemaName),
                EscapeSqlLiteral(request.Owner)
            );
        }

        public static string BuildGetTableCommentSql(string owner, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Comment Information");

            sbSql.AppendLine("SELECT Comments");
            sbSql.AppendLine("  FROM All_Tab_Comments");
            sbSql.AppendLine(" WHERE UPPER(Owner) = '{0}'");
            sbSql.AppendLine("   AND Table_Name = '{1}'");
            sbSql.Append("   AND Comments IS NOT NULL");

            return string.Format
            (
                sbSql.ToString(),
                EscapeSqlLiteral((owner ?? string.Empty).ToUpperInvariant()),
                EscapeSqlLiteral(schemaName)
            );
        }

        public static string BuildGetColumnCommentSql(string owner, string schemaName)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Column Comment Information");

            sbSql.AppendLine("SELECT cols.Column_Name, com.Comments");
            sbSql.AppendLine("  FROM All_Tab_Columns cols");
            sbSql.AppendLine("       LEFT JOIN All_Col_Comments com");
            sbSql.AppendLine("              ON cols.Owner = com.Owner");
            sbSql.AppendLine("             AND cols.Table_Name = com.Table_Name");
            sbSql.AppendLine("             AND cols.Column_Name = com.Column_Name");
            sbSql.AppendLine(" WHERE UPPER(cols.Owner) = '{0}'");
            sbSql.AppendLine("   AND cols.Table_Name = '{1}'");
            sbSql.AppendLine("   AND com.Comments IS NOT NULL");
            sbSql.Append(" ORDER BY cols.Column_ID");

            return string.Format
            (
                sbSql.ToString(),
                EscapeSqlLiteral((owner ?? string.Empty).ToUpperInvariant()),
                EscapeSqlLiteral(schemaName)
            );
        }

        private static string EscapeSqlLiteral(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}