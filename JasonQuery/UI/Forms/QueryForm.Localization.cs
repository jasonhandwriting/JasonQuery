using C1.Win.C1Input;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;
using VisualStyle = C1.Win.C1Input.VisualStyle;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private const int QueryResultOptionSpacing = 1;

        private void ApplyLocalizationSetting(bool isFormLoad)
        {
            ApplyLocalizationBaseTexts(isFormLoad);
            ApplyLocalizationToolTipsAndGroupingRow();
            ApplyLocalizationThemeAndVisualStyles();
            ApplyLocalizationCommitAndPagingTexts();
            ApplyLocalizationLayoutAdjustments();
            ApplyLocalizationStatusBarTexts();
            ApplyLocalizationContextMenus();
            ApplyLocalizationFinalCleanup();

            if (_isFormLoadFinished)
            {
                ApplyEditorMessageWelcomeLayout();
                ApplyQueryResultOptionsToolbarLayout();
            }
        }

        private void ApplyLocalizationBaseTexts(bool isFormLoad)
        {
            using (TraceLogger.Time("Apply Localization Setting - call ApplyLanguageInfo()"))
            {
                LocalizationHelper.ApplyLanguageInfo(this);
            }

            lblColumnFilter.Text = LocalizationHelper.GetLanguageString("Column Filter:", "form", GetType().Name, "object", "lblColumnFilter", "Text");
            lblColumnFilterPosition.Text = lblColumnFilter.Text;
            txtColumnFilter.Location = new Point(lblColumnFilterPosition.Left + lblColumnFilterPosition.Width + 3, txtColumnFilter.Top);
            txtColumnFilter.Size = new Size(c1GridColumns.Width - lblColumnFilterPosition.Width - 4, 21);
            GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridColumns, Name, true, "gridheader");

            _operationObject = LocalizationHelper.GetLanguageString("Query Editor", "Global", "Global", "msg", "QueryEditor", "Text");
            _sqlWithoutCommentMessage = LocalizationHelper.GetLanguageString("The SQL statement you are querying does not contain any comments, and the \"Show Column Comments\" function has been automatically ignored!", "form", GetType().Name, "msg", "SqlWithoutComment", "Text");
            _sqlCannotGetColumnInfoMessage = LocalizationHelper.GetLanguageString("This SQL statement cannot retrieve information about the underlying table or view. (may contain a subquery or be enclosed in parentheses)", "form", GetType().Name, "msg", "SqlCannotGetColumnInfo", "Text");

            var dtTemp = c1GridTabList.GetDataTableSourceOrNull();

            if (dtTemp?.Rows.Count > 0)
            {
                c1GridTabList.Columns[0].Caption = LocalizationHelper.GetLanguageString("Double-click the cell to switch to the tab", "form", GetType().Name, "gridheader", "DoubleClickToSwitchTab", "Text");
            }

            dtTemp = c1GridSqlNavigator.GetDataTableSourceOrNull();

            if (dtTemp?.Rows.Count > 0)
            {
                c1GridSqlNavigator.Columns[0].Caption = LocalizationHelper.GetLanguageString("Type", "form", GetType().Name, "gridheader", "Type", "Text");
                c1GridSqlNavigator.Columns[1].Caption = LocalizationHelper.GetLanguageString("SQL Statement", "form", GetType().Name, "gridheader", "SQLStatement", "Text");
            }

            if (!isFormLoad)
            {
                ApplyAutoReplaceLocalizedCaptions();

                if (string.IsNullOrEmpty(DatabaseSqlExecutor.DatabaseName) && (IsSqlServer || IsMySql))
                {
                    _languageText = LocalizationHelper.GetLanguageString("Please use the USE statement to select a particular database first.。", "form", GetType().Name, "msg", "SelectDatabaseFirst", "Text");
                    SetEditorStatusBarInfo(_languageText, Color.DarkRed);
                }
            }
        }

        private void ApplyLocalizationToolTipsAndGroupingRow()
        {
            var toolTip1 = new ToolTip
            {
                ForeColor = Color.Blue,
                BackColor = Color.Gray,
                AutoPopDelay = 5000
            };

            var languageText = string.Empty;

            languageText = LocalizationHelper.GetLanguageString("Show or hide the filter row.\r\nEnter conditions in one or more columns to filter result rows.\r\nTurn it off to clear all filter conditions and restore the original result.", "form", GetType().Name, "object", "chkShowFilterRow", "ToolTipText");
            toolTip1.SetToolTip(chkShowFilterRow, languageText);

            languageText = LocalizationHelper.GetLanguageString("Automatically adjust column widths.", "form", GetType().Name, "object", "chkSize", "ToolTipText");
            toolTip1.SetToolTip(chkSize, languageText);

            languageText = LocalizationHelper.GetLanguageString("Show or hide each result column's data type in the header.", "form", GetType().Name, "object", "chkShowColumnType", "ToolTipText");
            toolTip1.SetToolTip(chkShowColumnType, languageText);

            languageText = LocalizationHelper.GetLanguageString("Show or hide each result column's comment in the header.", "form", GetType().Name, "object", "chkShowColumnComments", "ToolTipText");
            toolTip1.SetToolTip(chkShowColumnComments, languageText);

            languageText = LocalizationHelper.GetLanguageString("Show or hide the grouping row. Drag columns there to group the result rows.", "form", GetType().Name, "object", "chkShowGroupingRow", "ToolTipText");
            toolTip1.SetToolTip(chkShowGroupingRow, languageText);

            languageText = LocalizationHelper.GetLanguageString("Display results in raw data mode. Use the ? button for details.", "form", GetType().Name, "object", "chkRawDataMode", "ToolTipText");
            toolTip1.SetToolTip(chkRawDataMode, languageText);

            languageText = LocalizationHelper.GetLanguageString("0, 30-3600\r\n0 indicates no limit.", "form", GetType().Name, "object", "nudQueryTimeout", "ToolTipText");
            toolTip1.SetToolTip(nudQueryTimeout, languageText);

            c1TrueDBGrid1.GroupStyle.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);
            c1TrueDBGrid1.GroupStyle.BackColor = Color.LightYellow;
            c1TrueDBGrid1.GroupByCaption = LocalizationHelper.GetLanguageString("Drag a column header here to group by that column", "form", GetType().Name, "object", "GroupingRowCaption", "Text");
        }

        private void ApplyLocalizationThemeAndVisualStyles()
        {
            if (MyLibrary.IsDarkMode)
            {
                c1ThemeController1.SetTheme(c1TrueDBGrid1, "VS2013Dark");
                GridHelper.SetGridVisualStyle(c1TrueDBGrid1, MyLibrary.GridFontSize);

                if (AppConfigHelper.IsChangeColorThemeNeedRestart)
                {
                    _fontSize = _fontSize == 0 ? 10 : _fontSize;
                    c1TrueDBGrid1.Styles["Normal"].Font = new Font(MyLibrary.GridFontName, _fontSize * 1);
                }

                c1TrueDBGrid1.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1StatusBar1, "ExpressionDark");
                c1ThemeController1.SetTheme(c1StatusBar2, "ExpressionDark");
                _toolstripFocused = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                _toolstripUnfocused = ColorTranslator.FromHtml("#2D2D30");

                lblInfoEditor.BackColor = ColorTranslator.FromHtml("#DFE9F5");

                ApplyAutoReplaceTheme();

                c1ThemeController1.SetTheme(c1GridTabList, "VS2013Dark");
                c1GridTabList.BackColor = ColorTranslator.FromHtml("#2D2D30");

                c1ThemeController1.SetTheme(c1GridSqlNavigator, "VS2013Dark");
                c1GridSqlNavigator.BackColor = ColorTranslator.FromHtml("#2D2D30");

                UIHelper.SetDockingTabColor(c1DockingTab1,
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor));

                UIHelper.SetDockingTabColor(c1DockingTab2,
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor));
            }
            else
            {
                c1ThemeController1.SetTheme(c1TrueDBGrid1, "(default)");
                GridHelper.SetGridVisualStyle(c1TrueDBGrid1, MyLibrary.GridFontSize);
                c1TrueDBGrid1.BackColor = SystemColors.Control;

                c1ThemeController1.SetTheme(this, "(default)");
                c1ThemeController1.SetTheme(c1DockingTab1, "(default)");
                BackColor = SystemColors.Control;

                c1ThemeController1.SetTheme(c1StatusBar1, "(default)");
                c1ThemeController1.SetTheme(c1StatusBar2, "(default)");
                _toolstripFocused = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                _toolstripUnfocused = SystemColors.Control;

                lblInfoEditor.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);

                ApplyAutoReplaceTheme();

                c1ThemeController1.SetTheme(c1GridTabList, "(default)");
                c1GridTabList.BackColor = ColorTranslator.FromHtml("#F0F0F0");

                c1ThemeController1.SetTheme(c1GridSqlNavigator, "(default)");
                c1GridSqlNavigator.BackColor = ColorTranslator.FromHtml("#F0F0F0");

                UIHelper.SetDockingTabColor(c1DockingTab1,
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor));

                UIHelper.SetDockingTabColor(c1DockingTab2,
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor),
                                            ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor));

                txtIndentWord.VisualStyle = VisualStyle.Office2007Blue;
                chkShowFilterRow.VisualStyle = VisualStyle.Office2007Blue;
                chkShowFilterRow.BackColor = Color.Transparent;
                chkSize.VisualStyle = VisualStyle.Office2007Blue;
                chkSize.BackColor = Color.Transparent;
                cboFindGrid.VisualStyle = VisualStyle.Office2007Blue;
                chkShowColumnType.VisualStyle = VisualStyle.Office2007Blue;
                chkShowColumnType.BackColor = Color.Transparent;
                chkShowGroupingRow.VisualStyle = VisualStyle.Office2007Blue;
                chkShowGroupingRow.BackColor = Color.Transparent;
                chkShowColumnComments.VisualStyle = VisualStyle.Office2007Blue;
                chkShowColumnComments.BackColor = Color.Transparent;
                chkRawDataMode.VisualStyle = VisualStyle.Office2007Blue;
                chkRawDataMode.BackColor = Color.Transparent;
            }
        }

        private void ApplyLocalizationCommitAndPagingTexts()
        {
            btnPaginationOn.Text = LocalizationHelper.GetLanguageString("Paging Query", "form", GetType().Name, "statusbarobject", "btnPaginationOn", "Text");
            btnPaginationOn.ToolTip = LocalizationHelper.GetLanguageString("{qty} rows per page", "form", GetType().Name, "statusbarobject", "btnPaginationOn", "ToolTipText").Replace("{qty}", MyLibrary.GridRowsPerPage);

            btnPaginationOff.Text = LocalizationHelper.GetLanguageString("Paging Query", "form", GetType().Name, "statusbarobject", "btnPaginationOff", "Text");
            btnPaginationOff.ToolTip = LocalizationHelper.GetLanguageString("{qty} rows per page", "form", GetType().Name, "statusbarobject", "btnPaginationOn", "ToolTipText").Replace("{qty}", MyLibrary.GridRowsPerPage);

            btnNextPage.ToolTip = LocalizationHelper.GetLanguageString("Next Page", "form", GetType().Name, "statusbarobject", "btnNextPage", "ToolTipText");

            btnAppendingQueriesOn.Text = LocalizationHelper.GetLanguageString("Appending", "form", GetType().Name, "statusbarobject", "btnAppendingQueriesOn", "Text");
            btnAppendingQueriesOn.ToolTip = LocalizationHelper.GetLanguageString("Appending", "form", GetType().Name, "statusbarobject", "btnAppendingQueriesOn", "ToolTipText");

            btnAppendingQueriesOff.Text = LocalizationHelper.GetLanguageString("Appending", "form", GetType().Name, "statusbarobject", "btnAppendingQueriesOff", "Text");
            btnAppendingQueriesOff.ToolTip = LocalizationHelper.GetLanguageString("Appending", "form", GetType().Name, "statusbarobject", "btnAppendingQueriesOff", "ToolTipText");
        }

        private void ApplyLocalizationLayoutAdjustments()
        {
            lblSchemaFilterPosition.Text = lblSchemaFilter.Text;
            txtSchemaFilter.Location = new Point(lblSchemaFilterPosition.Left + lblSchemaFilterPosition.Width + 3, txtSchemaFilter.Top);
            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);

            AutoResizeGridColumnWidthForSchema();

            c1TrueDBGrid1.AllowRowSizing = TextHelper.GetKeyFromDictionary(MyGlobal.dicRowSizing, MyGlobal.RowSize) == "AllRows"
                                           ? RowSizingEnum.AllRows : RowSizingEnum.IndividualRows;

            c1TrueDBGrid1.HeadingStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHeadingForeColor);

            lblQueryTimeoutPosition.Text = lblQueryTimeout.Text;
            nudQueryTimeout.Location = new Point(lblQueryTimeoutPosition.Left + lblQueryTimeoutPosition.Width, nudQueryTimeout.Top);

            ApplyAutoReplaceLayoutAdjustments();

            btnHelp_QueryTimeout.Location = new Point(nudQueryTimeout.Left + nudQueryTimeout.Width + 3, btnHelp_QueryTimeout.Top);
        }

        private void ApplyQueryResultOptionsToolbarLayout()
        {
            var parent = splitContainer3.Panel2;

            parent.SuspendLayout();

            try
            {
                ApplyQueryResultToolbarCheckBoxSize();

                chkSize.Left = chkShowFilterRow.Right + QueryResultOptionSpacing;
                chkShowColumnType.Left = chkSize.Right + QueryResultOptionSpacing;
                chkShowColumnComments.Left = chkShowColumnType.Right - QueryResultOptionSpacing;

                UpdateDataGridToolStripSpacer(chkShowColumnComments.Right + 3);

                tsDataGrid.PerformLayout();

                if (lblFindGrid.Bounds.Width > 0)
                {
                    cboFindGrid.Left = lblFindGrid.Bounds.Right + 3;
                }
                else
                {
                    cboFindGrid.Left = chkShowColumnComments.Right + 40;
                }

                if (btnClearHighlightsGrid.Bounds.Width > 0)
                {
                    chkShowGroupingRow.Left = btnClearHighlightsGrid.Bounds.Right + 15;
                }
                else
                {
                    chkShowGroupingRow.Left = cboFindGrid.Right + 134;
                }

                chkRawDataMode.Left = chkShowGroupingRow.Right + QueryResultOptionSpacing;
                btnHelp_RawDataMode.Left = chkRawDataMode.Right - 6;
            }
            finally
            {
                parent.ResumeLayout(true);
            }
        }

        private void UpdateDataGridToolStripSpacer(int desiredFindLabelLeft)
        {
            lblSpace.Font = chkSize.Font;
            lblSpace.Text = string.Empty;

            tsDataGrid.PerformLayout();

            var bestSpaces = 0;
            var bestDelta = int.MaxValue;

            for (var i = 0; i < 200; i++)
            {
                lblSpace.Text = new string(' ', i);
                tsDataGrid.PerformLayout();

                var delta = Math.Abs(lblFindGrid.Bounds.Left - desiredFindLabelLeft);

                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestSpaces = i;
                }

                if (lblFindGrid.Bounds.Left >= desiredFindLabelLeft)
                {
                    break;
                }
            }

            lblSpace.Text = new string(' ', bestSpaces);
            tsDataGrid.PerformLayout();
        }

        private void ApplyQueryResultToolbarCheckBoxSize()
        {
            ApplyToolbarCheckBoxSize(chkShowFilterRow);
            ApplyToolbarCheckBoxSize(chkSize);
            ApplyToolbarCheckBoxSize(chkShowColumnType);
            ApplyToolbarCheckBoxSize(chkShowColumnComments);
            ApplyToolbarCheckBoxSize(chkShowGroupingRow);
            ApplyToolbarCheckBoxSize(chkRawDataMode);
        }

        private static void ApplyToolbarCheckBoxSize(C1CheckBox checkBox)
        {
            if (checkBox == null)
            {
                return;
            }

            checkBox.AutoSize = false;
            checkBox.Width = GetToolbarCheckBoxWidth(checkBox);
        }

        private static int GetToolbarCheckBoxWidth(C1CheckBox checkBox)
        {
            var text = checkBox.Text ?? string.Empty;

            var textSize = TextRenderer.MeasureText
            (
                text,
                checkBox.Font,
                Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
            );

            const int checkBoxGlyphAndGapWidth = 18;
            const int safetyWidth = 4;

            return textSize.Width
                 + checkBoxGlyphAndGapWidth
                 + checkBox.Padding.Left
                 + checkBox.Padding.Right
                 + safetyWidth;
        }

        private void UpdateDataGridToolStripSpacer()
        {
            tsDataGrid.PerformLayout();

            var desiredFindLabelLeft = chkShowColumnComments.Right + 8;
            var spacerStart = toolStripLabel2.Bounds.Right;
            var separatorWidth = toolStripSeparator11.Width;

            var targetWidth = desiredFindLabelLeft - spacerStart - separatorWidth;

            if (targetWidth < 0)
            {
                targetWidth = 0;
            }

            lblSpace.Font = chkSize.Font;

            for (var i = 0; i < 200; i++)
            {
                lblSpace.Text = new string(' ', i);
                tsDataGrid.PerformLayout();

                if (lblSpace.Width >= targetWidth)
                {
                    break;
                }
            }
        }

        private static int GetRight(Control control)
        {
            return control.Left + GetLayoutWidth(control);
        }

        private static int GetLayoutWidth(Control control)
        {
            if (control == null)
            {
                return 0;
            }

            if (control.AutoSize)
            {
                return Math.Max(control.Width, control.PreferredSize.Width);
            }

            return control.Width;
        }

        private void ApplyLocalizationStatusBarTexts()
        {
            _languageText = LocalizationHelper.GetLanguageString(TextHelper.GetSafeString(tabMessage.Tag), "form", GetType().Name, "object", tabMessage.Name, "Text");
            tabMessage.Text = _languageText;
            _originalTabName = $"`{_languageText}`";

            _dataGridTabName = LocalizationHelper.GetLanguageString(TextHelper.GetSafeString(tabDataGrid.Tag), "form", GetType().Name, "object", tabDataGrid.Name, "Text");
            tabDataGrid.Text = _dataGridTabName;
            _originalTabName = $"{_originalTabName}{_dataGridTabName}`";

            if (string.IsNullOrEmpty(_dataGridTabNameOriginal))
            {
                _dataGridTabNameOriginal = _dataGridTabName;
            }

            if (c1DockingTab1.TabPages.Count > 3 && _dataGridTabNameOriginal != _dataGridTabName)
            {
                foreach (Control ctrl in c1DockingTab1.TabPages)
                {
                    ctrl.Text = ctrl.Text.Replace(_dataGridTabNameOriginal, _dataGridTabName);
                }

                _dataGridTabNameOriginal = _dataGridTabName;
            }

            _editorLength = LocalizationHelper.GetLanguageString(lblEditorLength.Text, "form", GetType().Name, "statusbarobject", "lblEditorLength", "Text");
            _editorLines = LocalizationHelper.GetLanguageString(lblEditorLines.Text, "form", GetType().Name, "statusbarobject", "lblEditorLines", "Text");
            _editorLn = LocalizationHelper.GetLanguageString(lblEditorLn.Text, "form", GetType().Name, "statusbarobject", "lblEditorLn", "Text");
            _editorCol = LocalizationHelper.GetLanguageString(lblEditorCol.Text, "form", GetType().Name, "statusbarobject", "lblEditorCol", "Text");
            _editorPos = LocalizationHelper.GetLanguageString(lblEditorPos.Text, "form", GetType().Name, "statusbarobject", "lblEditorPos", "Text");
            _editorSel = LocalizationHelper.GetLanguageString(lblEditorSel.Text, "form", GetType().Name, "statusbarobject", "lblEditorSel", "Text");
            _execTime = LocalizationHelper.GetLanguageString(lblExecTime.Text, "form", GetType().Name, "statusbarobject", "lblExecTime", "Text");
            lblExecTime.Text = $"{_execTime} 00:00.000";
            lblExecTime.ToolTip = LocalizationHelper.GetLanguageString("Total time including the query and all result processing in JasonQuery.", "form", GetType().Name, "object", "lblExecTime", "ToolTipText");

            _queryTime = LocalizationHelper.GetLanguageString(lblQueryTime.Text, "form", GetType().Name, "statusbarobject", "lblQueryTime", "Text");
            lblQueryTime.Text = $"{_queryTime} 00:00.000";
            lblQueryTime.ToolTip = LocalizationHelper.GetLanguageString("Time taken for the database to run the SQL and return the results.", "form", GetType().Name, "object", "lblQueryTime", "ToolTipText");

            _rows = LocalizationHelper.GetLanguageString(lblRows.Text, "form", GetType().Name, "statusbarobject", "lblRows", "Text");
            lblRows.Text = $"0 {_rows}";

            lblAverage.Text = LocalizationHelper.GetLanguageString(lblAverage.Text, "form", GetType().Name, "statusbarobject", "lblAverage", "Text");
            lblCount.Text = LocalizationHelper.GetLanguageString(lblCount.Text, "form", GetType().Name, "statusbarobject", "lblCount", "Text");
            lblSummary.Text = LocalizationHelper.GetLanguageString(lblSummary.Text, "form", GetType().Name, "statusbarobject", "lblSummary", "Text");

            UpdateEditorStatusBarLanguage();
        }

        private void ApplyLocalizationContextMenus()
        {
            _messageEditorContextMenu = new ContextMenuStrip();
            _messageEditorContextMenu.Items.Add(LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menueditor", "SelectAll", "Text"));
            _messageEditorContextMenu.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");
            ((ToolStripMenuItem)_messageEditorContextMenu.Items[0]).ShortcutKeys = Keys.Control | Keys.A;

            _messageEditorContextMenu.Items[0].Click += delegate
            {
                editorMessage.SelectionStart = 0;
                editorMessage.SelectionEnd = editorMessage.Text.Length;
            };

            _messageEditorContextMenu.Items.Add("-");

            _messageEditorContextMenu.Items.Add(LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menueditor", "Copy", "Text"));
            _messageEditorContextMenu.Items[2].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");
            ((ToolStripMenuItem)_messageEditorContextMenu.Items[2]).ShortcutKeys = Keys.Control | Keys.C;

            _messageEditorContextMenu.Items[2].Click += delegate
            {
                if (MyLibrary.CopyAsHTML)
                {
                    Clipboard.Clear();
                    editorMessage.Copy(ScintillaNET.CopyFormat.Text | ScintillaNET.CopyFormat.Rtf | ScintillaNET.CopyFormat.Html);
                }
                else
                {
                    editorMessage.Copy();
                }
            };

            ApplyEditorMenu();
            ApplyGridMenu();
            GridHelper.SetGridVisualStyle(c1GridSchemaBrowser, 10);
        }

        private void ApplyLocalizationFinalCleanup()
        {
            lblInfo.Text = string.Empty;
            lblInfo.Tag = string.Empty;
            c1StatusBar1.Tag = string.Empty;

            lblInfoEditor.Text = string.Empty;
            lblInfoEditor.Tag = string.Empty;
            c1StatusBar2.Tag = string.Empty;
        }

        private void RequestQueryResultOptionsToolbarLayoutAfterLocalization()
        {
            if (!_isFormLoadFinished)
            {
                return;
            }

            if (!IsHandleCreated || IsDisposed || Disposing)
            {
                return;
            }

            var requestId = ++_queryResultOptionsToolbarLayoutRequestId;

            BeginInvoke
            (
                new Action
                (
                    () =>
                    {
                        ApplyQueryResultOptionsToolbarLayoutAfterLocalization(requestId);

                        BeginInvoke
                        (
                            new Action
                            (
                                () =>
                                {
                                    ApplyQueryResultOptionsToolbarLayoutAfterLocalization(requestId);
                                }
                            )
                        );
                    }
                )
            );
        }

        private void ApplyQueryResultOptionsToolbarLayoutAfterLocalization(int requestId)
        {
            if (requestId != _queryResultOptionsToolbarLayoutRequestId)
            {
                return;
            }

            if (IsDisposed || Disposing)
            {
                return;
            }

            if (!splitContainer1.Visible)
            {
                return;
            }

            splitContainer3.Panel2.SuspendLayout();
            tsDataGrid.SuspendLayout();

            try
            {
                tsDataGrid.PerformLayout();

                ApplyQueryResultOptionsToolbarLayout();

                tsDataGrid.PerformLayout();
            }
            finally
            {
                tsDataGrid.ResumeLayout(true);
                splitContainer3.Panel2.ResumeLayout(true);
            }

            tsDataGrid.Invalidate();
            splitContainer3.Panel2.Invalidate(true);
        }
    }
}