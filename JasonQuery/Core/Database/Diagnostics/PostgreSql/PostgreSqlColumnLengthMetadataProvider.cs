using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlColumnLengthMetadataProvider : IPostgreSqlColumnLengthMetadataProvider
    {
        private readonly DataTable _schemaTable;

        public PostgreSqlColumnLengthMetadataProvider(DataTable schemaTable)
        {
            _schemaTable = schemaTable;
        }

        public IReadOnlyList<PostgreSqlColumnLengthInfo> GetColumnLengthInfo(string schemaName, string tableName)
        {
            var result = new List<PostgreSqlColumnLengthInfo>();

            if (_schemaTable == null || string.IsNullOrWhiteSpace(tableName) || !HasRequiredColumns(_schemaTable))
            {
                return result;
            }

            var normalizedSchemaName = NormalizeSchemaName(schemaName);
            var normalizedTableName = NormalizeIdentifierName(tableName);
            var ordinalPosition = 0;

            foreach (DataRow row in _schemaTable.Rows)
            {
                var rowSchemaName = NormalizeSchemaName(Convert.ToString(row["SchemaNode"]));
                var rowTableName = NormalizeSchemaObjectName(Convert.ToString(row["SchemaName"]));

                if (!string.Equals(rowSchemaName, normalizedSchemaName, StringComparison.Ordinal) || !string.Equals(rowTableName, normalizedTableName, StringComparison.Ordinal))
                {
                    continue;
                }

                var schemaBrowserText = Convert.ToString(row["Schema_Browser"]);

                if (!TryParseSchemaBrowserText(schemaBrowserText, out var columnName, out var dataType, out var maxLength))
                {
                    continue;
                }

                ordinalPosition++;

                result.Add(new PostgreSqlColumnLengthInfo
                {
                    SchemaName = rowSchemaName,
                    TableName = rowTableName,
                    ColumnName = columnName,
                    DataType = dataType,
                    CharacterMaximumLength = maxLength,
                    OrdinalPosition = ordinalPosition
                });
            }

            return result;
        }

        public bool TryGetColumnLengthInfo(string schemaName, string tableName, string columnName, out PostgreSqlColumnLengthInfo columnInfo)
        {
            columnInfo = null;

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            var normalizedColumnName = NormalizeIdentifierName(columnName);
            var columns = GetColumnLengthInfo(schemaName, tableName);

            foreach (var column in columns)
            {
                if (string.Equals(column.ColumnName, normalizedColumnName, StringComparison.Ordinal))
                {
                    columnInfo = column;
                    return true;
                }
            }

            return false;
        }

        private static bool HasRequiredColumns(DataTable schemaTable)
        {
            return schemaTable.Columns.Contains("SchemaNode") && schemaTable.Columns.Contains("SchemaName") && schemaTable.Columns.Contains("Schema_Browser");
        }

        private static bool TryParseSchemaBrowserText(string schemaBrowserText, out string columnName, out string dataType, out int maxLength)
        {
            columnName = string.Empty;
            dataType = string.Empty;
            maxLength = 0;

            if (string.IsNullOrWhiteSpace(schemaBrowserText))
            {
                return false;
            }

            var commaIndex = schemaBrowserText.IndexOf(',');

            if (commaIndex <= 0 || commaIndex + 1 >= schemaBrowserText.Length)
            {
                return false;
            }

            columnName = NormalizeIdentifierName(schemaBrowserText.Substring(0, commaIndex));

            var typeText = schemaBrowserText.Substring(commaIndex + 1).Trim();

            //暫不支援 character varying(32)[] 這類 array，避免誤判 array element 的長度錯誤
            if (typeText.EndsWith("[]", StringComparison.Ordinal))
            {
                return false;
            }

            var match = Regex.Match
            (
                typeText,
                @"^(character\s+varying|varchar|character|char)\s*\((\d+)\)",
                RegexOptions.IgnoreCase
            );

            if (!match.Success)
            {
                return false;
            }

            dataType = NormalizePostgreSqlCharacterType(match.Groups[1].Value);

            if (!int.TryParse(match.Groups[2].Value, out maxLength))
            {
                return false;
            }

            return !string.IsNullOrEmpty(columnName) && maxLength > 0;
        }

        private static string NormalizePostgreSqlCharacterType(string dataType)
        {
            var value = Regex.Replace(dataType ?? string.Empty, @"\s+", " ").Trim().ToLowerInvariant();

            if (string.Equals(value, "varchar", StringComparison.OrdinalIgnoreCase))
            {
                return "character varying";
            }

            if (string.Equals(value, "char", StringComparison.OrdinalIgnoreCase))
            {
                return "character";
            }

            return value;
        }

        private static string NormalizeSchemaName(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                return "public";
            }

            return NormalizeIdentifierName(schemaName);
        }

        private static string NormalizeSchemaObjectName(string schemaObjectName)
        {
            var value = NormalizeIdentifierName(schemaObjectName);

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            //dtSchema 的 SchemaName 目前像：a_test (0)
            //這裡移除最後的筆數資訊
            var match = Regex.Match(value, @"^(?<name>.+?)\s+\(\d+\)$");

            if (match.Success)
            {
                return match.Groups["name"].Value.Trim();
            }

            return value;
        }

        private static string NormalizeIdentifierName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = value.Trim();

            if (result.Length >= 2 && result[0] == '"' && result[result.Length - 1] == '"')
            {
                result = result.Substring(1, result.Length - 2);
                result = result.Replace("\"\"", "\"");

                return result;
            }

            return result.ToLowerInvariant();
        }
    }
}