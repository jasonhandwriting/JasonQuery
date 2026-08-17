using JasonQuery.Core.Database.Connection;
using System;
using System.Globalization;

namespace JasonQuery.Core.Database.DdlPreview.ColumnDefinitions
{
    internal static class ColumnDefaultValueFormatter
    {
        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd"
        };

        private static readonly string[] TimeFormats =
        {
            "HH:mm",
            "HH:mm:ss",
            "HH:mm:ss.FFFFFFF"
        };

        private static readonly string[] DateTimeFormats =
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF"
        };

        public static ColumnDefaultFormatResult Format(DataSourceType dataSourceType, ColumnTypeDefinition typeDefinition, ColumnDefinition column)
        {
            if (column == null || column.DefaultValueKind == ColumnDefaultValueKind.None)
            {
                return Success(string.Empty);
            }

            if (typeDefinition == null)
            {
                return Failure("A data type is required before a default value can be formatted.");
            }

            if (!typeDefinition.SupportsDefault)
            {
                return Failure("The selected data type does not support a default value in JasonQuery's basic Add Column editor.");
            }

            var value = column.DefaultValue ?? string.Empty;

            if (column.DefaultValueKind == ColumnDefaultValueKind.SqlExpression)
            {
                var expression = value.Trim();

                return DdlInputSafetyValidator.IsSafeSqlExpression(expression)
                       ? Success($" DEFAULT {expression}")
                       : Failure("The SQL expression is empty or contains an unsafe statement delimiter or comment.");
            }

            return FormatLiteral(dataSourceType, typeDefinition.DefaultLiteralKind, value);
        }

        private static ColumnDefaultFormatResult FormatLiteral(DataSourceType dataSourceType, ColumnDefaultLiteralKind literalKind, string value)
        {
            switch (literalKind)
            {
                case ColumnDefaultLiteralKind.String:
                case ColumnDefaultLiteralKind.Other:
                    {
                        return Success($" DEFAULT '{EscapeSqlString(value)}'");
                    }
                case ColumnDefaultLiteralKind.UnicodeString:
                    {
                        return Success($" DEFAULT N'{EscapeSqlString(value)}'");
                    }
                case ColumnDefaultLiteralKind.Integer:
                    {
                        return FormatInteger(value);
                    }
                case ColumnDefaultLiteralKind.Decimal:
                    {
                        return FormatDecimal(value);
                    }
                case ColumnDefaultLiteralKind.Boolean:
                    {
                        return FormatBoolean(dataSourceType, value);
                    }
                case ColumnDefaultLiteralKind.Date:
                    {
                        return FormatDate(dataSourceType, value);
                    }
                case ColumnDefaultLiteralKind.Time:
                    {
                        return FormatTime(dataSourceType, value);
                    }
                case ColumnDefaultLiteralKind.DateTime:
                    {
                        return FormatDateTime(dataSourceType, value);
                    }
                default:
                    {
                        return Failure("The selected data type uses an unsupported default literal.");
                    }
            }
        }

        private static ColumnDefaultFormatResult FormatInteger(string value)
        {
            if (!long.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed))
            {
                return Failure("The default value must be a valid integer.");
            }

            return Success($" DEFAULT {parsed.ToString(CultureInfo.InvariantCulture)}");
        }

        private static ColumnDefaultFormatResult FormatDecimal(string value)
        {
            var normalized = (value ?? string.Empty).Trim();

            if (!decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal parsed))
            {
                return Failure("The default value must be a valid decimal number using a period as the decimal separator.");
            }

            return Success($" DEFAULT {parsed.ToString(CultureInfo.InvariantCulture)}");
        }

        private static ColumnDefaultFormatResult FormatBoolean(DataSourceType dataSourceType, string value)
        {
            var normalized = (value ?? string.Empty).Trim();

            bool parsed;

            if (string.Equals(normalized, "1", StringComparison.Ordinal)
                || string.Equals(normalized, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
            }
            else if (string.Equals(normalized, "0", StringComparison.Ordinal)
                     || string.Equals(normalized, "false", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
            }
            else
            {
                return Failure("The default value must be true, false, 1, or 0.");
            }

            if (dataSourceType == DataSourceType.PostgreSql)
            {
                return Success(parsed ? " DEFAULT TRUE" : " DEFAULT FALSE");
            }

            return Success(parsed ? " DEFAULT 1" : " DEFAULT 0");
        }

        private static ColumnDefaultFormatResult FormatDate(DataSourceType dataSourceType, string value)
        {
            if (!DateTime.TryParseExact((value ?? string.Empty).Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return Failure("The default date must use the yyyy-MM-dd format.");
            }

            var text = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return Success($" DEFAULT DATE '{text}'");
                    }
                case DataSourceType.PostgreSql:
                    {
                        return Success($" DEFAULT DATE '{text}'");
                    }
                default:
                    {
                        return Success($" DEFAULT '{text}'");
                    }
            }
        }

        private static ColumnDefaultFormatResult FormatTime(DataSourceType dataSourceType, string value)
        {
            if (!DateTime.TryParseExact((value ?? string.Empty).Trim(), TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return Failure("The default time must use HH:mm, HH:mm:ss, or HH:mm:ss.fffffff.");
            }

            var text = parsed.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture)
                             .TrimEnd('0')
                             .TrimEnd('.');

            return dataSourceType == DataSourceType.PostgreSql ? Success($" DEFAULT TIME '{text}'") : Success($" DEFAULT '{text}'");
        }

        private static ColumnDefaultFormatResult FormatDateTime(DataSourceType dataSourceType, string value)
        {
            if (!DateTime.TryParseExact((value ?? string.Empty).Trim(), DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return Failure("The default date/time must use yyyy-MM-dd HH:mm:ss or an equivalent ISO format.");
            }

            var text = parsed.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return Success($" DEFAULT TIMESTAMP '{text}'");
                    }
                case DataSourceType.PostgreSql:
                    {
                        return Success($" DEFAULT TIMESTAMP '{text}'");
                    }
                default:
                    {
                        return Success($" DEFAULT '{text}'");
                    }
            }
        }

        private static string EscapeSqlString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static ColumnDefaultFormatResult Success(string sqlClause)
        {
            return new ColumnDefaultFormatResult
            {
                Succeeded = true,
                SqlClause = sqlClause ?? string.Empty,
                ErrorMessage = string.Empty
            };
        }

        private static ColumnDefaultFormatResult Failure(string errorMessage)
        {
            return new ColumnDefaultFormatResult
            {
                Succeeded = false,
                SqlClause = string.Empty,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}