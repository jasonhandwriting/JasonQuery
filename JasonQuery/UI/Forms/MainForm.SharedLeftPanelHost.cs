using JasonQuery.UI.LeftPanel;
using JasonQuery.Database.Internal.Repositories;
using System.Globalization;
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
            InitializeSharedWorkspaceSplitterPersistence();

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
            DisposeSharedWorkspaceSplitterPersistence();

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

        //L2-A: the shared MainForm width is independent from QueryForm/SchemaBrowserForm splitters.
        private const string SharedWorkspaceSplitterSettingKey = "SplitterConfig";
        private const string SharedWorkspaceSplitterSettingName = "MainForm_L/R";
        private bool _sharedWorkspaceSplitterUserDragging;
        private bool _sharedWorkspaceSplitterRestoring;
        private int? _sharedWorkspacePreferredDistance;

        private void InitializeSharedWorkspaceSplitterPersistence()
        {
            splitContainerMainWorkspace.SplitterMoving += SharedWorkspaceSplitterMoving;
            splitContainerMainWorkspace.SplitterMoved += SharedWorkspaceSplitterMoved;
            splitContainerMainWorkspace.Panel1.VisibleChanged += SharedWorkspaceLeftPanelVisibleChanged;
        }

        private void DisposeSharedWorkspaceSplitterPersistence()
        {
            splitContainerMainWorkspace.SplitterMoving -= SharedWorkspaceSplitterMoving;
            splitContainerMainWorkspace.SplitterMoved -= SharedWorkspaceSplitterMoved;
            splitContainerMainWorkspace.Panel1.VisibleChanged -= SharedWorkspaceLeftPanelVisibleChanged;
            _sharedWorkspaceSplitterUserDragging = false;
        }

        private void RestoreSharedWorkspaceSplitterDistance()
        {
            var workspace = splitContainerMainWorkspace;

            if (IsDisposed || Disposing || workspace.IsDisposed)
            {
                return;
            }

            //Like QueryForm, the setting uses the current DomainUser and database MPID.
            //A missing or malformed setting leaves the VS2022 Designer width intact.
            var saved = JasonQueryRepository.GetSettingValue
            (
                SharedWorkspaceSplitterSettingKey,
                SharedWorkspaceSplitterSettingName
            );

            if (!int.TryParse(saved, NumberStyles.Integer, CultureInfo.InvariantCulture, out var distance) || distance <= 0)
            {
                _sharedWorkspacePreferredDistance = null;
                return;
            }

            _sharedWorkspacePreferredDistance = distance;
            ApplySharedWorkspacePreferredDistance();
        }

        private void SharedWorkspaceLeftPanelVisibleChanged(object sender, EventArgs e)
        {
            if (!splitContainerMainWorkspace.Panel1.Visible)
            {
                return;
            }

            //Panel1 may have been collapsed while restoring; apply the saved width only after it becomes visible. Programmatic layout must never write SystemConfig.
            ApplySharedWorkspacePreferredDistance();
        }

        private void ApplySharedWorkspacePreferredDistance()
        {
            var workspace = splitContainerMainWorkspace;

            if (!_sharedWorkspacePreferredDistance.HasValue || _sharedWorkspaceSplitterRestoring || workspace.IsDisposed || !workspace.Visible || workspace.Panel1Collapsed)
            {
                return;
            }

            var distance = MainFormSplitterDistancePolicy.Clamp
            (
                _sharedWorkspacePreferredDistance.Value,
                workspace.ClientSize.Width,
                workspace.SplitterWidth,
                workspace.Panel1MinSize,
                workspace.Panel2MinSize
            );

            if (distance < 0 || distance == workspace.SplitterDistance)
            {
                return;
            }

            _sharedWorkspaceSplitterRestoring = true;

            try
            {
                workspace.SplitterDistance = distance;
            }
            finally
            {
                _sharedWorkspaceSplitterRestoring = false;
            }
        }

        private void SharedWorkspaceSplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            var workspace = splitContainerMainWorkspace;

            if (_sharedWorkspaceSplitterRestoring || IsDisposed || Disposing || !workspace.Visible || workspace.Panel1Collapsed)
            {
                return;
            }

            _sharedWorkspaceSplitterUserDragging = true;
        }

        private void SharedWorkspaceSplitterMoved(object sender, SplitterEventArgs e)
        {
            var workspace = splitContainerMainWorkspace;
            var userDragging = _sharedWorkspaceSplitterUserDragging;

            _sharedWorkspaceSplitterUserDragging = false;

            if (IsDisposed || Disposing || workspace.IsDisposed)
            {
                return;
            }

            var currentDistance = workspace.SplitterDistance;

            var allowedDistance = MainFormSplitterDistancePolicy.Clamp
            (
                currentDistance,
                workspace.ClientSize.Width,
                workspace.SplitterWidth,
                workspace.Panel1MinSize,
                workspace.Panel2MinSize
            );

            if (allowedDistance != currentDistance || !MainFormSplitterDistancePolicy.ShouldPersist(userDragging, _sharedWorkspaceSplitterRestoring, workspace.Visible, workspace.Panel1Collapsed, currentDistance, _sharedWorkspacePreferredDistance ?? -1))
            {
                return;
            }

            _sharedWorkspacePreferredDistance = currentDistance;

            JasonQueryRepository.UpdateSetting
            (
                SharedWorkspaceSplitterSettingKey,
                SharedWorkspaceSplitterSettingName,
                currentDistance.ToString(CultureInfo.InvariantCulture)
            );
        }
    }

    //Pure layout policy: leaves VS2022 Designer output and C1 layout ownership untouched.
    internal static class MainFormSplitterDistancePolicy
    {
        internal static int Clamp(int preferred, int clientWidth, int splitterWidth, int minLeft, int minRight)
        {
            var minimum = Math.Max(0, minLeft);
            var maximum = clientWidth - Math.Max(0, splitterWidth) - Math.Max(0, minRight);

            if (maximum < minimum)
            {
                return -1;
            }

            return Math.Max(minimum, Math.Min(maximum, preferred));
        }

        internal static bool ShouldPersist(bool userDragging, bool restoring, bool visible, bool collapsed, int distance, int previousDistance)
        {
            return userDragging && !restoring && visible && !collapsed && distance >= 0 && distance != previousDistance;
        }
    }
}
