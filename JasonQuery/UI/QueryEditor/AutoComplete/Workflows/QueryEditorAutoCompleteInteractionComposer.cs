using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Filters;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal sealed class QueryEditorAutoCompleteInteractionComposer
    {
        public QueryEditorAutoCompletePopupPresenter CreatePopupPresenter
        (
            QueryEditorAutoCompletePopupPresenterHostImplementation popupPresenterHostImplementation
        )
        {
            if (popupPresenterHostImplementation == null)
            {
                throw new ArgumentNullException(nameof(popupPresenterHostImplementation));
            }

            return new QueryEditorAutoCompletePopupPresenter
            (
                popupPresenterHostImplementation
            );
        }

        public QueryEditorAutoCompleteInteractionBundle ComposeWorkflows
        (
            QueryEditorAutoCompletePopupPresenter popupPresenter,
            QueryEditorAutoCompletePopupContextFactory popupContextFactory,
            QueryEditorAutoCompleteVisiblePopupRefreshHandler visiblePopupRefresher,
            QueryEditorAutoCompleteEditorKeyUpHandler editorKeyUpHandler,
            QueryEditorAutoCompleteFilterCoordinator filterCoordinator,
            QueryEditorPeriodTriggerCoordinator periodTriggerCoordinator,
            Func<QueryEditorAutoCompleteSession> getActiveSession,
            Func<string, DataTable> buildPeriodAutoCompleteData,
            Func<string, DataTable> buildSpaceAutoCompleteData,
            Func<bool> isAutoListMembersEnabled,
            Action clearHighlightSelectionCopyState,
            Func<KeyEventArgs, bool> shouldSkipEditorContentCheckOnKeyUp,
            Func<bool> tryConsumeCompoundCtrlShiftKeyUp,
            Func<KeyEventArgs, bool> shouldIgnoreEditorKeyUp,
            Action<KeyEventArgs> handleAutoReplaceOnSpaceKeyUp,
            Func<QueryEditorAutoCompleteEditorKeyUpContext> createEditorKeyUpContext,
            Action checkEditorContent,
            Action<Exception> showException
        )
        {
            if (popupPresenter == null)
            {
                throw new ArgumentNullException(nameof(popupPresenter));
            }

            if (popupContextFactory == null)
            {
                throw new ArgumentNullException(nameof(popupContextFactory));
            }

            if (visiblePopupRefresher == null)
            {
                throw new ArgumentNullException(nameof(visiblePopupRefresher));
            }

            if (editorKeyUpHandler == null)
            {
                throw new ArgumentNullException(nameof(editorKeyUpHandler));
            }

            if (filterCoordinator == null)
            {
                throw new ArgumentNullException(nameof(filterCoordinator));
            }

            if (periodTriggerCoordinator == null)
            {
                throw new ArgumentNullException(nameof(periodTriggerCoordinator));
            }

            if (getActiveSession == null)
            {
                throw new ArgumentNullException(nameof(getActiveSession));
            }

            if (buildPeriodAutoCompleteData == null)
            {
                throw new ArgumentNullException(nameof(buildPeriodAutoCompleteData));
            }

            if (buildSpaceAutoCompleteData == null)
            {
                throw new ArgumentNullException(nameof(buildSpaceAutoCompleteData));
            }

            if (isAutoListMembersEnabled == null)
            {
                throw new ArgumentNullException(nameof(isAutoListMembersEnabled));
            }

            if (clearHighlightSelectionCopyState == null)
            {
                throw new ArgumentNullException(nameof(clearHighlightSelectionCopyState));
            }

            if (shouldSkipEditorContentCheckOnKeyUp == null)
            {
                throw new ArgumentNullException(nameof(shouldSkipEditorContentCheckOnKeyUp));
            }

            if (tryConsumeCompoundCtrlShiftKeyUp == null)
            {
                throw new ArgumentNullException(nameof(tryConsumeCompoundCtrlShiftKeyUp));
            }

            if (shouldIgnoreEditorKeyUp == null)
            {
                throw new ArgumentNullException(nameof(shouldIgnoreEditorKeyUp));
            }

            if (handleAutoReplaceOnSpaceKeyUp == null)
            {
                throw new ArgumentNullException(nameof(handleAutoReplaceOnSpaceKeyUp));
            }

            if (createEditorKeyUpContext == null)
            {
                throw new ArgumentNullException(nameof(createEditorKeyUpContext));
            }

            if (checkEditorContent == null)
            {
                throw new ArgumentNullException(nameof(checkEditorContent));
            }

            if (showException == null)
            {
                throw new ArgumentNullException(nameof(showException));
            }

            var visiblePopupRefreshWorkflow = new QueryEditorAutoCompleteVisiblePopupRefreshWorkflow
            (
                new VisiblePopupRefreshWorkflowHost
                (
                    getActiveSession,
                    buildPeriodAutoCompleteData,
                    buildSpaceAutoCompleteData
                ),
                popupContextFactory,
                visiblePopupRefresher,
                popupPresenter
            );

            TryHandleAutoCompleteEditorKeyUpDelegate tryHandleEditorKeyUp =
                (KeyEventArgs e, ref bool checkEditorContentRef) =>
                    editorKeyUpHandler.TryHandle(createEditorKeyUpContext(), e, ref checkEditorContentRef);

            var editorKeyUpWorkflow = new QueryEditorAutoCompleteEditorKeyUpWorkflow
            (
                new EditorKeyUpWorkflowHost
                (
                    isAutoListMembersEnabled,
                    clearHighlightSelectionCopyState,
                    shouldSkipEditorContentCheckOnKeyUp,
                    tryConsumeCompoundCtrlShiftKeyUp,
                    shouldIgnoreEditorKeyUp,
                    handleAutoReplaceOnSpaceKeyUp,
                    tryHandleEditorKeyUp,
                    periodTriggerCoordinator.TryTriggerPeriodAutoCompleteOnIdentifierKeyUp,
                    filterCoordinator.TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp,
                    visiblePopupRefreshWorkflow.RefreshOnEditorKeyUp,
                    checkEditorContent,
                    showException
                )
            );

            return new QueryEditorAutoCompleteInteractionBundle
            {
                VisiblePopupRefreshWorkflow = visiblePopupRefreshWorkflow,
                EditorKeyUpWorkflow = editorKeyUpWorkflow
            };
        }

        private sealed class VisiblePopupRefreshWorkflowHost : IQueryEditorAutoCompleteVisiblePopupRefreshWorkflowHost
        {
            private readonly Func<QueryEditorAutoCompleteSession> _getActiveSession;
            private readonly Func<string, DataTable> _buildPeriodAutoCompleteData;
            private readonly Func<string, DataTable> _buildSpaceAutoCompleteData;

            public VisiblePopupRefreshWorkflowHost(Func<QueryEditorAutoCompleteSession> getActiveSession, Func<string, DataTable> buildPeriodAutoCompleteData,
                                                   Func<string, DataTable> buildSpaceAutoCompleteData)
            {
                _getActiveSession = getActiveSession ?? throw new ArgumentNullException(nameof(getActiveSession));
                _buildPeriodAutoCompleteData = buildPeriodAutoCompleteData ?? throw new ArgumentNullException(nameof(buildPeriodAutoCompleteData));
                _buildSpaceAutoCompleteData = buildSpaceAutoCompleteData ?? throw new ArgumentNullException(nameof(buildSpaceAutoCompleteData));
            }

            public QueryEditorAutoCompleteSession GetActiveAutoCompleteSession()
            {
                return _getActiveSession();
            }

            public DataTable BuildPeriodAutoCompleteData(string keyword)
            {
                return _buildPeriodAutoCompleteData(keyword);
            }

            public DataTable BuildSpaceAutoCompleteData(string keyword)
            {
                return _buildSpaceAutoCompleteData(keyword);
            }
        }

        private sealed class EditorKeyUpWorkflowHost : IQueryEditorAutoCompleteEditorKeyUpWorkflowHost
        {
            private readonly Func<bool> _isAutoListMembersEnabled;
            private readonly Action _clearHighlightSelectionCopyState;
            private readonly Func<KeyEventArgs, bool> _shouldSkipEditorContentCheckOnKeyUp;
            private readonly Func<bool> _tryConsumeCompoundCtrlShiftKeyUp;
            private readonly Func<KeyEventArgs, bool> _shouldIgnoreEditorKeyUp;
            private readonly Action<KeyEventArgs> _handleAutoReplaceOnSpaceKeyUp;
            private readonly TryHandleAutoCompleteEditorKeyUpDelegate _tryHandleAutoCompleteEditorKeyUp;
            private readonly Func<KeyEventArgs, bool> _tryTriggerPeriodAutoCompleteOnIdentifierKeyUp;
            private readonly Func<KeyEventArgs, bool> _tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp;
            private readonly Action<KeyEventArgs> _refreshVisibleAutoCompleteOnEditorKeyUp;
            private readonly Action _checkEditorContent;
            private readonly Action<Exception> _showException;

            public EditorKeyUpWorkflowHost(Func<bool> isAutoListMembersEnabled, Action clearHighlightSelectionCopyState, Func<KeyEventArgs, bool> shouldSkipEditorContentCheckOnKeyUp,
                                           Func<bool> tryConsumeCompoundCtrlShiftKeyUp, Func<KeyEventArgs, bool> shouldIgnoreEditorKeyUp,
                                           Action<KeyEventArgs> handleAutoReplaceOnSpaceKeyUp, TryHandleAutoCompleteEditorKeyUpDelegate tryHandleAutoCompleteEditorKeyUp,
                                           Func<KeyEventArgs, bool> tryTriggerPeriodAutoCompleteOnIdentifierKeyUp, Func<KeyEventArgs, bool> tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp,
                                           Action<KeyEventArgs> refreshVisibleAutoCompleteOnEditorKeyUp, Action checkEditorContent, Action<Exception> showException)
            {
                _isAutoListMembersEnabled = isAutoListMembersEnabled ?? throw new ArgumentNullException(nameof(isAutoListMembersEnabled));
                _clearHighlightSelectionCopyState = clearHighlightSelectionCopyState ?? throw new ArgumentNullException(nameof(clearHighlightSelectionCopyState));
                _shouldSkipEditorContentCheckOnKeyUp = shouldSkipEditorContentCheckOnKeyUp ?? throw new ArgumentNullException(nameof(shouldSkipEditorContentCheckOnKeyUp));
                _tryConsumeCompoundCtrlShiftKeyUp = tryConsumeCompoundCtrlShiftKeyUp ?? throw new ArgumentNullException(nameof(tryConsumeCompoundCtrlShiftKeyUp));
                _shouldIgnoreEditorKeyUp = shouldIgnoreEditorKeyUp ?? throw new ArgumentNullException(nameof(shouldIgnoreEditorKeyUp));
                _handleAutoReplaceOnSpaceKeyUp = handleAutoReplaceOnSpaceKeyUp ?? throw new ArgumentNullException(nameof(handleAutoReplaceOnSpaceKeyUp));
                _tryHandleAutoCompleteEditorKeyUp = tryHandleAutoCompleteEditorKeyUp ?? throw new ArgumentNullException(nameof(tryHandleAutoCompleteEditorKeyUp));
                _tryTriggerPeriodAutoCompleteOnIdentifierKeyUp = tryTriggerPeriodAutoCompleteOnIdentifierKeyUp ?? throw new ArgumentNullException(nameof(tryTriggerPeriodAutoCompleteOnIdentifierKeyUp));
                _tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp = tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp ?? throw new ArgumentNullException(nameof(tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp));
                _refreshVisibleAutoCompleteOnEditorKeyUp = refreshVisibleAutoCompleteOnEditorKeyUp ?? throw new ArgumentNullException(nameof(refreshVisibleAutoCompleteOnEditorKeyUp));
                _checkEditorContent = checkEditorContent ?? throw new ArgumentNullException(nameof(checkEditorContent));
                _showException = showException ?? throw new ArgumentNullException(nameof(showException));
            }

            public bool IsAutoListMembersEnabled
            {
                get { return _isAutoListMembersEnabled(); }
            }

            public void ClearHighlightSelectionCopyState()
            {
                _clearHighlightSelectionCopyState();
            }

            public bool ShouldSkipEditorContentCheckOnKeyUp(KeyEventArgs e)
            {
                return _shouldSkipEditorContentCheckOnKeyUp(e);
            }

            public bool TryConsumeCompoundCtrlShiftKeyUp()
            {
                return _tryConsumeCompoundCtrlShiftKeyUp();
            }

            public bool ShouldIgnoreEditorKeyUp(KeyEventArgs e)
            {
                return _shouldIgnoreEditorKeyUp(e);
            }

            public void HandleAutoReplaceOnSpaceKeyUp(KeyEventArgs e)
            {
                _handleAutoReplaceOnSpaceKeyUp(e);
            }

            public bool TryHandleAutoCompleteEditorKeyUp(KeyEventArgs e, ref bool checkEditorContent)
            {
                return _tryHandleAutoCompleteEditorKeyUp(e, ref checkEditorContent);
            }

            public bool TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(KeyEventArgs e)
            {
                return _tryTriggerPeriodAutoCompleteOnIdentifierKeyUp(e);
            }

            public bool TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(KeyEventArgs e)
            {
                return _tryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(e);
            }

            public void RefreshVisibleAutoCompleteOnEditorKeyUp(KeyEventArgs e)
            {
                _refreshVisibleAutoCompleteOnEditorKeyUp(e);
            }

            public void CheckEditorContent()
            {
                _checkEditorContent();
            }

            public void ShowException(Exception ex)
            {
                _showException(ex);
            }
        }

        internal delegate bool TryHandleAutoCompleteEditorKeyUpDelegate(KeyEventArgs e, ref bool checkEditorContent);
    }
}
