using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Text;
using System;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HandleSqlExecuteErrorPosition_PostgreSql(string messageInfo, int positionOffset)
        {
            var temp = string.Empty;
            var tempWord = string.Empty;
            var tempWordVariables = string.Empty;
            var isKeywordNotFound = false;
            var errorInfo = ParseSqlExecuteErrorInfo(messageInfo);
            var executedResult = errorInfo.ExecutedResult;
            var errorCode = errorInfo.ErrorCode;
            var errorMessage = errorInfo.ErrorMessage;
            var errorHint = errorInfo.ErrorHint;
            var position = errorInfo.PositionText;
            var sqlExecuted = errorInfo.ExecutedSql;
            var sql = editor.Text;
            var positionOriginal = errorInfo.Position;
            var specialLength = 0;

            if (_queryStatus == "Cancel")
            {
                ShowSqlExecuteCancelMessage(errorInfo);
                return;
            }

            //202410123 錯誤的定位點若往前抓，就要調整這個地方 (判斷 "permission denied for" 開頭)
            if (errorMessage.StartsWith("permission denied for", StringComparison.OrdinalIgnoreCase) || errorMessage.StartsWith("current transaction is aborted, commands ignored", StringComparison.OrdinalIgnoreCase))
            {
                positionOffset = 0;
            }

            if (string.IsNullOrEmpty(sqlExecuted)) //如果 sqlExecuted 為空，表示錯誤不是在 ExecuteQuery() 攔截到的，不需要處理
            {
                ShowSqlExecuteSimpleErrorMessage(errorInfo);
            }
            else
            {
                if (errorCode.IndexOf("22P02", StringComparison.Ordinal) >= 0)
                {
                    specialLength = 2; //前後有單引號，所以 +2
                }

                //其中一種情況是，"aa"."name"→aa.name
                errorMessage = NormalizeDottedDoubleQuotedNameInErrorMessage(errorMessage);

                var from = 0;
                var to = 0;

                if (TryExtractFirstDoubleQuotedText(errorMessage, out var doubleQuoted))
                {
                    from = doubleQuoted.From;
                    to = doubleQuoted.To;

                    //錯誤訊息有明確指出哪個字串
                    tempWord = doubleQuoted.Text;

                    //判斷 tempWord 是否有出現在執行的 SQL 裡面
                    if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        isKeywordNotFound = true;
                    }
                }
                else if (TryExtractLastSingleQuotedText(errorMessage, out var singleQuoted))
                {
                    from = singleQuoted.From;
                    to = singleQuoted.To;

                    //錯誤訊息有明確指出哪個字串
                    tempWord = singleQuoted.Text;

                    var temp2 = errorMessage.ToUpper()
                                      .Replace("PARAMETER '", string.Empty)
                                      .Replace("' IS MISSING", string.Empty);

                    if (string.Equals(temp2, tempWord, StringComparison.OrdinalIgnoreCase))
                    {
                        tempWord = $":{tempWord}";
                    }

                    //判斷 tempWord 是否有出現在執行的 SQL 裡面
                    if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        isKeywordNotFound = true;
                    }
                }
                else if (errorMessage.StartsWith("current transaction is aborted, commands ignored", StringComparison.OrdinalIgnoreCase))
                {
                    tempWordVariables = string.Empty;
                    tempWord = string.Empty;
                }
                else if (string.IsNullOrEmpty(_queryTextParametersPositionMapping)
                         && PostgreSqlErrorTargetResolver.TryResolvePermissionDeniedTarget(sql, sqlExecuted, errorCode, errorMessage, _queryTextParametersStart, out var permissionDeniedPosition, out var permissionDeniedTarget))
                {
                    //20260711 針對錯誤訊息為 "permission denied for xxx"，解析出定位位置與目標字串
                    positionOriginal = permissionDeniedPosition;
                    positionOffset = 0;
                    tempWord = permissionDeniedTarget;
                    specialLength = 0;
                    isKeywordNotFound = false;
                }
                else if (string.IsNullOrEmpty(_queryTextParametersPositionMapping)
                         && PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget(sql, sqlExecuted, errorCode, errorMessage, _queryTextParametersStart, out var mustBeOwnerPosition, out var mustBeOwnerTarget))
                {
                    //20260711 針對錯誤訊息為 "must be owner of xxx"，解析出定位位置與目標字串
                    positionOriginal = mustBeOwnerPosition;
                    positionOffset = 0;
                    tempWord = mustBeOwnerTarget;
                    specialLength = 0;
                    isKeywordNotFound = false;
                }
                else
                {
                    isKeywordNotFound = true;

                    //20250907 錯誤訊息是否為 "column xxxxxxx does not exist" 格式
                    var fromNew = errorMessage.IndexOf("column ", StringComparison.OrdinalIgnoreCase);
                    var toNew = errorMessage.IndexOf(" does not exist", StringComparison.OrdinalIgnoreCase);

                    if (fromNew >= 0 && toNew > 7)
                    {
                        //取得中間的欄位名稱
                        tempWord = TextHelper.GetSafeSubstring(errorMessage, fromNew + 7, toNew - 7);

                        //判斷 tempWord 是否有出現在執行的 SQL 裡面
                        if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            isKeywordNotFound = false;
                        }
                        else
                        {
                            tempWord = string.Empty;
                        }
                    }
                    else
                    {
                        fromNew = errorMessage.IndexOf("function ", StringComparison.OrdinalIgnoreCase);
                        toNew = errorMessage.IndexOf(" does not exist", StringComparison.OrdinalIgnoreCase);

                        if (fromNew >= 0 && toNew > 9)
                        {
                            //取得 Function 名稱
                            tempWord = TextHelper.GetStringBetween2(errorMessage, "function ", "(", true);

                            //判斷 tempWord 是否有出現在執行的 SQL 裡面
                            if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                isKeywordNotFound = false;
                            }
                            else
                            {
                                tempWord = string.Empty;
                            }
                        }
                        else if (errorCode.IndexOf("42703", StringComparison.Ordinal) >= 0) //20250930 "column xxxxxxx does not exist"，但因為語系關係，顯示的不是英文
                        {
                            //取得欄位名稱
                            tempWord = TextHelper.GetStringBetween2(errorMessage, " ", " ", true);

                            //判斷 tempWord 是否有出現在執行的 SQL 裡面
                            if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                isKeywordNotFound = false;
                            }
                            else
                            {
                                tempWord = string.Empty;
                            }
                        }
                        else if (errorCode.IndexOf("42883", StringComparison.Ordinal) >= 0) //20250930 "function xxxxxxx does not exist"，但因為語系關係，顯示的不是英文
                        {
                            //取得 Function 名稱
                            tempWord = TextHelper.GetStringBetween2(errorMessage, " ", " ", true);

                            var tempValue = tempWord.IndexOf("(", StringComparison.Ordinal);

                            if (tempValue > 0)
                            {
                                tempWord = tempWord.Substring(0, tempValue);
                            }

                            //判斷 tempWord 是否有出現在執行的 SQL 裡面
                            if (sqlExecuted.IndexOf(tempWord, StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                isKeywordNotFound = false;
                            }
                            else
                            {
                                tempWord = string.Empty;
                            }
                        }
                    }

                    if (isKeywordNotFound && positionOriginal + positionOffset >= 0)
                    {
                        //錯誤訊息並未明確指出哪個字串，所以從「OffSet」指示的「位置」找出這個字串
                        var startIndex = positionOriginal + positionOffset;
                        var sbTempWord = new StringBuilder();

                        for (var i = startIndex; i < sql.Length; i++)
                        {
                            char c = sql[i];

                            //如果下一個字元符合以下條件，則不再往下找
                            if (c == ' ' || c == '\r' || c == '\n' || c == '\'' || c == '\"' || c == ')' || c == ':' || (!string.IsNullOrEmpty(tempWord) && c == '='))
                            {
                                break;
                            }

                            sbTempWord.Append(c);
                        }

                        tempWord = sbTempWord.ToString();

                        //20260605 雖然錯誤訊息沒有明確指出關鍵字，但已經從 PostgreSQL 回報的 offset 位置成功推算出關鍵字
                        //         因此後續應該走一般定位流程，才能正確設定 Squiggle / SelectionStart / CurrentPosition
                        if (!string.IsNullOrEmpty(tempWord))
                        {
                            isKeywordNotFound = false;
                        }

                        if (!string.IsNullOrEmpty(_queryTextParametersPositionMapping) && !string.IsNullOrEmpty(tempWord))
                        {
                            tempWordVariables = tempWord;
                            tempWord = string.Empty;

                            //參數 mapping 情境下，tempWord 會先清空，後面會用 tempWordVariables 補回
                            //此時仍然應該視為已找到定位字串
                            isKeywordNotFound = false;
                        }
                    }
                }

                bool isTemp = errorMessage.StartsWith("ErrorCode: 25", StringComparison.Ordinal) || errorMessage.StartsWith("ErrorCode: 22", StringComparison.Ordinal);

                if (positionOriginal > 0 && tempWord.Length > 0 && isTemp)
                {
                    positionOriginal = editor.CurrentPosition;
                    tempWord = " ";
                }

                if (!string.IsNullOrEmpty(_queryTextParametersPositionMapping))
                {
                    specialLength = 0;

                    if (SqlParameterPositionMapper.TryMapExecutedSqlPositionToOriginalSqlPosition(_queryTextParametersPositionMapping, _queryTextParametersStart, positionOriginal,
                                                                                                  out var mappedPosition, out var mappedParameterName))
                    {
                        positionOriginal = mappedPosition;

                        if (!string.IsNullOrEmpty(mappedParameterName))
                        {
                            tempWord = mappedParameterName;
                        }
                    }
                }

                if (string.IsNullOrEmpty(_queryTextParametersPositionMapping)
                    && PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget(sql, errorCode, positionOriginal, positionOffset, tempWord, out var quotedIdentifierPosition, out var quotedIdentifier))
                {
                    positionOriginal = quotedIdentifierPosition;
                    positionOffset = 0;
                    tempWord = quotedIdentifier;
                    specialLength = 0;
                    isKeywordNotFound = false;
                }

                if (!isKeywordNotFound)
                {
                    if (positionOriginal + positionOffset >= 0)
                    {
                        if (!string.IsNullOrEmpty(_queryTextParametersPositionMapping) && string.IsNullOrEmpty(tempWord))
                        {
                            tempWord = tempWordVariables;
                        }

                        var keywordSubstring = TextHelper.GetSafeSubstring(sql, positionOriginal + positionOffset, tempWord.Length + specialLength);

                        if (string.Equals(keywordSubstring, tempWord, StringComparison.OrdinalIgnoreCase)) //錯誤定位OK
                        {
                            SetSquiggle(false, positionOriginal + positionOffset, tempWord.Length + specialLength);
                        }
                        else //有找到錯誤關鍵字，但 PostgreSQL 回報的定位 position 有問題，導致兩者不一致
                        {
                            if (TextHelper.GetSafeSubstring(sql, positionOriginal + positionOffset, 1) == "\n" && positionOffset == -1)
                            {
                                positionOffset = 0;
                            }

                            //20250720 PostgreSQL 回報的位置找不到關鍵字，再次檢查指定位置之後是否有出現關鍵字
                            var isResult = TextHelper.GetKeywordPosition(sql, string.Empty, tempWord, positionOriginal, out var resultPosition);

                            if (isResult)
                            {
                                positionOriginal = resultPosition;
                                SetSquiggle(false, resultPosition, tempWord.Length);
                            }
                            else
                            {
                                tempWord = string.Empty; //讓錯誤定位在 SQL 的最前面位置
                            }
                        }

                        editor.SelectionStart = positionOriginal + positionOffset + tempWord.Length + specialLength;
                        editor.CurrentPosition = positionOriginal + positionOffset;
                    }
                }
                else
                {
                    editor.CurrentPosition = positionOriginal;
                }

                editor.ScrollCaret();
                temp = string.Empty;

                #region 取得 Message 要呈現的文字 (^^^^ 指示線)
                temp = BuildSqlErrorPointer(new SqlErrorPointerContext
                {
                    Sql = sql,
                    Position = positionOriginal,
                    PositionOffset = positionOffset,
                    TargetWord = tempWord,
                    SpecialLength = specialLength
                });
                #endregion

                temp = BuildSqlExecuteDetailedErrorMessage(executedResult, errorCode, errorMessage, errorHint, temp);

                ShowSqlExecuteDetailedErrorMessage(temp, errorMessage);
            }

            if (!DatabaseSqlExecutor.UseAutoRollback)
            {
                return;
            }

            try
            {
                MyGlobal.PostgreSqlReader.Rollback();
                //20210531 待確認：是否要執行 UpdateNotCommitYetInfo("");
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
