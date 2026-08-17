using C1.Win.C1Themes;
using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SingleRecordViewerForm : Form
    {
        private int _totalRecordCount;
        private int _currentRecordIndex;
        private int _sourceColumnIndex;
        private DataTable _dtData;
        private string _languageText = string.Empty;

        public Func<int, DataTable> RecordLoader { get; set; }

        public Action<int, int> CurrentRecordChanged { get; set; }

        public void Initialize(DataTable data, int totalRecordCount, int currentRecordIndex, int sourceColumnIndex)
        {
            _dtData = data;
            _totalRecordCount = totalRecordCount;
            _currentRecordIndex = currentRecordIndex;
            _sourceColumnIndex = sourceColumnIndex;
        }

        public SingleRecordViewerForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                if (_dtData == null)
                {
                    return;
                }

                c1Grid.Tag = _dtData.Rows.Count.ToString();

                CheckButtonStatus();

                c1Grid.DataSource = _dtData;
                ApplyLocalizationSetting();

                LocalizationHelper.ApplyLanguageInfo(this, false);
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1Grid, Name);
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void RefreshDataGrid()
        {
            var i = 0;
            var width = 0;

            foreach (C1DisplayColumn col in c1Grid.Splits[0].DisplayColumns)
            {
                if (i == 0)
                {
                    try
                    {
                        col.AutoSize();
                    }
                    catch (Exception)
                    {
                        col.Width = 200;
                    }
                }
                else
                {
                    col.Width = Width - width - 65;
                }

                i++;
            }

            if (!string.Equals(MyLibrary.GridNullShowAs, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                var colorNull = new Style
                {
                    ForeColor = ColorTranslator.FromHtml(MyLibrary.GridNullShowColor)
                };

                for (var j = 0; j < c1Grid.Columns.Count; j++)
                {
                    //套用「使用者指定的 NULL」顯示格式
                    c1Grid.Splits[0].DisplayColumns[j].AddRegexCellStyle(CellStyleFlag.AllCells, colorNull, MyLibrary.GridNullShowAs);
                }
            }

            c1Grid.Refresh();
        }

        private void CheckButtonStatus()
        {
            btnFirst.Enabled = true;
            btnPrevious.Enabled = true;
            btnNext.Enabled = true;
            btnLast.Enabled = true;

            if (_totalRecordCount <= 1)
            {
                btnFirst.Enabled = false;
                btnPrevious.Enabled = false;
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
            else if (_currentRecordIndex == 0)
            {
                btnFirst.Enabled = false;
                btnPrevious.Enabled = false;
            }
            else if (_currentRecordIndex == _totalRecordCount - 1)
            {
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SingleRecordViewerFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "SingleRecordViewerFormHeight", Size.Height.ToString());

            RefreshDataGrid();
        }

        private void ApplyLocalizationSetting()
        {
            if (MyLibrary.IsDarkMode)
            {
                C1ThemeController.ApplicationTheme = "VS2013Dark";
            }

            chkShowFilterRow.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
            tsViewer.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

            GridHelper.SetGridVisualStyle(c1Grid);
            GridFontAndBackColor();
            GridZoom();

            GridHelper.SetGridVisualStyle(c1Grid, 10);

            Cursor = Cursors.Default;
        }

        private void GridFontAndBackColor()
        {
            const int fontSize = 12;

            //字型 + 字體大小
            c1Grid.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1Grid.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1Grid.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        private void chkShowFilterRow_CheckedChanged(object sender, EventArgs e)
        {
            c1Grid.FilterBar = chkShowFilterRow.Checked;
        }

        private void GridZoom()
        {
            const int fontSize = 11;
            const float pcnt = 0.9F;
            var rowHeight = c1Grid.RowHeight;
            //var iRecSelWidth = c1Grid.RecordSelectorWidth;

            c1Grid.RowHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid.Styles["Normal"].Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
        }

        private void c1Grid_Filter(object sender, FilterEventArgs e)
        {
            var dataView = (c1Grid.DataSource as DataTable)?.DefaultView;

            if (dataView == null || dataView.RowFilter == e.Condition)
            {
                return;
            }

            var condition = e.Condition;

            if (condition.Length > 0)
            {
                condition = e.Condition;

                var count = c1Grid.Splits[0].DisplayColumns.Count;

                for (var i = 0; i < count; i++)
                {
                    var caption = c1Grid.Columns[i].Caption;

                    if (!condition.Contains($"[{caption}]"))
                    {
                        continue;
                    }

                    var paramIndex = condition.IndexOf('\'', condition.IndexOf($"[{caption}]", StringComparison.Ordinal)) + 1;

                    condition = condition.Insert(paramIndex, "*");
                }
            }

            dataView.RowFilter = condition;
        }

        private void c1Grid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var row = c1Grid.RowContaining(e.Y);

            if (row != -1)
            {
                CellViewer();
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void SelectAll()
        {
            c1Grid.SelectedRows.Clear();

            for (var i = 0; i < c1Grid.Splits[0].Rows.Count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            MoveToRecord(0);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            MoveToRecord(_currentRecordIndex - 1);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            MoveToRecord(_currentRecordIndex + 1);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            MoveToRecord(_totalRecordCount - 1);
        }

        private void MoveToRecord(int recordIndex)
        {
            if (recordIndex < 0 || recordIndex >= _totalRecordCount)
            {
                return;
            }

            _currentRecordIndex = recordIndex;
            LoadCurrentRecord();
        }

        private void LoadCurrentRecord()
        {
            if (RecordLoader == null)
            {
                return;
            }

            var currentViewerRow = c1Grid.Row;
            var currentViewerCol = c1Grid.Col;

            Cursor = Cursors.WaitCursor;
            tsViewer.Cursor = Cursors.WaitCursor;

            try
            {
                var data = RecordLoader(_currentRecordIndex);

                if (data == null)
                {
                    return;
                }

                _dtData = data;
                c1Grid.DataSource = _dtData;
                c1Grid.Tag = _dtData.Rows.Count.ToString();

                RefreshDataGrid();

                RestoreViewerGridPosition(currentViewerRow, currentViewerCol);

                CurrentRecordChanged?.Invoke(_currentRecordIndex, _sourceColumnIndex);
                CheckButtonStatus();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
                tsViewer.Cursor = Cursors.Default;
            }
        }

        private void RestoreViewerGridPosition(int rowIndex, int colIndex)
        {
            if (_dtData == null || _dtData.Rows.Count == 0)
            {
                return;
            }

            var maxRowIndex = c1Grid.Splits[0].Rows.Count - 1;
            var maxColIndex = c1Grid.Columns.Count - 1;

            if (maxRowIndex < 0 || maxColIndex < 0)
            {
                return;
            }

            c1Grid.Row = Math.Min(Math.Max(rowIndex, 0), maxRowIndex);
            c1Grid.Col = Math.Min(Math.Max(colIndex, 0), maxColIndex);
            c1Grid.Select();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopyDataFromDataGrid();
        }

        private void c1Grid_MouseClick(object sender, MouseEventArgs e)
        {
            var isCornerSelected = false;
            var row = c1Grid.RowContaining(e.Y);
            var col = c1Grid.ColContaining(e.X);

            if (row == -1 && col == -1)
            {
                isCornerSelected = !chkShowFilterRow.Checked || e.Y <= c1Grid.Splits[0].ColumnCaptionHeight;
            }

            if (!isCornerSelected)
            {
                return;
            }

            c1Grid.SelectedRows.Clear();

            for (var i = 0; i < c1Grid.Splits[0].Rows.Count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }
        }

        private void c1Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var gMenu2 = new ContextMenuStrip();

            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menugrid", "SelectAll", "Text");
            gMenu2.Items.Add(_languageText);

            gMenu2.Items[0].Click += delegate
            {
                SelectAll();
            };

            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menugrid", "Copy", "Text");
            gMenu2.Items.Add(_languageText);

            gMenu2.Items[1].Click += delegate
            {
                CopyDataFromDataGrid();
            };

            if (MyLibrary.IsDarkMode)
            {
                gMenu2.BackColor = ColorTranslator.FromHtml("#2D2D30");
                gMenu2.ForeColor = Color.White;
                gMenu2.RenderMode = ToolStripRenderMode.System;
            }

            c1Grid.ContextMenuStrip = gMenu2;
            gMenu2.Show(c1Grid, new Point(e.X, e.Y));
        }

        private void c1Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
            }
        }

        private void CopyDataFromDataGrid()
        {
            var i = 0;
            var data = string.Empty;
            var columnName = string.Empty;
            var dataType = string.Empty;
            var isActiveCell = true; //是否為「只點選單一個 cell，並沒有『選取範圍』」?
            var selCol = c1Grid.SelectedCols.Count;
            var quotingWith = string.Empty;
            var fieldSeparator = ",";
            var isSelectedWholeColumn = c1Grid.SelectedRows.Count == 0 && c1Grid.SelectedCols.Count > 0;

            if (isSelectedWholeColumn) //整欄選取
            {
                isActiveCell = false;

                for (var row = 0; row < c1Grid.Splits[0].Rows.Count; row++)
                {
                    foreach (C1DataColumn column in c1Grid.SelectedCols)
                    {
                        var columnString = column.ToString();
                        var caption = c1Grid.Columns[columnString].Caption;
                        var cellText = c1Grid.Columns[columnString].CellText(row);
                        string[] splitters = { "\r\n", "\r", "\n" };
                        var parts = caption.Split(splitters, 2, StringSplitOptions.None);
                        var temp1 = parts[0];
                        var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                        if (row == 0)
                        {
                            //收集 Column Name & Data Type
                            columnName += $"{temp1}{fieldSeparator}";
                            dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                        }

                        data += $"{quotingWith}{cellText}{quotingWith}{fieldSeparator}";
                    }

                    if (!string.IsNullOrEmpty(data))
                    {
                        var temp = data.Substring(0, data.Length - fieldSeparator.Length);

                        data = $"{temp}\r\n";
                    }
                }
            }
            else //非整欄選取
            {
                foreach (int row in c1Grid.SelectedRows)
                {
                    var vr = c1Grid.Splits[0].Rows[row];

                    if (selCol == 0) //整列選取
                    {
                        isActiveCell = false;

                        foreach (C1DataColumn column in c1Grid.Columns)
                        {
                            var caption = column.Caption;
                            var cellText = column.CellText(vr.DataRowIndex);
                            string[] splitters = { "\r\n", "\r", "\n" };
                            var parts = caption.Split(splitters, 2, StringSplitOptions.None);
                            var temp1 = parts[0];
                            var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                            if (i == 0)
                            {
                                //收集 Column Name & Data Type
                                columnName += $"{temp1}{fieldSeparator}";
                                dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                            }

                            data += $"{cellText}{fieldSeparator}";
                        }

                        i++;
                    }
                    else //非整列選取 (選取區塊)
                    {
                        foreach (C1DataColumn column in c1Grid.SelectedCols)
                        {
                            isActiveCell = false;

                            var caption = column.Caption;
                            var cellText = column.CellText(vr.DataRowIndex);
                            string[] splitters = { "\r\n", "\r", "\n" };
                            var parts = caption.Split(splitters, 2, StringSplitOptions.None);
                            var temp1 = parts[0];
                            var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                            if (i == 0)
                            {
                                //收集 Column Name & Data Type
                                columnName += $"{temp1}{fieldSeparator}";
                                dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                            }

                            data += $"{cellText}{fieldSeparator}";
                        }

                        i++;
                    }

                    if (!string.IsNullOrEmpty(data))
                    {
                        var temp = data.Substring(0, data.Length - fieldSeparator.Length);

                        data = $"{temp}\r\n";
                    }
                }
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                var temp = columnName.Substring(0, columnName.Length - fieldSeparator.Length);

                columnName = $"{temp}\r\n";
            }

            if (!string.IsNullOrEmpty(dataType))
            {
                var temp = dataType.Substring(0, dataType.Length - fieldSeparator.Length);

                dataType = $"{temp}\r\n";
            }

            if (isActiveCell)
            {
                var row = c1Grid.Splits[0].Rows[c1Grid.Row].DataRowIndex;

                data = c1Grid[row, c1Grid.Col].ToString();
            }

            TextHelper.CopyTextToClipboard($"{columnName}{dataType}{data}", "CopyDataFromDataGrid()");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    {
                        Close();
                        return true;
                    }
                case Keys.Control | Keys.C:
                case Keys.Control | Keys.Insert: //20231020 Ctrl+INS
                    {
                        if (c1Grid.Focused)
                        {
                            CopyDataFromDataGrid();
                            return true; //此處必須為 true，否則會「使用 Grid 原生的 Copy 功能」
                        }

                        break;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}