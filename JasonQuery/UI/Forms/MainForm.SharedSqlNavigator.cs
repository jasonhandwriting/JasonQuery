using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        private SharedSqlNavigatorSnapshot _sharedSqlNavigatorSnapshot;
        private bool _sharedSqlNavigatorInitialized;
        private bool _sharedSqlNavigatorRefreshing;
        private int _sharedSqlNavigatorBaseRowHeight;
        private bool _sharedSqlNavigatorApplyingLayout;
        private int _sharedSqlNavigatorTypeWidth;
        private int _sharedSqlNavigatorSqlWidth;
        private int _sharedSqlNavigatorLayoutWidth;

        private void InitializeSharedSqlNavigator()
        {
            if (_sharedSqlNavigatorInitialized)
            {
                return;
            }

            var grid = c1GridSharedSqlNavigator;

            _sharedSqlNavigatorBaseRowHeight = grid.RowHeight;

            grid.AllowUpdate = false;
            grid.AllowUpdateOnBlur = false;
            grid.AllowFilter = false;
            grid.Cursor = Cursors.Default;
            grid.SpringMode = false;

            ApplySharedSqlNavigatorRowStyle();

            tabLeftSqlNavigator.TabVisible = false;

            grid.SizeChanged += SharedSqlNavigatorSizeChanged;
            grid.VisibleChanged += SharedSqlNavigatorVisibleChanged;
            grid.ColResize += SharedSqlNavigatorColResize;
            grid.Filter += SharedSqlNavigatorFilter;
            grid.MouseDoubleClick += SharedSqlNavigatorMouseDoubleClick;

            _sharedSqlNavigatorInitialized = true;
        }

        private void DisposeSharedSqlNavigator()
        {
            _sharedSqlNavigatorInitialized = false;
            c1GridSharedSqlNavigator.SizeChanged -= SharedSqlNavigatorSizeChanged;
            c1GridSharedSqlNavigator.VisibleChanged -= SharedSqlNavigatorVisibleChanged;
            c1GridSharedSqlNavigator.ColResize -= SharedSqlNavigatorColResize;
            c1GridSharedSqlNavigator.Filter -= SharedSqlNavigatorFilter;
            c1GridSharedSqlNavigator.MouseDoubleClick -= SharedSqlNavigatorMouseDoubleClick;
            _sharedSqlNavigatorSnapshot = null;
        }

        //A notification carries no display data or navigation authority.
        internal void NotifySharedSqlNavigatorRefreshed(QueryForm source)
        {
            if (!_sharedSqlNavigatorInitialized || IsDisposed || Disposing || source == null
                || !ReferenceEquals(tabControl1.SelectedTab?.Control, source))
            {
                return;
            }

            RefreshSharedSqlNavigator();
        }

        private void RefreshSharedSqlNavigator()
        {
            var grid = c1GridSharedSqlNavigator;

            if (!_sharedSqlNavigatorInitialized || _sharedSqlNavigatorRefreshing || IsDisposed || Disposing || grid.IsDisposed)
            {
                return;
            }

            _sharedSqlNavigatorRefreshing = true;

            try
            {
                var context = CaptureLeftPanelContext();
                var selectedTab = tabControl1.SelectedTab;
                var query = selectedTab?.Control as QueryForm;
                var identity = selectedTab?.AccessibleDescription;

                if (query == null || query.IsDisposed || query.Disposing || !LeftPanelAvailabilityPolicy.Resolve(context).SqlList)
                {
                    ClearSharedSqlNavigator();
                    return;
                }

                if (query.IsSharedSqlNavigatorSnapshotCurrent(_sharedSqlNavigatorSnapshot, context, identity))
                {
                    return;
                }

                var snapshot = query.CaptureSharedSqlNavigatorSnapshot();

                if (!query.IsSharedSqlNavigatorSnapshotCurrent(snapshot, context, identity))
                {
                    ClearSharedSqlNavigator();
                    return;
                }

                //Save the session layout before C1 recreates its bound columns.
                CaptureSharedSqlNavigatorColumnWidths();

                //Never bind the QueryForm's working DataTable to this grid.
                _sharedSqlNavigatorSnapshot = null;

                grid.DataSource = snapshot.Table;
                grid.AllowUpdate = false;
                grid.AllowUpdateOnBlur = false;
                grid.AllowFilter = false;
                grid.AllowSort = false;
                grid.Columns["Type"].Tag = "Type";
                grid.Columns["Sql"].Tag = "Sql";
                grid.Columns["Position"].Tag = "Position";

                ApplySharedSqlNavigatorLocalization();
                GridHelper.SetGridHeaderLine(grid);
                ApplySharedSqlNavigatorRowStyle();

                //Hide Position and restore saved widths even if the page is not visible yet.
                ApplySharedSqlNavigatorColumnLayout();

                _sharedSqlNavigatorSnapshot = snapshot;
                tabLeftSqlNavigator.TabVisible = true;

                //The first usable layout may only exist after TabVisible changes.
                ApplySharedSqlNavigatorColumnLayout();
            }
            finally
            {
                _sharedSqlNavigatorRefreshing = false;
            }
        }

        private void ClearSharedSqlNavigator()
        {
            CaptureSharedSqlNavigatorColumnWidths();

            _sharedSqlNavigatorSnapshot = null;

            if (c1GridSharedSqlNavigator.DataSource != null)
            {
                c1GridSharedSqlNavigator.DataSource = null;
            }

            if (c1DockingTabLeftPanel.SelectedTab == tabLeftSqlNavigator)
            {
                c1DockingTabLeftPanel.SelectedTab = tabLeftSchemaExplorer;
            }

            tabLeftSqlNavigator.TabVisible = false;
        }

        private void ApplySharedSqlNavigatorLocalization()
        {
            var grid = c1GridSharedSqlNavigator;

            if (grid.IsDisposed || grid.DataSource == null || _sharedSqlNavigatorApplyingLayout)
            {
                return;
            }

            CaptureSharedSqlNavigatorColumnWidths();

            _sharedSqlNavigatorApplyingLayout = true;

            try
            {
                grid.Columns["Type"].Caption = LocalizationHelper.GetLanguageString("Type", "form", GetType().Name, "gridheader", "Type", "Text");
                grid.Columns["Sql"].Caption = LocalizationHelper.GetLanguageString("SQL Statement", "form", GetType().Name, "gridheader", "SQLStatement", "Text");
            }
            finally
            {
                _sharedSqlNavigatorApplyingLayout = false;
            }

            if (!_sharedSqlNavigatorRefreshing)
            {
                ApplySharedSqlNavigatorColumnLayout();
            }
        }

        private void ApplySharedSqlNavigatorVisualParity()
        {
            var grid = c1GridSharedSqlNavigator;

            if (!_sharedSqlNavigatorInitialized || grid.IsDisposed || _sharedSqlNavigatorApplyingLayout)
            {
                return;
            }

            CaptureSharedSqlNavigatorColumnWidths();

            _sharedSqlNavigatorApplyingLayout = true;

            try
            {
                c1ThemeController1.SetTheme(grid, MyLibrary.IsDarkMode ? "VS2013Dark" : "(default)");

                grid.BackColor = ColorTranslator.FromHtml(MyLibrary.IsDarkMode ? "#2D2D30" : "#F0F0F0");

                GridHelper.SetGridVisualStyle(grid);

                var originalMarqueeStyle = grid.MarqueeStyle;

                GridHelper.SetGridVisualStyle(grid, 10);

                grid.MarqueeStyle = originalMarqueeStyle;
                grid.AllowUpdate = false;
                grid.AllowUpdateOnBlur = false;
                grid.AllowFilter = false;
                grid.AllowSort = false;
                grid.SpringMode = false;

                ApplySharedSqlNavigatorRowStyle();
            }
            finally
            {
                _sharedSqlNavigatorApplyingLayout = false;
            }

            ApplySharedSqlNavigatorColumnLayout();
        }

        private void SharedSqlNavigatorSizeChanged(object sender, EventArgs e)
        {
            var grid = c1GridSharedSqlNavigator;

            if (_sharedSqlNavigatorInitialized && !_sharedSqlNavigatorRefreshing && !_sharedSqlNavigatorApplyingLayout
                && !IsDisposed && !Disposing && !grid.IsDisposed && grid.Visible
                && grid.ClientSize.Width > 0 && grid.ClientSize.Width != _sharedSqlNavigatorLayoutWidth)
            {
                ApplySharedSqlNavigatorColumnLayout();
            }
        }

        private void SharedSqlNavigatorVisibleChanged(object sender, EventArgs e)
        {
            if (!_sharedSqlNavigatorRefreshing && c1GridSharedSqlNavigator.Visible)
            {
                //A hidden page may complete C1 layout when shown, even without a width change.
                ApplySharedSqlNavigatorColumnLayout();
            }
        }

        private static C1DisplayColumn FindSharedSqlNavigatorDisplayColumn(C1TrueDBGrid grid, string dataField)
        {
            if (grid.IsDisposed || grid.DataSource == null || grid.Splits.Count == 0)
            {
                return null;
            }

            foreach (C1DisplayColumn column in grid.Splits[0].DisplayColumns)
            {
                if (string.Equals(column.DataColumn?.DataField, dataField, StringComparison.Ordinal))
                {
                    return column;
                }
            }

            return null;
        }

        private void CaptureSharedSqlNavigatorColumnWidths()
        {
            var grid = c1GridSharedSqlNavigator;

            if (_sharedSqlNavigatorSnapshot == null || grid.IsDisposed || !grid.Visible
                || _sharedSqlNavigatorLayoutWidth <= 0 || grid.ClientSize.Width != _sharedSqlNavigatorLayoutWidth
                || !ReferenceEquals(grid.DataSource, _sharedSqlNavigatorSnapshot.Table))
            {
                return;
            }

            var typeColumn = FindSharedSqlNavigatorDisplayColumn(grid, "Type");
            var sqlColumn = FindSharedSqlNavigatorDisplayColumn(grid, "Sql");

            if (typeColumn != null && typeColumn.Visible && typeColumn.Width > 0)
            {
                _sharedSqlNavigatorTypeWidth = typeColumn.Width;
            }

            if (sqlColumn != null && sqlColumn.Visible && sqlColumn.Width > 0)
            {
                _sharedSqlNavigatorSqlWidth = sqlColumn.Width;
            }
        }

        private void SharedSqlNavigatorColResize(object sender, ColResizeEventArgs e)
        {
            var grid = c1GridSharedSqlNavigator;

            if (!_sharedSqlNavigatorInitialized || _sharedSqlNavigatorRefreshing || _sharedSqlNavigatorApplyingLayout
                || IsDisposed || Disposing || grid.IsDisposed || !grid.Visible || !ReferenceEquals(sender, grid)
                || _sharedSqlNavigatorSnapshot == null || !ReferenceEquals(grid.DataSource, _sharedSqlNavigatorSnapshot.Table)
                || _sharedSqlNavigatorLayoutWidth <= 0 || grid.ClientSize.Width != _sharedSqlNavigatorLayoutWidth
                || e.Cancel || e.Column == null || e.Column.Width <= 0)
            {
                return;
            }

            //Only user-visible column widths belong to the session layout.
            var field = e.Column.DataColumn?.DataField;

            if (string.Equals(field, "Type", StringComparison.Ordinal))
            {
                _sharedSqlNavigatorTypeWidth = e.Column.Width;
            }
            else if (string.Equals(field, "Sql", StringComparison.Ordinal))
            {
                _sharedSqlNavigatorSqlWidth = e.Column.Width;
            }
        }

        private void ApplySharedSqlNavigatorColumnLayout()
        {
            var grid = c1GridSharedSqlNavigator;

            if (!_sharedSqlNavigatorInitialized || _sharedSqlNavigatorApplyingLayout || IsDisposed || Disposing
                || grid.IsDisposed || grid.DataSource == null || grid.Splits.Count == 0)
            {
                return;
            }

            _sharedSqlNavigatorApplyingLayout = true;

            try
            {
                var typeColumn = FindSharedSqlNavigatorDisplayColumn(grid, "Type");
                var sqlColumn = FindSharedSqlNavigatorDisplayColumn(grid, "Sql");
                var positionColumn = FindSharedSqlNavigatorDisplayColumn(grid, "Position");

                if (positionColumn != null)
                {
                    positionColumn.Visible = false;
                    positionColumn.Frozen = true;
                }

                if (typeColumn == null || sqlColumn == null)
                {
                    return;
                }

                grid.SpringMode = false;
                typeColumn.Visible = true;
                sqlColumn.Visible = true;

                var clientWidth = grid.ClientSize.Width;

                //Initialize only with a usable visible grid. Hidden-page layout must not overwrite saved widths.
                if (grid.Visible && clientWidth > 0)
                {
                    if (_sharedSqlNavigatorTypeWidth <= 0)
                    {
                        _sharedSqlNavigatorTypeWidth = MeasureSharedSqlNavigatorTypeWidth(grid);
                    }

                    if (_sharedSqlNavigatorSqlWidth <= 0 || clientWidth != _sharedSqlNavigatorLayoutWidth)
                    {
                        //Keep the user's Type width. Only a genuine client-width change fills the remaining SQL space.
                        _sharedSqlNavigatorSqlWidth = Math.Max(24, clientWidth - _sharedSqlNavigatorTypeWidth - 22);
                        _sharedSqlNavigatorLayoutWidth = clientWidth;
                    }
                }

                if (_sharedSqlNavigatorTypeWidth > 0 && typeColumn.Width != _sharedSqlNavigatorTypeWidth)
                {
                    typeColumn.Width = _sharedSqlNavigatorTypeWidth;
                }

                if (_sharedSqlNavigatorSqlWidth > 0 && sqlColumn.Width != _sharedSqlNavigatorSqlWidth)
                {
                    sqlColumn.Width = _sharedSqlNavigatorSqlWidth;
                }
            }
            finally
            {
                _sharedSqlNavigatorApplyingLayout = false;
            }
        }

        private static int MeasureSharedSqlNavigatorTypeWidth(C1TrueDBGrid grid)
        {
            //Measure only the short Type labels once per MainForm session. No content-driven C1 AutoSize.
            const int minimumWidth = 40;
            const int maximumWidth = 120;
            const int padding = 10;

            var width = TextRenderer.MeasureText(grid.Columns["Type"].Caption, grid.HeadingStyle.Font ?? grid.Font).Width + padding;
            var types = new HashSet<string>(StringComparer.Ordinal);

            if (grid.DataSource is DataTable table)
            {
                foreach (DataRow row in table.Rows)
                {
                    var type = row["Type"] as string;

                    if (!string.IsNullOrEmpty(type) && types.Add(type))
                    {
                        width = Math.Max(width, TextRenderer.MeasureText(type, grid.Font).Width + padding);
                    }

                    if (width >= maximumWidth)
                    {
                        break;
                    }
                }
            }

            return Math.Max(minimumWidth, Math.Min(maximumWidth, width));
        }

        private void ApplySharedSqlNavigatorRowStyle()
        {
            var grid = c1GridSharedSqlNavigator;

            if (grid.IsDisposed)
            {
                return;
            }

            //A bounded two-line preview height; the Sql column retains the complete text.
            var singleRowHeight = Math.Max(_sharedSqlNavigatorBaseRowHeight, grid.Font.Height + 3);
            var previewHeight = singleRowHeight * 2 - 3;

            grid.AllowRowSizing = RowSizingEnum.None;

            if (grid.RowHeight != previewHeight)
            {
                grid.RowHeight = previewHeight;
            }
        }

        private void SharedSqlNavigatorMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var grid = c1GridSharedSqlNavigator;
            var snapshot = _sharedSqlNavigatorSnapshot;

            if (!_sharedSqlNavigatorInitialized || _sharedSqlNavigatorRefreshing || _sharedSqlNavigatorApplyingLayout
                || IsDisposed || Disposing || grid.IsDisposed || !grid.Visible || !ReferenceEquals(sender, grid)
                || e.Button != MouseButtons.Left || c1DockingTabLeftPanel.SelectedTab != tabLeftSqlNavigator
                || snapshot == null || !ReferenceEquals(grid.DataSource, snapshot.Table) || grid.Splits.Count != 1)
            {
                return;
            }

            try
            {
                //A header double-click (AutoSize) or a click outside the data area must not navigate.
                if (!grid.CellContaining(e.X, e.Y, out var displayRowIndex, out var displayColumnIndex)
                    || displayRowIndex < 0 || displayRowIndex >= grid.Splits[0].Rows.Count
                    || displayColumnIndex < 0 || displayColumnIndex >= grid.Splits[0].DisplayColumns.Count)
                {
                    return;
                }

                var field = grid.Splits[0].DisplayColumns[displayColumnIndex].DataColumn?.DataField;

                if (!string.Equals(field, "Type", StringComparison.Ordinal) && !string.Equals(field, "Sql", StringComparison.Ordinal))
                {
                    return;
                }

                //CellValue follows the displayed row, including filtering and scrolling.
                if (!(grid.Columns["Position"].CellValue(displayRowIndex) is int oneBasedPosition))
                {
                    return;
                }

                var selectedTab = tabControl1.SelectedTab;
                var query = selectedTab?.Control as QueryForm;

                if (query == null || !ReferenceEquals(snapshot, _sharedSqlNavigatorSnapshot)
                    || !ReferenceEquals(grid.DataSource, snapshot.Table))
                {
                    return;
                }

                query.TryNavigateSharedSqlNavigatorPosition(snapshot, CaptureLeftPanelContext(), selectedTab.AccessibleDescription, oneBasedPosition);
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(this, ex);
            }
        }

        private void SharedSqlNavigatorFilter(object sender, FilterEventArgs e)
        {
            var grid = c1GridSharedSqlNavigator;
            var snapshot = _sharedSqlNavigatorSnapshot;

            if (!_sharedSqlNavigatorInitialized || _sharedSqlNavigatorRefreshing || IsDisposed || Disposing
                || !ReferenceEquals(sender, grid) || snapshot == null || !ReferenceEquals(grid.DataSource, snapshot.Table))
            {
                return;
            }

            try
            {
                var view = snapshot.Table.DefaultView;

                if (view.RowFilter != e.Condition)
                {
                    view.RowFilter = QueryForm.BuildWildcardFilterCondition(grid, e.Condition, true, 0);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(this, ex);
            }
        }
    }
}
