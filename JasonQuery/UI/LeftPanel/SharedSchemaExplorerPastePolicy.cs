using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //L2-C3-C1: pure, non-destructive paste command contract.
    //Never mutate a grid, editor, database, or global application state here.
    public static class SharedSchemaExplorerPastePolicy
    {
        public static bool IsPasteCommand(SharedSchemaExplorerMenuCommand command)
        {
            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.PasteSelectAll:
                    {
                        return true;
                    }
                case SharedSchemaExplorerMenuCommand.PasteName:
                    {
                        return true;
                    }
                case SharedSchemaExplorerMenuCommand.PasteComma:
                    {
                        return true;
                    }
                case SharedSchemaExplorerMenuCommand.PasteCommaNewLine:
                    {
                        return true;
                    }
                case SharedSchemaExplorerMenuCommand.PasteNewLine:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static bool TryBuildText(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command, out string pasteText)
        {
            pasteText = string.Empty;

            if (!IsPasteCommand(command) || selection == null)
            {
                return false;
            }

            var permitted = false;

            foreach (var offered in SharedSchemaExplorerContextMenuPolicy.Plan(target, selection))
            {
                if (offered == command)
                {
                    permitted = true;
                    break;
                }
            }

            if (!permitted || !SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(selection, out var name))
            {
                return false;
            }

            switch (command)
            {
                case SharedSchemaExplorerMenuCommand.PasteSelectAll:
                    {
                        return TryBuildSelectAll(selection, name, out pasteText);
                    }
                case SharedSchemaExplorerMenuCommand.PasteName:
                    {
                        pasteText = name;
                        break;
                    }
                case SharedSchemaExplorerMenuCommand.PasteComma:
                    {
                        pasteText = name + ", ";
                        break;
                    }
                case SharedSchemaExplorerMenuCommand.PasteCommaNewLine:
                    {
                        pasteText = name + ", \r\n";
                        break;
                    }
                case SharedSchemaExplorerMenuCommand.PasteNewLine:
                    {
                        pasteText = name + "\r\n";
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            return !string.IsNullOrWhiteSpace(pasteText);
        }

        private static bool TryBuildSelectAll(SchemaExplorerSelectionInfo selection, string name, out string sql)
        {
            sql = string.Empty;

            if (selection == null || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            switch (selection.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        sql = $"SELECT * FROM {name}";
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        if (string.IsNullOrWhiteSpace(selection.SchemaNode))
                        {
                            return false;
                        }

                        sql = $"select * from {selection.SchemaNode}.{name}";
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        if (string.IsNullOrWhiteSpace(selection.SchemaDbo))
                        {
                            return false;
                        }

                        var prefix = selection.SchemaDbo + ".";
                        var table = name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? name.Substring(prefix.Length) : name;

                        sql = $"select * from {selection.SchemaDbo}.{table}";
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        sql = $"select * from {name}";
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            return true;
        }
    }
}
