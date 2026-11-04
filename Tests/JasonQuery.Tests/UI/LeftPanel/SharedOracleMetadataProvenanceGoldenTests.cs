using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2A")]
    public sealed class SharedOracleMetadataProvenanceGoldenTests
    {
        private static SharedOracleMetadataValidationContext Fresh()
        {
            var table = new DataTable("QualifiedOracleSchema");

            table.Columns.Add("OracleMetadataOwner", typeof(string));
            table.Columns.Add("OracleMetadataTableName", typeof(string));
            table.Columns.Add("OracleMetadataColumnName", typeof(string));
            table.Rows.Add("JQ98XX", "T1", "C1");

            var physicalId = Guid.NewGuid();
            var loadId = Guid.NewGuid();

            return new SharedOracleMetadataValidationContext
            {
                IsCurrentLogicalSnapshot = true,
                IsCurrentMenuAndTab = true,
                IsExplicitRecoveryClick = true,
                HasPendingOrUnknownTransaction = false,
                IsVerifiedLiveOracleSession = true,
                IsNewDeliberateActionClick = true,
                IsOriginalMenuReplay = false,
                CurrentConnectionRevision = 2,
                PreviousConnectionId = Guid.NewGuid(),
                CurrentConnectionId = physicalId,
                CurrentVerifiedSessionUser = "JQ98XX",
                CurrentVerifiedDatabaseName = "XE",
                CurrentVerifiedContainerName = "XEPDB1",
                ExpectedSessionUser = "JQ98XX",
                ExpectedDatabaseName = "XE",
                ExpectedContainerName = "XEPDB1",
                CandidateSchemaSource = table,
                Provenance = new SharedOracleMetadataProvenance
                {
                    CapturedSourceTable = table,
                    SourceType = DataSourceType.Oracle,
                    ConnectionRevision = 2,
                    ConnectionId = physicalId,
                    VerifiedSessionUser = "JQ98XX",
                    VerifiedDatabaseName = "XE",
                    VerifiedContainerName = "XEPDB1",
                    CompletedLoadId = loadId,
                    CaptureKind = SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad,
                    LoadCompletedSuccessfully = true
                },
                ExactRawTableOwnerAndNameReResolved = true,
                ExactRawColumnOwnerTableAndNameReResolved = true
            };
        }

        [TestMethod]
        public void P01_CachedSnapshotRemainsReadableAfterPhysicalDisconnect()
        {
            var context = Fresh();

            context.IsVerifiedLiveOracleSession = false;
            context.CurrentConnectionId = Guid.Empty;

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanReadOldCache(context));
        }

        [TestMethod]
        public void P02_PassiveMenuBuildNeverRequestsRecovery()
        {
            var context = Fresh();

            context.IsExplicitRecoveryClick = false;
            context.IsVerifiedLiveOracleSession = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanRequestExplicitRecovery(context));
        }

        [TestMethod]
        public void P03_ExplicitQualifiedClickCanRequestRecoveryOnly()
        {
            var context = Fresh();

            context.IsVerifiedLiveOracleSession = false;

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanRequestExplicitRecovery(context));
            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P04_PendingOrUnknownTransactionBlocksRecovery()
        {
            var context = Fresh();

            context.IsVerifiedLiveOracleSession = false;
            context.HasPendingOrUnknownTransaction = true;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanRequestExplicitRecovery(context));
        }

        [TestMethod]
        public void P05_FailedRecoveryKeepsOnlyReadOnlyCache()
        {
            var context = Fresh();

            context.IsVerifiedLiveOracleSession = false;
            context.CurrentConnectionId = Guid.Empty;

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanReadOldCache(context));
            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P06_ReconnectRequiresDistinctNonEmptyVerifiedConnectionId()
        {
            var context = Fresh();

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.HasNewPhysicalConnectionIdentity(context));

            context.PreviousConnectionId = context.CurrentConnectionId;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.HasNewPhysicalConnectionIdentity(context));

            context.PreviousConnectionId = Guid.Empty;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.HasNewPhysicalConnectionIdentity(context));
        }

        [TestMethod]
        public void P07_SameUsernameButDifferentSessionRejectsOldMetadata()
        {
            var context = Fresh();

            context.Provenance.ConnectionId = context.PreviousConnectionId;

            Assert.AreEqual(context.CurrentVerifiedSessionUser, context.Provenance.VerifiedSessionUser);
            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P08_DifferentPdbOrDatabaseScopeRejectsPublication()
        {
            var context = Fresh();

            context.CurrentVerifiedContainerName = "OTHERPDB";

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context.CurrentVerifiedContainerName = "XEPDB1";
            context.CurrentVerifiedDatabaseName = "OTHERDB";

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P09_StaleDataTableCannotBeRelabeledWithNewReaderConnectionId()
        {
            var context = Fresh();

            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.PreviewRebind;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad;
            context.CandidateSchemaSource = context.Provenance.CapturedSourceTable.Copy();

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context.CandidateSchemaSource = context.Provenance.CapturedSourceTable;

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P10_SameSessionCompletedAuthoritativeLoadCanPublish()
        {
            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(Fresh()));
        }

        [TestMethod]
        public void P11_FailedOrPartialLoadCannotPublishEvenAfterReconnect()
        {
            var context = Fresh();

            context.Provenance.LoadCompletedSuccessfully = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context.Provenance.LoadCompletedSuccessfully = true;
            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.PartialLoad;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad;

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P12_ChangedLogicalConnectionRevisionRejectsReuse()
        {
            var context = Fresh();

            context.CurrentConnectionRevision++;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void P13_FreshExactRawTableIdentityCanBeRequalified()
        {
            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanQualifyFreshTable(Fresh()));
        }

        [TestMethod]
        public void P14_FreshExactRawColumnIdentityCanBeRequalified()
        {
            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanQualifyFreshColumn(Fresh()));
        }

        [TestMethod]
        public void P15_ChangedRowOrIdentifierCaseFailsClosed()
        {
            var context = Fresh();

            context.ExactRawTableOwnerAndNameReResolved = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanQualifyFreshTable(context));

            context.ExactRawTableOwnerAndNameReResolved = true;
            context.ExactRawColumnOwnerTableAndNameReResolved = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanQualifyFreshColumn(context));

            context.ExactRawColumnOwnerTableAndNameReResolved = true;
            context.Provenance.VerifiedSessionUser = "jq98xx";

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanQualifyFreshTable(context));
        }

        [TestMethod]
        public void P16_PreReconnectCapturedMenuNeverReplaysAction()
        {
            var context = Fresh();

            context.IsOriginalMenuReplay = true;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, false));
        }

        [TestMethod]
        public void P17_RecoverySuccessNeverAutoExecutesOriginalDdl()
        {
            var context = Fresh();

            context.IsNewDeliberateActionClick = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, false));
            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, true));
        }

        [TestMethod]
        public void P18_NewDeliberatePostRefreshClickRequiresFreshQualification()
        {
            var context = Fresh();

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, false));
            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, true));

            context.IsCurrentMenuAndTab = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanDispatchNewDeliberateAction(context, false));
        }
    }
}
