using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Editor.FindAndReplace;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void InitializeFormLoadBaseUi()
        {
            //20241101 預設不顯示左側「欄位訊息」
            splitContainer4.Panel1Collapsed = true;
            splitContainer4.SplitterWidth = 2;

            GridHelper.SetGridVisualStyle(c1GridTabList, 10);
            GridHelper.SetGridVisualStyle(c1GridSqlNavigator, 10);
            _sqlNavigatorRowHeight = c1GridSqlNavigator.RowHeight;

            using (TraceLogger.Time("Load/Set Splitter"))
            {
                LoadSplitterData("L/R");
                LoadSplitterData("LL/RR");
            }

            _backupFileName = AccessibleName;
            nudQueryTimeout.Value = DatabaseSqlExecutor.QueryTimeoutSeconds;
            lblIndentWord.Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136);

            lblAverage.Visible = false;
            lblAverageValue.Visible = false;
            lblCount.Visible = false;
            lblCountValue.Visible = false;
            lblSummary.Visible = false;
            lblSummaryValue.Visible = false;
            lblSeparator1.Visible = false;
            lblSeparator2.Visible = false;
            lblSeparator3.Visible = false;

            btnHighlightAllGrid.Tag = "0";

            lblFindGrid.Tag = string.Empty;
            btnCancelQuery.Tag = string.Empty;
            btnNextPage.Tag = string.Empty;
            btnPaginationOff.Tag = string.Empty;
            btnPaginationOn.Tag = MyLibrary.GridRowsPerPage;

            ClearHighlightSelectionCopyState();

            lblInfo.Text = string.Empty;
            lblInfo.Tag = string.Empty;
            c1StatusBar1.Tag = string.Empty;

            lblInfoEditor.Text = string.Empty;
            lblInfoEditor.Tag = string.Empty;
            c1StatusBar2.Tag = string.Empty;

            tmrlblInfo.Enabled = true;
            tmrlblInfoEditor.Enabled = true;
        }

        private void InitializeFormLoadEditorAndIcons()
        {
            tabDataGrid.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Data Grid 16x16.ico");

            //20250909 圖示的外觀品質會莫名變差，故此處重新載入
            btnSaveRed.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Save 24x24 Red.ico");
            btnClearHighlightsGrid.Image = IconManager.GetImage(MyGlobal.IconLibrary, "ClearHighlight 24x24.ico");
            btnShowAllCharacters.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Paragraph New 24x24.ico");
            btnNew.Image = IconManager.GetImage(MyGlobal.IconLibrary, "New File 24x24.ico");
            //btnShowColumns.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column 16x16.ico"); //20260620 添加此圖示

            if (MyLibrary.CommitRollbackIcon != 1)
            {
                btnCommit.Image = IconManager.GetImage(MyGlobal.IconLibrary, $"Commit {MyLibrary.CommitRollbackIcon} 24x24.ico");
                btnRollback.Image = IconManager.GetImage(MyGlobal.IconLibrary, $"Rollback {MyLibrary.CommitRollbackIcon} 24x24.ico");
            }

            _queryEditorFontSize = MyLibrary.QueryEditorFontSizeText;
            editor.MouseWheel += editor_MouseWheel;

            _findAndReplace = new FindAndReplace();
            _findAndReplace.Scintilla = editor;

            if (MyLibrary.IsDarkMode && DatabaseSqlExecutor.dtSchema == null)
            {
                SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
                editor.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
                editor.Styler = new SqlStyler();

                editorMessage.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
                editorMessage.Styler = new SqlStyler();
            }

            c1GridAutoCompleteForPeriod.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
            c1GridAutoCompleteForSpace.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
            c1GridAutoCompleteForAll.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorEditorBackground);
        }

        private void InitializeFormLoadToolbarAndOptions()
        {
            mnuFocusOnQueryEditor.Checked = AppConfigHelper.IsAfterPasteFocusOnQueryEditor;
            mnuFocusOnDataGrid.Checked = !AppConfigHelper.IsAfterPasteFocusOnQueryEditor;

            btnPaginationOn.Visible = MyLibrary.GridPagingQuery;
            btnPaginationOff.Visible = !MyLibrary.GridPagingQuery;
            btnAppendingQueriesOn.Visible = MyLibrary.GridAppendingQueries;
            btnAppendingQueriesOff.Visible = !MyLibrary.GridAppendingQueries;
            btnNextPage.Enabled = false;

            using (TraceLogger.Time("Apply Localization Setting"))
            {
                ApplyLocalizationSetting(true);
            }

            using (TraceLogger.Time("Set Grid Style for Auto Complete"))
            {
                GridHelper.SetGridVisualStyle(c1GridAutoCompleteForPeriod, 10);
                GridHelper.SetGridVisualStyle(c1GridAutoCompleteForSpace, 10);
                GridHelper.SetGridVisualStyle(c1GridAutoCompleteForAll, 10);
                btnExpandCollapse.DropDownItemClicked += SplitButton_DropDownItemClicked;
                SetGridToolStripBackColor(false);
            }

            switch (MyLibrary.GridQuotationMarks)
            {
                case "\"":
                    {
                        mnuResultCopyQuotingWithDoubleQuoting.Checked = true;
                        break;
                    }
                case "'":
                    {
                        mnuResultCopyQuotingWithSingleQuoting.Checked = true;
                        break;
                    }
                default:
                    {
                        mnuResultCopyQuotingWithNone.Checked = true;
                        break;
                    }
            }

            mnuResultCopyQuotingWith.Tag = MyLibrary.GridQuotationMarks;

            switch (MyLibrary.GridFieldSeparator)
            {
                case ";":
                    {
                        mnuResultCopyFieldSeparatorSemicolon.Checked = true;
                        break;
                    }
                case "|":
                    {
                        mnuResultCopyFieldSeparatorI.Checked = true;
                        break;
                    }
                default:
                    {
                        mnuResultCopyFieldSeparatorComma.Checked = true;
                        break;
                    }
            }

            mnuResultCopyFieldSeparator.Tag = MyLibrary.GridFieldSeparator;
        }

        private void InitializeFormLoadFindAutoReplaceAndGrid()
        {
            using (TraceLogger.Time("Initial DragDrop File"))
            {
                InitDragDropFile();
            }

            c1TrueDBGrid1.MouseWheel += c1TrueDBGrid1_MouseWheel;
            c1TrueDBGrid1.KeyDown += Detect_KeyDown;

            _rowHeight = c1TrueDBGrid1.RowHeight;
            _recSelWidth = c1TrueDBGrid1.RecordSelectorWidth;
            _fontSize = MyLibrary.GridFontSize;

            chkShowFilterRow.Checked = MyLibrary.GridShowFilterRow;
            chkShowColumnType.Checked = MyLibrary.GridShowColumnDataType;
            chkShowColumnComments.Checked = MyLibrary.GridShowColumnComment;
            chkRawDataMode.Checked = MyLibrary.GridRawDataMode;
            chkShowGroupingRow.Checked = MyLibrary.GridShowGroupingRow;
            c1TrueDBGrid1.FilterBar = chkShowFilterRow.Checked;
            chkSize.Checked = MyLibrary.GridResize;

            c1DockingTab1.Focus();
            editor.Focus();

            txtIndentWord.ContextMenuStrip = new ContextMenuStrip();

            using (TraceLogger.Time("Load Find List / Replace List"))
            {
                LoadFindList("Grid", cboFindGrid);
                LoadReplaceList();
            }

            InitializeAutoReplaceOnFormLoad();

            c1TrueDBGrid1.AllowFilter = false;
            c1TrueDBGrid1.Filter += C1TrueDBGrid_Filter;

            using (TraceLogger.Time("Set Grid Style for Query Grid"))
            {
                ApplyQueryResultGridVisualStyle(c1TrueDBGrid1);
                GridHelper.SetGridVisualStyle(c1TrueDBGrid1, MyLibrary.GridFontSize);
                ApplyFixedGridVisualStyles();
            }

            CreateNullTable();
            c1TrueDBGrid1.DataSource = _dtNullTable;

            using (TraceLogger.Time("Set Grid Style for Columns"))
            {
                GridHelper.SetGridVisualStyle(c1GridColumns);
                GridHelper.SetGridVisualStyle(c1GridColumns, 10);
                c1GridColumns.DataSource = _dtNullTable;
            }

            using (TraceLogger.Time("Set Docking TabControl"))
            {
                SetDockingTabControl();
            }

            txtIndentWord.Text = MyGlobal.TabWidth.ToString();
            editor.TabWidth = MyGlobal.TabWidth;
            editorMessage.TabWidth = MyGlobal.TabWidth;
            editor.IndentationGuides = MyLibrary.ShowIndentGuide ? ScintillaNET.IndentView.LookBoth : ScintillaNET.IndentView.None;

            if (MyGlobal.IsDefaultTabSchemaBrowser)
            {
                c1DockingTab2.SelectedTab = tabSchemaInformation;
                c1GridSchemaBrowser.Focus();
            }
            else if (MyLibrary.EnableAutoReplace)
            {
                c1DockingTab2.SelectedTab = tabAutoReplace;
                c1GridAutoReplaceInfo.Focus();
            }

            editor.Enabled = false;
            c1DockingTab2.Enabled = false;

            UIHelper.SetDockingTabColor
            (
                c1DockingTab2,
                ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveBackColor),
                ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabActiveForeColor),
                ColorTranslator.FromHtml(MyLibrary.ColorOptionsTabInactiveForeColor)
            );
        }

        private void InitializeFormLoadSchemaAndConnectionState()
        {
            var form = new MessageForm();

            try
            {
                if (DatabaseSqlExecutor.dtSchema == null)
                {
                    var message = LocalizationHelper.GetLanguageString("Please wait...", "Global", "Global", "msg", "PleaseWait", "Text");

                    form.Caption = message;

                    if (MyGlobal.IsShowColumnInfo && MyGlobal.ShowColumnInfo != -1)
                    {
                        message = LocalizationHelper.GetLanguageString("Getting schema information (include all column information of Tables) ...", "Global", "Global", "msg", "GetSchemaInfoIncludeColumn", "Text");

                        form.Info = message;
                        form.IsNeedToMovePosition = true;
                        form.StartPosition = FormStartPosition.CenterScreen;
                        form.TopLevel = true;
                        form.Show(this);
                        form.Refresh();
                    }
                }

                if (DatabaseSqlExecutor.dtSchema != null)
                {
                    using (TraceLogger.Time("Update SchemaData, call MyGlobal.UpdateSchemaData()"))
                    {
                        GridHelper.UpdateSchemaData(c1GridSchemaBrowser);
                    }
                }
                else
                {
                    if (MyLibrary.EnableAutoComplete)
                    {
                        using (TraceLogger.Time("Get/Set Auto Complete Menu"))
                        {
                            BuildAutoCompleteMenu();
                        }
                    }
                }

                switch (_currentSourceType)
                {
                    case DataSourceType.PostgreSql:
                        {
                            TransferValueToMainForm($"UpdateDatabaseInfo`{DatabaseSqlExecutor.DatabaseName}");
                            break;
                        }
                    case DataSourceType.SqlServer:
                    case DataSourceType.MySql:
                        {
                            if (string.IsNullOrEmpty(DatabaseSqlExecutor.DatabaseName))
                            {
                                _languageText = LocalizationHelper.GetLanguageString("Please use the USE statement to select a particular database first.", "form", GetType().Name, "msg", "SelectDatabaseFirst", "Text");
                                SetEditorStatusBarInfo(_languageText, Color.DarkRed);
                            }
                            else
                            {
                                TransferValueToMainForm($"UpdateDatabaseInfo`{DatabaseSqlExecutor.DatabaseName}");
                            }

                            break;
                        }
                }

                c1GridAutoReplaceInfo.Cursor = Cursors.Default;
                c1GridTabList.Cursor = Cursors.Default;
                c1GridSqlNavigator.Cursor = Cursors.Default;
                c1GridSchemaBrowser.Cursor = Cursors.Default;

                using (TraceLogger.Time("Set Grid Style for SchemaBrowser"))
                {
                    GridHelper.SetGridVisualStyle(c1GridSchemaBrowser, 10);
                }

                c1GridSchemaBrowser.AllowRowSizing = RowSizingEnum.None;

                ApplyCommitRollbackButtonState(AppConfigHelper.IsNotCommitYet);

                if (!AppConfigHelper.IsNotCommitYet)
                {
                    DisconnectDatabase();
                }

                using (TraceLogger.Time("Auto Resize Grid Column Width"))
                {
                    AutoResizeGridColumnWidth();
                }
            }
            finally
            {
                form.Dispose();
            }
        }

        private void FinalizeFormLoadLayoutAndEditorSettings()
        {
            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblSchemaFilterPosition.Left - lblSchemaFilterPosition.Width - 2, 21);

            using (TraceLogger.Time("Auto Resize Grid Column Width for Schema"))
            {
                AutoResizeGridColumnWidthForSchema();
            }

            using (TraceLogger.Time("Auto Resize Grid Column Width for Windows List"))
            {
                ResizeTabListGrid();
            }

            if ((DataTable)c1GridSqlNavigator.DataSource == null)
            {
                tabSqlNavigator.TabVisible = false;
            }
            else
            {
                using (TraceLogger.Time("Auto Resize Grid Column Width for Sql Navigator"))
                {
                    ResizeSqlNavigatorGrid();
                }
            }

            using (TraceLogger.Time("ReloadQueryEditorSetting - Apply Ediotr Setting"))
            {
                ReloadQueryEditorSetting();
            }

            tmrCheckIdleTime.Enabled = MyGlobal.ShowColumnInfo == -1;
        }
    }
}
