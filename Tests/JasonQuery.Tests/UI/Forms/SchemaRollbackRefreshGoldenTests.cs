using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.Forms
{
    //F2-C2-F3A: policy-only RED contract. No database, transaction or UI is executed.
    [TestClass]
    [TestCategory("F2C2F3A")]
    public sealed class SchemaRollbackRefreshGoldenTests
    {
        private static bool CanSuppress(DataSourceType sourceType, string completedAction, bool transactionSucceeded, bool hasTransactionBoundDirty, bool metadataTrusted, bool sameDatabaseScope, bool otherRefreshRequired)
        {
            var policyType = typeof(QueryForm).Assembly.GetType
            (
                "JasonQuery.Core.Database.Execution.SchemaDirtyPolicy",
                false
            );

            Assert.IsNotNull(policyType, "Missing existing SchemaDirtyPolicy.");

            var method = policyType.GetMethod
            (
                "ShouldSkipMetadataReloadForConfirmedRollback",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    typeof(DataSourceType), typeof(string), typeof(bool),
                    typeof(bool), typeof(bool), typeof(bool), typeof(bool)
                },
                null
            );

            Assert.IsNotNull
            (
                method,
                "F2C2F3A_EXPECT_RED: missing rollback-specific suppression policy. Do not change the original Evaluate() contract to make this test green."
            );

            var result = method.Invoke
            (
                null,
                new object[]
                {
                    sourceType, completedAction, transactionSucceeded,
                    hasTransactionBoundDirty, metadataTrusted,
                    sameDatabaseScope, otherRefreshRequired
                }
            );

            Assert.IsInstanceOfType(result, typeof(bool));
            return (bool)result;
        }

        [TestMethod]
        public void F301_PostgreSqlSuccessfulRollbackWithTrustedBaselineSkipsReload()
        {
            Assert.IsTrue(CanSuppress(DataSourceType.PostgreSql, "ROLLBACK", true, true, true, true, false));
        }

        [TestMethod]
        public void F302_SqlServerSuccessfulRollbackWithTrustedBaselineSkipsReload()
        {
            Assert.IsTrue(CanSuppress(DataSourceType.SqlServer, "ROLLBACK", true, true, true, true, false));
        }

        [TestMethod]
        public void F303_CommitMustNotSuppressReload()
        {
            Assert.IsFalse(CanSuppress(DataSourceType.PostgreSql, "COMMIT", true, true, true, true, false));
            Assert.IsFalse(CanSuppress(DataSourceType.SqlServer, "COMMIT", true, true, true, true, false));
        }

        [TestMethod]
        public void F304_OracleAndMySqlAreNotEligible()
        {
            Assert.IsFalse(CanSuppress(DataSourceType.Oracle, "ROLLBACK", true, true, true, true, false));
            Assert.IsFalse(CanSuppress(DataSourceType.MySql, "ROLLBACK", true, true, true, true, false));
        }

        [TestMethod]
        public void F305_FailedOrUnconfirmedRollbackNeverSuppressesReload()
        {
            Assert.IsFalse(CanSuppress(DataSourceType.PostgreSql, "ROLLBACK", false, true, true, true, false));
            Assert.IsFalse(CanSuppress(DataSourceType.SqlServer, "ROLLBACK", false, true, true, true, false));
        }

        [TestMethod]
        public void F306_UnknownAndSavepointActionsCannotPretendToCloseTransaction()
        {
            foreach (var action in new[] { "", "UNKNOWN", "ROLLBACK TO SAVEPOINT s1", "ROLLBACK FAILED", "ROLLBACK; COMMIT" })
            {
                Assert.IsFalse(CanSuppress(DataSourceType.PostgreSql, action, true, true, true, true, false), action);
            }
        }

        [TestMethod]
        public void F307_StaleOrDifferentDatabaseSnapshotMustNotBeSilentlyAccepted()
        {
            Assert.IsFalse(CanSuppress(DataSourceType.SqlServer, "ROLLBACK", true, true, false, true, false));
            Assert.IsFalse(CanSuppress(DataSourceType.SqlServer, "ROLLBACK", true, true, true, false, false));
        }

        [TestMethod]
        public void F308_NoTransactionDirtyOrOtherRefreshRequirementMustNotBeSuppressed()
        {
            Assert.IsFalse(CanSuppress(DataSourceType.PostgreSql, "ROLLBACK", true, false, true, true, false));
            Assert.IsFalse(CanSuppress(DataSourceType.PostgreSql, "ROLLBACK", true, true, true, true, true));
        }
    }
}
