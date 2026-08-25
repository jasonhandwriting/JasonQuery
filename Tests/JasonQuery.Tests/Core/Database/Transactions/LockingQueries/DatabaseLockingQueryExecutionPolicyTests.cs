using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Transactions.LockingQueries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Transactions.LockingQueries
{
    [TestClass]
    public sealed class DatabaseLockingQueryExecutionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "SELECT * FROM a_test FOR UPDATE")]
        [DataRow("PostgreSql", "SELECT * FROM a_test FOR NO KEY UPDATE")]
        [DataRow("PostgreSql", "SELECT * FROM a_test FOR SHARE")]
        [DataRow("PostgreSql", "SELECT * FROM a_test FOR KEY SHARE")]
        [DataRow("MySql", "SELECT * FROM a_test FOR UPDATE")]
        [DataRow("MySql", "SELECT * FROM a_test LOCK IN SHARE MODE")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (UPDLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (XLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (HOLDLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (TABLOCKX)")]
        public void Evaluate_LockingQuery_DisablesPagingAndRequiresSinglePass(string dataSourceTypeName, string sql)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);

            var decision = DatabaseLockingQueryExecutionPolicy.Evaluate
            (
                dataSourceType,
                sql,
                pagingRequested: true
            );

            Assert.IsTrue(decision.IsLockingQuery);
            Assert.IsTrue(decision.RequiresConfirmation);
            Assert.IsFalse(decision.ShouldUsePagedExecution);
            Assert.IsTrue(decision.ShouldUseSinglePassReader);
            Assert.IsTrue(decision.ShouldKeepConnectionOpenAfterSuccess);
            Assert.IsTrue(decision.ShouldMarkPendingAfterSuccess);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "SELECT * FROM a_test")]
        [DataRow("PostgreSql", "SELECT * FROM a_test")]
        [DataRow("MySql", "SELECT * FROM a_test")]
        [DataRow("SqlServer", "SELECT * FROM a_test")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (ROWLOCK)")]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (NOLOCK)")]
        [DataRow("PostgreSql", "SELECT 'FOR UPDATE'")]
        [DataRow("MySql", "SELECT `FOR UPDATE` FROM a_test")]
        public void Evaluate_OrdinaryQuery_PreservesRequestedPaging(string dataSourceTypeName, string sql)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);

            var decision = DatabaseLockingQueryExecutionPolicy.Evaluate
            (
                dataSourceType,
                sql,
                pagingRequested: true
            );

            Assert.IsFalse(decision.IsLockingQuery);
            Assert.IsFalse(decision.RequiresConfirmation);
            Assert.IsTrue(decision.ShouldUsePagedExecution);
            Assert.IsFalse(decision.ShouldUseSinglePassReader);
            Assert.IsFalse(decision.ShouldKeepConnectionOpenAfterSuccess);
            Assert.IsFalse(decision.ShouldMarkPendingAfterSuccess);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "SELECT * FROM a_test")]
        [DataRow("PostgreSql", "SELECT * FROM a_test")]
        [DataRow("MySql", "SELECT * FROM a_test")]
        [DataRow("SqlServer", "SELECT * FROM a_test")]
        public void Evaluate_WhenPagingWasNotRequested_DoesNotEnablePaging(string dataSourceTypeName, string sql)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);

            var decision = DatabaseLockingQueryExecutionPolicy.Evaluate
            (
                dataSourceType,
                sql,
                pagingRequested: false
            );

            Assert.IsFalse(decision.ShouldUsePagedExecution);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LockingQuery")]
        [DataRow("Oracle", "SELECT * FROM a_test FOR UPDATE", true)]
        [DataRow("PostgreSql", "SELECT * FROM a_test FOR SHARE", true)]
        [DataRow("MySql", "SELECT * FROM a_test LOCK IN SHARE MODE", true)]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (UPDLOCK)", true)]
        [DataRow("Oracle", "SELECT * FROM a_test", false)]
        [DataRow("PostgreSql", "SELECT 'FOR UPDATE'", false)]
        [DataRow("MySql", "SELECT * FROM a_test", false)]
        [DataRow("SqlServer", "SELECT * FROM a_test WITH (ROWLOCK)", false)]
        public void ShouldUseSinglePassReader_ReturnsExpectedValue(string dataSourceTypeName, string sql, bool expected)
        {
            var dataSourceType = ParseDataSourceType(dataSourceTypeName);

            Assert.AreEqual
            (
                expected,
                DatabaseLockingQueryExecutionPolicy.ShouldUseSinglePassReader
                (
                    dataSourceType,
                    sql
                )
            );
        }

        private static DataSourceType ParseDataSourceType(string value)
        {
            return (DataSourceType)Enum.Parse
            (
                typeof(DataSourceType),
                value,
                ignoreCase: true
            );
        }
    }
}
