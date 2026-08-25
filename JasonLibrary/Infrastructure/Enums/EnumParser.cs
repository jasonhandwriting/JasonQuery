using System;
using System.Collections.Generic;

namespace JasonLibrary.Infrastructure.Enums
{
    public static class EnumParser<TEnum> where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> _map;

        static EnumParser()
        {
            _map = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                _map[value.ToString()] = (TEnum)value;
            }
        }

        public static bool TryParse(string value, out TEnum result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            return _map.TryGetValue(value, out result);
        }

        public static TEnum ParseOrDefault(string value, TEnum defaultValue = default)
        {
            return TryParse(value, out var result) ? result : defaultValue;
        }
    }
}
