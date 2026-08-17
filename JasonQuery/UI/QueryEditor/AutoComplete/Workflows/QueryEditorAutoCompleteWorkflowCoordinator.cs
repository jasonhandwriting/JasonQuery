using System;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorAutoCompleteWorkflowHost
    {
        int EditorCurrentPosition { get; }

        QueryEditorAutoCompletePeriodResolveContext CreatePeriodResolveContext();

        QueryEditorAutoCompleteSpaceResolveContext CreateSpaceResolveContext();

        void HidePeriodPopup();

        void HideSpacePopup();

        void ShowException(Exception ex);
    }

    internal sealed class QueryEditorAutoCompleteWorkflowCoordinator
    {
        private readonly IQueryEditorAutoCompleteWorkflowHost _host;
        private readonly QueryEditorAutoCompletePeriodResolver _periodResolver;
        private readonly QueryEditorAutoCompleteSpaceResolver _spaceResolver;
        private readonly QueryEditorAutoCompletePopupPresenter _popupPresenter;

        public QueryEditorAutoCompleteWorkflowCoordinator(IQueryEditorAutoCompleteWorkflowHost host, QueryEditorAutoCompletePeriodResolver periodResolver,
                                                          QueryEditorAutoCompleteSpaceResolver spaceResolver, QueryEditorAutoCompletePopupPresenter popupPresenter)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _periodResolver = periodResolver ?? throw new ArgumentNullException(nameof(periodResolver));
            _spaceResolver = spaceResolver ?? throw new ArgumentNullException(nameof(spaceResolver));
            _popupPresenter = popupPresenter ?? throw new ArgumentNullException(nameof(popupPresenter));
        }

        public bool HandlePeriod(int triggerPositionOverride = 0)
        {
            try
            {
                var resolveContext = _host.CreatePeriodResolveContext();

                if (!_periodResolver.TryResolve(resolveContext, triggerPositionOverride, out var request))
                {
                    _host.HidePeriodPopup();
                    return false;
                }

                var initialCaretPosition = triggerPositionOverride == 0 ? _host.EditorCurrentPosition : request.TriggerPosition;

                return _popupPresenter.TryPresentPeriod(request, initialCaretPosition);
            }
            catch (Exception ex)
            {
                _host.HidePeriodPopup();
                _host.ShowException(ex);
                return false;
            }
        }

        public bool HandleSpace(int triggerPositionOverride = 0)
        {
            try
            {
                var resolveContext = _host.CreateSpaceResolveContext();

                if (!_spaceResolver.TryResolve(resolveContext, out var request))
                {
                    _host.HideSpacePopup();
                    return false;
                }

                var initialCaretPosition = triggerPositionOverride == 0 ? _host.EditorCurrentPosition : triggerPositionOverride;

                return _popupPresenter.TryPresentSpace(request, initialCaretPosition);
            }
            catch (Exception ex)
            {
                _host.HideSpacePopup();
                _host.ShowException(ex);
                return false;
            }
        }
    }
}