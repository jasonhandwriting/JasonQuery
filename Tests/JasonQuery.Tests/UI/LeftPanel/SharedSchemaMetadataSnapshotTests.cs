using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedSchemaMetadataSnapshotTests
    {
        private static DataTable Metadata(bool includeName = true, bool includeNode = true, bool includeExtra = true)
        {
            var table = new DataTable("Schema");

            table.Columns.Add("SchemaObject", typeof(string));

            if (includeNode)
            {
                table.Columns.Add("SchemaNode", typeof(string));
            }

            table.Columns.Add("SchemaType", typeof(string));

            if (includeName)
            {
                table.Columns.Add("SchemaName", typeof(string));
            }

            if (includeExtra)
            {
                table.Columns.Add("ColumnInfo", typeof(string));
                table.Columns.Add("CreateDate", typeof(string));
                table.Columns.Add("ModifyDate", typeof(string));
                table.Columns.Add("SchemaDbo", typeof(string));
                table.Columns.Add("ObjectID", typeof(string));
            }

            var row = table.NewRow();

            row["SchemaObject"] = "catalog";

            if (includeNode)
            {
                row["SchemaNode"] = "public";
            }

            row["SchemaType"] = "Tables";

            if (includeName)
            {
                row["SchemaName"] = "orders";
            }

            if (includeExtra)
            {
                row["ColumnInfo"] = "order_id";
            }

            table.Rows.Add(row);

            return table;
        }

        private static SharedSchemaMetadataSnapshot Capture(DataSourceType type = DataSourceType.PostgreSql, bool columns = false, DataTable table = null, int revision = 1)
        {
            SharedSchemaMetadataSnapshot snapshot;
            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(table ?? Metadata(), type, columns, revision, out snapshot));

            return snapshot;
        }

        private static SharedLeftPanelHostState Active(int revision = 1)
        {
            var state = SharedLeftPanelHostStatePolicy.Empty;

            for (var i = 0; i < revision; i++)
            {
                if (i == 0)
                {
                    state = SharedLeftPanelHostStatePolicy.Refresh(state, true, true, "q1", LeftPanelTargetKind.QueryForm);
                }

                if (i > 0)
                {
                    state = SharedLeftPanelHostStatePolicy.Refresh(state, false, false, null, LeftPanelTargetKind.None);
                    state = SharedLeftPanelHostStatePolicy.Refresh(state, true, true, "q1", LeftPanelTargetKind.QueryForm);
                }
            }

            return state;
        }

        [TestMethod]
        public void NullSource_IsRejected()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(null, DataSourceType.Oracle, false, 1, out result)); Assert.IsNull(result);
        }

        [TestMethod]
        public void NoneSourceType_IsRejected()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(), DataSourceType.None, false, 1, out result));
        }

        [TestMethod]
        public void UndefinedSourceType_IsRejected()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(), (DataSourceType)9999, false, 1, out result));
        }

        [TestMethod]
        public void ZeroConnectionRevision_IsRejected()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(), DataSourceType.Oracle, false, 0, out result));
        }

        [TestMethod]
        public void NegativeConnectionRevision_IsRejected()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(), DataSourceType.Oracle, false, -1, out result));
        }

        [TestMethod]
        public void MissingSchemaObject_IsRejected()
        {
            var table = Metadata();

            table.Columns.Remove("SchemaObject");
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(table, DataSourceType.PostgreSql, false, 1, out result));
        }

        [TestMethod]
        public void MissingSchemaType_IsRejected()
        {
            var table = Metadata();

            table.Columns.Remove("SchemaType");
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(table, DataSourceType.Oracle, false, 1, out result));
        }

        [TestMethod]
        public void MissingNode_IsRejectedForPostgreSql()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(includeNode: false), DataSourceType.PostgreSql, false, 1, out result));
        }

        [TestMethod]
        public void MissingNode_IsAcceptedForOracle()
        {
            Assert.AreEqual(2, Capture(DataSourceType.Oracle, table: Metadata(includeNode: false)).GroupColumns.Length);
        }

        [TestMethod]
        public void MissingName_IsAcceptedWithoutColumnGrouping()
        {
            Assert.AreEqual(3, Capture(table: Metadata(includeName: false)).GroupColumns.Length);
        }

        [TestMethod]
        public void MissingName_IsRejectedWithColumnGrouping()
        {
            SharedSchemaMetadataSnapshot result;
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.TryCapture(Metadata(includeName: false), DataSourceType.PostgreSql, true, 1, out result));
        }

        [TestMethod]
        public void Oracle_GroupPlan()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaType" }, Capture(DataSourceType.Oracle).GroupColumns);
        }

        [TestMethod]
        public void PostgreSql_GroupPlan()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaNode", "SchemaType" }, Capture(DataSourceType.PostgreSql).GroupColumns);
        }

        [TestMethod]
        public void SqlServer_GroupPlan()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaNode", "SchemaType" }, Capture(DataSourceType.SqlServer).GroupColumns);
        }

        [TestMethod]
        public void MySql_GroupPlan()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaNode", "SchemaType" }, Capture(DataSourceType.MySql).GroupColumns);
        }

        [TestMethod]
        public void Oracle_ColumnGroupIsFourthNotThird()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaType", "SchemaName" }, Capture(DataSourceType.Oracle, true).GroupColumns);
        }

        [TestMethod]
        public void PostgreSql_ColumnGroupIsFourth()
        {
            CollectionAssert.AreEqual(new[] { "SchemaObject", "SchemaNode", "SchemaType", "SchemaName" }, Capture(DataSourceType.PostgreSql, true).GroupColumns);
        }

        [TestMethod]
        public void Snapshot_ClonesDataTable()
        {
            var t = Metadata();
            var snapshot = Capture(table: t);

            Assert.AreNotSame(t, snapshot.Table);
            Assert.AreEqual(t.Rows.Count, snapshot.Table.Rows.Count);
        }

        [TestMethod]
        public void Snapshot_OriginalColumnsUnchanged()
        {
            var t = Metadata();
            var snapshot = Capture(table: t);

            Assert.IsTrue(t.Columns.Contains("ColumnInfo"));
            Assert.IsFalse(t.Columns.Contains("Schema_Browser"));
            Assert.IsTrue(snapshot.Table.Columns.Contains("Schema_Browser"));
        }

        [TestMethod]
        public void Snapshot_CopyAliasNoConflict()
        {
            var t = Metadata();

            t.Columns.Add("Schema_Browser", typeof(string));

            var snapshot = Capture(table: t);

            Assert.IsTrue(snapshot.Table.Columns.Contains("ColumnInfo"));
            Assert.IsTrue(snapshot.Table.Columns.Contains("Schema_Browser"));
        }

        [TestMethod]
        public void Snapshot_SourceRowsUnaffectedByCopyMutation()
        {
            var t = Metadata();
            var snapshot = Capture(table: t);

            snapshot.Table.Rows[0]["SchemaType"] = "Views";
            Assert.AreEqual("Tables", t.Rows[0]["SchemaType"]);
        }

        [TestMethod]
        public void Snapshot_OriginalTableNameUnchanged()
        {
            var t = Metadata();

            Capture(table: t);
            Assert.AreEqual("Schema", t.TableName);
        }

        [TestMethod]
        public void Snapshot_GroupColumnsCannotBeMutated()
        {
            var snap = Capture();
            var group = snap.GroupColumns;

            group[0] = "corrupt";
            Assert.AreEqual("SchemaObject", snap.GroupColumns[0]);
        }

        [TestMethod]
        public void Snapshot_HiddenColumnsCannotBeMutated()
        {
            var snap = Capture(DataSourceType.SqlServer);
            var hidden = snap.HiddenColumns;

            hidden[0] = "corrupt";
            Assert.AreEqual("SchemaDbo", snap.HiddenColumns[0]);
        }

        [TestMethod]
        public void SqlServer_HiddenColumns()
        {
            CollectionAssert.AreEqual(new[] { "SchemaDbo", "ObjectID", "CreateDate", "ModifyDate" }, Capture(DataSourceType.SqlServer).HiddenColumns);
        }

        [TestMethod]
        public void MySql_HiddenColumns()
        {
            CollectionAssert.AreEqual(new[] { "CreateDate", "ModifyDate" }, Capture(DataSourceType.MySql).HiddenColumns);
        }

        [TestMethod]
        public void PostgreSql_HasNoForcedHiddenColumns()
        {
            Assert.AreEqual(0, Capture(DataSourceType.PostgreSql).HiddenColumns.Length);
        }

        [TestMethod]
        public void Oracle_HasNoForcedHiddenColumns()
        {
            Assert.AreEqual(0, Capture(DataSourceType.Oracle).HiddenColumns.Length);
        }

        [TestMethod]
        public void Snapshot_KeepsSourceType()
        {
            Assert.AreEqual(DataSourceType.MySql, Capture(DataSourceType.MySql).SourceType);
        }

        [TestMethod]
        public void Snapshot_KeepsConnectionRevision()
        {
            Assert.AreEqual(9, Capture(revision: 9).ConnectionRevision);
        }

        [TestMethod]
        public void CompatibleConnection_CanBind()
        {
            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind(Active(), Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void NullHost_DoesNotBind()
        {
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(null, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void NullSnapshot_DoesNotBind()
        {
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(Active(), null, DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void DisconnectedHost_DoesNotBind()
        {
            var state = SharedLeftPanelHostStatePolicy.Empty;

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(state, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void ClosedHost_DoesNotBind()
        {
            var state = SharedLeftPanelHostStatePolicy.MarkDisposed(Active());

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(state, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void WrongDatabaseEngine_DoesNotBind()
        {
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(Active(), Capture(), DataSourceType.Oracle));
        }

        [TestMethod]
        public void NoneCurrentDatabase_DoesNotBind()
        {
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(Active(), Capture(), DataSourceType.None));
        }

        [TestMethod]
        public void WrongConnectionRevision_DoesNotBind()
        {
            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(Active(), Capture(revision: 10), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void TabSwitchSameConnection_CanBind()
        {
            var s = SharedLeftPanelHostStatePolicy.Refresh(Active(), true, true, "q2", LeftPanelTargetKind.QueryForm);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind(s, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void NoActiveTab_PreservesConnectionScopedMetadata()
        {
            var s = SharedLeftPanelHostStatePolicy.Refresh(Active(), true, false, null, LeftPanelTargetKind.None);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind(s, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void NonEditorTab_PreservesConnectionScopedMetadata()
        {
            var s = SharedLeftPanelHostStatePolicy.Refresh(Active(), true, true, "options", LeftPanelTargetKind.OtherForm);

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.CanBind(s, Capture(), DataSourceType.PostgreSql));
        }

        [TestMethod]
        public void ReconnectOldSnapshot_DoesNotBind()
        {
            var current = Active(2);

            Assert.IsFalse(SharedSchemaMetadataSnapshotPolicy.CanBind(current, Capture(), DataSourceType.PostgreSql));
        }
    }
}
