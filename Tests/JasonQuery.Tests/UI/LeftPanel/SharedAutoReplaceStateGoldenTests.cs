using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F6B")]
    public sealed class SharedAutoReplaceStateGoldenTests
    {
        private static DataTable CreateSource()
        {
            var table = new DataTable();

            table.Columns.Add(SharedAutoReplaceState.KeywordColumn, typeof(string));
            table.Columns.Add(SharedAutoReplaceState.ReplacementColumn, typeof(string));

            return table;
        }

        [TestMethod]
        public void F6B01_DisabledDoesNotLoadOrCreateWorkingTable()
        {
            var state = new SharedAutoReplaceState();
            var loads = 0;

            Assert.IsFalse(state.EnsureLoaded(false, () => { loads++; return CreateSource(); }));

            state.PublishEdits(false);

            Assert.AreEqual(0, loads);
            Assert.IsNull(state.WorkingTable);
            Assert.AreEqual(0, state.GetMap(false).Count);
        }

        [TestMethod]
        public void F6B02_LoadOnceAndKeepTemporaryTableIndependentOfPermanentSource()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "select * from");

            var state = new SharedAutoReplaceState();
            var loads = 0;

            Func<DataTable> loader = () => { loads++; return source; };

            Assert.IsTrue(state.EnsureLoaded(true, loader));

            var working = state.WorkingTable;

            Assert.AreNotSame(source, working);
            Assert.AreEqual(11, working.Rows.Count);
            Assert.AreEqual(2, working.Columns.Count);

            working.Rows[0][1] = "select * from employee";
            state.PublishEdits(true);

            Assert.IsTrue(state.EnsureLoaded(true, loader));
            Assert.AreSame(working, state.WorkingTable);
            Assert.AreEqual(1, loads);
            Assert.AreEqual("select * from employee", state.GetMap(true)["sf"]);
            Assert.AreEqual("select * from", source.Rows[0][1]);
        }

        [TestMethod]
        public void F6B03_IncompleteAndBlankRowsAreIgnoredWithoutWarnings()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "select * from");
            source.Rows.Add("missing", string.Empty);
            source.Rows.Add(string.Empty, "value");
            source.Rows.Add("   ", "value");
            source.Rows.Add("blank", " \r\n\t");
            source.Rows.Add(DBNull.Value, "value");
            source.Rows.Add("null", DBNull.Value);

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            Assert.AreEqual(1, state.GetMap(true).Count);
            Assert.AreEqual("select * from", state.GetMap(true)["sf"]);
        }

        [TestMethod]
        public void F6B04_CaseInsensitiveLastValidDuplicateWinsAndTextIsPreserved()
        {
            var source = CreateSource();
            var replacement = "select *\r\nfrom employee\r\nwhere ^";

            source.Rows.Add("sf", "old");
            source.Rows.Add("SF", replacement);
            source.Rows.Add("sf", string.Empty);

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            Assert.AreEqual(1, state.GetMap(true).Count);
            Assert.AreEqual(replacement, state.GetMap(true)["sF"]);
        }

        [TestMethod]
        public void F6B05_PublishedEditsAreSharedAndDoNotKeepDeletedOrRenamedKeys()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "old");
            source.Rows.Add("del", "delete me");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var oldMap = state.GetMap(true);

            state.WorkingTable.AcceptChanges();
            state.WorkingTable.Rows[0][0] = "new";
            state.WorkingTable.Rows[0][1] = "new value";
            state.WorkingTable.Rows[1].Delete();
            state.WorkingTable.Rows[2][0] = "added";
            state.WorkingTable.Rows[2][1] = "added value";
            state.PublishEdits(true);

            var consumerA = state.GetMap(true);
            var consumerB = state.GetMap(true);

            Assert.AreSame(consumerA, consumerB);
            Assert.AreNotSame(oldMap, consumerA);
            Assert.IsFalse(consumerA.ContainsKey("sf"));
            Assert.IsFalse(consumerA.ContainsKey("del"));
            Assert.AreEqual("new value", consumerA["NEW"]);
            Assert.AreEqual("added value", consumerB["added"]);
            Assert.AreEqual("old", oldMap["sf"]);
            Assert.IsTrue(consumerA.IsReadOnly);
        }

        [TestMethod]
        public void F6B06_UncommittedCellEditDoesNotBecomePublishedRule()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "old");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var row = state.WorkingTable.Rows[0];

            row.BeginEdit();
            row[1] = "pending";
            state.PublishEdits(true);

            Assert.AreEqual("old", state.GetMap(true)["sf"]);

            row.CancelEdit();
            state.PublishEdits(true);

            Assert.AreEqual("old", state.GetMap(true)["sf"]);

            row.BeginEdit();
            row[1] = "committed";
            row.EndEdit();
            state.PublishEdits(true);

            Assert.AreEqual("committed", state.GetMap(true)["sf"]);
        }

        [TestMethod]
        public void F6B07_DisableSuppressesRulesWithoutDiscardingTemporaryEdits()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "old");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var working = state.WorkingTable;

            working.Rows[0][1] = "temporary";
            state.PublishEdits(true);

            var map = state.GetMap(true);

            Assert.IsFalse(state.EnsureLoaded(false, null));

            state.PublishEdits(false);

            Assert.AreEqual(0, state.GetMap(false).Count);
            Assert.IsTrue(state.EnsureLoaded(true, null));
            Assert.AreSame(working, state.WorkingTable);
            Assert.AreSame(map, state.GetMap(true));
            Assert.AreEqual("temporary", state.GetMap(true)["sf"]);
        }

        [TestMethod]
        public void F6B08_FailedInitialLoadDoesNotPublishAndCanRetry()
        {
            var state = new SharedAutoReplaceState();

            Assert.ThrowsExactly<InvalidOperationException>(() => state.EnsureLoaded(true, () => null));
            Assert.IsNull(state.WorkingTable);
            Assert.AreEqual(0, state.GetMap(true).Count);

            var source = CreateSource();

            source.Rows.Add("sf", "select * from");

            Assert.IsTrue(state.EnsureLoaded(true, () => source));
            Assert.AreEqual("select * from", state.GetMap(true)["sf"]);
        }

        [TestMethod]
        public void F6D101_UnchangedPermanentRulesKeepTemporaryEdits()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "original");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var working = state.WorkingTable;

            working.Rows[0][1] = "temporary";
            state.PublishEdits(true);

            Assert.IsTrue(state.RefreshPermanentRules(true, () => source.Copy()));
            Assert.AreSame(working, state.WorkingTable);
            Assert.AreEqual("temporary", state.GetMap(true)["sf"]);
        }

        [TestMethod]
        public void F6D102_ChangedPermanentRulesReplaceWorkingTableAndMap()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "original");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var previous = state.WorkingTable;

            previous.Rows[1][0] = "tmp";
            previous.Rows[1][1] = "temporary";
            state.PublishEdits(true);
            source.Rows[0][1] = "permanent change";

            Assert.IsTrue(state.RefreshPermanentRules(true, () => source));
            Assert.AreNotSame(previous, state.WorkingTable);
            Assert.AreEqual(11, state.WorkingTable.Rows.Count);
            Assert.AreEqual("permanent change", state.GetMap(true)["sf"]);
            Assert.IsFalse(state.GetMap(true).ContainsKey("tmp"));
        }

        [TestMethod]
        public void F6D103_DisabledRefreshDoesNotLoadAndReenableDetectsPermanentChange()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "original");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var working = state.WorkingTable;

            source.Rows.Clear();
            source.Rows.Add("scf", "select count(*) from");

            var calls = 0;

            Assert.IsFalse(state.RefreshPermanentRules(false, () => { calls++; return source; }));
            Assert.AreEqual(0, calls);
            Assert.AreSame(working, state.WorkingTable);
            Assert.AreEqual(0, state.GetMap(false).Count);

            Assert.IsTrue(state.RefreshPermanentRules(true, () => source));
            Assert.IsFalse(state.GetMap(true).ContainsKey("sf"));
            Assert.AreEqual("select count(*) from", state.GetMap(true)["scf"]);
        }

        [TestMethod]
        public void F6D104_FailedPermanentRefreshPreservesPublishedState()
        {
            var source = CreateSource();

            source.Rows.Add("sf", "original");

            var state = new SharedAutoReplaceState();

            state.EnsureLoaded(true, () => source);

            var working = state.WorkingTable;

            working.Rows[0][1] = "temporary";
            state.PublishEdits(true);

            var map = state.GetMap(true);

            Assert.ThrowsExactly<InvalidOperationException>(() => state.RefreshPermanentRules(true, () => null));
            Assert.AreSame(working, state.WorkingTable);
            Assert.AreSame(map, state.GetMap(true));
            Assert.AreEqual("temporary", map["sf"]);
        }
    }
}
