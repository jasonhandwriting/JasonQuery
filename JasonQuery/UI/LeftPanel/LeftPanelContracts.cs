using JasonQuery.Core.SchemaExplorer.Selection;
using System;
using System.Data;

namespace JasonQuery.UI.LeftPanel
{
    //These are contracts only. The v0.97 form layout and message paths remain unchanged in B1-A.
    public enum LeftPanelPage
    {
        AutoReplace = 6,
        SchemaInformation = 7,
        TabList = 8,
        SqlList = 9
    }

    public enum LeftPanelTargetKind
    {
        None = 0,
        QueryForm = 1,
        SchemaBrowserForm = 2,
        OtherForm = 3
    }

    public enum SchemaExplorerActionKind
    {
        SelectionChanged = 1,
        DoubleClick = 2,
        ContextMenuRequested = 3
    }

    //Snapshot of the MainForm workspace at the instant a page is requested.
    //Do not retain a stale snapshot across a tab change or database switch.
    public sealed class LeftPanelContext
    {
        public LeftPanelContext(bool isConnected, LeftPanelTargetKind activeTarget, bool autoReplaceEnabled, bool schemaReady, bool hasOpenTabs, bool hasSqlStatements)
        {
            IsConnected = isConnected;
            ActiveTarget = activeTarget;
            AutoReplaceEnabled = autoReplaceEnabled;
            SchemaReady = schemaReady;
            HasOpenTabs = hasOpenTabs;
            HasSqlStatements = hasSqlStatements;
        }

        public bool IsConnected { get; }
        public LeftPanelTargetKind ActiveTarget { get; }
        public bool AutoReplaceEnabled { get; }
        public bool SchemaReady { get; }
        public bool HasOpenTabs { get; }
        public bool HasSqlStatements { get; }
    }

    public sealed class LeftPanelAvailability
    {
        public LeftPanelAvailability(bool autoReplace, bool schemaInformation, bool tabList, bool sqlList)
        {
            AutoReplace = autoReplace;
            SchemaInformation = schemaInformation;
            TabList = tabList;
            SqlList = sqlList;
        }

        public bool AutoReplace { get; }
        public bool SchemaInformation { get; }
        public bool TabList { get; }
        public bool SqlList { get; }

        public bool IsEnabled(LeftPanelPage page)
        {
            switch (page)
            {
                case LeftPanelPage.AutoReplace:
                    {
                        return AutoReplace;
                    }
                case LeftPanelPage.SchemaInformation:
                    {
                        return SchemaInformation;
                    }
                case LeftPanelPage.TabList:
                    {
                        return TabList;
                    }
                case LeftPanelPage.SqlList:
                    {
                        return SqlList;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }

    public static class LeftPanelAvailabilityPolicy
    {
        public static LeftPanelAvailability Resolve(LeftPanelContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //Before a successful connection, MainForm's lower workspace is hidden.
            if (!context.IsConnected)
            {
                return new LeftPanelAvailability(false, false, false, false);
            }

            var isQuery = context.ActiveTarget == LeftPanelTargetKind.QueryForm;

            return new LeftPanelAvailability(isQuery && context.AutoReplaceEnabled, context.SchemaReady, context.HasOpenTabs, isQuery && context.HasSqlStatements);
        }
    }

    //Implemented per QueryForm in a later phase. The returned working tables must belong to that QueryForm; the MainForm grid never owns the AutoReplace dictionary.
    public interface IQueryLeftPanelDataSource
    {
        DataTable AutoReplaceWorkingTable { get; }
        DataTable SqlNavigatorWorkingTable { get; }
        bool TryCommitAutoReplaceEdits();
        bool TryNavigateToSql(int oneBasedPosition);
    }

    //A form receives actions only if it remains the active tab when dispatch occurs.
    //The Explorer itself handles expanding/collapsing group rows.
    public interface ISchemaExplorerActionReceiver
    {
        string TabIdentity { get; }
        LeftPanelTargetKind TargetKind { get; }
        bool TryHandleSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action);
    }

    public static class SchemaExplorerActionRouter
    {
        public static bool TryDispatch(LeftPanelContext context, string activeTabIdentity, ISchemaExplorerActionReceiver receiver, SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
        {
            if (context == null || !context.IsConnected || !context.SchemaReady || receiver == null || selection == null || string.IsNullOrWhiteSpace(activeTabIdentity))
            {
                return false;
            }

            if (context.ActiveTarget != receiver.TargetKind || !string.Equals(activeTabIdentity, receiver.TabIdentity, StringComparison.Ordinal))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(SchemaExplorerActionKind), action))
            {
                return false;
            }

            return receiver.TryHandleSchemaExplorerAction(selection, action);
        }
    }
}
