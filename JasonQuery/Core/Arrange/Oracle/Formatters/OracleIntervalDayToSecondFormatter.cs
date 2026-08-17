using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleIntervalDayToSecondFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.IntervalDayToSecond;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.IntervalDayToSecond)
            {
                formatted = null;
                return false;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}