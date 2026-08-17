using System.Collections.Generic;

namespace JasonQuery.Core.Data.Dictionaries
{
    internal static class DictionarySafeValueExtensions
    {
        /// <summary>
        /// 安全取得 Dictionary 指定 Key 的值。
        /// Dictionary 為 null、Key 不存在時，回傳 defaultValue。
        /// 支援以下幾種用法：(後兩種用法放在 ..\其他\Dictionaries GetValueOrDefault.txt
        /// Dictionary<(string TableName, string ColumnName), string>
        /// Dictionary<(string SchemaName, string TableName, string ColumnName), string>
        /// Dictionary<string, int>
        /// Dictionary<int, DataRow>
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                             TKey key, TValue defaultValue = default)
        {
            if (dictionary == null)
            {
                return defaultValue;
            }

            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}