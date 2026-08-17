using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.Text;
using System;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteRangeResolver
    {
        public static QueryEditorAutoCompleteTextRange GetActiveRange(QueryEditorAutoCompleteRangeMode mode, string editorText, int triggerPosition)
        {
            var text = editorText ?? string.Empty;
            var textLength = text.Length;
            var safeTriggerPosition = Clamp(triggerPosition, 0, textLength);

            switch (mode)
            {
                case QueryEditorAutoCompleteRangeMode.ForwardFromTrigger:
                    {
                        return new QueryEditorAutoCompleteTextRange(safeTriggerPosition, FindIdentifierEndExclusive(text, safeTriggerPosition));
                    }
                case QueryEditorAutoCompleteRangeMode.WholeIdentifier:
                    {
                        return new QueryEditorAutoCompleteTextRange(FindIdentifierStartInclusive(text, safeTriggerPosition),
                                                                    FindIdentifierEndExclusive(text, safeTriggerPosition));
                    }
                default:
                    {
                        return new QueryEditorAutoCompleteTextRange(safeTriggerPosition, safeTriggerPosition);
                    }
            }
        }

        public static QueryEditorAutoCompleteTextRange GetReplaceRange(QueryEditorAutoCompleteRangeMode mode, string editorText, int triggerPosition,
                                                                       int sessionCaretPosition, int currentPosition)
        {
            var range = GetActiveRange(mode, editorText, triggerPosition);
            var textLength = (editorText ?? string.Empty).Length;
            var caretHint = Clamp(Math.Min(sessionCaretPosition, currentPosition), 0, textLength);
            var endExclusive = Math.Max(range.EndExclusive, caretHint);

            return new QueryEditorAutoCompleteTextRange(range.Start, endExclusive);
        }

        public static int GetSessionCaretPosition(int sessionCaretPosition, int currentPosition, int textLength)
        {
            return Clamp(Math.Min(sessionCaretPosition, currentPosition), 0, Math.Max(0, textLength));
        }

        private static int FindIdentifierStartInclusive(string text, int position)
        {
            var start = Clamp(position, 0, text == null ? 0 : text.Length);

            while (start > 0 && IsIdentifierChar(text[start - 1]))
            {
                start--;
            }

            return start;
        }

        private static int FindIdentifierEndExclusive(string text, int position)
        {
            var end = Clamp(position, 0, text == null ? 0 : text.Length);

            while (end < text.Length && IsIdentifierChar(text[end]))
            {
                end++;
            }

            return end;
        }

        private static bool IsIdentifierChar(char value)
        {
            return TextHelper.IsEngAlphabetOrNumber(value, '_');
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
            {
                return minimum;
            }

            return value > maximum ? maximum : value;
        }
    }
}