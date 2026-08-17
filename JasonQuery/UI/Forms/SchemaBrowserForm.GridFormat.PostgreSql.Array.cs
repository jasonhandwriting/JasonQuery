using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private static readonly Regex PostgreSqlDecimalArrayElementRegex = new Regex
        (
            @"^[+-]?(?:(?:\d+(?:\.\d*)?)|(?:\.\d+))(?:[eE][+-]?\d+)?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        /// <summary>
        /// 將 PostgreSQL 一維數值陣列整理成 {value1,value2} 格式。
        /// 只在所有元素都可安全驗證時才回傳 true；驗證失敗時不修改使用者原始內容。
        /// </summary>
        private static bool TryNormalizePostgreSqlNumericArray(string value, string arrayDataType, out string normalizedValue)
        {
            normalizedValue = value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmedValue = value.Trim();

            if (string.Equals(trimmedValue, "{}", StringComparison.Ordinal))
            {
                normalizedValue = "{}";
                return true;
            }

            if (trimmedValue.StartsWith("ARRAY", StringComparison.OrdinalIgnoreCase))
            {
                trimmedValue = trimmedValue.Substring(5).Trim();
            }

            if ((trimmedValue.StartsWith("{", StringComparison.Ordinal) && trimmedValue.EndsWith("}", StringComparison.Ordinal))
                || (trimmedValue.StartsWith("[", StringComparison.Ordinal) && trimmedValue.EndsWith("]", StringComparison.Ordinal)))
            {
                trimmedValue = trimmedValue.Substring(1, trimmedValue.Length - 2);
            }

            //本 helper 只處理一維數值陣列。遇到巢狀陣列時保留原始輸入，交由 PostgreSQL 驗證。
            if (trimmedValue.IndexOfAny(new[] { '{', '}', '[', ']' }) >= 0)
            {
                return false;
            }

            var elements = trimmedValue.Split(new[] { ',' }, StringSplitOptions.None);
            var normalizedElements = new List<string>(elements.Length);

            foreach (var element in elements)
            {
                var elementText = element.Trim();

                if (string.IsNullOrEmpty(elementText))
                {
                    return false;
                }

                if (string.Equals(elementText, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedElements.Add("NULL");
                    continue;
                }

                if (!TryNormalizePostgreSqlNumericArrayElement(elementText, arrayDataType, out var normalizedElement))
                {
                    return false;
                }

                normalizedElements.Add(normalizedElement);
            }

            normalizedValue = $"{{{string.Join(",", normalizedElements)}}}";
            return true;
        }

        private static bool TryNormalizePostgreSqlNumericArrayElement(string value, string arrayDataType, out string normalizedValue)
        {
            normalizedValue = value;

            switch (arrayDataType)
            {
                case "smallint[]":
                    {
                        if (!short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt16))
                        {
                            return false;
                        }

                        normalizedValue = parsedInt16.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                case "integer[]":
                    {
                        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt32))
                        {
                            return false;
                        }

                        normalizedValue = parsedInt32.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                case "bigint[]":
                    {
                        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt64))
                        {
                            return false;
                        }

                        normalizedValue = parsedInt64.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                case "real[]":
                case "double precision[]":
                    {
                        if (IsPostgreSqlSpecialFloatingPointValue(value))
                        {
                            normalizedValue = value;
                            return true;
                        }

                        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        {
                            return false;
                        }

                        //保留使用者輸入的小數位與科學記號，不以 double 重新格式化，避免不必要的精度變更。
                        normalizedValue = value;
                        return true;
                    }
                case "numeric[]":
                case "decimal[]":
                    {
                        if (string.Equals(value, "NaN", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(value, "Infinity", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(value, "+Infinity", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(value, "-Infinity", StringComparison.OrdinalIgnoreCase))
                        {
                            normalizedValue = value;
                            return true;
                        }

                        if (!PostgreSqlDecimalArrayElementRegex.IsMatch(value))
                        {
                            return false;
                        }

                        //numeric 的有效位數可能遠大於 System.Decimal，因此只做字串語法驗證，不轉成 CLR 數值型別。
                        normalizedValue = value;
                        return true;
                    }
                default:
                    return false;
            }
        }

        private static bool IsPostgreSqlSpecialFloatingPointValue(string value)
        {
            return string.Equals(value, "NaN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Infinity", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "+Infinity", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "-Infinity", StringComparison.OrdinalIgnoreCase);
        }
    }
}
