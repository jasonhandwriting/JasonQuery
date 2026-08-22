using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using System;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal interface IQueryEditorAutoCompleteWorkflowHost
    {
        int EditorCurrentPosition { get; }

        QueryEditorAutoCompletePeriodResolveContext CreatePeriodResolveContext();

        QueryEditorAutoCompleteSpaceResolveContext CreateSpaceResolveContext();

        void HidePeriodPopup();

        void HideSpacePopup();

        void ShowResolveErrorStatus();

        void ClearResolveErrorStatus();

        void ShowException(Exception ex);
    }

    internal enum QueryEditorAutoCompleteFeedbackAction
    {
        None = 0,
        ShowResolveError,
        ClearResolveError
    }

    internal static class QueryEditorAutoCompleteFeedbackPolicy
    {
        public static QueryEditorAutoCompleteFeedbackAction Resolve(bool queryExecutionFailed, bool autoCompleteSucceeded)
        {
            if (autoCompleteSucceeded)
            {
                return QueryEditorAutoCompleteFeedbackAction.ClearResolveError;
            }

            return queryExecutionFailed ? QueryEditorAutoCompleteFeedbackAction.ShowResolveError : QueryEditorAutoCompleteFeedbackAction.None;
        }
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

                if (!_periodResolver.TryResolve(resolveContext, triggerPositionOverride, out var request, out var queryExecutionFailed))
                {
                    _host.HidePeriodPopup();
                    ApplyFeedback(QueryEditorAutoCompleteFeedbackPolicy.Resolve(queryExecutionFailed, false));

                    return false;
                }

                var initialCaretPosition = triggerPositionOverride == 0 ? _host.EditorCurrentPosition : request.TriggerPosition;
                var succeeded = _popupPresenter.TryPresentPeriod(request, initialCaretPosition);

                ApplyFeedback(QueryEditorAutoCompleteFeedbackPolicy.Resolve(false, succeeded));
                return succeeded;
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

                if (!_spaceResolver.TryResolve(resolveContext, out var request, out var queryExecutionFailed))
                {
                    _host.HideSpacePopup();
                    ApplyFeedback(QueryEditorAutoCompleteFeedbackPolicy.Resolve(queryExecutionFailed, false));

                    return false;
                }

                var initialCaretPosition = triggerPositionOverride == 0 ? _host.EditorCurrentPosition : triggerPositionOverride;
                var succeeded = _popupPresenter.TryPresentSpace(request, initialCaretPosition);

                ApplyFeedback(QueryEditorAutoCompleteFeedbackPolicy.Resolve(false, succeeded));
                return succeeded;
            }
            catch (Exception ex)
            {
                _host.HideSpacePopup();
                _host.ShowException(ex);
                return false;
            }
        }

        private void ApplyFeedback(QueryEditorAutoCompleteFeedbackAction action)
        {
            switch (action)
            {
                case QueryEditorAutoCompleteFeedbackAction.ShowResolveError:
                    {
                        _host.ShowResolveErrorStatus();
                        break;
                    }
                case QueryEditorAutoCompleteFeedbackAction.ClearResolveError:
                    {
                        _host.ClearResolveErrorStatus();
                        break;
                    }
            }
        }
    }
}