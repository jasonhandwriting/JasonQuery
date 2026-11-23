using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //MainForm owns shortcuts for the shared left workspace.
    //F8 is the first activated route; F6, F7 and F9 remain on their existing paths until each page's behavior and shortcut conflicts are qualified.
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
                //Accept the unmodified F8 key only. A message filter receives key events before QueryForm or ComponentOne key processing.
                if (message.Msg != WmKeyDown || message.WParam.ToInt32() != (int)Keys.F8 || Control.ModifierKeys != Keys.None)
                {
                    return false;
                }

                //The application-wide filter must NOT intercept keyboard input from other windows, modal dialogs, or disposed MainForms.
                if (_owner.IsDisposed || _owner.Disposing || !_owner.ContainsFocus)
                {
                    return false;
                }

                //F8 belongs to MainForm, regardless of which embedded child control (QueryForm, SchemaBrowserForm, F7 grid, etc.) has focus.
                //When no workspace is available, consume F8 without falling through to the retired QueryForm-local F8 shortcut.
                _owner.TrySelectSharedTabListFromShortcut();
                return true;
            }
        }
    }
}
