using System;
using System.Data;

namespace JasonQuery.Core.Database.Transactions
{
    internal static class DatabaseTransactionActionCoordinator
    {
        private const string DisconnectFailureMessage = "ErrorMsg: The transaction was completed, but the database connection could not be closed.";

        public static DatabaseTransactionActionResult Execute<TReader>(DatabaseTransactionActionKind actionKind, TReader reader,
                                                                       Func<TReader, ConnectionState> getState, Func<TReader, string> executeTransaction,
                                                                       Func<TReader, bool> disconnect, Func<string> timestampProvider) where TReader : class
        {
            if (getState == null)
            {
                throw new ArgumentNullException(nameof(getState));
            }

            if (executeTransaction == null)
            {
                throw new ArgumentNullException(nameof(executeTransaction));
            }

            if (disconnect == null)
            {
                throw new ArgumentNullException(nameof(disconnect));
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException(nameof(timestampProvider));
            }

            var timestamp = GetTimestamp(timestampProvider);
            var result = CreateBaseResult(actionKind);

            if (reader == null)
            {
                result.Message = BuildNotExecutedMessage(actionKind, "the database reader is not available", timestamp);
                return result;
            }

            ConnectionState state;

            try
            {
                state = getState(reader);
            }
            catch (Exception ex)
            {
                result.Message = BuildExceptionMessage(ex, timestamp);
                return result;
            }

            if (state != ConnectionState.Open)
            {
                result.Message = BuildNotExecutedMessage(actionKind, "the database connection is not open", timestamp);
                return result;
            }

            result.WasAttempted = true;

            string transactionMessage;

            try
            {
                transactionMessage = executeTransaction(reader) ?? string.Empty;
            }
            catch (Exception ex)
            {
                result.Message = BuildExceptionMessage(ex, timestamp);
                return result;
            }

            result.TransactionMessage = transactionMessage;

            if (!string.IsNullOrWhiteSpace(transactionMessage))
            {
                result.Message = BuildFailureMessage(transactionMessage, timestamp);
                return result;
            }

            result.TransactionSucceeded = true;
            result.DisconnectAttempted = true;

            try
            {
                result.DisconnectSucceeded = disconnect(reader);
            }
            catch (Exception ex)
            {
                result.DisconnectSucceeded = false;
                result.Message = BuildSuccessMessage(actionKind, timestamp) + "\r\n" + BuildExceptionMessage(ex, string.Empty);

                return result;
            }

            result.Message = BuildSuccessMessage(actionKind, timestamp);

            if (!result.DisconnectSucceeded)
            {
                result.Message += "\r\n" + DisconnectFailureMessage;
            }

            return result;
        }

        private static DatabaseTransactionActionResult CreateBaseResult(DatabaseTransactionActionKind actionKind)
        {
            return new DatabaseTransactionActionResult
            {
                ActionKind = actionKind,
                DisconnectSucceeded = false
            };
        }

        private static string GetTimestamp(Func<string> timestampProvider)
        {
            try
            {
                return timestampProvider() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string BuildNotExecutedMessage(DatabaseTransactionActionKind actionKind, string reason, string timestamp)
        {
            var actionText = GetActionText(actionKind);
            var message = $"ErrorMsg: {actionText} was not executed because {reason}.";

            return AppendTimestamp(message, timestamp);
        }

        private static string BuildFailureMessage(string transactionMessage, string timestamp)
        {
            return AppendTimestamp(transactionMessage.Trim(), timestamp);
        }

        private static string BuildSuccessMessage(DatabaseTransactionActionKind actionKind, string timestamp)
        {
            var actionText = GetActionText(actionKind);

            return string.IsNullOrWhiteSpace(timestamp) ? $"{actionText} executed." : $"{actionText} executed at {timestamp}";
        }

        private static string BuildExceptionMessage(Exception exception, string timestamp)
        {
            var message = $"ErrorMsg: {exception?.Message ?? "Unknown transaction error."}";

            return AppendTimestamp(message, timestamp);
        }

        private static string AppendTimestamp(string message, string timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp))
            {
                return message ?? string.Empty;
            }

            return $"{message} {timestamp}".Trim();
        }

        private static string GetActionText(DatabaseTransactionActionKind actionKind)
        {
            return actionKind == DatabaseTransactionActionKind.Rollback ? "Rollback" : "Commit";
        }
    }
}
