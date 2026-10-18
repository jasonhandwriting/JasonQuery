using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C2: only actions with an established form-local behavior are accepted.
    //A central C1 grid's row indices and mouse coordinates never cross this boundary.
    public static class SchemaExplorerReceiverActionPolicy
    {
        public static bool TryGetQueryPasteText(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action, out string pasteText)
        {
            pasteText = string.Empty;

            if (selection == null || action != SchemaExplorerActionKind.DoubleClick || selection.IsGroupRow || selection.DisplayRowIndex != -1 || selection.GroupLevel != -1)
            {
                return false;
            }

            if (selection.Kind != SchemaExplorerSelectionKind.SchemaObject && selection.Kind != SchemaExplorerSelectionKind.ColumnInfo)
            {
                return false;
            }

            if (!SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables) || string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                return false;
            }

            //Match the pre-existing QueryForm double-click rule: retain the comma and
            //following space for a column label written as "column, data type".
            var text = selection.SchemaName;
            var separator = text.IndexOf(", ", StringComparison.Ordinal);

            pasteText = separator >= 0 ? text.Substring(0, separator + 2) : text;
            return !string.IsNullOrWhiteSpace(pasteText);
        }

        public static bool CanDisplayInSchemaBrowser(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            return selection != null
                && action == SchemaExplorerActionKind.SelectionChanged
                && selection.CanDisplayObject
                && !selection.IsColumnInfoRow
                && selection.DisplayRowIndex == -1
                && selection.GroupLevel == -1
                && !string.IsNullOrWhiteSpace(selection.SchemaType)
                && !string.IsNullOrWhiteSpace(selection.SchemaName);
        }
    }
}
