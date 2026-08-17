namespace JasonQuery.Core.Database.Transactions
{
    internal enum DatabaseTransactionImpact
    {
        Unchanged,
        Pending,
        Closed
    }
}