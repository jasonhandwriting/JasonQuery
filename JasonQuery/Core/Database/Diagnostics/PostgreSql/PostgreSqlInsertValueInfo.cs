using System.Globalization;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlInsertValueInfo
    {
        public int ValuesRowIndex { get; set; } = 1;

        public int ColumnIndex { get; set; }

        public string RawText { get; set; } = string.Empty;

        public bool IsStringLiteral { get; set; }

        public string StringValue { get; set; } = string.Empty;

        public int ActualLength
        {
            get
            {
                if (!IsStringLiteral || string.IsNullOrEmpty(StringValue))
                {
                    return 0;
                }

                return new StringInfo(StringValue).LengthInTextElements;
            }
        }
    }
}