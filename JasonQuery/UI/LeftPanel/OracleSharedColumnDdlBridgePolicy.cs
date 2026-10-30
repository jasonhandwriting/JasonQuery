using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //C2: only a live, qualified Oracle ColumnInfo DataRow can authorize a column DDL form.
    //UI captions and DatabaseSqlExecutor.DbUser must never supply the target name or owner.
    internal static class OracleSharedColumnDdlBridgePolicy
    {
        public static bool TryQualifySelection(SchemaExplorerSelectionInfo selection, DataRow row, string snapshotVerifiedUser)
        {
            if (selection == null)
            {
                return false;
            }

            selection.HasQualifiedOracleColumnIdentity = false;
            selection.HasQualifiedOracleSelectedOwner = false;
            selection.OracleSelectedOwner = string.Empty;
            selection.OracleMetadataTableName = string.Empty;
            selection.OracleMetadataColumnName = string.Empty;

            if (selection.SourceType != DataSourceType.Oracle
                || selection.Kind != SchemaExplorerSelectionKind.ColumnInfo
                || !selection.IsColumnInfoRow || selection.IsGroupRow
                || !SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                || row == null || row.Table == null
                || !row.Table.Columns.Contains("OracleMetadataOwner")
                || !row.Table.Columns.Contains("OracleMetadataTableName")
                || !row.Table.Columns.Contains("OracleMetadataColumnName"))
            {
                return false;
            }

            var owner = Convert.ToString(row["OracleMetadataOwner"]);
            var table = Convert.ToString(row["OracleMetadataTableName"]);
            var column = Convert.ToString(row["OracleMetadataColumnName"]);

            if (string.IsNullOrWhiteSpace(snapshotVerifiedUser) || !string.Equals(owner, snapshotVerifiedUser, StringComparison.Ordinal) || !CanQuote(table) || !CanQuote(column))
            {
                return false;
            }

            selection.OracleSelectedOwner = owner;
            selection.OracleMetadataTableName = table;
            selection.OracleMetadataColumnName = column;
            selection.HasQualifiedOracleSelectedOwner = true;
            selection.HasQualifiedOracleColumnIdentity = true;

            return true;
        }

        public static bool IsQualifiedColumnDataRow(SchemaExplorerSelectionInfo selection)
        {
            return selection != null
                && selection.SourceType == DataSourceType.Oracle
                && selection.Kind == SchemaExplorerSelectionKind.ColumnInfo
                && selection.IsColumnInfoRow && !selection.IsGroupRow
                && SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                && selection.HasQualifiedOracleSelectedOwner && selection.HasQualifiedOracleColumnIdentity
                && !string.IsNullOrWhiteSpace(selection.OracleSelectedOwner)
                && CanQuote(selection.OracleMetadataTableName)
                && CanQuote(selection.OracleMetadataColumnName);
        }

        public static bool IsSameColumnDataRow(SchemaExplorerSelectionInfo captured, SchemaExplorerSelectionInfo current)
        {
            return IsQualifiedColumnDataRow(captured) && IsQualifiedColumnDataRow(current)
                && captured.DisplayRowIndex == current.DisplayRowIndex
                && string.Equals(captured.SchemaType, current.SchemaType, StringComparison.Ordinal)
                && string.Equals(captured.SchemaName, current.SchemaName, StringComparison.Ordinal)
                && string.Equals(captured.OracleSelectedOwner, current.OracleSelectedOwner, StringComparison.Ordinal)
                && string.Equals(captured.OracleMetadataTableName, current.OracleMetadataTableName, StringComparison.Ordinal)
                && string.Equals(captured.OracleMetadataColumnName, current.OracleMetadataColumnName, StringComparison.Ordinal);
        }

        public static bool TryCreate(SchemaExplorerSelectionInfo selection, string snapshotVerifiedUser, Guid snapshotConnectionId, string currentVerifiedUser, Guid currentConnectionId, out OracleDdlTargetIdentity qualified)
        {
            qualified = null;

            if (!IsQualifiedColumnDataRow(selection)
                || string.IsNullOrWhiteSpace(snapshotVerifiedUser)
                || string.IsNullOrWhiteSpace(currentVerifiedUser)
                || snapshotConnectionId == Guid.Empty || currentConnectionId == Guid.Empty
                || snapshotConnectionId != currentConnectionId
                || !string.Equals(snapshotVerifiedUser, currentVerifiedUser, StringComparison.Ordinal)
                || !string.Equals(selection.OracleSelectedOwner, snapshotVerifiedUser, StringComparison.Ordinal))
            {
                return false;
            }

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

        private static bool CanQuote(string identifier)
        {
            return !string.IsNullOrWhiteSpace(identifier) && !string.IsNullOrEmpty(DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(DataSourceType.Oracle, identifier));
        }
    }
}
