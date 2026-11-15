using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4A-A: data-only Golden-first contracts. No WinForms, C1 Grid or DB connection.
    //Expected against reviewed main: A01-A03 GREEN; A04-A15 RED (filter policy not yet implemented).
    [TestClass]
    [TestCategory("F7U4A")]
    public sealed class SchemaExplorerFilterGoldenTests
    {
        private static DataTable Metadata()
        {
            var source = new DataTable("Schema");

            source.Columns.Add("SchemaObject", typeof(string));
            source.Columns.Add("SchemaNode", typeof(string));
            source.Columns.Add("SchemaType", typeof(string));
            source.Columns.Add("SchemaName", typeof(string));
            source.Columns.Add("ColumnInfo", typeof(string));
            source.Columns.Add("SchemaDbo", typeof(string));
            source.Columns.Add("ObjectID", typeof(string));

            return source;
        }

        private static void Add(DataTable source, string schema, string type, string name, string display, string objectId = "")
        {
            var row = source.NewRow();

            row["SchemaObject"] = "catalog";
            row["SchemaNode"] = schema;
            row["SchemaType"] = type;
            row["SchemaName"] = name;
            row["ColumnInfo"] = display;
            row["SchemaDbo"] = schema;
            row["ObjectID"] = objectId;

            source.Rows.Add(row);
        }

        private static DataTable TwoObjects()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "orders", "orders", "101");
            Add(source, "public", "Tables", "customers", "customers", "102");

            return source;
        }

        private static SharedSchemaMetadataSnapshot Capture(DataTable source, DataSourceType provider, bool includeColumns = false)
        {
            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(source, provider, includeColumns, 1, out snapshot));
            Assert.IsNotNull(snapshot);

            return snapshot;
        }

        //Reflection keeps the Golden RED baseline compilable before the new policy exists.
        //U4A-B contract: static bool TryApplyFilter(SharedSchemaMetadataSnapshot snapshot, string searchText).
        private static bool TryFilter(SharedSchemaMetadataSnapshot snapshot, string searchText)
        {
            var policyType = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SharedSchemaExplorerFilterPolicy", false
            );

            Assert.IsNotNull(policyType, "F7-U4A expected RED: SharedSchemaExplorerFilterPolicy not implemented yet.");

            var method = policyType.GetMethod
            (
                "TryApplyFilter",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(SharedSchemaMetadataSnapshot), typeof(string) },
                null
            );

            Assert.IsNotNull(method, "F7-U4A expected RED: TryApplyFilter(snapshot, searchText) not implemented yet.");

            return (bool)method.Invoke(null, new object[] { snapshot, searchText });
        }

        private static string NameAt(SharedSchemaMetadataSnapshot snapshot, int index)
        {
            return Convert.ToString(snapshot.Table.DefaultView[index]["SchemaName"]);
        }

        [TestMethod]
        public void A01_SnapshotUsesOwnedCopyNotGlobalData()
        {
            var original = TwoObjects();
            var snapshot = Capture(original, DataSourceType.PostgreSql);

            Assert.AreNotSame(original, snapshot.Table);
            Assert.AreEqual(2, snapshot.Table.Rows.Count);
            Assert.IsTrue(original.Columns.Contains("ColumnInfo"));
            Assert.IsFalse(original.Columns.Contains("Schema_Browser"));
            Assert.IsTrue(snapshot.Table.Columns.Contains("Schema_Browser"));
        }

        [TestMethod]
        public void A02_SnapshotCapturesAllFourProviders()
        {
            foreach (var provider in new[] { DataSourceType.Oracle, DataSourceType.PostgreSql, DataSourceType.SqlServer, DataSourceType.MySql })
            {
                var snapshot = Capture(TwoObjects(), provider, true);

                Assert.AreEqual(provider, snapshot.SourceType);

                CollectionAssert.Contains(snapshot.GroupColumns, "SchemaName");
            }
        }

        [TestMethod]
        public void A03_SeparateSnapshotsHaveIndependentDisplayFilters()
        {
            var source = TwoObjects();
            var first = Capture(source, DataSourceType.PostgreSql);
            var second = Capture(source, DataSourceType.PostgreSql);

            first.Table.DefaultView.RowFilter = "[SchemaName] = 'orders'";

            Assert.AreEqual(1, first.Table.DefaultView.Count);
            Assert.AreEqual(2, second.Table.DefaultView.Count);
            Assert.AreEqual(2, source.DefaultView.Count);
        }

        [TestMethod]
        public void A04_Oracle_ObjectPrefixSearch()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.Oracle);

            Assert.IsTrue(TryFilter(snapshot, "ord*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("orders", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A05_PostgreSql_ObjectSuffixSearch()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.PostgreSql);
            Assert.IsTrue(TryFilter(snapshot, "*ders"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("orders", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A06_SqlServer_ObjectSearchIsCaseInsensitive()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.SqlServer);

            Assert.IsTrue(TryFilter(snapshot, "CUST*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("customers", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A07_MySql_ObjectNameSearch()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.MySql);

            Assert.IsTrue(TryFilter(snapshot, "ord*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("orders", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A08_ColumnInfoEnabled_SearchByColumnNameFindsParentTable()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "orders", "order_id, INTEGER");
            Add(source, "public", "Tables", "customers", "customer_id, INTEGER");

            var snapshot = Capture(source, DataSourceType.PostgreSql, true);

            Assert.IsTrue(TryFilter(snapshot, "*order_id*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("orders", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A09_ColumnInfoEnabled_SearchByDataTypeFindsParentTable()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "payments", "amount, DECIMAL(18,2)");
            Add(source, "public", "Tables", "flags", "enabled, BOOLEAN");

            var snapshot = Capture(source, DataSourceType.MySql, true);

            Assert.IsTrue(TryFilter(snapshot, "*DECIMAL*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("payments", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A10_WildcardMatchesOrderedFragments()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "orders", "orders");
            Add(source, "public", "Tables", "order_archive", "order_archive");

            var snapshot = Capture(source, DataSourceType.Oracle);

            Assert.IsTrue(TryFilter(snapshot, "ord*ers"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("orders", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A11_ApostropheIsLiteralNotRowFilterSyntax()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "O'Connor", "O'Connor");
            Add(source, "public", "Tables", "orders", "orders");

            var snapshot = Capture(source, DataSourceType.PostgreSql);

            Assert.IsTrue(TryFilter(snapshot, "O'Con*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual("O'Connor", NameAt(snapshot, 0));
        }

        [TestMethod]
        public void A12_StarResetsFilterToFullView()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.PostgreSql);

            Assert.IsTrue(TryFilter(snapshot, "ord*"));
            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.IsTrue(TryFilter(snapshot, "*"));
            Assert.AreEqual(2, snapshot.Table.DefaultView.Count);
        }

        [TestMethod]
        public void A13_ValidSearchWithNoMatchesReturnsEmptyView()
        {
            var snapshot = Capture(TwoObjects(), DataSourceType.SqlServer);

            Assert.IsTrue(TryFilter(snapshot, "no_matching_object*"));
            Assert.AreEqual(0, snapshot.Table.DefaultView.Count);
            Assert.AreEqual(2, snapshot.Table.Rows.Count);
        }

        [TestMethod]
        public void A14_DuplicateObjectNamesKeepDistinctDataRowIdentity()
        {
            var source = Metadata();

            Add(source, "public", "Tables", "orders", "orders", "101");
            Add(source, "archive", "Tables", "orders", "orders", "202");
            Add(source, "public", "Tables", "customers", "customers", "303");

            var snapshot = Capture(source, DataSourceType.SqlServer);

            Assert.IsTrue(TryFilter(snapshot, "orders"));
            Assert.AreEqual(2, snapshot.Table.DefaultView.Count);

            var left = snapshot.Table.DefaultView[0].Row;
            var right = snapshot.Table.DefaultView[1].Row;

            Assert.AreNotSame(left, right);
            Assert.AreNotEqual(Convert.ToString(left["SchemaNode"]), Convert.ToString(right["SchemaNode"]));
            Assert.AreNotEqual(Convert.ToString(left["ObjectID"]), Convert.ToString(right["ObjectID"]));
        }

        [TestMethod]
        public void A15_InvalidSnapshotFailsClosedAndDoesNotMutateOriginal()
        {
            var source = TwoObjects();
            var snapshot = Capture(source, DataSourceType.MySql);

            snapshot.Table.DefaultView.RowFilter = "[SchemaName] = 'orders'";

            var before = snapshot.Table.DefaultView.RowFilter;

            snapshot.Table.Columns.Remove("Schema_Browser");

            Assert.IsFalse(TryFilter(snapshot, "ord*"));
            Assert.AreEqual(before, snapshot.Table.DefaultView.RowFilter);
            Assert.IsFalse(TryFilter(null, "ord*"));
            Assert.AreEqual(2, source.Rows.Count);
            Assert.AreEqual(2, source.DefaultView.Count);
            Assert.IsTrue(source.Columns.Contains("ColumnInfo"));
        }
    }
}
