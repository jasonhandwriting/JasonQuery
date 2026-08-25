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
        private static GridNumericValueKind GetSqlServerNumericValueKind(string dataType, string dataTypeName, int numericPrecision)
        {
            switch (dataType)
            {
                case "System.Byte":
                    {
                        return GridNumericValueKind.Byte;
                    }
                case "System.Int16":
                    {
                        return GridNumericValueKind.Int16;
                    }
                case "System.Int32":
                    {
                        return GridNumericValueKind.Int32;
                    }
                case "System.Int64":
                    {
                        return GridNumericValueKind.Int64;
                    }
                case "System.Single":
                    {
                        return GridNumericValueKind.Single;
                    }
                case "System.Double":
                    {
                        return GridNumericValueKind.Double;
                    }
                case "System.Decimal":
                    {
                        return numericPrecision > 28 ? GridNumericValueKind.ArbitraryPrecisionDecimal : GridNumericValueKind.Decimal;
                    }
                default:
                    {
                        return dataTypeName == "money" || dataTypeName == "smallmoney"
                               ? GridNumericValueKind.Decimal
                               : GridNumericValueKind.ArbitraryPrecisionDecimal;
                    }
            }
        }

        private void ConfigureSqlServerGridDataEditors()
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

                var hasColumnInfo = TryGetGridDataColumnInfo(columnName, out var columnInfo);

                //共通欄位資訊優先使用 ColumnInfoCollector，Devart Provider 資訊仍由 RawSchema 補足。
                var isNullable = hasColumnInfo ? columnInfo.IsNullable
                                 : string.Equals(rawSchemaRow.GetSafeString("AllowDBNull"), "True", StringComparison.OrdinalIgnoreCase);

                var defaultValue = DataTableSearchHelper.FindValueFromDataTable(_dtStructuredSchemaTable, "Default", ("ColumnName", columnName));

                if (!string.IsNullOrEmpty(defaultValue) && (defaultValue.StartsWith("'", StringComparison.Ordinal) || defaultValue.StartsWith("N'", StringComparison.Ordinal)))
                {
                    var unquotedDefaultValue = TextHelper.GetStringBetween(defaultValue, "'", "'");

                    if (!string.IsNullOrEmpty(unquotedDefaultValue))
                    {
                        defaultValue = unquotedDefaultValue;
                    }
                }

                var isNumeric = false;
                var dataType = rawSchemaRow.GetSafeString("DataType");
                var dataTypeName = rawSchemaRow.GetSafeString("DataTypeName");
                var providerSpecificDataType = rawSchemaRow.GetSafeString("ProviderSpecificDataType");
                var numericPrecision = hasColumnInfo ? columnInfo.NumericPrecision : rawSchemaRow.GetSafeInt("NumericPrecision");
                var numericScale = hasColumnInfo ? columnInfo.NumericScale : rawSchemaRow.GetSafeInt("NumericScale");
                var columnSize = hasColumnInfo ? columnInfo.ColumnSize : rawSchemaRow.GetSafeInt("ColumnSize");

                if (hasColumnInfo && IsReadOnlyLargeTextColumn(columnInfo))
                {
                    RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.LargeText,
                                                   isNullable, defaultValue);

                    gridColumnIndex++;
                    continue;
                }

                switch (dataType)
                {
                    case "System.Byte": //型態 tinyint: 0 to 255
                    case "System.Int16": //型態 smallint: -32768 to 32767
                    case "System.Int32": //型態 int: -2147483648 to 2147483647
                    case "System.Int64": //型態 bigint: -9223372036854775808 to 9223372036854775807
                        {
                            isNumeric = true;
                            break;
                        }
                    case "System.Decimal":
                    case "System.Double":
                    case "System.Single":
                        {
                            isNumeric = true;
                            break;
                        }
                    default:
                        {
                            //20240817 money, smallmoney 也算是數值
                            if (dataTypeName == "money" || dataTypeName == "smallmoney")
                            {
                                //money: −922,337,203,685,477.5808 to +922,337,203,685,477.5807
                                //smallmoney: -214,748.3648 to 214,748.3647
                                isNumeric = true;
                            }

                            break;
                        }
                }

                //針對日期欄位特別處理
                if (dataType.StartsWith("System.DateTime", StringComparison.Ordinal) || dataType == "System.TimeSpan" || dataType == "System.DateTimeOffset")
                {
                    //20240817 date 是單純的日期；smalldatetime 是單純的日期+時分秒
                    if (dataTypeName != "date" && dataTypeName != "smalldatetime" && (dataTypeName == "datetime2" || (dataTypeName == "time" && dataType == "System.TimeSpan") || dataType == "System.DateTimeOffset" || providerSpecificDataType == "Devart.Data.SqlServer.SqlTypes.SqlDateTime") && numericScale >= 0)
                    {
                        var timeZoneFormat = dataType == "System.DateTimeOffset" ? " zzz" : string.Empty;
                        var fractionSeparator = numericScale > 0 ? "." : string.Empty;
                        var fractionDigitsFormat = new string('f', Math.Min(7, numericScale));
                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{fractionSeparator}{fractionDigitsFormat}{timeZoneFormat}";

                        if (dataTypeName == "time" && dataType == "System.TimeSpan")
                        {
                            var timeFractionDigitsFormat = new string('f', Math.Min(7, numericScale));

                            customFormat = $"HH:mm:ss{fractionSeparator}{timeFractionDigitsFormat}";
                        }

                        //C1.Win.C1Input.C1DateEdit 毫秒最多只支援 7位數 (受限於 .Net DateTime 型態)
                        var dateEditor = new C1DateEdit
                        {
                            CustomFormat = customFormat,
                            FormatType = FormatTypeEnum.CustomFormat,
                            ShowUpDownButtons = false,
                            NullText = MyLibrary.GridNullShowAs
                        };

                        if (dataTypeName == "time" && dataType == "System.TimeSpan")
                        {
                            dateEditor.ShowDropDownButton = false;
                            dateEditor.ShowUpDownButtons = false;
                        }

                        dateEditor.DisplayFormat.CustomFormat = customFormat;
                        dateEditor.DisplayFormat.FormatType = FormatTypeEnum.CustomFormat;
                        dateEditor.EditFormat.CustomFormat = customFormat;
                        dateEditor.EditFormat.FormatType = FormatTypeEnum.CustomFormat;
                        dateEditor.Calendar.TodayText = LocalizationHelper.GetLanguageString("&Today", "form", GetType().Name, "msg", "TodayText", "Text");
                        dateEditor.Calendar.ClearText = LocalizationHelper.GetLanguageString("&Clear", "form", GetType().Name, "msg", "ClearText", "Text");

                        //使用者手動輸入或是點選「日期下拉框」並選定日期的動作
                        RegisterDateTimeGridDataColumnEditor(gridColumnIndex, columnName, dateEditor,
                                                             isNullable, defaultValue,
                                                             new DateTimeGridEditorOptions
                                                             {
                                                                 ValueKind = dataType == "System.DateTimeOffset" ? GridDateTimeValueKind.DateTimeOffset
                                                                                         : dataType == "System.TimeSpan" ? GridDateTimeValueKind.TimeSpan
                                                                                         : GridDateTimeValueKind.DateTime,
                                                                 Format = customFormat,
                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                             });

                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                    }
                    else
                    {
                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss";

                        //20240817 date 是單純的日期；smalldatetime 是單純的日期+時分秒
                        if (dataTypeName == "date")
                        {
                            customFormat = MyLibrary.DateFormat;
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
                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                             });

                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                    }
                }
                else if (dataType == "System.String")
                {
                    var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                    {
                        MaxLength = columnSize //設定字串最大的可輸入長度
                    };

                    //針對可能輸入中文字，此處必須額外處理，否則使用者輸入「中、英混合字串，長度會被誤判」會出現例外錯誤
                    RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                     isNullable, defaultValue,
                                                     new TextGridEditorOptions
                                                     {
                                                         MaxLength = columnSize,
                                                         LengthMode = GridTextLengthMode.CharacterCount,
                                                         UseCellEditorResult = true,
                                                         DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                         RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                         NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                     });
                }
                else if (dataType.StartsWith("System.Boolean", StringComparison.Ordinal))
                {
                    var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                    {
                        MaxLength = 5 //設定字串最大的可輸入長度
                    };

                    RegisterBooleanGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                        isNullable, defaultValue, false);
                }
                else if (isNumeric)
                {
                    //數值型態，使用 C1NumericEdit 當作編輯器，因為 C1NumericEdit 有「全選」的屬性
                    var numericEditor = new C1NumericEdit
                    {
                        //NumericInput = true, //這個屬性在這裡沒有效果，改用 KeyPress 事件替代
                        TextAlign = HorizontalAlignment.Right,
                        InitialSelection = InitialSelectionEnum.SelectAll,
                        VisibleButtons = DropDownControlButtonFlags.DropDown
                    };

                    //tinyint: 沒有負數，不允許輸入負號
                    var numericValueKind = GetSqlServerNumericValueKind(dataType, dataTypeName, numericPrecision);
                    decimal? minimumValue = null;
                    decimal? maximumValue = null;

                    if (string.Equals(dataTypeName, "SMALLMONEY", StringComparison.OrdinalIgnoreCase))
                    {
                        minimumValue = -214748.3648m;
                        maximumValue = 214748.3647m;
                    }
                    else if (string.Equals(dataTypeName, "MONEY", StringComparison.OrdinalIgnoreCase))
                    {
                        minimumValue = -922337203685477.5808m;
                        maximumValue = 922337203685477.5807m;
                    }

                    RegisterNumericGridDataColumnEditor(gridColumnIndex, columnName, numericEditor,
                                                        isNullable, defaultValue,
                                                        new NumericGridEditorOptions
                                                        {
                                                            NumericValueKind = numericValueKind,
                                                            Precision = numericPrecision,
                                                            Scale = numericScale,
                                                            AllowNegative = numericValueKind != GridNumericValueKind.Byte,
                                                            AllowExponent = numericValueKind == GridNumericValueKind.Single || numericValueKind == GridNumericValueKind.Double,
                                                            MinimumValue = minimumValue,
                                                            MaximumValue = maximumValue,
                                                            DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                            RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.Zero,
                                                            NullableEmptyBehavior = GridEditorNullableEmptyBehavior.Zero
                                                        });
                }
                else if (providerSpecificDataType == "Devart.Data.SqlServer.SqlTypes.SqlBinary" && dataTypeName.EndsWith("binary", StringComparison.Ordinal))
                {
                    RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.Binary,
                                                   isNullable, defaultValue);
                }
                else
                {
                    RegisterGridDataColumnMetadata(gridColumnIndex, columnName, GridColumnContentKind.None,
                                                   isNullable, defaultValue);
                }

                gridColumnIndex++;
            }
        }
    }
}
