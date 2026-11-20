using JasonQuery.Core.Database.Connection;
using System;
using System.Collections.Generic;

namespace JasonQuery.Core.Database.DdlPreview
{
    //F7-S1-B1: a pure decision. Never opens a connection, commits, rolls back, or dispatches DDL.
    //"Clear" permits later existing gates to run; it is NOT authorization to execute SQL.
    internal sealed class DdlPendingTransactionProtectionDecision
    {
        internal DdlPendingTransactionProtectionDecision(bool blockExecute, bool showWarningDialog, bool requiresFormReopen, bool allowSqlPreview, string reasonCode)
        {
            BlockExecute = blockExecute;
            ShowWarningDialog = showWarningDialog;
            RequiresFormReopen = requiresFormReopen;
            AllowSqlPreview = allowSqlPreview;
            ReasonCode = reasonCode;
        }

        public bool BlockExecute { get; }

        public bool ShowWarningDialog { get; }

        public bool RequiresFormReopen { get; }

        public bool AllowSqlPreview { get; }

        public string ReasonCode { get; }
    }

    internal static class DdlPendingTransactionProtectionPolicy
    {
        //Canonical operation IDs, not SQL text. Unknown operations fail closed for Oracle/MySQL.
        private static readonly HashSet<string> ProtectedOperations = new HashSet<string>(StringComparer.Ordinal)
        {
            "Table.Comment",
            "Table.Rename",
            "Table.Drop",
            "Table.Truncate",
            "Column.Add",
            "Column.Comment",
            "Column.Rename",
            "Column.Drop"
        };

        //The caller MUST independently establish an explicit Clean transaction state.
        //AppConfigHelper.IsNotCommitYet == false, by itself, does not prove a verified Clean state.
        //true = Pending; false = verified Clean; null = Unknown.
        public static DdlPendingTransactionProtectionDecision Evaluate(DataSourceType provider, string operation, bool? pendingTransaction)
        {
            //S1 does not change PostgreSQL/SQL Server transaction or DDL authorization gates.
            if (provider == DataSourceType.PostgreSql || provider == DataSourceType.SqlServer)
            {
                return Decision(false, false, false, true, "S1_NOT_APPLICABLE");
            }

            if (provider != DataSourceType.Oracle && provider != DataSourceType.MySql)
            {
                return Decision(true, false, true, false, "S1_UNSUPPORTED_PROVIDER");
            }

            if (!ProtectedOperations.Contains(operation ?? string.Empty))
            {
                return Decision(true, false, true, false, "S1_UNSUPPORTED_OPERATION");
            }

            if (!pendingTransaction.HasValue)
            {
                return Decision(true, true, true, true, "S1_TRANSACTION_UNKNOWN");
            }

            if (pendingTransaction.Value)
            {
                return Decision(true, true, true, true, "S1_TRANSACTION_PENDING");
            }

            //Only a caller-proven Clean state can reach the existing connection/identity/SQL gates.
            return Decision(false, false, false, true, "S1_CLEAR");
        }

        private static DdlPendingTransactionProtectionDecision Decision(bool blockExecute, bool showWarningDialog, bool requiresFormReopen, bool allowSqlPreview, string reasonCode)
        {
            return new DdlPendingTransactionProtectionDecision
            (
                blockExecute,
                showWarningDialog,
                requiresFormReopen,
                allowSqlPreview,
                reasonCode
            );
        }
    }
}
