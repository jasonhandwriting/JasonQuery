using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C4C2-B: supplemental negative/edge tests for pure proof; no Reader, MySQL connection or DDL.
    [TestClass]
    [TestCategory("F7U3C4C2B")]
    public sealed class SchemaExplorerMySqlCharacterDefinitionProofSafetyTests
    {
        private const string Definition = "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'original'";

        private static string Validate(string definition = Definition, string type = "varchar(32)", string defaultValue = "abc",
                                       string oldComment = "original", string charset = "utf8mb4", string collation = "utf8mb4_bin",
                                       string extra = "", string generation = "", string key = "", bool verifiedPair = true)
        {
            return MySqlCharacterColumnDefinitionProofPolicy.Validate
            (
                definition, type, "NO", defaultValue, oldComment, charset, collation,
                extra, generation, key, verifiedPair
            );
        }

        [TestMethod]
        public void C4C2B01_UnverifiedCatalogPairMustFailEvenIfNameLooksCorrect()
        {
            Assert.IsFalse(string.IsNullOrEmpty(Validate(verifiedPair: false)));
        }

        [TestMethod]
        public void C4C2B02_CharLengthBeyond255MustFail()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "char(256) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'Z'", out _
            ));

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(65536) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'Z'", out _
            ));
        }

        [TestMethod]
        public void C4C2B03_ExistingIndexOrKeyMetadataMustFail()
        {
            Assert.IsFalse(string.IsNullOrEmpty(Validate(key: "PRI")));
            Assert.IsFalse(string.IsNullOrEmpty(Validate(extra: null)));
        }

        [TestMethod]
        public void C4C2B04_EscapedSingleQuoteMustBeComparedAgainstUnescapedMetadata()
        {
            const string definition = "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'it''s' COMMENT 'owner''s'";

            Assert.AreEqual(string.Empty, Validate(definition: definition, defaultValue: "it's", oldComment: "owner's"));
            Assert.IsFalse(string.IsNullOrEmpty(Validate(definition: definition, defaultValue: "it''s", oldComment: "owner's")));
        }

        [TestMethod]
        public void C4C2B05_BackslashOrUnverifiedDefaultMustFail()
        {
            Assert.IsFalse(string.IsNullOrEmpty(Validate(definition: @"varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'a\b' COMMENT 'original'")));
            Assert.IsFalse(string.IsNullOrEmpty(Validate(defaultValue: null)));
        }

        [TestMethod]
        public void C4C2B06_EmptyAndInheritedAttributesCannotBeReinterpretedAsExplicitProof()
        {
            Assert.IsFalse(string.IsNullOrEmpty(Validate(definition: "varchar(32) NOT NULL DEFAULT 'abc' COMMENT 'original'")));
            Assert.IsFalse(string.IsNullOrEmpty(Validate(collation: "utf8mb4_0900_ai_ci")));
        }
    }
}
