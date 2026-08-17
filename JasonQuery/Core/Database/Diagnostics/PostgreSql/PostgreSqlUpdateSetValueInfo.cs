using System.Globalization;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public sealed class PostgreSqlUpdateSetValueInfo
    {
        public string ColumnName { get; set; } = string.Empty;

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