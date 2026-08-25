using JasonQuery.Core.Arrange.Formatting;
using System.Collections.Generic;

namespace JasonQuery.Core.Arrange.Oracle.Formatters
{
    public static class OracleFormatterProvider
    {
        public static IEnumerable<IColumnValueFormatter> Create()
        {
            return new IColumnValueFormatter[]
            {
                new OracleCharFormatter(),
                new OracleNumberFormatter(),
                new OracleDateFormatter(),
                new OracleIntervalDayToSecondFormatter(),
                new OracleIntervalYearToMonthFormatter(),
                new OracleTimestampFormatter(),
                new OracleTimestampWithLocalTimeZoneFormatter(),
                new OracleTimestampWithTimeZoneFormatter()
            };
        }
    }
}
