using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4A2: MySQL version-qualified modern rename is still preview-only until last-mile integration.
    //These tests do not connect to any database, open forms, or dispatch DDL.
    [TestClass]
    [TestCategory("F7U3C4A2")]
    public sealed class SchemaExplorerMySqlModernRenameExecutionGoldenTests
    {
        private const string ModernSql = "ALTER TABLE `sakila`.`jq_column_ddl_test` RENAME COLUMN `old_name` TO `new_name`;";
        private const string ModernSqlWithoutTerminator = "ALTER TABLE `sakila`.`jq_column_ddl_test` RENAME COLUMN `old_name` TO `new_name`";

        [TestMethod]
        public void M501_ModernRenameSqlMustBeAllowedForMySqlRenameOnly()
        {
            Assert.IsTrue
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, ModernSql
                )
            );
        }

        [TestMethod]
        public void M502_SingleParsedModernRenameWithoutFinalSemicolonMustBeAllowed()
        {
            //The Reader's single-statement splitter may return statement.Text without the final semicolon.
            Assert.IsTrue
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, ModernSqlWithoutTerminator
                )
            );
        }

        [TestMethod]
        public void M503_QualifiedModernRenameIntentMustAcceptExactRebuiltSql()
        {
            Assert.AreEqual
            (
                string.Empty,
                NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                (
                    DataSourceType.MySql,
                    ColumnDdlOperation.Rename,
                    true,
                    "old_name",
                    "old_name",
                    ModernSql,
                    ModernSql
                )
            );
        }

        [TestMethod]
        public void M504_ModernRenameCannotMasqueradeAsCommentOrDrop()
        {
            Assert.IsFalse
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Comment, ModernSql
                )
            );

            Assert.IsFalse
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Drop, ModernSql
                )
            );
        }

        [TestMethod]
        public void M505_ModernRenameAllowlistRejectsAdditionalStatements()
        {
            var sql = ModernSql + " DROP TABLE `sakila`.`jq_column_ddl_test`;";

            Assert.IsFalse
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, sql
                )
            );
        }

        [TestMethod]
        public void M506_ModernRenameIntentRejectsModifiedPreviewOrTarget()
        {
            var changedSql = ModernSql.Replace("`new_name`", "`other_name`");

            Assert.AreEqual
            (
                "F7U3C2B_SQL_CHANGED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, true,
                    "old_name", "old_name", ModernSql, changedSql
                )
            );

            Assert.AreEqual
            (
                "F7U3C2B_COLUMN_IDENTITY_CHANGED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, true,
                    "old_name", "another_column", ModernSql, ModernSql
                )
            );
        }

        [TestMethod]
        public void M507_VersionPolicyMustNeverTreatLegacyOrMariaDbAsModernMySql()
        {
            Assert.AreEqual
            (
                MySqlColumnRenameSyntax.RenameColumn,
                MySqlColumnDdlVersionPolicy.SelectRenameSyntax("8.0.3")
            );

            Assert.AreEqual
            (
                MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition,
                MySqlColumnDdlVersionPolicy.SelectRenameSyntax("5.7.44")
            );

            Assert.AreEqual
            (
                MySqlColumnRenameSyntax.Unknown,
                MySqlColumnDdlVersionPolicy.SelectRenameSyntax("10.6.0-MariaDB")
            );
        }

        [TestMethod]
        public void M508_ReaderVersionLookupFailsClosedWithoutPhysicalConnectionIdentity()
        {
            var reader = new MySqlReader();

            Assert.AreEqual(Guid.Empty, reader.CurrentOpenConnectionId);

            Assert.IsFalse
            (
                reader.TryReadServerVersionOnVerifiedConnection
                (
                    Guid.Empty, out var serverVersion, out var errorMessage
                )
            );

            Assert.AreEqual(string.Empty, serverVersion);
            Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage));
        }

        [TestMethod]
        public void M509_UnqualifiedRawColumnNeverAuthorizesModernRename()
        {
            Assert.AreEqual
            (
                "F7U3C2B_COLUMN_IDENTITY_UNQUALIFIED",
                NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, false,
                    "old_name", "old_name", ModernSql, ModernSql
                )
            );
        }

        [TestMethod]
        public void M510_LegacyChangeColumnAllowlistRemainsAvailableForFutureVerifiedDefinition()
        {
            var legacySql = "ALTER TABLE `sakila`.`jq_column_ddl_test` "
                + "CHANGE COLUMN `old_name` `new_name` int NOT NULL;";

            Assert.IsTrue
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, legacySql
                )
            );
        }

        [TestMethod]
        public void M511_ModernRenameMustRejectAnIdenticalNewColumnName()
        {
            var noChangeSql = "ALTER TABLE `sakila`.`jq_column_ddl_test` RENAME COLUMN `old_name` TO `old_name`;";

            Assert.IsFalse
            (
                NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
                (
                    DataSourceType.MySql, ColumnDdlOperation.Rename, noChangeSql
                )
            );
        }
    }
}
