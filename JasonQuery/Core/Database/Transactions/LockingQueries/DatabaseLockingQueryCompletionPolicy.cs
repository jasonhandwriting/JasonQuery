namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal static class DatabaseLockingQueryCompletionPolicy
    {
        public static DatabaseLockingQueryCompletionDecision Evaluate(bool isLockingQuery, bool executionStarted, bool querySucceeded,
                                                                      bool cancellationRequested, bool hadPendingTransactionBeforeExecution)
        {
            if (!isLockingQuery)
            {
                return CreateDecision(DatabaseLockingQueryCompletionKind.NotLockingQuery, hadPendingTransactionBeforeExecution,
                                      hadPendingTransactionBeforeExecution, false, false);
            }

            if (!executionStarted)
            {
                return CreateDecision(DatabaseLockingQueryCompletionKind.CancelledBeforeExecution, hadPendingTransactionBeforeExecution,
                                      hadPendingTransactionBeforeExecution, false, false);
            }

            if (cancellationRequested)
            {
                //Once the command has started, cancellation may arrive after the database has already acquired locks. Keep the transaction open and require an explicit Commit or Rollback.
                return CreateDecision(DatabaseLockingQueryCompletionKind.CancelledAfterExecutionStarted, true, true, true, true);
            }

            if (querySucceeded)
            {
                return CreateDecision(DatabaseLockingQueryCompletionKind.CompletedSuccessfully, true, true, true, true);
            }

            //A failed locking statement does not by itself prove that a new lock remains active. Preserve an existing pending transaction, but do not invent a new pending state solely from the failed statement.
            return CreateDecision(DatabaseLockingQueryCompletionKind.Failed, hadPendingTransactionBeforeExecution, hadPendingTransactionBeforeExecution,
                                  false, hadPendingTransactionBeforeExecution);
        }

        private static DatabaseLockingQueryCompletionDecision CreateDecision(DatabaseLockingQueryCompletionKind completionKind, bool shouldMarkPendingState, bool shouldKeepConnectionOpen,
                                                                             bool shouldShowCompletionWarning, bool shouldDisablePaging)
        {
            return new DatabaseLockingQueryCompletionDecision
            {
                CompletionKind = completionKind,
                ShouldMarkPendingState = shouldMarkPendingState,
                ShouldKeepConnectionOpen = shouldKeepConnectionOpen,
                ShouldPreventAutomaticDisconnect = shouldKeepConnectionOpen,
                ShouldDisablePaging = shouldDisablePaging,
                ShouldShowCompletionWarning = shouldShowCompletionWarning
            };
        }
    }
}