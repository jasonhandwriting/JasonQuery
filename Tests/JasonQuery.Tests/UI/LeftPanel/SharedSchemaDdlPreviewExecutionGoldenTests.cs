using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2CDDL")]
    public sealed class SharedSchemaDdlPreviewExecutionGoldenTests
    {
        private static readonly Guid ConnectionId = new Guid("F7C30000-0000-4000-8000-000000000001");
        private static readonly Guid LoadId = new Guid("F7C30000-0000-4000-8000-000000000002");

        private static SchemaExplorerSelectionInfo Table()
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = DataSourceType.Oracle,
                SchemaType = "Tables",
                SchemaName = "JQ98_B2_C_ADD",
                OracleSelectedOwner = "JQ98XX",
                HasQualifiedOracleSelectedOwner = true,
                DisplayRowIndex = 4
            };
        }

        private static SchemaExplorerSelectionInfo Column()
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                IsColumnInfoRow = true,
                SourceType = DataSourceType.Oracle,
                SchemaType = "Tables",
                SchemaName = "NOTE",
                OracleSelectedOwner = "JQ98XX",
                HasQualifiedOracleSelectedOwner = true,
                OracleMetadataTableName = "JQ98_B2_C_ADD",
                OracleMetadataColumnName = "NOTE",
                HasQualifiedOracleColumnIdentity = true,
                DisplayRowIndex = 5
            };
        }

        private static bool Open(SchemaExplorerSelectionInfo row, SharedSchemaExplorerMenuCommand command, bool cached = true)
        {
            return SharedSchemaDdlInteractionPolicy.CanOpenGenerator(LeftPanelTargetKind.QueryForm, row, command, cached);
        }

        private static SharedOracleMetadataValidationContext ExecutionContext()
        {
            var source = new DataTable();
            source.Columns.Add("OracleMetadataOwner", typeof(string));
            source.Columns.Add("OracleMetadataTableName", typeof(string));
            source.Columns.Add("OracleMetadataColumnName", typeof(string));
            source.Rows.Add("JQ98XX", "JQ98_B2_C_ADD", "NOTE");

            return new SharedOracleMetadataValidationContext
            {
                IsCurrentLogicalSnapshot = true,
                IsCurrentMenuAndTab = true,
                IsNewDeliberateActionClick = true,
                IsOriginalMenuReplay = false,
                HasPendingOrUnknownTransaction = false,
                IsVerifiedLiveOracleSession = true,
                CurrentConnectionRevision = 1,
                CurrentConnectionId = ConnectionId,
                CurrentVerifiedSessionUser = "JQ98XX",
                CurrentVerifiedDatabaseName = "XEPDB1",
                CurrentVerifiedContainerName = "XEPDB1",
                ExpectedSessionUser = "JQ98XX",
                ExpectedDatabaseName = "XEPDB1",
                ExpectedContainerName = "XEPDB1",
                CandidateSchemaSource = source,
                Provenance = new SharedOracleMetadataProvenance
                {
                    CapturedSourceTable = source,
                    SourceType = DataSourceType.Oracle,
                    ConnectionRevision = 1,
                    ConnectionId = ConnectionId,
                    VerifiedSessionUser = "JQ98XX",
                    VerifiedDatabaseName = "XEPDB1",
                    VerifiedContainerName = "XEPDB1",
                    CompletedLoadId = LoadId,
                    CaptureKind = SharedOracleMetadataCaptureKind.AuthoritativeCompletedLoad,
                    LoadCompletedSuccessfully = true
                },
                ExactRawTableOwnerAndNameReResolved = true,
                ExactRawColumnOwnerTableAndNameReResolved = true
            };
        }

        private static bool Execute(SharedOracleMetadataValidationContext context, bool isColumn = false, string displayed = "DROP TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\"", string rebuilt = "DROP TABLE \"JQ98XX\".\"JQ98_B2_C_ADD\"")
        {
            return SharedSchemaDdlInteractionPolicy.CanExecuteDirectly(context, isColumn, displayed, rebuilt);
        }

        [TestMethod]
        public void D01_NullSelectionCannotOpenGenerator()
        {
            Assert.IsFalse(Open(null, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod]
        public void D02_NonOracleSelectionCannotUseOracleGeneratorGate()
        {
            var row = Table();

            row.SourceType = DataSourceType.PostgreSql;

            Assert.IsFalse(Open(row, SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod]
        public void D03_InvalidLogicalCacheCannotOpenGenerator()
        {
            Assert.IsFalse(Open(Table(), SharedSchemaExplorerMenuCommand.DropTable, false));
        }

        [TestMethod]
        public void D04_CopyCommandIsNotADdlGeneratorAction()
        {
            Assert.IsFalse(Open(Table(), SharedSchemaExplorerMenuCommand.CopyToClipboard));
        }

        [TestMethod]
        public void D05_OfflineTableDropMayOpenSqlGenerator()
        {
            Assert.IsTrue(Open(Table(), SharedSchemaExplorerMenuCommand.DropTable));
        }

        [TestMethod]
        public void D06_OfflineColumnRenameMayOpenSqlGenerator()
        {
            Assert.IsTrue(Open(Column(), SharedSchemaExplorerMenuCommand.RenameColumn));
        }

        [TestMethod]
        public void D07_OfflineTableCommentMayOpenPreviewWithoutQuery()
        {
            //The pure policy grants only entry; the form must be checked separately to ensure Comment preview makes no database call without explicit refresh.
            Assert.IsTrue(Open(Table(), SharedSchemaExplorerMenuCommand.CommentTable));
        }

        [TestMethod]
        public void D08_IncompleteColumnIdentityCannotOpenGenerator()
        {
            var row = Column();

            row.OracleMetadataColumnName = string.Empty;

            Assert.IsFalse(Open(row, SharedSchemaExplorerMenuCommand.DropColumn));
        }

        [TestMethod]
        public void D09_OfflineGeneratedSqlMayBeCopied()
        {
            Assert.IsTrue(SharedSchemaDdlInteractionPolicy.CanCopyGeneratedSql("DROP TABLE \"JQ98XX\".\"X\"", false));
        }

        [TestMethod]
        public void D10_BlankSqlIsNotACompletedSqlPreview()
        {
            Assert.IsFalse(SharedSchemaDdlInteractionPolicy.CanCopyGeneratedSql("   ", false));
        }

        [TestMethod]
        public void D11_DisconnectedReaderDoesNotJustifyExecute()
        {
            var context = ExecutionContext();

            context.IsVerifiedLiveOracleSession = false;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D12_FreshVerifiedDirectExecuteMayProceed()
        {
            Assert.IsTrue(Execute(ExecutionContext()));
        }

        [TestMethod]
        public void D13_MissingExactTargetRequalificationBlocksExecute()
        {
            var context = ExecutionContext();

            context.ExactRawTableOwnerAndNameReResolved = false;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D14_DatabaseMismatchBlocksExecute()
        {
            var context = ExecutionContext();

            context.ExpectedDatabaseName = "XE_OTHER";

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D15_ContainerMismatchBlocksExecute()
        {
            var context = ExecutionContext();

            context.ExpectedContainerName = "OTHER_PDB";

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D16_StaleOrPreviewOnlyMetadataCannotAuthorizeExecute()
        {
            var context = ExecutionContext();

            context.Provenance.CaptureKind = SharedOracleMetadataCaptureKind.PreviewRebind;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D17_ChangedSqlPreviewBlocksExecute()
        {
            Assert.IsFalse(Execute(ExecutionContext(), false, "DROP TABLE WRONG", "DROP TABLE RIGHT"));
        }

        [TestMethod]
        public void D18_NoNewExplicitClickBlocksExecute()
        {
            var context = ExecutionContext();

            context.IsNewDeliberateActionClick = false;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D19_PendingOrUnknownTransactionBlocksFormExecute()
        {
            var context = ExecutionContext();

            context.HasPendingOrUnknownTransaction = true;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D20_ReplayedOldMenuCannotAuthorizeExecute()
        {
            var context = ExecutionContext();

            context.IsOriginalMenuReplay = true;

            Assert.IsFalse(Execute(context));
        }

        [TestMethod]
        public void D21_OnlineGeneratedSqlMayAlsoBeCopied()
        {
            Assert.IsTrue(SharedSchemaDdlInteractionPolicy.CanCopyGeneratedSql("COMMENT ON TABLE X IS 'ok'", true));
        }
    }
}
