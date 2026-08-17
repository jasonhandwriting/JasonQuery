using JasonLibrary.Core.Schema;
using JasonQuery.Core.Data.Formatters;
using JasonQuery.Core.QueryEngine.Types;
using System;

namespace JasonQuery.Core.Arrange.Builders
{
    internal static class LargeBinaryValueBuilder
    {
        internal static LargeBinaryDataType Build(object rawValue, ColumnInfo columnInfo, ArrangeContext context)
        {
            var fullBytes = rawValue as byte[] ?? Array.Empty<byte>();
            var fullLength = fullBytes.Length;

            return new LargeBinaryDataType
            (
                LargeValueFormatter.FormatValue(columnInfo, fullLength),
                string.Empty,
                fullLength,
                false,
                () => fullBytes
            );
        }
    }
}