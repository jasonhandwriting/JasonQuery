using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteRangeResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("", 0, 0, 0)]
        [DataRow("abc", 0, 0, 3)]
        [DataRow("abc", 1, 1, 3)]
        [DataRow("abc", 3, 3, 3)]
        [DataRow("abc def", 4, 4, 7)]
        [DataRow("abc_def", 0, 0, 7)]
        [DataRow("abc.def", 4, 4, 7)]
        [DataRow("abc", -5, 0, 3)]
        [DataRow("abc", 99, 3, 3)]
        public void GetActiveRange_ForwardFromTrigger_ReturnsExpectedRange(string text, int triggerPosition, int expectedStart, int expectedEnd)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetActiveRange(QueryEditorAutoCompleteRangeMode.ForwardFromTrigger, text, triggerPosition);

            Assert.AreEqual(expectedStart, range.Start);
            Assert.AreEqual(expectedEnd, range.EndExclusive);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("", 0, 0, 0)]
        [DataRow("abc", 0, 0, 3)]
        [DataRow("abc", 1, 0, 3)]
        [DataRow("abc", 3, 0, 3)]
        [DataRow("abc def", 5, 4, 7)]
        [DataRow("abc_def", 4, 0, 7)]
        [DataRow("abc.def", 5, 4, 7)]
        [DataRow("abc def", 3, 0, 3)]
        [DataRow("abc", -5, 0, 3)]
        [DataRow("abc", 99, 0, 3)]
        public void GetActiveRange_WholeIdentifier_ReturnsExpectedRange(string text, int triggerPosition, int expectedStart, int expectedEnd)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetActiveRange(QueryEditorAutoCompleteRangeMode.WholeIdentifier, text, triggerPosition);

            Assert.AreEqual(expectedStart, range.Start);
            Assert.AreEqual(expectedEnd, range.EndExclusive);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("abc", 0, 3, 3, 0, 3)]
        [DataRow("abc", 0, 5, 2, 0, 3)]
        [DataRow("abc xyz", 4, 7, 7, 4, 7)]
        [DataRow("abc", 3, 10, 10, 3, 3)]
        [DataRow("", 0, 0, 0, 0, 0)]
        [DataRow("abc", -1, -1, -1, 0, 3)]
        public void GetReplaceRange_ForwardFromTrigger_ReturnsExpectedRange(string text, int triggerPosition, int sessionCaretPosition, int currentPosition, int expectedStart, int expectedEnd)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetReplaceRange
            (
                QueryEditorAutoCompleteRangeMode.ForwardFromTrigger,
                text,
                triggerPosition,
                sessionCaretPosition,
                currentPosition
            );

            Assert.AreEqual(expectedStart, range.Start);
            Assert.AreEqual(expectedEnd, range.EndExclusive);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("abc", 1, 3, 3, 0, 3)]
        [DataRow("abc", 1, 2, 2, 0, 3)]
        [DataRow("abc def", 5, 7, 7, 4, 7)]
        [DataRow("abc def", 5, 20, 20, 4, 7)]
        [DataRow("abc_def", 4, 7, 7, 0, 7)]
        public void GetReplaceRange_WholeIdentifier_ReturnsExpectedRange(string text, int triggerPosition, int sessionCaretPosition, int currentPosition, int expectedStart, int expectedEnd)
        {
            var range = QueryEditorAutoCompleteRangeResolver.GetReplaceRange
            (
                QueryEditorAutoCompleteRangeMode.WholeIdentifier,
                text,
                triggerPosition,
                sessionCaretPosition,
                currentPosition
            );

            Assert.AreEqual(expectedStart, range.Start);
            Assert.AreEqual(expectedEnd, range.EndExclusive);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(5, 7, 10, 5)]
        [DataRow(7, 5, 10, 5)]
        [DataRow(-1, 5, 10, 0)]
        [DataRow(5, -1, 10, 0)]
        [DataRow(20, 30, 10, 10)]
        [DataRow(5, 7, -1, 0)]
        public void GetSessionCaretPosition_ReturnsClampedMinimum(int sessionCaretPosition, int currentPosition, int textLength, int expected)
        {
            Assert.AreEqual
            (
                expected,
                QueryEditorAutoCompleteRangeResolver.GetSessionCaretPosition
                (
                    sessionCaretPosition,
                    currentPosition,
                    textLength
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(2, true)]
        [DataRow(3, true)]
        [DataRow(5, true)]
        [DataRow(6, false)]
        [DataRow(-1, false)]
        public void TextRange_ContainsPosition_IncludesBothBoundaries(int position, bool expected)
        {
            var range = new QueryEditorAutoCompleteTextRange(2, 5);

            Assert.AreEqual(expected, range.ContainsPosition(position));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void TextRange_EndBeforeStart_IsNormalizedToEmptyRange()
        {
            var range = new QueryEditorAutoCompleteTextRange(5, 2);

            Assert.AreEqual(5, range.Start);
            Assert.AreEqual(5, range.EndExclusive);
            Assert.AreEqual(0, range.Length);
        }
    }
}
