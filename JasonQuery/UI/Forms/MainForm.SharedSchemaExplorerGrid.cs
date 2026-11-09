using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
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

        //Select F7 only once for a successfully loaded logical database scope.
        private int _sharedF7StartupSelectedRevision = -1;
        private string _sharedF7StartupSelectedScope = string.Empty;

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

        //A physical Reader reconnect does not change the current logical database scope.
        //No passwords, connection strings or live Session IDs are stored in the scope.
        private string GetSharedSchemaDatabaseScope()
        {
            if (_currentSourceType == DataSourceType.None || DatabaseSqlExecutor.CurrentDataSource != _currentSourceType)
            {
                return string.Empty;
            }

            return string.Join("|", new[]
            {
                _currentSourceType.ToString(),
                DatabaseSqlExecutor.DbConnectionServer ?? string.Empty,
                DatabaseSqlExecutor.DbConnectionPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DatabaseSqlExecutor.DbUser ?? string.Empty,
                DatabaseSqlExecutor.DatabaseName ?? string.Empty,
                _currentSourceType == DataSourceType.Oracle ? DatabaseSqlExecutor.OracleSid ?? string.Empty : string.Empty,
                _currentSourceType == DataSourceType.Oracle ? DatabaseSqlExecutor.OracleConnectAs ?? string.Empty : string.Empty
            });
        }

        private bool IsSharedSchemaSnapshotInCurrentScope()
        {
            return SharedSchemaMetadataSnapshotPolicy.CanBindForScope
            (
                _sharedLeftPanelHostState,
                _sharedSchemaMetadataSnapshot,
                _currentSourceType,
                GetSharedSchemaDatabaseScope()
            );
        }

        //The three-argument API remains for existing callers, but an unverified cache cannot be stamped with new source provenance by a mere rebind.
        internal bool TryBindSharedSchemaExplorerMetadata(DataTable source, DataSourceType sourceType, bool includeColumnGroup)
        {
            return TryBindSharedSchemaExplorerMetadata(source, sourceType, includeColumnGroup, false);
        }

        private bool TryBindSharedSchemaExplorerMetadata(DataTable source, DataSourceType sourceType, bool includeColumnGroup, bool justReloaded)
        {
            if (!justReloaded || IsDisposed || Disposing || InvokeRequired
                || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed
                || _sharedSchemaExplorerGrid == null || _sharedSchemaExplorerGrid.IsDisposed
                || !ReferenceEquals(_sharedSchemaExplorerGrid, c1GridSharedSchemaExplorer) || sourceType != _currentSourceType)
            {
                return false;
            }

            var state = CaptureSharedLeftPanelHostState();
            var scope = GetSharedSchemaDatabaseScope();
            SharedSchemaMetadataSnapshot next = null;
            bool captured;

            if (sourceType == DataSourceType.Oracle)
            {
                var reader = MyGlobal.OracleReader;

                captured = reader != null && SharedSchemaMetadataSnapshotPolicy.TryCaptureVerifiedOracleForScope
                (
                    source,
                    includeColumnGroup,
                    state.ConnectionRevision,
                    scope,
                    reader.VerifiedSessionUser,
                    reader.VerifiedConnectionId,
                    out next
                );

                if (reader == null)
                {
                    return false;
                }
            }
            else
            {
                captured = SharedSchemaMetadataSnapshotPolicy.TryCaptureForScope
                (
                    source,
                    sourceType,
                    includeColumnGroup,
                    state.ConnectionRevision,
                    scope,
                    out next
                );
            }

            if (!captured || !SharedSchemaMetadataSnapshotPolicy.CanBindForScope(state, next, sourceType, scope))
            {
                return false;
            }

            var grid = _sharedSchemaExplorerGrid;
            var previous = _sharedSchemaMetadataSnapshot;
            var currentFocus = _schemaExplorerFocusPendingRestore ?? CaptureSchemaExplorerFocusBeforeRebind();

            _schemaExplorerFocusPendingRestore = null;

            try
            {
                BindSharedSchemaExplorerSnapshot(grid, next);
                _sharedSchemaMetadataSnapshot = next;
                _schemaExplorerFocusPendingRestore = currentFocus;

                return true;
            }
            catch (Exception)
            {
                _schemaExplorerFocusPendingRestore = null;

                //Binding failure must not replace an already published, same-scope view.
                if (SharedSchemaMetadataSnapshotPolicy.CanBindForScope(state, previous, sourceType, scope))
                {
                    try
                    {
                        BindSharedSchemaExplorerSnapshot(grid, previous);
                        _sharedSchemaMetadataSnapshot = previous;

                        return false;
                    }
                    catch (Exception)
                    {
                        //If both binds fail, reject the unsafe C1 layout.
                    }
                }

                _sharedSchemaMetadataSnapshot = null;
                ClearSharedSchemaExplorerGridBinding(grid);

                grid.Visible = false;
                return false;
            }
        }

        //A successful legacy QueryForm Refresh has already queried the provider.
        //Bind its published batch without issuing a second metadata SQL request.
        private void TryBindSharedSchemaExplorerAfterLegacyReload()
        {
            if (!_sharedSchemaPreviewRequested || _sharedSchemaPreviewBinding || _sharedSchemaExplorerGrid == null
                || _sharedSchemaExplorerGrid.IsDisposed || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed)
            {
                return;
            }

            //The existing snapshot distinguishes an initial load from an ordinary refresh.
            //A physical Reader reconnect is not a new logical database scope.
            CaptureSharedLeftPanelHostState();

            var hadCurrentScopeSnapshot = IsSharedSchemaSnapshotInCurrentScope();
            var includeColumns = MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1;

            if (!TryBindSharedSchemaExplorerMetadata(DatabaseSqlExecutor.dtSchema, _currentSourceType, includeColumns, true))
            {
                return;
            }

            _sharedSchemaExplorerGrid.Visible = true;

            LayoutSharedSchemaExplorerPreviewGrid();
            ExpandSharedSchemaExplorerPreviewDefaultGroups();
            RestoreSchemaExplorerFocusAfterSuccessfulRebind();
            SyncSharedSchemaExplorerPreviewUI();

            var state = CaptureSharedLeftPanelHostState();
            var scope = GetSharedSchemaDatabaseScope();

            var startup = new SharedSchemaExplorerStartupDecisionContext
            {
                HostState = state,
                SourceType = _currentSourceType,
                WorkspaceVisible = splitContainerMainWorkspace.Visible && !splitContainerMainWorkspace.Panel1Collapsed,
                WorkspaceWidth = GetSharedSchemaPreviewAvailableWidth(),
                CurrentDatabaseScope = scope,
                PublishedDatabaseScope = _sharedSchemaMetadataSnapshot?.DatabaseScope,
                PublishedConnectionRevision = _sharedSchemaMetadataSnapshot?.ConnectionRevision ?? 0,
                HasTrustedPublishedMetadata = IsSharedSchemaSnapshotInCurrentScope(),
                HasPendingOrUnknownTransaction = AppConfigHelper.IsNotCommitYet,
                IsInitialLoad = !hadCurrentScopeSnapshot,
                UserSelectedDifferentLeftTab = c1DockingTabLeftPanel.SelectedIndex != 1,
                AlreadyAutoSelectedForConnection = _sharedF7StartupSelectedRevision == state.ConnectionRevision && string.Equals(_sharedF7StartupSelectedScope, scope, StringComparison.Ordinal)
            };

            //Startup is a UI selection only; it never grants Oracle Qualified DDL rights.
            //The policy reads the already-published snapshot and executes no metadata SQL.
            if (SharedSchemaExplorerStartupDecisionPolicy.MayAutoSelectF7(startup))
            {
                c1DockingTabLeftPanel.SelectedIndex = 1;
                _sharedF7StartupSelectedRevision = state.ConnectionRevision;
                _sharedF7StartupSelectedScope = scope;
            }
        }

        private void InvalidateSharedSchemaExplorerMetadataIfStale()
        {
            if (_sharedSchemaMetadataSnapshot == null || IsSharedSchemaSnapshotInCurrentScope())
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
                    //A stale database-scope Grid can never authorize an object action.
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
