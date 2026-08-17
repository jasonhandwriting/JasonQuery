using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.Helpers;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void AutoResizeGridColumnWidth(bool bSchemaBrowser = true)
        {
            _isColumnAutoResizing = true;

            if (c1GridSchemaBrowser.IsDataTableSourceNull())
            {
                return;
            }

            var value = MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || bSchemaBrowser);
            var width = 0;
            var i = 3 + (value ? 1 : 0);

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        i--;
                        width = 50 + (value ? 24 : 0);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        width = 80 + (value ? -10 : -20);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        width = 60 + (value ? 15 : -10);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        width = 88;
                        break;
                    }
            }

            c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width = splitContainer1.Panel1.Width - width;

            if (c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width < 60)
            {
                c1GridSchemaBrowser.Splits[0].DisplayColumns[i].Width = 60;
            }

            c1GridSchemaBrowser.Refresh();

            _isColumnAutoResizing = false;
        }

        private void AutoResizeGridColumnWidthForColumns()
        {
            _isColumnAutoResizing = true;

            if (c1GridColumns.IsDataTableSourceNull())
            {
                return;
            }

            txtColumnFilter.Size = new Size(c1GridColumns.Width - lblPosition2.Width - 4, 21);
            c1GridColumns.Splits[0].DisplayColumns[0].Width = c1GridColumns.Width - 22;

            if (c1GridColumns.Splits[0].DisplayColumns[0].Width < 60)
            {
                c1GridColumns.Splits[0].DisplayColumns[0].Width = 60;
            }

            c1GridColumns.Refresh();
            _isColumnAutoResizing = false;
        }

        private void GridFontAndBackColor()
        {
            const int fontSize = 12;

            //字型 + 字體大小
            c1GridSchemaBrowser.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1GridStructure.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid100RowsTop.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1GridData.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1GridSchemaBrowser.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1GridSchemaBrowser.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1GridSchemaBrowser.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1GridSchemaBrowser.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
            c1GridStructure.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1GridStructure.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1GridStructure.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1GridStructure.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
            c1Grid100RowsTop.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid100RowsTop.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid100RowsTop.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid100RowsTop.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
            c1GridData.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1GridData.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1GridData.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1GridData.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1GridSchemaBrowser.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridSchemaBrowser.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
            c1GridStructure.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridStructure.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
            c1Grid100RowsTop.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid100RowsTop.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
            c1GridData.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridData.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        private void GridZoom()
        {
            const int fontSize = 11;
            const float pcnt = 0.9F;
            var rowHeight = c1GridSchemaBrowser.RowHeight;
            var recordSelectorWidth = c1GridSchemaBrowser.RecordSelectorWidth;

            //adjust row height
            c1GridSchemaBrowser.RowHeight = (int)(rowHeight * pcnt) + 5;
            c1GridStructure.RowHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid100RowsTop.RowHeight = (int)(rowHeight * pcnt) + 5;
            c1GridData.RowHeight = (int)(rowHeight * pcnt) + 5;

            //標題列的高度
            c1GridSchemaBrowser.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) + 5;
            c1GridStructure.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid100RowsTop.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) * 2 + 3;
            c1GridData.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) * 2 + 3;

            //adjust recordselector width
            c1GridSchemaBrowser.RecordSelectorWidth = (int)(recordSelectorWidth * pcnt);
            c1GridStructure.RecordSelectorWidth = (int)(recordSelectorWidth * pcnt);
            c1Grid100RowsTop.RecordSelectorWidth = (int)(recordSelectorWidth * pcnt);
            c1GridData.RecordSelectorWidth = (int)(recordSelectorWidth * pcnt);

            //adjust font sizes.  Normal is the root style so changing its sizes adjust all other styles
            c1GridSchemaBrowser.Styles["Normal"].Font = new Font(c1GridSchemaBrowser.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
            c1GridStructure.Styles["Normal"].Font = new Font(c1GridStructure.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
            c1Grid100RowsTop.Styles["Normal"].Font = new Font(c1Grid100RowsTop.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
            c1GridData.Styles["Normal"].Font = new Font(c1GridData.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
        }

        private void ApplyEditorSetting()
        {
            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editorSqlPane.Styler = new SqlStyler();
            editorSqlPreview.Styler = new SqlStyler();
        }

        private void ArrangeDataFindControls()
        {
            const int spacingAfterFindLabel = 2;
            const int spacingAfterLastFindButton = 15;
            const int fallbackSpacingAfterCombo = 134;

            lblFind2.Text = lblFindData.Text;

            tsData.PerformLayout();

            if (lblFindData.Bounds.Width > 0)
            {
                cboFind.Left = lblFindData.Bounds.Right + spacingAfterFindLabel;
            }
            else
            {
                cboFind.Left = lblFind2.Right + spacingAfterFindLabel;
            }

            if (btnClearHighlightData.Bounds.Width > 0)
            {
                chkShowFilterRowData.Left = btnClearHighlightData.Bounds.Right + spacingAfterLastFindButton;
            }
            else
            {
                chkShowFilterRowData.Left = cboFind.Right + fallbackSpacingAfterCombo;
            }

            chkShowFilterRowData.Checked = MyLibrary.GridShowFilterRow;
        }

        private void ApplyGridInteractionDirection()
        {
            var direction = MyLibrary.GridInteractionDirection;

            GridHelper.SetGridInteractionDirection(c1GridStructure, direction);
            GridHelper.SetGridInteractionDirection(c1Grid100RowsTop, direction);
            GridHelper.SetGridInteractionDirection(c1GridData, direction);
        }
    }
}