using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-S1-B2B: evidence-only Golden tests. No database access, reconnect, or DDL dispatch.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class DdlTransactionEvidenceGoldenTests
    {
        private static object Evaluate(string provider, bool pending = false, bool uncertain = false, bool scopeCurrent = true, bool readerAvailable = true, ConnectionState readerState = ConnectionState.Open)
        {
            var type = typeof(TableDdlRequest).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.DdlTransactionEvidencePolicy",
                false
            );

            Assert.IsNotNull(type, "Missing DdlTransactionEvidencePolicy.");

            var method = type.GetMethod
            (
                "Evaluate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[]
                {
                    typeof(DataSourceType),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(ConnectionState)
                },
                null
            );

            Assert.IsNotNull(method, "Missing Evaluate method with the B2B evidence signature.");

            var source = (DataSourceType)Enum.Parse(typeof(DataSourceType), provider);

            var result = method.Invoke
            (
                null,
                new object[] { source, pending, uncertain, scopeCurrent, readerAvailable, readerState }
            );

            Assert.IsNotNull(result);
            return result;
        }

        private static T Get<T>(object decision, string propertyName)
        {
            var property = decision.GetType().GetProperty
            (
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property, "Missing evidence property: " + propertyName);
            return (T)property.GetValue(decision);
        }

        private static void IsBlocked(object decision, string reason, bool reopen)
        {
            Assert.IsTrue(Get<bool>(decision, "BlockExecute"));
            Assert.IsFalse(Get<bool>(decision, "CanAttemptReconnect"));
            Assert.AreEqual(reopen, Get<bool>(decision, "RequiresFormReopen"));
            Assert.AreEqual(reason, Get<string>(decision, "ReasonCode"));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B01_PendingBlocksBeforeAnyReconnect(string provider)
        {
            var decision = Evaluate(provider, pending: true, scopeCurrent: false, readerAvailable: false, readerState: ConnectionState.Closed);

            IsBlocked(decision, "S1_EVIDENCE_PENDING", true);

            Assert.AreEqual(true, Get<bool?>(decision, "HasPendingTransaction"));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B02_CleanOpenReaderMayProceedToExistingGates(string provider)
        {
            var decision = Evaluate(provider);

            Assert.IsFalse(Get<bool>(decision, "BlockExecute"));
            Assert.IsFalse(Get<bool>(decision, "CanAttemptReconnect"));
            Assert.IsFalse(Get<bool>(decision, "RequiresFormReopen"));
            Assert.AreEqual(false, Get<bool?>(decision, "HasPendingTransaction"));
            Assert.AreEqual("S1_EVIDENCE_CLEAN", Get<string>(decision, "ReasonCode"));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B03_NormalDisconnectRequiresRecheckButNotStickyLock(string provider)
        {
            var decision = Evaluate(provider, readerState: ConnectionState.Closed);

            Assert.IsTrue(Get<bool>(decision, "BlockExecute"));
            Assert.IsTrue(Get<bool>(decision, "CanAttemptReconnect"));
            Assert.IsFalse(Get<bool>(decision, "RequiresFormReopen"));
            Assert.IsNull(Get<bool?>(decision, "HasPendingTransaction"));
            Assert.AreEqual("S1_EVIDENCE_RECONNECT_REQUIRED", Get<string>(decision, "ReasonCode"));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B04_UncertainOutcomeBlocksEvenWithOpenReader(string provider)
        {
            var decision = Evaluate(provider, uncertain: true);

            IsBlocked(decision, "S1_EVIDENCE_UNKNOWN", true);

            Assert.IsNull(Get<bool?>(decision, "HasPendingTransaction"));
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B05_LogicalScopeMismatchBlocksDdl(string provider)
        {
            var decision = Evaluate(provider, scopeCurrent: false);

            IsBlocked(decision, "S1_EVIDENCE_UNKNOWN", true);
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B06_MissingReaderCannotProveClean(string provider)
        {
            var decision = Evaluate(provider, readerAvailable: false);

            IsBlocked(decision, "S1_EVIDENCE_UNKNOWN", true);
        }

        [TestMethod]
        [DataRow(ConnectionState.Broken)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Executing)]
        public void B2B07_AbnormalReaderStateMustFailClosed(ConnectionState state)
        {
            var decision = Evaluate("Oracle", readerState: state);

            IsBlocked(decision, "S1_EVIDENCE_UNKNOWN", true);
        }

        [TestMethod]
        [DataRow("PostgreSql")]
        [DataRow("SqlServer")]
        public void B2B08_OtherProvidersRemainOnExistingTransactionGates(string provider)
        {
            var decision = Evaluate(provider, pending: true);

            Assert.IsFalse(Get<bool>(decision, "BlockExecute"));
            Assert.IsFalse(Get<bool>(decision, "CanAttemptReconnect"));
            Assert.AreEqual("S1_EVIDENCE_NOT_APPLICABLE", Get<string>(decision, "ReasonCode"));
        }

        [TestMethod]
        public void B2B09_UnsupportedProviderNeverReceivesExecutePermission()
        {
            var decision = Evaluate("None");

            IsBlocked(decision, "S1_EVIDENCE_UNSUPPORTED_PROVIDER", true);
        }

        [TestMethod]
        [DataRow("Oracle")]
        [DataRow("MySql")]
        public void B2B10_ReconnectIsNotExecuteAuthorization(string provider)
        {
            var before = Evaluate(provider, readerState: ConnectionState.Closed);
            var after = Evaluate(provider, readerState: ConnectionState.Open);

            Assert.IsTrue(Get<bool>(before, "CanAttemptReconnect"));
            Assert.IsTrue(Get<bool>(before, "BlockExecute"));
            Assert.IsFalse(Get<bool>(after, "CanAttemptReconnect"));
            Assert.IsFalse(Get<bool>(after, "BlockExecute"));
            Assert.AreEqual(false, Get<bool?>(after, "HasPendingTransaction"));
        }
    }
}
