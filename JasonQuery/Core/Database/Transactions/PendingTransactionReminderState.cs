using System;

namespace JasonQuery.Core.Database.Transactions
{
    internal sealed class PendingTransactionReminderState
    {
        public bool IsPending { get; set; }

        public DateTime? PendingSince { get; set; }

        public DateTime? NextWarningTime { get; set; }
    }
}
