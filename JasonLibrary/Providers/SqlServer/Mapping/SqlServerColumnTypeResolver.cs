using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Infrastructure.Enums;
using JasonLibrary.Providers.SqlServer.Enums;

public class SqlServerColumnTypeResolver
{
    private const int SqlServerMaxLength = 2147483647;

    public static ColumnTypeResolver Resolve(string baseDataType, string dataType, int columnSize,
                                             int numericPrecision, int numericScale, string providerSpecificDataType)
    {
        if (string.IsNullOrWhiteSpace(baseDataType))
        {
            return new ColumnTypeResolver
            {
                BaseDataType = baseDataType ?? string.Empty,
                FullDataType = baseDataType ?? string.Empty,
                SimpleDataType = "string",
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
        var columnTypeEnum = EnumAliasParser<SqlServerColumnTypeName>.ParseOrDefault(baseDataType);

        switch (columnTypeEnum)
        {
            case SqlServerColumnTypeName.Char: //20260709 實測，SSMS 查詢得到的結果，並不會補空白，不需特別處理！
            case SqlServerColumnTypeName.VarChar:
                {
                    schema = BuildLengthSchema(columnSize);
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.NChar: //20260709 實測，SSMS 查詢得到的結果，並不會補空白，不需特別處理！
            case SqlServerColumnTypeName.NVarchar:
                {
                    schema = BuildLengthSchema(columnSize);
                    isUpdateValueSupported = true;
                    specialDataTypeKind = SpecialDataTypeKind.NString;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.Binary:
            case SqlServerColumnTypeName.VarBinary:
                {
                    schema = BuildLengthSchema(columnSize);
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    break;
                }

            case SqlServerColumnTypeName.Bit:
                {
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Bit;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.BigInt:
            case SqlServerColumnTypeName.Int:
            case SqlServerColumnTypeName.Integer:
            case SqlServerColumnTypeName.SmallInt:
            case SqlServerColumnTypeName.TinyInt:
            case SqlServerColumnTypeName.Float:
            case SqlServerColumnTypeName.Real:
            case SqlServerColumnTypeName.Money:
            case SqlServerColumnTypeName.SmallMoney:
                {
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.Decimal:
            case SqlServerColumnTypeName.Numeric:
                {
                    schema = BuildPrecisionScaleSchema(numericPrecision, numericScale);
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.Date:
                {
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.Date; //YYYY-MM-DD
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.Time:
                {
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.Time; //HH:mm:ss[.nnnnnnn]
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.SmallDateTime: //沒有毫秒
            case SqlServerColumnTypeName.DateTime: //有毫秒，格式為YYYY-MM-DD HH:mm:ss[.nnn]
                {
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.DateTime;
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.DateTime2:
                {
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.DateTime; //有毫秒，格式為YYYY-MM-DD HH:mm:ss[.nnnnnnn]
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.DateTimeOffset:
                {
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.DateTimeOffset; //有時區，格式為YYYY-MM-DD HH:mm:ss[.nnnnnnn][+-]hh:mm
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    break;
                }
            case SqlServerColumnTypeName.Timestamp: //Timestamp 是 RowVersion 的同義字，與日期時間無關 (8字節的二進制欄位型態，通常用於行版本控制，與日期時間無關)
            case SqlServerColumnTypeName.RowVersion:
                {
                    specialDataTypeKind = SpecialDataTypeKind.RowVersion;
                    categoryDataTypeKind = CategoryDataTypeKind.String; //內容短，可控，直接當作字串處理
                    updateValueSupportKind = ColumnUpdateValueSupportKind.RowVersionOrTimestamp;
                    break;
                }
            case SqlServerColumnTypeName.Image:
                {
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.BinaryType;
                    break;
                }
            case SqlServerColumnTypeName.NText:
                {
                    specialDataTypeKind = SpecialDataTypeKind.NString;
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                    break;
                }
            case SqlServerColumnTypeName.Text:
            case SqlServerColumnTypeName.Xml:
            case SqlServerColumnTypeName.Json:
            case SqlServerColumnTypeName.Vector:
                {
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                    break;
                }
            case SqlServerColumnTypeName.UniqueIdentifier:
            case SqlServerColumnTypeName.SqlVariant:
            case SqlServerColumnTypeName.Geography:
            case SqlServerColumnTypeName.Geometry:
            case SqlServerColumnTypeName.HierarchyId:
                {
                    //暫時先視為文字
                    updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    break;
                }
            default:
                {
                    //20260315 Devart 所回傳的 Schema 中，無法直接從 DataTypeName 獲取到型別
                    if (baseDataType.EndsWith(".GEOGRAPHY", System.StringComparison.OrdinalIgnoreCase))
                    {
                        baseDataType = "geography";
                        updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    }
                    else if (baseDataType.EndsWith(".GEOMETRY", System.StringComparison.OrdinalIgnoreCase))
                    {
                        baseDataType = "geometry";
                        updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    }
                    else if (baseDataType.EndsWith(".HIERARCHYID", System.StringComparison.OrdinalIgnoreCase))
                    {
                        baseDataType = "hierarchyid";
                        updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    }

                    //其他暫時先視為文字
                    break;
                }
        }

        return new ColumnTypeResolver
        {
            BaseDataType = baseDataType,
            FullDataType = $"{baseDataType}{schema}",
            SimpleDataType = simpleDataType,
            SpecialDataTypeKind = specialDataTypeKind,
            CategoryDataTypeKind = categoryDataTypeKind,
            IsUpdateValueSupported = isUpdateValueSupported,
            UpdateValueSupportKind = updateValueSupportKind
        };
    }

    private static string BuildLengthSchema(int columnSize)
    {
        if (columnSize == SqlServerMaxLength || columnSize == -1)
        {
            return "(max)";
        }

        if (columnSize > 0)
        {
            return $"({columnSize})";
        }

        return string.Empty;
    }

    private static string BuildPrecisionScaleSchema(int numericPrecision, int numericScale)
    {
        if (numericPrecision <= 0)
        {
            return string.Empty;
        }

        if (numericScale < 0)
        {
            numericScale = 0;
        }

        return $"({numericPrecision},{numericScale})";
    }

    private static string BuildDateTimeScaleSchema(int numericScale)
    {
        //Time, DateTime2, DateTimeOffset 的 fractional seconds precision 為 0~7
        if (numericScale >= 0 && numericScale <= 7)
        {
            return $"({numericScale})";
        }

        return string.Empty;
    }
}