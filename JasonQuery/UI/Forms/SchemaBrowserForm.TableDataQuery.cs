using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Logging;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private string BuildTableDataPreviewSql(SchemaExplorerSelectionInfo selection, string sqlCondition)
        {
            if (selection == null)
            {
                return string.Empty;
            }

            return BuildTableDataPreviewSql
            (
                selection.SchemaNode,
                selection.SchemaDbo,
                selection.SchemaName,
                sqlCondition
            );
        }

        private string BuildTableDataPreviewSql(string schemaNode, string schemaDbo, string schemaName, string sqlCondition)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                return string.Empty;
            }

            var filterCondition = NormalizeTableDataFilterCondition(sqlCondition);
            var sbSql = new StringBuilder();

            SqlTraceHelper.AppendHeader(sbSql, "---Get the First 500 Records");

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        sbSql.AppendLine($"SELECT ROWID AS {MyGlobal.Row_Id_PK_JQ}, jq.*");
                        sbSql.AppendLine($"  FROM {schemaName} jq");
                        sbSql.Append($" WHERE {filterCondition}");
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        var tableName = BuildSchemaQualifiedTableName(schemaNode, schemaName);

                        sbSql.AppendLine($"SELECT CTID AS {MyGlobal.Row_Id_PK_JQ}, jq.*");
                        sbSql.AppendLine($"  FROM {tableName} jq");
                        sbSql.Append($" WHERE {filterCondition}");
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        var tableName = BuildSqlServerTableName(schemaDbo, schemaName);

                        sbSql.AppendLine($"SELECT RIGHT('000' + CAST(ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS VARCHAR), 3) AS {MyGlobal.Row_Id_PK_JQ}, jq.*");
                        sbSql.AppendLine($"  FROM {tableName} jq");
                        sbSql.Append($" WHERE {filterCondition}");
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        var tableName = BuildSchemaQualifiedTableName(schemaNode, schemaName);

                        sbSql.AppendLine($"SELECT LPAD(@rownum := @rownum + 1, 3, '0') AS {MyGlobal.Row_Id_PK_JQ}, jq.*");
                        sbSql.AppendLine($"  FROM {tableName} jq, (SELECT @rownum := 0) r");
                        sbSql.Append($" WHERE {filterCondition}");
                        break;
                    }
            }

            return sbSql.ToString();
        }

        private static string NormalizeTableDataFilterCondition(string sqlCondition)
        {
            return string.IsNullOrWhiteSpace(sqlCondition) ? "1 = 1" : sqlCondition;
        }

        private static string BuildSchemaQualifiedTableName(string schemaNode, string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaNode))
            {
                return schemaName;
            }

            return $"{schemaNode}.{schemaName}";
        }

        private static string BuildSqlServerTableName(string schemaDbo, string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaDbo))
            {
                return schemaName;
            }

            if (schemaName.StartsWith($"{schemaDbo}.", StringComparison.OrdinalIgnoreCase))
            {
                return schemaName;
            }

            return $"{schemaDbo}.{schemaName}";
        }
    }
}