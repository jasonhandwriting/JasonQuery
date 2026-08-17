using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorAutoCompleteVisiblePopupRefreshWorkflowHost
    {
        QueryEditorAutoCompleteSession GetActiveAutoCompleteSession();

        DataTable BuildPeriodAutoCompleteData(string keyword);

        DataTable BuildSpaceAutoCompleteData(string keyword);
    }

    internal sealed class QueryEditorAutoCompleteVisiblePopupRefreshWorkflow
    {
        private readonly IQueryEditorAutoCompleteVisiblePopupRefreshWorkflowHost _host;
        private readonly QueryEditorAutoCompletePopupContextFactory _popupContextFactory;
        private readonly QueryEditorAutoCompleteVisiblePopupRefreshHandler _visiblePopupRefresher;
        private readonly QueryEditorAutoCompletePopupPresenter _popupPresenter;

        public QueryEditorAutoCompleteVisiblePopupRefreshWorkflow(IQueryEditorAutoCompleteVisiblePopupRefreshWorkflowHost host,
                                                                  QueryEditorAutoCompletePopupContextFactory popupContextFactory,
                                                                  QueryEditorAutoCompleteVisiblePopupRefreshHandler visiblePopupRefresher,
                                                                  QueryEditorAutoCompletePopupPresenter popupPresenter)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _popupContextFactory = popupContextFactory ?? throw new ArgumentNullException(nameof(popupContextFactory));
            _visiblePopupRefresher = visiblePopupRefresher ?? throw new ArgumentNullException(nameof(visiblePopupRefresher));
            _popupPresenter = popupPresenter ?? throw new ArgumentNullException(nameof(popupPresenter));
        }

        public void RefreshOnEditorKeyUp(KeyEventArgs e)
        {
            var session = _host.GetActiveAutoCompleteSession();

            if (session == null)
            {
                return;
            }

            switch (session.Kind)
            {
                case QueryEditorAutoCompletePopupKind.Period:
                    {
                        var context = _popupContextFactory.CreateVisiblePopupRefreshContext
                        (
                            session,
                            e,
                            _host.BuildPeriodAutoCompleteData,
                            request => _popupPresenter.ShowResolvedPeriodAutoComplete(request),
                            () => _popupPresenter.ResizePeriodPopup()
                        );

                        _visiblePopupRefresher.TryRefreshForwardTriggerPopup(context);
                        break;
                    }

                case QueryEditorAutoCompletePopupKind.Space:
                    {
                        var context = _popupContextFactory.CreateVisiblePopupRefreshContext
                        (
                            session,
                            e,
                            _host.BuildSpaceAutoCompleteData,
                            request => _popupPresenter.ShowResolvedSpaceAutoComplete(request),
                            () => _popupPresenter.ResizeSpacePopup()
                        );

                        _visiblePopupRefresher.TryRefreshForwardTriggerPopup(context);
                        break;
                    }
            }
        }
    }
}