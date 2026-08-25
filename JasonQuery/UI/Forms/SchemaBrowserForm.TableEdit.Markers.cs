using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Data.DataRows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void RefreshTableEditStateMarkers() //Refresh table edit markers and redraw grid
        {
            if (_dtTableData == null)
            {
                return;
            }

            SetDataGridOwnerDraw(false);

            try
            {
                _modifiedCells.Clear();
                _newRows.Clear();
                _deletedRows.Clear();

                c1GridData.ClearCellStyle(CellStyleFlag.AllCells);

                if (_dtTableData.Rows.Count == 0)
                {
                    return;
                }

                var visibleRowCount = GetVisibleDataGridRowCount();

                if (visibleRowCount == 0)
                {
                    return;
                }

                var originalRowMap = BuildOriginalRowMap();

                CollectVisibleRowEditStateMarkers(originalRowMap);
            }
            finally
            {
                SetDataGridOwnerDraw(true);
                c1GridData.Invalidate();
                c1GridData.Refresh();
            }
        }

        private void CollectVisibleRowEditStateMarkers(Dictionary<string, DataRow> originalRowMap)
        {
            var visibleRowCount = GetVisibleDataGridRowCount();

            for (var displayRowIndex = 0; displayRowIndex < visibleRowCount; displayRowIndex++)
            {
                if (!IsVisibleDataGridDataRow(displayRowIndex))
                {
                    continue;
                }

                var currentRow = GetDataRowFromGridDisplayRow(c1GridData, displayRowIndex);

                if (!IsUsableTableDataRow(currentRow))
                {
                    continue;
                }

                var operationMode = currentRow.GetSafeString(_identifyColumnName);

                if (_operationModeNewClone.Contains(operationMode))
                {
                    AddRowCellMarkers(_newRows, displayRowIndex);
                    continue;
                }

                if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    AddRowCellMarkers(_deletedRows, displayRowIndex);
                    continue;
                }

                if (!string.IsNullOrEmpty(operationMode))
                {
                    continue;
                }

                var rowId = currentRow.GetSafeString(0);

                if (string.IsNullOrEmpty(rowId))
                {
                    continue;
                }

                if (!originalRowMap.TryGetValue(rowId, out var originalRow))
                {
                    continue;
                }

                CollectModifiedCellsInVisibleRow(displayRowIndex, currentRow, originalRow);
            }
        }

        private void CollectModifiedCellsInVisibleRow(int displayRowIndex, DataRow currentRow, DataRow originalRow)
        {
            for (var col = 1; col < c1GridData.Columns.Count; col++) //第 0 欄是 RowId，不比對
            {
                var columnName = c1GridData.Columns[col].DataField;

                if (string.IsNullOrWhiteSpace(columnName))
                {
                    continue;
                }

                if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (currentRow.Table == null || !currentRow.Table.Columns.Contains(columnName))
                {
                    continue;
                }

                if (originalRow.Table == null || !originalRow.Table.Columns.Contains(columnName))
                {
                    continue;
                }

                if (GetDirectBinaryChange(currentRow, columnName) != null)
                {
                    continue;
                }

                var currentValue = currentRow[columnName];
                var originalValue = originalRow[columnName];

                if (IsCellValueChanged(columnName, originalValue, currentValue))
                {
                    _modifiedCells.Add(new Point(col, displayRowIndex));
                }
            }
        }

        private void SetDataGridOwnerDraw(bool ownerDraw)
        {
            foreach (C1DisplayColumn col in c1GridData.Splits[0].DisplayColumns)
            {
                col.OwnerDraw = ownerDraw;
            }
        }

        private void AddRowCellMarkers(List<Point> targetCells, int row)
        {
            for (var col = 0; col < c1GridData.Columns.Count; col++)
            {
                var columnName = c1GridData.Columns[col].DataField;

                if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                targetCells.Add(new Point(col, row));
            }
        }

        private Dictionary<string, DataRow> BuildOriginalRowMap()
        {
            var originalRowMap = new Dictionary<string, DataRow>(StringComparer.Ordinal);

            if (_dtOriginalTableData == null || _dtOriginalTableData.Rows.Count == 0)
            {
                return originalRowMap;
            }

            foreach (DataRow row in _dtOriginalTableData.Rows)
            {
                if (row == null || row.RowState == DataRowState.Detached)
                {
                    continue;
                }

                var rowId = row.GetSafeString(0);

                if (string.IsNullOrEmpty(rowId))
                {
                    continue;
                }

                if (!originalRowMap.ContainsKey(rowId))
                {
                    originalRowMap.Add(rowId, row);
                }
            }

            return originalRowMap;
        }

        private bool IsCellValueChanged(string columnName, object originalValue, object currentValue)
        {
            var isOriginalDBNull = originalValue == null || originalValue == DBNull.Value;
            var isCurrentDBNull = currentValue == null || currentValue == DBNull.Value;

            if (isOriginalDBNull != isCurrentDBNull)
            {
                return true;
            }

            if (isOriginalDBNull && isCurrentDBNull)
            {
                return false;
            }

            if (IsOracle && IsDateTimeColumn(columnName))
            {
                if (TryGetOracleDateTimeCompareText(originalValue, out var originalDateTimeText)
                    && TryGetOracleDateTimeCompareText(currentValue, out var currentDateTimeText))
                {
                    return !string.Equals(originalDateTimeText, currentDateTimeText, StringComparison.Ordinal);
                }
            }

            if (ReferenceEquals(originalValue, currentValue))
            {
                return false;
            }

            var originalText = Convert.ToString(originalValue);
            var currentText = Convert.ToString(currentValue);

            return !string.Equals(originalText, currentText, StringComparison.Ordinal);
        }

        private bool IsDateTimeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            if (_columnInfoCollector == null)
            {
                return false;
            }

            if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return false;
            }

            return columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.DateTime;
        }

        private bool TryGetOracleDateTimeCompareText(object value, out string compareText)
        {
            compareText = string.Empty;

            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            if (value is DateTime dateTimeValue)
            {
                compareText = GetOracleTimeStamp(dateTimeValue, "TIMESTAMP(9)");
                return true;
            }

            if (DateTime.TryParse(Convert.ToString(value), out var parsedDateTime))
            {
                compareText = GetOracleTimeStamp(parsedDateTime, "TIMESTAMP(9)");
                return true;
            }

            return false;
        }
    }
}
