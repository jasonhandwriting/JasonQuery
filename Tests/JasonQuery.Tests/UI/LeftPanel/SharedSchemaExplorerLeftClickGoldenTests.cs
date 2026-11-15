using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4A-C2A: Golden-first contracts for left-click selection routing.
    //No C1 controls, database connections, or DDL execution are constructed here.
    [TestClass]
    [TestCategory("F7U4AC2")]
    public sealed class SharedSchemaExplorerLeftClickGoldenTests
    {
        private static SharedSchemaExplorerInputKind LeftClick()
        {
            SharedSchemaExplorerInputKind input;

            //Parse at runtime so the Golden RED baseline compiles before the new LeftClick enum member has been implemented.
            Assert.IsTrue
            (
                Enum.TryParse("LeftClick", out input),
                "F7-U4A-C2: SharedSchemaExplorerInputKind.LeftClick is not yet implemented."
            );

            return input;
        }

        private static SchemaExplorerSelectionInfo Object(string type = "Tables", bool group = false)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = DataSourceType.PostgreSql,
                SchemaType = type,
                SchemaName = type == "Views" ? "v_orders" : "orders",
                SchemaNode = "public",
                DisplayRowIndex = 7,
                GroupLevel = group ? 2 : -1,
                IsGroupRow = group,
                IsColumnInfoRow = false
            };
        }

        private static bool Route(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerInputKind input, out SchemaExplorerActionKind action)
        {
            return SharedSchemaExplorerInteractionPolicy.TryGetAction(target, selection, input, out action);
        }

        [TestMethod]
        public void C2A01_LeftClickInputKindIsDefined()
        {
            Assert.AreEqual("LeftClick", LeftClick().ToString());
        }

        [TestMethod]
        public void C2A02_SchemaBrowserTableLeftClickDispatchesSelectionChanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), LeftClick(), out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void C2A03_SchemaBrowserViewLeftClickDispatchesSelectionChanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object("Views"), LeftClick(), out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void C2A04_SchemaBrowserTableObjectGroupCanDispatch()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(group: true), LeftClick(), out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void C2A05_QueryFormLeftClickDoesNotPasteOrDispatch()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, Object(), LeftClick(), out action));
        }

        [TestMethod]
        public void C2A06_OtherFormLeftClickDoesNotDispatch()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.OtherForm, Object(), LeftClick(), out action));
        }

        [TestMethod]
        public void C2A07_ColumnInfoLeftClickDoesNotLoadObject()
        {
            var column = Object();

            column.Kind = SchemaExplorerSelectionKind.ColumnInfo;
            column.IsColumnInfoRow = true;
            column.SchemaName = "order_id, integer";

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, column, LeftClick(), out action));
        }

        [TestMethod]
        public void C2A08_InvalidSelectionLeftClickDoesNotDispatch()
        {
            var invalid = Object();

            invalid.Kind = SchemaExplorerSelectionKind.Invalid;

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, invalid, LeftClick(), out action));
        }

        [TestMethod]
        public void C2A09_CategoryGroupWithoutObjectNameIsNotDispatched()
        {
            var category = Object(group: true);

            category.SchemaName = string.Empty;

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, category, LeftClick(), out action));
        }

        [TestMethod]
        public void C2A10_QueryDoubleClickContractRemainsUnchanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.QueryForm, Object(), SharedSchemaExplorerInputKind.DoubleClick, out action));
            Assert.AreEqual(SchemaExplorerActionKind.DoubleClick, action);
        }

        [TestMethod]
        public void C2A11_SchemaBrowserEnterContractRemainsUnchanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), SharedSchemaExplorerInputKind.Enter, out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void C2A12_RightClickNeverTriggersSchemaBrowserNavigation()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), SharedSchemaExplorerInputKind.RightClick, out action));
        }
    }
}
