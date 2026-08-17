using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.SqlServer;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using System;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HandleSqlExecuteErrorPosition_SqlServer(string messageInfo, int positionOffset)
        {
            var errorInfo = ParseSqlExecuteErrorInfo(messageInfo);

            if (_queryStatus == "Cancel")
            {
                ShowSqlExecuteCancelMessage(errorInfo);
                return;
            }

            if (string.IsNullOrEmpty(errorInfo.ExecutedSql))
            {
                ShowSqlExecuteSimpleErrorMessage(errorInfo);
                return;
            }

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = editor.Text,
                    OriginalExecutedSql = _sqlWhenError,
                    ExecutedSql = errorInfo.ExecutedSql,
                    ErrorCode = errorInfo.ErrorCode,
                    ErrorMessage = errorInfo.ErrorMessage,
                    SecondaryErrorMessage = errorInfo.SecondaryErrorMessage,
                    ReportedPosition = errorInfo.Position,
                    PositionOffset = positionOffset,
                    PreferredExecutionStart = _queryTextParametersStart,
                    ParameterPositionMapping = _queryTextParametersPositionMapping,
                    ParameterStartPosition = _queryTextParametersStart
                }
            );

            ApplySqlServerErrorPositionResult(result);

            var pointer = BuildSqlErrorPointer
            (
                new SqlErrorPointerContext
                {
                    Sql = editor.Text,
                    Position = result.PositionResult.Position,
                    PositionOffset = 0,
                    TargetWord = result.PositionResult.TargetText
                }
            );

            var errorMessage = BuildSqlServerResolvedErrorMessage(result);
            var errorMessagePrefix = result.UsedSecondaryErrorMessage ? string.Empty : "ErrorMsg: ";

            var message = BuildSqlExecuteDetailedErrorMessage
            (
                errorInfo.ExecutedResult,
                errorInfo.ErrorCode,
                errorMessage,
                errorInfo.ErrorHint,
                pointer,
                errorMessagePrefix: errorMessagePrefix
            );

            if (!ShowSqlExecuteDetailedErrorMessage(message, errorMessage))
            {
                return;
            }

            if (string.IsNullOrEmpty(DatabaseSqlExecutor.DatabaseName))
            {
                ShowMySqlSelectDatabaseFirstMessage();
            }
        }

        private void ApplySqlServerErrorPositionResult(SqlServerErrorPositionResolution result)
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

        private string BuildSqlServerResolvedErrorMessage(SqlServerErrorPositionResolution result)
        {
            var errorMessage = result?.ErrorMessage ?? string.Empty;

            if (result == null || !result.ShouldAppendStringConcatenationHint)
            {
                return errorMessage;
            }

            var concatMessage = LocalizationHelper.GetLanguageString
            (
                "In SQL Server, you should use + for string concatenation.",
                "Global",
                "Global",
                "msg",
                "SqlStringConcatenation",
                "Text"
            );

            return $"{errorMessage}\r\n{concatMessage}";
        }
    }
}