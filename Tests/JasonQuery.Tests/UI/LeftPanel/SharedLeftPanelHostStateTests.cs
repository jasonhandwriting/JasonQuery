using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class SharedLeftPanelHostStateTests
    {
        private static SharedLeftPanelHostState Query(SharedLeftPanelHostState previous = null, string identity = "q1")
        {
            return SharedLeftPanelHostStatePolicy.Refresh(previous, true, true, identity, LeftPanelTargetKind.QueryForm);
        }

        [TestMethod]
        public void Empty_IsDisconnectedAndHasNoTab()
        {
            var value = SharedLeftPanelHostStatePolicy.Empty;

            Assert.IsFalse(value.IsConnected);
            Assert.IsFalse(value.HasOpenTabs);
            Assert.IsFalse(value.HasActiveTab);
            Assert.AreEqual(LeftPanelTargetKind.None, value.ActiveTarget);
        }

        [TestMethod]
        public void Empty_HasNoStaleIdentity()
        {
            Assert.AreEqual(string.Empty, SharedLeftPanelHostStatePolicy.Empty.ActiveTabIdentity);
        }

        [TestMethod]
        public void Empty_HasZeroRevisions()
        {
            var value = SharedLeftPanelHostStatePolicy.Empty;

            Assert.AreEqual(0, value.Revision);
            Assert.AreEqual(0, value.ConnectionRevision);
        }

        [TestMethod]
        public void NullPrevious_UsesEmpty()
        {
            Assert.AreEqual(1, Query().Revision);
        }

        [TestMethod]
        public void ConnectedQueryTab_IsActive()
        {
            var value = Query();

            Assert.IsTrue(value.IsConnected);
            Assert.IsTrue(value.HasOpenTabs);
            Assert.IsTrue(value.HasActiveTab);
            Assert.AreEqual("q1", value.ActiveTabIdentity);
        }

        [TestMethod]
        public void QueryTarget_IsRetained()
        {
            Assert.AreEqual(LeftPanelTargetKind.QueryForm, Query().ActiveTarget);
        }

        [TestMethod]
        public void ConnectionTransition_AdvancesConnectionRevision()
        {
            Assert.AreEqual(1, Query().ConnectionRevision);
        }

        [TestMethod]
        public void RefreshUnchanged_ReusesSnapshot()
        {
            var state = Query();

            Assert.AreSame(state, Query(state));
        }

        [TestMethod]
        public void SwitchingQueryTabs_ChangesIdentityNotConnectionRevision()
        {
            var first = Query();
            var second = Query(first, "q2");

            Assert.AreEqual("q2", second.ActiveTabIdentity);
            Assert.AreEqual(first.ConnectionRevision, second.ConnectionRevision);
            Assert.AreEqual(first.Revision + 1, second.Revision);
        }

        [TestMethod]
        public void Identity_IsCaseSensitive()
        {
            var state = Query();

            Assert.AreEqual("Q1", Query(state, "Q1").ActiveTabIdentity);
        }

        [TestMethod]
        public void SchemaBrowserTarget_PreservesIdentity()
        {
            var state = SharedLeftPanelHostStatePolicy.Refresh(Query(), true, true, "browser", LeftPanelTargetKind.SchemaBrowserForm);

            Assert.AreEqual(LeftPanelTargetKind.SchemaBrowserForm, state.ActiveTarget);
            Assert.AreEqual("browser", state.ActiveTabIdentity);
        }

        [TestMethod]
        public void OtherFormTarget_PreservesIdentity()
        {
            var state = SharedLeftPanelHostStatePolicy.Refresh(Query(), true, true, "options", LeftPanelTargetKind.OtherForm);

            Assert.AreEqual(LeftPanelTargetKind.OtherForm, state.ActiveTarget);
        }

        [TestMethod]
        public void Disconnected_ResetsIdentityAndTarget()
        {
            var state = SharedLeftPanelHostStatePolicy.Refresh(Query(), false, true, "q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(state.HasActiveTab);
            Assert.IsFalse(state.HasOpenTabs);
            Assert.AreEqual(string.Empty, state.ActiveTabIdentity);
            Assert.AreEqual(LeftPanelTargetKind.None, state.ActiveTarget);
        }

        [TestMethod]
        public void Disconnected_AdvancesConnectionRevision()
        {
            var q = Query();
            var state = SharedLeftPanelHostStatePolicy.Refresh(q, false, false, null, LeftPanelTargetKind.None);

            Assert.AreEqual(q.ConnectionRevision + 1, state.ConnectionRevision);
        }

        [TestMethod]
        public void Reconnected_RequiresFreshIdentity()
        {
            var disconnected = SharedLeftPanelHostStatePolicy.Refresh(Query(), false, false, null, LeftPanelTargetKind.None);
            var connected = SharedLeftPanelHostStatePolicy.Refresh(disconnected, true, true, null, LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(connected.HasActiveTab);
            Assert.AreEqual(string.Empty, connected.ActiveTabIdentity);
        }

        [TestMethod]
        public void Reconnected_CanCaptureNewQueryTab()
        {
            var disconnected = SharedLeftPanelHostStatePolicy.Refresh(Query(), false, false, null, LeftPanelTargetKind.None);

            Assert.AreEqual("new-q1", Query(disconnected, "new-q1").ActiveTabIdentity);
        }

        [TestMethod]
        public void NoOpenTabs_ClearsIdentityEvenWhenConnected()
        {
            var state = SharedLeftPanelHostStatePolicy.Refresh(Query(), true, false, "q1", LeftPanelTargetKind.QueryForm);

            Assert.IsFalse(state.HasActiveTab);
            Assert.AreEqual(LeftPanelTargetKind.None, state.ActiveTarget);
        }

        [TestMethod]
        public void WhitespaceIdentity_IsNotActive()
        {
            var state = Query(null, "   ");

            Assert.AreEqual(string.Empty, state.ActiveTabIdentity);
            Assert.IsFalse(state.HasActiveTab);
        }

        [TestMethod]
        public void MissingIdentity_HasNoActiveTarget()
        {
            var state = Query(null, null);

            Assert.AreEqual(LeftPanelTargetKind.None, state.ActiveTarget);
        }

        [TestMethod]
        public void ChangingTarget_AdvancesStateRevision()
        {
            var q = Query();
            var s = SharedLeftPanelHostStatePolicy.Refresh(q, true, true, "q1", LeftPanelTargetKind.SchemaBrowserForm);

            Assert.AreEqual(q.Revision + 1, s.Revision);
        }

        [TestMethod]
        public void MarkDisposed_ClearsContext()
        {
            var state = SharedLeftPanelHostStatePolicy.MarkDisposed(Query());

            Assert.IsTrue(state.IsDisposed);
            Assert.IsFalse(state.IsConnected);
            Assert.AreEqual(string.Empty, state.ActiveTabIdentity);
        }

        [TestMethod]
        public void DisposedState_RejectsLaterRefresh()
        {
            var closed = SharedLeftPanelHostStatePolicy.MarkDisposed(Query());

            Assert.AreSame(closed, Query(closed, "q2"));
        }

        [TestMethod]
        public void MarkDisposed_IsIdempotent()
        {
            var closed = SharedLeftPanelHostStatePolicy.MarkDisposed(Query());

            Assert.AreSame(closed, SharedLeftPanelHostStatePolicy.MarkDisposed(closed));
        }

        [TestMethod]
        public void MarkDisposed_FromNull_IsSafe()
        {
            Assert.IsTrue(SharedLeftPanelHostStatePolicy.MarkDisposed(null).IsDisposed);
        }
    }
}
