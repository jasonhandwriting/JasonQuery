using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.SqlServer.Formatters
{
    public sealed class SqlServerRowVersionFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.RowVersion;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.RowVersion)
            {
                formatted = null;
                return false;
            }

            if (rawValue is byte[] bytes)
            {
                formatted = "0x" + BitConverter.ToString(bytes).Replace("-", string.Empty);
                return true;
            }

            formatted = rawValue?.ToString() ?? string.Empty;
            return true;
        }
    }
}
