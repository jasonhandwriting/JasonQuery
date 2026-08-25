using JasonLibrary.Core;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.SqlBuilder.Oracle.CreateScript;
using JasonQuery.Core.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.Oracle
{
    internal static class OracleCreateTableScriptBeautifier
    {
        private sealed class OracleCreateTableColumnDefinition
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public string Other { get; set; }
        }

        public static string Beautify(string script, string schemaName, string owner, Func<string, DataTable> executeQuery)
        {
            var normalized = NormalizeRawScript(script);
            var rebuilt = RebuildTableDefinition(normalized);
            var fixedValue = FixConstraintAndCommaLayout(rebuilt);
            var aligned = AlignColumnDefinitions(fixedValue);

            return AppendCommentScripts(aligned, schemaName, owner, executeQuery);
        }

        private static string NormalizeRawScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            script = script.Replace("\r\n", "\n")
                           .Replace("\r", "\n")
                           .Trim()
                           .Replace("\t", "  ")
                           .Replace("\n", "\r\n");

            return script;
        }

        private static string RebuildTableDefinition(string script)
        {
            var temp = string.Empty;
            var sbScriptResult = new StringBuilder();
            var isConstraintBegin = false;
            var parts = script.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var prefixes1 = new[] { "PRIMARY KEY (", "UNIQUE (", "CONSTRAINT \"" };
            var prefixes2 = new[] { ") ON COMMIT", ") ORGANIZATION" };

            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = Regex.Replace(parts[i], @"TIMESTAMP\s*\((\d+)\)", "TIMESTAMP($1)");

                var textTrim = parts[i].Trim();

                if (i == 0)
                {
                    sbScriptResult.AppendLine(textTrim);
                }
                else if (i == 1)
                {
                    temp = TextHelper.CleanVarcharString(textTrim.Substring(1).Trim());
                    sbScriptResult.AppendLine("(");
                    sbScriptResult.AppendLine($"  {temp}");
                }
                else
                {
                    temp = textTrim.Replace(" NUMBER(*,0)", " INTEGER");
                    temp = TextHelper.CleanVarcharString(temp);

                    if (prefixes1.Any(prefix => temp.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        isConstraintBegin = true;
                        sbScriptResult.AppendLine($"  {temp}");
                    }
                    else
                    {
                        if (prefixes2.Any(prefix => temp.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                        {
                            sbScriptResult.AppendLine(")");
                            sbScriptResult.AppendLine($"{temp.Substring(2)};");
                            break;
                        }
                        else if (temp.StartsWith(") ", StringComparison.Ordinal))
                        {
                            sbScriptResult.AppendLine(");");
                            break;
                        }
                        else if (!isConstraintBegin)
                        {
                            sbScriptResult.AppendLine($"  {temp}");
                        }
                    }
                }
            }

            return sbScriptResult.ToString().TrimEnd('\r', '\n');
        }

        private static string FixConstraintAndCommaLayout(string script)
        {
            var parts = script.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var listLines = new List<string>();

            for (var i = 0; i < parts.Length; i++)
            {
                if (i <= 1)
                {
                    listLines.Add(parts[i]);
                }
                else if (!parts[i].StartsWith(")", StringComparison.Ordinal))
                {
                    var line = NormalizeCheckConstraintText(parts[i]);
                    var suffix = line.EndsWith(",", StringComparison.Ordinal) ? string.Empty : ",";

                    listLines.Add($"{line}{suffix}");
                }
                else
                {
                    if (listLines.Count > 0)
                    {
                        listLines[listLines.Count - 1] = listLines[listLines.Count - 1].TrimEnd(',');
                    }

                    listLines.Add(string.Empty);
                    listLines.Add(parts[i]);
                }
            }

            return string.Join("\r\n", listLines).TrimEnd('\r', '\n');
        }

        private static string NormalizeCheckConstraintText(string text)
        {
            return text.Replace(" CHECK (active in(", " CHECK (ACTIVE IN (")
                       .Replace(" CHECK (disproved in(", " CHECK (DISPROVED IN (")
                       .Replace(" CHECK (instance_id in (", " CHECK (INSTANCE_ID IN (");
        }

        private static string AlignColumnDefinitions(string script)
        {
            var parts = script.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var listColumns = new List<OracleCreateTableColumnDefinition>();
            var sbScriptResult = new StringBuilder();
            var createScriptBegin = string.Empty;
            var createScriptEnd = string.Empty;

            for (var i = 0; i < parts.Length; i++)
            {
                if (i <= 1)
                {
                    createScriptBegin += $"{parts[i]}\r\n";
                }
                else if (IsColumnDefinitionLine(parts[i]))
                {
                    listColumns.Add(ParseColumnDefinition(parts[i]));
                }
                else
                {
                    createScriptEnd += $"{parts[i]}\r\n";
                }
            }

            var listNormalizedColumns = listColumns
                                        .Select
                                         (
                                             column => new OracleCreateTableColumnDefinition
                                             {
                                                 ColumnName = column.ColumnName,
                                                 DataType = NormalizeDataTypeForDisplay(column.DataType),
                                                 Other = column.Other
                                             }
                                         )
                                        .ToList();

            var columnName = listNormalizedColumns.Count > 0 ? listNormalizedColumns.Max(x => x.ColumnName.Length) : 0;
            var dataType = listNormalizedColumns.Count > 0 ? listNormalizedColumns.Max(x => x.DataType.Length) : 0;

            foreach (var column in listNormalizedColumns)
            {
                var paddingColumn = new string(' ', Math.Max(0, columnName - column.ColumnName.Length));
                var paddingType = new string(' ', Math.Max(0, dataType - column.DataType.Length));

                sbScriptResult.Append("  ").Append(column.ColumnName).Append(paddingColumn);
                sbScriptResult.Append("  ").Append(column.DataType).Append(paddingType);

                if (!string.IsNullOrWhiteSpace(column.Other))
                {
                    sbScriptResult.Append("  ").Append(column.Other);
                }

                sbScriptResult.AppendLine();
            }

            var createScript = TrimEndSpacesEachLine(sbScriptResult.ToString());

            sbScriptResult.Clear();
            sbScriptResult.Append(createScriptBegin);
            sbScriptResult.Append(createScript);

            if (!string.IsNullOrWhiteSpace(createScriptEnd))
            {
                sbScriptResult.AppendLine();
                sbScriptResult.Append(createScriptEnd.TrimStart('\r', '\n'));
            }

            return sbScriptResult.ToString().TrimEnd('\r', '\n');
        }

        private static bool IsColumnDefinitionLine(string line)
        {
            var trim = line.Trim();

            return trim.StartsWith("\"", StringComparison.Ordinal) && !trim.StartsWith(")", StringComparison.Ordinal);
        }

        private static OracleCreateTableColumnDefinition ParseColumnDefinition(string line)
        {
            var parts = line.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var columnName = parts.Length > 0 ? parts[0] : string.Empty;
            var dataType = parts.Length > 1 ? parts[1] : string.Empty;
            var other = string.Empty;

            if (dataType.StartsWith("LONG", StringComparison.OrdinalIgnoreCase))
            {
                other = ParseLongRawOther(parts, ref dataType);
            }
            else if (dataType.StartsWith("TIMESTAMP(", StringComparison.OrdinalIgnoreCase))
            {
                other = ParseTimestampOther(parts, ref dataType);
            }
            else
            {
                other = ParseDefaultOther(parts);
            }

            other = NormalizeOtherText(other);

            return new OracleCreateTableColumnDefinition
            {
                ColumnName = columnName,
                DataType = dataType,
                Other = other
            };
        }

        private static string ParseLongRawOther(string[] parts, ref string dataType)
        {
            if (parts.Length <= 2)
            {
                return string.Empty;
            }

            var sbOther = new StringBuilder();

            if (parts[2].StartsWith("RAW", StringComparison.OrdinalIgnoreCase))
            {
                dataType += $" {parts[2]}";

                for (var j = 3; j < parts.Length; j++)
                {
                    sbOther.Append(parts[j]).Append(" ");
                }
            }
            else
            {
                for (var j = 2; j < parts.Length; j++)
                {
                    sbOther.Append(parts[j]).Append(" ");
                }
            }

            return sbOther.ToString().Trim();
        }

        private static string ParseTimestampOther(string[] parts, ref string dataType)
        {
            if (parts.Length <= 2)
            {
                return string.Empty;
            }

            var sbOther = new StringBuilder();

            if (string.Equals(parts[2], "WITH", StringComparison.OrdinalIgnoreCase))
            {
                dataType += " WITH";

                var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "LOCAL",
                    "TIME"
                };

                for (var j = 3; j < parts.Length; j++)
                {
                    if (j <= 5 && (keywords.Contains(parts[j]) || parts[j].StartsWith("ZONE", StringComparison.OrdinalIgnoreCase)))
                    {
                        dataType += $" {parts[j]}";
                    }
                    else
                    {
                        sbOther.Append(parts[j]).Append(" ");
                    }
                }
            }
            else
            {
                for (var j = 2; j < parts.Length; j++)
                {
                    sbOther.Append(parts[j]).Append(" ");
                }
            }

            return sbOther.ToString().Trim();
        }

        private static string ParseDefaultOther(string[] parts)
        {
            if (parts.Length <= 2)
            {
                return string.Empty;
            }

            var sbOther = new StringBuilder();

            for (var j = 2; j < parts.Length; j++)
            {
                sbOther.Append(parts[j]).Append(" ");
            }

            return sbOther.ToString().Trim();
        }

        private static string NormalizeOtherText(string other)
        {
            if (string.IsNullOrWhiteSpace(other))
            {
                return string.Empty;
            }

            var replacements = new Dictionary<string, string>
            {
                { "DEFAULT null", "DEFAULT NULL" },
                { "DEFAULT timestamp", "DEFAULT TIMESTAMP" },
                { "DEFAULT sysdate", "DEFAULT SYSDATE" },
                { "DEFAULT current_timestamp", "DEFAULT CURRENT_TIMESTAMP" },
                { " CONSTRAINT ", "  CONSTRAINT " },
                { " NOT NULL", "  NOT NULL" },
                { " ENABLE", "  ENABLE" }
            };

            if (other.StartsWith("DEFAULT ", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var replacement in replacements)
                {
                    other = other.Replace(replacement.Key, replacement.Value);
                }
            }
            else if (other.StartsWith("CONSTRAINT ", StringComparison.OrdinalIgnoreCase))
            {
                other = other.Replace(" NOT NULL", "  NOT NULL")
                               .Replace(" ENABLE", "  ENABLE");
            }
            else if (other.StartsWith("NOT NULL", StringComparison.OrdinalIgnoreCase))
            {
                other = other.Replace(" ENABLE", "  ENABLE");
            }

            return other;
        }

        private static string NormalizeDataTypeForDisplay(string dataType)
        {
            if (TextHelper.CheckTextStartWithAndContains(dataType, "NUMBER(", ",0)") || dataType.EndsWith(",0),", StringComparison.Ordinal))
            {
                dataType = dataType.Replace(",0)", ")");
            }

            return dataType;
        }

        private static string TrimEndSpacesEachLine(string text)
        {
            var parts = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            for (var i = 0; i < parts.Length; i++)
            {
                sb.AppendLine(parts[i].TrimEnd(' '));
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string AppendCommentScripts(string script, string schemaName, string owner, Func<string, DataTable> executeQuery)
        {
            DataTable dtTemp = null;
            var sbScriptResult = new StringBuilder();

            sbScriptResult.Append(script);

            try
            {
                var sql = OracleCreateScriptSqlBuilder.BuildGetTableCommentSql(owner, schemaName);

                dtTemp = executeQuery(sql);

                if (dtTemp?.Rows.Count > 0)
                {
                    var comment = EscapeSql(dtTemp.Rows[0].GetSafeString("Comments"));

                    sbScriptResult.AppendLine();
                    sbScriptResult.AppendLine();
                    sbScriptResult.AppendLine($"COMMENT ON TABLE \"{owner}\".\"{schemaName}\" IS '{comment}';");
                }
            }
            finally
            {
                dtTemp?.Dispose();
            }

            dtTemp = null;

            try
            {
                var sql = OracleCreateScriptSqlBuilder.BuildGetColumnCommentSql(owner, schemaName);

                dtTemp = executeQuery(sql);

                if (dtTemp?.Rows.Count > 0)
                {
                    sbScriptResult.AppendLine();

                    foreach (DataRow dr in dtTemp?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var columnName = dr.GetSafeString("Column_Name");
                        var comment = EscapeSql(dr.GetSafeString("Comments"));

                        sbScriptResult.AppendLine($"COMMENT ON COLUMN \"{owner}\".\"{schemaName}\".\"{columnName}\" IS '{comment}';");
                    }
                }
            }
            finally
            {
                dtTemp?.Dispose();
            }

            return sbScriptResult.ToString().TrimEnd('\r', '\n');
        }

        private static string EscapeSql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
