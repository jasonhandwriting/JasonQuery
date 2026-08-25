using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.SqlServer;
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
}
