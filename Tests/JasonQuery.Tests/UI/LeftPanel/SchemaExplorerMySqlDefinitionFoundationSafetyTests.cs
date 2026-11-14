using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4B3A: negative/safety tests. No DB and no DDL.
    [TestClass]
    [TestCategory("F7U3C4B3A")]
    public sealed class SchemaExplorerMySqlDefinitionFoundationSafetyTests
    {
        private static ColumnDdlRequest CreateRequest(ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = operation,
                SchemaDatabase = "demo",
                TableName = "b2_table",
                ColumnName = "old_name",
                NewColumnName = "new_name",
                Comment = "new comment"
            };
        }

        [TestMethod]
        public void B3A01_ExistingPreviewOnlyGateMustRemainClosed()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
        }

        [TestMethod]
        public void B3A02_OmittedAndAmbiguousDefaultsMustBeRejected()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, "int NOT NULL", out _));
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, "int DEFAULT NULL", out _));
            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, "int NULL DEFAULT NULL", out _));
        }

        [TestMethod]
        public void B3A03_UnsupportedExtrasMustBeRejected()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            var definitions = new[]
            {
                "int NOT NULL DEFAULT 0 INVISIBLE",
                "int NOT NULL DEFAULT 0 AUTO_INCREMENT",
                "int NOT NULL DEFAULT 0 ON UPDATE CURRENT_TIMESTAMP",
                "int NOT NULL DEFAULT (1 + 1)",
                "enum('a','b') NOT NULL DEFAULT 'a'"
            };

            foreach (var definition in definitions)
            {
                Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, definition, out var sql));
                Assert.AreEqual(string.Empty, sql);
            }
        }

        [TestMethod]
        public void B3A04_SqlSeparatorAndCommentsMustBeRejected()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            var definitions = new[]
            {
                "int NOT NULL DEFAULT 7; DROP TABLE demo.b2_table",
                "int NOT NULL DEFAULT 7 -- trailing",
                "int NOT NULL DEFAULT 7 /* trailing */",
                "int NOT NULL DEFAULT 7\r\nDROP TABLE x",
                "int NOT NULL DEFAULT 7\\n"
            };

            foreach (var definition in definitions)
            {
                Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, definition, out _));
            }
        }

        [TestMethod]
        public void B3A05_CommentMustStripOldCommentAndPreserveRest()
        {
            var request = CreateRequest(ColumnDdlOperation.Comment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
            (
                request, "int unsigned NOT NULL DEFAULT 7 COMMENT 'old comment'", out var sql
            ));

            Assert.AreEqual("ALTER TABLE `demo`.`b2_table` MODIFY COLUMN `old_name` int unsigned NOT NULL DEFAULT 7 COMMENT 'new comment';", sql);
        }

        [TestMethod]
        public void B3A06_UnsafeNewCommentsMustFailClosed()
        {
            var request = CreateRequest(ColumnDdlOperation.Comment);

            request.Comment = "injected\\' SQL";

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, "int NOT NULL DEFAULT 7", out _));

            request.Comment = "test\nnext";

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, "int NOT NULL DEFAULT 7", out _));
        }

        [TestMethod]
        public void B3A07_UnsupportedTargetAndOperationsMustFailClosed()
        {
            var request = CreateRequest(ColumnDdlOperation.Rename);

            request.DataSourceType = DataSourceType.PostgreSql;

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, "int NOT NULL DEFAULT 7", out _));

            request.DataSourceType = DataSourceType.MySql;
            request.NewColumnName = "old_name";

            Assert.IsFalse(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, "int NOT NULL DEFAULT 7", out _));
        }

        [TestMethod]
        public void B3A08_ReceiptMustBeReadOnlyAndHaveNoPublicConstructors()
        {
            var type = typeof(ColumnDdlSqlBuilder).Assembly.GetType("JasonQuery.Core.Database.DdlPreview.MySqlVerifiedColumnDefinitionSnapshot", true);

            Assert.IsTrue(type.IsSealed);
            Assert.AreEqual(0, type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length);
            Assert.IsTrue(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).All(p => !p.CanWrite));
        }

        [TestMethod]
        public void B3A09_UnverifiedReaderMustNotIssueMetadataOrReceipt()
        {
            var reader = new MySqlReader();

            Assert.IsFalse(reader.TryReadVerifiedColumnDefinitionOnConnection
            (
                Guid.Empty, "demo", "b2_table", "old_name", out var snapshot, out var error
            ));

            Assert.IsNull(snapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(error));
        }

        [TestMethod]
        public void B3A10_UnsafeIdentifiersMustBeRejectedBeforeConnect()
        {
            var reader = new MySqlReader();

            Assert.IsFalse(reader.TryReadVerifiedColumnDefinitionOnConnection
            (
                Guid.NewGuid(), "demo", "b2_table; DROP TABLE x", "old_name", out var snapshot, out _
            ));

            Assert.IsNull(snapshot);
        }

        [TestMethod]
        public void B3A11_LegacyRenameMustPreserveExistingCommentAndUnsigned()
        {
            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename
            (
                CreateRequest(ColumnDdlOperation.Rename),
                "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'", out var sql
            ));

            Assert.AreEqual("ALTER TABLE `demo`.`b2_table` CHANGE COLUMN `old_name` `new_name` int unsigned NOT NULL DEFAULT 7 COMMENT 'original';", sql);
        }
    }
}
