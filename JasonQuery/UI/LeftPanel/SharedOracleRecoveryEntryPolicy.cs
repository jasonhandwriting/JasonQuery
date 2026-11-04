namespace JasonQuery.UI.LeftPanel
{
    //L2B-1: classify an explicit F7 recovery affordance, not database authorization.
    public enum SharedOracleRecoveryEntryKind
    {
        None,
        ObjectRead,
        Table,
        Column
    }

    public static class SharedOracleRecoveryEntryPolicy
    {
        //L2B-1: classify eligible Table/Column recovery guidance without database access.
        public static bool CanOfferEntry(SharedOracleSessionRecoveryContext context, SharedOracleRecoveryEntryKind kind)
        {
            return IsQualifiedKind(kind) && SharedOracleSessionRecoveryPolicy.CanOfferRecovery(context);
        }

        //A click can request an operation, but this pure policy never connects to a database.
        public static bool CanAcceptExplicitClick(SharedOracleSessionRecoveryContext context, SharedOracleRecoveryEntryKind kind)
        {
            return IsQualifiedKind(kind) && SharedOracleSessionRecoveryPolicy.CanStartRecovery(context);
        }

        private static bool IsQualifiedKind(SharedOracleRecoveryEntryKind kind)
        {
            return kind == SharedOracleRecoveryEntryKind.Table || kind == SharedOracleRecoveryEntryKind.Column;
        }
    }
}
