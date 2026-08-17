using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Text;
using System;

//負責從錯誤訊息抽 Target Word

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class SqlErrorQuotedTextResult
        {
            public bool Found { get; set; }
            public int From { get; set; }
            public int To { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        private sealed class SqlErrorTargetWordResult
        {
            public string ErrorMessage { get; set; } = string.Empty;
            public string TargetWord { get; set; } = string.Empty;
            public string TargetWordVariables { get; set; } = string.Empty;
            public string PositionText { get; set; } = string.Empty;
            public bool IsKeywordNotFound { get; set; }
        }

        private string NormalizeDottedDoubleQuotedNameInErrorMessage(string errorMessage)
        {
            //其中一種情況是："aa"."name" → "aa.name"
            return (errorMessage ?? string.Empty).Replace("\".\"", ".");
        }

        private bool TryExtractFirstDoubleQuotedText(string text, out SqlErrorQuotedTextResult result)
        {
            return TryExtractQuotedText(text, '\"', out result);
        }

        private bool TryExtractLastDoubleQuotedText(string text, out SqlErrorQuotedTextResult result)
        {
            return TryExtractLastQuotedText(text, '\"', out result);
        }

        private bool TryExtractFirstSingleQuotedText(string text, out SqlErrorQuotedTextResult result)
        {
            return TryExtractQuotedText(text, '\'', out result);
        }

        private bool TryExtractLastSingleQuotedText(string text, out SqlErrorQuotedTextResult result)
        {
            return TryExtractLastQuotedText(text, '\'', out result);
        }

        private bool TryExtractQuotedText(string text, char quoteChar, out SqlErrorQuotedTextResult result)
        {
            result = new SqlErrorQuotedTextResult();

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var from = text.IndexOf(quoteChar) + 1;

            if (from <= 0)
            {
                return false;
            }

            var to = text.IndexOf(quoteChar, from);

            if (to <= from)
            {
                return false;
            }

            result.Found = true;
            result.From = from;
            result.To = to;
            result.Text = TextHelper.GetSafeSubstring(text, from, to - from);

            return true;
        }

        private bool TryExtractLastQuotedText(string text, char quoteChar, out SqlErrorQuotedTextResult result)
        {
            result = new SqlErrorQuotedTextResult();

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var from = text.IndexOf(quoteChar) + 1;
            var to = text.LastIndexOf(quoteChar);

            if (from <= 0 || to <= from)
            {
                return false;
            }

            result.Found = true;
            result.From = from;
            result.To = to;
            result.Text = TextHelper.GetSafeSubstring(text, from, to - from);

            return true;
        }

        private bool HasAtLeastTwoSingleQuotes(string text)
        {
            return CountChar(text, '\'') >= 2;
        }

        private bool HasAtLeastTwoDoubleQuotes(string text)
        {
            return CountChar(text, '\"') >= 2;
        }

        private int CountChar(string text, char value)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var count = 0;

            foreach (var c in text)
            {
                if (c == value)
                {
                    count++;
                }
            }

            return count;
        }

        private string TrimErrorTargetWordToFirstTokenIfNeeded(string targetWord)
        {
            if (string.IsNullOrEmpty(targetWord))
            {
                return string.Empty;
            }

            return targetWord.IndexOfAny(new[] { '\r', '\n', ' ', '-', '/', ';' }) >= 0
                   ? TextHelper.GetFirstWord(targetWord)
                   : targetWord;
        }

        private bool ContainsIgnoreCase(string source, string value)
        {
            return (source ?? string.Empty).IndexOf(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int IndexOfIgnoreCase(string source, string value)
        {
            return (source ?? string.Empty).IndexOf(value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private string RemoveMySqlDatabasePrefixFromTargetWord(string errorMessage, string targetWord)
        {
            if (string.IsNullOrEmpty(targetWord))
            {
                return string.Empty;
            }

            if (!TextHelper.CheckTextStartWithAndContains(errorMessage, "Table", "doesn't exist"))
            {
                return targetWord;
            }

            if (!targetWord.Contains("."))
            {
                return targetWord;
            }

            var result = targetWord.Replace($"{DatabaseSqlExecutor.DatabaseName}.", string.Empty);

            if (result.Contains("."))
            {
                var parts = result.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    result = parts[parts.Length - 1];
                }
            }

            return result;
        }

        private string TryApplyMySqlBacktickTargetWord(string targetWord, string sqlExecuted, out bool isKeywordNotFound)
        {
            isKeywordNotFound = false;

            if (string.IsNullOrEmpty(targetWord))
            {
                return string.Empty;
            }

            if (ContainsIgnoreCase(sqlExecuted, targetWord))
            {
                return targetWord;
            }

            isKeywordNotFound = true;

            //20240922 判斷使用者是否將欄位名稱前後加上 ` 符號？
            if (targetWord.Contains("."))
            {
                var backtickTargetWord = $"{targetWord.Replace(".", ".`")}`";

                if (ContainsIgnoreCase(sqlExecuted, backtickTargetWord))
                {
                    isKeywordNotFound = false;
                    return backtickTargetWord;
                }
            }

            return targetWord;
        }

        private bool ShouldDeferTargetWordToQueryVariableMapping(string targetWord)
        {
            return !string.IsNullOrEmpty(_queryTextParametersPositionMapping) && !string.IsNullOrEmpty(targetWord);
        }

        private SqlErrorTargetWordResult CreateTargetWordResult(string errorMessage, string targetWord, string positionText, string sqlExecuted)
        {
            var result = new SqlErrorTargetWordResult
            {
                ErrorMessage = errorMessage ?? string.Empty,
                TargetWord = targetWord ?? string.Empty,
                PositionText = positionText ?? string.Empty
            };

            if (ShouldDeferTargetWordToQueryVariableMapping(result.TargetWord))
            {
                result.TargetWordVariables = result.TargetWord;
                result.TargetWord = string.Empty;

                if (result.PositionText == "1")
                {
                    result.PositionText = "0";
                }

                return result;
            }

            if (result.PositionText == "0" && !string.IsNullOrEmpty(result.TargetWord))
            {
                result.PositionText = IndexOfIgnoreCase(sqlExecuted, result.TargetWord).ToString();
            }

            return result;
        }

        private SqlErrorTargetWordResult ResolveMySqlSqlExecuteErrorTargetWord(string errorMessage, string sqlExecuted, string positionText)
        {
            var normalizedErrorMessage = NormalizeDottedDoubleQuotedNameInErrorMessage(errorMessage);
            var targetWord = string.Empty;
            var isKeywordNotFound = false;

            if (TextHelper.CheckTextStartWithAndContains(normalizedErrorMessage, "Unknown column '", " in 'where clause'"))
            {
                var tempErrorMessage = normalizedErrorMessage.Replace(" in 'where clause'", string.Empty);

                if (TryExtractLastSingleQuotedText(tempErrorMessage, out var quoted))
                {
                    targetWord = quoted.Text;
                }
            }
            else if (TextHelper.CheckTextStartWithAndContains(normalizedErrorMessage, "Column '", "' in where clause is ambiguous"))
            {
                if (TryExtractLastSingleQuotedText(normalizedErrorMessage, out var quoted))
                {
                    targetWord = quoted.Text;
                }
            }
            else if (normalizedErrorMessage.StartsWith("Unknown column '", StringComparison.Ordinal))
            {
                if (TryExtractFirstSingleQuotedText(normalizedErrorMessage, out var quoted))
                {
                    targetWord = quoted.Text;
                }
            }
            else if (HasAtLeastTwoDoubleQuotes(normalizedErrorMessage) && TryExtractFirstDoubleQuotedText(normalizedErrorMessage, out var doubleQuoted))
            {
                targetWord = doubleQuoted.Text;
            }
            else if (HasAtLeastTwoSingleQuotes(normalizedErrorMessage))
            {
                if (TextHelper.CheckTextStartWithAndContains(normalizedErrorMessage, "Table", "doesn't exist"))
                {
                    var tempErrorMessage = normalizedErrorMessage.Replace("doesn't exist", string.Empty);

                    if (TryExtractLastSingleQuotedText(tempErrorMessage, out var quoted))
                    {
                        targetWord = quoted.Text;
                    }
                }
                else if (TryExtractLastSingleQuotedText(normalizedErrorMessage, out var quoted))
                {
                    targetWord = quoted.Text;
                }
            }

            targetWord = TrimErrorTargetWordToFirstTokenIfNeeded(targetWord);
            targetWord = RemoveMySqlDatabasePrefixFromTargetWord(normalizedErrorMessage, targetWord);
            targetWord = TryApplyMySqlBacktickTargetWord(targetWord, sqlExecuted, out isKeywordNotFound);

            var result = CreateTargetWordResult(normalizedErrorMessage, targetWord, positionText, sqlExecuted);

            result.IsKeywordNotFound = isKeywordNotFound;

            return result;
        }

        private SqlErrorTargetWordResult ResolveSqlServerSqlExecuteErrorTargetWord(string errorMessage, string sqlExecuted, string editorSql, string positionText,
                                                                                   int positionOffset, int errorLine, bool hasSecondaryErrorMessage)
        {
            var normalizedErrorMessage = NormalizeDottedDoubleQuotedNameInErrorMessage(errorMessage);
            var targetWord = string.Empty;
            var isKeywordNotFound = false;

            if (hasSecondaryErrorMessage)
            {
                return CreateTargetWordResult(normalizedErrorMessage, targetWord, positionText, sqlExecuted);
            }

            if (TryExtractFirstDoubleQuotedText(normalizedErrorMessage, out var doubleQuoted))
            {
                targetWord = doubleQuoted.Text;
            }
            else if (TryExtractFirstSingleQuotedText(normalizedErrorMessage, out var singleQuoted))
            {
                targetWord = singleQuoted.Text;
            }

            if (string.IsNullOrEmpty(targetWord))
            {
                return CreateTargetWordResult(normalizedErrorMessage, targetWord, positionText, sqlExecuted);
            }

            if (sqlExecuted.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase) == -1)
            {
                isKeywordNotFound = true;
            }

            var result = CreateTargetWordResult(normalizedErrorMessage, targetWord, positionText, sqlExecuted);

            result.IsKeywordNotFound = isKeywordNotFound;

            if (!isKeywordNotFound)
            {
                result.PositionText = ResolveSqlServerTargetWordPositionText
                (
                    sqlExecuted,
                    editorSql,
                    result.PositionText,
                    positionOffset,
                    errorLine,
                    targetWord
                );
            }

            return result;
        }

        private string ResolveSqlServerTargetWordPositionText(string sqlExecuted, string editorSql, string positionText, int positionOffset, int errorLine, string targetWord)
        {
            if (string.IsNullOrEmpty(targetWord))
            {
                return positionText;
            }

            int.TryParse(positionText, out var positionOriginal);

            var positionByErrorLine = TryResolveSqlServerTargetWordPositionByErrorLine
            (
                sqlExecuted,
                positionOriginal,
                errorLine,
                targetWord
            );

            if (positionByErrorLine.HasValue)
            {
                return positionByErrorLine.Value.ToString();
            }

            var positionByEditorSql = TryResolveSqlServerTargetWordPositionByEditorSql
            (
                editorSql,
                positionOriginal,
                positionOffset,
                targetWord
            );

            if (positionByEditorSql.HasValue)
            {
                return positionByEditorSql.Value.ToString();
            }

            return positionText;
        }

        private int? TryResolveSqlServerTargetWordPositionByErrorLine(string sqlExecuted, int positionOriginal, int errorLine, string targetWord)
        {
            if (string.IsNullOrEmpty(sqlExecuted) || string.IsNullOrEmpty(targetWord))
            {
                return null;
            }

            var lines = sqlExecuted.Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (errorLine <= 0 || errorLine > lines.Length)
            {
                return null;
            }

            var targetLine = lines[errorLine - 1];
            var positionInLine = targetLine.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase);

            if (positionInLine < 0)
            {
                return null;
            }

            var position = positionOriginal;

            for (var i = 0; i < lines.Length; i++)
            {
                if (i == errorLine - 1)
                {
                    position += positionInLine;
                    break;
                }

                position += lines[i].Length + 2;
            }

            return position + 1;
        }

        private int? TryResolveSqlServerTargetWordPositionByEditorSql(string editorSql, int positionOriginal, int positionOffset, string targetWord)
        {
            if (string.IsNullOrEmpty(editorSql) || string.IsNullOrEmpty(targetWord))
            {
                return null;
            }

            var start = positionOriginal + positionOffset;

            if (start < 0)
            {
                return null;
            }

            for (var i = start; i < editorSql.Length; i++)
            {
                if (TextHelper.GetSafeSubstring(editorSql, i, 1) != TextHelper.GetSafeSubstring(targetWord, 0, 1) || TextHelper.GetSafeSubstring(editorSql, i, targetWord.Length) != targetWord)
                {
                    continue;
                }

                return i + 1;
            }

            return null;
        }
    }
}