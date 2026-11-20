using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //Cross-operation connection independence is NOT permission to bypass live DDL validation.
    //The Reader must still bind the final metadata proof and dispatch to its CURRENT connection.
    [TestClass]
    [TestCategory("F7S1C3CD3A")]
    public sealed class SchemaExplorerMySqlPreviewConnectionIndependenceGoldenTests
    {
        private const string Definition = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
        private const string Signature = "verified-effective-attributes";
        private const string Database = "sakila";
        private const string Table = "jq_c3c_comment_smoke";
        private const string Column = "id";
        private const string OrdinaryMode = "STRICT_TRANS_TABLES";
        private const string NoBackslashMode = "STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES";

        private static readonly Guid FirstConnection = new Guid("181ec03a-7391-4511-a969-e12bb6532b01");
        private static readonly Guid SecondConnection = new Guid("4995984e-b707-46cd-acf4-9e3f6e35b4e9");

        private static ColumnDdlRequest Request(string comment = @"It's C:\Data; SELECT 2; --")
        {
            return new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = Database,
                TableName = Table,
                ColumnName = Column,
                Comment = comment
            };
        }

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(Guid id, long sessionId, string mode, string definition = Definition, string table = Table)
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                id, Database, table, Column, "8.0.34", definition,
                sessionId, Signature, mode
            );
        }

        private static string Sql(ColumnDdlRequest request, bool noBackslashEscapes = false)
        {
            Assert.IsTrue
            (
                MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment
                (
                    request, Definition, noBackslashEscapes, out var sql
                )
            );

            return sql;
        }

        private static string Verify(MySqlVerifiedColumnDefinitionSnapshot preview, MySqlVerifiedColumnDefinitionSnapshot live, ColumnDdlRequest request, bool noBackslashEscapes = false)
        {
            return MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
            (
                ColumnDdlOperation.Comment, preview, live, request,
                Sql(request, noBackslashEscapes)
            );
        }

        [TestMethod]
        public void G01_NoOpenReaderMustNotDispatch()
        {
            var reader = new MySqlReader();
            var receipt = Receipt(FirstConnection, 6001, OrdinaryMode);
            var sql = Sql(Request());

            var count = reader.ExecuteCompleteColumnDdlOnVerifiedConnection
            (
                sql, FirstConnection, ColumnDdlOperation.Comment, receipt,
                out var error, out var pending, out var closed
            );

            Assert.AreEqual(0, count);
            Assert.IsFalse(string.IsNullOrEmpty(error));
            Assert.IsFalse(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        public void G02_ChangedTableMustStillReject()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(FirstConnection, 6001, OrdinaryMode, table: "different_table");

            Assert.AreNotEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void G03_ChangedDefinitionMustStillReject()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(FirstConnection, 6001, OrdinaryMode, definition: "int unsigned NOT NULL DEFAULT 8 COMMENT 'original'");

            Assert.AreNotEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void G04_ChangedSessionSqlModeMustStillReject()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(FirstConnection, 6001, NoBackslashMode);

            Assert.AreNotEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void G05_TamperedSqlMustStillReject()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(FirstConnection, 6001, OrdinaryMode);
            var modified = Sql(request) + " SELECT 1;";

            Assert.AreNotEqual
            (
                string.Empty,
                MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
                (
                    ColumnDdlOperation.Comment, preview, live, request, modified
                )
            );
        }

        [TestMethod]
        public void G06_LegacyReceiptWithoutModeEvidenceMustRejectSensitiveComment()
        {
            var request = Request();

            var preview = new MySqlVerifiedColumnDefinitionSnapshot
            (
                FirstConnection, Database, Table, Column, "8.0.34", Definition,
                6001, Signature
            );

            var live = Receipt(FirstConnection, 6001, OrdinaryMode);

            Assert.AreNotEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void R07_ChangedManagedConnectionWithSameVerifiedProofMustQualify()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(SecondConnection, 6001, OrdinaryMode);

            Assert.AreEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void R08_ChangedServerConnectionWithSameVerifiedProofMustQualify()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, OrdinaryMode);
            var live = Receipt(FirstConnection, 6002, OrdinaryMode);

            Assert.AreEqual(string.Empty, Verify(preview, live, request));
        }

        [TestMethod]
        public void R09_ChangedBothConnectionIdsWithSameVerifiedProofMustQualify()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, NoBackslashMode);
            var live = Receipt(SecondConnection, 6002, NoBackslashMode);

            Assert.AreEqual(string.Empty, Verify(preview, live, request, noBackslashEscapes: true));
        }

        [TestMethod]
        public void R10_EmptyButVerifiedModeAcrossConnectionsMustQualify()
        {
            var request = Request();
            var preview = Receipt(FirstConnection, 6001, string.Empty);
            var live = Receipt(SecondConnection, 6002, string.Empty);

            Assert.IsTrue(preview.HasVerifiedSessionSqlMode);
            Assert.IsTrue(live.HasVerifiedSessionSqlMode);
            Assert.AreEqual(string.Empty, Verify(preview, live, request));
        }
    }
}
