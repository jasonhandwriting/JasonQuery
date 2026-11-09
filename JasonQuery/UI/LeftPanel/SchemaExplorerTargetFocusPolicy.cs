using System;

namespace JasonQuery.UI.LeftPanel
{
    //Pure GroupBy focus decisions; independent of provider, DDL action and C1 row indices.
    internal static class SchemaExplorerTargetFocusPolicy
    {
        internal static string[] BuildRetainedGroupPath(string[] capturedGroups, bool sameScope, bool reloadSucceeded)
        {
            if (!sameScope || !reloadSucceeded || capturedGroups == null || capturedGroups.Length < 2)
            {
                return Array.Empty<string>();
            }

            foreach (var name in capturedGroups)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Array.Empty<string>();
                }
            }

            return (string[])capturedGroups.Clone();
        }

        internal static int FindNearestFocusIndex(string originalName, int previousOrdinal, string[] remainingNames)
        {
            if (remainingNames == null || remainingNames.Length == 0)
            {
                return -1;
            }

            //Preserve exact identifier case and restore a still-present object first.
            if (!string.IsNullOrWhiteSpace(originalName))
            {
                for (var i = 0; i < remainingNames.Length; i++)
                {
                    if (string.Equals(originalName, remainingNames[i], StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
            }

            //A removed/renamed object has no exact match. Choose the nearest same-parent ordinal.
            return previousOrdinal < 0 ? -1 : Math.Min(previousOrdinal, remainingNames.Length - 1);
        }

        internal static bool ShouldApplyAfterBinding(bool trustedBefore, bool sameScope, bool bindingSucceeded, bool metadataTrustedAfter)
        {
            return trustedBefore && sameScope && bindingSucceeded && metadataTrustedAfter;
        }
    }
}
