using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;
using System.Collections;
using System.Text;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlBitFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.Bit;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.Bit)
            {
                formatted = null;
                return false;
            }

            if (rawValue is BitArray bitArray)
            {
                formatted = ConvertBitArrayToString(bitArray);
                return true;
            }

            formatted = rawValue?.ToString();
            return true;
        }

        private static string ConvertBitArrayToString(BitArray bitArray)
        {
            var sb = new StringBuilder(bitArray.Count);

            foreach (bool bit in bitArray)
            {
                sb.Append(bit ? '1' : '0');
            }

            return sb.ToString();
        }
    }
}
