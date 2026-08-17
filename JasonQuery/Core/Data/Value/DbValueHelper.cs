using System;

namespace JasonQuery.Core.Data.Value
{
    internal static class DbValueHelper
    {
        /// <summary>
        /// 檢查物件是否為 null 或 DBNull。
        /// </summary>
        public static bool IsDbNull(object value) => value == null || value == DBNull.Value;
    }
}