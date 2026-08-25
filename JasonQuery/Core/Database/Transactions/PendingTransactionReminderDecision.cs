using System;

namespace JasonQuery.Core.Database.Transactions
{
    internal sealed class PendingTransactionReminderDecision
    {
        public PendingTransactionReminderState State { get; set; }

        public TimeSpan Elapsed { get; set; }

        public bool ShouldShowWarning { get; set; }

        public bool ShouldEnableTimer { get; set; }
    }
}
