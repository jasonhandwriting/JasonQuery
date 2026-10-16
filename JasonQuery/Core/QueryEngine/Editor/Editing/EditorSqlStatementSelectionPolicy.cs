using System;
using System.Collections.Generic;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    //Improvement #6: identify an interactive statement without changing the legacy
    //blank-row block used by Space/Period AutoComplete. This is not a SQL parser.
    internal static class EditorSqlStatementSelectionPolicy
    {
        public static EditorSelectionRange Resolve(string text, int caretPosition, DataSourceType dataSourceType, EditorSelectionRange legacyRange)
        {
            if (legacyRange == null || string.IsNullOrEmpty(text))
            {
                return legacyRange ?? new EditorSelectionRange();
            }

            var blockStart = Math.Max(0, Math.Min(legacyRange.Start, text.Length));
            var blockEnd = Math.Max(blockStart, Math.Min(legacyRange.End, text.Length));
            var caret = Math.Max(blockStart, Math.Min(caretPosition, blockEnd));

            if (blockStart == blockEnd || dataSourceType == DataSourceType.None)
            {
                return legacyRange;
            }

            var tokenization = SqlTokenizer.Tokenize(text, dataSourceType);
            var semicolons = new List<int>();
            var expectsStatementKeyword = true;
            var hasStatementKeyword = false;

            foreach (var token in tokenization.Tokens)
            {
                if (token.EndExclusive <= blockStart)
                {
                    continue;
                }

                if (token.Start >= blockEnd)
                {
                    break;
                }

                //Do not parse a legacy block that intersects a string/comment
                //spanning the blank-row boundary. The old behavior is safer.
                if (token.Start < blockStart || token.EndExclusive > blockEnd)
                {
                    if (token.Kind == SqlTokenKind.StringLiteral || token.Kind == SqlTokenKind.BlockComment || token.Kind == SqlTokenKind.DelimitedIdentifier)
                    {
                        return legacyRange;
                    }
                }

                if (token.Kind == SqlTokenKind.StringLiteral || token.Kind == SqlTokenKind.BlockComment || token.Kind == SqlTokenKind.DelimitedIdentifier)
                {
                    //An unfinished protected region at the document end is ambiguous. Preserve the old selection rather than guess.
                    if (token.EndExclusive == text.Length && IsPossiblyUnterminated(text, token))
                    {
                        return legacyRange;
                    }
                }

                if (token.Kind == SqlTokenKind.Word)
                {
                    if (IsComplexScriptKeyword(token.Text))
                    {
                        return legacyRange;
                    }

                    if (expectsStatementKeyword)
                    {
                        if (!IsSimpleStatementKeyword(token.Text))
                        {
                            return legacyRange;
                        }

                        hasStatementKeyword = true;
                        expectsStatementKeyword = false;
                    }
                }

                if (token.Depth == 0 && token.IsSymbol(";"))
                {
                    semicolons.Add(token.Start);
                    expectsStatementKeyword = true;
                }
            }

            //Unknown or procedural dialect text cannot be safely segmented solely on semicolons; keep the historically selected block.
            if (!hasStatementKeyword || semicolons.Count == 0)
            {
                return legacyRange;
            }

            var previous = blockStart - 1;
            var next = -1;

            foreach (var delimiter in semicolons)
            {
                if (delimiter < caret)
                {
                    previous = delimiter;
                    continue;
                }

                next = delimiter;
                break;
            }

            var start = Math.Max(blockStart, previous + 1);
            var end = next >= 0 ? next + 1 : blockEnd;

            while (start < end && char.IsWhiteSpace(text[start]))
            {
                start++;
            }

            //If the caret is after the final semicolon in whitespace, select the final statement rather than a blank fragment.
            if (start >= end && previous >= blockStart)
            {
                var last = previous;

                previous = blockStart - 1;

                foreach (var delimiter in semicolons)
                {
                    if (delimiter >= last)
                    {
                        break;
                    }

                    previous = delimiter;
                }

                start = Math.Max(blockStart, previous + 1);
                end = last + 1;

                while (start < end && char.IsWhiteSpace(text[start]))
                {
                    start++;
                }
            }

            if (start >= end || start < blockStart || end > blockEnd)
            {
                return legacyRange;
            }

            return new EditorSelectionRange
            {
                Start = start,
                End = end,
                FirstNonSpaceStart = 0
            };
        }

        private static bool IsSimpleStatementKeyword(string word)
        {
            switch ((word ?? string.Empty).ToUpperInvariant())
            {
                case "SELECT":
                case "WITH":
                case "INSERT":
                case "UPDATE":
                case "DELETE":
                case "MERGE":
                case "VALUES":
                case "EXPLAIN":
                case "SHOW":
                case "DESCRIBE":
                case "DESC":
                    {
                        return true;
                    }
            }

            return false;
        }

        private static bool IsComplexScriptKeyword(string word)
        {
            switch ((word ?? string.Empty).ToUpperInvariant())
            {
                case "BEGIN":
                case "END":
                case "DECLARE":
                case "EXCEPTION":
                case "LOOP":
                case "WHILE":
                case "IF":
                case "CASE":
                case "TRY":
                case "CATCH":
                case "DELIMITER":
                case "PROCEDURE":
                case "FUNCTION":
                case "TRIGGER":
                case "PACKAGE":
                case "CREATE":
                case "ALTER":
                case "DO":
                case "GO":
                case "EXEC":
                case "EXECUTE":
                    {
                        return true;
                    }
            }

            return false;
        }

        private static bool IsPossiblyUnterminated(string text, SqlToken token)
        {
            if (token.Kind == SqlTokenKind.BlockComment)
            {
                return !token.Text.EndsWith("*/", StringComparison.Ordinal);
            }

            if (token.Kind == SqlTokenKind.DelimitedIdentifier && token.Text.Length > 0)
            {
                var opener = token.Text[0];
                var closer = opener == '[' ? ']' : opener;

                return token.Text.Length < 2 || token.Text[token.Text.Length - 1] != closer;
            }

            if (token.Kind != SqlTokenKind.StringLiteral || token.Text.Length == 0)
            {
                return false;
            }

            if (token.Text[0] == '\'')
            {
                return token.Text.Length < 2 || token.Text[token.Text.Length - 1] != '\'';
            }

            //Dollar and alternative quoting formats can be validated by the token itself, but ambiguity always falls back conservatively.
            if (token.Text[0] == '$')
            {
                var second = token.Text.IndexOf('$', 1);

                if (second < 0)
                {
                    return true;
                }

                var delimiter = token.Text.Substring(0, second + 1);

                return token.Text.Length < delimiter.Length * 2 || !token.Text.EndsWith(delimiter, StringComparison.Ordinal);
            }

            return false;
        }
    }
}
