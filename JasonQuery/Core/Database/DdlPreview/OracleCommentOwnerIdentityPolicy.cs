using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.Core.Database.DdlPreview
{
    /// <summary>
    /// Oracle Comment-only Owner identity. Metadata matching and Comment DDL
    /// must resolve the same name; other DDL retains the original quoting policy.
    /// The caller must preserve explicit double quotes for case-sensitive Owners.
    /// </summary>
    internal static class OracleCommentOwnerIdentityPolicy
    {
        public static string ResolveMetadataOwner(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName))
            {
                return string.Empty;
            }

            var isQuoted = schemaName.Length >= 2 && schemaName[0] == '"' && schemaName[schemaName.Length - 1] == '"';

            //A malformed or ambiguous delimiter must not silently select a different Owner.
            if (!isQuoted && schemaName.IndexOf('"') >= 0)
            {
                return string.Empty;
            }

            var value = DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(DataSourceType.Oracle, schemaName);

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (isQuoted)
            {
                //Require a round-trip valid quoted identifier, not a malformed one that GetExistingIdentifierValue would otherwise preserve verbatim.
                var correctlyQuoted = "\"" + value.Replace("\"", "\"\"") + "\"";

                if (!string.Equals(schemaName, correctlyQuoted, StringComparison.Ordinal))
                {
                    return string.Empty;
                }

                return value;
            }

            return value.ToUpperInvariant();
        }

        public static string QuoteDdlOwner(string schemaName)
        {
            var owner = ResolveMetadataOwner(schemaName);

            return string.IsNullOrEmpty(owner) ? string.Empty : "\"" + owner.Replace("\"", "\"\"") + "\"";
        }
    }
}
