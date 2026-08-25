using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.MySql.Formatters
{
    public sealed class MySqlTimeFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Time;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Time)
            {
                formatted = null;
                return false;
            }

            if (rawValue is TimeSpan ts)
            {
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 6));
                var baseFormat = @"hh\:mm\:ss";
                var format = precision > 0 ? $"{baseFormat}\\.{new string('f', precision)}" : baseFormat;

                formatted = ts.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
