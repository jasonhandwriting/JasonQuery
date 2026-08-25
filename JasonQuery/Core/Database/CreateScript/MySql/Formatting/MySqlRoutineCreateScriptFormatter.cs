using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlRoutineCreateScriptFormatter : MySqlCreateScriptFormatterBase
    {
        private sealed class RoutineLine
        {
            public string Text { get; set; }
            public bool HasSemicolon { get; set; }
        }

        private enum RoutineLineKind
        {
            Normal,
            Begin,
            End,
            IfThen,
            ElseIfThen,
            Else,
            EndIf,
            WhileDo,
            EndWhile,
            Repeat,
            Until,
            EndRepeat,
            Loop,
            EndLoop
        }

        private readonly string _schemaType;

        public MySqlRoutineCreateScriptFormatter(string schemaType)
        {
            _schemaType = schemaType ?? string.Empty;
        }

        protected override bool MatchesSchemaType(string schemaType)
        {
            return string.Equals(schemaType, _schemaType, StringComparison.Ordinal);
        }

        protected override string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);

            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            var isFunction = SchemaObjectTypeHelper.Is(context.SchemaType, SchemaObjectNames.Functions);
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

            var bodyStartIndex = FindRoutineBodyStartIndex(script, closeParenIndex + 1);

            if (bodyStartIndex < 0)
            {
                return script;
            }

            var header = script.Substring(0, openParenIndex).TrimEnd();
            var parameterText = script.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            var tailText = script.Substring(closeParenIndex + 1, bodyStartIndex - closeParenIndex - 1).Trim();
            var bodyText = script.Substring(bodyStartIndex).TrimStart();

            var listParameters = MySqlCreateScriptFormatterHelper.SplitTopLevelCommaItems(parameterText)
                                                                 .Select(x => x.Trim())
                                                                 .Where(x => !string.IsNullOrWhiteSpace(x))
                                                                 .ToList();

            var sb = new StringBuilder();

            if (listParameters.Count <= 0)
            {
                sb.Append($"{header}()");
            }
            else if (listParameters.Count == 1)
            {
                sb.Append($"{header}({listParameters[0]})");
            }
            else
            {
                sb.AppendLine(header);
                sb.AppendLine("(");

                for (var i = 0; i < listParameters.Count; i++)
                {
                    var suffix = i == listParameters.Count - 1 ? string.Empty : ",";

                    sb.AppendLine($"    {listParameters[i]}{suffix}");
                }

                sb.Append(")");
            }

            var clauses = SplitRoutineTailClauses(tailText, isFunction);

            foreach (var clause in clauses)
            {
                sb.AppendLine();
                sb.Append(clause);
            }

            var formattedBody = MySqlFormatterCommonHelper.FormatStoredProgramBody(bodyText);

            if (!string.IsNullOrWhiteSpace(formattedBody))
            {
                sb.AppendLine();
                sb.Append(formattedBody);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        #region Routine Declaration
        private static int FindRoutineBodyStartIndex(string script, int searchStart)
        {
            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(script);

            foreach (var token in tokens)
            {
                if (token.StartIndex < searchStart)
                {
                    continue;
                }

                if (string.Equals(token.Text, "BEGIN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "RETURN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "SELECT", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "INSERT", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "UPDATE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "DELETE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "SET", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "IF", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "CASE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "WHILE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "REPEAT", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "LOOP", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "DECLARE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "LEAVE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token.Text, "ITERATE", StringComparison.OrdinalIgnoreCase))
                {
                    return token.StartIndex;
                }
            }

            return -1;
        }

        private static List<string> SplitRoutineTailClauses(string tailText, bool isFunction)
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

            var clauseTokenIndexes = new List<int>();
            var i = 0;

            while (i < tokens.Count)
            {
                if (isFunction && string.Equals(tokens[i].Text, "RETURNS", StringComparison.OrdinalIgnoreCase))
                {
                    clauseTokenIndexes.Add(i);
                    i++;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "NOT", "DETERMINISTIC"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 2;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "DETERMINISTIC"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 1;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "LANGUAGE", "SQL"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 2;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "CONTAINS", "SQL"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 2;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "NO", "SQL"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 2;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "READS", "SQL", "DATA"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 3;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "MODIFIES", "SQL", "DATA"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 3;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "SQL", "SECURITY", "DEFINER"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 3;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "SQL", "SECURITY", "INVOKER"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 3;
                    continue;
                }

                if (MatchesPhrase(tokens, i, "COMMENT"))
                {
                    clauseTokenIndexes.Add(i);
                    i += 1;
                    continue;
                }

                i++;
            }

            clauseTokenIndexes = clauseTokenIndexes.Distinct().OrderBy(x => x).ToList();

            if (clauseTokenIndexes.Count <= 0)
            {
                results.Add(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(tailText));
                return results;
            }

            for (var j = 0; j < clauseTokenIndexes.Count; j++)
            {
                var startTokenIndex = clauseTokenIndexes[j];
                var startCharIndex = tokens[startTokenIndex].StartIndex;
                var endCharIndex = j == clauseTokenIndexes.Count - 1 ? tailText.Length : tokens[clauseTokenIndexes[j + 1]].StartIndex;
                var clause = tailText.Substring(startCharIndex, endCharIndex - startCharIndex).Trim();

                if (!string.IsNullOrWhiteSpace(clause))
                {
                    results.Add(MySqlCreateScriptFormatterHelper.NormalizeSimpleLines(clause));
                }
            }

            return results;
        }
        #endregion

        #region Split Helpers
        private static bool MatchesPhrase(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens, int startIndex, params string[] words)
        {
            return MySqlFormatterCommonHelper.MatchesPhrase(tokens, startIndex, words);
        }
        #endregion
    }
}
