using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSessionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(-5, 10, 0)]
        [DataRow(0, 10, 0)]
        [DataRow(5, 10, 5)]
        [DataRow(10, 10, 10)]
        [DataRow(15, 10, 10)]
        [DataRow(5, -1, 0)]
        public void ClampEditorPosition_ReturnsSafePosition(int position, int textLength, int expected)
        {
            Assert.AreEqual(expected, QueryEditorAutoCompleteSessionPolicy.ClampEditorPosition(position, textLength));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(4, 5, true)]
        [DataRow(5, 5, true)]
        [DataRow(6, 5, false)]
        [DataRow(10, 5, false)]
        public void ShouldHideOnEditorBackspace_ReturnsExpected(int currentPosition, int triggerPosition, bool expected)
        {
            Assert.AreEqual(expected, QueryEditorAutoCompleteSessionPolicy.ShouldHideOnEditorBackspace(currentPosition, triggerPosition));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(4, 5, true)]
        [DataRow(5, 5, false)]
        [DataRow(6, 5, false)]
        [DataRow(10, 5, false)]
        public void ShouldHideAfterGridBackspace_ReturnsExpected(int currentPosition, int triggerPosition, bool expected)
        {
            Assert.AreEqual(expected, QueryEditorAutoCompleteSessionPolicy.ShouldHideAfterGridBackspace(currentPosition, triggerPosition));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(2, -1, true)]
        [DataRow(3, -1, false)]
        [DataRow(3, 1, false)]
        [DataRow(4, 1, false)]
        [DataRow(5, 1, true)]
        [DataRow(5, -1, false)]
        public void ShouldHideOnHorizontalMove_ReturnsExpected(int currentPosition, int delta, bool expected)
        {
            var range = new QueryEditorAutoCompleteTextRange(2, 5);

            Assert.AreEqual(expected, QueryEditorAutoCompleteSessionPolicy.ShouldHideOnHorizontalMove(range, currentPosition, delta));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void ShouldHideOnHorizontalMove_WithNullRange_ReturnsTrue()
        {
            Assert.IsTrue(QueryEditorAutoCompleteSessionPolicy.ShouldHideOnHorizontalMove(null, 5, 1));
        }
    }
}
