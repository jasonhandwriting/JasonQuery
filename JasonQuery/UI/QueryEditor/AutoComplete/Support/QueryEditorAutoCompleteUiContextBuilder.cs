using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using ScintillaNET;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Support
{
    internal sealed class QueryEditorAutoCompleteUiContextBuilder
    {
        private readonly Scintilla _editor;
        private readonly Action<C1TrueDBGrid> _hidePopup;

        public QueryEditorAutoCompleteUiContextBuilder(Scintilla editor, Action<C1TrueDBGrid> hidePopup)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _hidePopup = hidePopup ?? throw new ArgumentNullException(nameof(hidePopup));
        }

        public QueryEditorAutoCompleteEditorKeyUpContext CreateEditorKeyUpContext(QueryEditorAutoCompleteSession periodSession,
                                                                                  QueryEditorAutoCompleteSession spaceSession,
                                                                                  bool isCtrlJPressed,
                                                                                  Action resetCtrlJPressed,
                                                                                  Action hideAllAutoCompletePopups,
                                                                                  C1TrueDBGrid periodGrid,
                                                                                  C1TrueDBGrid spaceGrid,
                                                                                  Action triggerPeriodAutoComplete,
                                                                                  Action triggerSpaceAutoComplete)
        {
            return new QueryEditorAutoCompleteEditorKeyUpContext
            {
                PeriodSession = periodSession,
                SpaceSession = spaceSession,
                IsCtrlJPressed = isCtrlJPressed,
                ResetCtrlJPressed = resetCtrlJPressed,
                HideAllAutoCompletePopups = hideAllAutoCompletePopups,
                HidePeriodPopup = () => _hidePopup(periodGrid),
                HideSpacePopup = () => _hidePopup(spaceGrid),
                TriggerPeriodAutoComplete = triggerPeriodAutoComplete,
                TriggerSpaceAutoComplete = triggerSpaceAutoComplete
            };
        }

        public QueryEditorAutoCompleteVisiblePopupRefreshContext CreateVisiblePopupRefreshContext(QueryEditorAutoCompleteSession session,
                                                                                                  KeyEventArgs keyEventArgs,
                                                                                                  Func<string, DataTable> buildData,
                                                                                                  Action<QueryEditorAutoCompleteRequest> showResolvedRequest,
                                                                                                  Action afterShow)
        {
            return new QueryEditorAutoCompleteVisiblePopupRefreshContext
            {
                Editor = _editor,
                Session = session,
                KeyEventArgs = keyEventArgs,
                BuildData = buildData,
                ShowResolvedRequest = showResolvedRequest,
                HidePopup = _hidePopup,
                AfterShow = afterShow
            };
        }
    }
}