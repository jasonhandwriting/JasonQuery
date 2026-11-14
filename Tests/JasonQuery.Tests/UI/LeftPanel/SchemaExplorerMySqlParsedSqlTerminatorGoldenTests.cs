using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4B3B2B-R1: SQL splitter may return statement.Text without a terminal semicolon.
    //This class checks SQL shape only; it never opens a database or authorizes DDL.
    [TestClass]
    [TestCategory("F7U3C4B3B2BR1")]
    public sealed class SchemaExplorerMySqlParsedSqlTerminatorGoldenTests
    {
        private const string FullDefinition = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
        private static readonly Guid ConnectionId = new Guid("16fdcd58-3494-42e9-96de-f8043809223c");

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

        private static string WithoutTerminator(string sql)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(sql));
            Assert.IsTrue(sql.EndsWith(";", StringComparison.Ordinal));

            return sql.Substring(0, sql.Length - 1);
        }

        [TestMethod]
        public void S07R101_ModifyCommentWithAndWithoutSemicolonBothQualifySqlShape()
        {
            var request = Request(ColumnDdlOperation.Comment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, FullDefinition, out var sql));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, sql));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, WithoutTerminator(sql)));
        }

        [TestMethod]
        public void S07R102_LegacyChangeRenameWithAndWithoutSemicolonBothQualifySqlShape()
        {
            var request = Request(ColumnDdlOperation.Rename);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildLegacyRename(request, FullDefinition, out var sql));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Rename, sql));
            Assert.IsTrue(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Rename, WithoutTerminator(sql)));
        }

        [TestMethod]
        public void S07R103_OldChangeColumnCommentMustStillBeRejected()
        {
            const string unsafeSql = "ALTER TABLE `demo`.`b2_table` CHANGE COLUMN `old_name` `old_name` int COMMENT 'new comment';";

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, unsafeSql));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, WithoutTerminator(unsafeSql)));
        }

        [TestMethod]
        public void S07R104_ExtraStatementsAndTrailingSqlCommentsMustStillBeRejected()
        {
            var request = Request(ColumnDdlOperation.Comment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, FullDefinition, out var sql));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, sql + " DROP TABLE `demo`.`b2_table`;"));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, sql + " -- trailing"));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, WithoutTerminator(sql) + " -- trailing"));
        }

        [TestMethod]
        public void S07R105_TerminatorToleranceMustNotGrantCrossOperationAccess()
        {
            var request = Request(ColumnDdlOperation.Comment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, FullDefinition, out var sql));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Rename, WithoutTerminator(sql)));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Drop, WithoutTerminator(sql)));
        }

        [TestMethod]
        public void S07R106_ShapeWithoutSemicolonIsNotAnExactSqlExecutePermit()
        {
            var request = Request(ColumnDdlOperation.Comment);

            Assert.IsTrue(MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment(request, FullDefinition, out var sql));

            var receipt = new MySqlVerifiedColumnDefinitionSnapshot
            (
                ConnectionId, "demo", "b2_table", "old_name", "8.0.34", FullDefinition, 381
            );

            Assert.AreEqual(string.Empty, MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, receipt, receipt, request, sql));
            Assert.IsFalse(string.IsNullOrEmpty(MySqlVerifiedColumnDefinitionExecutionPolicy.Validate(request.Operation, receipt, receipt, request, WithoutTerminator(sql))));
        }
    }
}
