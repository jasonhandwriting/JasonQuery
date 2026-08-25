using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.Oracle;
using JasonQuery.Core.Localization;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void HandleSqlExecuteErrorPosition_Oracle(string messageInfo, int positionOffset)
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

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = editor.Text,
                    OriginalExecutedSql = _sqlWhenError,
                    ExecutedSql = errorInfo.ExecutedSql,
                    ErrorCode = errorInfo.ErrorCode,
                    ErrorMessage = errorInfo.ErrorMessage,
                    ReportedPosition = errorInfo.Position + positionOffset,
                    PreferredExecutionStart = _queryTextParametersStart,
                    ParameterPositionMapping = _queryTextParametersPositionMapping,
                    ParameterStartPosition = _queryTextParametersStart
                }
            );

            ApplyOracleErrorPositionResult(result);

            var pointer = BuildSqlErrorPointer
            (
                new SqlErrorPointerContext
                {
                    Sql = editor.Text,
                    Position = result.Position,
                    PositionOffset = 0,
                    TargetWord = result.TargetText
                }
            );

            var message = BuildOracleSqlExecuteErrorMessage
            (
                errorInfo,
                result,
                pointer
            );

            if (!ShowSqlExecuteDetailedErrorMessage(message, errorInfo.ErrorMessage))
            {
                return;
            }

            if (errorInfo.ErrorMessage == "ORA-03106: fatal two-task communication protocol error")
            {
                MyGlobal.OracleReader.Disconnect();
            }
        }

        private void ApplyOracleErrorPositionResult(JasonQuery.Core.Database.Diagnostics.Common.SqlErrorResolutionResult result)
        {
            if (result == null || !result.Found)
            {
                return;
            }

            if (result.ShouldSetSquiggle && result.Length > 0)
            {
                SetSquiggle(false, result.Position, result.Length);
                editor.SelectionStart = result.Position + result.Length;
            }
            else
            {
                editor.SelectionStart = result.Position;
            }

            editor.CurrentPosition = result.Position;
            editor.ScrollCaret();
        }

        private string BuildOracleSqlExecuteErrorMessage(SqlExecuteErrorInfo errorInfo, JasonQuery.Core.Database.Diagnostics.Common.SqlErrorResolutionResult result, string pointer)
        {
            var extraMessage = string.Empty;

            if (errorInfo.ErrorMessage == "ORA-00932: inconsistent datatypes: expected - got CLOB"
                || errorInfo.ErrorMessage == "ORA-00932: inconsistent datatypes: expected - got NCLOB")
            {
                extraMessage = "\r\nYou can try: DBMS_LOB.SUBSTR(col, 4000, 1) OR TO_CHAR(col)";

                if (result != null && !string.IsNullOrEmpty(result.TargetText)
                    && !string.Equals(result.TargetText, "SUBSTR", StringComparison.OrdinalIgnoreCase))
                {
                    extraMessage = extraMessage.Replace("col", result.TargetText);
                }
            }
            else if (result != null && result.HasManyNonAsciiCharactersBeforePosition)
            {
                var text = "This SQL statement contains a lot of two-byte characters, which may affect the positioning effect of JasonQuery error guidance!";

                var localizedText = LocalizationHelper.GetLanguageString
                (
                    text,
                    "Global",
                    "Global",
                    "msg",
                    "TwoByteCharacters",
                    "Text"
                );

                extraMessage = $"\r\n{localizedText}";
            }

            return BuildSqlExecuteDetailedErrorMessage
            (
                errorInfo.ExecutedResult,
                errorInfo.ErrorCode,
                errorInfo.ErrorMessage,
                errorInfo.ErrorHint,
                pointer,
                errorMessageExtra: extraMessage
            );
        }
    }
}
