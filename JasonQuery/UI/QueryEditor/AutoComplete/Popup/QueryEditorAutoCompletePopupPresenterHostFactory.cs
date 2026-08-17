using C1.Win.C1TrueDBGrid;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Filters;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Workflows;
using System;
using System.Data;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal interface IQueryEditorAutoCompletePopupPresenterHostFactoryHost
    {
        int EditorTextLength { get; }

        void CaptureAutoCompleteMousePosition();

        QueryEditorAutoCompleteSession PeriodSession { get; }

        QueryEditorAutoCompleteSession SpaceSession { get; }

        C1TrueDBGrid PeriodGrid { get; }

        C1TrueDBGrid SpaceGrid { get; }

        DataTable PeriodAutoCompleteTable { get; set; }

        DataTable SpaceAutoCompleteTable { get; set; }

        QueryEditorAutoCompletePopupContextFactory PopupContextFactory { get; }

        QueryEditorAutoCompleteFilterCoordinator FilterCoordinator { get; }

        QueryEditorPeriodTriggerCoordinator PeriodTriggerCoordinator { get; }

        int[] PeriodPopupFetchStyleColumns { get; }

        int[] SpacePopupFetchStyleColumns { get; }
    }

    internal sealed class QueryEditorAutoCompletePopupPresenterHostFactory
    {
        private readonly IQueryEditorAutoCompletePopupPresenterHostFactoryHost _host;

        public QueryEditorAutoCompletePopupPresenterHostFactory
        (
            IQueryEditorAutoCompletePopupPresenterHostFactoryHost host
        )
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public QueryEditorAutoCompletePopupPresenterHostImplementation Create()
        {
            return new QueryEditorAutoCompletePopupPresenterHostImplementation
            (
                () => _host.EditorTextLength,
                _host.CaptureAutoCompleteMousePosition,
                new QueryEditorAutoCompletePopupPresenterHostImplementation.PopupBinding
                {
                    Session = _host.PeriodSession,
                    Grid = _host.PeriodGrid,
                    GetTable = () => _host.PeriodAutoCompleteTable,
                    SetTable = dt => _host.PeriodAutoCompleteTable = dt,
                    CreatePopupContext = request => _host.PopupContextFactory.CreatePeriodPopupContext(request, _host.PeriodPopupFetchStyleColumns),
                    TryApplyKeywordFilterFromEditor = triggerPosition =>
                        _host.FilterCoordinator.TryApplyPeriodKeywordFilterFromEditor
                        (
                            triggerPosition,
                            _host.PeriodTriggerCoordinator.IsCtrlJPeriodFallbackToAll
                        ),
                    FetchStyleColumns = _host.PeriodPopupFetchStyleColumns
                },
                new QueryEditorAutoCompletePopupPresenterHostImplementation.PopupBinding
                {
                    Session = _host.SpaceSession,
                    Grid = _host.SpaceGrid,
                    GetTable = () => _host.SpaceAutoCompleteTable,
                    SetTable = dt => _host.SpaceAutoCompleteTable = dt,
                    CreatePopupContext = request => _host.PopupContextFactory.CreateSpacePopupContext(_host.SpacePopupFetchStyleColumns),
                    TryApplyKeywordFilterFromEditor = triggerPosition => _host.FilterCoordinator.TryApplySpaceKeywordFilterFromEditor(triggerPosition),
                    FetchStyleColumns = _host.SpacePopupFetchStyleColumns
                }
            );
        }
    }
}