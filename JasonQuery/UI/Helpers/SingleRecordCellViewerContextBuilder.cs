using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.UI.Models;
using System;

namespace JasonQuery.UI.Helpers
{
    public static class SingleRecordCellViewerContextBuilder
    {
        public static bool TryBuild(C1TrueDBGrid c1Grid, int gridRowIndex, int gridColIndex, out CellViewerCellContext context)
        {
            context = null;

            if (c1Grid == null || gridRowIndex < 0 || gridColIndex < 0)
            {
                return false;
            }

            var fullColumnName = c1Grid.Splits[0].DisplayColumns[gridColIndex].ToString();

            CellViewerGridHelper.ResolveDisplayColumnInfo(fullColumnName, out var displayColumnName, out var columnType);

            var sourceColumnName = CellViewerGridHelper.ConvertCellValueToText(c1Grid[gridRowIndex, 0]);
            var columnName = string.IsNullOrWhiteSpace(sourceColumnName) ? displayColumnName : sourceColumnName;

            object rawValue = c1Grid[gridRowIndex, gridColIndex];
            var displayText = CellViewerGridHelper.ConvertCellValueToText(rawValue);

            var categoryDataTypeKind = CategoryDataTypeKind.String;
            var isBinaryColumn = false;
            var hasBinaryContent = false;
            Func<byte[]> binaryContentLoader = null;
            int? binaryLength = null;

            if (rawValue is LargeTextDataType text)
            {
                categoryDataTypeKind = CategoryDataTypeKind.LargeText;

                if (text.IsNull)
                {
                    displayText = text.DisplayText;
                }
                else
                {
                    var loadResult = LargeValueContentLoader.LoadText(text.LoadContent);

                    displayText = loadResult.Succeeded
                                  ? CellViewerGridHelper.ConvertCellValueToText(loadResult.Content)
                                  : loadResult.ErrorMessage;
                }
            }
            else if (rawValue is LargeBinaryDataType binary)
            {
                categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                isBinaryColumn = true;
                hasBinaryContent = !binary.IsNull;

                if (!binary.IsNull)
                {
                    binaryLength = CellViewerGridHelper.TryExtractBinaryLengthFromDisplayText(displayText);
                    binaryContentLoader = () => binary.LoadContent();
                    displayText = CellViewerGridHelper.BuildBinarySummaryText(columnType, binaryLength);
                }
                else
                {
                    displayText = string.Empty;
                }
            }
            else
            {
                TryApplySingleRecordBinaryContext(c1Grid, gridRowIndex, gridColIndex, displayText, ref columnType, ref categoryDataTypeKind,
                                                  ref isBinaryColumn, ref hasBinaryContent, ref binaryContentLoader, ref binaryLength, ref displayText);
            }

            context = new CellViewerCellContext
            {
                ColumnName = columnName,
                ColumnType = columnType,
                DisplayText = displayText,
                CategoryDataTypeKindValue = categoryDataTypeKind,
                IsBinaryColumn = isBinaryColumn,
                HasBinaryContent = hasBinaryContent,
                BinaryContentLoader = binaryContentLoader,
                BinaryLength = binaryLength
            };

            return true;
        }

        private static void TryApplySingleRecordBinaryContext(C1TrueDBGrid c1Grid, int gridRowIndex, int gridColIndex, string originalDisplayText,
                                                              ref string columnType, ref CategoryDataTypeKind categoryDataTypeKind, ref bool isBinaryColumn,
                                                              ref bool hasBinaryContent, ref Func<byte[]> binaryContentLoader, ref int? binaryLength, ref string displayText)
        {
            var dtData = c1Grid.GetDataTableSourceOrNull();

            if (dtData == null || dtData.Columns.Count <= 2)
            {
                return;
            }

            var sourceColumnName = CellViewerGridHelper.ConvertCellValueToText(c1Grid[gridRowIndex, 0]);
            var dtRow = DataTableSearchHelper.FindDataTableFirstRow(dtData, ("ColumnName", sourceColumnName));

            if (dtRow == null || dtRow.IsNull(2) || !(dtRow[2] is byte[] bytes))
            {
                return;
            }

            isBinaryColumn = true;
            categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;

            //SingleRecordViewer 的第 1 欄是欄位名稱，第 2 欄才是 Value 欄位
            if (gridColIndex != 1 || bytes.Length == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(columnType))
            {
                columnType = CellViewerGridHelper.TryExtractColumnTypeFromBinaryDisplayText(originalDisplayText);
            }

            binaryLength = bytes.Length;
            binaryContentLoader = () => bytes;
            hasBinaryContent = true;
            displayText = CellViewerGridHelper.BuildBinarySummaryText(columnType, binaryLength);
        }
    }
}