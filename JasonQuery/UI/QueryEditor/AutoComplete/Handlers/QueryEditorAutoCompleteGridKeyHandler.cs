using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.Text;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Handlers
{
    /// <summary>
    /// 專管 Grid 取得焦點後 的鍵盤互動。
    /// Enter / Tab / Backspace 放 KeyDown
    /// 英數 / _ 放 KeyPress
    /// </summary>
    internal static class QueryEditorAutoCompleteGridKeyHandler
    {
        public static void HandleKeyDown(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session, KeyEventArgs e)
        {
            if (context == null || session == null || e == null)
            {
                return;
            }

            if (context.Editor == null || session.Grid == null)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        if (session.Grid.Row == 0)
                        {
                            e.Handled = true;
                            session.MarkReturnedToEditorByGridUp();
                            context.MarkKeyUpFromAutoCompleteGrid?.Invoke();
                            context.Editor.Focus();
                        }

                        break;
                    }
                case Keys.Enter:
                case Keys.Tab:
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        if (e.KeyCode == Keys.Tab)
                        {
                            session.MarkCommitByTab();
                        }

                        QueryEditorAutoCompleteCommitHandler.TryCommit(context, session);
                        break;
                    }
                case Keys.Back:
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        HandleBackspace(context, session);
                        break;
                    }
            }
        }

        public static void HandleKeyPress(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session, KeyPressEventArgs e)
        {
            if (context == null || session == null || e == null)
            {
                return;
            }

            if (!TextHelper.IsEngAlphabetOrNumber(e.KeyChar, '_'))
            {
                return;
            }

            e.Handled = true;
            HandleInput(context, session, e.KeyChar);
        }

        private static void HandleBackspace(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session)
        {
            var editor = context.Editor;
            var caret = QueryEditorAutoCompleteRangeHelper.GetSessionCaretPosition(session, editor.CurrentPosition, editor.TextLength);

            if (caret <= 0)
            {
                context.HidePopup?.Invoke(session.Grid);
                session.Close(QueryEditorAutoCompleteSessionCloseReason.Navigation);
                editor.Focus();
                return;
            }

            var deletePosition = caret - 1;

            editor.DeleteRange(deletePosition, 1);
            editor.CurrentPosition = deletePosition;
            editor.SelectionStart = deletePosition;
            editor.SelectionEnd = deletePosition;

            session.CaretPosition = deletePosition;

            editor.Focus();

            if (QueryEditorAutoCompleteSessionPolicy.ShouldHideAfterGridBackspace(editor.CurrentPosition, session.TriggerPosition))
            {
                context.HidePopup?.Invoke(session.Grid);
                session.Close(QueryEditorAutoCompleteSessionCloseReason.Navigation);
            }
        }

        private static void HandleInput(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session, char inputChar)
        {
            var editor = context.Editor;
            var caret = QueryEditorAutoCompleteRangeHelper.GetSessionCaretPosition(session, editor.CurrentPosition, editor.TextLength);

            editor.CurrentPosition = caret;
            editor.SelectionStart = caret;
            editor.SelectionEnd = caret;
            editor.ReplaceSelection(inputChar.ToString());

            session.CaretPosition = editor.CurrentPosition;
            editor.Focus();
        }
    }
}