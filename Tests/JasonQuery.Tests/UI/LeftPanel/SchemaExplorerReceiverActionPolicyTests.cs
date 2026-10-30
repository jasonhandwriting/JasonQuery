using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SchemaExplorerReceiverActionPolicyTests
    {
        private static SchemaExplorerSelectionInfo Selection(SchemaExplorerSelectionKind kind = SchemaExplorerSelectionKind.SchemaObject)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = kind,
                SourceType = DataSourceType.PostgreSql,
                DisplayRowIndex = -1,
                GroupLevel = -1,
                SchemaType = "Tables",
                SchemaName = "orders",
                SchemaNode = "public",
                SchemaDbo = "dbo",
                ObjectId = "42"
            };
        }

        [TestMethod]
        public void QueryDoubleClick_TableObject_IsAccepted()
        {
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(Selection(), SchemaExplorerActionKind.DoubleClick, out var text));
            Assert.AreEqual("orders", text);
        }

        [TestMethod]
        public void QueryDoubleClick_ColumnInfo_IsAccepted()
        {
            var source = Selection(SchemaExplorerSelectionKind.ColumnInfo);

            source.SchemaName = "order_id";
            source.IsColumnInfoRow = true;
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out var text));
            Assert.AreEqual("order_id", text);
        }

        [TestMethod]
        public void QueryDoubleClick_CommaSuffix_PreservesLegacyPasteText()
        {
            var source = Selection();

            source.SchemaName = "order_id, integer";
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out var text));
            Assert.AreEqual("order_id, ", text);
        }

        [TestMethod]
        public void QuerySelectionChanged_IsNotEditorPaste()
        {
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(Selection(), SchemaExplorerActionKind.SelectionChanged, out var text));
            Assert.AreEqual(string.Empty, text);
        }

        [TestMethod]
        public void QueryContextMenu_DoesNotUseLegacyGridState()
        {
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(Selection(), SchemaExplorerActionKind.ContextMenuRequested, out _));
        }

        [TestMethod]
        public void QueryGroupRowDoubleClick_RemainsExplorerNavigation()
        {
            var source = Selection();

            source.IsGroupRow = true;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out _));
        }

        [TestMethod]
        public void QueryOtherSchemaType_DoesNotPaste()
        {
            var source = Selection();

            source.SchemaType = "Views";
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out _));
        }

        [TestMethod]
        public void QuerySourceRowIndex_IsNeverTreatedAsPortable()
        {
            var source = Selection();

            source.DisplayRowIndex = 18;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out _));
        }

        [TestMethod]
        public void QueryMissingName_IsRejected()
        {
            var source = Selection();

            source.SchemaName = " ";
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(source, SchemaExplorerActionKind.DoubleClick, out _));
        }

        [TestMethod]
        public void BrowserObjectSelection_IsAccepted()
        {
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(Selection(), SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserExternalObjectSelection_IsAccepted()
        {
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(Selection(SchemaExplorerSelectionKind.ExternalObject), SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserObjectGroupSelection_IsAcceptedWithoutSourceRow()
        {
            var source = Selection();

            source.IsGroupRow = true;
            Assert.IsTrue(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(source, SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserColumnInfo_IsNotDisplayableObject()
        {
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(Selection(SchemaExplorerSelectionKind.ColumnInfo), SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserOtherActions_RemainWithExplorer()
        {
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(Selection(), SchemaExplorerActionKind.DoubleClick));
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(Selection(), SchemaExplorerActionKind.ContextMenuRequested));
        }

        [TestMethod]
        public void BrowserSourceRowIndex_IsNeverReused()
        {
            var source = Selection();

            source.DisplayRowIndex = 18;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(source, SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserMissingSemanticFields_AreRejected()
        {
            var source = Selection();

            source.SchemaType = string.Empty;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(source, SchemaExplorerActionKind.SelectionChanged));
            source.SchemaType = "Tables";
            source.SchemaName = null;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(source, SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void NullSelection_IsRejectedForBothTargets()
        {
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.TryGetQueryPasteText(null, SchemaExplorerActionKind.DoubleClick, out _));
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(null, SchemaExplorerActionKind.SelectionChanged));
        }

        [TestMethod]
        public void BrowserGroupLevel_IsNotPortable()
        {
            var source = Selection();

            source.GroupLevel = 2;
            Assert.IsFalse(SchemaExplorerReceiverActionPolicy.CanDisplayInSchemaBrowser(source, SchemaExplorerActionKind.SelectionChanged));
        }
    }
}
