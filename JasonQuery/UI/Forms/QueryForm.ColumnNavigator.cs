using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private const int ColumnNavigatorMaxWidth = 700;
        private const int ColumnNavigatorMinColumnWidth = 60;
        private const int ColumnNavigatorFilterHeight = 21;
        private const int ColumnNavigatorFilterPadding = 4;
        private const int ColumnNavigatorGridPadding = 22;

        private void txtColumnFilter_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                GridColumnFilterHelper.ApplyColumnNameFilterOnEnter(c1GridColumns, txtColumnFilter, e);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void txtColumnFilter_MouseClick(object sender, MouseEventArgs e)
        {
            GridColumnFilterHelper.SelectAllText(txtColumnFilter);
        }

        private void splitContainer4_SplitterMoved(object sender, SplitterEventArgs e)
        {
            try
            {
                LimitColumnNavigatorWidth();
                ResizeColumnNavigator();

                if (!_isSplitterSaveRequired)
                {
                    return;
                }

                SaveSplitterData("LL/RR", splitContainer4.SplitterDistance);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
            finally
            {
                _isSplitterSaveRequired = false;
            }
        }

        private void splitContainer4_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            _isSplitterSaveRequired = true;
        }

        private void LimitColumnNavigatorWidth()
        {
            if (splitContainer4.Panel1.Width <= ColumnNavigatorMaxWidth)
            {
                return;
            }

            splitContainer4.SplitterDistance = ColumnNavigatorMaxWidth;
        }

        private void ResizeColumnNavigator()
        {
            ResizeColumnFilterTextBox();

            if (c1GridColumns.IsDataTableSourceNullOrEmpty())
            {
                return;
            }

            var columnWidth = c1GridColumns.Width - ColumnNavigatorGridPadding;

            if (columnWidth < ColumnNavigatorMinColumnWidth)
            {
                columnWidth = ColumnNavigatorMinColumnWidth;
            }

            c1GridColumns.Splits[0].DisplayColumns[0].Width = columnWidth;
            c1GridColumns.Refresh();
        }

        private void ResizeColumnFilterTextBox()
        {
            var width = c1GridColumns.Width - lblColumnFilterPosition.Width - ColumnNavigatorFilterPadding;

            if (width < ColumnNavigatorMinColumnWidth)
            {
                width = ColumnNavigatorMinColumnWidth;
            }

            txtColumnFilter.Size = new Size(width, ColumnNavigatorFilterHeight);
        }

        private void c1GridColumns_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            var displayRowIndex = c1GridColumns.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return;
            }

            PositionQueryResultGridColumn(displayRowIndex);
        }

        private void PositionQueryResultGridColumn(int displayRowIndex)
        {
            var c1Grid = GetWhichGrid();

            if (c1Grid == null)
            {
                return;
            }

            GridColumnNavigatorHelper.PositionColumn(c1GridColumns, c1Grid, displayRowIndex);
        }

        private void btnShowColumns_Click(object sender, EventArgs e)
        {
            ToggleColumnNavigatorPanel();
        }

        private void ToggleColumnNavigatorPanel()
        {
            splitContainer4.Panel1Collapsed = !splitContainer4.Panel1Collapsed;
        }

        private void btnHelp_ColumnName_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString(
                "After clicking on the column name, you can quickly switch to the specified column.",
                "form",
                GetType().Name,
                "msg",
                "Help_ColumnName",
                "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}