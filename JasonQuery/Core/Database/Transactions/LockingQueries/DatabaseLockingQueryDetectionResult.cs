namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal sealed class DatabaseLockingQueryDetectionResult
    {
        public bool IsLockingQuery { get; set; }

        public DatabaseLockingQueryKind Kind { get; set; }

        public string MatchedText { get; set; } = string.Empty;

        public int MatchedPosition { get; set; } = -1;

        public static DatabaseLockingQueryDetectionResult NotDetected()
        {
            return new DatabaseLockingQueryDetectionResult
            {
                IsLockingQuery = false,
                Kind = DatabaseLockingQueryKind.None,
                MatchedText = string.Empty,
                MatchedPosition = -1
            };
        }
    }
}