using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //F8-B: MainForm owns only the shared, read-only Tab List view.
    //QueryForm's legacy F8 grid and notification path remain unchanged.
    public partial class MainForm
    {
        private void InitializeSharedTabList()
        {
            c1GridSharedTabList.MouseDoubleClick += SharedTabListMouseDoubleClick;
            c1GridSharedTabList.SizeChanged += SharedTabListSizeChanged;
        }

        internal bool TryShowSharedTabList()
        {
            if (IsDisposed || Disposing || splitContainerMainWorkspace.IsDisposed || !splitContainerMainWorkspace.Visible
                || c1DockingTabLeftPanel.IsDisposed || !LeftPanelAvailabilityPolicy.Resolve(CaptureLeftPanelContext()).IsEnabled(LeftPanelPage.TabList))
            {
                return false;
            }

            if (splitContainerMainWorkspace.Panel1Collapsed)
            {
                splitContainerMainWorkspace.Panel1Collapsed = false;
            }

            c1DockingTabLeftPanel.SelectedTab = tabLeftTabList;
            return true;
        }

        private void DisposeSharedTabList()
        {
            c1GridSharedTabList.MouseDoubleClick -= SharedTabListMouseDoubleClick;
            c1GridSharedTabList.SizeChanged -= SharedTabListSizeChanged;
        }

        private void RefreshSharedTabList()
        {
            if (IsDisposed || Disposing || c1GridSharedTabList.IsDisposed)
            {
                return;
            }

            //Never bind MyGlobal.dtTabList directly: legacy QueryForm uses that table.
            c1GridSharedTabList.DataSource = SharedTabListSnapshotPolicy.Create(MyGlobal.dtTabList);
            c1GridSharedTabList.AllowUpdate = false;

            if (c1GridSharedTabList.Splits.Count > 0)
            {
                c1GridSharedTabList.Splits[0].RecordSelectors = false;

                foreach (C1DisplayColumn column in c1GridSharedTabList.Splits[0].DisplayColumns)
                {
                    if (string.Equals(column.Name, "Tab", StringComparison.OrdinalIgnoreCase))
                    {
                        column.Visible = true;
                    }
                    else if (string.Equals(column.Name, "AccessibleDescription", StringComparison.OrdinalIgnoreCase))
                    {
                        column.Visible = false;
                        column.Frozen = true;
                    }
                }
            }

            ResizeSharedTabListColumn();
            ApplySharedTabListLocalization();
        }

        private void ApplySharedTabListLocalization()
        {
            if (c1GridSharedTabList.IsDisposed || c1GridSharedTabList.DataSource == null)
            {
                return;
            }

            //Reuse the existing QueryForm F8 gridheader key, without adding localization keys.
            c1GridSharedTabList.Columns["Tab"].Caption = LocalizationHelper.GetLanguageString
            (
                "Double-click the cell to switch to the tab",
                "form",
                "QueryForm",
                "gridheader",
                "DoubleClickToSwitchTab",
                "Text"
            );
        }

        private void SharedTabListSizeChanged(object sender, EventArgs e)
        {
            ResizeSharedTabListColumn();
        }

        private void ResizeSharedTabListColumn()
        {
            if (c1GridSharedTabList.IsDisposed || c1GridSharedTabList.DataSource == null || c1GridSharedTabList.Splits.Count == 0)
            {
                return;
            }

            foreach (C1DisplayColumn column in c1GridSharedTabList.Splits[0].DisplayColumns)
            {
                if (string.Equals(column.Name, "Tab", StringComparison.OrdinalIgnoreCase))
                {
                    var desiredWidth = Math.Max(60, c1GridSharedTabList.ClientSize.Width - 4);

                    if (column.Width != desiredWidth)
                    {
                        column.Width = desiredWidth;
                    }

                    return;
                }
            }
        }

        private void SharedTabListMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!ReferenceEquals(sender, c1GridSharedTabList)
                || e.Button != MouseButtons.Left
                || IsDisposed
                || Disposing
                || c1GridSharedTabList.IsDisposed
                || c1GridSharedTabList.DataSource == null
                || !tabControl1.Visible)
            {
                return;
            }

            var displayRowIndex = c1GridSharedTabList.RowContaining(e.Y);

            if (displayRowIndex < 0 || c1GridSharedTabList.Splits.Count == 0 || displayRowIndex >= c1GridSharedTabList.Splits[0].Rows.Count)
            {
                return;
            }

            var tabIdentity = c1GridSharedTabList.Columns["AccessibleDescription"].CellValue(displayRowIndex)?.ToString();

            if (string.IsNullOrWhiteSpace(tabIdentity))
            {
                return;
            }

            //TryActivateTab rechecks current MDI identities, rejecting stale/closed tabs.
            TryActivateTab(tabIdentity);
        }
    }
}
