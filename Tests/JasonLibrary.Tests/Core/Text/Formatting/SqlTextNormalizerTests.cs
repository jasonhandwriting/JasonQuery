using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlTextNormalizerTests
    {
        [TestMethod]
        public void Normalize_ConvertsAllLineEndingsToCrLf()
        {
            const string sql = "SELECT 1\rSELECT 2\nSELECT 3\r\nSELECT 4";

            var result = SqlTextNormalizer.Normalize(sql);

            Assert.AreEqual("SELECT 1\r\nSELECT 2\r\nSELECT 3\r\nSELECT 4", result);
        }

        [TestMethod]
        public void Normalize_ConvertsEveryTabToFourSpaces()
        {
            const string sql = "\tSELECT\t1";

            var result = SqlTextNormalizer.Normalize(sql);

            Assert.AreEqual("    SELECT    1", result);
        }

        [TestMethod]
        public void Normalize_RemovesOnlyTrailingLineEndings()
        {
            const string sql = "SELECT 1  \n\n";

            var result = SqlTextNormalizer.Normalize(sql);

            Assert.AreEqual("SELECT 1  ", result);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Normalize_NullOrEmpty_ReturnsEmpty(string sql)
        {
            Assert.AreEqual(string.Empty, SqlTextNormalizer.Normalize(sql));
        }
    }
}
