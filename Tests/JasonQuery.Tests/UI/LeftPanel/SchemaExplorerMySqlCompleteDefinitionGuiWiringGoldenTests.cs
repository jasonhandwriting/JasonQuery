using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C4B3B2B: opt-in GUI eligibility checks. No DB connection or DDL execution.
    [TestClass]
    [TestCategory("F7U3C4B3B2B")]
    public sealed class SchemaExplorerMySqlCompleteDefinitionGuiWiringGoldenTests
    {
        private const string CompleteDefinition = "int unsigned NOT NULL DEFAULT 7 COMMENT 'original'";
        private static readonly Guid ConnectionId = new Guid("0eaad980-d21d-40d7-a3ce-bc369d1fc126");

        private static MySqlVerifiedColumnDefinitionSnapshot Receipt(string version, string definition = CompleteDefinition, Guid? connectionId = null, long serverConnectionId = 381, string databaseName = "demo")
        {
            return new MySqlVerifiedColumnDefinitionSnapshot
            (
                connectionId ?? ConnectionId, databaseName, "b2_table", "old_name", version, definition, serverConnectionId
            );
        }

        [TestMethod]
        public void B3B2B01_MissingReceiptCannotEnableCommentOrLegacyRename()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, null, false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Rename, null, false));
        }

        [TestMethod]
        public void B3B2B02_LegacyServerMayEnableOnlyVerifiedCompleteDefinition()
        {
            var receipt = Receipt("5.7.44");

            Assert.IsTrue(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Rename, receipt, false));
            Assert.IsTrue(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, receipt, false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Drop, receipt, false));
        }

        [TestMethod]
        public void B3B2B03_ModernServerCannotUseLegacyChangeRename()
        {
            var receipt = Receipt("8.0.34");

            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Rename, receipt, false));
            Assert.IsTrue(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, receipt, false));
        }

        [TestMethod]
        public void B3B2B04_LegacyRenameNeverSharesModernRenamePreviewPermit()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Rename, Receipt("5.7.44"), true));
        }

        [TestMethod]
        public void B3B2B05_UnknownVersionCannotEnableCompleteDefinitionPath()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt(string.Empty), false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Rename, Receipt(string.Empty), false));
        }

        [TestMethod]
        public void B3B2B06_UnverifiedConnectionCannotEnablePreview()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt("8.0.34", connectionId: Guid.Empty), false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt("8.0.34", serverConnectionId: 0), false));
        }

        [TestMethod]
        public void B3B2B07_IncompleteOrUnsafeDefinitionCannotEnablePreview()
        {
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt("8.0.34", "int NOT NULL"), false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt("8.0.34", "int NOT NULL DEFAULT 0; DROP TABLE x"), false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.MySql, EditColumnMode.Comment, Receipt("8.0.34", CompleteDefinition, databaseName: string.Empty), false));
        }

        [TestMethod]
        public void B3B2B08_OtherProvidersMustNotUseMySqlReceipt()
        {
            var receipt = Receipt("5.7.44");

            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.PostgreSql, EditColumnMode.Comment, receipt, false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.SqlServer, EditColumnMode.Rename, receipt, false));
            Assert.IsFalse(EditColumnForm.CanEnableVerifiedMySqlCompleteDefinition(DataSourceType.Oracle, EditColumnMode.Comment, receipt, false));
        }

        [TestMethod]
        public void B3B2B09_StaticLegacyAndCommentExecutionPermitRemainsClosed()
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Rename));
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(DataSourceType.MySql, EditColumnMode.Comment));
        }
    }
}
