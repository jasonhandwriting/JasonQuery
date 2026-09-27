using JasonQuery.Core.Security.ConnectionCredentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Tests.Core.Security.ConnectionCredentials
{
    [TestClass]
    public class ConnectionCredentialStorageMigrationBoundaryTests
    {
        [TestMethod]
        public void Execute_WhenVersionIsMissing_MigratesWritesVersionLastAndCommits()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);
            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            boundary.Execute
            (
                connection,
                (dbConnection, transaction) =>
                {
                    Assert.AreSame(connection, dbConnection);
                    Assert.AreSame(connection.LastTransaction, transaction);
                    events.Add("migrate");
                }
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "write-current",
                "commit",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenVersionIsLegacy_MigratesAndCommits()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);

            var store = new FakeVersionStore
            (
                events,
                ConnectionCredentialStorageContract.LegacyVersion
            );

            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            boundary.Execute
            (
                connection,
                (dbConnection, transaction) => events.Add("migrate")
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "write-current",
                "commit",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenVersionIsCurrent_RejectsMigrationAndRollsBack()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);

            var store = new FakeVersionStore
            (
                events,
                ConnectionCredentialStorageContract.CurrentVersion
            );

            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) => events.Add("migrate")
                )
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenVersionIsUnknown_FailsClosedAndRollsBack()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);
            var store = new FakeVersionStore(events, 999);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<NotSupportedException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) => events.Add("migrate")
                )
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenMigrationWorkFails_RollsBackWithoutWritingVersion()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);
            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<TestMigrationException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) =>
                    {
                        events.Add("migrate");
                        throw new TestMigrationException();
                    }
                )
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenVersionWriteFails_RollsBackWithoutCommit()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);

            var store = new FakeVersionStore(events, null)
            {
                ThrowOnWrite = true
            };

            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<TestVersionWriteException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) => events.Add("migrate")
                )
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "write-current",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenCommitFails_AttemptsRollback()
        {
            var events = new List<string>();

            var connection = new FakeConnection(events)
            {
                ThrowOnCommit = true
            };

            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<TestCommitException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) => events.Add("migrate")
                )
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "write-current",
                "commit",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenRollbackAlsoFails_PreservesBothFailures()
        {
            var events = new List<string>();

            var connection = new FakeConnection(events)
            {
                ThrowOnRollback = true
            };

            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            var exception = Assert.ThrowsExactly<InvalidOperationException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) =>
                    {
                        events.Add("migrate");
                        throw new TestMigrationException();
                    }
                )
            );

            Assert.IsInstanceOfType
            (
                exception.InnerException,
                typeof(AggregateException)
            );

            var aggregate = (AggregateException)exception.InnerException;

            Assert.HasCount(2, aggregate.InnerExceptions);

            Assert.IsInstanceOfType
            (
                aggregate.InnerExceptions[0],
                typeof(TestMigrationException)
            );

            Assert.IsInstanceOfType
            (
                aggregate.InnerExceptions[1],
                typeof(TestRollbackException)
            );

            AssertEvents
            (
                events,
                "begin",
                "read",
                "migrate",
                "rollback",
                "dispose"
            );
        }

        [TestMethod]
        public void Execute_WhenConnectionIsClosed_RejectsBeforeBeginningTransaction()
        {
            var events = new List<string>();

            var connection = new FakeConnection(events)
            {
                State = ConnectionState.Closed
            };

            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => boundary.Execute
                (
                    connection,
                    (dbConnection, transaction) => events.Add("migrate")
                )
            );

            Assert.IsEmpty(events);
        }

        [TestMethod]
        public void Execute_WhenMigrationWorkIsNull_RejectsBeforeBeginningTransaction()
        {
            var events = new List<string>();
            var connection = new FakeConnection(events);
            var store = new FakeVersionStore(events, null);
            var boundary = new ConnectionCredentialStorageMigrationBoundary(store);

            Assert.ThrowsExactly<ArgumentNullException>
            (
                () => boundary.Execute(connection, null)
            );

            Assert.IsEmpty(events);
        }

        private static void AssertEvents(IList<string> actual, params string[] expected)
        {
            CollectionAssert.AreEqual
            (
                expected,
                new List<string>(actual)
            );
        }

        private sealed class FakeVersionStore : IConnectionCredentialStorageVersionStore
        {
            private readonly IList<string> _events;
            private readonly int? _persistedVersion;

            public FakeVersionStore(IList<string> events, int? persistedVersion)
            {
                _events = events;
                _persistedVersion = persistedVersion;
            }

            public bool ThrowOnWrite { get; set; }

            public int? ReadPersistedVersion(IDbConnection connection, IDbTransaction transaction)
            {
                _events.Add("read");
                return _persistedVersion;
            }

            public void WriteCurrentVersion(IDbConnection connection, IDbTransaction transaction)
            {
                _events.Add("write-current");

                if (ThrowOnWrite)
                {
                    throw new TestVersionWriteException();
                }
            }
        }

        private sealed class FakeConnection : IDbConnection
        {
            private readonly IList<string> _events;

            public FakeConnection(IList<string> events)
            {
                _events = events;
                State = ConnectionState.Open;
            }

            public bool ThrowOnCommit { get; set; }

            public bool ThrowOnRollback { get; set; }

            public FakeTransaction LastTransaction { get; private set; }

            public string ConnectionString { get; set; }

            public int ConnectionTimeout => 0;

            public string Database => "Test";

            public ConnectionState State { get; set; }

            public IDbTransaction BeginTransaction()
            {
                return BeginTransaction(IsolationLevel.Unspecified);
            }

            public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
            {
                _events.Add("begin");

                LastTransaction =new FakeTransaction
                (
                    this,
                    _events,
                    ThrowOnCommit,
                    ThrowOnRollback
                );

                return LastTransaction;
            }

            public void ChangeDatabase(string databaseName)
            {
                throw new NotSupportedException();
            }

            public void Close()
            {
                State = ConnectionState.Closed;
            }

            public IDbCommand CreateCommand()
            {
                throw new NotSupportedException();
            }

            public void Open()
            {
                State = ConnectionState.Open;
            }

            public void Dispose()
            {
            }
        }

        private sealed class FakeTransaction : IDbTransaction
        {
            private readonly IList<string> _events;
            private readonly bool _throwOnCommit;
            private readonly bool _throwOnRollback;

            public FakeTransaction(IDbConnection connection, IList<string> events, bool throwOnCommit, bool throwOnRollback)
            {
                Connection = connection;
                _events = events;
                _throwOnCommit = throwOnCommit;
                _throwOnRollback = throwOnRollback;
            }

            public IDbConnection Connection { get; }

            public IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

            public void Commit()
            {
                _events.Add("commit");

                if (_throwOnCommit)
                {
                    throw new TestCommitException();
                }
            }

            public void Rollback()
            {
                _events.Add("rollback");

                if (_throwOnRollback)
                {
                    throw new TestRollbackException();
                }
            }

            public void Dispose()
            {
                _events.Add("dispose");
            }
        }

        private sealed class TestMigrationException : Exception
        {
        }

        private sealed class TestVersionWriteException : Exception
        {
        }

        private sealed class TestCommitException : Exception
        {
        }

        private sealed class TestRollbackException : Exception
        {
        }
    }
}
