using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Transactions.LockingQueries;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private DatabaseLockingQueryDetectionResult _activeLockingQueryDetection = DatabaseLockingQueryDetectionResult.NotDetected();

        private bool _lockingQueryExecutionStarted;
        private bool _preventAutomaticDisconnectForCompletedQuery;

        private bool ConfirmLockingQueryExecutionIfNeeded(
            QueryExecutionKindResult queryKind)
        {
            if (queryKind == null || !queryKind.IsLockingQuery)
            {
                return true;
            }

            var message = LocalizationHelper.GetLanguageString
            (
                "This query requests database locks.\r\n\r\nJasonQuery will:\r\n• execute the original SQL without automatic paging;\r\n• keep the transaction and connection open;\r\n• require you to Commit or Rollback after execution.\r\n\r\nReview the WHERE condition carefully before continuing.",
                "form",
                GetType().Name,
                "msg",
                "LockingQueryExecutionWarning",
                "Text"
            );

            if (!string.IsNullOrWhiteSpace(queryKind.LockingQueryDetection?.MatchedText))
            {
                var detectedText = LocalizationHelper.GetLanguageString
                (
                    "Detected locking clause:",
                    "form",
                    GetType().Name,
                    "msg",
                    "DetectedLockingClause",
                    "Text"
                );

                message += $"\r\n\r\n{detectedText} " + queryKind.LockingQueryDetection.MatchedText;
            }

            var caption = LocalizationHelper.GetLanguageString
            (
                "Locking query",
                "form",
                GetType().Name,
                "msg",
                "LockingQuery",
                "Text"
            );

            var result = MessageBox.Show
            (
                message,
                caption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (result != DialogResult.Yes)
            {
                ResetLockingQueryExecutionState();
                return false;
            }

            _activeLockingQueryDetection = queryKind.LockingQueryDetection ?? DatabaseLockingQueryDetectionResult.NotDetected();

            return true;
        }

        private void MarkLockingQueryExecutionStarted(
            QueryExecutionKindResult queryKind)
        {
            if (queryKind == null || !queryKind.IsLockingQuery)
            {
                return;
            }

            _activeLockingQueryDetection = queryKind.LockingQueryDetection ?? DatabaseLockingQueryDetectionResult.NotDetected();

            _lockingQueryExecutionStarted = true;
        }

        private void ApplyLockingQueryCompletionState(
            QueryResultLoadResult loadResult)
        {
            if (!_activeLockingQueryDetection.IsLockingQuery)
            {
                return;
            }

            var cancellationRequested = string.Equals(TextHelper.GetSafeString(btnCancelQuery.Tag), "Cancel", StringComparison.OrdinalIgnoreCase);
            var querySucceeded = loadResult != null && !loadResult.HasError && !cancellationRequested;

            var decision = DatabaseLockingQueryCompletionPolicy.Evaluate
                           (
                               isLockingQuery: true,
                               executionStarted: _lockingQueryExecutionStarted,
                               querySucceeded: querySucceeded,
                               cancellationRequested: cancellationRequested,
                               hadPendingTransactionBeforeExecution: AppConfigHelper.IsNotCommitYet
                           );

            _preventAutomaticDisconnectForCompletedQuery = decision.ShouldPreventAutomaticDisconnect;

            if (decision.ShouldDisablePaging)
            {
                btnNextPage.Enabled = false;
            }

            if (decision.ShouldMarkPendingState)
            {
                //Apply immediately in this QueryForm so the completion timer cannot take the ordinary QUERY -> Disconnect path before the MainForm broadcast reaches every editor.
                ApplyCommitRollbackButtonState(true);
                TransferValueToMainForm("UpdateCommitRollbackButton`");
            }

            if (!decision.ShouldShowCompletionWarning)
            {
                return;
            }

            lblInfo.Text = GetLockingQueryCompletionMessage(decision.CompletionKind);
        }

        private string GetLockingQueryCompletionMessage(DatabaseLockingQueryCompletionKind completionKind)
        {
            if (completionKind == DatabaseLockingQueryCompletionKind.CancelledAfterExecutionStarted)
            {
                return LocalizationHelper.GetLanguageString
                (
                    "The locking query was cancelled after execution started. The transaction state may be uncertain. Commit or Rollback is required.",
                    "form",
                    GetType().Name,
                    "msg",
                    "LockingQueryCancelledPending",
                    "Text"
                );
            }

            return LocalizationHelper.GetLanguageString
            (
                "The locking query completed. The transaction remains open. Commit or Rollback is required.",
                "form",
                GetType().Name,
                "msg",
                "LockingQueryCompletedPending",
                "Text"
            );
        }

        private bool ShouldPreventAutomaticDisconnectForCompletedQuery()
        {
            return _preventAutomaticDisconnectForCompletedQuery;
        }

        private void ResetLockingQueryExecutionState()
        {
            _activeLockingQueryDetection = DatabaseLockingQueryDetectionResult.NotDetected();
            _lockingQueryExecutionStarted = false;
            _preventAutomaticDisconnectForCompletedQuery = false;
        }
    }
}