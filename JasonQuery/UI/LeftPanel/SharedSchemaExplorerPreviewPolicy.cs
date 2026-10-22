using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C3C1: opt-in preview layout policy; no WinForms or database dependencies.
    public sealed class SharedSchemaExplorerPreviewGridBounds
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public SharedSchemaExplorerPreviewGridBounds(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public static class SharedSchemaExplorerPreviewPolicy
    {
        public const int PreviewWidth = 220;
        public const int MinimumWorkspaceWidth = 890;

        public static bool CanOffer(SharedLeftPanelHostState state, DataSourceType sourceType, bool workspaceVisible, int workspaceWidth)
        {
            return state != null
                   && !state.IsDisposed
                   && state.IsConnected
                   && state.ConnectionRevision > 0
                   && workspaceVisible
                   && sourceType != DataSourceType.None
                   && workspaceWidth >= MinimumWorkspaceWidth;
        }

        //The left workspace belongs to MainForm and cannot be collapsed by a Tab switch or an F7 Preview toggle.
        //Connection visibility is logical, never a per-SQL-command physical connection state.
        public static bool ShouldShowWorkspace(SharedLeftPanelHostState state, bool workspaceVisible)
        {
            return state != null && !state.IsDisposed && state.IsConnected && workspaceVisible;
        }

        //The preview Grid must never overlap the fixed-height preview header.
        public static SharedSchemaExplorerPreviewGridBounds GetGridBounds(int hostWidth, int hostHeight, int headerHeight)
        {
            var width = Math.Max(0, hostWidth);
            var height = Math.Max(0, hostHeight);
            var top = Math.Min(height, Math.Max(0, headerHeight));

            return new SharedSchemaExplorerPreviewGridBounds(0, top, width, height - top);
        }

        public static bool CanReuse(SharedLeftPanelHostState state, SharedSchemaMetadataSnapshot snapshot, DataSourceType sourceType)
        {
            return SharedSchemaMetadataSnapshotPolicy.CanBind(state, snapshot, sourceType);
        }

        //Hide/show is purely visual. Rebinding destroys the live C1 GroupBy expansion state.
        //Rebind only when no live bound Grid exists or the snapshot belongs to a stale connection.
        public static bool ShouldRefreshOnShow(SharedLeftPanelHostState state, SharedSchemaMetadataSnapshot snapshot, DataSourceType sourceType, bool hasBoundGrid)
        {
            return !hasBoundGrid || !CanReuse(state, snapshot, sourceType);
        }
    }
}
