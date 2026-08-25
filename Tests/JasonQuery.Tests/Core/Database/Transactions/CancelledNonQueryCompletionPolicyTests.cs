using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class CancelledNonQueryCompletionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Regression")]
        public void Evaluate_CancelledWithoutExistingPending_RequestsRollback()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: true, hadPending: false, rollbackSucceeded: null);

            AssertDecision
            (
                decision,
                CancelledNonQueryCompletionKind.RollbackRequired,
                isHandled: true,
                shouldAttemptRollback: true,
                shouldClearPending: false,
                shouldKeepPending: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Regression")]
        public void Evaluate_CancelledWithExistingPending_PreservesPendingWithoutRollback()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: true, hadPending: true, rollbackSucceeded: null);

            AssertDecision
            (
                decision,
                CancelledNonQueryCompletionKind.PreserveExistingPending,
                isHandled: true,
                shouldAttemptRollback: false,
                shouldClearPending: false,
                shouldKeepPending: true
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public void Evaluate_NonCancelledOrNonQueryExecution_DoesNotHandle(bool cancellationRequested, bool wasNonQuery)
        {
            var decision = Evaluate(cancellationRequested, wasNonQuery, hadPending: false, rollbackSucceeded: null);

            AssertDecision
            (
                decision,
                CancelledNonQueryCompletionKind.NotApplicable,
                isHandled: false,
                shouldAttemptRollback: false,
                shouldClearPending: false,
                shouldKeepPending: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Regression")]
        public void Evaluate_ExplicitTransactionTokenAlreadyClearedNonQueryFlag_DoesNotOverrideReturnedState()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: false, hadPending: false, rollbackSucceeded: true);

            Assert.AreEqual(CancelledNonQueryCompletionKind.NotApplicable, decision.CompletionKind);
            Assert.IsFalse(decision.IsHandled);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Regression")]
        public void Evaluate_SuccessfulRollback_ClearsPendingState()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: true, hadPending: false, rollbackSucceeded: true);

            AssertDecision
            (
                decision,
                CancelledNonQueryCompletionKind.RollbackSucceeded,
                isHandled: true,
                shouldAttemptRollback: false,
                shouldClearPending: true,
                shouldKeepPending: false
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [TestCategory("Regression")]
        public void Evaluate_FailedRollback_PreservesPendingForSafety()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: true, hadPending: false, rollbackSucceeded: false);

            AssertDecision
            (
                decision,
                CancelledNonQueryCompletionKind.RollbackFailedOrUnknown,
                isHandled: true,
                shouldAttemptRollback: false,
                shouldClearPending: false,
                shouldKeepPending: true
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_ExistingPendingTakesPrecedenceOverRollbackOutcome()
        {
            var decision = Evaluate(cancellationRequested: true, wasNonQuery: true, hadPending: true, rollbackSucceeded: true);

            Assert.AreEqual(CancelledNonQueryCompletionKind.PreserveExistingPending, decision.CompletionKind);
            Assert.IsTrue(decision.ShouldKeepPendingState);
            Assert.IsFalse(decision.ShouldClearPendingState);
        }

        private static CancelledNonQueryCompletionDecision Evaluate(bool cancellationRequested, bool wasNonQuery, bool hadPending, bool? rollbackSucceeded)
        {
            return CancelledNonQueryCompletionPolicy.Evaluate
            (
                cancellationRequested,
                wasNonQuery,
                hadPending,
                rollbackSucceeded
            );
        }

        private static void AssertDecision(CancelledNonQueryCompletionDecision decision, CancelledNonQueryCompletionKind expectedKind,
                                           bool isHandled, bool shouldAttemptRollback, bool shouldClearPending, bool shouldKeepPending)
        {
            Assert.AreEqual(expectedKind, decision.CompletionKind);
            Assert.AreEqual(isHandled, decision.IsHandled);
            Assert.AreEqual(shouldAttemptRollback, decision.ShouldAttemptRollback);
            Assert.AreEqual(shouldClearPending, decision.ShouldClearPendingState);
            Assert.AreEqual(shouldKeepPending, decision.ShouldKeepPendingState);
        }
    }
}
