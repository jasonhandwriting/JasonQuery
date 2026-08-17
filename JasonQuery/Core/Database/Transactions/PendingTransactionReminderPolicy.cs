using System;

namespace JasonQuery.Core.Database.Transactions
{
    internal static class PendingTransactionReminderPolicy
    {
        private const int MinimumIntervalMilliseconds = 1;

        public static PendingTransactionReminderState SetPending(PendingTransactionReminderState currentState, bool isPending,
                                                                 DateTime now, int warningIntervalMilliseconds)
        {
            if (!isPending)
            {
                return CreateClearedState();
            }

            var interval = NormalizeInterval(warningIntervalMilliseconds);

            if (currentState != null && currentState.IsPending && currentState.PendingSince.HasValue)
            {
                return new PendingTransactionReminderState
                {
                    IsPending = true,
                    PendingSince = currentState.PendingSince,
                    NextWarningTime = currentState.NextWarningTime ?? now.AddMilliseconds(interval)
                };
            }

            return new PendingTransactionReminderState
            {
                IsPending = true,
                PendingSince = now,
                NextWarningTime = now.AddMilliseconds(interval)
            };
        }

        public static PendingTransactionReminderDecision Evaluate(PendingTransactionReminderState currentState,
                                                                  DateTime now, bool warningEnabled, int warningIntervalMilliseconds)
        {
            if (currentState == null || !currentState.IsPending)
            {
                return new PendingTransactionReminderDecision
                {
                    State = CreateClearedState(),
                    Elapsed = TimeSpan.Zero,
                    ShouldShowWarning = false,
                    ShouldEnableTimer = false
                };
            }

            var normalizedState = SetPending(currentState, true, now, warningIntervalMilliseconds);
            var elapsed = now - normalizedState.PendingSince.Value;

            if (elapsed < TimeSpan.Zero)
            {
                elapsed = TimeSpan.Zero;
            }

            var shouldShowWarning = warningEnabled && normalizedState.NextWarningTime.HasValue && now >= normalizedState.NextWarningTime.Value;

            return new PendingTransactionReminderDecision
            {
                State = normalizedState,
                Elapsed = elapsed,
                ShouldShowWarning = shouldShowWarning,
                ShouldEnableTimer = true
            };
        }

        public static PendingTransactionReminderState ScheduleNextWarning(PendingTransactionReminderState currentState,
                                                                          DateTime now, int warningIntervalMilliseconds)
        {
            if (currentState == null || !currentState.IsPending)
            {
                return CreateClearedState();
            }

            var interval = NormalizeInterval(warningIntervalMilliseconds);

            return new PendingTransactionReminderState
            {
                IsPending = true,
                PendingSince = currentState.PendingSince ?? now,
                NextWarningTime = now.AddMilliseconds(interval)
            };
        }

        public static TimeSpan GetElapsed(PendingTransactionReminderState currentState, DateTime now)
        {
            if (currentState == null || !currentState.IsPending || !currentState.PendingSince.HasValue)
            {
                return TimeSpan.Zero;
            }

            var elapsed = now - currentState.PendingSince.Value;

            return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        }

        private static PendingTransactionReminderState CreateClearedState()
        {
            return new PendingTransactionReminderState
            {
                IsPending = false,
                PendingSince = null,
                NextWarningTime = null
            };
        }

        private static int NormalizeInterval(int warningIntervalMilliseconds)
        {
            return warningIntervalMilliseconds < MinimumIntervalMilliseconds ? MinimumIntervalMilliseconds : warningIntervalMilliseconds;
        }
    }
}