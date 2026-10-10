using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support
{
    internal static class QueryEditorAutoCompleteTriggerPolicy
    {
        private const string LexicalStateProbe = ";";

        private const SqlTokenizerOptions TriggerPolicyTokenizerOptions = SqlTokenizerOptions.RecognizeForeignDelimitedIdentifiers | SqlTokenizerOptions.MySqlDashCommentWithoutWhitespace | SqlTokenizerOptions.MySqlHashStartsCommentInsideWord | SqlTokenizerOptions.DisableNestedBlockComments | SqlTokenizerOptions.DisableMySqlSingleQuotedStringBackslashEscape;

        public static bool IsPeriodTriggerAllowed(string text, int periodPosition)
        {
            if (string.IsNullOrEmpty(text) || periodPosition <= 0 || periodPosition > text.Length)
            {
                return false;
            }

            var periodIndex = periodPosition - 1;

            if (periodIndex < 0 || periodIndex >= text.Length || text[periodIndex] != '.')
            {
                return false;
            }

            if (!IsCodePosition(text, periodIndex))
            {
                return false;
            }

            if (IsNumericLiteralPeriod(text, periodIndex))
            {
                return false;
            }

            return true;
        }

        private static bool IsNumericLiteralPeriod(string text, int periodIndex)
        {
            var tokenStart = periodIndex - 1;

            while (tokenStart >= 0 && char.IsDigit(text[tokenStart]))
            {
                tokenStart--;
            }

            var digitStart = tokenStart + 1;
            var hasDigitsBefore = digitStart < periodIndex;
            var nextIndex = periodIndex + 1;
            var hasDigitAfter = nextIndex < text.Length && char.IsDigit(text[nextIndex]);

            if (!hasDigitsBefore && !hasDigitAfter)
            {
                return false;
            }

            if (hasDigitsBefore)
            {
                if (tokenStart >= 0 && (char.IsLetter(text[tokenStart]) || text[tokenStart] == '_' || text[tokenStart] == '$' || text[tokenStart] == '#'))
                {
                    return false;
                }

                return true;
            }

            return hasDigitAfter;
        }

        public static bool IsCodePosition(string text, int position)
        {
            if (string.IsNullOrEmpty(text) || position < 0 || position >= text.Length)
            {
                return false;
            }

            return IsCodePositionUsingSharedTokenizer(text, position);
        }

        private static bool IsCodePositionUsingSharedTokenizer(string text, int position)
        {
            var textWithProbe = string.Concat(text.Substring(0, position), LexicalStateProbe);
            var tokenizationResult = SqlTokenizer.Tokenize(textWithProbe, DataSourceType.MySql, TriggerPolicyTokenizerOptions);
            var probeToken = tokenizationResult.FindTokenContaining(position);

            return probeToken == null || !probeToken.SuppressesKeywordMatching;
        }

        public static bool ShouldSuppressSpaceAfterComma(string text, int caretPosition, bool isAnyPopupVisible)
        {
            if (isAnyPopupVisible || string.IsNullOrEmpty(text) || caretPosition < 0 || caretPosition > text.Length)
            {
                return false;
            }

            if (!TryFindPreviousNonWhiteSpaceCharacter(text, caretPosition - 1, out var commaIndex, out var previousCharacter) || previousCharacter != ',' || !IsCodePosition(text, commaIndex))
            {
                return false;
            }

            if (!TryFindNextNonWhiteSpaceCharacter(text, caretPosition, out _, out var nextCharacter))
            {
                return false;
            }

            return IsSqlTokenStartAfterComma(nextCharacter);
        }

        private static bool TryFindPreviousNonWhiteSpaceCharacter(string text, int startIndex, out int index, out char value)
        {
            index = -1;
            value = '\0';

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var safeStartIndex = Math.Min(startIndex, text.Length - 1);

            for (var i = safeStartIndex; i >= 0; i--)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    continue;
                }

                index = i;
                value = text[i];
                return true;
            }

            return false;
        }

        private static bool TryFindNextNonWhiteSpaceCharacter(string text, int startIndex, out int index, out char value)
        {
            index = -1;
            value = '\0';

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var safeStartIndex = Math.Max(0, startIndex);

            for (var i = safeStartIndex; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    continue;
                }

                index = i;
                value = text[i];
                return true;
            }

            return false;
        }

        private static bool IsSqlTokenStartAfterComma(char value)
        {
            if (char.IsLetterOrDigit(value) || value == '_')
            {
                return true;
            }

            switch (value)
            {
                case '"':
                case '`':
                case '\'':
                case '[':
                case '(':
                case ':':
                case '@':
                case '?':
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
