using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.MySql.Formatters
{
    public sealed class MySqlBitFormatter : IColumnValueFormatter
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
                formatted = byteValue;
                return true;
            }

            if (rawValue is short shortValue)
            {
                formatted = shortValue;
                return true;
            }

            if (rawValue is int intValue)
            {
                formatted = intValue;
                return true;
            }

            if (rawValue is long longValue)
            {
                formatted = longValue;
                return true;
            }

            if (rawValue is ulong ulongValue)
            {
                formatted = ulongValue;
                return true;
            }

            var text = rawValue?.ToString();

            if (string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "1", StringComparison.OrdinalIgnoreCase))
            {
                formatted = 1;
                return true;
            }

            if (string.Equals(text, "FALSE", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "0", StringComparison.OrdinalIgnoreCase))
            {
                formatted = 0;
                return true;
            }

            formatted = text ?? string.Empty;
            return true;
        }
    }
}