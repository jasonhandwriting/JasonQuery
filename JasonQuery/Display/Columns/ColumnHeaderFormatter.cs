using System.Text;
using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;

namespace JasonQuery.Display.Columns
{
    public static class ColumnHeaderFormatter
    {
        public static string Format(ColumnInfo column, ColumnMode mode)
        {
            if (column == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.Append(column.ColumnName);

            if (mode.HasFlag(ColumnMode.ShowColumnType))
            {
                if (!string.IsNullOrWhiteSpace(column.FullDataType))
                {
                    sb.AppendLine();
                    sb.Append(column.FullDataType);
                }
            }

            if (mode.HasFlag(ColumnMode.ShowColumnComment))
            {
                if (!string.IsNullOrWhiteSpace(column.ColumnComment))
                {
                    sb.AppendLine();
                    sb.Append(column.ColumnComment);
                }
            }

            return sb.ToString();
        }
    }
}