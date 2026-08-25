using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlTimestampWithTimeZoneFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.TimestampWithTimeZone;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.TimestampWithTimeZone)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTime dt)
            {
                int precision = PostgreSqlDateTimeFormatHelper.NormalizePrecision(columnInfo.NumericPrecision);
                string baseFormat = PostgreSqlDateTimeFormatHelper.GetSafeDateTimeFormat(context);
                string format = precision > 0 ? $"{baseFormat}.{new string('f', precision)}zz" : $"{baseFormat}zz";

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
