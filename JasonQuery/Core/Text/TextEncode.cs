using JasonQuery.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.Core.Text
{
    internal class CodePageHeuristicReport
    {
        //統計解讀為ASCII、符號、常用字、次常用字及亂碼(非有效字元)字數
        public int Ascii, Symbol, Common, Rare, Unknown;

        /// <summary>
        /// 亂碼指標(數值愈大，不是該編碼的機率愈高)
        /// </summary>
        public float SuspicionScore
        {
            get
            {
                var total = Ascii + Symbol + Common + Rare + Unknown;

                if (total == 0)
                {
                    return 0;
                }

                return (float)(Rare + Unknown * 3) / total;
            }
        }
    }

    internal class TextEncodingDetector
    {
        private static readonly byte[] _utf16BeBom =
        {
            0xFE,
            0xFF
        };

        private static readonly byte[] _utf16LeBom =
        {
            0xFF,
            0xFE
        };

        private static readonly byte[] _utf8Bom =
        {
            0xEF,
            0xBB,
            0xBF
        };

        private static bool _nullSuggestsBinary = true;
        private static double _utf16ExpectedNullPercent = 70;
        private static double _utf16UnexpectedNullPercent = 10;

        private enum DetectedTextEncoding
        {
            None, //Unknown or binary
            Ansi, //0-255
            Ascii, //0-127
            Utf8Bom, //UTF8 with BOM
            Utf8NoBom, //UTF8 without BOM
            Utf16LeBom, //UTF16 LE with BOM
            Utf16LeNoBom, //UTF16 LE without BOM
            Utf16BeBom, //UTF16-BE with BOM
            Utf16BeNoBom, //UTF16-BE without BOM
            Big5,
            Gb2312
        }

        private static string DetectLineEndingStyle(string pathToFile) //檢查檔尾的換行符號
        {
            const int BUFFER_SIZE = 4096;
            var buffer = new char[BUFFER_SIZE];
            int readCount;

            try
            {
                using var stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                readCount = reader.ReadBlock(buffer, 0, buffer.Length);
            }
            catch (Exception ex) //指定的檔案被鎖定，開啟失敗
            {
                var message = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";

                MessageBox.Show(message, @"JasonQuery - DetermineEndOfLineStyle", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return "ERROR";
            }

            int countCrlf = 0;
            int countCr = 0;
            int countLf = 0;

            for (var i = 0; i < readCount; i++)
            {
                if (buffer[i] == '\r')
                {
                    if (i + 1 < readCount && buffer[i + 1] == '\n')
                    {
                        countCrlf++;
                        i++; //Skip the '\n'
                    }
                    else
                    {
                        countCr++;
                    }
                }
                else if (buffer[i] == '\n')
                {
                    countLf++;
                }
            }

            //20250529 新增：如果沒有任何的換行符號(只有單列文字)，也算是 "Windows (CR LF)"
            if ((countCrlf > countCr && countCrlf > countLf) || (countCr == 0 && countCrlf == 0 && countLf == 0))
            {
                return "Windows (CR LF)";
            }

            if (countLf > countCrlf && countLf > countCr)
            {
                return "Unix (LF)";
            }

            if (countCr > countCrlf && countCr > countLf)
            {
                return "MacOs (CR)";
            }

            return "Unknown";
        }

        private static DetectedTextEncoding CheckBom(IReadOnlyList<byte> buffer, int size)
        {
            if (size >= 2 && buffer[0] == _utf16LeBom[0] && buffer[1] == _utf16LeBom[1])
            {
                return DetectedTextEncoding.Utf16LeBom;
            }

            if (size >= 2 && buffer[0] == _utf16BeBom[0] && buffer[1] == _utf16BeBom[1])
            {
                return DetectedTextEncoding.Utf16BeBom;
            }

            if (size >= 3 && buffer[0] == _utf8Bom[0] && buffer[1] == _utf8Bom[1] && buffer[2] == _utf8Bom[2])
            {
                return DetectedTextEncoding.Utf8Bom;
            }

            return DetectedTextEncoding.None;
        }

        public static int CompareBig5AndGb2312Likelihood(byte[] data)
        {
            var resBig5 = AnalyzeBig5(data);
            var resGB = AnalyzeGB2312(data);

            if (resBig5.SuspicionScore < resGB.SuspicionScore)
            {
                return 1;
            }

            if (resBig5.SuspicionScore > resGB.SuspicionScore)
            {
                return -1;
            }

            return 0;
        }

        private static CodePageHeuristicReport AnalyzeBig5(IEnumerable<byte> data)
        {
            var res = new CodePageHeuristicReport();
            var isDblBytes = false;
            byte dblByteHi = 0;

            foreach (var b in data)
            {
                if (isDblBytes)
                {
                    if (b >= 0x40 && b <= 0x7e || b >= 0xa1 && b <= 0xfe)
                    {
                        var c = dblByteHi * 0x100 + b;

                        if (c >= 0xa140 && c <= 0xa3bf)
                        {
                            res.Symbol++; //符號
                        }
                        else if (c >= 0xa440 && c <= 0xc67e)
                        {
                            res.Common++; //常用字
                        }
                        else if (c >= 0xc940 && c <= 0xf9d5)
                        {
                            res.Rare++; //次常用字
                        }
                        else
                        {
                            res.Unknown++; //無效字元
                        }
                    }
                    else
                    {
                        res.Unknown++;
                    }

                    isDblBytes = false;
                }
                else if (b >= 0x80 && b <= 0xfe)
                {
                    isDblBytes = true;
                    dblByteHi = b;
                }
                else if (b < 0x80)
                {
                    res.Ascii++;
                }
            }
            return res;
        }

        private static CodePageHeuristicReport AnalyzeGB2312(IEnumerable<byte> data)
        {
            var res = new CodePageHeuristicReport();
            var isDblBytes = false;
            byte dblByteHi = 0;

            foreach (var b in data)
            {
                if (isDblBytes)
                {
                    if (b >= 0xa1 && b <= 0xfe)
                    {
                        if (dblByteHi >= 0xa1 && dblByteHi <= 0xa9)
                        {
                            res.Symbol++; //符號
                        }
                        else if (dblByteHi >= 0xb0 && dblByteHi <= 0xd7)
                        {
                            res.Common++; //一級漢字(常用字)
                        }
                        else if (dblByteHi >= 0xd8 && dblByteHi <= 0xf7)
                        {
                            res.Rare++; //二級漢字(次常用字)
                        }
                        else
                        {
                            res.Unknown++; //無效字元
                        }
                    }
                    else
                    {
                        res.Unknown++; //無效字元
                    }

                    isDblBytes = false;
                }
                else if (b >= 0xa1 && b <= 0xf7)
                {
                    isDblBytes = true;
                    dblByteHi = b;
                }
                else if (b < 0x80)
                {
                    res.Ascii++;
                }
            }
            return res;
        }

        private static DetectedTextEncoding DetectEncoding(byte[] buffer)
        {
            var size = buffer?.Length ?? 0;

            if (size == 0)
            {
                return DetectedTextEncoding.Utf8NoBom;
            }

            var encoding = CheckBom(buffer, size);

            if (encoding != DetectedTextEncoding.None)
            {
                return encoding;
            }

            encoding = CheckUtf8(buffer, size);

            if (encoding != DetectedTextEncoding.None)
            {
                return encoding;
            }

            encoding = CheckUtf16NewlineChars(buffer, size);

            if (encoding != DetectedTextEncoding.None)
            {
                return encoding;
            }

            encoding = CheckUtf16Ascii(buffer, size);

            if (encoding != DetectedTextEncoding.None)
            {
                return encoding;
            }

            if (!HasHighBitBytes(buffer))
            {
                return DetectedTextEncoding.Ascii;
            }

            var isBig5 = CanDecodeAsBig5(buffer);
            var isGb2312 = CanDecodeAsGb2312(buffer);

            if (isBig5 && isGb2312)
            {
                var score = CompareBig5AndGb2312Likelihood(buffer);

                return score switch
                {
                    1 => DetectedTextEncoding.Big5,
                    -1 => DetectedTextEncoding.Gb2312,
                    _ => DetectedTextEncoding.Big5
                };
            }

            if (isBig5)
            {
                return DetectedTextEncoding.Big5;
            }

            if (isGb2312)
            {
                return DetectedTextEncoding.Gb2312;
            }

            if (!DoesContainNulls(buffer, size))
            {
                return DetectedTextEncoding.Ansi;
            }

            return _nullSuggestsBinary ? DetectedTextEncoding.None : DetectedTextEncoding.Ansi;
        }

        private static bool HasHighBitBytes(IEnumerable<byte> buffer)
        {
            foreach (var value in buffer)
            {
                if (value >= 0x80)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEncodingValid(byte[] bytes, Encoding encoding)
        {
            try
            {
                var decoder = encoding.GetDecoder();

                decoder.Fallback = DecoderFallback.ExceptionFallback;

                var charCount = decoder.GetCharCount(bytes, 0, bytes.Length);
                var chars = new char[charCount];

                decoder.GetChars(bytes, 0, bytes.Length, chars, 0);
                return true;
            }
            catch (DecoderFallbackException)
            {
                return false;
            }
        }

        private static bool CanDecodeAsBig5(byte[] bytes)
        {
            return IsEncodingValid(bytes, Encoding.GetEncoding(950));
        }

        private static bool CanDecodeAsGb2312(byte[] bytes)
        {
            return IsEncodingValid(bytes, Encoding.GetEncoding(936));
        }


        private static string DetectBinaryFileType(byte[] buffer)
        {
            var size = buffer.Length;
            var binaryEncode = string.Empty;

            switch (buffer.Length > 5)
            {
                case true when buffer[0] == 0xff && buffer[1] == 0xd8 && buffer[2] == 0xff && buffer[3] == 0xe0:
                    {
                        binaryEncode = "JPG Image"; //jpg, jpeg
                        break;
                    }
                case true when buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4e && buffer[3] == 0x47:
                    {
                        binaryEncode = "PNG Image"; //png
                        break;
                    }
                case true when buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38:
                    {
                        binaryEncode = "GIF Image"; //gif
                        break;
                    }
                case true when buffer[0] == 0x49 && buffer[1] == 0x49 && buffer[2] == 0x2a && buffer[3] == 0x00:
                    {
                        binaryEncode = "TIF Image"; //tif
                        break;
                    }
                default:
                    {
                        if (buffer.Length > 2 && buffer[0] == 0x42 && buffer[1] == 0x4d)
                        {
                            binaryEncode = "BMP Image"; //bmp
                        }
                        else if (buffer.Length > 5 && buffer[0] == 0x7b && buffer[1] == 0x5c && buffer[2] == 0x72 && buffer[3] == 0x74)
                        {
                            binaryEncode = "Microsoft RTF"; //rtf
                        }
                        else
                        {
                            switch (buffer.Length > 10)
                            {
                                case true when buffer[0] == 0xd0 && buffer[1] == 0xcf && buffer[2] == 0x11 && buffer[3] == 0xe0 && buffer[4] == 0xa1 && buffer[5] == 0xb1 && buffer[6] == 0x1a && buffer[7] == 0xe1:
                                    {
                                        if (buffer.Length > 30 && buffer[26] == 0x04 && buffer[30] == 0x0c)
                                        {
                                            binaryEncode = "Microsoft MSI";
                                        }
                                        else
                                        {
                                            binaryEncode = "Microsoft Office"; //doc, xls, ppt
                                        }

                                        break;
                                    }
                                case true when buffer[0] == 0x50 && buffer[1] == 0x4b && buffer[2] == 0x03 && buffer[3] == 0x04 && buffer[4] == 0x14 && buffer[5] == 0x00 && buffer[6] == 0x06 && buffer[7] == 0x00 && buffer[8] == 0x08:
                                    {
                                        binaryEncode = "Microsoft Office(x)"; //docx, xlsx, pptx
                                        break;
                                    }
                                default:
                                    {
                                        if (buffer.Length > 20 && buffer[0] == 0x00 && buffer[1] == 0x01 && buffer[2] == 0x00 && buffer[3] == 0x00 && buffer[4] == 0x53 && buffer[13] == 0x41 && buffer[14] == 0x43 && buffer[15] == 0x45 && buffer[16] == 0x20 && buffer[17] == 0x44 && buffer[18] == 0x42)
                                        {
                                            binaryEncode = "Access ACCDB"; //20191111 add accdb
                                        }
                                        else if (buffer.Length > 10 && buffer[0] == 0x00 && buffer[1] == 0x01 && buffer[2] == 0x00 && buffer[3] == 0x00 && buffer[4] == 0x53 && buffer[5] == 0x74 && buffer[6] == 0x61 && buffer[7] == 0x6e)
                                        {
                                            binaryEncode = "Access MDB"; //mdb
                                        }
                                        else if (buffer.Length > 5 && buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46)
                                        {
                                            binaryEncode = "PDF"; //pdf
                                        }
                                        else if (buffer.Length > 10 && buffer[0] == 0x50 && buffer[1] == 0x4b && buffer[2] == 0x03 && buffer[3] == 0x04 && buffer[4] == 0x14 && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x00 && buffer[8] == 0x08)
                                        {
                                            binaryEncode = "ZIP"; //zip
                                        }
                                        else
                                        {
                                            switch (buffer.Length > 5)
                                            {
                                                case true when buffer[0] == 0x50 && buffer[1] == 0x4b && buffer[2] == 0x03 && buffer[3] == 0x04 && buffer[4] == 0x0a:
                                                    {
                                                        binaryEncode = "ZIP"; //20191111 add zip
                                                        break;
                                                    }
                                                case true when buffer[0] == 0x52 && buffer[1] == 0x61 && buffer[2] == 0x72 && buffer[3] == 0x21:
                                                    {
                                                        binaryEncode = "RAR"; //rar
                                                        break;
                                                    }
                                                case true when buffer[0] == 0x37 && buffer[1] == 0x7a && buffer[2] == 0xbc && buffer[3] == 0xaf:
                                                    {
                                                        binaryEncode = "7z"; //7z
                                                        break;
                                                    }
                                            }
                                        }

                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }

            return binaryEncode;
        }

        public static string GetTextEncode(string fileName, ref string endOfLineStyle)
        {
            var textEncode = string.Empty;
            byte[] buffer = null;
            var maxBytesToRead = 1 * 1024 * 1024; //20250529 改用前 1MB 的內容判斷編碼方式

            //20231023 發現新的鎖定方式，档案無法被正常開啟，故採用 try/catch 攔截錯誤訊息
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var lengthToRead = Math.Min(fileStream.Length, maxBytesToRead);
                    var bytesToRead = Convert.ToInt32(lengthToRead);

                    buffer = new byte[bytesToRead];
                    fileStream.Read(buffer, 0, bytesToRead);
                }

                endOfLineStyle = DetectLineEndingStyle(fileName);
            }
            catch (Exception ex)
            {
                endOfLineStyle = "ERROR";

                var message = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{ex.Message}\r\n\r\n{MyGlobal.PleaseTryAgain}";

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (endOfLineStyle == "ERROR") //指定的檔案被鎖定，開啟失敗
            {
                return "ERROR";
            }

            var encoding = DetectEncoding(buffer);

            switch (encoding)
            {
                case DetectedTextEncoding.None:
                    {
                        var encoding2 = DetectBinaryFileType(buffer);

                        textEncode = $"Binary`{encoding2}";
                        break;
                    }
                case DetectedTextEncoding.Ascii:
                    {
                        textEncode = "ASCII";
                        break;
                    }
                case DetectedTextEncoding.Ansi:
                    {
                        textEncode = "ANSI"; //ANSI (chars in the range 0-255 range)";
                        break;
                    }
                case DetectedTextEncoding.Utf8Bom:
                case DetectedTextEncoding.Utf8NoBom:
                    {
                        textEncode = "UTF-8";
                        break;
                    }
                case DetectedTextEncoding.Utf16LeBom:
                case DetectedTextEncoding.Utf16LeNoBom:
                    {
                        textEncode = "UTF-16 LE";
                        break;
                    }
                case DetectedTextEncoding.Utf16BeBom:
                case DetectedTextEncoding.Utf16BeNoBom:
                    {
                        textEncode = "UTF-16 BE";
                        break;
                    }
                case DetectedTextEncoding.Big5:
                    {
                        textEncode = "Big5 (Traditional)";
                        break;
                    }
                case DetectedTextEncoding.Gb2312:
                    {
                        textEncode = "GB2312 (Simplified)";
                        break;
                    }
            }

            return textEncode;
        }

        private static DetectedTextEncoding CheckUtf16NewlineChars(byte[] buffer, int size)
        {
            if (size < 2)
            {
                return DetectedTextEncoding.None;
            }

            size--;

            var leControlChars = 0;
            var beControlChars = 0;

            uint pos = 0;

            while (pos < size)
            {
                var ch1 = buffer[pos++];
                var ch2 = buffer[pos++];

                if (ch1 == 0)
                {
                    if (ch2 == 0x0a || ch2 == 0x0d)
                    {
                        ++beControlChars;
                    }
                }
                else if (ch2 == 0)
                {
                    if (ch1 == 0x0a || ch1 == 0x0d)
                    {
                        ++leControlChars;
                    }
                }

                if (leControlChars > 0 && beControlChars > 0)
                {
                    return DetectedTextEncoding.None;
                }
            }

            if (leControlChars > 0)
            {
                return DetectedTextEncoding.Utf16LeNoBom;
            }

            return beControlChars > 0 ? DetectedTextEncoding.Utf16BeNoBom : DetectedTextEncoding.None;
        }

        private static bool DoesContainNulls(byte[] buffer, int size)
        {
            uint pos = 0;

            while (pos < size)
            {
                if (buffer[pos++] == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static DetectedTextEncoding CheckUtf16Ascii(byte[] buffer, int size)
        {
            var numOddNulls = 0;
            var numEvenNulls = 0;
            uint pos = 0;

            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numEvenNulls++;
                }

                pos += 2;
            }

            pos = 1;

            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numOddNulls++;
                }

                pos += 2;
            }

            var evenNullThreshold = numEvenNulls * 2.0 / size;
            var oddNullThreshold = numOddNulls * 2.0 / size;
            var expectedNullThreshold = _utf16ExpectedNullPercent / 100.0;
            var unexpectedNullThreshold = _utf16UnexpectedNullPercent / 100.0;

            if (evenNullThreshold < unexpectedNullThreshold && oddNullThreshold > expectedNullThreshold)
            {
                return DetectedTextEncoding.Utf16LeNoBom;
            }

            if (oddNullThreshold < unexpectedNullThreshold && evenNullThreshold > expectedNullThreshold)
            {
                return DetectedTextEncoding.Utf16BeNoBom;
            }

            return DetectedTextEncoding.None;
        }

        private static DetectedTextEncoding CheckUtf8(byte[] buffer, int size)
        {
            var onlySawAsciiRange = true;
            uint pos = 0;

            while (pos < size)
            {
                var ch = buffer[pos++];

                if (ch == 0 && _nullSuggestsBinary)
                {
                    return DetectedTextEncoding.None;
                }

                int moreChars;

                if (ch <= 127)
                {
                    moreChars = 0;
                }
                else if (ch >= 194 && ch <= 223)
                {
                    moreChars = 1;
                }
                else if (ch >= 224 && ch <= 239)
                {
                    moreChars = 2;
                }
                else if (ch >= 240 && ch <= 244)
                {
                    moreChars = 3;
                }
                else
                {
                    return DetectedTextEncoding.None;
                }

                while (moreChars > 0 && pos < size)
                {
                    onlySawAsciiRange = false;
                    ch = buffer[pos++];

                    if (ch < 128 || ch > 191)
                    {
                        return DetectedTextEncoding.None;
                    }

                    --moreChars;
                }

                if (moreChars > 0)
                {
                    return DetectedTextEncoding.None;
                }
            }

            return onlySawAsciiRange ? DetectedTextEncoding.Ascii : DetectedTextEncoding.Utf8NoBom;
        }
    }
}