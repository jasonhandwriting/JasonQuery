using System;
using System.Collections.Generic;

namespace JasonQuery.UI.LeftPanel
{
    //The MainForm implements this interface; child forms request tab navigation without string commands.
    public interface IWorkspaceTabNavigator
    {
        bool TryActivateTab(string tabIdentity);
    }

    public static class WorkspaceTabNavigationPolicy
    {
        //The identity is the existing AccessibleDescription (case-sensitive, not the displayed tab title).
        //Return -1 for stale, invalid, missing, or disconnected requests; do not guess a target.
        public static int FindTargetIndex(IReadOnlyList<string> tabIdentities, string tabIdentity)
        {
            if (tabIdentities == null || string.IsNullOrWhiteSpace(tabIdentity))
            {
                return -1;
            }

            for (var index = 0; index < tabIdentities.Count; index++)
            {
                if (string.Equals(tabIdentities[index], tabIdentity, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
