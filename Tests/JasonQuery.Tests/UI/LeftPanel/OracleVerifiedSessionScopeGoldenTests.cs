using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2B")]
    public sealed class OracleVerifiedSessionScopeGoldenTests
    {
        private static readonly Guid OldId = new Guid("D2A9457A-7B77-4899-9B84-6AA00B700001");
        private static readonly Guid NewId = new Guid("D2A9457A-7B77-4899-9B84-6AA00B700002");

        private static OracleVerifiedSessionScope Scope(Guid connectionId)
        {
            return new OracleVerifiedSessionScope
            {
                SessionUser = "JQ98XX",
                DatabaseName = "XE",
                ContainerName = "XEPDB1",
                ConnectionId = connectionId
            };
        }

        [TestMethod]
        public void I01_BlankSessionUserCannotCreateVerifiedScope()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.TryCreate(" ", "XE", "XEPDB1", NewId, out var captured));
            Assert.IsNull(captured);
        }

        [TestMethod]
        public void I02_CompleteIdentityCanCreateVerifiedScope()
        {
            Assert.IsTrue(OracleVerifiedSessionScopePolicy.TryCreate("JQ98XX", "XE", "XEPDB1", NewId, out var captured));
            Assert.IsNotNull(captured);
            Assert.AreEqual("JQ98XX", captured.SessionUser);
            Assert.AreEqual("XE", captured.DatabaseName);
            Assert.AreEqual("XEPDB1", captured.ContainerName);
            Assert.AreEqual(NewId, captured.ConnectionId);
        }

        [TestMethod]
        public void I03_BlankDatabaseNameCannotCreateVerifiedScope()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.TryCreate("JQ98XX", " ", "XEPDB1", NewId, out var captured));
            Assert.IsNull(captured);
        }

        [TestMethod]
        public void I04_BlankContainerNameCannotCreateVerifiedScope()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.TryCreate("JQ98XX", "XE", null, NewId, out var captured));
            Assert.IsNull(captured);
        }

        [TestMethod]
        public void I05_EmptyPhysicalIdCannotCreateVerifiedScope()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.TryCreate("JQ98XX", "XE", "XEPDB1", Guid.Empty, out var captured));
            Assert.IsNull(captured);
        }

        [TestMethod]
        public void I06_ExactAuthenticatedScopeMatchesExpectedScope()
        {
            Assert.IsTrue(OracleVerifiedSessionScopePolicy.MatchesLogicalScope(Scope(NewId), "JQ98XX", "XE", "XEPDB1"));
        }

        [TestMethod]
        public void I07_NewConnectionIdSameUserDatabaseAndPdbIsNewSession()
        {
            Assert.IsTrue(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(OldId), Scope(NewId)));
        }

        [TestMethod]
        public void I08_ReuseOfOldConnectionIdIsNotNewSession()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(OldId), Scope(OldId)));
        }

        [TestMethod]
        public void I09_ChangedAuthenticatedUserRejectsNewSessionQualification()
        {
            var current = Scope(NewId);

            current.SessionUser = "OTHERUSER";

            Assert.IsFalse(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(OldId), current));
        }

        [TestMethod]
        public void I10_ChangedDatabaseRejectsNewSessionQualification()
        {
            var current = Scope(NewId);

            current.DatabaseName = "OTHERDB";

            Assert.IsFalse(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(OldId), current));
        }

        [TestMethod]
        public void I11_ChangedPdbRejectsNewSessionQualification()
        {
            var current = Scope(NewId);

            current.ContainerName = "OTHERPDB";

            Assert.IsFalse(OracleVerifiedSessionScopePolicy.IsNewVerifiedSession(Scope(OldId), current));
        }

        [TestMethod]
        public void I12_CaseChangeDoesNotMatchExactVerifiedScope()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.MatchesLogicalScope(Scope(NewId), "jq98xx", "XE", "XEPDB1"));
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.MatchesLogicalScope(Scope(NewId), "JQ98XX", "XE", "xepdb1"));
        }

        [TestMethod]
        public void I13_MissingExpectedScopeFailsClosed()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.MatchesLogicalScope(Scope(NewId), "JQ98XX", " ", "XEPDB1"));
        }

        [TestMethod]
        public void I14_FreshSessionMayBindMetadataWithExactIdAndScope()
        {
            Assert.IsTrue(OracleVerifiedSessionScopePolicy.CanBindMetadataToSession(Scope(NewId), NewId, "JQ98XX", "XE", "XEPDB1"));
        }

        [TestMethod]
        public void I15_StaleConnectionIdMayNotBindAfterReconnect()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.CanBindMetadataToSession(Scope(NewId), OldId, "JQ98XX", "XE", "XEPDB1"));
        }

        [TestMethod]
        public void I16_UnknownExpectedContainerMayNotBindMetadata()
        {
            Assert.IsFalse(OracleVerifiedSessionScopePolicy.CanBindMetadataToSession(Scope(NewId), NewId, "JQ98XX", "XE", string.Empty));
        }

        [TestMethod]
        public void I17_OracleReaderExposesVerifiedDatabaseName()
        {
            var property = typeof(OracleReader).GetProperty("VerifiedDatabaseName", BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(property, "OracleReader must expose verified DB_NAME, not a connection setting.");
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsTrue(property.CanRead);
            Assert.IsFalse(property.CanWrite);
        }

        [TestMethod]
        public void I18_OracleReaderExposesVerifiedContainerName()
        {
            var property = typeof(OracleReader).GetProperty("VerifiedContainerName", BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(property, "OracleReader must expose verified CON_NAME, not the configured SID or Service Name.");
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsTrue(property.CanRead);
            Assert.IsFalse(property.CanWrite);
        }
    }
}
