using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Data.DataRows;
using System;
using System.Data;

namespace JasonQuery.UI.Helpers
{
    public static class GridColumnNavigatorHelper
    {
        public static void PositionColumn(C1TrueDBGrid columnListGrid, C1TrueDBGrid targetGrid, int row)
        {
            if (columnListGrid == null || targetGrid == null)
            {
                return;
            }

            if (row < 0)
            {
                return;
            }

            var dtColumns = GetColumnListDataTable(columnListGrid, row);

            if (dtColumns == null || row >= dtColumns.Rows.Count)
            {
                return;
            }

            var columnName = dtColumns.Rows[row].GetSafeString(0);

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return;
            }

            var targetColumnIndex = FindColumnIndex(targetGrid, columnName);

            if (targetColumnIndex < 0)
            {
                return;
            }

            targetGrid.Col = targetColumnIndex;
            targetGrid.Select();
        }

        private static DataTable GetColumnListDataTable(C1TrueDBGrid columnListGrid, int row)
        {
            if (row < 0 || row >= columnListGrid.FocusedSplit.Rows.Count)
            {
                return columnListGrid.GetDataTableSourceOrNull();
            }

            if (columnListGrid.FocusedSplit.Rows[row].RowType != RowTypeEnum.DataRow)
            {
                return columnListGrid.GetDataTableSourceOrNull();
            }

            var rowBookmark = columnListGrid.RowBookmark(row);
            var rowView = columnListGrid[rowBookmark] as DataRowView;

            if (rowView == null)
            {
                return columnListGrid.GetDataTableSourceOrNull();
            }

            return rowView.DataView.ToTable();
        }

        private static int FindColumnIndex(C1TrueDBGrid targetGrid, string columnName)
        {
            for (var i = 0; i < targetGrid.Columns.Count; i++)
            {
                var dataField = targetGrid.Columns[i].DataField;

                if (string.Equals(dataField, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}