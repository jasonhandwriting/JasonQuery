using C1.Win.C1Command;
using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ApplyLoadedQueryResultToGrid(DataTable dtData, DataTable dtSchemaTable)
        {
            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                return;
            }

            if (_queryIndex == 1)
            {
                ArrangeSingleQueryResultGrid(dtData, dtSchemaTable);
            }
            else
            {
                ArrangeMultipleQueryResultGrid(dtData, dtSchemaTable);
            }

            ApplyPrimaryResultGridPostBindingState();
        }

        private void ArrangeSingleQueryResultGrid(DataTable dtData, DataTable dtSchemaTable)
        {
            ArrangeDataTable(c1TrueDBGrid1, dtData, dtSchemaTable); //單一查詢
        }

        private void ArrangeMultipleQueryResultGrid(DataTable dtData, DataTable dtSchemaTable)
        {
            var grid = FindQueryResultGridByQueryIndex(_queryIndex);

            if (grid == null)
            {
                return;
            }

            ArrangeDataTable(grid, dtData, dtSchemaTable); //多個查詢
            ApplyDynamicQueryResultGridSettings(grid);
        }

        private C1TrueDBGrid FindQueryResultGridByQueryIndex(int queryIndex)
        {
            foreach (Control tab in c1DockingTab1.TabPages)
            {
                if (!(tab is C1DockingTabPage tabPage))
                {
                    continue;
                }

                foreach (Control ctrlTab in tabPage.Controls)
                {
                    if (!(ctrlTab is C1TrueDBGrid grid))
                    {
                        continue;
                    }

                    if (string.Equals(grid.Name, $"c1TrueDBGrid{queryIndex}", StringComparison.Ordinal))
                    {
                        return grid;
                    }
                }
            }

            return null;
        }

        private void ApplyQueryTimeDisplay()
        {
            lblQueryTime.Tag = MyGlobal.DateDiff(_startTime, DateTime.Now);
            lblQueryTime.Text = $"{_queryTime} {lblQueryTime.Tag}";

            tmrQueryTime.Enabled = false;
        }

        private void ApplyDynamicQueryResultGridSettings(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            c1ThemeController1.SetTheme(grid, "(default)");

            //套用 Grid 外觀
            ApplyQueryResultGridVisualStyle(grid);
            GridHelper.SetGridVisualStyle(grid, MyLibrary.GridFontSize);

            grid.AllowFilter = false;
            grid.FilterBar = chkShowFilterRow.Checked;
            grid.Filter += C1TrueDBGrid_Filter;
            grid.MouseWheel += c1TrueDBGrid1_MouseWheel;
            grid.KeyDown += Detect_KeyDown;
            grid.OwnerDrawCell += c1TrueDBGrid1_OwnerDrawCell;
            grid.DataView = chkShowGroupingRow.Checked ? DataViewEnum.GroupBy : DataViewEnum.Normal;
            grid.GroupStyle.BackColor = Color.LightYellow;
            grid.GroupStyle.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);
        }

        private void ApplyPrimaryResultGridPostBindingState()
        {
            ApplyPrimaryResultGridNullDisplayStyleIfNeeded();
            ApplyResultGridHeadingStyle(c1TrueDBGrid1);
        }

        private void ApplyPrimaryResultGridNullDisplayStyleIfNeeded()
        {
            if (!(c1TrueDBGrid1.DataSource is DataTable dataTable) || dataTable.Rows.Count == 0)
            {
                return;
            }

            if (!string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                var colorNull = new Style
                {
                    ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
                };

                for (var i = 0; i < c1TrueDBGrid1.Columns.Count; i++)
                {
                    //套用「使用者指定的 NULL」顯示格式
                    c1TrueDBGrid1.Splits[0].DisplayColumns[i].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
                }
            }

            cboFindGrid.Enabled = true;
        }
    }
}