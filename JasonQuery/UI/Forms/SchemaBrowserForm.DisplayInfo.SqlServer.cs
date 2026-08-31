using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.LazyLoading;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private bool DisplayInfo_SqlServer(SchemaExplorerSelectionInfo selection)
        {
            var schemaNode = selection.SchemaNode;
            var schemaType = selection.SchemaType;
            var schemaName = selection.SchemaName;
            var schemaDbo = selection.SchemaDbo;
            var objectId = selection.ObjectId;
            var createDate = selection.CreateDate;
            var modifyDate = selection.ModifyDate;

            tabSettings.TabVisible = false;
            tabSqlPreview.TabVisible = false;

            lblTableName01.Text = schemaName;
            lblTableName02.Text = schemaName;
            tabData.Text = $"{tabData.Tag} - {schemaName}";

            var sql = string.Empty;
            var sbSql = new StringBuilder();
            var script = string.Empty;
            var schemaNameWithoutSchemaDbo = schemaName.Replace($"{schemaDbo}.", string.Empty);

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
                            script = DatabaseSqlExecutor.GetCreateScript_SqlServer(schemaType, schemaNode, schemaDbo, schemaNameWithoutSchemaDbo);
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
                            if (errorMessage.StartsWith("Invalid object name:", StringComparison.OrdinalIgnoreCase))
                            {
                                var message = LocalizationHelper.GetLanguageString("Table does not exist!", "Global", "Global", "msg", "TableNotExist", "Text");

                                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            else
                            {
                                MessageBox.Show(errorMessage, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }

                            return true;
                        }
                        else
                        {
                            if (IsLazyTab(SchemaBrowserLazyTab.TableData))
                            {
                                PrepareLoadedTableDataForEditing();
                            }

                            //取得 Constraint 資訊
                            sql = SqlServerTableSqlBuilder.BuildGetTableConstraintsSql(schemaNode, schemaNameWithoutSchemaDbo, schemaDbo);

                            var dtConstraint = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtConstraint);

                            var constraintMap = (dtConstraint?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                .GroupBy
                                 (
                                     row => row.GetSafeString("ColumnName")
                                 )
                                .ToDictionary
                                 (
                                     g => g.Key,
                                     g => (
                                              ConstraintName: g.First()["ConstraintName"]?.ToString(),
                                              ConstraintType: g.First()["ConstraintType"]?.ToString()
                                          )
                                 ) ?? new Dictionary<string, (string ConstraintName, string ConstraintType)>();

                            #region 20240818 取得 SQL Server 此 Table 的所有主鍵
                            _dtSqlServerPrimaryKeyTable = new DataTable();
                            _dtSqlServerPrimaryKeyTable = dtConstraint.Clone();

                            var primaryKeyRows = (dtConstraint.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                                                  .Where
                                                   (
                                                       r => string.Equals(r.Field<string>("ConstraintType"), "PRIMARY KEY", StringComparison.OrdinalIgnoreCase)
                                                   );

                            foreach (var row in primaryKeyRows)
                            {
                                _dtSqlServerPrimaryKeyTable.ImportRow(row);
                            }
                            #endregion

                            //取得所有欄位資訊 (包含 DefaultValue)
                            sql = SqlServerTableColumnInfoSqlBuilder.BuildColumnInfo(schemaNode, schemaDbo, schemaNameWithoutSchemaDbo);

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
                                    var columnName = dr.GetSafeString("Column_Name");

                                    //20260502 透過 columnInfoCollector，取得完整的 DataType
                                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                                    {
                                        var row = _dtStructuredSchemaTable.NewRow();

                                        row["ColumnName"] = columnName;
                                        row["ID"] = dr.GetSafeString("Ordinal_Position");
                                        row["DataType"] = columnInfo.FullDataType;

                                        var constraintInfo = string.Empty;

                                        if (constraintMap.TryGetValue(columnName, out var constraint))
                                        {
                                            var constraintName = constraint.ConstraintName;
                                            var constraintType = constraint.ConstraintType;
                                            var temp2 = string.IsNullOrEmpty(constraintType) ? string.Empty : ", " + constraintType;

                                            constraintInfo = $"{constraintName}{temp2}";
                                        }

                                        row["ConstraintInfo"] = constraintInfo;

                                        //20260510 移除包住整個字串的外層成對小括號
                                        var defaultValue = dr.GetSafeString("DefaultValue"); //20260524 此處不能直接使用 columnInfo.ColumnDefaultValue

                                        defaultValue = TextHelper.RemoveEnclosingParentheses(defaultValue, trimOuterWhiteSpace: true);

                                        row["Default"] = defaultValue;
                                        row["Nullable"] = dr.GetSafeString("Is_Nullable");
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

                                if (IsLazyTab(SchemaBrowserLazyTab.TableData))
                                {
                                    SetGridFormat();
                                }
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
                            script = DatabaseSqlExecutor.GetCreateScript_SqlServer(schemaType, schemaNode, schemaDbo, schemaName);
                        }
                        else if (IsLazyTab(SchemaBrowserLazyTab.ViewData))
                        {
                            sbSql.Clear();
                            SqlTraceHelper.AppendHeader(sbSql, "---Get the First 100 Records (Using Devart ExecutePageReader)");
                            sbSql.Append($"SELECT * FROM {schemaNameWithoutSchemaDbo}");
                            sql = sbSql.ToString();
                            ExecuteQuery100Rows(sql, 0, 100);
                        }

                        break;
                    }
                case SchemaObjectNames.Functions:
                case SchemaObjectNames.Triggers:
                case SchemaObjectNames.Procedures:
                case SchemaObjectNames.Indexes:
                    {
                        script = DatabaseSqlExecutor.GetCreateScript_SqlServer(schemaType, schemaNode, schemaDbo, schemaName, objectId);
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
