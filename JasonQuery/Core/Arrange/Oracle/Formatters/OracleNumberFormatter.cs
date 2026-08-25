using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using JasonQuery.Core.Text;
using System;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public sealed class OracleNumberFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Number;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Number)
            {
                formatted = null;
                return false;
            }

            var rawValueString = rawValue?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(rawValueString))
            {
                formatted = string.Empty;
                return true;
            }

            var dataType = columnInfo.BaseDataType;

            //20260315 Binary_Float, Binary_Double 這兩個型別，Devart 已處理過了，此處不再處理
            if (string.Equals(dataType, "BINARY_DOUBLE", StringComparison.OrdinalIgnoreCase) || string.Equals(dataType, "BINARY_FLOAT", StringComparison.OrdinalIgnoreCase))
            {
                formatted = rawValueString;
                return true;
            }

            if (columnInfo.NumericScale >= 0)
            {
                rawValueString = TextHelper.TruncateDecimal(rawValueString, columnInfo.NumericScale);
            }

            if (rawValueString.IndexOf('.') >= 0)
            {
                rawValueString = rawValueString.TrimEnd('0').TrimEnd('.');
            }

            formatted = rawValueString;
            return true;
        }
    }
}
