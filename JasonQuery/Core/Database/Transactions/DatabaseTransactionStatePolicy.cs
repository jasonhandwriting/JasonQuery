using JasonLibrary.Core;
using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.Core.Database.Transactions
{
    internal enum CancelledNonQueryCompletionKind
    {
        NotApplicable = 0,
        PreserveExistingPending = 1,
        RollbackRequired = 2,
        RollbackSucceeded = 3,
        RollbackFailedOrUnknown = 4
    }

    internal sealed class CancelledNonQueryCompletionDecision
    {
        public CancelledNonQueryCompletionKind CompletionKind { get; set; }

        public bool IsHandled { get; set; }

        public bool ShouldAttemptRollback { get; set; }

        public bool ShouldClearPendingState { get; set; }

        public bool ShouldKeepPendingState { get; set; }
    }

    internal static class CancelledNonQueryCompletionPolicy
    {
        public static CancelledNonQueryCompletionDecision Evaluate(bool cancellationRequested, bool wasNonQuery, bool hadPendingTransactionBeforeExecution, bool? rollbackSucceeded)
        {
            if (!cancellationRequested || !wasNonQuery)
            {
                return CreateDecision(CancelledNonQueryCompletionKind.NotApplicable, false, false, false, false);
            }

            if (hadPendingTransactionBeforeExecution)
            {
                return CreateDecision(CancelledNonQueryCompletionKind.PreserveExistingPending, true, false, false, true);
            }

            if (!rollbackSucceeded.HasValue)
            {
                return CreateDecision(CancelledNonQueryCompletionKind.RollbackRequired, true, true, false, false);
            }

            return rollbackSucceeded.Value
                   ? CreateDecision(CancelledNonQueryCompletionKind.RollbackSucceeded, true, false, true, false)
                   : CreateDecision(CancelledNonQueryCompletionKind.RollbackFailedOrUnknown, true, false, false, true);
        }

        private static CancelledNonQueryCompletionDecision CreateDecision(CancelledNonQueryCompletionKind completionKind, bool isHandled, bool shouldAttemptRollback, bool shouldClearPendingState, bool shouldKeepPendingState)
        {
            return new CancelledNonQueryCompletionDecision
            {
                CompletionKind = completionKind,
                IsHandled = isHandled,
                ShouldAttemptRollback = shouldAttemptRollback,
                ShouldClearPendingState = shouldClearPendingState,
                ShouldKeepPendingState = shouldKeepPendingState
            };
        }
    }

    /// <summary>
    /// Centralizes the transaction impact rules that were previously duplicated
    /// in OracleReader, PostgreSqlReader, SqlServerReader, and MySqlReader.
    /// </summary>
    internal static class DatabaseTransactionStatePolicy
    {
        public const string StateUnchanged = "TX_UNCHANGED";
        public const string StatePending = "TX_PENDING";
        public const string StateClosed = "TX_CLOSED";

        public static DatabaseTransactionImpact GetImpact(DataSourceType dataSourceType,string sqlStatementType)
        {
            var value = NormalizeStatementType(sqlStatementType);

            if (string.IsNullOrEmpty(value))
            {
                return DatabaseTransactionImpact.Unchanged;
            }

            if (IsCommitOrRollback(value))
            {
                return DatabaseTransactionImpact.Closed;
            }

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return GetOracleImpact(value);
                    }
                case DataSourceType.PostgreSql:
                    {
                        return GetPostgreSqlImpact(value);
                    }
                case DataSourceType.SqlServer:
                    {
                        return GetSqlServerImpact(value);
                    }
                case DataSourceType.MySql:
                    {
                        return GetMySqlImpact(value);
                    }
                default:
                    {
                        return DatabaseTransactionImpact.Unchanged;
                    }
            }
        }

        public static void Apply(DataSourceType dataSourceType, string sqlStatementType, ref bool hasPendingTransactionAfterExecute, ref bool isTransactionClosedBySqlCommand)
        {
            ApplyImpact
            (
                GetImpact(dataSourceType, sqlStatementType),
                ref hasPendingTransactionAfterExecute,
                ref isTransactionClosedBySqlCommand
            );
        }

        public static void ApplyImpact(DatabaseTransactionImpact impact, ref bool hasPendingTransactionAfterExecute, ref bool isTransactionClosedBySqlCommand)
        {
            switch (impact)
            {
                case DatabaseTransactionImpact.Pending:
                    {
                        hasPendingTransactionAfterExecute = true;
                        isTransactionClosedBySqlCommand = false;
                        break;
                    }
                case DatabaseTransactionImpact.Closed:
                    {
                        hasPendingTransactionAfterExecute = false;
                        isTransactionClosedBySqlCommand = true;
                        break;
                    }
                case DatabaseTransactionImpact.Unchanged:
                default:
                    {
                        break;
                    }
            }
        }

        public static string ResolveStateToken(bool hasPendingTransactionAfterExecute, bool isTransactionClosedBySqlCommand)
        {
            if (hasPendingTransactionAfterExecute)
            {
                return StatePending;
            }

            if (isTransactionClosedBySqlCommand)
            {
                return StateClosed;
            }

            return StateUnchanged;
        }

        private static DatabaseTransactionImpact GetOracleImpact(string value)
        {
            if (IsOracleImplicitCommit(value))
            {
                return DatabaseTransactionImpact.Closed;
            }

            return RequiresOracleCommitOrRollback(value) ? DatabaseTransactionImpact.Pending : DatabaseTransactionImpact.Unchanged;
        }

        private static DatabaseTransactionImpact GetPostgreSqlImpact(string value)
        {
            return RequiresPostgreSqlCommitOrRollback(value) ? DatabaseTransactionImpact.Pending : DatabaseTransactionImpact.Unchanged;
        }

        private static DatabaseTransactionImpact GetSqlServerImpact(string value)
        {
            return RequiresSqlServerCommitOrRollback(value) ? DatabaseTransactionImpact.Pending : DatabaseTransactionImpact.Unchanged;
        }

        private static DatabaseTransactionImpact GetMySqlImpact(string value)
        {
            if (IsMySqlImplicitCommit(value))
            {
                return DatabaseTransactionImpact.Closed;
            }

            return RequiresMySqlCommitOrRollback(value) ? DatabaseTransactionImpact.Pending : DatabaseTransactionImpact.Unchanged;
        }

        private static string NormalizeStatementType(string sqlStatementType)
        {
            if (string.IsNullOrWhiteSpace(sqlStatementType))
            {
                return string.Empty;
            }

            return sqlStatementType.Replace("\r", "\n")
                                   .Replace("\n", " ")
                                   .Trim()
                                   .ToUpperInvariant();
        }

        private static bool IsCommitOrRollback(string value)
        {
            return string.Equals(value, "COMMIT", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "ROLLBACK", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOracleImplicitCommit(string value)
        {
            switch (value)
            {
                case "CREATE":
                case "ALTER":
                case "DROP":
                case "TRUNCATE":
                case "COMMENT":
                case "GRANT":
                case "REVOKE":
                case "ANALYZE":
                case "AUDIT":
                case "NOAUDIT":
                case "RENAME":
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool RequiresOracleCommitOrRollback(string value)
        {
            switch (value)
            {
                case "INSERT":
                case "UPDATE":
                case "DELETE":
                case "MERGE":
                case "LOCK":
                case "CALL":
                case "EXECUTE":
                    {
                        return true;
                    }
            }

            return ContainsEmbeddedDml(value);
        }

        private static bool RequiresPostgreSqlCommitOrRollback(string value)
        {
            switch (value)
            {
                // DML
                case "INSERT":
                case "UPDATE":
                case "DELETE":
                case "MERGE":
                case "CALL":
                case "EXECUTE":

                // Lock / transaction-affecting
                case "LOCK":

                // Transactional DDL
                case "CREATE":
                case "ALTER":
                case "DROP":
                case "TRUNCATE":
                case "COMMENT":
                case "RENAME":

                // DCL
                case "GRANT":
                case "REVOKE":

                // DDL-like / utility commands
                case "REFRESH":
                case "CLUSTER":
                case "REINDEX":
                    {
                        return true;
                    }
            }

            return ContainsEmbeddedDml(value);
        }

        private static bool RequiresSqlServerCommitOrRollback(string value)
        {
            switch (value)
            {
                // DML
                case "INSERT":
                case "UPDATE":
                case "DELETE":
                case "MERGE":

                // Procedure / metadata command
                case "EXEC":
                case "EXECUTE":

                // DDL
                case "CREATE":
                case "ALTER":
                case "DROP":
                case "TRUNCATE":

                // DCL
                case "GRANT":
                case "REVOKE":
                case "DENY":
                    {
                        return true;
                    }
            }

            return ContainsEmbeddedDml(value);
        }

        private static bool IsMySqlImplicitCommit(string value)
        {
            switch (value)
            {
                // DDL
                case "CREATE":
                case "ALTER":
                case "DROP":
                case "TRUNCATE":
                case "RENAME":

                // DCL / account-level command
                case "GRANT":
                case "REVOKE":

                // Table maintenance / DDL-like
                case "ANALYZE":
                case "OPTIMIZE":
                case "REPAIR":
                case "CHECK":

                // LOCK TABLES / UNLOCK TABLES affect transaction boundaries
                case "LOCK":
                case "UNLOCK":
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static bool RequiresMySqlCommitOrRollback(string value)
        {
            switch (value)
            {
                // DML
                case "INSERT":
                case "UPDATE":
                case "DELETE":
                case "MERGE":

                // Procedure / prepared statement
                case "CALL":
                case "EXEC":
                case "EXECUTE":
                    {
                        return true;
                    }
            }

            return ContainsEmbeddedDml(value);
        }

        private static bool ContainsEmbeddedDml(string value)
        {
            return value.Contains(" INSERT") || value.Contains(" UPDATE") || value.Contains(" DELETE");
        }
    }
}