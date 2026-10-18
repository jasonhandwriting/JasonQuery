using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //B1-C3C1: temporary, user-opt-in F7 preview. Legacy F7 remains the functional editor.
    public partial class MainForm
    {
        private ToolStripMenuItem _sharedSchemaPreviewMenu;
        private Panel _sharedSchemaPreviewHeader;
        private Label _sharedSchemaPreviewStatus;
        private Button _sharedSchemaPreviewRefreshButton;
        private bool _sharedSchemaPreviewRequested;
        private bool _sharedSchemaPreviewChangingLayout;
        private bool _sharedSchemaPreviewBinding;

        private void InitializeSharedSchemaExplorerPreviewUI()
        {
            if (_sharedSchemaPreviewMenu != null || _sharedLeftPanelHostPanel == null)
            {
                return;
            }

            var menu = new ToolStripMenuItem("F7 Preview")
            {
                ToolTipText = "Read-only MainForm Schema Explorer preview; legacy F7 remains active."
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
                Text = "F7 Preview (read-only). Use the original F7 for actions."
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
            _sharedLeftPanelHostPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            _sharedLeftPanelHostPanel.Controls.Add(header);
            _sharedLeftPanelHostPanel.SizeChanged += SharedSchemaPreviewHostSizeChanged;
            header.BringToFront();
            _sharedSchemaPreviewHeader = header;
            _sharedSchemaPreviewStatus = status;
            _sharedSchemaPreviewRefreshButton = refreshButton;

            SizeChanged += SharedSchemaPreviewMainFormSizeChanged;
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
            SyncSharedSchemaExplorerPreviewUI();
            LayoutSharedSchemaExplorerPreviewGrid();
        }

        private void SharedSchemaPreviewHostSizeChanged(object sender, EventArgs e)
        {
            LayoutSharedSchemaExplorerPreviewGrid();
        }

        private int GetSharedSchemaPreviewAvailableWidth()
        {
            var previewWidth = _sharedLeftPanelHostPanel != null && _sharedLeftPanelHostPanel.Visible ? _sharedLeftPanelHostPanel.Width : 0;

            return tabControl1.Width + previewWidth;
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

            var show = _sharedSchemaPreviewRequested && canOffer;
            var host = _sharedLeftPanelHostPanel;

            if (host.Visible == show)
            {
                _sharedSchemaPreviewMenu.Checked = show;
                return;
            }

            _sharedSchemaPreviewChangingLayout = true;

            try
            {
                tabControl1.SuspendLayout();

                if (show)
                {
                    var previewWidth = SharedSchemaExplorerPreviewPolicy.PreviewWidth;

                    host.Bounds = new Rectangle(tabControl1.Left, tabControl1.Top, previewWidth, tabControl1.Height);
                    tabControl1.SetBounds(tabControl1.Left + previewWidth, tabControl1.Top, tabControl1.Width - previewWidth, tabControl1.Height);
                    host.Visible = true;
                    host.BringToFront();
                }

                if (!show)
                {
                    var previewWidth = host.Width;

                    host.Visible = false;
                    tabControl1.SetBounds(tabControl1.Left - previewWidth, tabControl1.Top, tabControl1.Width + previewWidth, tabControl1.Height);
                }

                _sharedSchemaPreviewMenu.Checked = show;
            }
            finally
            {
                tabControl1.ResumeLayout(true);
                _sharedSchemaPreviewChangingLayout = false;
            }
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

                    _sharedSchemaPreviewStatus.Text = visibleRows > 1 ? "F7 Preview (read-only). Use the original F7 for actions." : "F7 Preview: metadata bound, but grouped rows have not expanded.";
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
