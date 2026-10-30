using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //F7 (Schema Information / Schema Explorer): typed object selection receiver.
    //Never resolve the external selection against this form's private grid row index.
    public partial class SchemaBrowserForm : ISchemaExplorerActionReceiver
    {
        public string TabIdentity => AccessibleDescription;

        public LeftPanelTargetKind TargetKind => LeftPanelTargetKind.SchemaBrowserForm;

        public bool TryHandleSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            if (IsDisposed || InvokeRequired || !_isFormLoadFinished)
            {
                return false;
            }

            if (!SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(selection, action))
            {
                return false;
            }

            //Same edit-protection question as the existing DisplaySchemaInfo2 flow.
            //A refusal must occur before the active selection and data are changed.
            if (_modifiedCells.Count > 0 || _deletedRows.Count > 0 || _newRows.Count > 0)
            {
                var answer = MessageBox.Show(_confirmExitTableEditMessage, tabData.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes)
                {
                    return false;
                }
            }

            //The new overload consumes the portable selection directly, retaining ObjectId, PackageSpecBody and other engine-specific semantic fields.
            //Existing error handling, tab restoration and lazy loading stay in one place.
            DisplaySchemaInfo(-1, selection);
            return true;
        }
    }
}
