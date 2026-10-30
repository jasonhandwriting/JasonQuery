using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using System;
using System.Data;

namespace JasonQuery.UI.Forms
{
    //L2-C3-D0-B: the shared F7 grid is Designer-owned and never replaced at runtime.
    public partial class MainForm
    {
        private C1TrueDBGrid _sharedSchemaExplorerGrid;
        private SharedSchemaMetadataSnapshot _sharedSchemaMetadataSnapshot;

        private void InitializeSharedSchemaExplorerGrid()
        {
            if (_sharedSchemaExplorerGrid != null)
            {
                return;
            }

            var grid = c1GridSharedSchemaExplorer;

            if (grid == null || grid.IsDisposed || !ReferenceEquals(grid.Parent, tabLeftSchemaExplorer))
            {
                throw new InvalidOperationException("The shared F7 Designer Grid is unavailable.");
            }

            //Never reparent, recreate or dispose the control owned by MainForm.Designer.
            _sharedSchemaExplorerGrid = grid;
            grid.Visible = false;

            AttachSharedSchemaExplorerInputHandlers(grid);
        }

        //C1TrueDBGrid may retain GroupBy definitions and hidden columns after a DataSource change.
        //Reset the old binding before assigning a different connection-scoped snapshot.
        private static void ClearSharedSchemaExplorerGridBinding(C1TrueDBGrid grid)
        {
            if (grid == null || grid.IsDisposed)
            {
                return;
            }

            grid.GroupedColumns.Clear();
            grid.DataSource = null;
        }

        private static void BindSharedSchemaExplorerSnapshot(C1TrueDBGrid grid, SharedSchemaMetadataSnapshot snapshot)
        {
            ClearSharedSchemaExplorerGridBinding(grid);
            grid.DataSource = snapshot.Table;

            foreach (var name in snapshot.GroupColumns)
            {
                grid.GroupedColumns.Add(grid.Columns[name]);
                grid.GroupedColumns[grid.GroupedColumns.Count - 1].GroupInfo.HeaderText = "{0}";
            }

            if (grid.Splits.Count == 0)
            {
                throw new InvalidOperationException("The shared F7 Grid has no C1 split after binding.");
            }

            //The old implementation started with a fresh grid, where every display column was initially visible. Restore that baseline before applying DB-specific hides.
            foreach (C1DisplayColumn column in grid.Splits[0].DisplayColumns)
            {
                column.Visible = true;
            }

            foreach (var name in snapshot.HiddenColumns)
            {
                if (snapshot.Table.Columns.Contains(name))
                {
                    grid.Splits[0].DisplayColumns[name].Visible = false;
                }
            }
        }

        //Explicit staging entry for the future shared F7 UI. No automatic DB query or UI switch.
        internal bool TryBindSharedSchemaExplorerMetadata(DataTable source, DataSourceType sourceType, bool includeColumnGroup)
        {
            if (IsDisposed || Disposing || InvokeRequired || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed
                || _sharedSchemaExplorerGrid == null || _sharedSchemaExplorerGrid.IsDisposed
                || !ReferenceEquals(_sharedSchemaExplorerGrid, c1GridSharedSchemaExplorer) || sourceType != _currentSourceType)
            {
                return false;
            }

            var state = CaptureSharedLeftPanelHostState();

            SharedSchemaMetadataSnapshot snapshot;
            var oracleReader = sourceType == DataSourceType.Oracle ? MyGlobal.OracleReader : null;
            var verifiedSessionUser = oracleReader?.VerifiedSessionUser ?? string.Empty;
            var verifiedConnectionId = oracleReader?.VerifiedConnectionId ?? Guid.Empty;

            if (!SharedSchemaMetadataSnapshotPolicy.TryCapture(source, sourceType, includeColumnGroup, state.ConnectionRevision, out snapshot, verifiedSessionUser, verifiedConnectionId)
                || !SharedSchemaMetadataSnapshotPolicy.CanBind(state, snapshot, _currentSourceType))
            {
                return false;
            }

            var grid = _sharedSchemaExplorerGrid;

            //Fail closed when C1 rejects a rebind: never retain a snapshot that describes different columns from the live control. The original QueryForm F7 is unaffected.
            _sharedSchemaMetadataSnapshot = null;

            try
            {
                BindSharedSchemaExplorerSnapshot(grid, snapshot);
                _sharedSchemaMetadataSnapshot = snapshot;

                return true;
            }
            catch (Exception)
            {
                try
                {
                    ClearSharedSchemaExplorerGridBinding(grid);
                }
                catch (Exception)
                {
                    //An invalid C1 layout must not crash or replace a Designer control.
                }

                grid.Visible = false;
                return false;
            }
        }

        //The grid remains hidden until a later F7 UI activation batch.
        private void InvalidateSharedSchemaExplorerMetadataIfStale()
        {
            if (_sharedSchemaMetadataSnapshot == null)
            {
                return;
            }

            if (SharedSchemaMetadataSnapshotPolicy.CanBind(_sharedLeftPanelHostState, _sharedSchemaMetadataSnapshot, _currentSourceType))
            {
                return;
            }

            _sharedSchemaMetadataSnapshot = null;

            if (_sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed)
            {
                _sharedSchemaExplorerGrid.Visible = false;

                try
                {
                    ClearSharedSchemaExplorerGridBinding(_sharedSchemaExplorerGrid);
                }
                catch (Exception)
                {
                    //Reject the stale snapshot even if the C1 Grid cannot reset cleanly.
                }
            }
        }

        private void DisposeSharedSchemaExplorerGrid()
        {
            _sharedSchemaMetadataSnapshot = null;

            var grid = _sharedSchemaExplorerGrid;

            DetachSharedSchemaExplorerInputHandlers(grid);
            DisposeSharedSchemaExplorerInputHandlers();
            _sharedSchemaExplorerGrid = null;

            if (grid != null && !grid.IsDisposed)
            {
                grid.Visible = false;

                try
                {
                    ClearSharedSchemaExplorerGridBinding(grid);
                }
                catch (Exception)
                {
                    //The Form/Designer owns disposal of this control.
                }
            }
        }
    }
}
