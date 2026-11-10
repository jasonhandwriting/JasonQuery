using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //L2-C3-C2: non-destructive object command qualification. UI and DB work belong to MainForm.
    public static class SharedSchemaExplorerObjectCommandPolicy
    {
        public static bool CanExecute(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command)
        {
            //Only structural Table commands may consider a C1 object group. All other group actions keep their previous safety boundary; Table classification is checked by the bridge policy.
            if (selection == null || (selection.IsGroupRow && !IsOracleTableDdlCommand(command)) || (selection.IsColumnInfoRow && !IsOracleColumnDdlCommand(command)) || (selection.Kind != SchemaExplorerSelectionKind.SchemaObject && selection.Kind != SchemaExplorerSelectionKind.ExternalObject && !(selection.Kind == SchemaExplorerSelectionKind.ColumnInfo && IsOracleColumnDdlCommand(command))) || string.IsNullOrWhiteSpace(selection.SchemaType) || string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                return false;
            }

            var offered = false;

            foreach (var item in SharedSchemaExplorerContextMenuPolicy.Plan(target, selection))
            {
                if (item == command)
                {
                    offered = true;
                    break;
                }
            }

            if (!offered)
            {
                return false;
            }

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.ShowDdl:
                    {
                        if (target != LeftPanelTargetKind.QueryForm && target != LeftPanelTargetKind.OtherForm)
                        {
                            return false;
                        }

                        return HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.OpenSchemaBrowser:
                    {
                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.OtherForm) && SchemaObjectTypeHelper.IsAny(selection.SchemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views) && HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.GenerateSqlStatement:
                    {
                        return SchemaObjectTypeHelper.IsAny(selection.SchemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views) && HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.CommentTable:
                    {
                        if (selection.SourceType == DataSourceType.Oracle)
                        {
                            return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm) && OracleSharedTableDdlBridgePolicy.IsTableObjectRow(selection);
                        }

                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm) && IsNonOracleAddColumnTableCandidate(selection);
                    }
                case SharedSchemaExplorerMenuCommand.DropTable:
                case SharedSchemaExplorerMenuCommand.RenameTable:
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                    {
                        if (selection.SourceType == DataSourceType.Oracle)
                        {
                            return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm) && OracleSharedTableDdlBridgePolicy.IsTableObjectRow(selection);
                        }

                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm) && IsNonOracleAddColumnTableCandidate(selection);
                    }
                case SharedSchemaExplorerMenuCommand.AddColumn:
                    {
                        if (target != LeftPanelTargetKind.QueryForm && target != LeftPanelTargetKind.SchemaBrowserForm && target != LeftPanelTargetKind.OtherForm)
                        {
                            return false;
                        }

                        //Keep the existing Oracle U1 bridge and last-mile gate untouched.
                        if (selection.SourceType == DataSourceType.Oracle)
                        {
                            return OracleSharedTableDdlBridgePolicy.IsTableObjectRow(selection) || OracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(selection);
                        }

                        //A non-Oracle Table candidate is NOT execute-time authorization.
                        //Column rows need authoritative parent-Table identity in U2-B.
                        if (selection.Kind == SchemaExplorerSelectionKind.ColumnInfo || selection.IsColumnInfoRow)
                        {
                            return NonOracleSharedAddColumnPreviewPolicy.IsQualifiedColumn(selection);
                        }

                        return IsNonOracleAddColumnTableCandidate(selection);
                    }
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                case SharedSchemaExplorerMenuCommand.DropColumn:
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm || target == LeftPanelTargetKind.OtherForm) && OracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(selection);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        //Only the selected Table object may offer Add Column. No generic group, Column caption or first-match metadata row may supply the DDL target identity.
        private static bool IsNonOracleAddColumnTableCandidate(SchemaExplorerSelectionInfo selection)
        {
            if (selection == null
                || selection.Kind != SchemaExplorerSelectionKind.SchemaObject
                || selection.IsColumnInfoRow
                || !SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables)
                || !HasRequiredObjectScope(selection)
                || (selection.IsGroupRow && selection.GroupLevel != SchemaExplorerGroupLevelPolicy.GetMinimumObjectGroupLevel(selection.SourceType)))
            {
                return false;
            }

            switch (selection.SourceType)
            {
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        return true;
                    }
                case DataSourceType.SqlServer:
                    {
                        return !string.IsNullOrWhiteSpace(selection.ObjectId);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool IsOracleColumnDdlCommand(SharedSchemaExplorerMenuCommand command)
        {
            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.AddColumn:
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                case SharedSchemaExplorerMenuCommand.DropColumn:
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool IsOracleTableDdlCommand(SharedSchemaExplorerMenuCommand command)
        {
            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.CommentTable:
                case SharedSchemaExplorerMenuCommand.DropTable:
                case SharedSchemaExplorerMenuCommand.RenameTable:
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                case SharedSchemaExplorerMenuCommand.AddColumn:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool HasRequiredObjectScope(SchemaExplorerSelectionInfo selection)
        {
            switch (selection.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return true;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        return !string.IsNullOrWhiteSpace(selection.SchemaNode);
                    }
                case DataSourceType.SqlServer:
                    {
                        return !string.IsNullOrWhiteSpace(selection.SchemaNode) && !string.IsNullOrWhiteSpace(selection.SchemaDbo);
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        //Old SQL Server menus strip an existing dbo. prefix before opening GenerateSqlForm.
        public static string GetGenerateSqlObjectName(SchemaExplorerSelectionInfo selection)
        {
            var name = selection?.SchemaName ?? string.Empty;
            var schema = selection?.SchemaDbo ?? string.Empty;

            if (selection != null && selection.SourceType == DataSourceType.SqlServer && !string.IsNullOrWhiteSpace(schema))
            {
                var prefix = schema + ".";

                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length);
                }
            }

            return name;
        }
    }
}
