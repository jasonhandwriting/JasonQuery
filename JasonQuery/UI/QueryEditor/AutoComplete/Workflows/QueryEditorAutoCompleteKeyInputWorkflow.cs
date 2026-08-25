using System;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Workflows
{
    internal sealed class QueryEditorAutoCompleteKeyInputWorkflow
    {
        private readonly QueryEditorAutoCompleteSessionCoordinator _sessionCoordinator;
        private readonly QueryEditorPeriodTriggerCoordinator _periodTriggerCoordinator;

        public QueryEditorAutoCompleteKeyInputWorkflow(QueryEditorAutoCompleteSessionCoordinator sessionCoordinator, QueryEditorPeriodTriggerCoordinator periodTriggerCoordinator)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _periodTriggerCoordinator = periodTriggerCoordinator ?? throw new ArgumentNullException(nameof(periodTriggerCoordinator));
        }

        public void HandleEditorKeyDown(KeyEventArgs e)
        {
            _sessionCoordinator.HandleEditorKeyDown(e);
        }

        public bool TryHandleProcessCmdKey(Keys keyData)
        {
            return _sessionCoordinator.TryHandleProcessCmdKey(keyData, _periodTriggerCoordinator.TryHandleCtrlJAutoComplete);
        }
    }
}
