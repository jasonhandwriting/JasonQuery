using JasonQuery.UI.LeftPanel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //B1-C3A: MainForm owns a dormant left-panel host; no old F6-F9 control moves.
    public partial class MainForm
    {
        private Panel _sharedLeftPanelHostPanel;
        private SharedLeftPanelHostState _sharedLeftPanelHostState = SharedLeftPanelHostStatePolicy.Empty;

        private void InitializeSharedLeftPanelHost()
        {
            if (_sharedLeftPanelHostPanel != null)
            {
                return;
            }

            var host = new Panel
            {
                Name = "sharedLeftPanelHost",
                Visible = false,
                Size = Size.Empty,
                TabStop = false
            };

            //The Designer-owned MainForm SplitContainer controls workspace geometry.
            //The temporary preview host is a child of the shared F7 page, never a sibling of the workspace.
            splitContainerMainWorkspace.Panel1Collapsed = true;
            tabLeftSchemaExplorer.Controls.Add(host);
            _sharedLeftPanelHostPanel = host;
            InitializeSharedSchemaExplorerGrid();
            InitializeSharedSchemaExplorerPreviewUI();

            //One subscription per MainForm lifetime; no QueryForm or Grid event hooks.
            tabControl1.SelectionChanged += SharedLeftPanelTabSelectionChanged;
            tabControl1.VisibleChanged += SharedLeftPanelWorkspaceVisibilityChanged;
        }

        private void SharedLeftPanelTabSelectionChanged(object sender, EventArgs e)
        {
            RefreshSharedLeftPanelHostState();
        }

        private void SharedLeftPanelWorkspaceVisibilityChanged(object sender, EventArgs e)
        {
            RefreshSharedLeftPanelHostState();
        }

        //Always re-read the selected Tab; never retain an MDI child Form reference.
        private void RefreshSharedLeftPanelHostState()
        {
            if (IsDisposed || Disposing || _sharedLeftPanelHostPanel == null || _sharedLeftPanelHostPanel.IsDisposed)
            {
                return;
            }

            var context = CaptureLeftPanelContext();
            var tabIdentity = tabControl1.SelectedTab?.AccessibleDescription;

            _sharedLeftPanelHostState = SharedLeftPanelHostStatePolicy.Refresh
            (
                _sharedLeftPanelHostState,
                context.IsConnected,
                context.HasOpenTabs,
                tabIdentity,
                context.ActiveTarget
            );

            InvalidateSharedSchemaExplorerMetadataIfStale();
            SyncSharedSchemaExplorerPreviewUI();
        }

        //Later F7/F6/F8/F9 hosts can read a fresh state without a cached child reference.
        internal SharedLeftPanelHostState CaptureSharedLeftPanelHostState()
        {
            RefreshSharedLeftPanelHostState();
            return _sharedLeftPanelHostState;
        }

        private void DisposeSharedLeftPanelHost()
        {
            if (_sharedLeftPanelHostPanel == null)
            {
                return;
            }

            tabControl1.SelectionChanged -= SharedLeftPanelTabSelectionChanged;
            tabControl1.VisibleChanged -= SharedLeftPanelWorkspaceVisibilityChanged;

            DisposeSharedSchemaExplorerPreviewUI();
            DisposeSharedSchemaExplorerGrid();

            var host = _sharedLeftPanelHostPanel;

            _sharedLeftPanelHostPanel = null;

            if (!host.IsDisposed)
            {
                tabLeftSchemaExplorer.Controls.Remove(host);
                host.Dispose();
            }

            _sharedLeftPanelHostState = SharedLeftPanelHostStatePolicy.MarkDisposed(_sharedLeftPanelHostState);
        }
    }
}
