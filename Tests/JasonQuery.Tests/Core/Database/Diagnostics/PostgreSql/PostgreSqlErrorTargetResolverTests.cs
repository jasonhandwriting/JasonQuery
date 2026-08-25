using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlErrorTargetResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WithEmptySql_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                string.Empty,
                "42P01",
                15,
                0,
                "a_test",
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WithEmptyTarget_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                "select * from a_test",
                "42P01",
                15,
                0,
                string.Empty,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WithUnsupportedErrorCode_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                "select * from a_test",
                "22001",
                15,
                0,
                "a_test",
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WhenTargetAlreadyContainsQuote_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                "select * from \"a_test\"",
                "42P01",
                15,
                0,
                "\"a_test\"",
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WhenTargetIsMoreThanFiveCharactersAway_ReturnsFalse()
        {
            const string sql = "select 1;\r\n" + "select * from \"a_test\"";

            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                sql,
                "42P01",
                0,
                0,
                "a_test",
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(-1, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_WithRepeatedTarget_SelectsNearestOccurrence()
        {
            const string sql = "select * from custinfo23;\r\n" + "select * from \"custinfo23\"";
            var expectedPosition = sql.LastIndexOf("\"custinfo23\"", StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                sql,
                "42P01",
                expectedPosition,
                0,
                "custinfo23",
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual("\"custinfo23\"", target);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        [DataRow("select * from \"public\".custinfo23 where 1=1", "\"public\".custinfo23")]
        [DataRow("select * from public.\"custinfo23\" where 1=1", "public.\"custinfo23\"")]
        [DataRow("select * from public.custinfo23 where 1=1", "public.custinfo23")]
        [DataRow("select * from \"public.custinfo23\" where 1=1", "\"public.custinfo23\"")]
        [DataRow("select * from \"public\".\"custinfo23\" where 1=1", "\"public\".\"custinfo23\"")]
        public void TryResolveDoubleQuotedIdentifierTarget_42P01QualifiedRelationHistoryCases_ReturnExpectedTarget(string sql, string expectedTarget)
        {
            AssertDoubleQuotedTarget(sql, "42P01", 15, "public.custinfo23", expectedTarget);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        [DataRow("select * from \"custinfo23\" where 1=1", "\"custinfo23\"")]
        [DataRow("select * from custinfo23 where 1=1", "custinfo23")]
        public void TryResolveDoubleQuotedIdentifierTarget_42P01UnqualifiedRelationHistoryCases_ReturnExpectedTarget(string sql, string expectedTarget)
        {
            AssertDoubleQuotedTarget(sql, "42P01", 15, "custinfo23", expectedTarget);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_42703QualifiedSetTarget_SelectsAliasNearReportedPosition()
        {
            const string sql = "update a_test aa\r\n" +
                               "   set aa.t4 = '12345671234567890123'\r\n" +
                               " where aa.t1 = '123456789012345678890123'";

            var expectedPosition = sql.IndexOf("aa.t4", StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                sql,
                "42703",
                25,
                0,
                "aa",
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual("aa", target);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void TryResolveDoubleQuotedIdentifierTarget_42703MissingColumn_SelectsReportedColumn()
        {
            const string sql = "select * from custinfo\r\n" +
                               " where email = 'mail' and balance15 = 1111111";

            var expectedPosition = sql.IndexOf("balance15", StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                sql,
                "42703",
                50,
                0,
                "balance15",
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual("balance15", target);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        [DataRow("COMMENT ON COLUMN \"public\".a_test.\"t1\" IS 'test'", "\"public\".a_test")]
        [DataRow("COMMENT ON COLUMN public.\"a_test\".t1 IS 'test'", "public.\"a_test\"")]
        [DataRow("COMMENT ON COLUMN \"public\".\"a_test\".t1 IS 'test'", "\"public\".\"a_test\"")]
        [DataRow("COMMENT ON COLUMN \"public\".a_test.t1 IS 'test'", "\"public\".a_test")]
        [DataRow("COMMENT ON COLUMN public.a_test.t1 IS 'test'", "public.a_test")]
        public void TryResolveMustBeOwnerTarget_42501ColumnHistoryCases_ReturnTableTarget(string sql, string expectedTarget)
        {
            AssertMustBeOwnerTarget(sql, "must be owner of relation a_test", expectedTarget);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        [DataRow("COMMENT ON TABLE \"a_test\" IS 'test'", "\"a_test\"")]
        [DataRow( "COMMENT ON TABLE public.\"a_test\" IS 'test'", "public.\"a_test\"")]
        [DataRow("COMMENT ON TABLE \"public\".\"a_test\" IS 'test'", "\"public\".\"a_test\"")]
        public void TryResolveMustBeOwnerTarget_42501TableHistoryCases_ReturnTableTarget(string sql, string expectedTarget)
        {
            AssertMustBeOwnerTarget(sql, "must be owner of table a_test", expectedTarget);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_WithMultipleStatements_UsesExecutedStatementRange()
        {
            const string firstSql = "COMMENT ON TABLE public.a_test IS 'first';";
            const string secondSql = "COMMENT ON TABLE public.\"a_test\" IS 'second'";

            var sql = firstSql + "\r\n" + secondSql;
            var queryStart = sql.IndexOf(secondSql, StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                sql,
                secondSql,
                "42501",
                "must be owner of table a_test",
                queryStart,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(sql.IndexOf("public.\"a_test\"", queryStart, StringComparison.Ordinal), position);
            Assert.AreEqual("public.\"a_test\"", target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_DoesNotMatchIdentifierPrefix()
        {
            const string sql = "COMMENT ON TABLE a_test_backup IS 'backup';\r\n" +
                               "COMMENT ON TABLE a_test IS 'test'";

            var expectedPosition = sql.LastIndexOf("a_test", StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                sql,
                sql,
                "42501",
                "must be owner of table a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual("a_test", target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_WithWrongErrorCode_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                "COMMENT ON TABLE a_test IS 'test'",
                "COMMENT ON TABLE a_test IS 'test'",
                "42P01",
                "must be owner of table a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_WithUnsupportedMessage_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                "COMMENT ON TABLE a_test IS 'test'",
                "COMMENT ON TABLE a_test IS 'test'",
                "42501",
                "ownership is required for table a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_WithEmptyExecutedSql_ReturnsFalse()
        {
            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                "COMMENT ON TABLE a_test IS 'test'",
                string.Empty,
                "42501",
                "must be owner of table a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolveMustBeOwnerTarget_TrimsTrailingPunctuation()
        {
            const string sql = "COMMENT ON TABLE public.a_test IS 'test'";

            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                sql,
                sql,
                "42501",
                "must be owner of table a_test.;",
                0,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(sql.IndexOf( "public.a_test", StringComparison.Ordinal), position);
            Assert.AreEqual("public.a_test", target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_Table_ReturnsQualifiedTable()
        {
            const string sql = "select * from public.a_test";

            AssertPermissionDeniedTarget(sql, "permission denied for table a_test", "public.a_test");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_QuotedTable_ReturnsQualifiedTable()
        {
            const string sql = "select * from \"public\".\"a_test\"";

            AssertPermissionDeniedTarget(sql, "permission denied for relation a_test", "\"public\".\"a_test\"");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_Schema_ReturnsSchema()
        {
            const string sql = "create table public.a_test(id integer)";

            AssertPermissionDeniedTarget(sql, "permission denied for schema public", "public");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_Sequence_ReturnsQualifiedSequence()
        {
            const string sql = "select nextval('public.order_seq')";

            AssertPermissionDeniedTarget(sql, "permission denied for sequence order_seq", "public.order_seq");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_MaterializedView_ReturnsQualifiedView()
        {
            const string sql = "select * from \"public\".\"mv_test\"";

            AssertPermissionDeniedTarget(sql, "permission denied for materialized view mv_test", "\"public\".\"mv_test\"");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_WithWrongErrorCode_ReturnsFalse()
        {
            const string sql = "select * from public.a_test";

            var success = PostgreSqlErrorTargetResolver.TryResolvePermissionDeniedTarget
            (
                sql,
                sql,
                "42P01",
                "permission denied for table a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_WithUnsupportedObjectKind_ReturnsFalse()
        {
            const string sql = "select * from public.a_test";

            var success = PostgreSqlErrorTargetResolver.TryResolvePermissionDeniedTarget
            (
                sql,
                sql,
                "42501",
                "permission denied for unknown-kind a_test",
                0,
                out int position,
                out string target
            );

            Assert.IsFalse(success);
            Assert.AreEqual(0, position);
            Assert.AreEqual(string.Empty, target);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryResolvePermissionDeniedTarget_WithMultipleStatements_UsesExecutedStatementRange()
        {
            const string firstSql = "select * from public.a_test;";
            const string secondSql = "select * from \"public\".\"a_test\"";
            var sql = firstSql + "\r\n" + secondSql;
            var queryStart = sql.IndexOf(secondSql, StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolvePermissionDeniedTarget
            (
                sql,
                secondSql,
                "42501",
                "permission denied for table a_test",
                queryStart,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(sql.IndexOf("\"public\".\"a_test\"", queryStart, StringComparison.Ordinal), position);
            Assert.AreEqual("\"public\".\"a_test\"", target);
        }

        private static void AssertDoubleQuotedTarget(string sql, string errorCode, int reportedPosition, string targetWord, string expectedTarget)
        {
            var expectedPosition = sql.IndexOf(expectedTarget, StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveDoubleQuotedIdentifierTarget
            (
                sql,
                errorCode,
                reportedPosition,
                0,
                targetWord,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual(expectedTarget, target);
        }

        private static void AssertMustBeOwnerTarget(string sql, string errorMessage, string expectedTarget)
        {
            var expectedPosition = sql.IndexOf(expectedTarget, StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolveMustBeOwnerTarget
            (
                sql,
                sql,
                "42501",
                errorMessage,
                0,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual(expectedTarget, target);
        }

        private static void AssertPermissionDeniedTarget(string sql, string errorMessage, string expectedTarget)
        {
            var expectedPosition = sql.IndexOf(expectedTarget, StringComparison.Ordinal);

            var success = PostgreSqlErrorTargetResolver.TryResolvePermissionDeniedTarget
            (
                sql,
                sql,
                "42501",
                errorMessage,
                0,
                out int position,
                out string target
            );

            Assert.IsTrue(success);
            Assert.AreEqual(expectedPosition, position);
            Assert.AreEqual(expectedTarget, target);
        }
    }
}
