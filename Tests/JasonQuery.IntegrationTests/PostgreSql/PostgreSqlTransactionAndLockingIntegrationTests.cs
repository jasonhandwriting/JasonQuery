using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.PostgreSql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class PostgreSqlTransactionAndLockingIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Transaction")]
        public void InsertCommit_IsInvisibleUntilCommit()
        {
            IntegrationTestTransactionAssertions.AssertInsertCommitVisibility(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Transaction")]
        public void UpdateRollback_RestoresOriginalValue()
        {
            IntegrationTestTransactionAssertions.AssertUpdateRollbackVisibility(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Transaction")]
        public void DeleteRollback_RestoresDeletedRow()
        {
            IntegrationTestTransactionAssertions.AssertDeleteRollbackVisibility(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Transaction")]
        public void FailedSql_DoesNotReportPendingTransaction()
        {
            IntegrationTestTransactionAssertions.AssertFailedSqlDoesNotReportPending(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("DDL")]
        public void Ddl_ReportsAndMatchesRealTransactionSemantics()
        {
            IntegrationTestTransactionAssertions.AssertDdlTransactionSemantics(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Transaction")]
        public void RepeatedCommitAndRollback_AreDefensive()
        {
            IntegrationTestTransactionAssertions.AssertRepeatedTransactionActionsAreDefensive(TestContext, DataSourceType.PostgreSql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("PostgreSql")]
        [TestCategory("Locking")]
        public void LockingQuery_RepeatedExecutionKeepsLockUntilRelease()
        {
            IntegrationTestTransactionAssertions.AssertLockingQueryRepeatConflictAndRelease(TestContext, DataSourceType.PostgreSql);
        }

    }
}
