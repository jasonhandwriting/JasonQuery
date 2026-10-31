using System;

namespace JasonQuery.UI.LeftPanel
{
    //F7-L1: only policy decisions. No DB connection, commit, UI refresh, or cache mutations.
    public enum SharedSchemaLifecycleEvent
    {
        QueryCompleted,
        NonStructuralDdlSucceeded,
        StructuralDdlPending,
        StructuralDdlImplicitCommitted,
        CommitSucceeded,
        RollbackSucceeded,
        TransactionOutcomeUnknown,
        ManualRefreshRequested,
        MetadataReloadSucceeded,
        MetadataReloadFailed
    }

    public sealed class SharedSchemaLifecycleState
    {
        public static SharedSchemaLifecycleState Empty => new SharedSchemaLifecycleState(false, false, false);

        public bool HasPendingStructuralChanges { get; }
        public bool NeedsMetadataReload { get; }
        public bool HasUnknownTransactionOutcome { get; }

        public SharedSchemaLifecycleState(bool pending, bool needsReload, bool unknownOutcome)
        {
            HasPendingStructuralChanges = pending;
            NeedsMetadataReload = needsReload;
            HasUnknownTransactionOutcome = unknownOutcome;
        }
    }

    public sealed class SharedSchemaLifecycleDecision
    {
        public SharedSchemaLifecycleState State { get; }
        public bool RequestMetadataReload { get; }
        public bool PublishEditorKeywords { get; }

        public SharedSchemaLifecycleDecision(SharedSchemaLifecycleState state, bool requestReload, bool publishKeywords)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            RequestMetadataReload = requestReload;
            PublishEditorKeywords = publishKeywords;
        }
    }

    public static class SharedSchemaLifecyclePolicy
    {
        //Physical disconnect does not invalidate logically scoped, already loaded metadata.
        //The caller is responsible for checking connection revision, database scope and binding.
        public static bool CanReadCachedMetadata(bool logicalSnapshotReady)
        {
            return logicalSnapshotReady;
        }

        //A cached row is never a substitute for the current Oracle session identity.
        public static bool CanRunSessionBoundAction(bool canReadCache, bool liveSessionIdentityCurrent)
        {
            return canReadCache && liveSessionIdentityCurrent;
        }

        //Pure policy only. No DB work, Commit/Rollback, UI refresh or global keyword mutation.
        public static SharedSchemaLifecycleDecision Apply(SharedSchemaLifecycleState state, SharedSchemaLifecycleEvent eventKind)
        {
            var current = state ?? SharedSchemaLifecycleState.Empty;
            var pending = current.HasPendingStructuralChanges;
            var needsReload = current.NeedsMetadataReload;
            var unknown = current.HasUnknownTransactionOutcome;

            switch (eventKind)
            {
                case SharedSchemaLifecycleEvent.QueryCompleted:
                    {
                        return new SharedSchemaLifecycleDecision(current, false, false);
                    }
                case SharedSchemaLifecycleEvent.NonStructuralDdlSucceeded:
                    {
                        return new SharedSchemaLifecycleDecision(current, false, false);
                    }
                case SharedSchemaLifecycleEvent.StructuralDdlPending:
                    {
                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(true, needsReload, unknown), false, false);
                    }
                case SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted:
                    {
                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(false, true, false), true, false);
                    }
                case SharedSchemaLifecycleEvent.CommitSucceeded:
                    {
                        var shouldReload = pending || needsReload || unknown;

                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(false, shouldReload, false), shouldReload, false);
                    }
                case SharedSchemaLifecycleEvent.RollbackSucceeded:
                    {
                        //A confirmed rollback preserves the old committed cache unless another confirmed/uncertain change was already waiting for revalidation.
                        var shouldReload = needsReload || unknown;

                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(false, shouldReload, false), shouldReload, false);
                    }
                case SharedSchemaLifecycleEvent.TransactionOutcomeUnknown:
                    {
                        //Do not publish transactional metadata when the outcome is uncertain.
                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(pending, true, true), false, false);
                    }
                case SharedSchemaLifecycleEvent.ManualRefreshRequested:
                    {
                        if (pending || unknown)
                        {
                            //Transaction-local preview is intentionally not implemented in L1.
                            return new SharedSchemaLifecycleDecision(current, false, false);
                        }

                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(false, true, false), true, false);
                    }
                case SharedSchemaLifecycleEvent.MetadataReloadSucceeded:
                    {
                        if (pending || unknown || !needsReload)
                        {
                            //Unrequested, uncommitted or uncertain reloads must not publish keywords.
                            return new SharedSchemaLifecycleDecision(current, false, false);
                        }

                        return new SharedSchemaLifecycleDecision(SharedSchemaLifecycleState.Empty, false, true);
                    }
                case SharedSchemaLifecycleEvent.MetadataReloadFailed:
                    {
                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(pending, true, unknown), false, false);
                    }
                default:
                    {
                        //An unrecognized event invalidates confidence in the committed snapshot.
                        return new SharedSchemaLifecycleDecision(new SharedSchemaLifecycleState(pending, true, true), false, false);
                    }
            }
        }
    }
}
