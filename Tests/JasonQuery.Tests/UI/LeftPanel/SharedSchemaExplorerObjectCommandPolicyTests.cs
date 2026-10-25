using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaExplorerObjectCommandPolicyTests
    {
        private static SchemaExplorerSelectionInfo Selection(DataSourceType sourceType = DataSourceType.Oracle, string schemaType = "Tables", bool column = false)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = column ? SchemaExplorerSelectionKind.ColumnInfo : SchemaExplorerSelectionKind.SchemaObject,
                IsColumnInfoRow = column,
                SourceType = sourceType,
                SchemaType = schemaType,
                SchemaName = column ? "ID, VARCHAR2" : "ORDERS",
                SchemaNode = "sampledb",
                SchemaDbo = "dbo",
                ObjectId = "1234"
            };
        }

        private static bool Allowed(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerMenuCommand command)
        {
            return SharedSchemaExplorerObjectCommandPolicy.CanExecute(target, selection, command);
        }

        [TestMethod]
        public void QueryTable_AllThreeNonDestructiveObjectCommandsAllowed()
        {
            var s = Selection();

            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void QueryView_AllThreeNonDestructiveObjectCommandsAllowed()
        {
            var s = Selection(schemaType: "Views");

            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void BrowserTable_OnlyGenerateSqlEnabled()
        {
            var s = Selection();

            Assert.IsFalse(Allowed(LeftPanelTargetKind.SchemaBrowserForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.SchemaBrowserForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
            Assert.IsTrue(Allowed(LeftPanelTargetKind.SchemaBrowserForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void BrowserView_CanGenerateSql()
        {
            Assert.IsTrue(Allowed(LeftPanelTargetKind.SchemaBrowserForm, Selection(schemaType: "Views"), SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void Column_HasNoObjectCommands()
        {
            var s = Selection(column: true);

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void ViewColumn_HasNoObjectCommands()
        {
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, Selection(schemaType: "Views", column: true), SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void Function_QueryOnlyShowDdl()
        {
            var s = Selection(schemaType: "Functions");

            Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void DangerousEditCommandsRemainDisabled()
        {
            foreach (var command in new[] { SharedSchemaExplorerMenuCommand.CommentTable, SharedSchemaExplorerMenuCommand.DropTable,
                     SharedSchemaExplorerMenuCommand.RenameTable, SharedSchemaExplorerMenuCommand.TruncateTable,
                     SharedSchemaExplorerMenuCommand.AddColumn, SharedSchemaExplorerMenuCommand.DropColumn })
            {
                Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, Selection(), command), command.ToString());
            }
        }

        [TestMethod]
        public void CopyPasteCommandsAreNotObjectCommands()
        {
            var s = Selection();

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.CopyToClipboard));
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.PasteName));
        }

        [TestMethod]
        public void UnknownTargetDenied()
        {
            Assert.IsFalse(Allowed(LeftPanelTargetKind.OtherForm, Selection(), SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void NullSelectionDenied()
        {
            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, null, SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void GroupRowDenied()
        {
            var s = Selection();

            s.IsGroupRow = true;

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void InvalidKindDenied()
        {
            var s = Selection();

            s.Kind = SchemaExplorerSelectionKind.Invalid;

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void BlankNameDenied()
        {
            var s = Selection();

            s.SchemaName = " ";

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void PostgreSqlRequiresSchemaNode()
        {
            var s = Selection(DataSourceType.PostgreSql);

            s.SchemaNode = " ";

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.GenerateSqlStatement));
        }

        [TestMethod]
        public void MySqlRequiresSchemaNode()
        {
            var s = Selection(DataSourceType.MySql);

            s.SchemaNode = string.Empty;

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
        }

        [TestMethod]
        public void SqlServerRequiresSchemaDboAndNode()
        {
            var s = Selection(DataSourceType.SqlServer);

            s.SchemaDbo = string.Empty;

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));

            s.SchemaDbo = "dbo";
            s.SchemaNode = string.Empty;

            Assert.IsFalse(Allowed(LeftPanelTargetKind.QueryForm, s, SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod]
        public void AllFourDatabasesOfferTableGenerationWithScope()
        {
            foreach (var type in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                Assert.IsTrue(Allowed(LeftPanelTargetKind.QueryForm, Selection(type), SharedSchemaExplorerMenuCommand.GenerateSqlStatement), type.ToString());
            }
        }

        [TestMethod]
        public void SqlServerGenerateSqlRemovesSchemaPrefix()
        {
            var s = Selection(DataSourceType.SqlServer);

            s.SchemaName = "DBO.ORDERS";

            Assert.AreEqual("ORDERS", SharedSchemaExplorerObjectCommandPolicy.GetGenerateSqlObjectName(s));
        }

        [TestMethod]
        public void OtherDatabasesDoNotChangeGenerateSqlName()
        {
            var s = Selection(DataSourceType.PostgreSql);

            s.SchemaName = "public.ORDERS";

            Assert.AreEqual("public.ORDERS", SharedSchemaExplorerObjectCommandPolicy.GetGenerateSqlObjectName(s));
        }
    }
}
