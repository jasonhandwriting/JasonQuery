using JasonQuery.Core.Database.Diagnostics.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Diagnostics.Oracle
{
    [TestClass]
    public sealed class OracleErrorCodeParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [DataRow("904", "", "00904")]
        [DataRow("00904", "", "00904")]
        [DataRow("ErrorCode: 1843", "", "01843")]
        [DataRow("ORA-01722", "", "01722")]
        [DataRow("", "ORA-00942: table or view does not exist", "00942")]
        [DataRow("", "ORA-01400: cannot insert NULL", "01400")]
        [DataRow("", "ORA-00972: identifier is too long", "00972")]
        [DataRow(null, "ORA-00923: FROM keyword not found", "00923")]
        [DataRow("", "message without code", "")]
        [DataRow(null, null, "")]
        public void Normalize_ReturnsExpectedCode(string errorCode, string errorMessage, string expected)
        {
            Assert.AreEqual
            (
                expected,
                OracleErrorCodeParser.Normalize
                (
                    errorCode,
                    errorMessage
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        [DataRow("904", "00904", true)]
        [DataRow("00904", "904", true)]
        [DataRow("1843", "01843", true)]
        [DataRow("00942", "00904", false)]
        [DataRow("", "00904", false)]
        public void Is_ReturnsExpectedResult(string actual, string expected, bool expectedResult)
        {
            Assert.AreEqual
            (
                expectedResult,
                OracleErrorCodeParser.Is(actual, expected)
            );
        }
    }
}