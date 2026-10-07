using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.SqlServer;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.SqlServer
{
    [TestClass]
    public sealed class SqlServerErrorPositionResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNullRequest_ReturnsNotFound()
        {
            var result = SqlServerErrorPositionResolver.Resolve(null);

            Assert.IsFalse(result.PositionResult.Found);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithEmptyEditorSql_ReturnsNotFound()
        {
            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    ExecutedSql = "select 1",
                    ErrorMessage = "Invalid column name 'x'."
                }
            );

            Assert.IsFalse(result.PositionResult.Found);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("dbo.CustomerInfo22", "dbo.CustomerInfo22")]
        [DataRow("[dbo].[CustomerInfo22]", "[dbo].[CustomerInfo22]")]
        [DataRow("[dbo].CustomerInfo22", "[dbo].CustomerInfo22")]
        [DataRow("dbo.[CustomerInfo22]", "dbo.[CustomerInfo22]")]
        [DataRow("[dbo.CustomerInfo22]", "[dbo.CustomerInfo22]")]
        public void Resolve_InvalidObjectName_SelectsActualSourceForm(string source, string expectedTarget)
        {
            var sql = $"select * from {source}";

            var result = Resolve
            (
                sql,
                "Invalid object name 'dbo.CustomerInfo22'."
            );

            AssertTarget(sql, result, expectedTarget);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_SecondaryError_UsesLineAndPosition()
        {
            const string sql = "select 1\r\nwhere MissingColumn = 1";

            var secondary = Secondary(2, "MissingColumn", 6);

            var result = Resolve
            (
                sql,
                "Invalid column name 'MissingColumn'.",
                secondary
            );

            AssertTarget(sql, result, "MissingColumn");
            Assert.IsTrue(result.UsedSecondaryErrorMessage);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_MultipleSecondaryErrors_SelectsLastValidTarget()
        {
            const string sql = "select * from t\r\nwhere CreatePerson2 = \"aaa\"";

            var secondary = Secondary(2, "CreatePerson2", 6) + Secondary(2, "aaa", 23);

            var result = Resolve
            (
                sql,
                "Invalid column name 'CreatePerson2'.\r\nInvalid column name 'aaa'.",
                secondary
            );

            AssertTarget(sql, result, "aaa");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_DoubleQuotedIdentifierContainingSingleQuotes_SelectsInnerText()
        {
            const string sql = "select * from t where name = \"'aaa'\"";

            var secondary = Secondary(1, "'aaa'", sql.IndexOf("'aaa'", StringComparison.Ordinal));

            var result = Resolve
            (
                sql,
                "Invalid column name ''aaa''.",
                secondary
            );

            AssertTarget(sql, result, "aaa");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_DoesNotSelectTargetInsideSingleQuotedLiteral()
        {
            const string sql = "select 'MissingColumn' from t where MissingColumn = 1";

            var result = Resolve
            (
                sql,
                "Invalid column name 'MissingColumn'."
            );

            Assert.AreEqual
            (
                sql.LastIndexOf("MissingColumn", StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_ErrorLineWithoutSecondary_SelectsTargetOnSpecifiedLine()
        {
            const string sql = "select MissingColumn\r\nfrom t\r\nwhere MissingColumn = 1";

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorCode = "ErrorLine: 3",
                    ErrorMessage = "Invalid column name 'MissingColumn'."
                }
            );

            Assert.AreEqual
            (
                sql.LastIndexOf("MissingColumn", StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("訊息 156, 層級 15, 狀態 1, 列 3\r\nIncorrect syntax near the keyword 'AND'.")]
        [DataRow("Msg 156, Level 15, State 1, Line 3\r\nIncorrect syntax near the keyword 'AND'.")]
        public void Resolve_ParsesLocalizedErrorLine(string errorMessage)
        {
            const string sql = "select *\r\nfrom t\r\nAND x = 1";

            var result = Resolve(sql, errorMessage);

            AssertTarget(sql, result, "AND");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_SelectedSecondStatement_UsesEditorOffset()
        {
            const string first = "select 1;";
            const string second = "select * from dbo.CustomerInfo22";

            var editorSql = first + "\r\n\r\n" + second;
            var start = editorSql.IndexOf(second, StringComparison.Ordinal);

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = editorSql,
                    OriginalExecutedSql = second,
                    ExecutedSql = second,
                    ErrorMessage = "Invalid object name 'dbo.CustomerInfo22'.",
                    PreferredExecutionStart = start
                }
            );

            Assert.AreEqual
            (
                editorSql.IndexOf("dbo.CustomerInfo22", start, StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithParameterMapping_MapsTargetBackToOriginalSql()
        {
            const string originalSql = "select * from t where id = :id and MissingColumn = 1";
            const string executedSql = "select * from t where id = 'aaaaaaaaaa' and MissingColumn = 1";

            var parameterPosition = originalSql.IndexOf(":id", StringComparison.Ordinal);

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "Invalid column name 'MissingColumn'.",
                    SecondaryErrorMessage = Secondary
                    (
                        1,
                        "MissingColumn",
                        executedSql.IndexOf("MissingColumn", StringComparison.Ordinal)
                    ),
                    ParameterPositionMapping = $":id|{parameterPosition}|'aaaaaaaaaa'",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual
            (
                originalSql.IndexOf("MissingColumn", StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_TargetInsideExpandedParameter_MapsToParameterName()
        {
            const string originalSql = "select * from t where id = :id";
            const string executedSql = "select * from t where id = \"MissingColumn\"";

            var parameterPosition = originalSql.IndexOf(":id", StringComparison.Ordinal);

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "Invalid column name 'MissingColumn'.",
                    SecondaryErrorMessage = Secondary
                    (
                        1,
                        "MissingColumn",
                        executedSql.IndexOf("MissingColumn", StringComparison.Ordinal)
                    ),
                    ParameterPositionMapping = $":id|{parameterPosition}|\"MissingColumn\"",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual(parameterPosition, result.PositionResult.Position);
            Assert.AreEqual(":id", result.PositionResult.TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithNonZeroParameterStart_MapsToAbsoluteEditorPosition()
        {
            const string prefix = "select 1;\r\n\r\n";
            const string originalSql = "select * from t where id = :id and MissingColumn = 1";
            const string executedSql = "select * from t where id = 'aaaaaaaaaa' and MissingColumn = 1";

            var editorSql = prefix + originalSql;
            var start = prefix.Length;
            var parameterRelativePosition = originalSql.IndexOf(":id", StringComparison.Ordinal);

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = editorSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "Invalid column name 'MissingColumn'.",
                    SecondaryErrorMessage = Secondary
                    (
                        1,
                        "MissingColumn",
                        executedSql.IndexOf("MissingColumn", StringComparison.Ordinal)
                    ),
                    PreferredExecutionStart = start,
                    ParameterPositionMapping = $":id|{parameterRelativePosition}|'aaaaaaaaaa'",
                    ParameterStartPosition = start
                }
            );

            Assert.AreEqual
            (
                editorSql.IndexOf("MissingColumn", start, StringComparison.Ordinal),
                result.PositionResult.Position
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_IncorrectPipeSyntax_RequestsConcatenationHint()
        {
            const string sql = "select 'a' | 'b'";

            var result = Resolve
            (
                sql,
                "Incorrect syntax near '|'.",
                Secondary(1, "|", sql.IndexOf('|'))
            );

            Assert.IsTrue(result.ShouldAppendStringConcatenationHint);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Resolve_WithoutTarget_UsesReportedPositionFallback()
        {
            const string sql = "select 1";

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = "Unknown provider error",
                    ReportedPosition = 4,
                    PositionOffset = -1
                }
            );

            Assert.IsTrue(result.PositionResult.Found);
            Assert.AreEqual(3, result.PositionResult.Position);
            Assert.IsTrue(result.PositionResult.UsedReportedPositionFallback);
            Assert.IsFalse(result.PositionResult.ShouldSetSquiggle);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("select MissingColumn from t", "MissingColumn")]
        [DataRow("select 'MissingColumn' from t", "MissingColumn")]
        [DataRow("select 'x''MissingColumn' from t", "MissingColumn")]
        [DataRow("select 'x' as x, MissingColumn from t", "MissingColumn")]
        [DataRow("select \"'MissingColumn'\" from t", "MissingColumn")]
        [DataRow("select [abc'MissingColumn] from t", "MissingColumn")]
        [DataRow("select \"a\"\"'MissingColumn'\" from t", "MissingColumn")]
        [DataRow("select [a]]'MissingColumn] from t", "MissingColumn")]
        [DataRow("select -- 'x'\r\nMissingColumn from t", "MissingColumn")]
        [DataRow("select /* 'x' */ MissingColumn from t", "MissingColumn")]
        [DataRow("select 'abc' /* x */ MissingColumn from t", "MissingColumn")]
        [DataRow("select \"abc\" MissingColumn from t", "MissingColumn")]
        [DataRow("select [abc] MissingColumn from t", "MissingColumn")]
        [DataRow("select '' as x, MissingColumn from t", "MissingColumn")]
        [DataRow("select 'prefix' + 'MissingColumn'", "MissingColumn")]
        [DataRow("select 'a''b' as x, MissingColumn", "MissingColumn")]
        public void IsInsideSingleQuotedString_SharedTokenizer_MatchesLegacy(string sql, string marker)
        {
            var position = sql.IndexOf(marker, StringComparison.Ordinal);

            Assert.IsTrue(position >= 0);

            var shared = SqlServerErrorPositionResolver.IsInsideSingleQuotedStringForParityTest
            (
                sql,
                position
            );

            var legacy = SqlServerErrorPositionResolver.IsInsideSingleQuotedStringLegacyForParityTest
            (
                sql,
                position
            );

            Assert.AreEqual(legacy, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("-- old ' note\r\nselect MissingColumn from t")]
        [DataRow("-- old ' note\nselect MissingColumn from t")]
        [DataRow("-- old ' note\rselect MissingColumn from t")]
        [DataRow("/* old ' note */ select MissingColumn from t")]
        public void IsInsideSingleQuotedString_SharedTokenizer_CorrectsCommentQuoteStatePoisoning(string sql)
        {
            const string target = "MissingColumn";
            var position = sql.IndexOf(target, StringComparison.Ordinal);

            Assert.IsTrue(position >= 0);

            var shared = SqlServerErrorPositionResolver.IsInsideSingleQuotedStringForParityTest
            (
                sql,
                position
            );

            var legacy = SqlServerErrorPositionResolver.IsInsideSingleQuotedStringLegacyForParityTest
            (
                sql,
                position
            );

            Assert.IsFalse(shared);
            Assert.IsTrue(legacy);

            var result = Resolve
            (
                sql,
                "Invalid column name 'MissingColumn'."
            );

            Assert.AreEqual(position, result.PositionResult.Position);
            Assert.AreEqual(target, result.PositionResult.TargetText);
            Assert.AreEqual(target.Length, result.PositionResult.Length);
            Assert.IsTrue(result.PositionResult.ShouldSetSquiggle);
        }

        private static SqlServerErrorPositionResolution Resolve(string sql, string errorMessage, string secondary = "")
        {
            return SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = sql,
                    OriginalExecutedSql = sql,
                    ExecutedSql = sql,
                    ErrorMessage = errorMessage,
                    SecondaryErrorMessage = secondary
                }
            );
        }

        private static string Secondary(int line, string target, int position)
        {
            return $"{line}{MyGlobal.SeparatorPlus4}{target}{MyGlobal.SeparatorPlus4}{position}{MyGlobal.SeparatorPlus3}";
        }

        private static void AssertTarget(string sql, SqlServerErrorPositionResolution result, string expectedTarget)
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

    [TestClass]
    public sealed class SqlServerReaderErrorTargetLexicalPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("MissingColumn", "MissingColumn")]
        [DataRow("x MissingColumn", "MissingColumn")]
        [DataRow("'MissingColumn'", "MissingColumn")]
        [DataRow("'x''MissingColumn'", "MissingColumn")]
        [DataRow("'x' MissingColumn", "MissingColumn")]
        [DataRow("'' MissingColumn", "MissingColumn")]
        [DataRow("'a''b' MissingColumn", "MissingColumn")]
        [DataRow("\"abc\" MissingColumn", "MissingColumn")]
        [DataRow("[abc] MissingColumn", "MissingColumn")]
        [DataRow("/* note */ MissingColumn", "MissingColumn")]
        [DataRow("-- note MissingColumn", "MissingColumn")]
        [DataRow("'unterminated MissingColumn", "MissingColumn")]
        public void ReaderSharedPolicy_MatchesLegacy(string text, string marker)
        {
            var position = text.IndexOf(marker, StringComparison.Ordinal);

            Assert.IsTrue(position >= 0);

            var shared = SqlServerReader.IsInsideSingleQuotedStringForParityTest
            (
                text,
                position
            );

            var legacy = SqlServerReader.IsInsideSingleQuotedStringLegacyForParityTest
            (
                text,
                position
            );

            Assert.AreEqual(legacy, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("/* old ' note */ MissingColumn")]
        [DataRow("\"old'MissingColumn\"")]
        [DataRow("[old'MissingColumn]")]
        [DataRow("\"a\"\"'MissingColumn\"")]
        public void ReaderSharedPolicy_CorrectsLegacyQuoteStatePoisoning(string text)
        {
            const string target = "MissingColumn";
            var position = text.IndexOf(target, StringComparison.Ordinal);

            Assert.IsTrue(position >= 0);

            var shared = SqlServerReader.IsInsideSingleQuotedStringForParityTest
            (
                text,
                position
            );

            var legacy = SqlServerReader.IsInsideSingleQuotedStringLegacyForParityTest
            (
                text,
                position
            );

            Assert.IsFalse(shared);
            Assert.IsTrue(legacy);

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    0,
                    out var foundPosition,
                    out var matchedLength
                )
            );

            Assert.AreEqual(position, foundPosition);
            Assert.AreEqual(target.Length, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_SkipsSingleQuotedLiteralAndFindsLaterOccurrence()
        {
            const string text = "select 'MissingColumn' as x, MissingColumn from t";
            const string target = "MissingColumn";

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(text.LastIndexOf(target, StringComparison.Ordinal), position);
            Assert.AreEqual(target.Length, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_ColumnPrefixErrorPrefersPrefixCandidate()
        {
            const string text = "select a.MissingColumn from dbo.TableA a";
            const string target = "a";
            const string message = "The column prefix 'a' does not match with a table name or alias name used in the query.";

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    message,
                    target,
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(text.IndexOf("a.", StringComparison.Ordinal), position);
            Assert.AreEqual(2, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_StartPositionSkipsEarlierOccurrence()
        {
            const string text = "MissingColumn x MissingColumn";
            const string target = "MissingColumn";
            var startPosition = text.IndexOf("x", StringComparison.Ordinal);

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    startPosition,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(text.LastIndexOf(target, StringComparison.Ordinal), position);
            Assert.AreEqual(target.Length, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_OnlyOccurrenceInsideSingleQuotedLiteral_ReturnsFalse()
        {
            const string text = "select 'MissingColumn'";
            const string target = "MissingColumn";

            Assert.IsFalse
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(-1, position);
            Assert.AreEqual(0, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_PreservesCaseSensitiveMatching()
        {
            const string text = "select MissingColumn";

            Assert.IsFalse
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'missingcolumn'.",
                    "missingcolumn",
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(-1, position);
            Assert.AreEqual(0, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_NormalTargetPreservesMatchedLength()
        {
            const string text = "select MissingColumn from t";
            const string target = "MissingColumn";

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(text.IndexOf(target, StringComparison.Ordinal), position);
            Assert.AreEqual(target.Length, matchedLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void ReaderTryFind_DoubleQuotedIdentifierRemainsEligible()
        {
            const string text = "select \"MissingColumn\" from t";
            const string target = "MissingColumn";

            Assert.IsTrue
            (
                SqlServerReader.TryFindSqlServerErrorTokenPositionForTest
                (
                    text,
                    "Invalid column name 'MissingColumn'.",
                    target,
                    0,
                    out var position,
                    out var matchedLength
                )
            );

            Assert.AreEqual(text.IndexOf(target, StringComparison.Ordinal), position);
            Assert.AreEqual(target.Length, matchedLength);
        }
    }
}
