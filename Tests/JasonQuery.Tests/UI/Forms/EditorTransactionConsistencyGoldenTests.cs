using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace JasonQuery.Tests.UI.Forms
{
    [TestClass]
    [TestCategory("F2C2F1")]
    public sealed class EditorTransactionConsistencyGoldenTests
    {
        private static string Classify(string sql)
        {
            var assembly = typeof(QueryForm).Assembly;
            var type = assembly.GetType("JasonQuery.Core.Database.Execution.EditorTransactionCommandPolicy", false);

            Assert.IsNotNull(type, "F2C2F1_EXPECT_RED: missing transaction-control classifier.");

            var method = type.GetMethod("Classify", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method, "Missing Classify(string).");

            var value = method.Invoke(null, new object[] { sql });

            Assert.IsNotNull(value);
            return value.ToString();
        }

        private static string ReadSource(params string[] parts)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var dir = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && dir != null; depth++)
                {
                    var file = Path.Combine(dir.FullName, Path.Combine(parts));

                    if (File.Exists(file))
                    {
                        return File.ReadAllText(file);
                    }

                    dir = dir.Parent;
                }
            }

            Assert.Fail("Source not found in solution working tree: " + Path.Combine(parts));
            return string.Empty;
        }

        private static string Reader(string name)
        {
            return ReadSource("JasonQuery", "Database", "Providers", "Readers", name);
        }

        private static string ExtractEditorBody(string source)
        {
            var begin = source.IndexOf("public void ExecuteNonQuery(object sqlQuery)", StringComparison.Ordinal);
            var end = source.IndexOf("public int ExecuteSingleNonQueryOnVerifiedConnection(", begin >= 0 ? begin : 0, StringComparison.Ordinal);

            Assert.IsTrue(begin >= 0 && end > begin, "Cannot locate protected Editor ExecuteNonQuery range.");
            return source.Substring(begin, end - begin);
        }

        private static void VerifyReaderRouting(string file)
        {
            var source = Reader(file);
            var editor = ExtractEditorBody(source);
            var call = "ExecuteEditorManagedNonQueryStatement(";

            Assert.AreEqual(2, System.Text.RegularExpressions.Regex.Matches(editor, System.Text.RegularExpressions.Regex.Escape(call)).Count, "Both single-statement and batch Editor paths must use the same transaction-aware dispatch.");
            Assert.DoesNotContain(editor, "affectedRows = _command.ExecuteNonQuery();", "Direct SQL execution must not bypass transaction-aware Editor dispatch.");
        }

        private static void VerifyManagedTransaction(string file)
        {
            var source = Reader(file);
            var start = source.IndexOf("private int ExecuteEditorManagedNonQueryStatement(", StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing Reader transaction-aware dispatch.");

            var end = source.IndexOf("public string GetResultString(", start, StringComparison.Ordinal);

            Assert.IsTrue(end > start, "Cannot isolate Reader transaction-aware dispatch.");

            var method = source.Substring(start, end - start);

            StringAssert.Contains(method, "transaction.Commit();");
            StringAssert.Contains(method, "transaction.Rollback();");
            StringAssert.Contains(method, "_command.Transaction = _transaction;");
            StringAssert.Contains(method, "throw new InvalidOperationException");
            StringAssert.Contains(method, "_transaction = null;");
        }

        [TestMethod]
        public void F101_CommitCommandsAreRecognized()
        {
            Assert.AreEqual("Commit", Classify("commit;"));
            Assert.AreEqual("Commit", Classify("COMMIT TRANSACTION"));
            Assert.AreEqual("Commit", Classify("COMMIT WORK;"));
        }

        [TestMethod]
        public void F102_RollbackCommandsAreRecognized()
        {
            Assert.AreEqual("Rollback", Classify("rollback;"));
            Assert.AreEqual("Rollback", Classify("ROLLBACK TRANSACTION;"));
            Assert.AreEqual("Rollback", Classify("ROLLBACK WORK"));
        }

        [TestMethod]
        public void F103_SavepointRollbackMustNotEndManagedTransaction()
        {
            Assert.AreEqual("None", Classify("ROLLBACK TO SAVEPOINT s1"));
            Assert.AreEqual("None", Classify("ROLLBACK TRANSACTION SavePoint1"));
        }

        [TestMethod]
        public void F104_QuotedAndCommentedKeywordsAreNotTransactionCommands()
        {
            Assert.AreEqual("None", Classify("SELECT 'COMMIT'"));
            Assert.AreEqual("None", Classify("-- COMMIT"));
            Assert.AreEqual("None", Classify("/* ROLLBACK */ SELECT 1"));
        }

        [TestMethod]
        public void F105_MultiStatementTextCannotMasqueradeAsOneCommit()
        {
            Assert.AreEqual("None", Classify("COMMIT; DROP TABLE t"));
            Assert.AreEqual("None", Classify("ROLLBACK; DELETE FROM t"));
        }

        [TestMethod]
        public void F106_EmptyOrOtherStatementIsNotTransactionCompletion()
        {
            Assert.AreEqual("None", Classify(null));
            Assert.AreEqual("None", Classify(""));
            Assert.AreEqual("None", Classify("ALTER TABLE t ADD c INT"));
        }

        [TestMethod]
        public void F107_PostgreSqlBothEditorPathsUseManagedDispatch()
        {
            VerifyReaderRouting("PostgreSqlReader.cs");
        }

        [TestMethod]
        public void F108_SqlServerBothEditorPathsUseManagedDispatch()
        {
            VerifyReaderRouting("SqlServerReader.cs");
        }

        [TestMethod]
        public void F109_PostgreSqlDispatchOwnsCommitRollbackAndEnlistsCommands()
        {
            VerifyManagedTransaction("PostgreSqlReader.cs");
        }

        [TestMethod]
        public void F110_SqlServerDispatchOwnsCommitRollbackAndEnlistsCommands()
        {
            VerifyManagedTransaction("SqlServerReader.cs");
        }

        private static string[] SplitSqlServerEditorTransaction(string sql)
        {
            var assembly = typeof(QueryForm).Assembly;
            var type = assembly.GetType("JasonQuery.Database.Providers.Readers.SqlServerEditorTransactionSplitter", false);

            Assert.IsNotNull(type, "F2C2F2_EXPECT_RED: SQL Server Editor trailing transaction splitter is missing.");

            var method = type.GetMethod("Split", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            return (string[])method.Invoke(null, new object[] { sql });
        }

        [TestMethod]
        public void F111_SqlServerEditorSeparatesDdlAndManagedCommit()
        {
            const string sql = "ALTER TABLE [dbo].[f2c2_probe] ADD [col1] int NULL;\r\nCOMMIT;";

            var parts = SplitSqlServerEditorTransaction(sql);

            Assert.AreEqual(2, parts.Length);
            Assert.AreEqual("ALTER TABLE [dbo].[f2c2_probe] ADD [col1] int NULL;", parts[0].Trim());
            Assert.AreEqual("COMMIT;", parts[1].Trim());
        }

        [TestMethod]
        public void F112_QueryFormMustNotReenablePendingAfterCompletion()
        {
            var source = ReadSource("JasonQuery", "UI", "Forms", "QueryForm.cs");

            var start = source.IndexOf
            (
                "var cancelledNonQueryHandled =",
                StringComparison.Ordinal
            );

            var end = source.IndexOf
            (
                "ResetLockingQueryExecutionState();",
                start >= 0 ? start : 0,
                StringComparison.Ordinal
            );

            Assert.IsTrue(start >= 0 && end > start);

            var completionSection = source.Substring(start, end - start);

            Assert.DoesNotContain
            (
                completionSection,
                "TransferValueToMainForm(\"UpdateCommitRollbackButton`\");",
                "The legacy completion handler must not reenable Pending."
            );
        }

        [TestMethod]
        public void F113_SqlServerEditorSplitterIgnoresQuotedSemicolons()
        {
            const string sql = "UPDATE [dbo].[t] SET [note] = 'COMMIT; not a command';\r\nROLLBACK;";

            var parts = SplitSqlServerEditorTransaction(sql);

            Assert.AreEqual(2, parts.Length);
            StringAssert.Contains(parts[0], "'COMMIT; not a command'");
            Assert.AreEqual("ROLLBACK;", parts[1].Trim());
        }

        [TestMethod]
        public void F114_SqlServerEditorSplitterRejectsNestedTransactionBatch()
        {
            const string sql = "BEGIN TRANSACTION; ALTER TABLE [dbo].[t] ADD [id2] int; COMMIT;";

            try
            {
                SplitSqlServerEditorTransaction(sql);

                Assert.Fail("An embedded BEGIN TRANSACTION must not be sent as a managed batch.");
            }
            catch (TargetInvocationException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidOperationException));
            }
        }
    }
}
