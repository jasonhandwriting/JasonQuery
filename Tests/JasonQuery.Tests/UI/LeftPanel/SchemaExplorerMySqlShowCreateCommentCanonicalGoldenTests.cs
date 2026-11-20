using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-S1-C3C-D3C-03A: Golden RED for observed MySQL 8.0.34 SHOW CREATE TABLE serialization.
    //Pure policy tests only. They do not open a connection, change sql_mode, or execute DDL.
    [TestClass]
    [TestCategory("F7S1C3CD3C03A")]
    public sealed class SchemaExplorerMySqlShowCreateCommentCanonicalGoldenTests
    {
        //From D3C-02-BEFORE-SHOW.csv: SHOW CREATE emits TWO backslashes.
        private const string ShowCreateDefinition = @"int NOT NULL DEFAULT '1' COMMENT 'It''s C:\\Data; SELECT 1; --'";

        //From D3C-02-BEFORE.csv: INFORMATION_SCHEMA.COLUMN_COMMENT contains ONE backslash.
        private const string CatalogComment = @"It's C:\Data; SELECT 1; --";
        private const string NewComment = @"D3 It's C:\Data; SELECT 2; --";
        private const string DatabaseName = "sakila";
        private const string TableName = "jq_c3c_comment_smoke";
        private const string ColumnName = "id";
        private const string NoBackslashMode = "STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES";
        private const string OrdinaryMode = "STRICT_TRANS_TABLES";

        private static readonly Guid VerifiedConnectionId = new Guid("8CC2F233-5BD3-4A11-9EB3-597043BD60DA");

        private static ColumnDdlRequest Request(string comment = NewComment)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = DatabaseName,
                TableName = TableName,
                ColumnName = ColumnName,
                Comment = comment
            };
        }

        private static string VerifyColumnMetadata(string catalogComment, bool noBackslashEscapes)
        {
            //The actual D3C-02 column is a simple single-column PRIMARY KEY.
            //This pure helper receives the pre-verified PK flag; it does not grant a Reader receipt.
            return MySqlCommonColumnDefinitionProofPolicy.Validate
            (
                ShowCreateDefinition,
                "int", "NO", "1", catalogComment,
                string.Empty, string.Empty, string.Empty, "utf8mb4_0900_ai_ci",
                string.Empty, string.Empty, "PRI",
                false, true, noBackslashEscapes
            );
        }

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(string verifiedMode)
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                VerifiedConnectionId, DatabaseName, TableName, ColumnName,
                "8.0.34", ShowCreateDefinition, 901, "verified-effective-attributes", verifiedMode
            );
        }

        [TestMethod]
        public void D3C03A_G01_NoBackslashModeMustCorroborateActualShowCreateAndCatalogComment()
        {
            //EXPECTED RED on current D3B: F7U3C4B3_ATTRIBUTES_NOT_PROVEN.
            //A real metadata mismatch still MUST fail (G04); do not bypass the proof.
            Assert.AreEqual
            (
                string.Empty,
                VerifyColumnMetadata(CatalogComment, noBackslashEscapes: true),
                "D3C-03A RED: SHOW CREATE has \\\\ while COLUMN_COMMENT has \\; "
                + "the old comment must be decoded as SHOW CREATE output, not as new NBE SQL."
            );
        }

        [TestMethod]
        public void D3C03A_G02_OrdinaryModeMustStillCorroborateSameMetadata()
        {
            Assert.AreEqual
            (
                string.Empty,
                VerifyColumnMetadata(CatalogComment, noBackslashEscapes: false)
            );
        }

        [TestMethod]
        public void D3C03A_G03_NoBackslashModeGrammarMustRetainExactBaseDefinition()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                ShowCreateDefinition, true, out var withoutOldComment
            ));

            Assert.AreEqual("int NOT NULL DEFAULT '1'", withoutOldComment);
        }

        [TestMethod]
        public void D3C03A_G04_DifferentActualCatalogCommentMustFailClosed()
        {
            const string different = @"It's C:\Changed; SELECT 1; --";

            Assert.AreEqual
            (
                "F7U3C4B3_ATTRIBUTES_NOT_PROVEN",
                VerifyColumnMetadata(different, noBackslashEscapes: true)
            );

            Assert.AreEqual
            (
                "F7U3C4B3_ATTRIBUTES_NOT_PROVEN",
                VerifyColumnMetadata(different, noBackslashEscapes: false)
            );
        }

        [TestMethod]
        public void D3C03A_G05_NoBackslashModeMustBuildSingleSlashAndCanonicalSingleStatement()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(), ShowCreateDefinition, true, out var sql
            ));

            const string expected = "ALTER TABLE `sakila`.`jq_c3c_comment_smoke` "
                + "MODIFY COLUMN `id` int NOT NULL DEFAULT '1' "
                + "COMMENT 'D3 It''s C:\\Data; SELECT 2; --';";

            Assert.AreEqual(expected, sql);
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(sql, true));

            Assert.IsTrue(MySqlColumnCommentSqlIntentPolicy.TryExtractCanonicalComment
            (
                sql, DatabaseName, TableName, ColumnName,
                ShowCreateDefinition, true, out var decoded
            ));

            Assert.AreEqual(NewComment, decoded);

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode
            (
                sql + " SELECT 3;", true
            ));
        }

        [TestMethod]
        public void D3C03A_G06_OrdinaryModeMustStillGenerateDoubleSlashForNewSql()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(), ShowCreateDefinition, false, out var sql
            ));

            const string expectedSuffix = "COMMENT 'D3 It''s C:\\\\Data; SELECT 2; --';";

            Assert.IsTrue(sql.EndsWith(expectedSuffix, StringComparison.Ordinal));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedMySqlCommentSqlForSessionMode(sql, false));

            Assert.IsTrue(MySqlColumnCommentSqlIntentPolicy.TryExtractCanonicalComment
            (
                sql, DatabaseName, TableName, ColumnName,
                ShowCreateDefinition, false, out var decoded
            ));

            Assert.AreEqual(NewComment, decoded);
        }

        [TestMethod]
        public void D3C03A_G07_ExecuteTimeSessionModeChangeMustRemainBlocked()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                Request(), ShowCreateDefinition, true, out var previewSql
            ));

            Assert.AreEqual
            (
                "F7S1_C3C_SESSION_MODE_CHANGED",
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment,
                    Receipt(NoBackslashMode), Receipt(OrdinaryMode),
                    Request(), previewSql
                )
            );
        }
    }
}
