using C1.Win.C1Themes;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonLibrary.Core.Events;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AlignHorzEnum = C1.Win.C1TrueDBGrid.AlignHorzEnum;

namespace JasonQuery.UI.Forms
{
    public partial class OracleCreateTableWizardForm : Form
    {
        //20240409 判斷並標記需要變更 BackColor 的儲存格
        private List<Point> _modifiedCellPoints = new List<Point>();

        //20241017 將 CreatTable 改為顯示在 MainForm 裡面，故新增以下事件，與 MainForm 連動
        public event ValueUpdatedEventHandler ValueUpdated;

        //20240418 欄位名稱最大長度值
        private int _identifierLengthLimit = 30;

        private ContextMenuStrip _sqlPreviewMenu = new ContextMenuStrip(); //Editor's menu
        private bool _isFormLoadFinished = false; //Form_Load 是否已結束

        private DataTable _dtColumnData;
        private DataTable _dtIndexesData;
        private DataTable _dtIndexExpressionsData;
        private DataTable _dtPrimaryKeyData;
        private DataTable _dtPrimaryKeyConstraintsData;
        private DataTable _dtUniqueData;
        private DataTable _dtUniqueConstraintsData;
        private DataTable _dtForeignKeyData;
        private DataTable _dtForeignKeyThisTableData;
        private DataTable _dtForeignKeyReferencedTableData;
        private DataTable _dtCheckData;

        //檢查名稱是否重複
        private int _duplicateColumnNameRowIndex = -1;
        private int _duplicateIndexNameRowIndex = -1;
        private int _duplicateUniqueNameRowIndex = -1;
        private int _duplicateForeignKeyNameRowIndex = -1;
        private int _duplicateCheckNameRowIndex = -1;

        private Dictionary<string, string> _onDeleteDisplayValues;
        private string _defaultOnDeleteText;
        private Dictionary<string, string> _deferrableDisplayValues;
        private string _defaultDeferrableText;
        private Dictionary<string, string> _indexTypeDisplayValues;
        private string _defaultIndexTypeText;
        private string _notSpecifiedText;

        private bool _isForeignKeySelectionSyncing = false; //避免 event 重複(交互)觸發

        //根據語系，記住所有勾選欄位的相關訊息
        private int _primaryKeyColumnWidth = -1;
        private int _notNullColumnWidth = -1;
        private int _visibleColumnWidth = -1;
        private int _checkedColumnWidth = -1;

        //20240408 編輯模式相關變數
        private int _currentEditRowIndex = -1;
        private int _currentEditColumnIndex = -1;
        private bool _isEditMode = false;

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public OracleCreateTableWizardForm()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                //20240413 按鈕圖示會出現幾個黑點，故統一在 Form Load 載入原始圖示
                btnColumnAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnColumnRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");
                btnIndexesAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnIndexesRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");
                btnPrimaryKeyAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnPrimaryKeyRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");
                btnUniqueAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnUniqueRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");
                btnForeignKeyAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnForeignKeyRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");
                btnCheckAdd.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Add 16x16.ico");
                btnCheckRemove.Image = IconManager.GetImage(MyGlobal.IconLibrary, "Column Remove 16x16.ico");

                pnlIndexes.Size = new Size(pnlIndexes.Width, 171);
                pnlIndexExpressions.Size = new Size(pnlIndexExpressions.Width, tabCreateTable.Height - pnlIndexes.Height - 37);
                pnlPrimaryKey.Size = new Size(pnlPrimaryKey.Width, 171);
                pnlPrimaryKeyConstraints.Size = new Size(pnlPrimaryKeyConstraints.Width, tabCreateTable.Height - pnlPrimaryKey.Height - 37);
                pnlUnique.Size = new Size(pnlUnique.Width, 171);
                pnlUniqueExpressions.Size = new Size(pnlUniqueExpressions.Width, tabCreateTable.Height - pnlUnique.Height - 37);
                pnlForeignKey.Size = new Size(pnlForeignKey.Width, 171);
                pnlForeignKeyThisTable.Size = new Size(pnlForeignKeyThisTable.Width, tabCreateTable.Height - pnlForeignKey.Height - 37);
                pnlForeignKeyReferencedTable.Size = new Size(pnlForeignKeyReferencedTable.Width, 100);
                pnlForeignKeyReferencedTable2.Size = new Size(pnlForeignKeyReferencedTable2.Width, tabCreateTable.Height - pnlForeignKey.Height - pnlForeignKeyReferencedTable.Height - 36);

                LocalizationHelper.ApplyLanguageInfo(this);

                lblColumns2.Text = lblColumns.Text;
                chkDoubleQuotes.Location = new Point(lblColumns2.Left + lblColumns2.Width + 5, chkDoubleQuotes.Top);
                chkDoubleQuotes.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                _sqlPreviewMenu = new ContextMenuStrip();

                var languageText = LocalizationHelper.GetLanguageString("Refresh", "form", GetType().Name, "menu_sqlpreview", "Refresh", "Text");

                _sqlPreviewMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_sqlPreviewMenu.Items[SqlPreviewColumn.Refresh]).ShortcutKeys = Keys.F5;

                _sqlPreviewMenu.Items[SqlPreviewColumn.Refresh].Click += delegate
                {
                    RefreshSqlPreview();
                };

                _sqlPreviewMenu.Items[SqlPreviewColumn.Refresh].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Refresh 16x16.ico");

                _sqlPreviewMenu.Items.Add("-");

                languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menu_sqlpreview", "SelectAll", "Text");
                _sqlPreviewMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_sqlPreviewMenu.Items[SqlPreviewColumn.SelectAll]).ShortcutKeys = Keys.Control | Keys.A;

                _sqlPreviewMenu.Items[SqlPreviewColumn.SelectAll].Click += delegate
                {
                    SelectAllSqlPreview();
                };

                _sqlPreviewMenu.Items[SqlPreviewColumn.SelectAll].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

                languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menu_sqlpreview", "Copy", "Text");
                _sqlPreviewMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_sqlPreviewMenu.Items[SqlPreviewColumn.Copy]).ShortcutKeys = Keys.Control | Keys.C;

                _sqlPreviewMenu.Items[SqlPreviewColumn.Copy].Click += delegate
                {
                    CopySqlPreview();
                };

                _sqlPreviewMenu.Items[SqlPreviewColumn.Copy].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy 16x16.ico");

                languageText = LocalizationHelper.GetLanguageString("Save As", "form", GetType().Name, "menu_sqlpreview", "SaveAs", "Text");
                _sqlPreviewMenu.Items.Add(languageText);
                ((ToolStripMenuItem)_sqlPreviewMenu.Items[SqlPreviewColumn.SaveAs]).ShortcutKeys = Keys.F12;

                _sqlPreviewMenu.Items[SqlPreviewColumn.SaveAs].Click += delegate
                {
                    SaveAsSqlPreview();
                };

                _sqlPreviewMenu.Items[SqlPreviewColumn.SaveAs].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Save As 16x16.ico");

                _onDeleteDisplayValues = new Dictionary<string, string>();
                languageText = LocalizationHelper.GetLanguageString("No Action", "form", GetType().Name, "dropdownlist", "NoAction", "Text");
                _onDeleteDisplayValues.Add("NoAction", languageText);
                _defaultOnDeleteText = languageText;
                languageText = LocalizationHelper.GetLanguageString("Cascade", "form", GetType().Name, "dropdownlist", "Cascade", "Text");
                _onDeleteDisplayValues.Add("Cascade", languageText);
                languageText = LocalizationHelper.GetLanguageString("Set Null", "form", GetType().Name, "dropdownlist", "SetNull", "Text");
                _onDeleteDisplayValues.Add("SetNull", languageText);

                _deferrableDisplayValues = new Dictionary<string, string>();
                languageText = LocalizationHelper.GetLanguageString("Not Deferrable", "form", GetType().Name, "dropdownlist", "NotDeferrable", "Text");
                _deferrableDisplayValues.Add("NotDeferrable", languageText);
                _defaultDeferrableText = languageText;
                languageText = LocalizationHelper.GetLanguageString("Initially Immediate", "form", GetType().Name, "dropdownlist", "InitiallyImmediate", "Text");
                _deferrableDisplayValues.Add("InitiallyImmediate", languageText);
                languageText = LocalizationHelper.GetLanguageString("Initially Deferred", "form", GetType().Name, "dropdownlist", "InitiallyDeferred", "Text");
                _deferrableDisplayValues.Add("InitiallyDeferred", languageText);

                _indexTypeDisplayValues = new Dictionary<string, string>();
                languageText = LocalizationHelper.GetLanguageString("Non-Unique", "form", GetType().Name, "dropdownlist", "Non-Unique", "Text");
                _indexTypeDisplayValues.Add("Non-Unique", languageText);
                _defaultIndexTypeText = languageText;
                languageText = LocalizationHelper.GetLanguageString("Unique", "form", GetType().Name, "dropdownlist", "Unique", "Text");
                _indexTypeDisplayValues.Add("Unique", languageText);
                languageText = LocalizationHelper.GetLanguageString("Bitmap", "form", GetType().Name, "dropdownlist", "Bitmap", "Text");
                _indexTypeDisplayValues.Add("Bitmap", languageText);

                _notSpecifiedText = LocalizationHelper.GetLanguageString("<Not Specified>", "form", GetType().Name, "dropdownlist", "NotSpecified", "Text");

                if (MyLibrary.IsDarkMode)
                {
                    C1ThemeController.ApplicationTheme = "VS2013Dark";
                }

                tsColumns.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsIndexes.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsIndexExpressions.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsPrimaryKey.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsPrimaryKeyConstraints.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsUnique.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsUniqueConstraints.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsCheck.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsSqlPreview.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsForeignKey.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsForeignKeyThisTable.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);
                tsForeignKeyReferencedTable.BackColor = ColorTranslator.FromHtml(MyLibrary.ColorToolstripBackground);

                txtTableName.Text = "NEWTABLE";
                cboSchema.Location = new Point(lblSchema.Left + lblSchema.Width, cboSchema.Top);
                txtTableName.Location = new Point(lblTableName.Left + lblTableName.Width, txtTableName.Top);
                cboTableType.Location = new Point(lblTableType.Left + lblTableType.Width, cboTableType.Top);
                txtTableComment.Location = new Point(lblTableComment.Left + lblTableComment.Width, txtTableComment.Top);
                cboForeignKeySchema.Location = new Point(lblForeignKeySchema.Left + lblForeignKeySchema.Width, cboForeignKeySchema.Top);
                cboForeignKeyTable.Location = new Point(lblForeignKeyTable.Left + lblForeignKeyTable.Width, cboForeignKeyTable.Top);

                ApplyEditorSetting();

                CreateColumnData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridColumn, Name, true, "gridheader_column");
                ResizeColumnWidth("COLUMN");

                c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.DataType].FetchStyle = true;

                CreateIndexesData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridIndexes, Name, true, "gridheader_index");
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridIndexExpressions, Name, true, "gridheader_indexexpression");
                ResizeColumnWidth("INDEX");
                ResizeColumnWidth("INDEXEXPRESSION");

                CreatePrimaryKeyData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridPrimaryKey, Name, true, "gridheader_primarykey");
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridPrimaryKeyConstraints, Name, true, "gridheader_primarykeyconstraint");
                ResizeColumnWidth("PRIMARYKEY");
                ResizeColumnWidth("PRIMARYKEYCONSTRAINT");

                CreateUniqueData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridUnique, Name, true, "gridheader_unique");
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridUniqueConstraints, Name, true, "gridheader_uniqueconstraint");
                ResizeColumnWidth("UNIQUE");
                ResizeColumnWidth("UNIQUECONSTRAINT");

                CreateForeignKeyData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridForeignKey, Name, true, "gridheader_foreignkey");
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridForeignKeyThisTable, Name, true, "gridheader_foreignkeythistable");
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridForeignKeyReferencedTable, Name, true, "gridheader_foreignkeyreferencedtable");
                ResizeColumnWidth("FOREIGNKEY");
                ResizeColumnWidth("FOREIGNKEYTHISTABLE");
                ResizeColumnWidth("FOREIGNKEYREFERENCEDTABLE");
                c1GridForeignKeyReferencedTable.Tag = string.Empty;

                CreateCheckData();
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridCheck, Name, true, "gridheader_check");
                ResizeColumnWidth("CHECK");

                var sbSql = new StringBuilder();

                SqlTraceHelper.AppendHeader(sbSql, "---Get all Schema");

                sbSql.AppendLine("SELECT DISTINCT UPPER(Owner) AS Schema");
                sbSql.AppendLine("  FROM All_Objects");
                sbSql.AppendLine(" WHERE OBJECT_TYPE = 'TABLE'");
                sbSql.Append(" ORDER BY UPPER(Owner)");

                var sql = sbSql.ToString();
                DataTable dtTemp = null;

                DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtTemp);

                UIHelper.SetC1ComboBoxItemsFromDataTable(cboSchema, dtTemp);
                UIHelper.SetC1ComboBoxItemsFromDataTable(cboForeignKeySchema, dtTemp);

                cboSchema.Text = DatabaseSqlExecutor.DbUserUppercase;
                cboForeignKeySchema.Text = DatabaseSqlExecutor.DbUserUppercase;

                GridHelper.SetGridVisualStyle(c1GridColumn);
                GridHelper.SetGridVisualStyle(c1GridColumn, 10);
                GridHelper.SetGridVisualStyle(c1GridIndexes);
                GridHelper.SetGridVisualStyle(c1GridIndexes, 10);
                GridHelper.SetGridVisualStyle(c1GridIndexExpressions);
                GridHelper.SetGridVisualStyle(c1GridIndexExpressions, 10);
                GridHelper.SetGridVisualStyle(c1GridPrimaryKey);
                GridHelper.SetGridVisualStyle(c1GridPrimaryKey, 10);
                GridHelper.SetGridVisualStyle(c1GridPrimaryKeyConstraints);
                GridHelper.SetGridVisualStyle(c1GridPrimaryKeyConstraints, 10);
                GridHelper.SetGridVisualStyle(c1GridUnique);
                GridHelper.SetGridVisualStyle(c1GridUnique, 10);
                GridHelper.SetGridVisualStyle(c1GridUniqueConstraints);
                GridHelper.SetGridVisualStyle(c1GridUniqueConstraints, 10);
                GridHelper.SetGridVisualStyle(c1GridForeignKey);
                GridHelper.SetGridVisualStyle(c1GridForeignKey, 10);
                GridHelper.SetGridVisualStyle(c1GridForeignKeyThisTable);
                GridHelper.SetGridVisualStyle(c1GridForeignKeyThisTable, 10);
                GridHelper.SetGridVisualStyle(c1GridForeignKeyReferencedTable);
                GridHelper.SetGridVisualStyle(c1GridForeignKeyReferencedTable, 10);
                GridHelper.SetGridVisualStyle(c1GridCheck);
                GridHelper.SetGridVisualStyle(c1GridCheck, 10);

                #region 依資料庫版本，調整欄位名稱的最大長度
                var databaseVersion = DatabaseSqlExecutor.DbServerVersion;
                var parts = databaseVersion.Split('.');

                if (parts.Length > 2)
                {
                    databaseVersion = $"{parts[0]}.{parts[1]}";
                }

                double.TryParse(databaseVersion, out var version);

                if (version >= 12.2)
                {
                    _identifierLengthLimit = 128;
                    txtTableName.MaxLength = _identifierLengthLimit;
                }
                #endregion
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;

                _isFormLoadFinished = true;
                cboTableType.SelectedIndex = 0;
                txtTableName.Focus();
                txtTableName.SelectionStart = 0;
                txtTableName.SelectionLength = txtTableName.Text.Length;
            }
        }

        private void TransferValueToMainForm(string sValue)
        {
            //使用時機：
            //選定某一個SQL，按下右鍵，傳送至「SQL Editor」
            //使用方式如下範例：
            //uTransferValueToMainForm("TransferSelectSQL`" + "要傳送的 SQL 內容");

            var valueArgs = new ValueUpdatedEventArgs(sValue);

            ValueUpdated(this, valueArgs);
        }

        private void ResizeColumnWidth(string gridName)
        {
            var columnIndex = 0;
            var width = 9;

            switch (gridName)
            {
                case "COLUMN":
                    {
                        foreach (C1DisplayColumn col in c1GridColumn.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == ColumnColumn.PrimaryKey)
                            {
                                if (_primaryKeyColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _primaryKeyColumnWidth = col.Width + width;
                                }

                                col.Width = _primaryKeyColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else if (columnIndex == ColumnColumn.NotNull)
                            {
                                if (_notNullColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _notNullColumnWidth = col.Width + width;
                                }

                                col.Width = _notNullColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else if (columnIndex == ColumnColumn.Visible)
                            {
                                if (_visibleColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _visibleColumnWidth = col.Width + width;
                                }

                                col.Width = _visibleColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else
                            {
                                col.AutoSize();

                                if (columnIndex == ColumnColumn.ColumnName && col.Width < 200)
                                {
                                    col.Width = 200;
                                }
                                else if (columnIndex == ColumnColumn.DataType)
                                {
                                    col.Width += 20;
                                }
                                else if (columnIndex == ColumnColumn.Size && col.Width < 60)
                                {
                                    col.Width = 60;
                                }
                                else if (columnIndex == ColumnColumn.Precision && col.Width < 75)
                                {
                                    col.Width = 75;
                                }
                                else if (columnIndex == ColumnColumn.Scale && col.Width < 60)
                                {
                                    col.Width = 60;
                                }
                                else if (columnIndex == ColumnColumn.Default && col.Width < 80)
                                {
                                    col.Width = 80;
                                }
                                else if (columnIndex == ColumnColumn.Comment && col.Width < 180)
                                {
                                    col.Width = 180;
                                }
                                else if (columnIndex == ColumnColumn.ColumnPid)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                                else if (columnIndex == ColumnColumn.ColumnOrder)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "INDEX":
                    {
                        foreach (C1DisplayColumn col in c1GridIndexes.Splits[0].DisplayColumns)
                        {
                            col.AutoSize();

                            if (columnIndex == IndexesColumn.IndexesName && col.Width < 200)
                            {
                                col.Width = 200;
                            }
                            else if (columnIndex == IndexesColumn.IndexesType)
                            {
                                col.Width += 40;
                            }
                            else if (columnIndex == IndexesColumn.IndexesPid)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "INDEXEXPRESSION":
                    {
                        foreach (C1DisplayColumn col in c1GridIndexExpressions.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == IndexExpressionsColumn.Checked && col.Width < 38)
                            {
                                if (_checkedColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _checkedColumnWidth = col.Width + width;
                                }

                                col.Width = _checkedColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else
                            {
                                col.AutoSize();

                                if (columnIndex == IndexExpressionsColumn.ColumnName)
                                {
                                    col.Width += 30;
                                    col.Locked = true; //欄位名稱繼承自 Column 定義
                                }
                                else if (columnIndex == IndexExpressionsColumn.OrderBy && col.Width < 130)
                                {
                                    col.Width = 130;
                                }
                                else if (columnIndex == IndexExpressionsColumn.IndexesPid)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                                else if (columnIndex == IndexExpressionsColumn.ColumnPid)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                                else if (columnIndex == IndexExpressionsColumn.ColumnOrder)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "PRIMARYKEY":
                    {
                        foreach (C1DisplayColumn col in c1GridPrimaryKey.Splits[0].DisplayColumns)
                        {
                            col.AutoSize();

                            if (columnIndex == PrimaryKeyColumn.PrimaryKeyName && col.Width < 200)
                            {
                                col.Width = 200;
                            }
                            else if (columnIndex == PrimaryKeyColumn.Enabled && col.Width < 60)
                            {
                                col.Width = 60;
                            }
                            else if (columnIndex == PrimaryKeyColumn.Validate && col.Width < 75)
                            {
                                col.Width = 75;
                            }
                            else if (columnIndex == PrimaryKeyColumn.PrimaryKeyPid)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }
                            else if (columnIndex == PrimaryKeyColumn.DeferrableState)
                            {
                                col.Width += 80;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "PRIMARYKEYCONSTRAINT":
                    {
                        foreach (C1DisplayColumn col in c1GridPrimaryKeyConstraints.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == PrimaryKeyConstraintsColumn.Checked)
                            {
                                if (_checkedColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _checkedColumnWidth = col.Width + width;
                                }

                                col.Width = _checkedColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else if (columnIndex == PrimaryKeyConstraintsColumn.ColumnName)
                            {
                                col.AutoSize();
                                col.Width += 30;
                                col.Locked = true; //欄位名稱繼承自 Column 定義
                            }
                            else if (columnIndex == PrimaryKeyConstraintsColumn.PrimaryKeyPid || columnIndex == PrimaryKeyConstraintsColumn.ColumnPid || columnIndex == PrimaryKeyConstraintsColumn.ColumnOrder)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "UNIQUE":
                    {
                        foreach (C1DisplayColumn col in c1GridUnique.Splits[0].DisplayColumns)
                        {
                            col.AutoSize();

                            if (columnIndex == UniqueColumn.UniqueName && col.Width < 200)
                            {
                                col.Width = 200;
                            }
                            else if (columnIndex == UniqueColumn.UniquePid)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }
                            else if (columnIndex == UniqueColumn.Enabled && col.Width < 60)
                            {
                                col.Width = 60;
                            }
                            else if (columnIndex == UniqueColumn.Validate && col.Width < 75)
                            {
                                col.Width = 75;
                            }
                            else if (columnIndex == UniqueColumn.DeferrableState)
                            {
                                col.Width += 80;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "UNIQUECONSTRAINT":
                    {
                        foreach (C1DisplayColumn col in c1GridUniqueConstraints.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == UniqueConstraintsColumn.Checked)
                            {
                                if (_checkedColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _checkedColumnWidth = col.Width + width;
                                }

                                col.Width = _checkedColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else if (columnIndex == UniqueConstraintsColumn.ColumnName)
                            {
                                col.AutoSize();
                                col.Width += 30;
                                col.Locked = true; //欄位名稱繼承自 Column 定義
                            }
                            else if (columnIndex == UniqueConstraintsColumn.UniquePid || columnIndex == UniqueConstraintsColumn.ColumnPid || columnIndex == UniqueConstraintsColumn.ColumnOrder)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "FOREIGNKEY":
                    {
                        foreach (C1DisplayColumn col in c1GridForeignKey.Splits[0].DisplayColumns)
                        {
                            col.AutoSize();

                            if (columnIndex == ForeignKeyColumn.ForeignKeyName && col.Width < 200)
                            {
                                col.Width = 200;
                            }
                            else if (columnIndex == ForeignKeyColumn.ForeignKeyPid || columnIndex == ForeignKeyColumn.ForeignKeyReferencedSchema || columnIndex == ForeignKeyColumn.ForeignKeyReferencedTable)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }
                            else if (columnIndex == ForeignKeyColumn.Enabled && col.Width < 60)
                            {
                                col.Width = 60;
                            }
                            else if (columnIndex == ForeignKeyColumn.Validate && col.Width < 75)
                            {
                                col.Width = 75;
                            }
                            else if (columnIndex == ForeignKeyColumn.DeferrableState)
                            {
                                col.Width += 80;
                            }
                            else if (columnIndex == ForeignKeyColumn.OnDelete)
                            {
                                col.Width += 100;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "FOREIGNKEYTHISTABLE":
                    {
                        foreach (C1DisplayColumn col in c1GridForeignKeyThisTable.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == ForeignKeyThisTableColumn.Checked)
                            {
                                if (_checkedColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _checkedColumnWidth = col.Width + width;
                                }

                                col.Width = _checkedColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else if (columnIndex == ForeignKeyThisTableColumn.ColumnName)
                            {
                                col.AutoSize();
                                col.Width += 30;
                                col.Locked = true; //欄位名稱繼承自 Column 定義
                            }
                            else if (columnIndex == ForeignKeyThisTableColumn.ForeignKeyPid || columnIndex == ForeignKeyThisTableColumn.ColumnPid || columnIndex == ForeignKeyThisTableColumn.ColumnOrder)
                            {
                                col.AllowSizing = false;
                                col.Visible = false;
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "FOREIGNKEYREFERENCEDTABLE":
                    {
                        foreach (C1DisplayColumn col in c1GridForeignKeyReferencedTable.Splits[0].DisplayColumns)
                        {
                            if (columnIndex == ForeignKeyReferencedTableColumn.Checked)
                            {
                                if (_checkedColumnWidth == -1)
                                {
                                    col.AutoSize();
                                    _checkedColumnWidth = col.Width + width;
                                }

                                col.Width = _checkedColumnWidth; //寬度固定：避免勾選過程中，系統不斷地在自動調整寬度
                                col.HeadingStyle.HorizontalAlignment = AlignHorzEnum.Center;
                            }
                            else
                            {
                                col.AutoSize();

                                if (columnIndex == ForeignKeyReferencedTableColumn.ColumnName)
                                {
                                    col.Width += 30;
                                    col.Locked = true; //欄位名稱繼承自 Column 定義
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.ForeignKeyPid)
                                {
                                    col.AllowSizing = false;
                                    col.Visible = false;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.Id && col.Width < 25)
                                {
                                    col.Width = 25;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.DataType && col.Width < 80)
                                {
                                    col.Width = 80;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.ConstraintInfo && col.Width < 120)
                                {
                                    col.Width = 120;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.Nullable && col.Width < 70)
                                {
                                    col.Width = 70;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.Default && col.Width < 120)
                                {
                                    col.Width = 120;
                                }
                                else if (columnIndex == ForeignKeyReferencedTableColumn.Comment && col.Width < 150)
                                {
                                    col.Width = 150;
                                }
                            }

                            columnIndex++;
                        }

                        break;
                    }
                case "CHECK":
                    {
                        foreach (C1DisplayColumn col in c1GridCheck.Splits[0].DisplayColumns)
                        {
                            col.AutoSize();

                            if (columnIndex == CheckColumn.CheckName && col.Width < 200)
                            {
                                col.Width = 200;
                            }
                            else if (columnIndex == CheckColumn.CheckCondition && col.Width < 300)
                            {
                                col.Width = 300;
                            }
                            else if (columnIndex == CheckColumn.Enabled && col.Width < 60)
                            {
                                col.Width = 60;
                            }
                            else if (columnIndex == CheckColumn.Validate && col.Width < 75)
                            {
                                col.Width = 75;
                            }
                            else if (columnIndex == CheckColumn.DeferrableState)
                            {
                                col.Width += 80;
                            }

                            columnIndex++;
                        }

                        break;
                    }
            }
        }

        private void CreateColumnData()
        {
            _dtColumnData = new DataTable();
            _dtColumnData.Columns.Add("ColumnPID");
            _dtColumnData.Columns.Add("ColumnOrder", typeof(int));
            _dtColumnData.Columns.Add("ColumnName");
            _dtColumnData.Columns.Add("DataType");
            _dtColumnData.Columns.Add("Size", typeof(int));
            _dtColumnData.Columns.Add("Precision", typeof(int));
            _dtColumnData.Columns.Add("Scale", typeof(int));
            _dtColumnData.Columns.Add("PK");
            _dtColumnData.Columns.Add("NotNull");
            _dtColumnData.Columns.Add("Visible");
            _dtColumnData.Columns.Add("Default");
            _dtColumnData.Columns.Add("Comment");

            var nud = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100000,
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Microsoft JhengHei", 9F, FontStyle.Regular, GraphicsUnit.Point, 136)
            };

            c1GridColumn.DataSource = _dtColumnData;

            //20240501 此處不使用 NumericUpDown，因為編輯時，原本的數字並不會是全選狀態，例如原本為 50，輸入 1 會變成 150，待解決此問題再使用 NumericUpDown
            //c1GridColumn.Columns[(int)_eColumn.Size].Editor = nud;
            //c1GridColumn.Columns[(int)_eColumn.Precision].Editor = nud;
            //c1GridColumn.Columns[(int)_eColumn.Scale].Editor = nud;

            var items = c1GridColumn.Columns[ColumnColumn.DataType].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();
            items.Values.Add(new ValueItem("VARCHAR2", "VARCHAR2"));
            items.Values.Add(new ValueItem("CHAR", "CHAR"));
            items.Values.Add(new ValueItem("NUMBER", "NUMBER"));
            items.Values.Add(new ValueItem("INTEGER", "INTEGER"));
            items.Values.Add(new ValueItem("DATE", "DATE"));
            items.Values.Add(new ValueItem("LONG", "LONG"));
            items.Values.Add(new ValueItem("LONG RAW", "LONG RAW"));
            items.Values.Add(new ValueItem("RAW", "RAW"));
            items.Values.Add(new ValueItem("NVARCHAR2", "NVARCHAR2"));
            items.Values.Add(new ValueItem("ROWID", "ROWID"));
            items.Values.Add(new ValueItem("NCHAR", "NCHAR"));
            items.Values.Add(new ValueItem("CLOB", "CLOB"));
            items.Values.Add(new ValueItem("NCLOB", "NCLOB"));
            items.Values.Add(new ValueItem("BLOB", "BLOB"));
            items.Values.Add(new ValueItem("BFILE", "BFILE"));
            items.Values.Add(new ValueItem("FLOAT", "FLOAT"));
            items.Values.Add(new ValueItem("BINARY_DOUBLE", "BINARY_DOUBLE"));
            items.Values.Add(new ValueItem("BINARY_FLOAT", "BINARY_FLOAT"));
            items.Values.Add(new ValueItem("XMLTYPE", "XMLTYPE"));
            items.Values.Add(new ValueItem("TIMESTAMP", "TIMESTAMP"));
            items.Values.Add(new ValueItem("TIMESTAMP WITH TIME ZONE", "TIMESTAMP WITH TIME ZONE"));
            items.Values.Add(new ValueItem("TIMESTAMP WITH LOCAL TIME ZONE", "TIMESTAMP WITH LOCAL TIME ZONE"));
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.DataType].DropDownList = true;

            var chkPK = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridColumn.Columns[ColumnColumn.PrimaryKey].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridColumn.Columns[ColumnColumn.PrimaryKey].Editor = chkPK;
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.PrimaryKey].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkNotNull = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridColumn.Columns[ColumnColumn.NotNull].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridColumn.Columns[ColumnColumn.NotNull].Editor = chkNotNull;
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.NotNull].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkVisible = new C1.Win.C1Input.C1CheckBox
            {
                Checked = true,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridColumn.Columns[ColumnColumn.Visible].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridColumn.Columns[ColumnColumn.Visible].Editor = chkVisible;
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.Visible].Style.HorizontalAlignment = AlignHorzEnum.Center;

            btnColumnAdd.PerformClick();
        }

        private void CreateIndexesData()
        {
            _dtIndexesData = new DataTable();
            _dtIndexesData.Columns.Add("Indexepid");
            _dtIndexesData.Columns.Add("IndexesName");
            _dtIndexesData.Columns.Add("IndexesType");
            c1GridIndexes.DataSource = _dtIndexesData;

            var items = c1GridIndexes.Columns[IndexesColumn.IndexesType].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _indexTypeDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            c1GridIndexes.Splits[0].DisplayColumns[IndexesColumn.IndexesType].DropDownList = true;

            _dtIndexExpressionsData = new DataTable();
            _dtIndexExpressionsData.Columns.Add("Indexepid");
            _dtIndexExpressionsData.Columns.Add("ColumnPID");
            _dtIndexExpressionsData.Columns.Add("ColumnOrder", typeof(int));
            _dtIndexExpressionsData.Columns.Add("Checked");
            _dtIndexExpressionsData.Columns.Add("ColumnName");
            _dtIndexExpressionsData.Columns.Add("OrderBy");
            c1GridIndexExpressions.DataSource = _dtIndexExpressionsData;

            var chkChecked = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridIndexExpressions.Columns[IndexExpressionsColumn.Checked].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridIndexExpressions.Columns[IndexExpressionsColumn.Checked].Editor = chkChecked;
            c1GridIndexExpressions.Splits[0].DisplayColumns[IndexExpressionsColumn.Checked].Style.HorizontalAlignment = AlignHorzEnum.Center;
        }

        private void CreatePrimaryKeyData()
        {
            _dtPrimaryKeyData = new DataTable();
            _dtPrimaryKeyData.Columns.Add("PrimaryKeyPID");
            _dtPrimaryKeyData.Columns.Add("PrimaryKeyName");
            _dtPrimaryKeyData.Columns.Add("Enabled");
            _dtPrimaryKeyData.Columns.Add("Validate");
            _dtPrimaryKeyData.Columns.Add("DeferrableState");
            c1GridPrimaryKey.DataSource = _dtPrimaryKeyData;

            var chkEnabled = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridPrimaryKey.Columns[PrimaryKeyColumn.Enabled].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridPrimaryKey.Columns[PrimaryKeyColumn.Enabled].Editor = chkEnabled;
            c1GridPrimaryKey.Splits[0].DisplayColumns[PrimaryKeyColumn.Enabled].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkValidate = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridPrimaryKey.Columns[PrimaryKeyColumn.Validate].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridPrimaryKey.Columns[PrimaryKeyColumn.Validate].Editor = chkValidate;
            c1GridPrimaryKey.Splits[0].DisplayColumns[PrimaryKeyColumn.Validate].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var items = c1GridPrimaryKey.Columns[PrimaryKeyColumn.DeferrableState].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _deferrableDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            c1GridPrimaryKey.Splits[0].DisplayColumns[PrimaryKeyColumn.DeferrableState].DropDownList = true;

            _dtPrimaryKeyConstraintsData = new DataTable();
            _dtPrimaryKeyConstraintsData.Columns.Add("PrimaryKeyPID");
            _dtPrimaryKeyConstraintsData.Columns.Add("ColumnPID");
            _dtPrimaryKeyConstraintsData.Columns.Add("ColumnOrder", typeof(int));
            _dtPrimaryKeyConstraintsData.Columns.Add("Checked");
            _dtPrimaryKeyConstraintsData.Columns.Add("ColumnName");
            c1GridPrimaryKeyConstraints.DataSource = _dtPrimaryKeyConstraintsData;

            var chkChecked = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridPrimaryKeyConstraints.Columns[PrimaryKeyConstraintsColumn.Checked].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridPrimaryKeyConstraints.Columns[PrimaryKeyConstraintsColumn.Checked].Editor = chkChecked;
            c1GridPrimaryKeyConstraints.Splits[0].DisplayColumns[PrimaryKeyConstraintsColumn.Checked].Style.HorizontalAlignment = AlignHorzEnum.Center;
        }

        private void CreateUniqueData()
        {
            _dtUniqueData = new DataTable();
            _dtUniqueData.Columns.Add("UniquePID");
            _dtUniqueData.Columns.Add("UniqueName");
            _dtUniqueData.Columns.Add("Enabled");
            _dtUniqueData.Columns.Add("Validate");
            _dtUniqueData.Columns.Add("DeferrableState");
            c1GridUnique.DataSource = _dtUniqueData;

            var chkEnabled = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridUnique.Columns[UniqueColumn.Enabled].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridUnique.Columns[UniqueColumn.Enabled].Editor = chkEnabled;
            c1GridUnique.Splits[0].DisplayColumns[UniqueColumn.Enabled].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkValidate = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridUnique.Columns[UniqueColumn.Validate].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridUnique.Columns[UniqueColumn.Validate].Editor = chkValidate;
            c1GridUnique.Splits[0].DisplayColumns[UniqueColumn.Validate].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var items = c1GridUnique.Columns[UniqueColumn.DeferrableState].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _deferrableDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            c1GridUnique.Splits[0].DisplayColumns[UniqueColumn.DeferrableState].DropDownList = true;

            _dtUniqueConstraintsData = new DataTable();
            _dtUniqueConstraintsData.Columns.Add("UniquePID");
            _dtUniqueConstraintsData.Columns.Add("ColumnPID");
            _dtUniqueConstraintsData.Columns.Add("ColumnOrder", typeof(int));
            _dtUniqueConstraintsData.Columns.Add("Checked");
            _dtUniqueConstraintsData.Columns.Add("ColumnName");
            c1GridUniqueConstraints.DataSource = _dtUniqueConstraintsData;

            var chkChecked = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridUniqueConstraints.Columns[UniqueConstraintsColumn.Checked].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridUniqueConstraints.Columns[UniqueConstraintsColumn.Checked].Editor = chkChecked;
            c1GridUniqueConstraints.Splits[0].DisplayColumns[UniqueConstraintsColumn.Checked].Style.HorizontalAlignment = AlignHorzEnum.Center;
        }

        private void CreateForeignKeyData()
        {
            _dtForeignKeyData = new DataTable();
            _dtForeignKeyData.Columns.Add("ForeignKeyPID");
            _dtForeignKeyData.Columns.Add("ForeignKeyReferencedSchema");
            _dtForeignKeyData.Columns.Add("ForeignKeyReferencedTable");
            _dtForeignKeyData.Columns.Add("ForeignKeyName");
            _dtForeignKeyData.Columns.Add("Enabled");
            _dtForeignKeyData.Columns.Add("Validate");
            _dtForeignKeyData.Columns.Add("DeferrableState");
            _dtForeignKeyData.Columns.Add("OnDelete");
            c1GridForeignKey.DataSource = _dtForeignKeyData;

            var chkEnabled = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridForeignKey.Columns[ForeignKeyColumn.Enabled].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridForeignKey.Columns[ForeignKeyColumn.Enabled].Editor = chkEnabled;
            c1GridForeignKey.Splits[0].DisplayColumns[ForeignKeyColumn.Enabled].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkValidate = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridForeignKey.Columns[ForeignKeyColumn.Validate].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridForeignKey.Columns[ForeignKeyColumn.Validate].Editor = chkValidate;
            c1GridForeignKey.Splits[0].DisplayColumns[ForeignKeyColumn.Validate].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var items = c1GridForeignKey.Columns[ForeignKeyColumn.DeferrableState].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _deferrableDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            items = c1GridForeignKey.Columns[ForeignKeyColumn.OnDelete].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _onDeleteDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            items.MaxComboItems = 4;
            c1GridForeignKey.Splits[0].DisplayColumns[ForeignKeyColumn.OnDelete].DropDownList = true;

            _dtForeignKeyThisTableData = new DataTable();
            _dtForeignKeyThisTableData.Columns.Add("ForeignKeyPID");
            _dtForeignKeyThisTableData.Columns.Add("ColumnPID");
            _dtForeignKeyThisTableData.Columns.Add("ColumnOrder", typeof(int));
            _dtForeignKeyThisTableData.Columns.Add("Checked");
            _dtForeignKeyThisTableData.Columns.Add("ColumnName");
            c1GridForeignKeyThisTable.DataSource = _dtForeignKeyThisTableData;

            var chkChecked = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridForeignKeyThisTable.Columns[ForeignKeyThisTableColumn.Checked].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridForeignKeyThisTable.Columns[ForeignKeyThisTableColumn.Checked].Editor = chkChecked;
            c1GridForeignKeyThisTable.Splits[0].DisplayColumns[ForeignKeyThisTableColumn.Checked].Style.HorizontalAlignment = AlignHorzEnum.Center;

            _dtForeignKeyReferencedTableData = new DataTable();
            _dtForeignKeyReferencedTableData.Columns.Add("ForeignKeyPID");
            _dtForeignKeyReferencedTableData.Columns.Add("Checked");
            _dtForeignKeyReferencedTableData.Columns.Add("ColumnName");
            _dtForeignKeyReferencedTableData.Columns.Add("ID", typeof(int));
            _dtForeignKeyReferencedTableData.Columns.Add("DataType"); //包含長度，ex.Varchar2(50), Numeric(5, 3)
            _dtForeignKeyReferencedTableData.Columns.Add("ConstraintInfo");
            _dtForeignKeyReferencedTableData.Columns.Add("Nullable");
            _dtForeignKeyReferencedTableData.Columns.Add("Default");
            _dtForeignKeyReferencedTableData.Columns.Add("Comment");
            c1GridForeignKeyReferencedTable.DataSource = _dtForeignKeyReferencedTableData;

            c1GridForeignKeyReferencedTable.Columns[ForeignKeyReferencedTableColumn.Checked].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridForeignKeyReferencedTable.Columns[ForeignKeyReferencedTableColumn.Checked].Editor = chkChecked;
            c1GridForeignKeyReferencedTable.Splits[0].DisplayColumns[ForeignKeyReferencedTableColumn.Checked].Style.HorizontalAlignment = AlignHorzEnum.Center;
        }

        private void CreateCheckData()
        {
            _dtCheckData = new DataTable();
            _dtCheckData.Columns.Add("CheckName");
            _dtCheckData.Columns.Add("CheckCondition");
            _dtCheckData.Columns.Add("Enabled");
            _dtCheckData.Columns.Add("Validate");
            _dtCheckData.Columns.Add("DeferrableState");
            c1GridCheck.DataSource = _dtCheckData;

            var chkEnabled = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridCheck.Columns[CheckColumn.Enabled].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridCheck.Columns[CheckColumn.Enabled].Editor = chkEnabled;
            c1GridCheck.Splits[0].DisplayColumns[CheckColumn.Enabled].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var chkValidate = new C1.Win.C1Input.C1CheckBox
            {
                Checked = false,
                VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue
            };

            c1GridCheck.Columns[CheckColumn.Validate].ValueItems.Presentation = PresentationEnum.CheckBox;
            c1GridCheck.Columns[CheckColumn.Validate].Editor = chkValidate;
            c1GridCheck.Splits[0].DisplayColumns[CheckColumn.Validate].Style.HorizontalAlignment = AlignHorzEnum.Center;

            var items = c1GridCheck.Columns[CheckColumn.DeferrableState].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;
            items.Values.Clear();

            foreach (var item in _deferrableDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            c1GridCheck.Splits[0].DisplayColumns[CheckColumn.DeferrableState].DropDownList = true;
        }

        private void tabCreateTable_SelectedTabChanged(object sender, EventArgs e)
        {
            try
            {
                if (tabCreateTable.SelectedTab == tabSqlPreview)
                {
                    btnRefresh.PerformClick();
                }
                else if (tabCreateTable.SelectedTab == tabColumns) //切換到 Column 頁籤
                {
                    if (_dtPrimaryKeyConstraintsData?.Rows.Count > 0)
                    {
                        //將 PK 頁籤有打勾的欄位，在 Column 頁籤也打勾
                        for (var i = 0; i < _dtPrimaryKeyConstraintsData.Rows.Count; i++)
                        {
                            var dr2 = _dtPrimaryKeyConstraintsData.Rows[i];
                            var checkedValue = dr2.GetSafeString(PrimaryKeyConstraintsColumn.Checked);
                            var columnPid = dr2.GetSafeString(PrimaryKeyConstraintsColumn.ColumnPid);
                            int value = 0;

                            if (string.Equals(checkedValue, "TRUE", StringComparison.OrdinalIgnoreCase) || checkedValue == "1") //20240403 可能是 "TRUE" 也可能是 "1"
                            {
                                value = 1;
                            }

                            var dr = _dtColumnData.Select($"ColumnPID = '{columnPid}'");

                            for (var j = 0; j < dr.Length; j++)
                            {
                                dr[j][ColumnColumn.PrimaryKey] = value;
                            }
                        }
                    }
                }
                else if (tabCreateTable.SelectedTab == tabPrimaryKeyConstraints) //切換到 PK 頁籤
                {
                    var isPrimaryKeyChecked = false; //是否有任何一個欄位有勾選 PK
                    var isUpdatePrimaryKeyChecked = false; //是否需要更新 PK 勾選狀態

                    for (var i = 0; i < _dtColumnData.Rows.Count; i++)
                    {
                        var primaryKey = _dtColumnData.Rows[i].GetSafeString(ColumnColumn.PrimaryKey);

                        if (string.Equals(primaryKey, "TRUE", StringComparison.OrdinalIgnoreCase) || primaryKey == "1") //20240403 可能是 "TRUE" 也可能是 "1"
                        {
                            isPrimaryKeyChecked = true;
                            break;
                        }
                    }

                    if (!isPrimaryKeyChecked)
                    {
                        if (_dtPrimaryKeyData?.Rows.Count > 0)
                        {
                            isUpdatePrimaryKeyChecked = true;
                            //_dtPrimaryKeyData.Clear();
                            //_dtPrimaryKeyConstraintsData.Clear();
                        }
                    }
                    else
                    {
                        if (_dtPrimaryKeyConstraintsData?.Rows.Count > 0)
                        {
                            //已存在 PK 設定
                            isUpdatePrimaryKeyChecked = true;
                        }
                        else
                        {
                            //沒有 PK 設定：新建
                            btnPrimaryKeyAdd.PerformClick();
                            isUpdatePrimaryKeyChecked = true;
                        }
                    }

                    if (isUpdatePrimaryKeyChecked)
                    {
                        //更新打勾狀態：將 Column 頁籤有打勾的欄位，在 PK 頁籤也打勾
                        for (var i = 0; i < _dtColumnData.Rows.Count; i++)
                        {
                            var primaryKey = _dtColumnData.Rows[i].GetSafeString(ColumnColumn.PrimaryKey);
                            int value = (string.Equals(primaryKey, "TRUE", StringComparison.OrdinalIgnoreCase) || primaryKey == "1") ? 1 : 0; //20240403 可能是 "TRUE" 也可能是 "1"

                            var dr = _dtPrimaryKeyConstraintsData.Select($"ColumnPID = '{_dtColumnData.Rows[i].GetSafeString(ColumnColumn.ColumnPid)}'");

                            for (var j = 0; j < dr.Length; j++)
                            {
                                dr[j][PrimaryKeyConstraintsColumn.Checked] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ApplyEditorSetting()
        {
            editorSql.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorSql.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorSql.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));
            editorSql.Zoom = Convert.ToInt16(MyLibrary.QueryEditorZoom);

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

            editorSql.Styler = new SqlStyler();
        }

        private void btnColumnAdd_Click(object sender, EventArgs e)
        {
            AddColumn();
        }

        private void AddColumn()
        {
            try
            {
                var columnName = string.Empty;
                var row = NewDataRowColumn();

                _dtColumnData.Rows.Add(row);

                if (_dtIndexExpressionsData?.Rows.Count > 0 || _dtUniqueConstraintsData?.Rows.Count > 0 || _dtPrimaryKeyConstraintsData?.Rows.Count > 0 || _dtForeignKeyThisTableData?.Rows.Count > 0)
                {
                    columnName = row.GetSafeString(ColumnColumn.ColumnName);
                }

                UpdateColumnOrder(columnName, "ADD");
                CheckColumnButtons();

                c1GridColumn.Row = _dtColumnData.Rows.Count - 1;
                c1GridColumn.Col = 0;

                DrawGridBackColor();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <param name="sColumnName4AddInsert">新增或插入的欄位名稱</param>
        private void UpdateColumnOrder(string sColumnName4AddInsert = "", string sMode = "")
        {
            var index = 0;
            var columnOrderForAddInsert = -1;

            for (var i = 0; i < _dtColumnData.Rows.Count; i++)
            {
                var dr2 = _dtColumnData.Rows[i];

                dr2[ColumnColumn.ColumnOrder] = index;
                index++;

                var sColumnOrder = dr2.GetSafeString(ColumnColumn.ColumnOrder);

                int.TryParse(sColumnOrder, out columnOrderForAddInsert);

                //20240330 修正 IndexExpression 的欄位呈現順序
                if (_dtIndexExpressionsData?.Rows.Count > 0)
                {
                    var dr = _dtIndexExpressionsData.Select($"ColumnPID = '{dr2.GetSafeString(ColumnColumn.ColumnPid)}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j][IndexExpressionsColumn.ColumnOrder] = sColumnOrder;
                    }

                    if (!string.IsNullOrEmpty(sColumnName4AddInsert) && dr2.GetSafeString(ColumnColumn.ColumnName) == sColumnName4AddInsert)
                    {
                        //for Index: 每一組都需要新增欄位資訊
                        for (var k = 0; k < _dtIndexesData.Rows.Count; k++)
                        {
                            var row2 = _dtIndexExpressionsData.NewRow();

                            row2[IndexExpressionsColumn.IndexesPid] = _dtIndexesData.Rows[k].GetSafeString(IndexesColumn.IndexesPid);
                            row2[IndexExpressionsColumn.ColumnPid] = dr2.GetSafeString(ColumnColumn.ColumnPid);
                            row2[IndexExpressionsColumn.ColumnOrder] = columnOrderForAddInsert;
                            row2[IndexExpressionsColumn.Checked] = 0; //預設不勾
                            row2[IndexExpressionsColumn.ColumnName] = dr2.GetSafeString(ColumnColumn.ColumnName);
                            row2[IndexExpressionsColumn.OrderBy] = _notSpecifiedText;
                            _dtIndexExpressionsData.Rows.Add(row2);
                        }
                    }
                }

                //20240402 修正 PrimaryKey Constraints 的欄位呈現順序
                if (_dtPrimaryKeyConstraintsData?.Rows.Count > 0)
                {
                    var pid = dr2.GetSafeString(ColumnColumn.ColumnPid);
                    var dr = _dtPrimaryKeyConstraintsData.Select($"ColumnPID = '{pid}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j][PrimaryKeyConstraintsColumn.ColumnOrder] = sColumnOrder;
                    }

                    if (!string.IsNullOrEmpty(sColumnName4AddInsert) && dr2.GetSafeString(ColumnColumn.ColumnName) == sColumnName4AddInsert)
                    {
                        //for PrimaryKey: 新增欄位資訊
                        var row = _dtPrimaryKeyConstraintsData.NewRow();

                        row[PrimaryKeyConstraintsColumn.PrimaryKeyPid] = _dtPrimaryKeyData.Rows[0].GetSafeString(PrimaryKeyColumn.PrimaryKeyPid);
                        row[PrimaryKeyConstraintsColumn.ColumnPid] = dr2.GetSafeString(ColumnColumn.ColumnPid);
                        row[PrimaryKeyConstraintsColumn.ColumnOrder] = columnOrderForAddInsert;
                        row[PrimaryKeyConstraintsColumn.Checked] = 0; //預設不勾
                        row[PrimaryKeyConstraintsColumn.ColumnName] = dr2.GetSafeString(ColumnColumn.ColumnName);
                        _dtPrimaryKeyConstraintsData.Rows.Add(row);
                    }
                }

                //20240330 修正 Unique Constraints 的欄位呈現順序
                if (_dtUniqueConstraintsData?.Rows.Count > 0)
                {
                    var dr = _dtUniqueConstraintsData.Select($"ColumnPID = '{dr2.GetSafeString(ColumnColumn.ColumnPid)}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j][UniqueConstraintsColumn.ColumnOrder] = sColumnOrder;
                    }

                    if (!string.IsNullOrEmpty(sColumnName4AddInsert) && dr2.GetSafeString(ColumnColumn.ColumnName) == sColumnName4AddInsert)
                    {
                        //每一組都需要新增欄位資訊
                        for (var k = 0; k < _dtUniqueData.Rows.Count; k++)
                        {
                            var row = _dtUniqueConstraintsData.NewRow();

                            row[UniqueConstraintsColumn.UniquePid] = _dtUniqueData.Rows[k].GetSafeString(UniqueColumn.UniquePid);
                            row[UniqueConstraintsColumn.ColumnPid] = dr2.GetSafeString(ColumnColumn.ColumnPid);
                            row[UniqueConstraintsColumn.ColumnOrder] = columnOrderForAddInsert;
                            row[UniqueConstraintsColumn.Checked] = 0; //預設不勾
                            row[UniqueConstraintsColumn.ColumnName] = dr2.GetSafeString(ColumnColumn.ColumnName);
                            _dtUniqueConstraintsData.Rows.Add(row);
                        }
                    }
                }

                //20240406 修正 ForeignKey This Table 的欄位呈現順序
                if (_dtForeignKeyThisTableData?.Rows.Count > 0)
                {
                    var dr = _dtForeignKeyThisTableData.Select($"ColumnPID = '{dr2.GetSafeString(ColumnColumn.ColumnPid)}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j][ForeignKeyThisTableColumn.ColumnOrder] = sColumnOrder;
                    }

                    if (!string.IsNullOrEmpty(sColumnName4AddInsert) && dr2.GetSafeString(ColumnColumn.ColumnName) == sColumnName4AddInsert)
                    {
                        //每一組都需要新增欄位資訊
                        for (var k = 0; k < _dtForeignKeyData.Rows.Count; k++)
                        {
                            var row = _dtForeignKeyThisTableData.NewRow();

                            row[ForeignKeyThisTableColumn.ForeignKeyPid] = _dtForeignKeyData.Rows[k].GetSafeString(ForeignKeyColumn.ForeignKeyPid);
                            row[ForeignKeyThisTableColumn.ColumnPid] = dr2.GetSafeString(ColumnColumn.ColumnPid);
                            row[ForeignKeyThisTableColumn.ColumnOrder] = columnOrderForAddInsert;
                            row[ForeignKeyThisTableColumn.Checked] = 0; //預設不勾
                            row[ForeignKeyThisTableColumn.ColumnName] = dr2.GetSafeString(ColumnColumn.ColumnName);
                            _dtForeignKeyThisTableData.Rows.Add(row);
                        }
                    }
                }
            }

            //for Index: 刪除欄位名稱
            if (_dtIndexExpressionsData != null && !string.IsNullOrEmpty(sColumnName4AddInsert) && sMode == "DEL")
            {
                var drDeleted = _dtIndexExpressionsData.Select($"ColumnName = '{sColumnName4AddInsert}'");

                foreach (var row in drDeleted)
                {
                    row.Delete();
                }

                _dtIndexExpressionsData.AcceptChanges();
            }

            //for Index: 依調整後的欄位順序進行排序
            if (_dtIndexExpressionsData != null)
            {
                var dtView = _dtIndexExpressionsData.DefaultView;

                dtView.Sort = "ColumnOrder ASC";
            }

            //for PrimaryKey: 刪除欄位名稱
            if (_dtPrimaryKeyConstraintsData != null && !string.IsNullOrEmpty(sColumnName4AddInsert) && sMode == "DEL")
            {
                var drDeleted = _dtPrimaryKeyConstraintsData.Select($"ColumnName = '{sColumnName4AddInsert}'");

                foreach (var row in drDeleted)
                {
                    row.Delete();
                }

                _dtPrimaryKeyConstraintsData.AcceptChanges();
            }

            //for PrimaryKey: 依調整後的欄位順序進行排序
            if (_dtPrimaryKeyConstraintsData != null)
            {
                var dtView = _dtPrimaryKeyConstraintsData.DefaultView;

                dtView.Sort = "ColumnOrder ASC";
            }

            //for Unique: 刪除欄位名稱
            if (_dtUniqueConstraintsData != null && !string.IsNullOrEmpty(sColumnName4AddInsert) && sMode == "DEL")
            {
                var drDeleted = _dtUniqueConstraintsData.Select($"ColumnName = '{sColumnName4AddInsert}'");

                foreach (var row in drDeleted)
                {
                    row.Delete();
                }

                _dtUniqueConstraintsData.AcceptChanges();
            }

            //for Unique: 依調整後的欄位順序進行排序
            if (_dtUniqueConstraintsData != null)
            {
                var dtView = _dtUniqueConstraintsData.DefaultView;

                dtView.Sort = "ColumnOrder ASC";
            }

            //for ForeignKey This Table: 刪除欄位名稱
            if (_dtForeignKeyThisTableData != null && !string.IsNullOrEmpty(sColumnName4AddInsert) && sMode == "DEL")
            {
                var drDeleted = _dtForeignKeyThisTableData.Select($"ColumnName = '{sColumnName4AddInsert}'");

                foreach (var row in drDeleted)
                {
                    row.Delete();
                }

                _dtForeignKeyThisTableData.AcceptChanges();
            }

            //for ForeignKey This Table: 依調整後的欄位順序進行排序
            if (_dtForeignKeyThisTableData != null)
            {
                var dtView = _dtForeignKeyThisTableData.DefaultView;

                dtView.Sort = "ColumnOrder ASC";
            }

            //20240406 針對 Column 再排序一次，保持 c1GridColumn 與 _dtColumn 資料一致性
            c1GridColumn.ApplyDataTableSourceSort("ColumnOrder ASC");
        }

        private DataRow NewDataRowColumn()
        {
            var next = GetNextSequence(_dtColumnData, ColumnColumn.ColumnName, "COLUMN");
            var row = _dtColumnData.NewRow();

            row[ColumnColumn.ColumnPid] = $"{DateTime.Now:yyyyMMddHHmmssfff}";
            row[ColumnColumn.ColumnOrder] = 0;
            row[ColumnColumn.ColumnName] = $"COLUMN{next}";
            row[ColumnColumn.DataType] = "VARCHAR2"; //預設使用 VARCHAR2, 長度 50
            row[ColumnColumn.Size] = 50;
            row[ColumnColumn.Precision] = 0;
            row[ColumnColumn.Scale] = 0;
            row[ColumnColumn.PrimaryKey] = 0;
            row[ColumnColumn.NotNull] = 0;
            row[ColumnColumn.Visible] = 1;
            return row;
        }

        private void btnColumnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                var columnName = string.Empty;

                if (_dtColumnData?.Rows.Count > 0)
                {
                    if (_dtIndexExpressionsData?.Rows.Count > 0 || _dtUniqueConstraintsData?.Rows.Count > 0)
                    {
                        columnName = _dtColumnData.Rows[c1GridColumn.Row].GetSafeString(ColumnColumn.ColumnName);
                    }

                    c1GridColumn.AllowDelete = true;
                    c1GridColumn.Delete(c1GridColumn.Row);
                    c1GridColumn.AllowDelete = false;
                }

                if (_dtColumnData == null || _dtColumnData.Rows.Count == 0)
                {
                    btnColumnAdd.PerformClick();
                }
                else
                {
                    UpdateColumnOrder(columnName, "DEL");
                    CheckColumnButtons();
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnColumnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                var columnName = string.Empty;
                var currentRow = c1GridColumn.Row;
                var row = NewDataRowColumn();

                _dtColumnData.Rows.InsertAt(row, currentRow);
                c1GridColumn.Row = currentRow;

                if (_dtIndexExpressionsData?.Rows.Count > 0 || _dtUniqueConstraintsData?.Rows.Count > 0)
                {
                    columnName = row.GetSafeString(ColumnColumn.ColumnName);
                }

                UpdateColumnOrder(columnName, "ADD");
                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnColumnTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridColumn.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtColumnData.Rows[currentRow];
                var newRow = _dtColumnData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtColumnData.Rows.Remove(selectedRow);
                _dtColumnData.Rows.InsertAt(newRow, 0);
                UpdateColumnOrder();
                c1GridColumn.Row = 0;
                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnColumnUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridColumn.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtColumnData.Rows[currentRow];
                var newRow = _dtColumnData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtColumnData.Rows.Remove(selectedRow);
                _dtColumnData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                UpdateColumnOrder();
                c1GridColumn.Row = currentRow - 1;

                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnColumnDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridColumn.Row;

                if (currentRow == _dtColumnData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtColumnData.Rows[currentRow];
                var newRow = _dtColumnData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtColumnData.Rows.Remove(selectedRow);
                _dtColumnData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                UpdateColumnOrder();
                c1GridColumn.Row = currentRow + 1;
                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnColumnBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridColumn.Row;

                if (currentRow == _dtColumnData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtColumnData.Rows[currentRow];
                var newRow = _dtColumnData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtColumnData.Rows.Remove(selectedRow);
                _dtColumnData.Rows.InsertAt(newRow, _dtColumnData.Rows.Count);
                UpdateColumnOrder();
                c1GridColumn.Row = _dtColumnData.Rows.Count - 1;
                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridColumn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && c1GridColumn.Row == _dtColumnData.Rows.Count - 1)
            {
                AddColumn();
            }
        }

        private void c1GridColumn_AfterColUpdate(object sender, ColEventArgs e)
        {
            try
            {
                var currentRow = c1GridColumn.Row;
                var currentCol = c1GridColumn.Col;
                var dr = _dtColumnData.Rows[currentRow];
                var dataType = dr.GetSafeString(ColumnColumn.DataType);

                if (currentCol == ColumnColumn.DataType)
                {
                    DrawGridBackColor();

                    //20240412 強制更新 Size/Precision/Scale 3個欄位值
                    c1GridColumn.Row = currentRow;
                    c1GridColumn.Col = ColumnColumn.ColumnName;
                    c1GridColumn.Select(); //先切換到 ColumnName 欄位
                    c1GridColumn.Row = currentRow;
                    c1GridColumn.Col = ColumnColumn.DataType;
                    c1GridColumn.Select(); //再切換回 DataType 欄位 (觸發更新值的動作)
                }
                else if (currentCol == ColumnColumn.Size || currentCol == ColumnColumn.Precision || currentCol == ColumnColumn.Scale)
                {
                    //20240412 預防使用者將數字刪除造成例外錯誤
                    if (string.IsNullOrEmpty(dr.GetSafeString(ColumnColumn.Size)))
                    {
                        dr[ColumnColumn.Size] = 0;
                    }

                    if (string.IsNullOrEmpty(dr.GetSafeString(ColumnColumn.Precision)))
                    {
                        dr[ColumnColumn.Precision] = 0;
                    }

                    if (string.IsNullOrEmpty(dr.GetSafeString(ColumnColumn.Scale)))
                    {
                        dr[ColumnColumn.Scale] = 0;
                    }

                    switch (dataType)
                    {
                        case "CHAR":
                        case "NCHAR":
                        case "RAW": //Size<=2000
                            {
                                if (dr.GetSafeInt(ColumnColumn.Size) == 0)
                                {
                                    dr[ColumnColumn.Size] = 50;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Size) > 2000)
                                {
                                    dr[ColumnColumn.Size] = 2000;
                                }

                                break;
                            }
                        case "NVARCHAR2":
                        case "VARCHAR2": //Size<=32767
                            {
                                if (dr.GetSafeInt(ColumnColumn.Size) == 0)
                                {
                                    dr[ColumnColumn.Size] = 50;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Size) > 32767)
                                {
                                    dr[ColumnColumn.Size] = 32767;
                                }

                                break;
                            }
                        case "FLOAT": //Precision 1~126
                            {
                                if (dr.GetSafeInt(ColumnColumn.Precision) < 1)
                                {
                                    dr[ColumnColumn.Precision] = 1;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Precision) > 126)
                                {
                                    dr[ColumnColumn.Precision] = 126;
                                }

                                break;
                            }
                        case "NUMBER": //Precision 1~38, Scale -84~127
                            {
                                if (dr.GetSafeInt(ColumnColumn.Precision) < 1)
                                {
                                    dr[ColumnColumn.Precision] = 1;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Precision) > 38)
                                {
                                    dr[ColumnColumn.Precision] = 38;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Scale) < -84)
                                {
                                    dr[ColumnColumn.Scale] = -84;
                                }
                                else if (dr.GetSafeInt(ColumnColumn.Scale) > 127)
                                {
                                    dr[ColumnColumn.Scale] = 127;
                                }

                                break;
                            }
                    }
                }
                else if (currentCol == ColumnColumn.ColumnName) //異動 Column Name 欄位
                {
                    var columnNameOld = dr.GetSafeString(ColumnColumn.ColumnName);

                    if (string.IsNullOrEmpty(columnNameOld))
                    {
                        var next = GetNextSequence(_dtColumnData, ColumnColumn.ColumnName, "COLUMN");

                        //欄位名稱被清空了
                        dr[ColumnColumn.ColumnName] = $"COLUMN{next}";
                    }
                    else
                    {
                        for (var i = 0; i < _dtColumnData.Rows.Count; i++)
                        {
                            var columnName = _dtColumnData.Rows[i].GetSafeString(ColumnColumn.ColumnName);

                            if (i != currentRow && string.Equals(columnName, columnNameOld, StringComparison.OrdinalIgnoreCase))
                            {
                                var next = GetNextSequence(_dtColumnData, ColumnColumn.ColumnName, "COLUMN");
                                var columnNameNew = $"COLUMN{next}";

                                dr[ColumnColumn.ColumnName] = columnNameNew;

                                //20240324 記住 row
                                _duplicateColumnNameRowIndex = currentRow;
                            }
                        }
                    }

                    //20240330 修正 IndexExpression 的欄位名稱
                    if (_dtIndexExpressionsData?.Rows.Count > 0)
                    {
                        var dr3 = _dtIndexExpressionsData.Select($"ColumnPID = '{dr.GetSafeString(ColumnColumn.ColumnPid)}'");

                        for (var i = 0; i < dr3.Length; i++)
                        {
                            dr3[i][IndexExpressionsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                        }

                        ResizeColumnWidth("INDEXEXPRESSION");
                    }

                    //20240402 修正 PrimaryKeyConstraint 的欄位名稱
                    if (_dtPrimaryKeyConstraintsData?.Rows.Count > 0)
                    {
                        var dr3 = _dtPrimaryKeyConstraintsData.Select($"ColumnPID = '{dr.GetSafeString(ColumnColumn.ColumnPid)}'");

                        for (var i = 0; i < dr3.Length; i++)
                        {
                            dr3[i][PrimaryKeyConstraintsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                        }

                        ResizeColumnWidth("PRIMARYKEYCONSTRAINT");
                    }

                    //20240331 修正 UniqueConstraint 的欄位名稱
                    if (_dtUniqueConstraintsData?.Rows.Count > 0)
                    {
                        var dr3 = _dtUniqueConstraintsData.Select($"ColumnPID = '{dr.GetSafeString(ColumnColumn.ColumnPid)}'");

                        for (var i = 0; i < dr3.Length; i++)
                        {
                            dr3[i][UniqueConstraintsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                        }

                        ResizeColumnWidth("UNIQUECONSTRAINT");
                    }

                    //20240406 修正 ForeignKey This Table 的欄位名稱
                    if (_dtForeignKeyThisTableData?.Rows.Count > 0)
                    {
                        var dr3 = _dtForeignKeyThisTableData.Select($"ColumnPID = '{dr.GetSafeString(ColumnColumn.ColumnPid)}'");

                        for (var i = 0; i < dr3.Length; i++)
                        {
                            dr3[i][ForeignKeyThisTableColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                        }

                        ResizeColumnWidth("FOREIGNKEYTHISTABLE");
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _isEditMode = false;
                btnColumnAdd.Enabled = !_isEditMode;
                btnColumnRemove.Enabled = !_isEditMode;
                btnColumnInsert.Enabled = !_isEditMode;
                btnColumnTop.Enabled = !_isEditMode;
                btnColumnUp.Enabled = !_isEditMode;
                btnColumnDown.Enabled = !_isEditMode;
                btnColumnBottom.Enabled = !_isEditMode;
            }
        }

        private void c1GridColumn_AfterUpdate(object sender, EventArgs e)
        {
            if (_duplicateColumnNameRowIndex != -1)
            {
                c1GridColumn.Row = _duplicateColumnNameRowIndex;
                c1GridColumn.Col = ColumnColumn.ColumnName;
                _duplicateColumnNameRowIndex = -1;
            }
        }

        private void c1GridColumn_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (_duplicateColumnNameRowIndex != -1)
                {
                    c1GridColumn.Row = _duplicateColumnNameRowIndex;
                    c1GridColumn.Col = ColumnColumn.ColumnName;
                    _duplicateColumnNameRowIndex = -1;
                    c1GridColumn.EditActive = true; //20240407 進入編輯模式
                }

                if (!_isFormLoadFinished)
                {
                    return;
                }

                //根據異動 Column Name 欄位，調整其他欄位是否可以被使用者異動
                UpdateColumnEnabled();

                CheckColumnButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridColumn_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            //20240408 除了勾選欄位，其餘欄位進入編輯模式要特別處理
            switch (c1GridColumn.Col)
            {
                case ColumnColumn.PrimaryKey:
                case ColumnColumn.NotNull:
                case ColumnColumn.Visible:
                    {
                        return;
                    }
                case ColumnColumn.Size:
                case ColumnColumn.Precision:
                case ColumnColumn.Scale: //20240410 此三個欄位目前還沒辦法做到：編輯前將數字全選
                    {
                        c1GridColumn.SelectionStart = 0;
                        c1GridColumn.SelectionLength = 1;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            _currentEditRowIndex = c1GridColumn.Row;
            _currentEditColumnIndex = c1GridColumn.Col;

            _isEditMode = true;
            btnColumnAdd.Enabled = !_isEditMode;
            btnColumnRemove.Enabled = !_isEditMode;
            btnColumnInsert.Enabled = !_isEditMode;
            btnColumnTop.Enabled = !_isEditMode;
            btnColumnUp.Enabled = !_isEditMode;
            btnColumnDown.Enabled = !_isEditMode;
            btnColumnBottom.Enabled = !_isEditMode;
        }

        private void c1GridColumn_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridColumn.RowContaining(e.Y);
                var col = c1GridColumn.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("COLUMN");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridColumn_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("COLUMN");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridColumn_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
        {
            var pointModified = new Point(e.Row, e.Col);

            if (!_modifiedCellPoints.Contains(pointModified))
            {
                return;
            }

            e.Style.ForeColor = ColorTranslator.FromHtml("#F2F2F2"); //非常淡的灰色
            e.Style.BackColor = ColorTranslator.FromHtml("#F2F2F2");
        }

        private void CheckColumnButtons()
        {
            var currentRow = c1GridColumn.Row;

            btnColumnAdd.Enabled = true;
            btnColumnRemove.Enabled = true;
            btnColumnInsert.Enabled = true;

            if (currentRow == 0)
            {
                btnColumnUp.Enabled = false;
                btnColumnTop.Enabled = false;
            }
            else
            {
                btnColumnUp.Enabled = true;
                btnColumnTop.Enabled = true;
            }

            if (_dtColumnData == null || currentRow == _dtColumnData.Rows.Count - 1)
            {
                btnColumnDown.Enabled = false;
                btnColumnBottom.Enabled = false;
            }
            else
            {
                btnColumnDown.Enabled = true;
                btnColumnBottom.Enabled = true;
            }
        }

        private void UpdateColumnEnabled()
        {
            var isSizeLocked = true;
            var isPrecisionLocked = true;
            var isScaleLocked = true;
            var dataType = string.Empty;

            try
            {
                dataType = _dtColumnData.Rows[c1GridColumn.Row].GetSafeString(ColumnColumn.DataType);
            }
            catch (Exception)
            {
                //20240409 只有一筆資料，此時又再刪除它，_dtColumnData.Rows.Count = 1，但無法正常取值，故忽略它
                return;
            }

            //根據 Data Type 欄位，調整其他欄位是否可以被使用者異動
            switch (dataType)
            {
                case "CHAR":
                case "NCHAR":
                case "NVARCHAR2":
                case "VARCHAR2":
                case "RAW":
                    {
                        isSizeLocked = false;
                        isPrecisionLocked = true;
                        isScaleLocked = true;

                        if (_dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Size].ToString() == "0")
                        {
                            _dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Size] = 50;
                        }

                        break;
                    }
                case "FLOAT":
                    {
                        isSizeLocked = true;
                        isPrecisionLocked = false;
                        isScaleLocked = true;

                        if (_dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Precision].ToString() == "0")
                        {
                            _dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Precision] = 1;
                        }

                        break;
                    }
                case "BFILE":
                case "BLOB":
                case "BINARY_DOUBLE":
                case "BINARY_FLOAT":
                case "CLOB":
                case "NCLOB":
                case "DATE":
                case "INTEGER":
                case "LONG":
                case "LONG RAW":
                case "ROWID":
                case "XMLTYPE":
                    {
                        isSizeLocked = true;
                        isPrecisionLocked = true;
                        isScaleLocked = true;
                        break;
                    }
                case "NUMBER":
                    {
                        isSizeLocked = true;
                        isPrecisionLocked = false;
                        isScaleLocked = false;

                        if (_dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Precision].ToString() == "0")
                        {
                            _dtColumnData.Rows[c1GridColumn.Row][ColumnColumn.Precision] = 1;
                        }

                        break;
                    }
                case "TIMESTAMP":
                case "TIMESTAMP WITH TIME ZONE":
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    {
                        isSizeLocked = true;
                        isPrecisionLocked = true;
                        isScaleLocked = true;
                        break;
                    }
            }

            ResizeColumnWidth("COLUMN");

            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.Size].Locked = isSizeLocked;
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.Precision].Locked = isPrecisionLocked;
            c1GridColumn.Splits[0].DisplayColumns[ColumnColumn.Scale].Locked = isScaleLocked;
        }

        private void btnIndexesAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var row = NewDataRowIndexes();

                _dtIndexesData.Rows.Add(row);
                CheckIndexesButtons();

                c1GridIndexes.Row = _dtIndexesData.Rows.Count - 1;
                c1GridIndexes.Col = 0;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow NewDataRowIndexes()
        {
            var pid = $"{DateTime.Now:yyyyMMddHHmmssfff}";
            var next = GetNextSequence(_dtIndexesData, IndexesColumn.IndexesName, $"{txtTableName.Text}_INDEX");
            var row = _dtIndexesData.NewRow();

            row[IndexesColumn.IndexesPid] = pid;
            row[IndexesColumn.IndexesName] = $"{txtTableName.Text}_INDEX{next}";
            row[IndexesColumn.IndexesType] = _defaultIndexTypeText;

            DataRow row2;

            for (var i = 0; i < _dtColumnData.Rows.Count; i++)
            {
                var dr = _dtColumnData.Rows[i];

                row2 = _dtIndexExpressionsData.NewRow();
                row2[IndexExpressionsColumn.IndexesPid] = pid;
                row2[IndexExpressionsColumn.ColumnPid] = dr.GetSafeString(ColumnColumn.ColumnPid);
                row2[IndexExpressionsColumn.ColumnOrder] = dr.GetSafeString(ColumnColumn.ColumnOrder);
                row2[IndexExpressionsColumn.Checked] = 0; //預設不勾
                row2[IndexExpressionsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                row2[IndexExpressionsColumn.OrderBy] = _notSpecifiedText;
                _dtIndexExpressionsData.Rows.Add(row2);
            }

            c1GridIndexExpressions.DataSource = _dtIndexExpressionsData;

            var items2 = c1GridIndexExpressions.Columns[IndexExpressionsColumn.OrderBy].ValueItems;

            items2.Translate = true;
            items2.Presentation = PresentationEnum.ComboBox;
            items2.Validate = true;
            items2.Values.Clear();
            items2.Values.Add(new ValueItem(_notSpecifiedText, _notSpecifiedText));
            items2.Values.Add(new ValueItem("ASC", "ASC"));
            items2.Values.Add(new ValueItem("DESC", "DESC"));
            c1GridIndexExpressions.Splits[0].DisplayColumns[IndexExpressionsColumn.OrderBy].DropDownList = true;

            return row;
        }

        private void btnIndexesRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtIndexesData?.Rows.Count > 0)
                {
                    //20240406 同步刪除 Constraint 資料
                    var dr = _dtIndexExpressionsData.Select($"Indexepid = '{_dtIndexesData.Rows[c1GridIndexes.Row][IndexesColumn.IndexesPid]}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j].Delete();
                    }

                    c1GridIndexes.AllowDelete = true;
                    c1GridIndexes.Delete(c1GridIndexes.Row);
                    c1GridIndexes.AllowDelete = false;

                    //強制觸發 c1GridIndexes_RowColChange 事件 (更新 c1GridIndexExpressions 內容)
                    c1GridIndexes.Col = 0;
                    c1GridIndexes.Col = 1;
                }

                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnIndexesTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridIndexes.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtIndexesData.Rows[currentRow];
                var newRow = _dtIndexesData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtIndexesData.Rows.Remove(selectedRow);
                _dtIndexesData.Rows.InsertAt(newRow, 0);
                c1GridIndexes.Row = 0;

                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnIndexesUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridIndexes.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtIndexesData.Rows[currentRow];
                var newRow = _dtIndexesData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtIndexesData.Rows.Remove(selectedRow);
                _dtIndexesData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridIndexes.Row = currentRow - 1;

                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnIndexesDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridIndexes.Row;

                if (currentRow == _dtIndexesData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtIndexesData.Rows[currentRow];
                var newRow = _dtIndexesData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtIndexesData.Rows.Remove(selectedRow);
                _dtIndexesData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridIndexes.Row = currentRow + 1;

                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnIndexesBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridIndexes.Row;

                if (currentRow == _dtIndexesData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtIndexesData.Rows[currentRow];
                var newRow = _dtIndexesData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtIndexesData.Rows.Remove(selectedRow);
                _dtIndexesData.Rows.InsertAt(newRow, _dtIndexesData.Rows.Count);
                c1GridIndexes.Row = _dtIndexesData.Rows.Count - 1;

                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridIndexes_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (_duplicateIndexNameRowIndex != -1)
                {
                    c1GridIndexes.Row = _duplicateIndexNameRowIndex;
                    c1GridIndexes.Col = IndexesColumn.IndexesName;
                    _duplicateIndexNameRowIndex = -1;
                }

                if (!_isFormLoadFinished)
                {
                    return;
                }

                var dt = c1GridIndexExpressions.GetDataTableSourceOrNull();

                if (dt == null || dt.Rows.Count == 0)
                {
                    return;
                }

                var dtView = dt.DefaultView;
                var pid = _dtIndexesData.Rows[c1GridIndexes.Row].GetSafeString(IndexesColumn.IndexesPid);

                dtView.RowFilter = $"Indexepid = '{pid}'";
                CheckIndexesButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridIndexes_AfterColUpdate(object sender, ColEventArgs e)
        {
            try
            {
                var currentRow = c1GridIndexes.Row;
                var currentCol = c1GridIndexes.Col;

                //異動 Index Name 欄位：檢查 Index Name 是否已存在
                if (currentCol == IndexesColumn.IndexesName)
                {
                    var indexesNameOld = _dtIndexesData.Rows[currentRow].GetSafeString(IndexesColumn.IndexesName);

                    if (string.IsNullOrEmpty(indexesNameOld))
                    {
                        var next = GetNextSequence(_dtIndexesData, IndexesColumn.IndexesName, $"{txtTableName.Text}.INDEX");

                        //欄位名稱被清空了
                        _dtIndexesData.Rows[currentRow][IndexesColumn.IndexesName] = $"{txtTableName.Text}.INDEX{next}";
                    }
                    else
                    {
                        for (var i = 0; i < _dtIndexesData.Rows.Count; i++)
                        {
                            var indexesName = _dtIndexesData.Rows[i].GetSafeString(IndexesColumn.IndexesName);

                            if (i != currentRow && string.Equals(indexesName, indexesNameOld, StringComparison.OrdinalIgnoreCase))
                            {
                                var next = GetNextSequence(_dtIndexesData, IndexesColumn.IndexesName, $"{txtTableName.Text}.INDEX");
                                var indexesNameNew = $"{txtTableName.Text}.INDEX{next}";

                                _dtIndexesData.Rows[currentRow][IndexesColumn.IndexesName] = indexesNameNew;

                                //20240324 記住 row
                                _duplicateIndexNameRowIndex = currentRow;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _isEditMode = false;
                btnIndexesAdd.Enabled = !_isEditMode;
                btnIndexesRemove.Enabled = !_isEditMode;
                btnIndexesTop.Enabled = !_isEditMode;
                btnIndexesUp.Enabled = !_isEditMode;
                btnIndexesDown.Enabled = !_isEditMode;
                btnIndexesBottom.Enabled = !_isEditMode;
            }
        }

        private void c1GridIndexes_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            _currentEditRowIndex = c1GridIndexes.Row;
            _currentEditColumnIndex = c1GridIndexes.Col;

            _isEditMode = true;
            btnIndexesAdd.Enabled = !_isEditMode;
            btnIndexesRemove.Enabled = !_isEditMode;
            btnIndexesTop.Enabled = !_isEditMode;
            btnIndexesUp.Enabled = !_isEditMode;
            btnIndexesDown.Enabled = !_isEditMode;
            btnIndexesBottom.Enabled = !_isEditMode;
        }

        private void c1GridIndexes_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridIndexes.RowContaining(e.Y);
                var col = c1GridIndexes.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("INDEXES");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridIndexes_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("INDEXES");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnPrimaryKeyAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var row = NewDataRowPrimaryKey(true);

                _dtPrimaryKeyData.Rows.Add(row);

                CheckPrimaryKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow NewDataRowPrimaryKey(bool bClickAddButton = false)
        {
            var pid = $"{DateTime.Now:yyyyMMddHHmmssfff}";
            var row = _dtPrimaryKeyData.NewRow();

            row[PrimaryKeyColumn.PrimaryKeyPid] = pid;
            row[PrimaryKeyColumn.PrimaryKeyName] = $"{txtTableName.Text}_PK";
            row[PrimaryKeyColumn.Enabled] = 1;
            row[PrimaryKeyColumn.Validate] = 1;
            row[PrimaryKeyColumn.DeferrableState] = _defaultDeferrableText;

            DataRow row2;

            for (var i = 0; i < _dtColumnData.Rows.Count; i++)
            {
                var dr = _dtColumnData.Rows[i];

                row2 = _dtPrimaryKeyConstraintsData.NewRow();
                row2[PrimaryKeyConstraintsColumn.PrimaryKeyPid] = pid;
                row2[PrimaryKeyConstraintsColumn.ColumnPid] = dr.GetSafeString(ColumnColumn.ColumnPid);
                row2[PrimaryKeyConstraintsColumn.ColumnOrder] = dr.GetSafeString(ColumnColumn.ColumnOrder);
                row2[PrimaryKeyConstraintsColumn.Checked] = bClickAddButton ? 0 : (string.IsNullOrEmpty(dr.GetSafeString(ColumnColumn.PrimaryKey)) ? 0 : 1); //透過新增按鈕，一律為 0，否則則判斷是否有在欄位頁籤勾選 PK
                row2[PrimaryKeyConstraintsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                _dtPrimaryKeyConstraintsData.Rows.Add(row2);
            }

            c1GridPrimaryKeyConstraints.DataSource = _dtPrimaryKeyConstraintsData;

            return row;
        }

        private void btnPrimaryKeyRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtPrimaryKeyData?.Rows.Count > 0)
                {
                    c1GridPrimaryKey.AllowDelete = true;
                    c1GridPrimaryKey.Delete(c1GridPrimaryKey.Row);
                    c1GridPrimaryKey.AllowDelete = false;

                    _dtPrimaryKeyConstraintsData.Clear();

                    //20240403 同步將 Column 頁籤所有的 PK 都取消勾選
                    for (var i = 0; i < _dtColumnData.Rows.Count; i++)
                    {
                        _dtColumnData.Rows[i][ColumnColumn.PrimaryKey] = 0;
                    }
                }

                CheckPrimaryKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridPrimaryKey_AfterColUpdate(object sender, ColEventArgs e)
        {
            _isEditMode = false;
            btnPrimaryKeyAdd.Enabled = !_isEditMode;
            btnPrimaryKeyRemove.Enabled = !_isEditMode;
        }

        private void c1GridPrimaryKey_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            _currentEditRowIndex = c1GridPrimaryKey.Row;
            _currentEditColumnIndex = c1GridPrimaryKey.Col;

            _isEditMode = true;
            btnPrimaryKeyAdd.Enabled = !_isEditMode;
            btnPrimaryKeyRemove.Enabled = !_isEditMode;
        }

        private void c1GridPrimaryKey_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridPrimaryKey.RowContaining(e.Y);
                var col = c1GridPrimaryKey.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("PRIMARYKEY");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridPrimaryKey_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("PRIMARYKEY");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUniqueAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var row = NewDataRowUnique();

                _dtUniqueData.Rows.Add(row);
                CheckUniqueButtons();

                c1GridUnique.Row = _dtUniqueData.Rows.Count - 1;
                c1GridUnique.Col = 0;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow NewDataRowUnique()
        {
            var pid = $"{DateTime.Now:yyyyMMddHHmmssfff}";
            var next = GetNextSequence(_dtUniqueData, UniqueColumn.UniqueName, $"{txtTableName.Text}_UK");
            var row = _dtUniqueData.NewRow();

            row[UniqueColumn.UniquePid] = pid;
            row[UniqueColumn.UniqueName] = $"{txtTableName.Text}_UK{next}";
            row[UniqueColumn.Enabled] = 1;
            row[UniqueColumn.Validate] = 1;
            row[UniqueColumn.DeferrableState] = _defaultDeferrableText;

            DataRow row2;

            for (var i = 0; i < _dtColumnData.Rows.Count; i++)
            {
                var dr = _dtColumnData.Rows[i];

                row2 = _dtUniqueConstraintsData.NewRow();
                row2[UniqueConstraintsColumn.UniquePid] = pid;
                row2[UniqueConstraintsColumn.ColumnPid] = dr.GetSafeString(ColumnColumn.ColumnPid);
                row2[UniqueConstraintsColumn.ColumnOrder] = dr.GetSafeString(ColumnColumn.ColumnOrder);
                row2[UniqueConstraintsColumn.Checked] = 0; //預設不勾
                row2[UniqueConstraintsColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                _dtUniqueConstraintsData.Rows.Add(row2);
            }

            c1GridUniqueConstraints.DataSource = _dtUniqueConstraintsData;

            return row;
        }

        private void btnUniqueRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtUniqueData?.Rows.Count > 0)
                {
                    //20240406 同步刪除 Constraint 資料
                    var dr = _dtUniqueConstraintsData.Select($"UniquePID = '{_dtUniqueData.Rows[c1GridUnique.Row][UniqueColumn.UniquePid]}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j].Delete();
                    }

                    c1GridUnique.AllowDelete = true;
                    c1GridUnique.Delete(c1GridUnique.Row);
                    c1GridUnique.AllowDelete = false;

                    //強制觸發 c1GridUnique_RowColChange 事件 (更新 c1GridUniqueConstraints 內容)
                    c1GridUnique.Col = 0;
                    c1GridUnique.Col = 1;
                }

                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUniqueTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridUnique.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtUniqueData.Rows[currentRow];
                var newRow = _dtUniqueData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtUniqueData.Rows.Remove(selectedRow);
                _dtUniqueData.Rows.InsertAt(newRow, 0);
                c1GridUnique.Row = 0;

                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUniqueUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridUnique.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtUniqueData.Rows[currentRow];
                var newRow = _dtUniqueData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtUniqueData.Rows.Remove(selectedRow);
                _dtUniqueData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridUnique.Row = currentRow - 1;

                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUniqueDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridUnique.Row;

                if (currentRow == _dtUniqueData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtUniqueData.Rows[currentRow];
                var newRow = _dtUniqueData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtUniqueData.Rows.Remove(selectedRow);
                _dtUniqueData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridUnique.Row = currentRow + 1;

                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnUniqueBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridUnique.Row;

                if (currentRow == _dtUniqueData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtUniqueData.Rows[currentRow];
                var newRow = _dtUniqueData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtUniqueData.Rows.Remove(selectedRow);
                _dtUniqueData.Rows.InsertAt(newRow, _dtUniqueData.Rows.Count);
                c1GridUnique.Row = _dtUniqueData.Rows.Count - 1;

                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridUnique_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (_duplicateUniqueNameRowIndex != -1)
                {
                    c1GridUnique.Row = _duplicateUniqueNameRowIndex;
                    c1GridUnique.Col = UniqueColumn.UniqueName;
                    _duplicateUniqueNameRowIndex = -1;
                }

                if (!_isFormLoadFinished)
                {
                    return;
                }

                var dt = c1GridUniqueConstraints.GetDataTableSourceOrNull();

                if (dt == null || dt.Rows.Count == 0)
                {
                    return;
                }

                var dtView = dt.DefaultView;
                var pid = _dtUniqueData.Rows[c1GridUnique.Row].GetSafeString(UniqueColumn.UniquePid);

                dtView.RowFilter = $"UniquePID = '{pid}'";
                CheckUniqueButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridUnique_AfterColUpdate(object sender, ColEventArgs e)
        {
            try
            {
                var currentRow = c1GridUnique.Row;
                var currentCol = c1GridUnique.Col;

                //異動 Unique Name 欄位：檢查 Unique Name 是否已存在
                if (currentCol == UniqueColumn.UniqueName)
                {
                    var uniqueNameOld = _dtUniqueData.Rows[currentRow].GetSafeString(UniqueColumn.UniqueName);

                    if (string.IsNullOrEmpty(uniqueNameOld))
                    {
                        var next = GetNextSequence(_dtUniqueData, UniqueColumn.UniqueName, $"{txtTableName.Text}.INDEX");

                        //欄位名稱被清空了
                        _dtUniqueData.Rows[currentRow][UniqueColumn.UniqueName] = $"{txtTableName.Text}.INDEX{next}";
                    }
                    else
                    {
                        for (var i = 0; i < _dtUniqueData.Rows.Count; i++)
                        {
                            var uniqueName = _dtUniqueData.Rows[i].GetSafeString(UniqueColumn.UniqueName);

                            if (i != currentRow && string.Equals(uniqueName, uniqueNameOld, StringComparison.OrdinalIgnoreCase))
                            {
                                var next = GetNextSequence(_dtUniqueData, UniqueColumn.UniqueName, $"{txtTableName.Text}.INDEX");
                                var uniqueNameNew = $"{txtTableName.Text}.INDEX{next}";

                                _dtUniqueData.Rows[currentRow][UniqueColumn.UniqueName] = uniqueNameNew;

                                //20240324 記住 row
                                _duplicateUniqueNameRowIndex = currentRow;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _isEditMode = false;
                btnUniqueAdd.Enabled = !_isEditMode;
                btnUniqueRemove.Enabled = !_isEditMode;
                btnUniqueTop.Enabled = !_isEditMode;
                btnUniqueUp.Enabled = !_isEditMode;
                btnUniqueDown.Enabled = !_isEditMode;
                btnUniqueBottom.Enabled = !_isEditMode;
            }
        }

        private void c1GridUnique_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            //20240408 除了勾選欄位，其餘欄位進入編輯模式要特別處理
            switch (c1GridUnique.Col)
            {
                case UniqueColumn.Enabled:
                case UniqueColumn.Validate:
                    {
                        return;
                    }
                default:
                    {
                        break;
                    }
            }

            _currentEditRowIndex = c1GridUnique.Row;
            _currentEditColumnIndex = c1GridUnique.Col;

            _isEditMode = true;
            btnUniqueAdd.Enabled = !_isEditMode;
            btnUniqueRemove.Enabled = !_isEditMode;
            btnUniqueTop.Enabled = !_isEditMode;
            btnUniqueUp.Enabled = !_isEditMode;
            btnUniqueDown.Enabled = !_isEditMode;
            btnUniqueBottom.Enabled = !_isEditMode;
        }

        private void c1GridUnique_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridUnique.RowContaining(e.Y);
                var col = c1GridUnique.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("UNIQUE");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridUnique_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("UNIQUE");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyAdd_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                var row = NewDataRowForeignKey();

                if (!pnlForeignKeyReferencedTable.Enabled)
                {
                    GetAllTables4ForeignKeySchema();
                }

                _dtForeignKeyData.Rows.Add(row);
                CheckForeignKeyButtons();

                cboForeignKeySchema.Text = DatabaseSqlExecutor.DbUserUppercase;
                cboForeignKeyTable.Text = string.Empty;
                c1GridForeignKey.Row = _dtForeignKeyData.Rows.Count - 1;
                c1GridForeignKey.Col = 0;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            Cursor = Cursors.Default;
        }

        private DataRow NewDataRowForeignKey()
        {
            var pid = $"{DateTime.Now:yyyyMMddHHmmssfff}";
            var next = GetNextSequence(_dtForeignKeyData, ForeignKeyColumn.ForeignKeyName, $"{txtTableName.Text}_FK");
            var row = _dtForeignKeyData.NewRow();

            row[ForeignKeyColumn.ForeignKeyPid] = pid;
            row[ForeignKeyColumn.ForeignKeyName] = $"{txtTableName.Text}_FK{next}";
            row[ForeignKeyColumn.Enabled] = 1;
            row[ForeignKeyColumn.Validate] = 1;
            row[ForeignKeyColumn.DeferrableState] = _defaultDeferrableText;
            row[ForeignKeyColumn.OnDelete] = _defaultOnDeleteText;

            DataRow row2;

            for (var i = 0; i < _dtColumnData.Rows.Count; i++)
            {
                var dr = _dtColumnData.Rows[i];

                row2 = _dtForeignKeyThisTableData.NewRow();
                row2[ForeignKeyThisTableColumn.ForeignKeyPid] = pid;
                row2[ForeignKeyThisTableColumn.ColumnPid] = dr.GetSafeString(ColumnColumn.ColumnPid);
                row2[ForeignKeyThisTableColumn.ColumnOrder] = dr.GetSafeString(ColumnColumn.ColumnOrder);
                row2[ForeignKeyThisTableColumn.Checked] = 0; //預設不勾
                row2[ForeignKeyThisTableColumn.ColumnName] = dr.GetSafeString(ColumnColumn.ColumnName);
                _dtForeignKeyThisTableData.Rows.Add(row2);
            }

            c1GridForeignKeyThisTable.DataSource = _dtForeignKeyThisTableData;

            return row;
        }

        private void btnForeignKeyRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtForeignKeyData?.Rows.Count > 0)
                {
                    //20240406 同步刪除 Constraint 資料
                    var dr = _dtForeignKeyThisTableData.Select($"ForeignKeyPID = '{_dtForeignKeyData.Rows[c1GridForeignKey.Row][ForeignKeyColumn.ForeignKeyPid]}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j].Delete();
                    }

                    dr = _dtForeignKeyReferencedTableData.Select($"ForeignKeyPID = '{_dtForeignKeyData.Rows[c1GridForeignKey.Row][ForeignKeyColumn.ForeignKeyPid]}'");

                    for (var j = 0; j < dr.Length; j++)
                    {
                        dr[j].Delete();
                    }

                    c1GridForeignKey.AllowDelete = true;
                    c1GridForeignKey.Delete(c1GridForeignKey.Row);
                    c1GridForeignKey.AllowDelete = false;

                    //強制觸發 c1GridForeignKey_RowColChange 事件 (更新 c1GridForeignKeyConstraints 內容)
                    c1GridForeignKey.Col = 0;
                    c1GridForeignKey.Col = 1;
                }

                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyInsert_Click(object sender, EventArgs e)
        {
            var currentRow = c1GridForeignKey.Row;
            var row = NewDataRowForeignKey();

            _dtForeignKeyData.Rows.InsertAt(row, currentRow);
            c1GridForeignKey.Row = currentRow;

            CheckForeignKeyButtons();
        }

        private void btnForeignKeyTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKey.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyData.Rows[currentRow];
                var newRow = _dtForeignKeyData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyData.Rows.Remove(selectedRow);
                _dtForeignKeyData.Rows.InsertAt(newRow, 0);
                c1GridForeignKey.Row = 0;

                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKey.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyData.Rows[currentRow];
                var newRow = _dtForeignKeyData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyData.Rows.Remove(selectedRow);
                _dtForeignKeyData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridForeignKey.Row = currentRow - 1;

                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKey.Row;

                if (currentRow == _dtForeignKeyData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyData.Rows[currentRow];
                var newRow = _dtForeignKeyData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyData.Rows.Remove(selectedRow);
                _dtForeignKeyData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridForeignKey.Row = currentRow + 1;

                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKey.Row;

                if (currentRow == _dtForeignKeyData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyData.Rows[currentRow];
                var newRow = _dtForeignKeyData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyData.Rows.Remove(selectedRow);
                _dtForeignKeyData.Rows.InsertAt(newRow, _dtForeignKeyData.Rows.Count);
                c1GridForeignKey.Row = _dtForeignKeyData.Rows.Count - 1;

                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyThisTableTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyThisTable.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyThisTableData.Rows[currentRow];
                var newRow = _dtForeignKeyThisTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyThisTableData.Rows.Remove(selectedRow);
                _dtForeignKeyThisTableData.Rows.InsertAt(newRow, 0);
                c1GridForeignKeyThisTable.Row = 0;

                CheckForeignKeyThisTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyThisTableUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyThisTable.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyThisTableData.Rows[currentRow];
                var newRow = _dtForeignKeyThisTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyThisTableData.Rows.Remove(selectedRow);
                _dtForeignKeyThisTableData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridForeignKeyThisTable.Row = currentRow - 1;

                CheckForeignKeyThisTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyThisTableDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyThisTable.Row;

                if (currentRow == _dtForeignKeyThisTableData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyThisTableData.Rows[currentRow];
                var newRow = _dtForeignKeyThisTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyThisTableData.Rows.Remove(selectedRow);
                _dtForeignKeyThisTableData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridForeignKeyThisTable.Row = currentRow + 1;

                CheckForeignKeyThisTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyThisTableBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyThisTable.Row;

                if (currentRow == _dtForeignKeyThisTableData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyThisTableData.Rows[currentRow];
                var newRow = _dtForeignKeyThisTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyThisTableData.Rows.Remove(selectedRow);
                _dtForeignKeyThisTableData.Rows.InsertAt(newRow, _dtForeignKeyThisTableData.Rows.Count);
                c1GridForeignKeyThisTable.Row = _dtForeignKeyThisTableData.Rows.Count - 1;

                CheckForeignKeyThisTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyReferencedTableTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyReferencedTable.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyReferencedTableData.Rows[currentRow];
                var newRow = _dtForeignKeyReferencedTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyReferencedTableData.Rows.Remove(selectedRow);
                _dtForeignKeyReferencedTableData.Rows.InsertAt(newRow, 0);
                c1GridForeignKeyReferencedTable.Row = 0;

                CheckForeignKeyReferencedTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyReferencedTableUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyReferencedTable.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyReferencedTableData.Rows[currentRow];
                var newRow = _dtForeignKeyReferencedTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyReferencedTableData.Rows.Remove(selectedRow);
                _dtForeignKeyReferencedTableData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridForeignKeyReferencedTable.Row = currentRow - 1;

                CheckForeignKeyReferencedTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyReferencedTableDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyReferencedTable.Row;

                if (currentRow == _dtForeignKeyReferencedTableData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyReferencedTableData.Rows[currentRow];
                var newRow = _dtForeignKeyReferencedTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyReferencedTableData.Rows.Remove(selectedRow);
                _dtForeignKeyReferencedTableData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridForeignKeyReferencedTable.Row = currentRow + 1;

                CheckForeignKeyReferencedTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnForeignKeyReferencedTableBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridForeignKeyReferencedTable.Row;

                if (currentRow == _dtForeignKeyReferencedTableData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtForeignKeyReferencedTableData.Rows[currentRow];
                var newRow = _dtForeignKeyReferencedTableData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtForeignKeyReferencedTableData.Rows.Remove(selectedRow);
                _dtForeignKeyReferencedTableData.Rows.InsertAt(newRow, _dtForeignKeyReferencedTableData.Rows.Count);
                c1GridForeignKeyReferencedTable.Row = _dtForeignKeyReferencedTableData.Rows.Count - 1;

                CheckForeignKeyReferencedTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridForeignKey_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                _isForeignKeySelectionSyncing = false;

                if (_duplicateForeignKeyNameRowIndex != -1)
                {
                    c1GridForeignKey.Row = _duplicateForeignKeyNameRowIndex;
                    c1GridForeignKey.Col = ForeignKeyColumn.ForeignKeyName;
                    _duplicateForeignKeyNameRowIndex = -1;
                }

                if (!_isFormLoadFinished)
                {
                    return;
                }

                var pid = _dtForeignKeyData.Rows[c1GridForeignKey.Row].GetSafeString(ForeignKeyColumn.ForeignKeyPid);

                if (c1GridForeignKeyThisTable.DataSource is DataTable dtView)
                {
                    dtView.DefaultView.RowFilter = $"ForeignKeyPID = '{pid}'";

                    //強制觸發 c1GridForeignKeyThisTable_RowColChange
                    c1GridForeignKeyThisTable.Row = 0;
                    c1GridForeignKeyThisTable.Col = ForeignKeyThisTableColumn.ColumnName;
                }

                if (c1GridForeignKeyReferencedTable.DataSource is DataTable dtView2)
                {
                    dtView2.DefaultView.RowFilter = $"ForeignKeyPID = '{pid}'";

                    //強制觸發 c1GridForeignKeyReferencedTable_RowColChange
                    c1GridForeignKeyReferencedTable.Row = 0;
                    c1GridForeignKeyReferencedTable.Col = ForeignKeyReferencedTableColumn.ColumnName;

                    var referencedSchema = _dtForeignKeyData.Rows[c1GridForeignKey.Row].GetSafeString(ForeignKeyColumn.ForeignKeyReferencedSchema);

                    if (!string.IsNullOrEmpty(referencedSchema))
                    {
                        _isForeignKeySelectionSyncing = true; //避免重複觸發 cboForeignKeyTable_SelectedIndexChanged
                        cboForeignKeySchema.Text = referencedSchema;
                        cboForeignKeyTable.Text = _dtForeignKeyData.Rows[c1GridForeignKey.Row].GetSafeString(ForeignKeyColumn.ForeignKeyReferencedTable);
                    }
                    else if (!string.IsNullOrEmpty(cboForeignKeyTable.Text))
                    {
                        cboForeignKeyTable.Text = string.Empty;
                    }
                }

                c1GridForeignKeyReferencedTable.Tag = pid;
                CheckForeignKeyButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridForeignKey_AfterColUpdate(object sender, ColEventArgs e)
        {
            var currentRow = c1GridForeignKey.Row;
            var currentCol = c1GridForeignKey.Col;

            try
            {
                //異動 ForeignKey Name 欄位：檢查 ForeignKey Name 是否已存在
                if (currentCol == ForeignKeyColumn.ForeignKeyName)
                {
                    var foreignKeyNameOld = _dtForeignKeyData.Rows[currentRow].GetSafeString(ForeignKeyColumn.ForeignKeyName);

                    if (string.IsNullOrEmpty(foreignKeyNameOld))
                    {
                        var next = GetNextSequence(_dtForeignKeyData, ForeignKeyColumn.ForeignKeyName, $"{txtTableName.Text}.FK");

                        //欄位名稱被清空了
                        _dtForeignKeyData.Rows[currentRow][ForeignKeyColumn.ForeignKeyName] = $"{txtTableName.Text}.FK{next}";
                    }
                    else
                    {
                        for (var i = 0; i < _dtForeignKeyData.Rows.Count; i++)
                        {
                            var foreignKeyName = _dtForeignKeyData.Rows[i].GetSafeString(ForeignKeyColumn.ForeignKeyName);

                            if (i != currentRow && string.Equals(foreignKeyName, foreignKeyNameOld, StringComparison.OrdinalIgnoreCase))
                            {
                                var next = GetNextSequence(_dtForeignKeyData, ForeignKeyColumn.ForeignKeyName, $"{txtTableName.Text}.FK");
                                var foreignKeyNameNew = $"{txtTableName.Text}.FK{next}";

                                _dtForeignKeyData.Rows[currentRow][ForeignKeyColumn.ForeignKeyName] = foreignKeyNameNew;

                                //20240324 記住 row
                                _duplicateForeignKeyNameRowIndex = currentRow;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _isEditMode = false;
                btnForeignKeyAdd.Enabled = !_isEditMode;
                btnForeignKeyRemove.Enabled = !_isEditMode;
                btnForeignKeyTop.Enabled = !_isEditMode;
                btnForeignKeyUp.Enabled = !_isEditMode;
                btnForeignKeyDown.Enabled = !_isEditMode;
                btnForeignKeyBottom.Enabled = !_isEditMode;
            }
        }

        private void c1GridForeignKey_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            //20240408 除了勾選欄位，其餘欄位進入編輯模式要特別處理
            switch (c1GridForeignKey.Col)
            {
                case ForeignKeyColumn.Enabled:
                case ForeignKeyColumn.Validate:
                    {
                        return;
                    }
                default:
                    {
                        break;
                    }
            }

            _currentEditRowIndex = c1GridForeignKey.Row;
            _currentEditColumnIndex = c1GridForeignKey.Col;

            _isEditMode = true;
            btnForeignKeyAdd.Enabled = !_isEditMode;
            btnForeignKeyRemove.Enabled = !_isEditMode;
            btnForeignKeyTop.Enabled = !_isEditMode;
            btnForeignKeyUp.Enabled = !_isEditMode;
            btnForeignKeyDown.Enabled = !_isEditMode;
            btnForeignKeyBottom.Enabled = !_isEditMode;
        }

        private void c1GridForeignKey_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridForeignKey.RowContaining(e.Y);
                var col = c1GridForeignKey.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("FOREIGNKEY");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridForeignKey_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("FOREIGNKEY");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridForeignKeyThisTable_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (!_isFormLoadFinished)
                {
                    return;
                }

                CheckForeignKeyThisTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridForeignKeyReferencedTable_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (!_isFormLoadFinished)
                {
                    return;
                }

                CheckForeignKeyReferencedTableButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var row = NewDataRowCheck();

                _dtCheckData.Rows.Add(row);
                CheckCheckButtons();

                c1GridCheck.Row = _dtCheckData.Rows.Count - 1;
                c1GridCheck.Col = 0;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow NewDataRowCheck()
        {
            var next = GetNextSequence(_dtCheckData, CheckColumn.CheckName, $"{txtTableName.Text}_CHK");
            var row = _dtCheckData.NewRow();

            row[CheckColumn.CheckName] = $"{txtTableName.Text}_CHK{next}";
            row[CheckColumn.CheckCondition] = string.Empty;
            row[CheckColumn.Enabled] = 1;
            row[CheckColumn.Validate] = 1;
            row[CheckColumn.DeferrableState] = _defaultDeferrableText;

            return row;
        }

        private void btnCheckRemove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_dtCheckData?.Rows.Count > 0)
                {
                    c1GridCheck.AllowDelete = true;
                    c1GridCheck.Delete(c1GridCheck.Row);
                    c1GridCheck.AllowDelete = false;
                }

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckInsert_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridCheck.Row;
                var row = NewDataRowCheck();

                _dtCheckData.Rows.InsertAt(row, currentRow);
                c1GridCheck.Row = currentRow;

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckTop_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridCheck.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtCheckData.Rows[currentRow];
                var newRow = _dtCheckData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtCheckData.Rows.Remove(selectedRow);
                _dtCheckData.Rows.InsertAt(newRow, 0);
                c1GridCheck.Row = 0;

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckUp_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridCheck.Row;

                if (currentRow == 0)
                {
                    return;
                }

                var selectedRow = _dtCheckData.Rows[currentRow];
                var newRow = _dtCheckData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtCheckData.Rows.Remove(selectedRow);
                _dtCheckData.Rows.InsertAt(newRow, currentRow + 1 / -1);
                c1GridCheck.Row = currentRow - 1;

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckDown_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridCheck.Row;

                if (currentRow == _dtCheckData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtCheckData.Rows[currentRow];
                var newRow = _dtCheckData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtCheckData.Rows.Remove(selectedRow);
                _dtCheckData.Rows.InsertAt(newRow, currentRow - 1 / -1);
                c1GridCheck.Row = currentRow + 1;

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnCheckBottom_Click(object sender, EventArgs e)
        {
            try
            {
                var currentRow = c1GridCheck.Row;

                if (currentRow == _dtCheckData.Rows.Count - 1)
                {
                    return;
                }

                var selectedRow = _dtCheckData.Rows[currentRow];
                var newRow = _dtCheckData.NewRow();

                newRow.ItemArray = selectedRow.ItemArray;
                _dtCheckData.Rows.Remove(selectedRow);
                _dtCheckData.Rows.InsertAt(newRow, _dtCheckData.Rows.Count);
                c1GridCheck.Row = _dtCheckData.Rows.Count - 1;

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridCheck_RowColChange(object sender, RowColChangeEventArgs e)
        {
            try
            {
                if (_duplicateCheckNameRowIndex != -1)
                {
                    c1GridCheck.Row = _duplicateCheckNameRowIndex;
                    c1GridCheck.Col = CheckColumn.CheckName;
                    _duplicateCheckNameRowIndex = -1;
                }

                if (!_isFormLoadFinished)
                {
                    return;
                }

                CheckCheckButtons();
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridCheck_AfterColUpdate(object sender, ColEventArgs e)
        {
            var currentRow = c1GridCheck.Row;
            var currentCol = c1GridCheck.Col;

            try
            {
                //異動 Check Name 欄位：檢查 Check Name 是否已存在
                if (currentCol == CheckColumn.CheckName)
                {
                    var checkNameOld = _dtCheckData.Rows[currentRow].GetSafeString(CheckColumn.CheckName);

                    if (string.IsNullOrEmpty(checkNameOld))
                    {
                        var next = GetNextSequence(_dtCheckData, CheckColumn.CheckName, $"{txtTableName.Text}_CHK");

                        //名稱被清空了
                        _dtCheckData.Rows[currentRow][CheckColumn.CheckName] = $"{txtTableName.Text}_CHK{next}";

                        return;
                    }

                    for (var i = 0; i < _dtCheckData.Rows.Count; i++)
                    {
                        var checkName = _dtCheckData.Rows[i].GetSafeString(CheckColumn.CheckName);

                        if (i != currentRow && string.Equals(checkName, checkNameOld, StringComparison.OrdinalIgnoreCase))
                        {
                            var next = GetNextSequence(_dtCheckData, CheckColumn.CheckName, $"{txtTableName.Text}_CHK");
                            var checkNameNew = $"{txtTableName.Text}_CHK{next}";

                            _dtCheckData.Rows[currentRow][CheckColumn.CheckName] = checkNameNew;

                            _duplicateCheckNameRowIndex = currentRow;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _isEditMode = false;
                btnCheckAdd.Enabled = !_isEditMode;
                btnCheckRemove.Enabled = !_isEditMode;
                btnCheckInsert.Enabled = !_isEditMode;
                btnCheckTop.Enabled = !_isEditMode;
                btnCheckUp.Enabled = !_isEditMode;
                btnCheckDown.Enabled = !_isEditMode;
                btnCheckBottom.Enabled = !_isEditMode;
            }
        }

        private void c1GridCheck_BeforeColEdit(object sender, BeforeColEditEventArgs e)
        {
            //20240408 除了勾選欄位，其餘欄位進入編輯模式要特別處理
            switch (c1GridCheck.Col)
            {
                case CheckColumn.Enabled:
                case CheckColumn.Validate:
                    {
                        return;
                    }
                default:
                    {
                        break;
                    }
            }

            _currentEditRowIndex = c1GridCheck.Row;
            _currentEditColumnIndex = c1GridCheck.Col;

            _isEditMode = true;
            btnCheckAdd.Enabled = !_isEditMode;
            btnCheckRemove.Enabled = !_isEditMode;
            btnCheckInsert.Enabled = !_isEditMode;
            btnCheckTop.Enabled = !_isEditMode;
            btnCheckUp.Enabled = !_isEditMode;
            btnCheckDown.Enabled = !_isEditMode;
            btnCheckBottom.Enabled = !_isEditMode;
        }

        private void c1GridCheck_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                var row = c1GridCheck.RowContaining(e.Y);
                var col = c1GridCheck.ColContaining(e.X);

                if (_isEditMode && (row == -1 || row != _currentEditRowIndex || col != _currentEditColumnIndex))
                {
                    UpdateEditMode("CHECK");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void c1GridCheck_Leave(object sender, EventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    UpdateEditMode("CHECK");
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string GetFullDataType(string dataType, string columnSize, string precision, string scale, string defaultString, out string defaultValue)
        {
            defaultValue = string.Empty;

            var result = string.Empty;

            switch (dataType)
            {
                case "CHAR":
                case "NCHAR":
                case "NVARCHAR2":
                case "VARCHAR2":
                case "RAW":
                    {
                        result = $"{dataType}({columnSize})";

                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            if (defaultString.StartsWith("'", StringComparison.Ordinal) && defaultString.EndsWith("'", StringComparison.Ordinal))
                            {
                                defaultValue = $"DEFAULT {defaultString}";
                            }
                            else
                            {
                                defaultValue = $"DEFAULT '{defaultString}'";
                            }
                        }

                        break;
                    }
                case "FLOAT":
                    {
                        result = $"{dataType}({precision})";

                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            defaultValue = $"DEFAULT {defaultString}";
                        }

                        break;
                    }
                case "BFILE":
                case "BLOB":
                case "BINARY_DOUBLE":
                case "BINARY_FLOAT":
                case "CLOB":
                case "NCLOB":
                case "DATE":
                case "INTEGER":
                case "LONG":
                case "LONG RAW":
                case "ROWID":
                case "XMLTYPE":
                    {
                        result = dataType;

                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            defaultValue = $"DEFAULT {defaultString}";
                        }

                        break;
                    }
                case "NUMBER":
                    {
                        result = $"{dataType}({precision},{scale})";

                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            defaultValue = $"DEFAULT {defaultString}";
                        }

                        break;
                    }
                case "TIMESTAMP":
                case "TIMESTAMP WITH TIME ZONE":
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    {
                        result = dataType;

                        if (!string.IsNullOrEmpty(defaultString))
                        {
                            defaultValue = $"DEFAULT {defaultString.ToUpper()}";
                        }

                        break;
                    }
            }

            return result;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSqlPreview();
        }

        private void RefreshSqlPreview()
        {
            var tableType = string.Empty;
            var tableComment = string.Empty;
            var columnNameLengthErrorMessage = string.Empty;
            var indexes = string.Empty;
            var indexesErrorMessage = string.Empty;
            var indexesNameLengthErrorMessage = string.Empty;
            var primaryKey = string.Empty;
            var primaryKeyNameLengthErrorMessage = string.Empty;
            var unique = string.Empty;
            var uniqueErrorMessage = string.Empty;
            var uniqueNameLengthErrorMessage = string.Empty;
            var foreignKey = string.Empty;
            var foreignKeyErrorMessage = string.Empty;
            var foreignKeyErrorMessageThisTable = string.Empty;
            var foreignKeyErrorMessageReferencedTable = string.Empty;
            var foreignKeyNameLengthErrorMessage = string.Empty;
            var check = string.Empty;
            var checkErrorMessage = string.Empty;
            var checkNameLengthErrorMessage = string.Empty;
            var comment = string.Empty;
            var dtCreate = new DataTable();
            var doubleQuotes = chkDoubleQuotes.Checked ? "\"" : string.Empty;

            try
            {
                dtCreate.Columns.Add("ColumnName");
                dtCreate.Columns.Add("DataType");
                dtCreate.Columns.Add("Invisible");
                dtCreate.Columns.Add("Default");
                dtCreate.Columns.Add("NotNull");

                for (var i = 0; i < _dtColumnData.Rows.Count; i++)
                {
                    var dr = _dtColumnData.Rows[i];

                    var columnName = dr.GetSafeString(ColumnColumn.ColumnName);
                    var dataType = dr.GetSafeString(ColumnColumn.DataType);
                    var defaultString = string.Empty;
                    var visibleString = dr.GetSafeString(ColumnColumn.Visible);
                    var notNullString = dr.GetSafeString(ColumnColumn.NotNull);
                    var primaryKeyChecked = dr.GetSafeString(ColumnColumn.PrimaryKey);

                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(dataType))
                    {
                        var row = dtCreate.NewRow();

                        row["ColumnName"] = $"{doubleQuotes}{columnName}{doubleQuotes}";

                        if (string.IsNullOrEmpty(columnNameLengthErrorMessage) && columnName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Column name length exceeds limit! The maximum length of column name is {length} characters.", "form", GetType().Name, "msg", "ColumnNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            columnNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        row["DataType"] = GetFullDataType(dataType, _dtColumnData.Rows[i].GetSafeString(ColumnColumn.Size), _dtColumnData.Rows[i].GetSafeString(ColumnColumn.Precision), _dtColumnData.Rows[i].GetSafeString(ColumnColumn.Scale), _dtColumnData.Rows[i].GetSafeString(ColumnColumn.Default), out defaultString);
                        row["Invisible"] = (visibleString == "1" || string.Equals(visibleString, "TRUE", StringComparison.OrdinalIgnoreCase)) ? string.Empty : "INVISIBLE";
                        row["Default"] = defaultString;
                        row["NotNull"] = (notNullString == "0" || string.Equals(notNullString, "FALSE", StringComparison.OrdinalIgnoreCase)) ? string.Empty : "NOT NULL";
                        dtCreate.Rows.Add(row);

                        if (primaryKeyChecked == "1" || string.Equals(primaryKeyChecked, "TRUE", StringComparison.OrdinalIgnoreCase))
                        {
                            primaryKey += $"{columnName}, ";
                        }

                        if (!string.IsNullOrEmpty(_dtColumnData.Rows[i][ColumnColumn.Comment].ToString()))
                        {
                            var comment2 = _dtColumnData.Rows[i][ColumnColumn.Comment].ToString().Replace("'", "''");

                            comment += $"\r\n\r\nCOMMENT ON COLUMN {cboSchema.Text}.{txtTableName.Text}.{columnName} IS '{comment2}';";
                        }
                    }
                }

                if (_dtIndexesData?.Rows.Count > 0)
                {
                    for (var i = 0; i < _dtIndexesData.Rows.Count; i++)
                    {
                        var indexName = _dtIndexesData.Rows[i].GetSafeString(IndexesColumn.IndexesName);

                        if (string.IsNullOrEmpty(indexesNameLengthErrorMessage) && indexName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Index name length exceeds limit! The maximum length of index name is {length} characters.", "form", GetType().Name, "msg", "IndexNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            indexesNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        var indexPid1 = _dtIndexesData.Rows[i].GetSafeString(IndexesColumn.IndexesPid);
                        var indexType = TextHelper.GetKeyFromDictionary(_indexTypeDisplayValues, _dtIndexesData.Rows[i].GetSafeString(IndexesColumn.IndexesType));
                        var indexColumn = string.Empty;

                        if (string.Equals(indexType, "UNIQUE", StringComparison.OrdinalIgnoreCase) || string.Equals(indexType, "BITMAP", StringComparison.OrdinalIgnoreCase))
                        {
                            indexType = $" {indexType}";
                        }
                        else
                        {
                            indexType = string.Empty;
                        }

                        for (var j = 0; j < _dtIndexExpressionsData.Rows.Count; j++)
                        {
                            var dr = _dtIndexExpressionsData.Rows[j];

                            var columnName = dr.GetSafeString(IndexExpressionsColumn.ColumnName);
                            var checkedValue = dr.GetSafeString(IndexExpressionsColumn.Checked);
                            var orderBy = dr.GetSafeString(IndexExpressionsColumn.OrderBy);
                            var indexPid2 = dr.GetSafeString(IndexExpressionsColumn.IndexesPid);

                            if (string.Equals(indexPid2, indexPid1, StringComparison.OrdinalIgnoreCase) && (checkedValue == "1" || string.Equals(checkedValue, "TRUE", StringComparison.OrdinalIgnoreCase)))
                            {
                                if (!string.Equals(orderBy, "ASC", StringComparison.OrdinalIgnoreCase) && !string.Equals(orderBy, "DESC", StringComparison.OrdinalIgnoreCase))
                                {
                                    orderBy = string.Empty;
                                }
                                else
                                {
                                    orderBy = $" {orderBy}";
                                }

                                indexColumn += $"{columnName}{orderBy}, ";
                            }
                        }

                        if (!string.IsNullOrEmpty(indexColumn))
                        {
                            indexColumn = indexColumn.Substring(0, indexColumn.Length - 2);
                        }
                        else
                        {
                            var temp = LocalizationHelper.GetLanguageString("The index named '{0}' does not specify any index expression", "form", GetType().Name, "msg", "IndexError", "Text").Replace("{0}", indexName);

                            //沒有勾選 Indexes 欄位名稱！
                            indexesErrorMessage = $"\r\n--{temp}";
                        }

                        var lineBreak = i == _dtIndexesData.Rows.Count - 1 ? string.Empty : "\r\n\r\n";

                        indexes += $"CREATE{indexType} INDEX {indexName}\r\nON {cboSchema.Text}.{txtTableName.Text} ({indexColumn});{indexesErrorMessage}{lineBreak}";
                    }

                    indexes = $"\r\n\r\n{indexes}";
                }

                if (!string.IsNullOrEmpty(primaryKey))
                {
                    primaryKey = primaryKey.Substring(0, primaryKey.Length - 2);

                    var primaryKeyTemp = string.Empty;
                    var enabled = string.Empty;
                    var validate = string.Empty;
                    var deferrableState = string.Empty;

                    if (_dtPrimaryKeyData == null || _dtPrimaryKeyData.Rows.Count == 0)
                    {
                        //20240412 使用者在 Column 勾選了 PK，但沒有切換到 Primary Key 頁籤，使用預設 Primary Key Name
                        primaryKeyTemp = $"ALTER TABLE {cboSchema.Text}.{txtTableName.Text} ADD\r\n(\r\n  CONSTRAINT {txtTableName.Text}_PK\r\n  PRIMARY KEY ({primaryKey})\r\n";
                        primaryKeyTemp += "  ENABLE VALIDATE\r\n);";

                        primaryKey = $"\r\n\r\n{primaryKeyTemp}";
                    }
                    else
                    {
                        var dr = _dtPrimaryKeyData.Rows[0];
                        var primaryKeyName = dr.GetSafeString(PrimaryKeyColumn.PrimaryKeyName);

                        if (primaryKeyName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Primary key name length exceeds limit! The maximum length of primary key name is {length} characters.", "form", GetType().Name, "msg", "PrimaryKeyNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            primaryKeyNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        primaryKeyTemp = $"ALTER TABLE {cboSchema.Text}.{txtTableName.Text} ADD\r\n(\r\n  CONSTRAINT {primaryKeyName}\r\n  PRIMARY KEY ({primaryKey})\r\n";
                        enabled = dr.GetSafeString(PrimaryKeyColumn.Enabled);
                        validate = dr.GetSafeString(PrimaryKeyColumn.Validate);
                        deferrableState = TextHelper.GetKeyFromDictionary(_deferrableDisplayValues, _dtPrimaryKeyData.Rows[0][PrimaryKeyColumn.DeferrableState].ToString());

                        if (deferrableState == "InitiallyImmediate")
                        {
                            primaryKeyTemp += "  DEFERRABLE INITIALLY IMMEDIATE\r\n";
                        }
                        else if (deferrableState == "InitiallyDeferred")
                        {
                            primaryKeyTemp += "  DEFERRABLE INITIALLY DEFERRED\r\n";
                        }

                        primaryKeyTemp += (enabled == "1" || string.Equals(enabled, "TRUE", StringComparison.OrdinalIgnoreCase)) ? "  ENABLE" : "  DISABLE";
                        primaryKeyTemp += (validate == "1" || string.Equals(validate, "TRUE", StringComparison.OrdinalIgnoreCase)) ? " VALIDATE\r\n);" : " NOVALIDATE\r\n);";

                        primaryKey = $"\r\n\r\n{primaryKeyTemp}";
                    }
                }

                if (_dtUniqueData?.Rows.Count > 0)
                {
                    unique = $"ALTER TABLE {cboSchema.Text}.{txtTableName.Text} ADD\r\n(";

                    for (var i = 0; i < _dtUniqueData.Rows.Count; i++)
                    {
                        var dr = _dtUniqueData.Rows[i];
                        var uniquePid = dr.GetSafeString(UniqueColumn.UniquePid);
                        var uniqueName = dr.GetSafeString(UniqueColumn.UniqueName);

                        if (string.IsNullOrEmpty(uniqueNameLengthErrorMessage) && uniqueName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Unique name length exceeds limit! The maximum length of unique name is {length} characters.", "form", GetType().Name, "msg", "UniqueNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            uniqueNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        var enabled = dr.GetSafeString(UniqueColumn.Enabled);
                        var validate = dr.GetSafeString(UniqueColumn.Validate);
                        var deferrableState = TextHelper.GetKeyFromDictionary(_deferrableDisplayValues, dr.GetSafeString(UniqueColumn.DeferrableState));
                        var uniqueColumn = string.Empty;

                        for (var j = 0; j < _dtUniqueConstraintsData.Rows.Count; j++)
                        {
                            var dr2 = _dtUniqueConstraintsData.Rows[j];
                            var columnName = dr2.GetSafeString(UniqueConstraintsColumn.ColumnName);
                            var checkedValue = dr2.GetSafeString(UniqueConstraintsColumn.Checked);
                            var uniquePid2 = dr2.GetSafeString(UniqueConstraintsColumn.UniquePid);

                            if (uniquePid2 == uniquePid && (checkedValue == "1" || string.Equals(checkedValue, "TRUE", StringComparison.OrdinalIgnoreCase)))
                            {
                                uniqueColumn += $"{columnName}, ";
                            }
                        }

                        if (!string.IsNullOrEmpty(uniqueColumn))
                        {
                            uniqueColumn = uniqueColumn.Substring(0, uniqueColumn.Length - 2);
                        }
                        else
                        {
                            var temp = LocalizationHelper.GetLanguageString("The unique named '{0}' does not specify any index expression", "form", GetType().Name, "msg", "UniqueError", "Text").Replace("{0}", uniqueName);

                            //沒有勾選 Unique 欄位名稱！
                            uniqueErrorMessage += $"\r\n--{temp}";
                        }

                        unique += i == 0 ? "\r\n" : ",\r\n";
                        unique += $"  CONSTRAINT {uniqueName}\r\n  UNIQUE ({uniqueColumn})\r\n";

                        if (deferrableState == "InitiallyImmediate")
                        {
                            unique += "  DEFERRABLE INITIALLY IMMEDIATE\r\n";
                        }
                        else if (deferrableState == "InitiallyDeferred")
                        {
                            unique += "  DEFERRABLE INITIALLY DEFERRED\r\n";
                        }

                        unique += (enabled == "1" || string.Equals(enabled, "TRUE", StringComparison.OrdinalIgnoreCase)) ? "  ENABLE" : "  DISABLE";
                        unique += (validate == "1" || string.Equals(validate, "TRUE", StringComparison.OrdinalIgnoreCase)) ? " VALIDATE" : " NOVALIDATE";
                        unique += i == _dtUniqueData.Rows.Count - 1 ? "\r\n" : string.Empty;
                    }

                    unique = $"\r\n\r\n{unique});{uniqueErrorMessage}";
                }

                if (_dtForeignKeyData?.Rows.Count > 0)
                {
                    foreignKey = $"ALTER TABLE {cboSchema.Text}.{txtTableName.Text} ADD\r\n(";

                    for (var i = 0; i < _dtForeignKeyData.Rows.Count; i++)
                    {
                        var dr = _dtForeignKeyData.Rows[i];
                        var foreignKeyName = dr.GetSafeString(ForeignKeyColumn.ForeignKeyName);

                        if (string.IsNullOrEmpty(foreignKeyNameLengthErrorMessage) && foreignKeyName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Foreign key name length exceeds limit! The maximum length of foreign key name is {length} characters.", "form", GetType().Name, "msg", "ForeignKeyNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            foreignKeyNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        var foreignKeyPid = dr.GetSafeString(ForeignKeyColumn.ForeignKeyPid);
                        var enabled = dr.GetSafeString(ForeignKeyColumn.Enabled);
                        var validate = dr.GetSafeString(ForeignKeyColumn.Validate);
                        var deferrableState = TextHelper.GetKeyFromDictionary(_deferrableDisplayValues, dr.GetSafeString(ForeignKeyColumn.DeferrableState));
                        var foreignKeyOnDelete = TextHelper.GetKeyFromDictionary(_onDeleteDisplayValues, dr.GetSafeString(ForeignKeyColumn.OnDelete));
                        var foreignKeyThisTableColumn = string.Empty;
                        var foreignKeyThisTableColumnQty = 0;
                        var foreignKeyReferencedTableColumn = string.Empty;
                        var foreignKeyReferencedTableColumnQty = 0;

                        for (var j = 0; j < _dtForeignKeyThisTableData.Rows.Count; j++)
                        {
                            var dr2 = _dtForeignKeyThisTableData.Rows[j];
                            var columnName = dr2.GetSafeString(ForeignKeyThisTableColumn.ColumnName);
                            var checkedValue = dr2.GetSafeString(ForeignKeyThisTableColumn.Checked);
                            var foreignKeyPid2 = dr2.GetSafeString(ForeignKeyThisTableColumn.ForeignKeyPid);

                            if (foreignKeyPid2 == foreignKeyPid && (checkedValue == "1" || string.Equals(checkedValue, "TRUE", StringComparison.OrdinalIgnoreCase)))
                            {
                                foreignKeyThisTableColumn += $"{columnName}, ";
                            }
                        }

                        if (!string.IsNullOrEmpty(foreignKeyThisTableColumn))
                        {
                            foreignKeyThisTableColumn = foreignKeyThisTableColumn.Substring(0, foreignKeyThisTableColumn.Length - 2);
                            foreignKeyThisTableColumnQty = foreignKeyThisTableColumn.Length - foreignKeyThisTableColumn.Replace(",", string.Empty).Length;
                        }
                        else
                        {
                            var temp = LocalizationHelper.GetLanguageString("No columns of this table were selected in the foreign key constraint named '{0}'", "form", GetType().Name, "msg", "ForeignKeyErrorThisTable", "Text").Replace("{0}", foreignKeyName);

                            //沒有勾選 This Table 欄位名稱！
                            foreignKeyErrorMessageThisTable += $"\r\n--{temp}";
                        }

                        for (var j = 0; j < _dtForeignKeyReferencedTableData.Rows.Count; j++)
                        {
                            var dr2 = _dtForeignKeyReferencedTableData.Rows[j];
                            var columnName = dr2.GetSafeString(ForeignKeyReferencedTableColumn.ColumnName);
                            var checkedValue = dr2.GetSafeString(ForeignKeyReferencedTableColumn.Checked);
                            var foreignKeyPid2 = dr2.GetSafeString(ForeignKeyReferencedTableColumn.ForeignKeyPid);

                            if (foreignKeyPid2 == foreignKeyPid && (checkedValue == "1" || string.Equals(checkedValue, "TRUE", StringComparison.OrdinalIgnoreCase)))
                            {
                                foreignKeyReferencedTableColumn += $"{columnName}, ";
                            }
                        }

                        if (!string.IsNullOrEmpty(foreignKeyReferencedTableColumn))
                        {
                            foreignKeyReferencedTableColumn = foreignKeyReferencedTableColumn.Substring(0, foreignKeyReferencedTableColumn.Length - 2);
                            foreignKeyReferencedTableColumnQty = foreignKeyReferencedTableColumn.Length - foreignKeyReferencedTableColumn.Replace(",", string.Empty).Length;
                        }
                        else
                        {
                            var temp = LocalizationHelper.GetLanguageString("No columns of referenced table were selected in the foreign key constraint named '{0}'", "form", GetType().Name, "msg", "ForeignKeyErrorReferencedTable", "Text").Replace("{0}", foreignKeyName);

                            //沒有勾選 Referenced Table 欄位名稱！
                            foreignKeyErrorMessageReferencedTable += $"\r\n--{temp}";
                        }

                        if (foreignKeyThisTableColumnQty != foreignKeyReferencedTableColumnQty)
                        {
                            var temp = LocalizationHelper.GetLanguageString("The number of columns of this table and referenced table do not match in the foreign key constraint named '{0}'", "form", GetType().Name, "msg", "ForeignKeyError", "Text").Replace("{0}", foreignKeyName);

                            //This Table 勾選欄位數量與 Referenced Table 不一致！
                            foreignKeyErrorMessage += $"\r\n--{temp}";
                        }

                        var foreignKeyReferencedSchema = _dtForeignKeyData.Rows[i].GetSafeString(ForeignKeyColumn.ForeignKeyReferencedSchema);
                        var foreignKeyReferencedTable = _dtForeignKeyData.Rows[i].GetSafeString(ForeignKeyColumn.ForeignKeyReferencedTable);

                        foreignKey += i == 0 ? "\r\n" : ",\r\n";
                        foreignKey += $"  CONSTRAINT {foreignKeyName}\r\n  FOREIGNKEY ({foreignKeyThisTableColumn})\r\n  REFERENCES {foreignKeyReferencedSchema}.{foreignKeyReferencedTable} ({foreignKeyReferencedTableColumn})\r\n";

                        if (deferrableState == "InitiallyImmediate")
                        {
                            foreignKey += "  DEFERRABLE INITIALLY IMMEDIATE\r\n";
                        }
                        else if (deferrableState == "InitiallyDeferred")
                        {
                            foreignKey += "  DEFERRABLE INITIALLY DEFERRED\r\n";
                        }

                        if (foreignKeyOnDelete == "Cascade")
                        {
                            foreignKey += "  ON DELETE CASCADE\r\n";
                        }
                        else if (foreignKeyOnDelete == "SetNull")
                        {
                            foreignKey += "  ON DELETE SET NULL\r\n";
                        }

                        foreignKey += (enabled == "1" || string.Equals(enabled, "TRUE", StringComparison.OrdinalIgnoreCase)) ? "  ENABLE" : "  DISABLE";
                        foreignKey += (validate == "1" || string.Equals(validate, "TRUE", StringComparison.OrdinalIgnoreCase)) ? " VALIDATE" : " NOVALIDATE";
                        foreignKey += i == _dtForeignKeyData.Rows.Count - 1 ? "\r\n" : string.Empty;
                    }

                    foreignKey = $"\r\n\r\n{foreignKey});{foreignKeyErrorMessageThisTable}{foreignKeyErrorMessageReferencedTable}{foreignKeyErrorMessage}";
                }

                if (_dtCheckData?.Rows.Count > 0)
                {
                    check = $"ALTER TABLE {cboSchema.Text}.{txtTableName.Text} ADD\r\n(";

                    for (var i = 0; i < _dtCheckData.Rows.Count; i++)
                    { 
                        var dr = _dtCheckData.Rows[i];
                        var checkName = dr.GetSafeString(CheckColumn.CheckName);

                        if (string.IsNullOrEmpty(checkNameLengthErrorMessage) && checkName.Length > _identifierLengthLimit)
                        {
                            var temp = LocalizationHelper.GetLanguageString("Check name length exceeds limit! The maximum length of check name is {length} characters.", "form", GetType().Name, "msg", "CheckNameLengthError", "Text").Replace("{length}", _identifierLengthLimit.ToString());

                            checkNameLengthErrorMessage = $"\r\n--{temp}";
                        }

                        var enabled = dr.GetSafeString(CheckColumn.Enabled);
                        var validate = dr.GetSafeString(CheckColumn.Validate);
                        var deferrableState = TextHelper.GetKeyFromDictionary(_deferrableDisplayValues, dr.GetSafeString(CheckColumn.DeferrableState));
                        var checkCondition = dr.GetSafeString(CheckColumn.CheckCondition);
                        var checkName2 = dr.GetSafeString(CheckColumn.CheckName);

                        check += i == 0 ? "\r\n" : ",\r\n";
                        check += $"  CONSTRAINT {checkName2}\r\n  CHECK ({checkCondition})\r\n";

                        if (string.IsNullOrEmpty(checkCondition))
                        {
                            var temp = LocalizationHelper.GetLanguageString("The check constraint named '{0}' does not define a condition", "form", GetType().Name, "msg", "CheckError", "Text").Replace("{0}", checkName);

                            //沒有輸入 Check Condition！
                            checkErrorMessage += $"\r\n--{temp}";
                        }

                        if (deferrableState == "InitiallyImmediate")
                        {
                            check += "  DEFERRABLE INITIALLY IMMEDIATE\r\n";
                        }
                        else if (deferrableState == "InitiallyDeferred")
                        {
                            check += "  DEFERRABLE INITIALLY DEFERRED\r\n";
                        }

                        check += (enabled == "1" || string.Equals(enabled, "TRUE", StringComparison.OrdinalIgnoreCase)) ? "  ENABLE" : "  DISABLE";
                        check += (validate == "1" || string.Equals(validate, "TRUE", StringComparison.OrdinalIgnoreCase)) ? " VALIDATE" : " NOVALIDATE";
                        check += i == _dtCheckData.Rows.Count - 1 ? "\r\n" : string.Empty;
                    }

                    check = $"\r\n\r\n{check});{checkErrorMessage}";
                }

                switch (cboTableType.Text)
                {
                    case "Temporary (Transaction)":
                        {
                            tableType = "\r\nON COMMIT DELETE ROWS;";
                            break;
                        }
                    case "Temporary (Session)":
                        {
                            tableType = "\r\nON COMMIT PRESERVE ROWS;";
                            break;
                        }
                    case "Index Organized":
                        {
                            tableType = "\r\nORGANIZATION INDEX;";
                            break;
                        }
                    case "External":
                        {
                            tableType = "\r\nORGANIZATION EXTERNAL;";
                            break;
                        }
                    default:
                        {
                            tableType = ";";
                            break;
                        }
                }

                var sbCreateScript = new StringBuilder();

                sbCreateScript.AppendLine($"CREATE TABLE {cboSchema.Text}.{txtTableName.Text}").AppendLine("(");

                var columnNameValue = 0;
                var dataTypeValue = 0;
                var invisibleValue = 0;
                var defaultValue = 0;
                var notNullValue = 0;

                for (var i = 0; i < dtCreate.Rows.Count; i++)
                {
                    var dr = dtCreate.Rows[i];

                    columnNameValue = Math.Max(columnNameValue, dr.GetSafeString("ColumnName").Length);
                    dataTypeValue = Math.Max(dataTypeValue, dr.GetSafeString("DataType").Length);
                    invisibleValue = Math.Max(invisibleValue, dr.GetSafeString("Invisible").Length);
                    defaultValue = Math.Max(defaultValue, dr.GetSafeString("Default").Length);
                    notNullValue = Math.Max(notNullValue, dr.GetSafeString("NotNull").Length);
                }

                for (var i = 0; i < dtCreate.Rows.Count; i++)
                {
                    var dr = dtCreate.Rows[i];

                    var columnName = dr.GetSafeString("ColumnName");
                    var columnNameLength = Math.Max(0, columnNameValue - columnName.Length);
                    var columnName2 = new string(' ', columnNameLength);
                    var dataType = dr.GetSafeString("DataType");
                    var dataTypeLength = Math.Max(0, dataTypeValue - dataType.Length);
                    var dataType2 = new string(' ', dataTypeLength);

                    sbCreateScript.Append($"  {columnName}{columnName2}");
                    sbCreateScript.Append($"  {dataType}{dataType2}");

                    if (invisibleValue > 0)
                    {
                        var invisible = dr.GetSafeString("Invisible");
                        var invisibleLength = Math.Max(0, invisibleValue - invisible.Length);
                        var invisible2 = new string(' ', invisibleLength);

                        sbCreateScript.Append($"  {invisible}{invisible2}");
                    }

                    if (defaultValue > 0)
                    {
                        var defaultString = dr.GetSafeString("Default");
                        var defaultLength = Math.Max(0, defaultValue - defaultString.Length);
                        var defaultString2 = new string(' ', defaultLength);

                        sbCreateScript.Append($"  {defaultString}{defaultString2}");
                    }

                    if (notNullValue > 0)
                    {
                        var notNull = dr.GetSafeString("NotNull");
                        var notNullLength = Math.Max(0, notNullValue - notNull.Length);
                        var notNull2 = new string(' ', notNullLength);

                        sbCreateScript.Append($"  {notNull}{notNull2}");
                    }

                    if (i == dtCreate.Rows.Count - 1)
                    {
                        sbCreateScript.Append($"\r\n){tableType}{columnNameLengthErrorMessage}");
                    }
                    else
                    {
                        sbCreateScript.AppendLine(",");
                    }
                }

                if (!string.IsNullOrEmpty(txtTableComment.Text))
                {
                    sbCreateScript.Append($"\r\n\r\nCOMMENT ON TABLE {cboSchema.Text}.{txtTableName.Text} IS '{txtTableComment.Text.Replace("'", "''")}';");
                }

                sbCreateScript.Append(comment);
                sbCreateScript.Append(indexes).Append(indexesNameLengthErrorMessage);
                sbCreateScript.Append(primaryKey).Append(primaryKeyNameLengthErrorMessage);
                sbCreateScript.Append(unique).Append(uniqueNameLengthErrorMessage);
                sbCreateScript.Append(foreignKey).Append(foreignKeyNameLengthErrorMessage);
                sbCreateScript.Append(check).Append(checkNameLengthErrorMessage);

                editorSql.ReadOnly = false;
                editorSql.Text = sbCreateScript.ToString();
                editorSql.ReadOnly = true;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SelectAllSqlPreview();
        }

        private void SelectAllSqlPreview()
        {
            editorSql.SelectionStart = 0;
            editorSql.SelectionEnd = editorSql.Text.Length;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopySqlPreview();
        }

        private void CopySqlPreview()
        {
            Clipboard.Clear();
            editorSql.Copy(CopyFormat.Text | CopyFormat.Rtf | CopyFormat.Html);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveAsSqlPreview();
        }

        private void SaveAsSqlPreview()
        {
            var sf = new SaveFileDialog
            {
                Title = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text"),
                FileName = string.Empty,
                RestoreDirectory = true,
                Filter = @"Query file (*.sql)|*.sql|All files (*.*)|*.*".Replace("All files", LocalizationHelper.GetLanguageString("All files", "Global", "Global", "msg", "AllFiles", "Text")).Replace("Query file", LocalizationHelper.GetLanguageString("Query file", "Global", "Global", "msg", "QueryFile", "Text"))
            };

            if (sf.ShowDialog() != DialogResult.OK)
            {
                return; //無論檔案是否存在，只要不是按「取消」或「否」，都會回傳 OK
            }

            try
            {
                if (string.IsNullOrEmpty(Path.GetExtension(sf.FileName)))
                {
                    sf.FileName += ".sql";
                }

                TextEngine.WriteContentToFile(editorSql.Text, sf.FileName, TextEncodes.UTF8);
            }
            catch (Exception ex)
            {
                var sSaveAs = LocalizationHelper.GetLanguageString("Save As", "Global", "Global", "msg", "SaveAs", "Text");

                MessageBox.Show($"{sSaveAs}\r\n\r\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            WordWrapSqlPreview();
        }

        private void WordWrapSqlPreview()
        {
            btnWordWrap.Visible = editorSql.WrapMode == WrapMode.Word;
            btnWordWrap2.Visible = !btnWordWrap.Visible;
            editorSql.WrapMode = editorSql.WrapMode == WrapMode.Word ? WrapMode.None : WrapMode.Word;
            editorSql.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? WrapVisualFlags.Start : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? WrapVisualFlags.End : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? WrapVisualFlags.Margin : WrapVisualFlags.None);

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSql.ScrollCaret();
        }

        private void btnShowAllCharacters_Click(object sender, EventArgs e)
        {
            ShowallCharactersSqlPreview();
        }

        private void ShowallCharactersSqlPreview()
        {
            btnShowAllCharacters.Visible = editorSql.ViewEol;
            btnShowAllCharacters2.Visible = !btnShowAllCharacters.Visible;
            editorSql.ViewEol = !editorSql.ViewEol;
            editorSql.ViewWhitespace = btnShowAllCharacters.Visible ? WhitespaceMode.Invisible : WhitespaceMode.VisibleAlways;
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            editorSql.ZoomIn();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            editorSql.ZoomOut();
        }

        private void CheckIndexesButtons()
        {
            var currentRow = c1GridIndexes.Row;

            btnIndexesAdd.Enabled = true;

            if (currentRow == 0)
            {
                btnIndexesUp.Enabled = false;
                btnIndexesTop.Enabled = false;
            }
            else
            {
                btnIndexesUp.Enabled = true;
                btnIndexesTop.Enabled = true;
            }

            if (_dtIndexesData == null || currentRow == _dtIndexesData.Rows.Count - 1)
            {
                btnIndexesDown.Enabled = false;
                btnIndexesBottom.Enabled = false;
            }
            else
            {
                btnIndexesDown.Enabled = true;
                btnIndexesBottom.Enabled = true;
            }

            if (_dtIndexesData == null || _dtIndexesData.Rows.Count == 0)
            {
                btnIndexesRemove.Enabled = false;
            }
            else
            {
                btnIndexesRemove.Enabled = true;
            }
        }

        private void CheckPrimaryKeyButtons()
        {
            if (_dtPrimaryKeyData == null || _dtPrimaryKeyData.Rows.Count == 0)
            {
                btnPrimaryKeyAdd.Enabled = true;
                btnPrimaryKeyRemove.Enabled = false;
            }
            else
            {
                btnPrimaryKeyAdd.Enabled = false;
                btnPrimaryKeyRemove.Enabled = true;
            }

            if (_dtPrimaryKeyData == null || _dtPrimaryKeyData.Rows.Count == 0)
            {
                btnPrimaryKeyRemove.Enabled = false;
            }
            else
            {
                btnPrimaryKeyRemove.Enabled = true;
            }
        }

        private void CheckUniqueButtons()
        {
            var currentRow = c1GridUnique.Row;

            btnUniqueAdd.Enabled = true;

            if (currentRow == 0)
            {
                btnUniqueUp.Enabled = false;
                btnUniqueTop.Enabled = false;
            }
            else
            {
                btnUniqueUp.Enabled = true;
                btnUniqueTop.Enabled = true;
            }

            if (_dtUniqueData == null || currentRow == _dtUniqueData.Rows.Count - 1)
            {
                btnUniqueDown.Enabled = false;
                btnUniqueBottom.Enabled = false;
            }
            else
            {
                btnUniqueDown.Enabled = true;
                btnUniqueBottom.Enabled = true;
            }

            if (_dtUniqueData == null || _dtUniqueData.Rows.Count == 0)
            {
                btnUniqueRemove.Enabled = false;
            }
            else
            {
                btnUniqueRemove.Enabled = true;
            }
        }

        private void CheckForeignKeyButtons()
        {
            var currentRow = c1GridForeignKey.Row;

            if (currentRow == 0)
            {
                btnForeignKeyUp.Enabled = false;
                btnForeignKeyTop.Enabled = false;
            }
            else
            {
                btnForeignKeyUp.Enabled = true;
                btnForeignKeyTop.Enabled = true;
            }

            if (_dtForeignKeyData == null || currentRow == _dtForeignKeyData.Rows.Count - 1)
            {
                btnForeignKeyDown.Enabled = false;
                btnForeignKeyBottom.Enabled = false;
            }
            else
            {
                btnForeignKeyDown.Enabled = true;
                btnForeignKeyBottom.Enabled = true;
            }

            if (_dtForeignKeyData == null || _dtForeignKeyData.Rows.Count == 0)
            {
                btnForeignKeyRemove.Enabled = false;
                pnlForeignKeyReferencedTable.Enabled = false;
            }
            else
            {
                btnForeignKeyRemove.Enabled = true;
                pnlForeignKeyReferencedTable.Enabled = true;
            }
        }

        private void CheckForeignKeyThisTableButtons()
        {
            var currentRow = c1GridForeignKeyThisTable.Row;

            if (currentRow == 0)
            {
                btnForeignKeyThisTableUp.Enabled = false;
                btnForeignKeyThisTableTop.Enabled = false;
            }
            else
            {
                btnForeignKeyThisTableUp.Enabled = true;
                btnForeignKeyThisTableTop.Enabled = true;
            }

            if (_dtForeignKeyThisTableData == null || currentRow == _dtForeignKeyThisTableData.Rows.Count - 1)
            {
                btnForeignKeyThisTableDown.Enabled = false;
                btnForeignKeyThisTableBottom.Enabled = false;
            }
            else
            {
                btnForeignKeyThisTableDown.Enabled = true;
                btnForeignKeyThisTableBottom.Enabled = true;
            }
        }

        private void CheckForeignKeyReferencedTableButtons()
        {
            var currentRow = c1GridForeignKeyReferencedTable.Row;

            if (currentRow == 0)
            {
                btnForeignKeyReferencedTableUp.Enabled = false;
                btnForeignKeyReferencedTableTop.Enabled = false;
            }
            else
            {
                btnForeignKeyReferencedTableUp.Enabled = true;
                btnForeignKeyReferencedTableTop.Enabled = true;
            }

            if (_dtForeignKeyReferencedTableData == null || currentRow == _dtForeignKeyReferencedTableData.Rows.Count - 1)
            {
                btnForeignKeyReferencedTableDown.Enabled = false;
                btnForeignKeyReferencedTableBottom.Enabled = false;
            }
            else
            {
                btnForeignKeyReferencedTableDown.Enabled = true;
                btnForeignKeyReferencedTableBottom.Enabled = true;
            }
        }

        private void CheckCheckButtons()
        {
            var currentRow = c1GridCheck.Row;

            if (currentRow == 0)
            {
                btnCheckUp.Enabled = false;
                btnCheckTop.Enabled = false;
            }
            else
            {
                btnCheckUp.Enabled = true;
                btnCheckTop.Enabled = true;
            }

            if (_dtCheckData == null || _dtCheckData.Rows.Count == 0 || currentRow == _dtCheckData.Rows.Count - 1)
            {
                btnCheckDown.Enabled = false;
                btnCheckBottom.Enabled = false;
            }
            else
            {
                btnCheckDown.Enabled = true;
                btnCheckBottom.Enabled = true;
            }

            if (_dtCheckData?.Rows.Count > 0)
            {
                btnCheckRemove.Enabled = true;
                btnCheckInsert.Enabled = true;
            }
            else
            {
                btnCheckRemove.Enabled = false;
                btnCheckInsert.Enabled = false;
            }

            if (_dtCheckData == null || _dtCheckData.Rows.Count == 0)
            {
                btnCheckRemove.Enabled = false;
            }
            else
            {
                btnCheckRemove.Enabled = true;
            }
        }

        private void txtTableName_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTableName.Text))
            {
                txtTableName.Text = "NEWTABLE";
            }
        }

        private void cboForeignKeySchema_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            GetAllTables4ForeignKeySchema();
            Cursor = Cursors.Default;
        }

        private void GetAllTables4ForeignKeySchema()
        {
            try
            {
                var sbSql = new StringBuilder();

                SqlTraceHelper.AppendHeader(sbSql, "---Get Schema Name");

                sbSql.AppendLine("SELECT Object_Name AS SchemaName, s.*");
                sbSql.AppendLine("  FROM All_Objects s");
                sbSql.AppendLine(" WHERE s.OBJECT_TYPE = 'TABLE'");
                sbSql.AppendLine($"   AND UPPER(Owner) = '{cboForeignKeySchema.Text}'");
                sbSql.Append(" ORDER BY Object_Name");

                var sql = sbSql.ToString();

                DataTable dtTemp = null;

                DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtTemp);
                UIHelper.SetC1ComboBoxItemsFromDataTable(cboForeignKeyTable, dtTemp);
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cboForeignKeyTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (TextHelper.IsNullOrEmptyTag(c1GridForeignKeyReferencedTable.Tag))
                {
                    //尚未按下新增按鈕，不處理
                    return;
                }

                if (_isForeignKeySelectionSyncing)
                {
                    _isForeignKeySelectionSyncing = false;
                    return;
                }

                Cursor = Cursors.WaitCursor;

                //刪除舊資料
                if (!string.IsNullOrEmpty(cboForeignKeyTable.Text))
                {
                    var drDeleted = _dtForeignKeyReferencedTableData.Select($"ForeignKeyPID = '{c1GridForeignKeyReferencedTable.Tag}'");

                    foreach (var row in drDeleted)
                    {
                        row.Delete();
                    }

                    _dtForeignKeyReferencedTableData.AcceptChanges();

                    //更新 Schema 及 Table 值
                    var dr = _dtForeignKeyData.Select($"ForeignKeyPID = '{c1GridForeignKeyReferencedTable.Tag}'");

                    if (dr.Length > 0)
                    {
                        dr[0][ForeignKeyColumn.ForeignKeyReferencedSchema] = cboForeignKeySchema.Text;
                        dr[0][ForeignKeyColumn.ForeignKeyReferencedTable] = cboForeignKeyTable.Text;
                    }

                    _dtForeignKeyData.AcceptChanges();
                }

                DataTable dtTemp = null;

                //取得指定的 Table 欄位資訊
                //取得 Constraint 資訊
                var sbSql = new StringBuilder();

                SqlTraceHelper.AppendHeader(sbSql, "---Get Table Constraint Information");

                sbSql.AppendLine("SELECT cols.Table_Name AS TableName, cols.Column_Name AS ColumnName,");
                sbSql.AppendLine("       cons.Constraint_Type AS ConstraintType, cons.Constraint_Name AS ConstraintName");
                sbSql.AppendLine("  FROM All_Constraints cons, All_Cons_Columns cols");
                sbSql.AppendLine(" WHERE cons.Constraint_Name = cols.Constraint_Name");
                sbSql.AppendLine($"   AND UPPER(cols.Owner) = '{cboForeignKeySchema.Text}'");
                sbSql.Append(" ORDER BY cons.Constraint_Type DESC");

                var sql = sbSql.ToString();

                var dtConstraint = new DataTable();

                ExecuteQueryToDataTable(sql, ref dtConstraint);

                sbSql.Clear();

                SqlTraceHelper.AppendHeader(sbSql, "---Get Column Information");

                sbSql.AppendLine("SELECT 'TABLE' AS Schema_Type, ss.Table_Name AS TableName, ss.Column_Name AS ColumnName,");
                sbSql.AppendLine("       ss.Column_ID AS ColumnID, ss.Data_Type AS DataType, ss.Data_Length AS DataLength, ss.Nullable,");
                sbSql.AppendLine("       ss.Data_Default AS DefaultValue, ss.Data_Precision || ',' || ss.Data_Scale as Scale, cc.Comments");
                sbSql.AppendLine("  FROM User_Tab_Columns ss");
                sbSql.AppendLine("       LEFT JOIN User_Col_Comments cc on (ss.Column_Name = cc.Column_Name AND ss.Table_Name = cc.Table_Name)");
                sbSql.AppendLine(" WHERE ss.Table_Name IN");
                sbSql.AppendLine("       (SELECT Object_Name FROM All_Objects");
                sbSql.AppendLine("         WHERE Object_Type IN ('TABLE')");
                sbSql.AppendLine($"           AND UPPER(Owner) = '{cboForeignKeySchema.Text}')");
                sbSql.AppendLine($"   AND ss.Table_Name = '{cboForeignKeyTable.Text}'");
                sbSql.Append(" ORDER BY TO_NUMBER(ss.Column_ID)");

                sql = sbSql.ToString();

                ExecuteQueryToDataTable(sql, ref dtTemp);

                foreach (DataRow dr in dtTemp?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var sTableName = dr.GetSafeString("TableName");
                    var sColumnName = dr.GetSafeString("ColumnName");
                    var sColumnID = dr.GetSafeString("ColumnID", "0");
                    var sDataType = dr.GetSafeString("DataType").ToUpper();
                    var sDataLength = dr.GetSafeString("DataLength", "0");
                    var sScale = dr.GetSafeString("Scale");
                    var defaultValue = dr.GetSafeString("DefaultValue");
                    var sNullable = dr.GetSafeString("Nullable");
                    var sComments = dr.GetSafeString("Comments");

                    int.TryParse(sColumnID, out var columnID);

                    var row = _dtForeignKeyReferencedTableData.NewRow();

                    row[ForeignKeyReferencedTableColumn.ForeignKeyPid] = TextHelper.GetSafeString(c1GridForeignKeyReferencedTable.Tag);
                    row[ForeignKeyReferencedTableColumn.Checked] = 0;
                    row[ForeignKeyReferencedTableColumn.ColumnName] = sColumnName;
                    row[ForeignKeyReferencedTableColumn.Id] = columnID;

                    switch (sDataType)
                    {
                        case "DATE":
                            {
                                row["DataType"] = "DATE";
                                break;
                            }
                        case "VARCHAR":
                        case "VARCHAR2":
                            {
                                row["DataType"] = $"{sDataType} ({sDataLength})";
                                break;
                            }
                        case "TIMESTAMP(6)":
                            {
                                row["DataType"] = "TIMESTAMP (6)";
                                break;
                            }
                        case "TIMESTAMP(0) WITH TIME ZONE":
                            {
                                row["DataType"] = "TIMESTAMP (0) WITH TIME ZONE";
                                break;
                            }
                        case "TIMESTAMP(6) WITH TIME ZONE":
                            {
                                row["DataType"] = "TIMESTAMP (6) WITH TIME ZONE";
                                break;
                            }
                        case "NUMBER":
                            {
                                if (sScale == "," || sScale == ",0") //20230911 新增 ",0"
                                {
                                    row["DataType"] = sDataType;
                                }
                                else
                                {
                                    row["DataType"] = $"{sDataType} ({sScale})";
                                }

                                break;
                            }
                        default:
                            {
                                row["DataType"] = sDataType;
                                break;
                            }
                    }

                    var sConstraintInfo = string.Empty;

                    if (dtConstraint != null)
                    {
                        var dtRow = DataTableSearchHelper.FindDataTableFirstRow(dtConstraint, ("TableName", sTableName), ("ColumnName", sColumnName));

                        if (dtRow != null)
                        {
                            var sConstraintType = dtRow.GetSafeString("ConstraintType");
                            var sConstraintName = dtRow.GetSafeString("ConstraintName");

                            sConstraintInfo = $"{sConstraintType}, {sConstraintName}";
                        }
                    }

                    row[ForeignKeyReferencedTableColumn.ConstraintInfo] = sConstraintInfo;
                    row[ForeignKeyReferencedTableColumn.Default] = defaultValue;
                    row[ForeignKeyReferencedTableColumn.Nullable] = sNullable;
                    row[ForeignKeyReferencedTableColumn.Comment] = sComments;
                    _dtForeignKeyReferencedTableData.Rows.Add(row);
                }

                Cursor = Cursors.Default;

                var dataView = _dtForeignKeyReferencedTableData.DefaultView;

                dataView.Sort = "ConstraintInfo DESC, ID";
                c1GridForeignKeyReferencedTable.DataSource = _dtForeignKeyReferencedTableData;
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridForeignKeyReferencedTable, Name, true, "gridheader_foreignkeyreferencedtable");
                GridHelper.ResizeGridColumnWidth(c1GridForeignKeyReferencedTable);

                //強制觸發 c1GridForeignKeyReferencedTable_RowColChange
                c1GridForeignKeyReferencedTable.Row = 0;
                c1GridForeignKeyReferencedTable.Col = ForeignKeyReferencedTableColumn.ColumnName;
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ExecuteQueryToDataTable(string sql, ref DataTable dt)
        {
            var oracleReader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

            oracleReader.EnsureConnectionOpen();

            dt = oracleReader.ExecuteQueryToDataTable(sql);
        }

        private void editorSql_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            editorSql.ContextMenuStrip = _sqlPreviewMenu;

            if (MyLibrary.IsDarkMode)
            {
                _sqlPreviewMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _sqlPreviewMenu.ForeColor = Color.White;
                _sqlPreviewMenu.RenderMode = ToolStripRenderMode.System;
            }

            _sqlPreviewMenu.Show(editorSql, new Point(e.X, e.Y));
        }

        private void UpdateEditMode(string sObject)
        {
            _isEditMode = false;

            switch (sObject)
            {
                case "COLUMN":
                    {
                        c1GridColumn.EditActive = false;
                        CheckColumnButtons();
                        break;
                    }
                case "INDEXES":
                    {
                        c1GridIndexes.EditActive = false;
                        CheckIndexesButtons();
                        break;
                    }
                case "PRIMARYKEY":
                    {
                        c1GridPrimaryKey.EditActive = false;
                        CheckPrimaryKeyButtons();
                        break;
                    }
                case "UNIQUE":
                    {
                        c1GridUnique.EditActive = false;
                        CheckUniqueButtons();
                        break;
                    }
                case "FOREIGNKEY":
                    {
                        c1GridForeignKey.EditActive = false;
                        CheckForeignKeyButtons();
                        break;
                    }
                case "CHECK":
                    {
                        c1GridCheck.EditActive = false;
                        CheckCheckButtons();
                        break;
                    }
            }
        }

        private int GetNextSequence(DataTable dt, int columnIndex, string sKeyword)
        {
            var iNext = 1;

            try
            {
                if (dt?.Rows.Count > 0)
                {
                    var index = ",";

                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        var sValue = dt.Rows[i].GetSafeString(columnIndex);

                        if (sValue.StartsWith(sKeyword, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(sValue.Replace(sKeyword, string.Empty)))
                        {
                            int.TryParse(sValue.Replace(sKeyword, string.Empty), out var iTemp);
                            index += $"{iTemp:0000},";
                        }
                    }

                    if (index != ",")
                    {
                        var sArrayIndex = index.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        Array.Sort(sArrayIndex, string.CompareOrdinal); //ASCII排序
                        int.TryParse(sArrayIndex[sArrayIndex.Length - 1], out iNext);
                        iNext++;
                    }
                }
            }
            catch (Exception ex)
            {
                var message =TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return iNext;
        }

        private void DrawGridBackColor()
        {
            foreach (C1DisplayColumn cd in c1GridColumn.Splits[0].DisplayColumns)
            {
                cd.OwnerDraw = false;
            }

            _modifiedCellPoints.Clear();
            c1GridColumn.ClearCellStyle(CellStyleFlag.AllCells);

            var iCount = c1GridColumn.Splits[0].Rows.Count;

            for (var row = 0; row < iCount; row++)
            {
                var col = 0;
                var sDataType = string.Empty;
                var vr = c1GridColumn.Splits[0].Rows[row];

                foreach (C1DataColumn column in c1GridColumn.Columns)
                {
                    if (col == ColumnColumn.DataType)
                    {
                        sDataType = column.CellText(vr.DataRowIndex);
                    }

                    switch (sDataType)
                    {
                        case "CHAR" when col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "NCHAR" when col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "NVARCHAR2" when col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "VARCHAR2" when col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "RAW" when col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                            {
                                _modifiedCellPoints.Add(new Point(row, col));
                                break;
                            }
                        case "FLOAT" when col == ColumnColumn.Size || col == ColumnColumn.Scale:
                            {
                                _modifiedCellPoints.Add(new Point(row, col));
                                break;
                            }
                        case "NUMBER" when col == ColumnColumn.Size:
                            {
                                _modifiedCellPoints.Add(new Point(row, col));
                                break;
                            }
                        case "BFILE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "BLOB" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "BINARY_DOUBLE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "BINARY_FLOAT" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "CLOB" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "NCLOB" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "DATE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "INTEGER" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "LONG" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "LONG RAW" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "ROWID" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "XMLTYPE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "TIMESTAMP" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "TIMESTAMP WITH TIME ZONE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                        case "TIMESTAMP WITH LOCAL TIME ZONE" when col == ColumnColumn.Size || col == ColumnColumn.Precision || col == ColumnColumn.Scale:
                            {
                                _modifiedCellPoints.Add(new Point(row, col));
                                break;
                            }
                    }

                    col++;
                }
            }

            foreach (C1DisplayColumn cd in c1GridColumn.Splits[0].DisplayColumns)
            {
                cd.OwnerDraw = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Home:
                    {
                        if (c1GridColumn.Focused)
                        {
                            c1GridColumn.Row = 0;
                            c1GridColumn.Select();
                            return true;
                        }
                        else if (c1GridIndexes.Focused)
                        {
                            c1GridIndexes.Row = 0;
                            c1GridIndexes.Select();
                            return true;
                        }
                        else if (c1GridPrimaryKey.Focused)
                        {
                            c1GridPrimaryKey.Row = 0;
                            c1GridPrimaryKey.Select();
                            return true;
                        }
                        else if (c1GridUnique.Focused)
                        {
                            c1GridUnique.Row = 0;
                            c1GridUnique.Select();
                            return true;
                        }
                        else if (c1GridForeignKey.Focused)
                        {
                            c1GridForeignKey.Row = 0;
                            c1GridForeignKey.Select();
                            return true;
                        }
                        else if (c1GridCheck.Focused)
                        {
                            c1GridCheck.Row = 0;
                            c1GridCheck.Select();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.End:
                    {
                        if (c1GridColumn.Focused)
                        {
                            c1GridColumn.Row = c1GridColumn.Splits[0].Rows.Count - 1;
                            c1GridColumn.Select();
                            return true;
                        }
                        else if (c1GridIndexes.Focused)
                        {
                            c1GridIndexes.Row = c1GridIndexes.Splits[0].Rows.Count - 1;
                            c1GridIndexes.Select();
                            return true;
                        }
                        else if (c1GridPrimaryKey.Focused)
                        {
                            c1GridPrimaryKey.Row = c1GridPrimaryKey.Splits[0].Rows.Count - 1;
                            c1GridPrimaryKey.Select();
                            return true;
                        }
                        else if (c1GridUnique.Focused)
                        {
                            c1GridUnique.Row = c1GridUnique.Splits[0].Rows.Count - 1;
                            c1GridUnique.Select();
                            return true;
                        }
                        else if (c1GridForeignKey.Focused)
                        {
                            c1GridForeignKey.Row = c1GridForeignKey.Splits[0].Rows.Count - 1;
                            c1GridForeignKey.Select();
                            return true;
                        }
                        else if (c1GridCheck.Focused)
                        {
                            c1GridCheck.Row = c1GridCheck.Splits[0].Rows.Count - 1;
                            c1GridCheck.Select();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.C:
                    {
                        if (editorSql.Focused)
                        {
                            CopySqlPreview();
                            return true;
                        }

                        break;
                    }
                case Keys.Control | Keys.A:
                    {
                        if (editorSql.Focused)
                        {
                            SelectAllSqlPreview();
                            return true;
                        }

                        break;
                    }
                case Keys.F5:
                    {
                        if (editorSql.Focused)
                        {
                            RefreshSqlPreview();
                            return true;
                        }

                        break;
                    }
                case Keys.F12:
                    {
                        if (editorSql.Focused)
                        {
                            SaveAsSqlPreview();
                            return true;
                        }

                        break;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private static class ColumnColumn
        {
            public const int ColumnPid = 0;
            public const int ColumnOrder = 1;
            public const int ColumnName = 2;
            public const int DataType = 3;
            public const int Size = 4;
            public const int Precision = 5;
            public const int Scale = 6;
            public const int PrimaryKey = 7;
            public const int NotNull = 8;
            public const int Visible = 9;
            public const int Default = 10;
            public const int Comment = 11;
        }

        private static class IndexesColumn
        {
            public const int IndexesPid = 0;
            public const int IndexesName = 1;
            public const int IndexesType = 2;
        }

        private static class IndexExpressionsColumn
        {
            public const int IndexesPid = 0;
            public const int ColumnPid = 1;
            public const int ColumnOrder = 2;
            public const int Checked = 3;
            public const int ColumnName = 4;
            public const int OrderBy = 5;
        }

        private static class PrimaryKeyColumn
        {
            public const int PrimaryKeyPid = 0;
            public const int PrimaryKeyName = 1;
            public const int Enabled = 2;
            public const int Validate = 3;
            public const int DeferrableState = 4;
        }

        private static class PrimaryKeyConstraintsColumn
        {
            public const int PrimaryKeyPid = 0;
            public const int ColumnPid = 1;
            public const int ColumnOrder = 2;
            public const int Checked = 3;
            public const int ColumnName = 4;
        }

        private static class UniqueColumn
        {
            public const int UniquePid = 0;
            public const int UniqueName = 1;
            public const int Enabled = 2;
            public const int Validate = 3;
            public const int DeferrableState = 4;
        }

        private static class UniqueConstraintsColumn
        {
            public const int UniquePid = 0;
            public const int ColumnPid = 1;
            public const int ColumnOrder = 2;
            public const int Checked = 3;
            public const int ColumnName = 4;
        }

        private static class ForeignKeyColumn
        {
            public const int ForeignKeyPid = 0;
            public const int ForeignKeyReferencedSchema = 1;
            public const int ForeignKeyReferencedTable = 2;
            public const int ForeignKeyName = 3;
            public const int Enabled = 4;
            public const int Validate = 5;
            public const int DeferrableState = 6;
            public const int OnDelete = 7;
        }

        private static class ForeignKeyThisTableColumn
        {
            public const int ForeignKeyPid = 0;
            public const int ColumnPid = 1;
            public const int ColumnOrder = 2;
            public const int Checked = 3;
            public const int ColumnName = 4;
        }

        private static class ForeignKeyReferencedTableColumn
        {
            public const int ForeignKeyPid = 0;
            public const int Checked = 1;
            public const int ColumnName = 2;
            public const int Id = 3;
            public const int DataType = 4;
            public const int ConstraintInfo = 5;
            public const int Nullable = 6;
            public const int Default = 7;
            public const int Comment = 8;
        }

        private static class CheckColumn
        {
            public const int CheckName = 0;
            public const int CheckCondition = 1;
            public const int Enabled = 2;
            public const int Validate = 3;
            public const int DeferrableState = 4;
        }

        private static class SqlPreviewColumn
        {
            public const int Refresh = 0;
            public const int Dash1 = 1;
            public const int SelectAll = 2;
            public const int Copy = 3;
            public const int SaveAs = 4;
        }
    }
}