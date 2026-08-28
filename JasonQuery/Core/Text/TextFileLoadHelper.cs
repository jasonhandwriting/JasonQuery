using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Text
{
    internal sealed class TextFileLoadResult
    {
        public bool IsSuccess { get; set; }
        public bool IsBinaryFile { get; set; }
        public string BinaryFileType { get; set; } = string.Empty;
        public string EncodingName { get; set; } = string.Empty;
        public string EndOfLineStyle { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    internal static class TextFileLoadHelper
    {
        public static TextFileLoadResult Load(string fileName, bool assumeUtf8WindowsLineEnding)
        {
            var result = new TextFileLoadResult();

            if (assumeUtf8WindowsLineEnding)
            {
                result.EncodingName = "UTF-8 BOM";
                result.EndOfLineStyle = "Windows (CR LF)";
            }
            else
            {
                var endOfLineStyle = string.Empty;
                var encodingName = TextEncodingDetector.GetTextEncode(fileName, ref endOfLineStyle);

                result.EncodingName = encodingName;
                result.EndOfLineStyle = endOfLineStyle;
            }

            if (string.Equals(result.EncodingName, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            if (result.EncodingName.StartsWith("Binary", StringComparison.Ordinal))
            {
                result.IsBinaryFile = true;
                result.BinaryFileType = ExtractBinaryFileType(result.EncodingName);
                return result;
            }

            var encoding = ResolveEncoding(result.EncodingName);

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, encoding, true))
            {
                var contents = reader.ReadToEnd();

                contents = NormalizeToWindowsLineEndings(contents);
                contents = NormalizeTabs(contents);

                result.Text = contents;
                result.IsSuccess = true;
            }

            return result;
        }

        private static Encoding ResolveEncoding(string encodingName)
        {
            switch (encodingName)
            {
                case "UTF-8 BOM":
                case "UTF-8":
                case "ASCII":
                    {
                        return Encoding.UTF8;
                    }
                case "UTF-16":
                case "UTF-16 LE":
                    {
                        return Encoding.Unicode;
                    }
                case "UTF-16 BE":
                    {
                        return Encoding.BigEndianUnicode;
                    }
                case "Big5 (Traditional)":
                    {
                        return Encoding.GetEncoding(950);
                    }
                case "GB2312 (Simplified)":
                    {
                        return Encoding.GetEncoding(936);
                    }
                case "ANSI":
                default:
                    {
                        return Encoding.Default;
                    }
            }
        }

        private static string ExtractBinaryFileType(string encodingName)
        {
            const string prefix = "Binary`";

            if (!encodingName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return encodingName.Substring(prefix.Length);
        }

        public static string NormalizeToWindowsLineEndings(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("\r\n", "\n")
                       .Replace("\r", "\n")
                       .Replace("\n", "\r\n");
        }

        private static string NormalizeTabs(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\t", "    ");
        }
    }
}
