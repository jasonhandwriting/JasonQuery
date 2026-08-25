//負責 Message Tab 顯示與詳細錯誤訊息組裝

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private const string SqlExecuteMessageTagError = "error";
        private const string SqlExecuteMessageTagCancel = "cancel";

        private void ShowSqlExecuteCancelMessage(SqlExecuteErrorInfo errorInfo)
        {
            ApplySqlExecuteMessageToMessageTab
            (
                BuildSqlExecuteCancelMessage(errorInfo),
                SqlExecuteMessageTagCancel
            );
        }

        private void ShowSqlExecuteSimpleErrorMessage(SqlExecuteErrorInfo errorInfo)
        {
            ApplySqlExecuteMessageToMessageTab
            (
                BuildSqlExecuteSimpleErrorMessage(errorInfo),
                SqlExecuteMessageTagError
            );
        }

        private bool ShowSqlExecuteDetailedErrorMessage(string message, string errorMessage)
        {
            UpdateMessage(message);

            if (string.IsNullOrEmpty(errorMessage))
            {
                return false;
            }

            SelectSqlExecuteMessageTab(SqlExecuteMessageTagError);
            return true;
        }

        private void ApplySqlExecuteMessageToMessageTab(string message, string tag)
        {
            UpdateMessage(message);
            SelectSqlExecuteMessageTab(tag);
        }

        private void SelectSqlExecuteMessageTab(string tag)
        {
            c1DockingTab1.SelectedTab = tabMessage;
            editorMessage.Tag = tag;
        }

        private string BuildSqlExecuteCancelMessage(SqlExecuteErrorInfo errorInfo)
        {
            if (errorInfo == null)
            {
                return "ErrorMsg: ";
            }

            return $"{errorInfo.ErrorCode}ErrorMsg: {errorInfo.ErrorMessage}";
        }

        private string BuildSqlExecuteSimpleErrorMessage(SqlExecuteErrorInfo errorInfo)
        {
            if (errorInfo == null)
            {
                return "ErrorMsg: ";
            }

            return $"ErrorMsg: {errorInfo.ErrorMessage}";
        }

        private string BuildSqlExecuteDetailedErrorMessage(string executedResult, string errorCode, string errorMessage, string errorHint, string pointerText,
                                                           string errorMessagePrefix = "ErrorMsg: ", string errorMessageExtra = "", bool includeQueryTextParametersMapping = true)
        {
            var executedResultText = string.IsNullOrEmpty(executedResult) ? string.Empty : $"{executedResult}\r\n";

            var queryTextParametersMappingText = includeQueryTextParametersMapping && !string.IsNullOrEmpty(_queryTextParametersMapping)
                                                 ? $"\r\n\r\n{_queryTextParametersMapping}"
                                                 : string.Empty;

            return $"{executedResultText}{errorCode}{errorMessagePrefix}{errorMessage}{errorMessageExtra}{errorHint}{pointerText}{queryTextParametersMappingText}";
        }
    }
}
