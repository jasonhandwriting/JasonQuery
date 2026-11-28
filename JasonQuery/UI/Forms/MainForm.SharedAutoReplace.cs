using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.Services;
using System;
using System.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        private readonly SharedAutoReplaceState _sharedAutoReplaceState = new SharedAutoReplaceState();

        private void InitializeSharedAutoReplace()
        {
            var grid = c1GridSharedAutoReplace;

            grid.AllowUpdate = true;
            grid.AllowUpdateOnBlur = true;
            grid.CausesValidation = true;
            grid.AllowAddNew = false;
            grid.AllowDelete = false;
            grid.AllowSort = false;
            grid.AllowFilter = false;
            grid.SpringMode = false;
            grid.Cursor = Cursors.Default;

            grid.SizeChanged += SharedAutoReplaceSizeChanged;
            grid.VisibleChanged += SharedAutoReplaceSizeChanged;

            btnHelp_AutoReplace.Click += SharedAutoReplaceHelpClick;

            //Configuration is not available in the MainForm constructor.
            tabLeftAutoReplace.TabVisible = false;
        }

        internal IDictionary<string, string> GetSharedAutoReplaceMap()
        {
            return _sharedAutoReplaceState.GetMap(MyLibrary.EnableAutoReplace);
        }

        private void DisposeSharedAutoReplace()
        {
            var table = _sharedAutoReplaceState.WorkingTable;

            if (table != null)
            {
                table.RowChanged -= SharedAutoReplaceRowChanged;
                table.RowDeleted -= SharedAutoReplaceRowChanged;
            }

            c1GridSharedAutoReplace.SizeChanged -= SharedAutoReplaceSizeChanged;
            c1GridSharedAutoReplace.VisibleChanged -= SharedAutoReplaceSizeChanged;
            btnHelp_AutoReplace.Click -= SharedAutoReplaceHelpClick;
        }

        //Called after LoadDefaultSetting has loaded the feature and grid settings.
        //Reuse the already retrieved settings table; never query per editor or tab change.
        private void RefreshSharedAutoReplace(DataTable settings)
        {
            if (!MyLibrary.EnableAutoReplace)
            {
                HideSharedAutoReplace();
                return;
            }

            try
            {
                var previousTable = _sharedAutoReplaceState.WorkingTable;

                _sharedAutoReplaceState.RefreshPermanentRules(true, () => CreateSharedAutoReplaceSource(settings));

                var table = _sharedAutoReplaceState.WorkingTable;

                if (previousTable != null && !ReferenceEquals(previousTable, table))
                {
                    previousTable.RowChanged -= SharedAutoReplaceRowChanged;
                    previousTable.RowDeleted -= SharedAutoReplaceRowChanged;
                }

                var grid = c1GridSharedAutoReplace;
                var needsBinding = !ReferenceEquals(grid.DataSource, table);

                if (needsBinding)
                {
                    grid.DataSource = table;
                    table.RowChanged += SharedAutoReplaceRowChanged;
                    table.RowDeleted += SharedAutoReplaceRowChanged;
                }

                _sharedAutoReplaceState.PublishEdits(true);

                ApplySharedAutoReplaceLocalization();
                ApplySharedAutoReplaceVisualParity();

                if (needsBinding)
                {
                    var keyword = grid.Splits[0].DisplayColumns[0];

                    keyword.AutoSize();

                    if (keyword.Width > 100)
                    {
                        keyword.Width = 70;
                    }

                    keyword.Width = Math.Max(40, keyword.Width);

                    ResizeSharedAutoReplaceRows();
                }

                tabLeftAutoReplace.TabVisible = true;

                LayoutSharedAutoReplace();
            }
            catch (Exception ex)
            {
                HideSharedAutoReplace();

                ExceptionDialogService.Show(this, ex);
            }
        }

        private static DataTable CreateSharedAutoReplaceSource(DataTable settings)
        {
            if (settings == null || !settings.Columns.Contains("AttributeKey")
                || !settings.Columns.Contains("AttributeName") || !settings.Columns.Contains("AttributeValue"))
            {
                throw new InvalidOperationException("Auto Replace settings could not be loaded.");
            }

            var source = new DataTable();

            source.Columns.Add(SharedAutoReplaceState.KeywordColumn, typeof(string));
            source.Columns.Add(SharedAutoReplaceState.ReplacementColumn, typeof(string));

            foreach (DataRow row in settings.Rows)
            {
                if (!string.Equals(row.GetSafeString("AttributeKey"), "AutoReplaceConfig", StringComparison.Ordinal)
                    || !string.Equals(row.GetSafeString("AttributeName"), "AutoReplace", StringComparison.Ordinal))
                {
                    continue;
                }

                var parts = row.GetSafeString("AttributeValue").Split(new[] { MyGlobal.Separator3s }, StringSplitOptions.None);

                source.Rows.Add(parts.Length > 0 ? parts[0] : string.Empty, parts.Length > 1 ? parts[1] : string.Empty);
            }

            return source;
        }

        private void HideSharedAutoReplace()
        {
            if (c1DockingTabLeftPanel.SelectedTab == tabLeftAutoReplace)
            {
                c1DockingTabLeftPanel.SelectedTab = tabLeftSchemaExplorer;
            }

            tabLeftAutoReplace.TabVisible = false;
        }

        private void SharedAutoReplaceRowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (ReferenceEquals(sender, _sharedAutoReplaceState.WorkingTable))
            {
                _sharedAutoReplaceState.PublishEdits(MyLibrary.EnableAutoReplace);
            }
        }

        private void ApplySharedAutoReplaceLocalization()
        {
            lblAutoReplace.Text = tabLeftAutoReplace.Text;

            var grid = c1GridSharedAutoReplace;

            if (!MyLibrary.EnableAutoReplace || grid.DataSource == null)
            {
                return;
            }

            grid.Columns[SharedAutoReplaceState.KeywordColumn].Caption = LocalizationHelper.GetLanguageString("Keyword", "form", GetType().Name, "gridheader", "Keyword", "Text");
            grid.Columns[SharedAutoReplaceState.ReplacementColumn].Caption = LocalizationHelper.GetLanguageString("Replacement", "form", GetType().Name, "gridheader", "Replacement", "Text");

            LayoutSharedAutoReplace();
        }

        private void ApplySharedAutoReplaceVisualParity()
        {
            var grid = c1GridSharedAutoReplace;

            if (!MyLibrary.EnableAutoReplace || grid.DataSource == null)
            {
                return;
            }

            var originalMarqueeStyle = grid.MarqueeStyle;

            c1ThemeController1.SetTheme(grid, MyLibrary.IsDarkMode ? "VS2013Dark" : "(default)");
            grid.BackColor = ColorTranslator.FromHtml(MyLibrary.IsDarkMode ? "#2D2D30" : "#F0F0F0");
            GridHelper.SetGridVisualStyle(grid, 10);

            grid.MarqueeStyle = originalMarqueeStyle;
            grid.AllowUpdate = true;
            grid.AllowUpdateOnBlur = true;
            grid.CausesValidation = true;
            grid.AllowAddNew = false;
            grid.AllowDelete = false;
            grid.AllowSort = false;
            grid.AllowFilter = false;
            grid.SpringMode = false;
        }

        private void ResizeSharedAutoReplaceRows()
        {
            var grid = c1GridSharedAutoReplace;

            grid.AllowRowSizing = RowSizingEnum.IndividualRows;
            grid.Splits[0].ColumnCaptionHeight = 20;

            for (var i = 0; i < grid.Splits[0].Rows.Count; i++)
            {
                var row = grid.Splits[0].Rows[i];

                row.AutoSize();
                row.Height = Math.Min(48, Math.Max(20, row.Height)) + 1;
            }
        }

        private void SharedAutoReplaceSizeChanged(object sender, EventArgs e)
        {
            LayoutSharedAutoReplace();
        }

        private void LayoutSharedAutoReplace()
        {
            var grid = c1GridSharedAutoReplace;

            if (!MyLibrary.EnableAutoReplace || grid.IsDisposed || grid.DataSource == null || !grid.Visible)
            {
                return;
            }

            var columns = grid.Splits[0].DisplayColumns;

            if (columns.Count == 2)
            {
                columns[1].Width = Math.Max(60, grid.ClientSize.Width - columns[0].Width - 22);
            }

            var helpLeft = tsAutoReplace.Left + lblAutoReplace.Bounds.Right;

            btnHelp_AutoReplace.Visible = lblAutoReplace.Placement == ToolStripItemPlacement.Main && helpLeft + btnHelp_AutoReplace.Width <= tsAutoReplace.Right;

            if (btnHelp_AutoReplace.Visible)
            {
                btnHelp_AutoReplace.Left = helpLeft;
            }
        }

        private void SharedAutoReplaceHelpClick(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString
            (
                "Auto Replace allows you to bind frequently used query fragments to short keywords.\r\n\r\n" +
                "After typing a keyword and pressing the space key, JasonQuery automatically expands it into the predefined replacement.\r\n\r\n" +
                "This feature is useful for:\r\n" +
                "• Frequently used query templates\r\n" +
                "• Repetitive SQL structures\r\n" +
                "• Quickly inserting long statements or clauses\r\n\r\n" +
                "You can press Ctrl+Z at any time to undo the replacement.",
                "form",
                "OptionsForm",
                "msg",
                "Help_AutoReplace",
                "Text"
            );

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
