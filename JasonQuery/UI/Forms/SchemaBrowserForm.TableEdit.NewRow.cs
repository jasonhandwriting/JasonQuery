using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using JasonQuery.Core.QueryEngine.Types;
using System;
using System.Data;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void btnInsertNewRow_Click(object sender, EventArgs e)
        {
            InsertNewRow();
        }

        private void InsertNewRow()
        {
            var currentRow = c1GridData.Row;
            DataRow row = null;

            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            row = NewRow_Oracle();
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            row = NewRow_PostgreSql();
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            row = NewRow_SqlServer();
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            row = NewRow_MySql();
                            break;
                        }
                }

                _editingRowIndex = currentRow + 1;
                _dtTableData.Rows.InsertAt(row, currentRow + 1);
                c1GridData.Row = currentRow + 1;

                SetGridFormat();
                RefreshTableEditStateMarkers();
                CheckButtonsStatus(); //20260529 檢查按鈕狀態
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private DataRow NewRow_Oracle()
        {
            var row = _dtTableData.NewRow();

            foreach (C1DataColumn column in c1GridData.Columns)
            {
                var columnName = column.DataField;

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                {
                    row[MyGlobal.Row_Id_PK_JQ] = DateTime.Now.ToString(_dateTimeFormat);
                }
                else if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    row[_identifyColumnName] = "NEW";
                }
                else
                {
                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        var isEditable = IsTableDataColumnEditable(columnInfo);
                        var isNullable = columnInfo.IsNullable;
                        var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));
                        var nullShowAs = MyLibrary.GridNullShowAs;
                        var hasDefaultValue = !string.IsNullOrEmpty(defaultValue);

                        switch (columnInfo.CategoryDataTypeKind)
                        {
                            case CategoryDataTypeKind.String when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue.Trim('\'') : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.Number when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : "0");
                                    break;
                                }
                            case CategoryDataTypeKind.DateTime when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeBinary:
                                {
                                    row[columnName] = LargeBinaryDataType.CreateNull(nullShowAs);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeText:
                                {
                                    row[columnName] = LargeTextDataType.CreateNull("(Unsupported)");
                                    break;
                                }
                            default:
                                {
                                    if (!isEditable)
                                    {
                                        row[columnName] = "(Unsupported)";
                                    }
                                    else
                                    {
                                        row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return row;
        }

        private DataRow NewRow_PostgreSql()
        {
            var row = _dtTableData.NewRow();

            foreach (C1DataColumn column in c1GridData.Columns)
            {
                var columnName = column.DataField;

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                {
                    row[MyGlobal.Row_Id_PK_JQ] = DateTime.Now.ToString(_dateTimeFormat);
                }
                else if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    row[_identifyColumnName] = "NEW";
                }
                else
                {
                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        var isEditable = IsTableDataColumnEditable(columnInfo);
                        var isNullable = columnInfo.IsNullable;
                        var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));
                        var nullShowAs = MyLibrary.GridNullShowAs;
                        var hasDefaultValue = !string.IsNullOrEmpty(defaultValue);

                        switch (columnInfo.CategoryDataTypeKind)
                        {
                            case CategoryDataTypeKind.String when isEditable:
                                {
                                    var tempValue = defaultValue.IndexOf("::", StringComparison.Ordinal);

                                    if (tempValue >= 0)
                                    {
                                        defaultValue = defaultValue.Substring(0, tempValue);
                                    }

                                    row[columnName] = hasDefaultValue ? defaultValue.Trim('\'') : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.Number when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : "0");
                                    break;
                                }
                            case CategoryDataTypeKind.DateTime when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeBinary:
                                {
                                    row[columnName] = LargeBinaryDataType.CreateNull(nullShowAs);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeText:
                                {
                                    row[columnName] = LargeTextDataType.CreateNull("(Unsupported)");
                                    break;
                                }
                            default:
                                {
                                    if (!isEditable)
                                    {
                                        row[columnName] = "(Unsupported)";
                                    }
                                    else
                                    {
                                        row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return row;
        }

        private DataRow NewRow_SqlServer()
        {
            var row = _dtTableData.NewRow();

            foreach (C1DataColumn column in c1GridData.Columns)
            {
                var columnName = column.DataField;

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                {
                    row[MyGlobal.Row_Id_PK_JQ] = DateTime.Now.ToString(_dateTimeFormat);
                }
                else if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    row[_identifyColumnName] = "NEW";
                }
                else
                {
                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        var isEditable = IsTableDataColumnEditable(columnInfo);
                        var isNullable = columnInfo.IsNullable;
                        var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));
                        var nullShowAs = MyLibrary.GridNullShowAs;
                        var hasDefaultValue = !string.IsNullOrEmpty(defaultValue);

                        switch (columnInfo.CategoryDataTypeKind)
                        {
                            case CategoryDataTypeKind.String when isEditable:
                                {
                                    if (defaultValue.StartsWith("N", StringComparison.OrdinalIgnoreCase))
                                    {
                                        defaultValue = defaultValue.Substring(1);
                                    }

                                    row[columnName] = hasDefaultValue ? defaultValue.Trim('\'') : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.Number when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : "0");
                                    break;
                                }
                            case CategoryDataTypeKind.DateTime when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeBinary:
                                {
                                    row[columnName] = LargeBinaryDataType.CreateNull(nullShowAs);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeText:
                                {
                                    row[columnName] = LargeTextDataType.CreateNull("(Unsupported)");
                                    break;
                                }
                            default:
                                {
                                    if (!isEditable)
                                    {
                                        row[columnName] = "(Unsupported)";
                                    }
                                    else
                                    {
                                        row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return row;
        }

        private DataRow NewRow_MySql()
        {
            var row = _dtTableData.NewRow();

            foreach (C1DataColumn column in c1GridData.Columns)
            {
                var columnName = column.DataField;

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase))
                {
                    row[MyGlobal.Row_Id_PK_JQ] = DateTime.Now.ToString(_dateTimeFormat);
                }
                else if (string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    row[_identifyColumnName] = "NEW";
                }
                else
                {
                    if (_columnInfoCollector != null && _columnInfoCollector.TryGet(columnName, out var columnInfo))
                    {
                        var isEditable = IsTableDataColumnEditable(columnInfo);
                        var isNullable = columnInfo.IsNullable;
                        var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));
                        var nullShowAs = MyLibrary.GridNullShowAs;
                        var hasDefaultValue = !string.IsNullOrEmpty(defaultValue);

                        switch (columnInfo.CategoryDataTypeKind)
                        {
                            case CategoryDataTypeKind.String when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue.Trim('\'') : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.Number when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : "0");
                                    break;
                                }
                            case CategoryDataTypeKind.DateTime when isEditable:
                                {
                                    row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeBinary:
                                {
                                    row[columnName] = LargeBinaryDataType.CreateNull(nullShowAs);
                                    break;
                                }
                            case CategoryDataTypeKind.LargeText:
                                {
                                    row[columnName] = LargeTextDataType.CreateNull("(Unsupported)");
                                    break;
                                }
                            default:
                                {
                                    if (columnInfo.SpecialDataTypeKind == SpecialDataTypeKind.Bit)
                                    {
                                        if (defaultValue.StartsWith("B'", StringComparison.OrdinalIgnoreCase))
                                        {
                                            defaultValue = defaultValue.Substring(1);
                                        }

                                        defaultValue = defaultValue.TrimEnd('\'');
                                        row[columnName] = hasDefaultValue ? defaultValue.Trim('\'') : (isNullable ? nullShowAs : string.Empty);
                                    }
                                    else if (!isEditable)
                                    {
                                        row[columnName] = "(Unsupported)";
                                    }
                                    else
                                    {
                                        row[columnName] = hasDefaultValue ? defaultValue : (isNullable ? nullShowAs : string.Empty);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            return row;
        }
    }
}