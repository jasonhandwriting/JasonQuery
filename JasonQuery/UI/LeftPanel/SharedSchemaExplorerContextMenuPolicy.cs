using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Collections.Generic;

namespace JasonQuery.UI.LeftPanel
{
    //L2-C3-A: describe visible F7 commands only. No WinForms/UI state or database operations.
    //The old QueryForm/SchemaBrowserForm context menus remain operational until L2-C4.
    public enum SharedSchemaExplorerMenuCommand
    {
        ShowDdl,
        OpenSchemaBrowser,
        GenerateSqlStatement,
        CommentTable,
        DropTable,
        RenameTable,
        TruncateTable,
        AddColumn,
        CommentColumn,
        DropColumn,
        RenameColumn,
        CopyToClipboard,
        PasteSelectAll,
        PasteName,
        PasteComma,
        PasteCommaNewLine,
        PasteNewLine
    }

    public static class SharedSchemaExplorerContextMenuPolicy
    {
        public static IReadOnlyList<SharedSchemaExplorerMenuCommand> Plan(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            var commands = new List<SharedSchemaExplorerMenuCommand>();

            if (target != LeftPanelTargetKind.QueryForm && target != LeftPanelTargetKind.SchemaBrowserForm)
            {
                return commands;
            }

            if (selection == null || selection.Kind == SchemaExplorerSelectionKind.None || selection.Kind == SchemaExplorerSelectionKind.Invalid
                ||  string.IsNullOrWhiteSpace(selection.SchemaName) || string.IsNullOrWhiteSpace(selection.SchemaType) || !IsSupportedDatabase(selection.SourceType))
            {
                return commands;
            }

            if (selection.Kind != SchemaExplorerSelectionKind.SchemaObject && selection.Kind != SchemaExplorerSelectionKind.ColumnInfo && selection.Kind != SchemaExplorerSelectionKind.ExternalObject)
            {
                return commands;
            }

            var isColumn = selection.Kind == SchemaExplorerSelectionKind.ColumnInfo || selection.IsColumnInfoRow;
            var isTable = SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Tables);
            var isView = SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Views);

            //Group rows are owned by the grid; a ColumnInfo classification cannot be a group row.
            if (isColumn && selection.IsGroupRow)
            {
                return commands;
            }

            if (!isColumn && target == LeftPanelTargetKind.QueryForm)
            {
                commands.Add(SharedSchemaExplorerMenuCommand.ShowDdl);
            }

            if (!isColumn && (isTable || isView))
            {
                if (target == LeftPanelTargetKind.QueryForm)
                {
                    commands.Add(SharedSchemaExplorerMenuCommand.OpenSchemaBrowser);
                }

                commands.Add(SharedSchemaExplorerMenuCommand.GenerateSqlStatement);

                if (isTable)
                {
                    commands.Add(SharedSchemaExplorerMenuCommand.CommentTable);
                    commands.Add(SharedSchemaExplorerMenuCommand.DropTable);
                    commands.Add(SharedSchemaExplorerMenuCommand.RenameTable);
                    commands.Add(SharedSchemaExplorerMenuCommand.TruncateTable);
                }
            }

            if (isColumn && isTable)
            {
                commands.Add(SharedSchemaExplorerMenuCommand.AddColumn);
                commands.Add(SharedSchemaExplorerMenuCommand.CommentColumn);
                commands.Add(SharedSchemaExplorerMenuCommand.DropColumn);
                commands.Add(SharedSchemaExplorerMenuCommand.RenameColumn);
            }

            commands.Add(SharedSchemaExplorerMenuCommand.CopyToClipboard);

            if (!isColumn && (isTable || isView))
            {
                commands.Add(SharedSchemaExplorerMenuCommand.PasteSelectAll);
            }

            commands.Add(SharedSchemaExplorerMenuCommand.PasteName);

            //The suffix-paste commands are deliberately retained until the approved L2-C3.5 simplification.
            if (isColumn || isTable || isView)
            {
                commands.Add(SharedSchemaExplorerMenuCommand.PasteComma);
                commands.Add(SharedSchemaExplorerMenuCommand.PasteCommaNewLine);
                commands.Add(SharedSchemaExplorerMenuCommand.PasteNewLine);
            }

            return commands;
        }

        public static bool TryGetCopyText(SchemaExplorerSelectionInfo selection, out string text)
        {
            text = string.Empty;

            if (selection == null || string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                return false;
            }

            text = selection.SchemaName;

            if (selection.Kind == SchemaExplorerSelectionKind.ColumnInfo || selection.IsColumnInfoRow)
            {
                var suffixIndex = text.IndexOf(", ", StringComparison.Ordinal);

                if (suffixIndex >= 0)
                {
                    text = text.Substring(0, suffixIndex);
                }
            }

            return !string.IsNullOrWhiteSpace(text);
        }

        private static bool IsSupportedDatabase(DataSourceType sourceType)
        {
            return sourceType == DataSourceType.Oracle || sourceType == DataSourceType.PostgreSql || sourceType == DataSourceType.SqlServer || sourceType == DataSourceType.MySql;
        }
    }
}
