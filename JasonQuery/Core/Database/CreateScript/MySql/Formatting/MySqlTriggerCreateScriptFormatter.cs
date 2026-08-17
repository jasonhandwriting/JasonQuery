using JasonQuery.Core.Database.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal sealed class MySqlTriggerCreateScriptFormatter : MySqlCreateScriptFormatterBase
    {
        private sealed class TriggerLine
        {
            public string Text { get; set; }
            public bool HasSemicolon { get; set; }
        }

        private enum TriggerLineKind
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

        protected override bool MatchesSchemaType(string schemaType)
        {
            return SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Triggers);
        }

        protected override string FormatScript(string script, MySqlCreateScriptFormatContext context)
        {
            script = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(script);

            if (string.IsNullOrWhiteSpace(script))
            {
                return string.Empty;
            }

            if (!TrySplitDeclarationAndBody(script, out var declaration, out var body))
            {
                return script;
            }

            var formattedDeclaration = FormatTriggerDeclaration(declaration);
            var formattedBody = FormatTriggerBody(body);

            if (string.IsNullOrWhiteSpace(formattedBody))
            {
                return formattedDeclaration;
            }

            return $"{formattedDeclaration}\r\n{formattedBody}";
        }

        #region Declaration
        private static bool TrySplitDeclarationAndBody(string script, out string declaration, out string body)
        {
            return MySqlFormatterCommonHelper.TrySplitDeclarationAndBodyAfterPhrase
            (
                script,
                out declaration,
                out body,
                "FOR",
                "EACH",
                "ROW"
            );
        }

        private static string FormatTriggerDeclaration(string declaration)
        {
            declaration = MySqlCreateScriptFormatterHelper.TrimOuterBlankLines(declaration);

            if (string.IsNullOrWhiteSpace(declaration))
            {
                return string.Empty;
            }

            var tokens = MySqlCreateScriptFormatterHelper.TokenizeTopLevelWords(declaration);

            if (tokens.Count <= 0)
            {
                return declaration;
            }

            var triggerIndex = MySqlFormatterCommonHelper.FindWordIndex(tokens, "TRIGGER");
            var forIndex = MySqlFormatterCommonHelper.FindPhraseIndex(tokens, "FOR", "EACH", "ROW");
            var timingIndex = FindTimingIndex(tokens);
            var lines = new List<string>();

            if (triggerIndex > 0)
            {
                lines.Add(declaration.Substring(0, tokens[triggerIndex].StartIndex).Trim());
            }

            if (triggerIndex >= 0)
            {
                var triggerEnd = MySqlFormatterCommonHelper.GetEndIndex(tokens, triggerIndex, declaration.Length, timingIndex, forIndex);

                lines.Add(declaration.Substring(tokens[triggerIndex].StartIndex, triggerEnd - tokens[triggerIndex].StartIndex).Trim());
            }

            if (timingIndex >= 0)
            {
                var timingEnd = MySqlFormatterCommonHelper.GetEndIndex(tokens, timingIndex, declaration.Length, forIndex);

                lines.Add(declaration.Substring(tokens[timingIndex].StartIndex, timingEnd - tokens[timingIndex].StartIndex).Trim());
            }

            if (forIndex >= 0)
            {
                lines.Add(declaration.Substring(tokens[forIndex].StartIndex).Trim());
            }

            return string.Join("\r\n", lines.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static int FindTimingIndex(List<MySqlCreateScriptFormatterHelper.TokenInfo> tokens)
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                if (string.Equals(tokens[i].Text, "BEFORE", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(tokens[i].Text, "AFTER", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion

        #region Trigger Body Beautifier
        private static string FormatTriggerBody(string body)
        {
            return MySqlFormatterCommonHelper.FormatStoredProgramBody(body);
        }
        #endregion
    }
}