using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //MySQL Column Comment literal safety: SQL grammar only, never an Execute permit.
    //No database connections, DDL dispatch, transaction changes, or Git mutations.
    //Expected baseline: G01-G08 GREEN, R09-R20 RED.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlColumnCommentLiteralGoldenTests
    {
        private const string CompleteDefinition = "varchar(32) NOT NULL DEFAULT 'a' COMMENT 'previous'";
        private const string TablePrefix = "ALTER TABLE `demo`.`comment_test` MODIFY COLUMN `note` ";
        private const string LiteralPolicyTypeName = "JasonQuery.Core.Database.DdlPreview.MySqlColumnCommentLiteralPolicy";

        private static ColumnDdlRequest NewRequest(string comment)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "demo",
                TableName = "comment_test",
                ColumnName = "note",
                Comment = comment
            };
        }

        private static bool TryBuild(string comment, out string sql)
        {
            return MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                NewRequest(comment), CompleteDefinition, out sql
            );
        }

        //Planned pure literal API, not an authorization token:
        //  internal static bool TryQuote(string value, bool noBackslashEscapes, out string quotedLiteral)
        //  The caller MUST obtain the effective SESSION sql_mode from the verified Reader.
        //  B4-C3 later needs to recheck the same mode before executing the exact Preview SQL.
        private static bool TryQuoteByVerifiedMode(string value, bool noBackslashEscapes, out string literal)
        {
            literal = string.Empty;

            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType(LiteralPolicyTypeName, false);

            Assert.IsNotNull(type, "EXPECTED RED: the shared MySQL comment literal policy is not yet implemented.");

            var method = type.GetMethod
            (
                "TryQuote",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(bool), typeof(string).MakeByRefType() },
                null
            );

            Assert.IsNotNull(method, "EXPECTED RED: TryQuote(string, bool, out string) contract is missing.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            var args = new object[] { value, noBackslashEscapes, null };
            var result = (bool)method.Invoke(null, args);

            literal = args[2] as string ?? string.Empty;
            return result;
        }

        [TestMethod]
        public void G01_ExistingAsciiCommentBuilderRemainsQualified()
        {
            Assert.IsTrue(TryBuild("updated", out var sql));
            Assert.AreEqual(TablePrefix + "varchar(32) NOT NULL DEFAULT 'a' COMMENT 'updated';", sql);
        }

        [TestMethod]
        public void G02_ExistingAsciiSqlShapeRemainsQualified()
        {
            Assert.IsTrue(TryBuild("updated", out var sql));

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql
            ));
        }

        [TestMethod]
        public void G03_ExistingDoubledQuoteInOldDefinitionRemainsQualified()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int NOT NULL DEFAULT 7 COMMENT 'it''s old'", out var definition
            ));

            Assert.AreEqual("int NOT NULL DEFAULT 7", definition);
        }

        [TestMethod]
        public void G04_NulCommentIsRejectedBeforeSqlGeneration()
        {
            Assert.IsFalse(TryBuild("before\0after", out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void G05_InvalidUtf16SurrogateIsRejected()
        {
            Assert.IsFalse(TryBuild("broken " + '\uD800', out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void G06_OverlongCommentRemainsRejected()
        {
            Assert.IsFalse(TryBuild(new string('a', 1025), out var sql));
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        public void G07_CommentSqlDoesNotAcquireRenamePermission()
        {
            Assert.IsTrue(TryBuild("updated", out var sql));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, sql
            ));

            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl
            (
                DataSourceType.MySql, EditColumnMode.Comment
            ));
        }

        [TestMethod]
        public void G08_AdditionalStatementOutsideLiteralRemainsRejected()
        {
            Assert.IsTrue(TryBuild("updated", out var sql));

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment,
                sql + " DROP TABLE `demo`.`comment_test`;"
            ));
        }

        [TestMethod]
        public void R09_ChineseAndJapaneseAreOrdinaryCommentText()
        {
            Assert.IsTrue(TryBuild("繁體中文、简体中文、こんにちは", out var sql));
            Assert.AreEqual(TablePrefix + "varchar(32) NOT NULL DEFAULT 'a' COMMENT '繁體中文、简体中文、こんにちは';", sql);
        }

        [TestMethod]
        public void R10_VietnameseAndEmojiDoNotNeedLanguageWhitelist()
        {
            Assert.IsTrue(TryBuild("Tiếng Việt 🚀 🐿️", out var sql));
            Assert.IsTrue(sql.Contains("COMMENT 'Tiếng Việt 🚀 🐿️';"));
        }

        [TestMethod]
        public void R11_SqlWordsAndSemicolonsInsideCommentRemainData()
        {
            const string raw = "'; DROP TABLE `demo`.`comment_test`; --";

            Assert.IsTrue(TryBuild(raw, out var sql));
            Assert.IsTrue(sql.EndsWith("COMMENT '''; DROP TABLE `demo`.`comment_test`; --';", StringComparison.Ordinal));

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql
            ));
        }

        [TestMethod]
        public void R12_SingleQuoteIsDoubledRatherThanDiscarded()
        {
            Assert.IsTrue(TryBuild("John's data", out var sql));
            Assert.IsTrue(sql.EndsWith("COMMENT 'John''s data';", StringComparison.Ordinal));
        }

        [TestMethod]
        public void R13_Exactly1024BasicUnicodeCharactersAreAllowed()
        {
            Assert.IsTrue(TryBuild(new string('字', 1024), out var sql));
            Assert.IsTrue(sql.Contains(new string('字', 1024)));
        }

        [TestMethod]
        public void R14_LengthCountsUnicodeScalarsRatherThanUtf16CodeUnits()
        {
            var comment = string.Concat(System.Linq.Enumerable.Repeat("🚀", 1024));

            Assert.IsTrue(TryBuild(comment, out var sql));
            Assert.IsTrue(sql.EndsWith("COMMENT '" + comment + "';", StringComparison.Ordinal));
        }

        [TestMethod]
        public void R15_ExistingChineseCommentRemainsValidMetadataGrammar()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "varchar(32) DEFAULT NULL COMMENT '先前註解'", out var definition
            ));

            Assert.AreEqual("varchar(32) DEFAULT NULL", definition);
        }

        [TestMethod]
        public void R16_ExistingUnicodeAndDoubledQuoteRemainValidMetadataGrammar()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryQualifyCompleteDefinition
            (
                "int DEFAULT NULL COMMENT 'Mary''s 註解'", out var definition
            ));

            Assert.AreEqual("int DEFAULT NULL", definition);
        }

        [TestMethod]
        public void R17_SqlShapeAcceptsValidUnicodeCommentLiteral()
        {
            const string sql = TablePrefix + "varchar(32) NOT NULL DEFAULT 'a' COMMENT '欄位說明';";

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql
            ));
        }

        [TestMethod]
        public void R18_SqlShapeAcceptsDoubledQuotesInsideTheLiteral()
        {
            const string sql = TablePrefix + "varchar(32) NOT NULL DEFAULT 'a' COMMENT 'John''s data';";

            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.MySql, ColumnDdlOperation.Comment, sql
            ));
        }

        [TestMethod]
        public void R19_BackslashesAreQuotedAccordingToVerifiedSessionSqlMode()
        {
            const string raw = "It's C:\\Data\\";

            Assert.IsTrue(TryQuoteByVerifiedMode(raw, false, out var defaultLiteral));
            Assert.AreEqual("'It''s C:\\\\Data\\\\'", defaultLiteral);

            Assert.IsTrue(TryQuoteByVerifiedMode(raw, true, out var noBackslashEscapesLiteral));
            Assert.AreEqual("'It''s C:\\Data\\'", noBackslashEscapesLiteral);
        }

        [TestMethod]
        public void R20_NewlineIsEncodedWithoutCreatingAnotherSqlStatement()
        {
            Assert.IsTrue(TryQuoteByVerifiedMode("line1\r\nline2", false, out var quoted));
            Assert.AreEqual("'line1\\r\\nline2'", quoted);
            Assert.IsFalse(quoted.Contains("\r"));
            Assert.IsFalse(quoted.Contains("\n"));
        }
    }
}
