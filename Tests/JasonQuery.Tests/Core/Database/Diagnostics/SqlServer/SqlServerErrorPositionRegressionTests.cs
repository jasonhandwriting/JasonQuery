using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.SqlServer
{
    [TestClass]
    public sealed class SqlServerErrorPositionRegressionTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        [DataRow("select * from dbo.CustomerInfo22", "dbo.CustomerInfo22")]
        [DataRow("select * from [dbo].[CustomerInfo22]", "[dbo].[CustomerInfo22]")]
        [DataRow("select * from [dbo].CustomerInfo22", "[dbo].CustomerInfo22")]
        [DataRow("select * from dbo.[CustomerInfo22]", "dbo.[CustomerInfo22]")]
        [DataRow("select * from [dbo.CustomerInfo22]", "[dbo.CustomerInfo22]")]
        public void Resolve_208_RealHistory_SelectsActualObjectSource(string sql, string expectedTarget)
        {
            var result = Resolve
            (
                sql,
                "訊息 208, 層級 16, 狀態 1, 列 1\r\nInvalid object name 'dbo.CustomerInfo22'."
            );

            AssertTarget(sql, result, expectedTarget);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Resolve_156_RealHistory_SelectsAndKeywordAndBuildsSquiggleTarget()
        {
            const string sql = "SELECT '1' AS \" \", c.Name AS Column_Name,\r\n"
                               + "       c.Column_ID,\r\n"
                               + "       TYPE_NAME(User_Type_ID) AS TypeName,\r\n"
                               + "       TYPE_NAME(User_Type_ID) AS DataTypeName,\r\n"
                               + "       TYPE_NAME(User_Type_ID) AS DataType,\r\n"
                               + "       c.Max_Length AS ColumnSize,\r\n"
                               + "       c.Scale AS NumericScale,\r\n"
                               + "       c.Precision AS NumericPrecision\r\n"
                               + "  FROM MyDB.sys.Columns c\r\n"
                               + "       JOIN MyDB.sys.Views v ON v.Object_ID = c.Object_ID\r\n"
                               + "       JOIN MyDB.sys.Schemas s ON v.Schema_ID = s.Schema_ID\r\n"
                               + " WHERE c.Object_ID = \r\n"
                               + "   AND s.Name = 'calc'\r\n"
                               + " ORDER BY c.Column_ID";

            var line = 13;
            var lineText = "   AND s.Name = 'calc'";

            var result = Resolve
            (
                sql,
                "訊息 156, 層級 15, 狀態 1, 列 13\r\nIncorrect syntax near the keyword 'AND'.",
                Secondary(line, "AND", lineText.IndexOf("AND", StringComparison.Ordinal))
            );

            AssertTarget(sql, result, "AND");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Resolve_207_RealHistory_WithParameterMapping_SelectsQuotedIdentifierText()
        {
            const string originalSql = "select * from tS_BoardRequest as u\r\n"
                                       + " where BoardRequestID = :id2 and CreatePerson = \"'aaa'\"";
            const string executedSql = "select * from tS_BoardRequest as u\r\n"
                                       + " where BoardRequestID = 'aaa' and CreatePerson = \"'aaa'\"";

            var parameterPosition = originalSql.IndexOf(":id2", StringComparison.Ordinal);
            var targetPosition = executedSql.LastIndexOf("aaa", StringComparison.Ordinal);

            var result = SqlServerErrorPositionResolver.Resolve
            (
                new SqlServerErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "Invalid column name ''aaa''.",
                    SecondaryErrorMessage = Secondary(2, "'aaa'", targetPosition - executedSql.IndexOf("where", StringComparison.Ordinal)),
                    ParameterPositionMapping = $":id2|{parameterPosition}|'aaa'",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual
            (
                originalSql.LastIndexOf("aaa", StringComparison.Ordinal),
                result.PositionResult.Position
            );

            Assert.AreEqual("aaa", result.PositionResult.TargetText);
            Assert.IsTrue(result.PositionResult.ShouldSetSquiggle);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Resolve_207_RealHistory_SelectsCreatePerson2()
        {
            const string sql = "select * from tS_BoardRequest as u\r\n"
                               + "where BoardRequestID = :id2 and CreatePerson2 = 'aaa'";

            var result = Resolve
            (
                sql,
                "訊息 207, 層級 16, 狀態 1, 列 2\r\nInvalid column name 'CreatePerson2'.",
                Secondary(2, "CreatePerson2", 32)
            );

            AssertTarget(sql, result, "CreatePerson2");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Resolve_207_MultipleRealHistoryErrors_SelectsLastAaaTarget()
        {
            const string sql = "select * from tS_BoardRequest as u\r\n"
                               + " where BoardRequestID = :id2 and CreatePerson2 = \"aaa\"";

            var lineText = " where BoardRequestID = :id2 and CreatePerson2 = \"aaa\"";
            var secondary = Secondary(2, "CreatePerson2", lineText.IndexOf("CreatePerson2", StringComparison.Ordinal))
                            + Secondary(2, "aaa", lineText.IndexOf("aaa", StringComparison.Ordinal));

            var result = Resolve
            (
                sql,
                "訊息 207, 層級 16, 狀態 1, 列 2\r\nInvalid column name 'CreatePerson2'.\r\n"
                + "訊息 207, 層級 16, 狀態 1, 列 2\r\nInvalid column name 'aaa'.",
                secondary
            );

            AssertTarget(sql, result, "aaa");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Resolve_207_QualifiedTableRealHistory_SelectsAaaTarget()
        {
            const string sql = "select * from [Mydb].[dbo].[tS_BoardRequest] as u\r\n"
                               + " where BoardRequestID = :id2 and CreatePerson = \"aaa\"";

            var lineText = " where BoardRequestID = :id2 and CreatePerson = \"aaa\"";

            var result = Resolve
            (
                sql,
                "訊息 207, 層級 16, 狀態 1, 列 2\r\nInvalid column name 'aaa'.",
                Secondary(2, "aaa", lineText.IndexOf("aaa", StringComparison.Ordinal))
            );

            AssertTarget(sql, result, "aaa");
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