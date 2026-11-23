using System;
using System.Collections.Generic;
using System.Data;

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

    public static class SharedTabListSnapshotPolicy
    {
        public static DataTable Create(DataTable source)
        {
            var snapshot = new DataTable();

            snapshot.Columns.Add("Tab", typeof(string));
            snapshot.Columns.Add("AccessibleDescription", typeof(string));

            if (source == null || !source.Columns.Contains("Tab") || !source.Columns.Contains("AccessibleDescription"))
            {
                return snapshot;
            }

            foreach (DataRow row in source.Rows)
            {
                if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
                {
                    continue;
                }

                var title = row.IsNull("Tab") ? string.Empty : row["Tab"].ToString();
                var identity = row.IsNull("AccessibleDescription") ? string.Empty : row["AccessibleDescription"].ToString();

                snapshot.Rows.Add(title, identity);
            }

            return snapshot;
        }
    }
}
