using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteSchemaQueryExecutionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void IsSuccessful_WithDataAndNoError_ReturnsTrueWithoutExecutionFailure()
        {
            using (var data = new DataTable())
            {
                var succeeded = QueryEditorAutoCompleteSchemaQueryExecutionPolicy.IsSuccessful(data, string.Empty, out var queryExecutionFailed);

                Assert.IsTrue(succeeded);
                Assert.IsFalse(queryExecutionFailed);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void IsSuccessful_WithDatabaseError_ReturnsFalseWithExecutionFailure()
        {
            using (var data = new DataTable())
            {
                var succeeded = QueryEditorAutoCompleteSchemaQueryExecutionPolicy.IsSuccessful(data, "table does not exist", out var queryExecutionFailed);

                Assert.IsFalse(succeeded);
                Assert.IsTrue(queryExecutionFailed);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void IsSuccessful_WithNullDataAndNoDatabaseError_DoesNotReportResolveError()
        {
            var succeeded = QueryEditorAutoCompleteSchemaQueryExecutionPolicy.IsSuccessful(null, string.Empty, out var queryExecutionFailed);

            Assert.IsFalse(succeeded);
            Assert.IsFalse(queryExecutionFailed);
        }
    }
}
