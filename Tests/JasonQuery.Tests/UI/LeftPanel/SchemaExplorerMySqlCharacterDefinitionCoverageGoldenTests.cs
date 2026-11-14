using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4C2-A: SQL grammar + metadata-proof contract, not a Reader permit.
    //Compile-GREEN / behavioral-RED. No database calls, DDL, or Git mutations.
    //First text-column slice: explicit CHARACTER SET + COLLATE, NOT NULL, quoted ASCII DEFAULT.
    [TestClass]
    [TestCategory("F7U3C4C2A")]
    public sealed class SchemaExplorerMySqlCharacterDefinitionCoverageGoldenTests
    {
        private const string CharDefinition = "char(12) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'Z' COMMENT 'original'";
        private const string VarcharDefinition = "varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'original'";
        private const string ProofTypeName = "JasonQuery.Core.Database.DdlPreview.MySqlCharacterColumnDefinitionProofPolicy";

        private static ColumnDdlRequest Request(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = operation,
                SchemaDatabase = "demo",
                TableName = "character_test",
                ColumnName = "old_name",
                NewColumnName = "new_name",
                Comment = "updated"
            };
        }

        //The planned pure Reader-facing policy must use data from the verified physical connection.
        //Its final boolean may only be true after confirming the (CHARSET, COLLATE) pair against INFORMATION_SCHEMA.COLLATIONS on the same transaction/connection. The policy itself never creates a receipt, opens a connection, or authorizes Execute.
        //Contract: Validate(definition, columnType, isNullable, columnDefault, oldComment,
        //                   characterSet, collation, extra, generationExpression, columnKey,
        //                   verifiedCharsetCollationPair) -> empty on qualified, nonempty otherwise.
        private static string ProofStatus(string definition = VarcharDefinition, string columnType = "varchar(32)", string isNullable = "NO", string defaultValue = "abc", string oldComment = "original", string charset = "utf8mb4", string collation = "utf8mb4_bin", string extra = "", string generationExpression = "", string columnKey = "", bool verifiedPair = true)
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(ProofTypeName, false);

            Assert.IsNotNull(type, "C4C2-B must introduce a pure, fail-closed charset/collation metadata proof policy.");

            var method = type.GetMethod
            (
                "Validate",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                    typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                    typeof(bool)
                },
                null
            );

            Assert.IsNotNull(method, "C4C2-B requires Validate with ten string inputs and one verified-pair bool.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    definition, columnType, isNullable, defaultValue, oldComment,
                    charset, collation, extra, generationExpression, columnKey, verifiedPair
                }
            );
        }

        [TestMethod]
        public void C4C2A01_ExistingIntegerDefinitionMustRemainQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition("int NOT NULL DEFAULT 7", out var baseDefinition));
            Assert.AreEqual("int NOT NULL DEFAULT 7", baseDefinition);
        }

        [TestMethod]
        public void C4C2A02_ExistingIntegerLegacyBuilderMustRemainAvailable()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename
            (
                Request(ColumnDdlOperation.Rename), "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'", out var sql
            ));

            Assert.Contains("`new_name` int unsigned NOT NULL DEFAULT 7 COMMENT 'original';", sql);
        }

        [TestMethod]
        public void C4C2A03_PlainVarcharGrammarAloneDoesNotProveReaderMetadata()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(32) NOT NULL DEFAULT 'abc'", out var withoutComment
            ));

            Assert.AreEqual("varchar(32) NOT NULL DEFAULT 'abc'", withoutComment);
        }

        [TestMethod]
        public void C4C2A04_ExpressionDefaultAndSqlInjectionMustRemainRejected()
        {
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(32) NOT NULL DEFAULT (uuid())", out _
            ));

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(32) NOT NULL DEFAULT 'abc'; DROP TABLE demo.character_test;", out _
            ));
        }

        [TestMethod]
        public void C4C2A05_ModernRenameMustNotRequireFullDefinition()
        {
            Assert.AreEqual(MySqlColumnRenameSyntax.RenameColumn, MySqlColumnDdlVersionPolicy.SelectRenameSyntax("8.0.34"));

            var sql = ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql(Request(ColumnDdlOperation.Rename));

            Assert.AreEqual("ALTER TABLE `demo`.`character_test` RENAME COLUMN `old_name` TO `new_name`;", sql);
        }

        [TestMethod]
        public void C4C2A06_TextCoverageMustNotUnlockExistingColumnWithoutReceipt()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, null, false));
        }

        //Expected Red: the next stage must extend the strict, pure grammar.
        [TestMethod]
        public void C4C2A07_ExplicitCharsetCollationVarcharDefinitionMustQualify()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(VarcharDefinition, out var baseDefinition));
            Assert.AreEqual("varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc'", baseDefinition);
        }

        [TestMethod]
        public void C4C2A08_ExplicitCharsetCollationCharDefinitionMustQualify()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition(CharDefinition, out var baseDefinition));
            Assert.AreEqual("char(12) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL DEFAULT 'Z'", baseDefinition);
        }

        [TestMethod]
        public void C4C2A09_CommentMustRetainCharsetCollationAndDefaultExactly()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(Request(ColumnDdlOperation.Comment), VarcharDefinition, out var sql));
            Assert.AreEqual("ALTER TABLE `demo`.`character_test` MODIFY COLUMN `old_name` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL DEFAULT 'abc' COMMENT 'updated';", sql);
        }

        [TestMethod]
        public void C4C2A10_LegacyRenameMustRetainCharsetCollationAndOldComment()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(Request(ColumnDdlOperation.Rename), CharDefinition, out var sql));
            Assert.AreEqual("ALTER TABLE `demo`.`character_test` CHANGE COLUMN `old_name` `new_name` " + CharDefinition + ";", sql);
        }

        //Expected Red: metadata corroboration cannot be inferred from SQL grammar alone.
        [TestMethod]
        public void C4C2A11_ProofMustAcceptMatchingExplicitVarcharEvidence()
        {
            Assert.AreEqual(string.Empty, ProofStatus());
        }

        [TestMethod]
        public void C4C2A12_ProofMustAcceptMatchingExplicitCharEvidence()
        {
            Assert.AreEqual(string.Empty, ProofStatus
            (
                definition: CharDefinition, columnType: "char(12)", defaultValue: "Z",
                charset: "latin1", collation: "latin1_swedish_ci"
            ));
        }

        [TestMethod]
        public void C4C2A13_MismatchedCollationMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(collation: "utf8mb4_0900_ai_ci")));
        }

        [TestMethod]
        public void C4C2A14_MismatchedCharacterSetMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(charset: "latin1")));
        }

        [TestMethod]
        public void C4C2A15_MismatchedDefaultMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(defaultValue: "other")));
        }

        [TestMethod]
        public void C4C2A16_MismatchedOriginalCommentMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(oldComment: "different")));
        }

        [TestMethod]
        public void C4C2A17_UnverifiedCharsetCollationCatalogPairMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(verifiedPair: false)));
        }

        [TestMethod]
        public void C4C2A18_GeneratedAndExtraAttributesMustBeRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(extra: "VIRTUAL GENERATED", generationExpression: "(1)")));
        }

        [TestMethod]
        public void C4C2A19_InheritedCharsetWithoutExplicitColumnClausesRemainsOutOfScope()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus
            (
                definition: "varchar(32) NOT NULL DEFAULT 'abc' COMMENT 'original'"
            )));
        }

        [TestMethod]
        public void C4C2A20_AmbiguousNullDefaultMustRemainRejected()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ProofStatus(defaultValue: null)));
        }
    }
}
