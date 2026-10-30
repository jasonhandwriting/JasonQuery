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
