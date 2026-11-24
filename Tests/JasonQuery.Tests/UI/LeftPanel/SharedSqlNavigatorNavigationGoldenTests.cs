using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F9D1")]
    public sealed class SharedSqlNavigatorNavigationGoldenTests
    {
        private const string Identity = "Query-A";
        private const string FirstSql = "SELECT '中文';";
        private const string MiddleSql = "UPDATE table_a SET id = 1;";
        private const string EditorText = FirstSql + "\r\n\r\n" + MiddleSql + "\r\n\r\n" + FirstSql;

        private static int MiddlePosition => FirstSql.Length + 5;
        private static int LastPosition => FirstSql.Length + MiddleSql.Length + 9;

        private static LeftPanelContext Context()
        {
            return new LeftPanelContext(true, LeftPanelTargetKind.QueryForm, false, false, true, true);
        }

        private static DataTable CreateSource()
        {
            var table = new DataTable();

            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Sql", typeof(string));
            table.Columns.Add("Position", typeof(int));
            table.Rows.Add("Select", FirstSql, 1);
            table.Rows.Add("Update", MiddleSql, MiddlePosition);
            table.Rows.Add("Select", FirstSql, LastPosition);

            return table;
        }

        [TestMethod]
        public void F9D101_FilteredFirstRowResolvesItsPublishedPosition()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            snapshot.Table.DefaultView.RowFilter = "[Type] = 'Update'";

            Assert.AreEqual(1, snapshot.Table.DefaultView.Count);
            Assert.AreEqual(3, source.DefaultView.Count);
            Assert.AreEqual(1, snapshot.Table.Rows[0]["Position"]);

            //The first displayed row is the second original row, not Table.Rows[0].
            var position = (int)snapshot.Table.DefaultView[0]["Position"];

            Assert.AreEqual(MiddlePosition, position);
            Assert.IsTrue(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, position, out var offset));
            Assert.AreEqual(EditorText.IndexOf(MiddleSql, StringComparison.Ordinal), offset);
        }

        [TestMethod]
        public void F9D102_IdenticalSqlInFilteredRowsKeepsDistinctPositions()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            snapshot.Table.DefaultView.RowFilter = "[Type] = 'Select'";

            var view = snapshot.Table.DefaultView;

            Assert.AreEqual(2, view.Count);
            Assert.AreEqual(view[0]["Sql"], view[1]["Sql"]);

            Assert.IsTrue(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, (int)view[0]["Position"], out var first));
            Assert.IsTrue(snapshot.TryResolvePosition(Context(), Identity, source, EditorText, (int)view[1]["Position"], out var last));
            Assert.AreEqual(0, first);
            Assert.AreEqual(EditorText.LastIndexOf(FirstSql, StringComparison.Ordinal), last);
            Assert.AreNotEqual(first, last);
        }

        [TestMethod]
        public void F9D103_FilteredPositionFromOldPublicationCannotNavigateAfterRefresh()
        {
            var source = CreateSource();
            var snapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, source, EditorText);

            snapshot.Table.DefaultView.RowFilter = "[Type] = 'Update'";

            var position = (int)snapshot.Table.DefaultView[0]["Position"];
            var refreshedSource = CreateSource();

            //Even identical SQL and offsets do not make a previous publication current.
            Assert.IsFalse(snapshot.TryResolvePosition(Context(), Identity, refreshedSource, EditorText, position, out var rejected));
            Assert.AreEqual(-1, rejected);

            var refreshedSnapshot = SharedSqlNavigatorSnapshotPolicy.Create(Identity, refreshedSource, EditorText);

            Assert.IsTrue(refreshedSnapshot.TryResolvePosition(Context(), Identity, refreshedSource, EditorText, position, out var accepted));
            Assert.AreEqual(MiddlePosition - 1, accepted);
        }
    }
}
