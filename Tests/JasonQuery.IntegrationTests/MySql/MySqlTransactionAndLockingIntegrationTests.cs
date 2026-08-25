using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.MySql
{
    [TestClass]
    [DoNotParallelize]
    public sealed class MySqlTransactionAndLockingIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Transaction")]
        public void InsertCommit_IsInvisibleUntilCommit()
        {
            IntegrationTestTransactionAssertions.AssertInsertCommitVisibility(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Transaction")]
        public void UpdateRollback_RestoresOriginalValue()
        {
            IntegrationTestTransactionAssertions.AssertUpdateRollbackVisibility(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Transaction")]
        public void DeleteRollback_RestoresDeletedRow()
        {
            IntegrationTestTransactionAssertions.AssertDeleteRollbackVisibility(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Transaction")]
        public void FailedSql_DoesNotReportPendingTransaction()
        {
            IntegrationTestTransactionAssertions.AssertFailedSqlDoesNotReportPending(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("DDL")]
        public void Ddl_ReportsAndMatchesRealTransactionSemantics()
        {
            IntegrationTestTransactionAssertions.AssertDdlTransactionSemantics(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Transaction")]
        public void RepeatedCommitAndRollback_AreDefensive()
        {
            IntegrationTestTransactionAssertions.AssertRepeatedTransactionActionsAreDefensive(TestContext, DataSourceType.MySql);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("MySql")]
        [TestCategory("Locking")]
        public void LockingQuery_RepeatedExecutionKeepsLockUntilRelease()
        {
            IntegrationTestTransactionAssertions.AssertLockingQueryRepeatConflictAndRelease(TestContext, DataSourceType.MySql);
        }

    }
}
