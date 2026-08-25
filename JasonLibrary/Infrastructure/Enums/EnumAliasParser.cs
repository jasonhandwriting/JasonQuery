using System;
using System.Collections.Generic;
using System.Reflection;

namespace JasonLibrary.Infrastructure.Enums
{
    public static class EnumAliasParser<TEnum> where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> _map;

        static EnumAliasParser()
        {
            _map = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

            var type = typeof(TEnum);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var enumValue = (TEnum)field.GetValue(null);

                //1. Enum 名稱本身
                _map[field.Name] = enumValue;

                //2️. Alias
                var aliasAttributes = field.GetCustomAttributes<EnumAliasAttribute>();

                foreach (var attr in aliasAttributes)
                {
                    if (!_map.ContainsKey(attr.Alias))
                    {
                        _map[attr.Alias] = enumValue;
                    }
                }
            }
        }

        public static bool TryParse(string value, out TEnum result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            return _map.TryGetValue(value.Trim(), out result);
        }

        public static TEnum ParseOrDefault(string value, TEnum defaultValue = default)
        {
            return TryParse(value, out var result) ? result : defaultValue;
        }
    }
}
