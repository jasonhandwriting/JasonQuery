using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlIndexCreateScriptFormatter : MySqlCreateScriptFormatterBase
    {
        private sealed class TailClauseMarker
        {
            public int StartIndex { get; set; }
        }

        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Indexes);
        }

        protected override string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);

            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            var lines = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(script)
                                                        .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                        .ToList();

            var commentLines = new List<string>();
            var statementLines = new List<string>();

            foreach (var lineRaw in lines)
            {
                var line = (lineRaw ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("--", StringComparison.Ordinal))
                {
                    commentLines.Add(line);
                }
                else
                {
                    statementLines.Add(line);
                }
            }

            if (statementLines.Count <= 0)
            {
                return string.Join("\r\n", commentLines);
            }

            var statement = string.Join(" ", statementLines);
            var formattedStatement = TryFormatCreateIndexStatement(statement, out var result) ? result : MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(statement);

            if (commentLines.Count <= 0)
            {
                return formattedStatement;
            }

            return $"{string.Join("\r\n", commentLines)}\r\n{formattedStatement}";
        }

        private static bool TryFormatCreateIndexStatement(string statement, out string result)
        {
            result = string.Empty;

            if (string.IsNullOrWhiteSpace(statement))
            {
                return false;
            }

            var normalized = MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(statement);
            var hasSemicolon = normalized.EndsWith(";", StringComparison.Ordinal);

            normalized = RemoveTrailingSemicolon(normalized).Trim();

            if (!normalized.StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var onIndex = FindTopLevelPhraseIndex(normalized, "ON");

            if (onIndex < 0)
            {
                return false;
            }

            var beforeOn = normalized.Substring(0, onIndex).TrimEnd();
            var afterOn = normalized.Substring(onIndex + 2).TrimStart();
            var openParenIndex = MySqlCreateScriptFormatterHelper.FindFirstTopLevelChar(afterOn, '(');

            if (openParenIndex < 0)
            {
                return false;
            }

            var closeParenIndex = MySqlCreateScriptFormatterHelper.FindMatchingParen(afterOn, openParenIndex);

            if (closeParenIndex < 0)
            {
                return false;
            }

            var tableName = afterOn.Substring(0, openParenIndex).TrimEnd();
            var columnText = afterOn.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var tailText = afterOn.Substring(closeParenIndex + 1).Trim();

            SplitHeadAndUsingClause(beforeOn, out var createHead, out var usingClause);

            if (string.IsNullOrWhiteSpace(createHead))
            {
                return false;
            }

            var columns = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(columnText)
                                                          .Select(x => NormalizeIndexColumn(x))
                                                          .Where(x => !string.IsNullOrWhiteSpace(x))
                                                          .ToList();

            if (columns.Count <= 0)
            {
                return false;
            }

            var tailClauses = SplitTailClauses(tailText);
            var sb = new StringBuilder();

            sb.AppendLine(createHead);

            if (!string.IsNullOrWhiteSpace(usingClause))
            {
                sb.AppendLine(usingClause);
            }

            sb.AppendLine($"ON {tableName}");
            sb.AppendLine("(");

            for (var i = 0; i < columns.Count; i++)
            {
                var suffix = i == columns.Count - 1 ? string.Empty : ",";

                sb.AppendLine($"    {columns[i]}{suffix}");
            }

            sb.Append(")");

            if (tailClauses.Count > 0)
            {
                for (var i = 0; i < tailClauses.Count; i++)
                {
                    sb.AppendLine();
                    sb.Append(tailClauses[i]);
                }
            }

            if (hasSemicolon)
            {
                sb.Append(";");
            }

            result = sb.ToString().TrimEnd('\r', '\n');
            return true;
        }

        private static void SplitHeadAndUsingClause(string beforeOn, out string createHead, out string usingClause)
        {
            createHead = beforeOn?.Trim() ?? string.Empty;
            usingClause = string.Empty;

            if (string.IsNullOrWhiteSpace(beforeOn))
            {
                return;
            }

            var usingIndex = FindTopLevelPhraseIndex(beforeOn, "USING");

            if (usingIndex < 0)
            {
                return;
            }

            createHead = beforeOn.Substring(0, usingIndex).TrimEnd();
            usingClause = beforeOn.Substring(usingIndex).Trim();
        }

        private static List<string> SplitTailClauses(string tailText)
        {
            var results = new List<string>();

            tailText = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(tailText);

            if (string.IsNullOrWhiteSpace(tailText))
            {
                return results;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(tailText);

            if (tokens.Count <= 0)
            {
                results.Add(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(tailText));
                return results;
            }

            var markers = new List<TailClauseMarker>();

            for (var i = 0; i < tokens.Count; i++)
            {
                if (MatchesPhrase(tokens, i, "COMMENT"))
                {
                    markers.Add(new TailClauseMarker
                    {
                        StartIndex = tokens[i].StartIndex
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "VISIBLE")
                    || MatchesPhrase(tokens, i, "INVISIBLE")
                    || MatchesPhrase(tokens, i, "KEY_BLOCK_SIZE")
                    || MatchesPhrase(tokens, i, "ALGORITHM")
                    || MatchesPhrase(tokens, i, "LOCK")
                    || MatchesPhrase(tokens, i, "ENGINE_ATTRIBUTE")
                    || MatchesPhrase(tokens, i, "SECONDARY_ENGINE_ATTRIBUTE"))
                {
                    markers.Add(new TailClauseMarker
                    {
                        StartIndex = tokens[i].StartIndex
                    });

                    continue;
                }

                if (MatchesPhrase(tokens, i, "WITH", "PARSER"))
                {
                    markers.Add(new TailClauseMarker
                    {
                        StartIndex = tokens[i].StartIndex
                    });

                    i += 1;
                    continue;
                }
            }

            markers = markers.OrderBy(x => x.StartIndex)
                             .GroupBy(x => x.StartIndex)
                             .Select(x => x.First())
                             .ToList();

            if (markers.Count <= 0)
            {
                results.Add(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(tailText));
                return results;
            }

            for (var i = 0; i < markers.Count; i++)
            {
                var start = markers[i].StartIndex;
                var end = i == markers.Count - 1 ? tailText.Length : markers[i + 1].StartIndex;
                var clause = tailText.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(clause))
                {
                    results.Add(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
            }

            return results;
        }

        private static int FindTopLevelPhraseIndex(string text, params string[] words)
        {
            return MySqlFormatterCommonHelper.FindTopLevelPhraseIndex(text, words);
        }

        private static bool MatchesPhrase(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, int startIndex, params string[] words)
        {
            return MySqlFormatterCommonHelper.MatchesPhrase(tokens, startIndex, words);
        }

        private static string NormalizeIndexColumn(string text)
        {
            text = MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(text).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text;
        }

        private static string RemoveTrailingSemicolon(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.TrimEnd();

            return text.EndsWith(";", StringComparison.Ordinal) ? text.Substring(0, text.Length - 1).TrimEnd() : text;
        }
    }
}