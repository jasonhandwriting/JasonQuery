using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    /// <summary>
    /// Rechecks the qualified Oracle target against the current OracleReader identity.
    /// A caller must obtain the original selection from authoritative metadata;
    /// this class cannot establish provenance for a legacy, display-only row.
    /// </summary>
    internal static class OracleDdlGuiGate
    {
        //Q2-98B1: UI availability depends on SQL content, not on an already-open Oracle session.
        //This grants no database authorization; the live checks run only on an explicit Execute click.
        public static bool CanOfferExecuteSql(string sql)
        {
            return !string.IsNullOrWhiteSpace(sql);
        }

        //Q2-98B1: small orchestration contract. Callbacks supplied by the production form must do actual live scope/target verification and execute only on the verified connection.
        //Empty result means success; nonempty result is an explicit, user-visible failure reason.
        public static string TryExecuteF7(bool hasPendingOrUnknownTransaction, Func<string> ensureConnection, Func<string> verifyLiveScopeTargetAndSql, Func<string> executeOnVerifiedConnection)
        {
            if (hasPendingOrUnknownTransaction)
            {
                return "Q298_TRANSACTION_PENDING";
            }

            if (ensureConnection == null || verifyLiveScopeTargetAndSql == null || executeOnVerifiedConnection == null)
            {
                return "Q298_INVALID_GATE";
            }

            var result = ensureConnection();

            if (result == null)
            {
                return "Q298_CONNECTION_FAILED";
            }

            if (result.Length != 0)
            {
                return result;
            }

            result = verifyLiveScopeTargetAndSql();

            if (result == null)
            {
                return "Q298_VERIFICATION_FAILED";
            }

            if (result.Length != 0)
            {
                return result;
            }

            result = executeOnVerifiedConnection();
            return result ?? "Q298_EXECUTION_FAILED";
        }

        public static bool TryRefresh(OracleDdlTargetIdentity selection, string currentVerifiedUser, Guid currentVerifiedConnectionId, out OracleDdlTargetIdentity refreshed)
        {
            refreshed = null;

            if (selection == null || string.IsNullOrWhiteSpace(currentVerifiedUser)
                || currentVerifiedConnectionId == Guid.Empty
                || !string.Equals(selection.VerifiedSessionUser, currentVerifiedUser, StringComparison.Ordinal)
                || selection.VerifiedConnectionId != currentVerifiedConnectionId
                || selection.SelectionConnectionId != currentVerifiedConnectionId)
            {
                return false;
            }

            //Build a new immutable-by-convention request snapshot. Never edit the original selection or trust a caller-supplied CurrentConnectionId.
            var candidate = new OracleDdlTargetIdentity
            {
                VerifiedSessionUser = currentVerifiedUser,
                MetadataOwner = selection.MetadataOwner,
                SelectedOwner = selection.SelectedOwner,
                VerifiedConnectionId = currentVerifiedConnectionId,
                SelectionConnectionId = selection.SelectionConnectionId,
                CurrentConnectionId = currentVerifiedConnectionId,
                HasQualifiedSelectedOwner = selection.HasQualifiedSelectedOwner
            };

            if (!OracleDdlTargetIdentityPolicy.TryQuoteOwner(candidate, out _))
            {
                return false;
            }

            refreshed = candidate;
            return true;
        }

        public static bool CanExecute(OracleDdlTargetIdentity selection, string currentVerifiedUser, Guid currentVerifiedConnectionId, string displayedPreview, string rebuiltPreview)
        {
            if (!TryRefresh(selection, currentVerifiedUser, currentVerifiedConnectionId, out _) || string.IsNullOrWhiteSpace(displayedPreview) || string.IsNullOrWhiteSpace(rebuiltPreview))
            {
                return false;
            }

            //Byte-for-byte equality also rejects a forged/truncated SQL preview.
            return string.Equals(displayedPreview, rebuiltPreview, StringComparison.Ordinal);
        }
    }
}
