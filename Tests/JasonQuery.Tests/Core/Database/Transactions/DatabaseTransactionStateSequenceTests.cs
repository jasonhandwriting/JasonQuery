using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class DatabaseTransactionStateSequenceTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ApplyImpact_Pending_SetsPendingAndClearsClosed()
        {
            var pending = false;
            var closed = true;

            DatabaseTransactionStatePolicy.ApplyImpact(DatabaseTransactionImpact.Pending, ref pending, ref closed);

            Assert.IsTrue(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ApplyImpact_Closed_ClearsPendingAndSetsClosed()
        {
            var pending = true;
            var closed = false;

            DatabaseTransactionStatePolicy.ApplyImpact(DatabaseTransactionImpact.Closed, ref pending, ref closed);

            Assert.IsFalse(pending);
            Assert.IsTrue(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(false, false)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(true, true)]
        public void ApplyImpact_Unchanged_PreservesExistingFlags(bool originalPending, bool originalClosed)
        {
            var pending = originalPending;
            var closed = originalClosed;

            DatabaseTransactionStatePolicy.ApplyImpact(DatabaseTransactionImpact.Unchanged, ref pending, ref closed);

            Assert.AreEqual(originalPending, pending);
            Assert.AreEqual(originalClosed, closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Apply_WithPendingStatement_UsesDatabaseSpecificPolicy()
        {
            var pending = false;
            var closed = false;

            DatabaseTransactionStatePolicy.Apply(DataSourceType.PostgreSql, "COMMENT", ref pending, ref closed);

            Assert.IsTrue(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Apply_WithMySqlImplicitCommit_ClosesTransaction()
        {
            var pending = true;
            var closed = false;

            DatabaseTransactionStatePolicy.Apply(DataSourceType.MySql, "ALTER", ref pending, ref closed);

            Assert.IsFalse(pending);
            Assert.IsTrue(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Apply_PendingThenUnchanged_RemainsPending()
        {
            var pending = false;
            var closed = false;

            DatabaseTransactionStatePolicy.Apply(DataSourceType.PostgreSql, "UPDATE", ref pending, ref closed);
            DatabaseTransactionStatePolicy.Apply(DataSourceType.PostgreSql, "SELECT", ref pending, ref closed);

            Assert.IsTrue(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Apply_PendingThenClosed_EndsClosed()
        {
            var pending = false;
            var closed = false;

            DatabaseTransactionStatePolicy.Apply(DataSourceType.PostgreSql, "UPDATE", ref pending, ref closed);
            DatabaseTransactionStatePolicy.Apply(DataSourceType.PostgreSql, "COMMIT", ref pending, ref closed);

            Assert.IsFalse(pending);
            Assert.IsTrue(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Apply_ClosedThenPending_EndsPending()
        {
            var pending = false;
            var closed = false;

            DatabaseTransactionStatePolicy.Apply(DataSourceType.MySql, "ALTER", ref pending, ref closed);
            DatabaseTransactionStatePolicy.Apply(DataSourceType.MySql, "UPDATE", ref pending, ref closed);

            Assert.IsTrue(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(false, false, "TX_UNCHANGED")]
        [DataRow(true, false, "TX_PENDING")]
        [DataRow(false, true, "TX_CLOSED")]
        [DataRow(true, true, "TX_PENDING")]
        public void ResolveStateToken_WithDifferentFlags_ReturnsExpectedToken(bool pending, bool closed, string expected)
        {
            var actual = DatabaseTransactionStatePolicy.ResolveStateToken(pending, closed);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void ResolveStateToken_UsesPublishedConstants()
        {
            Assert.AreEqual("TX_UNCHANGED", DatabaseTransactionStatePolicy.StateUnchanged);
            Assert.AreEqual("TX_PENDING", DatabaseTransactionStatePolicy.StatePending);
            Assert.AreEqual("TX_CLOSED", DatabaseTransactionStatePolicy.StateClosed);
        }
    }
}
