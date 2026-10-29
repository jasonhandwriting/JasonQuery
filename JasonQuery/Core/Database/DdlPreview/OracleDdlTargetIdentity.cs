using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    /// <summary>
    /// Evidence supplied by a qualified Oracle selection and the current verified session.
    /// This is a request contract, NOT proof that the GUI has revalidated a live connection.
    /// Only the later Preview/Execute gate may populate it from the active OracleReader.
    /// Never populate these fields using login text, a displayed schema, or DbUser.
    /// </summary>
    internal sealed class OracleDdlTargetIdentity
    {
        public string VerifiedSessionUser { get; set; }
        public string MetadataOwner { get; set; }
        public string SelectedOwner { get; set; }
        public Guid VerifiedConnectionId { get; set; }
        public Guid SelectionConnectionId { get; set; }
        public Guid CurrentConnectionId { get; set; }
        public bool HasQualifiedSelectedOwner { get; set; }
    }

    internal static class OracleDdlTargetIdentityPolicy
    {
        public static bool TryQuoteOwner(OracleDdlTargetIdentity identity, out string quotedOwner)
        {
            quotedOwner = string.Empty;

            if (identity == null || !identity.HasQualifiedSelectedOwner
                || identity.VerifiedConnectionId == Guid.Empty
                || identity.SelectionConnectionId == Guid.Empty
                || identity.CurrentConnectionId == Guid.Empty
                || identity.VerifiedConnectionId != identity.SelectionConnectionId
                || identity.VerifiedConnectionId != identity.CurrentConnectionId)
            {
                return false;
            }

            var owner = identity.MetadataOwner;

            if (!IsValidRawOwner(owner) || !string.Equals(identity.VerifiedSessionUser, owner, StringComparison.Ordinal) || !string.Equals(identity.SelectedOwner, owner, StringComparison.Ordinal))
            {
                return false;
            }

            //MetadataOwner is an exact database value, NOT an unquoted input name.
            //Never normalize casing or parse delimiters a second time.
            quotedOwner = "\"" + owner.Replace("\"", "\"\"") + "\"";
            return true;
        }

        private static bool IsValidRawOwner(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                return false;
            }

            foreach (var character in owner)
            {
                if (char.IsControl(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
