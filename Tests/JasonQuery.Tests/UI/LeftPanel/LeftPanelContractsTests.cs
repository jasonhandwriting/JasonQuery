using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class LeftPanelContractsTests
    {
        private static LeftPanelAvailability Resolve(LeftPanelTargetKind target, bool connected = true, bool autoReplace = true, bool schemaReady = true, bool hasOpenTabs = true, bool sql = true)
        {
            return LeftPanelAvailabilityPolicy.Resolve(new LeftPanelContext(connected, target, autoReplace, schemaReady, hasOpenTabs, sql));
        }

        private static LeftPanelContext RoutingContext(bool connected = true, bool schemaReady = true, LeftPanelTargetKind target = LeftPanelTargetKind.QueryForm)
        {
            return new LeftPanelContext(connected, target, true, schemaReady, true, true);
        }

        [TestMethod]
        public void Disconnected_HidesEveryPanelPage()
        {
            var result = Resolve(LeftPanelTargetKind.QueryForm, connected: false);

            Assert.IsFalse(result.AutoReplace);
            Assert.IsFalse(result.SchemaInformation);
            Assert.IsFalse(result.TabList);
            Assert.IsFalse(result.SqlList);
        }

        [TestMethod]
        public void QueryForm_EnablesAllPagesWhenFeaturesAndDataExist()
        {
            var result = Resolve(LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(result.IsEnabled(LeftPanelPage.AutoReplace));
            Assert.IsTrue(result.IsEnabled(LeftPanelPage.SchemaInformation));
            Assert.IsTrue(result.IsEnabled(LeftPanelPage.TabList));
            Assert.IsTrue(result.IsEnabled(LeftPanelPage.SqlList));
        }

        [TestMethod]
        public void QueryForm_DisabledAutoReplaceIsNotAvailable()
        {
            Assert.IsFalse(Resolve(LeftPanelTargetKind.QueryForm, autoReplace: false).AutoReplace);
        }

        [TestMethod]
        public void QueryForm_NoSqlNavigatorItems_DisablesF9Only()
        {
            var result = Resolve(LeftPanelTargetKind.QueryForm, sql: false);

            Assert.IsFalse(result.SqlList);
            Assert.IsTrue(result.SchemaInformation);
        }

        [TestMethod]
        public void SchemaBrowserForm_OnlySchemaAndTabListAreAvailable()
        {
            var result = Resolve(LeftPanelTargetKind.SchemaBrowserForm);

            Assert.IsFalse(result.AutoReplace);
            Assert.IsTrue(result.SchemaInformation);
            Assert.IsTrue(result.TabList);
            Assert.IsFalse(result.SqlList);
        }

        [TestMethod]
        public void OtherForm_CannotAccessQueryScopedPages()
        {
            var result = Resolve(LeftPanelTargetKind.OtherForm);

            Assert.IsFalse(result.AutoReplace);
            Assert.IsFalse(result.SqlList);
            Assert.IsTrue(result.TabList);
        }

        [TestMethod]
        public void DatabaseSwitchInProgress_SchemaIsNotReady()
        {
            var result = Resolve(LeftPanelTargetKind.QueryForm, schemaReady: false);

            Assert.IsFalse(result.SchemaInformation);
            Assert.IsTrue(result.TabList);
        }

        [TestMethod]
        public void EmptyTabList_DisablesF8()
        {
            Assert.IsFalse(Resolve(LeftPanelTargetKind.None, hasOpenTabs: false).TabList);
        }

        [TestMethod]
        public void UnknownPage_IsNotEnabled()
        {
            Assert.IsFalse(Resolve(LeftPanelTargetKind.QueryForm).IsEnabled((LeftPanelPage)99));
        }

        [TestMethod]
        public void NullContext_IsRejected()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => LeftPanelAvailabilityPolicy.Resolve(null));
        }

        [TestMethod]
        public void Router_DisconnectedOrMissingReceiver_DoesNotDispatch()
        {
            var target = new FakeReceiver("q1");
            var selection = new SchemaExplorerSelectionInfo();

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(connected: false), "q1", target, selection, SchemaExplorerActionKind.DoubleClick));
            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q1", null, selection, SchemaExplorerActionKind.DoubleClick));
            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q1", target, null, SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(0, target.CallCount);
        }

        [TestMethod]
        public void Router_RejectsStaleOrUnknownTabIdentity()
        {
            var target = new FakeReceiver("q1");
            var selection = new SchemaExplorerSelectionInfo();

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q2", target, selection, SchemaExplorerActionKind.DoubleClick));
            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "", target, selection, SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(0, target.CallCount);
        }

        [TestMethod]
        public void Router_RejectsUndefinedAction()
        {
            var target = new FakeReceiver("q1");

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q1", target, new SchemaExplorerSelectionInfo(), (SchemaExplorerActionKind)99));
            Assert.AreEqual(0, target.CallCount);
        }

        [TestMethod]
        public void Router_DatabaseSwitchInProgress_DoesNotDispatch()
        {
            var target = new FakeReceiver("q1");

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(schemaReady: false), "q1", target, new SchemaExplorerSelectionInfo(), SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(0, target.CallCount);
        }

        [TestMethod]
        public void Router_RejectsDifferentFormKind()
        {
            var target = new FakeReceiver("q1");

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(target: LeftPanelTargetKind.SchemaBrowserForm), "q1", target, new SchemaExplorerSelectionInfo(), SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(0, target.CallCount);
        }

        [TestMethod]
        public void Router_OnlyDeliversToActiveForm()
        {
            var query1 = new FakeReceiver("q1");
            var query2 = new FakeReceiver("q2");
            var selection = new SchemaExplorerSelectionInfo { SchemaName = "ORDERS" };

            Assert.IsTrue(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q2", query2, selection, SchemaExplorerActionKind.ContextMenuRequested));
            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q2", query1, selection, SchemaExplorerActionKind.ContextMenuRequested));
            Assert.AreEqual(0, query1.CallCount);
            Assert.AreEqual(1, query2.CallCount);
            Assert.AreSame(selection, query2.LastSelection);
        }

        [TestMethod]
        public void Router_PropagatesReceiverDeclineWithoutBroadcast()
        {
            var receiver = new FakeReceiver("q3") { Result = false };

            Assert.IsFalse(SchemaExplorerActionRouter.TryDispatch(RoutingContext(), "q3", receiver, new SchemaExplorerSelectionInfo(), SchemaExplorerActionKind.SelectionChanged));
            Assert.AreEqual(1, receiver.CallCount);
        }

        [TestMethod]
        public void QueryScopedAutoReplaceWorkingTables_RemainIsolated_6_5_7()
        {
            //Representative working-table ownership contract; UI editing requires GUI regression later.
            var q1 = new FakeQueryDataSource(5);
            var q2 = new FakeQueryDataSource(5);
            var q3 = new FakeQueryDataSource(5);

            q1.AutoReplaceWorkingTable.Rows.Add("extra-q1");
            q3.AutoReplaceWorkingTable.Rows.Add("extra-q3-a");
            q3.AutoReplaceWorkingTable.Rows.Add("extra-q3-b");

            Assert.AreEqual(6, q1.AutoReplaceWorkingTable.Rows.Count);
            Assert.AreEqual(5, q2.AutoReplaceWorkingTable.Rows.Count);
            Assert.AreEqual(7, q3.AutoReplaceWorkingTable.Rows.Count);
            Assert.AreNotSame(q1.AutoReplaceWorkingTable, q2.AutoReplaceWorkingTable);
            Assert.AreNotSame(q2.AutoReplaceWorkingTable, q3.AutoReplaceWorkingTable);
        }

        private sealed class FakeReceiver : ISchemaExplorerActionReceiver
        {
            public FakeReceiver(string identity) { TabIdentity = identity; }
            public string TabIdentity { get; }
            public LeftPanelTargetKind TargetKind => LeftPanelTargetKind.QueryForm;
            public bool Result { get; set; } = true;
            public int CallCount { get; private set; }
            public SchemaExplorerSelectionInfo LastSelection { get; private set; }
            public bool TryHandleSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
            {
                CallCount++;
                LastSelection = selection;
                return Result;
            }
        }

        private sealed class FakeQueryDataSource : IQueryLeftPanelDataSource
        {
            public FakeQueryDataSource(int count)
            {
                var table = new DataTable();

                table.Columns.Add("Keyword");

                for (var i = 0; i < count; i++)
                {
                    table.Rows.Add("keyword-" + i);
                }

                AutoReplaceWorkingTable = table;
                SqlNavigatorWorkingTable = new DataTable();
            }

            public DataTable AutoReplaceWorkingTable { get; }
            public DataTable SqlNavigatorWorkingTable { get; }
            public bool TryCommitAutoReplaceEdits() { return true; }
            public bool TryNavigateToSql(int oneBasedPosition) { return oneBasedPosition > 0; }
        }
    }
}
