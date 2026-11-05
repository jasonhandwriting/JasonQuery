using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    // Q2-98B2: this coordinator is used ONLY by the new shared F7 Oracle Table/Column GUI.
    // The F7 cache is for display. Execution authority is obtained after normal reconnect
    // from the current physical Reader, fresh scope observations, and the exact target.
    internal static class OracleF7ExecuteTimeHelper
    {
        public static string CheckMetadata(bool currentCompletedLoad, bool logicalScopeCurrent,
                                           string receiptSessionUser, string selectedOwner,
                                           string displayedSql, string rebuiltSql)
        {
            if (!currentCompletedLoad || !logicalScopeCurrent)
            {
                return "Q298_METADATA_STALE";
            }

            if (string.IsNullOrWhiteSpace(receiptSessionUser) || string.IsNullOrWhiteSpace(selectedOwner)
                || !string.Equals(receiptSessionUser, selectedOwner, StringComparison.Ordinal))
            {
                return "Q298_OWNER_MISMATCH";
            }

            if (string.IsNullOrWhiteSpace(displayedSql) || string.IsNullOrWhiteSpace(rebuiltSql)
                || !string.Equals(displayedSql, rebuiltSql, StringComparison.Ordinal))
            {
                return "Q298_SQL_CHANGED";
            }

            return string.Empty;
        }

        public static string Execute(SharedOracleMetadataCompletedLoadReceipt receipt,
                                     string selectedOwner, string tableName, string columnName,
                                     bool requireExistingColumn, string displayedSql, string rebuiltSql,
                                     out string errorMessage, out bool pendingAfterExecute,
                                     out bool transactionClosed, out bool dispatchAttempted)
        {
            var reader = MyGlobal.OracleReader;
            var detail = string.Empty;
            var pending = false;
            var closed = false;
            var dispatched = false;
            var verifiedConnectionId = Guid.Empty;
            var failure = string.Empty;

            try
            {
                failure = OracleDdlGuiGate.TryExecuteF7
                (
                    AppConfigHelper.IsNotCommitYet,
                    () =>
                    {
                        if (DatabaseSqlExecutor.CurrentDataSource != DataSourceType.Oracle)
                        {
                            return "Q298_SCOPE_MISMATCH";
                        }

                        if (reader == null)
                        {
                            return "Q298_CONNECTION_FAILED";
                        }

                        if (!reader.TryEnsureConnectionOpen(out var connectionError))
                        {
                            detail = connectionError;
                            return "Q298_CONNECTION_FAILED";
                        }

                        return string.Empty;
                    },
                    () =>
                    {
                        // The loader receipt remains valid across an ordinary closed/open physical
                        // session; its ORIGINAL source and logical configuration must still match.
                        var currentCompletedLoad = receipt != null
                            && ReferenceEquals(receipt, DatabaseSqlExecutor.LastOracleCompletedLoadReceipt)
                            && ReferenceEquals(receipt.SourceTable, DatabaseSqlExecutor.dtSchema)
                            && SharedOracleMetadataCompletedLoadReceipt.HasRawOracleIdentifiers(receipt.SourceTable);
                        var currentLogicalScope = receipt != null
                            && string.Equals(receipt.LogicalScopeKey,
                                             DatabaseSqlExecutor.GetOracleLoadLogicalScopeKey(),
                                             StringComparison.Ordinal);

                        var metadataStatus = CheckMetadata
                        (
                            currentCompletedLoad,
                            currentLogicalScope,
                            receipt?.SessionUser,
                            selectedOwner,
                            displayedSql,
                            rebuiltSql
                        );

                        if (metadataStatus.Length != 0)
                        {
                            return metadataStatus;
                        }

                        if (AppConfigHelper.IsNotCommitYet)
                        {
                            return "Q298_TRANSACTION_PENDING";
                        }

                        verifiedConnectionId = reader.VerifiedConnectionId;

                        return reader.VerifyF7DdlTargetOnCurrentConnection
                        (
                            verifiedConnectionId,
                            receipt.SessionUser,
                            receipt.DatabaseName,
                            receipt.ContainerName,
                            selectedOwner,
                            tableName,
                            columnName,
                            requireExistingColumn
                        );
                    },
                    () =>
                    {
                        if (reader.VerifiedConnectionId != verifiedConnectionId
                            || reader.GetState() != ConnectionState.Open)
                        {
                            return "Q298_SESSION_CHANGED";
                        }

                        // The execution method MUST NOT open/retry/reconnect another Oracle session.
                        dispatched = true;
                        reader.ExecuteSingleNonQueryOnVerifiedConnection
                        (
                            displayedSql,
                            verifiedConnectionId,
                            out detail,
                            out pending,
                            out closed
                        );

                        return string.IsNullOrEmpty(detail) ? string.Empty : "Q298_EXECUTION_FAILED";
                    }
                );
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                failure = dispatched ? "Q298_EXECUTION_FAILED" : "Q298_VERIFICATION_FAILED";
            }

            errorMessage = detail;
            pendingAfterExecute = pending;
            transactionClosed = closed;
            dispatchAttempted = dispatched;
            return failure ?? "Q298_VERIFICATION_FAILED";
        }

        public static string GetFailureMessage(string failure, string detail, bool dispatchAttempted)
        {
            var code = failure ?? "Q298_VERIFICATION_FAILED";
            var colon = code.IndexOf(':');

            if (colon >= 0)
            {
                var extra = code.Substring(colon + 1).Trim();
                code = code.Substring(0, colon);

                if (string.IsNullOrWhiteSpace(detail))
                {
                    detail = extra;
                }
            }

            var localized = LocalizationHelper.GetLanguageString
            (
                "Oracle DDL was not executed. Check the connection, then click Refresh in the left Schema Information panel before retrying.",
                "Global", "Global", "msg", code, "Text"
            );

            if (failure == "Q298_EXECUTION_FAILED" || dispatchAttempted)
            {
                localized = LocalizationHelper.GetLanguageString
                (
                    "An Oracle DDL execution was attempted, but the result is uncertain. Check the object state before retrying.",
                    "Global", "Global", "msg", "Q298_EXECUTION_FAILED", "Text"
                );
            }

            if (!string.IsNullOrWhiteSpace(detail))
            {
                localized += "\r\n\r\n" + detail;
            }

            return localized + "\r\n[" + code + "]";
        }
    }
}
