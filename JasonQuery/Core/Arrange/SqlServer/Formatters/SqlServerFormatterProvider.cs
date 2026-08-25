using JasonQuery.Core.Arrange.Formatting;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange.SqlServer.Formatters
{
    public static class SqlServerFormatterProvider
    {
        public static IEnumerable<IColumnValueFormatter> Create()
        {
            return new IColumnValueFormatter[]
            {
                new SqlServerBitFormatter(),
                new SqlServerDateFormatter(),
                new SqlServerDateTimeFormatter(), //DateTime, DateTime2, SmallDateTime
                new SqlServerDateTimeOffsetFormatter(),
                new SqlServerTimeFormatter(),
                new SqlServerRowVersionFormatter()
            };
        }
    }
}
