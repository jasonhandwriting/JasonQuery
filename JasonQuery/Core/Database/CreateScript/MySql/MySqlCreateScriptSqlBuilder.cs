using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal static class MySqlCreateScriptSqlBuilder
    {
        public static string BuildShowCreateSql(MySqlCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();
            var dbName = QuoteIdentifier(request.SchemaNode);
            var objectName = QuoteIdentifier(request.SchemaName);

            switch (request.SchemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Table Creation Script");
                        sbSql.Append($"SHOW CREATE TABLE {dbName}.{objectName};");
                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get View Creation Script");
                        sbSql.Append($"SHOW CREATE VIEW {dbName}.{objectName};");
                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Function Creation Script");
                        sbSql.Append($"SHOW CREATE FUNCTION {dbName}.{objectName};");
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Creation Script");
                        sbSql.Append($"SHOW CREATE TRIGGER {dbName}.{objectName};");
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Creation Script");
                        sbSql.Append($"SHOW CREATE PROCEDURE {dbName}.{objectName};");
                        break;
                    }
            }

            return sbSql.ToString();
        }

        public static string BuildLocateIndexTableSql(MySqlCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Locate Index Base Table");

            sbSql.AppendLine("SELECT Table_Name");
            sbSql.AppendLine("  FROM Information_Schema.Statistics");
            sbSql.AppendLine($" WHERE Table_Schema = '{EscapeSqlLiteral(request.SchemaNode)}'");
            sbSql.AppendLine($"   AND Index_Name = '{EscapeSqlLiteral(request.IndexName)}'");
            sbSql.Append(" GROUP BY Table_Name");

            return sbSql.ToString();
        }

        public static string BuildIndexMetadataSql(MySqlCreateScriptRequest request)
        {
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Index Metadata");

            sbSql.AppendLine("SELECT Table_Name,");
            sbSql.AppendLine("       Index_Name,");
            sbSql.AppendLine("       Non_Unique,");
            sbSql.AppendLine("       Index_Type,");
            sbSql.AppendLine("       Seq_In_Index,");
            sbSql.AppendLine("       Column_Name,");
            sbSql.AppendLine("       Collation,");
            sbSql.AppendLine("       Sub_Part,");
            sbSql.AppendLine("       Index_Comment");
            sbSql.AppendLine("  FROM Information_Schema.Statistics");
            sbSql.AppendLine($" WHERE Table_Schema = '{EscapeSqlLiteral(request.SchemaNode)}'");
            sbSql.AppendLine($"   AND Table_Name = '{EscapeSqlLiteral(request.BaseTableName)}'");
            sbSql.AppendLine($"   AND Index_Name = '{EscapeSqlLiteral(request.IndexName)}'");
            sbSql.Append(" ORDER BY Seq_In_Index");

            return sbSql.ToString();
        }

        public static string QuoteIdentifier(string identifier)
        {
            return $"`{(identifier ?? string.Empty).Replace("`", "``")}`";
        }

        public static string EscapeSqlLiteral(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}