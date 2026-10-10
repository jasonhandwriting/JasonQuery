using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Sql.Lexing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using JasonQuery.UI.Services;

namespace JasonQuery.Core.Text
{
    internal static partial class TextHelper
    {
        private const SqlTokenizerOptions TransferStringTokenizerOptions = SqlTokenizerOptions.DisableNestedBlockComments;

        public static string CleanVarcharString(string sql)
        {
            var parts = sql.ToUpper().Split(new[] { " " }, StringSplitOptions.None);
            var prefixes = new[] { "VARCHAR2(", "NVARCHAR2(", "CHAR(" };

            //20240525 檢查字串型態的定義是否有 CHAR 單位，例如：VARCHAR2(20 CHAR)
            if (prefixes.Any(p => parts[1].StartsWith(p, StringComparison.OrdinalIgnoreCase)) && !parts[1].EndsWith(")", StringComparison.Ordinal))
            {
                if (parts.Length > 2 && parts[2].StartsWith("CHAR)", StringComparison.OrdinalIgnoreCase))
                {
                    //清理 VARCHAR2, NVARCHAR2, CHAR 類型中的括弧錯誤
                    sql = sql.Replace(" CHAR)", ")");
                }
            }
            else if (parts[1] == "INTERVAL")
            {
                //20250809 使用正則表達式處理字符串
                sql = Regex.Replace(sql, @"\b(YEAR|MONTH|DAY|HOUR|MINUTE|SECOND)\s*\(", "$1(");
            }
            else if (parts[parts.Length - 1] == ",") //此處結尾為逗號，表示逗號前面有空白
            {
                var sb = new StringBuilder();
                var count = parts.Length - 1;

                for (var i = 0; i < count; i++) //不包含最後一個逗號
                {
                    //處理以逗號結尾的情況
                    sb.Append(parts[i]).Append(" ");
                }

                sb.Append(parts[parts.Length - 1]); //添加逗號
                sql = sb.ToString().Trim(); //去掉多餘空格
            }

            return sql;
        }

        public static string GetFirstWord(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            char separatorChar = MyGlobal.Separator[0];
            var separators = new HashSet<char> { '\r', '\n', ' ', separatorChar, '-', '/', ';' };
            var sb = new StringBuilder();

            foreach (char ch in text)
            {
                if (separators.Contains(ch))
                {
                    break;
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }

        public static bool IsEngAlphabetOrNumberOrSpecialCharacters(string char0, string char1 = "0", string char2 = "0", string char3 = "0")
        {
            var reg = new Regex(@"^[A-Za-z0-9]+$");
            var symbol = "a`a~a!a@a#a$a%a^a&a*a(a)a_a-a+a=a[a{a]a}a|a\\a;a:a'a\"a,a<a.a>a/a?a".Replace("a", MyGlobal.Separator);

            return reg.IsMatch(char0) || char0 == char1 || char0 == char2 || char0 == char3 || symbol.IndexOf($"{MyGlobal.Separator}{char0}{MyGlobal.Separator}", StringComparison.Ordinal) >= 0;
        }

        public static bool IsEngAlphabetOrNumber(char ch0, char ch1 = '0', char ch2 = '0', char ch3 = '0')
        {
            return (ch0 >= 'A' && ch0 <= 'Z') || (ch0 >= 'a' && ch0 <= 'z') || (ch0 >= '0' && ch0 <= '9') || ch0 == ch1 || ch0 == ch2 || ch0 == ch3;
        }

        public static bool IsEngAlphabet(string char0)
        {
            var reg = new Regex(@"^[A-Za-z]+$");

            return reg.IsMatch(char0);
        }

        //20240421 判斷是否包含中文
        public static bool IsContainsChinese(string input)
        {
            return Regex.IsMatch(input, @"[\u4e00-\u9fa5]");
        }

        public static string GetValueFromDictionary(IDictionary<string, string> dic, string key)
        {
            if (dic == null || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return dic.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
        }

        public static int GetTwoByteCharCount(string source)
        {
            if (source.Length == 0)
            {
                return 0;
            }

            //使用正則表達式分割字串，包含所有的空白、符號等
            var pattern = @"(\s|\p{P}|\p{S}|\w|.)";
            var matches = Regex.Matches(source, pattern);
            int twoByteCharCount = 0;

            foreach (Match match in matches)
            {
                var character = match.Value;

                //判斷是否為佔兩個字元的字元 (Unicode 大於 U+007F)
                if (character.Length == 1 && character[0] > 127)
                {
                    twoByteCharCount++;
                }
            }

            return twoByteCharCount;
        }

        public static string GetTwoByteCharSubString(string text, int length)
        {
            var temp = text;
            int j = 0;
            int k = 0;
            var count = text.Length;

            for (var i = 0; i < count; i++)
            {
                if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
                {
                    j += 2; //一個中文字算 2個字元
                }
                else
                {
                    j += 1;
                }

                if (j <= length)
                {
                    k += 1;
                }

                if (j >= length)
                {
                    temp = temp.Substring(0, k);
                    break;
                }
            }

            return temp;
        }

        public static string GetSafeSubstring(string source, int startIndex, int? length = null)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            //確保 iStartIndex/iLength 在允許範圍內
            if (startIndex < 0 || startIndex >= source.Length || (length != null && length < 0))
            {
                return string.Empty;
            }

            //沒有指定長度，直接取到結尾
            if (length == null)
            {
                return source.Substring(startIndex);
            }

            //確保 iLength 不超過剩餘的字串長度
            var safeLength = Math.Min(length.Value, source.Length - startIndex);

            return source.Substring(startIndex, safeLength);
        }

        /// <summary>
        /// 依指定的分隔符號，將指定的字串進行拆解，單引號或雙引號括起的字串，拆解為一組字串(裡面可包含指定的分隔符號)
        /// </summary>
        /// <param name="source">指定的字串</param>
        /// <returns>字串陣列</returns>
        public static string[] SplitByComma(string source)
        {
            //使用正則表達式、逗號來分割指定的字串，忽略雙引號或單引號內的逗號
            var resultList = new List<string>();
            var pattern = @"(?:(?<=,)|^)(?:""(?<quote>[^""]*)""|'(?<quote>[^']*)'|(?<plain>[^,]*))(?:,|$)";

            foreach (Match match in Regex.Matches(source, pattern))
            {
                if (match.Groups["quote"].Success)
                {
                    //保留雙引號或單引號
                    resultList.Add(match.Value.TrimEnd(','));
                }
                else
                {
                    resultList.Add(match.Groups["plain"].Value);
                }
            }

            //將 List<string> 轉為 string[]
            return resultList.ToArray();
        }

        /// <summary>
        /// 根據指定位置從文字內容中取得單詞
        /// </summary>
        /// <param name="text">文字內容</param>
        /// <param name="index">指定的位置索引</param>
        /// <returns>指定位置的單詞，如果不存在則回傳空字串</returns>
        public static string GetWordFromPosition(string text, int index, out int startIndexNew)
        {
            startIndexNew = -1;

            if (string.IsNullOrWhiteSpace(text) || index < 0 || index >= text.Length)
            {
                return string.Empty; // 無效的輸入
            }

            //確保指定位置不是空白或換行符號，往前或往後移動到非空白字符
            int startIndex = index;

            while (startIndex >= 0 && (char.IsWhiteSpace(text[startIndex]) || text[startIndex] == '\n'))
            {
                startIndex--;
            }

            int endIndex = index;

            while (endIndex < text.Length && (char.IsWhiteSpace(text[endIndex]) || text[endIndex] == '\n'))
            {
                endIndex++;
            }

            if (startIndex < 0 || endIndex >= text.Length)
            {
                return string.Empty; //無效的輸入
            }

            //利用正則表達式找到該位置的完整單詞
            var pattern = @"\b\w+\b";
            MatchCollection matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                if (index >= match.Index && index < match.Index + match.Length)
                {
                    startIndexNew = match.Index;
                    return match.Value; //找到匹配的單詞
                }
            }

            return string.Empty; //如果沒有匹配到單詞
        }

        public static string[] GenerateDateFormats(CultureInfo culture)
        {
            var separators = new[] { '-', '/', ' ', '.' };
            var timeFormats = new List<string>();

            //原有不含毫秒的格式 (保持兼容性)
            timeFormats.AddRange
            (
                new[]
                {
                    " HH:mm:ss",
                    " H:mm:ss",
                    " tt hh:mm:ss",
                    "tt hh:mm:ss",
                    " tt hh:mm",
                    "tt hh:mm",
                    "THH:mm:ss"
                }
            );

            var datePartFormats = new List<string>();

            if (culture.Name == "zh-CN" || culture.Name == "zh-TW")
            {
                datePartFormats.AddRange
                (
                    new[]
                    {
                        "dd{0}MMMM{0}yyyy",
                        "MMMM{0}dd{0}yyyy",
                        "yyyy{0}MMMM{0}dd",
                        "dd{0}MM{0}yyyy",
                        "MM{0}dd{0}yyyy",
                        "yyyy{0}MM{0}dd"
                    }
                );
            }
            else if (culture.Name == "en-US")
            {
                datePartFormats.AddRange
                (
                    new[]
                    {
                        "dd{0}MMMM{0}yyyy",
                        "MMMM{0}dd{0}yyyy",
                        "yyyy{0}MMMM{0}dd",
                        "dd{0}MMM{0}yyyy",
                        "MMM{0}dd{0}yyyy",
                        "yyyy{0}MMM{0}dd",
                        "dd{0}MM{0}yyyy",
                        "MM{0}dd{0}yyyy",
                        "yyyy{0}MM{0}dd"
                    }
                );
            }

            var formats = new HashSet<string>();

            foreach (var sep in separators)
            {
                foreach (var datePart in datePartFormats)
                {
                    var dateFormat = string.Format(datePart, sep);

                    foreach (var timePart in timeFormats)
                    {
                        formats.Add(dateFormat + timePart);
                    }
                }
            }

            return formats.ToArray();
        }

        /// <summary>
        /// 傳入兩個版本號，比較兩個版本號的大小，例如 "0.90.2" 與 "0.90.1"
        /// 如果 sVersion1 大於 sVersion2，返回 1
        /// 如果 sVersion1 小於 sVersion2，返回 -1
        /// 如果兩者相等，返回 0
        /// </summary>
        public static int CompareVersions(string version1, string version2)
        {
            //根據「小數點符號」分割字串
            var parts1 = version1.Split('.');
            var parts2 = version2.Split('.');

            //取較長的長度作為比較基準
            var maxLength = Math.Max(parts1.Length, parts2.Length);

            for (var i = 0; i < maxLength; i++)
            {
                //自動補零：當比較的位數不同時，自動將缺少的位數視為0，例如 0.90 與 0.90.1 比較時，0.90 會被視為 0.90.0
                int num1 = (i < parts1.Length) ? int.Parse(parts1[i]) : 0;
                int num2 = (i < parts2.Length) ? int.Parse(parts2[i]) : 0;

                if (num1 > num2)
                {
                    return 1;
                }
                else if (num1 < num2)
                {
                    return -1;
                }
            }

            return 0;
        }

        public static bool CheckTextStartEndWith(string text, string preFix, string sufFix,
                                                 StringComparison comparisonTypeStart = StringComparison.Ordinal,
                                                 StringComparison comparisonTypeEnd = StringComparison.Ordinal)
        {
            return text.StartsWith(preFix, comparisonTypeStart) && text.EndsWith(sufFix, comparisonTypeEnd);
        }

        public static bool CheckTextStartEndWith(string text, string preFix1, string preFix2, string sufFix,
                                                 StringComparison comparisonTypeStart = StringComparison.Ordinal,
                                                 StringComparison comparisonTypeEnd = StringComparison.Ordinal)
        {
            return (text.StartsWith(preFix1, comparisonTypeStart) || text.StartsWith(preFix2, comparisonTypeStart)) && text.EndsWith(sufFix, comparisonTypeEnd);
        }

        public static bool CheckTextStartWithAndContains(string text, string preFix, string contain,
                                                         StringComparison comparisonTypeStart = StringComparison.Ordinal,
                                                         StringComparison comparisonTypeContain = StringComparison.Ordinal)
        {
            if (text == null)
            {
                return false;
            }

            return text.StartsWith(preFix, comparisonTypeStart) && text.IndexOf(contain, comparisonTypeContain) >= 0;
        }

        //20250607 判斷物件的 Tag 屬性是否為空值
        public static bool IsNullOrEmptyTag(object obj)
        {
            return !(obj is string s) || string.IsNullOrEmpty(s);
        }

        //20250607 回傳安全的字串
        public static string GetSafeString(object obj, string defaultValue = "")
        {
            return obj?.ToString() ?? defaultValue;
        }

        public static string TruncateDecimal(string value, int numericScale)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            int decimalIndex = value.IndexOf('.');

            if (decimalIndex < 0)
            {
                return value;
            }

            int scale = Math.Max(0, numericScale);

            if (scale == 0)
            {
                return value.Substring(0, decimalIndex);
            }

            int endIndex = Math.Min(decimalIndex + 1 + scale, value.Length);

            return value.Substring(0, endIndex);
        }

        public static void CopyTextToClipboard(string text, string key)
        {
            const int maxRetry = 10; //最多嘗試次數
            const int retryDelay = 100; //延遲毫秒數
            bool isSuccess = false;

            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    Clipboard.SetDataObject(text, true);
                    isSuccess = true;
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(retryDelay); //讓出 CPU，等待下一次嘗試
                }
            }

            if (!isSuccess)
            {
                try
                {
                    //雙重確認：剪貼簿是否已複製成功？
                    if (!Clipboard.ContainsText() || Clipboard.GetText() != text)
                    {
                        var defaultMessage = "An error occurred while copying to the clipboard.\r\nPossible reasons are as follows:\r\n1. The clipboard is locked by another application.\r\n2. The scrapbook content has expired or been cleared.";
                        var message = LocalizationHelper.GetLanguageString(defaultMessage, "Global", "Global", "msg", "CopyToClipboardError", "Text");
                        var languageText = $"{MyGlobal.AnUnexpectedErrorHasOccurred}\r\n\r\n{message}\r\n\r\n{MyGlobal.PleaseTryAgain}";

                        MessageBox.Show(languageText, $"{AppConfigHelper.MessageBoxCaption} - CopyTextToClipboard {key}", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                catch
                {
                    //避免 UI 當掉
                }
            }
        }

        public static string TransferWordCase(string word, bool toLowercase)
        {
            word = toLowercase ? word.ToLower() : word;

            return word;
        }

        /// <summary>
        /// 轉換大小寫／是否要處理註解／是否要進行置換處理
        /// </summary>
        /// <param name="toUppercase">轉換大小寫</param>
        /// <param name="sql">SQL Statement</param>
        /// <param name="isComment">是否要處理註解</param>
        /// <param name="isReplace">是否要進行置換處理</param>
        /// <returns></returns>
        public static string GetTransferString(bool toUppercase, string sql, bool isComment = false, bool isReplace = false)
        {
            var protectedPositions = new bool[sql.Length];
            var commentPositions = new bool[sql.Length];
            var tokenizationResult = SqlTokenizer.Tokenize(sql, DataSourceType.None, TransferStringTokenizerOptions);

            foreach (var token in tokenizationResult.Tokens)
            {
                if (token.SuppressesKeywordMatching)
                {
                    MarkTransferStringPositions(protectedPositions, token.Start, token.EndExclusive);
                }

                if (!token.IsComment)
                {
                    continue;
                }

                MarkTransferStringPositions(commentPositions, token.Start, token.EndExclusive);

                //Legacy GetTransferString masks the first CR/LF character that terminates a -- comment.
                if (token.Kind == SqlTokenKind.LineComment && token.EndExclusive < sql.Length && (sql[token.EndExclusive] == '\r' || sql[token.EndExclusive] == '\n'))
                {
                    commentPositions[token.EndExclusive] = true;
                }
            }

            if (isComment)
            {
                var array = sql.ToCharArray();

                for (var i = 0; i < array.Length; i++)
                {
                    if (commentPositions[i])
                    {
                        array[i] = ' ';
                    }
                }

                var result = new string(array);

                if (isReplace)
                {
                    result = NormalizeTransferStringForWithClassification(result);
                }

                return result;
            }

            var sbResult = new StringBuilder(sql.Length);

            for (var i = 0; i < sql.Length; i++)
            {
                var letterString = sql[i].ToString();

                if (protectedPositions[i])
                {
                    sbResult.Append(letterString);
                    continue;
                }

                sbResult.Append(toUppercase ? letterString.ToUpper() : letterString.ToLower());
            }

            return sbResult.ToString();
        }

        private static void MarkTransferStringPositions(bool[] positions, int start, int endExclusive)
        {
            var normalizedStart = Math.Max(0, start);
            var normalizedEnd = Math.Min(positions.Length, endExclusive);

            for (var i = normalizedStart; i < normalizedEnd; i++)
            {
                positions[i] = true;
            }
        }

        private static string NormalizeTransferStringForWithClassification(string source)
        {
            var result = Regex.Replace(source.ToUpper(), @"\r\n|\r|\n", " ") //替換所有換行符號為空格
                              .Replace(")", ") ") //在右括號後加空格
                              .Replace("SELECT", " SELECT ") //在 SELECT 前後加空格
                              .Replace("DELETE", " DELETE ") //在 DELETE 前後加空格
                              .Replace("UPDATE", " UPDATE ") //在 UPDATE 前後加空格
                              .Replace("INSERT", " INSERT ") //在 INSERT 前後加空格
                              .Replace("INTO", " INTO ") //在 INTO 前後加空格
                              .Trim(); //去除多餘的空格

            //確保所有連續的空白字符替換為一個空格
            return Regex.Replace(result, @"\s+", " ");
        }

        public static string GetStringBetween(string source, string startString, string endString)
        {
            bool isStart = source.Contains(startString);
            bool isEnd = source.Contains(endString);
            int sourceLength = source.Length;
            int startEndLength = startString.Length + endString.Length;
            int replacedLength = source.Replace(startString, string.Empty).Length;

            if (!isStart || !isEnd || startString != endString || sourceLength - startEndLength != replacedLength)
            {
                return string.Empty;
            }

            var start = source.IndexOf(startString, StringComparison.Ordinal) + startString.Length;
            var end = source.IndexOf(endString, start, StringComparison.Ordinal);
            var result = GetSafeSubstring(source, start, end - start);

            return result;
        }

        public static string GetStringBetween2(string source, string startString, string endString, bool isContainsStart)
        {
            var i = isContainsStart ? startString.Length : 0;

            if (!source.Contains(startString))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(endString) && !source.Contains(endString))
            {
                return string.Empty;
            }

            var start = source.IndexOf(startString, StringComparison.Ordinal) + startString.Length;
            int end;

            if (string.IsNullOrEmpty(endString))
            {
                end = source.Length - start;
            }
            else
            {
                end = source.IndexOf(endString, start, StringComparison.Ordinal);
            }

            var value1 = GetSafeSubstring(source, start - i, end + i);
            var value2 = GetSafeSubstring(source, start, end - start);
            var result = string.IsNullOrEmpty(endString) ? value1 : value2;

            return result;
        }

        public static string GetKeyFromDictionary(Dictionary<string, string> dic, string value)
        {
            var key = string.Empty;

            foreach (var item in dic.Where(item => value == item.Value))
            {
                key = item.Key;
                break;
            }

            return key;
        }

        public static string GetMilliseconds(DateTime timeMilliseconds, int numericScale, string timeZone)
        {
            var timeOfDayMilliseconds = timeMilliseconds.TimeOfDay.ToString();
            var temp = string.Empty;

            if (timeOfDayMilliseconds.IndexOf('.') >= 0)
            {
                var temp1 = timeOfDayMilliseconds.IndexOf('.');
                var timeOfDayMilliseconds2 = timeOfDayMilliseconds.Substring(temp1 + 1);

                timeOfDayMilliseconds = $".{timeOfDayMilliseconds2}";
            }
            else
            {
                //20240505 沒有包含 . 符號，表示毫秒值為 0
                timeOfDayMilliseconds = new string('0', numericScale);

                if (!string.IsNullOrEmpty(timeOfDayMilliseconds))
                {
                    timeOfDayMilliseconds = $".{timeOfDayMilliseconds}";
                }
            }

            if (timeOfDayMilliseconds.Length >= numericScale + 1)
            {
                var timeOfDayMilliseconds2 = timeOfDayMilliseconds.Substring(0, numericScale + 1);

                temp = timeMilliseconds.ToString(format: $"{MyLibrary.DateFormat} HH:mm:ss{timeOfDayMilliseconds2}{timeZone}");
            }
            else
            {
                var len = Math.Max(0, numericScale + 1 - timeOfDayMilliseconds.Length);
                var timeOfDayMilliseconds2 = new string('0', len);

                temp = timeMilliseconds.ToString(format: $"{MyLibrary.DateFormat} HH:mm:ss{timeOfDayMilliseconds}{timeOfDayMilliseconds2}{timeZone}");

                //20240825 沒有毫秒位數，也沒有時區，只取年月日時分秒即可
                if (numericScale == 0 && string.IsNullOrEmpty(timeZone))
                {
                    temp = temp.Substring(0, 19);
                }
            }

            return temp;
        }

        public static string GetMilliseconds(DateTime dtTime, int numericScale)
        {
            var milliseconds = string.Empty;
            var timeOfDayMilliseconds = dtTime.TimeOfDay.ToString();

            if (timeOfDayMilliseconds.IndexOf('.') >= 0 && numericScale > 0)
            {
                timeOfDayMilliseconds += "000";

                var temp1 = timeOfDayMilliseconds.IndexOf('.');
                var temp = timeOfDayMilliseconds.Substring(temp1 + 1, numericScale);

                milliseconds = $".{temp}";
            }
            else
            {
                if (numericScale > 0)
                {
                    var temp = new string('0', numericScale);

                    milliseconds = $".{temp}";
                }
            }

            return milliseconds;
        }

        public static string GetMilliseconds(DateTimeOffset dtTime, int numericScale)
        {
            var milliseconds = string.Empty;
            var timeOfDayMilliseconds = dtTime.TimeOfDay.ToString();
            var timeZone = dtTime.ToString().Substring(dtTime.ToString().LastIndexOf(" ", StringComparison.Ordinal));

            if (timeOfDayMilliseconds.IndexOf('.') >= 0 && numericScale > 0)
            {
                timeOfDayMilliseconds += "000";

                var temp1 = timeOfDayMilliseconds.IndexOf('.');
                var temp = timeOfDayMilliseconds.Substring(temp1 + 1, numericScale);

                milliseconds = $".{temp}{timeZone}";
            }
            else
            {
                if (numericScale > 0)
                {
                    var temp = new string('0', numericScale);

                    milliseconds = $".{temp}{timeZone}";
                }
            }

            return milliseconds;
        }

        //20250720 取得雙引號(或單引號)中間指定文字的起始位置 (必須出現在 iStartPosition 之後)
        public static bool GetKeywordPosition(string text, string quoteSymbol, string keyword, int startPosition, out int resultPosition)
        {
            resultPosition = -1;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            //建立要檢查的引號列表
            List<string> quoteSymbols = new List<string>();

            if (string.IsNullOrWhiteSpace(quoteSymbol)) //沒有指定引號
            {
                quoteSymbols.Add("'");  //優先找單引號
                quoteSymbols.Add("\""); //再試著找雙引號
            }
            else
            {
                quoteSymbols.Add(quoteSymbol);
            }

            foreach (var quote in quoteSymbols)
            {
                //產生 Regex pattern，例如：'([^']*)' 或 "([^"]*)"
                var pattern = Regex.Escape(quote) + "([^" + Regex.Escape(quote) + "]*)" + Regex.Escape(quote);

                foreach (Match match in Regex.Matches(text, pattern))
                {
                    if (match.Index < startPosition)
                    {
                        continue;
                    }

                    var content = match.Groups[1].Value;
                    var innerIndex = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

                    if (innerIndex >= 0)
                    {
                        resultPosition = match.Index + quote.Length + innerIndex;
                        return true;
                    }
                }
            }

            //20250930 沒有指定引號，用關鍵字再找一次
            if (string.IsNullOrWhiteSpace(quoteSymbol))
            {
                var pos = text.IndexOf(keyword, startPosition, StringComparison.OrdinalIgnoreCase);

                if (pos >= 0)
                {
                    resultPosition = pos;
                    return true;
                }
            }

            return false;
        }

        public static string TransferSqlToCode_CSharpStyle1(string sql, string variableName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\\\"";
            var symbolEqual = "=";
            var symbolQuoted = "\"";
            var concateOperator = "+";
            var symbolEnd = ";";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{variableName} {symbolEqual} {symbolQuoted}{temp01}{symbolQuoted} {concateOperator}");
                    }
                    else
                    {
                        var temp01 = new string(' ', variableName.Length);
                        var temp02 = new string(' ', symbolEqual.Length);
                        var temp03 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{temp01} {temp02} {symbolQuoted}{temp03}{symbolQuoted} {concateOperator}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                sbResult.Length--;
                result = $"{sbResult}{symbolEnd}";

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_CSharpStyle2(string sql, string variableName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\\\"";
            var symbolEqualFirst = "=";
            var symbolEqualNext = "+=";
            var symbolQuoted = "\"";
            var symbolEnd = ";";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.AppendLine($"{variableName} {symbolEqualFirst} {symbolQuoted}{symbolQuoted}{symbolEnd}");
                        sbResult.Append($"{variableName} {symbolEqualNext} {symbolQuoted}{temp01}\\r\\n{symbolQuoted}{symbolEnd}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? "\\r\\n" : string.Empty;

                        sbResult.Append($"{variableName} {symbolEqualNext} {symbolQuoted}{temp01}{temp02}{symbolQuoted}{symbolEnd}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_CSharpStyle3(string sql, string variableName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\\\"";
            var symbolEqualFirst = "=";
            var symbolEqualNext = "+=";
            var symbolQuoted = "\"";
            var newLineCharacter = "+ Environment.NewLine";
            var symbolEnd = ";";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.AppendLine($"{variableName} {symbolEqualFirst} {symbolQuoted}{symbolQuoted}{symbolEnd}");
                        sbResult.Append($"{variableName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted} {newLineCharacter}{symbolEnd}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? "\r\n" : string.Empty;
                        var temp03 = i < parts.Length - 1 ? $" {newLineCharacter}" : string.Empty;

                        sbResult.Append($"{variableName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted}{temp03}{symbolEnd}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                result = $"{sbResult}{symbolEnd}";

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        /// <summary>
        /// 產生 StringBuilder 形式的程式碼
        /// </summary>
        /// <param name="sql">SQL Statement</param>
        /// <param name="variableName">SQL 變數名稱</param>
        /// <param name="stringBuilderName">StringBuilder 變數名稱</param>
        /// <returns></returns>
        public static string TransferSqlToCode_CSharpStyle4(string sql, string variableName, string stringBuilderName, bool isCopyToClipboard = true)
        {
            var lines = sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sbCode = new StringBuilder();
            var i = 0;

            sbCode.AppendLine($"var {stringBuilderName} = new StringBuilder();").AppendLine();

            foreach (var line in lines)
            {
                var lineKeyword = i == lines.Length - 1 ? string.Empty : "Line";
                var line1 = line.Replace("\"", "\\\"");

                sbCode.AppendLine($"{stringBuilderName}.Append{lineKeyword}(\"{line1}\");");

                i++;
            }

            sbCode.AppendLine();
            sbCode.AppendLine($"var {variableName} = {stringBuilderName}.ToString();");

            var result = sbCode.ToString();

            if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
            {
                Clipboard.SetText(result);
            }

            return result;
        }

        public static string TransferSqlToCode_VBNet1(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var symbolEqual = "=";
            var symbolQuoted = "\"";
            var concateOperator = "&";
            var doubleQualReplacement = "\"\"";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{varibleName} {symbolEqual} {symbolQuoted}{temp01}{symbolQuoted} {concateOperator}");
                    }
                    else
                    {
                        var temp01 = new string(' ', varibleName.Length);
                        var temp02 = new string(' ', symbolEqual.Length);
                        var temp03 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{temp01} {temp02} {symbolQuoted}{temp03}{symbolQuoted} {concateOperator}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                sbResult.Length--;
                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_VBNet2(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\"\"";
            var symbolEqualFirst = "=";
            var symbolEqualNext = "+=";
            var symbolQuoted = "\"";
            var newLineCharacter = "& VbCrLf";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.AppendLine($"{varibleName} {symbolEqualFirst} {symbolQuoted}{symbolQuoted}");
                        sbResult.Append($"{varibleName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted} {newLineCharacter}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? $" {newLineCharacter}" : string.Empty;

                        sbResult.Append($"{varibleName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted}{temp02}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_VBNet3(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\"\"";
            var symbolEqualFirst = "=";
            var symbolEqualNext = "+=";
            var symbolQuoted = "\"";
            var newLineCharacter = "& Environment.NewLine";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.AppendLine($"{varibleName} {symbolEqualFirst} {symbolQuoted}{symbolQuoted}");
                        sbResult.Append($"{varibleName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted} {newLineCharacter}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? $" {newLineCharacter}" : string.Empty;

                        sbResult.Append($"{varibleName} {symbolEqualNext} {symbolQuoted}{temp01}{symbolQuoted}{temp02}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_VB6VBA1(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\"\"";
            var symbolEqual = "=";
            var symbolQuoted = "\"";
            var concateOperator = "& _";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{varibleName} {symbolEqual} {symbolQuoted}{temp01}{symbolQuoted} {concateOperator}");
                    }
                    else
                    {
                        var temp01 = new string(' ', varibleName.Length);
                        var temp02 = new string(' ', symbolEqual.Length);
                        var temp03 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{temp01} {temp02} {symbolQuoted}{temp03}{symbolQuoted} {concateOperator}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                sbResult.Length--;
                sbResult.Length--;
                sbResult.Length--;
                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_VB6VBA2(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var varibleNameNext = varibleName + " & ";
            var doubleQualReplacement = "\"\"";
            var symbolEqual = "=";
            var symbolQuoted = "\"";
            var newLineCharacter = "& VbCrLf";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i];

                    if (i == 0)
                    {
                        sbResult.AppendLine($"{varibleName} {symbolEqual} {symbolQuoted}{symbolQuoted}");

                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{varibleName} {symbolEqual} {varibleNameNext}{symbolQuoted}{temp01}{symbolQuoted} {newLineCharacter}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? $" {newLineCharacter}" : string.Empty;

                        sbResult.Append($"{varibleName} {symbolEqual} {varibleNameNext}{symbolQuoted}{temp01}{symbolQuoted}{temp02}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                //sbResult.Length--;
                result = sbResult.ToString();

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_Delphi61(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\"";
            var symbolEqual = ":=";
            var symbolEnd = ";";
            var symbolQuoted = "'";
            var concateOperator = "+";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i].Replace("'", "''"); //直接將單引號置換成兩個單引號

                    if (i == 0)
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{varibleName} {symbolEqual} {symbolQuoted}{temp01}{symbolQuoted} {concateOperator}");
                    }
                    else
                    {
                        var temp01 = new string(' ', varibleName.Length);
                        var temp02 = new string(' ', symbolEqual.Length);
                        var temp03 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{temp01} {temp02} {symbolQuoted}{temp03}{symbolQuoted} {concateOperator}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                sbResult.Length--;
                result = $"{sbResult}{symbolEnd}";

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static string TransferSqlToCode_Delphi62(string sql, string varibleName, bool isCopyToClipboard = true)
        {
            var doubleQualReplacement = "\"";
            var symbolEqual = ":=";
            var symbolEnd = ";";
            var symbolQuoted = "'";
            var newLineCharacter = "+ #13#10";
            var varibleNameNext = $"{varibleName} + ";
            var result = string.Empty;
            var sbResult = new StringBuilder();

            try
            {
                var parts = sql.Split(new[] { "\r\n" }, StringSplitOptions.None);
                var count = parts.Length;

                for (var i = 0; i < count; i++)
                {
                    var line = parts[i].Replace("'", "''"); //直接將單引號置換成兩個單引號

                    if (i == 0)
                    {
                        sbResult.AppendLine($"{varibleName} {symbolEqual} {symbolQuoted}{symbolQuoted}{symbolEnd}");

                        var temp01 = line.Replace("\"", doubleQualReplacement);

                        sbResult.Append($"{varibleName} {symbolEqual} {varibleNameNext}{symbolQuoted}{temp01}{symbolQuoted} {newLineCharacter}{symbolEnd}");
                    }
                    else
                    {
                        var temp01 = line.Replace("\"", doubleQualReplacement);
                        var temp02 = i < parts.Length - 1 ? $" {newLineCharacter}" : string.Empty;

                        sbResult.Append($"{varibleName} {symbolEqual} {varibleNameNext}{symbolQuoted}{temp01}{symbolQuoted}{temp02}{symbolEnd}");
                    }

                    if (i < parts.Length - 1) //不是選取區的最後一列，後面加上換行符號
                    {
                        sbResult.AppendLine();
                    }
                }

                sbResult.Length--;
                result = $"{sbResult}{symbolEnd}";

                if (!string.IsNullOrEmpty(result) && isCopyToClipboard)
                {
                    Clipboard.SetText(result);
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return result;
        }

        public static void ShowSqlToCodeMessage(string language, string formName, string optionText = "")
        {
            var languageText = LocalizationHelper.GetLanguageString("{CODE} statement copied to clipboard.", "form", formName, "msg", "Sql2Code2Clipboard", "Text").Replace("{CODE}", language);
            var temp = LocalizationHelper.GetLanguageString("The current variable name is {VARIABLE}.", "form", formName, "msg", "Sql2Code4VariableName", "Text").Replace("{VARIABLE}", $"\"{MyLibrary.SqlToCodeSqlVariableName}\"{optionText}");

            languageText += $"\r\n\r\n{temp}";
            temp = LocalizationHelper.GetLanguageString("You can change the variable name from [Tools] > [Options] > [SQL to Code] > \"Variable Name\".", "form", formName, "msg", "Sql2Code4ChangeVariable", "Text");
            languageText += $"\r\n{temp}";

            MessageBox.Show(languageText, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 將非 null、非空字串、非空白字串的內容組合成字串。
        /// </summary>
        /// <param name="separator">分隔符號，例如 ", "</param>
        /// <param name="values">要組合的字串陣列</param>
        /// <returns>組合後的字串；若沒有有效內容則回傳空字串</returns>
        public static string JoinNonEmpty(string separator, params string[] values)
        {
            return JoinNonEmpty(separator, values as IEnumerable<string>);
        }

        /// <summary>
        /// 將非 null、非空字串、非空白字串的內容組合成字串。
        /// </summary>
        /// <param name="separator">分隔符號，例如 ", "</param>
        /// <param name="values">要組合的字串集合</param>
        /// <returns>組合後的字串；若沒有有效內容則回傳空字串</returns>
        public static string JoinNonEmpty(string separator, IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join
            (
                separator ?? string.Empty,
                values.Where(s => !string.IsNullOrWhiteSpace(s))
            );
        }

        /// <summary>
         /// 移除包住整個字串的外層成對小括號。
         /// 例如：(getdate()) -> getdate()，((1)) -> 1。
         /// 若外層小括號不是完整包住整個字串，則保留原字串。
         /// </summary>
        public static string RemoveEnclosingParentheses(string source, bool trimOuterWhiteSpace = false)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            var result = trimOuterWhiteSpace ? source.Trim() : source;

            while (IsWrappedByEnclosingParentheses(result))
            {
                result = result.Substring(1, result.Length - 2);

                if (trimOuterWhiteSpace)
                {
                    result = result.Trim();
                }
            }

            return result;
        }

        private static bool IsWrappedByEnclosingParentheses(string source)
        {
            if (string.IsNullOrEmpty(source) || source.Length < 2)
            {
                return false;
            }

            if (source[0] != '(' || source[source.Length - 1] != ')')
            {
                return false;
            }

            var depth = 0;

            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];

                if (ch == '(')
                {
                    depth++;
                }
                else if (ch == ')')
                {
                    depth--;

                    if (depth < 0)
                    {
                        return false;
                    }

                    //外層括號如果在最後一個字元之前就閉合，代表它不是包住整個字串，例如：(1)+(2)
                    if (depth == 0 && i < source.Length - 1)
                    {
                        return false;
                    }
                }
            }

            return depth == 0;
        }
    }
}
