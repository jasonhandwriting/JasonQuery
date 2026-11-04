using JasonQuery.Core.Database.Connection;
using System;

namespace JasonQuery.UI.LeftPanel
{
    //Read-only F7 startup selection; distinct from metadata provenance and DDL authorization.
    internal sealed class SharedSchemaExplorerStartupDecisionContext
    {
        internal SharedLeftPanelHostState HostState { get; set; }
        internal DataSourceType SourceType { get; set; }
        internal bool WorkspaceVisible { get; set; }
        internal int WorkspaceWidth { get; set; }
        internal string CurrentDatabaseScope { get; set; }
        internal string PublishedDatabaseScope { get; set; }
        internal int PublishedConnectionRevision { get; set; }
        internal bool HasTrustedPublishedMetadata { get; set; }
        internal bool HasPendingOrUnknownTransaction { get; set; }
        internal bool IsInitialLoad { get; set; }
        internal bool UserSelectedDifferentLeftTab { get; set; }
        internal bool AlreadyAutoSelectedForConnection { get; set; }
    }

    internal static class SharedSchemaExplorerStartupDecisionPolicy
    {
        internal static bool MayAutoSelectF7(SharedSchemaExplorerStartupDecisionContext request)
        {
            if (request == null || request.HostState == null)
            {
                return false;
            }

            var state = request.HostState;

            if (state.IsDisposed || !state.IsConnected || !state.HasActiveTab || request.SourceType == DataSourceType.None || !request.WorkspaceVisible || request.WorkspaceWidth <= 0)
            {
                return false;
            }

            if (!request.HasTrustedPublishedMetadata || request.HasPendingOrUnknownTransaction || !request.IsInitialLoad || request.UserSelectedDifferentLeftTab || request.AlreadyAutoSelectedForConnection)
            {
                return false;
            }

            if (request.PublishedConnectionRevision <= 0 || request.PublishedConnectionRevision != state.ConnectionRevision || string.IsNullOrWhiteSpace(request.CurrentDatabaseScope) || !string.Equals(request.CurrentDatabaseScope, request.PublishedDatabaseScope, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
