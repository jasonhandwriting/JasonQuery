using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;

namespace JasonQuery.UI.Forms
{
    //B1-C: MainForm-owned entry point for the future common F7 explorer.
    //No old QueryForm/SchemaBrowserForm grid events are redirected in this batch.
    public partial class MainForm
    {
        internal bool TryDispatchActiveSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            var activeTab = tabControl1.SelectedTab;

            if (activeTab == null)
            {
                return false;
            }

            var receiver = activeTab.Control as ISchemaExplorerActionReceiver;

            return SchemaExplorerActionDispatchBridge.TryDispatch
            (
                CaptureLeftPanelContext(),
                activeTab.AccessibleDescription,
                _currentSourceType,
                receiver,
                selection,
                action
            );
        }
    }
}
