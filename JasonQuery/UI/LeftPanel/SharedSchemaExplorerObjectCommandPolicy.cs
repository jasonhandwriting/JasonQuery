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
                        if (target != LeftPanelTargetKind.QueryForm)
                        {
                            return false;
                        }

                        return HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.OpenSchemaBrowser:
                    {
                        return target == LeftPanelTargetKind.QueryForm && SchemaObjectTypeHelper.IsAny(selection.SchemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views) && HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.GenerateSqlStatement:
                    {
                        return SchemaObjectTypeHelper.IsAny(selection.SchemaType, SchemaObjectNames.Tables, SchemaObjectNames.Views) && HasRequiredObjectScope(selection);
                    }
                case SharedSchemaExplorerMenuCommand.CommentTable:
                case SharedSchemaExplorerMenuCommand.DropTable:
                case SharedSchemaExplorerMenuCommand.RenameTable:
                case SharedSchemaExplorerMenuCommand.TruncateTable:
                    {
                        //B3-B2-T GUI correction: only a G2-qualified Oracle TABLE data row or object-level group row may expose commands. MainForm and EditTableForm independently check live OracleReader identity before Preview/Execute.
                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm) && OracleSharedTableDdlBridgePolicy.IsTableObjectRow(selection);
                    }
                case SharedSchemaExplorerMenuCommand.AddColumn:
                case SharedSchemaExplorerMenuCommand.CommentColumn:
                case SharedSchemaExplorerMenuCommand.DropColumn:
                case SharedSchemaExplorerMenuCommand.RenameColumn:
                    {
                        return (target == LeftPanelTargetKind.QueryForm || target == LeftPanelTargetKind.SchemaBrowserForm) && OracleSharedColumnDdlBridgePolicy.IsQualifiedColumnDataRow(selection);
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
