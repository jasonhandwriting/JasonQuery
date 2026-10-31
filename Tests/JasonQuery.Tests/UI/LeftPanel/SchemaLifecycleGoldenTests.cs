using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L1")]
    public sealed class SchemaLifecycleGoldenTests
    {
        private static SharedSchemaLifecycleDecision Step(SharedSchemaLifecycleEvent eventKind, SharedSchemaLifecycleState from = null)
        {
            return SharedSchemaLifecyclePolicy.Apply(from ?? SharedSchemaLifecycleState.Empty, eventKind);
        }

        [TestMethod] public void G01_CachedMetadataReadableAfterPhysicalDisconnect()
        {
            Assert.IsTrue(SharedSchemaLifecyclePolicy.CanReadCachedMetadata(true));
        }

        [TestMethod] public void G02_InvalidLogicalSnapshotIsNotReadable()
        {
            Assert.IsFalse(SharedSchemaLifecyclePolicy.CanReadCachedMetadata(false));
        }

        [TestMethod] public void G03_OracleSessionActionRequiresCurrentIdentity()
        {
            Assert.IsTrue(SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(true, true));
        }

        [TestMethod] public void G04_CachedOracleSnapshotCannotAuthorizeMissingSession()
        {
            Assert.IsFalse(SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(true, false));
        }

        [TestMethod] public void G05_LiveIdentityWithoutCacheIsInsufficient()
        {
            Assert.IsFalse(SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(false, true));
        }

        [TestMethod] public void G06_QueryDoesNotDirtySchema()
        {
            var result = Step(SharedSchemaLifecycleEvent.QueryCompleted);

            Assert.IsFalse(result.State.NeedsMetadataReload);
            Assert.IsFalse(result.RequestMetadataReload);
        }

        [TestMethod] public void G07_CommentOrTruncateDoesNotRequireFullRefresh()
        {
            var result = Step(SharedSchemaLifecycleEvent.NonStructuralDdlSucceeded);

            Assert.IsFalse(result.RequestMetadataReload);
            Assert.IsFalse(result.State.NeedsMetadataReload);
        }

        [TestMethod] public void G08_TransactionalDdlRecordsPendingDirty()
        {
            Assert.IsTrue(Step(SharedSchemaLifecycleEvent.StructuralDdlPending).State.HasPendingStructuralChanges);
        }

        [TestMethod] public void G09_TransactionalDdlDoesNotImmediatelyReload()
        {
            Assert.IsFalse(Step(SharedSchemaLifecycleEvent.StructuralDdlPending).RequestMetadataReload);
        }

        [TestMethod] public void G10_ImplicitCommitRequiresReload()
        {
            var result = Step(SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted);

            Assert.IsTrue(result.State.NeedsMetadataReload);
            Assert.IsTrue(result.RequestMetadataReload);
        }

        [TestMethod] public void G11_CommitAfterPendingDdlRequestsExactlyOneReload()
        {
            var pending = Step(SharedSchemaLifecycleEvent.StructuralDdlPending);
            var committed = Step(SharedSchemaLifecycleEvent.CommitSucceeded, pending.State);

            Assert.IsTrue(committed.RequestMetadataReload);
            Assert.IsFalse(committed.State.HasPendingStructuralChanges);
        }

        [TestMethod] public void G12_RollbackAfterPendingDdlRetainsCommittedCache()
        {
            var pending = Step(SharedSchemaLifecycleEvent.StructuralDdlPending);

            Assert.IsTrue(pending.State.HasPendingStructuralChanges);

            var rolledBack = Step(SharedSchemaLifecycleEvent.RollbackSucceeded, pending.State);

            Assert.IsFalse(rolledBack.State.HasPendingStructuralChanges);
            Assert.IsFalse(rolledBack.State.NeedsMetadataReload);
            Assert.IsFalse(rolledBack.RequestMetadataReload);
        }

        [TestMethod] public void G13_MultiplePendingDdlStillOnlyOneCommitReload()
        {
            var state = SharedSchemaLifecycleState.Empty;

            for (var i = 0; i < 3; i++)
            {
                state = Step(SharedSchemaLifecycleEvent.StructuralDdlPending, state).State;
            }

            var result = Step(SharedSchemaLifecycleEvent.CommitSucceeded, state);

            Assert.IsTrue(result.RequestMetadataReload);

            var loaded = Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, result.State);

            Assert.IsFalse(Step(SharedSchemaLifecycleEvent.CommitSucceeded, loaded.State).RequestMetadataReload);
        }

        [TestMethod] public void G14_UncertainTransactionOutcomeIsFailClosed()
        {
            var pending = Step(SharedSchemaLifecycleEvent.StructuralDdlPending);
            var unknown = Step(SharedSchemaLifecycleEvent.TransactionOutcomeUnknown, pending.State);

            Assert.IsTrue(unknown.State.HasUnknownTransactionOutcome);
            Assert.IsTrue(unknown.State.NeedsMetadataReload);
            Assert.IsFalse(unknown.RequestMetadataReload);
        }

        [TestMethod] public void G15_MetadataReloadIsNeverPublishedWhilePending()
        {
            var pending = Step(SharedSchemaLifecycleEvent.StructuralDdlPending);
            var result = Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, pending.State);

            Assert.IsFalse(result.PublishEditorKeywords);
            Assert.IsTrue(result.State.HasPendingStructuralChanges);
        }

        [TestMethod] public void G16_CompletedMetadataReloadPublishesKeywords()
        {
            var committed = Step(SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted);
            var loaded = Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, committed.State);

            Assert.IsTrue(loaded.PublishEditorKeywords);
            Assert.IsFalse(loaded.State.NeedsMetadataReload);
        }

        [TestMethod] public void G17_MetadataReloadFailureRetainsDirtyFlag()
        {
            var committed = Step(SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted);
            var failed = Step(SharedSchemaLifecycleEvent.MetadataReloadFailed, committed.State);

            Assert.IsTrue(failed.State.NeedsMetadataReload);
            Assert.IsFalse(failed.PublishEditorKeywords);
        }

        [TestMethod] public void G18_ManualRefreshOutsideTransactionRequestsReload()
        {
            Assert.IsTrue(Step(SharedSchemaLifecycleEvent.ManualRefreshRequested).RequestMetadataReload);
        }

        [TestMethod] public void G19_ManualRefreshDuringPendingDdlIsDeferred()
        {
            var pending = Step(SharedSchemaLifecycleEvent.StructuralDdlPending);
            var result = Step(SharedSchemaLifecycleEvent.ManualRefreshRequested, pending.State);

            Assert.IsFalse(result.RequestMetadataReload);
            Assert.IsTrue(result.State.HasPendingStructuralChanges);
        }

        [TestMethod] public void G20_UnknownTransactionSuppressesKeywordPublishing()
        {
            var uncertain = Step(SharedSchemaLifecycleEvent.TransactionOutcomeUnknown);

            Assert.IsTrue(uncertain.State.HasUnknownTransactionOutcome);
            Assert.IsFalse(Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, uncertain.State).PublishEditorKeywords);
        }

        [TestMethod] public void G21_RollbackAfterUnknownOutcomeRequiresRevalidation()
        {
            var uncertain = Step(SharedSchemaLifecycleEvent.TransactionOutcomeUnknown);
            var rolledBack = Step(SharedSchemaLifecycleEvent.RollbackSucceeded, uncertain.State);

            Assert.IsTrue(rolledBack.RequestMetadataReload);
        }

        [TestMethod] public void G22_CommitWithoutStructuralChangesDoesNotReload()
        {
            Assert.IsFalse(Step(SharedSchemaLifecycleEvent.CommitSucceeded).RequestMetadataReload);
        }

        [TestMethod] public void G23_SuccessfulReloadResetsDirtyAndUncertainState()
        {
            var committed = Step(SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted);

            Assert.IsTrue(committed.State.NeedsMetadataReload);

            var loaded = Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, committed.State);

            Assert.IsFalse(loaded.State.HasUnknownTransactionOutcome);
            Assert.IsFalse(loaded.State.HasPendingStructuralChanges);
            Assert.IsFalse(loaded.State.NeedsMetadataReload);
        }

        [TestMethod] public void G24_UnrequestedReloadResultMustNotPublishKeywords()
        {
            Assert.IsFalse(Step(SharedSchemaLifecycleEvent.MetadataReloadSucceeded).PublishEditorKeywords);
        }

        [TestMethod] public void G25_UnknownEventConservativelyRequiresRevalidation()
        {
            var result = Step((SharedSchemaLifecycleEvent)999);

            Assert.IsTrue(result.State.NeedsMetadataReload);
            Assert.IsTrue(result.State.HasUnknownTransactionOutcome);
        }
    }
}
