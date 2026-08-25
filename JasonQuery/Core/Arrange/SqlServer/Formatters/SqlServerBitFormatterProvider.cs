using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.SqlServer.Formatters
{
    public sealed class SqlServerBitFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Bit;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Bit)
            {
                formatted = null;
                return false;
            }

            if (rawValue is bool boolValue)
            {
                formatted = boolValue ? 1 : 0;
                return true;
            }

            if (rawValue is byte byteValue)
            {
                formatted = byteValue != 0 ? 1 : 0;
                return true;
            }

            if (rawValue is short shortValue)
            {
                formatted = shortValue != 0 ? 1 : 0;
                return true;
            }

            if (rawValue is int intValue)
            {
                formatted = intValue != 0 ? 1 : 0;
                return true;
            }

            var text = rawValue?.ToString();

            if (string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase))
            {
                formatted = 1;
                return true;
            }

            if (string.Equals(text, "FALSE", StringComparison.OrdinalIgnoreCase))
            {
                formatted = 0;
                return true;
            }

            formatted = text ?? string.Empty;
            return true;
        }
    }
}
