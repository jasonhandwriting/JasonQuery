namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal sealed class DatabaseLockingQueryCompletionDecision
    {
        public DatabaseLockingQueryCompletionKind CompletionKind { get; set; }

        public bool ShouldMarkPendingState { get; set; }

        public bool ShouldKeepConnectionOpen { get; set; }

        public bool ShouldPreventAutomaticDisconnect { get; set; }

        public bool ShouldDisablePaging { get; set; }

        public bool ShouldShowCompletionWarning { get; set; }
    }
}