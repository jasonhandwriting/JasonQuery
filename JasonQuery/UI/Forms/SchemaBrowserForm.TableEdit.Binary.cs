using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private sealed class DirectBinaryChange
        {
            public string RowId { get; set; }

            public string ColumnName { get; set; }

            public string BinaryTypeName { get; set; }

            public byte[] Value { get; set; }

            public string DisplayText { get; set; }

            public string SourceFileName { get; set; }

            public string Sql { get; set; }

            public string LocatorDescription { get; set; }
        }

        private readonly Dictionary<string, DirectBinaryChange> _directBinaryChanges = new Dictionary<string, DirectBinaryChange>(StringComparer.Ordinal);

        private void PrepareCellEditorBinaryMode(CellEditorForm form, C1TrueDBGrid grid, int row, int col, ColumnInfo columnInfo)
        {
            if (form == null || grid == null || columnInfo == null)
            {
                return;
            }

            if (columnInfo.CategoryDataTypeKind != CategoryDataTypeKind.LargeBinary)
            {
                return;
            }

            form.IsBlobColumn = true;
            form.BinaryUpdateExecutor = ExecuteCellEditorBinaryUpdate;

            var dataRow = GetDataRowFromGridDisplayRow(grid, row);
            var directChange = GetDirectBinaryChange(dataRow, columnInfo.ColumnName);

            if (directChange != null)
            {
                form.HasBlobContent = true;
                form.Blob = directChange.Value;
                form.BinaryContentLoader = () => directChange.Value;
                form.OriginalCellText = directChange.DisplayText;
                form.Result = directChange.DisplayText;
                return;
            }

            object rawValue = grid[row, col];

            if (rawValue is LargeBinaryDataType binary)
            {
                form.HasBlobContent = !binary.IsNull;
                form.BinaryContentLoader = binary.IsNull ? null : new Func<byte[]>(() => binary.LoadContent());
                return;
            }

            if (rawValue is byte[] bytes)
            {
                form.HasBlobContent = true;
                form.Blob = bytes;
                form.BinaryContentLoader = () => bytes;
            }
        }

        private CellEditorBinaryUpdateResult ExecuteCellEditorBinaryUpdate(CellEditorBinaryUpdateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FileName) || !File.Exists(request.FileName))
            {
                return CellEditorBinaryUpdateResult.Failed("The selected file does not exist.");
            }

            if (_editingRowIndex < 0 || _editingColumnIndex < 0 || _editingColumnIndex >= c1GridData.Columns.Count)
            {
                return CellEditorBinaryUpdateResult.Failed("The target cell is no longer available.");
            }

            var columnName = c1GridData.Columns[_editingColumnIndex].DataField;

            if (string.IsNullOrWhiteSpace(columnName) || _columnInfoCollector == null || !_columnInfoCollector.TryGet(columnName, out var columnInfo))
            {
                return CellEditorBinaryUpdateResult.Failed("The target column metadata is unavailable.");
            }

            if (columnInfo.CategoryDataTypeKind != CategoryDataTypeKind.LargeBinary)
            {
                return CellEditorBinaryUpdateResult.Failed("The target column is not a large binary column.");
            }

            var currentRow = GetDataRowFromGridDisplayRow(c1GridData, _editingRowIndex);

            if (!IsUsableTableDataRow(currentRow))
            {
                return CellEditorBinaryUpdateResult.Failed("The target row is no longer available.");
            }

            var operationMode = currentRow.GetSafeString(_identifyColumnName);

            if (!string.IsNullOrEmpty(operationMode))
            {
                return CellEditorBinaryUpdateResult.Failed("A binary file can only be written directly to an existing row that has not been marked for insert, clone, or delete.");
            }

            if (columnInfo.ColumnSize > 0 && request.FileLength > columnInfo.ColumnSize)
            {
                return CellEditorBinaryUpdateResult.Failed($"The file size exceeds the column length limit.\r\n{request.FileLength:N0} > {columnInfo.ColumnSize:N0}");
            }

            var sqlInfo = BuildCellEditorBinaryUpdateSql(currentRow, columnInfo);

            if (string.IsNullOrWhiteSpace(sqlInfo.Sql))
            {
                return CellEditorBinaryUpdateResult.Failed(sqlInfo.ErrorMessage);
            }

            var errorMessage = string.Empty;
            var affectedRows = ExecuteBinaryFileUpdate(sqlInfo.Sql, sqlInfo.ParameterName, request.FileName, out errorMessage);

            if (affectedRows > 0)
            {
                MarkTransactionAsPending();
                NotifyMainFormTransactionStateChanged();
            }

            var success = affectedRows == 1 && string.IsNullOrEmpty(errorMessage);
            var binaryTypeName = GetBinaryTypeDisplayName(columnInfo);
            var displayText = BuildCellEditorBinaryDisplayText(binaryTypeName, request.FileLength);
            var historyMessage = $":{sqlInfo.ParameterName}\r\n{Path.GetFileName(request.FileName)}\r\n\r\n{sqlInfo.LocatorDescription}";

            UpdateSqlHistory(affectedRows, success ? "Complete" : "Failed", historyMessage, sqlInfo.Sql);

            if (affectedRows > 1)
            {
                errorMessage = $"{errorMessage}\r\nSafety check failed: the UPDATE affected {affectedRows:N0} rows. Please ROLLBACK the transaction.".Trim();
            }

            return new CellEditorBinaryUpdateResult
            {
                Success = success,
                AffectedRows = affectedRows,
                Sql = sqlInfo.Sql,
                ErrorMessage = errorMessage,
                DisplayText = displayText,
                BinaryTypeName = binaryTypeName,
                LocatorDescription = sqlInfo.LocatorDescription
            };
        }

        private int ExecuteBinaryFileUpdate(string sql, string parameterName, string fileName, out string errorMessage)
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return MyGlobal.OracleReader.UploadFileToBlobField(sql, parameterName, fileName, out errorMessage);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return MyGlobal.PostgreSqlReader.UploadFileToBlobField(sql, parameterName, fileName, out errorMessage);
                    }
                case DataSourceType.SqlServer:
                    {
                        return MyGlobal.SqlServerReader.UploadFileToBlobField(sql, parameterName, fileName, out errorMessage);
                    }
                case DataSourceType.MySql:
                    {
                        return MyGlobal.MySqlReader.UploadFileToBlobField(sql, parameterName, fileName, out errorMessage);
                    }
                default:
                    {
                        errorMessage = "Unsupported database type.";
                        return 0;
                    }
            }
        }

        private sealed class BinaryUpdateSqlInfo
        {
            public string Sql { get; set; }

            public string ParameterName { get; set; }

            public string LocatorDescription { get; set; }

            public string ErrorMessage { get; set; }
        }

        private BinaryUpdateSqlInfo BuildCellEditorBinaryUpdateSql(DataRow currentRow, ColumnInfo columnInfo)
        {
            var result = new BinaryUpdateSqlInfo();
            var parameterName = $"{columnInfo.ColumnName}JQ";
            var tableName = BuildBinaryUpdateTableName();
            var columnName = QuoteBinaryUpdateIdentifier(columnInfo.ColumnName);
            var whereInfo = BuildBinaryUpdateWhereCondition(currentRow);

            result.ParameterName = parameterName;
            result.LocatorDescription = whereInfo.LocatorDescription;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                result.ErrorMessage = "The target table name is unavailable.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(whereInfo.WhereCondition))
            {
                result.ErrorMessage = whereInfo.ErrorMessage;
                return result;
            }

            var binaryExpression = _currentSourceType == DataSourceType.SqlServer ? "CONVERT(VARBINARY(MAX), '0x{HEX}', 1)" : $":{parameterName}";

            result.Sql = $"UPDATE {tableName}\r\n   SET {columnName} = {binaryExpression}\r\n WHERE {whereInfo.WhereCondition}";
            return result;
        }

        private sealed class BinaryWhereInfo
        {
            public string WhereCondition { get; set; }

            public string LocatorDescription { get; set; }

            public string ErrorMessage { get; set; }
        }

        private BinaryWhereInfo BuildBinaryUpdateWhereCondition(DataRow currentRow)
        {
            var originalRow = FindOriginalRowForBinaryUpdate(currentRow);
            var primaryKeyColumns = GetBinaryUpdatePrimaryKeyColumns().ToList();

            if (primaryKeyColumns.Count > 0)
            {
                var conditions = new List<string>();
                var descriptions = new List<string>();

                foreach (var primaryKeyColumn in primaryKeyColumns)
                {
                    if (originalRow == null || originalRow.Table == null || !originalRow.Table.Columns.Contains(primaryKeyColumn.ColumnName))
                    {
                        return new BinaryWhereInfo
                        {
                            ErrorMessage = $"The primary key value for {primaryKeyColumn.ColumnName} is unavailable."
                        };
                    }

                    var rawValue = originalRow[primaryKeyColumn.ColumnName];
                    var quotedColumnName = QuoteBinaryUpdateIdentifier(primaryKeyColumn.ColumnName);

                    if (rawValue == null || rawValue == DBNull.Value)
                    {
                        conditions.Add($"{quotedColumnName} IS NULL");
                        descriptions.Add($"{primaryKeyColumn.ColumnName}=NULL");
                    }
                    else
                    {
                        var valueText = Convert.ToString(rawValue);
                        var sqlLiteral = ConvertBinaryLocatorValueToSqlLiteral(valueText, primaryKeyColumn);

                        conditions.Add($"{quotedColumnName} = {sqlLiteral}");
                        descriptions.Add($"{primaryKeyColumn.ColumnName}={valueText}");
                    }
                }

                return new BinaryWhereInfo
                {
                    WhereCondition = string.Join("\r\n   AND ", conditions),
                    LocatorDescription = $"Primary key: {string.Join(", ", descriptions)}"
                };
            }

            if (_currentSourceType == DataSourceType.Oracle || _currentSourceType == DataSourceType.PostgreSql)
            {
                var rowId = currentRow == null ? string.Empty : currentRow.GetSafeString(0);

                if (string.IsNullOrWhiteSpace(rowId))
                {
                    return new BinaryWhereInfo
                    {
                        ErrorMessage = "Neither a complete primary key nor a physical row locator is available."
                    };
                }

                var locatorName = _currentSourceType == DataSourceType.Oracle ? "ROWID" : "ctid";

                return new BinaryWhereInfo
                {
                    WhereCondition = $"{locatorName} = '{EscapeBinarySqlString(rowId)}'",
                    LocatorDescription = $"{locatorName}: {rowId}"
                };
            }

            return new BinaryWhereInfo
            {
                ErrorMessage = "This table has no usable primary key. Direct binary updates are disabled for SQL Server and MySQL because a single target row cannot be identified safely."
            };
        }

        private DataRow FindOriginalRowForBinaryUpdate(DataRow currentRow)
        {
            if (currentRow == null || _dtOriginalTableData == null || _dtOriginalTableData.Rows.Count == 0)
            {
                return currentRow;
            }

            var rowId = currentRow.GetSafeString(MyGlobal.Row_Id_PK_JQ);

            if (string.IsNullOrEmpty(rowId))
            {
                return currentRow;
            }

            return _dtOriginalTableData.AsEnumerable().FirstOrDefault(row => StringComparer.Ordinal.Equals(row.GetSafeString(MyGlobal.Row_Id_PK_JQ), rowId)) ?? currentRow;
        }

        private IEnumerable<ColumnInfo> GetBinaryUpdatePrimaryKeyColumns()
        {
            if (_columnInfoCollector == null)
            {
                return Enumerable.Empty<ColumnInfo>();
            }

            var primaryKeyColumns = _columnInfoCollector.GetAll()
                                                        .Where(column => column != null && column.IsPrimaryKey && !string.IsNullOrWhiteSpace(column.ColumnName))
                                                        .OrderBy(column => GetBinaryColumnOrdinal(column.ColumnName))
                                                        .ToList();

            if (primaryKeyColumns.Count > 0)
            {
                return primaryKeyColumns;
            }

            var primaryKeyTable = _currentSourceType == DataSourceType.SqlServer ? _dtSqlServerPrimaryKeyTable : _currentSourceType == DataSourceType.MySql ? _dtMySqlPrimaryKeyTable : null;

            if (primaryKeyTable == null || primaryKeyTable.Rows.Count == 0 || !primaryKeyTable.Columns.Contains("ColumnName"))
            {
                return primaryKeyColumns;
            }

            foreach (DataRow row in primaryKeyTable.Rows)
            {
                var primaryKeyColumnName = row.GetSafeString("ColumnName");

                if (!string.IsNullOrWhiteSpace(primaryKeyColumnName) && _columnInfoCollector.TryGet(primaryKeyColumnName, out var columnInfo))
                {
                    primaryKeyColumns.Add(columnInfo);
                }
            }

            return primaryKeyColumns.OrderBy(column => GetBinaryColumnOrdinal(column.ColumnName));
        }

        private int GetBinaryColumnOrdinal(string columnName)
        {
            if (_dtStructuredSchemaTable == null || _dtStructuredSchemaTable.Rows.Count == 0 || !_dtStructuredSchemaTable.Columns.Contains("ColumnName") || !_dtStructuredSchemaTable.Columns.Contains("ID"))
            {
                return int.MaxValue;
            }

            var row = _dtStructuredSchemaTable.AsEnumerable()
                                              .FirstOrDefault(item => string.Equals(item.GetSafeString("ColumnName"), columnName, StringComparison.Ordinal));

            return row != null && int.TryParse(row.GetSafeString("ID"), out var ordinal) ? ordinal : int.MaxValue;
        }

        private string BuildBinaryUpdateTableName()
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return _selectedTableName;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return string.IsNullOrWhiteSpace(SchemaNode) ? _selectedTableName : $"{SchemaNode}.{_selectedTableName}";
                    }
                case DataSourceType.SqlServer:
                    {
                        var schemaName = !string.IsNullOrWhiteSpace(SchemaNode) ? SchemaNode : SchemaDbo;

                        return string.IsNullOrWhiteSpace(schemaName) ? QuoteSqlServerBinaryIdentifier(_selectedTableName) : $"{QuoteSqlServerBinaryIdentifier(schemaName)}.{QuoteSqlServerBinaryIdentifier(_selectedTableName)}";
                    }
                case DataSourceType.MySql:
                    {
                        return string.IsNullOrWhiteSpace(SchemaNode) ? QuoteMySqlBinaryIdentifier(_selectedTableName) : $"{QuoteMySqlBinaryIdentifier(SchemaNode)}.{QuoteMySqlBinaryIdentifier(_selectedTableName)}";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private string QuoteBinaryUpdateIdentifier(string identifier)
        {
            switch (_currentSourceType)
            {
                case DataSourceType.SqlServer:
                    {
                        return QuoteSqlServerBinaryIdentifier(identifier);
                    }
                case DataSourceType.MySql:
                    {
                        return QuoteMySqlBinaryIdentifier(identifier);
                    }
                default:
                    {
                        return identifier == null ? string.Empty : identifier.Trim();
                    }
            }
        }

        private static string QuoteSqlServerBinaryIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return string.Empty;
            }

            identifier = identifier.Trim();

            if (identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal))
            {
                return identifier;
            }

            var escapedIdentifier = identifier.Replace("]", "]]");

            return $"[{escapedIdentifier}]";
        }

        private static string QuoteMySqlBinaryIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return string.Empty;
            }

            identifier = identifier.Trim();

            if (identifier.StartsWith("`", StringComparison.Ordinal) && identifier.EndsWith("`", StringComparison.Ordinal))
            {
                return identifier;
            }

            var escapedIdentifier = identifier.Replace("`", "``");

            return $"`{escapedIdentifier}`";
        }

        private string ConvertBinaryLocatorValueToSqlLiteral(string value, ColumnInfo columnInfo)
        {
            value = value ?? string.Empty;

            switch (columnInfo.CategoryDataTypeKind)
            {
                case CategoryDataTypeKind.Number:
                    {
                        return value;
                    }
                case CategoryDataTypeKind.DateTime:
                case CategoryDataTypeKind.String:
                default:
                    {
                        var prefix = _currentSourceType == DataSourceType.SqlServer && columnInfo.SpecialDataTypeKind == SpecialDataTypeKind.NString ? "N" : string.Empty;

                        return $"{prefix}'{EscapeBinarySqlString(value)}'";
                    }
            }
        }

        private static string EscapeBinarySqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static string GetBinaryTypeDisplayName(ColumnInfo columnInfo)
        {
            if (columnInfo == null)
            {
                return "Binary";
            }

            if (!string.IsNullOrWhiteSpace(columnInfo.BaseDataType))
            {
                var baseDataType = columnInfo.BaseDataType.Trim();
                var parenthesisIndex = baseDataType.IndexOf('(');

                return parenthesisIndex > 0 ? baseDataType.Substring(0, parenthesisIndex) : baseDataType;
            }

            return "Binary";
        }

        private static string BuildCellEditorBinaryDisplayText(string binaryTypeName, long length)
        {
            var typeName = string.IsNullOrWhiteSpace(binaryTypeName) ? "Binary" : binaryTypeName;

            return $"({typeName})({length:N0})";
        }

        private void CellEditorForm_BinaryValueApplied(object sender, CellEditorBinaryValueAppliedEventArgs e)
        {
            if (e == null || e.UpdateResult == null || !e.UpdateResult.Success)
            {
                return;
            }

            var dataRow = GetDataRowFromGridDisplayRow(c1GridData, _editingRowIndex);

            if (!IsUsableTableDataRow(dataRow) || _editingColumnIndex < 0 || _editingColumnIndex >= c1GridData.Columns.Count)
            {
                return;
            }

            var columnName = c1GridData.Columns[_editingColumnIndex].DataField;
            var rowId = dataRow.GetSafeString(MyGlobal.Row_Id_PK_JQ);

            if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(rowId))
            {
                return;
            }

            var valueCopy = e.Value == null ? Array.Empty<byte>() : (byte[])e.Value.Clone();

            var change = new DirectBinaryChange
            {
                RowId = rowId,
                ColumnName = columnName,
                BinaryTypeName = e.UpdateResult.BinaryTypeName,
                Value = valueCopy,
                DisplayText = e.UpdateResult.DisplayText,
                SourceFileName = Path.GetFileName(e.SourceFileName),
                Sql = e.UpdateResult.Sql,
                LocatorDescription = e.UpdateResult.LocatorDescription
            };

            _directBinaryChanges[BuildDirectBinaryChangeKey(rowId, columnName)] = change;

            TryReplaceBinaryGridValue(dataRow, columnName, change);

            RefreshTableEditStateMarkers();
            CheckButtonsStatus();
            RefreshDataGridDisplayAfterCellEditorUpdate();
        }

        private void TryReplaceBinaryGridValue(DataRow dataRow, string columnName, DirectBinaryChange change)
        {
            if (dataRow == null || dataRow.Table == null || !dataRow.Table.Columns.Contains(columnName) || change == null)
            {
                return;
            }

            try
            {
                var valueCopy = change.Value == null ? Array.Empty<byte>() : (byte[])change.Value.Clone();
                var loadCore = new Func<byte[]>(() => valueCopy);

                //以反射集中相容目前 LargeBinaryDataType 的五參數建構式。
                //若日後建構式改名，只需調整此一處，不會讓 CellEditorForm 再次依賴型別內部實作。
                var value = Activator.CreateInstance
                (
                    typeof(LargeBinaryDataType),
                    new object[]
                    {
                        change.DisplayText,
                        null,
                        valueCopy.Length,
                        false,
                        loadCore
                    }
                );

                dataRow[columnName] = value;
                dataRow.EndEdit();
            }
            catch
            {
                //如果目前專案中的 LargeBinaryDataType 建構式已調整，
                //仍保留直接異動狀態與 Cell 標色；只需在此一處換成專案現行 factory。
            }
        }

        private DirectBinaryChange GetDirectBinaryChange(DataRow row, string columnName)
        {
            if (row == null || string.IsNullOrWhiteSpace(columnName))
            {
                return null;
            }

            var rowId = row.GetSafeString(MyGlobal.Row_Id_PK_JQ);

            if (string.IsNullOrWhiteSpace(rowId))
            {
                return null;
            }

            _directBinaryChanges.TryGetValue(BuildDirectBinaryChangeKey(rowId, columnName), out var change);

            return change;
        }

        private static string BuildDirectBinaryChangeKey(string rowId, string columnName)
        {
            return $"{rowId}\u001f{columnName}";
        }

        private bool IsDirectBinaryCell(int displayRowIndex, int columnIndex)
        {
            if (_directBinaryChanges.Count == 0 || displayRowIndex < 0 || columnIndex < 0 || columnIndex >= c1GridData.Columns.Count)
            {
                return false;
            }

            var row = GetDataRowFromGridDisplayRow(c1GridData, displayRowIndex);
            var columnName = c1GridData.Columns[columnIndex].DataField;

            return GetDirectBinaryChange(row, columnName) != null;
        }

        private bool HasDirectBinaryChanges()
        {
            return _directBinaryChanges.Count > 0;
        }

        private void ClearDirectBinaryChanges()
        {
            _directBinaryChanges.Clear();
        }

        private void ApplyDirectBinaryChangeToViewerContext(DataRow row, string columnName, CellViewerCellContext context)
        {
            if (context == null)
            {
                return;
            }

            var change = GetDirectBinaryChange(row, columnName);

            if (change == null)
            {
                return;
            }

            context.IsBinaryColumn = true;
            context.HasBinaryContent = true;
            context.BinaryLength = change.Value == null ? 0 : change.Value.Length;
            context.BinaryContent = change.Value;
            context.BinaryContentLoader = () => change.Value;
            context.DisplayText = change.DisplayText;
        }

        private string BuildDirectBinaryPreviewNotes()
        {
            if (_directBinaryChanges.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine("-- <JASONQUERY_DIRECT_BINARY_CHANGES>");
            sb.AppendLine("-- Binary changes already executed with provider parameters");
            sb.AppendLine("-- They cannot be represented safely as text SQL literals.");
            sb.AppendLine("-- COMMIT or ROLLBACK is still required for the current transaction.");

            foreach (var change in _directBinaryChanges.Values.OrderBy(item => item.RowId).ThenBy(item => item.ColumnName))
            {
                sb.AppendLine("--");
                sb.AppendLine($"-- Column: {change.ColumnName}");
                sb.AppendLine($"-- File: {change.SourceFileName}");
                sb.AppendLine($"-- Size: {(change.Value == null ? 0 : change.Value.Length):N0} bytes");
                sb.AppendLine($"-- Locator: {change.LocatorDescription}");
            }

            sb.AppendLine("-- </JASONQUERY_DIRECT_BINARY_CHANGES>");

            return sb.ToString().TrimEnd();
        }

        private static string RemoveDirectBinaryPreviewNotes(string previewText)
        {
            if (string.IsNullOrEmpty(previewText))
            {
                return string.Empty;
            }

            const string startMarker = "-- <JASONQUERY_DIRECT_BINARY_CHANGES>";
            const string endMarker = "-- </JASONQUERY_DIRECT_BINARY_CHANGES>";

            var startIndex = previewText.IndexOf(startMarker, StringComparison.Ordinal);

            if (startIndex < 0)
            {
                return previewText;
            }

            var endIndex = previewText.IndexOf(endMarker, startIndex, StringComparison.Ordinal);

            if (endIndex < 0)
            {
                return previewText.Substring(0, startIndex).TrimEnd();
            }

            endIndex += endMarker.Length;

            return (previewText.Substring(0, startIndex) + previewText.Substring(endIndex)).Trim();
        }

        private void NotifyMainFormTransactionStateChanged()
        {
            if (DisplayRowIndex == -1)
            {
                SetCommitRollbackButtonsEnabled(true);
                return;
            }

            TransferValueToMainForm("UpdateCommitRollbackButton`");
        }
    }
}
