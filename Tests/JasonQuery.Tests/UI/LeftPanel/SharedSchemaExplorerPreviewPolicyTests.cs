using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaExplorerPreviewPolicyTests
    {
        private static SharedLeftPanelHostState Ready()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, true, true, "q1", LeftPanelTargetKind.QueryForm);
        }

        private static SharedLeftPanelHostState WithoutTab()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, true, false, null, LeftPanelTargetKind.None);
        }

        private static SharedLeftPanelHostState Disconnected()
        {
            return SharedLeftPanelHostStatePolicy.Empty;
        }

        private static SharedLeftPanelHostState ZeroRevision()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, false, true, "q1", LeftPanelTargetKind.QueryForm);
        }

        private static SharedLeftPanelHostState Disposed()
        {
            return SharedLeftPanelHostStatePolicy.MarkDisposed(Ready());
        }

        private static SharedLeftPanelHostState OtherForm()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, true, true, "other", LeftPanelTargetKind.OtherForm);
        }

        private static SharedLeftPanelHostState NoneTarget()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, true, true, "no-target", LeftPanelTargetKind.None);
        }

        private static SharedLeftPanelHostState Browser()
        {
            return SharedLeftPanelHostStatePolicy.Refresh(SharedLeftPanelHostStatePolicy.Empty, true, true, "browser", LeftPanelTargetKind.SchemaBrowserForm);
        }

        private static SharedSchemaMetadataSnapshot Snapshot()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaObject");
            table.Columns.Add("SchemaNode");
            table.Columns.Add("SchemaType");

            SharedSchemaMetadataSnapshot snapshot;
            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(table, DataSourceType.PostgreSql, false, Ready().ConnectionRevision, out snapshot));

            return snapshot;
        }

        [TestMethod]
        public void PreviewBoundsStartBelowHeader()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(218, 710, 57);

            Assert.AreEqual(0, result.X);
            Assert.AreEqual(57, result.Y);
            Assert.AreEqual(218, result.Width);
            Assert.AreEqual(653, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsExcludeHeaderHeight()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(218, 677, 32);

            Assert.AreEqual(32, result.Y);
            Assert.AreEqual(645, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsWithoutHeaderFillHost()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(220, 700, 0);

            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(700, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsDoNotExtendPastVeryShortHost()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(200, 40, 57);

            Assert.AreEqual(40, result.Y);
            Assert.AreEqual(0, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsClampNegativeHostDimensions()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(-100, -10, 57);

            Assert.AreEqual(0, result.Width);
            Assert.AreEqual(0, result.Height);
            Assert.AreEqual(0, result.Y);
        }

        [TestMethod]
        public void PreviewBoundsClampNegativeHeaderHeight()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(200, 300, -5);

            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(300, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsFollowHostResize()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(260, 900, 57);

            Assert.AreEqual(260, result.Width);
            Assert.AreEqual(843, result.Height);
        }

        [TestMethod]
        public void PreviewBoundsAllowZeroWidthButMaintainVerticalOffset()
        {
            var result = SharedSchemaExplorerPreviewPolicy.GetGridBounds(0, 500, 57);

            Assert.AreEqual(0, result.Width);
            Assert.AreEqual(57, result.Y);
            Assert.AreEqual(443, result.Height);
        }

        [TestMethod]
        public void OracleQueryIsAvailable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.Oracle, true, 1008));
        }

        [TestMethod]
        public void PostgreSqlQueryIsAvailable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.PostgreSql, true, 1008));
        }

        [TestMethod]
        public void SqlServerQueryIsAvailable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.SqlServer, true, 1008));
        }

        [TestMethod]
        public void MySqlQueryIsAvailable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.MySql, true, 1008));
        }

        [TestMethod]
        public void AtMinimumWidthIsAvailable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.Oracle, true, 890));
        }

        [TestMethod]
        public void NarrowWindowDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.Oracle, true, 889));
        }

        [TestMethod]
        public void VeryNarrowWindowDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.Oracle, true, 500));
        }

        [TestMethod]
        public void WorkspaceHiddenDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.Oracle, false, 1008));
        }

        [TestMethod]
        public void UndefinedSourceDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Ready(), DataSourceType.None, true, 1008));
        }

        [TestMethod]
        public void NoTabDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(WithoutTab(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void DisconnectedDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Disconnected(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void NoConnectionRevisionDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(ZeroRevision(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void DisposedDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(Disposed(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void OtherFormDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(OtherForm(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void NoneTargetDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(NoneTarget(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void SchemaBrowserAllowed()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanOffer(Browser(), DataSourceType.PostgreSql, true, 1000));
        }

        [TestMethod]
        public void NoSnapshotNotReusable()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanReuse(Ready(), null, DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void WrongEngineNotReusable()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanReuse(Ready(), Snapshot(), DataSourceType.Oracle));
        }

        [TestMethod]
        public void ReopeningPreviewWithBoundGridPreservesGroupState()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Ready(), Snapshot(), DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void ReopeningPreviewWithoutBoundGridRefreshes()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Ready(), Snapshot(), DataSourceType.PostgreSql, false));
        }

        [TestMethod]
        public void ReopeningPreviewWithoutSnapshotRefreshes()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Ready(), null, DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void ReopeningPreviewAfterEngineChangeRefreshes()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Ready(), Snapshot(), DataSourceType.Oracle, true));
        }

        [TestMethod]
        public void ReopeningPreviewAfterDisconnectRefreshes()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Disconnected(), Snapshot(), DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void ReopeningPreviewAfterReconnectRefreshes()
        {
            var previous = Ready();
            var disconnected = SharedLeftPanelHostStatePolicy.Refresh(previous, false, false, null, LeftPanelTargetKind.None);
            var reconnected = SharedLeftPanelHostStatePolicy.Refresh(disconnected, true, true, "q1", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(reconnected, Snapshot(), DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void ReopeningPreviewInAnotherTabInSameConnectionPreservesGroupState()
        {
            var otherTab = SharedLeftPanelHostStatePolicy.Refresh(Ready(), true, true, "q2", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(otherTab, Snapshot(), DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void ReopeningPreviewWithDisposedStateRefreshes()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldRefreshOnShow(Disposed(), Snapshot(), DataSourceType.PostgreSql, true));
        }

        [TestMethod]
        public void MatchingSnapshotReusable()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanReuse(Ready(), Snapshot(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void MissingStateDoesNotReuse()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanReuse(null, Snapshot(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void MinimumWidthConstantIsGreaterThanPreviewWidth()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.MinimumWorkspaceWidth > SharedSchemaExplorerPreviewPolicy.PreviewWidth);
        }

        [TestMethod]
        public void MinimumWidthLeavesUsableEditor()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.MinimumWorkspaceWidth - SharedSchemaExplorerPreviewPolicy.PreviewWidth >= 650);
        }

        [TestMethod]
        public void ConnectionRevisionChangesPreventReuse()
        {
            var previous = Ready();
            var next = SharedLeftPanelHostStatePolicy.Refresh(previous, false, false, null, LeftPanelTargetKind.None);

            next = SharedLeftPanelHostStatePolicy.Refresh(next, true, true, "q1", LeftPanelTargetKind.QueryForm);
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanReuse(next, Snapshot(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void TabIdentityChangePreservesReuse()
        {
            var next = SharedLeftPanelHostStatePolicy.Refresh(Ready(), true, true, "q2", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.CanReuse(next, Snapshot(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void SnapshotRevisionMustMatch()
        {
            var next = SharedLeftPanelHostStatePolicy.Refresh(Ready(), false, false, null, LeftPanelTargetKind.None);

            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanReuse(next, Snapshot(), DataSourceType.PostgreSql));
        }
    }
}
