using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Diagnostics.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.Diagnostics.SqlServer
{
    [TestClass]
    public sealed class SqlServerSecondaryErrorParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("invalid")]
        [DataRow("1")]
        public void Parse_WithMissingOrMalformedValue_ReturnsEmpty(string value)
        {
            Assert.AreEqual(0, SqlServerSecondaryErrorParser.Parse(value).Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Parse_WithSingleItem_ReturnsValues()
        {
            var value = $"2{MyGlobal.SeparatorPlus4}CreatePerson2{MyGlobal.SeparatorPlus4}32{MyGlobal.SeparatorPlus3}";
            var items = SqlServerSecondaryErrorParser.Parse(value);

            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(2, items[0].LineNumber);
            Assert.AreEqual("CreatePerson2", items[0].TargetText);
            Assert.AreEqual(32, items[0].PositionInLine);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Parse_WithMultipleItems_SortsByLineAndPosition()
        {
            var value = $"2{MyGlobal.SeparatorPlus4}aaa{MyGlobal.SeparatorPlus4}50{MyGlobal.SeparatorPlus3}"
                        + $"1{MyGlobal.SeparatorPlus4}dbo.CustomerInfo22{MyGlobal.SeparatorPlus4}14{MyGlobal.SeparatorPlus3}"
                        + $"2{MyGlobal.SeparatorPlus4}CreatePerson2{MyGlobal.SeparatorPlus4}32{MyGlobal.SeparatorPlus3}";

            var items = SqlServerSecondaryErrorParser.Parse(value);

            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("dbo.CustomerInfo22", items[0].TargetText);
            Assert.AreEqual("CreatePerson2", items[1].TargetText);
            Assert.AreEqual("aaa", items[2].TargetText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("0", "name", "1")]
        [DataRow("1", "", "1")]
        [DataRow("1", "name", "-1")]
        [DataRow("x", "name", "1")]
        [DataRow("1", "name", "x")]
        public void Parse_WithInvalidField_ReturnsEmpty(string line, string target, string position)
        {
            var value = $"{line}{MyGlobal.SeparatorPlus4}{target}{MyGlobal.SeparatorPlus4}{position}{MyGlobal.SeparatorPlus3}";

            Assert.AreEqual(0, SqlServerSecondaryErrorParser.Parse(value).Count);
        }
    }
}