using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.Forms
{
    /// <summary>
    /// F2-C2: minimal schema-dirty lifecycle for acknowledged structural DDL.
    /// No database or WinForms UI is accessed by these contract tests.
    /// RED is expected until the C2 implementation is installed.
    /// </summary>
    [TestClass]
    [TestCategory("F2C2")]
    public sealed class SchemaDirtyPolicyGoldenTests
    {
        private const string PolicyTypeName = "JasonQuery.Core.Database.Execution.SchemaDirtyPolicy";
        private const string ClassifierTypeName = "JasonQuery.Core.Database.Execution.SchemaChangeDdlClassifier";

        private sealed class Decision
        {
            public bool IsDirty { get; set; }
            public bool ShouldRefresh { get; set; }
        }

        private static Decision Evaluate(bool alreadyDirty, bool verifiedStructuralDdl, bool transactionClosed, bool refreshSucceeded)
        {
            var type = typeof(QueryForm).Assembly.GetType(PolicyTypeName, false);

            Assert.IsNotNull(type, "F2C2_EXPECT_RED: missing " + PolicyTypeName);

            var method = type.GetMethod
            (
                "Evaluate",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool) },
                null
            );

            Assert.IsNotNull(method, "F2C2_EXPECT_RED: missing Evaluate(bool alreadyDirty, bool verifiedStructuralDdl, bool transactionClosed, bool refreshSucceeded).");

            var result = method.Invoke(null, new object[] { alreadyDirty, verifiedStructuralDdl, transactionClosed, refreshSucceeded });

            Assert.IsNotNull(result);

            var dirty = result.GetType().GetProperty("IsDirty", BindingFlags.Public | BindingFlags.Instance);
            var refresh = result.GetType().GetProperty("ShouldRefresh", BindingFlags.Public | BindingFlags.Instance);

            Assert.IsNotNull(dirty, "The decision must have an IsDirty property.");
            Assert.IsNotNull(refresh, "The decision must have a ShouldRefresh property.");
            Assert.AreEqual(typeof(bool), dirty.PropertyType);
            Assert.AreEqual(typeof(bool), refresh.PropertyType);

            return new Decision
            {
                IsDirty = (bool)dirty.GetValue(result),
                ShouldRefresh = (bool)refresh.GetValue(result)
            };
        }

        private static void Expect(bool alreadyDirty, bool verifiedStructuralDdl, bool transactionClosed, bool refreshSucceeded, bool expectedDirty, bool expectedRefresh)
        {
            var actual = Evaluate(alreadyDirty, verifiedStructuralDdl, transactionClosed, refreshSucceeded);

            Assert.AreEqual(expectedDirty, actual.IsDirty, "Schema dirty state mismatch.");
            Assert.AreEqual(expectedRefresh, actual.ShouldRefresh, "Metadata refresh request mismatch.");
        }

        private static bool IsStructuralDdl(string sql)
        {
            var type = typeof(QueryForm).Assembly.GetType(ClassifierTypeName, false);

            Assert.IsNotNull(type, "F2C2_EXPECT_RED: missing " + ClassifierTypeName);

            var method = type.GetMethod
            (
                "IsStructuralDdl",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(string) },
                null
            );

            Assert.IsNotNull(method, "F2C2_EXPECT_RED: missing IsStructuralDdl(string statementSql).");
            return (bool)method.Invoke(null, new object[] { sql });
        }

        [TestMethod]
        public void D201_NoSchemaChangeDoesNotRequestRefresh()
        {
            Expect(false, false, false, false, false, false);
        }

        [TestMethod]
        public void D202_VerifiedDdlBeforeTransactionCloseKeepsDirty()
        {
            Expect(false, true, false, false, true, false);
        }

        [TestMethod]
        public void D203_VerifiedDdlWithImplicitCommitRequestsRefresh()
        {
            Expect(false, true, true, false, true, true);
        }

        [TestMethod]
        public void D204_CommitAfterEarlierDdlRequestsRefresh()
        {
            Expect(true, false, true, false, true, true);
        }

        [TestMethod]
        public void D205_ConfirmedRollbackMaySuppressReloadOnlyWhenSafe()
        {
            //F2-C2-F3A supersedes the old unconditional Rollback -> Refresh expectation.
            //Original Evaluate() remains conservative when suppression cannot be justified.
            var afterDdl = Evaluate(false, true, false, false);

            Assert.IsTrue(afterDdl.IsDirty);
            Assert.IsFalse(afterDdl.ShouldRefresh);

            var policy = typeof(QueryForm).Assembly.GetType(PolicyTypeName, false);

            Assert.IsNotNull(policy);

            var method = policy.GetMethod
            (
                "ShouldSkipMetadataReloadForConfirmedRollback",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    typeof(JasonQuery.Core.Database.Connection.DataSourceType),
                    typeof(string), typeof(bool), typeof(bool),
                    typeof(bool), typeof(bool), typeof(bool)
                },
                null
            );

            Assert.IsNotNull(method, "F2C2F3A_EXPECT_RED: rollback-specific policy is missing.");

            var canSkip = (bool)method.Invoke
            (
                null,
                new object[]
                {
                    JasonQuery.Core.Database.Connection.DataSourceType.PostgreSql,
                    "ROLLBACK", true, afterDdl.IsDirty, true, true, false
                }
            );

            Assert.IsTrue(canSkip, "Confirmed same-scope Rollback with trusted baseline must avoid unnecessary Reload.");

            //If the F3 guard fails, the original policy still requires a safe refresh.
            var conservativeFallback = Evaluate(afterDdl.IsDirty, false, true, false);

            Assert.IsTrue(conservativeFallback.IsDirty);
            Assert.IsTrue(conservativeFallback.ShouldRefresh);
        }

        [TestMethod]
        public void D206_UnresolvedTransactionMustNotRefresh()
        {
            Expect(true, false, false, false, true, false);
        }

        [TestMethod]
        public void D207_SuccessfulMetadataReloadClearsDirty()
        {
            Expect(true, false, true, true, false, false);
        }

        [TestMethod]
        public void D208_FailedOrSkippedReloadPreservesDirty()
        {
            Expect(true, false, true, false, true, true);
        }

        [TestMethod]
        public void D209_CommitWithoutEarlierStructuralChangeDoesNotRefresh()
        {
            Expect(false, false, true, false, false, false);
        }

        [TestMethod]
        public void D210_CompletedUnrelatedStatementDoesNotClearExistingDirty()
        {
            var afterDdl = Evaluate(false, true, false, false);

            Assert.IsTrue(afterDdl.IsDirty);
            Assert.IsFalse(afterDdl.ShouldRefresh);

            var afterDml = Evaluate(afterDdl.IsDirty, false, false, false);

            Assert.IsTrue(afterDml.IsDirty);
            Assert.IsFalse(afterDml.ShouldRefresh);

            var afterCommit = Evaluate(afterDml.IsDirty, false, true, false);

            Assert.IsTrue(afterCommit.ShouldRefresh);
        }

        [TestMethod]
        public void D211_TableAndViewStructureDdlAreRecognized()
        {
            foreach (var sql in new[] { "CREATE TABLE jq98_demo (id int)", "ALTER TABLE \"public\".\"Demo\" ADD COLUMN \"new_c\" int", "DROP TABLE jq98_demo", "CREATE VIEW jq98_view AS SELECT 1 AS x", "ALTER VIEW dbo.jq98_view AS SELECT 2 AS x", "DROP VIEW jq98_view" })
            {
                Assert.IsTrue(IsStructuralDdl(sql), "Expected schema-changing DDL: " + sql);
            }
        }

        [TestMethod]
        public void D212_CommentDmlAndQuotedKeywordsAreNotStructuralDdl()
        {
            foreach (var sql in new[] { "COMMENT ON COLUMN t.c IS 'new comment'", "SELECT 'ALTER TABLE demo ADD COLUMN x int' AS sample", "UPDATE demo SET note = 'DROP TABLE demo'", "COMMIT", "ROLLBACK", "-- DROP TABLE fake\r\nSELECT 1" })
            {
                Assert.IsFalse(IsStructuralDdl(sql), "Not schema-changing DDL: " + sql);
            }
        }
    }
}
