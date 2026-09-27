using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal static class IntegrationTestTransactionAssertions
    {
        public static void AssertInsertCommitVisibility(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                var affected = fixture.SessionA.ExecuteNonQuery(fixture.Dialect.BuildInsertSql(2, "Committed"), out var error, out var pending, out var closed);

                Assert.AreEqual(string.Empty, error);
                Assert.AreEqual(1, affected);
                Assert.IsTrue(pending);
                Assert.IsFalse(closed);
                Assert.IsFalse(fixture.ObserverRowExists(fixture.SessionB, 2));
                Assert.AreEqual(string.Empty, fixture.SessionA.Commit());
                fixture.ReconnectSessionB();
                Assert.AreEqual("Committed", fixture.ReadName(fixture.SessionB, 2));
            }
        }

        public static void AssertUpdateRollbackVisibility(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                fixture.SessionA.ExecuteNonQuery(fixture.Dialect.BuildUpdateSql(1, "Changed"), out var error, out var pending, out var closed);

                Assert.AreEqual(string.Empty, error);
                Assert.IsTrue(pending);
                Assert.IsFalse(closed);
                Assert.AreEqual("Changed", fixture.ReadName(fixture.SessionA, 1));

                if (sourceType == DataSourceType.SqlServer)
                {
                    Assert.IsNull(fixture.ReadObserverName(fixture.SessionB, 1));
                }
                else
                {
                    Assert.AreEqual("Original", fixture.ReadObserverName(fixture.SessionB, 1));
                }
                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                fixture.ReconnectSessionA();
                fixture.ReconnectSessionB();
                Assert.AreEqual("Original", fixture.ReadName(fixture.SessionA, 1));
                Assert.AreEqual("Original", fixture.ReadName(fixture.SessionB, 1));
            }
        }

        public static void AssertDeleteRollbackVisibility(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                fixture.SessionA.ExecuteNonQuery(fixture.Dialect.BuildDeleteSql(1), out var error, out var pending, out var closed);

                Assert.AreEqual(string.Empty, error);
                Assert.IsTrue(pending);
                Assert.IsFalse(closed);
                Assert.IsFalse(fixture.RowExists(fixture.SessionA, 1));

                if (sourceType == DataSourceType.SqlServer)
                {
                    Assert.IsFalse(fixture.ObserverRowExists(fixture.SessionB, 1));
                }
                else
                {
                    Assert.IsTrue(fixture.ObserverRowExists(fixture.SessionB, 1));
                }
                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                fixture.ReconnectSessionA();
                Assert.IsTrue(fixture.RowExists(fixture.SessionA, 1));
            }
        }

        public static void AssertFailedSqlDoesNotReportPending(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                fixture.SessionA.ExecuteNonQuery(fixture.Dialect.BuildInvalidSql(), out var error, out var pending, out var closed);

                Assert.IsFalse(string.IsNullOrWhiteSpace(error));
                Assert.IsFalse(pending);
                Assert.IsFalse(closed);
            }
        }

        public static void AssertDdlTransactionSemantics(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                fixture.DropDdlTableIfPresent();
                fixture.SessionA.ExecuteNonQuery(fixture.Dialect.BuildCreateDdlTableSql(), out var error, out var pending, out var closed);

                Assert.AreEqual(string.Empty, error);
                Assert.AreEqual(!fixture.Dialect.DdlUsesImplicitCommit, pending);
                Assert.AreEqual(fixture.Dialect.DdlUsesImplicitCommit, closed);

                if (fixture.Dialect.DdlUsesImplicitCommit)
                {
                    fixture.ReconnectSessionA();
                }

                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                fixture.ReconnectSessionA();
                fixture.ReconnectSessionB();
                Assert.AreEqual(fixture.Dialect.DdlUsesImplicitCommit, fixture.DdlTableExists(fixture.SessionB));
            }
        }

        public static void AssertRepeatedTransactionActionsAreDefensive(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                Assert.AreEqual(string.Empty, fixture.SessionA.Commit());
                Assert.AreEqual(string.Empty, fixture.SessionA.Commit());
                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                Assert.AreEqual(System.Data.ConnectionState.Open, fixture.SessionA.State);
            }
        }

        public static void AssertLockingQueryRepeatConflictAndRelease(TestContext testContext, DataSourceType sourceType)
        {
            using (var fixture = CreateFixture(testContext, sourceType))
            {
                fixture.Prepare();
                var timeoutSql = fixture.Dialect.BuildSetLockTimeoutSql();

                if (!string.IsNullOrEmpty(timeoutSql))
                {
                    fixture.SessionB.ExecuteNonQuery(timeoutSql, out var timeoutError, out var timeoutPending, out var timeoutClosed);
                    Assert.AreEqual(string.Empty, timeoutError);
                    Assert.IsFalse(timeoutPending);
                    Assert.IsFalse(timeoutClosed);
                }

                var first = fixture.SessionA.ExecuteSinglePassQuery(fixture.Dialect.BuildLockingQuerySql(1, false));
                var second = fixture.SessionA.ExecuteSinglePassQuery(fixture.Dialect.BuildLockingQuerySql(1, false));

                Assert.IsTrue(first.Succeeded, first.ErrorPayload);
                Assert.IsTrue(second.Succeeded, second.ErrorPayload);
                Assert.HasCount(1, first.Data.Rows);
                Assert.HasCount(1, second.Data.Rows);
                Assert.IsNotNull(first.Schema);
                Assert.IsNotNull(second.Schema);

                var conflict = fixture.SessionB.ExecuteSinglePassQuery(fixture.Dialect.BuildLockingQuerySql(1, true));
                Assert.IsFalse(conflict.Succeeded, "Session B unexpectedly acquired a conflicting lock.");
                Assert.IsTrue(fixture.Dialect.IsExpectedLockConflict(conflict.ErrorPayload), conflict.ErrorPayload);

                Assert.AreEqual(string.Empty, fixture.SessionA.Rollback());
                fixture.ReconnectSessionA();
                fixture.ReconnectSessionB();

                if (!string.IsNullOrEmpty(timeoutSql))
                {
                    fixture.SessionB.ExecuteNonQuery(timeoutSql, out var timeoutError2);
                    Assert.AreEqual(string.Empty, timeoutError2);
                }

                var afterRelease = fixture.SessionB.ExecuteSinglePassQuery(fixture.Dialect.BuildLockingQuerySql(1, true));
                Assert.IsTrue(afterRelease.Succeeded, afterRelease.ErrorPayload);
                Assert.HasCount(1, afterRelease.Data.Rows);
            }
        }

        private static IntegrationTestTransactionFixture CreateFixture(TestContext testContext, DataSourceType sourceType)
        {
            var settings = IntegrationTestDatabaseSettings.Load(testContext, sourceType);
            settings.RequireConfigured();
            return new IntegrationTestTransactionFixture(settings);
        }
    }
}
