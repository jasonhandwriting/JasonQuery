using JasonQuery.UI.QueryEditor.AutoComplete.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.QueryEditor.AutoComplete.Workflows
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteFeedbackPolicyTests
    {
        private const int NoneAction = 0;
        private const int ShowResolveErrorAction = 1;
        private const int ClearResolveErrorAction = 2;

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(true, false, ShowResolveErrorAction)]
        [DataRow(false, false, NoneAction)]
        [DataRow(false, true, ClearResolveErrorAction)]
        [DataRow(true, true, ClearResolveErrorAction)]
        public void Resolve_ReturnsExpectedAction(bool queryExecutionFailed, bool autoCompleteSucceeded, int expectedValue)
        {
            var expected = (QueryEditorAutoCompleteFeedbackAction)expectedValue;
            var actual = QueryEditorAutoCompleteFeedbackPolicy.Resolve(queryExecutionFailed, autoCompleteSucceeded);

            Assert.AreEqual(expected, actual);
        }
    }
}
