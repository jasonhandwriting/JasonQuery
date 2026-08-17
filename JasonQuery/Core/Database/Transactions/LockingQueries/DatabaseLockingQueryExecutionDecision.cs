namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal sealed class DatabaseLockingQueryExecutionDecision
    {
        public DatabaseLockingQueryDetectionResult Detection { get; set; } = DatabaseLockingQueryDetectionResult.NotDetected();

        public bool IsLockingQuery
        {
            get
            {
                return Detection?.IsLockingQuery ?? false;
            }
        }

        public bool RequiresConfirmation { get; set; }

        public bool ShouldUsePagedExecution { get; set; }

        public bool ShouldUseSinglePassReader { get; set; }

        public bool ShouldKeepConnectionOpenAfterSuccess { get; set; }

        public bool ShouldMarkPendingAfterSuccess { get; set; }
    }
}