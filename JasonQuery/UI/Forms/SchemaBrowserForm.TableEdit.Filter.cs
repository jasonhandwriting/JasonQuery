using C1.Win.C1TrueDBGrid;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private int GetVisibleDataGridRowCount()
        {
            try
            {
                if (c1GridData == null || c1GridData.FocusedSplit == null || c1GridData.FocusedSplit.Rows == null)
                {
                    return 0;
                }

                return c1GridData.FocusedSplit.Rows.Count;
            }
            catch
            {
                return 0;
            }
        }

        private bool HasVisibleDataGridRows()
        {
            return GetVisibleDataGridRowCount() > 0;
        }

        private bool IsVisibleDataGridDataRow(int displayRowIndex)
        {
            if (displayRowIndex < 0)
            {
                return false;
            }

            try
            {
                if (c1GridData.FocusedSplit == null || c1GridData.FocusedSplit.Rows == null)
                {
                    return false;
                }

                if (displayRowIndex >= c1GridData.FocusedSplit.Rows.Count)
                {
                    return false;
                }

                return c1GridData.FocusedSplit.Rows[displayRowIndex].RowType == RowTypeEnum.DataRow;
            }
            catch
            {
                return false;
            }
        }

        private bool IsMouseOnDataGridFilterBar(int row, int col, int mouseY)
        {
            if (!c1GridData.FilterBar || col < 0)
            {
                return false;
            }

            try
            {
                var captionHeight = c1GridData.Splits[0].ColumnCaptionHeight;
                var filterBarBottom = captionHeight + c1GridData.RowHeight + 6;

                return mouseY >= captionHeight && mouseY <= filterBarBottom;
            }
            catch
            {
                return false;
            }
        }
    }
}