using JasonQuery.Core.Config;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.Builders;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Filters;
using JasonQuery.UI.QueryEditor.AutoComplete.Handlers;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using JasonQuery.UI.QueryEditor.AutoComplete.Workflows;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void InitializeAutoCompleteComposition()
        {
            InitializeAutoCompleteInfrastructure();
            InitializeAutoCompleteSessionCoordinator();
            InitializeAutoCompleteUiHelpers();
            InitializeAutoCompleteDataBuilder();
            InitializeAutoCompletePopupContextFactory();
            InitializeAutoCompleteResolveContextFactory();
            InitializeAutoCompleteResolvers();
            InitializePeriodTriggerCoordinator();
            InitializeAutoCompleteKeyInputWorkflow();
            InitializeAutoCompleteInteractionComposer();
            InitializeAutoCompletePopupPresenterHostFactory();
            InitializeAutoCompletePopupPresenter();
            InitializeAutoCompleteFilterCoordinator();
            InitializeAutoCompleteInteraction();
            InitializeAutoCompleteWorkflowCoordinator();
            InitializeAutoCompleteFacade();
        }

        private void InitializeAutoCompleteInfrastructure()
        {
            _autoCompleteContext = new QueryEditorAutoCompleteUiContext
            {
                Editor = editor,
                HidePopup = grid => HideAutoCompletePopup(grid)
            };

            _periodAutoCompleteSession = new QueryEditorAutoCompleteSession
            (
                QueryEditorAutoCompletePopupKind.Period,
                c1GridAutoCompleteForPeriod,
                QueryEditorAutoCompleteReplaceMode.ForwardFromTrigger
            );

            _spaceAutoCompleteSession = new QueryEditorAutoCompleteSession
            (
                QueryEditorAutoCompletePopupKind.Space,
                c1GridAutoCompleteForSpace,
                QueryEditorAutoCompleteReplaceMode.ForwardFromTrigger
            );
        }

        private void InitializeAutoCompleteResolvers()
        {
            _autoCompleteSchemaQueryExecutor = new QueryEditorAutoCompleteSchemaQueryExecutor(_currentSourceType);
            _periodResolver = new QueryEditorAutoCompletePeriodResolver(_autoCompleteSchemaQueryExecutor);
            _spaceResolver = new QueryEditorAutoCompleteSpaceResolver(_autoCompleteSchemaQueryExecutor);
        }

        private void InitializeAutoCompleteUiHelpers()
        {
            _visiblePopupRefresher = new QueryEditorAutoCompleteVisiblePopupRefreshHandler();
            _autoCompleteEditorKeyUpHandler = new QueryEditorAutoCompleteEditorKeyUpHandler();
            _autoCompleteUiContextBuilder = new QueryEditorAutoCompleteUiContextBuilder
            (
                editor,
                grid => HideAutoCompletePopup(grid)
            );
        }

        private void InitializeAutoCompleteDataBuilder()
        {
            _autoCompleteDataBuilder = new QueryEditorAutoCompleteDataBuilder();
        }

        private void InitializeAutoCompleteFilterCoordinator()
        {
            _autoCompleteFilterCoordinator = new QueryEditorAutoCompleteFilterCoordinator
            (
                new QueryEditorAutoCompleteFilterCoordinatorHostAdapter(this),
                _autoCompletePopupPresenter
            );
        }

        private void InitializeAutoCompleteResolveContextFactory()
        {
            _autoCompleteResolveContextFactory = new QueryEditorAutoCompleteResolveContextFactory
            (
                new QueryEditorAutoCompleteResolveContextFactoryHostAdapter(this)
            );
        }

        private void InitializePeriodTriggerCoordinator()
        {
            _periodTriggerCoordinator = new QueryEditorPeriodTriggerCoordinator
            (
                new QueryEditorPeriodTriggerCoordinatorHostAdapter(this),
                _queryEditorCtrlJPeriodResolver
            );
        }

        private void InitializeAutoCompleteSessionCoordinator()
        {
            _autoCompleteSessionCoordinator = new QueryEditorAutoCompleteSessionCoordinator
            (
                new QueryEditorAutoCompleteSessionCoordinatorHostAdapter(this)
            );
        }

        private void InitializeAutoCompletePopupContextFactory()
        {
            _autoCompletePopupContextFactory = new QueryEditorAutoCompletePopupContextFactory
            (
                new QueryEditorAutoCompletePopupContextFactoryHostAdapter(this),
                _autoCompleteUiContextBuilder
            );
        }

        private void InitializeAutoCompleteInteractionComposer()
        {
            _autoCompleteInteractionComposer = new QueryEditorAutoCompleteInteractionComposer();
        }

        private void InitializeAutoCompleteInteraction()
        {
            var bundle = _autoCompleteInteractionComposer.ComposeWorkflows
            (
                _autoCompletePopupPresenter,
                _autoCompletePopupContextFactory,
                _visiblePopupRefresher,
                _autoCompleteEditorKeyUpHandler,
                _autoCompleteFilterCoordinator,
                _periodTriggerCoordinator,
                GetActiveAutoCompleteSession,
                BuildPeriodAutoCompleteData,
                BuildSpaceAutoCompleteData,
                () => MyGlobal.IsAutoListMembers,
                () => ClearHighlightSelectionCopyState(),
                ShouldSkipEditorContentCheckOnKeyUp,
                TryConsumeCompoundCtrlShiftKeyUp,
                ShouldIgnoreEditorKeyUp,
                HandleAutoReplaceOnSpaceKeyUp,
                CreateEditorKeyUpContext,
                CheckEditorContent,
                ShowExceptionMessage
            );

            _visiblePopupRefreshWorkflow = bundle.VisiblePopupRefreshWorkflow;
            _autoCompleteEditorKeyUpWorkflow = bundle.EditorKeyUpWorkflow;
        }

        private void InitializeAutoCompletePopupPresenter()
        {
            _autoCompletePopupPresenterHostImplementation = _autoCompletePopupPresenterHostFactory.Create();

            _autoCompletePopupPresenter = _autoCompleteInteractionComposer.CreatePopupPresenter
            (
                _autoCompletePopupPresenterHostImplementation
            );
        }

        private void InitializeAutoCompleteKeyInputWorkflow()
        {
            _autoCompleteKeyInputWorkflow = new QueryEditorAutoCompleteKeyInputWorkflow
            (
                _autoCompleteSessionCoordinator,
                _periodTriggerCoordinator
            );
        }

        private void InitializeAutoCompletePopupPresenterHostFactory()
        {
            _autoCompletePopupPresenterHostFactory = new QueryEditorAutoCompletePopupPresenterHostFactory
            (
                new QueryEditorAutoCompletePopupPresenterHostFactoryHostAdapter(this)
            );
        }

        private void InitializeAutoCompleteWorkflowCoordinator()
        {
            _autoCompleteWorkflowCoordinator = new QueryEditorAutoCompleteWorkflowCoordinator
            (
                new QueryEditorAutoCompleteWorkflowHostAdapter(this),
                _periodResolver,
                _spaceResolver,
                _autoCompletePopupPresenter
            );
        }

        private void InitializeAutoCompleteFacade()
        {
            _autoCompleteFacade = new QueryEditorAutoCompleteFacade
            (
                _autoCompleteSessionCoordinator,
                _autoCompleteKeyInputWorkflow,
                _autoCompleteWorkflowCoordinator
            );
        }
    }
}
