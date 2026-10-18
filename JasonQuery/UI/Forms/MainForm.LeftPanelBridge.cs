using JasonLibrary.Core;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using System.Collections.Generic;
using System.Windows.Forms;
using TabPage = Crownwood.Magic.Controls.TabPage;

namespace JasonQuery.UI.Forms
{
    //B1-B: typed navigation + a read-only active-workspace snapshot.
    //The F6-F9 controls remain in QueryForm until the separate UI relocation phase.
    public partial class MainForm : IWorkspaceTabNavigator
    {
        public bool TryActivateTab(string tabIdentity)
        {
            if (IsDisposed || !tabControl1.Visible || string.IsNullOrWhiteSpace(tabIdentity))
            {
                return false;
            }

            var identities = new List<string>();

            foreach (TabPage tab in tabControl1.TabPages)
            {
                identities.Add(tab.AccessibleDescription);
            }

            var targetIndex = WorkspaceTabNavigationPolicy.FindTargetIndex(identities, tabIdentity);

            if (targetIndex < 0)
            {
                return false;
            }

            tabControl1.TabPages[targetIndex].Selected = true;
            return true;
        }

        //Snapshot only: no grid, database or timer side effects.
        //Must be recaptured when the tab, connection or schema changes.
        internal LeftPanelContext CaptureLeftPanelContext()
        {
            var isConnected = tabControl1.Visible && _currentSourceType != DataSourceType.None;
            var activeControl = tabControl1.SelectedTab?.Control;
            var activeQuery = activeControl as QueryForm;
            var target = LeftPanelTargetKind.None;

            if (activeQuery != null)
            {
                target = LeftPanelTargetKind.QueryForm;
            }
            else if (activeControl is SchemaBrowserForm)
            {
                target = LeftPanelTargetKind.SchemaBrowserForm;
            }
            else if (activeControl != null)
            {
                target = LeftPanelTargetKind.OtherForm;
            }

            return new LeftPanelContext
            (
                isConnected,
                target,
                MyLibrary.EnableAutoReplace,
                DatabaseSqlExecutor.dtSchema != null,
                tabControl1.TabPages.Count > 0,
                activeQuery != null && activeQuery.HasSqlNavigatorItemsForMainPanel
            );
        }
    }
}
