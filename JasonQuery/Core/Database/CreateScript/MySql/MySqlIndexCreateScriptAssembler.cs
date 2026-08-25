using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using JasonQuery.Core.Data.DataRows;

namespace JasonQuery.Core.Database.CreateScript.MySql
{
    internal static class MySqlIndexCreateScriptAssembler
    {
        private sealed class IndexColumnInfo
        {
            public string ColumnName { get; set; }
            public string Collation { get; set; }
            public string SubPart { get; set; }
        }

        public static string Build(MySqlCreateScriptRequest request, Func<string, DataTable> executeQuery)
        {
            if (request == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(request.IndexName))
            {
                return "-- Index name is empty.";
            }

            if (string.Equals(request.IndexName, "PRIMARY", StringComparison.OrdinalIgnoreCase))
            {
                return "-- MySQL PRIMARY KEY is part of CREATE TABLE / ALTER TABLE, not a standalone CREATE INDEX script.";
            }

            if (string.IsNullOrWhiteSpace(request.BaseTableName))
            {
                var locateSql = MySqlCreateScriptSqlBuilder.BuildLocateIndexTableSql(request);
                var dtLocate = MySqlCreateScriptExecutionHelper.ExecuteSql(locateSql, executeQuery);

                if (dtLocate == null || dtLocate.Rows.Count <= 0)
                {
                    return $"-- Index `{request.IndexName}` not found in database `{request.SchemaNode}`.";
                }

                if (dtLocate.Rows.Count > 1)
                {
                    return $"-- Index `{request.IndexName}` exists on multiple tables in database `{request.SchemaNode}`. Please provide `IndexName on TableName`.";
                }

                request = MySqlCreateScriptRequest.Create(request.SchemaType, request.SchemaNode, $"{request.IndexName} on {dtLocate.Rows[0].GetSafeString("TABLE_NAME")}");
            }

            var metadataSql = MySqlCreateScriptSqlBuilder.BuildIndexMetadataSql(request);
            var dtIndex = MySqlCreateScriptExecutionHelper.ExecuteSql(metadataSql, executeQuery);

            if (dtIndex == null || dtIndex.Rows.Count <= 0)
            {
                return $"-- Index `{request.IndexName}` not found on table `{request.BaseTableName}`.";
            }

            var firstRow = dtIndex.Rows[0];
            var nonUnique = firstRow.GetSafeString("NON_UNIQUE");
            var indexType = firstRow.GetSafeString("INDEX_TYPE");
            var createKeyword = BuildCreateIndexKeyword(nonUnique, indexType);
            var usingClause = BuildUsingClause(indexType);
            var listColumns = new List<IndexColumnInfo>();

            foreach (DataRow dr in dtIndex.Rows)
            {
                var columnName = dr.GetSafeString("COLUMN_NAME");

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    return "-- This MySQL index appears to contain unsupported expression-based parts for the current stage-1 generator.";
                }

                listColumns.Add(new IndexColumnInfo
                {
                    ColumnName = columnName,
                    Collation = dr.GetSafeString("COLLATION"),
                    SubPart = dr.GetSafeString("SUB_PART")
                });
            }

            var sb = new StringBuilder();

            sb.AppendLine($"-- Index: `{request.SchemaNode}`.`{request.BaseTableName}`.`{request.IndexName}`");
            sb.AppendLine($"-- Generated from INFORMATION_SCHEMA.STATISTICS because MySQL does not provide SHOW CREATE INDEX.");
            sb.Append($"CREATE {createKeyword} `{request.IndexName}`");

            if (!string.IsNullOrWhiteSpace(usingClause))
            {
                sb.Append($" {usingClause}");
            }

            sb.AppendLine($" ON `{request.SchemaNode}`.`{request.BaseTableName}`");
            sb.AppendLine("(");

            for (var i = 0; i < listColumns.Count; i++)
            {
                var col = listColumns[i];
                var suffix = i == listColumns.Count - 1 ? string.Empty : ",";
                var columnSql = $"    `{col.ColumnName}`";

                if (!string.IsNullOrWhiteSpace(col.SubPart))
                {
                    columnSql += $"({col.SubPart})";
                }

                if (string.Equals(col.Collation, "D", StringComparison.OrdinalIgnoreCase))
                {
                    columnSql += " DESC";
                }

                sb.AppendLine($"{columnSql}{suffix}");
            }

            sb.Append(");");

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string BuildCreateIndexKeyword(string nonUnique, string indexType)
        {
            if (string.Equals(indexType, "FULLTEXT", StringComparison.OrdinalIgnoreCase))
            {
                return "FULLTEXT INDEX";
            }

            if (string.Equals(indexType, "SPATIAL", StringComparison.OrdinalIgnoreCase))
            {
                return "SPATIAL INDEX";
            }

            if (string.Equals(nonUnique, "0", StringComparison.OrdinalIgnoreCase))
            {
                return "UNIQUE INDEX";
            }

            return "INDEX";
        }

        private static string BuildUsingClause(string indexType)
        {
            if (string.IsNullOrWhiteSpace(indexType))
            {
                return string.Empty;
            }

            if (string.Equals(indexType, "BTREE", StringComparison.OrdinalIgnoreCase)
                || string.Equals(indexType, "HASH", StringComparison.OrdinalIgnoreCase))
            {
                return $"USING {indexType.ToUpper()}";
            }

            return string.Empty;
        }
    }
}
