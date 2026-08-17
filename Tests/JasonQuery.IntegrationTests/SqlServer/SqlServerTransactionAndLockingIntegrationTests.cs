using JasonQuery.Core.Database.Connection;
using JasonQuery.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.IntegrationTests.SqlServer
{
    [TestClass]
    [DoNotParallelize]
    public sealed class SqlServerTransactionAndLockingIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Transaction")]
        public void InsertCommit_IsInvisibleUntilCommit()
        {
            IntegrationTestTransactionAssertions.AssertInsertCommitVisibility(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Transaction")]
        public void UpdateRollback_RestoresOriginalValue()
        {
            IntegrationTestTransactionAssertions.AssertUpdateRollbackVisibility(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Transaction")]
        public void DeleteRollback_RestoresDeletedRow()
        {
            IntegrationTestTransactionAssertions.AssertDeleteRollbackVisibility(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Transaction")]
        public void FailedSql_DoesNotReportPendingTransaction()
        {
            IntegrationTestTransactionAssertions.AssertFailedSqlDoesNotReportPending(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("DDL")]
        public void Ddl_ReportsAndMatchesRealTransactionSemantics()
        {
            IntegrationTestTransactionAssertions.AssertDdlTransactionSemantics(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Transaction")]
        public void RepeatedCommitAndRollback_AreDefensive()
        {
            IntegrationTestTransactionAssertions.AssertRepeatedTransactionActionsAreDefensive(TestContext, DataSourceType.SqlServer);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("SqlServer")]
        [TestCategory("Locking")]
        public void LockingQuery_RepeatedExecutionKeepsLockUntilRelease()
        {
            IntegrationTestTransactionAssertions.AssertLockingQueryRepeatConflictAndRelease(TestContext, DataSourceType.SqlServer);
        }

    }
}