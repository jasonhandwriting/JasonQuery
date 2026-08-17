namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal enum DatabaseLockingQueryCompletionKind
    {
        NotLockingQuery = 0,
        CancelledBeforeExecution = 1,
        CompletedSuccessfully = 2,
        Failed = 3,
        CancelledAfterExecutionStarted = 4
    }
}