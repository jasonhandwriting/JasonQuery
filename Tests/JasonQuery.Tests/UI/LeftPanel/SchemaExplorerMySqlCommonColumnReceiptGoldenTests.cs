using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //C3-B3B: effective-attribute receipt consistency, no database calls or DDL dispatch.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlCommonColumnReceiptGoldenTests
    {
        [TestMethod]
        public void B3B01_TableCollationChangeMustChangeVerifiedSignature()
        {
            var first = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature
            (
                "varchar(32)", "YES", null, "", "utf8mb4", "utf8mb4_0900_ai_ci", "utf8mb4", "utf8mb4_0900_ai_ci"
            );

            var second = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature
            (
                "varchar(32)", "YES", null, "", "utf8mb4", "utf8mb4_0900_ai_ci", "utf8mb4", "utf8mb4_bin"
            );

            Assert.AreNotEqual(first, second);
        }

        [TestMethod]
        public void B3B02_MissingAndEmptyMetadataMustNotCollide()
        {
            var missing = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature(null, "a");
            var empty = MySqlCommonColumnDefinitionProofPolicy.BuildVerifiedAttributesSignature(string.Empty, "a");

            Assert.AreNotEqual(missing, empty);
        }

        [TestMethod]
        public void B3B03_ChangedEffectiveMetadataMustBlockExecuteEvenIfSqlTextIsUnchanged()
        {
            var connectionId = Guid.NewGuid();

            const string definition = "varchar(32) DEFAULT NULL";

            var preview = new MySqlVerifiedColumnDefinitionSnapshot
            (
                connectionId, "demo", "sample", "name", "8.0.34", definition, 123, "before"
            );

            var live = new MySqlVerifiedColumnDefinitionSnapshot
            (
                connectionId, "demo", "sample", "name", "8.0.34", definition, 123, "after"
            );

            var request = new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Comment,
                SchemaDatabase = "demo",
                TableName = "sample",
                ColumnName = "name",
                Comment = "updated"
            };

            var status = MySqlVerifiedColumnDefinitionExecutionPolicy.Validate
            (
                ColumnDdlOperation.Comment, preview, live, request,
                "ALTER TABLE `demo`.`sample` MODIFY COLUMN `name` varchar(32) DEFAULT NULL COMMENT 'updated';"
            );

            Assert.AreEqual("F7U3C4B3B2_DEFINITION_OR_VERSION_CHANGED", status);
        }
    }
}
