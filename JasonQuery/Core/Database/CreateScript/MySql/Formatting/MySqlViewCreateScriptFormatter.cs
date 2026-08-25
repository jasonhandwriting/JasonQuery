using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlViewCreateScriptFormatter : MySqlCreateScriptFormatterBase
    {
        protected override bool MatchesSchemaType(string schemaType)
        {
            return string.Equals(schemaType, SchemaObjectNames.Views, StringComparison.Ordinal);
        }

        protected override string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);

            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            if (!TrySplitDeclarationAndQuery(script, out var declaration, out var query))
            {
                return script;
            }

            var formattedDeclaration = FormatViewDeclaration(declaration);
            var formattedQuery = FormatViewQuery(query);

            if (string.IsNullOrWhiteSpace(formattedDeclaration))
            {
                return string.IsNullOrWhiteSpace(formattedQuery) ? "AS" : $"AS\r\n{formattedQuery}";
            }

            if (string.IsNullOrWhiteSpace(formattedQuery))
            {
                return $"{formattedDeclaration}\r\nAS";
            }

            return $"{formattedDeclaration}\r\nAS\r\n{formattedQuery}";
        }

        private static bool TrySplitDeclarationAndQuery(string script, out string declaration, out string query)
        {
            return MySqlFormatterCommonHelper.TrySplitViewDeclarationAndQuery(script, out declaration, out query);
        }

        private static string FormatViewDeclaration(string declaration)
        {
            declaration = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(declaration);
            declaration = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            var lines = declaration.Split(new[] { "\r\n" }, StringSplitOptions.None)
                                   .Select(x => x.Trim())
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .ToList();

            return string.Join("\r\n", lines);
        }

        private static string FormatViewQuery(string query)
        {
            query = MySqlCreateScriptFormatterHelper.NormalizeLineEndings(query);
            query = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(query);

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
            var previousClause = string.Empty;

            foreach (var clause in clauses)
            {
                if (string.IsNullOrWhiteSpace(clause))
                {
                    continue;
                }

                if (sb.Length > 0 && ShouldInsertBlankLineBeforeClause(previousClause, clause))
                {
                    sb.AppendLine();
                }

                if (clause.StartsWith("WITH ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatWithClause(clause));
                }
                else if (clause.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatSelectClause(clause));
                }
                else if (clause.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(FormatFromClause(clause));
                }
                else if (StartsWithJoinClause(clause))
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
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
                else if (clause.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
                else if (clause.StartsWith("UNION ALL", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("UNION", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("EXCEPT", StringComparison.OrdinalIgnoreCase)
                         || clause.StartsWith("INTERSECT", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
                else
                {
                    sb.Append(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }

                previousClause = clause;
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static List<string> SplitTopLevelQueryClauses(string query)
        {
            return MySqlFormatterCommonHelper.SplitTopLevelQueryClauses
            (
                query,
                "WITH",
                "SELECT",
                "FROM",
                "INNER JOIN",
                "LEFT OUTER JOIN",
                "LEFT JOIN",
                "RIGHT OUTER JOIN",
                "RIGHT JOIN",
                "CROSS JOIN",
                "JOIN",
                "ON",
                "WHERE",
                "GROUP BY",
                "HAVING",
                "UNION ALL",
                "UNION",
                "EXCEPT",
                "INTERSECT",
                "ORDER BY",
                "LIMIT"
            );
        }

        private static string FormatWithClause(string clause)
        {
            var rest = clause.Substring(4).Trim();

            if (string.IsNullOrWhiteSpace(rest))
            {
                return "WITH";
            }

            var items = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(rest);

            if (items.Count <= 0)
            {
                return $"WITH {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(rest)}";
            }

            var sb = new StringBuilder();

            sb.AppendLine("WITH");

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

        private static string FormatSelectClause(string clause)
        {
            var rest = clause.Substring(6).Trim();

            if (string.IsNullOrWhiteSpace(rest))
            {
                return "SELECT";
            }

            return FormatHangingListClause("SELECT ", rest);
        }

        private static string FormatFromClause(string clause)
        {
            var rest = clause.Substring(4).Trim();

            if (string.IsNullOrWhiteSpace(rest))
            {
                return "  FROM";
            }

            return FormatHangingListClause("  FROM ", rest);
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
                return $"{keyword}\r\n    {MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(conditionText)}";
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

            string prefix;

            if (string.Equals(keyword, "GROUP BY", StringComparison.OrdinalIgnoreCase))
            {
                prefix = " GROUP BY ";
            }
            else if (string.Equals(keyword, "ORDER BY", StringComparison.OrdinalIgnoreCase))
            {
                prefix = " ORDER BY ";
            }
            else
            {
                prefix = $"{keyword} ";
            }

            return FormatHangingListClause(prefix, listText);
        }

        private static bool ShouldInsertBlankLineBeforeClause(string previousClause, string currentClause)
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
                   || currentClause.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase);
        }

        private static bool StartsWithJoinClause(string clause)
        {
            return clause.StartsWith("INNER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("LEFT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("RIGHT OUTER JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("CROSS JOIN ", StringComparison.OrdinalIgnoreCase)
                   || clause.StartsWith("JOIN ", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> SplitTopLevelLogicalConditions(string text)
        {
            return MySqlFormatterCommonHelper.SplitTopLevelLogicalConditions(text);
        }

        private static void AppendIndentedMultiLine(StringBuilder sb, string text, int indentSpaces, string lastLineSuffix)
        {
            MySqlFormatterCommonHelper.AppendIndentedMultiLine(sb, text, indentSpaces, lastLineSuffix);
        }

        private static string FormatHangingListClause(string prefix, string listText)
        {
            var items = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(listText)
                                                        .Select(x => MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(x).Trim())
                                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                                        .ToList();

            if (items.Count <= 0)
            {
                return prefix.TrimEnd();
            }

            if (items.Count == 1)
            {
                return $"{prefix}{items[0]}";
            }

            var sb = new StringBuilder();

            sb.Append(prefix).Append(items[0]);

            for (var i = 1; i < items.Count; i++)
            {
                sb.AppendLine(",");
                sb.Append(new string(' ', prefix.Length));
                sb.Append(items[i]);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
    }
}
