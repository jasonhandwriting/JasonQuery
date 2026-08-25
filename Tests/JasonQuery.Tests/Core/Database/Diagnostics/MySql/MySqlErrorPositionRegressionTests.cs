using JasonQuery.Core.Database.Diagnostics.MySql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.MySql
{
    [TestClass]
    public sealed class MySqlErrorPositionRegressionTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        [DataRow("'a'")]
        [DataRow("'aaaaaaaaaaaaaaaaaaaaaaaaaaa'")]
        public void Resolve_42S22_RealHistory_ParameterLengthDoesNotMoveLastUpdate22(string parameterValue)
        {
            const string originalSql = "select * from film_actor\r\n"
                                       + " where actor_id = :id and last_update22 = \"ssss\"";

            var executedSql = originalSql.Replace(":id", parameterValue);
            var parameterPosition = originalSql.IndexOf(":id", StringComparison.Ordinal);

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorCode = "42S22",
                    ErrorMessage = "Unknown column 'last_update22' in 'where clause'",
                    ParameterPositionMapping = $":id|{parameterPosition}|{parameterValue}",
                    ParameterStartPosition = 0
                }
            );

            AssertTarget(originalSql, result, "last_update22");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void Resolve_42000_RealHistory_MalformedDashCommentDoesNotSelectSelectKeyword()
        {
            const string sql ="SELECT r.* --, r.last_update\r\n"
                              + "  FROM `sakila`.`film_actor` r";

            var result = Resolve
            (
                sql,
                "42000",
                "You have an error in your SQL syntax at line 1"
            );

            AssertTarget(sql, result, "--,");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void Resolve_42S21_RealHistory_WrappedQuerySelectsExplicitDuplicateColumn()
        {
            const string originalSql = "SELECT r.*, r.last_update\r\n"
                                       + "  FROM `sakila`.`film_actor` r";

            var executedSql = $"SELECT * FROM ({originalSql}) JQ_SPACE_SUBQ";

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorCode = "42S21",
                    ErrorMessage = "Duplicate column name 'last_update'"
                }
            );

            AssertTarget(originalSql, result, "last_update");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        [DataRow("SELECT r.*, r.last_update\r\n    FROM `sakila.film_actor` r", "Table 'sakila.sakila.film_actor' doesn't exist", "`sakila.film_actor`")]
        [DataRow("SELECT r.*, r.last_update\r\n    FROM `sakila`.film_actor2 r", "Table 'sakila.film_actor2' doesn't exist", "`sakila`.film_actor2")]
        [DataRow("SELECT r.*, r.last_update\r\n    FROM sakila.film_actor2 r", "Table 'sakila.film_actor2' doesn't exist", "sakila.film_actor2")]
        [DataRow("SELECT r.*, r.last_update\r\n    FROM sakila.`film_actor2` r", "Table 'sakila.film_actor2' doesn't exist", "sakila.`film_actor2`")]
        public void Resolve_42S02_RealHistory_SelectsCompleteActualTableSource(string sql, string errorMessage, string expectedTarget)
        {
            var result = Resolve
            (
                sql,
                "42S02",
                errorMessage,
                "sakila"
            );

            AssertTarget(sql, result, expectedTarget);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void Resolve_42000_RealHistory_SecondStatementSelectsWhereInSecondStatement()
        {
            const string firstSql = "select * from time_zone_transition_type where Time_zone_id = '' and Is_DST=1 or ";
            const string secondSql = "SELECT u.Host FROM `mysql`.`user` u\r\n"
                                     + " where ";

            var editorSql = firstSql + "\r\n\r\n" + secondSql;
            var secondStart = editorSql.IndexOf(secondSql, StringComparison.Ordinal);

            var result = MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = editorSql,
                    OriginalExecutedSql = secondSql,
                    ExecutedSql = secondSql,
                    ErrorCode = "42000",
                    ErrorMessage = "You have an error in your SQL syntax at line 2",
                    PreferredExecutionStart = secondStart
                }
            );

            Assert.AreEqual
            (
                editorSql.LastIndexOf("where", StringComparison.Ordinal),
                result.PositionResult.Position
            );

            Assert.AreEqual("where", result.PositionResult.TargetText);
        }

        private static MySqlErrorPositionResolution Resolve(string sql, string errorCode, string errorMessage, string databaseName = "")
        {
            return MySqlErrorPositionResolver.Resolve
            (
                new MySqlErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorCode = errorCode,
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
