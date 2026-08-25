using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleTimestampWithLocalTimeZoneFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.TimestampWithLocalTimeZone;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.TimestampWithLocalTimeZone)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTime dt)
            {
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 7));
                var baseFormat = string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;
                var format = precision > 0 ? $"{baseFormat}.{new string('f', precision)}" : baseFormat;

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
