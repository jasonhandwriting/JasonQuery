using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Infrastructure.Enums;
using JasonLibrary.Providers.Oracle.Enums;
using System;

namespace JasonLibrary.Providers.Oracle.Mapping
{
    public class OracleColumnTypeResolver
    {
        public static ColumnTypeResolver Resolve(string baseDataType, string dataType, int columnSize,
                                                 int numericPrecision, int numericScale, string providerSpecificDataType)
        {
            if (string.IsNullOrWhiteSpace(baseDataType))
            {
                return new ColumnTypeResolver
                {
                    BaseDataType = baseDataType ?? string.Empty,
                    FullDataType = baseDataType ?? string.Empty,
                    SpecialDataTypeKind = SpecialDataTypeKind.String,
                    CategoryDataTypeKind = CategoryDataTypeKind.String,
                    IsUpdateValueSupported = false,
                    UpdateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType
                };
            }

            var simpleDataType = "string";
            var schema = string.Empty;
            var specialDataTypeKind = SpecialDataTypeKind.String;
            var categoryDataTypeKind = CategoryDataTypeKind.String;
            var isUpdateValueSupported = false;
            var updateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType;
            var columnTypeEnum = EnumAliasParser<OracleColumnTypeName>.ParseOrDefault(baseDataType);

            switch (columnTypeEnum)
            {
                case OracleColumnTypeName.Char:
                    {
                        schema = $"({columnSize})";
                        isUpdateValueSupported = true;
                        specialDataTypeKind = SpecialDataTypeKind.Char;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Varchar2:
                    {
                        schema = $"({columnSize})";
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.NChar: //20260709 實測，NChar(30) 可儲存剛好 30個中文字！
                    {
                        schema = $"({columnSize})";
                        isUpdateValueSupported = true;
                        specialDataTypeKind = SpecialDataTypeKind.Char; //20260709 實測，NChar 與 Char 的差異僅在於字元集，對於程式邏輯而言，仍可視為 Char 類型
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.NVarchar2:
                    {
                        schema = $"({columnSize})";
                        isUpdateValueSupported = true;
                        specialDataTypeKind = SpecialDataTypeKind.NString;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Boolean:
                    {
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.BinaryDouble:
                case OracleColumnTypeName.BinaryFloat:
                    {
                        simpleDataType = "number";
                        specialDataTypeKind = SpecialDataTypeKind.Number;
                        categoryDataTypeKind = CategoryDataTypeKind.Number;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Float:
                    {
                        simpleDataType = "number";
                        schema = $"({numericPrecision})";
                        specialDataTypeKind = SpecialDataTypeKind.Number;
                        categoryDataTypeKind = CategoryDataTypeKind.Number;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Number:
                case OracleColumnTypeName.Real:
                    {
                        simpleDataType = "number";

                        if (string.Equals(dataType, "SYSTEM.DECIMAL", StringComparison.OrdinalIgnoreCase) && numericPrecision == 38 && numericScale == 0)
                        {
                            baseDataType = "INTEGER";
                        }
                        else if (numericScale == -127 && providerSpecificDataType == "Devart.Data.Oracle.OracleNumber")
                        {
                            //有小數點
                            if (numericPrecision > 0)
                            {
                                schema = $"({numericPrecision})";
                            }
                        }
                        else if (numericScale == 0)
                        {
                            //沒有小數點
                            if (numericPrecision > 0)
                            {
                                schema = $"({numericPrecision})";
                            }
                        }
                        else
                        {
                            //有小數點
                            schema = $"({numericPrecision},{numericScale})";
                        }

                        specialDataTypeKind = SpecialDataTypeKind.Number;
                        categoryDataTypeKind = CategoryDataTypeKind.Number;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Integer:
                    {
                        simpleDataType = "number";
                        specialDataTypeKind = SpecialDataTypeKind.Number;
                        categoryDataTypeKind = CategoryDataTypeKind.Number;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.NClob:
                    {
                        specialDataTypeKind = SpecialDataTypeKind.NString;
                        categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                        break;
                    }
                case OracleColumnTypeName.Clob:
                case OracleColumnTypeName.Long: //舊版 Oracle 用來存大型文字 (已淘汰，應改用 CLOB)
                case OracleColumnTypeName.Json: //Oracle 23ai 原生型別，實際上是 CLOB
                    {
                        categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                        break;
                    }
                case OracleColumnTypeName.Raw: //小型二進位資料，最大 2000 bytes
                case OracleColumnTypeName.LongRaw: //舊版 Oracle 的大型 Binary (已淘汰，應改用 BLOB)
                case OracleColumnTypeName.Blob: //大型二進位資料
                    {
                        categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                        break;
                    }
                case OracleColumnTypeName.Date: //年, 月, 日, 時, 分, 秒
                    {
                        simpleDataType = "datetime";
                        specialDataTypeKind = SpecialDataTypeKind.DateTime;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.Timestamp: //年, 月, 日, 時, 分, 秒 + 毫秒，預設 6位 (最高 9位)
                    {
                        simpleDataType = "datetime";
                        schema = $"({numericScale})";
                        specialDataTypeKind = SpecialDataTypeKind.Timestamp;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.TimestampWithLocalTimeZone: //年, 月, 日, 時, 分, 秒 + 毫秒(預設 6位) + 本地時區偏移量
                    {
                        simpleDataType = "datetime";
                        schema = $"({numericScale})";
                        specialDataTypeKind = SpecialDataTypeKind.TimestampWithLocalTimeZone;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.TimestampWithTimeZone: //年, 月, 日, 時, 分, 秒 + 毫秒(預設 6位) + 時區偏移量
                    {
                        simpleDataType = "datetime";
                        schema = $"({numericScale})";
                        specialDataTypeKind = SpecialDataTypeKind.TimestampWithTimeZone;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        isUpdateValueSupported = true;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                        break;
                    }
                case OracleColumnTypeName.IntervalDayToSecond: //天、時、分、秒、毫秒的距離
                    {
                        baseDataType = baseDataType.Replace("DAY", $"DAY({numericPrecision})");
                        baseDataType = $"{baseDataType}({numericScale})";
                        specialDataTypeKind = SpecialDataTypeKind.DateTime;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.IntervalType;
                        break;
                    }
                case OracleColumnTypeName.IntervalYearToMonth: //年份與月份的距離
                    {
                        baseDataType = baseDataType.Replace("YEAR", $"YEAR({numericPrecision})");
                        specialDataTypeKind = SpecialDataTypeKind.DateTime;
                        categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.IntervalType;
                        break;
                    }
                case OracleColumnTypeName.XmlType:
                    {
                        categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                        updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                        break;
                    }
                case OracleColumnTypeName.AnsiString:
                case OracleColumnTypeName.Anydata:
                case OracleColumnTypeName.MlsLabel:
                case OracleColumnTypeName.RowID:
                case OracleColumnTypeName.Sdo_Geometry:
                case OracleColumnTypeName.UriType:
                case OracleColumnTypeName.URowID:
                    {
                        updateValueSupportKind = ColumnUpdateValueSupportKind.DatabaseSpecificUnsupported;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            var fullDataType = $"{baseDataType}{schema}";

            return new ColumnTypeResolver
            {
                BaseDataType = baseDataType,
                FullDataType = fullDataType,
                SimpleDataType = simpleDataType,
                SpecialDataTypeKind = specialDataTypeKind,
                CategoryDataTypeKind = categoryDataTypeKind,
                IsUpdateValueSupported = isUpdateValueSupported,
                UpdateValueSupportKind = updateValueSupportKind
            };
        }
    }
}
