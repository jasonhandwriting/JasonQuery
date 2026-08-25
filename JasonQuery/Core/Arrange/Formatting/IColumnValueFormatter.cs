using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;

namespace JasonQuery.Core.Arrange.Formatting
{
    public interface IColumnValueFormatter
    {
        SpecialDataTypeKind SupportedKind { get; }

        bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted);
    }
}
