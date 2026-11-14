using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C4C2-C1: Golden-first, pure Reader catalog-row contract; no connection or DDL.
    //All nine Red tests require one missing Reader-owned catalog-row validator.
    //The actual same-connection SELECT and receipt integration remain C4C2-C2 work.
    [TestClass]
    [TestCategory("F7U3C4C2C1")]
    public sealed class SchemaExplorerMySqlCharacterReaderCatalogGoldenTests
    {
        private const string CharacterDefinition = "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'original'";

        //The Reader must pass values from its own live INFORMATION_SCHEMA.COLLATIONS query.
        //The five arguments alone are NOT metadata provenance or Execute permission.
        //Signature: IsExactCharacterSetCollationCatalogPair(expectedCharset, expectedCollation, catalogCharset, catalogCollation, catalogRowCount) -> bool.
        private static MethodInfo CatalogMethod()
        {
            var method = typeof(MySqlReader).GetMethod
            (
                "IsExactCharacterSetCollationCatalogPair",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(int) },
                null
            );

            Assert.IsNotNull(method, "C4C2-C2 must add the Reader-owned exact catalog-pair validator; do not trust name prefixes.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            return method;
        }

        private static bool CatalogPair(string expectedCharset = "utf8mb4", string expectedCollation = "utf8mb4_bin", string catalogCharset = "utf8mb4", string catalogCollation = "utf8mb4_bin", int rowCount = 1)
        {
            return (bool)CatalogMethod().Invoke
            (
                null,
                new object[] { expectedCharset, expectedCollation, catalogCharset, catalogCollation, rowCount }
            );
        }

        [TestMethod]
        public void C4C2C01_ExistingReaderReceiptMethodRetainsItsSixArgumentContract()
        {
            var method = typeof(MySqlReader).GetMethod
            (
                "TryReadVerifiedColumnDefinitionOnConnection",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(bool), method.ReturnType);
            Assert.AreEqual(6, method.GetParameters().Length);
            Assert.AreEqual(typeof(Guid), method.GetParameters()[0].ParameterType);
            Assert.IsTrue(method.GetParameters()[4].IsOut);
            Assert.IsTrue(method.GetParameters()[5].IsOut);
        }

        [TestMethod]
        public void C4C2C02_ExistingCharacterProofRequiresAnAlreadyVerifiedCatalogPair()
        {
            var status = MySqlCharacterColumnDefinitionProofPolicy.Validate
            (
                CharacterDefinition, "varchar(32)", "NO", "abc", "original",
                "utf8mb4", "utf8mb4_bin", "", "", "", false
            );

            Assert.IsFalse(string.IsNullOrEmpty(status));
        }

        [TestMethod]
        public void C4C2C03_ExistingCharacterProofAcceptsFullyMatchedInputs()
        {
            var status = MySqlCharacterColumnDefinitionProofPolicy.Validate
            (
                CharacterDefinition, "varchar(32)", "NO", "abc", "original",
                "utf8mb4", "utf8mb4_bin", "", "", "", true
            );

            Assert.AreEqual(string.Empty, status);
        }

        [TestMethod]
        public void C4C2C04_IntegerDefinitionMustStillQualifyWithoutACharacterCatalog()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int NOT NULL DEFAULT 7 COMMENT 'original'", out _
            ));
        }

        [TestMethod]
        public void C4C2C05_MissingReceiptCannotEnableMySqlColumnComment()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition
            (
                DataSourceType.MySql, EditColumnMode.Comment, null, false
            ));
        }

        //Expected Red: C4C2-C2 adds the narrowly scoped Reader-owned catalog matcher.
        [TestMethod]
        public void C4C2C06_ReaderMustDeclareTheCatalogRowValidationContract()
        {
            Assert.IsNotNull(CatalogMethod());
        }

        [TestMethod]
        public void C4C2C07_OneExactCatalogRowMayQualify()
        {
            Assert.IsTrue(CatalogPair());
        }

        [TestMethod]
        public void C4C2C08_CaseInsensitiveCatalogIdentifierComparisonMustQualify()
        {
            Assert.IsTrue(CatalogPair(catalogCharset: "UTF8MB4", catalogCollation: "UTF8MB4_BIN"));
        }

        [TestMethod]
        public void C4C2C09_CatalogCharsetMismatchMustFailClosed()
        {
            Assert.IsFalse(CatalogPair(catalogCharset: "latin1"));
        }

        [TestMethod]
        public void C4C2C10_CatalogCollationMismatchMustFailClosed()
        {
            Assert.IsFalse(CatalogPair(catalogCollation: "utf8mb4_0900_ai_ci"));
        }

        [TestMethod]
        public void C4C2C11_NoCatalogRowMustNeverQualify()
        {
            Assert.IsFalse(CatalogPair(rowCount: 0));
        }

        [TestMethod]
        public void C4C2C12_DuplicateCatalogRowsMustNeverQualify()
        {
            Assert.IsFalse(CatalogPair(rowCount: 2));
        }

        [TestMethod]
        public void C4C2C13_MissingCatalogAttributeMustNeverQualify()
        {
            Assert.IsFalse(CatalogPair(catalogCharset: null));
            Assert.IsFalse(CatalogPair(catalogCollation: string.Empty));
            Assert.IsFalse(CatalogPair(expectedCharset: string.Empty));
        }

        [TestMethod]
        public void C4C2C14_UnsafeCatalogNamesMustNeverQualify()
        {
            Assert.IsFalse(CatalogPair(expectedCollation: "utf8mb4_bin;"));
            Assert.IsFalse(CatalogPair(expectedCharset: "utf8mb4 "));
            Assert.IsFalse(CatalogPair(catalogCharset: "utf8mb4\n"));
        }
    }
}
