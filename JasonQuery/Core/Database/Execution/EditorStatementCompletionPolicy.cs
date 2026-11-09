using System;

namespace JasonQuery.Core.Database.Execution
{
    //F2-C1: A SQL command being prepared is not proof of a completed transaction.
    //The caller must provide an acknowledgement from the actual Reader execution path.
    internal static class EditorStatementCompletionPolicy
    {
        internal static StatementCompletionDecision Evaluate(bool readerAcknowledged, string result, string transactionState, bool cancellationRequested)
        {
            var completed = readerAcknowledged && !cancellationRequested && string.Equals(result, "Complete", StringComparison.OrdinalIgnoreCase);
            var transactionClosed = completed && string.Equals(transactionState, "TX_CLOSED", StringComparison.OrdinalIgnoreCase);

            return new StatementCompletionDecision
            {
                CanNotifyStatementCompleted = completed,
                CanNotifyTransactionClosed = transactionClosed
            };
        }
    }

    internal sealed class StatementCompletionDecision
    {
        public bool CanNotifyStatementCompleted { get; set; }
        public bool CanNotifyTransactionClosed { get; set; }
    }
}
