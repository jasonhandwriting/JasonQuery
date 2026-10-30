using JasonQuery.Core.SchemaExplorer.Selection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C3C2A: explicit allowlist. Source-grid display indices never cross the dispatch boundary.
    public enum SharedSchemaExplorerInputKind
    {
        DoubleClick = 1,
        Enter = 2,
        RightClick = 3
    }

    public static class SharedSchemaExplorerInteractionPolicy
    {
        public static bool TryGetAction(LeftPanelTargetKind activeTarget, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerInputKind input, out SchemaExplorerActionKind action)
        {
            action = SchemaExplorerActionKind.SelectionChanged;

            if (selection == null || selection.Kind == SchemaExplorerSelectionKind.Invalid || selection.Kind == SchemaExplorerSelectionKind.None)
            {
                return false;
            }

            //The C1 GroupBy row owns expand/collapse. Never turn its activation into a Query paste.
            if (input != SharedSchemaExplorerInputKind.DoubleClick && input != SharedSchemaExplorerInputKind.Enter)
            {
                return false;
            }

            //The receiver contract requires portable -1 indices, but a C1 event carries a live display index. Normalize for policy validation; the existing bridge later creates its own full semantic copy before invoking the receiver.
            var portable = new SchemaExplorerSelectionInfo
            {
                Kind = selection.Kind,
                SourceType = selection.SourceType,
                DisplayRowIndex = -1,
                GroupLevel = -1,
                IsGroupRow = selection.IsGroupRow,
                IsColumnInfoRow = selection.IsColumnInfoRow,
                SchemaType = selection.SchemaType,
                SchemaName = selection.SchemaName
            };

            if (activeTarget == LeftPanelTargetKind.QueryForm)
            {
                string pasteText;

                if (!SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(portable, SchemaExplorerActionKind.DoubleClick, out pasteText))
                {
                    return false;
                }

                action = SchemaExplorerActionKind.DoubleClick;
                return true;
            }

            if (activeTarget == LeftPanelTargetKind.SchemaBrowserForm && SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(portable, SchemaExplorerActionKind.SelectionChanged))
            {
                action = SchemaExplorerActionKind.SelectionChanged;
                return true;
            }

            return false;
        }

        public static bool CanCopyDisplayName(SchemaExplorerSelectionInfo selection)
        {
            return selection != null && (selection.Kind == SchemaExplorerSelectionKind.SchemaObject || selection.Kind == SchemaExplorerSelectionKind.ColumnInfo || selection.Kind == SchemaExplorerSelectionKind.ExternalObject) && !string.IsNullOrWhiteSpace(selection.SchemaName);
        }
    }
}
