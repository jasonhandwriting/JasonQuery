using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4B3B1: compile-clean Golden Red; no DB connection and no DDL dispatch.
    //The RED tests describe B3-B2 opt-in Execute integration, not the existing Modern Rename.
    [TestClass]
    [TestCategory("F7U3C4B3B1")]
    public sealed class SchemaExplorerMySqlCompleteDefinitionExecuteIntegrationGoldenTests
    {
        private const string PolicyTypeName = "JasonQuery.Core.Database.DdlPreview.MySqlVerifiedColumnDefinitionExecutionPolicy";
        private const string CompleteDefinition = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
        private static readonly Guid SessionId = new Guid("1a7788ad-3de2-4b48-a1a2-18e705dcc723");

        private static ColumnDdlRequest NewRequest(ColumnDdlOperation operation)
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

        private static MySqlVerifiedColumnDefinitionSnapshot NewSnapshot(Guid sessionId, long serverSessionId, string version, string definition, string tableName = "b2_table")
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                sessionId, "demo", tableName, "old_name", version, definition, serverSessionId
            );
        }

        private static string BuildExpectedSql(ColumnDdlRequest request)
        {
            string sql;

            var built = request.Operation == ColumnDdlOperation.Rename
                ? MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, CompleteDefinition, out sql)
                : MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, CompleteDefinition, out sql);

            Assert.IsTrue(built, "B3-A complete-definition builder should already be Green.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(sql));

            return sql;
        }

        private static string InvokeExpectedPolicy(ColumnDdlRequest request, MySqlVerifiedColumnDefinitionSnapshot preview, MySqlVerifiedColumnDefinitionSnapshot live, string displayedSql)
        {
            var assembly = typeof(ColumnDdlSqlBuilder).Assembly;
            var type = assembly.GetType(PolicyTypeName, false);

            Assert.IsNotNull(type, "B3-B2 must supply a pure, fail-closed preview/live receipt and SQL policy.");

            var parameterTypes = new[]
            {
                typeof(ColumnDdlOperation),
                typeof(MySqlVerifiedColumnDefinitionSnapshot),
                typeof(MySqlVerifiedColumnDefinitionSnapshot),
                typeof(ColumnDdlRequest),
                typeof(string)
            };

            var method = type.GetMethod
            (
                "Validate",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameterTypes, null
            );

            Assert.IsNotNull(method, "Required Validate(operation, preview, live, request, displayedSql) contract is absent.");
            Assert.AreEqual(typeof(string), method.ReturnType);

            return (string)method.Invoke(null, new object[] { request.Operation, preview, live, request, displayedSql });
        }

        [TestMethod]
        public void B3B01_LegacyAndCommentMustStayClosedUntilExplicitOptIn()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
        }

        [TestMethod]
        public void B3B02_B3ACompleteDefinitionSqlRemainsAvailableWithoutDispatch()
        {
            var legacySql = BuildExpectedSql(NewRequest(ColumnDdlOperation.Rename));
            var commentSql = BuildExpectedSql(NewRequest(ColumnDdlOperation.Comment));

            StringAssert.Contains(legacySql, " CHANGE COLUMN ");
            StringAssert.Contains(legacySql, " COMMENT 'original';");
            StringAssert.Contains(commentSql, " MODIFY COLUMN ");
            StringAssert.Contains(commentSql, " COMMENT 'new comment';");
        }

        [TestMethod]
        public void B3B03_ModifyCommentSqlShapeMustBeQualified()
        {
            var sql = BuildExpectedSql(NewRequest(ColumnDdlOperation.Comment));

            Assert.IsTrue
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Comment, sql
                ),
                "B3-B2 must accept a qualified MODIFY COLUMN shape; shape alone does NOT authorize execution."
            );
        }

        [TestMethod]
        public void B3B04_OldChangeCommentSqlShapeMustBeRejected()
        {
            const string legacyComment = "ALTER TABLE `demo`.`b2_table` CHANGE COLUMN `old_name` `old_name` int COMMENT 'new comment';";

            Assert.IsFalse
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Comment, legacyComment
                ),
                "The old definition-incomplete CHANGE COLUMN Comment shape must not receive the new gate."
            );
        }

        [TestMethod]
        public void B3B05_ModernRenameMustRemainDefinitionFree()
        {
            var sql = ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql(NewRequest(ColumnDdlOperation.Rename));

            Assert.IsTrue
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, sql
                )
            );
        }

        [TestMethod]
        public void B3B06_FormMustHaveDedicatedReaderReceiptPreviewProperty()
        {
            var property = typeof(EditColumnForm).GetProperty
            (
                "NonOracleF7MySqlCompleteDefinitionPreview",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property, "Only a qualified F7 form may receive this opt-in receipt.");
            Assert.AreEqual(typeof(MySqlVerifiedColumnDefinitionSnapshot), property.PropertyType);
            Assert.IsNotNull(property.GetSetMethod(true));
            Assert.IsFalse(property.GetSetMethod(true).IsPublic);
        }

        [TestMethod]
        public void B3B07_ReaderMustHaveDedicatedVerifiedCompleteDefinitionDispatch()
        {
            var signature = new[]
            {
                typeof(string),
                typeof(Guid),
                typeof(ColumnDdlOperation),
                typeof(MySqlVerifiedColumnDefinitionSnapshot),
                typeof(string).MakeByRefType(),
                typeof(bool).MakeByRefType(),
                typeof(bool).MakeByRefType()
            };

            var method = typeof(MySqlReader).GetMethod
            (
                "ExecuteCompleteColumnDdlOnVerifiedConnection",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, signature, null
            );

            Assert.IsNotNull(method, "Generic SQL dispatch cannot substitute for Reader-owned last-mile receipt revalidation.");
            Assert.AreEqual(typeof(int), method.ReturnType);
        }

        [TestMethod]
        public void B3B08_IdenticalVerifiedLegacyReceiptsAndExactSqlMayQualify()
        {
            var request = NewRequest(ColumnDdlOperation.Rename);
            var preview = NewSnapshot(SessionId, 381, "5.7.44", CompleteDefinition);
            var live = NewSnapshot(SessionId, 381, "5.7.44", CompleteDefinition);

            Assert.AreEqual(string.Empty, InvokeExpectedPolicy(request, preview, live, BuildExpectedSql(request)));
        }

        [TestMethod]
        public void B3B09_ChangedDefinitionMustReject()
        {
            var request = NewRequest(ColumnDdlOperation.Comment);
            var preview = NewSnapshot(SessionId, 381, "8.0.34", CompleteDefinition);
            var live = NewSnapshot(SessionId, 381, "8.0.34", "int unsigned NOT NULL DEFAULT 8 COMMENT 'original'");

            Assert.IsFalse(string.IsNullOrEmpty(InvokeExpectedPolicy(request, preview, live, BuildExpectedSql(request))));
        }

        [TestMethod]
        public void B3B10_ChangedPhysicalSessionMustReject()
        {
            var request = NewRequest(ColumnDdlOperation.Rename);
            var preview = NewSnapshot(SessionId, 381, "5.7.44", CompleteDefinition);
            var live = NewSnapshot(SessionId, 382, "5.7.44", CompleteDefinition);

            Assert.IsFalse(string.IsNullOrEmpty(InvokeExpectedPolicy(request, preview, live, BuildExpectedSql(request))));
        }

        [TestMethod]
        public void B3B11_ModifiedDisplaySqlMustReject()
        {
            var request = NewRequest(ColumnDdlOperation.Comment);
            var preview = NewSnapshot(SessionId, 381, "5.7.44", CompleteDefinition);
            var live = NewSnapshot(SessionId, 381, "5.7.44", CompleteDefinition);
            var tampered = BuildExpectedSql(request).Replace("new comment", "different comment");

            Assert.IsFalse(string.IsNullOrEmpty(InvokeExpectedPolicy(request, preview, live, tampered)));
        }

        [TestMethod]
        public void B3B12_ModernServerMustNeverUseLegacyChangeRename()
        {
            var request = NewRequest(ColumnDdlOperation.Rename);
            var preview = NewSnapshot(SessionId, 381, "8.0.34", CompleteDefinition);
            var live = NewSnapshot(SessionId, 381, "8.0.34", CompleteDefinition);

            Assert.IsFalse(string.IsNullOrEmpty(InvokeExpectedPolicy(request, preview, live, BuildExpectedSql(request))));
        }

        [TestMethod]
        public void B3B13_PendingTransactionMustBlockBeforeConnectionOrDispatch()
        {
            var calls = 0;

            var result = NonOracleSchemaExplorerDdlExecuteGate.TryExecuteF7
            (
                DataSourceType.MySql,
                true,
                true,
                () => { calls++; return string.Empty; },
                () => { calls++; return string.Empty; },
                () => { calls++; return SessionId; },
                _ => { calls++; return string.Empty; }
            );

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual(0, calls);
        }
    }
}
