using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Schema;
using JasonQuery.Core.Text;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle;

namespace JasonQuery.UI.Helpers
{
    public static class GridHelper
    {
        public static string Direction = string.Empty;
        public static int MaxWidth = 500;

        public static void ChangeGridVisualStyle(C1TrueDBGrid c1Grid, string styleText)
        {
            switch (styleText)
            {
                case "Office 2007 Blue":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Blue;
                        break;
                    }
                case "Office 2007 Silver":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Silver;
                        break;
                    }
                case "Office 2007 Black":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Black;
                        break;
                    }
                case "Office 2010 Blue":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Blue;
                        break;
                    }
                case "Office 2010 Silver":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Silver;
                        break;
                    }
                case "Office 2010 Black":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Black;
                        break;
                    }
                default:
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Blue;
                        break;
                    }
            }
        }

        public static void SetGridVisualStyle(C1TrueDBGrid c1Grid, float fontSize)
        {
            if (fontSize == 0)
            {
                fontSize = 12;
            }

            var gridFontName = MyLibrary.GridFontName;

            SetGridInteractionDirection(c1Grid, Direction);

            #region Color
            if (string.IsNullOrEmpty(MyLibrary.GridOddRowForeColor))
            {
                MyLibrary.GridOddRowForeColor = MyLibrary.IsDarkMode ? "#FFFFFF" : string.Empty;
            }

            if (string.IsNullOrEmpty(MyLibrary.GridOddRowBackColor))
            {
                MyLibrary.GridOddRowBackColor = MyLibrary.IsDarkMode ? "#262626" : string.Empty;
            }

            if (string.IsNullOrEmpty(MyLibrary.GridEvenRowForeColor))
            {
                MyLibrary.GridEvenRowForeColor = MyLibrary.IsDarkMode ? "#FFFFFF" : string.Empty;
            }

            if (string.IsNullOrEmpty(MyLibrary.GridEvenRowBackColor))
            {
                MyLibrary.GridEvenRowBackColor = MyLibrary.IsDarkMode ? "#0F243E" : string.Empty;
            }

            if (string.IsNullOrEmpty(MyLibrary.GridSelectedForeColor))
            {
                MyLibrary.GridSelectedForeColor = string.Empty;
            }

            if (string.IsNullOrEmpty(MyLibrary.GridSelectedBackColor))
            {
                MyLibrary.GridSelectedBackColor = string.Empty;
            }
            #endregion

            //字型 + 字體大小
            c1Grid.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.HeadingStyle.Font = new Font(gridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.CaptionStyle.Font = new Font(gridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1Grid.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1Grid.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);

            if (MyLibrary.IsDarkMode)
            {
                #region Get Default Value
                if (string.IsNullOrEmpty(MyLibrary.GridHeadingForeColor))
                {
                    MyLibrary.GridHeadingForeColor = "#FFFFFF";
                }

                if (string.IsNullOrEmpty(MyLibrary.GridFontName))
                {
                    MyLibrary.GridFontName = string.Empty;
                }
                #endregion

                c1Grid.BorderColor = Color.White;
                c1Grid.HeadingStyle.Borders.Color = Color.White;
                c1Grid.HeadingStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHeadingForeColor);
                c1Grid.RowDivider.Color = Color.White;
                c1Grid.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
                c1Grid.HeadingStyle.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
                c1Grid.MarqueeStyle = MarqueeEnum.HighlightCell;
            }

            //20240412 修正標題列每一欄中間格線不顯示問題 (葡萄城給的建議)
            var displayColumns = c1Grid.Splits[0].DisplayColumns;

            foreach (C1DisplayColumn col in displayColumns)
            {
                col.HeadingStyle.Borders.Right = 2;
                col.HeadingStyle.Borders.BorderType = BorderTypeEnum.Flat;
            }
        }

        public static void SetGridInteractionDirection(C1TrueDBGrid c1Grid, string gridDirection)
        {
            switch (gridDirection)
            {
                case "Down":
                    {
                        c1Grid.DirectionAfterEnter = DirectionAfterEnterEnum.MoveDown;
                        break;
                    }
                case "Right":
                    {
                        c1Grid.DirectionAfterEnter = DirectionAfterEnterEnum.MoveRight;
                        break;
                    }
                case "Up":
                    {
                        c1Grid.DirectionAfterEnter = DirectionAfterEnterEnum.MoveUp;
                        break;
                    }
                case "Left":
                    {
                        c1Grid.DirectionAfterEnter = DirectionAfterEnterEnum.MoveLeft;
                        break;
                    }
            }
        }

        public static void SetGridVisualStyle(C1TrueDBGrid c1Grid)
        {
            var style = MyLibrary.IsDarkMode ? "Office 2010 Black" : MyLibrary.GridVisualStyle;

            switch (style)
            {
                case "Office 2007 Blue":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Blue;
                        break;
                    }
                case "Office 2007 Silver":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Silver;
                        break;
                    }
                case "Office 2007 Black":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2007Black;
                        break;
                    }
                case "Office 2010 Blue":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Blue;
                        break;
                    }
                case "Office 2010 Silver":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Silver;
                        break;
                    }
                case "Office 2010 Black":
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Black;
                        break;
                    }
                default:
                    {
                        c1Grid.VisualStyle = VisualStyle.Office2010Blue;
                        break;
                    }
            }
        }

        public static void SetGridHeaderLine(C1TrueDBGrid c1Grid)
        {
            //20240414 有的表格尚未綁定資料，故此處要再處理標題列的格線
            var displayColumns = c1Grid.Splits[0].DisplayColumns;
            var count = c1Grid.Columns.Count;

            for (var i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    displayColumns[i].HeadingStyle.Borders.Left = 2;
                }

                displayColumns[i].HeadingStyle.Borders.Right = 2;
                displayColumns[i].HeadingStyle.Borders.BorderType = BorderTypeEnum.Flat;
            }
        }

        public static void SetGridFontAndBackColor(C1TrueDBGrid c1Grid, string gridFontName, float fontSize)
        {
            if (fontSize == 0)
            {
                fontSize = 12;
            }

            //字型 + 字體大小
            c1Grid.Font = new Font(gridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.HeadingStyle.Font = new Font(gridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1Grid.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1Grid.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        public static void ReplaceColumnCaptionByLanguageInfo(C1TrueDBGrid c1Grid, string formName, bool isTag = false, string gridHeader = "gridheader")
        {
            foreach (C1DataColumn column in c1Grid.Columns)
            {
                var tag = TextHelper.GetSafeString(column.Tag);
                var isTagNull = string.IsNullOrEmpty(tag);

                //根據語系置換表頭欄位名稱
                //20260131 Use DataField instead of Caption to obtain the language content value
                if (isTag)
                {
                    tag = string.IsNullOrEmpty(tag) ? column.Caption : tag;
                    column.Tag = isTagNull ? string.Empty : column.Caption;
                    column.Caption = LocalizationHelper.GetLanguageString(column.DataField, "form", formName, gridHeader, tag, "Text");
                }
                else
                {
                    column.Caption = LocalizationHelper.GetLanguageString(column.DataField, "form", formName, gridHeader, column.Caption, "Text");
                }
            }
        }

        public static DataTable ReplaceColumnNameByLanguageInfo(DataTable dt, string formName)
        {
            if (dt == null || dt.Columns.Count == 0)
            {
                return dt;
            }

            foreach (DataColumn column in dt.Columns)
            {
                var caption = column.Caption;

                column.ColumnName = LocalizationHelper.GetLanguageString(caption, "form", formName, "gridheader", caption, "Text");
            }

            return dt;
        }

        public static int ResizeGridColumnWidth(C1TrueDBGrid c1Grid, string gridName = "")
        {
            var totalWidth = 0;
            int maxWidthLimit = MaxWidth;

            foreach (C1DisplayColumn col in c1Grid.Splits[0].DisplayColumns)
            {
                var caption = col.Name;

                try
                {
                    col.AutoSize();
                }
                catch (Exception)
                {
                    col.Width = 2000;
                }

                //20250615 針對 Primary Key 設定寬度
                if (caption == " " && !string.IsNullOrEmpty(gridName) && gridName == "c1GridStructure")
                {
                    col.Width = 17;
                    col.AllowSizing = false;
                }

                col.Width = Math.Min(col.Width, maxWidthLimit);

                //20251003 針對不允許 NULL 的欄位特別處理
                if (string.Equals(caption, "AllowDBNull", StringComparison.OrdinalIgnoreCase))
                {
                    col.Width = 0;
                    col.Visible = false;
                    col.Frozen = true;
                }

                totalWidth += col.Width;
            }

            return totalWidth;
        }

        public static int CountGridOccurrence(C1TrueDBGrid c1Grid, string findText, int indexValue)
        {
            int count = 0;
            var sFindTextUpper = findText.ToUpper();
            var iRowCount = c1Grid.Splits[indexValue].Rows.Count;

            for (var row = 0; row < iRowCount; row++)
            {
                var vr = c1Grid.Splits[indexValue].Rows[row];

                count += c1Grid.Columns.Cast<C1DataColumn>()
                                       .Count(col1 =>
                                        {
                                            var cellTextUpper = col1.CellText(vr.DataRowIndex).ToUpper();

                                            return cellTextUpper.Length != cellTextUpper.Replace(sFindTextUpper, string.Empty).Length;
                                        });
            }

            return count;
        }

        public static void UpdateSchemaData(C1TrueDBGrid c1Grid, bool isRename = false, bool isFromSchemaBrowser = false)
        {
            var i = 0;
            var columns = DatabaseSqlExecutor.dtSchema.Columns;

            switch (isRename)
            {
                case true when MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser):
                    {
                        if (columns.Contains("ColumnInfo"))
                        {
                            DatabaseSqlExecutor.dtSchema.Columns["ColumnInfo"].ColumnName = "Schema_Browser";
                        }

                        break;
                    }
                case true:
                    {
                        if (columns.Contains("SchemaName"))
                        {
                            DatabaseSqlExecutor.dtSchema.Columns["SchemaName"].ColumnName = "Schema_Browser";
                        }

                        break;
                    }
            }

            var _dtSchemaTable = DatabaseSqlExecutor.dtSchema.Copy();

            c1Grid.DataSource = _dtSchemaTable;

            switch (DatabaseSqlExecutor.CurrentDataSource)
            {
                case DataSourceType.Oracle:
                    {
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaObject"]);
                        c1Grid.GroupedColumns[0].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaType"]);
                        c1Grid.GroupedColumns[1].GroupInfo.HeaderText = "{0}";

                        if (MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser))
                        {
                            c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaName"]);
                            c1Grid.GroupedColumns[2].GroupInfo.HeaderText = "{0}";
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaObject"]);
                        c1Grid.GroupedColumns[0].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaNode"]);
                        c1Grid.GroupedColumns[1].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaType"]);
                        c1Grid.GroupedColumns[2].GroupInfo.HeaderText = "{0}";

                        if (MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser))
                        {
                            c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaName"]);
                            c1Grid.GroupedColumns[3].GroupInfo.HeaderText = "{0}";
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaObject"]);
                        c1Grid.GroupedColumns[0].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaNode"]);
                        c1Grid.GroupedColumns[1].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaType"]);
                        c1Grid.GroupedColumns[2].GroupInfo.HeaderText = "{0}";

                        if (MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser))
                        {
                            c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaName"]);
                            c1Grid.GroupedColumns[3].GroupInfo.HeaderText = "{0}";
                        }

                        i = 1;
                        c1Grid.Splits[0].DisplayColumns["SchemaDbo"].Visible = false;
                        c1Grid.Splits[0].DisplayColumns["ObjectID"].Visible = false;
                        c1Grid.Splits[0].DisplayColumns["CreateDate"].Visible = false;
                        c1Grid.Splits[0].DisplayColumns["ModifyDate"].Visible = false;

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaObject"]);
                        c1Grid.GroupedColumns[0].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaNode"]);
                        c1Grid.GroupedColumns[1].GroupInfo.HeaderText = "{0}";
                        c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaType"]);
                        c1Grid.GroupedColumns[2].GroupInfo.HeaderText = "{0}";

                        if (MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser))
                        {
                            c1Grid.GroupedColumns.Add(c1Grid.Columns["SchemaName"]);
                            c1Grid.GroupedColumns[3].GroupInfo.HeaderText = "{0}";
                        }

                        i = 1;
                        c1Grid.Splits[0].DisplayColumns["CreateDate"].Visible = false;
                        c1Grid.Splits[0].DisplayColumns["ModifyDate"].Visible = false;

                        break;
                    }
            }

            //展開指定的節點 (-1:標題列，所以，從 0 開始算，要展開哪一個)
            for (var j = 0; j <= i; j++)
            {
                c1Grid.ExpandGroupRow(j);
            }
        }

        public static void ApplyGridHeadingStyle(ColumnInfoCollector columnInfoCollector, C1TrueDBGrid c1Grid, bool isSetTop = false, GridHeadingCellTipHelper.GridHeadingCellTipOptions cellTipOptions = null)
        {
            if (columnInfoCollector == null) //Raw Data Mode
            {
                GridHeadingCellTipHelper.Clear(c1Grid);
                return;
            }

            if (cellTipOptions != null)
            {
                GridHeadingCellTipHelper.Apply(columnInfoCollector, c1Grid, cellTipOptions);
            }

            var headingFont = new Font(MyLibrary.GridFontName, c1Grid.Styles["Heading"].Font.Size, FontStyle.Bold, GraphicsUnit.Point);

            foreach (C1DataColumn column in c1Grid.Columns)
            {
                var columnName = column.DataField;

                if (!columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    continue;
                }

                var col = c1Grid.Splits[0].DisplayColumns[columnName];
                var categoryDataTypeKind = columnInfo.CategoryDataTypeKind;

                //設定 Cell 對齊方式 (Refer to the display format of Excel)
                switch (categoryDataTypeKind)
                {
                    case CategoryDataTypeKind.Number:
                    case CategoryDataTypeKind.DateTime:
                        {
                            col.Style.HorizontalAlignment = AlignHorzEnum.Far;
                            break;
                        }
                    default:
                        {
                            col.Style.HorizontalAlignment = AlignHorzEnum.Near;
                            break;
                        }
                }

                //Color (PrimaryKey/Nullable)
                if (columnInfo.IsPrimaryKey)
                {
                    col.HeadingStyle.ForeColor = Color.Purple;
                    col.HeadingStyle.Font = headingFont;
                }
                else if (!columnInfo.IsNullable)
                {
                    col.HeadingStyle.ForeColor = Color.Blue;
                    col.HeadingStyle.Font = headingFont;
                }

                //20251118 調整為靠上顯示
                if (isSetTop)
                {
                    col.HeadingStyle.VerticalAlignment = AlignVertEnum.Top;
                }
            }
        }

        /// <summary>
        /// 安全取得 C1TrueDBGrid 的 DataTable DataSource。
        /// Grid 為 null、DataSource 為 null、DataSource 不是 DataTable 時，回傳 null。
        /// </summary>
        public static DataTable GetDataTableSourceOrNull(this C1TrueDBGrid grid)
        {
            return grid?.DataSource as DataTable;
        }

        /// <summary>
        /// 安全取得 C1TrueDBGrid 的 DataTable DataSource。
        /// </summary>
        public static bool TryGetDataTableSource(this C1TrueDBGrid grid, out DataTable dataTable)
        {
            dataTable = grid?.DataSource as DataTable;
            return dataTable != null;
        }

        /// <summary>
        /// 判斷 C1TrueDBGrid 是否無法取得 DataTable DataSource。
        /// </summary>
        public static bool IsDataTableSourceNull(this C1TrueDBGrid grid)
        {
            return grid.GetDataTableSourceOrNull() == null;
        }

        /// <summary>
        /// 判斷 C1TrueDBGrid 的 DataTable DataSource 是否有資料列。
        /// </summary>
        public static bool HasDataTableRows(this C1TrueDBGrid grid)
        {
            var dt = grid.GetDataTableSourceOrNull();

            return dt != null && dt.Rows.Count > 0;
        }

        /// <summary>
        /// 安全複製 C1TrueDBGrid 的 DataTable DataSource。
        /// Grid 為 null、DataSource 為 null、DataSource 不是 DataTable 時，回傳空的 DataTable。
        /// </summary>
        public static DataTable CopyDataTableSourceOrEmpty(this C1TrueDBGrid grid)
        {
            var dt = grid.GetDataTableSourceOrNull();

            return dt?.Copy() ?? new DataTable();
        }

        /// <summary>
        /// 安全複製 C1TrueDBGrid 的 DataTable DataSource。
        /// 成功取得並複製 DataTable 時回傳 true，否則回傳 false 並輸出空的 DataTable。
        /// </summary>
        public static bool TryCopyDataTableSource(this C1TrueDBGrid grid, out DataTable copiedDataTable)
        {
            var dt = grid.GetDataTableSourceOrNull();

            if (dt == null)
            {
                copiedDataTable = new DataTable();
                return false;
            }

            copiedDataTable = dt.Copy();
            return true;
        }

        /// <summary>
         /// 將 C1TrueDBGrid 的目前列移到 DataTable DataSource 的最後一筆。
         /// Grid 為 null、DataSource 不是 DataTable、Rows.Count = 0 時，回傳 false。
         /// </summary>
        public static bool MoveToLastDataTableRow(this C1TrueDBGrid grid)
        {
            var dt = grid.GetDataTableSourceOrNull();

            if (grid == null || dt == null || dt.Rows.Count == 0)
            {
                return false;
            }

            grid.Row = dt.Rows.Count - 1;
            return true;
        }

        /// <summary>
        /// 判斷 C1TrueDBGrid 是否沒有可用的 DataTable 資料列。
        /// Grid 為 null、DataSource 為 null、DataSource 不是 DataTable、Rows.Count = 0，都回傳 true。
        /// </summary>
        public static bool IsDataTableSourceNullOrEmpty(this C1TrueDBGrid grid)
        {
            var dt = grid.GetDataTableSourceOrNull();

            return dt == null || dt.Rows.Count == 0;
        }

        /// <summary>
        /// 安全取得 C1TrueDBGrid 的 DataTable DataSource 筆數。
        /// Grid 為 null、DataSource 為 null、DataSource 不是 DataTable 時，回傳 0。
        /// </summary>
        public static int GetDataTableRowCount(this C1TrueDBGrid grid)
        {
            var dt = grid.GetDataTableSourceOrNull();

            return dt?.Rows.Count ?? 0;
        }

        /// <summary>
        /// 對 C1TrueDBGrid 的 DataTable DataSource 套用 DefaultView.Sort。
        /// Grid 為 null、DataSource 不是 DataTable、sortExpression 為空時，回傳 false。
        /// </summary>
        public static bool ApplyDataTableSourceSort(this C1TrueDBGrid grid, string sortExpression)
        {
            var dt = grid.GetDataTableSourceOrNull();

            if (dt == null || string.IsNullOrWhiteSpace(sortExpression))
            {
                return false;
            }

            dt.DefaultView.Sort = sortExpression;
            return true;
        }

        /// <summary>
         /// 從指定的 C1TrueDBGrid 清單中，依照傳入順序取得目前包含焦點的 Grid。
         /// 找不到時回傳 null。
         /// </summary>
        public static C1TrueDBGrid GetFocusedGridOrNull(params C1TrueDBGrid[] grids)
        {
            if (grids == null || grids.Length == 0)
            {
                return null;
            }

            return grids.FirstOrDefault(grid => grid != null && grid.ContainsFocus);
        }

        /// <summary>
        /// 從指定的 C1TrueDBGrid 清單中，依照傳入順序嘗試取得目前包含焦點的 Grid。
        /// </summary>
        public static bool TryGetFocusedGrid(out C1TrueDBGrid focusedGrid, params C1TrueDBGrid[] grids)
        {
            focusedGrid = GetFocusedGridOrNull(grids);

            return focusedGrid != null;
        }
    }
}
