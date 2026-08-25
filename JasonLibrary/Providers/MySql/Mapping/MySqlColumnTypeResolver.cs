using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Infrastructure.Enums;
using JasonLibrary.Providers.MySql.Enums;

public class MySqlColumnTypeResolver
{
    private const int TinyBlobLength = 255;
    private const int BlobLength = 65535;
    private const int MediumBlobLength = 16777215;
    private const int LongBlobLength = 2147483647;

    public static ColumnTypeResolver Resolve(string baseDataType, string dataType, int columnSize, int numericPrecision,
                                             int numericScale, int providerType, bool isEnum = false, bool isSet = false)
    {
        var normalizedBaseDataType = baseDataType?.Trim() ?? string.Empty;
        var resolvedBaseDataType = normalizedBaseDataType;
        var simpleDataType = "string";
        var schema = string.Empty;
        var usedProviderFallback = false;
        var specialDataTypeKind = SpecialDataTypeKind.String;
        var categoryDataTypeKind = CategoryDataTypeKind.String;
        var isUpdateValueSupported = false;
        var updateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType;

        var columnTypeEnum = ResolveByProviderType(providerType, columnSize, numericPrecision, numericScale, isEnum, isSet, out resolvedBaseDataType,
                                                   out schema, out simpleDataType, out specialDataTypeKind, out categoryDataTypeKind, out usedProviderFallback,
                                                   out isUpdateValueSupported, out updateValueSupportKind);

        return new ColumnTypeResolver
        {
            SimpleDataType = simpleDataType,
            BaseDataType = resolvedBaseDataType,
            FullDataType = $"{resolvedBaseDataType}{schema}",
            UsedProviderFallback = usedProviderFallback,
            SpecialDataTypeKind = specialDataTypeKind,
            CategoryDataTypeKind = categoryDataTypeKind,
            IsUpdateValueSupported = isUpdateValueSupported,
            UpdateValueSupportKind = updateValueSupportKind
        };
    }

    private static MySqlColumnTypeName ResolveByProviderType(int providerType, int columnSize, int numericPrecision, int numericScale, bool isEnum, bool isSet,
                                                             out string resolvedBaseDataType, out string schema, out string simpleDataType, out SpecialDataTypeKind specialDataTypeKind,
                                                             out CategoryDataTypeKind categoryDataTypeKind, out bool usedProviderFallback, out bool isUpdateValueSupported, out ColumnUpdateValueSupportKind updateValueSupportKind)
    {
        resolvedBaseDataType = string.Empty;
        schema = string.Empty;
        simpleDataType = "string";
        specialDataTypeKind = SpecialDataTypeKind.String;
        categoryDataTypeKind = CategoryDataTypeKind.String;
        isUpdateValueSupported = false;
        updateValueSupportKind = ColumnUpdateValueSupportKind.UnknownDataType;
        usedProviderFallback = false;

        switch (providerType)
        {
            case 16:
                {
                    resolvedBaseDataType = "tinyint";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.TinyInt;
                }
            case 12 when columnSize == 1 && numericPrecision == 0 && numericScale == 0:
                {
                    resolvedBaseDataType = "tinyint unsigned";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.TinyIntUnsigned;
                }
            case 12:
                {
                    resolvedBaseDataType = "smallint";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.SmallInt;
                }
            case 11 when columnSize == 9:
                {
                    resolvedBaseDataType = "mediumint";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.MediumInt;
                }
            case 11 when columnSize == 8 && numericPrecision == 0 && numericScale == 0:
                {
                    resolvedBaseDataType = "mediumint unsigned";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.MediumIntUnsigned;
                }
            case 11 when columnSize == 5 && numericPrecision == 0 && numericScale == 0:
                {
                    resolvedBaseDataType = "smallint unsigned";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.SmallIntUnsigned;
                }
            case 11:
                {
                    resolvedBaseDataType = "int";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Int;
                }
            case 1 when columnSize == 10 && numericPrecision == 0 && numericScale == 0:
                {
                    resolvedBaseDataType = "int unsigned";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.IntUnsigned;
                }
            case 1:
                {
                    resolvedBaseDataType = "bigint";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.BigInt;
                }
            case 10:
                {
                    // 依你舊註解：現階段無法可靠判斷是否 unsigned，先保守視為 float
                    resolvedBaseDataType = "float";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    usedProviderFallback = true;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Float;
                }
            case 9:
                {
                    //double unsigned 在目前 metadata 下不易區分，先保守視為 double
                    resolvedBaseDataType = "double";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    usedProviderFallback = true;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Double;
                }
            case 8 when columnSize == 20 && numericPrecision == 20 && numericScale == 0:
                {
                    resolvedBaseDataType = "bigint unsigned";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.BigIntUnsigned;
                }
            case 8:
                {
                    //decimal unsigned 在目前 metadata 下不易區分，先保守視為 decimal
                    resolvedBaseDataType = "decimal";
                    schema = BuildPrecisionScaleSchema(numericPrecision, numericScale);
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    usedProviderFallback = true;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Decimal;
                }
            case 6:
                {
                    resolvedBaseDataType = "date";
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.Date;
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Date;
                }
            case 7:
                {
                    resolvedBaseDataType = "datetime";
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.DateTime;
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.DateTime;
                }
            case 15:
                {
                    resolvedBaseDataType = "timestamp";
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.Timestamp;
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Timestamp;
                }
            case 14:
                {
                    resolvedBaseDataType = "time";
                    schema = BuildDateTimeScaleSchema(numericScale);
                    simpleDataType = "datetime";
                    specialDataTypeKind = SpecialDataTypeKind.Time;
                    categoryDataTypeKind = CategoryDataTypeKind.DateTime;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Time;
                }
            case 19:
                {
                    resolvedBaseDataType = "year";
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Number;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Year;
                }
            case 5 when isEnum:
                {
                    resolvedBaseDataType = "enum";
                    categoryDataTypeKind = CategoryDataTypeKind.String;
                    //isUpdateValueSupported = true;
                    //updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Enum;
                }
            case 5 when isSet:
                {
                    resolvedBaseDataType = "set";
                    categoryDataTypeKind = CategoryDataTypeKind.String;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    return MySqlColumnTypeName.Set;
                }
            case 5 when columnSize >= 0:
                {
                    resolvedBaseDataType = "char";
                    schema = BuildLengthSchemaAllowZero(columnSize);
                    categoryDataTypeKind = CategoryDataTypeKind.String;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Char;
                }
            case 18:
                {
                    resolvedBaseDataType = "varchar";
                    schema = BuildLengthSchema(columnSize);
                    categoryDataTypeKind = CategoryDataTypeKind.String;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.VarChar;
                }
            case 4 when columnSize == TinyBlobLength:
                {
                    resolvedBaseDataType = "tinyblob";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.TinyBlob;
                }
            case 4 when columnSize == BlobLength:
                {
                    resolvedBaseDataType = "blob";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.Blob;
                }
            case 4 when columnSize == MediumBlobLength:
                {
                    resolvedBaseDataType = "mediumblob";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.MediumBlob;
                }
            case 4:
                {
                    resolvedBaseDataType = "longblob";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.LongBlob;
                }
            case 13 when columnSize == TinyBlobLength:
                {
                    resolvedBaseDataType = "tinytext";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.TinyText;
                }
            case 13 when columnSize == BlobLength:
                {
                    resolvedBaseDataType = "text";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Text;
                }
            case 13 when columnSize == MediumBlobLength:
                {
                    resolvedBaseDataType = "mediumtext";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.MediumText;
                }
            case 13:
                {
                    resolvedBaseDataType = "longtext";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.LongText;
                }
            case 2:
                {
                    resolvedBaseDataType = "binary";
                    schema = BuildLengthSchema(columnSize);
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.Binary;
                }
            case 17:
                {
                    resolvedBaseDataType = "varbinary";
                    schema = BuildLengthSchema(columnSize);
                    categoryDataTypeKind = CategoryDataTypeKind.LargeBinary;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeBinaryType;
                    return MySqlColumnTypeName.VarBinary;
                }
            case 3:
                {
                    resolvedBaseDataType = "bit";
                    schema = BuildBitLengthSchema(columnSize);
                    simpleDataType = "number";
                    specialDataTypeKind = SpecialDataTypeKind.Bit;
                    categoryDataTypeKind = CategoryDataTypeKind.Number;
                    isUpdateValueSupported = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.Supported;
                    return MySqlColumnTypeName.Bit;
                }
            case 22:
                {
                    resolvedBaseDataType = "json";
                    categoryDataTypeKind = CategoryDataTypeKind.LargeText;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.LargeTextType;
                    return MySqlColumnTypeName.Json;
                }
            case 21:
                {
                    resolvedBaseDataType = "geometry";
                    categoryDataTypeKind = CategoryDataTypeKind.String;
                    usedProviderFallback = true;
                    updateValueSupportKind = ColumnUpdateValueSupportKind.IdentityOrGenerated;
                    return MySqlColumnTypeName.Geometry;
                }
            default:
                {
                    resolvedBaseDataType = string.IsNullOrWhiteSpace(resolvedBaseDataType) ? "unknown" : resolvedBaseDataType;
                    return MySqlColumnTypeName.Unknown;
                }
        }
    }

    private static string BuildLengthSchema(int columnSize)
    {
        if (columnSize > 0)
        {
            return $"({columnSize})";
        }

        return string.Empty;
    }

    private static string BuildLengthSchemaAllowZero(int columnSize)
    {
        if (columnSize >= 0)
        {
            return $"({columnSize})";
        }

        return string.Empty;
    }

    private static string BuildBitLengthSchema(int columnSize)
    {
        if (columnSize >= 1 && columnSize <= 64)
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
        //TIME, DATETIME, TIMESTAMP: fractional seconds precision = 0~6
        if (numericScale >= 1 && numericScale <= 6)
        {
            return $"({numericScale})";
        }

        return string.Empty;
    }
}
