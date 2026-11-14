using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //Authoritative non-Oracle Column row identity, separated from human-readable captions.
    //This policy never authorizes a database connection, transaction or DDL execution.
    public static class NonOracleSharedColumnDdlBridgePolicy
    {
        public static bool TryQualifySelection(SchemaExplorerSelectionInfo selection, DataRow selectedDataRow)
        {
            if (selection == null)
            {
                return false;
            }

            //A failed qualification or grid rebind must never preserve an old Column identity.
            selection.NonOracleRawColumnName = string.Empty;
            selection.HasQualifiedNonOracleColumnIdentity = false;

            //Requalify the same raw DataRow rather than trusting a cached UI caption or flag.
            if (!NonOracleSharedAddColumnPreviewPolicy.TryQualifyColumn(selection, selectedDataRow)
                || selectedDataRow?.Table == null
                || !selectedDataRow.Table.Columns.Contains("NonOracleMetadataColumnName"))
            {
                return false;
            }

            var rawName = Convert.ToString(selectedDataRow["NonOracleMetadataColumnName"]);

            if (string.IsNullOrWhiteSpace(rawName))
            {
                return false;
            }

            selection.NonOracleRawColumnName = rawName;
            selection.HasQualifiedNonOracleColumnIdentity = true;
            return true;
        }

        public static bool IsQualifiedColumnDataRow(SchemaExplorerSelectionInfo selection)
        {
            return selection != null
                   && selection.HasQualifiedNonOracleColumnIdentity
                   && !string.IsNullOrWhiteSpace(selection.NonOracleRawColumnName)
                   && NonOracleSharedAddColumnPreviewPolicy.IsQualifiedColumn(selection);
        }

        public static bool IsSameColumnDataRow(SchemaExplorerSelectionInfo captured, SchemaExplorerSelectionInfo current)
        {
            return IsQualifiedColumnDataRow(captured)
                   && IsQualifiedColumnDataRow(current)
                   && NonOracleSharedAddColumnPreviewPolicy.IsSameSelection(captured, current)
                   && string.Equals(captured.NonOracleRawColumnName, current.NonOracleRawColumnName, StringComparison.Ordinal);
        }
    }
}
