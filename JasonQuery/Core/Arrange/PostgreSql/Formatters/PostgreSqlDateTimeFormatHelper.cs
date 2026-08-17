using System;

namespace JasonQuery.Core.Arrange.PostgreSql.Formatters
{
    internal static class PostgreSqlDateTimeFormatHelper
    {
        private const long TicksPerSecond = 10_000_000L;

        public static int NormalizePrecision(int precision)
        {
            return Math.Max(0, Math.Min(precision, 7));
        }

        public static string GetFractionPart(long ticks, int precision)
        {
            precision = NormalizePrecision(precision);

            if (precision <= 0)
            {
                return string.Empty;
            }

            long ticksInSecond = Math.Abs(ticks % TicksPerSecond);
            long divisor = (long)Math.Pow(10, 7 - precision);
            long fracValue = ticksInSecond / divisor;

            return "." + fracValue.ToString().PadLeft(precision, '0');
        }

        public static string GetSafeDateFormat(ArrangeContext context)
        {
            return string.IsNullOrWhiteSpace(context?.DateFormat) ? "yyyy/MM/dd" : context.DateFormat;
        }

        public static string GetSafeDateTimeFormat(ArrangeContext context)
        {
            return string.IsNullOrWhiteSpace(context?.DateTimeFormat) ? "yyyy/MM/dd HH:mm:ss" : context.DateTimeFormat;
        }

        public static string BuildTimeText(TimeSpan ts, int precision)
        {
            int hour = (int)(ts.TotalHours % 24);

            if (hour < 0)
            {
                hour += 24;
            }

            int minute = ts.Minutes;
            int second = ts.Seconds;

            string timeBase = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, second);
            string fractionPart = GetFractionPart(ts.Ticks, precision);

            return string.Concat(timeBase, fractionPart);
        }
    }
}