using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Database.DmlPreview
{
    internal static class DmlPreviewSqlLiteralFormatter
    {
        public static string Format(DmlPreviewLiteralFormatRequest request)
        {
            if (request == null || request.ColumnInfo == null)
            {
                return string.Empty;
            }

            var cellValue = request.CellValue ?? string.Empty;
            var defaultValue = request.DefaultValue ?? string.Empty;
            var nullKeyword = request.UseUpperCaseKeywords ? "NULL" : "null";
            var defaultKeyword = request.UseUpperCaseKeywords ? "DEFAULT" : "default";

            if (IsNullIndicator(cellValue, request.NullValueIndicators))
            {
                return nullKeyword;
            }

            if (request.AllowDefaultKeyword && !string.IsNullOrEmpty(defaultValue) && IsDefaultIndicator(cellValue, defaultValue, defaultKeyword))
            {
                return defaultKeyword;
            }

            if (string.IsNullOrEmpty(cellValue))
            {
                if (request.AllowDefaultKeyword && !string.IsNullOrEmpty(defaultValue))
                {
                    return defaultKeyword;
                }

                return FormatEmptyValue(request.ColumnInfo.CategoryDataTypeKind, request.ColumnInfo.IsNullable, nullKeyword);
            }

            switch (request.DataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return FormatOracleValue(cellValue, request.ColumnInfo.CategoryDataTypeKind);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return FormatPostgreSqlValue(cellValue, request.ColumnInfo.CategoryDataTypeKind);
                    }
                case DataSourceType.SqlServer:
                    {
                        return FormatSqlServerValue(cellValue, request.ColumnInfo.CategoryDataTypeKind, request.ColumnInfo.SpecialDataTypeKind,
                                                    request.ColumnInfo.BaseDataType);
                    }
                case DataSourceType.MySql:
                    {
                        return FormatMySqlValue(cellValue, request.ColumnInfo.CategoryDataTypeKind, request.ColumnInfo.BaseDataType);
                    }
                default:
                    {
                        return cellValue;
                    }
            }
        }

        public static string EscapeSqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static bool IsNullIndicator(string value, IEnumerable<string> nullValueIndicators)
        {
            if (string.Equals(value, "NULL", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (nullValueIndicators == null)
            {
                return false;
            }

            foreach (var indicator in nullValueIndicators)
            {
                if (string.Equals(indicator, value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefaultIndicator(string value, string defaultValue, string defaultKeyword)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(defaultValue))
            {
                return false;
            }

            if (string.Equals(value, defaultKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(value, defaultValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var unquotedDefaultValue = defaultValue.Trim().Trim('\'');

            return string.Equals(value, unquotedDefaultValue, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatEmptyValue(CategoryDataTypeKind categoryDataTypeKind, bool isNullable, string nullKeyword)
        {
            switch (categoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                    {
                        return "''";
                    }
                case CategoryDataTypeKind.Number:
                    {
                        return isNullable ? nullKeyword : "0";
                    }
                case CategoryDataTypeKind.DateTime:
                    {
                        return isNullable ? nullKeyword : "''";
                    }
                default:
                    {
                        return isNullable ? nullKeyword : "''";
                    }
            }
        }

        private static string FormatOracleValue(string cellValue, CategoryDataTypeKind categoryDataTypeKind)
        {
            switch (categoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                    {
                        return $"'{EscapeSqlString(cellValue)}'";
                    }
                case CategoryDataTypeKind.DateTime:
                    {
                        return $"TIMESTAMP '{FormatOracleDateTimeValue(cellValue)}'";
                    }
                default:
                    {
                        return cellValue;
                    }
            }
        }

        private static string FormatPostgreSqlValue(string cellValue, CategoryDataTypeKind categoryDataTypeKind)
        {
            switch (categoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                case CategoryDataTypeKind.DateTime:
                    {
                        return $"'{EscapeSqlString(cellValue)}'";
                    }
                default:
                    {
                        return cellValue;
                    }
            }
        }

        private static string FormatSqlServerValue(string cellValue, CategoryDataTypeKind categoryDataTypeKind,
                                                   SpecialDataTypeKind specialDataTypeKind, string baseDataType)
        {
            baseDataType = baseDataType ?? string.Empty;

            if (string.Equals(baseDataType, "BIT", StringComparison.OrdinalIgnoreCase))
            {
                return FormatBitValue(cellValue);
            }

            if (string.Equals(baseDataType, "UNIQUEIDENTIFIER", StringComparison.OrdinalIgnoreCase))
            {
                return $"'{EscapeSqlString(cellValue)}'";
            }

            if (specialDataTypeKind == SpecialDataTypeKind.NString)
            {
                return $"N'{EscapeSqlString(cellValue)}'";
            }

            switch (categoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                    {
                        return $"'{EscapeSqlString(cellValue)}'";
                    }
                case CategoryDataTypeKind.DateTime:
                    {
                        var formattedValue = FormatSqlServerDateTimeValue(cellValue, baseDataType);

                        return $"'{EscapeSqlString(formattedValue)}'";
                    }
                default:
                    {
                        return cellValue;
                    }
            }
        }

        private static string FormatMySqlValue(string cellValue, CategoryDataTypeKind categoryDataTypeKind, string baseDataType)
        {
            baseDataType = baseDataType ?? string.Empty;

            if (string.Equals(baseDataType, "BIT", StringComparison.OrdinalIgnoreCase))
            {
                return FormatBitValue(cellValue);
            }

            switch (categoryDataTypeKind)
            {
                case CategoryDataTypeKind.String:
                    {
                        return $"'{EscapeSqlString(cellValue)}'";
                    }
                case CategoryDataTypeKind.DateTime:
                    {
                        var formattedValue = FormatMySqlDateTimeValue(cellValue, baseDataType);

                        return $"'{EscapeSqlString(formattedValue)}'";
                    }
                default:
                    {
                        return cellValue;
                    }
            }
        }

        private static string FormatBitValue(string cellValue)
        {
            if (string.Equals(cellValue, "TRUE", StringComparison.OrdinalIgnoreCase))
            {
                return "1";
            }

            if (string.Equals(cellValue, "FALSE", StringComparison.OrdinalIgnoreCase))
            {
                return "0";
            }

            return cellValue;
        }

        private static string FormatOracleDateTimeValue(string dateTimeValue)
        {
            var dateValue = GetSafeSubstring(dateTimeValue, 0, 10);
            var timeValue = GetSafeSubstring(dateTimeValue, 11, null);

            if (DateTime.TryParse(dateValue, out var parsedDate))
            {
                return $"{parsedDate:yyyy-MM-dd}{timeValue}";
            }

            return $"{dateValue}{timeValue}";
        }

        private static string FormatSqlServerDateTimeValue(string cellValue, string baseDataType)
        {
            if (string.Equals(baseDataType, "TIME", StringComparison.OrdinalIgnoreCase))
            {
                return cellValue;
            }

            if (!DateTime.TryParse(cellValue, out var dateTime))
            {
                return cellValue;
            }

            if (string.Equals(baseDataType, "DATE", StringComparison.OrdinalIgnoreCase))
            {
                return dateTime.ToString("yyyy-MM-dd");
            }

            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff").TrimEnd('0').TrimEnd('.');
        }

        private static string FormatMySqlDateTimeValue(string cellValue, string baseDataType)
        {
            if (string.Equals(baseDataType, "TIME", StringComparison.OrdinalIgnoreCase))
            {
                return cellValue;
            }

            if (!DateTime.TryParse(cellValue, out var dateTime))
            {
                return cellValue;
            }

            if (string.Equals(baseDataType, "DATE", StringComparison.OrdinalIgnoreCase))
            {
                return dateTime.ToString("yyyy-MM-dd");
            }

            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff").TrimEnd('0').TrimEnd('.');
        }

        private static string GetSafeSubstring(string value, int startIndex, int? length)
        {
            if (string.IsNullOrEmpty(value) || startIndex < 0 || startIndex >= value.Length)
            {
                return string.Empty;
            }

            if (!length.HasValue)
            {
                return value.Substring(startIndex);
            }

            if (length.Value <= 0)
            {
                return string.Empty;
            }

            var safeLength = Math.Min(length.Value, value.Length - startIndex);

            return value.Substring(startIndex, safeLength);
        }
    }
}
