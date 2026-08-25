using JasonLibrary.Core.Schema;

namespace JasonQuery.Core.Data.Formatters
{
    internal class LargeValueFormatter
    {
        public static string FormatValue(ColumnInfo columnInfo, int length)
        {
            var lengthText = DataSizeFormatter.FormatInt(length);

            return $"({columnInfo.BaseDataType})({lengthText})";
        }
    }
}
