using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
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
        private bool _oracleSqlPreviewValidationFailed;

        private string GenerateSqlPreview_Oracle()
        {
            _oracleSqlPreviewValidationFailed = false;
            Cursor = Cursors.WaitCursor;

            var finalInsertSql = string.Empty;
            var finalDeleteSql = string.Empty;
            var finalUpdateSql = string.Empty;
            var isUpperCase = MyLibrary.SqlFormatterConvertCaseForKeywordsCase == 1;
            var tableName = DmlPreviewSqlBuilder.BuildTableName(DataSourceType.Oracle, string.Empty, _selectedTableName);

            var dt = c1GridData.GetDataTableSourceOrNull();

            if (dt == null || dt.Rows.Count == 0)
            {
                editorSqlPreview.ReadOnly = false;
                editorSqlPreview.Text = string.Empty;
                editorSqlPreview.ReadOnly = true;

                Cursor = Cursors.Default;
                return editorSqlPreview.Text;
            }

            var defaultValueMap = BuildOracleDefaultValueMap();

            string GetDefaultValue(string columnName)
            {
                return defaultValueMap.TryGetValue(columnName, out var defaultValue) ? defaultValue : string.Empty;
            }

            bool IsNewOrCloneOperation(string operationMode)
            {
                return _operationModeNewClone.Contains(operationMode);
            }

            bool HasAnyInsertValue(DataRow dataRow)
            {
                var columnCount = dt.Columns.Count - 1;

                for (var i = 1; i < columnCount; i++)
                {
                    var value = dataRow[i]?.ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }

                return false;
            }

            IReadOnlyList<DmlPreviewPrimaryKeyColumn> GetOraclePrimaryKeyColumns()
            {
                if (_columnInfoCollector == null)
                {
                    return Array.Empty<DmlPreviewPrimaryKeyColumn>();
                }

                return _columnInfoCollector.GetAll()
                                           .Where(columnInfo => columnInfo != null && columnInfo.IsPrimaryKey)
                                           .Select
                                            (
                                               columnInfo => new DmlPreviewPrimaryKeyColumn
                                                             {
                                                                 ColumnName = columnInfo.ColumnName,
                                                                 OrdinalPosition = GetColumnOrdinalForSqlPreview(columnInfo.ColumnName),
                                                                 ColumnInfo = columnInfo
                                                             }
                                            )
                                           .ToList();
            }

            int GetColumnOrdinalForSqlPreview(string columnName)
            {
                if (_dtStructuredSchemaTable == null || _dtStructuredSchemaTable.Rows.Count == 0)
                {
                    return int.MaxValue;
                }

                if (!_dtStructuredSchemaTable.Columns.Contains("ColumnName") || !_dtStructuredSchemaTable.Columns.Contains("ID"))
                {
                    return int.MaxValue;
                }

                var row = _dtStructuredSchemaTable.AsEnumerable()
                                                  .FirstOrDefault
                                                   (
                                                       item => string.Equals(item.GetSafeString("ColumnName"), columnName, StringComparison.Ordinal)
                                                   );

                if (row == null)
                {
                    return int.MaxValue;
                }

                var idText = row.GetSafeString("ID");

                return int.TryParse(idText, out var id) ? id : int.MaxValue;
            }

            string BuildOracleWhereCondition(DataRow currentRow)
            {
                var primaryKeyColumns = GetOraclePrimaryKeyColumns();

                var result = DmlPreviewWhereConditionBuilder.Build
                             (
                                 new DmlPreviewWhereConditionRequest
                                 {
                                     DataSourceType = DataSourceType.Oracle,
                                     CurrentRow = currentRow,
                                     OriginalTable = _dtOriginalTableData,
                                     RowIdentityColumnName = MyGlobal.Row_Id_PK_JQ,
                                     HasDeclaredPrimaryKey = primaryKeyColumns.Count > 0,
                                     PrimaryKeyColumns = primaryKeyColumns,
                                     UseUpperCaseKeywords = isUpperCase,
                                     NullValueIndicators = GetOracleActiveNullValueIndicators()
                                 }
                             );

                return result.Success ? result.Condition : string.Empty;
            }

            for (var row = 0; row < dt.Rows.Count; row++) //處理 Delete, Insert
            {
                var dataRow = dt.Rows[row];
                var operationMode = dataRow.GetSafeString(_identifyColumnName);

                if (string.IsNullOrEmpty(operationMode))
                {
                    continue;
                }

                if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    var whereConditionString = BuildOracleWhereCondition(dataRow);

                    if (string.IsNullOrWhiteSpace(whereConditionString))
                    {
                        continue;
                    }

                    var deleteSql = DmlPreviewSqlBuilder.BuildDelete(tableName, whereConditionString, isUpperCase);

                    if (string.IsNullOrEmpty(deleteSql))
                    {
                        continue;
                    }

                    finalDeleteSql += $"{deleteSql}\r\n\r\n";
                    continue;
                }

                if (!IsNewOrCloneOperation(operationMode))
                {
                    continue;
                }

                if (!HasAnyInsertValue(dataRow))
                {
                    continue; //整列都是空的，忽略不處理！
                }

                var insertFields = new StringBuilder();
                var insertValues = new StringBuilder();
                var gridRow = c1GridData.Splits[0].Rows[row];

                foreach (C1DataColumn column in c1GridData.Columns)
                {
                    var columnName = column.DataField;

                    if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase) || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        continue;
                    }

                    if (!IsTableDataColumnEditable(columnInfo))
                    {
                        continue; //如果欄位不支援更新，就略過不處理
                    }

                    var defaultValue = GetDefaultValue(columnName);
                    var rawCellValue = dataRow.Table.Columns.Contains(columnName) ? dataRow[columnName] : null;
                    var cellValue = column.CellText(gridRow.DataRowIndex);
                    var sqlValue = ConvertOracleCellValueToSqlLiteral(rawCellValue, cellValue, columnInfo, defaultValue, isUpperCase);

                    if (!TryValidateOracleRequiredValue(rawCellValue, cellValue, sqlValue, columnInfo, out var validationMessage))
                    {
                        AbortOracleSqlPreviewForInvalidRequiredValue(row, GetOracleGridColumnIndex(columnName), validationMessage);
                        return string.Empty;
                    }

                    insertFields.Append($"{columnName}, ");
                    insertValues.Append($"{sqlValue}, ");
                }

                var fields = insertFields.ToString().TrimEnd(',', ' ');
                var values = insertValues.ToString().TrimEnd(',', ' ');

                if (string.IsNullOrWhiteSpace(fields))
                {
                    continue;
                }

                var insertSql = DmlPreviewSqlBuilder.BuildInsert(tableName, fields, values, isUpperCase);

                if (string.IsNullOrEmpty(insertSql))
                {
                    continue;
                }

                finalInsertSql += $"{insertSql}\r\n\r\n";
            }

            if (_modifiedCells.Count > 0) //處理 Update Cells
            {
                var modifiedRowGroups = GetModifiedCellDisplayRowGroups();

                foreach (var rowGroup in modifiedRowGroups)
                {
                    var rowIndex = rowGroup.Key;
                    var dataRow = GetDataRowFromGridDisplayRow(c1GridData, rowIndex);

                    if (!IsUsableTableDataRow(dataRow))
                    {
                        continue;
                    }

                    var operationMode = dataRow.GetSafeString(_identifyColumnName);

                    if (!string.IsNullOrEmpty(operationMode))
                    {
                        continue; //新增、複製、刪除列已由 Insert/Delete 區塊處理
                    }

                    var updateColumnNameAndValue = new StringBuilder();

                    foreach (var cell in rowGroup)
                    {
                        var columnIndex = cell.X;

                        if (columnIndex < 0 || columnIndex >= c1GridData.Columns.Count)
                        {
                            continue;
                        }

                        var columnName = c1GridData.Columns[columnIndex].DataField;

                        if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase) || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
                        {
                            continue;
                        }

                        if (!IsTableDataColumnEditable(columnInfo))
                        {
                            continue;
                        }

                        var defaultValue = GetDefaultValue(columnName);
                        var rawCellValue = dataRow.Table.Columns.Contains(columnName) ? dataRow[columnName] : dataRow[columnIndex];
                        var cellValue = rawCellValue == null || rawCellValue == DBNull.Value ? string.Empty : rawCellValue.ToString();
                        var sqlValue = ConvertOracleCellValueToSqlLiteral(rawCellValue, cellValue, columnInfo, defaultValue, isUpperCase);

                        if (!TryValidateOracleRequiredValue(rawCellValue, cellValue, sqlValue, columnInfo, out var validationMessage))
                        {
                            AbortOracleSqlPreviewForInvalidRequiredValue(rowIndex, columnIndex, validationMessage);
                            return string.Empty;
                        }

                        updateColumnNameAndValue.AppendLine($"{new string(' ', 7)}{columnName} = {sqlValue},");
                    }

                    if (updateColumnNameAndValue.Length == 0)
                    {
                        continue;
                    }

                    var setClause = updateColumnNameAndValue.ToString().TrimEnd('\r', '\n').TrimEnd(',');
                    var whereConditionString = BuildOracleWhereCondition(dataRow);

                    if (string.IsNullOrWhiteSpace(whereConditionString))
                    {
                        continue;
                    }

                    var updateSql = DmlPreviewSqlBuilder.BuildUpdate(tableName, setClause.Trim(), whereConditionString, isUpperCase);

                    if (string.IsNullOrEmpty(updateSql))
                    {
                        continue;
                    }

                    finalUpdateSql += $"{updateSql}\r\n\r\n";
                }
            }

            editorSqlPreview.ReadOnly = false;
            editorSqlPreview.Text = $"{finalDeleteSql}{finalInsertSql}{finalUpdateSql}".TrimEnd('\r', '\n');
            editorSqlPreview.ReadOnly = true;

            Cursor = Cursors.Default;
            return editorSqlPreview.Text;
        }

        private Dictionary<string, string> BuildOracleDefaultValueMap()
        {
            if (_dtStructuredSchemaTable == null || _dtStructuredSchemaTable.Rows.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            if (!_dtStructuredSchemaTable.Columns.Contains("ColumnName") || !_dtStructuredSchemaTable.Columns.Contains("Default"))
            {
                return new Dictionary<string, string>();
            }

            return _dtStructuredSchemaTable.AsEnumerable()
                                           .Where
                                            (
                                                row => !string.IsNullOrWhiteSpace(row.GetSafeString("ColumnName"))
                                            )
                                           .GroupBy
                                            (
                                                row => row.GetSafeString("ColumnName").Trim()
                                            )
                                           .ToDictionary
                                            (
                                                group => group.Key,
                                                group => group.First().GetSafeString("Default"),
                                                StringComparer.Ordinal
                                            );
        }

        private string GetOracleDefaultValue(string columnName)
        {
            var defaultValueMap = BuildOracleDefaultValueMap();

            return defaultValueMap.TryGetValue(columnName, out var defaultValue) ? defaultValue : string.Empty;
        }

        private string ConvertOracleCellValueToSqlLiteral(object rawCellValue, string cellValue, ColumnInfo columnInfo,
                                                          string defaultValue, bool isUpperCase, bool allowDefaultKeyword = true)
        {
            var sqlLiteral = DmlPreviewSqlLiteralFormatter.Format
                             (
                                 new DmlPreviewLiteralFormatRequest
                                 {
                                     DataSourceType = DataSourceType.Oracle,
                                     CellValue = cellValue,
                                     ColumnInfo = columnInfo,
                                     DefaultValue = defaultValue,
                                     AllowDefaultKeyword = allowDefaultKeyword,
                                     UseUpperCaseKeywords = isUpperCase,
                                     NullValueIndicators = GetOracleActiveNullValueIndicators()
                                 }
                             );

            //Oracle 會將零長度字串視為 NULL，只有 Formatter 原本確實產生空字串 literal 時才改寫，避免影響既有 DEFAULT 規則。
            if (columnInfo != null && columnInfo.CategoryDataTypeKind == CategoryDataTypeKind.String
                && IsOracleZeroLengthString(rawCellValue, cellValue) && string.Equals(sqlLiteral, "''", StringComparison.Ordinal))
            {
                return isUpperCase ? "NULL" : "null";
            }

            return sqlLiteral;
        }

        private bool TryValidateOracleRequiredCellAfterUpdate(int rowIndex, int columnIndex)
        {
            if (_currentSourceType != DataSourceType.Oracle)
            {
                return true;
            }

            if (rowIndex < 0 || columnIndex < 0 || columnIndex >= c1GridData.Columns.Count)
            {
                return true;
            }

            var dataRow = GetDataRowFromGridDisplayRow(c1GridData, rowIndex);

            if (!IsUsableTableDataRow(dataRow))
            {
                return true;
            }

            var operationMode = dataRow.GetSafeString(_identifyColumnName);

            if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var column = c1GridData.Columns[columnIndex];
            var columnName = column.DataField;

            if (string.IsNullOrEmpty(columnName)
                || string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase)
                || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (_columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return true;
            }

            if (!IsTableDataColumnEditable(columnInfo))
            {
                return true;
            }

            //AfterColUpdate、自訂 Editor Leave 已將值寫回 DataRow，此處直接讀取實際資料值，才能同時識別 DBNull、零長度字串及手動輸入目前設定的 Null Indicator
            var rawCellValue = dataRow.Table.Columns.Contains(columnName) ? dataRow[columnName] : null;
            var cellValue = rawCellValue == null || rawCellValue == DBNull.Value ? string.Empty : Convert.ToString(rawCellValue) ?? string.Empty;
            var defaultValue = GetOracleDefaultValue(columnName);
            var isUpperCase = MyLibrary.SqlFormatterConvertCaseForKeywordsCase == 1;
            var sqlLiteral = ConvertOracleCellValueToSqlLiteral(rawCellValue, cellValue, columnInfo, defaultValue, isUpperCase);

            if (TryValidateOracleRequiredValue(rawCellValue, cellValue, sqlLiteral, columnInfo, out var messageFull))
            {
                return true;
            }

            Cursor = Cursors.Default;
            MessageBox.Show(messageFull, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            FocusOracleInvalidCell(rowIndex, columnIndex);

            return false;
        }

        private bool TryValidateOracleRequiredValue(object rawCellValue, string cellValue, string sqlLiteral,
                                                    ColumnInfo columnInfo, out string messageFull)
        {
            messageFull = string.Empty;

            if (columnInfo == null || (!columnInfo.IsPrimaryKey && columnInfo.IsNullable))
            {
                return true;
            }

            //DEFAULT 代表資料庫將套用預設值，不是本次要寫入 NULL。
            if (string.Equals(sqlLiteral, "DEFAULT", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var isRawNull = rawCellValue == null || rawCellValue == DBNull.Value;
            var isZeroLengthString = IsOracleZeroLengthString(rawCellValue, cellValue);
            var isNullIndicator = IsOracleNullIndicator(cellValue);
            var isNullLiteral = string.Equals(sqlLiteral, "NULL", StringComparison.OrdinalIgnoreCase);

            if (!isRawNull && !isZeroLengthString && !isNullIndicator && !isNullLiteral)
            {
                return true;
            }

            BuildOracleRequiredValueValidationMessage(columnInfo, out messageFull);

            return false;
        }

        private static bool IsOracleZeroLengthString(object rawCellValue, string cellValue)
        {
            if (rawCellValue is string rawText)
            {
                return rawText.Length == 0;
            }

            return rawCellValue != DBNull.Value && cellValue != null && cellValue.Length == 0;
        }

        private static string[] GetOracleActiveNullValueIndicators()
        {
            var nullIndicator = MyLibrary.GridNullShowAs;

            //只將目前 Grid 實際採用的 NULL 顯示文字視為 NULL，例如目前設定為 <NULL> 時，使用者輸入 {NULL} 應保留為一般字串。
            if (string.IsNullOrEmpty(nullIndicator) || string.Equals(nullIndicator, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<string>();
            }

            return new[] { nullIndicator };
        }

        private static bool IsOracleNullIndicator(string cellValue)
        {
            var nullIndicators = GetOracleActiveNullValueIndicators();

            return nullIndicators.Length == 1 && string.Equals(cellValue, nullIndicators[0], StringComparison.OrdinalIgnoreCase);
        }

        private void BuildOracleRequiredValueValidationMessage(ColumnInfo columnInfo, out string messageFull)
        {
            var columnName = columnInfo.ColumnName ?? string.Empty;

            if (columnInfo.IsPrimaryKey)
            {
                messageFull = GetOracleValidationLocalizedText
                              (
                                  "InvalidPrimaryKeyValue",
                                  "Invalid Primary Key Value\r\n\r\nColumn \"{0}\" is a primary key and cannot be empty.\r\nOracle treats an empty string as NULL, and a primary key column cannot contain NULL.\r\nEnter a valid value or cancel the current edit.\r\n\r\nThe change has not been executed."
                              );

                messageFull = string.Format(messageFull, columnName);
                return;
            }

            var notNullMessageFull = GetOracleValidationLocalizedText
                                     (
                                         "InvalidNotNullValue",
                                         "Invalid Column Value\r\n\r\nColumn \"{0}\" does not allow NULL.\r\n\r\nOracle treats an empty string as NULL, and this column has a NOT NULL constraint.\r\nEnter a valid value or cancel the current edit.\r\n\r\nThe change has not been executed."
                                     );

            messageFull = string.Format(notNullMessageFull, columnName);
        }

        private string GetOracleValidationLocalizedText(string resourceId, string englishText)
        {
            var localizedText = LocalizationHelper.GetLanguageString(englishText, "form", GetType().Name, "msg", resourceId, "Text");

            return localizedText;
        }

        private void AbortOracleSqlPreviewForInvalidRequiredValue(int rowIndex, int columnIndex, string messageFull)
        {
            _oracleSqlPreviewValidationFailed = true;

            editorSqlPreview.ReadOnly = false;
            editorSqlPreview.Text = string.Empty;
            editorSqlPreview.ReadOnly = true;

            Cursor = Cursors.Default;
            MessageBox.Show(messageFull, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            FocusOracleInvalidCell(rowIndex, columnIndex);
        }

        private int GetOracleGridColumnIndex(string columnName)
        {
            for (var columnIndex = 0; columnIndex < c1GridData.Columns.Count; columnIndex++)
            {
                if (string.Equals(c1GridData.Columns[columnIndex].DataField, columnName, StringComparison.Ordinal))
                {
                    return columnIndex;
                }
            }

            return -1;
        }

        private void FocusOracleInvalidCell(int rowIndex, int columnIndex)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke
            (
                new Action
                (
                    () =>
                    {
                        if (IsDisposed || c1GridData == null)
                        {
                            return;
                        }

                        tabSchemaBrowser.SelectedTab = tabData;

                        if (rowIndex >= 0 && rowIndex < c1GridData.Splits[0].Rows.Count)
                        {
                            c1GridData.Row = rowIndex;
                        }

                        if (columnIndex >= 0 && columnIndex < c1GridData.Columns.Count)
                        {
                            c1GridData.Col = columnIndex;
                        }

                        c1GridData.Select();
                        c1GridData.Focus();
                    }
                )
            );
        }
    }
}