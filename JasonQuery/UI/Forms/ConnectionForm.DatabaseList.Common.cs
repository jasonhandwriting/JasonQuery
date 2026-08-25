using C1.Win.C1Input;
using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.UI.Helpers;
using System.Data;
using System.Drawing;
using System.Linq;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void UpdateComboBox(DataTable dtData, C1ComboBox cbo)
        {
            cbo.Items.Clear();

            foreach (DataRow dr in dtData?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var name = dr.GetSafeString("Name");

                cbo.Items.Add(name);
            }
        }

        //調整下拉清單的大小
        private void ResizeAutoCompleteGrid(C1TrueDBGrid c1Grid, int rowCount, int width)
        {
            var height = 0;

            if (rowCount <= 6)
            {
                width = GridHelper.ResizeGridColumnWidth(c1Grid) + 4;
            }
            else
            {
                width = GridHelper.ResizeGridColumnWidth(c1Grid) + c1Grid.VScrollBar.Width + 5;
            }

            height = 124;

            c1Grid.Size = new Size(width, height);
        }
    }
}
