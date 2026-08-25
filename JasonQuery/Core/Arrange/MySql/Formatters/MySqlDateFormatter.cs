using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.MySql.Formatters
{
    public sealed class MySqlDateFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Date;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Date)
            {
                formatted = null;
                return false;
            }

            if (rawValue is DateTime dt)
            {
                var format = string.IsNullOrWhiteSpace(context?.DateFormat) ? "yyyy/MM/dd" : context.DateFormat;

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
