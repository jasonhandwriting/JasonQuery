using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using System;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Handlers
{
    internal static class QueryEditorAutoCompleteCommitHandler //「把目前 Grid 選取列寫回 editor」
    {
        public static bool TryCommit(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session)
        {
            if (context == null || session == null)
            {
                return false;
            }

            if (context.Editor == null || session.Grid == null || !session.Grid.Visible)
            {
                return false;
            }

            if (session.Grid.Row < 0)
            {
                return false;
            }

            var cellText = Convert.ToString(session.Grid[session.Grid.Row, 0]) ?? string.Empty;

            if (string.IsNullOrEmpty(cellText))
            {
                return false;
            }

            var editor = context.Editor;
            var editorText = editor.Text ?? string.Empty;

            QueryEditorAutoCompleteRangeHelper.GetReplaceRange(session, editorText, editor.CurrentPosition, out int start, out int endExclusive);

            editor.SelectionStart = start;
            editor.SelectionEnd = endExclusive;
            editor.ReplaceSelection(cellText);

            session.CaretPosition = editor.CurrentPosition;

            context.HidePopup?.Invoke(session.Grid);

            session.Close
            (
                session.CommitByTab
                    ? QueryEditorAutoCompleteSessionCloseReason.CommitByTab
                    : QueryEditorAutoCompleteSessionCloseReason.Commit
            );

            editor.Focus();
            return true;
        }
    }
}