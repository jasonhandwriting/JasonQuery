using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C4C1-A: reason-classification contract only. No database connection and no DDL execution.
    //The missing feedback Policy and Form property are deliberate Golden Red requirements.
    [TestClass]
    [TestCategory("F7U3C4C1A")]
    public sealed class SchemaExplorerMySqlColumnDdlRejectionReasonGoldenTests
    {
        private static readonly Guid ConnectionId = new Guid("bf79ba8d-6df1-4492-8f6c-4799319aac02");

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(string definition)
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                ConnectionId, "demo", "b2_table", "old_name", "8.0.34", definition, 381
            );
        }

        private static string GetReasonKey(string failureStatus)
        {
            //Reflective on purpose: Golden Red must COMPILE before production policy exists.
            var policy = typeof(MySqlVerifiedColumnDefinitionExecutionPolicy).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.MySqlColumnDdlPreviewFeedbackPolicy",
                false
            );

            Assert.IsNotNull(policy, "C4C1-B must introduce a pure feedback policy; do not weaken the Reader/Execute gate.");

            var method = policy.GetMethod
            (
                "GetReasonKey",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null
            );

            Assert.IsNotNull(method, "Feedback policy must expose GetReasonKey(string).");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke(null, new object[] { failureStatus });
        }

        [TestMethod]
        public void C4C1A01_ReaderAlreadyProvidesExplicitFailureStatusOutput()
        {
            var method = typeof(MySqlReader).GetMethod
            (
                "TryReadVerifiedColumnDefinitionOnConnection",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);

            var parameters = method.GetParameters();

            Assert.AreEqual(6, parameters.Length);
            Assert.AreEqual(typeof(Guid), parameters[0].ParameterType);
            Assert.AreEqual(typeof(string).MakeByRefType(), parameters[5].ParameterType);
            Assert.IsTrue(parameters[5].IsOut);
        }

        [TestMethod]
        public void C4C1A02_MissingReceiptMustNotBecomeExecutePermission()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition
            (
                DataSourceType.MySql, EditColumnMode.Comment, null, false
            ));
        }

        [TestMethod]
        public void C4C1A03_IncompleteDefinitionMustRemainOutsideExecuteWhiteList()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition
            (
                DataSourceType.MySql, EditColumnMode.Comment, Receipt("int NOT NULL"), false
            ));
        }

        [TestMethod]
        public void C4C1A04_ModernRenameEligibilityMustRemainIndependent()
        {
            Assert.IsTrue(EditColumnForm.CanEnableVerifiedMySqlModernRename
            (
                DataSourceType.MySql, EditColumnMode.Rename, "8.0.34"
            ));

            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlModernRename
            (
                DataSourceType.MySql, EditColumnMode.Rename, "5.7.44"
            ));
        }

        [TestMethod]
        public void C4C1A05_UnsupportedColumnShapeMustHaveStableUserReason()
        {
            Assert.AreEqual("MySqlColumnDdlUnsupportedDefinition", GetReasonKey("F7U3C4B3_METADATA_OUTSIDE_SAFE_SUBSET"));
        }

        [TestMethod]
        public void C4C1A06_AmbiguousMetadataMustHaveDistinctUserReason()
        {
            Assert.AreEqual("MySqlColumnDdlMetadataUnverified", GetReasonKey("F7U3C4B3_METADATA_AMBIGUOUS"));
        }

        [TestMethod]
        public void C4C1A07_AttributesNotProvenMustFailClosedWithMetadataReason()
        {
            Assert.AreEqual("MySqlColumnDdlMetadataUnverified", GetReasonKey("F7U3C4B3_ATTRIBUTES_NOT_PROVEN"));
        }

        [TestMethod]
        public void C4C1A08_UnverifiedConnectionMustHaveActionableUserReason()
        {
            Assert.AreEqual("MySqlColumnDdlConnectionUnverified", GetReasonKey("F7U3C4B3_CONNECTION_UNVERIFIED"));
        }

        [TestMethod]
        public void C4C1A09_ChangedPhysicalConnectionMustNotBeHiddenAsUnsupportedType()
        {
            Assert.AreEqual("MySqlColumnDdlConnectionChanged", GetReasonKey("F7U3C4B3_CONNECTION_CHANGED"));
        }

        [TestMethod]
        public void C4C1A10_UnverifiedServerVersionMustHaveDistinctUserReason()
        {
            Assert.AreEqual("MySqlColumnDdlVersionUnverified", GetReasonKey("F7U3C4B3_VERSION_UNVERIFIED"));
        }

        [TestMethod]
        public void C4C1A11_PendingTransactionMustHavePriorityOverMetadataFeedback()
        {
            Assert.AreEqual("MySqlColumnDdlPendingTransaction", GetReasonKey("F7U2C2_TRANSACTION_UNSAFE"));
        }

        [TestMethod]
        public void C4C1A12_MetadataReadFailureMustNotRevealRawExceptionDetail()
        {
            const string secret = "Password=DO_NOT_SHOW_THIS_SECRET";

            var reason = GetReasonKey("F7U3C4B3_READ_FAILED: " + secret);

            Assert.AreEqual("MySqlColumnDdlMetadataReadFailed", reason);
            Assert.IsFalse(reason.Contains(secret));
            Assert.IsFalse(reason.Contains("F7U3C4B3_"));
        }

        [TestMethod]
        public void C4C1A13_UnknownReasonMustUseSafeGenericFallback()
        {
            Assert.AreEqual("MySqlColumnDdlDefinitionUnverified", GetReasonKey("F7U3C4B3_UNKNOWN_FUTURE_CODE"));
            Assert.AreEqual("MySqlColumnDdlDefinitionUnverified", GetReasonKey(null));
        }

        [TestMethod]
        public void C4C1A14_FormMustAcceptReasonWithoutChangingExecuteAuthorization()
        {
            var property = typeof(EditColumnForm).GetProperty
            (
                "NonOracleF7MySqlPreviewBlockReasonCode",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property, "C4C1-B must pass a diagnostic reason from MainForm to the modal Form.");
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsTrue(property.CanRead);
            Assert.IsTrue(property.CanWrite);
        }
    }
}
