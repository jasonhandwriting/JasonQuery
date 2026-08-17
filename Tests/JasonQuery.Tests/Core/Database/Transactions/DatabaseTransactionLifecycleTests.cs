using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class DatabaseTransactionLifecycleTests
    {
        private static readonly DateTime BaseTime = new DateTime(2026, 7, 19, 9, 0, 0);
        private const int FiveMinutes = 5 * 60 * 1000;

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void PendingThenCommitSuccess_ClearsReminderState()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var action = ExecuteAction
            (
                DatabaseTransactionActionKind.Commit,
                string.Empty,
                true
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                !action.ShouldClearPendingState,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            Assert.IsTrue(action.TransactionSucceeded);
            Assert.IsFalse(finalState.IsPending);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void PendingThenRollbackSuccess_ClearsReminderState()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var action = ExecuteAction
            (
                DatabaseTransactionActionKind.Rollback,
                string.Empty,
                true
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                !action.ShouldClearPendingState,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            Assert.IsTrue(action.TransactionSucceeded);
            Assert.IsFalse(finalState.IsPending);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void PendingThenCommitFailure_PreservesReminderStartTime()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var action = ExecuteAction
            (
                DatabaseTransactionActionKind.Commit,
                "ErrorMsg: commit failed",
                true
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                action.ShouldKeepPendingState,
                BaseTime.AddMinutes(2),
                FiveMinutes
            );

            Assert.IsFalse(action.TransactionSucceeded);
            Assert.IsTrue(finalState.IsPending);
            Assert.AreEqual(BaseTime, finalState.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), finalState.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void PendingThenRollbackFailure_PreservesReminderStartTime()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var action = ExecuteAction
            (
                DatabaseTransactionActionKind.Rollback,
                "ErrorMsg: rollback failed",
                true
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                action.ShouldKeepPendingState,
                BaseTime.AddMinutes(3),
                FiveMinutes
            );

            Assert.IsTrue(finalState.IsPending);
            Assert.AreEqual(BaseTime, finalState.PendingSince);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void CommitSuccessButDisconnectFailure_ClearsPending()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var action = ExecuteAction
            (
                DatabaseTransactionActionKind.Commit,
                string.Empty,
                false
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                !action.ShouldClearPendingState,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            Assert.IsTrue(action.TransactionSucceeded);
            Assert.IsFalse(action.DisconnectSucceeded);
            Assert.IsFalse(finalState.IsPending);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void RepeatedPendingEvents_DoNotPostponeWarning()
        {
            var first = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var repeated = PendingTransactionReminderPolicy.SetPending
            (
                first,
                true,
                BaseTime.AddMinutes(4),
                FiveMinutes
            );

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                repeated,
                BaseTime.AddMinutes(5),
                true,
                FiveMinutes
            );

            Assert.AreEqual(BaseTime, repeated.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), repeated.NextWarningTime);
            Assert.IsTrue(decision.ShouldShowWarning);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void WarningAcknowledged_SchedulesNextFiveMinuteWarning()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var afterWarning = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                pending,
                BaseTime.AddMinutes(5),
                FiveMinutes
            );

            var beforeNext = PendingTransactionReminderPolicy.Evaluate
            (
                afterWarning,
                BaseTime.AddMinutes(9),
                true,
                FiveMinutes
            );

            var atNext = PendingTransactionReminderPolicy.Evaluate
            (
                afterWarning,
                BaseTime.AddMinutes(10),
                true,
                FiveMinutes
            );

            Assert.IsFalse(beforeNext.ShouldShowWarning);
            Assert.IsTrue(atNext.ShouldShowWarning);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void CommitClearedThenNewDml_StartsNewPendingPeriod()
        {
            var first = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var cleared = PendingTransactionReminderPolicy.SetPending
            (
                first,
                false,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            var secondStart = BaseTime.AddMinutes(10);

            var second = PendingTransactionReminderPolicy.SetPending
            (
                cleared,
                true,
                secondStart,
                FiveMinutes
            );

            Assert.AreEqual(secondStart, second.PendingSince);
            Assert.AreEqual(secondStart.AddMinutes(5), second.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void WarningDisabled_PendingElapsedTimeStillAdvances()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                pending,
                BaseTime.AddMinutes(12),
                false,
                FiveMinutes
            );

            Assert.IsFalse(decision.ShouldShowWarning);
            Assert.IsTrue(decision.ShouldEnableTimer);
            Assert.AreEqual(TimeSpan.FromMinutes(12), decision.Elapsed);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Transaction")]
        public void ClosedConnectionCommitAttempt_DoesNotClearPending()
        {
            var pending = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            var reader = new FakeReader
            {
                State = ConnectionState.Closed
            };

            var action = DatabaseTransactionActionCoordinator.Execute
            (
                DatabaseTransactionActionKind.Commit,
                reader,
                item => item.State,
                item => string.Empty,
                item => true,
                () => "2026/07/19 09:01:00"
            );

            var finalState = PendingTransactionReminderPolicy.SetPending
            (
                pending,
                action.ShouldKeepPendingState,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            Assert.IsFalse(action.WasAttempted);
            Assert.IsTrue(finalState.IsPending);
        }

        private static DatabaseTransactionActionResult ExecuteAction(DatabaseTransactionActionKind actionKind, string transactionMessage, bool disconnectResult)
        {
            var reader = new FakeReader
            {
                State = ConnectionState.Open,
                TransactionMessage = transactionMessage,
                DisconnectResult = disconnectResult
            };

            return DatabaseTransactionActionCoordinator.Execute
            (
                actionKind,
                reader,
                item => item.State,
                item => item.TransactionMessage,
                item => item.DisconnectResult,
                () => "2026/07/19 09:01:00"
            );
        }

        private sealed class FakeReader
        {
            public ConnectionState State { get; set; }

            public string TransactionMessage { get; set; }

            public bool DisconnectResult { get; set; }
        }
    }
}