using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.SqlServer.Formatters
{
    public sealed class SqlServerDateTimeOffsetFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.DateTimeOffset;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.DateTimeOffset)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTimeOffset dto)
            {
                var precision = Math.Max(0, Math.Min(columnInfo.NumericScale, 7));
                var baseFormat = "yyyy/MM/dd HH:mm:ss";
                var fractionPart = precision > 0 ? "." + new string('f', precision) : string.Empty;
                var format = $"{baseFormat}{fractionPart} zzz";

                formatted = dto.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
