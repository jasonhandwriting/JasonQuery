using JasonQuery.Core.Arrange.Formatting;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    public static class PostgreSqlFormatterProvider
    {
        public static IEnumerable<IColumnValueFormatter> Create()
        {
            return new IColumnValueFormatter[]
            {
                new PostgreSqlArrayFormatter(),
                new PostgreSqlBooleanArrayFormatter(),
                new PostgreSqlBoxArrayFormatter(),
                new PostgreSqlCharArrayFormatter(),
                new PostgreSqlCharacterArrayFormatter(),
                new PostgreSqlBitFormatter(),
                new PostgreSqlBooleanFormatter(),
                new PostgreSqlDateFormatter(),
                new PostgreSqlTimeWithTimeZoneFormatter(),
                new PostgreSqlTimeWithoutTimeZoneFormatter(),
                new PostgreSqlTimestampWithTimeZoneFormatter(),
                new PostgreSqlTimestampWithoutTimeZoneFormatter()
            };
        }
    }
}