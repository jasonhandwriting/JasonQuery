using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HideAutoCompletePopup(C1TrueDBGrid grid, QueryEditorAutoCompleteSessionCloseReason closeReason = QueryEditorAutoCompleteSessionCloseReason.ExternalHide)
        {
            if (ReferenceEquals(grid, c1GridAutoCompleteForPeriod))
            {
                _periodAutoCompleteSession?.Close(closeReason);
            }
            else if (ReferenceEquals(grid, c1GridAutoCompleteForSpace))
            {
                _spaceAutoCompleteSession?.Close(closeReason);
            }

            QueryEditorAutoCompletePopupController.Hide(grid);
        }

        private void HidePeriodAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason closeReason = QueryEditorAutoCompleteSessionCloseReason.ExternalHide)
        {
            HideAutoCompletePopup(c1GridAutoCompleteForPeriod, closeReason);
        }

        private void HideSpaceAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason closeReason = QueryEditorAutoCompleteSessionCloseReason.ExternalHide)
        {
            HideAutoCompletePopup(c1GridAutoCompleteForSpace, closeReason);
        }

        private QueryEditorAutoCompleteSession GetActiveAutoCompleteSession()
        {
            return _autoCompleteFacade.GetActiveSession();
        }

        private void HandleAutoCompleteGridKeyDown(QueryEditorAutoCompleteSession session, KeyEventArgs e)
        {
            _autoCompleteFacade.HandleGridKeyDown(session, e);
        }

        private void HandleAutoCompleteGridKeyPress(QueryEditorAutoCompleteSession session, KeyPressEventArgs e)
        {
            _autoCompleteFacade.HandleGridKeyPress(session, e);
        }

        private void HandleAutoCompleteGridMouseDoubleClick(QueryEditorAutoCompleteSession session)
        {
            _autoCompleteFacade.HandleGridMouseDoubleClick(session);
        }

        private bool TryHandleAutoCompleteProcessCmdKey(Keys keyData)
        {
            return _autoCompleteFacade.TryHandleProcessCmdKey(keyData);
        }
    }
}
