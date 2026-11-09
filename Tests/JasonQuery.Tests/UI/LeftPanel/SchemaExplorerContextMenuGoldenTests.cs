using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SchemaExplorerContextMenuGoldenTests
    {
        private static SchemaExplorerSelectionInfo Selection(DataSourceType source = DataSourceType.Oracle, bool column = false, string type = "Tables")
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = column ? SchemaExplorerSelectionKind.ColumnInfo : SchemaExplorerSelectionKind.SchemaObject,
                SourceType = source,
                SchemaType = type,
                SchemaName = column ? "ID, VARCHAR2" : "JQ98_TABLE",
                SchemaNode = source == DataSourceType.PostgreSql ? "public" : "sampledb",
                SchemaDbo = "dbo",
                ObjectId = "123",
                IsColumnInfoRow = column,
                IsGroupRow = false
            };
        }

        private static SharedSchemaExplorerMenuCommand[] Plan(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            return SharedSchemaExplorerContextMenuPolicy.Plan(target, selection).ToArray();
        }

        [TestMethod, TestCategory("F2D")]
        public void D01_History_OracleTableShowsDdlAndTableCommands()
        {
            var menu = Plan(LeftPanelTargetKind.OtherForm, Selection());

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CommentTable);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.DropTable);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.RenameTable);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.TruncateTable);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.AddColumn);
        }

        [TestMethod, TestCategory("F2D")]
        public void D02_History_OracleColumnOffersFourColumnCommands()
        {
            var menu = Plan(LeftPanelTargetKind.OtherForm, Selection(column: true));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.AddColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CommentColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.DropColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.RenameColumn);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.DropTable);
        }

        [TestMethod, TestCategory("F2D")]
        public void D03_History_ThreeNonOracleTablesOfferAddColumn()
        {
            foreach (var source in new[] { DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var menu = Plan(LeftPanelTargetKind.OtherForm, Selection(source));

                CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.AddColumn, source.ToString());
            }
        }

        [TestMethod, TestCategory("F2D")]
        public void D04_History_CopyNameWithoutEditor()
        {
            var menu = Plan(LeftPanelTargetKind.OtherForm, Selection());

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CopyToClipboard);
        }

        [TestMethod, TestCategory("F2D")]
        public void D05_History_NoPasteIntoNonEditor()
        {
            var menu = Plan(LeftPanelTargetKind.OtherForm, Selection());

            Assert.IsTrue(menu.Length > 0);
            Assert.IsFalse(menu.Any(x => SharedSchemaExplorerPastePolicy.IsPasteCommand(x)));
        }

        [TestMethod, TestCategory("F2D")]
        public void D06_History_GenerateSqlWaitsForExplicitEditorReceiver()
        {
            var menu = Plan(LeftPanelTargetKind.OtherForm, Selection());

            Assert.IsTrue(menu.Length > 0);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
        }

        [TestMethod, TestCategory("F2D")]
        public void D07_OtherForm_ShowDdlObjectActionAllowed()
        {
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.OtherForm, Selection(), SharedSchemaExplorerMenuCommand.ShowDdl));
        }

        [TestMethod, TestCategory("F2D")]
        public void D08_OtherForm_OpenSchemaBrowserAllowedWithObjectScope()
        {
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.OtherForm, Selection(DataSourceType.MySql), SharedSchemaExplorerMenuCommand.OpenSchemaBrowser));
        }

        [TestMethod, TestCategory("F2D")]
        public void D09_OtherForm_QualifiedNonOracleTableAllowsAddColumn()
        {
            Assert.IsTrue(SharedSchemaExplorerObjectCommandPolicy.CanExecute(LeftPanelTargetKind.OtherForm, Selection(DataSourceType.MySql), SharedSchemaExplorerMenuCommand.AddColumn));
        }

        [TestMethod, TestCategory("F2D")]
        public void D10_NoneTargetNeverOffersMenu()
        {
            Assert.AreEqual(0, Plan(LeftPanelTargetKind.None, Selection()).Length);
        }

        [TestMethod, TestCategory("F2D")]
        public void D11_History_InvalidObjectNeverOffersMenu()
        {
            var selection = Selection();

            selection.Kind = SchemaExplorerSelectionKind.Invalid;

            Assert.AreEqual(0, Plan(LeftPanelTargetKind.OtherForm, selection).Length);
        }

        [TestMethod, TestCategory("F2D")]
        public void D12_ExistingQueryAndBrowserMenuConventionsRemainDistinct()
        {
            var query = Plan(LeftPanelTargetKind.QueryForm, Selection());
            var browser = Plan(LeftPanelTargetKind.SchemaBrowserForm, Selection());

            CollectionAssert.Contains(query, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(browser, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.Contains(query, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.Contains(browser, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
        }
    }
}
