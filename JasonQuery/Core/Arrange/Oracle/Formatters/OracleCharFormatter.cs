using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleCharFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Char;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Char)
            {
                formatted = null;
                return false;
            }

            var value = rawValue?.ToString() ?? string.Empty;
            var columnSize = Math.Max(0, columnInfo.ColumnSize);

            if (columnSize <= 0)
            {
                formatted = value;
                return true;
            }

            if (value.Length >= columnSize)
            {
                formatted = value;
                return true;
            }

            formatted = value.PadRight(columnSize, ' ');
            return true;
        }
    }
}
