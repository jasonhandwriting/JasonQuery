using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //B3-B2-T Post-GUI: bridge an authoritative shared-F7 Oracle Table data row OR true Table object group row.
    //The policy never infers an owner from DbUser, a login string or a displayed caption.
    internal static class OracleSharedTableDdlBridgePolicy
    {
        public static bool TryCreate(SchemaExplorerSelectionInfo selection, string snapshotVerifiedUser, Guid snapshotConnectionId, string currentVerifiedUser, Guid currentConnectionId, out OracleDdlTargetIdentity qualified)
        {
            qualified = null;

            if (!IsTableObjectRow(selection)
                || string.IsNullOrWhiteSpace(snapshotVerifiedUser)
                || string.IsNullOrWhiteSpace(currentVerifiedUser)
                || snapshotConnectionId == Guid.Empty
                || currentConnectionId == Guid.Empty
                || snapshotConnectionId != currentConnectionId
                || !string.Equals(snapshotVerifiedUser, currentVerifiedUser, StringComparison.Ordinal)
                || !string.Equals(selection.OracleSelectedOwner, snapshotVerifiedUser, StringComparison.Ordinal)
                || string.IsNullOrEmpty(DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.Oracle, selection.SchemaName)))
            {
                return false;
            }

            //Origin: per-row OracleMetadataOwner -> qualified F7 selection -> bound snapshot -> verified OracleReader on the same connection.
            var candidate = new OracleDdlTargetIdentity
            {
                VerifiedSessionUser = snapshotVerifiedUser,
                MetadataOwner = selection.OracleSelectedOwner,
                SelectedOwner = selection.OracleSelectedOwner,
                VerifiedConnectionId = snapshotConnectionId,
                SelectionConnectionId = snapshotConnectionId,
                CurrentConnectionId = currentConnectionId,
                HasQualifiedSelectedOwner = true
            };

            return OracleDdlGuiGate.TryRefresh(candidate, currentVerifiedUser, currentConnectionId, out qualified);
        }

        public static bool IsSameTableDataRow(SchemaExplorerSelectionInfo captured, SchemaExplorerSelectionInfo current)
        {
            //Keep the historical method name for G16/G17 callers; it now also protects a qualified object group row.
            return IsTableObjectRow(captured)
                   && IsTableObjectRow(current)
                   && captured.IsGroupRow == current.IsGroupRow
                   && (!captured.IsGroupRow || captured.GroupLevel == current.GroupLevel)
                   && captured.DisplayRowIndex == current.DisplayRowIndex
                   && string.Equals(captured.SchemaType, current.SchemaType, StringComparison.Ordinal)
                   && string.Equals(captured.SchemaName, current.SchemaName, StringComparison.Ordinal)
                   && string.Equals(captured.OracleSelectedOwner, current.OracleSelectedOwner, StringComparison.Ordinal);
        }

        //A valid object-level C1 group is authorized only when G2 already qualified its owner from all matching metadata rows. SchemaType/category groups and ColumnInfo are never eligible.
        public static bool IsTableObjectRow(SchemaExplorerSelectionInfo selection)
        {
            return selection != null
                   && selection.SourceType == DataSourceType.Oracle
                   && selection.Kind == SchemaExplorerSelectionKind.SchemaObject
                   && !selection.IsColumnInfoRow
                   && (!selection.IsGroupRow || SchemaExplorerGroupLevelPolicy.IsObjectGroupLevel(selection.SourceType, selection.GroupLevel))
                   && SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                   && !string.IsNullOrWhiteSpace(selection.SchemaName)
                   && selection.HasQualifiedOracleSelectedOwner
                   && !string.IsNullOrWhiteSpace(selection.OracleSelectedOwner);
        }
    }
}
