using JasonQuery.Core.Config;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Text;
using JasonQuery.UI.LeftPanel;
using System;

namespace JasonQuery.UI.Forms
{
    //F7 (Schema Information / Schema Explorer): typed receiver, per QueryForm.
    //F6 (AutoReplace) remains independently owned by each QueryForm.
    public partial class QueryForm : ISchemaExplorerActionReceiver
    {
        public string TabIdentity => AccessibleDescription;

        public LeftPanelTargetKind TargetKind => LeftPanelTargetKind.QueryForm;

        public bool TryHandleSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            if (IsDisposed || InvokeRequired || !_isFormLoadFinished || editor == null || editor.IsDisposed || editor.ReadOnly)
            {
                return false;
            }

            if (!SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(selection, action, out var pasteText))
            {
                return false;
            }

            try
            {
                TextHelper.CopyTextToClipboard(pasteText, "F7TypedQueryDoubleClick");
                editor.Paste();

                if (AppConfigHelper.IsAfterPasteFocusOnQueryEditor)
                {
                    editor.Focus();
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return false;
            }
        }
    }
}
