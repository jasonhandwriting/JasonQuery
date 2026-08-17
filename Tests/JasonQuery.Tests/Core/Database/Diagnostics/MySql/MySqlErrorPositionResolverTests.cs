using JasonQuery.Core.Database.Diagnostics.MySql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.MySql
{
    [TestClass]
    public sealed class MySqlErrorPositionResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_NullRequest_ReturnsNotFound()
        {
            Assert.IsFalse(MySqlErrorPositionResolver.Resolve(null).PositionResult.Found);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_EmptyEditorSql_ReturnsNotFound()
        {
            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = string.Empty,
                    ExecutedSql = "select 1",
                    ErrorMessage = "Unknown column 'c1' in 'field list'"
                }
            );

            Assert.IsFalse(result.PositionResult.Found);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        [DataRow("select c1 from t where c1 = 1", "where clause", 23)]
        [DataRow("select c1 from t order by c1", "order clause", 26)]
        public void Resolve_UnknownColumn_PrefersNamedClause(string sql, string clause, int expectedPosition)
        {
            var result = Resolve
            (
                sql,
                $"Unknown column 'c1' in '{clause}'"
            );

            Assert.AreEqual(expectedPosition, result.PositionResult.Position);
            Assert.AreEqual("c1", result.PositionResult.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_UnknownColumn_DoesNotSelectStringLiteral()
        {
            const string sql = "select 'missing' from t where missing = 1";

            var result = Resolve
            (
                sql,
                "Unknown column 'missing' in 'where clause'"
            );

            Assert.AreEqual(sql.LastIndexOf("missing", StringComparison.Ordinal), result.PositionResult.Position);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_UnknownColumn_DoesNotSelectCommentText()
        {
            const string sql = "select * from t -- missing\r\nwhere missing = 1";

            var result = Resolve
            (
                sql,
                "Unknown column 'missing' in 'where clause'"
            );

            Assert.AreEqual(sql.LastIndexOf("missing", StringComparison.Ordinal), result.PositionResult.Position);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_DuplicateColumn_SelectsLastExplicitOccurrence()
        {
            const string sql = "select last_update, r.last_update from t r";

            var result = Resolve
            (
                sql,
                "Duplicate column name 'last_update'"
            );

            Assert.AreEqual(sql.LastIndexOf("last_update", StringComparison.Ordinal), result.PositionResult.Position);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        [DataRow("SELECT * FROM sakila.film_actor2", "sakila.film_actor2")]
        [DataRow("SELECT * FROM `sakila`.`film_actor2`", "`sakila`.`film_actor2`")]
        [DataRow("SELECT * FROM `sakila`.film_actor2", "`sakila`.film_actor2")]
        [DataRow("SELECT * FROM sakila.`film_actor2`", "sakila.`film_actor2`")]
        [DataRow("SELECT * FROM `sakila.film_actor2`", "`sakila.film_actor2`")]
        public void Resolve_TableNotFound_SelectsActualSourceForm(string sql, string expectedTarget)
        {
            var result = Resolve
            (
                sql,
                "Table 'sakila.film_actor2' doesn't exist",
                databaseName: "sakila"
            );

            AssertTarget(sql, result, expectedTarget);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_ThreePartErrorTarget_RemovesCurrentDatabasePrefixOnlyOnce()
        {
            const string sql = "SELECT * FROM `sakila.film_actor`";

            var result = Resolve
            (
                sql,
                "Table 'sakila.sakila.film_actor' doesn't exist",
                databaseName: "sakila"
            );

            AssertTarget(sql, result, "`sakila.film_actor`");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_SyntaxErrorLine_SelectsFirstTokenOnReportedLine()
        {
            const string sql = "select * from t\r\nwhere ";

            var result = Resolve
            (
                sql,
                "You have an error in your SQL syntax at line 2"
            );

            AssertTarget(sql, result, "where");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_SyntaxErrorMalformedDashComment_SelectsMalformedToken()
        {
            const string sql = "SELECT r.* --, r.last_update\r\nFROM t r";

            var result = Resolve
            (
                sql,
                "You have an error in your SQL syntax at line 1"
            );

            AssertTarget(sql, result, "--,");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_SyntaxNearText_SelectsNearToken()
        {
            const string sql = "select * from t where and c1 = 1";

            var result = Resolve
            (
                sql,
                "You have an error in your SQL syntax near 'and c1 = 1' at line 1"
            );

            AssertTarget(sql, result, "and");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_NoDatabaseSelected_ReturnsStatusDecision()
        {
            var result = Resolve
            (
                "select * from t",
                "No database selected"
            );

            Assert.IsTrue(result.ShouldShowSelectDatabaseFirstMessage);
            Assert.IsTrue(result.PositionResult.UsedReportedPositionFallback);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_UnknownDatabase_SelectsDatabaseName()
        {
            const string sql = "use missing_db";

            var result = Resolve
            (
                sql,
                "Unknown database 'missing_db'"
            );

            AssertTarget(sql, result, "missing_db");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_SelectedSecondStatement_AddsEditorStart()
        {
            const string first = "select * from t1;";
            const string second = "SELECT u.Host FROM `mysql`.`user` u\r\nwhere ";
            var editorSql = first + "\r\n\r\n" + second;

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = editorSql,
                    OriginalExecutedSql = second,
                    ExecutedSql = second,
                    ErrorMessage = "You have an error in your SQL syntax at line 2",
                    PreferredExecutionStart = editorSql.IndexOf(second, StringComparison.Ordinal)
                }
            );

            Assert.AreEqual
            (
                editorSql.LastIndexOf("where", StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_ParameterReplacementLengthDoesNotMoveOriginalColumn()
        {
            const string originalSql = "select * from t where id = :id and missing = 1";
            const string executedSql = "select * from t where id = 'aaaaaaaaaaaaaaaaaaaa' and missing = 1";
            var parameterPosition = originalSql.IndexOf(":id", StringComparison.Ordinal);

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "Unknown column 'missing' in 'where clause'",
                    ParameterPositionMapping = $":id|{parameterPosition}|'aaaaaaaaaaaaaaaaaaaa'",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual
            (
                originalSql.IndexOf("missing", StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Resolve_UnsupportedMessage_UsesReportedPositionFallback()
        {
            const string sql = "select * from t";

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = "Other error",
                    ReportedPosition = 5
                }
            );

            Assert.AreEqual(5, result.PositionResult.Position);
            Assert.IsTrue(result.PositionResult.UsedReportedPositionFallback);
            Assert.IsFalse(result.PositionResult.ShouldSetSquiggle);
        }

        private static MySqlErrorPositionResolution Resolve(string sql, string errorMessage, string databaseName = "")
        {
            return MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = errorMessage,
                    CurrentDatabaseName = databaseName
                }
            );
        }

        private static void AssertTarget(string sql, MySqlErrorPositionResolution result, string expectedTarget)
        {
            Assert.IsTrue(result.PositionResult.Found);

            Assert.AreEqual
            (
                sql.IndexOf(expectedTarget, StringComparison.Ordinal),
                result.PositionResult.Position
            );

            Assert.AreEqual(expectedTarget, result.PositionResult.TargetText);
            Assert.AreEqual(expectedTarget.Length, result.PositionResult.Length);
            Assert.IsTrue(result.PositionResult.ShouldSetSquiggle);
        }
    }
}