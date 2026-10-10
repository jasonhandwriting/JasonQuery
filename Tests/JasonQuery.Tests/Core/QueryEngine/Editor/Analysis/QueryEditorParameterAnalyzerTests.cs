using JasonQuery.Core.QueryEngine.Editor.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Analysis
{
    [TestClass]
    public sealed class QueryEditorParameterAnalyzerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("QueryParameters")]
        [DataRow("select * from t where id = :id", ":id", "`:id|27`")]
        [DataRow("select * from t where a = :a and b = :b", ":a`:b", "`:a|26`:b|37`")]
        [DataRow("select * from t where a = :id or b = :ID", ":id", "`:id|26`:ID|37`")]
        [DataRow("select ':ignored' as x from t where id = :id", ":id", "`:id|41`")]
        [DataRow("select \":ignored\" from t where id = :id", ":id", "`:id|36`")]
        [DataRow("select 1 -- :ignored\r\nwhere id = :id", ":id", "`:id|33`")]
        [DataRow("select /* :ignored */ 1 where id = :id", ":id", "`:id|35`")]
        [DataRow("select value::text from t where id = :id", ":id", "`:id|37`")]
        [DataRow(":ignored_at_start select 1", "", "`")]
        [DataRow("select x:ignored from t", "", "`")]
        [DataRow("select\t:ignored from t", "", "`")]
        [DataRow("select [ :legacy ]", ":legacy", "`:legacy|9`")]
        [DataRow("select ` :legacy `", ":legacy", "`:legacy|9`")]
        [DataRow("select $$ :legacy $$", ":legacy", "`:legacy|10`")]
        [DataRow("select /* outer /* :ignored */ :legacy */ 1", ":legacy", "`:legacy|31`")]
        [DataRow("select 1\n:p", ":p", "`:p|9`")]
        public void ExtractParametersInfo_SharedTokenizer_PreservesExpectedResults(string sql, string expected, string expectedAll)
        {
            var actual = QueryEditorParameterAnalyzer.ExtractParametersInfo(sql, out string actualAll);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedAll, actualAll);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("QueryParameters")]
        public void ExtractParametersInfo_LineCommentContainingUnmatchedQuote_DoesNotPoisonFollowingSql()
        {
            const string sql = "select 1 -- 'comment\r\nwhere id = :p";

            AssertFindsSingleParameter(sql, ":p");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("QueryParameters")]
        public void ExtractParametersInfo_BlockCommentContainingUnmatchedQuote_DoesNotPoisonFollowingSql()
        {
            const string sql = "select /* ' comment */ 1 where id = :p";

            AssertFindsSingleParameter(sql, ":p");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("QueryParameters")]
        public void ExtractParametersInfo_LineCommentContainingBlockCommentOpener_DoesNotPoisonFollowingSql()
        {
            const string sql = "select 1 -- /* comment\r\nwhere id = :p";

            AssertFindsSingleParameter(sql, ":p");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("QueryParameters")]
        public void ExtractParametersInfo_BlockCommentContainingLineCommentMarker_DoesNotPoisonFollowingSql()
        {
            const string sql = "select /* -- comment */ 1 where id = :p";

            AssertFindsSingleParameter(sql, ":p");
        }

        private static void AssertFindsSingleParameter(string sql, string expectedParameter)
        {
            var actual = QueryEditorParameterAnalyzer.ExtractParametersInfo(sql, out string resultAll);
            var expectedPosition = sql.IndexOf(expectedParameter, System.StringComparison.Ordinal);

            Assert.AreEqual(expectedParameter, actual);
            Assert.AreEqual("`" + expectedParameter + "|" + expectedPosition + "`", resultAll);
        }
    }
}
