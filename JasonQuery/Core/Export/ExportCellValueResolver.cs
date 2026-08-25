using JasonQuery.Core.QueryEngine.Types;
using System;

namespace JasonQuery.Core.Export
{
    internal static class ExportCellValueResolver
    {
        public static ExportCellValueResolution Resolve(object cellValue, string cellText)
        {
            if (cellValue == null || cellValue is DBNull)
            {
                return CreateNull();
            }

            if (cellValue is LargeTextDataType largeText)
            {
                if (largeText.IsNull)
                {
                    return CreateNull();
                }

                var content = largeText.LoadContent();

                return CreateValue(content, content);
            }

            if (cellValue is LargeBinaryDataType largeBinary)
            {
                if (largeBinary.IsNull)
                {
                    return CreateNull();
                }

                var alias = largeBinary.DisplayText ?? string.Empty;

                return CreateValue(alias, alias);
            }

            if (cellValue is byte[] bytes)
            {
                var hex = BitConverter.ToString(bytes).Replace("-", string.Empty);

                return CreateValue(hex, hex);
            }

            var text = cellText ?? Convert.ToString(cellValue) ?? string.Empty;

            return CreateValue(cellValue, text);
        }

        private static ExportCellValueResolution CreateNull()
        {
            return new ExportCellValueResolution
            {
                IsNull = true,
                Value = null,
                Text = string.Empty
            };
        }

        private static ExportCellValueResolution CreateValue(object value, string text)
        {
            return new ExportCellValueResolution
            {
                IsNull = false,
                Value = value,
                Text = text ?? string.Empty
            };
        }
    }
}
