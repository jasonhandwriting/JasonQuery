using System;
using System.Globalization;

namespace JasonQuery.Core.Data.Formatters
{
    internal static class DataSizeFormatter
    {
        private static readonly CultureInfo _invariant = CultureInfo.InvariantCulture;

        /// <summary>
        /// 格式化整數型 Metadata (Length / Size / Precision / Scale)
        /// </summary>
        public static string FormatInt(long value) => value.ToString("#,##0", _invariant);

        public static string FormatBytes(long bytes)
        {
            const decimal unit = 1000m;

            if (bytes < 0)
            {
                bytes = 0;
            }

            decimal value = bytes;
            string suffix;

            if (value >= unit * unit * unit)
            {
                value /= unit * unit * unit;
                suffix = "GB";
            }
            else if (value >= unit * unit)
            {
                value /= unit * unit;
                suffix = "MB";
            }
            else if (value >= unit)
            {
                value /= unit;
                suffix = "kB";
            }
            else
            {
                suffix = "B";
            }

            value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            return value.ToString("0.##", CultureInfo.InvariantCulture) + suffix;
        }

        /// <summary>
        /// 可選：格式化 Nullable
        /// </summary>
        public static string FormatInt(long? value) => value.HasValue ? FormatInt(value.Value) : "0";

        /// <summary>
        /// Metadata 精度表示值
        /// </summary>
        /// <param name="numericPrecision"></param>
        /// <param name="numericScale"></param>
        /// <returns></returns>
        public static string FormatPrecisionScale(int numericPrecision, int numericScale)
        {
            var precision = $"{FormatInt(numericPrecision)}";
            var separation = ", ";
            var scale = $"{FormatInt(numericScale)}";

            if (scale == "0")
            {
                separation = string.Empty;
                scale = string.Empty;
            }

            return $"{precision}{separation}{scale}";
        }

        /// <summary>
        /// Metadata 小數 (例如 Average / Ratio)
        /// 預設最多 4 位小數，不補 0
        /// </summary>
        public static string FormatDecimal(decimal value, int maxDecimalPlaces = 4)
        {
            if (maxDecimalPlaces < 0)
            {
                maxDecimalPlaces = 0;
            }

            value = Math.Round(value, maxDecimalPlaces, MidpointRounding.AwayFromZero);

            string format = maxDecimalPlaces == 0 ? "#,##0" : "#,##0." + new string('#', maxDecimalPlaces);

            return value.ToString(format, _invariant);
        }

        public static string FormatDecimal(decimal? value, int maxDecimalPlaces = 4)
                             => value.HasValue
                             ? FormatDecimal(value.Value, maxDecimalPlaces)
                             : string.Empty;

        public static string FormatDecimal5(double value, int maxDecimalPlaces = 4)
        {
            string format = maxDecimalPlaces switch
            {
                0 => "#,##0",
                _ => "#,##0." + new string('#', maxDecimalPlaces)
            };

            return value.ToString(format, _invariant);
        }

        public static string FormatDecimal5(double? value, int maxDecimalPlaces = 4)
               => value.HasValue ? FormatDecimal5(value.Value, maxDecimalPlaces) : "0";
    }
}