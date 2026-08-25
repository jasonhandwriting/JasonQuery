using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.UI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace JasonQuery.UI.Helpers
{
    public static class CellViewerGridHelper
    {
        public static List<int> GetVisibleDataRowIndexes(C1TrueDBGrid c1Grid)
        {
            var rowIndexes = new List<int>();

            if (c1Grid == null)
            {
                return rowIndexes;
            }

            var split = c1Grid.FocusedSplit ?? (c1Grid.Splits.Count > 0 ? c1Grid.Splits[0] : null);

            if (split == null)
            {
                return rowIndexes;
            }

            for (var i = 0; i < split.Rows.Count; i++)
            {
                if (split.Rows[i].RowType == RowTypeEnum.DataRow)
                {
                    rowIndexes.Add(i);
                }
            }

            return rowIndexes;
        }

        public static bool IsDataRow(C1TrueDBGrid c1Grid, int rowIndex)
        {
            if (c1Grid == null || rowIndex < 0)
            {
                return false;
            }

            var split = c1Grid.FocusedSplit ?? (c1Grid.Splits.Count > 0 ? c1Grid.Splits[0] : null);

            if (split == null || rowIndex >= split.Rows.Count)
            {
                return false;
            }

            return split.Rows[rowIndex].RowType == RowTypeEnum.DataRow;
        }

        public static string ConvertCellValueToText(object rawValue)
        {
            if (rawValue == null || rawValue == DBNull.Value)
            {
                return string.Empty;
            }

            return rawValue.ToString();
        }

        public static string BuildBinarySummaryText(string columnType, int? binaryLength)
        {
            var typeText = string.IsNullOrWhiteSpace(columnType) ? "Binary" : columnType;
            var sizeText = binaryLength.HasValue ? $"{binaryLength.Value:N0} bytes" : "Not loaded yet";

            return $"This is a binary column.\r\n\r\nType: {typeText}\r\nSize: {sizeText}\r\n\r\nClick Hex Viewer to load and view the binary content.";
        }

        public static int? TryExtractBinaryLengthFromDisplayText(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return null;
            }

            var right = displayText.LastIndexOf(')');
            var left = right > 0 ? displayText.LastIndexOf('(', right - 1) : -1;

            if (left < 0 || right <= left)
            {
                return null;
            }

            var valueText = displayText.Substring(left + 1, right - left - 1).Replace(",", string.Empty).Trim();

            if (int.TryParse(valueText, out var length))
            {
                return length;
            }

            return null;
        }

        public static void ResolveDisplayColumnInfo(string fullColumnName, out string columnName, out string columnType)
        {
            string[] splitters = { "\r\n", "\r", "\n" };
            var parts = (fullColumnName ?? string.Empty).Split(splitters, 2, StringSplitOptions.None);

            columnName = parts.Length > 0 ? parts[0] : string.Empty;
            columnType = parts.Length > 1 ? parts[1] : string.Empty;
        }

        public static string TryExtractColumnTypeFromBinaryDisplayText(string displayText)
        {
            if (string.IsNullOrWhiteSpace(displayText))
            {
                return string.Empty;
            }

            var typeEndIndex = displayText.IndexOf(")(", StringComparison.Ordinal);
            var firstLeftParenthesisIndex = displayText.IndexOf('(');

            if (typeEndIndex >= 0)
            {
                return displayText.Substring(0, typeEndIndex).Replace("(", string.Empty).Trim();
            }

            if (firstLeftParenthesisIndex >= 0)
            {
                return displayText.Substring(0, firstLeftParenthesisIndex).Replace("(", string.Empty).Trim();
            }

            return string.Empty;
        }

        public static void ApplyCellViewerFormSize(CellViewerForm form)
        {
            var (formWidth, formHeight) = UIHelper.GetFormWidthHeightSettings(MyGlobal.DomainUser, "CellViewerFormWidth", "CellViewerFormHeight", defaultWidth: form.ClientSize.Width, defaultHeight: form.ClientSize.Height);

            form.ClientSize = new Size(formWidth - 16, formHeight - 38);
        }
    }
}
