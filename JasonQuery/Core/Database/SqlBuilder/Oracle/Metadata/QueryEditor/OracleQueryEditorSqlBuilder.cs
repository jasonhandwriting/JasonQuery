using JasonQuery.Core.Data;
using JasonQuery.Core.Database.QueryEditor;
using System;
using System.Text;

namespace JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor
{
    internal static class OracleQueryEditorSqlBuilder
    {
        public static string BuildTableAndViewNameSql(string ownerUppercase)
        {
            var safeOwner = EscapeOwnerLiteral(ownerUppercase);

            if (string.IsNullOrEmpty(safeOwner))
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table and View Information");

            sbSql.AppendLine($"SELECT '' AS SchemaNode, '{QueryEditorSchemaTypeNames.Table}' AS SchemaType, Table_Name AS SchemaName, Owner AS Schema");
            sbSql.AppendLine("  FROM All_Tables");
            sbSql.AppendLine($" WHERE NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('{safeOwner}', 'NLS_SORT=BINARY')");
            sbSql.AppendLine(" UNION ALL");
            sbSql.AppendLine($"SELECT '' AS SchemaNode, '{QueryEditorSchemaTypeNames.View}' AS SchemaType, View_Name AS SchemaName, Owner AS Schema");
            sbSql.AppendLine("  FROM All_Views");
            sbSql.AppendLine($" WHERE NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('{safeOwner}', 'NLS_SORT=BINARY')");
            sbSql.AppendLine(" ORDER BY SchemaType, SchemaName");

            return sbSql.ToString();
        }

        public static string BuildTableInfo(string schemaName)
        {
            var safeOwner = EscapeOwnerLiteral(schemaName);

            if (string.IsNullOrEmpty(safeOwner))
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Information");

            sbSql.AppendLine("SELECT Object_Name AS SchemaName");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'TABLE'");
            sbSql.AppendLine($"   AND NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('{safeOwner}', 'NLS_SORT=BINARY')");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }

        public static string BuildViewInfo(string schemaName)
        {
            var safeOwner = EscapeOwnerLiteral(schemaName);

            if (string.IsNullOrEmpty(safeOwner))
            {
                return string.Empty;
            }

            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get View Information");

            sbSql.AppendLine("SELECT Object_Name AS SchemaName");
            sbSql.AppendLine("  FROM All_Objects");
            sbSql.AppendLine(" WHERE Object_Type = 'VIEW'");
            sbSql.AppendLine($"   AND NLSSORT(Owner, 'NLS_SORT=BINARY') = NLSSORT('{safeOwner}', 'NLS_SORT=BINARY')");
            sbSql.Append(" ORDER BY Object_Name");

            return sbSql.ToString();
        }

        private static string EscapeOwnerLiteral(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                return string.Empty;
            }

            foreach (var value in owner)
            {
                if (char.IsControl(value))
                {
                    return string.Empty;
                }
            }

            return owner.Replace("'", "''");
        }
    }
}
