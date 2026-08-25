using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Handlers
{
    internal static class QueryEditorAutoCompleteEditorKeyHandler //專管 popup 顯示中，focus 還在 editor 時的鍵盤行為
    {
        public static void HandleKeyDown(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session, KeyEventArgs e)
        {
            if (context == null || session == null || e == null)
            {
                return;
            }

            if (context.Editor == null || !session.IsVisible)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Escape:
                case Keys.Space:
                case Keys.Tab:
                    {
                        HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        break;
                    }
                case Keys.Back:
                    {
                        if (QueryEditorAutoCompleteSessionPolicy.ShouldHideOnEditorBackspace(context.Editor.CurrentPosition, session.TriggerPosition))
                        {
                            HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        }

                        break;
                    }
                case Keys.Up:
                    {
                        if (session.HideOnEditorUp)
                        {
                            HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        }

                        break;
                    }
                case Keys.Down:
                    {
                        session.CaretPosition = context.Editor.CurrentPosition;
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        session.Grid.Focus();
                        break;
                    }
                case Keys.Enter:
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        break;
                    }
                case Keys.Left:
                    {
                        if (ShouldHideOnHorizontalMove(context, session, -1))
                        {
                            HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        }

                        break;
                    }
                case Keys.Right:
                    {
                        if (ShouldHideOnHorizontalMove(context, session, 1))
                        {
                            HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                        }

                        break;
                    }
            }
        }

        private static bool ShouldHideOnHorizontalMove(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session, int delta)
        {
            QueryEditorAutoCompleteRangeHelper.GetActiveRange(session, context.Editor.Text, out int start, out int endExclusive);

            return QueryEditorAutoCompleteSessionPolicy.ShouldHideOnHorizontalMove
            (
                new JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models.QueryEditorAutoCompleteTextRange(start, endExclusive),
                context.Editor.CurrentPosition,
                delta
            );
        }

        private static void HidePopup(QueryEditorAutoCompleteUiContext context, QueryEditorAutoCompleteSession session,
                                      QueryEditorAutoCompleteSessionCloseReason closeReason)
        {
            context.HidePopup?.Invoke(session.Grid);
            session.Close(closeReason);
        }
    }
}
