using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Dictionaries;
using JasonQuery.Core.Database.CreateScript.PostgreSql;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private bool DisplayInfo_PostgreSql(SchemaExplorerSelectionInfo selection)
        {
            var schemaNode = selection.SchemaNode;
            var schemaType = selection.SchemaType;
            var schemaName = selection.SchemaName;

            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;

            editorSqlPane.ReadOnly = false;
            editorSqlPane.Text = string.Empty;
            editorSqlPane.ReadOnly = true;

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

                        CreateTableSchemaTable();

                        //查詢前 500筆資料
                        sql = BuildTableDataPreviewSql(selection, TextHelper.GetSafeString(btnFilterData.Tag));

                        var errorMessage = ExecuteQuery100Rows(sql, 0, 500, false, true);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            if (errorMessage.StartsWith("42P01:", StringComparison.Ordinal))
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
                            tsData.Enabled = true;

                            #region 20240602 for 編輯 Table 資料
                            CheckButtonsStatus(); //檢查按鈕狀態
                            c1SuperTooltip1.Hide(); //隱藏 Tips 提示
                            cboFind.Enabled = c1GridData.HasDataTableRows(); //20240608 依查詢筆數，決定 cboFind.Enabled

                            var dtColumns = new DataTable(); //20240518 整理此 Table 的所有欄位 for c1GridColumns

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
                            #endregion

                            //取得 Constraint 資訊
                            sql = PostgreSqlTableSqlBuilder.BuildGetTableConstraintsSql(schemaNode, schemaName);

                            var dtConstraint = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtConstraint);

                            var constraintMap = (dtConstraint.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                                .GroupBy
                                                 (
                                                     row =>
                                                     (
                                                         TableName: row.Field<string>("TableName"),
                                                         ColumnName: row.Field<string>("ColumnName")
                                                     )
                                                 )
                                                .ToDictionary
                                                 (
                                                     g => g.Key,
                                                     g => $"{g.First()["ConstraintInfo"]}"
                                                 );

                            //取得欄位資訊 (包含 Default Value)
                            sql = PostgreSqlTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(schemaNode, schemaName);

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
                                foreach (DataRow dr in dtColumnInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                {
                                    var tableName = dr.GetSafeString("TableName");
                                    var columnName = dr.GetSafeString("ColumnName");

                                    //20260502 透過 columnInfoCollector，取得完整的 DataType
                                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                                    {
                                        var row = _dtStructuredSchemaTable.NewRow();

                                        row["ColumnName"] = columnName;
                                        row["ID"] = dr.GetSafeString("ColumnID");
                                        row["DataType"] = columnInfo.FullDataType;
                                        row["ConstraintInfo"] = constraintMap.GetValueOrDefault((tableName, columnName), string.Empty);
                                        row["Default"] = dr.GetSafeString("DefaultValue"); //20260524 此處不能直接使用 columnInfo.ColumnDefaultValue
                                        row["Nullable"] = dr.GetSafeString("Nullable");
                                        row["Comments"] = dr.GetSafeString("Comments");

                                        _dtStructuredSchemaTable.Rows.Add(row);
                                    }
                                }

                                c1GridStructure.DataSource = _dtStructuredSchemaTable;

                                GridHelper.ResizeGridColumnWidth(c1GridStructure, "c1GridStructure");
                                GridHelper.SetGridHeaderLine(c1GridStructure);
                                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridStructure, GetType().Name);
                                c1GridStructure.Splits[0].DisplayColumns[0].FetchStyle = true;
                                GridHelper.ResizeGridColumnWidth(c1GridColumns);
                                GridHelper.SetGridHeaderLine(c1GridColumns);
                                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1GridColumns, GetType().Name, true, "gridheader");
                                GridHelper.ResizeGridColumnWidth(c1GridData);
                                GridHelper.SetGridHeaderLine(c1GridData);
                            }

                            SetGridFormat();

                            //取得指定的 Table Creation Script
                            script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, schemaType, schemaName);
                            script = PostgreSqlCreateTableScriptBeautifier.Beautify(script);
                        }

                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        tabView100RowsTop.Tag = schemaName;
                        tabView100RowsTop.TabVisible = true;

                        script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, SchemaObjectNames.Views, schemaName);
                        sbSql.Clear();

                        SqlTraceHelper.AppendHeader(sbSql, "---Get the First 100 Records (Using Devart ExecutePageReader)");

                        //取得指定 View 的前 100 筆資料
                        sbSql.Append($"SELECT * FROM {schemaName}");

                        sql = sbSql.ToString();
                        ExecuteQuery100Rows(sql, 0, 100);
                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, SchemaObjectNames.Functions, schemaName);
                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, SchemaObjectNames.Triggers, schemaName);
                        break;
                    }
                case SchemaObjectNames.Indexes:
                    {
                        script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, SchemaObjectNames.Indexes, schemaName);
                        break;
                    }
                case SchemaObjectNames.Procedures:
                    {
                        script = DatabaseSqlExecutor.GetCreateScript_PostgreSql(schemaNode, SchemaObjectNames.Procedures, schemaName);
                        break;
                    }
                case SchemaObjectNames.Packages:
                    {
                        script = string.Empty; //20260510 目前已知，PostgreSQL 沒有 Package
                        break;
                    }
            }

            editorSqlPane.ReadOnly = false;
            editorSqlPane.Text = script;
            editorSqlPane.ReadOnly = true;
            editorSqlPane.Focus();

            return false;
        }
    }
}