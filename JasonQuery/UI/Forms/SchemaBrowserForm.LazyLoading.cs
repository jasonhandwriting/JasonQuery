using JasonQuery.Core.Config;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Schema;
using JasonQuery.Core.SchemaExplorer.LazyLoading;
using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.UI.Helpers;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private readonly SchemaBrowserLazyLoadState _schemaBrowserLazyLoadState = new SchemaBrowserLazyLoadState();
        private SchemaExplorerSelectionInfo _currentSchemaBrowserSelection;
        private SchemaBrowserLazyTab _schemaBrowserLazyRequestedTab;
        private bool _isRestoringSchemaBrowserTab;

        private void ConfigureSchemaBrowserLazySelection(SchemaExplorerSelectionInfo selection)
        {
            _currentSchemaBrowserSelection = selection;

            var isTable = string.Equals(selection.SchemaType, SchemaObjectNames.Tables, StringComparison.Ordinal);
            var isView = string.Equals(selection.SchemaType, SchemaObjectNames.Views, StringComparison.Ordinal);

            tabTableStructure.TabVisible = isTable;
            tabData.TabVisible = isTable;
            tabSettings.TabVisible = isTable;
            tabView100RowsTop.TabVisible = isView;
            tabSqlPreview.TabVisible = false;

            lblTableName01.Text = selection.SchemaName;
            lblTableName02.Text = selection.SchemaName;
            tabData.Text = $"{tabData.Tag} - {selection.SchemaName}";

            if (isTable)
            {
                _selectedTableName = selection.SchemaName;
                var filter = GetSetTableFilter(_selectedTableName);
                btnFilterData.Visible = string.IsNullOrEmpty(filter);
                btnFilterRedData.Visible = !string.IsNullOrEmpty(filter);
                btnFilterData.Tag = filter ?? string.Empty;
            }

            editorSqlPane.ReadOnly = false;
            editorSqlPane.Text = string.Empty;
            editorSqlPane.ReadOnly = true;
            c1GridStructure.DataSource = null;
            c1Grid100RowsTop.DataSource = null;
            c1GridData.DataSource = null;
            c1GridColumns.DataSource = null;
            tsData.Enabled = false;

            var key = $"{selection.SourceType}|{selection.SchemaType}|{selection.SchemaDbo}|{selection.SchemaName}|{selection.ObjectId}";

            _schemaBrowserLazyLoadState.Reset(key);
        }

        private SchemaBrowserLazyTab GetSelectedLazyTab()
        {
            var tabName = GetCurrentSchemaBrowserTabName();

            switch (tabName)
            {
                case "tabTableStructure": return SchemaBrowserLazyTab.TableStructure;
                case "tabView100RowsTop": return SchemaBrowserLazyTab.ViewData;
                case "tabData": return SchemaBrowserLazyTab.TableData;
                case "tabSqlPane": return SchemaBrowserLazyTab.SqlPane;
                default: return SchemaBrowserLazyTab.None;
            }
        }

        private void EnsureSelectedSchemaBrowserTabLoaded()
        {
            if (_isRestoringSchemaBrowserTab || _currentSchemaBrowserSelection == null)
            {
                return;
            }

            var tab = GetSelectedLazyTab();

            if (!_schemaBrowserLazyLoadState.TryBegin(tab))
            {
                return;
            }

            MessageForm form = null;

            var context = CreateSchemaBrowserTraceContext(tab);

            try
            {
                Cursor = Cursors.WaitCursor;
                c1GridSchemaBrowser.Cursor = Cursors.WaitCursor;
                form = CreateAndShowSchemaInfoLoadingForm();
                _schemaBrowserLazyRequestedTab = tab;

                using (TraceLogger.Time("SchemaBrowser", "LazyLoad." + tab, context))
                {
                    var canDisplayObject = LoadSchemaBrowserLazyTab(_currentSchemaBrowserSelection);

                    if (!canDisplayObject)
                    {
                        _schemaBrowserLazyLoadState.Fail(tab);
                        return;
                    }
                }

                _schemaBrowserLazyLoadState.Complete(tab);

                //Table Data 的編輯與格式化需要欄位 metadata，因此成功載入 Data 時，Structure 也已完成。
                if (tab == SchemaBrowserLazyTab.TableData)
                {
                    _schemaBrowserLazyLoadState.Complete(SchemaBrowserLazyTab.TableStructure);
                }

                if (tab == SchemaBrowserLazyTab.TableData)
                {
                    ScrollDataGridToLeft();
                }

                ApplyGridNullDisplayStyle();
            }
            catch (Exception ex)
            {
                _schemaBrowserLazyLoadState.Fail(tab);
                TraceLogger.LogError("LazyLoad." + tab, ex, context);

                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _schemaBrowserLazyRequestedTab = SchemaBrowserLazyTab.None;
                DisposeSchemaInfoLoadingForm(form);
                ResetDisplaySchemaInfoUiState();
            }
        }

        private bool LoadSchemaBrowserLazyTab(SchemaExplorerSelectionInfo selection)
        {
            switch (selection.SourceType)
            {
                case DataSourceType.Oracle: return !DisplayInfo_Oracle(selection);
                case DataSourceType.PostgreSql: return !DisplayInfo_PostgreSql(selection);
                case DataSourceType.SqlServer: return !DisplayInfo_SqlServer(selection);
                case DataSourceType.MySql: return !DisplayInfo_MySql(selection);
                default: return false;
            }
        }

        private TraceLogContext CreateSchemaBrowserTraceContext(SchemaBrowserLazyTab tab)
        {
            return new TraceLogContext
            {
                Category = "SchemaBrowser",
                ObjectType = _currentSchemaBrowserSelection?.SchemaType,
                ObjectName = _currentSchemaBrowserSelection?.SchemaName,
                RequestedRows = tab == SchemaBrowserLazyTab.TableData ? 500 :
                                tab == SchemaBrowserLazyTab.ViewData ? 100 : (int?)null,
                Message = "Tab=" + tab
            };
        }

        private bool IsLazyTab(SchemaBrowserLazyTab tab)
        {
            return _schemaBrowserLazyRequestedTab == tab;
        }

        private void PrepareLoadedTableDataForEditing()
        {
            using (TraceLogger.Time("SchemaBrowser", "Grid.PrepareEditing", CreateSchemaBrowserTraceContext(SchemaBrowserLazyTab.TableData)))
            {
                tsData.Enabled = true;
                CheckButtonsStatus();
                c1SuperTooltip1.Hide();
                cboFind.Enabled = c1GridData.HasDataTableRows();

                var dtColumns = new DataTable();

                dtColumns.Columns.Add("ColumnName");

                foreach (DataRow dr in _dtRawSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var row = dtColumns.NewRow();

                    row["ColumnName"] = dr.GetSafeString("ColumnName");
                    dtColumns.Rows.Add(row);
                }

                c1GridColumns.DataSource = dtColumns;
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridColumns, GetType().Name, true, "gridheader");
                GridHelper.ResizeGridColumnWidth(c1GridColumns);
                lblColumnNamePosition.Text = LocalizationHelper.GetLanguageString("Column Name", "form", GetType().Name, "gridheader", "ColumnName", "Text");
                lblColumnNamePosition.Font = c1GridColumns.HeadingStyle.Font;
                btnHelp_ColumnName.Location = new Point(lblColumnNamePosition.Left + lblColumnNamePosition.Width, btnHelp_ColumnName.Top);
                AutoResizeGridColumnWidthForColumns();
            }
        }
    }
}
