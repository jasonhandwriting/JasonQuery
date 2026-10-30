using System;

namespace JasonQuery.UI.LeftPanel
{
    //B1-C3A: logical state only; no Form, Control, Grid, DataTable or DB reference.
    public sealed class SharedLeftPanelHostState
    {
        internal SharedLeftPanelHostState(bool isConnected, bool hasOpenTabs, string activeTabIdentity, LeftPanelTargetKind activeTarget, int revision, int connectionRevision, bool isDisposed)
        {
            IsConnected = isConnected;
            HasOpenTabs = hasOpenTabs;
            ActiveTabIdentity = activeTabIdentity;
            ActiveTarget = activeTarget;
            Revision = revision;
            ConnectionRevision = connectionRevision;
            IsDisposed = isDisposed;
        }

        public bool IsConnected { get; }
        public bool HasOpenTabs { get; }
        public string ActiveTabIdentity { get; }
        public LeftPanelTargetKind ActiveTarget { get; }
        public int Revision { get; }
        public int ConnectionRevision { get; }
        public bool IsDisposed { get; }
        public bool HasActiveTab => IsConnected && HasOpenTabs && !string.IsNullOrEmpty(ActiveTabIdentity);
    }

    public static class SharedLeftPanelHostStatePolicy
    {
        public static SharedLeftPanelHostState Empty => new SharedLeftPanelHostState
        (
            false, false, string.Empty, LeftPanelTargetKind.None, 0, 0, false
        );

        public static SharedLeftPanelHostState Refresh(SharedLeftPanelHostState previous, bool connected, bool hasOpenTabs, string activeTabIdentity, LeftPanelTargetKind activeTarget)
        {
            var old = previous ?? Empty;

            if (old.IsDisposed)
            {
                return old;
            }

            //A disconnected workspace must not retain an identity from a prior connection.
            var availableTabs = connected && hasOpenTabs;
            var hasIdentity = availableTabs && !string.IsNullOrWhiteSpace(activeTabIdentity);
            var identity = hasIdentity ? activeTabIdentity : string.Empty;
            var target = hasIdentity ? activeTarget : LeftPanelTargetKind.None;

            if (old.IsConnected == connected && old.HasOpenTabs == availableTabs &&
                string.Equals(old.ActiveTabIdentity, identity, StringComparison.Ordinal) && old.ActiveTarget == target)
            {
                return old;
            }

            return new SharedLeftPanelHostState
            (
                connected,
                availableTabs,
                identity,
                target,
                old.Revision + 1,
                old.ConnectionRevision + (old.IsConnected == connected ? 0 : 1),
                false
            );
        }

        public static SharedLeftPanelHostState MarkDisposed(SharedLeftPanelHostState previous)
        {
            var old = previous ?? Empty;

            if (old.IsDisposed)
            {
                return old;
            }

            return new SharedLeftPanelHostState
            (
                false, false, string.Empty, LeftPanelTargetKind.None,
                old.Revision + 1, old.ConnectionRevision, true
            );
        }
    }
}
