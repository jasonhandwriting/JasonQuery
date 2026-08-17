using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerClauseStyleFormatterHelper
    {
        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        public static string FormatModuleBatch(string batch, string schemaType)
        {
            batch = SqlServerCreateScriptFormatterHelper.NormalizeBatchText(batch);

            if (string.IsNullOrWhiteSpace(batch))
            {
                return string.Empty;
            }

            if (!TrySplitDeclarationAndBody(batch, out var declaration, out var body))
            {
                return batch;
            }

            var formattedDeclaration = SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Triggers)
                                       ? FormatTriggerDeclaration(declaration)
                                       : FormatStandardModuleDeclaration(declaration);

            var formattedBody = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            return SqlServerCreateScriptFormatterHelper.CombineHeaderAndBody
            (
                formattedDeclaration,
                formattedBody
            );
        }

        public static string FormatIndexHeader(string header)
        {
            header = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(header);

            if (string.IsNullOrWhiteSpace(header))
            {
                return string.Empty;
            }

            var onIndex = FindTopLevelKeywordIndex(header, "ON");

            if (onIndex < 0)
            {
                return NormalizeSimpleLines(header);
            }

            var beforeOn = header.Substring(0, onIndex).TrimEnd();
            var onClause = header.Substring(onIndex).Trim();

            return $"{NormalizeSimpleLines(beforeOn)}\r\n{NormalizeSimpleLines(onClause)}";
        }

        public static string FormatIndexClauseSegment(string text)
        {
            text = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var segments = SplitByTopLevelKeywords
            (
                text,
                new[] { "INCLUDE", "WHERE", "ON" }
            );

            if (segments.Count <= 0)
            {
                return NormalizeSimpleLines(text);
            }

            var sb = new StringBuilder();

            for (var i = 0; i < segments.Count; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(segments[i].Trim());
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        public static string FormatWithClause(string withClause)
        {
            withClause = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(withClause);

            if (string.IsNullOrWhiteSpace(withClause))
            {
                return string.Empty;
            }

            var items = SplitTopLevelCommaItems(withClause);

            if (items.Count <= 0)
            {
                return string.Empty;
            }

            var normalizedItems = items.Select(x => x.Trim())
                                       .Where(x => !string.IsNullOrWhiteSpace(x))
                                       .ToList();

            if (normalizedItems.Count <= 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine("WITH");

            if (normalizedItems.Count == 1)
            {
                sb.Append($"    {normalizedItems[0]}");
                return sb.ToString().TrimEnd('\r', '\n');
            }

            sb.AppendLine("(");

            for (var i = 0; i < normalizedItems.Count; i++)
            {
                var suffix = i == normalizedItems.Count - 1 ? string.Empty : ",";

                sb.AppendLine($"    {normalizedItems[i]}{suffix}");
            }

            sb.Append(")");

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #region Module Declaration
        private static bool TrySplitDeclarationAndBody(string text, out string declaration, out string body)
        {
            declaration = string.Empty;
            body = string.Empty;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokenInfos = TokenizeTopLevelWords(text);

            if (tokenInfos.Count <= 0)
            {
                return false;
            }

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                var token = tokenInfos[i];

                if (!string.Equals(token.Text, "AS", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (i > 0 && string.Equals(tokenInfos[i - 1].Text, "EXECUTE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                declaration = text.Substring(0, token.StartIndex).TrimEnd();
                body = text.Substring(token.StartIndex).TrimStart();

                return !string.IsNullOrWhiteSpace(declaration);
            }

            return false;
        }

        private static string FormatStandardModuleDeclaration(string declaration)
        {
            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            var withIndex = FindTopLevelKeywordIndex(declaration, "WITH");

            if (withIndex < 0)
            {
                return NormalizeSimpleLines(declaration);
            }

            var header = declaration.Substring(0, withIndex).TrimEnd();
            var withClause = declaration.Substring(withIndex + 4).Trim();
            var sb = new StringBuilder();

            sb.AppendLine(NormalizeSimpleLines(header));
            sb.Append(FormatWithClause(withClause));

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatTriggerDeclaration(string declaration)
        {
            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            var onIndex = FindTopLevelKeywordIndex(declaration, "ON");
            var withIndex = FindTopLevelKeywordIndex(declaration, "WITH");
            var actionIndex = FindTriggerActionIndex(declaration);
            var createPart = declaration;
            var onPart = string.Empty;
            var withPart = string.Empty;
            var actionPart = string.Empty;

            if (onIndex > 0)
            {
                createPart = declaration.Substring(0, onIndex).TrimEnd();

                var endOfOnPart = declaration.Length;

                if (withIndex > onIndex && withIndex < endOfOnPart)
                {
                    endOfOnPart = withIndex;
                }

                if (actionIndex > onIndex && actionIndex < endOfOnPart)
                {
                    endOfOnPart = actionIndex;
                }

                onPart = declaration.Substring(onIndex, endOfOnPart - onIndex).Trim();
            }

            if (withIndex > 0)
            {
                var endOfWithPart = declaration.Length;

                if (actionIndex > withIndex && actionIndex < endOfWithPart)
                {
                    endOfWithPart = actionIndex;
                }

                withPart = declaration.Substring(withIndex + 4, endOfWithPart - withIndex - 4).Trim();
            }

            if (actionIndex > 0)
            {
                actionPart = declaration.Substring(actionIndex).Trim();
            }

            var sb = new StringBuilder();

            sb.Append(NormalizeSimpleLines(createPart));

            if (!string.IsNullOrWhiteSpace(onPart))
            {
                sb.AppendLine();
                sb.Append(NormalizeSimpleLines(onPart));
            }

            if (!string.IsNullOrWhiteSpace(withPart))
            {
                sb.AppendLine();
                sb.Append(FormatWithClause(withPart));
            }

            if (!string.IsNullOrWhiteSpace(actionPart))
            {
                sb.AppendLine();
                sb.Append(NormalizeSimpleLines(actionPart));
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static int FindTriggerActionIndex(string text)
        {
            var candidates = new List<int>();
            var afterIndex = FindTopLevelKeywordIndex(text, "AFTER");
            var forIndex = FindTopLevelKeywordIndex(text, "FOR");
            var insteadIndex = FindTopLevelKeywordIndex(text, "INSTEAD OF");

            if (afterIndex >= 0)
            {
                candidates.Add(afterIndex);
            }

            if (forIndex >= 0)
            {
                candidates.Add(forIndex);
            }

            if (insteadIndex >= 0)
            {
                candidates.Add(insteadIndex);
            }

            return candidates.Count <= 0 ? -1 : candidates.Min();
        }
        #endregion

        #region Keyword / Token Parser
        private static List<string> SplitByTopLevelKeywords(string text, string[] keywords)
        {
            var listIndexes = new List<int>();

            foreach (var keyword in keywords ?? Array.Empty<string>())
            {
                var index = FindTopLevelKeywordIndex(text, keyword);

                while (index >= 0)
                {
                    if (!listIndexes.Contains(index))
                    {
                        listIndexes.Add(index);
                    }

                    index = FindTopLevelKeywordIndex(text, keyword, index + keyword.Length);
                }
            }

            if (listIndexes.Count <= 0)
            {
                return new List<string> { text.Trim() };
            }

            listIndexes.Sort();

            var result = new List<string>();

            for (var i = 0; i < listIndexes.Count; i++)
            {
                var start = listIndexes[i];
                var end = i == listIndexes.Count - 1 ? text.Length : listIndexes[i + 1];
                var segment = text.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    result.Add(segment);
                }
            }

            if (listIndexes[0] > 0)
            {
                var prefix = text.Substring(0, listIndexes[0]).Trim();

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    result.Insert(0, prefix);
                }
            }

            return result;
        }

        private static int FindTopLevelKeywordIndex(string text, string keyword, int startIndex = 0)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            {
                return -1;
            }

            var tokenInfos = TokenizeTopLevelWords(text);

            for (var i = 0; i < tokenInfos.Count; i++)
            {
                if (tokenInfos[i].StartIndex < startIndex)
                {
                    continue;
                }

                if (string.Equals(keyword, "INSTEAD OF", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < tokenInfos.Count
                        && string.Equals(tokenInfos[i].Text, "INSTEAD", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "OF", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(tokenInfos[i].Text, keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return tokenInfos[i].StartIndex;
                }
            }

            return -1;
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