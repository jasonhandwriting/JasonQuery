using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange.Formatting
{
    public sealed class ColumnValueFormatterRegistry
    {
        private readonly Dictionary<SpecialDataTypeKind, IColumnValueFormatter> _formatterMap;

        public ColumnValueFormatterRegistry(IEnumerable<IColumnValueFormatter> formatters)
        {
            _formatterMap = new Dictionary<SpecialDataTypeKind, IColumnValueFormatter>();

            if (formatters == null)
            {
                return;
            }

            foreach (var formatter in formatters)
            {
                if (formatter == null)
                {
                    continue;
                }

                var kind = formatter.SupportedKind;

                if (_formatterMap.TryGetValue(kind, out var existingFormatter))
                {
                    throw new InvalidOperationException($"Duplicate formatter registration detected for SpecialDataTypeKind '{kind}'. " +
                                                        $"Existing: {existingFormatter.GetType().FullName}, " +
                                                        $"New: {formatter.GetType().FullName}");
                }

                _formatterMap.Add(kind, formatter);
            }
        }

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            formatted = null;

            if (columnInfo == null)
            {
                return false;
            }

            if (_formatterMap.TryGetValue(columnInfo.SpecialDataTypeKind, out var formatter))
            {
                return formatter.TryFormat(rawValue, columnInfo, context, out formatted);
            }

            return false;
        }
    }
}
