using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace JasonQuery.UI.Forms
{
    //B1-C3C2A: shared F7 preview supports limited typed actions; legacy F7 retains full context commands.
    public partial class MainForm
    {
        private ToolStripMenuItem _sharedSchemaPreviewMenu;
        private Panel _sharedSchemaPreviewHeader;
        private Label _sharedSchemaPreviewStatus;
        private Button _sharedSchemaPreviewRefreshButton;
        private bool _sharedSchemaPreviewRequested;
        private bool _sharedSchemaPreviewChangingLayout;
        private bool _sharedSchemaPreviewBinding;
        private FormWindowState _sharedSchemaPreviewPreviousWindowState;
        private bool _sharedSchemaPreviewRepaintQueued;

        private const uint RedrawInvalidate = 0x0001;
        private const uint RedrawErase = 0x0004;
        private const uint RedrawAllChildren = 0x0080;
        private const uint RedrawUpdateNow = 0x0100;
        private const uint RedrawFrame = 0x0400;
        private const uint SetRedrawMessage = 0x000B;

        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RedrawWindow(IntPtr handle, IntPtr updateRectangle, IntPtr updateRegion, uint flags);

        [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
        private static extern IntPtr SendRedrawMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);

        private void InitializeSharedSchemaExplorerPreviewUI()
        {
            if (_sharedSchemaPreviewMenu != null || _sharedLeftPanelHostPanel == null)
            {
                return;
            }

            var menu = new ToolStripMenuItem("F7 Preview")
            {
                ToolTipText = "Shared F7: double-click or Enter for typed actions; original F7 retains full context commands."
            };

            menu.Click += SharedSchemaPreviewMenuClick;
            mnuMainForm.Items.Add(menu);
            _sharedSchemaPreviewMenu = menu;

            var header = new Panel
            {
                Name = "sharedSchemaPreviewHeader",
                Dock = DockStyle.Top,
                Height = 57
            };

            var status = new Label
            {
                Name = "sharedSchemaPreviewStatus",
                Dock = DockStyle.Top,
                AutoEllipsis = true,
                Height = 32,
                Text = "F7 Preview: double-click / Enter; full context actions remain in original F7."
            };

            var refreshButton = new Button
            {
                Name = "sharedSchemaPreviewRefresh",
                Dock = DockStyle.Bottom,
                Height = 24,
                TabStop = false,
                Text = "Refresh preview metadata"
            };

            refreshButton.Click += SharedSchemaPreviewRefreshClick;
            header.Controls.Add(status);
            header.Controls.Add(refreshButton);
            _sharedLeftPanelHostPanel.BorderStyle = BorderStyle.FixedSingle;
            _sharedLeftPanelHostPanel.BackColor = SystemColors.Control;
            _sharedLeftPanelHostPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _sharedLeftPanelHostPanel.Controls.Add(header);
            _sharedLeftPanelHostPanel.SizeChanged += SharedSchemaPreviewHostSizeChanged;
            header.BringToFront();
            _sharedSchemaPreviewHeader = header;
            _sharedSchemaPreviewStatus = status;
            _sharedSchemaPreviewRefreshButton = refreshButton;

            tabLeftSchemaExplorer.SizeChanged += SharedSchemaPreviewPageSizeChanged;
            _sharedSchemaPreviewPreviousWindowState = WindowState;
            SizeChanged += SharedSchemaPreviewMainFormSizeChanged;
            LayoutSharedSchemaPreviewHost();
            SyncSharedSchemaExplorerPreviewUI();
        }

        private void SharedSchemaPreviewMenuClick(object sender, EventArgs e)
        {
            if (_sharedSchemaPreviewChangingLayout || IsDisposed || Disposing)
            {
                return;
            }

            var state = CaptureSharedLeftPanelHostState();
            var availableWidth = GetSharedSchemaPreviewAvailableWidth();

            if (!SharedSchemaExplorerPreviewPolicy.CanOffer(state, _currentSourceType, tabControl1.Visible, availableWidth))
            {
                _sharedSchemaPreviewRequested = false;
                SyncSharedSchemaExplorerPreviewUI();
                return;
            }

            _sharedSchemaPreviewRequested = !_sharedSchemaPreviewRequested;

            if (_sharedSchemaPreviewRequested)
            {
                //Opening the F7 preview always selects the shared F7 page.
                c1DockingTabLeftPanel.SelectedIndex = 1;
            }

            SyncSharedSchemaExplorerPreviewUI();

            if (_sharedSchemaPreviewRequested)
            {
                //Hide/show does not change the connection. Reuse the live Grid so GroupBy expansion, selection and scroll position survive reopening the preview.
                var hasBoundGrid = _sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed && _sharedSchemaExplorerGrid.DataSource != null;

                if (SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(_sharedLeftPanelHostState, _sharedSchemaMetadataSnapshot, _currentSourceType, hasBoundGrid))
                {
                    RefreshSharedSchemaExplorerPreviewMetadata();
                }
            }
        }

        private void SharedSchemaPreviewRefreshClick(object sender, EventArgs e)
        {
            RefreshSharedSchemaExplorerPreviewMetadata();
        }

        private void SharedSchemaPreviewMainFormSizeChanged(object sender, EventArgs e)
        {
            var currentWindowState = WindowState;
            var restoredFromMinimized = _sharedSchemaPreviewPreviousWindowState == FormWindowState.Minimized && currentWindowState != FormWindowState.Minimized;

            _sharedSchemaPreviewPreviousWindowState = currentWindowState;

            SyncSharedSchemaExplorerPreviewUI();
            LayoutSharedSchemaExplorerPreviewGrid();

            if (restoredFromMinimized)
            {
                QueueSharedSchemaPreviewWorkspaceRepaint();
            }
        }

        private void SharedSchemaPreviewHostSizeChanged(object sender, EventArgs e)
        {
            LayoutSharedSchemaExplorerPreviewGrid();
        }

        private void SharedSchemaPreviewPageSizeChanged(object sender, EventArgs e)
        {
            LayoutSharedSchemaPreviewHost();
        }

        private void LayoutSharedSchemaPreviewHost()
        {
            if (_sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed || tabLeftSchemaExplorer.IsDisposed)
            {
                return;
            }

            //Keep the Designer-owned F7 filter toolbar visible above the temporary read-only preview.
            var top = tsSchemaBrowser.Bottom;
            var height = Math.Max(0, tabLeftSchemaExplorer.ClientSize.Height - top);

            _sharedLeftPanelHostPanel.Bounds = new Rectangle(0, top, tabLeftSchemaExplorer.ClientSize.Width, height);
        }

        private int GetSharedSchemaPreviewAvailableWidth()
        {
            return splitContainerMainWorkspace.ClientSize.Width;
        }

        //B1-C3C1 hotfix: keep the Grid below the header, irrespective of WinForms Dock z-order.
        private void LayoutSharedSchemaExplorerPreviewGrid()
        {
            var host = _sharedLeftPanelHostPanel;
            var header = _sharedSchemaPreviewHeader;
            var grid = _sharedSchemaExplorerGrid;

            if (host == null || host.IsDisposed || header == null || header.IsDisposed || grid == null || grid.IsDisposed)
            {
                return;
            }

            grid.Dock = DockStyle.None;
            grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            var bounds = SharedSchemaExplorerPreviewPolicy.GetGridBounds(host.ClientSize.Width, host.ClientSize.Height, header.Height);

            grid.Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        //GroupBy initially exposes only its collapsed root; expand it after the Grid becomes visible.
        private void ExpandSharedSchemaExplorerPreviewDefaultGroups()
        {
            var grid = _sharedSchemaExplorerGrid;

            if (grid == null || grid.IsDisposed || grid.Splits.Count == 0)
            {
                return;
            }

            var defaultLevels = _currentSourceType == DataSourceType.MySql || _currentSourceType == DataSourceType.SqlServer ? 2 : 1;

            for (var index = 0; index < defaultLevels; index++)
            {
                var rows = grid.Splits[0].Rows;

                if (index >= rows.Count || rows[index].RowType != RowTypeEnum.CollapsedGroupRow)
                {
                    break;
                }

                grid.ExpandGroupRow(index);
            }
        }

        private void SyncSharedSchemaExplorerPreviewUI()
        {
            if (_sharedSchemaPreviewChangingLayout || _sharedSchemaPreviewMenu == null || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed || tabControl1.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            var state = _sharedLeftPanelHostState;

            var canOffer = SharedSchemaExplorerPreviewPolicy.CanOffer
            (
                state, _currentSourceType, tabControl1.Visible, GetSharedSchemaPreviewAvailableWidth()
            );

            if (state == null || !state.IsConnected)
            {
                _sharedSchemaPreviewRequested = false;
            }

            _sharedSchemaPreviewMenu.Enabled = canOffer;

            if (_sharedSchemaMetadataSnapshot == null && _sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed)
            {
                _sharedSchemaExplorerGrid.Visible = false;
            }

            var showPreview = _sharedSchemaPreviewRequested && canOffer;
            var host = _sharedLeftPanelHostPanel;
            var workspace = splitContainerMainWorkspace;
            var showWorkspace = SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(state, workspace.Visible);

            //L2-B: right-hand Tab selection and F7 Preview availability never own the left SplitContainer panel.
            //The L1 paint transaction below is reserved for actual connected-workspace transitions only.
            if (workspace.Panel1Collapsed == !showWorkspace)
            {
                //Showing an existing F7 preview never relocates Panel2, but its bounds must still follow the current page size.
                if (showPreview)
                {
                    LayoutSharedSchemaPreviewHost();
                    LayoutSharedSchemaExplorerPreviewGrid();
                }

                host.Visible = showPreview;
                _sharedSchemaPreviewMenu.Checked = showPreview;
                return;
            }

            _sharedSchemaPreviewChangingLayout = true;

            var rightPanel = workspace.Panel2;
            var redrawWasSuspended = false;

            try
            {
                //WM_SETREDRAW prevents native Crownwood/C1/Scintilla descendants from painting intermediate coordinates while Panel1Collapsed relocates Panel2.
                //Always restore it in finally, even if a layout callback throws.
                if (!rightPanel.IsDisposed && rightPanel.IsHandleCreated)
                {
                    SendRedrawMessage(rightPanel.Handle, SetRedrawMessage, IntPtr.Zero, IntPtr.Zero);
                    redrawWasSuspended = true;
                }

                workspace.SuspendLayout();
                rightPanel.SuspendLayout();
                tabControl1.SuspendLayout();
                workspace.Panel1Collapsed = !showWorkspace;
                host.Visible = showPreview;

                if (showPreview)
                {
                    LayoutSharedSchemaPreviewHost();
                    LayoutSharedSchemaExplorerPreviewGrid();
                }

                _sharedSchemaPreviewMenu.Checked = showPreview;
            }
            finally
            {
                try
                {
                    tabControl1.ResumeLayout(true);
                    rightPanel.ResumeLayout(true);
                    workspace.ResumeLayout(true);
                }
                finally
                {
                    if (redrawWasSuspended && !rightPanel.IsDisposed && rightPanel.IsHandleCreated)
                    {
                        SendRedrawMessage(rightPanel.Handle, SetRedrawMessage, new IntPtr(1), IntPtr.Zero);
                    }

                    _sharedSchemaPreviewChangingLayout = false;
                }
            }

            //WinForms/C1 child windows may paint asynchronously after the splitter moves.
            //Schedule one descendant redraw after the entire layout transaction completes.
            QueueSharedSchemaPreviewWorkspaceRepaint();
        }

        private void QueueSharedSchemaPreviewWorkspaceRepaint()
        {
            if (_sharedSchemaPreviewRepaintQueued || IsDisposed || Disposing || !IsHandleCreated || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _sharedSchemaPreviewRepaintQueued = true;

            try
            {
                BeginInvoke(new MethodInvoker(RedrawSharedSchemaPreviewWorkspace));
            }
            catch (InvalidOperationException)
            {
                _sharedSchemaPreviewRepaintQueued = false;
            }
        }

        private void RedrawSharedSchemaPreviewWorkspace()
        {
            _sharedSchemaPreviewRepaintQueued = false;

            if (IsDisposed || Disposing || WindowState == FormWindowState.Minimized)
            {
                return;
            }

            var workspace = splitContainerMainWorkspace;

            if (workspace.IsDisposed || !workspace.Visible || !workspace.IsHandleCreated || !tabControl1.Visible)
            {
                return;
            }

            var rightPanel = workspace.Panel2;

            if (rightPanel.IsDisposed || !rightPanel.Visible || !rightPanel.IsHandleCreated)
            {
                return;
            }

            //Revalidate the actual right-hand layout without recreating editors, Grids or tabs.
            rightPanel.PerformLayout();
            tabControl1.PerformLayout();
            rightPanel.Invalidate(true);
            rightPanel.Update();

            //Include native child windows owned by Crownwood, Scintilla and ComponentOne.
            RedrawWindow(rightPanel.Handle, IntPtr.Zero, IntPtr.Zero, RedrawInvalidate | RedrawErase | RedrawAllChildren | RedrawFrame | RedrawUpdateNow);
        }

        private void RefreshSharedSchemaExplorerPreviewMetadata()
        {
            if (_sharedSchemaPreviewBinding || !_sharedSchemaPreviewRequested || _sharedLeftPanelHostPanel == null || !_sharedLeftPanelHostPanel.Visible || _sharedSchemaPreviewStatus == null || IsDisposed || Disposing)
            {
                return;
            }

            _sharedSchemaPreviewBinding = true;

            try
            {
                //Only copy already-loaded metadata; no additional database query.
                var source = DatabaseSqlExecutor.dtSchema;
                var includeColumns = MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1;
                var bound = TryBindSharedSchemaExplorerMetadata(source, _currentSourceType, includeColumns);

                if (!bound)
                {
                    _sharedSchemaMetadataSnapshot = null;

                    if (_sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed)
                    {
                        _sharedSchemaExplorerGrid.DataSource = null;
                        _sharedSchemaExplorerGrid.Visible = false;
                    }
                }

                if (bound && _sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed)
                {
                    _sharedSchemaExplorerGrid.Visible = true;
                    _sharedSchemaExplorerGrid.BringToFront();
                    _sharedSchemaPreviewHeader.BringToFront();
                    LayoutSharedSchemaExplorerPreviewGrid();
                    ExpandSharedSchemaExplorerPreviewDefaultGroups();

                    var visibleRows = _sharedSchemaExplorerGrid.Splits.Count > 0 ? _sharedSchemaExplorerGrid.Splits[0].Rows.Count : 0;

                    _sharedSchemaPreviewStatus.Text = visibleRows > 1 ? "F7 Preview: double-click / Enter; full context actions remain in original F7." : "F7 Preview: metadata bound, but grouped rows have not expanded.";
                }

                if (!bound)
                {
                    _sharedSchemaPreviewStatus.Text = "F7 Preview: metadata unavailable. Refresh after loading the schema.";
                }
            }
            catch (Exception)
            {
                //The preview must not turn an incomplete metadata refresh into a UI error.
                _sharedSchemaMetadataSnapshot = null;

                if (_sharedSchemaExplorerGrid != null && !_sharedSchemaExplorerGrid.IsDisposed)
                {
                    _sharedSchemaExplorerGrid.DataSource = null;
                    _sharedSchemaExplorerGrid.Visible = false;
                }

                _sharedSchemaPreviewStatus.Text = "F7 Preview: metadata not ready. Use the original F7.";
            }
            finally
            {
                _sharedSchemaPreviewBinding = false;
            }
        }

        private void DisposeSharedSchemaExplorerPreviewUI()
        {
            SizeChanged -= SharedSchemaPreviewMainFormSizeChanged;
            tabLeftSchemaExplorer.SizeChanged -= SharedSchemaPreviewPageSizeChanged;

            if (_sharedLeftPanelHostPanel != null && !_sharedLeftPanelHostPanel.IsDisposed)
            {
                _sharedLeftPanelHostPanel.SizeChanged -= SharedSchemaPreviewHostSizeChanged;
            }

            if (_sharedSchemaPreviewMenu != null)
            {
                _sharedSchemaPreviewMenu.Click -= SharedSchemaPreviewMenuClick;
                mnuMainForm.Items.Remove(_sharedSchemaPreviewMenu);
                _sharedSchemaPreviewMenu.Dispose();
                _sharedSchemaPreviewMenu = null;
            }

            if (_sharedSchemaPreviewRefreshButton != null)
            {
                _sharedSchemaPreviewRefreshButton.Click -= SharedSchemaPreviewRefreshClick;
                _sharedSchemaPreviewRefreshButton = null;
            }

            _sharedSchemaPreviewStatus = null;
            _sharedSchemaPreviewHeader = null;
            _sharedSchemaPreviewRequested = false;
        }
    }
}
