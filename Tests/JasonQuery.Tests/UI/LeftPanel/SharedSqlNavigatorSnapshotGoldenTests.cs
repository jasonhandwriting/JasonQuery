using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F9B")]
    public sealed class SharedSqlNavigatorSnapshotGoldenTests
    {
        private const string FirstSql = "SELECT '中文';";
        private const string SecondSql = "SELECT 2;";
        private const string EditorText = FirstSql + "\r\n\r\n" + SecondSql;
        private const string Identity = "Query-A";

        private static int SecondPosition => FirstSql.Length + 4 + 1;

        private static DataTable CreateSource()
        {
            var table = new DataTable();

            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Sql", typeof(string));
            table.Columns.Add("Position", typeof(int));
            table.Rows.Add("Select", FirstSql, 1);
            table.Rows.Add("Select", SecondSql, SecondPosition);

            return table;
        }

        private static LeftPanelContext Context(LeftPanelTargetKind target = LeftPanelTargetKind.QueryForm, bool connected = true, bool hasSql = true)
        {
            return new LeftPanelContext(connected, target, false, false, true, hasSql);
        }

        [TestMethod]
        public void F9B01_PreservesCanonicalSchemaOrderAndOneBasedPositions()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            Assert.AreEqual(Identity, snapshot.SourceIdentity);
            Assert.AreEqual(3, snapshot.Table.Columns.Count);
            Assert.AreEqual("Type", snapshot.Table.Columns[0].ColumnName);
            Assert.AreEqual("Sql", snapshot.Table.Columns[1].ColumnName);
            Assert.AreEqual("Position", snapshot.Table.Columns[2].ColumnName);
            Assert.AreEqual(typeof(int), snapshot.Table.Columns[2].DataType);
            Assert.AreEqual(2, snapshot.Table.Rows.Count);
            Assert.AreEqual("Select", snapshot.Table.Rows[0]["Type"]);
            Assert.AreEqual(FirstSql, snapshot.Table.Rows[0]["Sql"]);
            Assert.AreEqual(1, snapshot.Table.Rows[0]["Position"]);
            Assert.AreEqual(SecondSql, snapshot.Table.Rows[1]["Sql"]);
            Assert.AreEqual(SecondPosition, snapshot.Table.Rows[1]["Position"]);
        }

        [TestMethod]
        public void F9B02_DisplayTableIsIsolatedFromQueryFormWorkingTable()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            Assert.AreNotSame(source, snapshot.Table);

            source.Rows[0]["Sql"] = "Changed in source";

            Assert.AreEqual(FirstSql, snapshot.Table.Rows[0]["Sql"]);

            snapshot.Table.Rows[0]["Sql"] = "Changed in display";

            Assert.AreEqual("Changed in source", source.Rows[0]["Sql"]);
        }

        [TestMethod]
        public void F9B03_NonQueryDisconnectedOrEmptyContextRejectsSnapshot()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            Assert.IsTrue(snapshot.IsCurrent(Context(), Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(null, Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(LeftPanelTargetKind.None), Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(LeftPanelTargetKind.OtherForm), Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(LeftPanelTargetKind.SchemaBrowserForm), Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(connected: false), Identity, source, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(hasSql: false), Identity, source, EditorText));
        }

        [TestMethod]
        public void F9B04_RequiresExactIdentityAndCurrentPublication()
        {
            var sourceA = CreateSource();
            var sourceB = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, sourceA, EditorText);

            Assert.IsTrue(snapshot.IsCurrent(Context(), Identity, sourceA, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(), "Query-B", sourceB, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(), "Query-B", sourceA, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(), "query-a", sourceA, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, sourceB, EditorText));
            Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, null, EditorText));

            var refreshed = SharedSqlNavigatorSnapshotPolicy.Create(Identity, sourceB, EditorText);

            Assert.IsTrue(refreshed.IsCurrent(Context(), Identity, sourceB, EditorText));
        }

        [TestMethod]
        public void F9B05_EditorChangeInvalidatesSnapshotBeforeNextRefresh()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, source, EditorText + "\r\n"));
            Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, source, EditorText.Replace("中文", "繁中")));
            Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, source, string.Empty));
        }

        [TestMethod]
        public void F9B06_RequiresPublishedPositionAndConvertsToZeroBasedCharacters()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            Assert.IsTrue(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, 1, out var first));
            Assert.AreEqual(0, first);
            Assert.IsTrue(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, SecondPosition, out var second));
            Assert.AreEqual(EditorText.IndexOf(SecondSql, System.StringComparison.Ordinal), second);

            foreach (var invalid in new[] { -1, 0, 2, EditorText.Length + 1 })
            {
                Assert.IsFalse(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, invalid, out var rejected));
                Assert.AreEqual(-1, rejected);
            }

            Assert.IsFalse(snapshot.TryResolvePosition(Context(), "Query-B", source, EditorText, SecondPosition, out _));
            Assert.IsFalse(snapshot.TryResolvePosition(Context(), Identity, source, EditorText + " ", SecondPosition, out _));
            Assert.IsFalse(snapshot.TryResolvePosition(Context(), Identity, CreateSource(), EditorText, SecondPosition, out _));

            snapshot.Table.Rows.Add("Select", "Not published", 2);

            Assert.IsFalse(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, 2, out _));
        }

        [TestMethod]
        public void F9B07_NullMalformedAndInvalidRowsAreNotNavigable()
        {
            var malformed = new DataTable();

            malformed.Columns.Add("Type");
            malformed.Columns.Add("Sql");
            malformed.Columns.Add("Position", typeof(string));

            var empty = CreateSource();

            empty.Clear();

            var invalid = CreateSource();

            invalid.Clear();
            invalid.Rows.Add("Select", FirstSql, 0);
            invalid.Rows.Add("Select", FirstSql, EditorText.Length + 1);
            invalid.Rows.Add("Select", " ", 1);
            invalid.Rows.Add("Select", FirstSql, System.DBNull.Value);
            invalid.Rows.Add("Select", FirstSql, 1);
            invalid.AcceptChanges();
            invalid.Rows[4].Delete();

            foreach (var source in new[] { null, malformed, empty, invalid })
            {
                var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

                Assert.AreEqual(3, snapshot.Table.Columns.Count);
                Assert.AreEqual(typeof(int), snapshot.Table.Columns["Position"].DataType);
                Assert.AreEqual(0, snapshot.Table.Rows.Count);
                Assert.IsFalse(snapshot.IsCurrent(Context(), Identity, source, EditorText));
            }

            Assert.AreEqual(0, SharedSqlNavigatorSnapshotPolicy.Create("", CreateSource(), EditorText).Table.Rows.Count);
            Assert.AreEqual(0, SharedSqlNavigatorSnapshotPolicy.Create(Identity, CreateSource(), null).Table.Rows.Count);
        }
    }
}
