using System;
using System.Data;

namespace JasonQuery.Core.Database.Connection
{
    internal static class DatabaseConnectionGuard
    {
        /// <summary>
        /// 確保資料庫連線可用。
        /// Closed / Broken 時會呼叫 connect。
        /// 連線失敗時丟出 InvalidOperationException。
        /// </summary>
        public static void EnsureOpen(Func<ConnectionState> getState, Func<string> connect)
        {
            if (!TryEnsureOpen(getState, connect, out var errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        public static bool TryEnsureOpen(Func<ConnectionState> getState, Func<string> connect, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (getState == null)
            {
                errorMessage = $"{nameof(getState)} is null.";
                return false;
            }

            if (connect == null)
            {
                errorMessage = $"{nameof(connect)} is null.";
                return false;
            }

            var state = getState();

            if (state == ConnectionState.Open)
            {
                return true;
            }

            if (state == ConnectionState.Closed || state == ConnectionState.Broken)
            {
                var connectError = connect();

                if (!string.IsNullOrEmpty(connectError))
                {
                    errorMessage = connectError;
                    return false;
                }

                return true;
            }

            errorMessage = $"Connection state is not available for reconnect. State = {state}";
            return false;
        }
    }
}