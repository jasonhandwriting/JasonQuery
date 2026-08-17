using JasonQuery.Core.Database.Diagnostics.MySql;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HandleSqlExecuteErrorPosition_MySql(string messageInfo, int positionOffset)
        {
            var errorInfo = ParseSqlExecuteErrorInfo(messageInfo);

            if (_queryStatus == "Cancel")
            {
                ShowSqlExecuteCancelMessage(errorInfo);
                return;
            }

            if (string.IsNullOrEmpty(errorInfo.ExecutedSql)) //errorInfo.ExecutedSql 為空，表示錯誤不是在 ExecuteQuery() 攔截到的，不需要處理
            {
                ShowSqlExecuteSimpleErrorMessage(errorInfo);
                return;
            }

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = editor.Text,
                    OriginalExecutedSql = _sqlWhenError,
                    ExecutedSql = errorInfo.ExecutedSql,
                    ErrorCode = errorInfo.ErrorCode,
                    ErrorMessage = errorInfo.ErrorMessage,
                    ReportedPosition = errorInfo.Position,
                    PositionOffset = positionOffset,
                    PreferredExecutionStart = _queryTextParametersStart,
                    ParameterPositionMapping = _queryTextParametersPositionMapping,
                    ParameterStartPosition = _queryTextParametersStart,
                    CurrentDatabaseName = DatabaseSqlExecutor.DatabaseName
                }
            );

            ApplyMySqlErrorPositionResult(result);

            var pointer = BuildSqlErrorPointer
            (
                new SqlErrorPointerContext
                {
                    Sql = editor.Text,
                    Position = result.PositionResult.Position,
                    PositionOffset = 0,
                    TargetWord = result.PositionResult.TargetText,
                    ClampWordLengthToLine = true
                }
            );

            var message = BuildSqlExecuteDetailedErrorMessage
            (
                errorInfo.ExecutedResult,
                errorInfo.ErrorCode,
                result.ErrorMessage,
                errorInfo.ErrorHint,
                pointer
            );

            if (!ShowSqlExecuteDetailedErrorMessage(message, result.ErrorMessage))
            {
                return;
            }

            if (result.ShouldShowSelectDatabaseFirstMessage)
            {
                ShowMySqlSelectDatabaseFirstMessage();
            }
        }

        private void ApplyMySqlErrorPositionResult(MySqlErrorPositionResolution result)
        {
            var positionResult = result?.PositionResult;

            if (positionResult == null || !positionResult.Found)
            {
                return;
            }

            if (positionResult.ShouldSetSquiggle && positionResult.Length > 0)
            {
                SetSquiggle
                (
                    false,
                    positionResult.Position,
                    positionResult.Length
                );

                editor.SelectionStart =
                    positionResult.Position
                    + positionResult.Length;
            }
            else
            {
                editor.SelectionStart = positionResult.Position;
            }

            editor.CurrentPosition = positionResult.Position;
            editor.ScrollCaret();
        }

        private void ShowMySqlSelectDatabaseFirstMessage()
        {
            var message = LocalizationHelper.GetLanguageString
            (
                "Please use the USE statement to select a particular database first.",
                "form",
                GetType().Name,
                "msg",
                "SelectDatabaseFirst",
                "Text"
            );

            SetEditorStatusBarInfo(message, Color.DarkRed);
        }
    }
}