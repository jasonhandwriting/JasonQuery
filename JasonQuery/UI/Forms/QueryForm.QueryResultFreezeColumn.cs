using C1.Win.C1TrueDBGrid;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void FrozenColumn(bool isFrozen = true)
        {
            try
            {
                var c1Grid = GetWhichGrid();

                if (!CanApplyFrozenColumn(c1Grid))
                {
                    ClearFrozenColumnState();
                    return;
                }

                ClearFrozenColumns(c1Grid);

                if (!isFrozen)
                {
                    ClearFrozenColumnState();
                    return;
                }

                var displayColumn = c1Grid.Splits[_splitsIndex].DisplayColumns[c1Grid.Col];

                displayColumn.Frozen = true;

                _unfreezeColumnName = ResolveFrozenColumnDisplayName(displayColumn);

                ApplyUnfreezeColumnMenuState(true);
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private bool CanApplyFrozenColumn(C1TrueDBGrid c1Grid)
        {
            if (c1Grid == null)
            {
                return false;
            }

            if (c1Grid.Splits.Count <= _splitsIndex)
            {
                return false;
            }

            var displayColumns = c1Grid.Splits[_splitsIndex].DisplayColumns;

            if (displayColumns.Count <= 0)
            {
                return false;
            }

            if (c1Grid.Col < 0)
            {
                return false;
            }

            return c1Grid.Col < displayColumns.Count;
        }

        private void ClearFrozenColumns(C1TrueDBGrid c1Grid)
        {
            var displayColumns = c1Grid.Splits[_splitsIndex].DisplayColumns;

            for (var i = 0; i < displayColumns.Count; i++)
            {
                displayColumns[i].Frozen = false;
            }
        }

        private void ClearFrozenColumnState()
        {
            _unfreezeColumnName = string.Empty;
            ApplyUnfreezeColumnMenuState(false);
        }

        private void ApplyUnfreezeColumnMenuState(bool enabled)
        {
            if (_gridContextMenu == null)
            {
                return;
            }

            if (_gridContextMenu.Items.Count <= GridColumn.UnfreezeColumn)
            {
                return;
            }

            _gridContextMenu.Items[GridColumn.UnfreezeColumn].Enabled = enabled;
        }

        private static string ResolveFrozenColumnDisplayName(C1DisplayColumn displayColumn)
        {
            if (displayColumn == null)
            {
                return string.Empty;
            }

            var dataField = displayColumn.DataColumn?.DataField ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(dataField))
            {
                return dataField;
            }

            return GetFirstCaptionLine(displayColumn.ToString());
        }

        private static string GetFirstCaptionLine(string caption)
        {
            if (string.IsNullOrEmpty(caption))
            {
                return string.Empty;
            }

            var index = caption.IndexOf("\r\n", StringComparison.Ordinal);

            if (index >= 0)
            {
                return caption.Substring(0, index);
            }

            index = caption.IndexOf('\n');

            if (index >= 0)
            {
                return caption.Substring(0, index);
            }

            return caption;
        }
    }
}