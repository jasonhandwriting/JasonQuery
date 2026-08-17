using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.Text;
using System;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal sealed class QueryEditorCtrlJPeriodResolver
    {
        private const int MinBackwardScanIndexExclusive = 5; //保留舊版行為
        private readonly ITextEditor _editor;

        public QueryEditorCtrlJPeriodResolver(ITextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public CtrlJPeriodTriggerInfo Resolve(bool IsSqlServer, bool IsMySql)
        {
            var result = new CtrlJPeriodTriggerInfo();
            var text = _editor.Text ?? string.Empty;
            var currentPosition = _editor.CurrentPosition;

            if (string.IsNullOrWhiteSpace(text) || currentPosition < 0)
            {
                return result;
            }

            var periodPosition = FindPeriodPosition(text, currentPosition);

            if (periodPosition <= 0 || periodPosition > text.Length
                || !QueryEditorAutoCompleteTriggerPolicy.IsPeriodTriggerAllowed(text, periodPosition))
            {
                return result;
            }

            var keywordEnd = FindKeywordEnd(text, periodPosition);
            var hasKeyword = keywordEnd > periodPosition;
            var keyword = hasKeyword ? text.Substring(periodPosition, keywordEnd - periodPosition).Trim() : string.Empty;
            var normalizedPeriodPosition = hasKeyword ? NormalizePeriodTriggerPosition(text, currentPosition, periodPosition) : periodPosition;

            result.CanTrigger = true;
            result.PeriodPosition = normalizedPeriodPosition;
            result.Keyword = keyword;

            //如果 CtrlJPeriodTriggerInfo 沒有由 Keyword 自動推導 HasKeyword，
            //就把下面這行打開：
            //result.HasKeyword = !string.IsNullOrWhiteSpace(keyword);

            return result;
        }

        private static int FindPeriodPosition(string text, int currentPosition)
        {
            if (string.IsNullOrEmpty(text) || currentPosition < 0)
            {
                return 0;
            }

            var textLength = text.Length;
            var safeCurrentPosition = Math.Min(currentPosition, textLength);

            //情境一：游標剛好停在 '.' 後面
            if (safeCurrentPosition > 0 && text[safeCurrentPosition - 1] == '.')
            {
                return safeCurrentPosition;
            }

            //情境二：游標停在 '.' 上
            if (safeCurrentPosition < textLength && text[safeCurrentPosition] == '.')
            {
                return safeCurrentPosition + 1;
            }

            //情境三：游標停在識別字中間或尾端，往左掃描找最近的 '.'
            var startIndex = Math.Min(safeCurrentPosition, textLength - 1);

            for (var i = startIndex; i > MinBackwardScanIndexExclusive; i--)
            {
                if (IsBackwardScanStopChar(text[i - 1]))
                {
                    break;
                }

                if (text[i] == '.')
                {
                    return i + 1;
                }
            }

            return 0;
        }

        private static int FindKeywordEnd(string text, int periodPosition)
        {
            if (string.IsNullOrEmpty(text) || periodPosition < 0 || periodPosition >= text.Length)
            {
                return periodPosition;
            }

            var keywordEnd = periodPosition;

            for (var i = periodPosition; i < text.Length; i++)
            {
                var currentChar = text[i];

                if (IsPeriodKeywordChar(currentChar))
                {
                    keywordEnd = i + 1;
                    continue;
                }

                switch (currentChar)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                    case ',':
                    case ')':
                    case '(':
                    case '=':
                    case '>':
                    case '<':
                    case '!':
                    case '-':
                    case '+':
                    case '*':
                    case '/':
                    case ';':
                        {
                            return keywordEnd;
                        }
                    default:
                        {
                            return keywordEnd;
                        }
                }
            }

            return keywordEnd;
        }

        private static bool IsPeriodKeywordChar(char ch)
        {
            return TextHelper.IsEngAlphabetOrNumber(ch, '_');
        }

        private static bool IsBackwardScanStopChar(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                case ',':
                case '(':
                case ')':
                case '=':
                case '>':
                case '<':
                case '!':
                case '+':
                case '-':
                case '*':
                case '/':
                case ';':
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private int NormalizePeriodTriggerPosition(string text, int currentPosition, int periodPosition)
        {
            if (string.IsNullOrEmpty(text))
            {
                return periodPosition;
            }

            //只有 periodPosition 左邊真的是 '.'，才做正規化
            if (periodPosition <= 0 || periodPosition >= text.Length || text[periodPosition - 1] != '.')
            {
                return periodPosition;
            }

            var textLength = text.Length;
            var caret = Math.Max(0, Math.Min(currentPosition, textLength));

            //優先使用 caret 位置本身；若 caret 落在識別字前方，則用 caret 左邊一格
            var index = -1;

            if (caret < textLength && IsPeriodKeywordChar(text[caret]))
            {
                index = caret;
            }
            else if (caret > 0 && IsPeriodKeywordChar(text[caret - 1]))
            {
                index = caret - 1;
            }

            if (index < 0)
            {
                return periodPosition;
            }

            //只在目前 caret 所在字元確實位於 periodPosition 之後時，才往左收斂
            if (index < periodPosition)
            {
                return periodPosition;
            }

            while (index > periodPosition && IsPeriodKeywordChar(text[index - 1]))
            {
                index--;
            }

            return index;
        }
    }
}