using JasonQuery.Core.Arrange.Formatting;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange.MySql.Formatters
{
    public static class MySqlFormatterProvider
    {
        public static IEnumerable<IColumnValueFormatter> Create()
        {
            return new IColumnValueFormatter[]
            {
                new MySqlBitFormatter(),
                new MySqlDateFormatter(),
                new MySqlDateTimeFormatter(),
                new MySqlTimeFormatter(),
                new MySqlTimestampFormatter()
            };
        }
    }
}