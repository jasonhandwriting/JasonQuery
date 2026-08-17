using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class SqlFormatContractsTests
    {
        [TestMethod]
        public void Request_WithoutOptions_UsesStableDefaults()
        {
            var request = new SqlFormatRequest("SELECT 1", DatabaseProviderKind.SqlServer, SqlFormatterEngineKind.MicrosoftScriptDom);

            Assert.AreEqual(4, request.Options.IndentSize);
            Assert.AreEqual(99, request.Options.MaxLineWidth);
            Assert.AreEqual(2, request.Options.LinesBetweenStatements);
            Assert.AreEqual(SqlFormatterKeywordCase.Preserve, request.Options.KeywordCase);
            Assert.AreEqual("    ", request.Options.GetIndentString());
        }

        [TestMethod]
        public void FailedResult_PreservesOriginalSql()
        {
            const string sql = "SELECT * FROM invalid";

            var result = SqlFormatResult.Failed(SqlFormatterEngineKind.Hogimn, sql, "Safety validation failed.");

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            Assert.AreEqual("Safety validation failed.", result.ErrorMessage);
        }
    }
}