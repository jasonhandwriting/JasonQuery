using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //B1-C3B: a separate dormant MainForm-owned F7 grid; existing F7 grids remain untouched.
    public partial class MainForm
    {
        private C1TrueDBGrid _sharedSchemaExplorerGrid;
        private SharedSchemaMetadataSnapshot _sharedSchemaMetadataSnapshot;

        private static C1TrueDBGrid CreateSharedSchemaExplorerGrid()
        {
            return new C1TrueDBGrid
            {
                Name = "sharedSchemaExplorerGrid",
                AllowUpdate = false,
                ColumnHeaders = false,
                DataView = DataViewEnum.GroupBy,
                Dock = DockStyle.Fill,
                GroupByAreaVisible = false,
                TabStop = false,
                Visible = false
            };
        }

        private void InitializeSharedSchemaExplorerGrid()
        {
            if (_sharedSchemaExplorerGrid != null || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed)
            {
                return;
            }

            var grid = CreateSharedSchemaExplorerGrid();

            _sharedLeftPanelHostPanel.Controls.Add(grid);
            _sharedSchemaExplorerGrid = grid;
            AttachSharedSchemaExplorerInputHandlers(grid);
        }

        //Explicit staging entry for the future shared F7 UI. No automatic DB query or UI switch.
        internal bool TryBindSharedSchemaExplorerMetadata(DataTable source, DataSourceType sourceType, bool includeColumnGroup)
        {
            if (IsDisposed || Disposing || InvokeRequired || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed || sourceType != _currentSourceType)
            {
                return false;
            }

            var state = CaptureSharedLeftPanelHostState();

            SharedSchemaMetadataSnapshot snapshot;

            if (!SharedSchemaMetadataSnapshotPolicy.TryCapture(source, sourceType, includeColumnGroup, state.ConnectionRevision, out snapshot)
                || !SharedSchemaMetadataSnapshotPolicy.CanBind(state, snapshot, _currentSourceType))
            {
                return false;
            }

            //Fresh grouped grid per binding: no stale C1 group definitions survive a refresh.
            var candidate = CreateSharedSchemaExplorerGrid();

            try
            {
                candidate.DataSource = snapshot.Table;

                foreach (var name in snapshot.GroupColumns)
                {
                    candidate.GroupedColumns.Add(candidate.Columns[name]);
                    candidate.GroupedColumns[candidate.GroupedColumns.Count - 1].GroupInfo.HeaderText = "{0}";
                }

                foreach (var name in snapshot.HiddenColumns)
                {
                    if (snapshot.Table.Columns.Contains(name))
                    {
                        candidate.Splits[0].DisplayColumns[name].Visible = false;
                    }
                }

                _sharedLeftPanelHostPanel.Controls.Add(candidate);
                AttachSharedSchemaExplorerInputHandlers(candidate);
            }
            catch (Exception)
            {
                candidate.Dispose();
                return false;
            }

            var previous = _sharedSchemaExplorerGrid;

            _sharedSchemaExplorerGrid = candidate;
            _sharedSchemaMetadataSnapshot = snapshot;

            if (previous != null)
            {
                DetachSharedSchemaExplorerInputHandlers(previous);
                _sharedLeftPanelHostPanel.Controls.Remove(previous);
                previous.Dispose();
            }

            return true;
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
                _sharedSchemaExplorerGrid.DataSource = null;
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
                grid.Dispose();
            }
        }
    }
}
