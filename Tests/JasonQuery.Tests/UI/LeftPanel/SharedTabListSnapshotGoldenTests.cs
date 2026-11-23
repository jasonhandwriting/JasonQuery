using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F8A2")]
    public sealed class SharedTabListSnapshotGoldenTests
    {
        private static DataTable CreateSource()
        {
            var table = new DataTable();

            table.Columns.Add("Tab", typeof(string));
            table.Columns.Add("AccessibleDescription", typeof(string));

            return table;
        }

        [TestMethod]
        public void F8G01_PreservesMdiTabOrderAndAllFormKinds()
        {
            var source = CreateSource();

            source.Rows.Add("SQL Editor 2", "query-2");
            source.Rows.Add("Schema Browser", "schema-1");
            source.Rows.Add("Options", "options-1");
            source.Rows.Add("SQL Editor 1", "query-1");

            var snapshot = SharedTabListSnapshotPolicy.Create(source);

            Assert.AreEqual(4, snapshot.Rows.Count);
            Assert.AreEqual("SQL Editor 2", snapshot.Rows[0]["Tab"]);
            Assert.AreEqual("Schema Browser", snapshot.Rows[1]["Tab"]);
            Assert.AreEqual("Options", snapshot.Rows[2]["Tab"]);
            Assert.AreEqual("SQL Editor 1", snapshot.Rows[3]["Tab"]);
        }

        [TestMethod]
        public void F8G02_PreservesExactIdentity_NotDisplayTitle()
        {
            var source = CreateSource();

            source.Rows.Add("SQL Editor", "Query-A");

            var snapshot = SharedTabListSnapshotPolicy.Create(source);

            Assert.AreEqual("Query-A", snapshot.Rows[0]["AccessibleDescription"]);
            Assert.AreEqual(0, WorkspaceTabNavigationPolicy.FindTargetIndex(new[] { "Query-A" }, "Query-A"));
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(new[] { "Query-A" }, "SQL Editor"));
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(new[] { "Query-A" }, "query-a"));
        }

        [TestMethod]
        public void F8G03_IsIsolatedFromLegacyWorkingTable()
        {
            var source = CreateSource();

            source.Rows.Add("Original title", "query-1");

            var snapshot = SharedTabListSnapshotPolicy.Create(source);

            Assert.AreNotSame(source, snapshot);

            source.Rows[0]["Tab"] = "Changed in old source";

            Assert.AreEqual("Original title", snapshot.Rows[0]["Tab"]);

            snapshot.Rows[0]["Tab"] = "Changed in shared snapshot";

            Assert.AreEqual("Changed in old source", source.Rows[0]["Tab"]);
        }

        [TestMethod]
        public void F8G04_NullOrMalformedSource_ReturnsEmptyCanonicalSchema()
        {
            var missing = SharedTabListSnapshotPolicy.Create(null);
            var malformed = new DataTable();

            malformed.Columns.Add("Tab");
            malformed.Rows.Add("Not usable");

            var invalid = SharedTabListSnapshotPolicy.Create(malformed);

            Assert.AreEqual(0, missing.Rows.Count);
            Assert.AreEqual(0, invalid.Rows.Count);
            Assert.IsTrue(missing.Columns.Contains("Tab"));
            Assert.IsTrue(missing.Columns.Contains("AccessibleDescription"));
            Assert.IsTrue(invalid.Columns.Contains("Tab"));
            Assert.IsTrue(invalid.Columns.Contains("AccessibleDescription"));
        }
    }
}
