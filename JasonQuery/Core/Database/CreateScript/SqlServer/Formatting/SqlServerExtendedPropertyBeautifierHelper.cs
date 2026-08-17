using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerExtendedPropertyBeautifierHelper
    {
        private sealed class ParameterItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string RawText { get; set; }
        }

        public static string BeautifyBatch(string batch)
        {
            batch = SqlServerCreateScriptFormatterHelper.RemoveLeadingCommentLine
            (
                batch,
                "--Table's Comment",
                "--Column Comment"
            );

            batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);

            if (string.IsNullOrWhiteSpace(batch))
            {
                return string.Empty;
            }

            if (!TrySplitExecAndParameters(batch, out var execLine, out var parameterText))
            {
                return batch;
            }

            var parameters = SplitTopLevelCommaItems(parameterText).Select(ParseParameterItem).Where(x => x != null).ToList();

            if (parameters.Count <= 0)
            {
                return batch;
            }

            var maxNameLength = parameters.Where(x => !string.IsNullOrWhiteSpace(x.Name))
                                          .Select(x => x.Name.Length)
                                          .DefaultIfEmpty(0)
                                          .Max();

            var sb = new StringBuilder();

            sb.AppendLine(NormalizeSimpleLines(execLine));

            const string sParamIndent = "     "; //5 spaces，對齊 "EXEC "

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var suffix = i == parameters.Count - 1 ? string.Empty : ",";

                if (!string.IsNullOrWhiteSpace(parameter.Name))
                {
                    var padding = new string(' ', Math.Max(0, maxNameLength - parameter.Name.Length));

                    sb.Append(sParamIndent);
                    sb.Append(parameter.Name);
                    sb.Append(padding);
                    sb.Append(" = ");
                    sb.Append(parameter.Value);
                    sb.Append(suffix);
                }
                else
                {
                    sb.Append(sParamIndent);
                    sb.Append(parameter.RawText);
                    sb.Append(suffix);
                }

                if (i < parameters.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool TrySplitExecAndParameters(string batch, out string execLine, out string parameterText)
        {
            execLine = string.Empty;
            parameterText = string.Empty;

            if (string.IsNullOrWhiteSpace(batch))
            {
                return false;
            }

            var firstParameterIndex = FindFirstTopLevelParameterIndex(batch);

            if (firstParameterIndex < 0)
            {
                return false;
            }

            execLine = batch.Substring(0, firstParameterIndex).TrimEnd();
            parameterText = batch.Substring(firstParameterIndex).Trim();

            return !string.IsNullOrWhiteSpace(execLine) && !string.IsNullOrWhiteSpace(parameterText);
        }

        private static int FindFirstTopLevelParameterIndex(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

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

                if (depth == 0 && ch == '@')
                {
                    return i;
                }
            }

            return -1;
        }

        private static ParameterItem ParseParameterItem(string text)
        {
            text = (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var eqIndex = IndexOfTopLevelChar(text, '=');

            if (eqIndex < 0)
            {
                return new ParameterItem
                {
                    Name = string.Empty,
                    Value = string.Empty,
                    RawText = text
                };
            }

            return new ParameterItem
            {
                Name = text.Substring(0, eqIndex).Trim(),
                Value = text.Substring(eqIndex + 1).Trim(),
                RawText = text
            };
        }

        private static int IndexOfTopLevelChar(string text, char target)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            var depth = 0;
            var inSingleQuote = false;
            var inDoubleQuote = false;
            var inBracket = false;

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

                if (depth == 0 && ch == target)
                {
                    return i;
                }
            }

            return -1;
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
    }
}