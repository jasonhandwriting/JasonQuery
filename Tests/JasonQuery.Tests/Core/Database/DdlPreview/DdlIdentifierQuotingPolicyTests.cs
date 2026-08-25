using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class DdlIdentifierQuotingPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "abc", "\"abc\"")]
        [DataRow(DataSourceType.Oracle, "Mixed Schema", "\"Mixed Schema\"")]
        [DataRow(DataSourceType.Oracle, "A\"B", "\"A\"\"B\"")]
        [DataRow(DataSourceType.Oracle, "\"A\"\"B\"", "\"A\"\"B\"")]
        [DataRow(DataSourceType.PostgreSql, "ABC", "\"ABC\"")]
        [DataRow(DataSourceType.PostgreSql, "Mixed Schema", "\"Mixed Schema\"")]
        [DataRow(DataSourceType.PostgreSql, "A\"B", "\"A\"\"B\"")]
        [DataRow(DataSourceType.PostgreSql, "\"A\"\"B\"", "\"A\"\"B\"")]
        [DataRow(DataSourceType.SqlServer, "Column1", "[Column1]")]
        [DataRow(DataSourceType.SqlServer, "Order Detail", "[Order Detail]")]
        [DataRow(DataSourceType.SqlServer, "A]B", "[A]]B]")]
        [DataRow(DataSourceType.SqlServer, "[A]]B]", "[A]]B]")]
        [DataRow(DataSourceType.MySql, "Column1", "`Column1`")]
        [DataRow(DataSourceType.MySql, "Order Detail", "`Order Detail`")]
        [DataRow(DataSourceType.MySql, "A`B", "`A``B`")]
        [DataRow(DataSourceType.MySql, "`A``B`", "`A``B`")]
        public void QuoteExistingIdentifier_PreservesMetadataValue(DataSourceType dataSourceType, string identifier, string expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "abc", "\"ABC\"")]
        [DataRow(DataSourceType.Oracle, "ABC", "\"ABC\"")]
        [DataRow(DataSourceType.Oracle, "\"abc\"", "\"abc\"")]
        [DataRow(DataSourceType.Oracle, "\"Mixed Name\"", "\"Mixed Name\"")]
        [DataRow(DataSourceType.PostgreSql, "ABC", "\"abc\"")]
        [DataRow(DataSourceType.PostgreSql, "column_1", "\"column_1\"")]
        [DataRow(DataSourceType.PostgreSql, "\"ABC\"", "\"ABC\"")]
        [DataRow(DataSourceType.PostgreSql, "\"Mixed Name\"", "\"Mixed Name\"")]
        [DataRow(DataSourceType.SqlServer, "Column1", "[Column1]")]
        [DataRow(DataSourceType.SqlServer, "[Mixed Name]", "[Mixed Name]")]
        [DataRow(DataSourceType.SqlServer, "[A]]B]", "[A]]B]")]
        [DataRow(DataSourceType.MySql, "Column1", "`Column1`")]
        [DataRow(DataSourceType.MySql, "`Mixed Name`", "`Mixed Name`")]
        [DataRow(DataSourceType.MySql, "`A``B`", "`A``B`")]
        public void QuoteNewIdentifier_NormalizesUnquotedAndPreservesDelimited(DataSourceType dataSourceType, string identifier, string expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.QuoteNewIdentifier(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.None, "column1")]
        [DataRow(DataSourceType.Oracle, null)]
        [DataRow(DataSourceType.Oracle, "")]
        [DataRow(DataSourceType.Oracle, "Column Name")]
        [DataRow(DataSourceType.Oracle, "schema.column")]
        [DataRow(DataSourceType.Oracle, "\"unclosed")]
        [DataRow(DataSourceType.Oracle, "[wrong]")]
        [DataRow(DataSourceType.PostgreSql, "column;drop")]
        [DataRow(DataSourceType.PostgreSql, "column -- comment")]
        [DataRow(DataSourceType.PostgreSql, "\"A\"B\"")]
        [DataRow(DataSourceType.SqlServer, "Order Detail")]
        [DataRow(DataSourceType.SqlServer, "[unclosed")]
        [DataRow(DataSourceType.SqlServer, "\"wrong\"")]
        [DataRow(DataSourceType.MySql, "a/*x*/")]
        [DataRow(DataSourceType.MySql, "`unclosed")]
        [DataRow(DataSourceType.MySql, "[wrong]")]
        [DataRow(DataSourceType.MySql, "a\r\nb")]
        public void QuoteNewIdentifier_WithUnsafeIdentifier_ReturnsEmpty(DataSourceType dataSourceType, string identifier)
        {
            Assert.AreEqual(string.Empty, DdlIdentifierQuotingPolicy.QuoteNewIdentifier(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MixedSchema", "MixedTable", null,
                 "\"MixedSchema\".\"MixedTable\"")]
        [DataRow(DataSourceType.PostgreSql, "Public", "MixedTable", null,
                 "\"Public\".\"MixedTable\"")]
        [DataRow(DataSourceType.SqlServer, "MyDB", "dbo", "Order Detail",
                 "[MyDB].[dbo].[Order Detail]")]
        [DataRow(DataSourceType.MySql, "Sakila", "City", null,
                 "`Sakila`.`City`")]
        public void QuoteExistingQualifiedName_QuotesEachPartIndependently(DataSourceType dataSourceType, string first, string second, string third, string expected)
        {
            var actual = third == null
                ? DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(dataSourceType, first, second)
                : DdlIdentifierQuotingPolicy.QuoteExistingQualifiedName(dataSourceType, first, second, third);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "abc", "abc")]
        [DataRow(DataSourceType.Oracle, "\"Mixed Case\"", "Mixed Case")]
        [DataRow(DataSourceType.PostgreSql, "ABC", "ABC")]
        [DataRow(DataSourceType.PostgreSql, "\"Mixed Case\"", "Mixed Case")]
        [DataRow(DataSourceType.SqlServer, "[A]]B]", "A]B")]
        [DataRow(DataSourceType.MySql, "`A``B`", "A`B")]
        public void GetExistingIdentifierValue_PreservesDatabaseMetadata(DataSourceType dataSourceType, string identifier, string expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.GetExistingIdentifierValue(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "abc", "ABC")]
        [DataRow(DataSourceType.Oracle, "\"abc\"", "abc")]
        [DataRow(DataSourceType.PostgreSql, "ABC", "abc")]
        [DataRow(DataSourceType.PostgreSql, "\"ABC\"", "ABC")]
        [DataRow(DataSourceType.SqlServer, "[A]]B]", "A]B")]
        [DataRow(DataSourceType.MySql, "`A``B`", "A`B")]
        public void GetNewIdentifierValue_AppliesNewNameSemantics(DataSourceType dataSourceType, string identifier, string expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.GetNewIdentifierValue(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "ABC", "abc", true)]
        [DataRow(DataSourceType.Oracle, "abc", "abc", false)]
        [DataRow(DataSourceType.Oracle, "abc", "\"abc\"", true)]
        [DataRow(DataSourceType.PostgreSql, "abc", "ABC", true)]
        [DataRow(DataSourceType.PostgreSql, "ABC", "ABC", false)]
        [DataRow(DataSourceType.PostgreSql, "ABC", "\"ABC\"", true)]
        [DataRow(DataSourceType.SqlServer, "Column1", "[Column1]", true)]
        [DataRow(DataSourceType.MySql, "Column1", "`Column1`", true)]
        public void AreExistingAndNewEquivalent_UsesCorrectSourceSemantics(DataSourceType dataSourceType, string existingIdentifier, string newIdentifier, bool expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.AreExistingAndNewEquivalent(dataSourceType, existingIdentifier, newIdentifier));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "A;B", "\"A;B\"")]
        [DataRow(DataSourceType.PostgreSql, "A--B", "\"A--B\"")]
        [DataRow(DataSourceType.SqlServer, "A]B", "[A]]B]")]
        [DataRow(DataSourceType.MySql, "A`B", "`A``B`")]
        public void QuoteExistingIdentifier_WithSpecialMetadataCharacters_RemainsSingleIdentifier(DataSourceType dataSourceType, string identifier, string expected)
        {
            Assert.AreEqual(expected, DdlIdentifierQuotingPolicy.QuoteExistingIdentifier(dataSourceType, identifier));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        public void TryQuoteNewIdentifier_WhenInvalid_ClearsOutput()
        {
            var success = DdlIdentifierQuotingPolicy.TryQuoteNewIdentifier(DataSourceType.PostgreSql, "a;drop", out string quotedIdentifier);

            Assert.IsFalse(success);
            Assert.AreEqual(string.Empty, quotedIdentifier);
        }
    }
}
