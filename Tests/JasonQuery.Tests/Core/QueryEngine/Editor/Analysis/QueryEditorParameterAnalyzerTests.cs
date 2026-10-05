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
        [DataRow("select * from t where id = :id")]
        [DataRow("select * from t where a = :a and b = :b")]
        [DataRow("select * from t where a = :id or b = :ID")]
        [DataRow("select ':ignored' as x from t where id = :id")]
        [DataRow("select \":ignored\" from t where id = :id")]
        [DataRow("select 1 -- :ignored\r\nwhere id = :id")]
        [DataRow("select /* :ignored */ 1 where id = :id")]
        [DataRow("select value::text from t where id = :id")]
        [DataRow(":ignored_at_start select 1")]
        [DataRow("select x:ignored from t")]
        [DataRow("select\t:ignored from t")]
        [DataRow("select [ :legacy ]")]
        [DataRow("select ` :legacy `")]
        [DataRow("select $$ :legacy $$")]
        [DataRow("select /* outer /* :ignored */ :legacy */ 1")]
        [DataRow("select 1\n:p")]
        public void ExtractParametersInfo_SharedTokenizer_MatchesLegacy(string sql)
        {
            var shared = QueryEditorParameterAnalyzer.ExtractParametersInfo(sql, out string sharedAll);
            var legacy = QueryEditorParameterAnalyzer.ExtractParametersInfoLegacyForParityTest(sql, out string legacyAll);

            Assert.AreEqual(legacy, shared);
            Assert.AreEqual(legacyAll, sharedAll);
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
