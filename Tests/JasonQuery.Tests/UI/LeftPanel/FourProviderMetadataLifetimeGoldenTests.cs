using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //B2c-1c-A: pin existing, source-neutral lifecycle invariants before any production change.
    //The new F7 database-refresh wiring and database-name scope tracking are NOT asserted here; those still require dedicated, implementation-facing RED tests in the next qualification.
    [TestClass]
    [TestCategory("F7L2B2C1CA")]
    public sealed class FourProviderMetadataLifetimeGoldenTests
    {
        private static readonly Guid OracleLoadConnectionId = new Guid("A1C00000-0000-4000-8000-000000000001");

        private static SharedLeftPanelHostState Connected()
        {
            return SharedLeftPanelHostStatePolicy.Refresh
            (
                SharedLeftPanelHostStatePolicy.Empty,
                true,
                true,
                "Query 1",
                LeftPanelTargetKind.QueryForm
            );
        }

        private static DataTable Source()
        {
            var table = new DataTable("Schema");

            table.Columns.Add("SchemaObject", typeof(string));
            table.Columns.Add("SchemaNode", typeof(string));
            table.Columns.Add("SchemaType", typeof(string));
            table.Columns.Add("SchemaName", typeof(string));
            table.Columns.Add("OracleMetadataOwner", typeof(string));
            table.Columns.Add("OracleMetadataTableName", typeof(string));
            table.Rows.Add("T1", "public", "Tables", "T1", "JQ98XX", "T1");

            return table;
        }

        private static SharedSchemaMetadataSnapshot Capture(DataSourceType sourceType, SharedLeftPanelHostState state)
        {
            var table = Source();
            SharedSchemaMetadataSnapshot snapshot;
            var captured = sourceType == DataSourceType.Oracle ? SharedSchemaMetadataSnapshotPolicy.TryCapture(table, sourceType, true, state.ConnectionRevision, out snapshot, "JQ98XX", OracleLoadConnectionId) : SharedSchemaMetadataSnapshotPolicy.TryCapture(table, sourceType, true, state.ConnectionRevision, out snapshot);

            Assert.IsTrue(captured);
            Assert.IsNotNull(snapshot);

            return snapshot;
        }

        private static SharedSchemaLifecycleDecision Apply(SharedSchemaLifecycleEvent kind, SharedSchemaLifecycleState state = null)
        {
            return SharedSchemaLifecyclePolicy.Apply(state ?? SharedSchemaLifecycleState.Empty, kind);
        }

        private static void QueryKeepsCache(DataSourceType provider)
        {
            var state = Connected();
            var snapshot = Capture(provider, state);
            var result = Apply(SharedSchemaLifecycleEvent.QueryCompleted);

            Assert.IsFalse(result.RequestMetadataReload);
            Assert.IsFalse(result.PublishEditorKeywords);
            Assert.IsFalse(result.State.NeedsMetadataReload);

            Assert.IsTrue(SharedSchemaLifecyclePolicy.CanReadCachedMetadata
            (
                SharedSchemaMetadataSnapshotPolicy.CanBind(state, snapshot, provider)
            ));
        }

        private static void DifferentLogicalConnectionInvalidates(DataSourceType provider)
        {
            var state = Connected();
            var snapshot = Capture(provider, state);

            var closed = SharedLeftPanelHostStatePolicy.Refresh
            (
                state, false, false, null, LeftPanelTargetKind.None
            );

            var next = SharedLeftPanelHostStatePolicy.Refresh
            (
                closed, true, true, "Query 2", LeftPanelTargetKind.QueryForm
            );

            Assert.IsTrue(next.ConnectionRevision > state.ConnectionRevision);
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(next, snapshot, provider));
        }

        [TestMethod]
        public void A01_OracleSelectKeepsCachedMetadata()
        {
            QueryKeepsCache(DataSourceType.Oracle);
        }

        [TestMethod]
        public void A02_PostgreSqlSelectKeepsCachedMetadata()
        {
            QueryKeepsCache(DataSourceType.PostgreSql);
        }

        [TestMethod]
        public void A03_SqlServerSelectKeepsCachedMetadata()
        {
            QueryKeepsCache(DataSourceType.SqlServer);
        }

        [TestMethod]
        public void A04_MySqlSelectKeepsCachedMetadata()
        {
            QueryKeepsCache(DataSourceType.MySql);
        }

        [TestMethod]
        public void A05_PhysicalReconnectDoesNotRewriteLogicalHostRevision()
        {
            var before = Connected();

            var afterQuery = SharedLeftPanelHostStatePolicy.Refresh
            (
                before, true, true, "Query 1", LeftPanelTargetKind.QueryForm
            );

            //Physical Reader.Close/Open is not a MainForm logical-disconnect event.
            Assert.AreSame(before, afterQuery);
            Assert.AreEqual(before.ConnectionRevision, afterQuery.ConnectionRevision);

            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind
                (
                    afterQuery, Capture(provider, before), provider
                ));
            }
        }

        [TestMethod]
        public void A06_QueryTabSwitchKeepsSameLogicalSnapshot()
        {
            var before = Connected();

            var next = SharedLeftPanelHostStatePolicy.Refresh
            (
                before, true, true, "Query 2", LeftPanelTargetKind.QueryForm
            );

            Assert.AreEqual(before.ConnectionRevision, next.ConnectionRevision);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind
            (
                next, Capture(DataSourceType.PostgreSql, before), DataSourceType.PostgreSql
            ));
        }

        [TestMethod]
        public void A07_PostgreSqlNewLogicalConnectionRejectsOldSnapshot()
        {
            DifferentLogicalConnectionInvalidates(DataSourceType.PostgreSql);
        }

        [TestMethod]
        public void A08_SqlServerNewLogicalConnectionRejectsOldSnapshot()
        {
            DifferentLogicalConnectionInvalidates(DataSourceType.SqlServer);
        }

        [TestMethod]
        public void A09_MySqlNewLogicalConnectionRejectsOldSnapshot()
        {
            DifferentLogicalConnectionInvalidates(DataSourceType.MySql);
        }

        [TestMethod]
        public void A10_OracleNewLogicalConnectionRejectsOldSnapshot()
        {
            DifferentLogicalConnectionInvalidates(DataSourceType.Oracle);
        }

        [TestMethod]
        public void A11_DifferentProviderNeverReusesSnapshot()
        {
            var state = Connected();
            var snapshot = Capture(DataSourceType.PostgreSql, state);

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind
            (
                state, snapshot, DataSourceType.SqlServer
            ));
        }

        [TestMethod]
        public void A12_ManualRefreshPublishesKeywordsOnlyAfterSuccessfulReload()
        {
            var requested = Apply(SharedSchemaLifecycleEvent.ManualRefreshRequested);

            Assert.IsTrue(requested.RequestMetadataReload);
            Assert.IsFalse(requested.PublishEditorKeywords);

            var complete = Apply(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, requested.State);

            Assert.IsTrue(complete.PublishEditorKeywords);
            Assert.IsFalse(complete.State.NeedsMetadataReload);
        }

        [TestMethod]
        public void A13_FailedManualRefreshRetainsPreviouslyBoundSnapshot()
        {
            var state = Connected();
            var snapshot = Capture(DataSourceType.MySql, state);
            var request = Apply(SharedSchemaLifecycleEvent.ManualRefreshRequested);
            var failed = Apply(SharedSchemaLifecycleEvent.MetadataReloadFailed, request.State);

            Assert.IsTrue(failed.State.NeedsMetadataReload);
            Assert.IsFalse(failed.PublishEditorKeywords);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind
            (
                state, snapshot, DataSourceType.MySql
            ));
        }

        [TestMethod]
        public void A14_OracleImplicitCommittedStructuralDdlRequestsOneReload()
        {
            var committed = Apply(SharedSchemaLifecycleEvent.StructuralDdlImplicitCommitted);

            Assert.IsTrue(committed.RequestMetadataReload);

            var complete = Apply(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, committed.State);

            Assert.IsTrue(complete.PublishEditorKeywords);
            Assert.IsFalse(Apply(SharedSchemaLifecycleEvent.QueryCompleted, complete.State).RequestMetadataReload);
        }

        [TestMethod]
        public void A15_TransactionalDdlCoalescesUntilCommit()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer })
            {
                var first = Apply(SharedSchemaLifecycleEvent.StructuralDdlPending);
                var second = Apply(SharedSchemaLifecycleEvent.StructuralDdlPending, first.State);

                Assert.IsFalse(first.RequestMetadataReload, provider.ToString());
                Assert.IsFalse(second.RequestMetadataReload, provider.ToString());

                var committed = Apply(SharedSchemaLifecycleEvent.CommitSucceeded, second.State);

                Assert.IsTrue(committed.RequestMetadataReload, provider.ToString());

                var complete = Apply(SharedSchemaLifecycleEvent.MetadataReloadSucceeded, committed.State);

                Assert.IsFalse(Apply(SharedSchemaLifecycleEvent.CommitSucceeded, complete.State).RequestMetadataReload);
            }
        }

        [TestMethod]
        public void A16_PendingDdlRollbackRetainsCommittedMetadata()
        {
            foreach (var provider in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer })
            {
                var pending = Apply(SharedSchemaLifecycleEvent.StructuralDdlPending);
                var rolledBack = Apply(SharedSchemaLifecycleEvent.RollbackSucceeded, pending.State);

                Assert.IsFalse(rolledBack.RequestMetadataReload, provider.ToString());
                Assert.IsFalse(rolledBack.State.HasPendingStructuralChanges);
                Assert.IsFalse(rolledBack.State.NeedsMetadataReload);
            }
        }

        [TestMethod]
        public void A17_ManualRefreshDuringPendingOrUnknownTransactionIsDeferred()
        {
            var pending = Apply(SharedSchemaLifecycleEvent.StructuralDdlPending);
            var uncertain = Apply(SharedSchemaLifecycleEvent.TransactionOutcomeUnknown);

            Assert.IsFalse(Apply(SharedSchemaLifecycleEvent.ManualRefreshRequested, pending.State).RequestMetadataReload);
            Assert.IsFalse(Apply(SharedSchemaLifecycleEvent.ManualRefreshRequested, uncertain.State).RequestMetadataReload);
        }

        [TestMethod]
        public void A18_OracleCachedViewMayRemainReadableWithoutLiveDdlAuthorization()
        {
            var state = Connected();
            var previous = Capture(DataSourceType.Oracle, state);

            Assert.IsTrue(SharedSchemaLifecyclePolicy.CanReadCachedMetadata
            (
                SharedSchemaMetadataSnapshotPolicy.CanBind(state, previous, DataSourceType.Oracle)
            ));

            Assert.IsFalse(SharedSchemaLifecyclePolicy.CanRunSessionBoundAction(true, false));

            //A newly supplied empty physical identity must NOT certify a fresh Oracle capture.
            SharedSchemaMetadataSnapshot untrusted;

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture
            (
                Source(), DataSourceType.Oracle, true, state.ConnectionRevision,
                out untrusted, string.Empty, Guid.Empty
            ));
        }

        [TestMethod]
        public void A19_CapturingSnapshotDoesNotMutateSourceRows()
        {
            var state = Connected();
            var original = Source();
            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture
            (
                original, DataSourceType.PostgreSql, true, state.ConnectionRevision, out snapshot
            ));

            original.Rows[0]["SchemaName"] = "CHANGED";

            Assert.AreEqual("T1", snapshot.Table.Rows[0]["SchemaName"]);
        }

        [TestMethod]
        public void A20_IncompleteMetadataCannotReplaceSnapshot()
        {
            var state = Connected();
            var incomplete = Source();

            incomplete.Columns.Remove("SchemaType");

            SharedSchemaMetadataSnapshot rejected;

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture
            (
                incomplete, DataSourceType.SqlServer, true, state.ConnectionRevision, out rejected
            ));

            Assert.IsNull(rejected);
        }
    }
}
