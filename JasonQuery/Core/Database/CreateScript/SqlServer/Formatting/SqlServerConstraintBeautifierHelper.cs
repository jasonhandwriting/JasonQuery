using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerConstraintBeautifierHelper
    {
        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        public static string BeautifyBatch(string batch)
        {
            batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);

            if (string.IsNullOrWhiteSpace(batch))
            {
                return string.Empty;
            }

            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(batch)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                            .Select(x => x.Trim())
                                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                                            .ToList();

            if (lines.Count <= 0)
            {
                return string.Empty;
            }

            var statement = string.Join(" ", lines);

            if (TryFormatForeignKey(statement, out var foreignKeyResult))
            {
                return foreignKeyResult;
            }

            if (TryFormatCheckAdd(statement, out var checkAddResult))
            {
                return checkAddResult;
            }

            if (TryFormatCheckConstraint(statement, out var checkConstraintResult))
            {
                return checkConstraintResult;
            }

            if (TryFormatDefaultConstraint(statement, out var defaultConstraintResult))
            {
                return defaultConstraintResult;
            }

            return SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);
        }

        #region Foreign Key
        private static bool TryFormatForeignKey(string statement, out string result)
        {
            result = string.Empty;

            var match = Regex.Match
            (
                statement,
                @"^(?<alter>ALTER\s+TABLE\s+.+?)(?:\s+(?<with>WITH\s+(?:NO)?CHECK))?\s+ADD\s+CONSTRAINT\s+(?<constraint>\[[^\]]+\]|""[^""]+""|\S+)\s+FOREIGN\s+KEY\s*\((?<columns>.*?)\)\s+REFERENCES\s+(?<refTable>.+?)\s*\((?<refColumns>.*?)\)\s+ON\s+UPDATE\s+(?<onUpdate>.+?)\s+ON\s+DELETE\s+(?<onDelete>.+?)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (!match.Success)
            {
                return false;
            }

            var alter = match.Groups["alter"].Value.Trim();
            var withClause = match.Groups["with"].Value.Trim();
            var constraint = match.Groups["constraint"].Value.Trim();
            var columns = match.Groups["columns"].Value.Trim();
            var refTable = match.Groups["refTable"].Value.Trim();
            var refColumns = match.Groups["refColumns"].Value.Trim();
            var onUpdate = match.Groups["onUpdate"].Value.Trim();
            var onDelete = match.Groups["onDelete"].Value.Trim();
            var sb = new StringBuilder();

            sb.AppendLine(alter);

            if (!string.IsNullOrWhiteSpace(withClause))
            {
                sb.AppendLine($"    {withClause}");
            }

            sb.AppendLine($"    ADD CONSTRAINT {constraint}");
            sb.AppendLine("    FOREIGN KEY");
            sb.AppendLine("    (");
            AppendCommaList(sb, columns, 8);
            sb.AppendLine();
            sb.AppendLine("    )");
            sb.AppendLine($"    REFERENCES {refTable}");
            sb.AppendLine("    (");
            AppendCommaList(sb, refColumns, 8);
            sb.AppendLine();
            sb.AppendLine("    )");
            sb.AppendLine($"    ON UPDATE {onUpdate}");
            sb.Append($"    ON DELETE {onDelete}");

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }
        #endregion

        #region Check Add
        private static bool TryFormatCheckAdd(string statement, out string result)
        {
            result = string.Empty;

            var match = Regex.Match
            (
                statement,
                @"^(?<alter>ALTER\s+TABLE\s+.+?)(?:\s+(?<with>WITH\s+(?:NO)?CHECK))?\s+ADD\s+CONSTRAINT\s+(?<constraint>\[[^\]]+\]|""[^""]+""|\S+)\s+CHECK\s*\((?<expr>.*)\)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (!match.Success)
            {
                return false;
            }

            var alter = match.Groups["alter"].Value.Trim();
            var withClause = match.Groups["with"].Value.Trim();
            var constraint = match.Groups["constraint"].Value.Trim();
            var expr = match.Groups["expr"].Value.Trim();
            var sb = new StringBuilder();

            sb.AppendLine(alter);

            if (!string.IsNullOrWhiteSpace(withClause))
            {
                sb.AppendLine($"    {withClause}");
            }

            sb.AppendLine($"    ADD CONSTRAINT {constraint}");
            sb.AppendLine("    CHECK");
            sb.AppendLine("    (");
            AppendCheckExpression(sb, expr, 8);
            sb.AppendLine();
            sb.Append("    )");

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }
        #endregion

        #region Check Constraint
        private static bool TryFormatCheckConstraint(string statement, out string result)
        {
            result = string.Empty;

            var match = Regex.Match
            (
                statement,
                @"^(?<alter>ALTER\s+TABLE\s+.+?)\s+CHECK\s+CONSTRAINT\s+(?<constraint>.+)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (!match.Success)
            {
                return false;
            }

            var alter = match.Groups["alter"].Value.Trim();
            var constraint = match.Groups["constraint"].Value.Trim();
            var sb = new StringBuilder();

            sb.AppendLine(alter);
            sb.Append($"    CHECK CONSTRAINT {constraint}");

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }
        #endregion

        #region Default Constraint
        private static bool TryFormatDefaultConstraint(string statement, out string result)
        {
            result = string.Empty;

            var match = Regex.Match
            (
                statement,
                @"^(?<alter>ALTER\s+TABLE\s+.+?)\s+ADD\s+CONSTRAINT\s+(?<constraint>\[[^\]]+\]|""[^""]+""|\S+)\s+DEFAULT\s+(?<default>.+?)\s+FOR\s+(?<column>\[[^\]]+\]|""[^""]+""|\S+)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (!match.Success)
            {
                return false;
            }

            var alter = match.Groups["alter"].Value.Trim();
            var constraint = match.Groups["constraint"].Value.Trim();
            var defaultValue = match.Groups["default"].Value.Trim();
            var column = match.Groups["column"].Value.Trim();
            var sb = new StringBuilder();

            sb.AppendLine(alter);
            sb.AppendLine($"    ADD CONSTRAINT {constraint}");
            sb.AppendLine($"    DEFAULT {defaultValue}");
            sb.Append($"    FOR {column}");

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }
        #endregion

        #region Helpers
        private static void AppendCommaList(StringBuilder sb, string text, int indentSpaces)
        {
            var items = SplitTopLevelCommaItems(text);

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i].Trim();
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                sb.Append(new string(' ', indentSpaces));
                sb.Append(item);
                sb.Append(suffix);
            }
        }

        private static void AppendCheckExpression(StringBuilder sb, string expr, int indentSpaces)
        {
            var parts = SplitTopLevelLogicalConditions(expr);

            if (parts.Count <= 1)
            {
                sb.Append(new string(' ', indentSpaces));
                sb.Append(NormalizeSimpleLines(expr));
                return;
            }

            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(new string(' ', indentSpaces));
                sb.Append(parts[i].Trim());
            }
        }

        private static List<string> SplitTopLevelLogicalConditions(string text)
        {
            var parts = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return parts;
            }

            var tokenInfos = TokenizeTopLevelWords(text);

            if (tokenInfos.Count <= 0)
            {
                parts.Add(text.Trim());
                return parts;
            }

            var splitIndexes = new List<int>();
            var caseDepth = 0;
            var waitingBetweenAnd = false;

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                var token = tokenInfos[i].Text;

                if (string.Equals(token, "CASE", StringComparison.OrdinalIgnoreCase))
                {
                    caseDepth++;
                    continue;
                }

                if (string.Equals(token, "END", StringComparison.OrdinalIgnoreCase) && caseDepth > 0)
                {
                    caseDepth--;
                    continue;
                }

                if (caseDepth > 0)
                {
                    continue;
                }

                if (string.Equals(token, "BETWEEN", StringComparison.OrdinalIgnoreCase))
                {
                    waitingBetweenAnd = true;
                    continue;
                }

                if (waitingBetweenAnd && string.Equals(token, "AND", StringComparison.OrdinalIgnoreCase))
                {
                    waitingBetweenAnd = false;
                    continue;
                }

                if (string.Equals(token, "AND", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "OR", StringComparison.OrdinalIgnoreCase))
                {
                    splitIndexes.Add(tokenInfos[i].StartIndex);
                }
            }

            if (splitIndexes.Count <= 0)
            {
                parts.Add(text.Trim());
                return parts;
            }

            var firstPart = text.Substring(0, splitIndexes[0]).Trim();

            if (!string.IsNullOrWhiteSpace(firstPart))
            {
                parts.Add(firstPart);
            }

            for (var i = 0; i < splitIndexes.Count; i++)
            {
                var start = splitIndexes[i];
                var end = i == splitIndexes.Count - 1 ? text.Length : splitIndexes[i + 1];
                var segment = text.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    parts.Add(segment);
                }
            }

            return parts;
        }

        private static List<TokenInfo> TokenizeTopLevelWords(string text)
        {
            var list = new List<TokenInfo>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;
            var tokenStart = -1;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        inBracket = false;
                        continue;
                    }
                }

                if (inSingleQuote || inDoubleQuote || inBracket)
                {
                    continue;
                }

                if (ch == '(')
                {
                    depth++;
                    continue;
                }

                if (ch == ')')
                {
                    depth--;
                    continue;
                }

                if (depth > 0)
                {
                    continue;
                }

                if (!char.IsWhiteSpace(ch))
                {
                    if (tokenStart < 0)
                    {
                        tokenStart = i;
                    }
                }
                else
                {
                    if (tokenStart >= 0)
                    {
                        list.Add(new TokenInfo
                        {
                            Text = text.Substring(tokenStart, i - tokenStart),
                            StartIndex = tokenStart
                        });

                        tokenStart = -1;
                    }
                }
            }

            if (tokenStart >= 0)
            {
                list.Add(new TokenInfo
                {
                    Text = text.Substring(tokenStart),
                    StartIndex = tokenStart
                });
            }

            return list;
        }

        private static List<string> SplitTopLevelCommaItems(string text)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }

            var sb = new StringBuilder();
            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (!inDoubleQuote && !inBracket && ch == '\'')
                {
                    sb.Append(ch);

                    if (inSingleQuote)
                    {
                        if (i + 1 < text.Length && text[i + 1] == '\'')
                        {
                            sb.Append(text[i + 1]);
                            i++;
                            continue;
                        }

                        inSingleQuote = false;
                    }
                    else
                    {
                        inSingleQuote = true;
                    }

                    continue;
                }

                if (!inSingleQuote && !inBracket && ch == '"')
                {
                    sb.Append(ch);
                    inDoubleQuote = !inDoubleQuote;
                    continue;
                }

                if (!inSingleQuote && !inDoubleQuote)
                {
                    if (ch == '[')
                    {
                        sb.Append(ch);
                        inBracket = true;
                        continue;
                    }

                    if (ch == ']')
                    {
                        sb.Append(ch);
                        inBracket = false;
                        continue;
                    }
                }

                if (!inSingleQuote && !inDoubleQuote && !inBracket)
                {
                    if (ch == '(')
                    {
                        depth++;
                    }
                    else if (ch == ')')
                    {
                        depth--;
                    }
                    else if (ch == ',' && depth == 0)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                sb.Append(ch);
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            return list;
        }

        private static string NormalizeSimpleLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(text)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                            .Select(x => x.Trim())
                                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                                            .ToList();

            return string.Join("\r\n", lines);
        }
        #endregion
    }
}