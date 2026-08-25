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
        private static GridNumericValueKind GetOracleNumericValueKind(string typeName, int numericPrecision, int numericScale)
        {
            if (string.Equals(typeName, "BINARY_FLOAT", StringComparison.OrdinalIgnoreCase))
            {
                return GridNumericValueKind.Single;
            }

            if (string.Equals(typeName, "BINARY_DOUBLE", StringComparison.OrdinalIgnoreCase))
            {
                return GridNumericValueKind.Double;
            }

            if (numericScale == 0 && numericPrecision > 0)
            {
                if (numericPrecision <= 9)
                {
                    return GridNumericValueKind.Int32;
                }

                if (numericPrecision <= 18)
                {
                    return GridNumericValueKind.Int64;
                }
            }

            if (numericPrecision > 0 && numericPrecision <= 28 && numericScale >= -28)
            {
                return GridNumericValueKind.Decimal;
            }

            return GridNumericValueKind.ArbitraryPrecisionDecimal;
        }

        private void ConfigureOracleGridDataEditors()
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

                //可由 ColumnInfoCollector 取得的共通欄位資訊，優先使用 Collector；
                //ProviderSpecificDataType、ProviderType 等 Devart 專屬資訊仍保留從 RawSchema 取得。
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

                var dataType = rawSchemaRow.GetSafeString("DataType");
                var typeName = rawSchemaRow.GetSafeString("TypeName");
                var providerSpecificDataType = rawSchemaRow.GetSafeString("ProviderSpecificDataType");
                var providerType = rawSchemaRow.GetSafeString("ProviderType");
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

                //針對日期欄位特別處理
                if (dataType == "System.DateTime") //Oracle
                {
                    if (providerSpecificDataType == "Devart.Data.Oracle.OracleTimeStamp" && numericScale >= 0)
                    {
                        /* ProviderType
                         * 25: TIMESTAMP
                         * 26: TIMESTAMP WITH LOCAL TIME ZONE
                         * 27: TIMESTAMP WITH TIME ZONE
                         */

                        var timeZoneFormat = providerType == "27" ? " zzz" : string.Empty;
                        var fractionSeparator = numericScale > 0 ? "." : string.Empty;
                        var fractionDigitsFormat = new string('f', Math.Min(7, numericScale));
                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss{fractionSeparator}{fractionDigitsFormat}{timeZoneFormat}";

                        //C1.Win.C1Input.C1DateEdit 毫秒最多只支援 7位數 (受限於 .Net DateTime 型態)
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
                                                                 ValueKind = providerType == "27" ? GridDateTimeValueKind.DateTimeOffset
                                                                                             : GridDateTimeValueKind.DateTime,
                                                                 Format = customFormat,
                                                                 DefaultValueBehavior = GridEditorDefaultValueBehavior.StoreEmptyForSqlDefault,
                                                                 RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.CurrentDateTime,
                                                                 NullableEmptyBehavior = GridEditorNullableEmptyBehavior.GridNullText
                                                             });

                        c1GridData.Columns[gridColumnIndex].NumberFormat = customFormat;
                    }
                    else
                    {
                        var customFormat = $"{MyLibrary.DateFormat} HH:mm:ss"; //Oracle 的 DATE 欄位會包含日期及時分秒

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
                    }
                }
                else if (dataType == "System.String")
                {
                    var textEditor = new TextBox //此處如果使用 C1TextBox，沒有看到長度的限制效果！
                    {
                        MaxLength = columnSize //設定字串最大的可輸入長度
                    };

                    RegisterTextGridDataColumnEditor(gridColumnIndex, columnName, textEditor,
                                                     isNullable, defaultValue,
                                                     new TextGridEditorOptions
                                                     {
                                                         MaxLength = columnSize,
                                                         LengthMode = GridTextLengthMode.CharacterCount,
                                                         UseCellEditorResult = true,
                                                         DefaultValueBehavior = GridEditorDefaultValueBehavior.Ignore,
                                                         RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.KeepEmpty,
                                                         NullableEmptyBehavior = GridEditorNullableEmptyBehavior.KeepEmpty
                                                     });
                }
                else if (providerSpecificDataType == "Devart.Data.Oracle.OracleNumber") //20240719 改用 ProviderSpecificDataType 判斷，因為大部份是 System.Decimal 但少數是 System.Double，不確定是否還有其他型態
                {
                    //20240622 數值型態，使用 C1NumericEdit 當作編輯器，因為 C1NumericEdit 有「全選」的屬性
                    var numericEditor = new C1NumericEdit
                    {
                        //NumericInput = true, //這個屬性在這裡沒有效果，改用 KeyPress 事件替代
                        //NumericInputKeys = NumericInputKeyFlags.F9 | NumericInputKeyFlags.Minus | NumericInputKeyFlags.Plus | NumericInputKeyFlags.X;
                        TextAlign = HorizontalAlignment.Right,
                        InitialSelection = InitialSelectionEnum.SelectAll,
                        //Font = new Font("微軟正黑體", 9F, FontStyle.Regular, GraphicsUnit.Point, 136)
                        VisibleButtons = DropDownControlButtonFlags.DropDown
                    };

                    var numericValueKind = GetOracleNumericValueKind(typeName, numericPrecision, numericScale);

                    RegisterNumericGridDataColumnEditor(gridColumnIndex, columnName, numericEditor,
                                                        isNullable, defaultValue,
                                                        new NumericGridEditorOptions
                                                        {
                                                            NumericValueKind = numericValueKind,
                                                            Precision = numericPrecision,
                                                            Scale = numericScale,
                                                            AllowNegative = true,
                                                            AllowExponent = numericValueKind == GridNumericValueKind.Single || numericValueKind == GridNumericValueKind.Double,
                                                            AllowNonFinite = numericValueKind == GridNumericValueKind.Single || numericValueKind == GridNumericValueKind.Double,
                                                            DefaultValueBehavior = GridEditorDefaultValueBehavior.ApplyDefaultText,
                                                            RequiredEmptyBehavior = GridEditorRequiredEmptyBehavior.Zero,
                                                            NullableEmptyBehavior = GridEditorNullableEmptyBehavior.Zero
                                                        });
                }
                else if (providerSpecificDataType == "Devart.Data.Oracle.OracleLob")
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
