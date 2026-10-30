using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaExplorerInteractionPolicyTests
    {
        private static SchemaExplorerSelectionInfo Column()
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.ColumnInfo,
                SourceType = DataSourceType.PostgreSql,
                SchemaType = "Tables",
                SchemaName = "ID, integer",
                IsColumnInfoRow = true,
                DisplayRowIndex = 7,
                GroupLevel = 2
            };
        }

        private static SchemaExplorerSelectionInfo Object()
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = DataSourceType.PostgreSql,
                SchemaType = "Tables",
                SchemaName = "SampleTable",
                DisplayRowIndex = 11,
                GroupLevel = 3
            };
        }

        private static bool Route(LeftPanelTargetKind target, SchemaExplorerSelectionInfo selection, SharedSchemaExplorerInputKind input, out SchemaExplorerActionKind action)
        {
            return SharedSchemaExplorerInteractionPolicy.TryGetAction(target, selection, input, out action);
        }

        [TestMethod]
        public void QueryDoubleClickColumnProducesDoubleClickAction()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.QueryForm, Column(), SharedSchemaExplorerInputKind.DoubleClick, out action));
            Assert.AreEqual(SchemaExplorerActionKind.DoubleClick, action);
        }

        [TestMethod]
        public void QueryEnterColumnProducesDoubleClickAction()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.QueryForm, Column(), SharedSchemaExplorerInputKind.Enter, out action));
            Assert.AreEqual(SchemaExplorerActionKind.DoubleClick, action);
        }

        [TestMethod]
        public void QueryRightClickDoesNotInvokePaste()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, Column(), SharedSchemaExplorerInputKind.RightClick, out action));
        }

        [TestMethod]
        public void QueryGroupDoubleClickIsNotDispatched()
        {
            var selection = Object();

            selection.IsGroupRow = true;

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void QueryNonTableDoubleClickIsNotDispatched()
        {
            var selection = Column();

            selection.SchemaType = "Views";

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void QueryObjectDoubleClickFollowsExistingReceiverPolicy()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.QueryForm, Object(), SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void SchemaBrowserDoubleClickObjectProducesSelectionChanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), SharedSchemaExplorerInputKind.DoubleClick, out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void SchemaBrowserEnterObjectProducesSelectionChanged()
        {
            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), SharedSchemaExplorerInputKind.Enter, out action));
            Assert.AreEqual(SchemaExplorerActionKind.SelectionChanged, action);
        }

        [TestMethod]
        public void SchemaBrowserColumnRowIsRejected()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, Column(), SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void SchemaBrowserObjectGroupAllowedAtReceiverBoundary()
        {
            var selection = Object();

            selection.IsGroupRow = true;

            SchemaExplorerActionKind action;

            Assert.IsTrue(Route(LeftPanelTargetKind.SchemaBrowserForm, selection, SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void SchemaBrowserRightClickDoesNotAutoNavigate()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.SchemaBrowserForm, Object(), SharedSchemaExplorerInputKind.RightClick, out action));
        }

        [TestMethod]
        public void OtherTabRejectsDoubleClick()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.OtherForm, Object(), SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void MissingTargetRejectsEnter()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.None, Object(), SharedSchemaExplorerInputKind.Enter, out action));
        }

        [TestMethod]
        public void NullSelectionIsRejected()
        {
            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, null, SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void InvalidSelectionIsRejected()
        {
            var selection = Object();

            selection.Kind = SchemaExplorerSelectionKind.Invalid;

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerInputKind.DoubleClick, out action));
        }

        [TestMethod]
        public void NoneSelectionIsRejected()
        {
            var selection = Object();

            selection.Kind = SchemaExplorerSelectionKind.None;

            SchemaExplorerActionKind action;

            Assert.IsFalse(Route(LeftPanelTargetKind.QueryForm, selection, SharedSchemaExplorerInputKind.Enter, out action));
        }

        [TestMethod]
        public void CopyNameForObjectIsAllowed()
        {
            Assert.IsTrue(SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(Object()));
        }

        [TestMethod]
        public void CopyNameForColumnIsAllowed()
        {
            Assert.IsTrue(SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(Column()));
        }

        [TestMethod]
        public void CopyNameForNullSelectionIsDenied()
        {
            Assert.IsFalse(SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(null));
        }

        [TestMethod]
        public void CopyNameForInvalidSelectionIsDenied()
        {
            var selection = Object();

            selection.Kind = SchemaExplorerSelectionKind.Invalid;
            Assert.IsFalse(SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(selection));
        }

        [TestMethod]
        public void CopyNameForBlankObjectIsDenied()
        {
            var selection = Object();

            selection.SchemaName = "  ";
            Assert.IsFalse(SharedSchemaExplorerInteractionPolicy.CanCopyDisplayName(selection));
        }
    }
}
