using JasonQuery.Core.Database.Diagnostics.Common;
using JasonQuery.Core.Database.Diagnostics.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.Diagnostics.Oracle
{
    [TestClass]
    public sealed class OracleErrorPositionRegressionTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_01400_RealHistory_SelectsInsertColumn()
        {
            const string sql = "INSERT INTO ABC\r\n" +
                               "       (T1_VARCHAR2_PK, T2_NUMBER25_5, T3_VARCHAR2_NEW, T4_DATE, T5_FLOAT, T6_TIMESTAMP)\r\n" +
                               "VALUES ('', NULL, '', NULL, NULL, NULL)";

            AssertHistoryTarget
            (
                sql,
                "ORA-01400: 無法將空值插入 (\"SYS\".\"ABC\".\"T1_VARCHAR2_PK\")",
                "T1_VARCHAR2_PK",
                sql.IndexOf("VALUES", StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        [DataRow("SELECT * FROM A_TEST WHERE ID = 1\r\n FOR UPDATE;", "A_TEST")]
        [DataRow("select * from sysabc0722 where 1=1", "sysabc0722")]
        [DataRow("select * from \"abc0722\" where 1=1", "\"abc0722\"")]
        public void Resolve_00942_RealHistory_SelectsMissingTable(string sql, string expectedTarget)
        {
            AssertHistoryTarget
            (
                sql,
                "ORA-00942: 表格或視觀表不存在",
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        [DataRow("select * from AABBCC c where c.\"column44\" = ''",
                 "ORA-00904: \"C\".\"column44\": 無效的 ID",
                 "c.\"column44\"")]
        [DataRow("select * from AABBCC c where \"d\".\"column4\" = ''",
                 "ORA-00904: \"d\".\"column4\": 無效的 ID",
                 "\"d\".\"column4\"")]
        [DataRow("select * from abc0722 aa where \"aa\".AGE = '15'",
                 "ORA-00904: \"aa\".\"AGE\": 無效的 ID",
                 "\"aa\".AGE")]
        [DataRow("select * from abc0722 aa where \"aa.AGE5\" = 15",
                 "ORA-00904: \"aa.AGE5\": 無效的 ID",
                 "\"aa.AGE5\"")]
        [DataRow("select * from abc0722 aa where \"aa\".\"AGE5\" = 15",
                 "ORA-00904: \"aa\".\"AGE5\": 無效的 ID",
                 "\"aa\".\"AGE5\"")]
        [DataRow("select * from abc0722 aa where aa.\"AGE5\" = 15",
                 "ORA-00904: \"AA\".\"AGE5\": 無效的 ID",
                 "aa.\"AGE5\"")]
        public void Resolve_00904_RealHistory_SelectsActualSourceForm(string sql, string errorMessage, string expectedTarget)
        {
            AssertHistoryTarget
            (
                sql,
                errorMessage,
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00904_RealHistory_SelectsAliasBeforeAsterisk()
        {
            const string sql = "select c.* from abc";
            const string expectedTarget = "c";
            const int expectedPosition = 7;

            var result = Resolve
            (
                sql,
                "ORA-00904: \"C\": 無效的 ID",
                expectedPosition
            );

            Assert.IsTrue(result.Found);
            Assert.AreEqual(expectedPosition, result.Position);
            Assert.AreEqual(expectedTarget, result.TargetText);
            Assert.AreEqual(expectedTarget.Length, result.Length);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        [DataRow("select * from AABBCC c where c.C5_TIMESTAMP3 = '2024/05/25 15:09:20.112233'",
                 "ORA-01843: 不是有效的月份",
                 "'2024/05/25 15:09:20.112233'")]
        [DataRow("select * from AABBCC c where c.C5_TIMESTAMP3 = TO_DATE('35/05/2024', 'DD/MM/YY')",
                 "ORA-01847: 當月天次必須介於 1 到當月的最後一天之間",
                 "'35/05/2024'")]
        [DataRow("select * from abc0722 aa where aa.AGE = 'a'",
                 "ORA-01722: 無效的數字",
                 "'a'")]
        public void Resolve_ConversionErrorRealHistory_SelectsInvalidLiteral(string sql, string errorMessage, string expectedTarget)
        {
            AssertHistoryTarget
            (
                sql,
                errorMessage,
                expectedTarget,
                sql.IndexOf(expectedTarget, StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00936_RealHistory_PointsAfterWhere()
        {
            const string sql = "select * from \"abc0722\" where ";

            var result = Resolve
            (
                sql,
                "ORA-00936: 遺漏表示式",
                sql.Length - 1
            );

            Assert.IsTrue(result.Found);
            Assert.AreEqual(sql.Length - 1, result.Position);
            Assert.AreEqual(" ", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00904_WithParameterRealHistory_SelectsOriginalColumn()
        {
            const string originalSql = "select c.* from AABBCC c\r\n" +
                                       " where COLUMN1 = :col1 AND COLUMN22 = '15'";
            const string executedSql = "select c.* from AABBCC c\r\n" +
                                       " where COLUMN1 = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaa' AND COLUMN22 = '15'";

            var parameterPosition = originalSql.IndexOf(":col1", StringComparison.Ordinal);
            var reportedPosition = executedSql.IndexOf("COLUMN22", StringComparison.Ordinal);

            var result = OracleErrorPositionResolver.Resolve
            (
                new OracleErrorPositionRequest
                {
                    EditorSql = originalSql,
                    OriginalExecutedSql = originalSql,
                    ExecutedSql = executedSql,
                    ErrorMessage = "ORA-00904: \"COLUMN22\": 無效的 ID",
                    ReportedPosition = reportedPosition,
                    PreferredExecutionStart = 0,
                    ParameterPositionMapping = $":col1|{parameterPosition}|'aaaaaaaaaaaaaaaaaaaaaaaaaaaaa'",
                    ParameterStartPosition = 0
                }
            );

            Assert.AreEqual
            (
                originalSql.IndexOf("COLUMN22", StringComparison.Ordinal),
                result.Position
            );

            Assert.AreEqual("COLUMN22", result.TargetText);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00933_RealHistory_SelectsAsKeyword()
        {
            const string sql = "select c.* from AABBCC AS c";

            AssertHistoryTarget
            (
                sql,
                "ORA-00933: SQL 命令的結束有問題",
                "AS",
                sql.Length - 1
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00923_ReportedFirstLinePosition_IsCorrectedToMalformedSecondLineToken()
        {
            const string sql = "SELECT a1.C5_TIMESTAMP3, a1.C7_TIMESTAMP1, a1.C8_TIMESTAMP0,\r\n" +
                               "       a[15.C9_TIMESTAMP3_WITHTIMEZONE, a1.C13_TIMESTAMP9_WITHLOCAL,\r\n" +
                               "       a1.C14_LONG_14, a1.C15_DATE\r\n" +
                               "  FROM aabbcc a1\r\n" +
                               " WHERE column3 = 'sss' OR column3 = 'cc1231'";
            const string target = "a[15.C9_TIMESTAMP3_WITHTIMEZONE";

            AssertHistoryTarget
            (
                sql,
                "ORA-00923: 在應出現的位置找不到 FROM 關鍵字",
                target,
                sql.IndexOf("C8_TIMESTAMP0", StringComparison.Ordinal)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Resolve_00972_ReportedFirstLinePosition_IsCorrectedToMalformedSecondLineToken()
        {
            const string sql = "SELECT a1.C5_TIMESTAMP3, a1.C7_TIMESTAMP1, a1.C8_TIMESTAMP0,\r\n" +
                               "       a\"15.C9_TIMESTAMP3_WITHTIMEZONE, a1.C13_TIMESTAMP9_WITHLOCAL,\r\n" +
                               "       a1.C14_LONG_14, a1.C15_DATE\r\n" +
                               "  FROM aabbcc a1\r\n" +
                               " WHERE column3 = 'sss' OR column3 = 'cc1231'";
            const string target = "a\"15.C9_TIMESTAMP3_WITHTIMEZONE";

            AssertHistoryTarget
            (
                sql,
                "ORA-00972: ID 太長",
                target,
                sql.IndexOf("C8_TIMESTAMP0", StringComparison.Ordinal)
            );
        }

        private static void AssertHistoryTarget(string sql, string errorMessage, string expectedTarget, int reportedPosition)
        {
            var result = Resolve(sql, errorMessage, reportedPosition);

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