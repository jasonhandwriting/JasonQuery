using C1.Win.C1TrueDBGrid;
using System;
using System.Drawing;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteGridStyleHelper
    {
        public static void ApplyFetchCellStyle(C1TrueDBGrid grid, FetchCellStyleEventArgs e, bool highlightDataTypeColumn)
        {
            if (grid == null || e == null)
            {
                return;
            }

            var allowDBNull = grid.Columns["AllowDBNull"].CellText(e.Row);

            ApplyKeywordColumnStyle(e, allowDBNull);

            if (highlightDataTypeColumn && e.Col == 2)
            {
                e.CellStyle.ForeColor = Color.DarkGreen;
            }
        }

        private static void ApplyKeywordColumnStyle(FetchCellStyleEventArgs e, string allowDBNull)
        {
            if (e == null || e.Col != 0 || string.IsNullOrWhiteSpace(allowDBNull))
            {
                return;
            }

            var myFontBold = new Font(e.CellStyle.Font, FontStyle.Bold);

            if (allowDBNull.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor = Color.Purple;
            }
            else
            {
                e.CellStyle.ForeColor = Color.Blue;
            }

            e.CellStyle.Font = myFontBold;
        }
    }
}
