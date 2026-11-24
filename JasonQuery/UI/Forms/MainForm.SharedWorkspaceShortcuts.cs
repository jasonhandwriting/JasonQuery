using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //MainForm owns shortcuts for the shared left workspace.
    //F8 and F9 are active shared routes; F6 and F7 remain on their existing paths until their shortcut ownership is migrated.
    public partial class MainForm
    {
        private SharedWorkspaceShortcutMessageFilter _sharedWorkspaceShortcutMessageFilter;

        private void InitializeSharedWorkspaceShortcuts()
        {
            if (_sharedWorkspaceShortcutMessageFilter != null)
            {
                return;
            }

            _sharedWorkspaceShortcutMessageFilter = new SharedWorkspaceShortcutMessageFilter(this);

            Application.AddMessageFilter(_sharedWorkspaceShortcutMessageFilter);

            Disposed += MainFormSharedWorkspaceShortcutsDisposed;
        }

        private void MainFormSharedWorkspaceShortcutsDisposed(object sender, EventArgs e)
        {
            Disposed -= MainFormSharedWorkspaceShortcutsDisposed;

            var filter = _sharedWorkspaceShortcutMessageFilter;

            _sharedWorkspaceShortcutMessageFilter = null;

            if (filter != null)
            {
                Application.RemoveMessageFilter(filter);
            }
        }

        private bool TrySelectSharedTabListFromShortcut()
        {
            if (IsDisposed || Disposing || !tabControl1.Visible || splitContainerMainWorkspace.IsDisposed
                || !splitContainerMainWorkspace.Visible || c1DockingTabLeftPanel.IsDisposed
                || !LeftPanelAvailabilityPolicy.Resolve(CaptureLeftPanelContext()).IsEnabled(LeftPanelPage.TabList))
            {
                return false;
            }

            //Panel1 can be collapsed internally during initialization; this does not introduce a user-facing collapse/expand command.
            if (splitContainerMainWorkspace.Panel1Collapsed)
            {
                splitContainerMainWorkspace.Panel1Collapsed = false;
            }

            c1DockingTabLeftPanel.SelectedTab = tabLeftTabList;
            return true;
        }

        private bool TrySelectSharedSqlNavigatorFromShortcut()
        {
            if (IsDisposed || Disposing || !tabControl1.Visible
                || splitContainerMainWorkspace.IsDisposed || !splitContainerMainWorkspace.Visible
                || c1DockingTabLeftPanel.IsDisposed || c1GridSharedSqlNavigator.IsDisposed
                || !_sharedSqlNavigatorInitialized || _sharedSqlNavigatorRefreshing
                || _sharedSqlNavigatorApplyingLayout
                || !LeftPanelAvailabilityPolicy.Resolve(CaptureLeftPanelContext()).IsEnabled(LeftPanelPage.SqlList))
            {
                return false;
            }

            RefreshSharedSqlNavigator();

            var selectedTab = tabControl1.SelectedTab;
            var query = selectedTab?.Control as QueryForm;
            var snapshot = _sharedSqlNavigatorSnapshot;

            if (query == null || snapshot == null || !tabLeftSqlNavigator.TabVisible
                || !ReferenceEquals(c1GridSharedSqlNavigator.DataSource, snapshot.Table)
                || !query.IsSharedSqlNavigatorSnapshotCurrent(snapshot, CaptureLeftPanelContext(), selectedTab.AccessibleDescription))
            {
                return false;
            }

            if (splitContainerMainWorkspace.Panel1Collapsed)
            {
                splitContainerMainWorkspace.Panel1Collapsed = false;
            }

            c1DockingTabLeftPanel.SelectedTab = tabLeftSqlNavigator;

            return true;
        }

        private sealed class SharedWorkspaceShortcutMessageFilter : IMessageFilter
        {
            private const int WmKeyDown = 0x0100;
            private readonly MainForm _owner;

            internal SharedWorkspaceShortcutMessageFilter(MainForm owner)
            {
                _owner = owner;
            }

            public bool PreFilterMessage(ref Message message)
            {
                //Accept only unmodified workspace function keys.
                if (message.Msg != WmKeyDown || Control.ModifierKeys != Keys.None)
                {
                    return false;
                }

                var keyData = (Keys)message.WParam.ToInt32();

                if (keyData != Keys.F8 && keyData != Keys.F9)
                {
                    return false;
                }

                //Do not intercept input from other windows, modal dialogs or disposed MainForms.
                if (_owner.IsDisposed || _owner.Disposing || !_owner.ContainsFocus)
                {
                    return false;
                }

                switch (keyData)
                {
                    case Keys.F8:
                        {
                            _owner.TrySelectSharedTabListFromShortcut();
                            return true;
                        }
                    case Keys.F9:
                        {
                            _owner.TrySelectSharedSqlNavigatorFromShortcut();
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
        }
    }
}
