using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange.Formatting;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public sealed class PostgreSqlBoxArrayFormatter : IColumnValueFormatter
    {
        public SpecialDataTypeKind SupportedKind => SpecialDataTypeKind.BoxArray;

        public bool TryFormat(object rawValue, ColumnInfo columnInfo, ArrangeContext context, out object formatted)
        {
            if (columnInfo == null || columnInfo.SpecialDataTypeKind != SpecialDataTypeKind.BoxArray)
            {
                formatted = null;
                return false;
            }

            var arrayText = rawValue?.ToString() ?? string.Empty;

            arrayText = arrayText.Trim('{', '}'); //20260314 Displays the same format as pgAdmin
            formatted = arrayText.Replace("\"", string.Empty).Replace(";", ","); //20260313 PostgreSQL's box array is represented as a string like "(x1,y1),(x2,y2)", remove the double quotes, the display will be the same as pgAdmin.
            return true;
        }
    }
}