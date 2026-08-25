using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.QueryEngine.Editor.Editing.AutoReplace;
using JasonQuery.UI.QueryEditor.AutoComplete.Builders;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Filters;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.Workflows;
using JasonQuery.UI.QueryEditor.KeyCommands;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        #region 20260330~20260425 for AutoComplete 重構
        private EditorAutoReplaceService _editorAutoReplaceService;
        private EditorBlockSelectionService _editorBlockSelectionService;
        private EditorCommentService _editorCommentService;
        private EditorIndentService _editorIndentService;
        private EditorLineSelectionService _editorLineSelectionService;
        private EditorSqlFormatterService _editorSqlFormatterService;
        private EditorTextCaseService _editorTextCaseService;
        private EditorWhitespaceCleanupService _editorWhitespaceCleanupService;

        private QueryEditorAutoCompleteUiContext _autoCompleteContext;
        private QueryEditorAutoCompleteSession _periodAutoCompleteSession;
        private QueryEditorAutoCompleteSession _spaceAutoCompleteSession;

        private QueryEditorAutoCompleteSchemaQueryExecutor _autoCompleteSchemaQueryExecutor;
        private QueryEditorAutoCompletePeriodResolver _periodResolver;
        private QueryEditorAutoCompleteSpaceResolver _spaceResolver;

        private QueryEditorAutoCompleteVisiblePopupRefreshHandler _visiblePopupRefresher;
        private QueryEditorAutoCompleteVisiblePopupRefreshWorkflow _visiblePopupRefreshWorkflow;
        private QueryEditorAutoCompleteEditorKeyUpHandler _autoCompleteEditorKeyUpHandler;
        private QueryEditorAutoCompleteEditorKeyUpWorkflow _autoCompleteEditorKeyUpWorkflow;

        private QueryEditorAutoCompleteUiContextBuilder _autoCompleteUiContextBuilder;
        private QueryEditorAutoCompleteDataBuilder _autoCompleteDataBuilder;
        private QueryEditorAutoCompletePopupContextFactory _autoCompletePopupContextFactory;
        private QueryEditorAutoCompleteResolveContextFactory _autoCompleteResolveContextFactory;

        private QueryEditorCtrlJPeriodResolver _queryEditorCtrlJPeriodResolver;
        private QueryEditorPeriodTriggerCoordinator _periodTriggerCoordinator;

        private QueryEditorKeyCommandContext _keyCommandContext;

        private QueryEditorAutoCompletePopupPresenterHostImplementation _autoCompletePopupPresenterHostImplementation;
        private QueryEditorAutoCompleteInteractionComposer _autoCompleteInteractionComposer;
        private QueryEditorAutoCompletePopupPresenterHostFactory _autoCompletePopupPresenterHostFactory;
        private QueryEditorAutoCompletePopupPresenter _autoCompletePopupPresenter;

        private QueryEditorAutoCompleteFilterCoordinator _autoCompleteFilterCoordinator;
        private QueryEditorAutoCompleteSessionCoordinator _autoCompleteSessionCoordinator;
        private QueryEditorAutoCompleteKeyInputWorkflow _autoCompleteKeyInputWorkflow;
        private QueryEditorAutoCompleteWorkflowCoordinator _autoCompleteWorkflowCoordinator;
        private QueryEditorAutoCompleteFacade _autoCompleteFacade;
        #endregion
    }
}
