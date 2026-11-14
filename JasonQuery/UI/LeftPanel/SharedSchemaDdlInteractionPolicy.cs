using JasonQuery.Core.SchemaExplorer.Selection;

namespace JasonQuery.UI.LeftPanel
{
    //B2c Golden Red contract. These are pure decisions, NOT an Oracle authorization source.
    //A future integration must independently read the current Reader and obtain provenance from the authoritative metadata loader. Never fabricate trusted inputs in the UI.
    public static class SharedSchemaDdlInteractionPolicy
    {
        //Viewing a generator must not silently connect or execute DDL.
        public static bool CanOpenGenerator(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command, bool cachedSnapshotReadable)
        {
            var isTableCommand = command == SharedSchemaExplorerMenuCommand.CommentTable || command == SharedSchemaExplorerMenuCommand.DropTable || command == SharedSchemaExplorerMenuCommand.RenameTable || command == SharedSchemaExplorerMenuCommand.TruncateTable;
            var isColumnCommand = command == SharedSchemaExplorerMenuCommand.AddColumn || command == SharedSchemaExplorerMenuCommand.CommentColumn || command == SharedSchemaExplorerMenuCommand.DropColumn || command == SharedSchemaExplorerMenuCommand.RenameColumn;

            if (!cachedSnapshotReadable || selection == null)
            {
                return false;
            }

            var oracle = selection.SourceType == JasonQuery.Core.Database.Connection.DataSourceType.Oracle;

            //Non-Oracle existing-column generators may open from qualified cached rows.
            //Keep the separate Add Column interaction path unchanged.
            var accepted = oracle ? (isTableCommand || isColumnCommand) : (isTableCommand || (isColumnCommand && command != SharedSchemaExplorerMenuCommand.AddColumn));

            return accepted
                   && (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm)
                   && SharedSchemaExplorerObjectCommandPolicy.CanExecute(target, selection, command);
        }

        //Copying already-generated text is not a database action.
        public static bool CanCopyGeneratedSql(string sqlText, bool liveOracleSessionVerified)
        {
            return !string.IsNullOrWhiteSpace(sqlText);
        }

        //A pure final-decision contract. An actual Execute handler MUST re-check trusted OracleReader, freshly loaded provenance, exact target, and rebuilt SQL.
        //It must also retain OracleDdlGuiGate.CanExecute before ExecuteSingleNonQuery.
        public static bool CanExecuteDirectly(SharedOracleMetadataValidationContext context, bool isColumn, string displayedSql, string rebuiltSql)
        {
            return context != null
                   && !string.IsNullOrWhiteSpace(displayedSql)
                   && !string.IsNullOrWhiteSpace(rebuiltSql)
                   && string.Equals(displayedSql, rebuiltSql, System.StringComparison.Ordinal)
                   && SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, isColumn);
        }
    }
}
