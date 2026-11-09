using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace JasonQuery.Tests.UI.Forms
{
    /// <summary>
    /// F2-C1: execution completion must be based on acknowledged reader outcome,
    /// never merely on SQL text, UI preparation, or a batch-ending callback.
    /// This file intentionally compiles before the proposed production policy exists.
    /// </summary>
    [TestClass]
    [TestCategory("F2C1")]
    public sealed class EditorExecutionCompletionGoldenTests
    {
        private const string PolicyTypeName = "JasonQuery.Core.Database.Execution.EditorStatementCompletionPolicy";

        private sealed class Decision
        {
            public bool StatementCompleted { get; set; }
            public bool TransactionClosed { get; set; }
        }

        private static Decision Evaluate(bool readerAcknowledged, string result, string transactionState, bool cancellationRequested)
        {
            var assembly = typeof(QueryForm).Assembly;
            var type = assembly.GetType(PolicyTypeName, false);

            Assert.IsNotNull(type, "F2C1_EXPECT_RED: missing " + PolicyTypeName + ". Do not fabricate a successful execution receipt.");

            var method = type.GetMethod
            (
                "Evaluate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(bool), typeof(string), typeof(string), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "F2C1_EXPECT_RED: missing Evaluate(bool readerAcknowledged, string result, string transactionState, bool cancellationRequested).");

            var value = method.Invoke(null, new object[] { readerAcknowledged, result, transactionState, cancellationRequested });

            Assert.IsNotNull(value, "A completion decision must not be null.");

            var successProperty = value.GetType().GetProperty("CanNotifyStatementCompleted", BindingFlags.Public | BindingFlags.Instance);
            var closeProperty = value.GetType().GetProperty("CanNotifyTransactionClosed", BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(successProperty, "Completion decision must expose CanNotifyStatementCompleted.");
            Assert.IsNotNull(closeProperty, "Completion decision must expose CanNotifyTransactionClosed.");
            Assert.AreEqual(typeof(bool), successProperty.PropertyType);
            Assert.AreEqual(typeof(bool), closeProperty.PropertyType);

            return new Decision
            {
                StatementCompleted = (bool)successProperty.GetValue(value),
                TransactionClosed = (bool)closeProperty.GetValue(value)
            };
        }

        private static void Expect(bool readerAcknowledged, string result, string transactionState, bool cancelled, bool statementCompleted, bool transactionClosed)
        {
            var actual = Evaluate(readerAcknowledged, result, transactionState, cancelled);

            Assert.AreEqual(statementCompleted, actual.StatementCompleted, "Verified statement completion mismatch.");
            Assert.AreEqual(transactionClosed, actual.TransactionClosed, "Verified transaction completion mismatch.");
        }

        private static string ReadSource(params string[] parts)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var level = 0; level < 12 && directory != null; level++)
                {
                    var relative = Path.Combine(parts);
                    var candidate = Path.Combine(directory.FullName, relative);

                    if (File.Exists(candidate))
                    {
                        return File.ReadAllText(candidate);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("Cannot locate JasonQuery source file " + Path.Combine(parts) + ". Run tests from the solution working tree.");
            return string.Empty;
        }

        [TestMethod]
        public void C101_NoReaderAcknowledgementNeverNotifies()
        {
            Expect(false, "Complete", "TX_CLOSED", false, false, false);
        }

        [TestMethod]
        public void C102_VerifiedSuccessfulCommitCanNotifyClosure()
        {
            Expect(true, "Complete", "TX_CLOSED", false, true, true);
        }

        [TestMethod]
        public void C103_VerifiedSuccessfulPendingStatementDoesNotNotifyClosure()
        {
            Expect(true, "Complete", "TX_PENDING", false, true, false);
        }

        [TestMethod]
        public void C104_VerifiedSuccessfulUnchangedStatementDoesNotNotifyClosure()
        {
            Expect(true, "Complete", "TX_UNCHANGED", false, true, false);
        }

        [TestMethod]
        public void C105_FailedStatementCannotCloseTransactionEvenWithStaleToken()
        {
            Expect(true, "Error", "TX_CLOSED", false, false, false);
        }

        [TestMethod]
        public void C106_CancelledStatementNeverNotifies()
        {
            Expect(true, "Cancel", "TX_CLOSED", true, false, false);
        }

        [TestMethod]
        public void C107_UnknownOutcomeNeverNotifies()
        {
            Expect(true, "Unknown", "TX_CLOSED", false, false, false);
        }

        [TestMethod]
        public void C108_CancellationRequestCannotBeOverriddenByAmbiguousSuccess()
        {
            Expect(true, "Complete", "TX_CLOSED", true, false, false);
        }

        [TestMethod]
        public void C109_PartiallySuccessfulBatchMustUseEachStatementReceipt()
        {
            //Illustrates two *separate* per-statement receipts from one batch.
            //The first success remains a success; the second failure must not turn the batch-ending TX_CLOSED token into a verified commit.
            var first = Evaluate(true, "Complete", "TX_PENDING", false);
            var second = Evaluate(true, "Error", "TX_CLOSED", false);

            Assert.IsTrue(first.StatementCompleted);
            Assert.IsFalse(first.TransactionClosed);
            Assert.IsFalse(second.StatementCompleted);
            Assert.IsFalse(second.TransactionClosed);
        }

        [TestMethod]
        public void C110_MissingResultAndUnknownTransactionTokenCannotCloseTransaction()
        {
            Expect(true, null, "TX_CLOSED", false, false, false);
            Expect(true, "Complete", "TX_UNKNOWN", false, true, false);
        }

        [TestMethod]
        public void C111_PreExecutionUiMustNeverBroadcastCompletedTransaction()
        {
            var source = ReadSource("JasonQuery", "UI", "Forms", "QueryForm.QueryExecutionUiState.cs");
            var start = source.IndexOf("private void ApplyBeforeNonQueryExecutionUiState(", StringComparison.Ordinal);
            var next = source.IndexOf("private void ApplyBeforeQueryExecutionQueryState(", StringComparison.Ordinal);

            Assert.IsTrue(start >= 0 && next > start, "Cannot locate the current pre-execution UI method.");

            var beforeExecution = source.Substring(start, next - start);

            Assert.DoesNotContain
            (
                beforeExecution,
                "TransferValueToMainForm(\"ExecuteCommitRollback`\"",
                "F2C1_EXPECT_RED: ExecuteCommitRollback is broadcast BEFORE SQL is dispatched."
            );
        }

        [TestMethod]
        public void C112_ExistingReadersAlreadyPublishExplicitTransactionTokens()
        {
            foreach (var fileName in new[] { "OracleReader.cs", "PostgreSqlReader.cs", "SqlServerReader.cs", "MySqlReader.cs" })
            {
                var source = ReadSource("JasonQuery", "Database", "Providers", "Readers", fileName);

                StringAssert.Contains(source, "DatabaseTransactionStatePolicy.ResolveStateToken", fileName);
                StringAssert.Contains(source, "SQLExecuteAffected", fileName);
            }
        }
    }
}
