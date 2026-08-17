using System;
using System.Data;
using System.Globalization;

namespace JasonQuery.Core.Data.DataRows
{
    internal static class DataRowSafeValueExtensions
    {
        private static readonly CultureInfo _invariant = CultureInfo.InvariantCulture;
        private static readonly double _decimalMaxAsDouble = (double)decimal.MaxValue;
        private static readonly double _decimalMinAsDouble = (double)decimal.MinValue;
        private static readonly float _decimalMaxAsFloat = (float)decimal.MaxValue;
        private static readonly float _decimalMinAsFloat = (float)decimal.MinValue;

        public static bool IsDbNull(this DataRow row, string columnName)
        {
            return row != null && row.Table != null && !string.IsNullOrWhiteSpace(columnName)
                   && row.Table.Columns.Contains(columnName) && row.IsNull(columnName);
        }

        public static bool IsDbNull(this DataRow row, int columnIndex)
        {
            return row != null && row.Table != null && columnIndex >= 0
                   && columnIndex < row.Table.Columns.Count && row.IsNull(columnIndex);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位的字串。
        /// </summary>
        public static string GetSafeString(this DataRow dr, string columnName, string defaultValue = "")
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return defaultValue;
            }

            return value?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的字串。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull 或 null，則回傳預設值。
        /// </summary>
        public static string GetSafeString(this DataRow dr, int columnIndex, string defaultValue = "")
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return defaultValue;
            }

            var value = dr[columnIndex];

            return value?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位名稱的 int 值。
        /// 若 DataRow 為 null、欄位不存在、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 int 範圍的數值，會自動限制在 int 的最大或最小值範圍內。
        /// </summary>
        public static int GetSafeInt(this DataRow dr, string columnName, int defaultValue = 0)
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return defaultValue;
            }

            return ConvertToInt(value, defaultValue);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的 int 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 int 範圍的數值，會自動限制在 int 的最大或最小值範圍內。
        /// </summary>
        public static int GetSafeInt(this DataRow dr, int columnIndex, int defaultValue = 0)
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return defaultValue;
            }

            var value = dr[columnIndex];

            return ConvertToInt(value, defaultValue);
        }

        private static int ConvertToInt(object value, int defaultValue)
        {
            return value switch
            {
                int i => i,
                short s => s,
                byte b => b,
                sbyte sb => sb,
                ushort us => us,
                uint ui => ui > int.MaxValue ? int.MaxValue : (int)ui,
                long l => l > int.MaxValue ? int.MaxValue : (l < int.MinValue ? int.MinValue : (int)l),
                ulong ul => ul > int.MaxValue ? int.MaxValue : (int)ul,
                decimal d => d > int.MaxValue ? int.MaxValue : (d < int.MinValue ? int.MinValue : (int)d),
                double d when double.IsNaN(d) || double.IsInfinity(d) => defaultValue,
                double d => d > int.MaxValue ? int.MaxValue : (d < int.MinValue ? int.MinValue : (int)d),
                float f when float.IsNaN(f) || float.IsInfinity(f) => defaultValue,
                float f => f > int.MaxValue ? int.MaxValue : (f < int.MinValue ? int.MinValue : (int)f),
                string s when int.TryParse(s, NumberStyles.Any, _invariant, out var r) => r,
                _ => defaultValue
            };
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位名稱的 bool 值。
        /// 若 DataRow 為 null、欄位不存在、值為 DBNull、無法轉換，則回傳預設值。
        /// </summary>
        public static bool GetSafeBool(this DataRow dr, string columnName, bool defaultValue = false)
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return defaultValue;
            }

            return ConvertToBool(value, defaultValue);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的 bool 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳預設值。
        /// </summary>
        public static bool GetSafeBool(this DataRow dr, int columnIndex, bool defaultValue = false)
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return defaultValue;
            }

            var value = dr[columnIndex];

            return ConvertToBool(value, defaultValue);
        }

        private static bool ConvertToBool(object value, bool defaultValue)
        {
            return value switch
            {
                bool b => b,
                int i => i != 0,
                short s => s != 0,
                byte b => b != 0,
                string s when bool.TryParse(s, out var r) => r,
                string s when string.Equals(s, "1", StringComparison.Ordinal) => true,
                string s when string.Equals(s, "0", StringComparison.Ordinal) => false,
                string s when string.Equals(s, "Y", StringComparison.OrdinalIgnoreCase) => true,
                string s when string.Equals(s, "YES", StringComparison.OrdinalIgnoreCase) => true,
                string s when string.Equals(s, "T", StringComparison.OrdinalIgnoreCase) => true,
                string s when string.Equals(s, "N", StringComparison.OrdinalIgnoreCase) => false,
                string s when string.Equals(s, "NO", StringComparison.OrdinalIgnoreCase) => false,
                string s when string.Equals(s, "F", StringComparison.OrdinalIgnoreCase) => false,
                _ => defaultValue
            };
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位的 long 值。
        /// 若 DataRow 為 null、欄位不存在、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 long 範圍的數值，會自動限制在 long 的最大或最小值範圍內。
        /// </summary>
        public static long GetSafeLong(this DataRow dr, string columnName, long defaultValue = 0)
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return defaultValue;
            }

            return ConvertToLong(value, defaultValue);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的 long 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 long 範圍的數值，會自動限制在 long 的最大或最小值範圍內。
        /// </summary>
        public static long GetSafeLong(this DataRow dr, int columnIndex, long defaultValue = 0)
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return defaultValue;
            }

            var value = dr[columnIndex];

            return ConvertToLong(value, defaultValue);
        }

        private static long ConvertToLong(object value, long defaultValue)
        {
            return value switch
            {
                long l => l,
                int i => i,
                short s => s,
                byte b => b,
                sbyte sb => sb,
                ushort us => us,
                uint ui => ui,
                ulong ul => ul > long.MaxValue ? long.MaxValue : (long)ul,
                decimal d => d > long.MaxValue ? long.MaxValue : (d < long.MinValue ? long.MinValue : (long)d),
                double d when double.IsNaN(d) || double.IsInfinity(d) => defaultValue,
                double d => d > long.MaxValue ? long.MaxValue : (d < long.MinValue ? long.MinValue : (long)d),
                float f when float.IsNaN(f) || float.IsInfinity(f) => defaultValue,
                float f => f > long.MaxValue ? long.MaxValue : (f < long.MinValue ? long.MinValue : (long)f),
                string s when long.TryParse(s, NumberStyles.Any, _invariant, out var r) => r,
                _ => defaultValue
            };
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位的 decimal 值。
        /// 若 DataRow 為 null、欄位不存在、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 decimal 範圍的數值，會自動限制在 decimal 的最大或最小值範圍內。
        /// </summary>
        public static decimal GetSafeDecimal(this DataRow dr, string columnName, decimal defaultValue = 0m)
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return defaultValue;
            }

            return ConvertToDecimal(value, defaultValue);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的 decimal 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳預設值。
        /// 對於超出 decimal 範圍的數值，會自動限制在 decimal 的最大或最小值範圍內。
        /// </summary>
        public static decimal GetSafeDecimal(this DataRow dr, int columnIndex, decimal defaultValue = 0m)
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return defaultValue;
            }

            var value = dr[columnIndex];

            return ConvertToDecimal(value, defaultValue);
        }

        private static decimal ConvertToDecimal(object value, decimal defaultValue)
        {
            return value switch
            {
                decimal d => d,
                int i => i,
                long l => l,
                short s => s,
                byte b => b,
                sbyte sb => sb,
                ushort us => us,
                uint ui => ui,
                ulong ul => ul,
                double d when double.IsNaN(d) || double.IsInfinity(d) => defaultValue,
                double d => d > _decimalMaxAsDouble ? decimal.MaxValue : (d < _decimalMinAsDouble ? decimal.MinValue : (decimal)d),
                float f when float.IsNaN(f) || float.IsInfinity(f) => defaultValue,
                float f => f > _decimalMaxAsFloat ? decimal.MaxValue : (f < _decimalMinAsFloat ? decimal.MinValue : (decimal)f),
                string s when decimal.TryParse(s, NumberStyles.Any, _invariant, out var r) => r,
                _ => defaultValue
            };
        }

        private static bool TryGetValue(DataRow dr, string columnName, out object value)
        {
            value = null;

            if (dr == null || dr.Table == null || string.IsNullOrWhiteSpace(columnName) || !dr.Table.Columns.Contains(columnName) || dr.IsNull(columnName))
            {
                return false;
            }

            value = dr[columnName];
            return value != null && value != DBNull.Value;
        }

        /// <summary>
        /// 安全取得指定欄位索引的 DateTime 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳預設值。
        /// </summary>
        public static DateTime GetSafeDateTime(this DataRow dr, int columnIndex, DateTime defaultValue)
        {
            var dtValue = dr.GetSafeNullableDateTime(columnIndex);

            return dtValue.HasValue ? dtValue.Value : defaultValue;
        }

        public static string GetSafeDateTimeText(this DataRow dr, string columnName, string format, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return defaultValue;
            }

            var dtValue = dr.GetSafeNullableDateTime(columnName);

            if (!dtValue.HasValue)
            {
                return defaultValue;
            }

            return dtValue.Value.ToString(format, _invariant);
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位名稱的 Nullable DateTime 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳 null。
        /// </summary>
        public static DateTime? GetSafeNullableDateTime(this DataRow dr, string columnName)
        {
            if (!TryGetValue(dr, columnName, out var value))
            {
                return null;
            }

            if (value is DateTime dtValue)
            {
                return dtValue;
            }

            return DateTime.TryParse(value.ToString(), _invariant, DateTimeStyles.None, out var result) ? (DateTime?)result : null;
        }

        /// <summary>
        /// DataRow 擴充方法：安全取得指定欄位索引的 Nullable DateTime 值。
        /// 若 DataRow 為 null、欄位索引超出範圍、值為 DBNull、無法轉換，則回傳 null。
        /// </summary>
        public static DateTime? GetSafeNullableDateTime(this DataRow dr, int columnIndex)
        {
            if (dr == null || dr.Table == null || columnIndex < 0 || columnIndex >= dr.Table.Columns.Count || dr.IsNull(columnIndex))
            {
                return null;
            }

            var value = dr[columnIndex];

            if (value is DateTime dtValue)
            {
                return dtValue;
            }

            return DateTime.TryParse(value.ToString(), _invariant, DateTimeStyles.None, out var result) ? (DateTime?)result : null;
        }
    }
}