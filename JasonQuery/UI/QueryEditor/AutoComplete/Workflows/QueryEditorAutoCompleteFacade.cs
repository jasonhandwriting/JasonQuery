using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal sealed class QueryEditorAutoCompleteFacade
    {
        private readonly QueryEditorAutoCompleteSessionCoordinator _sessionCoordinator;
        private readonly QueryEditorAutoCompleteKeyInputWorkflow _keyInputWorkflow;
        private readonly QueryEditorAutoCompleteWorkflowCoordinator _workflowCoordinator;

        public QueryEditorAutoCompleteFacade
        (
            QueryEditorAutoCompleteSessionCoordinator sessionCoordinator,
            QueryEditorAutoCompleteKeyInputWorkflow keyInputWorkflow,
            QueryEditorAutoCompleteWorkflowCoordinator workflowCoordinator
        )
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _keyInputWorkflow = keyInputWorkflow ?? throw new ArgumentNullException(nameof(keyInputWorkflow));
            _workflowCoordinator = workflowCoordinator ?? throw new ArgumentNullException(nameof(workflowCoordinator));
        }

        public QueryEditorAutoCompleteSession GetActiveSession()
        {
            return _sessionCoordinator.GetActiveSession();
        }

        public bool TryCommitSelection(QueryEditorAutoCompleteSession session)
        {
            return _sessionCoordinator.TryCommitSelection(session);
        }

        public void HandleGridKeyDown(QueryEditorAutoCompleteSession session, KeyEventArgs e)
        {
            _sessionCoordinator.HandleGridKeyDown(session, e);
        }

        public void HandleGridKeyPress(QueryEditorAutoCompleteSession session, KeyPressEventArgs e)
        {
            _sessionCoordinator.HandleGridKeyPress(session, e);
        }

        public void HandleGridMouseDoubleClick(QueryEditorAutoCompleteSession session)
        {
            _sessionCoordinator.HandleGridMouseDoubleClick(session);
        }

        public void HandleEditorKeyDown(KeyEventArgs e)
        {
            _keyInputWorkflow.HandleEditorKeyDown(e);
        }

        public bool TryHandleProcessCmdKey(Keys keyData)
        {
            return _keyInputWorkflow.TryHandleProcessCmdKey(keyData);
        }

        public bool HandlePeriod(int triggerPositionOverride = 0)
        {
            return _workflowCoordinator.HandlePeriod(triggerPositionOverride);
        }

        public bool HandleSpace(int triggerPositionOverride = 0)
        {
            return _workflowCoordinator.HandleSpace(triggerPositionOverride);
        }
    }
}
