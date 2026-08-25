using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;
using System.Globalization;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleDateFormatter : IColumnValueFormatter
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
                var format = string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;

                formatted = dt.ToString(format, CultureInfo.InvariantCulture);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
