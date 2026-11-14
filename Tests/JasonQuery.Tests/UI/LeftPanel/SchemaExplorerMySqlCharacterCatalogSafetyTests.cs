using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4C2-C2: standalone pure safety cases. No MySQL connection and no DDL.
    [TestClass]
    [TestCategory("F7U3C4C2C2")]
    public sealed class SchemaExplorerMySqlCharacterCatalogSafetyTests
    {
        private const string Definition = "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'old'";

        private static bool Catalog(string expectedCharset, string expectedCollation, string actualCharset, string actualCollation, int rows)
        {
            return MySqlReader.IsExactCharacterSetCollationCatalogPair
            (
                expectedCharset, expectedCollation, actualCharset, actualCollation, rows
            );
        }

        [TestMethod]
        public void C4C2C2S01_ExactCatalogPairMatchesCaseInsensitively()
        {
            Assert.IsTrue(Catalog("utf8mb4", "utf8mb4_bin", "UTF8MB4", "UTF8MB4_BIN", 1));
        }

        [TestMethod]
        public void C4C2C2S02_ZeroMultipleAndNegativeRowsRemainDenied()
        {
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin", 0));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin", 2));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin", -1));
        }

        [TestMethod]
        public void C4C2C2S03_MissingCatalogFieldsAreRejected()
        {
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", null, "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", null, 1));
            Assert.IsFalse(Catalog("", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "", "utf8mb4", "utf8mb4_bin", 1));
        }

        [TestMethod]
        public void C4C2C2S04_UnsafeCatalogIdentifiersFailClosed()
        {
            Assert.IsFalse(Catalog("utf8mb4' OR 1=1", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin;", "utf8mb4", "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4\n", "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_bin ", 1));
        }

        [TestMethod]
        public void C4C2C2S05_OversizedAndUnicodeNamesCannotMatch()
        {
            var tooLong = "a" + new string('b', 64);

            Assert.IsFalse(Catalog(tooLong, "utf8mb4_bin", tooLong, "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "定序規則", 1));
        }

        [TestMethod]
        public void C4C2C2S06_IndependentCharsetAndCollationMismatchRemainDenied()
        {
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "latin1", "utf8mb4_bin", 1));
            Assert.IsFalse(Catalog("utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_0900_ai_ci", 1));
        }

        [TestMethod]
        public void C4C2C2S07_CatalogBoolAloneCannotBypassCharacterProof()
        {
            var status = MySqlCharacterColumnDefinitionProofPolicy.Validate
            (
                Definition, "varchar(32)", "NO", "xyz", "old",
                "utf8mb4", "utf8mb4_bin", "", "", "", true
            );

            Assert.IsFalse(string.IsNullOrEmpty(status));
        }

        [TestMethod]
        public void C4C2C2S08_CatalogHelperIsNotPublicApiOrReceiptConstructor()
        {
            var method = typeof(MySqlReader).GetMethod
            (
                "IsExactCharacterSetCollationCatalogPair",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method);

            Assert.IsNull(typeof(MySqlReader).GetMethod
            (
                "IsExactCharacterSetCollationCatalogPair",
                BindingFlags.Static | BindingFlags.Public
            ));

            Assert.IsFalse(typeof(MySqlVerifiedColumnDefinitionSnapshot).IsPublic);
        }
    }
}
