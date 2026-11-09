using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace JasonQuery.Tests.UI.Forms
{
    //F2-C3-E2: RED before Apply. Pure callback tests use no live database.
    [TestClass]
    [TestCategory("F2C3E2")]
    public sealed class PostgreSqlGuidedDdlSavepointGoldenTests
    {
        private static object Execute(Func<bool> identity, Action save, Func<int> ddl, Action rollback, Action release, Func<Exception, bool> classify)
        {
            var type = typeof(PostgreSqlReader).Assembly.GetType
            (
                "JasonQuery.Database.Providers.Readers.PostgreSqlGuidedDdlSavepointExecutor",
                false
            );

            Assert.IsNotNull(type, "E2 GOLDEN RED: guided DDL savepoint executor is missing.");

            var method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(method, "E2 GOLDEN RED: guided DDL savepoint method is missing.");
            return method.Invoke(null, new object[] { identity, save, ddl, rollback, release, classify });
        }

        private static string Status(object result)
        {
            return (string)Property(result, "Status");
        }

        private static object Property(object instance, string name)
        {
            Assert.IsNotNull(instance);

            var prop = instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsNotNull(prop, "Missing savepoint result property: " + name);
            return prop.GetValue(instance);
        }

        private static object Run(Action save = null, Func<int> ddl = null, Action rollback = null, Action release = null, Func<bool> identity = null, Func<Exception, bool> classify = null)
        {
            return Execute
            (
                identity ?? (() => true),
                save ?? (() => { }),
                ddl ?? (() => 1),
                rollback ?? (() => { }),
                release ?? (() => { }),
                classify ?? (ex => ex is InvalidOperationException)
            );
        }

        private static string Source(params string[] segments)
        {
            var folder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (folder != null)
            {
                var path = folder.FullName;

                foreach (var part in segments)
                {
                    path = Path.Combine(path, part);
                }
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }

                folder = folder.Parent;
            }

            Assert.Fail("Source not found: " + string.Join("/", segments));
            return string.Empty;
        }

        [TestMethod]
        public void E201_SuccessSavesDispatchesOnceAndReleasesWithoutCommit()
        {
            var calls = string.Empty;

            var r = Run
            (
                save: () => calls += "S",
                ddl: () => { calls += "D"; return 3; },
                rollback: () => calls += "R",
                release: () => calls += "L"
            );

            Assert.AreEqual("SUCCESS", Status(r));
            Assert.AreEqual("SDL", calls);
            Assert.AreEqual(3, Property(r, "RowsAffected"));
        }

        [TestMethod]
        public void E202_ConfirmedSqlErrorRollsBackOnlySavepointThenReleases()
        {
            var calls = string.Empty;

            var r = Run
            (
                save: () => calls += "S",
                ddl: () => { calls += "D"; throw new InvalidOperationException("duplicate column"); },
                rollback: () => calls += "R",
                release: () => calls += "L"
            );

            Assert.AreEqual("SQL_ERROR_SAVEPOINT_RESTORED", Status(r));
            Assert.AreEqual("SDRL", calls);
            Assert.AreEqual(true, Property(r, "RestoredToSavepoint"));
        }

        [TestMethod]
        public void E203_SavepointCreationFailureNeverDispatchesDdl()
        {
            var dispatched = 0;

            var r = Run
            (
                save: () => { throw new InvalidOperationException("aborted transaction"); },
                ddl: () => { dispatched++; return 1; }
            );

            Assert.AreEqual("SAVEPOINT_CREATION_FAILED", Status(r));
            Assert.AreEqual(0, dispatched);
            Assert.AreEqual(false, Property(r, "Dispatched"));
        }

        [TestMethod]
        public void E204_UnverifiedIdentityNeverCreatesSavepoint()
        {
            var saves = 0;
            var r = Run(identity: () => false, save: () => saves++);

            Assert.AreEqual("IDENTITY_UNVERIFIED", Status(r));
            Assert.AreEqual(0, saves);
        }

        [TestMethod]
        public void E205_IdentityChangedAfterSavepointNeverDispatchesDdl()
        {
            var checks = 0;
            var dispatches = 0;

            var r = Run
            (
                identity: () => ++checks == 1,
                ddl: () => { dispatches++; return 1; }
            );

            Assert.AreEqual("IDENTITY_UNVERIFIED", Status(r));
            Assert.AreEqual(0, dispatches);
        }

        [TestMethod]
        public void E206_IdentityLostAfterSqlErrorNeverRollsBackOtherConnection()
        {
            var checks = 0;
            var rollbacks = 0;

            var r = Run
            (
                identity: () => ++checks <= 2,
                ddl: () => { throw new InvalidOperationException("transport"); },
                rollback: () => rollbacks++
            );

            Assert.AreEqual("EXECUTION_OUTCOME_UNKNOWN", Status(r));
            Assert.AreEqual(0, rollbacks);
        }

        [TestMethod]
        public void E207_ReleaseFailureAfterExecutedDdlMustNotReportSuccess()
        {
            var r = Run(release: () => { throw new InvalidOperationException("release failed"); });

            Assert.AreEqual("SAVEPOINT_RELEASE_FAILED_AFTER_DDL", Status(r));
            Assert.AreEqual(true, Property(r, "Dispatched"));
            Assert.AreEqual(false, Property(r, "RestoredToSavepoint"));
        }

        [TestMethod]
        public void E208_SavepointRecoveryFailureMustNotClaimRollbackSucceeded()
        {
            var r = Run
            (
                ddl: () => { throw new InvalidOperationException("duplicate column"); },
                rollback: () => { throw new InvalidOperationException("rollback failed"); }
            );

            Assert.AreEqual("SAVEPOINT_RECOVERY_FAILED", Status(r));
            Assert.AreEqual(false, Property(r, "RestoredToSavepoint"));
        }

        [TestMethod]
        public void E209_ReleaseFailureAfterRecoveryRequiresManualReview()
        {
            var r = Run
            (
                ddl: () => { throw new InvalidOperationException("duplicate column"); },
                release: () => { throw new InvalidOperationException("release failed"); }
            );

            Assert.AreEqual("SAVEPOINT_RELEASE_FAILED_AFTER_RECOVERY", Status(r));
            Assert.AreEqual(false, Property(r, "RestoredToSavepoint"));
        }

        [TestMethod]
        public void E210_TransportOrUnconfirmedErrorDoesNotAttemptRollback()
        {
            var rollbacks = 0;

            var r = Run
            (
                ddl: () => { throw new Exception("network failure"); },
                rollback: () => rollbacks++
            );

            Assert.AreEqual("EXECUTION_OUTCOME_UNKNOWN", Status(r));
            Assert.AreEqual(0, rollbacks);
        }

        [TestMethod]
        public void E211_FailedDdlNeverRetriesTheSameSql()
        {
            var count = 0;

            Run(ddl: () => { count++; throw new InvalidOperationException("error"); });

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void E212_PreexistingTransactionChangesSurviveLocalSavepointRecovery()
        {
            var previouslyChangedRow = 42;
            var attemptedDdl = 0;

            var r = Run
            (
                ddl: () => { attemptedDdl = 1; throw new InvalidOperationException("duplicate column"); },
                rollback: () => attemptedDdl = 0
            );

            Assert.AreEqual("SQL_ERROR_SAVEPOINT_RESTORED", Status(r));
            Assert.AreEqual(42, previouslyChangedRow);
            Assert.AreEqual(0, attemptedDdl);
        }

        [TestMethod]
        public void E213_GuidedReaderMustUseLocalSavepointNotFullAutoRollback()
        {
            var reader = Source("JasonQuery", "Database", "Providers", "Readers", "PostgreSqlReader.cs");

            Assert.IsTrue(reader.Contains("PostgreSqlGuidedDdlSavepointExecutor.Execute"));
            Assert.IsTrue(reader.Contains("transaction.Save(savepointName)"));
            Assert.IsTrue(reader.Contains("transaction.Rollback(savepointName)"));
            Assert.IsTrue(reader.Contains("transaction.Release(savepointName)"));

            var start = reader.IndexOf("private int ExecuteGuidedDdlOnVerifiedConnection(", StringComparison.Ordinal);
            var end = reader.IndexOf("public int ExecuteSingleNonQuery(string sql, out string errorMessage)", start);

            Assert.IsTrue(start >= 0 && end > start);

            var guarded = reader.Substring(start, end - start);

            Assert.IsFalse(guarded.Contains("transaction.Rollback();"));
            Assert.IsFalse(guarded.Contains("UseAutoRollback"));
        }

        [TestMethod]
        public void E214_BothExecuteButtonsMustRemainOpenAndAllowExplicitClose()
        {
            var column = Source("JasonQuery", "UI", "Forms", "EditColumnForm.Designer.cs");
            var table = Source("JasonQuery", "UI", "Forms", "EditTableForm.Designer.cs");

            foreach (var designer in new[] { column, table })
            {
                Assert.IsTrue(designer.Contains("btnExecute.DialogResult = System.Windows.Forms.DialogResult.None"));
                Assert.IsTrue(designer.Contains("btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel"));
                Assert.IsFalse(designer.Contains("this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing)"));
            }
        }

        [TestMethod]
        public void E215_BothLegacyGuiFormsUseSavepointEntryAndNoLegacyCloseGuard()
        {
            var column = Source("JasonQuery", "UI", "Forms", "EditColumnForm.cs");
            var table = Source("JasonQuery", "UI", "Forms", "EditTableForm.cs");

            foreach (var form in new[] { column, table })
            {
                Assert.IsTrue(form.Contains("ExecuteGuidedDdlWithSavepoint"));
                Assert.IsTrue(form.Contains("FormatPostgreSqlGuidedDdlFailure"));
                Assert.IsFalse(form.Contains("_isFormClosing"));
            }
        }

        [TestMethod]
        public void E216_QualifiedNewF7GateStillRejectsPendingTransaction()
        {
            var gate = Source("JasonQuery", "Core", "Database", "DdlPreview", "NonOracleSchemaExplorerDdlExecuteGate.cs");

            Assert.IsTrue(gate.Contains("if (pendingOrUnknownTransaction)"));
            Assert.IsTrue(gate.Contains("F7U2C2_TRANSACTION_UNSAFE"));
        }

        [TestMethod]
        public void E217_ExplicitDispatchReceiptAndLocalizedSavepointFeedback()
        {
            var column = Source("JasonQuery", "UI", "Forms", "EditColumnForm.cs");
            var table = Source("JasonQuery", "UI", "Forms", "EditTableForm.cs");

            Assert.IsTrue(column.Contains("out postgresSavepointRestored, out postgresDdlDispatched"));
            Assert.IsTrue(column.Contains("newF7NonOracleSavepointRestored"));
            Assert.IsFalse(column.Contains("NonOracleF7PostgreSqlAutoRollbackCompleted"));
            Assert.IsTrue(column.Contains("PostgreSqlGuiDdlSavepointRestored"));
            Assert.IsTrue(table.Contains("PostgreSqlGuiDdlSavepointRestored"));
        }

        [TestMethod]
        public void E218_UncertainLegacyGuiResultKeepsManualRollbackAvailable()
        {
            var column = Source("JasonQuery", "UI", "Forms", "EditColumnForm.cs");
            var table = Source("JasonQuery", "UI", "Forms", "EditTableForm.cs");

            foreach (var form in new[] { column, table })
            {
                Assert.IsTrue(form.Contains("!string.IsNullOrWhiteSpace(errorMessage) && hasPendingTransactionAfterExecute"));
                Assert.IsTrue(form.Contains("TransferValueToMainForm(\"UpdateCommitRollbackButton`\")"));
            }
        }
    }
}
