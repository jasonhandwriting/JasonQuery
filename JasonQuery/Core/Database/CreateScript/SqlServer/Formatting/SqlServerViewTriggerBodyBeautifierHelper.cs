using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal static class SqlServerViewTriggerBodyBeautifierHelper
    {
        private sealed class TokenInfo
        {
            public string Text { get; set; }
            public int StartIndex { get; set; }
        }

        private enum TriggerLineKind
        {
            Normal,
            If,
            While,
            Else,
            Begin,
            End,
            BeginTry,
            EndTry,
            BeginCatch,
            EndCatch
        }

        public static string FormatViewBatch(string batch)
        {
            batch = SqlServerClauseStyleFormatterHelper.FormatModuleBatch(batch, SchemaObjectNames.Views);

            if (!TrySplitDeclarationAndBody(batch, out var declaration, out var body))
            {
                return batch;
            }

            var bodyWithoutAs = RemoveLeadingAs(body);
            var formattedQuery = FormatViewQuery(bodyWithoutAs);

            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);
            formattedQuery = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(formattedQuery);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.IsNullOrWhiteSpace(formattedQuery) ? "AS" : $"AS\r\n{formattedQuery}";
            }

            if (string.IsNullOrWhiteSpace(formattedQuery))
            {
                return $"{declaration}\r\nAS";
            }

            return $"{declaration}\r\nAS\r\n{formattedQuery}";
        }

        public static string FormatTriggerBatch(string batch)
        {
            batch = SqlServerClauseStyleFormatterHelper.FormatModuleBatch(batch, SchemaObjectNames.Triggers);

            if (!TrySplitDeclarationAndBody(batch, out var declaration, out var body))
            {
                return batch;
            }

            var bodyWithoutAs = RemoveLeadingAs(body);
            var formattedBody = FormatTriggerExecutableBody(bodyWithoutAs);

            declaration = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);
            formattedBody = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(formattedBody);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.IsNullOrWhiteSpace(formattedBody) ? "AS" : $"AS\r\n{formattedBody}";
            }

            if (string.IsNullOrWhiteSpace(formattedBody))
            {
                return $"{declaration}\r\nAS";
            }

            return $"{declaration}\r\nAS\r\n{formattedBody}";
        }

        #region View
        private static string FormatViewQuery(string query)
        {
            query = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(query);
            query = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(query);

            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            var clauses = SplitTopLevelQueryClauses(query);

            if (clauses.Count <= 0)
            {
                return query;
            }

            var sb = new StringBuilder();
            string previousClause = string.Empty;

            for (var i = 0; i < clauses.Count; i++)
            {
                var clause = clauses[i];

                if (string.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                if (sb.Length > 0 && ShouldInsertBlankLineBeforeViewClause(previousClause, clause))
                {
                    sb.AppendLine();
                }

                if (clause.StartsWith("WITH ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatWithCteClause(clause));
                }
                else if (clause.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatSelectClause(clause));
                }
                else if (StartsWithJoinClause(clause))
                {
                    sb.Append(NormalizeSimpleLines(clause));
                }
                else if (clause.StartsWith("ON ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatLogicalConditionClause("ON", clause.Substring(2).Trim()));
                }
                else if (clause.StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatLogicalConditionClause("WHERE", clause.Substring(5).Trim()));
                }
                else if (clause.StartsWith("HAVING ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatLogicalConditionClause("HAVING", clause.Substring(6).Trim()));
                }
                else if (clause.StartsWith("GROUP BY ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatListClause("GROUP BY", clause.Substring(8).Trim()));
                }
                else if (clause.StartsWith("ORDER BY ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatListClause("ORDER BY", clause.Substring(8).Trim()));
                }
                else if (clause.StartsWith("UNION ALL", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("UNION", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("INTERSECT", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("EXCEPT", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(NormalizeSimpleLines(clause));
                }
                else if (clause.StartsWith("OPTION", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatOptionClause(clause));
                }
                else
                {
                    sb.Append(NormalizeSimpleLines(clause));
                }

                previousClause = clause;
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static List<string> SplitTopLevelQueryClauses(string query)
        {
            var clauses = new List<string>();
            var keywordIndexes = new List<int>();

            AddKeywordIndexes(keywordIndexes, query, "WITH");
            AddKeywordIndexes(keywordIndexes, query, "SELECT");
            AddKeywordIndexes(keywordIndexes, query, "FROM");
            AddKeywordIndexes(keywordIndexes, query, "INNER JOIN");
            AddKeywordIndexes(keywordIndexes, query, "LEFT OUTER JOIN");
            AddKeywordIndexes(keywordIndexes, query, "LEFT JOIN");
            AddKeywordIndexes(keywordIndexes, query, "RIGHT OUTER JOIN");
            AddKeywordIndexes(keywordIndexes, query, "RIGHT JOIN");
            AddKeywordIndexes(keywordIndexes, query, "FULL OUTER JOIN");
            AddKeywordIndexes(keywordIndexes, query, "FULL JOIN");
            AddKeywordIndexes(keywordIndexes, query, "CROSS JOIN");
            AddKeywordIndexes(keywordIndexes, query, "JOIN");
            AddKeywordIndexes(keywordIndexes, query, "ON");
            AddKeywordIndexes(keywordIndexes, query, "WHERE");
            AddKeywordIndexes(keywordIndexes, query, "GROUP BY");
            AddKeywordIndexes(keywordIndexes, query, "HAVING");
            AddKeywordIndexes(keywordIndexes, query, "UNION ALL");
            AddKeywordIndexes(keywordIndexes, query, "UNION");
            AddKeywordIndexes(keywordIndexes, query, "EXCEPT");
            AddKeywordIndexes(keywordIndexes, query, "INTERSECT");
            AddKeywordIndexes(keywordIndexes, query, "ORDER BY");
            AddKeywordIndexes(keywordIndexes, query, "OPTION");

            keywordIndexes = keywordIndexes.Distinct().OrderBy(x => x).ToList();

            if (keywordIndexes.Count <= 0)
            {
                clauses.Add(query.Trim());
                return clauses;
            }

            for (var i = 0; i < keywordIndexes.Count; i++)
            {
                var start = keywordIndexes[i];
                var end = i == keywordIndexes.Count - 1 ? query.Length : keywordIndexes[i + 1];
                var segment = query.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    clauses.Add(segment);
                }
            }

            if (keywordIndexes[0] > 0)
            {
                var prefix = query.Substring(0, keywordIndexes[0]).Trim();

                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    clauses.Insert(0, prefix);
                }
            }

            return clauses;
        }

        private static string FormatWithCteClause(string clause)
        {
            var rest = clause.Substring(4).Trim();

            if (string.IsNullOrWhiteSpace(rest))
            {
                return "WITH";
            }

            var cteItems = SplitTopLevelCommaItems(rest);

            if (cteItems.Count <= 0)
            {
                return $"WITH {NormalizeSimpleLines(rest)}";
            }

            var sb = new StringBuilder();

            sb.AppendLine("WITH");

            for (var i = 0; i < cteItems.Count; i++)
            {
                var item = cteItems[i].Trim();
                var suffix = i == cteItems.Count - 1 ? string.Empty : ",";

                AppendIndentedMultiLine(sb, item, 4, suffix);

                if (i < cteItems.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatSelectClause(string clause)
        {
            var rest = clause.Substring(6).Trim();

            if (string.IsNullOrWhiteSpace(rest))
            {
                return "SELECT";
            }

            var items = SplitTopLevelCommaItems(rest);

            if (items.Count <= 1)
            {
                return $"SELECT {NormalizeSimpleLines(rest)}";
            }

            var sb = new StringBuilder();

            sb.AppendLine("SELECT");

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i].Trim();
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                AppendIndentedMultiLine(sb, item, 4, suffix);

                if (i < items.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatLogicalConditionClause(string keyword, string conditionText)
        {
            conditionText = (conditionText ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(conditionText))
            {
                return keyword;
            }

            var parts = SplitTopLevelLogicalConditions(conditionText);

            if (parts.Count <= 1)
            {
                return $"{keyword}\r\n    {NormalizeSimpleLines(conditionText)}";
            }

            var sb = new StringBuilder();

            sb.AppendLine(keyword);

            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i].Trim();

                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                AppendIndentedMultiLine(sb, part, 4, string.Empty);

                if (i < parts.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatListClause(string keyword, string listText)
        {
            listText = (listText ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(listText))
            {
                return keyword;
            }

            var items = SplitTopLevelCommaItems(listText);

            if (items.Count <= 1)
            {
                return $"{keyword}\r\n    {NormalizeSimpleLines(listText)}";
            }

            var sb = new StringBuilder();

            sb.AppendLine(keyword);

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i].Trim();
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                AppendIndentedMultiLine(sb, item, 4, suffix);

                if (i < items.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static string FormatOptionClause(string clause)
        {
            clause = NormalizeSimpleLines(clause);

            if (string.IsNullOrWhiteSpace(clause))
            {
                return string.Empty;
            }

            var openParenIndex = clause.IndexOf('(');
            var closeParenIndex = clause.LastIndexOf(')');

            if (openParenIndex < 0 || closeParenIndex <= openParenIndex)
            {
                return clause;
            }

            var head = clause.Substring(0, openParenIndex).Trim();
            var optionText = clause.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var items = SplitTopLevelCommaItems(optionText);

            if (items.Count <= 1)
            {
                return clause;
            }

            var sb = new StringBuilder();

            sb.AppendLine(head);
            sb.AppendLine("(");

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i].Trim();
                var suffix = i == items.Count - 1 ? string.Empty : ",";

                AppendIndentedMultiLine(sb, item, 4, suffix);

                if (i < items.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            sb.Append(")");

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static bool ShouldInsertBlankLineBeforeViewClause(string previousClause, string currentClause)
        {
            if (string.IsNullOrWhiteSpace(previousClause))
            {
                return false;
            }

            if (StartsWithJoinClause(currentClause) || currentClause.StartsWith("ON ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return currentClause.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("WHERE ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("GROUP BY ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("HAVING ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("UNION ALL", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("UNION", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("EXCEPT", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("INTERSECT", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("ORDER BY ", StringComparison.OrdinalIgnoreCase)
                   || currentClause.StartsWith("OPTION", StringComparison.OrdinalIgnoreCase);
        }

        private static bool StartsWithJoinClause(string clause)
        {
            return clause.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("INNER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("FULL JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("FULL OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("CROSS JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("JOIN ", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> SplitTopLevelLogicalConditions(string text)
        {
            var parts = new List<string>();
            var keywordIndexes = new List<int>();

            AddKeywordIndexes(keywordIndexes, text, "AND");
            AddKeywordIndexes(keywordIndexes, text, "OR");

            keywordIndexes = keywordIndexes.Distinct().OrderBy(x => x).ToList();

            if (keywordIndexes.Count <= 0)
            {
                parts.Add(text.Trim());
                return parts;
            }

            var firstIndex = keywordIndexes[0];
            var firstPart = text.Substring(0, firstIndex).Trim();

            if (!string.IsNullOrWhiteSpace(firstPart))
            {
                parts.Add(firstPart);
            }

            for (var i = 0; i < keywordIndexes.Count; i++)
            {
                var start = keywordIndexes[i];
                var end = i == keywordIndexes.Count - 1 ? text.Length : keywordIndexes[i + 1];
                var segment = text.Substring(start, end - start).Trim();

                if (!string.IsNullOrWhiteSpace(segment))
                {
                    parts.Add(segment);
                }
            }

            return parts;
        }

        private static void AppendIndentedMultiLine(StringBuilder sb, string text, int indentSpaces, string lastLineSuffix)
        {
            var indent = new string(' ', indentSpaces);
            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(text)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None)
                                                            .Select(x => x.Trim())
                                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                                            .ToList();

            if (lines.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < lines.Count; i++)
            {
                sb.Append(indent).Append(lines[i]);

                if (i == lines.Count - 1 && !string.IsNullOrWhiteSpace(lastLineSuffix))
                {
                    sb.Append(lastLineSuffix);
                }

                if (i < lines.Count - 1)
                {
                    sb.AppendLine();
                }
            }
        }
        #endregion

        #region Trigger
        private static string FormatTriggerExecutableBody(string body)
        {
            body = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(body);
            body = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            var logicalLines = BuildTriggerLogicalLines(body);

            if (logicalLines.Count <= 0)
            {
                return string.Empty;
            }

            return BuildIndentedTriggerBody(logicalLines);
        }

        private static List<string> BuildTriggerLogicalLines(string body)
        {
            var lines = SqlServerCreateScriptFormatterHelper.NormalizeLineEndings(body)
                                                            .Split(new[] { "\r\n" }, StringSplitOptions.None);

            var result = new List<string>();

            foreach (var raw in lines)
            {
                var line = (raw ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Add(string.Empty);
                    continue;
                }

                ExpandCompoundTriggerLine(line, result);
            }

            RemoveOuterBlankLines(result);

            return result;
        }

        private static void ExpandCompoundTriggerLine(string line, List<string> output)
        {
            line = (line ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                output.Add(string.Empty);
                return;
            }

            if (string.Equals(line, "END ELSE BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                output.Add("END");
                output.Add("ELSE");
                output.Add("BEGIN");
                return;
            }

            if (line.StartsWith("END ELSE ", StringComparison.OrdinalIgnoreCase))
            {
                output.Add("END");
                ExpandCompoundTriggerLine(line.Substring(4).TrimStart(), output);
                return;
            }

            if (string.Equals(line, "END TRY BEGIN CATCH", StringComparison.OrdinalIgnoreCase))
            {
                output.Add("END TRY");
                output.Add("BEGIN CATCH");
                return;
            }

            if (string.Equals(line, "ELSE BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                output.Add("ELSE");
                output.Add("BEGIN");
                return;
            }

            if (line.StartsWith("ELSE IF ", StringComparison.OrdinalIgnoreCase))
            {
                output.Add("ELSE");
                output.Add(line.Substring(5).TrimStart());
                return;
            }

            if (EndsWithBlockBegin(line) && StartsWithControlStatement(line))
            {
                var lineWithoutBegin = line.Substring(0, line.Length - 5).TrimEnd();

                if (!string.IsNullOrWhiteSpace(lineWithoutBegin))
                {
                    output.Add(lineWithoutBegin);
                }

                output.Add("BEGIN");
                return;
            }

            output.Add(line);
        }

        private static string BuildIndentedTriggerBody(IReadOnlyList<string> logicalLines)
        {
            var sb = new StringBuilder();
            var indentLevel = 0;
            var pendingSingleStatementIndent = false;
            var hasPreviousMeaningfulLine = false;
            TriggerLineKind previousKind = TriggerLineKind.Normal;

            for (var i = 0; i < logicalLines.Count; i++)
            {
                var line = logicalLines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var kind = GetTriggerLineKind(line);

                if (ShouldInsertBlankLine(hasPreviousMeaningfulLine, previousKind, kind, line))
                {
                    sb.AppendLine();
                }

                if (kind == TriggerLineKind.End || kind == TriggerLineKind.EndTry || kind == TriggerLineKind.EndCatch)
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                var currentIndent = indentLevel;

                if (pendingSingleStatementIndent && kind != TriggerLineKind.Begin && kind != TriggerLineKind.BeginTry
                                                 && kind != TriggerLineKind.BeginCatch && kind != TriggerLineKind.End
                                                 && kind != TriggerLineKind.EndTry && kind != TriggerLineKind.EndCatch
                                                 && kind != TriggerLineKind.Else)
                {
                    currentIndent++;
                }

                sb.Append(new string(' ', currentIndent * 4));
                sb.AppendLine(line.Trim());

                if (pendingSingleStatementIndent)
                {
                    pendingSingleStatementIndent = false;
                }

                switch (kind)
                {
                    case TriggerLineKind.Begin:
                    case TriggerLineKind.BeginTry:
                    case TriggerLineKind.BeginCatch:
                        {
                            indentLevel++;
                            break;
                        }
                    case TriggerLineKind.If:
                    case TriggerLineKind.While:
                    case TriggerLineKind.Else:
                        {
                            pendingSingleStatementIndent = true;
                            break;
                        }
                }

                previousKind = kind;
                hasPreviousMeaningfulLine = true;
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static TriggerLineKind GetTriggerLineKind(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return TriggerLineKind.Normal;
            }

            var text = line.Trim();

            if (string.Equals(text, "BEGIN TRY", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.BeginTry;
            }

            if (string.Equals(text, "END TRY", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.EndTry;
            }

            if (string.Equals(text, "BEGIN CATCH", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.BeginCatch;
            }

            if (string.Equals(text, "END CATCH", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.EndCatch;
            }

            if (string.Equals(text, "BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.Begin;
            }

            if (string.Equals(text, "END", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.End;
            }

            if (string.Equals(text, "ELSE", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.Else;
            }

            if (text.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.If;
            }

            if (text.StartsWith("WHILE ", StringComparison.OrdinalIgnoreCase))
            {
                return TriggerLineKind.While;
            }

            return TriggerLineKind.Normal;
        }

        private static bool ShouldInsertBlankLine(bool hasPreviousMeaningfulLine, TriggerLineKind previousKind, TriggerLineKind currentKind, string currentLine)
        {
            if (!hasPreviousMeaningfulLine)
            {
                return false;
            }

            if (currentKind == TriggerLineKind.Else || currentKind == TriggerLineKind.BeginCatch)
            {
                return true;
            }

            if (IsMajorTriggerStatement(currentLine) && previousKind != TriggerLineKind.Begin && previousKind != TriggerLineKind.BeginTry
                                                     && previousKind != TriggerLineKind.BeginCatch && previousKind != TriggerLineKind.Else)
            {
                return true;
            }

            return false;
        }

        private static bool IsMajorTriggerStatement(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var text = line.Trim();

            return text.StartsWith("SET NOCOUNT ON", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("IF ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("WHILE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("MERGE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("EXEC ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("EXECUTE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("THROW", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("RAISERROR", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("DECLARE ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("WITH ", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("BEGIN TRY", StringComparison.OrdinalIgnoreCase)
                   || text.StartsWith("BEGIN CATCH", StringComparison.OrdinalIgnoreCase);
        }

        private static bool StartsWithControlStatement(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            return line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase)
                   || line.StartsWith("WHILE ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EndsWithBlockBegin(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            return line.EndsWith(" BEGIN", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(line, "BEGIN", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Shared Parser
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

        private static string RemoveLeadingAs(string text)
        {
            text = SqlServerCreateScriptFormatterHelper.TrimOuterBlankLines(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text.StartsWith("AS ", StringComparison.OrdinalIgnoreCase))
            {
                return text.Substring(2).TrimStart();
            }

            if (string.Equals(text, "AS", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (text.StartsWith("AS\r\n", StringComparison.OrdinalIgnoreCase))
            {
                return text.Substring(4).TrimStart();
            }

            return text;
        }

        private static void AddKeywordIndexes(List<int> indexes, string text, string keyword)
        {
            var index = FindTopLevelKeywordIndex(text, keyword);

            while (index >= 0)
            {
                if (!indexes.Contains(index))
                {
                    indexes.Add(index);
                }

                index = FindTopLevelKeywordIndex(text, keyword, index + keyword.Length);
            }
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

                if (string.Equals(keyword, "GROUP BY", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "GROUP", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "BY", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "ORDER BY", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "ORDER", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "BY", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "UNION ALL", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "UNION", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "INSTEAD OF", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "INSTEAD", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "OF", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "LEFT OUTER JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 2 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "LEFT", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "OUTER", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 2].Text, "JOIN", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "RIGHT OUTER JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 2 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "RIGHT", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "OUTER", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 2].Text, "JOIN", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "FULL OUTER JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 2 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, "FULL", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, "OUTER", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 2].Text, "JOIN", StringComparison.OrdinalIgnoreCase))
                    {
                        return tokenInfos[i].StartIndex;
                    }

                    continue;
                }

                if (string.Equals(keyword, "INNER JOIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(keyword, "LEFT JOIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(keyword, "RIGHT JOIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(keyword, "FULL JOIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(keyword, "CROSS JOIN", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = keyword.Split(' ');

                    if (i + 1 < tokenInfos.Count && string.Equals(tokenInfos[i].Text, parts[0], StringComparison.OrdinalIgnoreCase)
                        && string.Equals(tokenInfos[i + 1].Text, parts[1], StringComparison.OrdinalIgnoreCase))
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

        private static void RemoveOuterBlankLines(List<string> lines)
        {
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
            {
                lines.RemoveAt(0);
            }

            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
            {
                lines.RemoveAt(lines.Count - 1);
            }
        }
        #endregion
    }
}