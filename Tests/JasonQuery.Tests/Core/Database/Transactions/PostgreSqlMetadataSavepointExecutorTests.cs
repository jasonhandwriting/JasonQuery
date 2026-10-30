using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class PostgreSqlMetadataSavepointExecutorTests
    {
        [TestMethod]
        public void Success_CreatesQueriesAndReleasesWithoutRollback()
        {
            var calls = new List<string>();
            var result = PostgreSqlMetadataSavepointExecutor.Execute(() => calls.Add("save"), () => { calls.Add("query"); return "value"; }, () => calls.Add("rollback"), () => calls.Add("release")); Assert.AreEqual("value", result);

            CollectionAssert.AreEqual(new[] { "save", "query", "release" }, calls);
        }

        [TestMethod]
        public void QueryFailure_RestoresReleasesAndThrows()
        {
            var calls = new List<string>();

            var ex = Assert.ThrowsExactly<InvalidOperationException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<string>
                (
                    () => calls.Add("save"),
                    () => { calls.Add("query"); throw new Exception("bad query"); },
                    () => calls.Add("rollback"),
                    () => calls.Add("release")
                )
            );

            CollectionAssert.AreEqual(new[] { "save", "query", "rollback", "release" }, calls);
            Assert.IsNotNull(ex.InnerException);
        }

        [TestMethod]
        public void SavepointCreationFailure_DoesNotRunQueryOrFullRollback()
        {
            var calls = new List<string>();

            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>
                (
                    () => { calls.Add("save"); throw new Exception("save failed"); },
                    () => { calls.Add("query"); return 1; },
                    () => calls.Add("rollback"),
                    () => calls.Add("release")
                )
            );

            CollectionAssert.AreEqual(new[] { "save" }, calls);
        }

        [TestMethod]
        public void RollbackToSavepointFailure_ReportsUncertainState()
        {
            var calls = new List<string>();

            var ex = Assert.ThrowsExactly<AggregateException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>
                (
                    () => calls.Add("save"),
                    () => { calls.Add("query"); throw new Exception("query failed"); },
                    () => { calls.Add("rollback"); throw new Exception("rollback failed"); },
                    () => calls.Add("release")
                )
            );

            CollectionAssert.AreEqual(new[] { "save", "query", "rollback" }, calls);
            Assert.AreEqual(2, ex.InnerExceptions.Count);
        }

        [TestMethod]
        public void ReleaseAfterSuccessfulQueryFailure_AttemptsRollbackAndReleaseAgain()
        {
            var calls = new List<string>();
            var count = 0;

            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>
                (
                    () => calls.Add("save"),
                    () => { calls.Add("query"); return 1; },
                    () => calls.Add("rollback"),
                    () => { calls.Add("release"); if (++count == 1) throw new Exception("release failed"); }
                )
            );

            CollectionAssert.AreEqual(new[] { "save", "query", "release", "rollback", "release" }, calls);
        }

        [TestMethod]
        public void ReleaseAfterRollbackFailure_IsNotSilenced()
        {
            var calls = new List<string>();

            var ex = Assert.ThrowsExactly<AggregateException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>
                (
                    () => calls.Add("save"),
                    () => { calls.Add("query"); throw new Exception("query failed"); },
                    () => calls.Add("rollback"),
                    () => { calls.Add("release"); throw new Exception("release failed"); }
                )
            );

            CollectionAssert.AreEqual(new[] { "save", "query", "rollback", "release" }, calls);
            Assert.AreEqual(2, ex.InnerExceptions.Count);
        }

        [TestMethod]
        public void NullCallbacks_AreRejectedBeforeSavepoint()
        {
            Assert.ThrowsExactly<ArgumentNullException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>(null, () => 1, () => { }, () => { })
            );

            Assert.ThrowsExactly<ArgumentNullException>
            (
                () => PostgreSqlMetadataSavepointExecutor.Execute<int>(() => { }, null, () => { }, () => { })
            );
        }
    }
}
