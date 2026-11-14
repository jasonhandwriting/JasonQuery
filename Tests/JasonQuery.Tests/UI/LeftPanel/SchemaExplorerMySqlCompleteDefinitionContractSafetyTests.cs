using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //B3-B2A: pure negative guards. No database connection and no DDL execution.
    [TestClass]
    [TestCategory("F7U3C4B3B2A")]
    public sealed class SchemaExplorerMySqlCompleteDefinitionContractSafetyTests
    {
        private const string Definition = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
        private static readonly Guid ConnectionId = new Guid("9a799a7a-d425-45d6-aaed-b688c55513a2");

        private static ColumnDdlRequest Request(ColumnDdlOperation operation)
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

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(Guid id, long sessionId, string version, string definition, string databaseName = "demo", string tableName = "b2_table")
        {
            return new MySqlVerifiedColumnDefinitionSnapshot(id, databaseName, tableName, "old_name", version, definition, sessionId);
        }

        private static string Sql(ColumnDdlRequest request)
        {
            string sql;

            var ok = request.Operation == ColumnDdlOperation.Rename
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, Definition, out sql)
                : MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, Definition, out sql);

            Assert.IsTrue(ok);
            return sql;
        }

        [TestMethod]
        public void B3B2A01_MismatchedFormDatabaseMustReject()
        {
            var request = Request(ColumnDdlOperation.Comment);

            request.SchemaDatabase = "another_database";

            var preview = Receipt(ConnectionId, 381, "8.0.34", Definition);
            var live = Receipt(ConnectionId, 381, "8.0.34", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A02_ChangedManagedConnectionMustReject()
        {
            var request = Request(ColumnDdlOperation.Rename);
            var preview = Receipt(ConnectionId, 381, "5.7.44", Definition);
            var live = Receipt(Guid.NewGuid(), 381, "5.7.44", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A03_ChangedPhysicalConnectionMustReject()
        {
            var request = Request(ColumnDdlOperation.Rename);
            var preview = Receipt(ConnectionId, 381, "5.7.44", Definition);
            var live = Receipt(ConnectionId, 382, "5.7.44", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A04_ChangedVersionMustReject()
        {
            var request = Request(ColumnDdlOperation.Comment);
            var preview = Receipt(ConnectionId, 381, "8.0.34", Definition);
            var live = Receipt(ConnectionId, 381, "8.0.35", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A05_ChangedCommentMetadataMustReject()
        {
            var request = Request(ColumnDdlOperation.Comment);
            var preview = Receipt(ConnectionId, 381, "8.0.34", Definition);
            var live = Receipt(ConnectionId, 381, "8.0.34", "int unsigned NOT NULL DEFAULT 7 COMMENT 'changed'");

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A06_WhitespaceDifferenceInSqlMustReject()
        {
            var request = Request(ColumnDdlOperation.Comment);
            var preview = Receipt(ConnectionId, 381, "5.7.44", Definition);
            var live = Receipt(ConnectionId, 381, "5.7.44", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request).Replace(" MODIFY ", "  MODIFY "))));
        }

        [TestMethod]
        public void B3B2A07_ModernRenameCannotSwitchToLegacyChange()
        {
            var request = Request(ColumnDdlOperation.Rename);
            var preview = Receipt(ConnectionId, 381, "8.0.34", Definition);
            var live = Receipt(ConnectionId, 381, "8.0.34", Definition);

            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request))));
        }

        [TestMethod]
        public void B3B2A08_ReaderWithoutConnectionMustNotDispatch()
        {
            var reader = new MySqlReader();
            var preview = Receipt(ConnectionId, 381, "5.7.44", Definition);
            var request = Request(ColumnDdlOperation.Rename);

            var count = reader.ExecuteCompleteColumnDdlOnVerifiedConnection
            (
                Sql(request), ConnectionId, request.Operation, preview,
                out var error, out var pending, out var closed
            );

            Assert.AreEqual(0, count);
            Assert.IsFalse(string.IsNullOrEmpty(error));
            Assert.IsFalse(pending);
            Assert.IsFalse(closed);
        }

        [TestMethod]
        public void B3B2A09_ModernCommentMayQualifyButDoesNotAuthorizeDispatch()
        {
            var request = Request(ColumnDdlOperation.Comment);
            var preview = Receipt(ConnectionId, 381, "8.0.34", Definition);
            var live = Receipt(ConnectionId, 381, "8.0.34", Definition);

            Assert.AreEqual(string.Empty, MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, preview, live, request, Sql(request)));
        }
    }
}
