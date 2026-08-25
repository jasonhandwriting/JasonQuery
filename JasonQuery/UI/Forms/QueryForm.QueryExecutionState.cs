using JasonLibrary.Core;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Transactions;
using JasonQuery.Display.Columns;
using System;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool _hadPendingTransactionBeforeExecution;

        private void InitializeQueryExecutionState() //20260501 重構：查詢前狀態初始化, Column display context 記錄
        {
            _hadPendingTransactionBeforeExecution = AppConfigHelper.IsNotCommitYet;

            ResetLockingQueryExecutionState();
            btnCancelQuery.Tag = string.Empty;

            MyGlobal.ClearMemory(); //20250913 執行 SQL 前，判斷是否要 GC Collect (避免上一次查詢殘留大量查詢結果)

            _columnInfoCollector = null;
            _currentQueryResultHasColumnComments = false;
            _isNextQueryResultColumnNameSortAscending = true; //每次新查詢後，第一次按 Auto Sort 一律正向排序

            SetFormStatusBarInfo(string.Empty, Color.Black);
            SetEditorStatusBarInfo(string.Empty, Color.Black);

            //判斷 SQL 是否為查詢指令
            btnQuery.AccessibleDescription = string.Empty;

            CaptureCurrentColumnDisplayContext();
        }

        private void CaptureCurrentColumnDisplayContext()
        {
            ColumnMode mode = ColumnMode.None;

            if (chkShowColumnType.Checked)
            {
                mode |= ColumnMode.ShowColumnType;
            }

            if (chkShowColumnComments.Checked)
            {
                mode |= ColumnMode.ShowColumnComment;
            }

            _currentDisplayContext = new ColumnDisplayContext(mode, 0);
        }

        private void PrepareQueryExecutionBaseState(string queryText, bool isNextPage)
        {
            btnNextPage.Enabled = false;

            if (!isNextPage)
            {
                lblRows.Text = $"0 {_rows}";
            }

            btnQuery.Tag = queryText;
            lblQueryTime.Tag = "00:00:00";
        }

        private bool TryHandleCancelledNonQueryCompletion(string cancelQueryTag)
        {
            var cancellationRequested = string.Equals(cancelQueryTag, "CANCEL", StringComparison.OrdinalIgnoreCase);
            var wasNonQuery = string.Equals(btnQuery.AccessibleDescription, "NonQuery", StringComparison.OrdinalIgnoreCase);

            var decision = CancelledNonQueryCompletionPolicy.Evaluate
            (
                cancellationRequested,
                wasNonQuery,
                _hadPendingTransactionBeforeExecution,
                rollbackSucceeded: null
            );

            if (!decision.IsHandled)
            {
                return false;
            }

            //避免後段舊的 NonQuery fallback 再把取消操作視為成功 DML
            btnQuery.AccessibleDescription = string.Empty;

            DatabaseTransactionActionResult rollbackResult = null;

            if (decision.ShouldAttemptRollback)
            {
                //本次取消前沒有待確認異動。明確 Rollback 並中斷連線，讓 provider 的 transaction object 與 JasonQuery 的 UI 狀態一致結束
                rollbackResult = ExecuteTransactionAction(DatabaseTransactionActionKind.Rollback);

                decision = CancelledNonQueryCompletionPolicy.Evaluate
                (
                    cancellationRequested,
                    wasNonQuery,
                    _hadPendingTransactionBeforeExecution,
                    rollbackResult?.TransactionSucceeded == true
                );
            }

            if (decision.ShouldClearPendingState)
            {
                ApplyCommitRollbackButtonState(false);
                TransferValueToMainForm("ExecuteCommitRollback`");

                return true;
            }

            if (decision.ShouldKeepPendingState)
            {
                //取消前已有 pending，或 Rollback 結果無法確認時採安全優先：保留 pending，要求使用者再次處理
                ApplyCommitRollbackButtonState(true);
                TransferValueToMainForm("UpdateCommitRollbackButton`");

                if (rollbackResult != null && !string.IsNullOrWhiteSpace(rollbackResult.Message))
                {
                    var currentMessage = (editorMessage.Text ?? string.Empty).TrimEnd('\r', '\n');
                    var separator = string.IsNullOrEmpty(currentMessage) ? string.Empty : "\r\n";

                    UpdateMessage($"{currentMessage}{separator}{rollbackResult.Message}");
                }
            }

            return true;
        }
    }
}
