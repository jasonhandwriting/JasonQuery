namespace JasonQuery.UI.LeftPanel
{
    //F7-L2A: pure authorization decisions only. Every input must be obtained from the current workspace, live Reader, fresh metadata and re-resolved target.
    //Never connect, query, commit, roll back or execute DDL in this policy.
    public sealed class SharedOracleSessionRecoveryContext
    {
        public bool IsOracleWorkspace { get; set; }
        public bool HasCurrentLogicalSnapshot { get; set; }
        public bool IsDatabaseBackedAction { get; set; }
        public bool IsActiveTabAndMenuCurrent { get; set; }
        public bool IsExplicitUserInvocation { get; set; }
        public bool HasUnsafeTransactionState { get; set; }
        public bool IsLiveSessionVerified { get; set; }
        public bool IsFreshMetadataFromLiveSession { get; set; }
        public bool HasSameDatabaseAndOwnerScope { get; set; }
        public bool IsExactTargetReResolved { get; set; }
        public bool IsFreshSnapshotSessionIdentityCurrent { get; set; }
    }

    public static class SharedOracleSessionRecoveryPolicy
    {
        private static bool HasEligibleActionContext(SharedOracleSessionRecoveryContext context)
        {
            return context != null
                   && context.IsOracleWorkspace
                   && context.HasCurrentLogicalSnapshot
                   && context.IsDatabaseBackedAction
                   && context.IsActiveTabAndMenuCurrent
                   && !context.HasUnsafeTransactionState;
        }

        //A visible recovery affordance is allowed only for an eligible, disconnected Oracle DB action; passive Copy/Paste and a live session never need it.
        public static bool CanOfferRecovery(SharedOracleSessionRecoveryContext context)
        {
            return HasEligibleActionContext(context) && !context.IsLiveSessionVerified;
        }

        //Menu planning is not a user click. Only an explicit invocation may start on-demand recovery; this method itself does not touch the database.
        public static bool CanStartRecovery(SharedOracleSessionRecoveryContext context)
        {
            return CanOfferRecovery(context) && context.IsExplicitUserInvocation;
        }

        //After reconnect, fresh-session metadata, database/owner scope, exact raw target identity and live snapshot binding must ALL be independently proven.
        //Caller-provided booleans are not evidence by themselves (L2B integration).
        public static bool CanAuthorizeRecoveredAction(SharedOracleSessionRecoveryContext context)
        {
            return HasEligibleActionContext(context)
                   && context.IsExplicitUserInvocation
                   && context.IsLiveSessionVerified
                   && context.IsFreshMetadataFromLiveSession
                   && context.HasSameDatabaseAndOwnerScope
                   && context.IsExactTargetReResolved
                   && context.IsFreshSnapshotSessionIdentityCurrent;
        }
    }
}
