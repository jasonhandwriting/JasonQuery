using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaExplorerContextMenuPolicyTests
    {
        private static SchemaExplorerSelectionInfo Selection(string schemaType = "Tables", bool column = false, DataSourceType sourceType = DataSourceType.Oracle)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = column ? SchemaExplorerSelectionKind.ColumnInfo : SchemaExplorerSelectionKind.SchemaObject,
                IsColumnInfoRow = column,
                IsGroupRow = false,
                SourceType = sourceType,
                SchemaType = schemaType,
                SchemaName = column ? "ID, VARCHAR2" : "ORDERS",
                DisplayRowIndex = 12,
                GroupLevel = 3
            };
        }

        private static SharedSchemaExplorerMenuCommand[] Menu(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection)
        {
            return SharedSchemaExplorerContextMenuPolicy.Plan(target, selection).ToArray();
        }

        [TestMethod]
        public void QueryTableObject_RetainsLegacyCommandsInOrder()
        {
            CollectionAssert.AreEqual(new[]
            {
                SharedSchemaExplorerMenuCommand.ShowDdl,
                SharedSchemaExplorerMenuCommand.OpenSchemaBrowser,
                SharedSchemaExplorerMenuCommand.GenerateSqlStatement,
                SharedSchemaExplorerMenuCommand.CommentTable,
                SharedSchemaExplorerMenuCommand.DropTable,
                SharedSchemaExplorerMenuCommand.RenameTable,
                SharedSchemaExplorerMenuCommand.TruncateTable,
                SharedSchemaExplorerMenuCommand.CopyToClipboard,
                SharedSchemaExplorerMenuCommand.PasteSelectAll,
                SharedSchemaExplorerMenuCommand.PasteName,
                SharedSchemaExplorerMenuCommand.PasteComma,
                SharedSchemaExplorerMenuCommand.PasteCommaNewLine,
                SharedSchemaExplorerMenuCommand.PasteNewLine
            }, Menu(LeftPanelTargetKind.QueryForm, Selection()));
        }

        [TestMethod]
        public void QueryViewObject_KeepsSelectAndGenerator_ButNoTableEdits()
        {
            var menu = Menu(LeftPanelTargetKind.QueryForm, Selection("Views"));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteSelectAll);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.DropTable);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.RenameColumn);
        }

        [TestMethod]
        public void TableColumn_HasExactlyFourStructuralEditCommands()
        {
            var menu = Menu(LeftPanelTargetKind.QueryForm, Selection("Tables", true));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.AddColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CommentColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.DropColumn);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.RenameColumn);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.PasteSelectAll);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.DropTable);
        }

        [TestMethod]
        public void ViewColumn_HasCopyAndPasteButNoStructuralEdits()
        {
            var menu = Menu(LeftPanelTargetKind.QueryForm, Selection("Views", true));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CopyToClipboard);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteName);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.AddColumn);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.DropColumn);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.PasteSelectAll);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
        }

        [TestMethod]
        public void BrowserTableObject_DoesNotOfferQueryOnlyDdlOrOpenBrowser()
        {
            var menu = Menu(LeftPanelTargetKind.SchemaBrowserForm, Selection());

            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.OpenSchemaBrowser);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CommentTable);
        }

        [TestMethod]
        public void BrowserViewColumn_HasNoEditColumnActions()
        {
            var menu = Menu(LeftPanelTargetKind.SchemaBrowserForm, Selection("Views", true));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CopyToClipboard);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.RenameColumn);
        }

        [TestMethod]
        public void ColumnInfoFlag_IsHonoredEvenWhenKindIsSchemaObject()
        {
            var selection = Selection("Views");

            selection.IsColumnInfoRow = true;

            var menu = Menu(LeftPanelTargetKind.QueryForm, selection);

            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
        }

        [TestMethod]
        public void LegacySuffixCommands_StayUntilL2C35()
        {
            var menu = Menu(LeftPanelTargetKind.QueryForm, Selection("Views", true));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteComma);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteCommaNewLine);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteNewLine);
        }

        [TestMethod]
        public void OtherSchemaObject_IsCopyableWithoutTableEdits()
        {
            var menu = Menu(LeftPanelTargetKind.QueryForm, Selection("Functions"));

            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.ShowDdl);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.CopyToClipboard);
            CollectionAssert.Contains(menu, SharedSchemaExplorerMenuCommand.PasteName);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.GenerateSqlStatement);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.DropTable);
            CollectionAssert.DoesNotContain(menu, SharedSchemaExplorerMenuCommand.PasteComma);
        }

        [TestMethod]
        public void FourSupportedDatabaseEngines_ShareTableViewSafety()
        {
            foreach (var sourceType in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var table = Menu(LeftPanelTargetKind.QueryForm, Selection("Tables", true, sourceType));
                var view = Menu(LeftPanelTargetKind.QueryForm, Selection("Views", true, sourceType));

                CollectionAssert.Contains(table, SharedSchemaExplorerMenuCommand.DropColumn);
                CollectionAssert.DoesNotContain(view, SharedSchemaExplorerMenuCommand.DropColumn);
            }
        }

        [TestMethod]
        public void UnknownDatabaseOrTarget_DoesNotOfferMenu()
        {
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.QueryForm, Selection("Tables", false, DataSourceType.None)).Length);
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.OtherForm, Selection()).Length);
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.None, Selection()).Length);
        }

        [TestMethod]
        public void InvalidBlankNullSelections_AreDenied()
        {
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.QueryForm, null).Length);

            var blank = Selection();

            blank.SchemaName = "  ";
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.QueryForm, blank).Length);

            var invalid = Selection();

            invalid.Kind = SchemaExplorerSelectionKind.Invalid;
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.QueryForm, invalid).Length);
        }

        [TestMethod]
        public void GroupRowMisclassifiedAsColumn_IsDenied()
        {
            var selection = Selection("Tables", true);

            selection.IsGroupRow = true;
            Assert.AreEqual(0, Menu(LeftPanelTargetKind.QueryForm, selection).Length);
        }

        [TestMethod]
        public void CopyColumnName_StripsLegacyDatatypeSuffix()
        {
            Assert.IsTrue(SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(Selection("Tables", true), out var text));
            Assert.AreEqual("ID", text);
        }

        [TestMethod]
        public void CopyObjectName_DoesNotChangeObjectName()
        {
            Assert.IsTrue(SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(Selection(), out var text));
            Assert.AreEqual("ORDERS", text);
        }

        [TestMethod]
        public void CopyWithoutValidName_IsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(null, out _));

            var selection = Selection();

            selection.SchemaName = " ";
            Assert.IsFalse(SharedSchemaExplorerContextMenuPolicy.TryGetCopyText(selection, out _));
        }
    }
}
