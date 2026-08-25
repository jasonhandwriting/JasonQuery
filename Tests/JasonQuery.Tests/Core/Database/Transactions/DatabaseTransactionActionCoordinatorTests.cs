using JasonQuery.Core.Database.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.Transactions
{
    [TestClass]
    public sealed class DatabaseTransactionActionCoordinatorTests
    {
        private const string Timestamp = "2026/07/19 09:30:00";

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_CommitSuccess_DisconnectsAndClearsPending()
        {
            var reader = CreateReader();
            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.WasAttempted);
            Assert.IsTrue(result.TransactionSucceeded);
            Assert.IsTrue(result.DisconnectAttempted);
            Assert.IsTrue(result.DisconnectSucceeded);
            Assert.IsTrue(result.ShouldClearPendingState);
            Assert.IsFalse(result.ShouldKeepPendingState);
            Assert.AreEqual(1, reader.ExecuteCount);
            Assert.AreEqual(1, reader.DisconnectCount);
            Assert.AreEqual("Commit executed at " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_RollbackSuccess_ReturnsRollbackMessage()
        {
            var result = Execute(DatabaseTransactionActionKind.Rollback, CreateReader());

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.AreEqual("Rollback executed at " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullTransactionMessage_IsSuccessful()
        {
            var reader = CreateReader();

            reader.TransactionMessage = null;

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.AreEqual(string.Empty, result.TransactionMessage);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_WhiteSpaceTransactionMessage_IsSuccessful()
        {
            var reader = CreateReader();

            reader.TransactionMessage = "   ";

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.AreEqual(1, reader.DisconnectCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_CommitReturnsError_DoesNotDisconnectAndKeepsPending()
        {
            var reader = CreateReader();

            reader.TransactionMessage = "ErrorCode: 999\r\nErrorMsg: Commit failed";

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.WasAttempted);
            Assert.IsFalse(result.TransactionSucceeded);
            Assert.IsFalse(result.DisconnectAttempted);
            Assert.IsFalse(result.DisconnectSucceeded);
            Assert.IsFalse(result.ShouldClearPendingState);
            Assert.IsTrue(result.ShouldKeepPendingState);
            Assert.AreEqual(0, reader.DisconnectCount);
            Assert.AreEqual("ErrorCode: 999\r\nErrorMsg: Commit failed " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_RollbackReturnsError_DoesNotDisconnectAndKeepsPending()
        {
            var reader = CreateReader();

            reader.TransactionMessage = "ErrorMsg: Rollback failed";

            var result = Execute(DatabaseTransactionActionKind.Rollback, reader);

            Assert.IsFalse(result.TransactionSucceeded);
            Assert.AreEqual(0, reader.DisconnectCount);
            StringAssert.StartsWith(result.Message, "ErrorMsg: Rollback failed");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_TransactionDelegateThrows_DoesNotDisconnect()
        {
            var reader = CreateReader();

            reader.ThrowOnExecute = true;

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.WasAttempted);
            Assert.IsFalse(result.TransactionSucceeded);
            Assert.IsFalse(result.DisconnectAttempted);
            Assert.AreEqual(0, reader.DisconnectCount);
            Assert.AreEqual("ErrorMsg: execute failed " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_GetStateThrows_DoesNotAttemptTransaction()
        {
            var reader = CreateReader();

            reader.ThrowOnGetState = true;

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsFalse(result.WasAttempted);
            Assert.IsFalse(result.TransactionSucceeded);
            Assert.AreEqual(0, reader.ExecuteCount);
            Assert.AreEqual(0, reader.DisconnectCount);
            Assert.AreEqual("ErrorMsg: state failed " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullReader_ReturnsNotAttempted()
        {
            var result = DatabaseTransactionActionCoordinator.Execute<FakeReader>
            (
                DatabaseTransactionActionKind.Commit,
                null,
                GetState,
                ExecuteTransaction,
                Disconnect,
                GetTimestamp
            );

            Assert.IsFalse(result.WasAttempted);
            Assert.IsTrue(result.ShouldKeepPendingState);
            StringAssert.Contains(result.Message, "database reader is not available");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        [DataRow(ConnectionState.Closed)]
        [DataRow(ConnectionState.Broken)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Executing)]
        [DataRow(ConnectionState.Fetching)]
        public void Execute_ConnectionIsNotOpen_ReturnsNotAttempted(ConnectionState state)
        {
            var reader = CreateReader();

            reader.State = state;

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsFalse(result.WasAttempted);
            Assert.IsFalse(result.TransactionSucceeded);
            Assert.AreEqual(0, reader.ExecuteCount);
            Assert.AreEqual(0, reader.DisconnectCount);
            StringAssert.Contains(result.Message, "database connection is not open");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_DisconnectReturnsFalse_TransactionStillSucceeded()
        {
            var reader = CreateReader();

            reader.DisconnectResult = false;

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.IsTrue(result.ShouldClearPendingState);
            Assert.IsTrue(result.DisconnectAttempted);
            Assert.IsFalse(result.DisconnectSucceeded);
            StringAssert.Contains(result.Message, "transaction was completed");
            StringAssert.Contains(result.Message, "connection could not be closed");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_DisconnectThrows_TransactionStillSucceeded()
        {
            var reader = CreateReader();

            reader.ThrowOnDisconnect = true;

            var result = Execute(DatabaseTransactionActionKind.Rollback, reader);

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.IsTrue(result.ShouldClearPendingState);
            Assert.IsTrue(result.DisconnectAttempted);
            Assert.IsFalse(result.DisconnectSucceeded);
            StringAssert.StartsWith(result.Message, "Rollback executed at " + Timestamp);
            StringAssert.Contains(result.Message, "disconnect failed");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_ErrorMessage_IsTrimmedBeforeTimestamp()
        {
            var reader = CreateReader();

            reader.TransactionMessage = "  ErrorMsg: failed  ";

            var result = Execute(DatabaseTransactionActionKind.Commit, reader);

            Assert.AreEqual("ErrorMsg: failed " + Timestamp, result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_TimestampProviderReturnsNull_StillReturnsSuccessMessage()
        {
            var result = DatabaseTransactionActionCoordinator.Execute
            (
                DatabaseTransactionActionKind.Commit,
                CreateReader(),
                GetState,
                ExecuteTransaction,
                Disconnect,
                () => null
            );

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.AreEqual("Commit executed.", result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_TimestampProviderThrows_DoesNotFailTransaction()
        {
            var result = DatabaseTransactionActionCoordinator.Execute
            (
                DatabaseTransactionActionKind.Commit,
                CreateReader(),
                GetState,
                ExecuteTransaction,
                Disconnect,
                () => throw new InvalidOperationException("clock failed")
            );

            Assert.IsTrue(result.TransactionSucceeded);
            Assert.AreEqual("Commit executed.", result.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullGetState_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => DatabaseTransactionActionCoordinator.Execute
                (
                    DatabaseTransactionActionKind.Commit,
                    CreateReader(),
                    null,
                    ExecuteTransaction,
                    Disconnect,
                    GetTimestamp
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullExecuteTransaction_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => DatabaseTransactionActionCoordinator.Execute
                (
                    DatabaseTransactionActionKind.Commit,
                    CreateReader(),
                    GetState,
                    null,
                    Disconnect,
                    GetTimestamp
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullDisconnect_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => DatabaseTransactionActionCoordinator.Execute
                (
                    DatabaseTransactionActionKind.Commit,
                    CreateReader(),
                    GetState,
                    ExecuteTransaction,
                    null,
                    GetTimestamp
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_NullTimestampProvider_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => DatabaseTransactionActionCoordinator.Execute
                (
                    DatabaseTransactionActionKind.Commit,
                    CreateReader(),
                    GetState,
                    ExecuteTransaction,
                    Disconnect,
                    null
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_CommitResult_RecordsActionKind()
        {
            var result = Execute(DatabaseTransactionActionKind.Commit, CreateReader());

            Assert.AreEqual(DatabaseTransactionActionKind.Commit, result.ActionKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_RollbackResult_RecordsActionKind()
        {
            var result = Execute(DatabaseTransactionActionKind.Rollback, CreateReader());

            Assert.AreEqual(DatabaseTransactionActionKind.Rollback, result.ActionKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Transaction")]
        public void Execute_FailedTransaction_PreservesProviderMessage()
        {
            var reader = CreateReader();

            reader.TransactionMessage = "ERROR: current transaction is aborted";

            var result = Execute(DatabaseTransactionActionKind.Rollback, reader);

            Assert.AreEqual(reader.TransactionMessage, result.TransactionMessage);
        }

        private static DatabaseTransactionActionResult Execute(DatabaseTransactionActionKind actionKind, FakeReader reader)
        {
            return DatabaseTransactionActionCoordinator.Execute
            (
                actionKind,
                reader,
                GetState,
                ExecuteTransaction,
                Disconnect,
                GetTimestamp
            );
        }

        private static ConnectionState GetState(FakeReader reader)
        {
            reader.GetStateCount++;

            if (reader.ThrowOnGetState)
            {
                throw new InvalidOperationException("state failed");
            }

            return reader.State;
        }

        private static string ExecuteTransaction(FakeReader reader)
        {
            reader.ExecuteCount++;

            if (reader.ThrowOnExecute)
            {
                throw new InvalidOperationException("execute failed");
            }

            return reader.TransactionMessage;
        }

        private static bool Disconnect(FakeReader reader)
        {
            reader.DisconnectCount++;

            if (reader.ThrowOnDisconnect)
            {
                throw new InvalidOperationException("disconnect failed");
            }

            return reader.DisconnectResult;
        }

        private static string GetTimestamp()
        {
            return Timestamp;
        }

        private static FakeReader CreateReader()
        {
            return new FakeReader
            {
                State = ConnectionState.Open,
                TransactionMessage = string.Empty,
                DisconnectResult = true
            };
        }

        private sealed class FakeReader
        {
            public ConnectionState State { get; set; }

            public string TransactionMessage { get; set; }

            public bool DisconnectResult { get; set; }

            public bool ThrowOnGetState { get; set; }

            public bool ThrowOnExecute { get; set; }

            public bool ThrowOnDisconnect { get; set; }

            public int GetStateCount { get; set; }

            public int ExecuteCount { get; set; }

            public int DisconnectCount { get; set; }
        }
    }
}
