using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C: dispatch *semantic* selection data; never transport a C1TrueDBGrid row index.
    //MainForm re-reads the active tab each time before calling this bridge.
    //Actual receivers and the central F7 grid are connected in the later UI phase.
    public static class SchemaExplorerActionDispatchBridge
    {
        public static bool TryDispatch(LeftPanelContext context, string activeTabIdentity, DataSourceType activeSourceType, ISchemaExplorerActionReceiver receiver, SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            if (context == null || !context.IsConnected || !context.SchemaReady || receiver == null || selection == null || string.IsNullOrWhiteSpace(activeTabIdentity))
            {
                return false;
            }

            if (context.ActiveTarget != LeftPanelTargetKind.QueryForm && context.ActiveTarget != LeftPanelTargetKind.SchemaBrowserForm)
            {
                return false;
            }

            if (activeSourceType == DataSourceType.None || selection.SourceType != activeSourceType || !Enum.IsDefined(typeof(SchemaExplorerActionKind), action))
            {
                return false;
            }

            if (selection.Kind != SchemaExplorerSelectionKind.SchemaObject && selection.Kind != SchemaExplorerSelectionKind.ExternalObject && selection.Kind != SchemaExplorerSelectionKind.ColumnInfo)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(selection.SchemaName) || string.IsNullOrWhiteSpace(selection.SchemaType))
            {
                return false;
            }

            //Group-row double-click is reserved for the *source* explorer's expand/collapse.
            //A group-row context menu can be routed for object-level groups.
            if (action == SchemaExplorerActionKind.DoubleClick && selection.IsGroupRow)
            {
                return false;
            }

            //The recipient is free to mutate the selection, but cannot alter the source grid
            //model through a shared reference. DisplayRowIndex and GroupLevel are source-local.
            var portable = new SchemaExplorerSelectionInfo
            {
                Kind = selection.Kind,
                SourceType = selection.SourceType,
                DisplayRowIndex = -1,
                GroupLevel = -1,
                IsGroupRow = selection.IsGroupRow,
                IsColumnInfoRow = selection.IsColumnInfoRow,
                SchemaNode = selection.SchemaNode ?? string.Empty,
                SchemaType = selection.SchemaType ?? string.Empty,
                SchemaName = selection.SchemaName ?? string.Empty,
                SchemaDbo = selection.SchemaDbo ?? string.Empty,
                ObjectId = selection.ObjectId ?? string.Empty,
                CreateDate = selection.CreateDate ?? string.Empty,
                ModifyDate = selection.ModifyDate ?? string.Empty,
                PackageSpecBody = selection.PackageSpecBody ?? string.Empty
            };

            return SchemaExplorerActionRouter.TryDispatch(context, activeTabIdentity, receiver, portable, action);
        }
    }
}
