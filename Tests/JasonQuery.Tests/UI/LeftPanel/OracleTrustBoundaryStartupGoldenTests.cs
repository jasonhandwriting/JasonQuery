using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //B2c-3B1: only Golden contracts. Synthetic provenance verifies consistency, NOT the authority to issue a completed Oracle metadata load receipt.
    [TestClass]
    [TestCategory("F7B2C3B1")]
    public sealed class OracleTrustBoundaryStartupGoldenTests
    {
        private static SharedOracleMetadataValidationContext ConsistentSyntheticContext()
        {
            var schema = new DataTable("SyntheticOracleMetadata");

            schema.Columns.Add("SchemaObject", typeof(string));
            schema.Columns.Add("SchemaType", typeof(string));
            schema.Columns.Add("SchemaName", typeof(string));
            schema.Columns.Add("OracleMetadataOwner", typeof(string));
            schema.Columns.Add("OracleMetadataTableName", typeof(string));
            schema.Columns.Add("OracleMetadataColumnName", typeof(string));
            schema.Rows.Add("JQ98_TEST", "Tables", "T1", "JQ98XX", "T1", "C1");

            var connectionId = new Guid("8B3B1000-0000-4000-8000-000000000001");

            return new SharedOracleMetadataValidationContext
            {
                IsCurrentLogicalSnapshot = true,
                IsCurrentMenuAndTab = true,
                HasPendingOrUnknownTransaction = false,
                IsVerifiedLiveOracleSession = true,
                CurrentConnectionRevision = 2,
                CurrentConnectionId = connectionId,
                CurrentVerifiedSessionUser = "JQ98XX",
                CurrentVerifiedDatabaseName = "XE",
                CurrentVerifiedContainerName = "XEPDB1",
                ExpectedSessionUser = "JQ98XX",
                ExpectedDatabaseName = "XE",
                ExpectedContainerName = "XEPDB1",
                CandidateSchemaSource = schema,
                Provenance = new SharedOracleMetadataProvenance
                {
                    CapturedSourceTable = schema,
                    SourceType = DataSourceType.Oracle,
                    ConnectionRevision = 2,
                    ConnectionId = connectionId,
                    VerifiedSessionUser = "JQ98XX",
                    VerifiedDatabaseName = "XE",
                    VerifiedContainerName = "XEPDB1",
                    CompletedLoadId = new Guid("8B3B1000-0000-4000-8000-000000000002"),
                    CaptureKind = SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad,
                    LoadCompletedSuccessfully = true
                }
            };
        }

        [TestMethod]
        public void B3B101_CompleteRawOracleColumnsSatisfyConsistencyOnly()
        {
            var context = ConsistentSyntheticContext();

            Assert.IsTrue(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            //This positive check is deliberately only for the pure consistency policy.
            //The synthetic object must NOT be accepted as a loader-issued receipt.
        }

        [TestMethod]
        public void B3B102_MissingRawColumnCannotPublish()
        {
            var context = ConsistentSyntheticContext();

            context.CandidateSchemaSource.Columns.Remove("OracleMetadataColumnName");

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void B3B103_CopiedSourceCannotBeRelabeledWithOldReceipt()
        {
            var context = ConsistentSyntheticContext();

            context.CandidateSchemaSource = context.CandidateSchemaSource.Copy();

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void B3B104_SameUserNewPhysicalConnectionRejectsOldEvidence()
        {
            var context = ConsistentSyntheticContext();

            context.CurrentConnectionId = Guid.NewGuid();

            Assert.AreEqual("JQ98XX", context.Provenance.VerifiedSessionUser);
            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void B3B105_DifferentPdbOrLogicalRevisionRejectsPublication()
        {
            var context = ConsistentSyntheticContext();

            context.CurrentVerifiedContainerName = "OTHER_PDB";

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context = ConsistentSyntheticContext();
            context.CurrentConnectionRevision++;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void B3B106_PartialFailedOrPendingLoadCannotPublish()
        {
            var context = ConsistentSyntheticContext();

            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.PartialLoad;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context = ConsistentSyntheticContext();
            context.Provenance.LoadCompletedSuccessfully = false;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));

            context = ConsistentSyntheticContext();
            context.HasPendingOrUnknownTransaction = true;

            Assert.IsFalse(SharedOracleMetadataProvenancePolicy.CanPublishFreshMetadata(context));
        }

        [TestMethod]
        public void B3B107_CallerConstructedEvidenceCannotEnterPublicQualifiedBridge()
        {
            //A raw, publicly-settable provenance object can test consistency, but must never be a public qualified-snapshot authentication input.
            var rawBridge = typeof(SharedSchemaMetadataSnapshotPolicy).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(method => method.Name == "TryCaptureQualifiedOracle" && method.GetParameters().Any(parameter => parameter.ParameterType == typeof(SharedOracleMetadataProvenance)));

            Assert.IsNull(rawBridge, "Caller-constructed provenance must not authorize a qualified Oracle snapshot.");
        }

        [TestMethod]
        public void B3B108_CompletedOracleReceiptMustBeAnInternalImmutableType()
        {
            var receiptType = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SharedOracleMetadataCompletedLoadReceipt",
                false
            );

            Assert.IsNotNull(receiptType, "B2c-3B2 loader-issued completed-load receipt has not been implemented.");
            Assert.IsTrue(receiptType.IsSealed);
            Assert.IsFalse(receiptType.IsPublic);
            Assert.AreEqual(0, receiptType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.IsFalse(receiptType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(property => property.SetMethod != null && property.SetMethod.IsPublic));
        }

        [TestMethod]
        public void B3B109_ProductionAtomicOracleLoadMustIssueTypedReceipt()
        {
            var receiptType = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SharedOracleMetadataCompletedLoadReceipt",
                false
            );

            Assert.IsNotNull(receiptType, "Receipt must be issued by the production Oracle loader, not synthesized by UI rebind.");

            var loader = typeof(DatabaseSqlExecutor).GetMethod
            (
                "RunOracleMetadataAtomicLoad",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            Assert.IsNotNull(loader);
            Assert.AreEqual(receiptType, loader.ReturnType, "The production atomic-load path must issue the typed receipt after successful publication.");
        }

        [TestMethod]
        public void B3B110_QualifiedOracleBridgeMustRequireInternalReceipt()
        {
            var receiptType = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SharedOracleMetadataCompletedLoadReceipt",
                false
            );

            Assert.IsNotNull(receiptType, "Qualified snapshots must depend on an internal loader-issued receipt.");

            var bridge = typeof(SharedSchemaMetadataSnapshotPolicy).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(method => method.Name == "TryCaptureQualifiedOracle" && method.ReturnType == typeof(bool) && method.GetParameters().Any(parameter => parameter.ParameterType == receiptType));

            Assert.IsNotNull(bridge, "Qualified Oracle capture must require a typed receipt; arbitrary provenance fields are not authority.");
        }
    }
}
