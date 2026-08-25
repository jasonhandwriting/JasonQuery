using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorAutoCompleteSessionCoordinatorHost
    {
        QueryEditorAutoCompleteUiContext AutoCompleteContext { get; }

        QueryEditorAutoCompleteSession PeriodSession { get; }

        QueryEditorAutoCompleteSession SpaceSession { get; }

        void ShowException(Exception ex);
    }

    internal sealed class QueryEditorAutoCompleteSessionCoordinator
    {
        private readonly IQueryEditorAutoCompleteSessionCoordinatorHost _host;

        public QueryEditorAutoCompleteSessionCoordinator(IQueryEditorAutoCompleteSessionCoordinatorHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public QueryEditorAutoCompleteSession GetActiveSession()
        {
            if (_host.PeriodSession != null && _host.PeriodSession.IsVisible)
            {
                return _host.PeriodSession;
            }

            if (_host.SpaceSession != null && _host.SpaceSession.IsVisible)
            {
                return _host.SpaceSession;
            }

            return null;
        }

        public bool TryCommitSelection(QueryEditorAutoCompleteSession session)
        {
            return QueryEditorAutoCompleteCommitHandler.TryCommit(_host.AutoCompleteContext, session);
        }

        public void HandleGridKeyDown(QueryEditorAutoCompleteSession session, KeyEventArgs e)
        {
            try
            {
                QueryEditorAutoCompleteGridKeyHandler.HandleKeyDown(_host.AutoCompleteContext, session, e);
            }
            catch (Exception ex)
            {
                _host.ShowException(ex);
            }
        }

        public void HandleGridKeyPress(QueryEditorAutoCompleteSession session, KeyPressEventArgs e)
        {
            try
            {
                QueryEditorAutoCompleteGridKeyHandler.HandleKeyPress(_host.AutoCompleteContext, session, e);
            }
            catch (Exception ex)
            {
                _host.ShowException(ex);
            }
        }

        public void HandleGridMouseDoubleClick(QueryEditorAutoCompleteSession session)
        {
            try
            {
                TryCommitSelection(session);
            }
            catch (Exception ex)
            {
                _host.ShowException(ex);
            }
        }

        public void HandleEditorKeyDown(KeyEventArgs e)
        {
            try
            {
                var session = GetActiveSession();

                if (session == null)
                {
                    return;
                }

                QueryEditorAutoCompleteEditorKeyHandler.HandleKeyDown(_host.AutoCompleteContext, session, e);
            }
            catch (Exception ex)
            {
                _host.ShowException(ex);
            }
        }

        public bool TryHandleProcessCmdKey(Keys keyData, Func<bool> tryHandleCtrlJ)
        {
            if (keyData == (Keys.Control | Keys.J))
            {
                return tryHandleCtrlJ != null && tryHandleCtrlJ();
            }

            var session = GetActiveSession();

            if (session == null || session.Grid == null || !session.IsVisible)
            {
                return false;
            }

            var editor = _host.AutoCompleteContext?.Editor;
            var isEditorFocused = editor != null && editor.Focused;
            var isGridFocused = session.Grid.Focused;

            switch (keyData)
            {
                case Keys.Escape:
                    {
                        if (!isEditorFocused && !isGridFocused)
                        {
                            return false;
                        }

                        return HandleEscape(session);
                    }

                case Keys.Tab:
                    {
                        if (!isEditorFocused && !isGridFocused)
                        {
                            return false;
                        }

                        session.MarkCommitByTab();
                        return TryCommitSelection(session);
                    }
                case Keys.Enter:
                    {
                        if (isGridFocused)
                        {
                            session.ResetTransientFlags();
                            return TryCommitSelection(session);
                        }

                        if (isEditorFocused)
                        {
                            ForwardEditorKeyDown(session, keyData);
                            return true;
                        }

                        return false;
                    }
                case Keys.Down:
                    {
                        if (isEditorFocused)
                        {
                            ForwardEditorKeyDown(session, keyData);
                            return true;
                        }

                        return false;
                    }
                case Keys.Up:
                    {
                        if (isGridFocused && session.Grid.Row == 0)
                        {
                            ForwardGridKeyDown(session, keyData);
                            return true;
                        }

                        if (isEditorFocused && session.HideOnEditorUp)
                        {
                            ForwardEditorKeyDown(session, keyData);
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool HandleEscape(QueryEditorAutoCompleteSession session)
        {
            if (session == null)
            {
                return false;
            }

            var triggerPosition = session.TriggerPosition;

            _host.AutoCompleteContext?.HidePopup?.Invoke(session.Grid);
            session.Close(QueryEditorAutoCompleteSessionCloseReason.Escape);

            RestoreEditorCaret(triggerPosition);

            var editor = _host.AutoCompleteContext?.Editor;

            if (editor != null)
            {
                session.CaretPosition = editor.CurrentPosition;
            }

            return true;
        }

        private void RestoreEditorCaret(int position)
        {
            var editor = _host.AutoCompleteContext?.Editor;

            if (editor == null)
            {
                return;
            }

            var safePosition = QueryEditorAutoCompleteSessionPolicy.ClampEditorPosition(position, editor.TextLength);

            editor.CurrentPosition = safePosition;
            editor.SelectionStart = safePosition;
            editor.SelectionEnd = safePosition;
            editor.Focus();
        }

        private void ForwardEditorKeyDown(QueryEditorAutoCompleteSession session, Keys keyData)
        {
            var e = new KeyEventArgs(keyData);

            QueryEditorAutoCompleteEditorKeyHandler.HandleKeyDown(_host.AutoCompleteContext, session, e);
        }

        private void ForwardGridKeyDown(QueryEditorAutoCompleteSession session, Keys keyData)
        {
            var e = new KeyEventArgs(keyData);

            QueryEditorAutoCompleteGridKeyHandler.HandleKeyDown(_host.AutoCompleteContext, session, e);
        }
    }
}
