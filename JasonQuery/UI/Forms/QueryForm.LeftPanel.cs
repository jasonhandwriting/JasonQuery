using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Text;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void c1DockingTab2_Enter(object sender, EventArgs e)
        {
            ApplyLeftPanelEnterState();
        }

        private void c1DockingTab2_Leave(object sender, EventArgs e)
        {
            ApplyLeftPanelLeaveState();
        }

        private void ApplyLeftPanelEnterState()
        {
            HideAutoCompleteGrid();

            tsAutoReplace.BackColor = _toolstripFocused;
            tsSchemaBrowser.BackColor = _toolstripFocused;
        }

        private void ApplyLeftPanelLeaveState()
        {
            tsAutoReplace.BackColor = _toolstripUnfocused;
            tsSchemaBrowser.BackColor = _toolstripUnfocused;
        }

        private void c1GridTabList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var displayRowIndex = c1GridTabList.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return;
            }

            var accessibleDescription = TextHelper.GetSafeString(c1GridTabList.Columns["AccessibleDescription"].CellValue(displayRowIndex));

            if (string.IsNullOrEmpty(accessibleDescription))
            {
                return;
            }

            if (string.Equals(accessibleDescription, AccessibleDescription, StringComparison.Ordinal))
            {
                return;
            }

            //20241208 透過主表單切換到指定的頁籤
            TransferValueToMainForm($"DoubleClickToSwitchTab`{accessibleDescription}");
        }

        private void c1GridSqlNavigator_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var displayRowIndex = c1GridSqlNavigator.RowContaining(e.Y);

            if (displayRowIndex == -1)
            {
                return;
            }

            if (!TryGetSqlNavigatorPosition(displayRowIndex, out var position))
            {
                return;
            }

            NavigateEditorToSqlNavigatorPosition(displayRowIndex, position);
        }

        private bool TryGetSqlNavigatorPosition(int displayRowIndex, out int position)
        {
            position = 0;

            var positionText = TextHelper.GetSafeString(c1GridSqlNavigator.Columns["Position"].CellValue(displayRowIndex));

            if (!int.TryParse(positionText, out position))
            {
                return false;
            }

            return position > 0;
        }

        private void NavigateEditorToSqlNavigatorPosition(int displayRowIndex, int position)
        {
            c1GridSqlNavigator.Row = displayRowIndex;
            c1GridSqlNavigator.Col = 0;
            c1GridSqlNavigator.ScrollGrid(0, 0);

            editor.CurrentPosition = position - 1;
            editor.SelectionStart = position - 1;
            editor.Focus();

            SelectCurrentBlock();
            editor.ScrollCaret();
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            ResizeLeftPanelGrids();
        }

        private void ResizeLeftPanelGrids()
        {
            ResizeTabListGrid();
            ResizeSqlNavigatorGrid();
        }

        private void ResizeTabListGrid()
        {
            if (c1GridTabList.IsDataTableSourceNullOrEmpty())
            {
                return;
            }

            c1GridTabList.Size = new Size(tabTabList.Width - 2, tabTabList.Height - 2);

            foreach (C1DisplayColumn column in c1GridTabList.Splits[0].DisplayColumns)
            {
                column.Width = c1GridTabList.Width - 4;
            }

            c1GridTabList.Refresh();
        }

        private void ResizeSqlNavigatorGrid()
        {
            if (c1GridSqlNavigator.IsDataTableSourceNullOrEmpty())
            {
                return;
            }

            AdjustSqlNavigator();

            c1GridSqlNavigator.Refresh();
        }

        private void AdjustSqlNavigator()
        {
            var columnIndex = 0;
            var typeColumnWidth = 0;

            c1GridSqlNavigator.Size = new Size(tabSqlNavigator.Width - 2, tabSqlNavigator.Height - 2);

            foreach (C1DisplayColumn column in c1GridSqlNavigator.Splits[0].DisplayColumns)
            {
                if (columnIndex == 0)
                {
                    column.AutoSize();
                    typeColumnWidth = column.Width;
                }
                else if (columnIndex == 1)
                {
                    column.Width = c1GridSqlNavigator.Width - typeColumnWidth - 22;
                }
                else
                {
                    column.AutoSize();
                }

                if (string.Equals(column.Name, "POSITION", StringComparison.OrdinalIgnoreCase))
                {
                    column.Visible = false;
                    column.Frozen = true;
                }

                columnIndex++;
            }
        }
    }
}