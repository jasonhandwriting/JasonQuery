using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.MySql.Formatters
{
    public sealed class MySqlDateTimeFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.DateTime;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.DateTime)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTime dt)
            {
                var baseFormat = string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 6));
                var format = precision > 0 ? $"{baseFormat}.{new string('f', precision)}" : baseFormat;

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}