using System.Text;

namespace JasonQuery.Core.QueryEngine.Formatters
{
    public static class BinaryPreviewFormatter
    {
        /// <summary>
        /// Convert a byte array to a readable Hex format string (excluding the 0x prefix).
        /// </summary>
        public static string ToReadableHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }

            return ToHexString(data);
        }

        /// <summary>
        /// Convert to Hex string，ex. 0AFF237C
        /// </summary>
        private static string ToHexString(byte[] data)
        {
            //每 byte 兩個 hex 字元 + 一個空白
            var sb = new StringBuilder(data.Length * 3);

            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));

                //if (i < data.Length - 1)
                //{
                //    sb.Append(' ');
                //}
            }

            return sb.ToString();
        }
    }
}