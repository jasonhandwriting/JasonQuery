using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.Core.Schema;
using JasonQuery.UI.Models;

namespace JasonQuery.UI.Helpers
{
    public static class CellViewerContextBuilder
    {
        public static bool TryBuildFromColumnInfoCollector(C1TrueDBGrid c1Grid, int gridRowIndex, int gridColIndex, ColumnInfoCollector columnInfoCollector, out CellViewerCellContext context)
        {
            context = null;

            if (c1Grid == null || gridRowIndex < 0 || gridColIndex < 0)
            {
                return false;
            }

            if (columnInfoCollector == null)
            {
                return false;
            }

            var columnName = c1Grid.Columns[gridColIndex].DataField;

            if (string.IsNullOrWhiteSpace(columnName))
            {
                return false;
            }

            if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return false;
            }

            object rawValue = c1Grid[gridRowIndex, gridColIndex];
            var displayText = CellViewerGridHelper.ConvertCellValueToText(rawValue);
            var isLargeText = columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeText;
            var isLargeBinary = columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.LargeBinary;

            context = new CellViewerCellContext
            {
                ColumnName = columnName,
                ColumnType = columnInfo.BaseDataType,
                DisplayText = displayText,
                CategoryDataTypeKindValue = columnInfo.CategoryDataTypeKind,
                IsBinaryColumn = isLargeBinary
            };

            if (isLargeText && rawValue is LargeTextDataType text)
            {
                if (text.IsNull)
                {
                    context.DisplayText = text.DisplayText;
                }
                else
                {
                    var loadResult = LargeValueContentLoader.LoadText(text.LoadContent);

                    context.DisplayText = loadResult.Succeeded
                                          ? CellViewerGridHelper.ConvertCellValueToText(loadResult.Content)
                                          : loadResult.ErrorMessage;
                }

                return true;
            }

            if (isLargeBinary && rawValue is LargeBinaryDataType binary)
            {
                context.HasBinaryContent = !binary.IsNull;

                if (!binary.IsNull)
                {
                    context.BinaryLength = CellViewerGridHelper.TryExtractBinaryLengthFromDisplayText(displayText);
                    context.BinaryContentLoader = () => binary.LoadContent();
                    context.DisplayText = CellViewerGridHelper.BuildBinarySummaryText(columnInfo.BaseDataType, context.BinaryLength);
                }
                else
                {
                    context.DisplayText = string.Empty;
                }

                return true;
            }

            if (isLargeBinary && rawValue is byte[] bytes)
            {
                context.HasBinaryContent = bytes.Length > 0;
                context.BinaryLength = bytes.Length;
                context.BinaryContentLoader = () => bytes;
                context.DisplayText = CellViewerGridHelper.BuildBinarySummaryText(columnInfo.BaseDataType, context.BinaryLength);

                return true;
            }

            return true;
        }
    }
}
