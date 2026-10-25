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
            if (selection == null || selection.IsGroupRow || selection.IsColumnInfoRow
                || (selection.Kind != SchemaExplorerSelectionKind.SchemaObject && selection.Kind != SchemaExplorerSelectionKind.ExternalObject)
                || string.IsNullOrWhiteSpace(selection.SchemaType) || string.IsNullOrWhiteSpace(selection.SchemaName))
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
