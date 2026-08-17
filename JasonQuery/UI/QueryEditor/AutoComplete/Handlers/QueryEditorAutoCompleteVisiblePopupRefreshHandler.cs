using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.Core.Text;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using System;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Handlers
{
    internal sealed class QueryEditorAutoCompleteVisiblePopupRefreshHandler
    {
        public bool TryRefreshForwardTriggerPopup(QueryEditorAutoCompleteVisiblePopupRefreshContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var session = context.Session;

            if (session == null || !session.IsVisible)
            {
                return false;
            }

            if (ShouldHideOnNavigationKey(context))
            {
                HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                return true;
            }

            if (context.Editor.CurrentPosition <= session.TriggerPosition - 1)
            {
                HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.Navigation);
                return true;
            }

            if (!TryGetForwardTriggerWord(context, out var word))
            {
                HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.InvalidInput);
                return true;
            }

            var filterExpression = string.Format("[ColumnName] LIKE '{0}*'", word);
            var dt = context.BuildData(filterExpression);

            if (dt == null || dt.Rows.Count == 0)
            {
                HidePopup(context, session, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                return true;
            }

            context.ShowResolvedRequest
            (
                new QueryEditorAutoCompleteRequest
                {
                    TriggerPosition = session.TriggerPosition,
                    Data = dt
                }
            );

            context.AfterShow?.Invoke();
            return true;
        }


        private static void HidePopup(QueryEditorAutoCompleteVisiblePopupRefreshContext context,
                                      JasonQuery.UI.QueryEditor.AutoComplete.State.QueryEditorAutoCompleteSession session,
                                      QueryEditorAutoCompleteSessionCloseReason closeReason)
        {
            context.HidePopup(session.Grid);
            session.Close(closeReason);
        }

        private bool ShouldHideOnNavigationKey(QueryEditorAutoCompleteVisiblePopupRefreshContext context)
        {
            switch (context.KeyEventArgs.KeyCode)
            {
                case System.Windows.Forms.Keys.Home:
                case System.Windows.Forms.Keys.PageUp:
                case System.Windows.Forms.Keys.PageDown:
                    {
                        return true;
                    }
                case System.Windows.Forms.Keys.End:
                    {
                        return ShouldHideOnEndKey(context);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private bool ShouldHideOnEndKey(QueryEditorAutoCompleteVisiblePopupRefreshContext context)
        {
            var session = context.Session;
            var editor = context.Editor;
            var currentPosition = editor.CurrentPosition;
            var text = editor.Text ?? string.Empty;

            if (currentPosition < session.TriggerPosition - 1)
            {
                return true;
            }

            for (var i = session.TriggerPosition; i < currentPosition && i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    return true;
                }

                if (text[i] == '\r' || text[i] == '\n')
                {
                    break;
                }
            }

            return false;
        }

        private bool TryGetForwardTriggerWord(QueryEditorAutoCompleteVisiblePopupRefreshContext context, out string word)
        {
            word = string.Empty;

            var session = context.Session;
            var text = context.Editor.Text ?? string.Empty;
            var triggerPosition = session.TriggerPosition;

            if (triggerPosition < 0 || triggerPosition > text.Length)
            {
                return false;
            }

            var end = triggerPosition;

            for (var i = triggerPosition; i < text.Length; i++)
            {
                char currentChar = text[i];

                if (TextHelper.IsEngAlphabetOrNumber(currentChar, '_'))
                {
                    end = i + 1;
                    continue;
                }

                if (!IsAllowedSqlFollowingChar(currentChar))
                {
                    return false;
                }

                break;
            }

            if (end > triggerPosition)
            {
                word = text.Substring(triggerPosition, end - triggerPosition);
            }
            else
            {
                word = string.Empty;
            }

            return true;
        }

        private bool IsAllowedSqlFollowingChar(char currentChar)
        {
            switch (currentChar)
            {
                case ' ':
                case '\r':
                case '\n':
                case '=':
                case '>':
                case '<':
                case '!':
                case '-':
                case '+':
                case '*':
                case '/':
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}