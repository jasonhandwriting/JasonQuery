using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlTableCreateScriptFormatter : MySqlCreateScriptFormatterBase
    {
        private sealed class TableElement
        {
            public bool IsColumn { get; set; }
            public string OriginalText { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public string CharacterSetClause { get; set; }
            public string CollateClause { get; set; }
            public string GeneratedClause { get; set; }
            public string NullabilityClause { get; set; }
            public string DefaultClause { get; set; }
            public string OnUpdateClause { get; set; }
            public string AutoIncrementClause { get; set; }
            public string CommentClause { get; set; }
            public string KeyClause { get; set; }
            public string ReferenceClause { get; set; }
            public string ColumnFormatClause { get; set; }
            public string StorageClause { get; set; }
            public string SridClause { get; set; }
            public string ExtraClause { get; set; }
        }

        private sealed class ClauseMarker
        {
            public int TokenIndex { get; set; }
            public int StartIndex { get; set; }
            public string Kind { get; set; }
        }

        private sealed class ColumnFormatWidths
        {
            public int ColumnNameWidth { get; set; }
            public int DataTypeWidth { get; set; }
            public int CharacterSetWidth { get; set; }
            public int CollateWidth { get; set; }
            public int GeneratedWidth { get; set; }
            public int NullabilityWidth { get; set; }
            public int DefaultWidth { get; set; }
            public int OnUpdateWidth { get; set; }
            public int AutoIncrementWidth { get; set; }
            public int CommentWidth { get; set; }
        }

        protected override bool MatchesSchemaType(string schemaType)
        {
            return string.Equals(schemaType, SchemaObjectNames.Tables, StringComparison.Ordinal);
        }

        protected override string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);

            if (string.IsNullOrWhiteSpace(script) || !script.StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
            {
                return script;
            }

            var openParenIndex = MySqlCreateScriptFormatterHelper.FindFirstTopLevelChar(script, '(');

            if (openParenIndex < 0)
            {
                return script;
            }

            var closeParenIndex = MySqlCreateScriptFormatterHelper.FindMatchingParen(script, openParenIndex);

            if (closeParenIndex < 0)
            {
                return script;
            }

            var header = script.Substring(0, openParenIndex).TrimEnd();
            var bodyText = script.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var tail = script.Substring(closeParenIndex + 1).Trim();
            var listElements = BuildTableElements(bodyText);

            if (listElements.Count <= 0)
            {
                return script;
            }

            var widths = BuildColumnFormatWidths(listElements);
            var sb = new StringBuilder();

            sb.AppendLine(header);
            sb.AppendLine("(");

            for (var i = 0; i < listElements.Count; i++)
            {
                var element = listElements[i];
                var suffix = i == listElements.Count - 1 ? string.Empty : ",";

                if (element.IsColumn)
                {
                    sb.AppendLine($"{BuildFormattedColumnLine(element, widths)}{suffix}");
                }
                else
                {
                    sb.AppendLine($"{BuildFormattedNonColumnLine(element.OriginalText)}{suffix}");
                }
            }

            sb.Append(")");

            if (!string.IsNullOrWhiteSpace(tail))
            {
                sb.Append(" ").Append(tail);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #region Build Elements
        private static List<TableElement> BuildTableElements(string bodyText)
        {
            var list = new List<TableElement>();
            var items = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(bodyText);

            foreach (var item in items)
            {
                var text = (item ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (TryParseColumnDefinition(text, out var element))
                {
                    list.Add(element);
                }
                else
                {
                    list.Add(new TableElement
                    {
                        IsColumn = false,
                        OriginalText = text
                    });
                }
            }

            return list;
        }

        private static bool TryParseColumnDefinition(string text, out TableElement element)
        {
            element = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (text.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("UNIQUE INDEX", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("KEY ", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("INDEX ", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("FULLTEXT KEY", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("SPATIAL KEY", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("CONSTRAINT ", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("CHECK ", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var columnName = ExtractMySqlIdentifier(text);

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            var remain = text.Substring(columnName.Length).TrimStart();

            if (string.IsNullOrWhiteSpace(remain))
            {
                return false;
            }

            SplitDataTypeAndTail(remain, out var dataType, out var tailText);

            if (string.IsNullOrWhiteSpace(dataType))
            {
                return false;
            }

            element = new TableElement
            {
                IsColumn = true,
                ColumnName = columnName.Trim(),
                DataType = dataType.Trim(),
                OriginalText = text
            };

            ParseColumnTail(tailText, element);

            return true;
        }

        private static string ExtractMySqlIdentifier(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text[0] == '`')
            {
                for (var i = 1; i < text.Length; i++)
                {
                    if (text[i] == '`')
                    {
                        return text.Substring(0, i + 1);
                    }
                }

                return string.Empty;
            }

            var pos = text.IndexOf(' ');

            return pos < 0 ? text : text.Substring(0, pos);
        }

        private static void SplitDataTypeAndTail(string remain, out string dataType, out string tailText)
        {
            dataType = string.Empty;
            tailText = string.Empty;

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(remain);

            if (tokens.Count <= 0)
            {
                return;
            }

            var attrStartIndex = -1;

            for (var i = 0; i < tokens.Count; i++)
            {
                if (IsColumnAttributeStart(tokens, i))
                {
                    attrStartIndex = tokens[i].StartIndex;
                    break;
                }
            }

            if (attrStartIndex < 0)
            {
                dataType = remain.Trim();
                return;
            }

            dataType = remain.Substring(0, attrStartIndex).TrimEnd();
            tailText = remain.Substring(attrStartIndex).Trim();
        }

        private static bool IsColumnAttributeStart(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, int index)
        {
            if (tokens == null || index < 0 || index >= tokens.Count)
            {
                return false;
            }

            return MatchesPhrase(tokens, index, "CHARACTER", "SET")
                   || MatchesPhrase(tokens, index, "COLLATE")
                   || MatchesPhrase(tokens, index, "GENERATED", "ALWAYS", "AS")
                   || MatchesPhrase(tokens, index, "GENERATED")
                   || MatchesPhrase(tokens, index, "NOT", "NULL")
                   || MatchesPhrase(tokens, index, "NULL")
                   || MatchesPhrase(tokens, index, "DEFAULT")
                   || MatchesPhrase(tokens, index, "ON", "UPDATE")
                   || MatchesPhrase(tokens, index, "AUTO_INCREMENT")
                   || MatchesPhrase(tokens, index, "COMMENT")
                   || MatchesPhrase(tokens, index, "PRIMARY", "KEY")
                   || MatchesPhrase(tokens, index, "UNIQUE", "KEY")
                   || MatchesPhrase(tokens, index, "UNIQUE")
                   || MatchesPhrase(tokens, index, "REFERENCES")
                   || MatchesPhrase(tokens, index, "COLUMN_FORMAT")
                   || MatchesPhrase(tokens, index, "STORAGE")
                   || MatchesPhrase(tokens, index, "SRID");
        }

        private static void ParseColumnTail(string tailText, TableElement element)
        {
            if (element == null)
            {
                return;
            }

            tailText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(tailText);

            if (string.IsNullOrWhiteSpace(tailText))
            {
                return;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(tailText);

            if (tokens.Count <= 0)
            {
                element.ExtraClause = tailText;
                return;
            }

            var markers = new List<ClauseMarker>();

            for (var i = 0; i < tokens.Count; i++)
            {
                if (MatchesPhrase(tokens, i, "CHARACTER", "SET"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "CharacterSet"
                    });

                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "COLLATE"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Collate"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "GENERATED", "ALWAYS", "AS"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Generated"
                    });

                    i += 2;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "GENERATED"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Generated"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "NOT", "NULL"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Nullability"
                    });

                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "NULL"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Nullability"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "DEFAULT"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Default"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "ON", "UPDATE"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "OnUpdate"
                    });

                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "AUTO_INCREMENT"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "AutoIncrement"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "COMMENT"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Comment"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "PRIMARY", "KEY"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Key"
                    });

                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "UNIQUE", "KEY"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Key"
                    });

                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "UNIQUE"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Key"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "REFERENCES"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "References"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "COLUMN_FORMAT"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "ColumnFormat"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "STORAGE"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Storage"
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "SRID"))
                {
                    markers.Add(new ClauseMarker
                    {
                        TokenIndex = i,
                        StartIndex = tokens[i].StartIndex,
                        Kind = "Srid"
                    });
                }
            }

            if (markers.Count <= 0)
            {
                element.ExtraClause = tailText;
                return;
            }

            markers = markers.OrderBy(x => x.StartIndex).ToList();

            for (var i = 0; i < markers.Count; i++)
            {
                var start = markers[i].StartIndex;
                var end = i == markers.Count - 1 ? tailText.Length : markers[i + 1].StartIndex;
                var clause = tailText.Substring(start, end - start).Trim();

                if (string.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                switch (markers[i].Kind)
                {
                    case "CharacterSet":
                        {
                            element.CharacterSetClause = AppendClause(element.CharacterSetClause, clause);
                            break;
                        }
                    case "Collate":
                        {
                            element.CollateClause = AppendClause(element.CollateClause, clause);
                            break;
                        }
                    case "Generated":
                        {
                            element.GeneratedClause = AppendClause(element.GeneratedClause, clause);
                            break;
                        }
                    case "Nullability":
                        {
                            element.NullabilityClause = AppendClause(element.NullabilityClause, clause);
                            break;
                        }
                    case "Default":
                        {
                            element.DefaultClause = AppendClause(element.DefaultClause, clause);
                            break;
                        }
                    case "OnUpdate":
                        {
                            element.OnUpdateClause = AppendClause(element.OnUpdateClause, clause);
                            break;
                        }
                    case "AutoIncrement":
                        {
                            element.AutoIncrementClause = AppendClause(element.AutoIncrementClause, clause);
                            break;
                        }
                    case "Comment":
                        {
                            element.CommentClause = AppendClause(element.CommentClause, clause);
                            break;
                        }
                    case "Key":
                        {
                            element.KeyClause = AppendClause(element.KeyClause, clause);
                            break;
                        }
                    case "References":
                        {
                            element.ReferenceClause = AppendClause(element.ReferenceClause, clause);
                            break;
                        }
                    case "ColumnFormat":
                        {
                            element.ColumnFormatClause = AppendClause(element.ColumnFormatClause, clause);
                            break;
                        }
                    case "Storage":
                        {
                            element.StorageClause = AppendClause(element.StorageClause, clause);
                            break;
                        }
                    case "Srid":
                        {
                            element.SridClause = AppendClause(element.SridClause, clause);
                            break;
                        }
                    default:
                        {
                            element.ExtraClause = AppendClause(element.ExtraClause, clause);
                            break;
                        }
                }
            }
        }
        #endregion

        #region Output
        private static ColumnFormatWidths BuildColumnFormatWidths(List<TableElement> listElements)
        {
            var columns = (listElements ?? new List<TableElement>()).Where(x => x != null && x.IsColumn).ToList();

            return new ColumnFormatWidths
            {
                ColumnNameWidth = columns.Select(x => x.ColumnName?.Length ?? 0).DefaultIfEmpty(0).Max(),
                DataTypeWidth = columns.Select(x => x.DataType?.Length ?? 0).DefaultIfEmpty(0).Max(),
                CharacterSetWidth = columns.Select(x => x.CharacterSetClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                CollateWidth = columns.Select(x => x.CollateClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                GeneratedWidth = columns.Select(x => x.GeneratedClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                NullabilityWidth = columns.Select(x => x.NullabilityClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                DefaultWidth = columns.Select(x => x.DefaultClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                OnUpdateWidth = columns.Select(x => x.OnUpdateClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                AutoIncrementWidth = columns.Select(x => x.AutoIncrementClause?.Length ?? 0).DefaultIfEmpty(0).Max(),
                CommentWidth = columns.Select(x => x.CommentClause?.Length ?? 0).DefaultIfEmpty(0).Max()
            };
        }

        private static string BuildFormattedColumnLine(TableElement element, ColumnFormatWidths widths)
        {
            var sb = new StringBuilder();

            sb.Append("    ");
            sb.Append(element.ColumnName ?? string.Empty);
            sb.Append(new string(' ', Math.Max(0, widths.ColumnNameWidth - (element.ColumnName ?? string.Empty).Length)));
            sb.Append("  ");
            sb.Append(element.DataType ?? string.Empty);
            sb.Append(new string(' ', Math.Max(0, widths.DataTypeWidth - (element.DataType ?? string.Empty).Length)));

            AppendAlignedClause(sb, element.CharacterSetClause, widths.CharacterSetWidth);
            AppendAlignedClause(sb, element.CollateClause, widths.CollateWidth);
            AppendAlignedClause(sb, element.GeneratedClause, widths.GeneratedWidth);
            AppendAlignedClause(sb, element.NullabilityClause, widths.NullabilityWidth);
            AppendAlignedClause(sb, element.DefaultClause, widths.DefaultWidth);
            AppendAlignedClause(sb, element.OnUpdateClause, widths.OnUpdateWidth);
            AppendAlignedClause(sb, element.AutoIncrementClause, widths.AutoIncrementWidth);
            AppendAlignedClause(sb, element.CommentClause, widths.CommentWidth);

            AppendSimpleClause(sb, element.KeyClause);
            AppendSimpleClause(sb, element.ReferenceClause);
            AppendSimpleClause(sb, element.ColumnFormatClause);
            AppendSimpleClause(sb, element.StorageClause);
            AppendSimpleClause(sb, element.SridClause);
            AppendSimpleClause(sb, element.ExtraClause);

            return sb.ToString().TrimEnd(' ');
        }

        private static string BuildFormattedNonColumnLine(string text)
        {
            text = MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return $"    {text}";
        }

        private static void AppendAlignedClause(StringBuilder sb, string clause, int width)
        {
            if (width <= 0 && string.IsNullOrWhiteSpace(clause))
            {
                return;
            }

            sb.Append("  ");
            sb.Append(clause ?? string.Empty);
            sb.Append(new string(' ', Math.Max(0, width - (clause ?? string.Empty).Length)));
        }

        private static void AppendSimpleClause(StringBuilder sb, string clause)
        {
            if (string.IsNullOrWhiteSpace(clause))
            {
                return;
            }

            sb.Append("  ").Append(clause.Trim());
        }
        #endregion

        #region Helpers
        private static bool MatchesPhrase(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, int startIndex, params string[] words)
        {
            if (tokens == null || words == null || startIndex < 0 || startIndex + words.Length > tokens.Count)
            {
                return false;
            }

            for (var i = 0; i < words.Length; i++)
            {
                if (!string.Equals(tokens[startIndex + i].Text, words[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static string AppendClause(string original, string nextClause)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                return nextClause?.Trim() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(nextClause))
            {
                return original;
            }

            return $"{original} {nextClause.Trim()}";
        }
        #endregion
    }
}