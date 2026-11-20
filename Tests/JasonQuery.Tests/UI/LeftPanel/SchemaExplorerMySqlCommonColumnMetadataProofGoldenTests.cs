using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C3-B3A: pure MySQL metadata proof contract. No database connections or DDL dispatch.
    //A valid SQL grammar is not Reader evidence and never authorizes Execute.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlCommonColumnMetadataProofGoldenTests
    {
        private const string ProofTypeName = "JasonQuery.Core.Database.DdlPreview.MySqlCommonColumnDefinitionProofPolicy";

        //C3-B3B planned contract: 12 metadata/DDL strings and one catalog-proof flag.
        //null columnDefault represents INFORMATION_SCHEMA.COLUMNS.COLUMN_DEFAULT = SQL NULL.
        //definition MUST come from a unique SHOW CREATE TABLE column line, not caller SQL.
        //Return empty for qualified; nonempty diagnostic status for refusal.
        private static string Proof(string definition = "double DEFAULT NULL", string columnType = "double", string isNullable = "YES", string columnDefault = null, string oldComment = "", string characterSet = "", string collation = "", string tableCharacterSet = "", string tableCollation = "", string extra = "", string generationExpression = "", string columnKey = "", bool verifiedPair = false)
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(ProofTypeName, false);

            Assert.IsNotNull(type, "C3-B3A Expected RED: common column metadata proof policy is not implemented.");

            var method = type.GetMethod
            (
                "Validate",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(string), typeof(string), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string),
                    typeof(bool)
                },
                null
            );

            Assert.IsNotNull(method, "C3-B3A Expected RED: Validate signature is missing.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    definition, columnType, isNullable, columnDefault, oldComment,
                    characterSet, collation, tableCharacterSet, tableCollation,
                    extra, generationExpression, columnKey, verifiedPair
                }
            );
        }

        [TestMethod]
        public void B3G01_CommonNumericGrammarRemainsQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("double DEFAULT NULL", out _));
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("decimal(10,2) NULL DEFAULT 1.25", out _));
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("int NULL DEFAULT NULL", out _));
        }

        [TestMethod]
        public void B3G02_InheritedCharacterGrammarRemainsQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("varchar(32) DEFAULT NULL", out _));
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("char(4) NULL DEFAULT 'xy'", out _));
        }

        [TestMethod]
        public void B3G03_UnsupportedExtraClausesRemainRejectedByGrammar()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("int DEFAULT 1 AUTO_INCREMENT", out _));
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("varchar(20) DEFAULT (uuid())", out _));
        }

        [TestMethod]
        public void B3G04_CharacterCatalogPairRequiresExactSingleMatch()
        {
            Assert.IsTrue(MySqlReader.IsExactCharacterSetCollationCatalogPair
            (
                "utf8mb4", "utf8mb4_0900_ai_ci", "utf8mb4", "utf8mb4_0900_ai_ci", 1
            ));

            Assert.IsFalse(MySqlReader.IsExactCharacterSetCollationCatalogPair
            (
                "utf8mb4", "utf8mb4_bin", "utf8mb4", "utf8mb4_0900_ai_ci", 1
            ));
        }

        [TestMethod]
        public void B3G05_PendingTransactionCannotInvokeDdlDelegates()
        {
            var calls = 0;

            var status = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql,
                true,
                true,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return Guid.NewGuid(); },
                _ => { calls++; return string.Empty; }
            );

            Assert.AreEqual("F7U2C2_TRANSACTION_UNSAFE", status);
            Assert.AreEqual(0, calls);
        }

        //Expected RED below: Reader-facing metadata proof does not yet exist.
        //Positive cases are eligible ONLY after comparing SHOW CREATE and INFORMATION_SCHEMA.
        [TestMethod]
        public void B3R06_DoubleNullableExplicitDefaultNullMustMatchMetadata()
        {
            Assert.AreEqual(string.Empty, Proof());
        }

        [TestMethod]
        public void B3R07_DecimalPrecisionScaleAndNumericDefaultMustMatch()
        {
            Assert.AreEqual(string.Empty, Proof
            (
                definition: "decimal(10,2) NULL DEFAULT 1.25",
                columnType: "decimal(10,2)",
                columnDefault: "1.25"
            ));
        }

        [TestMethod]
        public void B3R08_IntegerNullableExplicitDefaultNullMustMatch()
        {
            Assert.AreEqual(string.Empty, Proof
            (
                definition: "int NULL DEFAULT NULL",
                columnType: "int"
            ));
        }

        [TestMethod]
        public void B3R09_InheritedVarcharMustMatchVerifiedTableCollation()
        {
            Assert.AreEqual(string.Empty, Proof
            (
                definition: "varchar(32) DEFAULT NULL",
                columnType: "varchar(32)",
                characterSet: "utf8mb4",
                collation: "utf8mb4_0900_ai_ci",
                tableCharacterSet: "utf8mb4",
                tableCollation: "utf8mb4_0900_ai_ci",
                verifiedPair: true
            ));
        }

        [TestMethod]
        public void B3R10_InheritedCharLiteralDefaultMustMatchEffectiveAttributes()
        {
            Assert.AreEqual(string.Empty, Proof
            (
                definition: "char(4) NULL DEFAULT 'xy'",
                columnType: "char(4)",
                columnDefault: "xy",
                characterSet: "utf8mb4",
                collation: "utf8mb4_0900_ai_ci",
                tableCharacterSet: "utf8mb4",
                tableCollation: "utf8mb4_0900_ai_ci",
                verifiedPair: true
            ));
        }

        [TestMethod]
        public void B3R11_NumericOldCommentMustMatchExactMetadata()
        {
            Assert.AreEqual(string.Empty, Proof
            (
                definition: "double NOT NULL DEFAULT 2.5 COMMENT 'legacy'",
                columnType: "double",
                isNullable: "NO",
                columnDefault: "2.5",
                oldComment: "legacy"
            ));
        }

        [TestMethod]
        public void B3R12_NullabilityMismatchMustFailClosed()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof(isNullable: "NO")));
        }

        [TestMethod]
        public void B3R13_DefaultNullCannotMatchNonNullMetadata()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof(columnDefault: "7")));
        }

        [TestMethod]
        public void B3R14_InheritedCollationMismatchMustFailClosed()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof
            (
                definition: "varchar(32) DEFAULT NULL",
                columnType: "varchar(32)",
                characterSet: "utf8mb4",
                collation: "utf8mb4_bin",
                tableCharacterSet: "utf8mb4",
                tableCollation: "utf8mb4_0900_ai_ci",
                verifiedPair: true
            )));
        }

        [TestMethod]
        public void B3R15_InheritedCharsetMustHaveVerifiedCatalogProof()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof
            (
                definition: "varchar(32) DEFAULT NULL",
                columnType: "varchar(32)",
                characterSet: "utf8mb4",
                collation: "utf8mb4_0900_ai_ci",
                tableCharacterSet: "utf8mb4",
                tableCollation: "utf8mb4_0900_ai_ci",
                verifiedPair: false
            )));
        }

        [TestMethod]
        public void B3R16_UnsafeExtraGenerationAndIndexMetadataMustFailClosed()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof(extra: "auto_increment")));
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof(generationExpression: "1+1")));
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof(columnKey: "MUL")));
        }

        [TestMethod]
        public void B3R17_OldCommentMismatchMustFailClosed()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(Proof
            (
                definition: "double NOT NULL DEFAULT 2.5 COMMENT 'legacy'",
                columnType: "double",
                isNullable: "NO",
                columnDefault: "2.5",
                oldComment: "different"
            )));
        }
    }
}
