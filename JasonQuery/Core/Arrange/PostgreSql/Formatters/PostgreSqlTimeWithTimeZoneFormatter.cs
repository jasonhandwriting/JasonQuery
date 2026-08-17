using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlTimeWithTimeZoneFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.TimeWithTimeZone;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.TimeWithTimeZone)
            {
                formatted = null;
                return false;
            }

            if (rawValue is TimeSpan ts)
            {
                // Devart currently maps PostgreSQL 'time with time zone' to System.TimeSpan,
                // which loses the original timezone offset information.
                // Therefore JasonQuery can only format the time portion here,
                // and cannot reconstruct the original +HH:mm / -HH:mm offset.
                int precision = PostgreSqlDateTimeFormatHelper.NormalizePrecision(columnInfo.NumericPrecision);

                formatted = PostgreSqlDateTimeFormatHelper.BuildTimeText(ts, precision);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}