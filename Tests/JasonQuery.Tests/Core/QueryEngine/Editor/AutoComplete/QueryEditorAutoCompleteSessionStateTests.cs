using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSessionStateTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(0, 0, 0, 0)]
        [DataRow(5, 5, 5, 5)]
        [DataRow(-1, -1, 0, 0)]
        [DataRow(-5, 3, 0, 3)]
        [DataRow(8, 3, 8, 8)]
        [DataRow(10, 25, 10, 25)]
        public void Start_NormalizesPositions(int triggerPosition, int caretPosition, int expectedTrigger, int expectedCaret)
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.Start(triggerPosition, caretPosition);

            Assert.AreEqual(expectedTrigger, state.TriggerPosition);
            Assert.AreEqual(expectedCaret, state.CaretPosition);
            Assert.IsTrue(state.IsOpen);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Start_ResetsTransientFlags()
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.MarkCommitByTab();
            state.MarkReturnedToEditorByGridUp();
            state.Start(5, 8);

            Assert.IsFalse(state.CommitByTab);
            Assert.IsFalse(state.ReturnedToEditorByGridUp);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(-5, 0)]
        [DataRow(0, 0)]
        [DataRow(12, 12)]
        public void UpdateCaretPosition_NormalizesValue(int value, int expected)
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.Start(0, 0);
            state.UpdateCaretPosition(value);

            Assert.AreEqual(expected, state.CaretPosition);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(-5, 0, 3)]
        [DataRow(5, 5, 5)]
        [DataRow(8, 8, 8)]
        public void UpdateTriggerPosition_UpdatesCaretWhenCaretIsBeforeTrigger(int value, int expectedTrigger, int expectedCaret)
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.Start(0, 3);
            state.UpdateTriggerPosition(value);

            Assert.AreEqual(expectedTrigger, state.TriggerPosition);
            Assert.AreEqual(expectedCaret, state.CaretPosition);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void MarkAndConsumeCommitByTab_ConsumesOnlyOnce()
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.MarkCommitByTab();

            Assert.IsTrue(state.CommitByTab);
            Assert.IsTrue(state.ConsumeCommitByTab());
            Assert.IsFalse(state.CommitByTab);
            Assert.IsFalse(state.ConsumeCommitByTab());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void MarkAndConsumeReturnedToEditorByGridUp_ConsumesOnlyOnce()
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.MarkReturnedToEditorByGridUp();

            Assert.IsTrue(state.ReturnedToEditorByGridUp);
            Assert.IsTrue(state.ConsumeReturnedToEditorByGridUp());
            Assert.IsFalse(state.ReturnedToEditorByGridUp);
            Assert.IsFalse(state.ConsumeReturnedToEditorByGridUp());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.ExternalHide, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.Escape, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.Commit, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.CommitByTab, true)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.Navigation, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.InvalidInput, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.NoCandidates, false)]
        [DataRow((int)QueryEditorAutoCompleteSessionCloseReason.RefreshFailed, false)]
        public void Close_SetsExpectedCommitByTabState(int closeReasonValue, bool expectedCommitByTab)
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.Start(5, 8);
            state.MarkCommitByTab();
            state.MarkReturnedToEditorByGridUp();
            state.Close((QueryEditorAutoCompleteSessionCloseReason)closeReasonValue);

            Assert.IsFalse(state.IsOpen);
            Assert.AreEqual(expectedCommitByTab, state.CommitByTab);
            Assert.IsFalse(state.ReturnedToEditorByGridUp);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void ResetTransientFlags_ClearsBothFlagsWithoutClosingSession()
        {
            var state = new QueryEditorAutoCompleteSessionState();

            state.Start(5, 8);
            state.MarkCommitByTab();
            state.MarkReturnedToEditorByGridUp();
            state.ResetTransientFlags();

            Assert.IsTrue(state.IsOpen);
            Assert.IsFalse(state.CommitByTab);
            Assert.IsFalse(state.ReturnedToEditorByGridUp);
        }
    }
}
