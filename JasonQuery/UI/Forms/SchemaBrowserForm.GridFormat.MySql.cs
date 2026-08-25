using C1.Win.C1Input;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private static GridNumericValueKind GetMySqlNumericValueKind(string dataType, int numericPrecision)
        {
            var isUnsigned = dataType.IndexOf("UNSIGNED", StringComparison.OrdinalIgnoreCase) >= 0;
            var baseDataType = dataType.Replace(" UNSIGNED", string.Empty).Trim();

            switch (baseDataType)
            {
                case "TINYINT":
                    {
                        return isUnsigned ? GridNumericValueKind.Byte : GridNumericValueKind.SByte;
                    }
                case "SMALLINT":
                    {
                        return isUnsigned ? GridNumericValueKind.UInt16 : GridNumericValueKind.Int16;
                    }
                case "MEDIUMINT":
                case "INT":
                case "INTEGER":
                    {
                        return isUnsigned ? GridNumericValueKind.UInt32 : GridNumericValueKind.Int32;
                    }
                case "BIGINT":
                    {
                        return isUnsigned ? GridNumericValueKind.UInt64 : GridNumericValueKind.Int64;
                    }
                case "FLOAT":
                    {
                        return GridNumericValueKind.Single;
                    }
                case "DOUBLE":
                case "REAL":
                    {
                        return GridNumericValueKind.Double;
                    }
                case "DECIMAL":
                case "NUMERIC":
                    {
                        return numericPrecision > 28 ? GridNumericValueKind.ArbitraryPrecisionDecimal : GridNumericValueKind.Decimal;
                    }
                default:
                    {
                        return GridNumericValueKind.ArbitraryPrecisionDecimal;
                    }
            }
        }

        private void ConfigureMySqlGridDataEditors()
        {
            var gridColumnIndex = 0;

            foreach (DataRow rawSchemaRow in _dtRawSchemaTable?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
            {
                var columnName = rawSchemaRow.GetSafeString("ColumnName");

                if (string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase) || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    gridColumnIndex++;
                    continue;
                }

                if (!TryAlignGridDataColumnIndex(ref gridColumnIndex, columnName))
                {
                    gridColumnIndex++;
                    continue;
                }

                //20260503 透過 _columnInfoCollector 取得相關的欄位資訊，包含是否為主鍵、是否為外鍵、是否允許為空、預設值、資料型態等資訊，提供給後續設定 Cell Editor 時使用
                if (!TryGetGridDataColumnInfo(columnName, out var columnInfo))
                {
                    gridColumnIndex++;
                    continue;
                }

                //20240907 取得預設值及是否為空
                var isNullable = columnInfo.IsNullable;
                var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));

                if (defaultValue == MyLibrary.GridNullShowAs)
                {
                    defaultValue = string.Empty;
                }

                if (!string.IsNullOrEmpty(defaultValue) && (defaultValue.StartsWith("'", StringComparison.Ordinal) || defaultValue.StartsWith("B'", StringComparison.OrdinalIgnoreCase)))
                {
                    var unquotedDefaultValue = TextHelper.GetStringBetween(defaultValue, "'", "'");

                    if (!string.IsNullOrEmpty(unquotedDefaultValue))
                    {
                        defaultValue = unquotedDefaultValue;
                    }
                }

                var dataType = rawSchemaRow.GetSafeString("DataType");
                var providerType = rawSchemaRow.GetSafeString("ProviderType");
                var providerSpecificDataType = rawSchemaRow.GetSafeString("ProviderSpecificDataType");
                var columnSize = columnInfo.ColumnSize;
                var numericPrecision = columnInfo.NumericPrecision;
                var numericScale = columnInfo.NumericScale;
                var millisecondsFormat = numericScale > 0 ? $".{new string('f', Math.Min(7, numericScale))}" : string.Empty;

                if (IsReadOnlyLargeTextColumn(columnInfo))
                {
                    RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                   isNullable, defaultValue);

                    gridColumnIndex++;
                    continue;
                }

                dataType = columnInfo.BaseDataType;

                switch (dataType)
                {
                    case "CHAR": //最大長度 0 到 255
                    case "VARCHAR": //最大長度 0 到 65535
                    case "TINYTEXT": //最大長度 255
                    case "TEXT": //最大長度 65535
                    case "MEDIUMTEXT": //最大長度 16777215
                    case "LONGTEXT": //最大長度 4294967295
                        {
                            //MySQL
                            var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                            {
                                MaxLength = columnSize, //設定字串最大的可輸入長度
                            };

                            //針對可能輸入中文字，此處必須額外處理，否則使用者輸入「中、英混合字串，長度會被誤判」會出現例外錯誤
                            RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                             isNullable, defaultValue,
                                                             new TextGridEditorOptions
                                                             {
                                                                 MaxLength = columnSize,
                                                                 LengthMode = GridTextLengthMode.OracleOrMySqlByteAware,
                                                                 UseCellEditorResult = true,
                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                             });
                            break;
                        }
                    case "INT": //20240831 int 與 integer 是一樣的，建表後會是 int (不會是 integer)
                    case "INT UNSIGNED": //20240831 int unsigned 與 integer unsigned 是一樣的，建表後會是 int unsigned (不會是 integer unsigned)
                    case "TINYINT":
                    case "TINYINT UNSIGNED":
                    case "SMALLINT":
                    case "SMALLINT UNSIGNED":
                    case "MEDIUMINT":
                    case "MEDIUMINT UNSIGNED":
                    case "BIGINT":
                    case "BIGINT UNSIGNED":
                    case "DOUBLE":
                    case "DOUBLE UNSIGNED":
                    case "FLOAT":
                    case "FLOAT UNSIGNED":
                    case "DECIMAL":
                    case "DECIMAL UNSIGNED":
                        {
                            //數值型態，使用 C1NumericEdit 當作編輯器，因為 C1NumericEdit 有「全選」的屬性
                            var numericEditor = new C1NumericEdit
                            {
                                //NumericInput = true, //這個屬性在這裡沒有效果，改用 KeyPress 事件替代
                                TextAlign = HorizontalAlignment.Right,
                                InitialSelection = InitialSelectionEnum.SelectAll,
                                VisibleButtons = DropDownControlButtonFlags.DropDown
                            };

                            //20240826 欄位型態帶有 unsigned，表示它只能輸入正數，且必須 >= 0

                            var numericValueKind = GetMySqlNumericValueKind(dataType, numericPrecision);

                            RegisterNumericGridDataColumnEditor(gridColumnIndex, columnName, numericEditor,
                                                                isNullable, defaultValue,
                                                                new NumericGridEditorOptions
                                                                {
                                                                    NumericValueKind = numericValueKind,
                                                                    Precision = numericPrecision,
                                                                    Scale = numericScale,
                                                                    AllowNegative = dataType.IndexOf("UNSIGNED", StringComparison.OrdinalIgnoreCase) < 0,
                                                                    AllowExponent = numericValueKind == GridNumericValueKind.Single || numericValueKind == GridNumericValueKind.Double,
                                                                    DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                    RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.Zero,
                                                                    NullableEmptyBehavior = GridEditorNullableEmptyBehavior.Zero
                                                                });
                            break;
                        }
                    case "YEAR":
                        {
                            //數值型態，使用 C1NumericEdit 當作編輯器，因為 C1NumericEdit 有「全選」的屬性
                            var numericEditor = new C1NumericEdit
                            {
                                //NumericInput = true, //這個屬性在這裡沒有效果，改用 KeyPress 事件替代
                                TextAlign = HorizontalAlignment.Right,
                                InitialSelection = InitialSelectionEnum.SelectAll,
                                VisibleButtons = DropDownControlButtonFlags.None,
                                MaxLength = 4 //設定字串最大的可輸入長度
                            };

                            RegisterNumericGridDataColumnEditor(gridColumnIndex, columnName, numericEditor,
                                                                isNullable, defaultValue,
                                                                new NumericGridEditorOptions
                                                                {
                                                                    NumericValueKind = GridNumericValueKind.Int32,
                                                                    Precision = 4,
                                                                    Scale = 0,
                                                                    AllowNegative = false,
                                                                    MinimumValue = 1901m,
                                                                    MaximumValue = 2155m,
                                                                    DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                    RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                                    NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText,
                                                                    RequiredEmptyValue = DateTime.Now.ToString("yyyy")
                                                                });
                            break;
                        }
                    case "BIT": //MySQL, BIT 長度範圍為 1~64
                        {
                            var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                            {
                                MaxLength = columnSize //設定字串最大的可輸入長度
                            };

                            RegisterBitGridDataColumnEditor(gridColumnIndex, columnName, textEditor, isNullable,
                                                            defaultValue, columnSize, true, true, true);

                            break;
                        }
                    case "JSON": //前後加上單引號即可，但使用者輸入的內容如果不正常，INSERT/UPDATE 就會失敗！故先不處理！
                    case "GEOMETRY":
                    case "POINT":
                    case "LINESTRING":
                    case "POLYGON":
                    case "MULTIPOINT":
                    case "MULTILINESTRING":
                    case "MULTIPOLYGON":
                    case "GEOMCOLLECTION":
                        {
                            RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.None,
                                                           isNullable, defaultValue);

                            break;
                        }
                    case "DATE": //只有年月日
                    case "DATETIME": //年月日時分秒毫秒
                        {
                            var customFormat = MyLibrary.DateFormat;

                            if (dataType == "DATETIME")
                            {
                                customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{millisecondsFormat}";
                            }

                            //C1.Win.C1Input.C1DateEdit 毫秒最多只支援 7位數 (受限於 .Net DateTime 型態)
                            var dateEditor = new C1DateEdit
                            {
                                CustomFormat = customFormat,
                                FormatType = FormatTypeEnum.CustomFormat,
                                //DateTimeInput = false, //只有日期(無時分秒)
                                ShowUpDownButtons = false,
                                NullText = MyLibrary.GridNullShowAs
                            };

                            dateEditor.DisplayFormat.CustomFormat = customFormat;
                            dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                            dateEditor.EditFormat.CustomFormat = customFormat;
                            dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                            dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                            dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                            RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                 isNullable, defaultValue,
                                                                 new DateTimeGridEditorOptions
                                                                 {
                                                                     ValueKind = GridDateTimeValueKind.DateTime,
                                                                     Format = customFormat,
                                                                     DefaultValueBehavior = GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault,
                                                                     RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                     NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                 });
                            c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                            break;
                        }
                    case "TIME": //時分秒毫秒；MySQL 允許 -838:59:59 到 838:59:59
                        {
                            var customFormat = $"HH:mm:ss{millisecondsFormat}";
                            var textEditor = new TextBox
                            {
                                MaxLength = 11 + (numericScale > 0 ? Math.Min(6, numericScale) + 1 : 0)
                            };

                            //MySQL TIME 可超過 24小時，也可以是負值；C1DateEdit 以 DateTime 為核心，無法完整表達這個範圍，因此改用 TextBox + 強型別 TimeSpan 驗證
                            RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                                 isNullable, defaultValue,
                                                                 new DateTimeGridEditorOptions
                                                                 {
                                                                     ValueKind = GridDateTimeValueKind.MySqlTimeSpan,
                                                                     Format = customFormat,
                                                                     DefaultValueBehavior = GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault,
                                                                     RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                     NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                 });
                            break;
                        }
                    case "TIMESTAMP": //年月日時分秒毫秒+時區
                        {
                            var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{millisecondsFormat}";

                            var dateEditor = new C1DateEdit
                            {
                                CustomFormat = customFormat,
                                FormatType = FormatTypeEnum.CustomFormat,
                                ShowUpDownButtons = false,
                                NullText = MyLibrary.GridNullShowAs
                            };

                            dateEditor.DisplayFormat.CustomFormat = customFormat;
                            dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                            dateEditor.EditFormat.CustomFormat = customFormat;
                            dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                            dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                            dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                            RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                                 isNullable, defaultValue,
                                                                 new DateTimeGridEditorOptions
                                                                 {
                                                                     ValueKind = GridDateTimeValueKind.DateTime,
                                                                     Format = customFormat,
                                                                     DefaultValueBehavior = GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault,
                                                                     RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                     NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                                 });

                            c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                            break;
                        }
                    default:
                        {
                            if (providerSpecificDataType == "Devart.Data.MySql.MySqlBlob" && dataType.IndexOf("BLOB", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.Binary,
                                                               isNullable, defaultValue);
                            }
                            else
                            {
                                RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.None,
                                                               isNullable, defaultValue);
                            }

                            break;
                        }
                }

                gridColumnIndex++;
            }
        }
    }
}
