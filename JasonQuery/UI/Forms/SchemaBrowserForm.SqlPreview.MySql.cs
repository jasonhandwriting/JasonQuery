using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using JasonQuery.Core.Data.DataRows;
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
        private string GenerateSqlPreview_MySql()
        {
            Cursor = Cursors.WaitCursor;

            var finalInsertSql = string.Empty;
            var finalDeleteSql = string.Empty;
            var finalUpdateSql = string.Empty;
            var isUpperCase = MyLibrary.SqlFormatterConvertCaseForKeywordsCase == 1;
            var tableName = BuildMySqlTableName();
            var dt = c1GridData.GetDataTableSourceOrNull();

            if (dt == null || dt.Rows.Count == 0)
            {
                editorSqlPreview.ReadOnly = false;
                editorSqlPreview.Text = string.Empty;
                editorSqlPreview.ReadOnly = true;

                Cursor = Cursors.Default;
                return editorSqlPreview.Text;
            }

            var defaultValueMap = BuildDefaultValueMap();
            var autoIncrementColumns = BuildAutoIncrementColumnSet();

            Dictionary<string, string> BuildDefaultValueMap()
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

            HashSet<string> BuildAutoIncrementColumnSet()
            {
                if (_dtStructuredSchemaTable == null || _dtStructuredSchemaTable.Rows.Count == 0)
                {
                    return new HashSet<string>(StringComparer.Ordinal);
                }

                if (!_dtStructuredSchemaTable.Columns.Contains("ColumnName") || !_dtStructuredSchemaTable.Columns.Contains("AutoInc"))
                {
                    return new HashSet<string>(StringComparer.Ordinal);
                }

                return new HashSet<string>
                (
                    _dtStructuredSchemaTable.AsEnumerable()
                                            .Where
                                             (
                                                 row =>
                                                 !string.IsNullOrWhiteSpace(row.GetSafeString("ColumnName")) &&
                                                 string.Equals(row.GetSafeString("AutoInc"), "Y", StringComparison.OrdinalIgnoreCase)
                                             )
                                            .Select(row => row.GetSafeString("ColumnName").Trim()),
                    StringComparer.Ordinal
                );
            }

            string GetDefaultValue(string columnName)
            {
                return defaultValueMap.TryGetValue(columnName, out var defaultValue) ? defaultValue : string.Empty;
            }

            bool IsAutoIncrementColumn(string columnName)
            {
                return autoIncrementColumns.Contains(columnName);
            }

            string QuoteMySqlIdentifier(string identifier)
            {
                return DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.MySql, identifier);
            }

            string BuildMySqlTableName()
            {
                return DmlPreviewSqlBuilder.BuildTableName(DataSourceType.MySql, SchemaNode, _selectedTableName);
            }

            bool IsNewOrCloneOperation(string operationMode)
            {
                return _operationModeNewClone.Contains(operationMode);
            }

            string ConvertMySqlCellValueToSqlLiteral(string cellValue, ColumnInfo columnInfo, string defaultValue, bool allowDefaultKeyword = true)
            {
                return DmlPreviewSqlLiteralFormatter.Format
                       (
                           new DmlPreviewLiteralFormatRequest
                           {
                               DataSourceType = DataSourceType.MySql,
                               CellValue = cellValue,
                               ColumnInfo = columnInfo,
                               DefaultValue = defaultValue,
                               AllowDefaultKeyword = allowDefaultKeyword,
                               UseUpperCaseKeywords = isUpperCase,
                               NullValueIndicators = MyGlobal.AllowedNullValueIndicators
                           }
                       );
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

            IReadOnlyList<DmlPreviewPrimaryKeyColumn> GetMySqlPrimaryKeyColumns()
            {
                if (_dtMySqlPrimaryKeyTable == null || _dtMySqlPrimaryKeyTable.Rows.Count == 0)
                {
                    return Array.Empty<DmlPreviewPrimaryKeyColumn>();
                }

                var primaryKeyColumns = new List<DmlPreviewPrimaryKeyColumn>();

                foreach (DataRow primaryKeyRow in _dtMySqlPrimaryKeyTable.Rows)
                {
                    var columnName = primaryKeyRow.GetSafeString("ColumnName").Trim();
                    var columnIdText = primaryKeyRow.GetSafeString("ColumnID");

                    int.TryParse(columnIdText, out var columnId);

                    ColumnInfo columnInfo = null;

                    if (_columnInfoCollector != null && !string.IsNullOrWhiteSpace(columnName))
                    {
                        _columnInfoCollector.TryGet(columnName, out columnInfo);
                    }

                    primaryKeyColumns.Add
                    (
                        new DmlPreviewPrimaryKeyColumn
                        {
                            ColumnName = columnName,
                            OrdinalPosition = columnId > 0 ? columnId : int.MaxValue,
                            ColumnInfo = columnInfo
                        }
                    );
                }

                return primaryKeyColumns;
            }

            string BuildMySqlPrimaryKeyWhereCondition(DataRow currentRow)
            {
                var primaryKeyColumns = GetMySqlPrimaryKeyColumns();

                var result = DmlPreviewWhereConditionBuilder.Build
                             (
                                 new DmlPreviewWhereConditionRequest
                                 {
                                     DataSourceType = DataSourceType.MySql,
                                     CurrentRow = currentRow,
                                     OriginalTable = _dtOriginalTableData,
                                     RowIdentityColumnName = MyGlobal.Row_Id_PK_JQ,
                                     HasDeclaredPrimaryKey = _dtMySqlPrimaryKeyTable != null && _dtMySqlPrimaryKeyTable.Rows.Count > 0,
                                     PrimaryKeyColumns = primaryKeyColumns,
                                     UseUpperCaseKeywords = isUpperCase,
                                     NullValueIndicators = MyGlobal.AllowedNullValueIndicators
                                 }
                             );

                return result.Success ? result.Condition : string.Empty;
            }

            for (var row = 0; row < dt.Rows.Count; row++) //處理 Delete / Insert
            {
                var dataRow = dt.Rows[row];
                var operationMode = dataRow.GetSafeString(_identifyColumnName);

                if (string.IsNullOrEmpty(operationMode))
                {
                    continue;
                }

                if (string.Equals(operationMode, "DEL", StringComparison.OrdinalIgnoreCase))
                {
                    var whereConditionString = BuildMySqlPrimaryKeyWhereCondition(dataRow);

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

                    if (IsAutoIncrementColumn(columnName))
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
                    var cellValue = column.CellText(gridRow.DataRowIndex);
                    var sqlValue = ConvertMySqlCellValueToSqlLiteral(cellValue, columnInfo, defaultValue);

                    insertFields.Append($"{QuoteMySqlIdentifier(columnName)}, ");
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
                var setString = isUpperCase ? "   SET " : "   set ";
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

                    var updateAssignments = new List<string>();

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

                        if (IsAutoIncrementColumn(columnName))
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
                        var cellValue = dataRow.Table.Columns.Contains(columnName) ? dataRow.GetSafeString(columnName) : dataRow[columnIndex]?.ToString();
                        var sqlValue = ConvertMySqlCellValueToSqlLiteral(cellValue, columnInfo, defaultValue);

                        updateAssignments.Add($"{QuoteMySqlIdentifier(columnName)} = {sqlValue}");
                    }

                    if (updateAssignments.Count == 0)
                    {
                        continue;
                    }

                    var setIndent = new string(' ', setString.Length);
                    var setClause = string.Join($",\r\n{setIndent}", updateAssignments);
                    var whereConditionString = BuildMySqlPrimaryKeyWhereCondition(dataRow);

                    if (string.IsNullOrWhiteSpace(whereConditionString))
                    {
                        continue;
                    }

                    var updateSql = DmlPreviewSqlBuilder.BuildUpdate(tableName, setClause, whereConditionString, isUpperCase);

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
    }
}
