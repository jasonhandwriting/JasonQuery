using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //C3 feedback: keep the pure policy's original status contract; enrich only the Execute-time receipt.
    [TestClass]
    [TestCategory("F7U3C3")]
    public sealed class PostgreSqlIdentifierTruncationMessageGoldenTests
    {
        private const string TruncateStatus = "F7U3C_PG_IDENTIFIER_WOULD_TRUNCATE";

        [TestMethod]
        public void M901_Ascii64ReportsMeasuredBytesAndLiveLimit()
        {
            var status = NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails(new string('a', 64), 63);

            Assert.AreEqual(TruncateStatus + "|64|63", status);
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.TryGetPostgreSqlIdentifierTruncationDetails(status, out var actual, out var limit));
            Assert.AreEqual(64, actual);
            Assert.AreEqual(63, limit);
        }

        [TestMethod]
        public void M902_ChineseAndMixedNamesReportUtf8Bytes()
        {
            Assert.AreEqual(TruncateStatus + "|66|63",
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails(new string('中', 22), 63));

            Assert.AreEqual(TruncateStatus + "|64|63",
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails(new string('a', 61) + "中", 63));
        }

        [TestMethod]
        public void M903_QuotedIdentifiersExcludeDelimitersAndEscapeSyntax()
        {
            Assert.AreEqual(TruncateStatus + "|3|2",
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails("\"a\"\"b\"", 2));
        }

        [TestMethod]
        public void M904_OtherFailuresNeverMasqueradeAsTruncation()
        {
            Assert.AreEqual(string.Empty,
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails(new string('a', 63), 63));

            Assert.AreEqual("F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails("new_column", 0));

            Assert.AreEqual("F7U3C_PG_IDENTIFIER_INVALID",
                NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails("a;b", 63));
        }

        [TestMethod]
        public void M905_RejectMalformedOrUnmeasuredReceipts()
        {
            var bad = new[]
            {
                (string)null, string.Empty, TruncateStatus, TruncateStatus + "|64", TruncateStatus + "|64|63|0",
                TruncateStatus + "|63|63", TruncateStatus + "|0|63", TruncateStatus + "|abc|63",
                TruncateStatus + "|64|0", "F7U3C_PG_IDENTIFIER_LIMIT_UNVERIFIED|64|63"
            };

            foreach (var status in bad)
            {
                Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.TryGetPostgreSqlIdentifierTruncationDetails(status, out _, out _),
                    "Unexpected accepted status: " + (status ?? "<null>"));
            }
        }

        [TestMethod]
        public void M906_ServerSpecificNonDefaultLimitIsRetained()
        {
            var status = NonOracleSchemaExplorerDdlExecuteGate.ValidatePostgreSqlNewIdentifierLengthWithDetails(new string('z', 128), 127);

            Assert.AreEqual(TruncateStatus + "|128|127", status);
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.TryGetPostgreSqlIdentifierTruncationDetails(status, out var actual, out var limit));
            Assert.AreEqual(128, actual);
            Assert.AreEqual(127, limit);
        }
    }
}
