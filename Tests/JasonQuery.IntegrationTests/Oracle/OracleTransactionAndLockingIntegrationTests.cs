using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.Oracle
{
    [TestClass]
    [DoNotParallelize]
    public sealed class OracleTransactionAndLockingIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Transaction")]
        public void InsertCommit_IsInvisibleUntilCommit()
        {
            IntegrationTestTransactionAssertions.AssertInsertCommitVisibility(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Transaction")]
        public void UpdateRollback_RestoresOriginalValue()
        {
            IntegrationTestTransactionAssertions.AssertUpdateRollbackVisibility(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Transaction")]
        public void DeleteRollback_RestoresDeletedRow()
        {
            IntegrationTestTransactionAssertions.AssertDeleteRollbackVisibility(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Transaction")]
        public void FailedSql_DoesNotReportPendingTransaction()
        {
            IntegrationTestTransactionAssertions.AssertFailedSqlDoesNotReportPending(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("DDL")]
        public void Ddl_ReportsAndMatchesRealTransactionSemantics()
        {
            IntegrationTestTransactionAssertions.AssertDdlTransactionSemantics(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Transaction")]
        public void RepeatedCommitAndRollback_AreDefensive()
        {
            IntegrationTestTransactionAssertions.AssertRepeatedTransactionActionsAreDefensive(TestContext, DataSourceType.Oracle);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Oracle")]
        [TestCategory("Locking")]
        public void LockingQuery_RepeatedExecutionKeepsLockUntilRelease()
        {
            IntegrationTestTransactionAssertions.AssertLockingQueryRepeatConflictAndRelease(TestContext, DataSourceType.Oracle);
        }

    }
}
