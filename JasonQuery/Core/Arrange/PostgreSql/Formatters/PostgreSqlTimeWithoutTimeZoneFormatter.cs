using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlTimeWithoutTimeZoneFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.TimeWithoutTimeZone;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.TimeWithoutTimeZone)
            {
                formatted = null;
                return false;
            }

            if (rawValue is TimeSpan ts)
            {
                int precision = PostgreSqlDateTimeFormatHelper.NormalizePrecision(columnInfo.NumericPrecision);

                formatted = PostgreSqlDateTimeFormatHelper.BuildTimeText(ts, precision);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
