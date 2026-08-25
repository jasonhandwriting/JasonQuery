namespace JasonQuery.Core.Database.Transactions
{
    internal sealed class DatabaseTransactionActionResult
    {
        public DatabaseTransactionActionKind ActionKind { get; set; }

        public bool WasAttempted { get; set; }

        public bool TransactionSucceeded { get; set; }

        public bool DisconnectAttempted { get; set; }

        public bool DisconnectSucceeded { get; set; }

        public string TransactionMessage { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool ShouldClearPendingState
        {
            get
            {
                return TransactionSucceeded;
            }
        }

        public bool ShouldKeepPendingState
        {
            get
            {
                return !TransactionSucceeded;
            }
        }
    }
}
