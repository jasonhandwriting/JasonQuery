using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.LazyLoading;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private bool DisplayInfo_MySql(SchemaExplorerSelectionInfo selection)
        {
            var schemaNode = selection.SchemaNode;
            var schemaType = selection.SchemaType;
            var schemaName = selection.SchemaName;

            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;

            lblTableName01.Text = schemaName;
            lblTableName02.Text = schemaName;
            tabData.Text = $"{tabData.Tag} - {schemaName}";

            var sql = string.Empty;
            var sbSql = new StringBuilder();
            var script = string.Empty;

            switch (schemaType)
            {
                case SchemaObjectNames.Tables:
                    {
                        tabTableStructure.TabVisible = true;
                        tabView100RowsTop.TabVisible = false;
                        tabSettings.TabVisible = true;
                        tabData.TabVisible = true;
                        _selectedTableName = schemaName;

                        var filter = GetSetTableFilter(_selectedTableName);

                        if (!string.IsNullOrEmpty(filter))
                        {
                            btnFilterData.Visible = false;
                            btnFilterRedData.Visible = true;
                            btnFilterData.Tag = filter;
                        }
                        else
                        {
                            btnFilterData.Visible = true;
                            btnFilterRedData.Visible = false;
                            btnFilterData.Tag = string.Empty;
                        }

                        if (IsLazyTab(SchemaBrowserLazyTab.SqlPane))
                        {
                            sbSql.Clear();
                            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Creation Script");
                            sbSql.Append($"SHOW CREATE TABLE `{schemaNode}`.`{schemaName}`;");
                            sql = sbSql.ToString();

                            var dtCreateTable = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtCreateTable);

                            if (dtCreateTable?.Rows.Count > 0)
                            {
                                script = dtCreateTable.Rows[0].GetSafeString("Create Table");
                            }

                            break;
                        }

                        CreateTableSchemaTable();

                        //查詢前 500筆資料
                        var dataFilter = IsLazyTab(SchemaBrowserLazyTab.TableData) ? TextHelper.GetSafeString(btnFilterData.Tag) : string.Empty;

                        sql = BuildTableDataPreviewSql(selection, dataFilter);

                        var pageLength = IsLazyTab(SchemaBrowserLazyTab.TableData) ? 500 : 0;
                        var errorMessage = ExecuteQuery100Rows(sql, 0, pageLength, false, true);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            if (TextHelper.CheckTextStartEndWith(errorMessage, "Table", "doesn't exist"))
                            {
                                var message = LocalizationHelper.GetLanguageString("Table does not exist!", "Global", "Global", "msg", "TableNotExist", "Text");

                                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }

                            return true;
                        }
                        else
                        {
                            if (IsLazyTab(SchemaBrowserLazyTab.TableData))
                            {
                                PrepareLoadedTableDataForEditing();
                            }

                            //取得欄位資訊 (包含 Default Value、Constraint 資訊)
                            sql = MySqlTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(schemaNode, schemaName);

                            var dtColumnInfo = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtColumnInfo);

                            if (dtColumnInfo == null || dtColumnInfo.Rows.Count == 0)
                            {
                                c1GridStructure.DataSource = null;
                                c1Grid100RowsTop.DataSource = null;
                                c1GridData.DataSource = null;
                                c1GridColumns.DataSource = null;
                            }
                            else
                            {
                                string GetMySqlConstraintInfo(string columnKey)
                                {
                                    switch ((columnKey ?? string.Empty).Trim().ToUpperInvariant())
                                    {
                                        case "PRI":
                                            {
                                                return "PRIMARY KEY";
                                            }
                                        case "UNI":
                                            {
                                                return "UNIQUE";
                                            }
                                        case "MUL":
                                            {
                                                return "INDEX";
                                            }
                                        default:
                                            {
                                                return string.Empty;
                                            }
                                    }
                                }

                                #region 20240820 取得 MySQL 此 Table 的所有主鍵
                                _dtMySqlPrimaryKeyTable = new DataTable();
                                _dtMySqlPrimaryKeyTable = dtColumnInfo.Clone();

                                var primaryKeyRows = (dtColumnInfo.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                                      .Where
                                                       (
                                                           r => string.Equals(r.Field<string>("ColumnKey"), "PRI", StringComparison.OrdinalIgnoreCase)
                                                       );

                                foreach (var row in primaryKeyRows)
                                {
                                    _dtMySqlPrimaryKeyTable.ImportRow(row);
                                }
                                #endregion

                                foreach (DataRow dr in dtColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                {
                                    var columnName = dr.GetSafeString("ColumnName");

                                    //20260502 透過 columnInfoCollector，取得完整的 DataType
                                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                                    {
                                        var row = _dtStructuredSchemaTable.NewRow();

                                        row["ColumnName"] = columnName;
                                        row["ID"] = dr.GetSafeString("ColumnID");
                                        row["DataType"] = columnInfo.FullDataType;

                                        var columnKey = dr.GetSafeString("ColumnKey");
                                        var constraintInfo = GetMySqlConstraintInfo(columnKey);
                                        var extra = dr.GetSafeString("Extra");

                                        row["ConstraintInfo"] = constraintInfo;
                                        row["Default"] = dr.GetSafeString("DefaultValue"); //20260524 此處不能直接使用 columnInfo.ColumnDefaultValue
                                        row["Nullable"] = dr.GetSafeString("Nullable");
                                        row["AutoInc"] = string.Equals(extra, "AUTO_INCREMENT", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
                                        row["Comments"] = dr.GetSafeString("Comments");

                                        _dtStructuredSchemaTable.Rows.Add(row);
                                    }
                                }

                                c1GridStructure.DataSource = _dtStructuredSchemaTable;

                                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridStructure, GetType().Name);
                                GridHelper.ResizeGridColumnWidth(c1GridStructure, "c1GridStructure");
                                GridHelper.SetGridHeaderLine(c1GridStructure);
                                c1GridStructure.Splits[0].DisplayColumns[0].FetchStyle = true;
                                GridHelper.ResizeGridColumnWidth(c1GridColumns);
                                GridHelper.SetGridHeaderLine(c1GridColumns);
                                GridHelper.ResizeGridColumnWidth(c1GridData);
                                GridHelper.SetGridHeaderLine(c1GridData);
                            }

                            if (IsLazyTab(SchemaBrowserLazyTab.TableData))
                            {
                                SetGridFormat();
                            }
                        }

                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        tabView100RowsTop.Tag = schemaName;
                        tabView100RowsTop.TabVisible = true;

                        if (IsLazyTab(SchemaBrowserLazyTab.SqlPane))
                        {
                            SqlTraceHelper.AppendHeader(sbSql, "---Get View Creation Script");
                            sbSql.Append($"SHOW CREATE VIEW `{schemaNode}`.`{schemaName}`;");
                            sql = sbSql.ToString();

                            var dtViewInfo = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtViewInfo);

                            if (dtViewInfo?.Rows.Count > 0)
                            {
                                script = dtViewInfo.Rows[0].GetSafeString("Create View");
                                script = script.Replace(" DEFINER=", "\r\n    DEFINER=").Replace(" SQL SECURITY ", "\r\nSQL SECURITY ").Replace($" VIEW `{schemaNode}`.`{schemaName}` AS ", $"\r\nVIEW `{schemaNode}`.`{schemaName}`\r\nAS\r\n");
                            }
                        }
                        else if (IsLazyTab(SchemaBrowserLazyTab.ViewData))
                        {
                            sbSql.Clear();
                            SqlTraceHelper.AppendHeader(sbSql, "---Get the First 100 Records (Using Devart ExecutePageReader)");
                            sbSql.Append($"SELECT * FROM `{schemaNode}`.`{schemaName}`;");
                            sql = sbSql.ToString();
                            ExecuteQuery100Rows(sql, 0, 100);
                        }

                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Function Creation Script");

                        sbSql.Append($"SHOW CREATE FUNCTION `{schemaNode}`.`{schemaName}`;");

                        sql = sbSql.ToString();

                        var dtFunctionInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtFunctionInfo);

                        if (dtFunctionInfo?.Rows.Count > 0)
                        {
                            script = dtFunctionInfo.Rows[0].GetSafeString("Create Function");
                        }

                        script = script.Replace("CREATE DEFINER", "CREATE\r\n    DEFINER").Replace(" FUNCTION ", "\r\nFUNCTION ").Replace($"`{schemaName}`(", $"`{schemaName}`\r\n    (").Replace(" RETURNS ", "\r\n    RETURNS ");
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Creation Script");

                        sbSql.Append($"SHOW CREATE TRIGGER `{schemaNode}`.`{schemaName}`;");

                        sql = sbSql.ToString();

                        var dtTriggerInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtTriggerInfo);

                        if (dtTriggerInfo?.Rows.Count > 0)
                        {
                            script = dtTriggerInfo.Rows[0].GetSafeString("SQL Original Statement");
                        }

                        script = script.Replace("CREATE DEFINER", "CREATE\r\n    DEFINER").Replace(" TRIGGER ", "\r\nTRIGGER ").Replace(" BEFORE INSERT ON ", "\r\n    BEFORE INSERT ON ").Replace(" AFTER INSERT ON ", "\r\n    AFTER INSERT ON ").Replace(" BEFORE UPDATE ON ", "\r\n    BEFORE UPDATE ON ").Replace(" AFTER UPDATE ON ", "\r\n    AFTER UPDATE ON ").Replace(" BEFORE DELETE ON ", "\r\n    BEFORE DELETE ON ").Replace(" AFTER DELETE ON ", "\r\n    AFTER DELETE ON ").Replace(" FOR EACH ROW BEGIN", "\r\n    FOR EACH ROW\r\n\r\n  BEGIN").Replace(" FOR EACH ROW SET", "\r\n    FOR EACH ROW\r\nSET");
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Creation Script");

                        sbSql.Append($"SHOW CREATE PROCEDURE `{schemaNode}`.`{schemaName}`;");

                        sql = sbSql.ToString();

                        var dtProcedureInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtProcedureInfo);

                        if (dtProcedureInfo?.Rows.Count > 0)
                        {
                            script = dtProcedureInfo.Rows[0].GetSafeString("Create Procedure");
                        }

                        script = script.Replace("CREATE DEFINER", "CREATE\r\n    DEFINER").Replace(" PROCEDURE ", "\r\nPROCEDURE ").Replace($"`{schemaName}`(", $"`{schemaName}`\r\n    (");
                        break;
                    }
            }

            if (IsLazyTab(SchemaBrowserLazyTab.SqlPane))
            {
                editorSqlPane.ReadOnly = false;
                editorSqlPane.Text = script;
                editorSqlPane.ReadOnly = true;
                editorSqlPane.Focus();
            }

            return false;
        }
    }
}
