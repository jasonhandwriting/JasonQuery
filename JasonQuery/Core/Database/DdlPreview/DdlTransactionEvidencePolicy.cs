using JasonQuery.Core.Database.Connection;
using System.Data;

namespace JasonQuery.Core.Database.DdlPreview
{
    //Pure evidence classification for Oracle/MySQL guided DDL.
    //This class does not open connections, authorize SQL, or commit/rollback transactions.
    internal enum DdlTransactionEvidenceKind
    {
        NotApplicable = 0,
        Pending = 1,
        Clean = 2,
        ReconnectRequired = 3,
        Unknown = 4
    }

    internal sealed class DdlTransactionEvidenceDecision
    {
        internal DdlTransactionEvidenceDecision(DdlTransactionEvidenceKind kind, bool? hasPendingTransaction, bool blockExecute, bool canAttemptReconnect, bool requiresFormReopen, string reasonCode)
        {
            Kind = kind;
            HasPendingTransaction = hasPendingTransaction;
            BlockExecute = blockExecute;
            CanAttemptReconnect = canAttemptReconnect;
            RequiresFormReopen = requiresFormReopen;
            ReasonCode = reasonCode;
        }

        public DdlTransactionEvidenceKind Kind { get; }

        //Only Clean is an explicitly qualified false. ReconnectRequired and Unknown remain null.
        public bool? HasPendingTransaction { get; }

        public bool BlockExecute { get; }

        public bool CanAttemptReconnect { get; }

        public bool RequiresFormReopen { get; }

        public string ReasonCode { get; }
    }

    internal static class DdlTransactionEvidencePolicy
    {
        //Inputs are observations, NOT caller wishes:
        //  hasPendingTransaction: JasonQuery's existing pending flag.
        //  hasUncertainOutcome: an actual unresolved execution/transaction outcome, if observed.
        //  logicalScopeCurrent: current provider/database/connection configuration still matches the form.
        //  readerAvailable/readerState: non-reconnecting observation of the current Reader.
        //No physical session ID is required by this additional S1 classification.
        public static DdlTransactionEvidenceDecision Evaluate(DataSourceType provider, bool hasPendingTransaction, bool hasUncertainOutcome, bool logicalScopeCurrent, bool readerAvailable, ConnectionState readerState)
        {
            //S1 is not a replacement for PostgreSQL/SQL Server's existing Execute Gates.
            if (provider == DataSourceType.PostgreSql || provider == DataSourceType.SqlServer)
            {
                return Create(DdlTransactionEvidenceKind.NotApplicable, null, false, false, false, "S1_EVIDENCE_NOT_APPLICABLE");
            }

            if (provider != DataSourceType.Oracle && provider != DataSourceType.MySql)
            {
                return Unknown("S1_EVIDENCE_UNSUPPORTED_PROVIDER");
            }

            //Never reconnect or start metadata loading when known user DML is pending.
            if (hasPendingTransaction)
            {
                return Create(DdlTransactionEvidenceKind.Pending, true, true, false, true, "S1_EVIDENCE_PENDING");
            }

            //A false pending flag alone is not proof of Clean.
            if (hasUncertainOutcome || !logicalScopeCurrent || !readerAvailable)
            {
                return Unknown("S1_EVIDENCE_UNKNOWN");
            }

            if (readerState == ConnectionState.Open)
            {
                //This is only admission to existing SQL/target/connection verification.
                return Create(DdlTransactionEvidenceKind.Clean, false, false, false, false, "S1_EVIDENCE_CLEAN");
            }

            if (readerState == ConnectionState.Closed)
            {
                //Normal SELECT-after-disconnect must not permanently lock the form.
                //Do not issue DDL until the safe reconnect has actually occurred and
                //this policy has been evaluated again on the live Reader.
                return Create(DdlTransactionEvidenceKind.ReconnectRequired, null, true, true, false, "S1_EVIDENCE_RECONNECT_REQUIRED");
            }

            //Broken, Connecting, Executing, Fetching, or unexpected values: fail closed.
            return Unknown("S1_EVIDENCE_UNKNOWN");
        }

        private static DdlTransactionEvidenceDecision Unknown(string reasonCode)
        {
            return Create(DdlTransactionEvidenceKind.Unknown, null, true, false, true, reasonCode);
        }

        private static DdlTransactionEvidenceDecision Create(DdlTransactionEvidenceKind kind, bool? hasPendingTransaction, bool blockExecute, bool canAttemptReconnect, bool requiresFormReopen, string reasonCode)
        {
            return new DdlTransactionEvidenceDecision
            (
                kind,
                hasPendingTransaction,
                blockExecute,
                canAttemptReconnect,
                requiresFormReopen,
                reasonCode
            );
        }
    }
}
