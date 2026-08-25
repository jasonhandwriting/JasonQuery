using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlBooleanFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Boolean;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Boolean)
            {
                formatted = null;
                return false;
            }

            var value = rawValue?.ToString();

            if (value.StartsWith("T", StringComparison.OrdinalIgnoreCase))
            {
                value = "true";
            }
            else
            {
                value = "false";
            }

            formatted = value;
            return true;
        }
    }
}
