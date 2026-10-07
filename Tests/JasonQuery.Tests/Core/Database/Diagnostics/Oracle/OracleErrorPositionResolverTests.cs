using JasonQuery.Core.Database.Diagnostics.Common;
using JasonQuery.Core.Database.Diagnostics.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.Oracle
{
    [TestClass]
    public sealed class OracleErrorPositionResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithNullRequest_ReturnsNotFound()
        {
            var result = OracleErrorPositionResolver.Resolve(null);

            Assert.IsFalse(result.Found);
            Assert.AreEqual(string.Empty, result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithEmptyEditorSql_ReturnsNotFound()
        {
            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = string.Empty,
                    ExecutedSql = "select 1 from dual",
                    ErrorMessage = "ORA-00942: table or view does not exist"
                }
            );

            Assert.IsFalse(result.Found);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [DataRow("select * from A_TEST", "ORA-00942: table or view does not exist", "A_TEST")]
        [DataRow("select * from \"A_TEST\"", "ORA-00942: table or view does not exist", "\"A_TEST\"")]
        [DataRow("select * from SYS.A_TEST", "ORA-00942: table or view does not exist", "SYS.A_TEST")]
        [DataRow("select * from \"SYS\".A_TEST", "ORA-00942: table or view does not exist", "\"SYS\".A_TEST")]
        [DataRow("select * from SYS.\"A_TEST\"", "ORA-00942: table or view does not exist", "SYS.\"A_TEST\"")]
        [DataRow("select * from \"SYS\".\"A_TEST\"", "ORA-00942: table or view does not exist", "\"SYS\".\"A_TEST\"")]
        public void Resolve_00942_ReturnsObjectAfterFrom(string sql, string errorMessage, string expectedTarget)
        {
            AssertTarget
            (
                sql,
                errorMessage,
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [DataRow("select * from A_TEST c where c.\"column44\" = ''", "ORA-00904: \"C\".\"column44\": invalid identifier", "c.\"column44\"")]
        [DataRow("select * from A_TEST c where \"c\".column44 = ''", "ORA-00904: \"C\".\"COLUMN44\": invalid identifier", "\"c\".column44")]
        [DataRow("select * from A_TEST c where \"c\".\"column44\" = ''", "ORA-00904: \"C\".\"column44\": invalid identifier", "\"c\".\"column44\"")]
        [DataRow("select * from A_TEST c where c.column44 = ''", "ORA-00904: \"C\".\"COLUMN44\": invalid identifier", "c.column44")]
        [DataRow("select * from A_TEST c where \"c.column44\" = ''", "ORA-00904: \"c.column44\": invalid identifier", "\"c.column44\"")]
        public void Resolve_00904_ReturnsMatchingQuotedCombination(string sql, string errorMessage, string expectedTarget)
        {
            AssertTarget
            (
                sql,
                errorMessage,
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_01400_UsesLeafColumnInsteadOfQualifiedMessagePath()
        {
            const string sql = "INSERT INTO ABC (T1_VARCHAR2_PK, T2_NUMBER25_5)\r\n" +
                               "VALUES ('', NULL)";

            var result = Resolve
            (
                sql,
                "ORA-01400: cannot insert NULL into (\"SYS\".\"ABC\".\"T1_VARCHAR2_PK\")",
                sql.IndexOf("VALUES", StringComparison.Ordinal)
            );

            Assert.IsTrue(result.Found);

            Assert.AreEqual
            (
                sql.IndexOf("T1_VARCHAR2_PK", StringComparison.Ordinal),
                result.Position
            );

            Assert.AreEqual("T1_VARCHAR2_PK", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_00904_WithAliasOnly_DoesNotMatchCharacterInsideSelectKeyword()
        {
            const string sql = "select c.* from abc";

            var result = Resolve
            (
                sql,
                "ORA-00904: \"C\": invalid identifier",
                0
            );

            Assert.IsTrue(result.Found);
            Assert.AreEqual(sql.IndexOf("c.*", StringComparison.Ordinal), result.Position);
            Assert.AreEqual("c", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_00933_ReturnsTableAliasAsKeyword()
        {
            const string sql = "select c.* from AABBCC AS c";

            AssertTarget
            (
                sql,
                "ORA-00933: SQL command not properly ended",
                "AS",
                0
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_00923_ReturnsMalformedBracketTokenEvenWhenReportedPositionPointsToPreviousLine()
        {
            const string sql = "SELECT a1.C5_TIMESTAMP3, a1.C7_TIMESTAMP1, a1.C8_TIMESTAMP0,\r\n" +
                               "       a[15.C9_TIMESTAMP3_WITHTIMEZONE, a1.C13_TIMESTAMP9_WITHLOCAL\r\n" +
                               "  FROM aabbcc a1";

            const string expectedTarget = "a[15.C9_TIMESTAMP3_WITHTIMEZONE";

            AssertTarget
            (
                sql,
                "ORA-00923: FROM keyword not found where expected",
                expectedTarget,
                sql.IndexOf("C8_TIMESTAMP0", StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_00972_ReturnsMalformedUnclosedQuotedTokenEvenWhenReportedPositionPointsToPreviousLine()
        {
            const string sql = "SELECT a1.C5_TIMESTAMP3, a1.C7_TIMESTAMP1, a1.C8_TIMESTAMP0,\r\n" +
                               "       a\"15.C9_TIMESTAMP3_WITHTIMEZONE, a1.C13_TIMESTAMP9_WITHLOCAL\r\n" +
                               "  FROM aabbcc a1";

            const string expectedTarget = "a\"15.C9_TIMESTAMP3_WITHTIMEZONE";

            AssertTarget
            (
                sql,
                "ORA-00972: identifier is too long",
                expectedTarget,
                sql.IndexOf("C8_TIMESTAMP0", StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_00936_AtTrailingWhere_ReturnsPointerSpaceWithoutSquiggle()
        {
            const string sql = "select * from \"abc0722\" where ";

            var result = Resolve
            (
                sql,
                "ORA-00936: missing expression",
                sql.Length - 1
            );

            Assert.IsTrue(result.Found);
            Assert.AreEqual(sql.Length - 1, result.Position);
            Assert.AreEqual(" ", result.TargetText);
            Assert.IsFalse(result.ShouldSetSquiggle);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [DataRow("select * from AABBCC where AGE = 'a'", "ORA-01722: invalid number", "'a'")]
        [DataRow("select * from AABBCC where D = '2024/05/25 15:09:20'", "ORA-01843: not a valid month", "'2024/05/25 15:09:20'")]
        [DataRow("select * from AABBCC where D = TO_DATE('35/05/2024', 'DD/MM/YY')", "ORA-01847: day of month must be between 1 and last day of month", "'35/05/2024'")]
        public void Resolve_KnownConversionErrors_ReturnNearestLiteral(string sql, string errorMessage, string expectedTarget)
        {
            AssertTarget
            (
                sql,
                errorMessage,
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithRepeatedIdentifier_SelectsOccurrenceNearestReportedPosition()
        {
            const string sql = "select COLUMN22 from AABBCC\r\n" +
                               " where COLUMN1 = 1 and COLUMN22 = 2";

            var expectedPosition = sql.LastIndexOf
            (
                "COLUMN22",
                StringComparison.Ordinal
            );

            var result = Resolve
            (
                sql,
                "ORA-00904: \"COLUMN22\": invalid identifier",
                expectedPosition
            );

            Assert.AreEqual(expectedPosition, result.Position);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithSelectedSql_UsesPreferredExecutionRange()
        {
            const string firstSql = "select * from FIRST_TABLE;\r\n";
            const string secondSql = "select * from SECOND_TABLE";

            var editorSql = firstSql + secondSql;
            var start = firstSql.Length;

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = editorSql,
                    OriginalExecutedSql = secondSql,
                    ExecutedSql = secondSql,
                    ErrorMessage = "ORA-00942: table or view does not exist",
                    ReportedPosition = start + secondSql.IndexOf("SECOND_TABLE", StringComparison.Ordinal),
                    PreferredExecutionStart = start
                }
            );

            Assert.AreEqual
            (
                editorSql.IndexOf("SECOND_TABLE", StringComparison.Ordinal),
                result.Position
            );

            Assert.AreEqual("SECOND_TABLE", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithParameterMappingAndUnnamedError_MapsBackToOriginalParameter()
        {
            const string originalSql = "select * from AABBCC where C1 = :p";
            const string executedSql = "select * from AABBCC where C1 = 'VALUE'";

            var parameterPosition = originalSql.IndexOf(":p", StringComparison.Ordinal);
            var executedPosition = executedSql.IndexOf("'VALUE'", StringComparison.Ordinal) + 2;

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "ORA-01008: not all variables bound",
                    ReportedPosition = executedPosition,
                    PreferredExecutionStart = 0,
                    ParameterPositionMapping = $":p|{parameterPosition}|'VALUE'",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual(parameterPosition, result.Position);
            Assert.AreEqual(":p", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithUtf8BytePosition_MapsToUtf16EditorPosition()
        {
            const string sql = "select '中文內容', BAD_COLUMN from dual";

            var expectedPosition = sql.IndexOf("BAD_COLUMN", StringComparison.Ordinal);
            var prefix = sql.Substring(0, expectedPosition);
            var reportedBytePosition = System.Text.Encoding.UTF8.GetByteCount(prefix);

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = "ORA-00001: test",
                    ReportedPosition = reportedBytePosition,
                    PreferredExecutionStart = 0
                }
            );

            Assert.IsTrue(result.HadUtf8PositionAdjustment);
            Assert.AreEqual(expectedPosition, result.Position);
            Assert.AreEqual("BAD_COLUMN", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Resolve_WithTwentyNonAsciiCharacters_SetsWarningFlag()
        {
            var prefix = new string('中', 20);
            var sql = $"select '{prefix}', BAD_COLUMN from dual";
            var expectedPosition = sql.IndexOf("BAD_COLUMN", StringComparison.Ordinal);

            var reportedBytePosition = System.Text.Encoding.UTF8.GetByteCount
            (
                sql.Substring(0, expectedPosition)
            );

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = "ORA-00001: test",
                    ReportedPosition = reportedBytePosition,
                    PreferredExecutionStart = 0
                }
            );

            Assert.IsTrue(result.HasManyNonAsciiCharactersBeforePosition);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("", "")]
        [DataRow("   \t\r\n", "")]
        [DataRow("select * from A_TEST", "select * from A_TEST")]
        [DataRow(" \tselect * from A_TEST  ", "select * from A_TEST  ")]
        [DataRow("-- note\r\nselect * from A_TEST", "select * from A_TEST")]
        [DataRow("-- note\nselect * from A_TEST", "select * from A_TEST")]
        [DataRow("-- note\rselect * from A_TEST", "select * from A_TEST")]
        [DataRow("-- note", "")]
        [DataRow("/* note */select * from A_TEST", "select * from A_TEST")]
        [DataRow(" \r\n/* note */ \t select * from A_TEST  ", "select * from A_TEST  ")]
        [DataRow("-- one\r\n/* two */\r\n-- three\r\nselect * from A_TEST", "select * from A_TEST")]
        [DataRow("/*+ INDEX(A IX_A) */ select * from A_TEST", "/*+ INDEX(A IX_A) */ select * from A_TEST")]
        [DataRow("  /*+ INDEX(A IX_A) */ select * from A_TEST", "/*+ INDEX(A IX_A) */ select * from A_TEST")]
        [DataRow("/* unterminated", "")]
        [DataRow("  /*+ INDEX(A IX_A)", "/*+ INDEX(A IX_A)")]
        [DataRow("/* outer /* inner */ SELECT */ select * from A_TEST", "SELECT */ select * from A_TEST")]
        [DataRow("q'[-- not a comment]' from dual", "q'[-- not a comment]' from dual")]
        [DataRow("'/* not a comment */' from dual", "'/* not a comment */' from dual")]
        [DataRow("\"/* not a comment */\" from dual", "\"/* not a comment */\" from dual")]
        [DataRow("/ select * from A_TEST", "/ select * from A_TEST")]
        public void RemoveLeadingOracleSqlTrivia_SharedTokenizer_MatchesLegacy(string sql, string expected)
        {
            var shared = OracleErrorPositionResolver.RemoveLeadingOracleSqlTriviaForParityTest(sql);
            var legacy = OracleErrorPositionResolver.RemoveLeadingOracleSqlTriviaLegacyForParityTest(sql);

            Assert.AreEqual(expected, legacy);
            Assert.AreEqual(legacy, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("-- leading\r\nselect * from MISSING_TABLE")]
        [DataRow("/* leading */\r\nselect * from MISSING_TABLE")]
        [DataRow("-- one\r\n/* two */\r\nselect * from MISSING_TABLE")]
        [DataRow("/*+ INDEX(MISSING_TABLE IX_X) */ select * from MISSING_TABLE")]
        public void Resolve_WithLeadingOracleTrivia_PreservesExecutionRangeBehavior(string sql)
        {
            const string target = "MISSING_TABLE";
            var expectedPosition = sql.LastIndexOf(target, StringComparison.Ordinal);

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = "ORA-00942: table or view does not exist",
                    ReportedPosition = expectedPosition,
                    PreferredExecutionStart = 0
                }
            );

            Assert.IsTrue(result.Found);
            Assert.AreEqual(expectedPosition, result.Position);
            Assert.AreEqual(target, result.TargetText);
            Assert.AreEqual(target.Length, result.Length);
            Assert.IsTrue(result.ShouldSetSquiggle);
        }

        private static void AssertTarget(string sql, string errorMessage, string expectedTarget, int reportedPosition)
        {
            var result = Resolve
            (
                sql,
                errorMessage,
                reportedPosition
            );

            Assert.IsTrue(result.Found);

            Assert.AreEqual
            (
                sql.IndexOf(expectedTarget, StringComparison.Ordinal),
                result.Position
            );

            Assert.AreEqual(expectedTarget, result.TargetText);
            Assert.AreEqual(expectedTarget.Length, result.Length);
        }

        private static SqlErrorResolutionResult Resolve(string sql, string errorMessage, int reportedPosition)
        {
            return OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = errorMessage,
                    ReportedPosition = reportedPosition,
                    PreferredExecutionStart = 0
                }
            );
        }
    }
}
