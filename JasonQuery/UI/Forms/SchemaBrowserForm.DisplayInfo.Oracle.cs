using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.Dictionaries;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table;
using JasonQuery.Core.Localization;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private bool DisplayInfo_Oracle(SchemaExplorerSelectionInfo selection)
        {
            var schemaType = selection.SchemaType;
            var schemaName = selection.SchemaName;
            var packageSpecBody = selection.PackageSpecBody;

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
            var scriptHeader = string.Empty;

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
                            if (errorMessage.StartsWith("ORA-00942", StringComparison.Ordinal))
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
                            sql = OracleTableSqlBuilder.BuildGetTableConstraintsSql(DatabaseSqlExecutor.DbUserUppercase, schemaName);

                            var dtConstraint = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtConstraint);

                            var constraintMap = (dtConstraint?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
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
                                     g => $"{g.First()["ConstraintType"]}, {g.First()["ConstraintName"]}"
                                 );

                            //取得欄位資訊 (包含 Default Value)
                            sql = OracleTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(DatabaseSqlExecutor.DbUserUppercase, schemaName);

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

                            sbSql.Clear();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get Table Creation Information");

                            sbSql.AppendLine("SELECT Object_Name AS Function_Name, Status, Created");
                            sbSql.AppendLine("  FROM All_Objects");
                            sbSql.AppendLine(" WHERE Object_Type = 'TABLE'");
                            sbSql.AppendLine($"   AND UPPER(Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                            sbSql.AppendLine($"   AND Object_Name = '{schemaName}'");

                            sql = sbSql.ToString();

                            var dtTableCreationInfo = new DataTable();

                            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtTableCreationInfo);

                            if (dtTableCreationInfo?.Rows.Count > 0)
                            {
                                var createDate = dtTableCreationInfo.Rows[0].GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                                scriptHeader = $"--Table: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {dtTableCreationInfo.Rows[0]["Status"]}\r\n--Created: {createDate}\r\n\r\n";
                            }
                        }

                        break;
                    }
                case SchemaObjectNames.Functions:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Function Creation Information");

                        sbSql.AppendLine("SELECT Object_Name AS Function_Name, Status, Created");
                        sbSql.AppendLine("  FROM All_Objects");
                        sbSql.AppendLine(" WHERE Object_Type = 'FUNCTION'");
                        sbSql.AppendLine($"   AND UPPER(Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.Append($"   AND Object_Name = '{schemaName}'");

                        sql = sbSql.ToString();

                        var dtFunctionInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtFunctionInfo);

                        if (dtFunctionInfo?.Rows.Count > 0)
                        {
                            var createDate = dtFunctionInfo.Rows[0].GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                            scriptHeader = $"--Function: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {dtFunctionInfo.Rows[0]["Status"]}\r\n--Created: {createDate}\r\n\r\n";
                        }

                        break;
                    }
                case SchemaObjectNames.Triggers:
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Trigger Creation Information");

                        sbSql.AppendLine("SELECT Object_Name AS Trigger_Name, Status, Created");
                        sbSql.AppendLine("  FROM All_Objects");
                        sbSql.AppendLine(" WHERE Object_Type = 'TRIGGER'");
                        sbSql.AppendLine($"   AND UPPER(Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.Append($"   AND Object_Name = '{schemaName}'");

                        sql = sbSql.ToString();

                        var dtTriggerInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtTriggerInfo);

                        if (dtTriggerInfo?.Rows.Count > 0)
                        {
                            var createDate = dtTriggerInfo.Rows[0].GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                            scriptHeader = $"--Trigger: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {dtTriggerInfo.Rows[0]["Status"]}\r\n--Created: {createDate}\r\n\r\n";
                        }

                        break;
                    }
                case SchemaObjectNames.Views:
                    {
                        tabView100RowsTop.Tag = schemaName;
                        tabView100RowsTop.TabVisible = true;

                        SqlTraceHelper.AppendHeader(sbSql, "---Get View Creation Information");

                        sbSql.AppendLine("SELECT os.Object_Name AS View_Name, vs.Text_Length, os.Status, os.Created");
                        sbSql.AppendLine("  FROM All_Objects os, All_Views vs");
                        sbSql.AppendLine(" WHERE os.Object_Type = 'VIEW'");
                        sbSql.AppendLine("   AND os.Object_Name = vs.View_Name");
                        sbSql.AppendLine($"   AND UPPER(os.Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.AppendLine($"   AND UPPER(vs.Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.Append($"   AND os.Object_Name = '{schemaName}'");

                        sql = sbSql.ToString();

                        var dtViewInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtViewInfo);

                        if (dtViewInfo == null || dtViewInfo.Rows.Count == 0)
                        {
                            c1GridStructure.DataSource = null;
                            c1Grid100RowsTop.DataSource = null;
                            c1GridData.DataSource = null;
                            c1GridColumns.DataSource = null;
                        }
                        else
                        {
                            var createDate = dtViewInfo.Rows[0].GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                            scriptHeader = $"--View: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {dtViewInfo.Rows[0]["Status"]}\r\n--Created: {createDate}\r\n\r\n";

                            sbSql.Clear();

                            SqlTraceHelper.AppendHeader(sbSql, "---Get the First 100 Records (Using Devart ExecutePageReader)");

                            //取得指定 View 的前 100 筆資料
                            sbSql.Append($"SELECT * FROM {schemaName}");

                            sql = sbSql.ToString();
                            ExecuteQuery100Rows(sql, 0, 100);
                        }

                        break;
                    }
                case SchemaObjectNames.Procedures: //20241221
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Procedure Creation Information");

                        sbSql.AppendLine("SELECT Object_Name AS Procedure_Name, Object_Type, Status, Created");
                        sbSql.AppendLine("  FROM All_Objects");
                        sbSql.AppendLine(" WHERE Object_Type = 'PROCEDURE'");
                        sbSql.AppendLine($"   AND UPPER(Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.Append($"   AND Object_Name = '{schemaName}'");

                        sql = sbSql.ToString();

                        var dtProcedureInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtProcedureInfo);

                        if (dtProcedureInfo?.Rows.Count > 0)
                        {
                            var createDate = dtProcedureInfo.Rows[0].GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                            scriptHeader = $"--Procedure: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {dtProcedureInfo.Rows[0]["Status"]}\r\n--Created: {createDate}\r\n\r\n";
                        }

                        break;
                    }
                case SchemaObjectNames.Packages: //20241221
                    {
                        SqlTraceHelper.AppendHeader(sbSql, "---Get Package Creation Information");

                        sbSql.AppendLine("SELECT Object_Name AS Package_Name, Object_Type, Status, Created");
                        sbSql.AppendLine("  FROM All_Objects");
                        sbSql.AppendLine($" WHERE Object_Type = 'PACKAGE{(packageSpecBody == "Body" ? " BODY" : string.Empty)}'");
                        sbSql.AppendLine($"   AND UPPER(Owner) = '{DatabaseSqlExecutor.DbUserUppercase}'");
                        sbSql.Append($"   AND Object_Name = '{schemaName}'");

                        sql = sbSql.ToString();

                        var dtPackageInfo = new DataTable();

                        DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtPackageInfo);

                        if (dtPackageInfo?.Rows.Count > 0)
                        {
                            var dr = dtPackageInfo.Rows[0];
                            var status = dr.GetSafeString("Status");
                            var package = packageSpecBody == "Body" ? " Body" : string.Empty;
                            var createDate = dr.GetSafeDateTimeText("Created", $"{MyLibrary.DateFormat} HH:mm:ss");

                            scriptHeader = $"--Package{package}: \"{DatabaseSqlExecutor.DbUserUppercase}\".\"{schemaName}\"\r\n--Status: {status}\r\n--Created: {createDate}\r\n\r\n";
                        }

                        break;
                    }
            }

            var type = schemaType.Substring(0, schemaType.Length - 1).ToUpper(); //去除末碼 s

            sbSql.Clear();

            SqlTraceHelper.AppendHeader(sbSql, "---Get Object Creation Script");

            //以下 SQL 可以取得指定 Function/Table/Trigger/View 的 Create Script
            sbSql.AppendLine($"SELECT dbms_metadata.GET_DDL('{type}', '{schemaName}', '{DatabaseSqlExecutor.DbUserUppercase}') AS ScriptText");
            sbSql.Append("  FROM DUAL");

            sql = sbSql.ToString();

            var dtScriptInfo = new DataTable();

            DatabaseSqlExecutor.ExecuteQueryToDataTable(sql, ref dtScriptInfo);

            if (dtScriptInfo?.Rows.Count > 0)
            {
                var script = Regex.Replace(dtScriptInfo.Rows[0]["ScriptText"].ToString(), @"(?<!\r)\n", "\r\n").Trim();

                //20240525 優化 Create Table Script
                if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
                {
                    script = DatabaseSqlExecutor.OracleCreateTableScriptBeautifier(script, schemaName, schemaType);
                }
                else if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
                {
                    script += script.EndsWith(";", StringComparison.Ordinal) ? string.Empty : ";";

                    var sb = new StringBuilder();
                    //var dtComment = DatabaseSqlExecutor.GetColumnComment(schemaName);
                    DataTable dtComment = null;

                    foreach (DataRow dr in dtComment?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var columnName = dr.GetSafeString("ColumnName");
                        var comment = dr.GetSafeString("Comments");

                        if (!string.IsNullOrEmpty(comment))
                        {
                            comment = comment.Replace("'", "''");
                            sb.Append($"\r\nCOMMENT ON COLUMN {DatabaseSqlExecutor.DbUserUppercase}.{schemaName}.{columnName} IS '{comment}';");
                        }
                    }

                    var value = sb.Length == 0 ? string.Empty : $"\r\n{sb}";

                    script += value;
                }
                else if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Packages))
                {
                    var keyword = "\r\nCREATE OR REPLACE";
                    var spec = string.Empty;
                    var body = string.Empty;
                    var parts = script.Split(new[] { keyword }, StringSplitOptions.None);

                    if (parts.Length == 1)
                    {
                        //找不到關鍵字 (可能是版本或是大小寫差異)，改用 SQL 去撈取 DDL 內容
                    }
                    else
                    {
                        spec = parts[0];
                        body = $"CREATE OR REPLACE{parts[1]}";

                        if (packageSpecBody == "Spec")
                        {
                            if (spec.StartsWith("\r\n", StringComparison.Ordinal))
                            {
                                spec = spec.Substring(3);
                            }

                            script = spec.TrimStart();
                        }
                        else
                        {
                            if (body.StartsWith("\r\n", StringComparison.Ordinal))
                            {
                                body = spec.Substring(3);
                            }

                            script = body.TrimStart();
                        }
                    }
                }
                else
                {
                    if (script.StartsWith("\r\n", StringComparison.Ordinal))
                    {
                        script = script.Substring(3).Trim();
                    }
                }

                editorSqlPane.ReadOnly = false;
                editorSqlPane.Text = $"{scriptHeader}{script}";
                editorSqlPane.ReadOnly = true;
                editorSqlPane.Focus();
            }

            return false;
        }
    }
}