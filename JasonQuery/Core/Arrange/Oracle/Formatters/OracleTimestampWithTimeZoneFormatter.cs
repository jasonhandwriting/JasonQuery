using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleTimestampWithTimeZoneFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.TimestampWithTimeZone;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.TimestampWithTimeZone)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTimeOffset dto)
            {
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 7));
                var baseFormat = string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;
                var format = precision > 0 ? $"{baseFormat}.{new string('f', precision)} zzz" : $"{baseFormat} zzz";

                formatted = dto.ToString(format);
                return true;
            }

            if (rawValue is DateTime dt) //Devart 回傳 DateTime 型別，原始 Time Zone Offset 已無法還原
            {
                //Devart currently maps Oracle TIMESTAMP WITH TIME ZONE to DateTime,
                //which does not preserve the original timezone offset reliably.
                //Therefore JasonQuery does not append zzz here when only DateTime is available.
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 7));
                var baseFormat = string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;
                var format = precision > 0 ? $"{baseFormat}.{new string('f', precision)}" : $"{baseFormat}";

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}