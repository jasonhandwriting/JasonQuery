using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Transactions;
using System;
using System.Data;

namespace JasonQuery.Core.Database.Connection
{
    internal static class DatabaseTransactionActionHelper
    {
        public static DatabaseTransactionActionResult TryCommitAndDisconnectResult<TReader>(TReader reader, Func<TReader, ConnectionState> getState,
                                                                                            Func<TReader, string> commit, Func<TReader, bool> disconnect) where TReader : class
        {
            return DatabaseTransactionActionCoordinator.Execute(DatabaseTransactionActionKind.Commit, reader, getState, commit, disconnect,
                                                                MyGlobal.DateTimeNowWithDateFormat);
        }

        public static DatabaseTransactionActionResult TryRollbackAndDisconnectResult<TReader>(TReader reader, Func<TReader, ConnectionState> getState,
                                                                                              Func<TReader, string> rollback, Func<TReader, bool> disconnect) where TReader : class
        {
            return DatabaseTransactionActionCoordinator.Execute(DatabaseTransactionActionKind.Rollback, reader, getState, rollback, disconnect,
                                                                MyGlobal.DateTimeNowWithDateFormat);
        }

        public static string TryCommitAndDisconnect<TReader>(TReader reader, Func<TReader, ConnectionState> getState, Func<TReader, string> commit, Func<TReader, bool> disconnect) where TReader : class
        {
            return TryCommitAndDisconnectResult(reader, getState, commit, disconnect).Message;
        }

        public static string TryRollbackAndDisconnect<TReader>(TReader reader, Func<TReader, ConnectionState> getState, Func<TReader, string> rollback, Func<TReader, bool> disconnect) where TReader : class
        {
            return TryRollbackAndDisconnectResult(reader, getState, rollback, disconnect).Message;
        }
    }
}
