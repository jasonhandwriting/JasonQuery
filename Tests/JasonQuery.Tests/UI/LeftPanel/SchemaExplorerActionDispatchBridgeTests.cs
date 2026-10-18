using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SchemaExplorerActionDispatchBridgeTests
    {
        private static LeftPanelContext Context(LeftPanelTargetKind target = LeftPanelTargetKind.QueryForm, bool connected = true, bool ready = true)
        {
            return new LeftPanelContext(connected, target, true, ready, true, true);
        }

        private static SchemaExplorerSelectionInfo ObjectSelection(DataSourceType source = DataSourceType.PostgreSql, bool isGroupRow = false)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = source,
                DisplayRowIndex = 18,
                GroupLevel = isGroupRow ? 3 : -1,
                IsGroupRow = isGroupRow,
                SchemaNode = "public",
                SchemaType = "Tables",
                SchemaName = "orders",
                SchemaDbo = "dbo",
                ObjectId = "42",
                PackageSpecBody = "Spec",
                CreateDate = "created",
                ModifyDate = "modified"
            };
        }

        private static bool Dispatch(Receiver receiver, SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action = SchemaExplorerActionKind.SelectionChanged, LeftPanelContext context = null, string tab = "q1", DataSourceType source = DataSourceType.PostgreSql)
        {
            return SchemaExplorerActionDispatchBridge.TryDispatch(context ?? Context(), tab, source, receiver, selection, action);
        }

        [TestMethod]
        public void ValidQueryObjectSelection_RoutesToActiveReceiver()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(Dispatch(receiver, ObjectSelection()));
            Assert.AreEqual(1, receiver.CallCount);
        }

        [TestMethod]
        public void ValidSchemaBrowserSelection_RoutesToBrowserOnly()
        {
            var receiver = new Receiver("browser", LeftPanelTargetKind.SchemaBrowserForm);

            Assert.IsTrue(Dispatch(receiver, ObjectSelection(), context: Context(LeftPanelTargetKind.SchemaBrowserForm), tab: "browser"));
            Assert.AreEqual(1, receiver.CallCount);
        }

        [TestMethod]
        public void DisconnectedContext_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), context: Context(connected: false)));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void SchemaNotReady_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), context: Context(ready: false)));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void OtherOrNoneForm_IsNeverARoutingTarget()
        {
            var other = new Receiver("other", LeftPanelTargetKind.OtherForm);

            Assert.IsFalse(Dispatch(other, ObjectSelection(), context: Context(LeftPanelTargetKind.OtherForm), tab: "other"));
            Assert.IsFalse(Dispatch(null, ObjectSelection(), context: Context(LeftPanelTargetKind.None)));
            Assert.AreEqual(0, other.CallCount);
        }

        [TestMethod]
        public void StaleTabIdentity_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), tab: "q2"));
            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), tab: "Q1"));
            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), tab: " "));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void MismatchedReceiverKind_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.SchemaBrowserForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection()));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void MismatchedDatabaseEngine_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(DataSourceType.Oracle)));
            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), source: DataSourceType.None));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void NullContextOrReceiverOrSelection_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(SchemaExplorerActionDispatchBridge.TryDispatch(null, "q1", DataSourceType.PostgreSql, receiver, ObjectSelection(), SchemaExplorerActionKind.SelectionChanged));
            Assert.IsFalse(Dispatch(null, ObjectSelection()));
            Assert.IsFalse(Dispatch(receiver, null));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void UndefinedAction_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(), (SchemaExplorerActionKind)999));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void NonObjectGroupNode_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var selection = ObjectSelection();

            selection.Kind = SchemaExplorerSelectionKind.GroupNode;
            Assert.IsFalse(Dispatch(receiver, selection, SchemaExplorerActionKind.ContextMenuRequested));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void NoneOrInvalidSelection_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var selection = ObjectSelection();

            selection.Kind = SchemaExplorerSelectionKind.None;
            Assert.IsFalse(Dispatch(receiver, selection));
            selection.Kind = SchemaExplorerSelectionKind.Invalid;
            Assert.IsFalse(Dispatch(receiver, selection));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void MissingSemanticObjectNameOrType_DoesNotDispatch()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var selection = ObjectSelection();

            selection.SchemaName = " ";
            Assert.IsFalse(Dispatch(receiver, selection));
            selection.SchemaName = "orders";
            selection.SchemaType = null;
            Assert.IsFalse(Dispatch(receiver, selection));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void ObjectGroupDoubleClick_RemainsSourceGridNavigation()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(Dispatch(receiver, ObjectSelection(isGroupRow: true), SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(0, receiver.CallCount);
        }

        [TestMethod]
        public void ObjectGroupContextMenu_CanBeDispatched()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(Dispatch(receiver, ObjectSelection(isGroupRow: true), SchemaExplorerActionKind.ContextMenuRequested));
            Assert.IsTrue(receiver.LastSelection.IsGroupRow);
            Assert.AreEqual(-1, receiver.LastSelection.GroupLevel);
        }

        [TestMethod]
        public void ColumnInfoSelection_PreservesSemanticKind()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var selection = ObjectSelection();

            selection.Kind = SchemaExplorerSelectionKind.ColumnInfo;
            selection.IsColumnInfoRow = true;
            selection.SchemaName = "order_id";
            Assert.IsTrue(Dispatch(receiver, selection, SchemaExplorerActionKind.DoubleClick));
            Assert.AreEqual(SchemaExplorerSelectionKind.ColumnInfo, receiver.LastSelection.Kind);
            Assert.IsTrue(receiver.LastSelection.IsColumnInfoRow);
        }

        [TestMethod]
        public void PortableSelection_PreservesObjectIdentityButNotGridRow()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var source = ObjectSelection();

            Assert.IsTrue(Dispatch(receiver, source));

            var target = receiver.LastSelection;

            Assert.AreNotSame(source, target);
            Assert.AreEqual(-1, target.DisplayRowIndex);
            Assert.AreEqual(-1, target.GroupLevel);
            Assert.AreEqual(18, source.DisplayRowIndex);
            Assert.AreEqual("orders", target.SchemaName);
            Assert.AreEqual("public", target.SchemaNode);
            Assert.AreEqual("dbo", target.SchemaDbo);
            Assert.AreEqual("42", target.ObjectId);
            Assert.AreEqual("Spec", target.PackageSpecBody);
            Assert.AreEqual("created", target.CreateDate);
            Assert.AreEqual("modified", target.ModifyDate);
        }

        [TestMethod]
        public void ReceiverMutation_CannotChangeSourceSelection()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm) { Mutate = true };
            var source = ObjectSelection();

            Assert.IsTrue(Dispatch(receiver, source));
            Assert.AreEqual("orders", source.SchemaName);
            Assert.AreEqual(18, source.DisplayRowIndex);
            Assert.AreEqual("changed-by-receiver", receiver.LastSelection.SchemaName);
        }

        [TestMethod]
        public void ReceiverDecline_DoesNotBecomeSuccess()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm) { Result = false };

            Assert.IsFalse(Dispatch(receiver, ObjectSelection()));
            Assert.AreEqual(1, receiver.CallCount);
        }

        [TestMethod]
        public void ExternalObject_IsPortableWithoutLocalRow()
        {
            var receiver = new Receiver("q1", LeftPanelTargetKind.QueryForm);
            var selection = ObjectSelection();

            selection.Kind = SchemaExplorerSelectionKind.ExternalObject;
            Assert.IsTrue(Dispatch(receiver, selection));
            Assert.AreEqual(SchemaExplorerSelectionKind.ExternalObject, receiver.LastSelection.Kind);
            Assert.AreEqual(-1, receiver.LastSelection.DisplayRowIndex);
        }

        private sealed class Receiver : ISchemaExplorerActionReceiver
        {
            public Receiver(string identity, LeftPanelTargetKind kind)
            {
                TabIdentity = identity;
                TargetKind = kind;
            }

            public string TabIdentity { get; }
            public LeftPanelTargetKind TargetKind { get; }
            public bool Result { get; set; } = true;
            public bool Mutate { get; set; }
            public int CallCount { get; private set; }
            public SchemaExplorerSelectionInfo LastSelection { get; private set; }

            public bool TryHandleSchemaExplorerAction(SchemaExplorerSelectionInfo selection, SchemaExplorerActionKind action)
            {
                CallCount++;
                LastSelection = selection;

                if (Mutate)
                {
                    selection.SchemaName = "changed-by-receiver";
                }

                return Result;
            }
        }
    }
}
