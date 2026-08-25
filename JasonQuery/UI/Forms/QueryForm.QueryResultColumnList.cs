using JasonLibrary.UI.Controls;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using System.Data;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ApplyQueryResultColumnList(DataTable dtData)
        {
            if (ShouldHideQueryResultColumnList(dtData))
            {
                HideQueryResultColumnList();
                return;
            }

            ShowQueryResultColumnList(dtData);
        }

        private bool ShouldHideQueryResultColumnList(DataTable dtData)
        {
            if (dtData == null)
            {
                return true;
            }

            //Column 數量太少，不需要顯示！
            return dtData.Rows.Count == 0 || dtData.Columns.Count < 5;
        }

        private void HideQueryResultColumnList()
        {
            btnShowColumns.Enabled = false;
            splitContainer4.Panel1Collapsed = true;
        }

        private void ShowQueryResultColumnList(DataTable dtData)
        {
            var dtColumns = BuildQueryResultColumnNameDataTable(dtData);

            btnShowColumns.Enabled = true;
            c1GridColumns.DataSource = dtColumns;

            ApplyQueryResultColumnListGridStyle();
            ApplyQueryResultColumnListCaption();
            ResizeColumnNavigator();
        }

        private DataTable BuildQueryResultColumnNameDataTable(DataTable dtData)
        {
            var dtColumns = new DataTable();

            dtColumns.Columns.Add("ColumnName");

            if (dtData == null)
            {
                return dtColumns;
            }

            foreach (DataColumn column in dtData.Columns)
            {
                var row = dtColumns.NewRow();

                row["ColumnName"] = column.ColumnName;
                dtColumns.Rows.Add(row);
            }

            return dtColumns;
        }

        private void ApplyQueryResultColumnListGridStyle()
        {
            GridHelper.ResizeGridColumnWidth(c1GridColumns);
            GridHelper.SetGridHeaderLine(c1GridColumns);
            GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridColumns, Name, true, "gridheader");
        }

        private void ApplyQueryResultColumnListCaption()
        {
            lblColumnNamePosition.Text = LocalizationHelper.GetLanguageString("Column Name", "form", GetType().Name, "gridheader", "ColumnName", "Text");
            lblColumnNamePosition.Font = c1GridColumns.HeadingStyle.Font;
            btnHelp_ColumnName.Location = new Point(lblColumnNamePosition.Left + lblColumnNamePosition.Width, btnHelp_ColumnName.Top);
        }
    }
}
