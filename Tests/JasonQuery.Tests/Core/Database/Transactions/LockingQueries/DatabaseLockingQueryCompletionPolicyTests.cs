using JasonQuery.Core.Database.Transactions.LockingQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Transactions.LockingQueries
{
    [TestClass]
    public sealed class DatabaseLockingQueryCompletionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_NonLockingQueryWithoutPending_DoesNothing()
        {
            var decision = Evaluate(isLockingQuery: false, executionStarted: true, querySucceeded: true, cancellationRequested: false, hadPending: false);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.NotLockingQuery,
                shouldMarkPending: false,
                shouldKeepConnection: false,
                shouldShowWarning: false,
                shouldDisablePaging: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_NonLockingQueryWithExistingPending_PreservesConnection()
        {
            var decision = Evaluate(isLockingQuery: false, executionStarted: true, querySucceeded: true, cancellationRequested: false, hadPending: true);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.NotLockingQuery,
                shouldMarkPending: true,
                shouldKeepConnection: true,
                shouldShowWarning: false,
                shouldDisablePaging: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_LockingQueryCancelledBeforeExecutionWithoutPending_DoesNothing()
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: false, querySucceeded: false, cancellationRequested: false, hadPending: false);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.CancelledBeforeExecution,
                shouldMarkPending: false,
                shouldKeepConnection: false,
                shouldShowWarning: false,
                shouldDisablePaging: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_LockingQueryCancelledBeforeExecutionWithPending_PreservesPending()
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: false, querySucceeded: false, cancellationRequested: false, hadPending: true);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.CancelledBeforeExecution,
                shouldMarkPending: true,
                shouldKeepConnection: true,
                shouldShowWarning: false,
                shouldDisablePaging: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow(false)]
        [DataRow(true)]
        public void Evaluate_SuccessfulLockingQuery_AlwaysCreatesPendingState(bool hadPending)
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: true, querySucceeded: true, cancellationRequested: false, hadPending: hadPending);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.CompletedSuccessfully,
                shouldMarkPending: true,
                shouldKeepConnection: true,
                shouldShowWarning: true,
                shouldDisablePaging: true
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow(false)]
        [DataRow(true)]
        public void Evaluate_CancelledAfterStart_ConservativelyCreatesPendingState(bool hadPending)
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: true, querySucceeded: false, cancellationRequested: true, hadPending: hadPending);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.CancelledAfterExecutionStarted,
                shouldMarkPending: true,
                shouldKeepConnection: true,
                shouldShowWarning: true,
                shouldDisablePaging: true
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_FailedLockingQueryWithoutExistingPending_DoesNotInventPendingState()
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: true, querySucceeded: false, cancellationRequested: false, hadPending: false);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.Failed,
                shouldMarkPending: false,
                shouldKeepConnection: false,
                shouldShowWarning: false,
                shouldDisablePaging: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_FailedLockingQueryWithExistingPending_PreservesPendingState()
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: true, querySucceeded: false, cancellationRequested: false, hadPending: true);

            AssertDecision
            (
                decision,
                DatabaseLockingQueryCompletionKind.Failed,
                shouldMarkPending: true,
                shouldKeepConnection: true,
                shouldShowWarning: false,
                shouldDisablePaging: true
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        public void Evaluate_CancellationTakesPrecedenceOverQuerySucceededFlag()
        {
            var decision = Evaluate(isLockingQuery: true, executionStarted: true, querySucceeded: true, cancellationRequested: true, hadPending: false);

            Assert.AreEqual
            (
                DatabaseLockingQueryCompletionKind.CancelledAfterExecutionStarted,
                decision.CompletionKind
            );

            Assert.IsTrue(decision.ShouldMarkPendingState);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("CompletedSuccessfully", true)]
        [DataRow("CancelledAfterExecutionStarted", true)]
        [DataRow("Failed", false)]
        [DataRow("CancelledBeforeExecution", false)]
        public void Evaluate_WarningFlagMatchesUncertainOrSuccessfulPendingCompletion(string expectedKindName, bool expectedWarning)
        {
            var expectedKind = (DatabaseLockingQueryCompletionKind)System.Enum.Parse
            (
                typeof(DatabaseLockingQueryCompletionKind),
                expectedKindName,
                ignoreCase: true
            );

            var executionStarted = expectedKind != DatabaseLockingQueryCompletionKind.CancelledBeforeExecution;
            var querySucceeded = expectedKind == DatabaseLockingQueryCompletionKind.CompletedSuccessfully;
            var cancellationRequested = expectedKind == DatabaseLockingQueryCompletionKind.CancelledAfterExecutionStarted;

            var decision = Evaluate
            (
                isLockingQuery: true,
                executionStarted: executionStarted,
                querySucceeded: querySucceeded,
                cancellationRequested: cancellationRequested,
                hadPending: false
            );

            Assert.AreEqual(expectedKind, decision.CompletionKind);
            Assert.AreEqual(expectedWarning, decision.ShouldShowCompletionWarning);
        }

        private static DatabaseLockingQueryCompletionDecision Evaluate(bool isLockingQuery, bool executionStarted, bool querySucceeded,
                                                                       bool cancellationRequested, bool hadPending)
        {
            return DatabaseLockingQueryCompletionPolicy.Evaluate
            (
                isLockingQuery,
                executionStarted,
                querySucceeded,
                cancellationRequested,
                hadPending
            );
        }

        private static void AssertDecision(DatabaseLockingQueryCompletionDecision decision, DatabaseLockingQueryCompletionKind expectedKind, bool shouldMarkPending,
                                           bool shouldKeepConnection, bool shouldShowWarning, bool shouldDisablePaging)
        {
            Assert.AreEqual(expectedKind, decision.CompletionKind);
            Assert.AreEqual(shouldMarkPending, decision.ShouldMarkPendingState);
            Assert.AreEqual(shouldKeepConnection, decision.ShouldKeepConnectionOpen);
            Assert.AreEqual(shouldKeepConnection, decision.ShouldPreventAutomaticDisconnect);
            Assert.AreEqual(shouldShowWarning, decision.ShouldShowCompletionWarning);
            Assert.AreEqual(shouldDisablePaging, decision.ShouldDisablePaging);
        }
    }
}
