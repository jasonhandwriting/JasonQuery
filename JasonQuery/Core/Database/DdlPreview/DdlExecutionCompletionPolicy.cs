namespace JasonQuery.Core.Database.DdlPreview
{
    internal static class DdlExecutionCompletionPolicy
    {
        public static DdlExecutionCompletionDecision Evaluate(string errorMessage, bool hasPendingTransactionAfterExecute,
                                                              bool isTransactionClosedBySqlCommand)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return new DdlExecutionCompletionDecision
                {
                    Succeeded = false,
                    NotificationKind = DdlMainFormNotificationKind.None
                };
            }

            var notificationKind = DdlMainFormNotificationKind.None;

            if (hasPendingTransactionAfterExecute)
            {
                notificationKind = DdlMainFormNotificationKind.PendingTransaction;
            }
            else if (isTransactionClosedBySqlCommand)
            {
                notificationKind = DdlMainFormNotificationKind.TransactionClosed;
            }

            return new DdlExecutionCompletionDecision
            {
                Succeeded = true,
                NotificationKind = notificationKind
            };
        }
    }
}