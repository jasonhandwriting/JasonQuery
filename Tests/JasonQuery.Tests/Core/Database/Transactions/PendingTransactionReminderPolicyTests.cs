using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class PendingTransactionReminderPolicyTests
    {
        private static readonly DateTime BaseTime =
            new DateTime(2026, 7, 19, 9, 0, 0);

        private const int FiveMinutes = 5 * 60 * 1000;

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void SetPending_False_ClearsAllState()
        {
            var current = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var state = PendingTransactionReminderPolicy.SetPending
            (
                current,
                false,
                BaseTime.AddMinutes(1),
                FiveMinutes
            );

            AssertCleared(state);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void SetPending_FirstPending_InitializesStartAndNextWarning()
        {
            var state = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                FiveMinutes
            );

            Assert.IsTrue(state.IsPending);
            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void SetPending_RepeatedPending_PreservesOriginalStartAndWarning()
        {
            var current = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var state = PendingTransactionReminderPolicy.SetPending
            (
                current,
                true,
                BaseTime.AddMinutes(3),
                FiveMinutes
            );

            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void SetPending_ExistingPendingWithoutNextWarning_PreservesStartAndCreatesNextWarning()
        {
            var current = CreatePendingState(BaseTime, null);
            var now = BaseTime.AddMinutes(3);

            var state = PendingTransactionReminderPolicy.SetPending
            (
                current,
                true,
                now,
                FiveMinutes
            );

            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(now.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void SetPending_NonPendingState_StartsNewPeriod()
        {
            var current = new PendingTransactionReminderState
            {
                IsPending = false,
                PendingSince = BaseTime.AddHours(-1),
                NextWarningTime = BaseTime.AddHours(-1)
            };

            var state = PendingTransactionReminderPolicy.SetPending
            (
                current,
                true,
                BaseTime,
                FiveMinutes
            );

            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-5000)]
        public void SetPending_NonPositiveInterval_UsesMinimumDelay(int interval)
        {
            var state = PendingTransactionReminderPolicy.SetPending
            (
                null,
                true,
                BaseTime,
                interval
            );

            Assert.AreEqual(BaseTime.AddMilliseconds(1), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_NullState_ReturnsClearedDecision()
        {
            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                null,
                BaseTime,
                true,
                FiveMinutes
            );

            AssertCleared(decision.State);
            Assert.IsFalse(decision.ShouldShowWarning);
            Assert.IsFalse(decision.ShouldEnableTimer);
            Assert.AreEqual(TimeSpan.Zero, decision.Elapsed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_NonPendingState_ReturnsClearedDecision()
        {
            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                new PendingTransactionReminderState(),
                BaseTime,
                true,
                FiveMinutes
            );

            AssertCleared(decision.State);
            Assert.IsFalse(decision.ShouldEnableTimer);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_BeforeWarningTime_DoesNotShowWarning()
        {
            var state = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                BaseTime.AddMinutes(4),
                true,
                FiveMinutes
            );

            Assert.IsTrue(decision.State.IsPending);
            Assert.IsTrue(decision.ShouldEnableTimer);
            Assert.IsFalse(decision.ShouldShowWarning);
            Assert.AreEqual(TimeSpan.FromMinutes(4), decision.Elapsed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_ExactlyAtWarningTime_ShowsWarning()
        {
            var state = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                BaseTime.AddMinutes(5),
                true,
                FiveMinutes
            );

            Assert.IsTrue(decision.ShouldShowWarning);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_AfterWarningTime_ShowsWarning()
        {
            var state = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                BaseTime.AddMinutes(8),
                true,
                FiveMinutes
            );

            Assert.IsTrue(decision.ShouldShowWarning);
            Assert.AreEqual(TimeSpan.FromMinutes(8), decision.Elapsed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_WarningDisabled_DoesNotShowWarningButKeepsTimer()
        {
            var state = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                BaseTime.AddMinutes(10),
                false,
                FiveMinutes
            );

            Assert.IsFalse(decision.ShouldShowWarning);
            Assert.IsTrue(decision.ShouldEnableTimer);
            Assert.IsTrue(decision.State.IsPending);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_MissingNextWarning_InitializesIt()
        {
            var state = CreatePendingState(BaseTime, null);
            var now = BaseTime.AddMinutes(2);

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                now,
                true,
                FiveMinutes
            );

            Assert.AreEqual(now.AddMinutes(5), decision.State.NextWarningTime);
            Assert.IsFalse(decision.ShouldShowWarning);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Evaluate_FuturePendingSince_ClampsElapsedToZero()
        {
            var state = CreatePendingState(BaseTime.AddMinutes(10), BaseTime.AddMinutes(15));

            var decision = PendingTransactionReminderPolicy.Evaluate
            (
                state,
                BaseTime,
                true,
                FiveMinutes
            );

            Assert.AreEqual(TimeSpan.Zero, decision.Elapsed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ScheduleNextWarning_NullState_ReturnsClearedState()
        {
            var state = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                null,
                BaseTime,
                FiveMinutes
            );

            AssertCleared(state);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ScheduleNextWarning_NonPendingState_ReturnsClearedState()
        {
            var state = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                new PendingTransactionReminderState(),
                BaseTime,
                FiveMinutes
            );

            AssertCleared(state);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ScheduleNextWarning_PendingState_PreservesStartAndSchedulesFromNow()
        {
            var current = CreatePendingState(BaseTime, BaseTime.AddMinutes(5));
            var now = BaseTime.AddMinutes(6);

            var state = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                current,
                now,
                FiveMinutes
            );

            Assert.IsTrue(state.IsPending);
            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(now.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ScheduleNextWarning_MissingStart_UsesCurrentTime()
        {
            var current = new PendingTransactionReminderState
            {
                IsPending = true
            };

            var state = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                current,
                BaseTime,
                FiveMinutes
            );

            Assert.AreEqual(BaseTime, state.PendingSince);
            Assert.AreEqual(BaseTime.AddMinutes(5), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(0)]
        [DataRow(-100)]
        public void ScheduleNextWarning_NonPositiveInterval_UsesMinimumDelay(int interval)
        {
            var current = CreatePendingState(BaseTime, BaseTime);

            var state = PendingTransactionReminderPolicy.ScheduleNextWarning
            (
                current,
                BaseTime,
                interval
            );

            Assert.AreEqual(BaseTime.AddMilliseconds(1), state.NextWarningTime);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetElapsed_NullState_ReturnsZero()
        {
            Assert.AreEqual(TimeSpan.Zero, PendingTransactionReminderPolicy.GetElapsed(null, BaseTime));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetElapsed_NonPendingState_ReturnsZero()
        {
            Assert.AreEqual
            (
                TimeSpan.Zero,
                PendingTransactionReminderPolicy.GetElapsed
                (
                    new PendingTransactionReminderState
                    {
                        IsPending = false,
                        PendingSince = BaseTime
                    },
                    BaseTime.AddMinutes(2)
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetElapsed_MissingStart_ReturnsZero()
        {
            Assert.AreEqual
            (
                TimeSpan.Zero,
                PendingTransactionReminderPolicy.GetElapsed
                (
                    new PendingTransactionReminderState
                    {
                        IsPending = true
                    },
                    BaseTime
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetElapsed_PendingState_ReturnsElapsedTime()
        {
            Assert.AreEqual
            (
                TimeSpan.FromMinutes(7),
                PendingTransactionReminderPolicy.GetElapsed
                (
                    CreatePendingState(BaseTime, BaseTime.AddMinutes(5)),
                    BaseTime.AddMinutes(7)
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void GetElapsed_FutureStart_ReturnsZero()
        {
            Assert.AreEqual
            (
                TimeSpan.Zero,
                PendingTransactionReminderPolicy.GetElapsed
                (
                    CreatePendingState(BaseTime.AddMinutes(2), BaseTime.AddMinutes(7)),
                    BaseTime
                )
            );
        }

        private static PendingTransactionReminderState CreatePendingState(DateTime pendingSince, DateTime? nextWarningTime)
        {
            return new PendingTransactionReminderState
            {
                IsPending = true,
                PendingSince = pendingSince,
                NextWarningTime = nextWarningTime
            };
        }

        private static void AssertCleared(PendingTransactionReminderState state)
        {
            Assert.IsNotNull(state);
            Assert.IsFalse(state.IsPending);
            Assert.IsNull(state.PendingSince);
            Assert.IsNull(state.NextWarningTime);
        }
    }
}