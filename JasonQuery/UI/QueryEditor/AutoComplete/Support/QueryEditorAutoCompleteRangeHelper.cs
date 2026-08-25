using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.State;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteRangeHelper //把 Period / Space / All 的有效範圍與 replace 範圍集中
    {
        public static void GetActiveRange(QueryEditorAutoCompleteSession session, string editorText, out int start, out int endExclusive)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetActiveRange(ToRangeMode(session), editorText, session?.TriggerPosition ?? 0);

            start = range.Start;
            endExclusive = range.EndExclusive;
        }

        public static void GetReplaceRange(QueryEditorAutoCompleteSession session, string editorText, int currentPosition,
                                           out int start, out int endExclusive)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetReplaceRange
            (
                ToRangeMode(session),
                editorText,
                session?.TriggerPosition ?? 0,
                session?.CaretPosition ?? currentPosition,
                currentPosition
            );

            start = range.Start;
            endExclusive = range.EndExclusive;
        }

        public static int GetSessionCaretPosition(QueryEditorAutoCompleteSession session, int currentPosition, int textLength)
        {
            return QueryEditorAutoCompleteRangeResolver.GetSessionCaretPosition(session?.CaretPosition ?? currentPosition, currentPosition, textLength);
        }

        private static QueryEditorAutoCompleteRangeMode ToRangeMode(QueryEditorAutoCompleteSession session)
        {
            if (session == null)
            {
                return QueryEditorAutoCompleteRangeMode.ForwardFromTrigger;
            }

            return session.ReplaceMode == QueryEditorAutoCompleteReplaceMode.WholeIdentifier
                   ? QueryEditorAutoCompleteRangeMode.WholeIdentifier
                   : QueryEditorAutoCompleteRangeMode.ForwardFromTrigger;
        }
    }
}
