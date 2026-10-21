using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.SchemaExplorer;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //B1-C3C2A: limited central F7 interactions; legacy engine-specific menus remain untouched.
    public partial class MainForm
    {
        private ContextMenuStrip _sharedSchemaCopyContextMenu;
        private ToolStripMenuItem _sharedSchemaCopyNameItem;
        private SchemaExplorerSelectionInfo _sharedSchemaCopySelection;

        private void AttachSharedSchemaExplorerInputHandlers(C1TrueDBGrid grid)
        {
            if (grid == null || grid.IsDisposed)
            {
                return;
            }

            //Each freshly bound grid is independent. Never subscribe twice to the same instance.
            grid.MouseDoubleClick -= SharedSchemaExplorerGridMouseDoubleClick;
            grid.MouseDoubleClick += SharedSchemaExplorerGridMouseDoubleClick;
            grid.KeyDown -= SharedSchemaExplorerGridKeyDown;
            grid.KeyDown += SharedSchemaExplorerGridKeyDown;
            grid.MouseUp -= SharedSchemaExplorerGridMouseUp;
            grid.MouseUp += SharedSchemaExplorerGridMouseUp;
        }

        private void DetachSharedSchemaExplorerInputHandlers(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            grid.MouseDoubleClick -= SharedSchemaExplorerGridMouseDoubleClick;
            grid.KeyDown -= SharedSchemaExplorerGridKeyDown;
            grid.MouseUp -= SharedSchemaExplorerGridMouseUp;
            _sharedSchemaCopyContextMenu?.Hide();
            _sharedSchemaCopySelection = null;
        }

        private bool CanHandleSharedSchemaExplorerInput(C1TrueDBGrid grid)
        {
            return !_sharedSchemaPreviewBinding
                   && _sharedSchemaPreviewRequested
                   && _sharedLeftPanelHostPanel != null
                   && _sharedLeftPanelHostPanel.Visible
                   && grid != null
                   && !grid.IsDisposed
                   && grid.Visible
                   && ReferenceEquals(grid, _sharedSchemaExplorerGrid)
                   && SharedSchemaMetadataSnapshotPolicy.CanBind(CaptureSharedLeftPanelHostState(), _sharedSchemaMetadataSnapshot, _currentSourceType);
        }

        private SchemaExplorerSelectionInfo ResolveSharedSchemaExplorerInput(C1TrueDBGrid grid, int displayRowIndex)
        {
            if (!CanHandleSharedSchemaExplorerInput(grid) || displayRowIndex < 0 || grid.Splits.Count == 0 || displayRowIndex >= grid.Splits[0].Rows.Count)
            {
                return null;
            }

            var request = new SchemaExplorerSelectionResolveRequest
            {
                SourceType = _currentSourceType,
                DisplayRowIndex = displayRowIndex,
                IsExternalSelection = false,
                IsShowColumnInfo = Array.IndexOf(_sharedSchemaMetadataSnapshot.GroupColumns, "SchemaName") >= 0
            };

            //Resolve from the MainForm grid. Never reuse a QueryForm or SchemaBrowserForm row index.
            return SchemaExplorerSelectionResolver.Resolve(request, new C1SchemaExplorerRowReader(grid));
        }

        private bool TryActivateSharedSchemaExplorerInput(C1TrueDBGrid grid, int displayRowIndex, SharedSchemaExplorerInputKind input)
        {
            var selection = ResolveSharedSchemaExplorerInput(grid, displayRowIndex);
            var state = CaptureSharedLeftPanelHostState();
            SchemaExplorerActionKind action;

            if (!SharedSchemaExplorerInteractionPolicy.TryGetAction(state.ActiveTarget, selection, input, out action))
            {
                return false;
            }

            //The existing bridge copies portable semantics, zeros local indices, then checks the active tab.
            return TryDispatchActiveSchemaExplorerAction(selection, action);
        }

        private void SharedSchemaExplorerGridMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || !(sender is C1TrueDBGrid grid))
            {
                return;
            }

            try
            {
                TryActivateSharedSchemaExplorerInput(grid, grid.RowContaining(e.Y), SharedSchemaExplorerInputKind.DoubleClick);
            }
            catch (Exception)
            {
                //The preview must not crash a working legacy F7 when C1 is regrouping.
            }
        }

        private void SharedSchemaExplorerGridKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !(sender is C1TrueDBGrid grid))
            {
                return;
            }

            try
            {
                if (TryActivateSharedSchemaExplorerInput(grid, grid.Row, SharedSchemaExplorerInputKind.Enter))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            catch (Exception)
            {
                //Leave the original F7 route available; do not consume an unsuccessful action.
            }
        }

        private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || !(sender is C1TrueDBGrid grid) || !CanHandleSharedSchemaExplorerInput(grid))
            {
                return;
            }

            SchemaExplorerSelectionInfo selection;

            try
            {
                selection = ResolveSharedSchemaExplorerInput(grid, grid.RowContaining(e.Y));
            }
            catch (Exception)
            {
                return;
            }

            if (!SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(selection))
            {
                return;
            }

            if (_sharedSchemaCopyContextMenu == null)
            {
                var menu = new ContextMenuStrip();
                var copy = new ToolStripMenuItem("Copy displayed name");

                copy.Click += SharedSchemaCopyNameClick;
                menu.Items.Add(copy);
                _sharedSchemaCopyContextMenu = menu;
                _sharedSchemaCopyNameItem = copy;
            }

            _sharedSchemaCopySelection = selection;
            _sharedSchemaCopyContextMenu.Show(grid, new Point(e.X, e.Y));
        }

        private void SharedSchemaCopyNameClick(object sender, EventArgs e)
        {
            var selection = _sharedSchemaCopySelection;

            _sharedSchemaCopySelection = null;

            if (!SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(selection) || !CanHandleSharedSchemaExplorerInput(_sharedSchemaExplorerGrid))
            {
                return;
            }

            try
            {
                Clipboard.SetText(selection.SchemaName);
            }
            catch (Exception)
            {
                //Clipboard availability is outside the preview's ownership.
            }
        }

        private void DisposeSharedSchemaExplorerInputHandlers()
        {
            _sharedSchemaCopySelection = null;

            if (_sharedSchemaCopyNameItem != null)
            {
                _sharedSchemaCopyNameItem.Click -= SharedSchemaCopyNameClick;
                _sharedSchemaCopyNameItem = null;
            }

            if (_sharedSchemaCopyContextMenu != null)
            {
                _sharedSchemaCopyContextMenu.Dispose();
                _sharedSchemaCopyContextMenu = null;
            }
        }
    }
}
