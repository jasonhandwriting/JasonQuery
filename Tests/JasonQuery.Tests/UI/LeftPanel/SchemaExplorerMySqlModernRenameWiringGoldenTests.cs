using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Database.Providers.Readers;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4A3: Modern MySQL Rename can be previewed and dispatched only after two independent capability checks.
    //No database is connected, no WinForms window is opened and no DDL is executed by these tests.
    [TestClass]
    [TestCategory("F7U3C4A3")]
    public sealed class SchemaExplorerMySqlModernRenameWiringGoldenTests
    {
        [TestMethod]
        [DataRow("8.0.3")]
        [DataRow("8.0.36-log")]
        [DataRow("8.4.0")]
        [DataRow("9.0.1")]
        [DataRow("10.0.0")]
        public void M601_ModernMySqlVersionMayQualifyRenamePreview(string version)
        {
            Assert.IsTrue(EditColumnForm.CanEnableVerifiedMySqlModernRename(DataSourceType.MySql, EditColumnMode.Rename, version));
        }

        [TestMethod]
        [DataRow("5.6.51")]
        [DataRow("5.7.44")]
        [DataRow("8.0.2")]
        [DataRow("10.6.0-MariaDB")]
        [DataRow("5.5.5-10.11.2-MariaDB")]
        [DataRow("unknown")]
        public void M602_LegacyMariaDbAndUnknownCannotEnableModernRename(string version)
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlModernRename(DataSourceType.MySql, EditColumnMode.Rename, version));
        }

        [TestMethod]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.None)]
        public void M603_OtherProvidersCannotUseMySqlModernRename(DataSourceType provider)
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlModernRename(provider, EditColumnMode.Rename, "10.0.0"));
        }

        [TestMethod]
        [DataRow(EditColumnMode.Comment)]
        [DataRow(EditColumnMode.Drop)]
        [DataRow(EditColumnMode.Add)]
        [DataRow(EditColumnMode.None)]
        public void M604_OtherColumnOperationsNeverInheritModernRename(EditColumnMode mode)
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlModernRename(DataSourceType.MySql, mode, "8.4.0"));
        }

        [TestMethod]
        public void M605_GenericMySqlRenameRemainsDeniedWithoutVerifiedVersion()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
            Assert.IsTrue(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Drop));
        }

        [TestMethod]
        public void M606_FormCapturesModernPreviewOnlyAsASeparateBoolean()
        {
            var property = typeof(EditColumnForm).GetProperty
            (
                "NonOracleF7MySqlModernRenamePreview", BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(bool), property.PropertyType);
            Assert.IsTrue(property.GetSetMethod(true).IsAssembly);
        }

        [TestMethod]
        public void M607_ModernRenameSqlDoesNotRestateColumnAttributes()
        {
            var sql = ColumnDdlSqlBuilder.BuildMySqlRenameColumnSql
            (
                new ColumnDdlRequest
                {
                    DataSourceType = DataSourceType.MySql,
                    SchemaDatabase = "sakila",
                    TableName = "jq_modern_test",
                    ColumnName = "old_column",
                    NewColumnName = "new_column",
                    Operation = ColumnDdlOperation.Rename,
                    ColumnType = string.Empty
                }
            );

            Assert.AreEqual("ALTER TABLE `sakila`.`jq_modern_test` RENAME COLUMN `old_column` TO `new_column`;", sql);
            Assert.IsFalse(sql.Contains("CHANGE COLUMN"));
        }

        [TestMethod]
        public void M608_ExactModernRenamePassesColumnIntentGate()
        {
            const string sql = "ALTER TABLE `sakila`.`jq_modern_test` RENAME COLUMN `old_column` TO `new_column`;";

            Assert.AreEqual(string.Empty, NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, true,
                "old_column", "old_column", sql, sql
            ));
        }

        [TestMethod]
        public void M609_ChangedPreviewStillFailsClosed()
        {
            const string sql = "ALTER TABLE `sakila`.`jq_modern_test` RENAME COLUMN `old_column` TO `new_column`;";

            Assert.AreEqual("F7U3C2B_SQL_CHANGED", NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent
            (
                DataSourceType.MySql, ColumnDdlOperation.Rename, true,
                "old_column", "old_column", sql, sql.Replace("new_column", "other_column")
            ));
        }

        [TestMethod]
        public void M610_MySqlReaderRetainsSameConnectionCapabilityLookupAndColumnDispatch()
        {
            Assert.IsNotNull(typeof(MySqlReader).GetMethod
            (
                "TryReadServerVersionOnVerifiedConnection", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(Guid), typeof(string).MakeByRefType(), typeof(string).MakeByRefType() }, null
            ));

            Assert.IsNotNull(typeof(MySqlReader).GetMethod
            (
                "ExecuteColumnDdlOnVerifiedConnection", BindingFlags.Instance | BindingFlags.NonPublic,
                null, new[] { typeof(string), typeof(Guid), typeof(ColumnDdlOperation), typeof(string).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType() }, null
            ));
        }

        [TestMethod]
        public void M611_LegacyChangeStillRequiresFullDefinition()
        {
            var sql = ColumnDdlSqlBuilder.Build(new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                SchemaDatabase = "sakila",
                TableName = "jq_modern_test",
                ColumnName = "old_column",
                NewColumnName = "new_column",
                Operation = ColumnDdlOperation.Rename,
                ColumnType = string.Empty
            });

            Assert.AreEqual(string.Empty, sql);

            Assert.AreEqual("F7U3C2B_MYSQL_DEFINITION_UNVERIFIED", NonOracleSchemaExplorerDdlExecuteGate.ValidateMySqlColumnDefinitionSafety
            (
                ColumnDdlOperation.Rename, false, null, null
            ));
        }

        [TestMethod]
        public void M612_MultiStatementOrDifferentOperationCannotReuseModernRenamePermit()
        {
            const string sql = "ALTER TABLE `sakila`.`jq_modern_test` RENAME COLUMN `old_column` TO `new_column`;";

            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Comment, sql));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Drop, sql));
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql(DataSourceType.MySql, ColumnDdlOperation.Rename, sql + " COMMIT;"));
        }
    }
}
