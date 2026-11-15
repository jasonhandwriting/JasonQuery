using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Localization;
using JasonQuery.UI.LeftPanel;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    //F7-U4A-C1: MainForm-owned display filter and existing F7 toolbar.
    //Filtering does not authorize a selected object or any database action.
    public partial class MainForm
    {
        private string _sharedSchemaFilterAppliedText = "*";

        private void InitializeSharedSchemaExplorerFilterUI()
        {
            txtSchemaFilter.KeyDown -= SharedSchemaFilterKeyDown;
            txtSchemaFilter.KeyDown += SharedSchemaFilterKeyDown;

            btnRefresh.Click -= SharedSchemaPreviewRefreshClick;
            btnRefresh.Click += SharedSchemaPreviewRefreshClick;

            btnExpandCollapse.DropDownItemClicked -= SharedSchemaExpandCollapseClicked;
            btnExpandCollapse.DropDownItemClicked += SharedSchemaExpandCollapseClicked;

            btnHelp_SchemaFilter.Click -= SharedSchemaFilterHelpClick;
            btnHelp_SchemaFilter.Click += SharedSchemaFilterHelpClick;

            LayoutSharedSchemaFilterUI();
        }

        private void DisposeSharedSchemaExplorerFilterUI()
        {
            txtSchemaFilter.KeyDown -= SharedSchemaFilterKeyDown;
            btnRefresh.Click -= SharedSchemaPreviewRefreshClick;
            btnExpandCollapse.DropDownItemClicked -= SharedSchemaExpandCollapseClicked;
            btnHelp_SchemaFilter.Click -= SharedSchemaFilterHelpClick;
        }

        //Designer owns the control; the left F7 page changes width as the MainForm SplitContainer divider moves. Preserve the control's Left.
        private void LayoutSharedSchemaFilterUI()
        {
            if (tabLeftSchemaExplorer.IsDisposed || txtSchemaFilter.IsDisposed)
            {
                return;
            }

            var width = Math.Max(24, tabLeftSchemaExplorer.ClientSize.Width - txtSchemaFilter.Left - 5);

            if (txtSchemaFilter.Width != width)
            {
                txtSchemaFilter.Width = width;
            }
        }

        //Retain only the last successfully applied filter after a trusted, same-scope metadata refresh. Never inherit a previous database's filter.
        private string GetSharedSchemaFilterForRebind(SharedSchemaMetadataSnapshot previous, SharedSchemaMetadataSnapshot next)
        {
            if (previous != null && next != null
                && previous.SourceType == next.SourceType
                && previous.ConnectionRevision == next.ConnectionRevision
                && !string.IsNullOrWhiteSpace(previous.DatabaseScope)
                && string.Equals(previous.DatabaseScope, next.DatabaseScope, StringComparison.Ordinal))
            {
                return _sharedSchemaFilterAppliedText;
            }

            return "*";
        }

        private void SharedSchemaFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || !ReferenceEquals(sender, txtSchemaFilter))
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            var grid = _sharedSchemaExplorerGrid;
            var snapshot = _sharedSchemaMetadataSnapshot;

            if (snapshot == null || !CanHandleSharedSchemaExplorerInput(grid))
            {
                return;
            }

            //Any previously captured right-click identity is stale after filtering.
            DisposeSharedSchemaCopyContextMenu();

            var searchText = txtSchemaFilter.Text ?? string.Empty;
            var normalizedText = string.IsNullOrWhiteSpace(searchText) ? "*" : searchText;

            if (!SharedSchemaExplorerFilterPolicy.TryApplyFilter(snapshot, normalizedText))
            {
                SetSharedSchemaExplorerStatus("F7: invalid filter; previous results retained.");

                return;
            }

            _sharedSchemaFilterAppliedText = normalizedText;
            txtSchemaFilter.Text = normalizedText;
            SetSharedSchemaExplorerStatus(string.Empty);

            grid.Refresh();
            ExpandSharedSchemaExplorerPreviewDefaultGroups();
            grid.Focus();
        }

        private void SharedSchemaExpandCollapseClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var grid = _sharedSchemaExplorerGrid;

            if (e == null || !CanHandleSharedSchemaExplorerInput(grid) || grid.Splits.Count == 0)
            {
                return;
            }

            var expand = ReferenceEquals(e.ClickedItem, mnuExpandAll);
            var collapse = ReferenceEquals(e.ClickedItem, mnuCollapseAll);

            if (!expand && !collapse)
            {
                return;
            }

            try
            {
                var rows = grid.Splits[0].Rows;

                if (expand)
                {
                    for (var i = 0; i < rows.Count; i++)
                    {
                        if (rows[i].RowType == RowTypeEnum.CollapsedGroupRow)
                        {
                            grid.ExpandGroupRow(i);
                        }
                    }
                }

                if (collapse)
                {
                    for (var i = rows.Count - 1; i >= 0; i--)
                    {
                        if (rows[i].RowType == RowTypeEnum.ExpandedGroupRow)
                        {
                            grid.CollapseGroupRow(i);
                        }
                    }

                    if (rows.Count > 0 && rows[0].RowType == RowTypeEnum.CollapsedGroupRow)
                    {
                        grid.ExpandGroupRow(0);
                    }
                }
            }
            catch (Exception)
            {
                //A C1 regroup is UI-only. Never modify metadata or dispatch DDL.
                SetSharedSchemaExplorerStatus("F7: could not update group expansion.");
            }
        }

        //MainForm.ApplyLocalization() currently translates the top-level menus, not these Designer-owned F6/F7/F8/F9 pages or their ToolStrip items. Apply only this known set.
        private void ApplySharedSchemaExplorerLocalization()
        {
            tabLeftAutoReplace.Text = GetSharedSchemaExplorerLocalizedText(tabLeftAutoReplace.Text, tabLeftAutoReplace.Name, "Text");
            tabLeftSchemaExplorer.Text = GetSharedSchemaExplorerLocalizedText(tabLeftSchemaExplorer.Text, tabLeftSchemaExplorer.Name, "Text");
            tabLeftTabList.Text = GetSharedSchemaExplorerLocalizedText(tabLeftTabList.Text, tabLeftTabList.Name, "Text");
            tabLeftSqlNavigator.Text = GetSharedSchemaExplorerLocalizedText(tabLeftSqlNavigator.Text, tabLeftSqlNavigator.Name, "Text");

            btnRefresh.ToolTipText = GetSharedSchemaExplorerLocalizedText(btnRefresh.ToolTipText, btnRefresh.Name, "ToolTipText");
            btnExpandCollapse.ToolTipText = GetSharedSchemaExplorerLocalizedText(btnExpandCollapse.ToolTipText, btnExpandCollapse.Name, "ToolTipText");
            mnuExpandAll.Text = GetSharedSchemaExplorerLocalizedText(mnuExpandAll.Text, mnuExpandAll.Name, "Text");
            mnuCollapseAll.Text = GetSharedSchemaExplorerLocalizedText(mnuCollapseAll.Text, mnuCollapseAll.Name, "Text");
            btnSettingOfFocus.ToolTipText = GetSharedSchemaExplorerLocalizedText(btnSettingOfFocus.ToolTipText, btnSettingOfFocus.Name, "ToolTipText");
            mnuFocusOnDataGrid.Text = GetSharedSchemaExplorerLocalizedText(mnuFocusOnDataGrid.Text, mnuFocusOnDataGrid.Name, "Text");
            mnuFocusOnQueryEditor.Text = GetSharedSchemaExplorerLocalizedText(mnuFocusOnQueryEditor.Text, mnuFocusOnQueryEditor.Name, "Text");
            lblSchemaFilter.Text = GetSharedSchemaExplorerLocalizedText(lblSchemaFilter.Text, lblSchemaFilter.Name, "Text");
            btnHelp_SchemaFilter.ToolTipText = GetSharedSchemaExplorerLocalizedText(btnHelp_SchemaFilter.ToolTipText, btnHelp_SchemaFilter.Name, "ToolTipText");

            LayoutSharedSchemaFilterUI();
        }

        private string GetSharedSchemaExplorerLocalizedText(string currentText, string controlName, string attribute)
        {
            return LocalizationHelper.GetLanguageString(currentText, "form", GetType().Name, "object", controlName, attribute);
        }

        private void SharedSchemaFilterHelpClick(object sender, EventArgs e)
        {
            var title = LocalizationHelper.GetLanguageString
            (
                "Schema Filter Overview",
                "form",
                GetType().Name,
                "msg",
                "Help_SchemaFilterTitle",
                "Text"
            );

            var message = LocalizationHelper.GetLanguageString
            (
                "The Schema Filter allows you to search database objects using keywords and display the results in a tree structure.",
                "form",
                GetType().Name,
                "msg",
                "Help_SchemaFilter",
                "Text"
            );

            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
